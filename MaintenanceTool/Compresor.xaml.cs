using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
using Windows.UI.Xaml.Navigation;

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



    public sealed partial class Compresor : Page,IDisposable
    {
        private MainPage rootPage = MainPage.Current;
        private static Compresor handler;
        private const int NUMBER_OF_FAULTS = 5;
        private const int NUMBER_OF_RELAYS = 3;
        private const int NUMBER_OF_SWITCHES = 2;
        // Track Read Operation
        private CancellationTokenSource ReadCancellationTokenSource;
        private Object ReadCancelLock = new Object();

        private Boolean IsReadTaskPending;
        private uint ReadBytesCounter = 0;
        DataReader DataReaderObject = null;
        private Byte relays_status;
        private ObservableCollection<ToggleSwitch> listOfToggles;
        public ObservableCollection<TextBlock> listOfTextBlocks;
        private ObservableCollection<Border> listOfBorders;
        private ObservableCollection<Slider> listOfSliders;
        public ObservableCollection<Windows.UI.Xaml.Shapes.Ellipse> listOfEllipses, listOfOvertemperatures;
        public ObservableCollection<AnimationSet> listOfDarkAnimations, listOfLightAnimations;
        private CancellationTokenSource WriteCancellationTokenSource;
        private Object WriteCancelLock = new Object();
        private Boolean IsWriteTaskPending, request_sucess, read_request_success;
        private static Byte[] floatArray = new Byte[4];
        DataWriter DataWriteObject = null;
        private Boolean IsNavigatedAway, updating_toogles_view,toogle_pressed ;
        private static Boolean manipulating;
        private static Byte[] received, toSend,byte_message;
        private static UInt16 speedValue, lastSpeedValue;
        private static Byte relaysCommand, lastCommand;
        private uint readBufferLength;
        private Task blink;
        private CompressorCompleteMessage message;
        private static AnimationSet Fault_Dark, Fault_Light;
        private float[] temperatureValues = new float[2];
        private float lastTemperatureValue, currentTemperatureValue;
        private UInt16 error_flags;
        private Int16 IQFilter, IDFilter;
        private static bool timer_disposed=false;


        private static System.Timers.Timer aTimer = null;

    

        private int sizeofStruct;
        private UInt32 magicHeader = 0;
        private bool updatingStatus = false;
        private const int UPDATING_TIME = 1000;
        private bool required_status = false;
        private UInt16[] compressor_intern_temperatures;
        public Compresor()
        {
            this.InitializeComponent();
            handler = this;
            readBufferLength = 64;
            received = new Byte[readBufferLength];
            compressor_intern_temperatures = new UInt16[3];
            sizeofStruct = Marshal.SizeOf(typeof(SingleTaskMessage));
            byte_message= new Byte[Marshal.SizeOf(typeof(CompressorCompleteMessage))];
            listOfToggles = new ObservableCollection<ToggleSwitch>();
            listOfToggles.Add(EnableToogle1);
            listOfToggles.Add(EnableToogle2);
            listOfToggles.Add(EnableToogle3);
            listOfBorders = new ObservableCollection<Border>();
            listOfBorders.Add(StatusBorder1);
            listOfBorders.Add(StatusBorder2);
            listOfBorders.Add(StatusBorder3);
            listOfTextBlocks = new ObservableCollection<TextBlock>();
            listOfTextBlocks.Add(StatusText1);
            listOfTextBlocks.Add(StatusText2);
            listOfTextBlocks.Add(StatusText3);
            listOfTextBlocks.Add(StatusLabel4);
            listOfTextBlocks.Add(ErrorFlagsLabel);
            listOfTextBlocks.Add(MotorTempText);
            listOfTextBlocks.Add(CoolantTempText);
            listOfTextBlocks.Add(PressureHighText);
            listOfTextBlocks.Add(PressureLowText);
            listOfTextBlocks.Add(IQCurrentText);
            listOfTextBlocks.Add(IDCurrentText);
            listOfTextBlocks.Add(MotorText);
            listOfTextBlocks.Add(ControllerText);
            listOfTextBlocks.Add(CPUText);
            listOfEllipses = new ObservableCollection<Ellipse>();
            listOfEllipses.Add(FaultSignal1);
            listOfEllipses.Add(FaultSignal2);
            listOfEllipses.Add(FaultSignal3);
            listOfEllipses.Add(ExternFaultSignal);
            listOfEllipses.Add(ErrorFlagsFaultSignal);
            listOfEllipses.Add(PressureHighSignal);
            listOfEllipses.Add(PressureLowSignal);
            listOfSliders = new ObservableCollection<Slider>();
            listOfSliders.Add(MotorTemperature);
            listOfSliders.Add(Coolant);
            listOfSliders.Add(MotorSlider);
            listOfSliders.Add(ControllerSlider);
            listOfSliders.Add(CPUSlider);
            listOfDarkAnimations = new ObservableCollection<AnimationSet>();
            listOfLightAnimations = new ObservableCollection<AnimationSet>();
            for (int i = 0; i < (NUMBER_OF_FAULTS+NUMBER_OF_SWITCHES); i++)
            {
                listOfDarkAnimations.Add(listOfEllipses[i].Fade(value: 0.15f, duration: 1000, delay: 25, easingType: EasingType.Sine));
                listOfLightAnimations.Add(listOfEllipses[i].Fade(value: 0.95f, duration: 1000, delay: 25, easingType: EasingType.Sine));
            }
            listOfDarkAnimations[listOfDarkAnimations.Count - 1].Completed += Fault_DarkAnimation_Completed; 
            listOfLightAnimations[listOfLightAnimations.Count - 1].Completed += FaultLigthAnimation_Completed; 

            foreach (AnimationSet an in listOfDarkAnimations)
            {
                an.StartAsync();
            }
            foreach  (ToggleSwitch ts in listOfToggles)
            {
                ts.ManipulationStarted += EnableToggle_ManipulationStarted;
                ts.ManipulationCompleted += EnableToggle_ManipulationCompleted;
            }
            Speed.ManipulationStarted += EnableToggle_ManipulationStarted;
            Speed.ManipulationCompleted += EnableToggle_ManipulationCompleted;
            Fault_Dark = Speed.Fade(value: 0.15f, duration: 1000, delay: 25, easingType: EasingType.Sine);
            Fault_Light = Speed.Fade(value: 0.95f, duration: 1000, delay: 25, easingType: EasingType.Sine);

        }

        private void EnableToggle_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            manipulating = false;
            // throw new NotImplementedException();
        }

        private void EnableToggle_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            manipulating = true;
            // throw new NotImplementedException();
        }

        private void FaultLigthAnimation_Completed(object sender, AnimationSetCompletedEventArgs e)
        {
            foreach (AnimationSet aS in listOfDarkAnimations)
            {
                aS.StartAsync();
            }
            // throw new NotImplementedException();
        }

        private void Fault_DarkAnimation_Completed(object sender, AnimationSetCompletedEventArgs e)
        {
            foreach (AnimationSet aS in listOfLightAnimations)
            {
                aS.StartAsync();
            }
            //  throw new NotImplementedException();
        }
        private void EnableToggle_Toggled(object sender, RoutedEventArgs e)
        {

        }
        private static async void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {

            if (handler.IsPerformingWrite() | manipulating)
            {

            }
            else
            {
                await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
         new DispatchedHandler(() => {
             handler.UpdateDataRelayStatus();
         }));
            }


            if (!timer_disposed)
            {
                aTimer.Start();
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
                timer_disposed = false;
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
                timer_disposed = true;
            }
            CancelAllIoTasks();
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
        private async Task Handle_Toogles(int i)
        {
            if (!updating_toogles_view)
            {
                lastCommand = relaysCommand;
            }

            if (listOfToggles[i].IsOn)
            {

                relaysCommand = (Byte)(lastCommand | (0x01 << i));
            }
            else
            {
                relaysCommand = (Byte)(lastCommand & (~(0x01 << i)));
            }
            if (!updatingStatus)
            {
                if (relaysCommand != lastCommand)
                {
                    if (!updating_toogles_view)
                    {
                        toogle_pressed = true;
                        await WriteAsyncCompressor();
                        toogle_pressed = false;
                    }

                }
            }
        }
        private async Task WriteAsyncCompressor()
        {

            if (IsPerformingRead())
            {
                CancelReadTask();
            }
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    rootPage.NotifyUser("Setting Compressor...", NotifyType.StatusMessage);
                    byte_message[0] = relaysCommand;
                    byte_message[1] =(byte) (speedValue>>8);
                    byte_message[2] = (byte)(speedValue&0x00ff);
                    toSend = new byte[64];
                    Protocol.Message.CreateCompressorMessage(byte_message).CopyTo(toSend, 0);
                        //lastCommand = relaysCommand;
                   
             
                    IsWriteTaskPending = true;
                    DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                    await WriteCompressorAsync(WriteCancellationTokenSource.Token);



                }
                catch (OperationCanceledException /*exception*/)
                {
                    NotifyWriteTaskCanceled();
                }
                catch (Exception exception)
                {
                    rootPage.NotifyUser(exception.Message.ToString(), NotifyType.ErrorMessage);
                 //   Debug.WriteLine(exception.Message.ToString());
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
        private async Task WriteCompressorAsync(CancellationToken cancellationToken)
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
            rootPage.NotifyUser("Write completed - ", NotifyType.StatusMessage);
       

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
                await ReadCompressorData();
                if (read_request_success)
                {
                    if (magicHeader.Equals(Commands.reverseMagic))
                    {
                        await UpdateViewCompressorStatus();
                    }
                }
            }


            updatingStatus = false;
        }
        private async Task RequestRelayStatus()
        {
            if (IsPerformingWrite())
            {
                rootPage.NotifyUser("already requesting to compresor...", NotifyType.WarningMessage);
            }
            else
            {

                if (IsPerformingRead())
                {
                    CancelReadTask();
                    rootPage.NotifyUser("Read task cancelled...", NotifyType.WarningMessage);
                    required_status = (relaysCommand > 0);
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
                        rootPage.NotifyUser("Sending request to Compressor...", NotifyType.StatusMessage);

                        // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                        // update the button states until after the write completes.
                        toSend = new byte[sizeofStruct];
                        Protocol.Message.CreateCompressorStatusRequestMessage().CopyTo(toSend, 0);
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
                       // Debug.WriteLine(exception.Message.ToString());
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
            rootPage.NotifyUser("Request Compresor Completed -  ", NotifyType.StatusMessage);


        }

        private async void EnableToogle1_Toggled(object sender, RoutedEventArgs e)
        {
           await Handle_Toogles(0);
        }

        private async void EnableToogle2_Toggled(object sender, RoutedEventArgs e)
        {
          await  Handle_Toogles(1);
        }

        private async void EnableToogle3_Toggled(object sender, RoutedEventArgs e)
        {
          await  Handle_Toogles(2);
        }

        private async void Speed_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (IsPerformingWrite())
            {

            }
            else
            {
                speedValue = (UInt16)(Speed.Value );
                if (speedValue != lastSpeedValue)
                {
                    await WriteAsyncCompressor();
                }
            }
            manipulating = false;
        }

        private void Speed_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            manipulating = true;
        }

        private async Task ReadCompressorData()
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
                    required_status = relaysCommand>0;
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
                        rootPage.NotifyUser("Reading Compressor Status...", NotifyType.StatusMessage);

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
                     //   Debug.WriteLine(exception.Message.ToString());
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
            rootPage.NotifyUser("Read Compresor Status completed ", NotifyType.StatusMessage);
        }
        private async Task UpdateViewCompressorStatus()
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
                   await Update_Compressor_View();
               }));

            }


        }
        private async Task Update_Compressor_View()
        {
            UpdateToggles();
            await UpdateFaultStatusSignal();
            UpdateComporessorSpeed();
            UpdateTemperatures();
            UpdateFilterCurrents();

        }
        private void UpdateFilterCurrents()
        {
            
            IQFilter =(Int16)( received[12] + received[13] * 256);
            IQCurrentText.Text = ((float)(IQFilter/10)).ToString("F1");
            IDFilter = (Int16)(received[14] + received[15] * 256);
            IDCurrentText.Text = ((float)(IDFilter/10)).ToString("F1");


        }

        private void UpdateTemperatures()
        {
            int k;
            for (int j = 0; j < 2; j++)
            {
                System.Buffer.BlockCopy(received, 22 + ((4 * j) ), floatArray, 0, 4);
                lastTemperatureValue = temperatureValues[j];
                currentTemperatureValue = BitConverter.ToSingle(floatArray, 0);
                if (lastTemperatureValue != currentTemperatureValue)
                {
                    temperatureValues[j] = currentTemperatureValue;
                    listOfSliders[j].Value = currentTemperatureValue;
                    UpdateValueTemperatureText(5+j);
                }

            }
            for (int j = 0; j < 3; j++)
            {
                compressor_intern_temperatures[j] = (UInt16)(received[16 + 2*j] + received[17 + 2*j] * 256);
                
                currentTemperatureValue = ((float)(compressor_intern_temperatures[j]))/10;
                listOfSliders[j + 2].Value = currentTemperatureValue;
                UpdateValueTemperatureText(11+j);
                

            }
        }
        private void UpdateValueTemperatureText( int sensor)
        {
            int index = (0 + sensor);
            listOfTextBlocks[index].Text = currentTemperatureValue.ToString("F1");
            if ((currentTemperatureValue < -20) | (currentTemperatureValue > 200))
            {
                listOfTextBlocks[index].Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
            }
            else
            {
                if (currentTemperatureValue > 0)
                {
                    listOfTextBlocks[index].Foreground = new SolidColorBrush(Windows.UI.Colors.Green);
                }
                else
                {
                    listOfTextBlocks[index].Foreground = new SolidColorBrush(Windows.UI.Colors.Blue);
                }
            }
        }

        private void UpdateToggles()
        {
            updating_toogles_view = true;
            for (int i = 0; i < NUMBER_OF_RELAYS; i++)
            {
                if ((received[6] & (0x01<<i)) > 0)
                {
                    listOfToggles[i].IsOn = true;
                    if (i==0)
                    {
                        Speed.Opacity = 1;
                    }
                   
                }
                else
                {
                    listOfToggles[i].IsOn = false;
                    if (i==0)
                    {
                        Speed.Opacity = 0.4;
                    }
                    
                    
                }
            }

          


            updating_toogles_view = false;

        }

        private void UpdateComporessorSpeed()
        {
           
            if (magicHeader.Equals(Commands.reverseMagic))
            {
              
                    Speed.Value = (received[8]*256+ received[7]) ;
                    lastSpeedValue = (UInt16)(received[8] * 256 + received[7]);
                    

            }

            





            
        }
        private async Task UpdateFaultStatusSignal()
        {
            int k;
            for (int i = 0; i < NUMBER_OF_RELAYS; i++)
            {
                if ((received[6] & (Byte)((0x20<<i))) == 0)
                {
                    listOfEllipses[i].Visibility = Visibility.Visible;
                    listOfBorders[i].Visibility = Visibility.Collapsed;
                    UpdateRelayStatusText(i, "Fault");
                    if (blink != null)
                    {
                        await blink;
                    }
                }
                else
                {
                    if (listOfToggles[i].IsOn)
                    {
                        UpdateRelayStatusText(i, "Enabled");
                        listOfBorders[i].Visibility = Visibility.Visible;
                        listOfEllipses[0].Visibility = Visibility.Collapsed;
                       
                
                        
                    }
                    else
                    {
                        listOfEllipses[i].Visibility = Visibility.Collapsed;
                        UpdateRelayStatusText(i, "Disabled");
                        listOfBorders[i].Visibility = Visibility.Collapsed;
                    }


                }
            }
            for (int i = 0; i < NUMBER_OF_SWITCHES; i++)
            {
                k = 5 + i;
                if ((received[9] & (Byte)((0x08 >> i))) >0)
                {
                    listOfEllipses[k].Visibility = Visibility.Visible;
                   
                    UpdateRelayStatusText(k+2, "Fault");
                    if (blink != null)
                    {
                        await blink;
                    }
                }
                else
                {

                    listOfEllipses[k].Visibility = Visibility.Collapsed;
                    UpdateRelayStatusText(k+2, "OK");

                }
            }
            if (listOfToggles[0].IsOn)
            {
                if ((received[6] & (Byte)((0x10))) == 0)
                {
                    listOfEllipses[3].Visibility = Visibility.Visible;
                    UpdateRelayStatusText(3, "Fault");
                }
                else
                {
                    listOfEllipses[3].Visibility = Visibility.Collapsed;
                    UpdateRelayStatusText(3, "OK");
                }
            }
            else
            {
                listOfEllipses[3].Visibility = Visibility.Collapsed;
                UpdateRelayStatusText(3, "   ");
            }
            if ((received[10]| received[11]) > 0)
            {
                listOfEllipses[4].Visibility = Visibility.Visible;
                error_flags = (UInt16)(received[10] * 256 + received[11]);
                UpdateRelayStatusText(4, string.Concat("0x", error_flags.ToString("X4")));
            }
            else
            {
                listOfEllipses[4].Visibility = Visibility.Collapsed;
                UpdateRelayStatusText(4, "OK");
            }






        }
        private void UpdateRelayStatusText(int i, string s)
        {
            listOfTextBlocks[i].Text = s;

        }

    }
}
