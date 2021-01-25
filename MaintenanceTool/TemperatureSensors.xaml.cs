using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Windows.UI.Core;
using Windows.Storage.Streams;
using System.Runtime.InteropServices;
using MaintenanceToolProtocol;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MaintenanceToolECSBOX
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TemperatureSensors : Page,IDisposable
    {
        private static TemperatureSensors handler;
        private MainPage rootPage = MainPage.Current;
        // Track Read Operation
        private CancellationTokenSource ReadCancellationTokenSource, WriteCancellationTokenSource;
        private Object ReadCancelLock = new Object();

        private Boolean IsReadTaskPending,request_succes, read_request_succes;
        private Boolean[] isToggleON = new Boolean[3];
        private uint ReadBytesCounter = 0;
        DataReader DataReaderObject = null;
        private Object WriteCancelLock = new Object();
        private Boolean IsWriteTaskPending;
        DataWriter DataWriteObject = null;
        private Boolean IsNavigatedAway;
        private static Byte[] floatArray = new Byte[4];
        private static Byte[] received, toSend;
        private static Byte fanPWMValue, lasPWMValue, lastFansEnabled, fanEnabled;
        private SingleTaskMessage readTemperatureCommand;
        private static System.Timers.Timer refreshValueTimer = null;
        private int sizeofStruct;
        private UInt32 magicHeader;
        private float[,] temperatureValues = new float[3,4];
        float lastValue,currentValue;
        private uint readBufferLength;

        public ObservableCollection<TextBlock> listOfTextBlocks;

        public TemperatureSensors()
        {
            this.InitializeComponent();
            handler = this;
            sizeofStruct = Marshal.SizeOf(readTemperatureCommand);
            toSend = new byte[sizeofStruct];
            readBufferLength = 64;
            received = new byte[readBufferLength];
            listOfTextBlocks = new ObservableCollection<TextBlock>();
            listOfTextBlocks.Add(TemperatureValue1);
            listOfTextBlocks.Add(TemperatureValue2);
            listOfTextBlocks.Add(TemperatureValue3);
            listOfTextBlocks.Add(TemperatureValue4);
            listOfTextBlocks.Add(TemperatureValue5);
            listOfTextBlocks.Add(TemperatureValue6);
            listOfTextBlocks.Add(TemperatureValue7);
            listOfTextBlocks.Add(TemperatureValue8);
            listOfTextBlocks.Add(TemperatureValue9);
            listOfTextBlocks.Add(TemperatureValue10);
            listOfTextBlocks.Add(TemperatureValue11);
            listOfTextBlocks.Add(TemperatureValue12);
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
            refreshValueTimer.Interval = 3000;

            // Hook up the Elapsed event for the timer. 
            refreshValueTimer.Elapsed += OnTimedEvent;

            // Have the timer fire repeated events (true is the default)
            refreshValueTimer.AutoReset = false;

            // Start the timer
            refreshValueTimer.Enabled = true;


        }
        public async void UpdateDataTemperatureValues()
        {
            request_succes= false;
            await RequestTemperatureValues();
            if (request_succes)
            {
                read_request_succes = false;
                await ReadTemperaturesValues();
                if (read_request_succes)
                {
                    FillFloatArray();
                }
            }
          
             


        }
        private Boolean IsPerformingRead()
        {
            return (IsReadTaskPending);
        }
        private async Task RequestTemperatureValues()
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
                    rootPage.NotifyUser("Cancelling reading task...", NotifyType.WarningMessage);
                }
                
                if (EventHandlerForDevice.Current.IsDeviceConnected)
                {
                    try
                    {
                        rootPage.NotifyUser("Reading Temperatures...", NotifyType.StatusMessage);

                        // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                        // update the button states until after the write completes.
                        IsWriteTaskPending = true;
                        DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                        Protocol.Message.CreateTemperatureRequestMessage().CopyTo(toSend, 0);
                        //UpdateWriteButtonStates();

                        await SendrequestTemperaturesAsync(WriteCancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException /*exception*/)
                    {
                        NotifyWriteTaskCanceled();
                    }
                    catch (Exception exception)
                    {
                        MainPage.Current.NotifyUser(exception.Message.ToString(), NotifyType.ErrorMessage);
                        //  Debug.WriteLine(exception.Message.ToString());
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
        private async Task SendrequestTemperaturesAsync(CancellationToken cancellationToken)
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
            if (bytesWritten>0)
            {
                request_succes = true;
            }
            rootPage.NotifyUser("Request Completed  " , NotifyType.StatusMessage);


        }
        private async Task ReadTemperaturesValues()
        {
            if (IsPerformingWrite())
            {
                rootPage.NotifyUser("busy on writting data...", NotifyType.StatusMessage);
            }
            else
            {
                if (IsPerformingRead())
                {

                    rootPage.NotifyUser("on reading...", NotifyType.WarningMessage);
                }
                else
                {
                    if (EventHandlerForDevice.Current.IsDeviceConnected)
                    {
                        try
                        {
                            rootPage.NotifyUser("Reading Temperatures...", NotifyType.StatusMessage);

                            // We need to set this to true so that the buttons can be updated to disable the read button. We will not be able to
                            // update the button states until after the read completes.
                            IsReadTaskPending = true;
                            DataReaderObject = new DataReader(EventHandlerForDevice.Current.Device.InputStream);
                            // UpdateReadButtonStates();

                            await ReaTemperaturesAsync(ReadCancellationTokenSource.Token);
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
        }

        private async Task ReaTemperaturesAsync(CancellationToken cancellationToken)
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
                read_request_succes = true;
                    


            }
            rootPage.NotifyUser("Temperatures values were read", NotifyType.StatusMessage);
        }
        private void FillFloatArray()
        {
            
            if (magicHeader.Equals(Commands.reverseMagic))
            {
        
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            System.Buffer.BlockCopy(received, 7 + 4 * ((4 * i) + (j)), floatArray, 0, 4);
                            lastValue = temperatureValues[i, j];
                            currentValue = BitConverter.ToSingle(floatArray, 0);
                            if (lastValue!=currentValue)
                            {
                                 temperatureValues[i, j]=currentValue;
                                UpdateValueText(i, j);
                            }
                            
                        }

                    }

               
    
            }
        }
        private void UpdateValueText(int module, int sensor)
        {
            int index = ((4 * module) + sensor);
            listOfTextBlocks[index].Text = currentValue.ToString("F1");
            if ((received[6]!=0x01)|(currentValue<-20)|(currentValue>200))
            {
                listOfTextBlocks[index].Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
            }
            else
            {
                if (currentValue>0)
                {
                    listOfTextBlocks[index].Foreground = new SolidColorBrush(Windows.UI.Colors.Green);
                }
                else
                {
                    listOfTextBlocks[index].Foreground = new SolidColorBrush(Windows.UI.Colors.Blue);
                }
            }
        }
        private static async void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            // Debug.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
            //  MaintenanceToolHandler.Current.SendAliveMessage();
            if (handler.IsPerformingWrite())
            {

            }
            else
            {
                await handler.rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                 new DispatchedHandler(() => { handler.UpdateDataTemperatureValues(); }));
            }
        
                
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
        private Boolean IsPerformingWrite()
        {
            return (IsWriteTaskPending);
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
