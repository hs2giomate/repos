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
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using MaintenanceToolProtocol;

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
        private static EventLoggerList handler;

        // Track Read Operation
        private CancellationTokenSource ReadCancellationTokenSource;
        private Object ReadCancelLock = new Object();

        private Boolean IsReadTaskPending,got_termination_bytes;
        private uint ReadBytesCounter = 0;
        private ValueSet table = null;
        DataReader DataReaderObject = null;
        DataReaderLoadOperation readerOperation = null;
        private string readText = null;
        DataWriter DataWriteObject = null;
        private CancellationTokenSource WriteCancellationTokenSource;
        private Object WriteCancelLock = new Object();
        private int null_counter = 0;
        

        // Track Write Operation
         private Boolean IsWriteTaskPending, request_sucess, read_request_success,block_read;




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
        private static System.Timers.Timer aTimer = null;
        private const int UPDATING_TIME = 1000;
        private const int MAXIMUM_BUFFERS = 64;
        private bool updatingLogger = false;
        private UInt32 magicHeader = 0;
        private UInt32 block_address_read = 0;
        private  const uint ReadBufferLength = 64;
         private static Byte[] received, toSend, byte_message, block_received;
        private int sizeofStruct;
        private bool on_reading_state=true;
        private bool stop_button_enabled = false;
        private UInt32 start_memory_address = 4 * 1024 * 6;
        private UInt32 memory_block_size = 4 * 1024 ;
        private UInt32 memory_flash_size  = 2*1028*1028;
        private DataLogAddressMessage dataLogMessage;
        private int datalog_size;
        private UInt32 current_memory_address,ucontroller_timestamp, host_timestamp,on_received_host_timestamp,adjusted_timestamp,offset_timestamp;
        private int start_index, current_index, end_line_index,next_index;
        private    EventLoggerManagment.DataLogItem data_log_item;
        private string single_message;
        public EventLoggerList()
        {

            this.InitializeComponent();
            handler = this;
            sizeofStruct = Marshal.SizeOf(typeof(SingleTaskMessage));
            received = new byte[ReadBufferLength];
            block_received = new byte[memory_block_size];
            listOfMessages = new ObservableCollection<EventItemValues>();
            eventValue = new EventItemValues("Logger Activated");
            listOfMessages.Add(eventValue);
            App.AppServiceConnected += MainPage_AppServiceConnected;
            datalog_size= Marshal.SizeOf(dataLogMessage);
            current_memory_address = start_memory_address;

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
                ResetWriteCancellationTokenSource();

                UpdateReadButtonStates();
                UpdateLogData();
                StartStatusCheckTimer();
                ItemValuesSource.Source = listOfMessages;
                // CyclicReading();


            }
        }
        public void StartStatusCheckTimer()
        {
            // Create a timer and set a two second interval.
            aTimer = new System.Timers.Timer();
            aTimer.Interval = UPDATING_TIME;

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;

            // Have the timer fire repeated events (true is the default)
            aTimer.AutoReset = false;

            // Start the timer
            aTimer.Enabled = true;



        }
        public async void UpdateLogData()
        {
            updatingLogger = true;
            SetStopButtonStates(false);
            request_sucess = false;
            await RequestLogData();
            if (request_sucess)
            {
                got_termination_bytes = false;
                on_reading_state = true;
            
                read_request_success = false;
                await ReadLogData();
                if (read_request_success)
                {
                    if (block_address_read==current_memory_address)
                    {
                        ucontroller_timestamp= BitConverter.ToUInt32(received, 10);
                        on_received_host_timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds;
                        read_request_success = false;
                        await ReadBlockData();
                        if (read_request_success)
                        {
                           
                           DecodeMessages()
                            SetStopButtonStates(true);
                        }

                    }

                   
               
                }
                else
                {
                    
                }
            
                on_reading_state = false;
                
            }

            SetStopButtonStates(true);
            updatingLogger = false;
        }
        private  bool SetStopButtonStates( bool st)
        {
            StopReading.IsEnabled = st;
            ExportExcel.IsEnabled = st;
            return st;

        } 
        private static async void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {

            if (handler.IsPerformingWrite() | handler.IsPerformingRead())
            {

            }
            else
            {
                await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                   new DispatchedHandler(() => {
                       handler.stop_button_enabled=handler.StopReading.Content.Equals(stop);
                   }));

                if (!handler.stop_button_enabled)
                {

                }
                else
                {
                    await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                     new DispatchedHandler(() => {
                         handler.UpdateLogData();
                     }));
                    
                }
                
            }

            aTimer.Start();



        }
        private async Task RequestLogData()
        {
            if (IsPerformingWrite())
            {
                rootPage.NotifyUser("already requesting to Logger...", NotifyType.WarningMessage);
            }
            else
            {

                if (IsPerformingRead())
                {
                    
                    rootPage.NotifyUser("already reading...", NotifyType.WarningMessage);



                }
                else
                {
                    if (EventHandlerForDevice.Current.IsDeviceConnected)
                    {
                        try
                        {
                            rootPage.NotifyUser("Sending request to Logger...", NotifyType.StatusMessage);

                            // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                            // update the button states until after the write completes.
                            toSend = new byte[datalog_size];
                            Protocol.Message.CreateDataLogRequestMessage(current_memory_address).CopyTo(toSend, 0);
                            IsWriteTaskPending = true;
                            DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                            //UpdateWriteButtonStates();

                            await SendrequestStatusAsync(WriteCancellationTokenSource.Token);
                        }
                        catch (OperationCanceledException /*exception*/)
                        {
                            NotifyWriteTaskCanceled();
                        }
                        catch (Exception exception)
                        {
                            rootPage.NotifyUser(exception.Message.ToString(), NotifyType.ErrorMessage);
                            
                        }
                        finally
                        {
                            IsWriteTaskPending = false;
                            try
                            {
                                DataWriteObject.DetachStream();
                            }
                            catch (Exception e)
                            {
                                rootPage.NotifyUser(e.Message.ToString(), NotifyType.ErrorMessage);
                                // throw;
                            }

                            DataWriteObject.Dispose();

                            // UpdateWriteButtonStates();
                        }
                    }
                    else
                    {
                        Utilities.NotifyDeviceNotConnected();
                    }
                }
               




            }

        }
        private async void NotifyWriteCancelingTask()
        {
            // Setting the dispatcher priority to high allows the UI to handle disabling of all the buttons
            // before any of the IO completion callbacks get a chance to modify the UI; that way this method
            // will never get the opportunity to overwrite UI changes made by IO callbacks
            await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                new DispatchedHandler(() =>
                {
                    //  WriteOffsetButton.IsEnabled = false;


                    if (!IsNavigatedAway)
                    {
                        rootPage.NotifyUser("Canceling Write... Please wait...", NotifyType.StatusMessage);
                    }
                }));
        }
        private async void NotifyWriteTaskCanceled()
        {
            await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                new DispatchedHandler(() =>
                {
                    if (!IsNavigatedAway)
                    {
                        rootPage.NotifyUser("Write request has been cancelled", NotifyType.StatusMessage);
                    }
                }));
        }
        private Boolean IsPerformingWrite()
        {
            return (IsWriteTaskPending);
        }
        private async Task SendrequestStatusAsync(CancellationToken cancellationToken)
        {

            Task<UInt32> storeAsyncTask;


            DataWriteObject.WriteBytes(toSend);


            // Don't start any IO if we canceled the task
            lock (WriteCancelLock)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Cancellation Token will be used so we can stop the task operation explicitly
                // The completion function should still be called so that we can properly handle a canceled task
                storeAsyncTask = DataWriteObject.StoreAsync().AsTask(cancellationToken);
            }

            UInt32 bytesWritten = await storeAsyncTask;
            if (bytesWritten > 0)
            {
                request_sucess = true;
            }
            rootPage.NotifyUser("Request Logger Completed -  ", NotifyType.StatusMessage);


        }
        private async Task ReadLogData()
        {
            if (IsPerformingWrite())
            {
                rootPage.NotifyUser("busy on writting data...", NotifyType.StatusMessage);
            }
            else
            {
                if (IsPerformingRead())
                {
                  
                    rootPage.NotifyUser("Cancelling reading task...", NotifyType.WarningMessage);


                }
                else
                {
                    if (EventHandlerForDevice.Current.IsDeviceConnected)
                    {
                        try
                        {
                            rootPage.NotifyUser("Reading Logger Data Status...", NotifyType.StatusMessage);

                            // We need to set this to true so that the buttons can be updated to disable the read button. We will not be able to
                            // update the button states until after the read completes.
                            IsReadTaskPending = true;
                            DataReaderObject = new DataReader(EventHandlerForDevice.Current.Device.InputStream);
                            // UpdateReadButtonStates();

                            await ReadDataLogAsync(ReadCancellationTokenSource.Token);
                        }
                        catch (OperationCanceledException /*exception*/)
                        {
                            NotifyReadTaskCanceled();
                            IsReadTaskPending = false;
                            // Debug.WriteLine("ReadOperation cancelled");
                        }
                        catch (Exception exception)
                        {
                            rootPage.NotifyUser(exception.Message.ToString(), NotifyType.ErrorMessage);
                            IsReadTaskPending = false;
                        }
                        finally
                        {
                            IsReadTaskPending = false;
                            
                            try
                            {
                                DataReaderObject.DetachStream();
                            }
                            catch (Exception e)
                            {
                                rootPage.NotifyUser(e.Message.ToString(), NotifyType.ErrorMessage);
                                // throw;
                            }

                            DataReaderObject.Dispose();


                        }
                    }
                    else
                    {
                        Utilities.NotifyDeviceNotConnected();
                    }
                }
               
            }


        }
        private async Task ReadBlockData()
        {
            if (IsPerformingWrite())
            {
                rootPage.NotifyUser("busy on writting data...", NotifyType.StatusMessage);
            }
            else
            {
                if (IsPerformingRead())
                {

                    rootPage.NotifyUser("Cancelling reading task...", NotifyType.WarningMessage);


                }
                else
                {
                    if (EventHandlerForDevice.Current.IsDeviceConnected)
                    {
                        try
                        {
                            rootPage.NotifyUser("Reading Logger Data Status...", NotifyType.StatusMessage);

                            // We need to set this to true so that the buttons can be updated to disable the read button. We will not be able to
                            // update the button states until after the read completes.
                            IsReadTaskPending = true;
                            DataReaderObject = new DataReader(EventHandlerForDevice.Current.Device.InputStream);
                            // UpdateReadButtonStates();

                            await ReadBlockDataLogAsync(ReadCancellationTokenSource.Token);
                        }
                        catch (OperationCanceledException /*exception*/)
                        {
                            NotifyReadTaskCanceled();
                            IsReadTaskPending = false;
                            // Debug.WriteLine("ReadOperation cancelled");
                        }
                        catch (Exception exception)
                        {
                            rootPage.NotifyUser(exception.Message.ToString(), NotifyType.ErrorMessage);
                            IsReadTaskPending = false;
                        }
                        finally
                        {
                            IsReadTaskPending = false;
                            readText = System.Text.Encoding.UTF8.GetString(block_received);
                            try
                            {
                                DataReaderObject.DetachStream();
                            }
                            catch (Exception e)
                            {
                                rootPage.NotifyUser(e.Message.ToString(), NotifyType.ErrorMessage);
                                // throw;
                            }

                            DataReaderObject.Dispose();


                        }
                    }
                    else
                    {
                        Utilities.NotifyDeviceNotConnected();
                    }
                }

            }


        }
        private async Task ReadDataLogAsync(CancellationToken cancellationToken)
        {

            Task<UInt32> loadAsyncTask;
            readText = string.Empty;
            // Don't start any IO if we canceled the task
            lock (ReadCancelLock)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Cancellation Token will be used so we can stop the task operation explicitly
                // The completion function should still be called so that we can properly handle a canceled task
                DataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
                loadAsyncTask = DataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);
            }

            UInt32 bytesRead = await loadAsyncTask;
            //  Debug.WriteLine(string.Concat("bytes Read", bytesRead.ToString()));
            if (bytesRead > 0)
            {


                DataReaderObject.ReadBytes(received);
                //readText = DataReaderObject.ReadString(bytesRead);
                block_address_read = BitConverter.ToUInt32(received, 6);
                read_request_success = true;



            }
            else
            {
                readText = null;
            }
            rootPage.NotifyUser("Read Logger Data completed ", NotifyType.StatusMessage);
        }
        private async Task ReadBlockDataLogAsync(CancellationToken cancellationToken)
        {

            Task<UInt32> loadAsyncTask;
            readText = string.Empty;
            // Don't start any IO if we canceled the task
            lock (ReadCancelLock)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Cancellation Token will be used so we can stop the task operation explicitly
                // The completion function should still be called so that we can properly handle a canceled task
                DataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
                loadAsyncTask = DataReaderObject.LoadAsync(memory_block_size).AsTask(cancellationToken);
            }

            UInt32 bytesRead = await loadAsyncTask;
            //  Debug.WriteLine(string.Concat("bytes Read", bytesRead.ToString()));
            if (bytesRead > 0)
            {


                DataReaderObject.ReadBytes(block_received);
                //readText = DataReaderObject.ReadString(bytesRead);
               
                read_request_success = true;
                if (bytesRead>= memory_block_size)
                {
                    if (current_memory_address+memory_block_size>memory_flash_size)
                    {
                        current_memory_address = start_memory_address;
                    }
                    else
                    {
                        current_memory_address += memory_block_size;
                    }
                 
                }



            }
            else
            {
                readText = null;
            }
            rootPage.NotifyUser("Read Logger Data completed ", NotifyType.StatusMessage);
        }

        /// <summary>
        /// Cancel any on going tasks when navigating away from the page so the device is in a consistent state throughout
        /// all the scenarios
        /// </summary>
        /// <param name="eventArgs"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs eventArgs)
        {
            IsNavigatedAway = true;
            if (aTimer != null)
            {
                aTimer.Stop();
                aTimer.Dispose();
            }
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

        private   void ShowMessages()
        {
            if (readText != string.Empty)
            {
                null_counter = 0;
                messages = readText.Split("\n");
                for (int i = 0; i < messages.Length; i++)
                {
                    if (messages[i].Equals(Char.MinValue))
                    {
                        null_counter++;
                    }
                    else
                    {

                        null_counter = 0;
                        if (messages[i].Length > 8)
                        {
                            AddItemToEnd(messages[i]);
                        }
                    }
                    if (null_counter>1)
                    {
                        break;
                    }
                 

                }
                
                
            }
        }
        private void DecodeMessages()
        {
            if (block_received.Length>=memory_block_size)
            {
                null_counter = 0;
                current_index = 0;
                while (null_counter < 1)
                {
                    data_log_item.timestamp = DecodeTimestamp(current_index);
                    next_index = FindEndLine(current_index);
                    single_message= System.Text.Encoding.UTF8.GetString(block_received,current_index,next_index-current_index);
                    if (single_message!=string.Empty)
                    {
                        data_log_item.message = single_message;
                        AddDataLogItemToEnd(data_log_item);

                    }
                    else
                    {
                        null_counter++;
                    }
                }
            }
        }
        private int FindEndLine(int si)
        {

           end_line_index= Array.FindIndex<Byte>(block_received, si, x => x == 0x0a);
            return end_line_index;
        }
        private UInt32 DecodeTimestamp(int index)
        {

            offset_timestamp = on_received_host_timestamp - ucontroller_timestamp;

            adjusted_timestamp = BitConverter.ToUInt32(received, index) + offset_timestamp;

            return  adjusted_timestamp;
        }

        private void AddItemToEnd(string s)
        {
            eventValue = new  EventLoggerManagment.EventItemValues(s);
            
            listOfMessages.Add(eventValue);
            //   BottomUpList.Items.Add(itemValues);

        }
        private void AddDataLogItemToEnd(DataLogItem dli)
        {
            eventValue = new EventItemValues(dli.message);
            eventValue.CreateTimeStamp(dli)
         
            listOfMessages.Add(eventValue);


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
            if (IsPerformingRead()|IsPerformingWrite())
            {

            }
            else
            {
                table = new ValueSet();
                table.Add("REQUEST", "CreateSpreadsheet");



                for (int i = 0; i < listOfMessages.Count; i++)
                {
                    table.Add("DateTime" + i.ToString(), listOfMessages[i].Datetime);
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
                    rootPage.NotifyUser("This feature is only available on Windows 10 Desktop SKU", NotifyType.ErrorMessage);

                }
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
          //  await CyclicReading();
        }

        private void ItemsStackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UpdateDeleteItemButtom();
        }
        private void ResetWriteCancellationTokenSource()
        {
            // Create a new cancellation token source so that can cancel all the tokens again
            WriteCancellationTokenSource = new CancellationTokenSource();

            // Hook the cancellation callback (called whenever Task.cancel is called)
            WriteCancellationTokenSource.Token.Register(() => NotifyWriteCancelingTask());
        }
       
    }
}
