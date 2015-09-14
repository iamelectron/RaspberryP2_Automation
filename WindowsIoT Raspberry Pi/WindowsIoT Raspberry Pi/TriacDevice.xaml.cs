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
    public sealed partial class TriacDevice : Page
    {

        private MainPage rootPage = MainPage.Current;
        private DispatcherTimer dim_bright1 = new DispatcherTimer();
        private DispatcherTimer day_delay_timer = new DispatcherTimer();

        public TriacDevice()
        {
            this.InitializeComponent();

    
            dim_bright1.Interval = TimeSpan.FromMilliseconds(50);
            dim_bright1.Tick += dim_bright_Tick;
            day_delay_timer.Tick += day_hour;

            bool device2_state, device3_state;
            device2_state = Convert.ToBoolean(rootPage.lastitem.device2_state);
            device3_state = Convert.ToBoolean(rootPage.lastitem.device3_state);
            buzzer.IsOn = device2_state;
            relay_on_off.IsOn = device3_state;
            slider.Value =  rootPage.lastitem.device1_state;
            rootPage.statusbar_string(device2_state.ToString() + device3_state.ToString(), false);  
        }

    
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
             
        }
 
        
        /// <summary>
        /// transmit all the commands from this page to respective nRF slave
        /// </summary>
        void transmit()
        {
            try
            {

                byte tchecked = Convert.ToByte(buzzer.IsOn);
                byte ssrelaychecked = Convert.ToByte(relay_on_off.IsOn);

                rootPage.lastitem.device2_state = tchecked;
                rootPage.lastitem.device3_state = ssrelaychecked;
                rootPage.lastitem.device1_state = (byte)(slider.Value);

                byte[] bb =  {(byte)(slider.Value) ,tchecked,ssrelaychecked
            ,ssrelaychecked  ,ssrelaychecked , ssrelaychecked };

                rootPage.nrf_send_data(bb, rootPage.lastitem.device_address, MainPage.packet_type.data_packet);
                rootPage.update_devicelist(rootPage.lastitem);
            }
            catch(Exception ex)
            {
                rootPage.statusbar_string(ex.ToString(), true);
            }
           
        }

        //Any change in slider value intiates a transmission of new value for intensity
        //controlling
        private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {   
            transmit();
     
        } 

        ///Buzzer on/off
        private void buzzer_Toggled(object sender, RoutedEventArgs e)
        {
            transmit();
            if ((bool)buzzer.IsOn)
                rootPage.statusbar_string("Buzzer On", false);
            else
                rootPage.statusbar_string("Buzzer Off", true);
        }
         

        /// <summary>
        /// dim-----------bright---------dim loop
        /// </summary>
        bool isdim = false;
        private void dim_bright_Tick(object sender, object e)
        {
            if(isdim)
            {
                if(slider.Value == 140) 
                    isdim =  false; 
               else 
                    slider.Value += 5;  
            }
            else
            {
                if (slider.Value == 0) 
                    isdim =   true; 
                else 
                    slider.Value -= 5; 
            }
        }
         
        ///Triac 2 timer based control interval 
        private void day_hour(object sender, object e)
        {
            relay_on_off.IsOn = !relay_on_off.IsOn;
            if (repeat.IsChecked == false)
                day_delay_timer.Stop();
        }

        private void relay_on_off_Toggled(object sender, RoutedEventArgs e)
        {
            transmit();
        }

        private void TRIAC2_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            transmit();
        }

        //SOC option
        private void soc_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
            day_delay_timer.Interval = TimeSpan.FromDays(double.Parse(days.Text)) + TimeSpan.FromHours(double.Parse(hours.Text)) + TimeSpan.FromMinutes(double.Parse(minutes.Text)) +
                 TimeSpan.FromSeconds(double.Parse(seconds.Text));
            day_delay_timer.Start();
            }
            catch(Exception ex)
            {
                rootPage.statusbar_string(ex.ToString(), true);
            }

        }

        ///Auto dim brigh option (Not light intensity based, simple dim bright)
        private void toggle_dim_bright_Checked(object sender, RoutedEventArgs e)
        {
            dim_bright1.Start();
        }

        private void toggle_dim_bright_Unchecked(object sender, RoutedEventArgs e)
        {
            dim_bright1.Stop();
        }
    }
}
