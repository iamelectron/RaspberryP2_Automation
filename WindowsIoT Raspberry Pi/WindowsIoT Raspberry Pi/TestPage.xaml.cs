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
    public sealed partial class TestPage : Page
    {
        private MainPage rootPage = MainPage.Current;
        public TestPage()
        {
            this.InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            byte[] bb = new byte[10];
            bb[0] = byte.Parse(b1.Text);
            bb[1] = byte.Parse(b2.Text);
            bb[2] = byte.Parse(b3.Text);
            bb[3] = byte.Parse(b4.Text);
            bb[4] = byte.Parse(b5.Text);
            bb[5] = byte.Parse(b6.Text);
            bb[6] = byte.Parse(b7.Text);
            bb[7] = byte.Parse(b8.Text);
            bb[8] = byte.Parse(b9.Text);
            bb[9] = byte.Parse(b10.Text); 
            rootPage.nrf_send_data(bb, 0x00, 0x00, true);
        }
    }
}
