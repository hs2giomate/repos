﻿using System;
using System.Diagnostics;
using System.Threading;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

using ParameterTransferProtocol;
using Windows.Storage.Streams;
using System.Runtime.InteropServices;
using System.Text;
using MaintenanceToolProtocol;
using System.Timers;
using System.Runtime.CompilerServices;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MaintenanceToolECSBOX
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HeaterOperation : Page, IDisposable

    {
        private MainPage rootPage = MainPage.Current;
        private static HeaterOperation handler;
        // Track Read Operation
        private CancellationTokenSource ReadCancellationTokenSource;
        private Object ReadCancelLock = new Object();

        private Boolean IsReadTaskPending;
        private uint ReadBytesCounter = 0;
        DataReader DataReaderObject = null;
        public Byte RelaysStatus { get; set; }

        private CancellationTokenSource WriteCancellationTokenSource;
        private Object WriteCancelLock = new Object();
        private Boolean IsWriteTaskPending;
        DataWriter DataWriteObject = null;
        private Boolean IsNavigatedAway;
        private static Byte[] received, toSend;
        private static Byte heaterRelaysCommand,lastRelaysCommand;
        UserParameters parameters;
        ParametersMessage parametersMessage;
        private static AnimationSet faultDarkAnimation,faultLighAnimation;
        private Task blink;
        private SingleTaskMessage heatersCommand;
        private static System.Timers.Timer aTimer = null;
        public HeaterOperation()
        {
            this.InitializeComponent();
            handler = this;
            faultDarkAnimation = Relay1FaultSignal.Fade(value: 0.25f, duration: 1000, delay: 25, easingType: EasingType.Sine);
            faultDarkAnimation.Completed += FaultDarkAnimation_Completed;
            faultLighAnimation = Relay1FaultSignal.Fade(value: 0.95f, duration: 1000, delay: 25, easingType: EasingType.Sine);
            faultLighAnimation.Completed += FaultLighAnimation_Completed;
            heaterRelaysCommand = 0;
        }
        public void StartStatusCheckTimer()
        {
            // Create a timer and set a two second interval.
            aTimer = new System.Timers.Timer();
            aTimer.Interval = 1000;

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;

            // Have the timer fire repeated events (true is the default)
            aTimer.AutoReset = true;

            // Start the timer
            aTimer.Enabled = true;


        }
        private void FaultLighAnimation_Completed(object sender, AnimationSetCompletedEventArgs e)
        {
            if (!RelayEnable1Toggle.IsOn)
            {

                blink = faultDarkAnimation.StartAsync();
            }
            //  throw new NotImplementedException();
        }
        private static async void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            // Debug.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
            //  MaintenanceToolHandler.Current.SendAliveMessage();
          await handler.UpdateDataRelayStatus();
           

        }
        public  async Task UpdateDataRelayStatus()
        {
            await RequestRelayStatus();
            await ReadRelaysStatus();


        }

        private void FaultDarkAnimation_Completed(object sender, AnimationSetCompletedEventArgs e)
        {
            if (!RelayEnable1Toggle.IsOn)
            {

                blink = faultLighAnimation.StartAsync();
            }
           // throw new NotImplementedException();
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

                UpdateRelayStatus();
                StartStatusCheckTimer();

                // InitialOffsetRead();

            }
        }
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
        private Boolean IsPerformingRead()
        {
            return (IsReadTaskPending);
        }
        private async Task RequestRelayStatus()
        {
            if (IsPerformingRead())
            {
                CancelReadTask();
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
                    //UpdateWriteButtonStates();

                    await SendrequestStatusAsync(WriteCancellationTokenSource.Token);
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

        private async Task ReadRelaysStatus()
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
                   // UpdateReadButtonStates();

                    await ReadStatusAsync(ReadCancellationTokenSource.Token);
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
        private async Task ReadStatusAsync(CancellationToken cancellationToken)
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
            Debug.WriteLine(string.Concat("bytes Read", bytesRead.ToString()));
            if (bytesRead > 0)
            {
                received = new byte[ReadBufferLength];

                DataReaderObject.ReadBytes(received);
                Debug.WriteLine(string.Concat("Bytes received: ", received.ToString()));
             //   ParametersStruct receivedParameters = new ParametersStruct();
             //   UserParameters p = receivedParameters.ConvertBytesParameters(received);
                RelaysStatus = received[6];
              //  ReadOffsetValueText.Text = String.Concat(ConvertOffsetToAngle(Offset).ToString("N0"), " °");
                ReadBytesCounter += bytesRead;

            }
            rootPage.NotifyUser("Read completed - " + bytesRead.ToString() + " bytes were read", NotifyType.StatusMessage);
        }
        private async Task WriteRelaEnablesAsync(CancellationToken cancellationToken)
        {

            Task<UInt32> storeAsyncTask;

            if (heaterRelaysCommand != lastRelaysCommand)
            {

                heatersCommand.description = heaterRelaysCommand;
                var n = Marshal.SizeOf(typeof(SingleTaskMessage));
                toSend = new byte[n];
                toSend.Initialize();
                Protocol.Message.CreateEnableHeatersMessage(heaterRelaysCommand).CopyTo(toSend, 0);

           //     ParametersProtocol.Current.CreateWriteParametersMessage(parameters).CopyTo(toSend, 0);

          //      Byte[] toSend2 = Encoding.ASCII.GetBytes("abcd<Gf");

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
        private async Task SendrequestStatusAsync(CancellationToken cancellationToken)
        {

            Task<UInt32> storeAsyncTask;

        
             
                var n = Marshal.SizeOf(typeof(SingleTaskMessage));
                toSend = new byte[n];
                toSend.Initialize();
                Protocol.Message.CreateHeatersStatusRequestMessage().CopyTo(toSend, 0);

                //     ParametersProtocol.Current.CreateWriteParametersMessage(parameters).CopyTo(toSend, 0);

                //      Byte[] toSend2 = Encoding.ASCII.GetBytes("abcd<Gf");

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
                rootPage.NotifyUser("Request Completed - " + bytesWritten.ToString() + " bytes written", NotifyType.StatusMessage);
    

        }
        private Boolean IsPerformingWrite()
        {
            return (IsWriteTaskPending);
        }
        private async void HeaterEnable_Toggled(object sender, RoutedEventArgs e)
        {
            lastRelaysCommand = heaterRelaysCommand;
            if (RelayEnable1Toggle.IsOn)
            {
                
                heaterRelaysCommand = (Byte)(lastRelaysCommand | 0x01);
            }
            else
            {
                heaterRelaysCommand = (Byte)(lastRelaysCommand & 0xfe);
            }
            if (heaterRelaysCommand!=lastRelaysCommand)
            {
                if (IsPerformingRead())
                {
                    CancelReadTask();
                }
                if (EventHandlerForDevice.Current.IsDeviceConnected)
                {
                    try
                    {
                        rootPage.NotifyUser("Sendind Command...", NotifyType.StatusMessage);

                        // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                        // update the button states until after the write completes.
                        IsWriteTaskPending = true;
                        DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                        //   UpdateWriteButtonStates();

                        await WriteRelaEnablesAsync(WriteCancellationTokenSource.Token);
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
          //  UpdateRelayStatus();
           

        }
        private void UpdateRelayStatus()
        {
            UpdateFaultStatus1Signal();
            UpdateRelayStatusText();
        }
        private void UpdateRelayStatusText()
        {
            if ((RelaysStatus &  ((Byte)0x01))==0)
            {
                Relay1StatusText.Text = "Fault";
            }
            else 
            {
                if (RelayEnable1Toggle.IsOn)
                {
                    Relay1StatusText.Text = "Heating";
                }
                else
                {
                    Relay1StatusText.Text = "Off";
                }
            }
     
        }
        private async void UpdateFaultStatus1Signal()
        {
            if ((RelaysStatus & ((Byte)0x01)) == 0)
            {
                Relay1StatusBorder.Visibility = Visibility.Collapsed;
                Relay1FaultSignal.Visibility = Visibility.Visible;
                blink = faultDarkAnimation.StartAsync();
            }
            else
            {
                Relay1FaultSignal.Fade().Stop();
                if (blink != null)
                {
                    await blink;
                }
                if (RelayEnable1Toggle.IsOn)
                {
                    Relay1StatusBorder.Visibility = Visibility.Visible;
                    Relay1FaultSignal.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Relay1StatusBorder.Visibility = Visibility.Collapsed;
                    Relay1FaultSignal.Visibility = Visibility.Collapsed;

                }
            }

    
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
    }
}
