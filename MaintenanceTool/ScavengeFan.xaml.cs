using System;
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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MaintenanceToolProtocol;
using Windows.Storage.Streams;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MaintenanceToolECSBOX
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScavengeFan : Page, IDisposable
    {
        private MainPage rootPage = MainPage.Current;
        private static ScavengeFan handler;
        private const int NUMBER_OF_FAULTS = 3;
        private const int NUMBER_OF_RELAYS = 4;
        // Track Read Operation
        private CancellationTokenSource ReadCancellationTokenSource;
        private Object ReadCancelLock = new Object();

        private Boolean IsReadTaskPending;
        private uint ReadBytesCounter = 0;
        DataReader DataReaderObject = null;
        private Byte relays_status;
        public ObservableCollection<TextBlock> listOfTextBlocks;
        public ObservableCollection<Border> listOfBorders;
        public ObservableCollection<Windows.UI.Xaml.Shapes.Ellipse> listOfEllipses, listOfOvertemperatures;
        public ObservableCollection<AnimationSet> listOfDarkAnimations, listOfLightAnimations;
        private CancellationTokenSource WriteCancellationTokenSource;
        private Object WriteCancelLock = new Object();
        private Boolean IsWriteTaskPending, request_sucess, read_request_success;
        DataWriter DataWriteObject = null;
        private Boolean IsNavigatedAway, updating_toogles_view,manipulating;
        private static Byte[] received, toSend;
        private static Byte relayCommand, lastCommand;
        private uint readBufferLength;
        private Task blink;
        
        private static System.Timers.Timer aTimer = null;
        private int sizeofStruct;
        private UInt32 magicHeader = 0;
        private bool updatingStatus = false;
        private const int UPDATING_TIME = 1000;
        private bool required_status = false;
        public ScavengeFan()
        {
            this.InitializeComponent();
            handler = this;
            readBufferLength = 64;
            received = new Byte[readBufferLength];
            sizeofStruct = Marshal.SizeOf(typeof(SingleTaskMessage));
            listOfTextBlocks = new ObservableCollection<TextBlock>();
            listOfTextBlocks.Add(Relay1StatusText);
            listOfTextBlocks.Add(OvertemperautureText);
            listOfTextBlocks.Add(LowSpeedText);
            listOfEllipses = new ObservableCollection<Ellipse>();
            listOfEllipses.Add(Relay1FaultSignal);
            listOfEllipses.Add(OverTemperatureFaultSignal);
            listOfEllipses.Add(LowSpeedFaultSignal);
            listOfDarkAnimations = new ObservableCollection<AnimationSet>();
            listOfLightAnimations = new ObservableCollection<AnimationSet>();
            for (int i = 0; i < NUMBER_OF_FAULTS; i++)
            {
                listOfDarkAnimations.Add(listOfEllipses[i].Fade(value: 0.15f, duration: 1000, delay: 25, easingType: EasingType.Sine));
                listOfLightAnimations.Add(listOfEllipses[i].Fade(value: 0.95f, duration: 1000, delay: 25, easingType: EasingType.Sine));
            }
            listOfDarkAnimations[listOfDarkAnimations.Count - 1].Completed += ScavengeDarkAnimation_Completed;
            listOfLightAnimations[listOfLightAnimations.Count - 1].Completed += ScavengeLigthAnimation_Completed; ;

            foreach (AnimationSet an in listOfDarkAnimations)
            {
                an.StartAsync();
            }
            EnableToggle.ManipulationStarted += EnableToggle_ManipulationStarted;
            EnableToggle.ManipulationCompleted += EnableToggle_ManipulationCompleted;
        }

        private void EnableToggle_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            manipulating = false;
            //throw new NotImplementedException();
        }

        private void EnableToggle_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            manipulating = true;
           // throw new NotImplementedException();
        }

        private void ScavengeLigthAnimation_Completed(object sender, AnimationSetCompletedEventArgs e)
        {
            foreach (AnimationSet aS in listOfDarkAnimations)
            {
                aS.StartAsync();
            }


            // throw new NotImplementedException();
        }

        private void ScavengeDarkAnimation_Completed(object sender, AnimationSetCompletedEventArgs e)
        {
            foreach (AnimationSet aS in listOfLightAnimations)
            {
                aS.StartAsync();
            }
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
                magicHeader = 0;
                UpdateDataRelayStatus();
                StartStatusCheckTimer();

                // InitialOffsetRead();

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
        private static async void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {

            if (handler.IsPerformingWrite()|handler.manipulating)
            {

            }
            else
            {
                await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
         new DispatchedHandler(() => {
             handler.UpdateDataRelayStatus();
         }));
            }
           



            aTimer.Start();
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
        private Boolean IsPerformingWrite()
        {
            return (IsWriteTaskPending);
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

        private async void EnableToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!updating_toogles_view)
            {
                lastCommand = relayCommand;
            }

            if (EnableToggle.IsOn)
            {

                relayCommand = (Byte)(lastCommand | (0x02));
            }
            else
            {
                relayCommand = (Byte)(lastCommand & (~(0x02)));
            }
            if (!updatingStatus)
            {
                if (relayCommand != lastCommand)
                {
                    if (!updating_toogles_view)
                    {
                        await WriteAsyncHeatersEnables();
                    }

                }
            }
        }
        private async Task WriteAsyncHeatersEnables()
        {

            if (IsPerformingRead())
            {
                CancelReadTask();
            }
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    if (relayCommand != lastCommand)
                    {


                        toSend = new byte[sizeofStruct];
                        toSend.Initialize();
                        Protocol.Message.CreateEnableScavengeMessage(relayCommand).CopyTo(toSend, 0);


                        rootPage.NotifyUser("Sendind Command...", NotifyType.StatusMessage);

                        // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                        // update the button states until after the write completes.
                        IsWriteTaskPending = true;
                        DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                        //   UpdateWriteButtonStates();

                        await WriteRelaEnablesAsync(WriteCancellationTokenSource.Token);
                    }
                    else
                    {
                        rootPage.NotifyUser("Already Set ", NotifyType.WarningMessage);
                    }

                  
                   
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
                    rootPage.NotifyUser("Heater Setting completed - ", NotifyType.StatusMessage);

                    //  UpdateWriteButtonStates();
                }
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }

        }
        private async Task WriteRelaEnablesAsync(CancellationToken cancellationToken)
        {

            Task<UInt32> storeAsyncTask;

            if (relayCommand != lastCommand)
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
                rootPage.NotifyUser("Write completed - ", NotifyType.StatusMessage);
            }
            else
            {
                rootPage.NotifyUser("No input received to write", NotifyType.StatusMessage);
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
        public async void UpdateDataRelayStatus()
        {
            updatingStatus = true;
            request_sucess = false;
            await RequestRelayStatus();
            if (request_sucess)
            {
                read_request_success = false;
                await ReadRelaysStatus();
                if (read_request_success)
                {
                    if (magicHeader.Equals(Commands.reverseMagic))
                    {
                        await UpdateViewRelayStatus();
                    }
                }
            }


            updatingStatus = false;
        }
        private async Task RequestRelayStatus()
        {
            if (IsPerformingWrite())
            {
                rootPage.NotifyUser("already reading Scavenge...", NotifyType.WarningMessage);
            }
            else
            {

                if (IsPerformingRead())
                {
                    CancelReadTask();
                    rootPage.NotifyUser("Read task cancelled...", NotifyType.WarningMessage);
                    required_status = (relayCommand > 0);
                    if (required_status)
                    {


                    }
                    else
                    {
                        return;
                    }

                }
                if (EventHandlerForDevice.Current.IsDeviceConnected)
                {
                    try
                    {
                        rootPage.NotifyUser("Sending request to Scavenge...", NotifyType.StatusMessage);

                        // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                        // update the button states until after the write completes.
                        toSend = new byte[sizeofStruct];
                        Protocol.Message.CreateScavengeStatusRequestMessage().CopyTo(toSend, 0);
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
            rootPage.NotifyUser("Request Completed - " + bytesWritten.ToString() + " bytes written", NotifyType.StatusMessage);


        }
        private async Task ReadRelaysStatus()
        {
            if (IsPerformingWrite())
            {
                rootPage.NotifyUser("busy on writting data...", NotifyType.StatusMessage);
            }
            else
            {
                if (IsPerformingRead())
                {
                    CancelReadTask();
                    rootPage.NotifyUser("Cancelling reading task...", NotifyType.WarningMessage);
                    required_status = EnableToggle.IsOn;
                    if (required_status)
                    {

                    }
                    else
                    {
                        return;
                    }

                }
                if (EventHandlerForDevice.Current.IsDeviceConnected)
                {
                    try
                    {
                        rootPage.NotifyUser("Reading Heater Status...", NotifyType.StatusMessage);

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
                        // Debug.WriteLine("ReadOperation cancelled");
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
                            // throw;
                        }

                        DataReaderObject.Dispose();

                        // UpdateReadButtonStates();
                        // UpdateAllToggleBits();
                    }
                }
                else
                {
                    Utilities.NotifyDeviceNotConnected();
                }
            }


        }
        private async Task ReadStatusAsync(CancellationToken cancellationToken)
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
                read_request_success = true;



            }
            rootPage.NotifyUser("Read Scavenge Status completed ", NotifyType.StatusMessage);
        }
        private async Task UpdateViewRelayStatus()
        {
            if (IsPerformingWrite())
            {

            }
            else
            {


                //   ParametersStruct receivedParameters = new ParametersStruct();
                //   UserParameters p = receivedParameters.ConvertBytesParameters(received);

                await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
               new DispatchedHandler(async () => {
                   await Update_Heaters_View();
               }));

            }


        }
        private async Task Update_Heaters_View()
        {
            UpdateToggles();
            await UpdateFaultStatusSignal();
         }
        private void UpdateToggles()
        {
            updating_toogles_view = true;
         
                    if ((received[6] & (0x02 )) > 0)
                    {
                        EnableToggle.IsOn = false;
                    }
                    else
                    {
                        EnableToggle.IsOn = true;
                        RelayStatusBorder.Visibility = Visibility.Visible;
                    }
            
       
            updating_toogles_view = false;

        }
        private async Task UpdateFaultStatusSignal()
        {
            
            if ((received[6] & (Byte)(0x010)) == 0)
            {
                listOfEllipses[0].Visibility = Visibility.Visible;
                RelayStatusBorder.Visibility= Visibility.Collapsed;
                UpdateRelayStatusText(0, "Fault");
                if (blink != null)
                {
                    await blink;
                }
            }
            else
            {
                if (EnableToggle.IsOn)
                {
                    UpdateRelayStatusText(0, "Running");
                    RelayStatusBorder.Visibility = Visibility.Visible;
                    listOfEllipses[0].Visibility = Visibility.Collapsed;
                    for (int i = 0; i < 2; i++)
                    {
                        if ((received[6] & (Byte)(0x40 << i)) == 0)
                        {

                            listOfEllipses[2-i].Visibility = Visibility.Visible;
                            UpdateRelayStatusText(2-i, "Fault");
                            if (blink != null)
                            {
                                await blink;
                            }
                            //  blink = faultDarkAnimation.StartAsync();
                        }
                        else
                        {
                            listOfEllipses[2-i].Visibility = Visibility.Collapsed;
                            UpdateRelayStatusText(2-i, "OK");
                        }

                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        listOfEllipses[i].Visibility = Visibility.Collapsed;
                        UpdateRelayStatusText(i, "Disabled");
                    }

                    RelayStatusBorder.Visibility = Visibility.Collapsed;
                }
                 
               
            }
            


        }
        private void UpdateRelayStatusText(int i,string s)
        {
                        listOfTextBlocks[i].Text = s;
                      
        }
    }
}
