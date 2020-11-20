using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using ParameterTransferProtocol;
using System.Runtime.InteropServices;
using System.Text;
using MaintenanceToolProtocol;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MaintenanceToolECSBOX
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MinimunFreshAir : Page, IDisposable
    {
        private static MinimunFreshAir handler;
        private MainPage rootPage = MainPage.Current;
        // Track Read Operation
        private CancellationTokenSource ReadCancellationTokenSource;
        private Object ReadCancelLock = new Object();

        private Boolean IsReadTaskPending;
        private uint ReadBytesCounter = 0;
        DataReader DataReaderObject = null;
        public Byte minimumAir, standAloneMinimumAir;

        private CancellationTokenSource WriteCancellationTokenSource;
        private Object WriteCancelLock = new Object();
        private Boolean IsWriteTaskPending;
        DataWriter DataWriteObject = null;
        private Boolean IsNavigatedAway;
        private static Byte[] received, toSend;
        UserParameters parameters;
        ParametersMessage parametersMessage;
        private UInt32 magicHeader;
        private uint readBufferLength = 64;
        private int toSendSize;
        public MinimunFreshAir()
        {
            this.InitializeComponent();
            handler = this;
            parameters.minimumPosition = minimumAir;
            received = new byte[readBufferLength];
            toSendSize = Marshal.SizeOf(typeof(ParametersMessage));
            toSend = new byte[toSendSize];
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
        protected override async void OnNavigatedTo(NavigationEventArgs eventArgs)
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
             await ReadStoredValues();

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

     

        private void FVToggleBit8_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit8.IsOn)
            {
                minimumAir = (Byte)(minimumAir | 0x80);
                FVToggleBitValue8.Text = "1";
            }
            else
            {
                minimumAir = (Byte)(minimumAir & 0x7F);
                FVToggleBitValue8.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit8()
        {
            bool currentState = (minimumAir & 0x80) > 1;
            FVToggleSwitchBit8.IsOn = currentState;
        }

        private void FVToggleBit7_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit7.IsOn)
            {
                minimumAir = (Byte)(minimumAir | 0x40);
                FVToggleBitValue7.Text = "1";
            }
            else
            {
                minimumAir = (Byte)(minimumAir & 0xBF);
                FVToggleBitValue7.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit7()
        {
            bool currentState = (minimumAir & 0x40) > 1;
            FVToggleSwitchBit7.IsOn = currentState;
        }


        private void FVToggleBit6_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit6.IsOn)
            {
                minimumAir = (Byte)(minimumAir | 0x20);
                FVToggleBitValue6.Text = "1";
            }
            else
            {
                minimumAir = (Byte)(minimumAir & 0xDF);
                FVToggleBitValue6.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit6()
        {
            bool currentState = (minimumAir & 0x20) > 1;
            FVToggleSwitchBit6.IsOn = currentState;
        }


        private void FVToggleBit5_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit5.IsOn)
            {
                minimumAir = (Byte)(minimumAir | 0x10);
                FVToggleBitValue5.Text = "1";
            }
            else
            {
                minimumAir = (Byte)(minimumAir & 0xEF);
                FVToggleBitValue5.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit5()
        {
            bool currentState = (minimumAir & 0x10) > 1;
            FVToggleSwitchBit5.IsOn = currentState;
        }

        private void FVToggleBit4_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit4.IsOn)
            {
                minimumAir = (Byte)(minimumAir | 0x08);
                FVToggleBitValue4.Text = "1";
            }
            else
            {
                minimumAir = (Byte)(minimumAir & 0xF7);
                FVToggleBitValue4.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit4()
        {
            bool currentState = (minimumAir & 0x08) > 1;
            FVToggleSwitchBit4.IsOn = currentState;
        }
        private void FVToggleBit3_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit3.IsOn)
            {
                minimumAir = (Byte)(minimumAir | 0x04);
                FVToggleBitValue3.Text = "1";
            }
            else
            {
                minimumAir = (Byte)(minimumAir & 0xFB);
                FVToggleBitValue3.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit3()
        {
            bool currentState = (minimumAir & 0x04) > 1;
            FVToggleSwitchBit3.IsOn = currentState;
        }
        private void FVToggleBit2_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit2.IsOn)
            {
                minimumAir = (Byte)(minimumAir | 0x02);
                FVToggleBitValue2.Text = "1";
            }
            else
            {
                minimumAir = (Byte)(minimumAir & 0xFD);
                FVToggleBitValue2.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit2()
        {
            bool currentState = (minimumAir & 0x02) > 1;
            FVToggleSwitchBit2.IsOn = currentState;
        }

        private void FVToggleBit1_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit1.IsOn)
            {
                minimumAir = (Byte)(minimumAir | 0x01);
                FVToggleBitValue1.Text = "1";
            }
            else
            {
                minimumAir = (Byte)(minimumAir & 0xFE);
                FVToggleBitValue1.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit1()
        {
            bool currentState = (minimumAir & 0x01) > 0;
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
            UpdateFVToggleBit9();
            UpdateFVToggleBit10();
            UpdateFVToggleBit11();
            UpdateFVToggleBit12();
            UpdateFVToggleBit13();
            UpdateFVToggleBit14();
            UpdateFVToggleBit15();
            UpdateFVToggleBit16();
        }
        private void UpdateMinimuSetValue()
        {

            OffsetValueText.Text = String.Concat(ConvertOffsetToAngle(minimumAir).ToString("N0"), " °");
        }
        private void UpdateStandAloneSetValue()
        {

            OffsetValueText1.Text = String.Concat(ConvertOffsetToAngle(standAloneMinimumAir).ToString("N0"), " °");
        }
        private async void WriteOffsetButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPerformingRead())
            {
                CancelReadTask();
            }
            SetEnalbeWriteRead(false);
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                SetEnalbeWriteRead(false);
                try
                {
                    rootPage.NotifyUser("Writing  Offset...", NotifyType.StatusMessage);

                    // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                    // update the button states until after the write completes.
                    IsWriteTaskPending = true;
                    DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                    //UpdateWriteButtonStates();
                    parameters.minimumPosition = minimumAir;
                    parameters.minimumStandAlonePosition = standAloneMinimumAir;

                    toSend.Initialize();

                    ParametersProtocol.Current.CreateWriteParametersMessage(parameters).CopyTo(toSend, 0);


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

                   // UpdateWriteButtonStates();
                }
                SetEnalbeWriteRead(true);
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }
        }

        private async Task GetStoredOffsetValue()
        {
            Task query = QueryParametersValues();
            await query;
            Task ro = ReadOffsetValue();
            await ro;

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
                //    UpdateWriteButtonStates();
                    // await SendMagic(WriteCancellationTokenSource.Token);
                    await SendReadParametersCommand(WriteCancellationTokenSource.Token);


                }
                catch (OperationCanceledException /*exception*/)
                {
                    NotifyWriteTaskCanceled();
                //    Debug.WriteLine("WriteOperation cancelled");
                }
                catch (Exception exception)
                {
                    MainPage.Current.NotifyUser(exception.Message.ToString(), NotifyType.ErrorMessage);
                  //  Debug.WriteLine(exception.Message.ToString());
                }
                finally
                {
                    IsWriteTaskPending = false;
                    DataWriteObject.DetachStream();
                    DataWriteObject = null;

                   // UpdateWriteButtonStates();
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

            

            // Don't start any IO if we canceled the task
            lock (ReadCancelLock)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Cancellation Token will be used so we can stop the task operation explicitly
                // The completion function should still be called so that we can properly handle a canceled task
                DataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
                loadAsyncTask = DataReaderObject.LoadAsync(readBufferLength).AsTask(cancellationToken);
            }

            UInt32 bytesRead = await loadAsyncTask;
        //    Debug.WriteLine(string.Concat("bytes Read", bytesRead.ToString()));
            if (bytesRead > 0)
            {
                DataReaderObject.ReadBytes(received);
                magicHeader = BitConverter.ToUInt32(received, 0);
                rootPage.NotifyUser("Minimun Air values were read ", NotifyType.StatusMessage);

            }
           
        }
        private void DecodeInputValues()
        {
            if (magicHeader.Equals(Commands.reverseMagic))
            {
                minimumAir = received[6];
                standAloneMinimumAir = received[7];
            }
        }

        private int ConvertOffsetToAngle(Byte o)
        {
            int angle = ((int)(o)) * 90 / 255;
            return angle;
        }

        private async Task WriteAsync(CancellationToken cancellationToken)
        {

            Task<UInt32> storeAsyncTask;

            if (minimumAir > 0)
            {

               
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
            rootPage.NotifyUser("Write completed ", NotifyType.StatusMessage);

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
        private void SetEnalbeWriteRead(bool enabled)
        {
            WriteOffsetButton.IsEnabled = enabled;
            ReadOffsetButton.IsEnabled = enabled;
        }
        private async Task ReadStoredValues()
        {
            SetEnalbeWriteRead(false);
            await GetStoredOffsetValue();
            DecodeInputValues();
            UpdateAllToggleBits();
            SetEnalbeWriteRead(true);
        }

        private async  void ReadOffsetButton_Click(object sender, RoutedEventArgs e)
        {
            await ReadStoredValues();
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
                  //  UpdateReadButtonStates();

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

                  //  UpdateReadButtonStates();
                    // UpdateAllToggleBits();
                }
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }

        }

        private void FVToggleSwitchBit16_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit16.IsOn)
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir | 0x01);
                FVToggleBitValue16.Text = "1";
            }
            else
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir & 0xFE);
                FVToggleBitValue16.Text = "0";
            }
            UpdateStandAloneSetValue();
        }
        private void UpdateFVToggleBit16()
        {
            bool currentState = (standAloneMinimumAir & 0x01) > 0;
            FVToggleSwitchBit16.IsOn = currentState;
        }
        private void FVToggleSwitchBit15_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit15.IsOn)
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir | 0x02);
                FVToggleBitValue15.Text = "1";
            }
            else
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir & 0xFD);
                FVToggleBitValue15.Text = "0";
            }
            UpdateStandAloneSetValue();
        }
        private void UpdateFVToggleBit15()
        {
            bool currentState = (standAloneMinimumAir & 0x02) > 1;
            FVToggleSwitchBit15.IsOn = currentState;
        }


        private void FVToggleSwitchBit14_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit14.IsOn)
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir | 0x04);
                FVToggleBitValue14.Text = "1";
            }
            else
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir & 0xFB);
                FVToggleBitValue14.Text = "0";
            }
            UpdateStandAloneSetValue();
        }
        private void UpdateFVToggleBit14()
        {
            bool currentState = (standAloneMinimumAir & 0x04) > 1;
            FVToggleSwitchBit14.IsOn = currentState;
        }

        private void FVToggleSwitchBit13_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit13.IsOn)
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir | 0x08);
                FVToggleBitValue13.Text = "1";
            }
            else
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir & 0xF7);
                FVToggleBitValue13.Text = "0";
            }
            UpdateStandAloneSetValue();
        }
        private void UpdateFVToggleBit13()
        {
            bool currentState = (standAloneMinimumAir & 0x08) > 1;
            FVToggleSwitchBit13.IsOn = currentState;
        }

        private void FVToggleSwitchBit12_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit12.IsOn)
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir | 0x10);
                FVToggleBitValue12.Text = "1";
            }
            else
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir & 0xEF);
                FVToggleBitValue12.Text = "0";
            }
            UpdateStandAloneSetValue();
        }
        private void UpdateFVToggleBit12()
        {
            bool currentState = (standAloneMinimumAir & 0x10) > 1;
            FVToggleSwitchBit12.IsOn = currentState;
        }

        private void FVToggleSwitchBit11_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit11.IsOn)
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir | 0x20);
                FVToggleBitValue11.Text = "1";
            }
            else
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir & 0xDF);
                FVToggleBitValue11.Text = "0";
            }
            UpdateStandAloneSetValue();
        }
        private void UpdateFVToggleBit11()
        {
            bool currentState = (standAloneMinimumAir & 0x20) > 1;
            FVToggleSwitchBit11.IsOn = currentState;
        }
        private void FVToggleSwitchBit10_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit10.IsOn)
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir | 0x40);
                FVToggleBitValue10.Text = "1";
            }
            else
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir & 0xBF);
                FVToggleBitValue10.Text = "0";
            }
            UpdateStandAloneSetValue();
        }
        private void UpdateFVToggleBit10()
        {
            bool currentState = (standAloneMinimumAir & 0x40) > 1;
            FVToggleSwitchBit10.IsOn = currentState;
        }
        private void FVToggleSwitchBit9_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit9.IsOn)
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir | 0x80);
                FVToggleBitValue9.Text = "1";
            }
            else
            {
                standAloneMinimumAir = (Byte)(standAloneMinimumAir & 0x7F);
                FVToggleBitValue9.Text = "0";
            }
            UpdateStandAloneSetValue();
        }
        private void UpdateFVToggleBit9()
        {
            bool currentState = (standAloneMinimumAir & 0x80) > 1;
            FVToggleSwitchBit9.IsOn = currentState;
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
