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

using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SerialPortExample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private  async void SendLetter_Click(object sender, RoutedEventArgs e)
        {
            DeviceInformationCollection serialDeviceInfos = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelectorFromUsbVidPid(0x03EB,0x2404));
            foreach (DeviceInformation serialDeviceInfo in serialDeviceInfos)
            {
                try
                {
                    SerialDevice serialDevice = await SerialDevice.FromIdAsync(serialDeviceInfo.Id);

                    if (serialDevice != null)
                    {
                        // Found a valid serial device.

                        // Reading a byte from the serial device.
                      

                        // Writing a byte to the serial device.
                        DataWriter dw = new DataWriter(serialDevice.OutputStream);
                        dw.WriteByte(0x42);


                        DataReader dr = new DataReader(serialDevice.InputStream);
                       // int readByte = dr.ReadByte();
                    }
                }
                catch (Exception)
                {
                    // Couldn't instantiate the device
                }
            }
        }
    }
}
 