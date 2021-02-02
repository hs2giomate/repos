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
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        private const int NUMBER_OF_HEATERS = 2;
        private const int NUMBER_OF_RELAYS = 4;
        // Track Read Operation
        private CancellationTokenSource ReadCancellationTokenSource;
        private Object ReadCancelLock = new Object();

        private Boolean IsReadTaskPending;
        private Boolean[] isToggleON=new Boolean[NUMBER_OF_HEATERS* NUMBER_OF_RELAYS];
        private uint ReadBytesCounter = 0;
        DataReader DataReaderObject = null;
        private Byte[] relays_status;
        private ObservableCollection<ToggleSwitch> listOfToggles;
        public ObservableCollection<TextBlock> listOfTextBlocks;
        public ObservableCollection<Border> listOfBorders;
        public ObservableCollection<Windows.UI.Xaml.Shapes.Ellipse> listOfEllipses,listOfOvertemperatures;
        public ObservableCollection<AnimationSet> listOfDarkAnimations,listOfLightAnimations;
        private CancellationTokenSource WriteCancellationTokenSource;
        private Object WriteCancelLock = new Object();
        private Boolean IsWriteTaskPending,request_sucess,read_request_success;
        DataWriter DataWriteObject = null;
        private Boolean IsNavigatedAway,updating_toogles_view;
        private static Byte[] received, toSend;
        private static Byte heaterRelaysCommand,lastRelaysCommand;
        private uint readBufferLength;
        private Task blink;
        private SingleTaskMessage heatersCommand;
        private static System.Timers.Timer heaterTimer = null;
        private int sizeofStruct;
        private UInt32 magicHeader = 0;
        private bool updatingStatus=false;
        private const int UPDATING_TIME = 1000;
        private bool required_status=false;
        private static bool timer_disposed;


        public HeaterOperation()
        {
            this.InitializeComponent();
            handler = this;
            readBufferLength = 64;
            received = new byte[readBufferLength];
            relays_status = new Byte[NUMBER_OF_HEATERS];
            heaterRelaysCommand = 0;
            sizeofStruct = Marshal.SizeOf(typeof(SingleTaskMessage));
            listOfToggles=new ObservableCollection<ToggleSwitch>();
            listOfToggles.Add(Relay1EnableToggle);
            listOfToggles.Add(Relay2EnableToggle);
            listOfToggles.Add(Relay3EnableToggle);
            listOfToggles.Add(Relay4EnableToggle);
            listOfToggles.Add(Relay1EnableToggle1);
            listOfToggles.Add(Relay2EnableToggle1);
            listOfToggles.Add(Relay3EnableToggle1);
            listOfToggles.Add(Relay4EnableToggle1);
            listOfTextBlocks = new ObservableCollection<TextBlock>();
            listOfTextBlocks.Add(Relay1StatusText);
            listOfTextBlocks.Add(Relay2StatusText);
            listOfTextBlocks.Add(Relay3StatusText);
            listOfTextBlocks.Add(Relay4StatusText);
            listOfTextBlocks.Add(OvertemperautureText);
            listOfTextBlocks.Add(Relay1StatusText1);
            listOfTextBlocks.Add(Relay2StatusText1);
            listOfTextBlocks.Add(Relay3StatusText1);
            listOfTextBlocks.Add(Relay4StatusText1);
            listOfTextBlocks.Add(OvertemperautureText1);
            listOfBorders = new ObservableCollection<Border>();
            listOfBorders.Add(Relay1StatusBorder);
            listOfBorders.Add(Relay2StatusBorder);
            listOfBorders.Add(Relay3StatusBorder);
            listOfBorders.Add(Relay4StatusBorder);
            listOfBorders.Add(Relay1StatusBorder1);
            listOfBorders.Add(Relay2StatusBorder1);
            listOfBorders.Add(Relay3StatusBorder1);
            listOfBorders.Add(Relay4StatusBorder1);
            listOfEllipses = new ObservableCollection<Ellipse>();
            listOfEllipses.Add(Relay1FaultSignal);
            listOfEllipses.Add(Relay2FaultSignal);
            listOfEllipses.Add(Relay3FaultSignal);
            listOfEllipses.Add(Relay4FaultSignal);
            listOfEllipses.Add(OverTemperatureFaultSignal);
            listOfEllipses.Add(Relay1FaultSignal1);
            listOfEllipses.Add(Relay2FaultSignal1);
            listOfEllipses.Add(Relay3FaultSignal1);
            listOfEllipses.Add(Relay4FaultSignal1);
                     
            listOfEllipses.Add(OverTemperatureFaultSignal1);
            listOfDarkAnimations = new ObservableCollection<AnimationSet>();
            listOfLightAnimations = new ObservableCollection<AnimationSet>();
            for (int j = 0; j < NUMBER_OF_HEATERS; j++)
            {
                for (int i = 5*j; i < 5*(1+j); i++)
                {
                    listOfDarkAnimations.Add(listOfEllipses[i].Fade(value: 0.15f, duration: 1000, delay: 25, easingType: EasingType.Sine));
                    listOfLightAnimations.Add(listOfEllipses[i].Fade(value: 0.95f, duration: 1000, delay: 25, easingType: EasingType.Sine));
                }
              //  listOfDarkAnimations.Add(listOfOvertemperatures[j].Fade(value: 0.15f, duration: 1000, delay: 25, easingType: EasingType.Sine));
              //  listOfLightAnimations.Add(listOfOvertemperatures[j].Fade(value: 0.95f, duration: 1000, delay: 25, easingType: EasingType.Sine));
              
             
               

            }
            listOfDarkAnimations[listOfDarkAnimations.Count-1].Completed += FaultDarkAnimation_Completed;
            listOfLightAnimations[listOfLightAnimations.Count-1].Completed += FaultLighAnimation_Completed;

            foreach  (AnimationSet an in listOfDarkAnimations) {
                an.StartAsync();
            }
           
           
        }
        protected  override void OnNavigatedTo(NavigationEventArgs eventArgs)
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
            heaterTimer = new System.Timers.Timer();
            heaterTimer.Interval = UPDATING_TIME;

            // Hook up the Elapsed event for the timer. 
            heaterTimer.Elapsed += OnTimedEvent;

            // Have the timer fire repeated events (true is the default)
            heaterTimer.AutoReset = false;

            // Start the timer
            heaterTimer.Enabled = true;
            timer_disposed = false;
           


        }
        private void FaultLighAnimation_Completed(object sender, AnimationSetCompletedEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
              blink=  listOfDarkAnimations[i].StartAsync();
            }

              
          
            //  throw new NotImplementedException();
        }
        private void FaultDarkAnimation_Completed(object sender, AnimationSetCompletedEventArgs e)
        {


            for (int i = 0; i < 10; i++)
            {
              blink=  listOfLightAnimations[i].StartAsync();
            }

            // throw new NotImplementedException();
        }
        private static async void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (handler.IsPerformingWrite())
            {

            }
            else
            {
                await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                             new DispatchedHandler(() => {
                                 handler.UpdateDataRelayStatus();
                             }));

            }

            if (heaterTimer!=null)
            {
                heaterTimer.Start();
            }

            
        }
        public  async void UpdateDataRelayStatus()
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
            if ((heaterTimer != null)&(!timer_disposed))
            {
                heaterTimer.Stop();
                heaterTimer.Dispose();
                timer_disposed = true;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs eventArgs)
        {
            IsNavigatedAway = true;
            if (heaterTimer != null)
            {
                    heaterTimer.Stop();
                     heaterTimer.Dispose();
                timer_disposed = true;
            }
            CancelAllIoTasks();
        }
        private Boolean IsPerformingRead()
        {
            return (IsReadTaskPending);
        }
    
        private async Task RequestRelayStatus()
        {
            if (IsPerformingWrite())
            {
                rootPage.NotifyUser("already reading heaters...", NotifyType.WarningMessage);
            }
            else
            {
                
                if (IsPerformingRead())
                {
                    CancelReadTask();
                    rootPage.NotifyUser("Read task cancelled...", NotifyType.WarningMessage);
                    required_status = (heaterRelaysCommand > 0);
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
                        rootPage.NotifyUser("Sendin request to Heaters...", NotifyType.StatusMessage);

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
                    required_status = (heaterRelaysCommand > 0);
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
            rootPage.NotifyUser("Read Heater Status completed ", NotifyType.StatusMessage);
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
        private async Task SendrequestStatusAsync(CancellationToken cancellationToken)
        {

            Task<UInt32> storeAsyncTask;

        
             
               
                toSend = new byte[sizeofStruct];
              //  toSend.Initialize();
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
                if (bytesWritten>0)
                {
                   request_sucess = true;
                }
                rootPage.NotifyUser("Request Completed - " + bytesWritten.ToString() + " bytes written", NotifyType.StatusMessage);
    

        }
        private Boolean IsPerformingWrite()
        {
            return (IsWriteTaskPending);
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
                        rootPage.NotifyUser("Heater Setting completed - ", NotifyType.StatusMessage);

                    //  UpdateWriteButtonStates();
                }
                }
                else
                {
                    Utilities.NotifyDeviceNotConnected();
                }
            
        }
        private async Task UpdateViewRelayStatus()
        {
            if (IsPerformingWrite())
            {

            }
            else {
               

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
            UpdateRelayStatusText();
        }
        private  void UpdateRelayStatusText()
        {
            for (int j = 0; j < NUMBER_OF_HEATERS; j++)
            {
                relays_status[j] = received[6+j*2];
                if ((relays_status[j] & (Byte)(0x01)) == 0)
                {
                    listOfTextBlocks[4+5*j].Text = "Fault";
                    for (int i = 5*j; i < 4+5*j; i++)
                    {
                        listOfTextBlocks[i].Text = "Off";
                    }
                }
                else
                {

                    listOfTextBlocks[4+5*j].Text = "OK";
                    for (int i = 4*j; i < 4*(1+j); i++)
                    {
                        if ((relays_status[j] & (Byte)(0x10 >> (i-4*j))) == 0)
                        {
                            listOfTextBlocks[i+j].Text = "Fault";
                        }
                        else
                        {

                            if (listOfToggles[i].IsOn)
                            {
                                listOfTextBlocks[i+j].Text = "Heating";
                            }
                            else
                            {
                                listOfTextBlocks[i+j].Text = "Off";
                            }
                        }

                    }
                }
            }

           


        }
        private   void UpdateToggles()
        {
            updating_toogles_view = true;
            for (int j = 0; j < NUMBER_OF_HEATERS; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    if ((received[7+2*j] & (0x01 << i)) > 0)
                    {
                        listOfToggles[i+4*j].IsOn = false;
                    }
                    else
                    {
                        listOfToggles[i+4*j].IsOn = true;
                    }
                    isToggleON[i] = listOfToggles[i].IsOn;
                }
            }
               
            updating_toogles_view = false;
              
        }
        private async Task UpdateFaultStatusSignal()
        {
            for (int j = 0; j < NUMBER_OF_HEATERS; j++)
            {
                if ((received[6+2*j] & (Byte)(0x01)) == 0)
                {
                    listOfEllipses[4+5*j].Visibility = Visibility.Visible;
                    for (int i = 4*j; i < 4*(j+1); i++)
                    {
                        listOfBorders[i].Visibility = Visibility.Collapsed;
                    }
                }
                else
                {

                    listOfEllipses[4+5*j].Visibility = Visibility.Collapsed;
                    for (int i = 4*j; i < 4*(j+1); i++)
                    {
                        if ((received[6 + 2 * j] & (Byte)(0x10 >> (i-4*j))) == 0)
                        {
                            listOfBorders[i].Visibility = Visibility.Collapsed;
                            listOfEllipses[i+j].Visibility = Visibility.Visible;
                            //  blink = faultDarkAnimation.StartAsync();
                        }
                        else
                        {
                            //  await StopFadingRelay1();
                            if (blink != null)
                            {
                                await blink;
                            }


                            if (listOfToggles[i].IsOn)
                            {
                                listOfBorders[i].Visibility = Visibility.Visible;
                                listOfEllipses[i+j].Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                listOfBorders[i].Visibility = Visibility.Collapsed;
                                listOfEllipses[i+j].Visibility = Visibility.Collapsed;

                            }
                        }

                    }
                }
            }


        }

        private async Task StopFadingRelay1()
        {
            // Setting the dispatcher priority to high allows the UI to handle disabling of all the buttons
            // before any of the IO completion callbacks get a chance to modify the UI; that way this method
            // will never get the opportunity to overwrite UI changes made by IO callbacks
            await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                new DispatchedHandler(() =>
                {
                    Relay1FaultSignal.Fade().Stop();
              

                  
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

        private  async void Relay1EnableToggle_Toggled(object sender, RoutedEventArgs e)
        {
          await   Handle_Toogles(0);
           
        }

        private async void Relay2Enable1Toggle_Toggled(object sender, RoutedEventArgs e)
        {
            await Handle_Toogles(1);
        }



        private async void Relay3HeaterEnable_Toggled(object sender, RoutedEventArgs e)
        {
            await Handle_Toogles(2);
        }

        private async void Relay4Enable1Toggle_Toggled(object sender, RoutedEventArgs e)
        {
            await Handle_Toogles(3);
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

        private async Task Handle_Toogles(int i)
        {
            if (!updating_toogles_view)
            {
                lastRelaysCommand = heaterRelaysCommand;
            }

            if (listOfToggles[i].IsOn)
            {

                heaterRelaysCommand = (Byte)(lastRelaysCommand | (0x01<<i));
            }
            else
            {
                heaterRelaysCommand = (Byte)(lastRelaysCommand & (~(0x01<<i)));
            }
            if (!updatingStatus)
            {
                if (heaterRelaysCommand != lastRelaysCommand)
                {
                    if (!updating_toogles_view)
                    {
                        await WriteAsyncHeatersEnables();
                    }

                }
            }
        }

        private async  void Relay1EnableToggle1_Toggled(object sender, RoutedEventArgs e)
        {
            await Handle_Toogles(4);
        }

        private async void Relay2EnableToggle1_Toggled(object sender, RoutedEventArgs e)
        {
            await Handle_Toogles(5);
        }

        private async void Relay3EnableToggle1_Toggled(object sender, RoutedEventArgs e)
        {
            await Handle_Toogles(6);
        }

        private async void Relay4EnableToggle1_Toggled(object sender, RoutedEventArgs e)
        {
            await Handle_Toogles(7);
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
