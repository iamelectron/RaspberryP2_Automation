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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsIoT_Raspberry_Pi
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RelayBoard : Page
    {
        /// <summary>
        /// to access main page 
        /// </summary>
        private MainPage rootPage = MainPage.Current;

        //4 tmers for delayed relay on/off function, 1 timer each
        private DispatcherTimer t_device1 = new DispatcherTimer();
        private DispatcherTimer t_device2 = new DispatcherTimer();
        private DispatcherTimer t_device3 = new DispatcherTimer();
        private DispatcherTimer t_device4 = new DispatcherTimer();


        //timer 1 tick
        private void t_device1_tick(object sender, object e)
        {
            //toggles current state of device  !state
            device1.IsChecked = !device1.IsChecked;
            timer1_on.IsOn = false;  //off timer toggle switch
            t_device1.Stop();       //Off timer
        }


        private void t_device2_tick(object sender, object e)
        {
            //toggles current state of device  !state
            device2.IsChecked = !device2.IsChecked;
            timer2_on.IsOn = false; //off timer toggle switch
            t_device2.Stop();
        }


        private void t_device3_tick(object sender, object e)
        {    //toggles current state of device  !state
            device3.IsChecked = !device3.IsChecked;
            timer3_on.IsOn = false; //off timer toggle switch
            t_device3.Stop();
        }


        private void t_device4_tick(object sender, object e)
        {    //toggles current state of device  !state
            device4.IsChecked = !device4.IsChecked;
            timer4_on.IsOn = false; //off timer toggle switch
            t_device4.Stop();
        }

        /// <summary>
        /// transmit all the commands from this page to respective nRF slave
        /// </summary>
        void transmit()
        {
            //4 DEVICE state bytes to transmit
            byte dev1_checked = Convert.ToByte(device1.IsChecked); 
            byte dev2_checked = Convert.ToByte(device2.IsChecked); 
            byte dev3_checked = Convert.ToByte(device3.IsChecked); 
            byte dev4_checked = Convert.ToByte(device4.IsChecked);

            //Update main page's selected device's states accordingly 
            rootPage.lastitem.device1_state = dev1_checked;
            rootPage.lastitem.device2_state = dev2_checked;
            rootPage.lastitem.device3_state = dev3_checked;
            rootPage.lastitem.device4_state = dev4_checked;
             
            //define bytes array
            byte[] bb =  { dev1_checked,dev2_checked, dev3_checked
            , dev4_checked , dev4_checked , dev4_checked };

            //send and update
            rootPage.nrf_send_data(bb, rootPage.lastitem.device_address, MainPage.packet_type.data_packet);
            rootPage.update_devicelist(rootPage.lastitem);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
        }

        //constructor for initialization
        public RelayBoard()
        { 
            this.InitializeComponent();
            t_device1.Tick += t_device1_tick;
            t_device2.Tick += t_device2_tick;
            t_device3.Tick += t_device3_tick;
            t_device4.Tick += t_device4_tick;
            //function to add 0-59 into seconds list
            for(byte second = 0; second <60; second++)
            {
                devic1_second.Items.Add(second);
                devic2_second.Items.Add(second);
                devic3_second.Items.Add(second);
                devic4_second.Items.Add(second);
            }

            //select 1 second as default second value
            devic1_second.SelectedIndex = 1;
            devic2_second.SelectedIndex = 1;
            devic3_second.SelectedIndex = 1;
            devic4_second.SelectedIndex = 1;

            //set/reset switches according to device's state
            bool d1state, d2state, d3state, d4state;
            d1state = Convert.ToBoolean(rootPage.lastitem.device1_state);
            d2state = Convert.ToBoolean(rootPage.lastitem.device2_state);
            d3state = Convert.ToBoolean(rootPage.lastitem.device3_state);
            d4state = Convert.ToBoolean(rootPage.lastitem.device4_state);
            device1.IsChecked = d1state;
            device2.IsChecked = d2state;
            device3.IsChecked = d3state;
            device4.IsChecked = d4state;

        }
         
   
        //Use checked unchecked events instead of this
        //This will also work but not recommended to do these kinda tasks
        //I used this event didn't change
        private void device1_Tapped(object sender, TappedRoutedEventArgs e)
        {
             
            ///update status bar info
            if ((bool)device1.IsChecked)
                rootPage.statusbar_string("Device1 On", false);
            else
                rootPage.statusbar_string("Device1 Off", true);
        }

        private void device2_Tapped(object sender, TappedRoutedEventArgs e)
        {
         
            if (device2.IsChecked == true)
                rootPage.statusbar_string("Device2 On", false);
            else
                rootPage.statusbar_string("Device2 Off", true);
        }

        private void device3_Tapped(object sender, TappedRoutedEventArgs e)
        {
            
            if ((bool)device3.IsChecked)
                rootPage.statusbar_string("Device3 On", false);
            else
                rootPage.statusbar_string("Device3 Off", true);
        }

        private void device4_Tapped(object sender, TappedRoutedEventArgs e)
        {
         
            if ((bool)device4.IsChecked)
                rootPage.statusbar_string("Device4 On", false);
            else
                rootPage.statusbar_string("Device4 Off", true);
        }   

        /// //////////////////////////////////////////////////////////////////////////
 
            ///Set timer interval 
        private void timer1_on_Toggled(object sender, RoutedEventArgs e)
        {  
            if (timer1_on.IsOn)
            {
                t_device1.Interval = new TimeSpan(timer1_time.Time.Hours, timer1_time.Time.Minutes,
                    devic1_second.SelectedIndex);
                t_device1.Start();
            }
            else
                t_device1.Stop();
        }
        ///Set timer interval
        private void timer2_on_Toggled(object sender, RoutedEventArgs e)
        {
            if (timer2_on.IsOn)
            {
                t_device2.Interval = new TimeSpan(timer2_time.Time.Hours, timer2_time.Time.Minutes,
                    devic2_second.SelectedIndex);              
                t_device2.Start();
            }
            else
                t_device2.Stop();
        }
        ///Set timer interval
        private void timer3_on_Toggled(object sender, RoutedEventArgs e)
        {
            if (timer3_on.IsOn)
            {
                t_device3.Interval = new TimeSpan(timer3_time.Time.Hours, timer3_time.Time.Minutes,
                    devic3_second.SelectedIndex);
                t_device3.Start();
            }
            else
                t_device3.Stop();
        }
        ///Set timer interval
        private void timer4_on_Toggled(object sender, RoutedEventArgs e)
        {
            if (timer4_on.IsOn)
            {
                t_device4.Interval = new TimeSpan(timer4_time.Time.Hours, timer4_time.Time.Minutes, 
                    devic4_second.SelectedIndex);
                t_device4.Start();
            }
            else
                t_device4.Stop();
        }



        // Send commands
        private void device1_Unchecked(object sender, RoutedEventArgs e)
        {
            transmit();
        }
        // Send commands
        private void device1_Checked(object sender, RoutedEventArgs e)
        {
            transmit();
        }
        // Send commands
        private void device2_Checked(object sender, RoutedEventArgs e)
        {
            transmit();
        }
        // Send commands
        private void device3_Checked(object sender, RoutedEventArgs e)
        {
            transmit();
        }
        // Send commands
        private void device4_Checked_1(object sender, RoutedEventArgs e)
        {
            transmit();
        }
        // Send commands
        private void device4_Unchecked(object sender, RoutedEventArgs e)
        {
            transmit();
        }
        // Send commands
        private void device3_Unchecked(object sender, RoutedEventArgs e)
        {
            transmit();
        }
        // Send commands
        private void device2_Unchecked(object sender, RoutedEventArgs e)
        {
            transmit();
        } 
    }
}
