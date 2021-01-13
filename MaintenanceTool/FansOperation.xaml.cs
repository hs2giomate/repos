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
        private const int NUMBER_OF_FANS = 3;
        private const int UPDATE_TIME = NUMBER_OF_FANS * 1000;
        // Track Read Operation
        private CancellationTokenSource ReadCancellationTokenSource, WriteCancellationTokenSource;
        private Object ReadCancelLock = new Object();
        private uint fan_id;
        private Boolean IsReadTaskPending,request_started;
        private Boolean[] isToggleON = new Boolean[3];
        private uint ReadBytesCounter = 0;
        DataReader DataReaderObject = null;
        private Object WriteCancelLock = new Object();
        private Boolean IsWriteTaskPending,beingManipulated,getting_updated;
        DataWriter DataWriteObject = null;
        private Boolean IsNavigatedAway,toogle_pressed,dial_pressed;
        private Boolean[] faultMode;
        private static Byte[] received, toSend;
        private static Byte[] fanPWMValue,lastPWMValue,display_pwm_value;
        private static Byte  lastFansEnabled, fans_enabled,fans_display;
        private SingleTaskMessage fansCommand;
        private static System.Timers.Timer refreshValueTimer = null;
        private int sizeofStruct;
        private uint readBufferLength;
        private UInt32 magicHeader;
        private static AnimationSet Fault_Dark, Fault_Light;
        private bool updatingdata = false;
        private ObservableCollection<Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge> listOfDials;
        private ObservableCollection<ToggleSwitch> listOfToggles;
        public FansOperation()
        {
            this.InitializeComponent();
            handler = this;
            sizeofStruct = 64;
            listOfDials = new ObservableCollection<Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge>();
            listOfDials.Add(SetPointFan1);
            listOfDials.Add(SetPointFan2);
            listOfDials.Add(SetPointFan3);
            listOfToggles = new ObservableCollection<ToggleSwitch>();
            listOfToggles.Add(EnableFan1);
            listOfToggles.Add(EnableFan2);
            listOfToggles.Add(EnableFan3);

            listOfDials[0].ManipulationCompleted += SetPointFan1_ManipulationCompleted;
            listOfDials[0].ManipulationStarted += SetPointFan1_ManipulationStarted;
            listOfDials[0].Tapped += SetPointFan1_Tapped1;
            listOfDials[1].ManipulationCompleted += FansOperation_ManipulationCompleted;
            listOfDials[1].ManipulationStarted += FansOperation_ManipulationStarted;
            listOfDials[1].Tapped += FansOperation_Tapped;
            listOfDials[2].ManipulationCompleted += FansOperation_ManipulationCompleted1; ;
            listOfDials[2].ManipulationStarted += FansOperation_ManipulationStarted1; ;
            listOfDials[2].Tapped += FansOperation_Tapped1; ;
            toSend = new byte[sizeofStruct];
            fanPWMValue = new byte[NUMBER_OF_FANS];
            lastPWMValue = new byte[NUMBER_OF_FANS];
            faultMode= new Boolean[NUMBER_OF_FANS];
            display_pwm_value= new byte[NUMBER_OF_FANS];
            readBufferLength = 64;
            received = new byte[readBufferLength];
            Fault_Dark = SetPointFan1.Fade(value: 0.15f, duration: 1000, delay: 25, easingType: EasingType.Sine);
            //   NBC_Mode_Light = new AnimationSet(position);
            Fault_Light = SetPointFan1.Fade(value: 0.95f, duration: 1000, delay: 25, easingType: EasingType.Sine);
            Fault_Dark.Completed += Fault_Dark_Completed;
            Fault_Light.Completed += Fault_Light_Completed;
        }

        private async void FansOperation_Tapped1(object sender, TappedRoutedEventArgs e)
        {
            if (!updatingdata)
            {
                await WritePWMValue(2);
            }
            //throw new NotImplementedException();
        }

        private void FansOperation_ManipulationStarted1(object sender, ManipulationStartedRoutedEventArgs e)
        {
            beingManipulated = true;
            // throw new NotImplementedException();
        }

        private async void FansOperation_ManipulationCompleted1(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            await WritePWMValue(2);
            beingManipulated = false;
            // throw new NotImplementedException();
        }

        private async void FansOperation_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!updatingdata)
            {
                await WritePWMValue(1);
            }
            //  throw new NotImplementedException();
        }

        private void FansOperation_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            beingManipulated = true;
            // throw new NotImplementedException();
        }

        private async  void FansOperation_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            await WritePWMValue(1);
            beingManipulated = false;
            // throw new NotImplementedException();
        }

        private async void SetPointFan1_Tapped1(object sender, TappedRoutedEventArgs e)
        {
            //  throw new NotImplementedException();
            if (!updatingdata)
            {
                await WritePWMValue(0);
            }
           
        }

        private void Fault_Light_Completed(object sender, AnimationSetCompletedEventArgs e)
        {
            if (faultMode[0])
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
            if (faultMode[0])
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
         
               
                await RequestStatus();
                await ReadFansData();
                //  EnableValve.IsEnabled = true;

                for (int i = 0; i < NUMBER_OF_FANS; i++)
                {
                    listOfDials[i].IsEnabled = false;
                }

                UpdateFansView();
                Get_Fans_Display();
                for (int i = 0; i < NUMBER_OF_FANS; i++)
                {
                    listOfDials[i].IsEnabled = true;
                }
                updatingdata = false;
            }

            // EnableValve.IsEnabled = false;



        }
        private Byte Get_Fans_Display()
        {
            fans_display = 0;
            for (int i = 0; i < NUMBER_OF_FANS; i++)
            {
                if (listOfToggles[i].IsOn)
                {
                    fans_display |= (byte)(0x01 << i);
                }

            }
            return fans_display;
            
        }
        private async Task ReadFansData()
        {
            if (IsPerformingWrite())
            {
                rootPage.NotifyUser("busy on setting fans...", NotifyType.StatusMessage);
                return;
            }
            else
            {
                if (IsPerformingRead())
                {
                    rootPage.NotifyUser("waiting for status...", NotifyType.StatusMessage);
                    return;
                }
                else
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
                            rootPage.NotifyUser("Status read completed...", NotifyType.StatusMessage);
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
      
            

        }
        private void UpdateFansView()
        {
            int k;
            if (magicHeader.Equals(Commands.reverseMagic))
            {
                getting_updated = true;
                for (int i = 0; i < NUMBER_OF_FANS; i++)
                {
                    listOfToggles[i].IsEnabled = false;
                    listOfDials[i].Value = received[10+i] * 14000 / 255;
                    lastPWMValue[i] = received[10 + i];
                    k = ((1 + i) % 3);
                    if (((received[6]&0x07) & (0x01<<k)) > 0)
                    {
                        if (listOfToggles[i].IsOn)
                        {
                            listOfToggles[i].IsOn = false;
                        }
                        
                        listOfDials[i].Opacity = 0.4;

                    }
                    else
                    {
                        if (listOfToggles[i].IsOn)
                        {

                        }
                        else
                        {
                            listOfToggles[i].IsOn = true;
                        }
                        
                        listOfDials[i].Opacity = 1;

                    }
                    if (i<2)
                    {
                        Set_fault_mode(received[7], i);
                    }
                    else
                    {
                        Set_fault_mode(received[6], i - 1);
                    }
                    listOfToggles[i].IsEnabled = true;

                }

                getting_updated = false;
               




            }
        }
        private void Set_fault_mode(Byte register,int index)
        {
            if ((register & (0x07 << (4 * index))) < 7)
            {
                if (!faultMode[index])
                {
                    faultMode[index] = true;

                    Fault_Dark.StartAsync();
                }
                else
                {
                 //   faultMode[index] = false;
                 //   Fault_Dark.Stop();
                }

            }
            else
            {
               
                if (faultMode[index])
                {
                    faultMode[index] = false;
                    Fault_Dark.Stop();
                }
                else
                {
                  //  faultMode[index] = true;
                 //   Fault_Dark.StartAsync();
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
            if (IsPerformingWrite())
            {
                rootPage.NotifyUser("busy setting fans...", NotifyType.StatusMessage);
                return;
            }
            else
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
            refreshValueTimer.Interval = UPDATE_TIME;

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
            if (handler.IsPerformingWrite())
            {

            }
            else
            {
                await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                            new DispatchedHandler(() => { handler.UpdateFanStatus(); }));
            }
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
            if (getting_updated)
            {

            }
            else
            {
                lastFansEnabled = fans_enabled;
                if (listOfToggles[0].IsOn)
                {
                    listOfDials[0].Opacity = 1;
                    fans_enabled = (Byte)(lastFansEnabled | 0x01);


                }
                else
                {
                    listOfDials[0].Opacity = 0.4;
                    fans_enabled = (Byte)(lastFansEnabled & 0xfe);
                }
                if ((fans_enabled != lastFansEnabled) | (fans_display != fans_enabled))
                {
                    toogle_pressed = true;
                    await WriteAsyncFan(0);
                    toogle_pressed = false;
                }
            }
            
            
        }

        private async void EnableFan2_Toggled(object sender, RoutedEventArgs e)
        {
            if (getting_updated)
            {

            }
            else
            {
                lastFansEnabled = fans_enabled;
                if (listOfToggles[1].IsOn)
                {
                    listOfDials[1].Opacity = 1;
                    fans_enabled = (Byte)(lastFansEnabled | 0x02);


                }
                else
                {
                    listOfDials[1].Opacity = 0.4;
                    fans_enabled = (Byte)(lastFansEnabled & 0xfd);
                }
                if ((fans_enabled != lastFansEnabled) | (fans_display != fans_enabled))
                {
                    toogle_pressed = true;
                    await WriteAsyncFan(1);
                    toogle_pressed = false;
                }
            }
        }

        private async void EnableFan3_Toggled(object sender, RoutedEventArgs e)
        {
            if (getting_updated)
            {

            }
            else
            {
                lastFansEnabled = fans_enabled;
                if (listOfToggles[2].IsOn)
                {
                    listOfDials[2].Opacity = 1;
                    fans_enabled = (Byte)(lastFansEnabled | 0x04);


                }
                else
                {
                    listOfDials[2].Opacity = 0.4;
                    fans_enabled = (Byte)(lastFansEnabled & 0xfb);
                }
                if ((fans_enabled != lastFansEnabled)|(fans_display!= fans_enabled))
                {
                    toogle_pressed = true;
                    await WriteAsyncFan(2);
                    toogle_pressed = false;
                }
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

        private async void SetPointFan1_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            await WritePWMValue(0);
            beingManipulated = false;
        }
        private async Task WritePWMValue(int id)
        {
            //  lastPWMValue[id] = fanPWMValue[id];
            if (IsPerformingWrite())
            {

            }
            else
            {
                fanPWMValue[id] = (byte)(listOfDials[id].Value * 255 / 14000);
                if (fanPWMValue[id] != lastPWMValue[id])
                {
                    await WriteAsyncFan(id);
                }
            }
           
        }

       

        private async Task WriteAsyncFan(int id)
        {

           
            if (IsPerformingWrite())
            {
                rootPage.NotifyUser("already writting", NotifyType.StatusMessage);
            }
            else
            {
                if (IsPerformingRead())
                {
                    CancelReadTask();
                }
                if (EventHandlerForDevice.Current.IsDeviceConnected)
                {
                    try
                    {
                        rootPage.NotifyUser("Setting Fan: " + (id + 1).ToString(), NotifyType.StatusMessage);

                        // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                        // update the button states until after the write completes.


                        //   UpdateWriteButtonStates();
                        if (toogle_pressed)
                        {
                            Protocol.Message.CreateEnableFansMessage(fans_enabled).CopyTo(toSend, 0);
                            lastFansEnabled = fans_enabled;
                        }
                        else
                        {
                            if (fanPWMValue[id] != lastPWMValue[id])
                            {
                                Protocol.Message.CreateSetpointFansMessage(fanPWMValue).CopyTo(toSend, 0);
                                // lastPWMValue = fanPWMValue;
                            }
                        }
                        IsWriteTaskPending = true;
                        DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
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
                        DataWriteObject.Dispose();
                        rootPage.NotifyUser("Setting fan " + (id + 1).ToString() + " completed ", NotifyType.StatusMessage);

                        //  UpdateWriteButtonStates();
                    }
                }
                else
                {
                    Utilities.NotifyDeviceNotConnected();
                }
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
            rootPage.NotifyUser("Setting fans .. " , NotifyType.StatusMessage);
      
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
