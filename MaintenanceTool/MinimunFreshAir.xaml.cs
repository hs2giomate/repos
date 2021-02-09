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
        public Byte minimumAir0, standAloneMinimumAir0,minimumValid0;
        public Byte minimumAir1, standAloneMinimumAir1, minimumValid1;
        private CancellationTokenSource WriteCancellationTokenSource;
        private Object WriteCancelLock = new Object();
        private Boolean IsWriteTaskPending,request_sucess,read_request_succes;
        DataWriter DataWriteObject = null;
        private Boolean IsNavigatedAway;
        private static Byte[] received, toSend;
        UserParameters parameters;
        ParametersMessage parametersMessage;
        private UInt32 magicHeader;
        private uint readBufferLength = 64;
        private int toSendSize;
        private bool currentState;
        public MinimunFreshAir()
        {
            this.InitializeComponent();
            handler = this;
  
            parameters.minimumPosition0 = minimumAir0;
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
              //  ScrollerToggleBits.Visibility = Visibility.Collapsed;
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
                minimumAir0 = (Byte)(minimumAir0 | 0x80);
                FVToggleBitValue8.Text = "1";
            }
            else
            {
                minimumAir0 = (Byte)(minimumAir0 & 0x7F);
                FVToggleBitValue8.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit8()
        {
             currentState = (minimumAir0 & 0x80) > 1;
            FVToggleSwitchBit8.IsOn = currentState;
            currentState = (minimumAir1 & 0x80) > 1;
            FVToggleSwitchBit17.IsOn = currentState;
        }

        private void FVToggleBit7_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit7.IsOn)
            {
                minimumAir0 = (Byte)(minimumAir0 | 0x40);
                FVToggleBitValue7.Text = "1";
            }
            else
            {
                minimumAir0 = (Byte)(minimumAir0 & 0xBF);
                FVToggleBitValue7.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit7()
        {
             currentState = (minimumAir0 & 0x40) > 1;
            FVToggleSwitchBit7.IsOn = currentState;
            currentState = (minimumAir1 & 0x40) > 1;
            FVToggleSwitchBit18.IsOn = currentState;
        }


        private void FVToggleBit6_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit6.IsOn)
            {
                minimumAir0= (Byte)(minimumAir0 | 0x20);
                FVToggleBitValue6.Text = "1";
            }
            else
            {
                minimumAir0 = (Byte)(minimumAir0 & 0xDF);
                FVToggleBitValue6.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit6()
        {
             currentState = (minimumAir0 & 0x20) > 1;
            FVToggleSwitchBit6.IsOn = currentState;
            currentState = (minimumAir1 & 0x20) > 1;
            FVToggleSwitchBit19.IsOn = currentState;
        }


        private void FVToggleBit5_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit5.IsOn)
            {
                minimumAir0 = (Byte)(minimumAir0 | 0x10);
                FVToggleBitValue5.Text = "1";
            }
            else
            {
                minimumAir0 = (Byte)(minimumAir0 & 0xEF);
                FVToggleBitValue5.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit5()
        {
             currentState = (minimumAir0 & 0x10) > 1;
            FVToggleSwitchBit5.IsOn = currentState;
            currentState = (minimumAir1 & 0x10) > 1;
            FVToggleSwitchBit20.IsOn = currentState;
        }

        private void FVToggleBit4_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit4.IsOn)
            {
                minimumAir0 = (Byte)(minimumAir0 | 0x08);
                FVToggleBitValue4.Text = "1";
            }
            else
            {
                minimumAir0 = (Byte)(minimumAir0 & 0xF7);
                FVToggleBitValue4.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit4()
        {
            currentState = (minimumAir0 & 0x08) > 1;
            FVToggleSwitchBit4.IsOn = currentState;
            currentState = (minimumAir1 & 0x08) > 1;
            FVToggleSwitchBit21.IsOn = currentState;
        }
        private void FVToggleBit3_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit3.IsOn)
            {
                minimumAir0 = (Byte)(minimumAir0 | 0x04);
                FVToggleBitValue3.Text = "1";
            }
            else
            {
                minimumAir0 = (Byte)(minimumAir0 & 0xFB);
                FVToggleBitValue3.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit3()
        {
             currentState = (minimumAir0 & 0x04) > 1;
            FVToggleSwitchBit3.IsOn = currentState;
            currentState = (minimumAir1 & 0x04) > 1;
            FVToggleSwitchBit22.IsOn = currentState;
        }
        private void FVToggleBit2_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit2.IsOn)
            {
                minimumAir0 = (Byte)(minimumAir0 | 0x02);
                FVToggleBitValue2.Text = "1";
            }
            else
            {
                minimumAir0 = (Byte)(minimumAir0 & 0xFD);
                FVToggleBitValue2.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit2()
        {
             currentState = (minimumAir0 & 0x02) > 1;
            FVToggleSwitchBit2.IsOn = currentState;
            currentState = (minimumAir1 & 0x02) > 1;
            FVToggleSwitchBit23.IsOn = currentState;
        }

        private void FVToggleBit1_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit1.IsOn)
            {
                minimumAir0 = (Byte)(minimumAir0 | 0x01);
                FVToggleBitValue1.Text = "1";
            }
            else
            {
                minimumAir0 = (Byte)(minimumAir0 & 0xFE);
                FVToggleBitValue1.Text = "0";
            }
            UpdateMinimuSetValue();
        }
        private void UpdateFVToggleBit1()
        {
             currentState = (minimumAir0 & 0x01) > 0;
            FVToggleSwitchBit1.IsOn = currentState;
            currentState = (minimumAir1 & 0x01) > 0;
            FVToggleSwitchBit24.IsOn = currentState;
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

            OffsetValueText.Text = String.Concat(ConvertOffsetToAngle(minimumAir0).ToString("N0"), " °");
            OffsetValueText2.Text = String.Concat(ConvertOffsetToAngle(minimumAir1).ToString("N0"), " °");
        }
        private void UpdateStandAloneSetValue()
        {

            OffsetValueText1.Text = String.Concat(ConvertOffsetToAngle(standAloneMinimumAir0).ToString("N0"), " °");
            OffsetValueText3.Text = String.Concat(ConvertOffsetToAngle(standAloneMinimumAir1).ToString("N0"), " °");
        }
        private async void WriteOffsetButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPerformingRead())
            {
                CancelReadTask();
            }
            SetEnalbleWriteRead(false);
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                SetEnalbleWriteRead(false);
                try
                {
                    rootPage.NotifyUser("Writing  Offset...", NotifyType.StatusMessage);

                    // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                    // update the button states until after the write completes.
                   
                    parameters.minimumPosition0 = minimumAir0;
                    parameters.minimumStandAlonePosition0 = standAloneMinimumAir0;
                    parameters.minimumPosition1 = minimumAir1;
                    parameters.minimumStandAlonePosition1 = standAloneMinimumAir1;
                 

                    ParametersProtocol.Current.CreateWriteParametersMessage(parameters).CopyTo(toSend, 0);
                    IsWriteTaskPending = true;
                    DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                    
                    await WriteAsync(WriteCancellationTokenSource.Token);
                    
                   
                    //UpdateWriteButtonStates();
                    


                    
                }
                catch (OperationCanceledException /*exception*/)
                {
                    NotifyWriteTaskCanceled();
                    IsWriteTaskPending = false;
                }
                catch (Exception exception)
                {
                    MainPage.Current.NotifyUser(exception.Message.ToString(), NotifyType.ErrorMessage);
                    Debug.WriteLine(exception.Message.ToString());
                    IsWriteTaskPending = false;
                }
                finally
                {
                    IsWriteTaskPending = false;
                    try
                    {
                        DataWriteObject.DetachStream();
                    }
                    catch (Exception ex)
                    {
                        rootPage.NotifyUser(ex.Message.ToString(), NotifyType.ErrorMessage);
                        //  throw;
                    }
                   
                    DataWriteObject.Dispose();

                   // UpdateWriteButtonStates();
                }
                SetEnalbleWriteRead(true);
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }
        }

        private async Task GetStoredOffsetValue()
        {
            request_sucess = false;
            Task query = QueryParametersValues();
            await query;
            if (request_sucess)
            {
                read_request_succes = false;
                Task ro = ReadOffsetValue();
                await ro;
            }
          

        }
        private async Task QueryParametersValues()
        {
            if (IsPerformingWrite())
            {
                rootPage.NotifyUser(" already writting to memory...", NotifyType.StatusMessage);
            }
            else
            {
                if (EventHandlerForDevice.Current.IsDeviceConnected)
                {
                    try
                    {
                        rootPage.NotifyUser(" asking minimum air positions...", NotifyType.StatusMessage);

                        // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                        // update the button states until after the write completes.
                        IsWriteTaskPending = true;
                        DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                        
                            await SendReadParametersCommand(WriteCancellationTokenSource.Token);
                        
                        
                        //    UpdateWriteButtonStates();
                        // await SendMagic(WriteCancellationTokenSource.Token);
                        


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
                        try
                        {
                            DataWriteObject.DetachStream();
                        }
                        catch (Exception ex)
                        {
                            rootPage.NotifyUser(ex.Message.ToString(), NotifyType.ErrorMessage);
                            // throw;
                        }
                       
                        DataWriteObject.Dispose();
                        rootPage.NotifyUser("Write succesfully ", NotifyType.StatusMessage);
                        request_sucess = true;
                        // UpdateWriteButtonStates();
                    }
                }
                else
                {
                    Utilities.NotifyDeviceNotConnected();
                }
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
                read_request_succes = true;

            }
           
        }
        private void DecodeInputValues()
        {
            if (magicHeader.Equals(Commands.reverseMagic))
            {
                minimumAir0 = received[6];
                standAloneMinimumAir0 = received[8];
                minimumValid0= received[10];
                minimumAir1 = received[7];
                standAloneMinimumAir1 = received[9];
                minimumValid1 = received[11];
            }
        }
        public async Task<Byte> GetminimunValidAirPosition(int i)
        {
          
            ResetReadCancellationTokenSource();
          //  IsReadTaskPending = false;
            ResetWriteCancellationTokenSource();
            IsWriteTaskPending = false;
            await GetStoredOffsetValue();
            if (read_request_succes)
            {
                DecodeInputValues();
                if (i==0)
                {
                    return minimumValid0;
                }
                else
                {
                    return minimumValid1;
                }
               
            }
            else
            {
              
                return 0x00;
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

            if (minimumAir0 > 0)
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
        private void SetEnalbleWriteRead(bool enabled)
        {
            WriteOffsetButton.IsEnabled = enabled;
            ReadOffsetButton.IsEnabled = enabled;
        }
        private async Task ReadStoredValues()
        {
            SetEnalbleWriteRead(false);
            await GetStoredOffsetValue();
            if (read_request_succes)
            {
                DecodeInputValues();
                UpdateAllToggleBits();
                Update_Stored_values();
                SetEnalbleWriteRead(true);
            }
       
        }
        private void Update_Stored_values()
        {
            ReadOffsetValueText.Text = (minimumAir0*90/255).ToString("N0");
            ReadOffsetValueText1.Text = (standAloneMinimumAir0 * 90 / 255).ToString("N0");
            ReadOffsetValueText2.Text = (minimumAir1 * 90 / 255).ToString("N0");
            ReadOffsetValueText3.Text = (standAloneMinimumAir1 * 90 / 255).ToString("N0");
        }

        private async  void ReadOffsetButton_Click(object sender, RoutedEventArgs e)
        {
            await ReadStoredValues();
        }
        private async Task ReadOffsetValue()
        {
            if (IsPerformingWrite())
            {
                rootPage.NotifyUser("busy on writting memory", NotifyType.StatusMessage);
            }
            else
            {
                if (IsPerformingRead())
                {
                    rootPage.NotifyUser("already reading", NotifyType.StatusMessage);
                }
                else
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
                            Debug.WriteLine("Read parameters Operation cancelled");
                        }
                        catch (Exception exception)
                        {
                            MainPage.Current.NotifyUser(exception.Message.ToString(), NotifyType.ErrorMessage);
                            Debug.WriteLine(exception.Message.ToString());
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
                                //throw;
                            }

                            DataReaderObject.Dispose();
                            
                            //  UpdateReadButtonStates();
                            // UpdateAllToggleBits();
                        }
                    }
                    else
                    {
                        Utilities.NotifyDeviceNotConnected();
                    }
                }
            }
            

        }

        private void FVToggleSwitchBit16_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit16.IsOn)
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 | 0x01);
                FVToggleBitValue16.Text = "1";
            }
            else
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 & 0xFE);
                FVToggleBitValue16.Text = "0";
            }
            UpdateStandAloneSetValue();
        }
        private void UpdateFVToggleBit16()
        {
             currentState = (standAloneMinimumAir0 & 0x01) > 0;
            FVToggleSwitchBit16.IsOn = currentState;
            currentState = (standAloneMinimumAir1 & 0x01) > 0;
            FVToggleSwitchBit32.IsOn = currentState;
        }
        private void FVToggleSwitchBit15_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit15.IsOn)
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 | 0x02);
                FVToggleBitValue15.Text = "1";
            }
            else
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 & 0xFD);
                FVToggleBitValue15.Text = "0";
            }
            UpdateStandAloneSetValue();
        }
        private void UpdateFVToggleBit15()
        {
             currentState = (standAloneMinimumAir0 & 0x02) > 1;
            FVToggleSwitchBit15.IsOn = currentState;
            currentState = (standAloneMinimumAir1 & 0x02) > 1;
            FVToggleSwitchBit31.IsOn = currentState;
        }


        private void FVToggleSwitchBit14_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit14.IsOn)
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 | 0x04);
                FVToggleBitValue14.Text = "1";
            }
            else
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 & 0xFB);
                FVToggleBitValue14.Text = "0";
            }
            UpdateStandAloneSetValue();
        }
        private void UpdateFVToggleBit14()
        {
             currentState = (standAloneMinimumAir0 & 0x04) > 1;
            FVToggleSwitchBit14.IsOn = currentState;
            currentState = (standAloneMinimumAir1 & 0x04) > 1;
            FVToggleSwitchBit30.IsOn = currentState;
        }

        private void FVToggleSwitchBit13_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit13.IsOn)
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 | 0x08);
                FVToggleBitValue13.Text = "1";
            }
            else
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 & 0xF7);
                FVToggleBitValue13.Text = "0";
            }
            UpdateStandAloneSetValue();
        }
        private void UpdateFVToggleBit13()
        {
             currentState = (standAloneMinimumAir0 & 0x08) > 1;
            FVToggleSwitchBit13.IsOn = currentState;
            currentState = (standAloneMinimumAir1 & 0x08) > 1;
            FVToggleSwitchBit29.IsOn = currentState;
        }

        private void FVToggleSwitchBit12_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit12.IsOn)
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 | 0x10);
                FVToggleBitValue12.Text = "1";
            }
            else
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 & 0xEF);
                FVToggleBitValue12.Text = "0";
            }
            UpdateStandAloneSetValue();
        }
        private void UpdateFVToggleBit12()
        {
            currentState = (standAloneMinimumAir0 & 0x10) > 1;
            FVToggleSwitchBit12.IsOn = currentState;
            currentState = (standAloneMinimumAir1 & 0x10) > 1;
            FVToggleSwitchBit28.IsOn = currentState;
        }

        private void FVToggleSwitchBit11_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit11.IsOn)
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 | 0x20);
                FVToggleBitValue11.Text = "1";
            }
            else
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 & 0xDF);
                FVToggleBitValue11.Text = "0";
            }
            UpdateStandAloneSetValue();
        }

        private void FVToggleSwitchBit24_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit24.IsOn)
            {
                minimumAir1 = (Byte)(minimumAir1 | 0x01);
                FVToggleBitValue24.Text = "1";
            }
            else
            {
                minimumAir1 = (Byte)(minimumAir1 & 0xFE);
                FVToggleBitValue24.Text = "0";
            }
            UpdateMinimuSetValue();

        }

        private void FVToggleSwitchBit23_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit23.IsOn)
            {
                minimumAir1 = (Byte)(minimumAir1 | 0x02);
                FVToggleBitValue23.Text = "1";
            }
            else
            {
                minimumAir1 = (Byte)(minimumAir1 & 0xFD);
                FVToggleBitValue23.Text = "0";
            }
            UpdateMinimuSetValue();
        }

        private void FVToggleSwitchBit22_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit22.IsOn)
            {
                minimumAir1 = (Byte)(minimumAir1 | 0x04);
                FVToggleBitValue22.Text = "1";
            }
            else
            {
                minimumAir1 = (Byte)(minimumAir1 & 0xFb);
                FVToggleBitValue22.Text = "0";
            }
            UpdateMinimuSetValue();
        }

        private void FVToggleSwitchBit21_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit21.IsOn)
            {
                minimumAir1 = (Byte)(minimumAir1 | 0x08);
                FVToggleBitValue21.Text = "1";
            }
            else
            {
                minimumAir1 = (Byte)(minimumAir1 & 0xF7);
                FVToggleBitValue21.Text = "0";
            }
            UpdateMinimuSetValue();
        }  
        
private void FVToggleSwitchBit20_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit20.IsOn)
            {
                minimumAir1 = (Byte)(minimumAir1 | 0x10);
                FVToggleBitValue20.Text = "1";
            }
            else
            {
                minimumAir1 = (Byte)(minimumAir1 & 0xef);
                FVToggleBitValue20.Text = "0";
            }
            UpdateMinimuSetValue();
        }

        private void FVToggleSwitchBit19_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit19.IsOn)
            {
                minimumAir1 = (Byte)(minimumAir1 | 0x20);
                FVToggleBitValue19.Text = "1";
            }
            else
            {
                minimumAir1 = (Byte)(minimumAir1 & 0xbf);
                FVToggleBitValue19.Text = "0";
            }
            UpdateMinimuSetValue();
        }

        private void FVToggleSwitchBit18_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit18.IsOn)
            {
                minimumAir1 = (Byte)(minimumAir1 | 0x40);
                FVToggleBitValue18.Text = "1";
            }
            else
            {
                minimumAir1 = (Byte)(minimumAir1 & 0xdf);
                FVToggleBitValue18.Text = "0";
            }
            UpdateMinimuSetValue();
        }

        private void FVToggleSwitchBit17_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit17.IsOn)
            {
                minimumAir1 = (Byte)(minimumAir1 | 0x80);
                FVToggleBitValue17.Text = "1";
            }
            else
            {
                minimumAir1 = (Byte)(minimumAir1 & 0x7f);
                FVToggleBitValue17.Text = "0";
            }
            UpdateMinimuSetValue();
        }

        private void FVToggleSwitchBit32_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit32.IsOn)
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 | 0x01);
                FVToggleBitValue32.Text = "1";
            }
            else
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 & 0xFE);
                FVToggleBitValue32.Text = "0";
            }
            UpdateStandAloneSetValue();
        }

        private void FVToggleSwitchBit31_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit31.IsOn)
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 | 0x02);
                FVToggleBitValue31.Text = "1";
            }
            else
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 & 0xFd);
                FVToggleBitValue31.Text = "0";
            }
            UpdateStandAloneSetValue();
        }

        private void FVToggleSwitchBit30_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit30.IsOn)
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 | 0x04);
                FVToggleBitValue30.Text = "1";
            }
            else
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 & 0xFb);
                FVToggleBitValue30.Text = "0";
            }
            UpdateStandAloneSetValue();
        }

        private void FVToggleSwitchBit29_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit29.IsOn)
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 | 0x08);
                FVToggleBitValue29.Text = "1";
            }
            else
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 & 0xF7);
                FVToggleBitValue29.Text = "0";
            }
            UpdateStandAloneSetValue();
        }

        private void FVToggleSwitchBit28_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit28.IsOn)
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 | 0x10);
                FVToggleBitValue28.Text = "1";
            }
            else
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 & 0xef);
                FVToggleBitValue28.Text = "0";
            }
            UpdateStandAloneSetValue();
        }

        private void FVToggleSwitchBit27_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit27.IsOn)
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 | 0x20);
                FVToggleBitValue27.Text = "1";
            }
            else
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 & 0xdf);
                FVToggleBitValue27.Text = "0";
            }
            UpdateStandAloneSetValue();
        }

        private void FVToggleSwitchBit26_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit32.IsOn)
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 | 0x40);
                FVToggleBitValue26.Text = "1";
            }
            else
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 & 0xbf);
                FVToggleBitValue26.Text = "0";
            }
            UpdateStandAloneSetValue();
        }

        private void FVToggleSwitchBit25_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit25.IsOn)
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 | 0x80);
                FVToggleBitValue25.Text = "1";
            }
            else
            {
                standAloneMinimumAir1 = (Byte)(standAloneMinimumAir1 & 0x7f);
                FVToggleBitValue25.Text = "0";
            }
            UpdateStandAloneSetValue();
        }

        private void UpdateFVToggleBit11()
        {
            currentState = (standAloneMinimumAir0 & 0x20) > 1;
            FVToggleSwitchBit11.IsOn = currentState;
            currentState = (standAloneMinimumAir1 & 0x20) > 1;
            FVToggleSwitchBit27.IsOn = currentState;

        }
        private void FVToggleSwitchBit10_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit10.IsOn)
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 | 0x40);
                FVToggleBitValue10.Text = "1";
            }
            else
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 & 0xBF);
                FVToggleBitValue10.Text = "0";
            }
            UpdateStandAloneSetValue();
        }
        private void UpdateFVToggleBit10()
        {
            currentState = (standAloneMinimumAir0 & 0x40) > 1;
            FVToggleSwitchBit10.IsOn = currentState;
            currentState = (standAloneMinimumAir1 & 0x40) > 1;
            FVToggleSwitchBit26.IsOn = currentState;
        }
        private void FVToggleSwitchBit9_Toggled(object sender, RoutedEventArgs e)
        {
            if (FVToggleSwitchBit9.IsOn)
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 | 0x80);
                FVToggleBitValue9.Text = "1";
            }
            else
            {
                standAloneMinimumAir0 = (Byte)(standAloneMinimumAir0 & 0x7F);
                FVToggleBitValue9.Text = "0";
            }
            UpdateStandAloneSetValue();
        }
        private void UpdateFVToggleBit9()
        {
            currentState = (standAloneMinimumAir0 & 0x80) > 1;
            FVToggleSwitchBit9.IsOn = currentState;
            currentState = (standAloneMinimumAir1 & 0x80) > 1;
            FVToggleSwitchBit25.IsOn = currentState;
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
