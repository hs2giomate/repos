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
        private Boolean IsWriteTaskPending;
        DataWriter DataWriteObject = null;
        private Boolean IsNavigatedAway;
        private static Byte[] received, toSend;
        private static Byte fanPWMValue,lasPWMValue, lastFansEnabled,fanEnabled;
        private SingleTaskMessage fansCommand;
        private static System.Timers.Timer refreshValueTimer = null;
        private int sizeofStruct;
        public FansOperation()
        {
            this.InitializeComponent();
            handler = this;
            sizeofStruct = Marshal.SizeOf(typeof(SingleTaskMessage));
           // SetPointFan1.ManipulationCompleted += SetPointFan1_ManipulationCompleted;
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
                SetPointFan1.Tapped += SetPointFan1_Tapped;
                SetPointFan1.Opacity = 0.2;
                // InitialOffsetRead();

            }
        }

        private async void SetPointFan1_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //  throw new NotImplementedException();
            await WritePWMValue();
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
            refreshValueTimer.Interval = 1000;

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
        //    await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
        //     new DispatchedHandler(() => { handler.UpdateRelayStatus(); }));
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
                fanEnabled = (Byte)(lastFansEnabled | 0x01);


            }
            else
            {
                SetPointFan1.Opacity = 0.4;
                fanEnabled = (Byte)(lastFansEnabled & 0xfe);
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

        private void SetPointFan1_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

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
