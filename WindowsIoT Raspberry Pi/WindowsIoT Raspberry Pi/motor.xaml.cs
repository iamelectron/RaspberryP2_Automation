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
    public sealed partial class motor : Page
    {
        private MainPage rootPage = MainPage.Current;
        byte[] tx_dat = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public motor()
        {
            this.InitializeComponent();
        }


        void transmit()
        {
            rootPage.nrf_send_data(tx_dat, rootPage.lastitem.device_address, 
                MainPage.packet_type.data_packet);
             
        }

        private void bup_KeyDown(object sender, RoutedEventArgs e)
        {
            tx_dat[0] = 1;
            tx_dat[1] = 0;

            tx_dat[2] = 1; 
            tx_dat[3] = 0;

            tx_dat[4] = 0;
            tx_dat[5] = 0; 
            transmit();
        }

        private void bup_KeyUp(object sender, RoutedEventArgs e)
        {
            tx_dat[0] = 0;
            tx_dat[1] = 0;

            tx_dat[2] = 0;
            tx_dat[3] = 0;

            tx_dat[4] = 0;
            tx_dat[5] = 0;
            transmit();
        }

        private void bdown_KeyDown(object sender, RoutedEventArgs e)
        {
            tx_dat[0] = 0;
            tx_dat[1] = 1;

            tx_dat[2] = 0;
            tx_dat[3] = 1;

            tx_dat[4] = 1;
            tx_dat[5] = 1;
            transmit(); 

        }

        private void bdown_KeyUp(object sender, RoutedEventArgs e)
        {
            tx_dat[0] = 0;
            tx_dat[1] = 0;

            tx_dat[2] = 0;
            tx_dat[3] = 0;

            tx_dat[4] = 0;
            tx_dat[5] = 0;
            transmit();
        }

        private void bright_KeyDown(object sender, RoutedEventArgs e)
        {
            tx_dat[0] = 1;
            tx_dat[1] = 0;

            tx_dat[2] = 0;
            tx_dat[3] = 1;

            tx_dat[4] = 1;
            tx_dat[5] = 1;
            transmit();
        }

        private void bright_KeyUp(object sender, RoutedEventArgs e)
        {
            tx_dat[0] = 0;
            tx_dat[1] = 0;

            tx_dat[2] = 0;
            tx_dat[3] = 0;

            tx_dat[4] = 0;
            tx_dat[5] = 0;
            transmit();
        }

        private void bleft_KeyDown(object sender, RoutedEventArgs e)
        {
            tx_dat[0] = 0;
            tx_dat[1] = 1;

            tx_dat[2] = 1;
            tx_dat[3] = 0;

            tx_dat[4] = 0;
            tx_dat[5] = 0;
            transmit();
        }

        private void bleft_KeyUp(object sender, RoutedEventArgs e)
        {
            tx_dat[0] = 0;
            tx_dat[1] = 0;

            tx_dat[2] = 0;
            tx_dat[3] = 0;

            tx_dat[4] = 0;
            tx_dat[5] = 0;
            transmit();
        }
         
    }
}
