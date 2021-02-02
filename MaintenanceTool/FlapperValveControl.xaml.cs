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
        private  const int NUMBER_VALVES= 2;
        private const double CALLING_INTERVAL=1000*NUMBER_VALVES;

        // Track Read Operation
        private CancellationTokenSource ReadCancellationTokenSource, WriteCancellationTokenSource;
        private Object ReadCancelLock = new Object();

        private Boolean IsReadTaskPending;
        private Boolean[] isToggleON = new Boolean[3];
        private uint ReadBytesCounter = 0;
        DataReader DataReaderObject = null;
        private Object WriteCancelLock = new Object();
        private Boolean IsWriteTaskPending,is_flapper_OK,invalid_position;
        private Boolean[] manipulating;
        DataWriter DataWriteObject = null;
        private Boolean IsNavigatedAway, toogle_pressed, is_updating_view, request_succesfull,read_succesfull;
        private static Byte[] received, toSend,last_minimum_air;
        private static Byte[] currentPosition, lastPosition, lastCommand, commandValve, lastSetpoint, currentSetpoint;
        private SingleTaskMessage fansCommand;
        private static System.Timers.Timer flapperValveTimer = null;
        private int sizeofStruct;
        private uint readBufferLength;
        private UInt32 magicHeader;
        private static AnimationSet NBC_Mode_Dark,NBC_Mode_Light;
        private bool[] nbcMode;
        private MinimunFreshAir minimunValues;
        private Byte minimunAirPosition,last_read_minimum;
        private const Byte minimunFailValue = 20;
        private ObservableCollection<Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge> listOfDials;
        private ObservableCollection<ToggleSwitch> listOfToggles;
        private ObservableCollection<AnimationSet> listOfDarkAnimations, listOfLightAnimations;
        private ObservableCollection<TextBlock> listOfTextBlocks;
        private ObservableCollection<Border> listOfBorders;
        private bool required_feedback;
        private const int DATA_OFFSET=21;
        private byte result_minimum;
        private static bool timer_disposed;
        
        public FlapperValveControl()
        {
            this.InitializeComponent();
            handler = this;
            sizeofStruct = 64;
            toSend = new byte[sizeofStruct];
            nbcMode = new bool[NUMBER_VALVES];
            manipulating = new Boolean[NUMBER_VALVES];
            currentPosition = new Byte[NUMBER_VALVES];
            lastPosition = new Byte[NUMBER_VALVES];
            lastCommand = new Byte[NUMBER_VALVES];
            commandValve = new Byte[NUMBER_VALVES];
            lastSetpoint = new Byte[NUMBER_VALVES];
            currentSetpoint= new Byte[NUMBER_VALVES];
            last_minimum_air = new Byte[NUMBER_VALVES];
            readBufferLength = 64;
            received = new byte[readBufferLength];
            listOfDials = new ObservableCollection<Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge>();
          
            listOfDials.Add(position);
            listOfDials.Add(position2);
            listOfDials.Add(MinimumPositionGauge);
            listOfDials.Add(MinimumPositionGauge2);
            listOfToggles = new ObservableCollection<ToggleSwitch>();
            listOfToggles.Add(EnableValve);
            listOfToggles.Add(EnableValve1);
            listOfTextBlocks = new ObservableCollection<TextBlock>();
            listOfTextBlocks.Add(AngleFlapper);
            listOfTextBlocks.Add(PressedLabel1);
            listOfTextBlocks.Add(PressedLabel2);
            listOfTextBlocks.Add(PressedLabel3);
            listOfTextBlocks.Add(AngleFlapper1);
            listOfTextBlocks.Add(PressedLabel4);
            listOfTextBlocks.Add(PressedLabel5);
            listOfTextBlocks.Add(PressedLabel6);
            listOfBorders = new ObservableCollection<Border>();
            listOfBorders.Add(LimitSwitchBorder1);
            listOfBorders.Add(LimitSwitchBorder2);
            listOfBorders.Add(LimitSwitchBorder3);
            listOfBorders.Add(LimitSwitchBorder4);
            listOfBorders.Add(LimitSwitchBorder5);
            listOfBorders.Add(LimitSwitchBorder6);
           // listOfDials[0].Tapped += Position_Tapped;
            listOfDials[0].ManipulationStarted += Position_ManipulationStarted;
            listOfDials[1].ManipulationStarted += FlapperValveControl_ManipulationStarted;
            listOfDials[0].ManipulationCompleted += position_ManipulationCompleted;
            listOfDials[1].ManipulationCompleted += FlapperValveControl_ManipulationCompleted2;
            listOfToggles[0].ManipulationStarted += EnableValve_ManipulationStarted;
            listOfToggles[0].ManipulationCompleted += EnableValve_ManipulationCompleted;
            listOfToggles[1].ManipulationStarted += FlapperValveControl_ManipulationStarted1;
            listOfToggles[1].ManipulationCompleted += FlapperValveControl_ManipulationCompleted;
           
            listOfDarkAnimations = new ObservableCollection<AnimationSet>();
            listOfLightAnimations = new ObservableCollection<AnimationSet>();
            for (int i = 0; i < NUMBER_VALVES; i++)
            {
                nbcMode[i] = false;
                manipulating[i] = false;
                  listOfDials[i].IsInteractive = false;
                // listOfDials[i].CanDrag = true;
                //   listOfDials[i].AllowDrop=true;
                listOfDials[i].IsHoldingEnabled=true;
                listOfDials[i].IsTapEnabled = false;
                listOfDials[i].Opacity = 0.2;
                listOfDarkAnimations.Add(listOfDials[i].Fade(value: 0.15f, duration: 1000, delay: 25, easingType: EasingType.Sine));
                listOfLightAnimations.Add(listOfDials[i].Fade(value: 0.95f, duration: 1000, delay: 25, easingType: EasingType.Sine));

            }

            listOfDarkAnimations[0].Completed += NBC_Mode_Dark_Completed;
            listOfLightAnimations[0].Completed += NBC_Mode_Light_Completed;
            listOfDarkAnimations[1].Completed += FlapperValveDarkAnimation_Completed1;
            listOfLightAnimations[1].Completed += FlapperValveLigthAnimation_Completed1;
            minimunValues = new MinimunFreshAir();
            minimunAirPosition = minimunFailValue;
            timer_disposed = false;
    
        }

        private void FlapperValveControl_ManipulationCompleted2(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void FlapperValveControl_ManipulationCompleted1(object sender, ManipulationCompletedRoutedEventArgs e)
        {
           // throw new NotImplementedException();
        }

        private void FlapperValveControl_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            manipulating[1] = false;
            // throw new NotImplementedException();
        }

        private void FlapperValveControl_ManipulationStarted1(object sender, ManipulationStartedRoutedEventArgs e)
        {
            manipulating[1] = true;
            if (IsPerformingRead())
            {
               // CancelReadTask();
                // while (IsPerformingRead()) ;
            }
            // throw new NotImplementedException();
        }

        private void FlapperValveLigthAnimation_Completed1(object sender, AnimationSetCompletedEventArgs e)
        {
            if (nbcMode[1])
            {
                listOfDarkAnimations[1].StartAsync();
            }
            else
            {
                listOfDarkAnimations[1].Stop();
            }
            //throw new NotImplementedException();
        }

        private void FlapperValveDarkAnimation_Completed1(object sender, AnimationSetCompletedEventArgs e)
        {
            if (nbcMode[1])
            {
                listOfLightAnimations[1].StartAsync();
            }
            else
            {
                listOfLightAnimations[1].Stop();
            }
            //  throw new NotImplementedException();
        }

        private void FlapperValveControl_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            manipulating[1] = true;
            if (IsPerformingRead())
            {
              //  CancelReadTask();
                // while (IsPerformingRead()) ;
            }
            // throw new NotImplementedException();
        }

        private void NBC_Mode_Light_Completed(object sender, AnimationSetCompletedEventArgs e)
        {
            if (nbcMode[0])
            {
                listOfDarkAnimations[0].StartAsync();
            }
            else
            {
                listOfDarkAnimations[0].Stop();
            }

            //throw new NotImplementedException();
        }

        private void NBC_Mode_Dark_Completed(object sender, AnimationSetCompletedEventArgs e)
        {
            if (nbcMode[0])
            {
                listOfLightAnimations[0].StartAsync();
            }
            else
            {
                listOfLightAnimations[0].Stop();
            }
            // throw new NotImplementedException();
            
        }

        private void EnableValve_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            manipulating[0] = false;
           // throw new NotImplementedException();
        }

        private void EnableValve_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            manipulating[0] = true;
            if (IsPerformingRead())
            {
               // CancelReadTask();
                // while (IsPerformingRead()) ;
            }
            // throw new NotImplementedException();
        }

        private void Position_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            manipulating[0] = true;
            if (IsPerformingRead())
            {
                //CancelReadTask();
                // while (IsPerformingRead()) ;
            }
            //throw new NotImplementedException();
        }

        private  void Position_Tapped(object sender, TappedRoutedEventArgs e)
        {
            
            Debug.WriteLine(position.Value);
          // await WriteSetpointValue();
           // throw new NotImplementedException();
        }

        protected override  void OnNavigatedTo(NavigationEventArgs eventArgs)
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
                UpdateValvesData();
                
               
           
              
                StartStatusCheckTimer();
               
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs eventArgs)
        {
            IsNavigatedAway = true;
            if (flapperValveTimer != null)
            {
                flapperValveTimer.Stop();
                flapperValveTimer.Dispose();
                timer_disposed = true;
            }
            CancelAllIoTasks();
        }
        private async Task UpdateMinimunValues()
        {
            Task<byte> return_byte;
            try
            {
                 return_byte= minimunValues.GetminimunValidAirPosition();
                result_minimum = await return_byte;
                if (result_minimum==0xff)
                {
                    minimunAirPosition = last_read_minimum;
                }
                else
                {
                    last_read_minimum = minimunAirPosition;
                    minimunAirPosition = (byte)(minimunValues.minimunValid);
                  //  minimunAirPosition = 16;
                }
              

            }
            catch (Exception e)
            {
                //Debug.WriteLine(e.Message);
                rootPage.NotifyUser(e.Message.ToString(), NotifyType.ErrorMessage);
                minimunAirPosition = last_read_minimum;
                //  throw;
            }

            
           int j ;
           for (int i = 0; i < NUMBER_VALVES; i++)
            {
                j = i + 2;
                if (minimunAirPosition < minimunFailValue)
                {
                    listOfDials[j].MinAngle = 90 - minimunFailValue * 90 / 255;
                    listOfDials[j].Minimum = 90 - minimunFailValue * 90 / 255;

                }
                else
                {
                    if (minimunAirPosition != last_minimum_air[i])
                    {
                        last_minimum_air[i] = minimunAirPosition;
                        listOfDials[j].MinAngle = 90 - minimunAirPosition * 90 / 255;
                        listOfDials[j].Minimum = 90 - minimunAirPosition * 90 / 255;
                        listOfDials[j].Value = 90 - minimunAirPosition * 90 / 255;
                    }

                }
            }
         
           
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
            if (flapperValveTimer != null)
            {
                flapperValveTimer.Stop();
                flapperValveTimer.Dispose();
            }
        }
        public void StartStatusCheckTimer()
        {
            // Create a timer and set a two second interval.
            flapperValveTimer = new System.Timers.Timer();
            flapperValveTimer.Interval = CALLING_INTERVAL;

            // Hook up the Elapsed event for the timer. 
            flapperValveTimer.Elapsed += OnTimedEvent;

            // Have the timer fire repeated events (true is the default)
            flapperValveTimer.AutoReset = false;

            // Start the timer
            flapperValveTimer.Enabled = true;
            timer_disposed = false;


        }
        private static async void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            // Debug.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
            //  MaintenanceToolHandler.Current.SendAliveMessage();
            if ((handler.IsPerformingWrite())|(handler.manipulating[0]| handler.manipulating[1]))
            {

            }
            else
            {
                await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
             new DispatchedHandler(() => {
                 handler.UpdateValvesData();
             }));
            }
            if (!timer_disposed)
            {
                flapperValveTimer.Start();
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


     
             

        private async void position_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            await WriteSetpointValue(0);
            manipulating[0] = false;
        }

        private async Task WriteSetpointValue(int valve_id)
        {
            if (listOfToggles[valve_id].IsOn)
            {
                lastSetpoint[valve_id] = currentSetpoint[valve_id];
                currentSetpoint[valve_id] = (byte)(listOfDials[valve_id].Value * 255 / 90);
               // position.Value = lastPosition;
                if (currentPosition[valve_id] != currentSetpoint[valve_id])
                {
                    listOfToggles[valve_id].IsEnabled = false;
                    listOfDials[valve_id].IsInteractive = false;
                    await WriteValveData(valve_id);
               //     await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                //       new DispatchedHandler(() => { handler.UpdateDataPosition(); }));
                    listOfDials[valve_id].IsInteractive = true;
                    listOfToggles[valve_id].IsEnabled = true;
                }
            }
            

        }
        private async Task WriteValveData(int valve_id)
        {

       
          
            if (IsPerformingWrite())
            {
                if (request_succesfull)
                {
                    rootPage.NotifyUser("already writting", NotifyType.StatusMessage);
                }
                else
                {
                  //  CancelWriteTask();
                  //  IsWriteTaskPending = false;
                 //   rootPage.NotifyUser("cancelling last write", NotifyType.WarningMessage);
                }
                
            }
            else
            {
                if (IsPerformingRead())
                {
                    CancelReadTask();
                    rootPage.NotifyUser("Cancelling reading task...", NotifyType.WarningMessage);
                }
                if (EventHandlerForDevice.Current.IsDeviceConnected)
                {
                    try
                    {
                        rootPage.NotifyUser("Sending Command...", NotifyType.StatusMessage);

                        // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                        // update the button states until after the write completes.
                       
                       // toSend.Initialize();
                        //   UpdateWriteButtonStates();
                        if (toogle_pressed)
                        {
                            Protocol.Message.CreateCommandFlapperValveMessage(commandValve).CopyTo(toSend, 0);
                           // lastCommand[valve_id] = commandValve[valve_id];
                        }
                        else if (listOfToggles[valve_id].IsOn)
                        {
                            if (currentSetpoint[valve_id] != currentPosition[valve_id])
                            {
                                Protocol.Message.CreateSetpointFlapperValveMessage(currentSetpoint).CopyTo(toSend, 0);
                               // currentSetpoint[valve_id] = lastSetpoint[valve_id];
                            }
                        }
                        IsWriteTaskPending = true;
                        DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                        await WriteAsync(WriteCancellationTokenSource.Token);

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
                        catch (Exception)
                        {
                            rootPage.NotifyUser("DataWriter detach failed...", NotifyType.StatusMessage);
                            //  throw;
                        }

                        DataWriteObject.Dispose();

                        //  UpdateWriteButtonStates();
                    }
                }
                else
                {
                    Utilities.NotifyDeviceNotConnected();
                }
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
        public async void UpdateValvesData()
        {
          //  for (int i = 0; i < number_flappers; i++)
          //  {
                if (!(manipulating[0]|manipulating[1]))
                {
                     request_succesfull = false;
                    await RequestValveData();
                    if (request_succesfull)
                    {
                        read_succesfull = false;
                        await ReadValveData();
                        if (read_succesfull)
                        {
                            await UpdateMinimunValues();
                            UpdateDialPositions();
                    }
                    else
                    {
                        read_succesfull = true;
                    }
                }
                else
                {
                    request_succesfull = true;
                }
                    
                    //  EnableValve.IsEnabled = true;
         

                   
                }
          //  }
     
            
           



        }
        private Boolean IsPerformingRead()
        {
            return (IsReadTaskPending);
        }
        private Boolean IsPerformingWrite()
        {
            return (IsWriteTaskPending);
        }
        private async Task RequestValveData()
        {
            if (IsPerformingWrite())
            {
                //  CancelReadTask();
                // while (IsPerformingRead()) ;
                rootPage.NotifyUser("Already asking data...", NotifyType.StatusMessage);
            }
            else
            {
                if (IsPerformingRead())
                {
                    CancelReadTask();
                    rootPage.NotifyUser("Cancelling reading...", NotifyType.WarningMessage);
                }
                else
                {
                    if (EventHandlerForDevice.Current.IsDeviceConnected)
                    {
                        Enable_Disable_Controls(false);
                        try
                        {
                            rootPage.NotifyUser("Sending request...", NotifyType.StatusMessage);
                           
                            // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                            // update the button states until after the write completes.
                            Protocol.Message.CreateValvePositionRequestMessage().CopyTo(toSend, 0);
                            IsWriteTaskPending = true;
                            DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                            
                                await SendRequestPositionAsync(WriteCancellationTokenSource.Token);
                            
                           
                           
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
                            catch (Exception)
                            {
                                rootPage.NotifyUser("request detach failed...", NotifyType.StatusMessage);
                                //  throw;
                            }
                            DataWriteObject.Dispose();;
                            request_succesfull = true;

                            // UpdateWriteButtonStates();
                        }
                        Enable_Disable_Controls(true);
                    }
                    else
                    {
                        Utilities.NotifyDeviceNotConnected();
                    }
                }
                
            }
           
        }
        private void Enable_Disable_Controls(bool st)
        {
            for (int i = 0; i < NUMBER_VALVES; i++)
            {
                listOfDials[i].IsEnabled = st;
                listOfDials[i].IsInteractive = st;
                listOfToggles[i].IsEnabled = st;
                
            }
        }
        private void Enable_Disable_Controls(bool st,int id)
        {
          
                listOfDials[id].IsEnabled = st;
                listOfDials[id].IsInteractive = st;
                listOfToggles[id].IsEnabled = st;

            
        }

        private async void  position2_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            await WriteSetpointValue(1);
            manipulating[1] = false;
        }

        private async void EnableValve1_Toggled(object sender, RoutedEventArgs e)
        {
            // lastCommand[1] = commandValve[1];
            if (!is_updating_view)
            {
                lastCommand[1] = commandValve[1];
            }
            if (listOfToggles[1].IsOn)
            {

                commandValve[1] = (Byte)(lastCommand[1] | 0x01);

            }
            else
            {

                commandValve[1] = (Byte)(lastCommand[1] & 0xfe);

            }
            if (is_updating_view)
            {

            }
            else
            {
                if (commandValve[1] != lastCommand[1])
                {
                    //  EnableValve.IsEnabled = false;
                    toogle_pressed = true;
                    await WriteValveData(1);
                    toogle_pressed = false;
                    //    await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    //     new DispatchedHandler(() => { handler.UpdateDataPosition(); }));
                    //  EnableValve.IsEnabled = true;

                }
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
        private async Task ReadValveData()
        {
            required_feedback = ((commandValve[0] > 0) | (commandValve[1] > 0));
            if (IsPerformingWrite())
            {
                rootPage.NotifyUser("busy on writting data...", NotifyType.StatusMessage);
            }
            else
            {
                if (required_feedback)
                {
                    if (IsPerformingRead())
                    {
                        CancelReadTask();
                        rootPage.NotifyUser("past read thread cancelled...", NotifyType.WarningMessage);
                        // while (IsPerformingRead()) ;
                    }
                    else
                    {

                    }
                }
                else
                {
                    if (IsPerformingRead())
                    {
                        rootPage.NotifyUser("busy on reading data...", NotifyType.StatusMessage);
                        return;
                        // CancelReadTask();
                        // while (IsPerformingRead()) ;
                    }
                }
                            
                if (EventHandlerForDevice.Current.IsDeviceConnected)
                {
                    Enable_Disable_Controls(false);
                    try
                    {
                        rootPage.NotifyUser("Reading Valve Data...", NotifyType.StatusMessage);

                        // We need to set this to true so that the buttons can be updated to disable the read button. We will not be able to
                        // update the button states until after the read completes.
                        IsReadTaskPending = true;
                        DataReaderObject = new DataReader(EventHandlerForDevice.Current.Device.InputStream);
                        
                            await ReadDataAsync(ReadCancellationTokenSource.Token);
                        
                        // UpdateReadButtonStates();

                       
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
                        try
                        {
                            DataReaderObject.DetachStream();
                        }
                        catch (Exception)
                        {
                            rootPage.NotifyUser("failed to Detach on read status...", NotifyType.ErrorMessage);
                            //  throw;
                        }

                        DataReaderObject.Dispose();
                        rootPage.NotifyUser("Valve Data got read...", NotifyType.StatusMessage);
                        // UpdateReadButtonStates();
                        // UpdateAllToggleBits();
                    }
                    Enable_Disable_Controls(true);
                }
                else
                {
                    Utilities.NotifyDeviceNotConnected();
                }

                
            }
            

           

        }

        private async void EnableValve_Toggled(object sender, RoutedEventArgs e)
        {
            if (!is_updating_view)
            {
                lastCommand[0] = commandValve[0];
            }
          
            if (listOfToggles[0].IsOn)
            {
                
                commandValve[0] = (Byte)(lastCommand[0] | 0x01);
              
            }
            else
            {
               
                commandValve[0] = (Byte)(lastCommand[0] & 0xfe);
              
            }
            if (is_updating_view)
            {

            }
            else
            {
                if (commandValve[0] != lastCommand[0])
                {
                    //  EnableValve.IsEnabled = false;
                    toogle_pressed = true;
                    await WriteValveData(0);
                    toogle_pressed = false;
                    //    await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    //     new DispatchedHandler(() => { handler.UpdateDataPosition(); }));
                    //  EnableValve.IsEnabled = true;

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
                rootPage.NotifyUser("Flapper Valve data was read", NotifyType.StatusMessage);
                read_succesfull = true;

            }
           
        }
        private void UpdateDialPositions()
        {
            int k;
            is_updating_view = true;
         
            for (int i = 0; i < NUMBER_VALVES; i++)
            {
                if (received[26+DATA_OFFSET * i]>0)
                {
                    if (magicHeader.Equals(Commands.reverseMagic))
                    {
                        lastPosition[i] = currentPosition[i];
                        currentPosition[i] = received[20 + DATA_OFFSET * i];
                        listOfDials[i].IsEnabled = false;
                        listOfDials[i].Value = currentPosition[i] * 90 / 255;
                        listOfTextBlocks[i * 4].Text = (90 - currentPosition[i] * 90 / 255).ToString("N0");
                        k = 8;
                        for (int j = 3 * i; j < listOfBorders.Count - 3 * (1 - i); j++)
                        {
                            listOfBorders[j].Visibility = received[k + (DATA_OFFSET * i)] > 0 ? Visibility.Visible : Visibility.Collapsed;
                            listOfTextBlocks[j + (1+i)].Visibility = received[k + (DATA_OFFSET * i)] > 0 ? Visibility.Visible : Visibility.Collapsed;
                            k++;
                        }
                        if (received[24 + DATA_OFFSET * i] > 0)
                        {
                            listOfDials[i].IsInteractive = false;
                            if (!nbcMode[i])
                            {
                                listOfDarkAnimations[i].StartAsync();
                                nbcMode[i] = true;
                            }

                            // EnableValve.IsOn = false;
                            listOfToggles[i].IsEnabled = false;


                        }
                        else
                        {
                            if (nbcMode[i])
                            {
                                listOfDarkAnimations[i].Stop();
                            }
                            nbcMode[i] = false;
                            Update_Dials(i);


                        }
                        listOfDials[i].IsEnabled = !Update_Notifications(i);

                    }
                }
                else
                {
                    Enable_Disable_Controls(false,i);
                    listOfDarkAnimations[i].StartAsync();

                }

               
            }
            is_updating_view = false;

           
        }
        private bool Update_Notifications(int id)
        {
            if (received[18 + DATA_OFFSET * id] > 0)
            {
                listOfDials[id].Opacity =  0.2;
                invalid_position = true;
                rootPage.NotifyUser("Invalid position at Valve: "+(1+id).ToString(), NotifyType.ErrorMessage);

            }
            else
            {
                invalid_position = false;
            }
            return invalid_position;
        }
        private void Update_Toogles(int id)
        {
            listOfToggles[id].IsEnabled = false;
            if (received[23 + DATA_OFFSET * id] > 0)
            {
                if (listOfToggles[id].IsOn)
                {

                }
                else
                {
                    listOfToggles[id].IsOn = true;
                }
            }else
            {
                if (listOfToggles[id].IsOn)
                {
                    listOfToggles[id].IsOn = false;
                }
                else
                {
                    
                }
            }
            listOfToggles[id].IsEnabled = true;

        }
        private void Update_Dials( int id)
        {
            listOfDials[id].IsEnabled = false;
            listOfDials[id].IsInteractive = false;
            if (received[23 + DATA_OFFSET * id] > 0)
            {
                listOfDials[id].Opacity = received[22+DATA_OFFSET * id] > 0 ? 0.6 : 1;
           
            }
            else
            {
                listOfDials[id].Opacity = 0.4;    

            }
            Update_Toogles(id);
            listOfDials[id].IsEnabled = true;
            listOfDials[id].IsInteractive = true;
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
