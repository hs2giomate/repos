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
        private  const int number_flappers= 2;
        private const double CALLING_INTERVAL=2000;

        // Track Read Operation
        private CancellationTokenSource ReadCancellationTokenSource, WriteCancellationTokenSource;
        private Object ReadCancelLock = new Object();

        private Boolean IsReadTaskPending;
        private Boolean[] isToggleON = new Boolean[3];
        private uint ReadBytesCounter = 0;
        DataReader DataReaderObject = null;
        private Object WriteCancelLock = new Object();
        private Boolean IsWriteTaskPending;
        private Boolean[] manipulating;
        DataWriter DataWriteObject = null;
        private Boolean IsNavigatedAway;
        private static Byte[] received, toSend;
        private static Byte[] currentPosition, lastPosition, lastCommand, commandValve, lastSetpoint, currentSetpoint;
        private SingleTaskMessage fansCommand;
        private static System.Timers.Timer refreshValueTimer = null;
        private int sizeofStruct;
        private uint readBufferLength;
        private UInt32 magicHeader;
        private static AnimationSet NBC_Mode_Dark,NBC_Mode_Light;
        private bool[] nbcMode;
        private MinimunFreshAir minimunValues;
        private Byte minimunAirPosition,lastMinimum,last_read_minimum;
        private const Byte minimunFailValue = 20;
        private ObservableCollection<Microsoft.Toolkit.Uwp.UI.Controls.RadialGauge> listOfDials;
        private ObservableCollection<ToggleSwitch> listOfToggles;
        private ObservableCollection<AnimationSet> listOfDarkAnimations, listOfLightAnimations;
        private ObservableCollection<TextBlock> listOfTextBlocks;
        private ObservableCollection<Border> listOfBorders;
        private bool required_feedback;
        private const int DATA_OFFSET=14;
        public FlapperValveControl()
        {
            this.InitializeComponent();
            handler = this;
            sizeofStruct = 64;
            toSend = new byte[sizeofStruct];
            nbcMode = new bool[number_flappers];
            manipulating = new Boolean[number_flappers];
            currentPosition = new Byte[number_flappers];
            lastPosition = new Byte[number_flappers];
            lastCommand = new Byte[number_flappers];
            commandValve = new Byte[number_flappers];
            lastSetpoint = new Byte[number_flappers];
            currentSetpoint= new Byte[number_flappers];
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
            listOfToggles[0].ManipulationStarted += EnableValve_ManipulationStarted;
            listOfToggles[0].ManipulationCompleted += EnableValve_ManipulationCompleted;
            listOfToggles[1].ManipulationStarted += FlapperValveControl_ManipulationStarted1;
            listOfToggles[1].ManipulationCompleted += FlapperValveControl_ManipulationCompleted;
           
            listOfDarkAnimations = new ObservableCollection<AnimationSet>();
            listOfLightAnimations = new ObservableCollection<AnimationSet>();
            for (int i = 0; i < number_flappers; i++)
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
            listOfDarkAnimations[1].Completed += FlapperValveControl_Completed;
            listOfLightAnimations[1].Completed += FlapperValveControl_Completed1;
            minimunValues = new MinimunFreshAir();
            minimunAirPosition = minimunFailValue;
    
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
                CancelReadTask();
                // while (IsPerformingRead()) ;
            }
            // throw new NotImplementedException();
        }

        private void FlapperValveControl_Completed1(object sender, AnimationSetCompletedEventArgs e)
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

        private void FlapperValveControl_Completed(object sender, AnimationSetCompletedEventArgs e)
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
                CancelReadTask();
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
                CancelReadTask();
                // while (IsPerformingRead()) ;
            }
            // throw new NotImplementedException();
        }

        private void Position_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            manipulating[0] = true;
            if (IsPerformingRead())
            {
                CancelReadTask();
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
                UpdateDataPosition();
                
               
           
              
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
        private async Task UpdateMinimunValues()
        {
            try
            {
               // await minimunValues.GetminimunValidAirPosition();
                last_read_minimum = minimunAirPosition;
               // minimunAirPosition = (byte)(minimunValues.minimunValid);
                minimunAirPosition = 16;

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                minimunAirPosition = last_read_minimum;
                //  throw;
            }

            
           int j ;
           for (int i = 0; i < number_flappers; i++)
            {
                j = i + 2;
                if (minimunAirPosition < minimunFailValue)
                {
                    listOfDials[j].MinAngle = 90 - minimunFailValue * 90 / 255;
                    listOfDials[j].Minimum = 90 - minimunFailValue * 90 / 255;

                }
                else
                {
                    if (minimunAirPosition != lastMinimum)
                    {
                        lastMinimum = minimunAirPosition;
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
        }
        public void StartStatusCheckTimer()
        {
            // Create a timer and set a two second interval.
            refreshValueTimer = new System.Timers.Timer();
            refreshValueTimer.Interval = CALLING_INTERVAL;

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
             new DispatchedHandler(() => {
                 handler.UpdateDataPosition(); }));
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


     
             

        private async void position_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            await WriteSetpointValue(0);
            manipulating[0] = false;
        }

        private async Task WriteSetpointValue(int valve_id)
        {
            if ((commandValve[valve_id] & 0x01)>0)
            {
                lastSetpoint[valve_id] = currentSetpoint[valve_id];
                currentSetpoint[valve_id] = (byte)(listOfDials[valve_id].Value * 255 / 90);
               // position.Value = lastPosition;
                if (currentPosition[valve_id] != currentSetpoint[valve_id])
                {
                    listOfDials[valve_id].IsInteractive = false;
                    await WriteValveData(valve_id);
               //     await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                //       new DispatchedHandler(() => { handler.UpdateDataPosition(); }));
                    listOfDials[valve_id].IsInteractive = true;
                }
            }
            

        }
        private async Task WriteValveData(int valve_id)
        {

            if (IsPerformingRead())
            {
                CancelReadTask();
                rootPage.NotifyUser("Cancelling reading task...", NotifyType.StatusMessage);
            }
          
            if (IsPerformingWrite())
            {
                //  CancelWriteTask();
                rootPage.NotifyUser("cancelling already writing...", NotifyType.StatusMessage);
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
                    toSend.Initialize();
                    //   UpdateWriteButtonStates();
                    if (commandValve[valve_id] != lastCommand[valve_id])
                    {
                        Protocol.Message.CreateCommandFlapperValveMessage(commandValve).CopyTo(toSend, 0);
                        lastCommand[valve_id] = commandValve[valve_id];
                    }
                    else if (listOfToggles[valve_id].IsOn)
                    {
                        if (currentSetpoint[valve_id] != currentPosition[valve_id])
                        {
                            Protocol.Message.CreateSetpointtFlapperValveMessage(currentSetpoint).CopyTo(toSend, 0);
                            currentSetpoint[valve_id] = lastSetpoint[valve_id];
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
                    try
                    {
                        DataWriteObject.DetachStream();
                    }
                    catch (Exception)
                    {
                        rootPage.NotifyUser("DataWriter detach failed...", NotifyType.StatusMessage);
                        //  throw;
                    }
                   
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
          //  for (int i = 0; i < number_flappers; i++)
          //  {
                if (!(manipulating[0]|(manipulating[1])))
                {
             
                    await RequestValveData();
                    await ReadValveData();
                    //  EnableValve.IsEnabled = true;
         

                    await UpdateMinimunValues();
                    GetLastPosition();
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
            if (IsPerformingRead())
            {
                CancelReadTask();
                // while (IsPerformingRead()) ;
            }
            else
            {
                if (IsPerformingWrite())
                {
                   // CancelWriteTask();
                }
                else
                {
                    if (EventHandlerForDevice.Current.IsDeviceConnected)
                    {
                        try
                        {
                            rootPage.NotifyUser("Reading Position...", NotifyType.StatusMessage);

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
                            catch (Exception)
                            {
                                rootPage.NotifyUser("DataWriter detach failed...", NotifyType.StatusMessage);
                                //  throw;
                            }
                            DataWriteObject = null;

                            // UpdateWriteButtonStates();
                        }
                    }
                    else
                    {
                        Utilities.NotifyDeviceNotConnected();
                    }
                }
                
            }
           
        }

        private async void  position2_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            await WriteSetpointValue(1);
            manipulating[1] = false;
        }

        private async void EnableValve1_Toggled(object sender, RoutedEventArgs e)
        {
            lastCommand[1] = commandValve[1];
            if (EnableValve.IsOn)
            {

                commandValve[1] = (Byte)(lastCommand[1] | 0x01);

            }
            else
            {

                commandValve[1] = (Byte)(lastCommand[1] & 0xfe);

            }
            if (commandValve[1] != lastCommand[1])
            {
                //  EnableValve.IsEnabled = false;
                await WriteValveData(1);
                //    await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                //     new DispatchedHandler(() => { handler.UpdateDataPosition(); }));
                //  EnableValve.IsEnabled = true;

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
                        rootPage.NotifyUser("past read thread cancelled...", NotifyType.StatusMessage);
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
                            rootPage.NotifyUser("failed to Detach...", NotifyType.StatusMessage);
                            //  throw;
                        }
                       
                        DataReaderObject = null;
                        rootPage.NotifyUser("Valve Data got read...", NotifyType.StatusMessage);
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

        private async void EnableValve_Toggled(object sender, RoutedEventArgs e)
        {
            lastCommand[0] = commandValve[0];
            if (EnableValve.IsOn)
            {
                
                commandValve[0] = (Byte)(lastCommand[0] | 0x01);
              
            }
            else
            {
               
                commandValve[0] = (Byte)(lastCommand[0] & 0xfe);
              
            }
            if (commandValve[0] != lastCommand[0])
            {
              //  EnableValve.IsEnabled = false;
                await WriteValveData(0);
            //    await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
             //     new DispatchedHandler(() => { handler.UpdateDataPosition(); }));
              //  EnableValve.IsEnabled = true;

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


            }
           
        }
        private void GetLastPosition()
        {
            int k;
            for (int i = 0; i < number_flappers; i++)
            {
                if (magicHeader.Equals(Commands.reverseMagic))
                {
                    lastPosition[i] = currentPosition[i];
                    currentPosition[i] = received[20+DATA_OFFSET*i];
                    listOfDials[i].Value = currentPosition[i] * 90 / 255;
                    listOfTextBlocks[i*3].Text = (90 - currentPosition[i]* 90 / 255).ToString("N0");
                    k = 0;
                    for (int j = 3*i; j < listOfBorders.Count-3*(1-i); j++)
                    {
                        listOfBorders[j].Visibility = received[8+k+(DATA_OFFSET * i)] > 0 ? Visibility.Visible : Visibility.Collapsed;
                        listOfTextBlocks[j+1] .Visibility = received[8 + k +(DATA_OFFSET * i)] < 1 ? Visibility.Visible : Visibility.Collapsed;
                        k++;
                    }
                    if (received[24+ DATA_OFFSET * i] > 0)
                    {
                        listOfDials[i].IsInteractive = false;
                        if (!nbcMode[i])
                        {
                            NBC_Mode_Dark.StartAsync();
                        }
                        nbcMode[i] = true;


                        // EnableValve.IsOn = false;
                        listOfToggles[i].IsEnabled = false;


                    }
                    else
                    {
                        nbcMode[i] = false;
                        if (!listOfToggles[i].IsEnabled)
                        {
                            listOfToggles[i].IsEnabled = true;
                        }
                        if (received[23+ DATA_OFFSET * i] > 0)
                        {
                            listOfDials[i].Opacity = received[22] > 0 ? 0.4 : 1;
                            listOfDials[i].IsInteractive = true;
                            //  position.AllowDrop = true;
                        }
                        else
                        {
                            listOfDials[i].Opacity = 0.4;
                            listOfDials[i].IsInteractive = false;
                            //    position.AllowDrop = false;
                            if (listOfToggles[i].IsOn)
                            {
                                listOfToggles[i].IsOn = false;
                            }

                        }
                    }

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
