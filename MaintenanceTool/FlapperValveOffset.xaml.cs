using System;
using System.Diagnostics;
using System.Threading;

using Windows.UI.Xaml;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

using Windows.UI.Xaml.Navigation;

using Windows.Devices.SerialCommunication;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using ParameterTransferProtocol;
using MaintenanceToolProtocol;
using System.Text;


using System.Runtime.InteropServices;




// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MaintenanceToolECSBOX
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FlapperValveOffset : Page, IDisposable
    {
        private MainPage rootPage = MainPage.Current;
        // Track Read Operation
        private CancellationTokenSource ReadCancellationTokenSource;
        private Object ReadCancelLock = new Object();

        private Boolean IsReadTaskPending;
        private uint ReadBytesCounter = 0;
        DataReader DataReaderObject = null;
        public   Byte    Offset { get; set; }

        private CancellationTokenSource WriteCancellationTokenSource;
        private Object WriteCancelLock = new Object();
        private Boolean IsWriteTaskPending;
        DataWriter DataWriteObject = null;
        private Boolean IsNavigatedAway;
        private  static Byte[] received,toSend;
        UserParameters parameters;
        ParametersMessage parametersMessage;
        public FlapperValveOffset()
        {
            this.InitializeComponent();
            parameters.flapperValveOffset = Offset;
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
        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {

            IsNavigatedAway = false;
            if (EventHandlerForDevice.Current.Device == null)
            {
                ScrollerToggleBits.Visibility = Visibility.Collapsed;
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
                if (!EventHandlerForDevice.Current.configurationUpdated)
                {
                  //  EventHandlerForDevice.Current.SetDeaultParameters();
                }

                ResetReadCancellationTokenSource();
                ResetWriteCancellationTokenSource();

                UpdateReadButtonStates();
                UpdateWriteButtonStates();
         
               // InitialOffsetRead();

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

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void EventsScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {

        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {

        }
        private void UpdateOffsetValue()
        {
            
           OffsetValueText.Text=String.Concat(ConvertOffsetToAngle(Offset).ToString("N0")," °");
        }

        private void FVToggleBit8_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit8.IsOn)
            {
                Offset =(Byte)(Offset | 0x80);
                FVToggleBitValue8.Text = "1";
            }
            else
            {
                Offset = (Byte)(Offset & 0x7F);
                FVToggleBitValue8.Text = "0";
            }
            UpdateOffsetValue();
        }
        private void UpdateFVToggleBit8()
        {
            bool currentState = (Offset & 0x80)>1;
            FVToggleSwitchBit8.IsOn = currentState;
        }

        private void FVToggleBit7_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit7.IsOn)
            {
                Offset = (Byte)(Offset | 0x40);
                FVToggleBitValue7.Text = "1";
            }
            else
            {
                Offset = (Byte)(Offset & 0xBF);
                FVToggleBitValue7.Text = "0";
            }
            UpdateOffsetValue();
        }
        private void UpdateFVToggleBit7()
        {
            bool currentState = (Offset & 0x40) > 1;
            FVToggleSwitchBit7.IsOn = currentState;
        }


        private void FVToggleBit6_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit6.IsOn)
            {
                Offset = (Byte)(Offset | 0x20);
                FVToggleBitValue6.Text = "1";
            }
            else
            {
                Offset = (Byte)(Offset & 0xDF);
                FVToggleBitValue6.Text = "0";
            }
            UpdateOffsetValue();
        }
        private void UpdateFVToggleBit6()
        {
            bool currentState = (Offset & 0x20) > 1;
            FVToggleSwitchBit6.IsOn = currentState;
        }


        private void FVToggleBit5_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit5.IsOn)
            {
                Offset = (Byte)(Offset | 0x10);
                FVToggleBitValue5.Text = "1";
            }
            else
            {
                Offset = (Byte)(Offset & 0xEF);
                FVToggleBitValue5.Text = "0";
            }
            UpdateOffsetValue();
        }
        private void UpdateFVToggleBit5()
        {
            bool currentState = (Offset & 0x10) > 1;
            FVToggleSwitchBit5.IsOn = currentState;
        }

        private void FVToggleBit4_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit4.IsOn)
            {
                Offset = (Byte)(Offset | 0x08);
                FVToggleBitValue4.Text = "1";
            }
            else
            {
                Offset = (Byte)(Offset & 0xF7);
                FVToggleBitValue4.Text = "0";
            }
            UpdateOffsetValue();
        }
        private void UpdateFVToggleBit4()
        {
            bool currentState = (Offset & 0x08) > 1;
            FVToggleSwitchBit4.IsOn = currentState;
        }
        private void FVToggleBit3_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit3.IsOn)
            {
                Offset = (Byte)(Offset | 0x04);
                FVToggleBitValue3.Text = "1";
            }
            else
            {
                Offset = (Byte)(Offset & 0xFB);
                FVToggleBitValue3.Text = "0";
            }
            UpdateOffsetValue();
        }
        private void UpdateFVToggleBit3()
        {
            bool currentState = (Offset & 0x04) > 1;
            FVToggleSwitchBit3.IsOn = currentState;
        }
        private void FVToggleBit2_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit2.IsOn)
            {
                Offset = (Byte)(Offset | 0x02);
                FVToggleBitValue2.Text = "1";
            }
            else
            {
                Offset = (Byte)(Offset & 0xFD);
                FVToggleBitValue2.Text = "0";
            }
            UpdateOffsetValue();
        }
        private void UpdateFVToggleBit2()
        {
            bool currentState = (Offset & 0x02) > 1;
            FVToggleSwitchBit2.IsOn = currentState;
        }

        private void FVToggleBit1_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit1.IsOn)
            {
                Offset = (Byte)(Offset | 0x01);
                FVToggleBitValue1.Text = "1";
            }
            else
            {
                Offset = (Byte)(Offset & 0xFE);
                FVToggleBitValue1.Text = "0";
            }
            UpdateOffsetValue();
        }
        private void UpdateFVToggleBit1()
        {
            bool currentState = (Offset & 0x01) > 0;
            FVToggleSwitchBit1.IsOn = currentState;
        }
        private void UpdateAllToggleBits()
        {
            UpdateFVToggleBit8();
            UpdateFVToggleBit7();
            UpdateFVToggleBit6();
            UpdateFVToggleBit5();
            UpdateFVToggleBit4();
            UpdateFVToggleBit3();
            UpdateFVToggleBit2();
            UpdateFVToggleBit1();
        }
        private async void WriteOffsetButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPerformingRead())
            {
                CancelReadTask();
            }
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    rootPage.NotifyUser("Writing  Offset...", NotifyType.StatusMessage);

                    // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                    // update the button states until after the write completes.
                    IsWriteTaskPending = true;
                    DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                    UpdateWriteButtonStates();

                    await WriteAsync(WriteCancellationTokenSource.Token);
                }
                catch (OperationCanceledException /*exception*/)
                {
                    NotifyWriteTaskCanceled();
                }
                catch (Exception exception)
                {
                    MainPage.Current.NotifyUser(exception.Message.ToString(), NotifyType.ErrorMessage);
                    Debug.WriteLine(exception.Message.ToString());
                }
                finally
                {
                    IsWriteTaskPending = false;
                    DataWriteObject.DetachStream();
                    DataWriteObject = null;

                    UpdateWriteButtonStates();
                }
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }
        }

       private async  Task GetStoredOffsetValue()
        {
           Task query= QueryParametersValues();
            await query;
            if (IsPerformingWrite())
            {
                Debug.WriteLine("IsperformingWrite") ;
               // CancelAllIoTasks();
               //  NotifyWriteTaskCanceled();

            }
            else
            {
               Task ro = ReadOffsetValue();
                await ro;
            }
            //WriteOffsetButton.IsEnabled = false;
           
           // WriteOffsetButton.IsEnabled = true;
        }
        private async Task QueryParametersValues()
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    rootPage.NotifyUser(" Writing  Command...", NotifyType.StatusMessage);

                    // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                    // update the button states until after the write completes.
                    IsWriteTaskPending = true;
                    DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                    UpdateWriteButtonStates();
                    // await SendMagic(WriteCancellationTokenSource.Token);
                    await SendReadParametersCommand(WriteCancellationTokenSource.Token);


                }
                catch (OperationCanceledException /*exception*/)
                {
                    NotifyWriteTaskCanceled();
                    Debug.WriteLine("WriteOperation cancelled");
                }
                catch (Exception exception)
                {
                    MainPage.Current.NotifyUser(exception.Message.ToString(), NotifyType.ErrorMessage);
                    Debug.WriteLine(exception.Message.ToString());
                }
                finally
                {
                    IsWriteTaskPending = false;
                    DataWriteObject.DetachStream();
                    DataWriteObject = null;

                    UpdateWriteButtonStates();
                }
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }
        }
        private void UpdateReadButtonStates()
        {
            if (IsPerformingRead())
            {
                ReadOffsetButton.IsEnabled = false;
               // WriteOffsetButton.IsEnabled = false;

            }
            else
            {
                ReadOffsetButton.IsEnabled = true;
              //  WriteOffsetButton.IsEnabled = true;

            }
        }

        private void UpdateWriteButtonStates()
        {
            if (IsPerformingWrite())
            {
                WriteOffsetButton.IsEnabled = false;
               // ReadOffsetButton.IsEnabled = false;
            }
            else
            {
              //  ReadOffsetButton.IsEnabled = true;
                WriteOffsetButton.IsEnabled = true;

            }
        }
        private Boolean IsPerformingRead()
        {
            return (IsReadTaskPending);
        }

        private Boolean IsPerformingWrite()
        {
            return (IsWriteTaskPending);
        }
        private async Task ReadAsync(CancellationToken cancellationToken)
        {

            Task<UInt32> loadAsyncTask;
           
            uint ReadBufferLength = 64;
           
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
            Debug.WriteLine(string.Concat("bytes Read" ,bytesRead.ToString()));
            if (bytesRead > 0)
            {
                received = new byte[ReadBufferLength];
            
                DataReaderObject.ReadBytes(received);
                Debug.WriteLine(string.Concat("Bytes received: " , received.ToString()));
                ParametersStruct receivedParameters = new ParametersStruct();
                UserParameters p = receivedParameters.ConvertBytesParameters(received);
                Offset = p.flapperValveOffset;
                ReadOffsetValueText.Text = String.Concat(ConvertOffsetToAngle(Offset).ToString("N0"), " °");
                ReadBytesCounter += bytesRead;
                
             }
            rootPage.NotifyUser("Read completed - " + bytesRead.ToString() + " bytes were read", NotifyType.StatusMessage);
        }
        private int ConvertOffsetToAngle(Byte o)
        {
            int angle = ((int)(o)) * 90 / 255;
            return angle;
        }

        private async Task WriteAsync(CancellationToken cancellationToken)
        {

            Task<UInt32> storeAsyncTask;

            if (Offset>0)
            {

                parameters.flapperValveOffset = Offset;
                var n = Marshal.SizeOf(typeof(ParametersMessage));
                toSend = new byte[n];
                toSend.Initialize();

               ParametersProtocol.Current.CreateWriteParametersMessage(parameters).CopyTo(toSend,0);

                Byte[] toSend2 = Encoding.ASCII.GetBytes("abcd<Gf");

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
               rootPage.NotifyUser("Write completed - " + bytesWritten.ToString() + " bytes written", NotifyType.StatusMessage);
            }
            else
            {
                rootPage.NotifyUser("No input received to write", NotifyType.StatusMessage);
            }

        }
        private async Task SendReadParametersCommand(CancellationToken cancellationToken)
        {

            Task<UInt32> storeAsyncTask;

            //   ParametersProtocol datagram = new ParametersProtocol();
            // Byte[] flushByteArray = datagram.CreateReadParametersRequest();
            //  DataWriteObject.WriteBytes(flushByteArray);
           
           
            String InputString = "abcd>H";
            DataWriteObject.WriteString(InputString);

            // Don't start any IO if we canceled the task
            lock (WriteCancelLock)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Cancellation Token will be used so we can stop the task operation explicitly
                // The completion function should still be called so that we can properly handle a canceled task
                storeAsyncTask = DataWriteObject.StoreAsync().AsTask(cancellationToken);
            }

                UInt32 bytesWritten = await storeAsyncTask;
                rootPage.NotifyUser("Write completed - " + bytesWritten.ToString() + " bytes written", NotifyType.StatusMessage);
     
        }


        /// <summary>
        /// It is important to be able to cancel tasks that may take a while to complete. Cancelling tasks is the only way to stop any pending IO
        /// operations asynchronously. If the Serial Device is closed/deleted while there are pending IOs, the destructor will cancel all pending IO 
        /// operations.
        /// </summary>
        /// 

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

        private void CancelWriteTask()
        {
            lock (WriteCancelLock)
            {
                if (WriteCancellationTokenSource != null)
                {
                    if (!WriteCancellationTokenSource.IsCancellationRequested)
                    {
                        WriteCancellationTokenSource.Cancel();

                        // Existing IO already has a local copy of the old cancellation token so this reset won't affect it
                        ResetWriteCancellationTokenSource();
                    }
                }
            }
        }
        private void CancelAllIoTasks()
        {
            CancelReadTask();
            CancelWriteTask();
        }
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
        private void ResetReadCancellationTokenSource()
        {
            // Create a new cancellation token source so that can cancel all the tokens again
            ReadCancellationTokenSource = new CancellationTokenSource();

            // Hook the cancellation callback (called whenever Task.cancel is called)
            ReadCancellationTokenSource.Token.Register(() => NotifyReadCancelingTask());
        }

        private void ResetWriteCancellationTokenSource()
        {
            // Create a new cancellation token source so that can cancel all the tokens again
            WriteCancellationTokenSource = new CancellationTokenSource();

            // Hook the cancellation callback (called whenever Task.cancel is called)
            WriteCancellationTokenSource.Token.Register(() => NotifyWriteCancelingTask());
        }

         async  private  void ReadOffsetButton_Click(object sender, RoutedEventArgs e)
        {
         await  GetStoredOffsetValue();
            UpdateAllToggleBits();
        }
        private async Task ReadOffsetValue()
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    rootPage.NotifyUser("Reading Parameters...", NotifyType.StatusMessage);

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
                    Debug.WriteLine("ReadOperation cancelled");
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
                   // UpdateAllToggleBits();
                }
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }
           
        }
        private async void NotifyReadCancelingTask()
        {
            // Setting the dispatcher priority to high allows the UI to handle disabling of all the buttons
            // before any of the IO completion callbacks get a chance to modify the UI; that way this method
            // will never get the opportunity to overwrite UI changes made by IO callbacks
            await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                new DispatchedHandler(() =>
                {
                    ReadOffsetButton.IsEnabled = false;
               

                    if (!IsNavigatedAway)
                    {
                        rootPage.NotifyUser("Canceling Read... Please wait...", NotifyType.StatusMessage);
                    }
                }));
        }

        private async void NotifyWriteCancelingTask()
        {
            // Setting the dispatcher priority to high allows the UI to handle disabling of all the buttons
            // before any of the IO completion callbacks get a chance to modify the UI; that way this method
            // will never get the opportunity to overwrite UI changes made by IO callbacks
            await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                new DispatchedHandler(() =>
                {
                    WriteOffsetButton.IsEnabled = false;
                

                    if (!IsNavigatedAway)
                    {
                        rootPage.NotifyUser("Canceling Write... Please wait...", NotifyType.StatusMessage);
                    }
                }));
        }
    }
}
