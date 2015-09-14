using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using WindowsIoT_Raspberry_Pi;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Graphics.Display;
using Windows.Storage.Pickers;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Datagram
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //This program is just a demo to show how windows pc/mobile can be used 
        //to control your home appliances through raspberry pi
     
        //azure object 
        AzureDBmanager adb;
        public MainPage()
        {
            this.InitializeComponent();
            //Initalize multicast (open port/listen/create object etc)
            multi_cast();
            adb = new AzureDBmanager();
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
                //get string length
                uint stringLength = eventArguments.GetDataReader().UnconsumedBufferLength;
                 
                    //read string from the stream
                    string receivedMessage = eventArguments.GetDataReader().ReadString(stringLength);

                    //it is not possible to access controls from here
                    var ignore = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    do_job(eventArguments.RemoteAddress.ToString(), receivedMessage, eventArguments.RemotePort.ToString(),
                    eventArguments.LocalAddress.ToString()));
                
            }
            catch (Exception ex)
            {
                //show exception on the Network Log textbox
                bytes_s.Content = ex.ToString(); 
            }
        }
        //Datagram socket object
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

            //datagram service name
            await listenerSocket.BindServiceNameAsync("22113");
            //Datagram group IP
            listenerSocket.JoinMulticastGroup(new HostName("224.3.0.5"));


        }

        /// <summary>
        /// multi cast string
        /// </summary>
        /// <param name="info"></param>
        private async void send_string(string info)
        {
            try
            {
                ///if azure is off packets will not forward to azure db but to local network
                if (!azure.IsOn)
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
                    //else access azure database and update command
                        adb.send(info, "WinPC"); 

                }
            }
            catch(Exception ex)
            {

            }
        }

         
        //if any network commands/messages received 
        private void do_job(string addrs, string msg_string, string port_, string local_addr_)
        {
            //Clear rx box for smooth operation (if length is greater than 1000
            if (receiverbox.Text.Length > 1000)
                receiverbox.Text = "";

            ///set text
            receiverbox.Text= msg_string.ToString()+"\nFrom: " + addrs.ToString() + "\n" +receiverbox.Text;
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////
        /// /////////////////////////////////////////////////////////////////////////////////////////////
        ///         /// /////////////////////////////////////////////////////////////////////////////////////////////
        /// /////////////////////////////////////////////////////////////////////////////////////////////
        /// /////////////////////////////////////////////////////////////////////////////////////////////
        /////every command starts wirg '!' for eg. !A51 = '!' (COMMAND) + A (REPRESENTS A COMMAND) + 51 (SLAVE DEVICE ID)
        /// ///////////////////////////////////////////////////////////////////////////////////////////// 
        /// /////////////////////////////////////////////////////////////////////////////////////////////

        private void r1on_Click(object sender, RoutedEventArgs e)
        {
            send_string( "!A" + deviceID.Text.ToString());
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {

            send_string("!B" + deviceID.Text.ToString());
        }

        private void button_Copy1_Click(object sender, RoutedEventArgs e)
        {

            send_string("!C" + deviceID.Text.ToString());
        }

        private void button_Copy2_Click(object sender, RoutedEventArgs e)
        {

            send_string("!D" + deviceID.Text.ToString());
        }

        private void button_Copy3_Click(object sender, RoutedEventArgs e)
        {

            send_string("!E" + deviceID.Text.ToString());
        }

        private void button_Copy4_Click(object sender, RoutedEventArgs e)
        {

            send_string("!F" + deviceID.Text.ToString());
        }

        private void button_Copy5_Click(object sender, RoutedEventArgs e)
        {

            send_string("!G" + deviceID.Text.ToString());
        }

        private void button_Copy6_Click(object sender, RoutedEventArgs e)
        {

            send_string("!H" + deviceID.Text.ToString());
        }

        private void toggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if(toggleSwitch.IsOn) 
                send_string("!I" + deviceID.Text.ToString()); 
            else
                send_string("!J" + deviceID.Text.ToString());
        }

        private void toggleSwitch1_Toggled(object sender, RoutedEventArgs e)
        {
            if (toggleSwitch1.IsOn)
                send_string("!J" + deviceID.Text.ToString());
            else
                send_string("!K" + deviceID.Text.ToString());
        }

        private void triac_1_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if(triac_1.Value<10)
                send_string("!M" + deviceID.Text.ToString() + "0" + triac_1.Value.ToString());
            else
                send_string("!M" + deviceID.Text.ToString() + triac_1.Value.ToString());
        }

        private void bsend_Click(object sender, RoutedEventArgs e)
        {
            send_string(txbox.Text.ToString());
            txbox.Text = "";
        }

        private void bup_Click(object sender, RoutedEventArgs e)
        {
            send_string("!N" + deviceID.Text.ToString() );
        }

        private void bdown_Click(object sender, RoutedEventArgs e)
        {
            send_string("!O" + deviceID.Text.ToString() + triac_1.Value.ToString());
        }

        private void bright_Click(object sender, RoutedEventArgs e)
        {
            send_string("!P" + deviceID.Text.ToString() + triac_1.Value.ToString());
        }

        private void bleft_Click(object sender, RoutedEventArgs e)
        {
            send_string("!Q" + deviceID.Text.ToString() + triac_1.Value.ToString());
        }

        private void bstop_Click(object sender, RoutedEventArgs e)
        {
            send_string("!R" + deviceID.Text.ToString() + triac_1.Value.ToString());
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}