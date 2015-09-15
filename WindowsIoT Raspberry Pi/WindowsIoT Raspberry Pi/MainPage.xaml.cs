using Rasp_Final;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Media.SpeechRecognition;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WindowsIoT_Raspberry_Pi
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private SpeechRecognizer speechRecognizer;


        //CONST and Variables Definitions 
        private SpiDevice Spi_port;                         //nRF SPI 
        GpioPin nRF_CE;                                     //nRF CE Pin
        private DispatcherTimer timer,azureMonitor;                      //nRF Receive Data Timer
        private DispatcherTimer autoupdate_timer;                      //nRF Receive Data Timer

        private DispatcherTimer scan_slaves_timer;          //timer for scanning slave devices
        private nRF nrf;
        private const string SPI_CONTROLLER_NAME = "SPI0";  /* nRF Connected to SPI0                        */
        private const Int32 SPI_CHIP_SELECT_LINE = 0;       /*nRF CSN (~Slave Select) connected to CS0*/
        public byte[] last_received = new byte[10];
        public static MainPage Current;
        public dev_info lastitem; 
        private AzureDBmanager DBM = new AzureDBmanager(); 
        BackgroundWorker azurebkwork = new BackgroundWorker();

        #region wireless packet enums
        /// <summary>
        /// Packet Type Representation
        /// </summary>
        public enum packet_type : byte
        {
            add_me_packet = 1,
            data_packet = 2,
            update_packet = 3,
            remove_packet = 4

        };
        /// <summary>
        /// Device Type Representation
        /// </summary>
        public enum device_type : byte
        {
            Test_Device = 0,
            Triac_Device = 1,
            Relay_Device = 2,
            Buzzer_Device = 3,
            Motor_Device = 4
        };

        /// <summary>
        /// Device Type String array
        /// </summary>
        /// You could make/add any type of devices to the windows iot raspberry pi automation network
        string[] device_type_string = { "Test Device", "Triac Device", "Relay Device", "Buzzer Device", "Motor Device" };


        #endregion


        public MainPage()
        {
            this.InitializeComponent();
            multi_cast(); //Create multicast object and start listener
            Current = this;
            var gpio = GpioController.GetDefault();
            nRF_CE = gpio.OpenPin(26);
            timer = new DispatcherTimer();
            autoupdate_timer = new DispatcherTimer();
            scan_slaves_timer = new DispatcherTimer();
            //init scan timer
            scan_slaves_timer.Tick += scan_slaves_proc;
            scan_slaves_timer.Interval = TimeSpan.FromMilliseconds(500);
            azureMonitor = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            azureMonitor.Interval = TimeSpan.FromSeconds(1);
            autoupdate_timer.Interval = TimeSpan.FromSeconds(1);
            autoupdate_timer.Tick += autoupdate_timer_Tick;
            azureMonitor.Tick += azureMonitor_tick;
            timer.Tick += Timer_Tick;
            Init_Hard();
            //////////////////////////////////////////// 
           
        } 

        #region nRF and SPI init function
        private async void Init_Hard()
        {
            try
            {
                await init_spi();                   /* Initialize the SPI controller                */
                nrf = new nRF(nRF_CE, Spi_port);    //nrf object
                nrf.init_nRF();
                timer.Start();
                azurebkwork.DoWork += azure_work;
                azureMonitor.Start(); 
                ///Other hardware initializations can be coded below

            }

            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                ///Exception
                //You could use a textbox to show exceptions
                //In my project I don't find any space to place textbox for exception :P
            }

        }
        /// <summary>
        /// init spi
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task init_spi()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE); /* Create SPI initialization settings                               */
                settings.ClockFrequency = 1000000;                             /*     */
                settings.Mode = SpiMode.Mode0;

                string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);       /* Find the selector string for the SPI bus controller          */
                var devicesInfo = await DeviceInformation.FindAllAsync(spiAqs);         /* Find the SPI bus controller device with our selector string  */
                Spi_port = await SpiDevice.FromIdAsync(devicesInfo[0].Id, settings);  /* Create an SpiDevice with our bus controller and SPI settings */

            }

            catch (Exception excep)
            {
                throw new Exception("SPI Initialization Failed", excep);
            }

        }
        #endregion


        #region nRF Receive and Transmit functions
        ///Data rx timer ticks every 10ms// no interrupt/event driven methods possible
        /// //polling method
        /// 
        byte[] databyte; 
        private void Timer_Tick(object sender, object e)
        {
            timer.Stop();
            nrf.RX_MODE();
            byte[] tmp = { 0xFF };
            byte[] read_byte = { 0x00 };
            Spi_port.TransferFullDuplex(tmp, read_byte);
            //Spi_port.Read(read_byte);
            //string hexValue = read_byte[0].ToString("x");
            //textBoxff.Text = hexValue;  
            if ((read_byte[0] & (1 << 6)) != 0)
            {
                databyte = nrf.RX_PAYLOAD();
                data_received(databyte);
                nrf.Flush_RX();
                //textBoxff.Text = data[0] + " " + data[1] + " " + data[2] + " ";
            }
            timer.Start();
        }


        //Azure Timer Tick every 1 second
        //Check for new commands 
        private  void azureMonitor_tick(object sender, object e)
        {
            azureMonitor.Stop();
            try
            {
                 azurebkwork.RunWorkerAsync(); 
            }
            catch(Exception ex)
            {

            }
            azureMonitor.Start();

        }

        //background task
        public async void azure_work(object sender, EventArgs e)
        { 
            try
            {
                ///Read azure db and if string is NoCommand
                /// return
                /// else execute command 
                string[] s = await DBM.read();
            if (s[1] == "NoCommand")
                return;
                DBM.update();
            send_string(s[1], true);
            var ignr = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => statusbar_string("Command from '" + s[2] + "' received", false));
            }
            catch(Exception ex)
            {

            }
           
        }

        /*
        nRF TX packet count != Raspbverryy Pi RX packet
        Raspberry Pi receives an extra byte at the begining (STATUS Register of nRF module)
        We don't need status register values in below function.
        data_byte[0] can be discarded.
        To know more about STATUS register and nRF module operation refer its datasheets
        */ 
        public void data_received(byte[] data_byte)
        {
            last_received = data_byte;
            //Check Received Packet Type
            switch (data_byte[1])
            {
                /*Device initialization packet
                every nRF slave device need to send an "add me" packet before starting
                to send an "add me" packet hold the "Switch 1(sw1)" during power on
                this will add the device into the list view item. */
                case (byte)packet_type.add_me_packet:

                    //foreach (byte dev in data_byte)               //check all list view items
                    //{                                                       //for duplicate entry
                    //    textBox.Text += dev.ToString();
                    //}

                    foreach (dev_info dev in deviceList.Items)               //check all list view items
                    {                                                       //for duplicate entry
                        if (dev.device_address == data_byte[2])
                        {
                            statusbar_string("Duplicate item found...", true);
                            return;
                        }

                    }
                    statusbar_string("New Slave board found... ID:" + data_byte[2].ToString(), false);
                    //Get Device Type from tha packet 3rd byte
                    string list_device_string;
                    if (data_byte[3] < device_type_string.Length)
                        list_device_string = device_type_string[data_byte[3]];
                    else
                        list_device_string = "Unknown Type/New Device";
                    //Add new item to the listview
                    dev_info new_device = new dev_info(list_device_string);
                    new_device.board_type = data_byte[3];                   //3rd byte = Board type (Triac, Relay, Buzzer etc
                    new_device.device_address = data_byte[2];               //2ndByte = nRF slave ID\
                    //Device 1 to 4 state (on/off, trigger angle), any number of relays/buzzers/sensors can be added (need to modify the code)
                    //Either using multiple packets or extracting bits (On/Off states require only 1 bit, multible device states can be added in a single byte
                    new_device.device1_state = data_byte[5];
                    new_device.device2_state = data_byte[6];
                    new_device.device3_state = data_byte[7];
                    new_device.device4_state = data_byte[8];


              //       statusbar_string(new_device.device1_state.ToString()
              //          + new_device.device2_state.ToString() + new_device.device3_state.ToString() +
              //          new_device.device4_state.ToString(), false);
                    //Sensor readings (No sensor limitation, the code can be modified for upto n number of sensors
                    new_device.sensor1_value = data_byte[9];
                    new_device.sensor2_value = data_byte[10];
                     //add to listview
                    deviceList.Items.Add(new_device);
                    break;

                /* 
                Data packet contains sensor informations, solid state relay(s) status, relay(s) status, buzzer on/of condition
                and other slave specific informations.
                 */
                case (byte)packet_type.update_packet:
                    statusbar_string("Update Packet Received..." + data_byte[5], false);  
                    //If you are using different sensors then you could make necessary calibration below
                    //Before displaying the value.
                    //you will get 1.29 by the following math (3.3/255) * 100 
                    //i'm using 8bit adc (even though it's a 10bit i discarded 2 lsb bits)
                    //I have no idea how to calibrate light intensity
                    double temp;
                    if((data_byte[3] == (byte)device_type.Triac_Device) || (data_byte[3] == (byte)device_type.Motor_Device))
                          temp =  ((data_byte[7] * 1.29)) ; 
                    else
                          temp = ((data_byte[7] * 1.29)) ;

                    Sensors.Text ="PIR: " + data_byte[8] + ", " + "lIght Intensity:" +  data_byte[6].ToString() + ", Temp: " + temp.ToString() + "'C";

                    //using temp variable for light intnsity control
                    if (databyte[6] < 70)
                        temp = 0;
                    else
                        temp = databyte[6] - 70;

                    if ((bool)autocontrol.IsChecked)
                    {
                        byte[] bb =  {0x00,0x00, 0x00
                                    , 0x00 , 0x00 , 0x00 };

                        if (temp < 140)
                            bb[0] = (byte)(temp);
                        else
                            bb[0] = 140;

                        bb[1] = lastitem.device2_state;
                        bb[2] = lastitem.device3_state;
                        bb[3] = lastitem.device4_state;
                        nrf_send_data(bb, lastitem.device_address, packet_type.data_packet);
                    }

                    break;

                case (byte)packet_type.remove_packet:

                    //foreach (byte dev in data_byte)               //check all list view items
                    //{                                                       //for duplicate entry
                    //    textBox.Text += dev.ToString();
                    //}
                   int itemnumber = 0;
                    foreach (dev_info dev in deviceList.Items)               //check all list view items
                    {
                        itemnumber++;
                        if (dev.device_address == data_byte[2])
                        {
                            ///Accidental removal of motor controller devices may cause danger
                            /// so motor controller boards cannot be removed once added
                            /// you need to restart the application for removing motor controller devices
                            if (dev.board_type == (byte)device_type.Motor_Device)
                            {
                                statusbar_string("Motor Device cannot be removed... ID:" + data_byte[1].ToString(), true);
                                return;
                            }
                            deviceList.Items.Remove(dev); 
                            statusbar_string("item Removed... ID:" + data_byte[1].ToString(), false);
                            return;
                        }

                    } 
                    statusbar_string("No Device found with ID:" + data_byte[1].ToString(), true);
                    break;

            }
        }
        #endregion

        #region nRF Data send
        /// <summary>
        /// Send Data Packet
        /// </summary>
        /// <param name="tx_packet">6 byte wide byte packet</param>
        public void nrf_send_data(byte[] tx_packet, byte device_id, packet_type req_type, bool fullbyte = false)
        {
            nrf.Flush_TX();
            timer.Stop();
            if(!fullbyte)
            {
                byte[] tmpByte = new byte[10];
                tmpByte[0] = (byte)req_type;                    //Packet Type : Data
                tmpByte[1] = (byte)device_id;                        //Destination Board ID

                tmpByte[2] = 0x00;                              //Board Type 0x00 (don't care)
                tmpByte[3] = 0x00;                              //Future use byte
            //custom data    
                tmpByte[4] = tx_packet[0];
                tmpByte[5] = tx_packet[1];
                tmpByte[6] = tx_packet[2];
                tmpByte[7] = tx_packet[3];
                tmpByte[8] = tx_packet[4];
                tmpByte[9] = tx_packet[5];
                nrf.TX_PAYLOAD(tmpByte);
            }
            else
            {
                byte[] tmpByte = tx_packet;
                nrf.TX_PAYLOAD(tmpByte);
            }
           
            timer.Start();
        }

        #endregion

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.frame.Navigate(typeof(TriacDevice));

        }

        #region device infotmation class
        public class dev_info
        {
            public string Name { get; set; }
            public byte device_address { get; set; }
            public byte board_type { get; set; }
            public byte device_count { get; set; }
            public byte device1_state { get; set; }
            public byte device2_state { get; set; }
            public byte device3_state { get; set; }
            public byte device4_state { get; set; }
            public byte sensor1_value { get; set; }
            public byte sensor2_value { get; set; }
            /// <summary>
            /// dev_info class constructor
            /// </summary>
            /// <param name="name">Namenyou want to give</param>
            /// <param name="device_addr">Device address 8th byte[0-9]</param>
            public dev_info(string name)
            {
                Name = name;
            }
            public dev_info()
            {
            }
            public override string ToString()
            {
                return (Name + " [ID: " + device_address.ToString() + "]");
            }
        }
        #endregion
        private void deviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dev_info dev = (dev_info)deviceList.SelectedItem;
                lastitem = dev;
                scrolltext.Text = "Device ID:" + dev.device_address.ToString() + ", Device Name:" + dev.Name.ToString() + ","
                   + dev.board_type.ToString();
                switch (dev.board_type)
                {
                    case (byte)device_type.Triac_Device:
                        frame.Navigate(typeof(TriacDevice));
                        break;
                    case (byte)device_type.Relay_Device:
                        frame.Navigate(typeof(RelayBoard));
                        break;
                 case (byte)device_type.Motor_Device:
                        frame.Navigate(typeof(motor));
                        break; 
                    default:
                        frame.Navigate(typeof(TestPage));
                        break;
                }
            }
            catch(Exception ex)
            {
                statusbar_string(ex.ToString(), true);
            }

            //  button.Content = dev.id.ToString(); 

        }

        /// <summary>
        /// StatusBar message
        /// </summary>
        /// <param name="st_status">status message</param>
        /// <param name="error">error or message</param>
        public void statusbar_string(string st_status, bool error)
        {
            if (error)
                scrollv.Background = new SolidColorBrush(Windows.UI.Colors.Red);
            else
                scrollv.Background = new SolidColorBrush(Windows.UI.Colors.Green);
            scrolltext.Text = st_status;
        }

        /// <summary>
        /// Update Device list with latest value
        /// </summary>
        /// <param name="dev">dev_info object</param>
        public void update_devicelist(dev_info dev)
        {
            deviceList.SelectedItem = dev;
        }

        #region SOCKET PROG
        private DatagramSocket listenerSocket = null;
        /// <summary>
        /// initialize multicast
        /// </summary>
        private async void multi_cast()
        { 
            listenerSocket = new DatagramSocket();              //datagram object
            listenerSocket.MessageReceived += MessageReceived;  //MessageReceived event 
            //listenerSocket.MessageReceived += MessageReceived;
            listenerSocket.Control.MulticastOnly = true;

            await listenerSocket.BindServiceNameAsync("22113");
            listenerSocket.JoinMulticastGroup(new HostName("224.3.0.5"));
          

        }

        /// <summary>
        /// multi cast string
        /// </summary>
        /// <param name="info"></param>
        private async void send_string(string info, bool isinternal = false)
        {
            if (!isinternal)
            {
                IOutputStream outputStream;
                HostName remoteHostname = new HostName("224.3.0.5");
                outputStream = await listenerSocket.GetOutputStreamAsync(remoteHostname, "22113");
                string stringToSend = info;
                DataWriter writer = new DataWriter(outputStream);
                writer.WriteString(stringToSend);
                await writer.StoreAsync();
            }
            else 
            {

                var ignore = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
               do_job("Azure Database Command", info, " ", " ")); 
            }
        }



        /// <summary>
        /// multi cast byte
        /// </summary>
        /// <param name="info"></param>
        private async void send_byte(byte[] info)
        {
            try
            {

                System.Net.Sockets.Socket soc; 


                IOutputStream outputStream;
                HostName remoteHostname = new HostName("224.3.0.5");
                outputStream = await listenerSocket.GetOutputStreamAsync(remoteHostname, "22113");
                byte[] stringToSend = info;
                DataWriter writer = new DataWriter(outputStream);
                writer.WriteBytes(stringToSend);
                await writer.StoreAsync();
            }
            catch(Exception ex)
            {
                netlog.Text = ex.ToString();
            }
        }


        /// <summary>
        /// Receive data and remote address here + pass to controls
        /// </summary>
        /// <param name="addrs"></param>
        /// <param name="msg_string"></param>
        private void do_job(string addrs, string msg_string, string port_, string local_addr_)
        {
            //do jobs here :D raspberry or windows
            //deviceList.Items.Add(addrs + ":" + data_rx); //list view and datagram test : DONE :)

            //to clear network log textblock 
            //otherwise system may become slow due to large amount of log information
            if (netlog.Text.Length > 500)
                netlog.Text = "";

            //add network activity/log info to textblock
            netlog.Text  = "Message: " + msg_string + "\n" + addrs + ", Group: " + local_addr_ + ", Port: " + port_ + "\n" + netlog.Text;
             
            char[] chararray = msg_string.ToCharArray();
            string n_id;
            byte n_deviceID, devicelist_index = 0;
         
            //dev_info object
            dev_info dev_class = new dev_info();
            try
            {
                ///wrong length of device ID may cause exception
                ///extract device ID from received byte
                n_id = new String(new char[] { chararray[2], chararray[3] });
                 n_deviceID = byte.Parse(n_id);
                statusbar_string(n_id, true);
            }
            catch
            {
                statusbar_string("Unknown Network Packet/Command", true);
                return;
            }

            bool flag = false;
             ///match received id with devices present. if failed 'return'
            foreach (dev_info dev in deviceList.Items)
            {
                if (dev.device_address == n_deviceID)
                {
                    flag = true;        //device found
                    dev_class = dev;
                    break;
                }

                devicelist_index++;

            }

            if(!flag)
            { 
                //No Device Found
                statusbar_string("Network Request Received, But wrong Device ID", true);
                return;
            }

            byte[] n_tx_packet = new byte[6]; 


            if (chararray[0] == '!')        //checks whether it is control packet or other message 
            {
                ///this switch statemnt will be clear if you look at the Android program
                ///each charactor of chararray[1] represents a control function
                /// eg. on/off relay, buzzer, triac etc
                switch (chararray[1])
                {
                    case 'A':
                        dev_class.device1_state = 1;
                    break;

                    case 'B':
                         dev_class.device2_state = 1;
                    break;

                    case 'C':
                        dev_class.device3_state = 1;
                    break;

                    case 'D':
                        dev_class.device4_state = 1;
                    break;

                    case 'E':
                        dev_class.device1_state = 0;
                        break;

                    case 'F':
                        dev_class.device2_state = 0;
                        break;

                    case 'G':
                        dev_class.device3_state = 0;
                        break;

                    case 'H':
                        dev_class.device4_state = 0;
                        break;

                    case 'I':
                        dev_class.device2_state = 1;
                        break;

                    case 'J':
                        dev_class.device2_state = 0;
                        break;

                    case 'K':
                        dev_class.device3_state = 1;
                        break;

                    case 'L':
                        dev_class.device3_state = 0;
                        break;

                    case 'M':
                         
                            byte trigger_value = byte.Parse(new String(new char[] { chararray[4],
                            chararray[5] }));

                            trigger_value = (byte)((trigger_value * 140.0) / 99.0);

                            dev_class.device1_state = trigger_value; 
                        break;
                    case 'N':
                        dev_class.device1_state = 1;
                        dev_class.device2_state = 0;
                        dev_class.device3_state = 1;
                        dev_class.device4_state = 0;

                        break;
                    case 'O':
                        dev_class.device1_state = 0;
                        dev_class.device2_state = 1;
                        dev_class.device3_state = 0;
                        dev_class.device4_state = 1;
                        break;
                    case 'P':
                        dev_class.device1_state = 1;
                        dev_class.device2_state = 0;
                        dev_class.device3_state = 0;
                        dev_class.device4_state = 1;
                        break;
                    case 'Q':
                        dev_class.device1_state = 0;
                        dev_class.device2_state = 1;
                        dev_class.device3_state = 1;
                        dev_class.device4_state = 0;
                        break;
                    case 'R':
                        dev_class.device1_state = 0;
                        dev_class.device2_state = 0;
                        dev_class.device3_state = 0;
                        dev_class.device4_state = 0;
                        break;

                    default:
                        return; 
                }

                //construct data byte from dev_class
                //by this way we made switch statement much simple
                byte[] n_tx_byte = {dev_class.device1_state,
                                    dev_class.device2_state,
                                    dev_class.device3_state,
                                    dev_class.device4_state,dev_class.device4_state,0x00};

                ///Transmit data byte
                nrf_send_data(n_tx_byte, dev_class.device_address, packet_type.data_packet);
                //Update chnages
                deviceList.Items[devicelist_index] = dev_class;
            }

           

        }


        /// <summary>
        /// MessageReceived event
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="eventArguments"></param>
        void MessageReceived(DatagramSocket socket, DatagramSocketMessageReceivedEventArgs eventArguments)
        {
            try
            { 
                uint stringLength = eventArguments.GetDataReader().UnconsumedBufferLength;
                string receivedMessage = eventArguments.GetDataReader().ReadString(stringLength); 
                var ignore = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                do_job(eventArguments.RemoteAddress.ToString(), receivedMessage, eventArguments.RemotePort.ToString(),
                eventArguments.LocalAddress.ToString())); 
            }
            catch (Exception ex)
            {
                //discard any exceptions
                //
            }
        }


        #endregion
         
        //Test button 
        //used this button while debugging. 
        private void appBarButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
            //frame.Navigate(typeof(TriacDevice));
        }
         

        //Auto update enable/disable toggle switch
        private void toggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (deviceList.Items.Count > 0)
            {
                if (autoupdate.IsOn)
                    autoupdate_timer.Start();
                else
                    autoupdate_timer.Stop();
                statusbar_string("Auto update enabled", false);

            }
            else
            {
                statusbar_string("Device list is empty...", true);
            }
        }

        private async Task<RenderTargetBitmap> CreateBitmapFromElement(FrameworkElement uielement)
        {
            try
            {
                var renderTargetBitmap = new RenderTargetBitmap();
                await renderTargetBitmap.RenderAsync(uielement);
                var pxl = await renderTargetBitmap.GetPixelsAsync();
                send_byte(pxl.ToArray()); 
                 
            }
            catch (Exception ex)
            {
                netlog.Text = ex.ToString();
            }
    

            return null;
        }

        private async void screenshot_Click(object sender, RoutedEventArgs e)
        {
            await CreateBitmapFromElement(this);
        }

        private void scan_slave_Checked(object sender, RoutedEventArgs e)
        {
            dev_id_tmp = 0;
            scan_slaves_timer.Start();  //start scanning : broadcast ID to get addme packet
        }

        //Scan timer procedure
        byte dev_id_tmp = 0; //am declaring this variable here for a better understanding
        private void scan_slaves_proc(object sender, object e)
        {
            byte[] bb =  {0xFF,0x00, 0x00   //1st payload byte = 0xFF this will command slaves to initiate addme packet
            , 0x00 , 0x00 , 0x00 };

            progress_scan.Value = ((dev_id_tmp / 255.0) * 100.0);

            nrf_send_data(bb, dev_id_tmp, packet_type.update_packet);
            if(dev_id_tmp == 255)
            {
                dev_id_tmp = 0;
                scan_slave.IsChecked = false;
                scan_slaves_timer.Stop();                       ///stop scanning if when id number reaches 255
                return;
            }
            dev_id_tmp++;

        }

        private void scan_slave_Unchecked(object sender, RoutedEventArgs e)
        {
            progress_scan.Value = 100;
            scan_slaves_timer.Stop();
        }

        //Auto update timer for updating sensor data
        //This timer sends update packets to selected slave device for sensor reading
        //slave device will respond to this packet with latest sensor readings and device state
        private void autoupdate_timer_Tick(object sender, object e)
        {
            byte[] bb =  {0x00,0x00, 0x00
            , 0x00 , 0x00 , 0x00 }; 

            if(lastitem != null)
                nrf_send_data(bb, lastitem.device_address, packet_type.update_packet);
      

        }
         
    }
}