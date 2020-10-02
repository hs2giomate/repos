using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ExcelOpener
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ValueSet table = null;
        public MainPage()
        {
            this.InitializeComponent();
            
            App.AppServiceConnected += MainPage_AppServiceConnected;
        }
        private async void MainPage_AppServiceConnected(object sender, EventArgs e)
        {
            // send the ValueSet to the fulltrust process
            AppServiceResponse response = await App.Connection.SendMessageAsync(table);

            // check the result
            object result;
            response.Message.TryGetValue("RESPONSE", out result);
            App.AppServiceDeferral.Complete();
            if (result.ToString() != "SUCCESS")
            {
                MessageDialog dialog = new MessageDialog(result.ToString());
                await dialog.ShowAsync();
            }
            // no longer need the AppService connection
            
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // create a ValueSet from the datacontext
            table = new ValueSet();
            table.Add("REQUEST", "CreateSpreadsheet");
            table.Add("TAG", "E- 0106-Ta2-X3-EVAP2");


            /*  for (int i = 0; i < items.Count; i++)
              {
                  table.Add("TAG", "W- 0334-ECS-X2-Cds Fan");
                  table.Add("Id" + i.ToString(), items[i].Id);
                  table.Add("Quantity" + i.ToString(), items[i].Quantity);
                  table.Add("UnitPrice" + i.ToString(), items[i].UnitPrice);
              }
            */

            // launch the fulltrust process and for it to connect to the app service            
            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }
            else
            {
                MessageDialog dialog = new MessageDialog("This feature is only available on Windows 10 Desktop SKU");
                await dialog.ShowAsync();
            }
        }
    }
}
