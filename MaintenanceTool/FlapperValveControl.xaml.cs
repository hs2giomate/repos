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
    public sealed partial class FlapperValveControl : Page, IDisposable
    {
        private static FlapperValveControl handler;
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
        private static Byte currentPosition, lastPosition, lastCommand, commandValve, lastSetpoint, currentSetpoint;
        private SingleTaskMessage fansCommand;
        private static System.Timers.Timer refreshValueTimer = null;
        private int sizeofStruct;
        private uint readBufferLength;
        private UInt32 magicHeader;
        public FlapperValveControl()
        {
            this.InitializeComponent();
            handler = this;
            sizeofStruct = Marshal.SizeOf(typeof(SingleTaskMessage));
            toSend = new byte[sizeofStruct];
            readBufferLength = 64;
            received = new byte[readBufferLength];
            position.IsInteractive = false;
            position.Tapped += Position_Tapped;

        }

        private async void Position_Tapped(object sender, TappedRoutedEventArgs e)
        {
           await WriteSetpointValue();
           // throw new NotImplementedException();
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
                StartStatusCheckTimer();
            }
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


            await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
             new DispatchedHandler(() => { handler.UpdateDataPosition(); }));
            refreshValueTimer.Start();
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


     

        private void position_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

        }

        private async void position_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            await WriteSetpointValue();
        }

        private async Task WriteSetpointValue()
        {
            if ((commandValve & 0x01)>0)
            {
                lastSetpoint = currentSetpoint;
                currentSetpoint = (byte)(position.Value * 255 / 90);
                if (lastSetpoint != currentSetpoint)
                {
                    position.IsInteractive = false;
                    await WriteAsyncValveData();
                    position.IsInteractive = true;
                }
            }
            await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            new DispatchedHandler(() => { handler.UpdateDataPosition(); }));

        }
        private async Task WriteAsyncValveData()
        {

            if (IsPerformingRead())
            {
                CancelReadTask();
            }
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    rootPage.NotifyUser("Sending Command...", NotifyType.StatusMessage);

                    // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                    // update the button states until after the write completes.
                    IsWriteTaskPending = true;
                    DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                    toSend = new byte[sizeofStruct];
                    toSend.Initialize();
                    //   UpdateWriteButtonStates();
                    if (commandValve != lastCommand)
                    {
                        Protocol.Message.CreateCommandFlapperValveMessage(commandValve).CopyTo(toSend, 0);
                        lastCommand=commandValve ;
                    }
                    else if(EnableValve.IsOn)
                    {
                        if (currentSetpoint != lastSetpoint)
                        {
                            Protocol.Message.CreateSetpointtFlapperValveMessage(currentSetpoint).CopyTo(toSend, 0);
                            currentSetpoint = lastSetpoint;
                        }
                    }

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

                    //  UpdateWriteButtonStates();
                }
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }

        }
        private async Task WriteAsync(CancellationToken cancellationToken)
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
            rootPage.NotifyUser("Write completed .. ", NotifyType.StatusMessage);

        }
        public async void UpdateDataPosition()
        {
            await RequestPositionValve();
            await ReadPositionValve();
            GetLastPosition();


        }
        private Boolean IsPerformingRead()
        {
            return (IsReadTaskPending);
        }
        private async Task RequestPositionValve()
        {
            if (IsPerformingRead())
            {
                CancelReadTask();
            }
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    rootPage.NotifyUser("Reading Position...", NotifyType.StatusMessage);

                    // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                    // update the button states until after the write completes.
                    IsWriteTaskPending = true;
                    DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                    Protocol.Message.CreateValvePositionRequestMessage().CopyTo(toSend, 0);
                    //UpdateWriteButtonStates();

                    await SendRequestPositionAsync(WriteCancellationTokenSource.Token);
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
        private async Task SendRequestPositionAsync(CancellationToken cancellationToken)
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
        private async Task ReadPositionValve()
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    rootPage.NotifyUser("Reading Valve Position...", NotifyType.StatusMessage);

                    // We need to set this to true so that the buttons can be updated to disable the read button. We will not be able to
                    // update the button states until after the read completes.
                    IsReadTaskPending = true;
                    DataReaderObject = new DataReader(EventHandlerForDevice.Current.Device.InputStream);
                    // UpdateReadButtonStates();

                    await ReadPositionAsync(ReadCancellationTokenSource.Token);
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

        private async void EnableValve_Toggled(object sender, RoutedEventArgs e)
        {
            lastCommand = commandValve;
            if (EnableValve.IsOn)
            {
                position.Opacity = 1;
                commandValve = (Byte)(lastCommand | 0x01);
                position.IsInteractive = true;


            }
            else
            {
                position.Opacity = 0.4;
                commandValve = (Byte)(lastCommand & 0xfe);
                position.IsInteractive = false;
            }
            if (commandValve != lastCommand)
            {
                await WriteAsyncValveData();
                await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                  new DispatchedHandler(() => { handler.UpdateDataPosition(); }));

            }
        }

        private async Task ReadPositionAsync(CancellationToken cancellationToken)
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
            rootPage.NotifyUser("Temperatures values were read", NotifyType.StatusMessage);
        }
        private void GetLastPosition()
        {

            if (magicHeader.Equals(Commands.reverseMagic))
            {

        
                lastPosition = currentPosition;
                currentPosition = received[6];
                if (lastPosition != currentPosition)
                {
                    position.Value = currentPosition * 90 / 255;
                }

     


            }
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
    }
}
