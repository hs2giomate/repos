using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using EventLoggerManagment;
using Windows.Storage.Streams;
using Windows.Foundation.Metadata;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MaintenanceToolECSBOX
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EventLoggerList : Page, IDisposable
    {
        // A pointer back to the main page.  This is needed if you want to call methods in MainPage such
        // as NotifyUser()
        private MainPage rootPage = MainPage.Current;


        // Track Read Operation
        private CancellationTokenSource ReadCancellationTokenSource;
        private Object ReadCancelLock = new Object();

        private Boolean IsReadTaskPending;
        private uint ReadBytesCounter = 0;
        private ValueSet table = null;
        DataReader DataReaderObject = null;
        DataReaderLoadOperation readerOperation = null;
        private string readText = null;

        // Track Write Operation
        private CancellationTokenSource WriteCancellationTokenSource;




        // Indicate if we navigate away from this page or not.
        private Boolean IsNavigatedAway;
        public ObservableCollection<EventItemValues> listOfMessages;
        public EventItemValues eventValue;
        private int messageNumber = 0;

        private const string reading = "Reading ...";
        private const string stopped = "Stopped ...";
        private const string start = "START";
        private const string stop = "STOP";
        private string[] messages = null;
        private Boolean onCycling = false;
        public EventLoggerList()
        {

            this.InitializeComponent();

            listOfMessages = new ObservableCollection<EventItemValues>();
            eventValue = new EventItemValues("Logger Activated");
            listOfMessages.Add(eventValue);
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
                rootPage.NotifyUser(result.ToString(), NotifyType.ErrorMessage);
            }else
            {
                rootPage.NotifyUser(result.ToString(), NotifyType.StatusMessage);
                do
                {
                    listOfMessages.RemoveAt(listOfMessages.Count-1);

                } while (listOfMessages.Count > 1);
            }
            // no longer need the AppService connection

        }
        public void Dispose()
        {
            if (ReadCancellationTokenSource != null)
            {
                ReadCancellationTokenSource.Dispose();
                ReadCancellationTokenSource = null;
            }

            if (WriteCancellationTokenSource != null)
            {
                WriteCancellationTokenSource.Dispose();
                WriteCancellationTokenSource = null;
            }
        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        ///
        /// We will enable/disable parts of the UI if the device doesn't support it.
        /// </summary>
        /// <param name="eventArgs">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {

            IsNavigatedAway = false;
            if (EventHandlerForDevice.Current.Device == null)
            {
                ReadWriteScollViewer.Visibility = Visibility.Collapsed;
                MainPage.Current.NotifyUser("Device is not connected", NotifyType.ErrorMessage);
            }
            else
            {
                if (EventHandlerForDevice.Current.Device.PortName != "")
                {
                    MainPage.Current.NotifyUser("Connected to " + EventHandlerForDevice.Current.Device.PortName,
                                                NotifyType.StatusMessage);
                }

                // So we can reset future tasks

                ResetReadCancellationTokenSource();


                UpdateReadButtonStates();
                ItemValuesSource.Source = listOfMessages;
                // CyclicReading();


            }
        }


        /// <summary>
        /// Cancel any on going tasks when navigating away from the page so the device is in a consistent state throughout
        /// all the scenarios
        /// </summary>
        /// <param name="eventArgs"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs eventArgs)
        {
            IsNavigatedAway = true;

            CancelAllIoTasks();
        }
        private async Task CyclicReading()
        {
            if ((StopReading.Content.ToString().Equals(stop)) & (IsNavigatedAway == false))
            {
                await StartReading();
                if (readText != string.Empty)
                {
                    messages = readText.Split("\r\n");
                    for (int i = 0; i < messages.Length; i++)
                    {
                        if (messages[i].Length>16)
                        {
                            AddItemToEnd(messages[i]);
                        }
                        
                    }
                    onCycling = true;
                    await CyclicReading();
                }


            }
            else
            {
                onCycling = false;
            }

        }

        private void AddItemToEnd(string s)
        {
            eventValue = new EventItemValues(s);
            listOfMessages.Add(eventValue);
            //   BottomUpList.Items.Add(itemValues);

        }
        private void DeleteSelectedItem()
        {
            int selectedIndex = BottomUpList.SelectedIndex;
            listOfMessages.RemoveAt(selectedIndex);
            //  listOfItems.Remove(selected);
            if (selectedIndex < BottomUpList.Items.Count)
            {
                BottomUpList.SelectedIndex = selectedIndex;
            }
        }

        private async void StopReading_Click(object sender, RoutedEventArgs e)
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                if (onCycling==true)
                {
                    StopReading.Content = start;
                    StatusEventLogger.Text = stopped;
                    
                    if (IsPerformingRead())
                    {
                        CancelReadTask();
                        onCycling = false;
                    }
                    UpdateExportExcelButton(true);
                }
                else
                {
                    UpdateExportExcelButton(false);
                    StopReading.Content = stop;
                    StatusEventLogger.Text = reading;
                    if (ExportExcel.IsEnabled==false)
                    {
                        await CyclicReading();
                    }
                }

              
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }
        }

        async private Task StartReading()
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    rootPage.NotifyUser("Reading....", NotifyType.StatusMessage);

                    // We need to set this to true so that the buttons can be updated to disable the read button. We will not be able to
                    // update the button states until after the read completes.
                    IsReadTaskPending = true;
                    DataReaderObject = new DataReader(EventHandlerForDevice.Current.Device.InputStream);
                    UpdateReadButtonStates();

                    await ReadAsync(ReadCancellationTokenSource.Token);
                }
                catch (OperationCanceledException /*exception*/)
                {
                    NotifyReadTaskCanceled();
                }
                catch (Exception exception)
                {
                    MainPage.Current.NotifyUser(exception.Message.ToString(), NotifyType.ErrorMessage);
                    Debug.WriteLine(exception.Message.ToString());
                }
                finally
                {
                    IsReadTaskPending = false;
                    DataReaderObject.DetachStream();
                    DataReaderObject = null;

                    UpdateReadButtonStates();
                }
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }
        }
        private async Task ReadAsync(CancellationToken cancellationToken)
        {

            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 128;
            readText = string.Empty;
            // Don't start any IO if we canceled the task
            lock (ReadCancelLock)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Cancellation Token will be used so we can stop the task operation explicitly
                // The completion function should still be called so that we can properly handle a canceled task
                DataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
                readerOperation = DataReaderObject.LoadAsync(ReadBufferLength);

                loadAsyncTask = readerOperation.AsTask(cancellationToken);
            }

            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                readText = DataReaderObject.ReadString(bytesRead);
                ReadBytesCounter = bytesRead;
            }
            else
            {
                readText = null;
            }
            rootPage.NotifyUser("Read completed - " + bytesRead.ToString() + " bytes were read", NotifyType.StatusMessage);
        }
        private void UpdateReadButtonStates()
        {
            if (IsPerformingRead())
            {
                ExportExcel.IsEnabled = false;
                DeleteItem.IsEnabled = false;

            }
            else
            {

            }
        }
        private bool UpdateExportExcelButton(bool state)
        {
            if ((!IsPerformingRead()) & (StopReading.Content.ToString().Equals(start)))
            {
                ExportExcel.IsEnabled = state;
            }
            return ExportExcel.IsEnabled;

        }

        private async void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            table = new ValueSet();
            table.Add("REQUEST", "CreateSpreadsheet");
           


             for (int i = 0; i < listOfMessages.Count; i++)
             {
                  table.Add("DateTime"+i.ToString(), listOfMessages[i].Datetime);
                  table.Add("Event" + i.ToString(), listOfMessages[i].EventName);
                  table.Add("Description" + i.ToString(), listOfMessages[i].Description);
                  
             }
            

            // launch the fulltrust process and for it to connect to the app service            
            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }
            else
            {
                rootPage.NotifyUser("This feature is only available on Windows 10 Desktop SKU",NotifyType.ErrorMessage);
                
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (BottomUpList.SelectedItems.Count > 0)
            {
                DeleteSelectedItem();
            }

        }
        private void UpdateDeleteItemButtom()
        {
            DeleteItem.IsEnabled = BottomUpList.SelectedItems.Count > 0;

        }
        private void CancelReadTask()
        {
            lock (ReadCancelLock)
            {
                if (ReadCancellationTokenSource != null)
                {
                    if (!ReadCancellationTokenSource.IsCancellationRequested)
                    {
                        ReadCancellationTokenSource.Cancel();

                        // Existing IO already has a local copy of the old cancellation token so this reset won't affect it
                        ResetReadCancellationTokenSource();
                    }
                }
            }
        }
        private void CancelAllIoTasks()
        {
            CancelReadTask();
            // CancelWriteTask();
        }

        /// <summary>
        /// Determines if we are reading, writing, or reading and writing.
        /// </summary>
        /// <returns>If we are doing any of the above operations, we return true; false otherwise</returns>
        private Boolean IsPerformingRead()
        {
            return (IsReadTaskPending);
        }



        private void ResetReadCancellationTokenSource()
        {
            // Create a new cancellation token source so that can cancel all the tokens again
            ReadCancellationTokenSource = new CancellationTokenSource();

            // Hook the cancellation callback (called whenever Task.cancel is called)
            ReadCancellationTokenSource.Token.Register(() => NotifyReadCancelingTask());
        }



        /// <summary>
        /// Notifies the UI that the operation has been cancelled
        /// </summary>
        private async void NotifyReadTaskCanceled()
        {
            await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                new DispatchedHandler(() =>
                {
                    if (!IsNavigatedAway)
                    {
                        rootPage.NotifyUser("Read request has been cancelled", NotifyType.StatusMessage);
                    }
                }));
        }


        /// <summary>
        /// Print a status message saying we are canceling a task and disable all buttons to prevent multiple cancel requests.
        /// <summary>
        private async void NotifyReadCancelingTask()
        {
            // Setting the dispatcher priority to high allows the UI to handle disabling of all the buttons
            // before any of the IO completion callbacks get a chance to modify the UI; that way this method
            // will never get the opportunity to overwrite UI changes made by IO callbacks
            await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                new DispatchedHandler(() =>
                {
                    ExportExcel.IsEnabled = true;
                    StopReading.Content = start;
                    if (!IsNavigatedAway)
                    {
                        rootPage.NotifyUser("Canceling Read... Please wait...", NotifyType.StatusMessage);
                    }
                }));
        }



        private void BottomUpList_ItemClick(object sender, ItemClickEventArgs e)
        {
            UpdateDeleteItemButtom();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await CyclicReading();
        }

        private void ItemsStackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UpdateDeleteItemButtom();
        }
    }
}
