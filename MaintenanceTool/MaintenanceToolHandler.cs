using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage.Streams;
using System.Diagnostics;
using System.Threading;

using MaintenanceToolProtocol;

using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Runtime.InteropServices;

namespace MaintenanceToolECSBOX
{
    class MaintenanceToolHandler
    {

        private MainPage rootPage = MainPage.Current;
        private static MaintenanceToolHandler handler;
        /// <summary>
        /// Used to synchronize threads to avoid multiple instantiations of eventHandlerForDevice.
        /// </summary>
        private static Object singletonCreationLock = new Object();
        // Track Read Operation
        private CancellationTokenSource ReadCancellationTokenSource;
        private Object ReadCancelLock = new Object();

        private Boolean IsReadTaskPending;
        private uint ReadBytesCounter = 0;
        DataReader DataReaderObject = null;
        private string readText;

        // Track Write Operation
        private CancellationTokenSource WriteCancellationTokenSource;
        private Object WriteCancelLock = new Object();

        private Boolean IsWriteTaskPending;
        private uint WriteBytesCounter = 0;
        DataWriter DataWriteObject = null;

        bool WriteBytesAvailable = false;
        private static Byte[] received, toSend;
        private Boolean IsNavigatedAway=false;

        public MaintenanceToolHandler() {
            ResetReadCancellationTokenSource();
            ResetWriteCancellationTokenSource();
            handler = this;
        }

        /// <summary>
        /// Enforces the singleton pattern so that there is only one object handling app events
        /// as it relates to the SerialDevice because this sample app only supports communicating with one device at a time. 
        ///
        /// An instance of EventHandlerForDevice is globally available because the device needs to persist across scenario pages.
        ///
        /// If there is no instance of EventHandlerForDevice created before this property is called,
        /// an EventHandlerForDevice will be created.
        /// </summary>
        public static MaintenanceToolHandler Current
        {
            get
            {
                if (handler == null)
                {
                    lock (singletonCreationLock)
                    {
                        if (handler == null)
                        {
                            CreateNewEventHandlerForDevice();
                        }
                    }
                }

                return handler;
            }
        }

        /// <summary>
        /// Creates a new instance of EventHandlerForDevice, enables auto reconnect, and uses it as the Current instance.
        /// </summary>
        public static void CreateNewEventHandlerForDevice()
        {
            handler = new MaintenanceToolHandler();
        }
        public async void SendAliveMessage()
        {
            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                  //  rootPage.NotifyUser("Writing...", NotifyType.StatusMessage);

                    // We need to set this to true so that the buttons can be updated to disable the write button. We will not be able to
                    // update the button states until after the write completes.
                    IsWriteTaskPending = true;
                    DataWriteObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
               

                    await SendAliveSymbol(WriteCancellationTokenSource.Token);
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

                
                }
            }
            else
            {
                Utilities.NotifyDeviceNotConnected();
            }
        }
        private async Task SendAliveSymbol(CancellationToken cancellationToken)
        {

            Task<UInt32> storeAsyncTask;

            var n = Marshal.SizeOf(typeof(SingleTaskCommand));
            toSend = new byte[n];
            toSend.Initialize();

            Maintenance_Tool.Current.BuilAliveMessage().CopyTo(toSend, 0);
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
            rootPage.NotifyUser("AliveSignal - " + bytesWritten.ToString() + " bytes written", NotifyType.StatusMessage);

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
        private async void NotifyWriteCancelingTask()
        {
            // Setting the dispatcher priority to high allows the UI to handle disabling of all the buttons
            // before any of the IO completion callbacks get a chance to modify the UI; that way this method
            // will never get the opportunity to overwrite UI changes made by IO callbacks
            await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                new DispatchedHandler(() =>
                {
                  //  WriteButton.IsEnabled = false;
                 //   WriteCancelButton.IsEnabled = false;

                    if (!IsNavigatedAway)
                   {
                        rootPage.NotifyUser("Canceling Write... Please wait...", NotifyType.StatusMessage);
                  }
                }));
        }
        /// <summary>
        /// Print a status message saying we are canceling a task and disable all buttons to prevent multiple cancel requests.
        /// <summary>
        private async void NotifyReadCancelingTask()
        {
            // Setting the dispatcher priority to high allows the UI to handle disabling of all the buttons
            // before any of the IO completion callbacks get a chance to modify the UI; that way this method
            // will never get the opportunity to overwrite UI changes made by IO callbacks
            await rootPage.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                new DispatchedHandler(() =>
                {
                  //  ReadButton.IsEnabled = false;
                 //   ReadCancelButton.IsEnabled = false;

                   if (!IsNavigatedAway)
                   {
                       rootPage.NotifyUser("Canceling Read... Please wait...", NotifyType.StatusMessage);
                   }
                }));
        }

        /// <summary>
        /// Notifies the UI that the operation has been cancelled
        /// </summary>
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
    }
   
}
