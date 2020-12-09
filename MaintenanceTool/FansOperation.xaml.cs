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
using ParameterTransferProtocol;
using Windows.Storage.Streams;
using System.Runtime.InteropServices;
using MaintenanceToolProtocol;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MaintenanceToolECSBOX
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FansOperation : Page, IDisposable
    {
        private static FansOperation handler;
        private MainPage rootPage = MainPage.Current;
    
        // Track Read Operation
        private CancellationTokenSource ReadCancellationTokenSource, WriteCancellationTokenSource;
        private Object ReadCancelLock = new Object();

        private Boolean IsReadTaskPending;
        private Boolean[] isToggleON = new Boolean[3];
        private uint ReadBytesCounter = 0;
        DataReader DataReaderObject = null;
        private Object WriteCancelLock = new Object();
        private Boolean IsWriteTaskPending,beingManipulated;
        DataWriter DataWriteObject = null;
        private Boolean IsNavigatedAway,faultMode;
        private static Byte[] received, toSend;
        private static Byte fanPWMValue,lasPWMValue, lastFansEnabled,fanEnabled;
        private SingleTaskMessage fansCommand;
        private static System.Timers.Timer refreshValueTimer = null;
        private int sizeofStruct;
        private uint readBufferLength;
        private UInt32 magicHeader;
        private static AnimationSet Fault_Dark, Fault_Light;
        private bool updatingdata = false;
        public FansOperation()
        {
            this.InitializeComponent();
            handler = this;
            sizeofStruct = Marshal.SizeOf(typeof(SingleTaskMessage));
            SetPointFan1.ManipulationCompleted += SetPointFan1_ManipulationCompleted;
            SetPointFan1.ManipulationStarted += SetPointFan1_ManipulationStarted;
            SetPointFan1.Tapped += SetPointFan1_Tapped1;
            toSend = new byte[sizeofStruct];
            readBufferLength = 64;
            received = new byte[readBufferLength];
            Fault_Dark = SetPointFan1.Fade(value: 0.15f, duration: 1000, delay: 25, easingType: EasingType.Sine);
            //   NBC_Mode_Light = new AnimationSet(position);
            Fault_Light = SetPointFan1.Fade(value: 0.95f, duration: 1000, delay: 25, easingType: EasingType.Sine);
            Fault_Dark.Completed += Fault_Dark_Completed;
            Fault_Light.Completed += Fault_Light_Completed;
        }

        private async void SetPointFan1_Tapped1(object sender, TappedRoutedEventArgs e)
        {
            //  throw new NotImplementedException();
            if (!updatingdata)
            {
                await WritePWMValue();
            }
           
        }

        private void Fault_Light_Completed(object sender, AnimationSetCompletedEventArgs e)
        {
            if (faultMode)
            {
                Fault_Dark.StartAsync();
            }
            else
            {
                Fault_Dark.Stop();
            }
            // throw new NotImplementedException();
        }

        private void Fault_Dark_Completed(object sender, AnimationSetCompletedEventArgs e)
        {
            if (faultMode)
            {
                Fault_Light.StartAsync();
            }
            else
            {
                Fault_Light.Stop();
            }
            //  throw new NotImplementedException();
        }

        private void SetPointFan1_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            beingManipulated = true;
            //throw new NotImplementedException();
        }

        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {

            IsNavigatedAway = false;
            if (EventHandlerForDevice.Current.Device == null)
            {
                // ScrollerToggleBits.Visibility = Visibility.Collapsed;
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
                //  SetPointFan1.IsInteractive = true;
              //  SetPointFan1.PointerReleased += SetPointFan1_PointerReleased;
            //    SetPointFan1.Tapped += SetPointFan1_Tapped;

                SetPointFan1.Opacity = 0.2;
                UpdateFanStatus();
                StartStatusCheckTimer();
                // InitialOffsetRead();

            }
        }

        public async void UpdateFanStatus()
        {
            if (!beingManipulated)
            {
                updatingdata = true;
                SetPointFan1.IsEnabled = false;
                await RequestStatus();
                await ReadFansData();
                //  EnableValve.IsEnabled = true;
             

             
                UpdateFansView();
                SetPointFan1.IsEnabled = true;
                updatingdata = false;
            }

            // EnableValve.IsEnabled = false;



        }
        private async Task ReadFansData()
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    rootPage.NotifyUser("Reading Fans Status...", NotifyType.StatusMessage);

                    // We need to set this to true so that the buttons can be updated to disable the read button. We will not be able to
                    // update the button states until after the read completes.
                    IsReadTaskPending = true;
                    DataReaderObject = new DataReader(EventHandlerForDevice.Current.Device.InputStream);
                    // UpdateReadButtonStates();

                    await ReadDataAsync(ReadCancellationTokenSource.Token);
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

                    // UpdateReadButtonStates();
                    // UpdateAllToggleBits();
                }
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }

        }
        private void UpdateFansView()
        {

            if (magicHeader.Equals(Commands.reverseMagic))
            {
                SetPointFan1.Value = received[12]*14000/255;
                                
                if ((received[6] & 0x07) > 0)
                {
                    EnableFan1.IsOn = false;
                    SetPointFan1.Opacity = 0.4;
              
                }
                else
                {
                    EnableFan1.IsOn = true;
                    SetPointFan1.Opacity = 1;
                                       
                }

                if (((received[6] & 0x70)==0x70)&((received[7] & 0x33)==0x33)&((received[7]&0x44)>0))
                {
                    if (faultMode)
                    {
                        faultMode = false;
                        
                    }
                    
                }
                else
                {
                    if (!faultMode)
                    {
                        faultMode = true;

                        Fault_Dark.StartAsync();
                    }
                }




            }
        }
        private async Task ReadDataAsync(CancellationToken cancellationToken)
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
            //  Debug.WriteLine(string.Concat("bytes Read", bytesRead.ToString()));
            if (bytesRead > 0)
            {
                DataReaderObject.ReadBytes(received);
                magicHeader = BitConverter.ToUInt32(received, 0);



            }
            rootPage.NotifyUser("Fans data was read", NotifyType.StatusMessage);
        }
        private async Task RequestStatus()
        {
            if (IsPerformingRead())
            {
                CancelReadTask();
                // while (IsPerformingRead()) ;
            }
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    rootPage.NotifyUser("Reading Status...", NotifyType.StatusMessage);

                    // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                    // update the button states until after the write completes.
                    IsWriteTaskPending = true;
                    DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                    Protocol.Message.CreateFansStatusRequestMessage().CopyTo(toSend, 0);
                    //UpdateWriteButtonStates();

                    await SendRequestStatusAsync(WriteCancellationTokenSource.Token);
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
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }
        }
        private async Task SendRequestStatusAsync(CancellationToken cancellationToken)
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
            rootPage.NotifyUser("Request Completed  ", NotifyType.StatusMessage);


        }
        protected override void OnNavigatedFrom(NavigationEventArgs eventArgs)
        {
            IsNavigatedAway = true;
            if (refreshValueTimer != null)
            {
                refreshValueTimer.Stop();
                refreshValueTimer.Dispose();
            }
            CancelAllIoTasks();
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
      
        public void StartStatusCheckTimer()
        {
            // Create a timer and set a two second interval.
            refreshValueTimer = new System.Timers.Timer();
            refreshValueTimer.Interval = 2000;

            // Hook up the Elapsed event for the timer. 
            refreshValueTimer.Elapsed += OnTimedEvent;

            // Have the timer fire repeated events (true is the default)
            refreshValueTimer.AutoReset = false;

            // Start the timer
            refreshValueTimer.Enabled = true;


        }
        private static async void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            // Debug.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
            //  MaintenanceToolHandler.Current.SendAliveMessage();

          //  await handler.UpdateDataRelayStatus();
           await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            new DispatchedHandler(() => { handler.UpdateFanStatus(); }));

                    refreshValueTimer.Start();
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
        private void CancelAllIoTasks()
        {
            CancelReadTask();
            CancelWriteTask();
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

        private async void EnableFan1_Toggled(object sender, RoutedEventArgs e)
        {
            lastFansEnabled = fanEnabled;
            if (EnableFan1.IsOn)
            {
                SetPointFan1.Opacity = 1;
                fanEnabled = (Byte)(lastFansEnabled | 0x07);


            }
            else
            {
                SetPointFan1.Opacity = 0.4;
                fanEnabled = (Byte)(lastFansEnabled & 0xf8);
            }
            if (fanEnabled!=lastFansEnabled)
            {
                await WriteAsyncFan();
            }
            
        }
        private Boolean IsPerformingRead()
        {
            return (IsReadTaskPending);
        }

        private async void SetPointFan1_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            await WritePWMValue();
            beingManipulated = false;
        }
        private async Task WritePWMValue()
        {
            lasPWMValue = fanPWMValue;
            fanPWMValue = (byte)(SetPointFan1.Value*255/14000);
            if (fanPWMValue != lasPWMValue)
            {
                await WriteAsyncFan();
            }
        }

       

        private async Task WriteAsyncFan()
        {

            if (IsPerformingRead())
            {
                CancelReadTask();
            }
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    rootPage.NotifyUser("Starting Fan...", NotifyType.StatusMessage);

                    // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                    // update the button states until after the write completes.
                    IsWriteTaskPending = true;
                    DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                    toSend = new byte[sizeofStruct];
                    toSend.Initialize();
                    //   UpdateWriteButtonStates();
                    if (fanEnabled!=lastFansEnabled)
                    {
                        Protocol.Message.CreateEnableFansMessage(fanEnabled).CopyTo(toSend, 0);
                        lastFansEnabled = fanEnabled;
                    }
                    else
                    {
                        if (fanPWMValue != lasPWMValue)
                        {
                            Protocol.Message.CreateSetpointFansMessage(fanPWMValue).CopyTo(toSend, 0);
                            lasPWMValue = fanPWMValue;
                        }
                    }
                
                    await WriteFansEnablesAsync(WriteCancellationTokenSource.Token);

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

                    //  UpdateWriteButtonStates();
                }
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }

        }
        private async Task WriteFansEnablesAsync(CancellationToken cancellationToken)
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
                rootPage.NotifyUser("Write completed .. " , NotifyType.StatusMessage);
      
        }

        private async void NotifyReadCancelingTask()
        {
            // Setting the dispatcher priority to high allows the UI to handle disabling of all the buttons
            // before any of the IO completion callbacks get a chance to modify the UI; that way this method
            // will never get the opportunity to overwrite UI changes made by IO callbacks
            await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                new DispatchedHandler(() =>
                {
                // ReadOffsetButton.IsEnabled = false;


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
                //  WriteOffsetButton.IsEnabled = false;


                if (!IsNavigatedAway)
                    {
                        rootPage.NotifyUser("Canceling Write... Please wait...", NotifyType.StatusMessage);
                    }
                }));
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
    }
    

}
