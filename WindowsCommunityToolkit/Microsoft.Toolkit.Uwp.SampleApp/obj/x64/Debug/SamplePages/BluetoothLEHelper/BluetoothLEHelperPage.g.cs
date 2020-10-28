#pragma warning disable  
﻿#pragma checksum "C:\Users\GMateusDP\source\repos\WindowsCommunityToolkit\Microsoft.Toolkit.Uwp.SampleApp\SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "84F3630123A074275F646AD5705D1603F74455B257ECC203D035ECDE88246A22"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Toolkit.Uwp.SampleApp.SamplePages
{
    partial class BluetoothLEHelperPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private static class XamlBindingSetters
        {
            public static void Set_Windows_UI_Xaml_Controls_ItemsControl_ItemsSource(global::Windows.UI.Xaml.Controls.ItemsControl obj, global::System.Object value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::System.Object) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::System.Object), targetNullValue);
                }
                obj.ItemsSource = value;
            }
            public static void Set_Windows_UI_Xaml_Documents_Run_Text(global::Windows.UI.Xaml.Documents.Run obj, global::System.String value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = targetNullValue;
                }
                obj.Text = value ?? global::System.String.Empty;
            }
            public static void Set_Windows_UI_Xaml_Controls_TextBlock_Text(global::Windows.UI.Xaml.Controls.TextBlock obj, global::System.String value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = targetNullValue;
                }
                obj.Text = value ?? global::System.String.Empty;
            }
        };

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class BluetoothLEHelperPage_obj10_Bindings :
            global::Windows.UI.Xaml.IDataTemplateExtension,
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            IBluetoothLEHelperPage_Bindings
        {
            private global::Microsoft.Toolkit.Uwp.Connectivity.ObservableGattCharacteristics dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);
            private bool removedDataContextHandler = false;

            // Fields for each control that has bindings.
            private global::System.WeakReference obj10;
            private global::Windows.UI.Xaml.Documents.Run obj11;

            public BluetoothLEHelperPage_obj10_Bindings()
            {
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 10: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 62
                        this.obj10 = new global::System.WeakReference((global::Windows.UI.Xaml.Controls.TextBlock)target);
                        break;
                    case 11: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 64
                        this.obj11 = (global::Windows.UI.Xaml.Documents.Run)target;
                        break;
                    default:
                        break;
                }
            }

            public void DataContextChangedHandler(global::Windows.UI.Xaml.FrameworkElement sender, global::Windows.UI.Xaml.DataContextChangedEventArgs args)
            {
                 if (this.SetDataRoot(args.NewValue))
                 {
                    this.Update();
                 }
            }

            // IDataTemplateExtension

            public bool ProcessBinding(uint phase)
            {
                throw new global::System.NotImplementedException();
            }

            public int ProcessBindings(global::Windows.UI.Xaml.Controls.ContainerContentChangingEventArgs args)
            {
                int nextPhase = -1;
                ProcessBindings(args.Item, args.ItemIndex, (int)args.Phase, out nextPhase);
                return nextPhase;
            }

            public void ResetTemplate()
            {
                Recycle();
            }

            // IDataTemplateComponent

            public void ProcessBindings(global::System.Object item, int itemIndex, int phase, out int nextPhase)
            {
                nextPhase = -1;
                switch(phase)
                {
                    case 0:
                        nextPhase = -1;
                        this.SetDataRoot(item);
                        if (!removedDataContextHandler)
                        {
                            removedDataContextHandler = true;
                            (this.obj10.Target as global::Windows.UI.Xaml.Controls.TextBlock).DataContextChanged -= this.DataContextChangedHandler;
                        }
                        this.initialized = true;
                        break;
                }
                this.Update_((global::Microsoft.Toolkit.Uwp.Connectivity.ObservableGattCharacteristics) item, 1 << phase);
            }

            public void Recycle()
            {
            }

            // IBluetoothLEHelperPage_Bindings

            public void Initialize()
            {
                if (!this.initialized)
                {
                    this.Update();
                }
            }
            
            public void Update()
            {
                this.Update_(this.dataRoot, NOT_PHASED);
                this.initialized = true;
            }

            public void StopTracking()
            {
            }

            public void DisconnectUnloadedObject(int connectionId)
            {
                throw new global::System.ArgumentException("No unloadable elements to disconnect.");
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                if (newDataRoot != null)
                {
                    this.dataRoot = (global::Microsoft.Toolkit.Uwp.Connectivity.ObservableGattCharacteristics)newDataRoot;
                    return true;
                }
                return false;
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::Microsoft.Toolkit.Uwp.Connectivity.ObservableGattCharacteristics obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_Name(obj.Name, phase);
                    }
                }
            }
            private void Update_Name(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 64
                    XamlBindingSetters.Set_Windows_UI_Xaml_Documents_Run_Text(this.obj11, obj, null);
                }
            }
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class BluetoothLEHelperPage_obj13_Bindings :
            global::Windows.UI.Xaml.IDataTemplateExtension,
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            IBluetoothLEHelperPage_Bindings
        {
            private global::Microsoft.Toolkit.Uwp.Connectivity.ObservableGattDeviceService dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);
            private bool removedDataContextHandler = false;

            // Fields for each control that has bindings.
            private global::System.WeakReference obj13;
            private global::Windows.UI.Xaml.Documents.Run obj14;

            public BluetoothLEHelperPage_obj13_Bindings()
            {
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 13: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 47
                        this.obj13 = new global::System.WeakReference((global::Windows.UI.Xaml.Controls.TextBlock)target);
                        break;
                    case 14: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 49
                        this.obj14 = (global::Windows.UI.Xaml.Documents.Run)target;
                        break;
                    default:
                        break;
                }
            }

            public void DataContextChangedHandler(global::Windows.UI.Xaml.FrameworkElement sender, global::Windows.UI.Xaml.DataContextChangedEventArgs args)
            {
                 if (this.SetDataRoot(args.NewValue))
                 {
                    this.Update();
                 }
            }

            // IDataTemplateExtension

            public bool ProcessBinding(uint phase)
            {
                throw new global::System.NotImplementedException();
            }

            public int ProcessBindings(global::Windows.UI.Xaml.Controls.ContainerContentChangingEventArgs args)
            {
                int nextPhase = -1;
                ProcessBindings(args.Item, args.ItemIndex, (int)args.Phase, out nextPhase);
                return nextPhase;
            }

            public void ResetTemplate()
            {
                Recycle();
            }

            // IDataTemplateComponent

            public void ProcessBindings(global::System.Object item, int itemIndex, int phase, out int nextPhase)
            {
                nextPhase = -1;
                switch(phase)
                {
                    case 0:
                        nextPhase = -1;
                        this.SetDataRoot(item);
                        if (!removedDataContextHandler)
                        {
                            removedDataContextHandler = true;
                            (this.obj13.Target as global::Windows.UI.Xaml.Controls.TextBlock).DataContextChanged -= this.DataContextChangedHandler;
                        }
                        this.initialized = true;
                        break;
                }
                this.Update_((global::Microsoft.Toolkit.Uwp.Connectivity.ObservableGattDeviceService) item, 1 << phase);
            }

            public void Recycle()
            {
            }

            // IBluetoothLEHelperPage_Bindings

            public void Initialize()
            {
                if (!this.initialized)
                {
                    this.Update();
                }
            }
            
            public void Update()
            {
                this.Update_(this.dataRoot, NOT_PHASED);
                this.initialized = true;
            }

            public void StopTracking()
            {
            }

            public void DisconnectUnloadedObject(int connectionId)
            {
                throw new global::System.ArgumentException("No unloadable elements to disconnect.");
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                if (newDataRoot != null)
                {
                    this.dataRoot = (global::Microsoft.Toolkit.Uwp.Connectivity.ObservableGattDeviceService)newDataRoot;
                    return true;
                }
                return false;
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::Microsoft.Toolkit.Uwp.Connectivity.ObservableGattDeviceService obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_Name(obj.Name, phase);
                    }
                }
            }
            private void Update_Name(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 49
                    XamlBindingSetters.Set_Windows_UI_Xaml_Documents_Run_Text(this.obj14, obj, null);
                }
            }
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class BluetoothLEHelperPage_obj18_Bindings :
            global::Windows.UI.Xaml.IDataTemplateExtension,
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            IBluetoothLEHelperPage_Bindings
        {
            private global::Microsoft.Toolkit.Uwp.Connectivity.ObservableBluetoothLEDevice dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);
            private bool removedDataContextHandler = false;

            // Fields for each control that has bindings.
            private global::System.WeakReference obj18;
            private global::Windows.UI.Xaml.Controls.TextBlock obj19;

            private BluetoothLEHelperPage_obj18_BindingsTracking bindingsTracking;

            public BluetoothLEHelperPage_obj18_Bindings()
            {
                this.bindingsTracking = new BluetoothLEHelperPage_obj18_BindingsTracking(this);
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 18: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 25
                        this.obj18 = new global::System.WeakReference((global::Windows.UI.Xaml.Controls.StackPanel)target);
                        break;
                    case 19: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 26
                        this.obj19 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    default:
                        break;
                }
            }

            public void DataContextChangedHandler(global::Windows.UI.Xaml.FrameworkElement sender, global::Windows.UI.Xaml.DataContextChangedEventArgs args)
            {
                 if (this.SetDataRoot(args.NewValue))
                 {
                    this.Update();
                 }
            }

            // IDataTemplateExtension

            public bool ProcessBinding(uint phase)
            {
                throw new global::System.NotImplementedException();
            }

            public int ProcessBindings(global::Windows.UI.Xaml.Controls.ContainerContentChangingEventArgs args)
            {
                int nextPhase = -1;
                ProcessBindings(args.Item, args.ItemIndex, (int)args.Phase, out nextPhase);
                return nextPhase;
            }

            public void ResetTemplate()
            {
                Recycle();
            }

            // IDataTemplateComponent

            public void ProcessBindings(global::System.Object item, int itemIndex, int phase, out int nextPhase)
            {
                nextPhase = -1;
                switch(phase)
                {
                    case 0:
                        nextPhase = -1;
                        this.SetDataRoot(item);
                        if (!removedDataContextHandler)
                        {
                            removedDataContextHandler = true;
                            (this.obj18.Target as global::Windows.UI.Xaml.Controls.StackPanel).DataContextChanged -= this.DataContextChangedHandler;
                        }
                        this.initialized = true;
                        break;
                }
                this.Update_((global::Microsoft.Toolkit.Uwp.Connectivity.ObservableBluetoothLEDevice) item, 1 << phase);
            }

            public void Recycle()
            {
                this.bindingsTracking.ReleaseAllListeners();
            }

            // IBluetoothLEHelperPage_Bindings

            public void Initialize()
            {
                if (!this.initialized)
                {
                    this.Update();
                }
            }
            
            public void Update()
            {
                this.Update_(this.dataRoot, NOT_PHASED);
                this.initialized = true;
            }

            public void StopTracking()
            {
                this.bindingsTracking.ReleaseAllListeners();
                this.initialized = false;
            }

            public void DisconnectUnloadedObject(int connectionId)
            {
                throw new global::System.ArgumentException("No unloadable elements to disconnect.");
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                this.bindingsTracking.ReleaseAllListeners();
                if (newDataRoot != null)
                {
                    this.dataRoot = (global::Microsoft.Toolkit.Uwp.Connectivity.ObservableBluetoothLEDevice)newDataRoot;
                    return true;
                }
                return false;
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::Microsoft.Toolkit.Uwp.Connectivity.ObservableBluetoothLEDevice obj, int phase)
            {
                this.bindingsTracking.UpdateChildListeners_(obj);
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_Name(obj.Name, phase);
                    }
                }
            }
            private void Update_Name(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED | DATA_CHANGED)) != 0)
                {
                    // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 26
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj19, obj, null);
                }
            }

            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            private class BluetoothLEHelperPage_obj18_BindingsTracking
            {
                private global::System.WeakReference<BluetoothLEHelperPage_obj18_Bindings> weakRefToBindingObj; 

                public BluetoothLEHelperPage_obj18_BindingsTracking(BluetoothLEHelperPage_obj18_Bindings obj)
                {
                    weakRefToBindingObj = new global::System.WeakReference<BluetoothLEHelperPage_obj18_Bindings>(obj);
                }

                public BluetoothLEHelperPage_obj18_Bindings TryGetBindingObject()
                {
                    BluetoothLEHelperPage_obj18_Bindings bindingObject = null;
                    if (weakRefToBindingObj != null)
                    {
                        weakRefToBindingObj.TryGetTarget(out bindingObject);
                        if (bindingObject == null)
                        {
                            weakRefToBindingObj = null;
                            ReleaseAllListeners();
                        }
                    }
                    return bindingObject;
                }

                public void ReleaseAllListeners()
                {
                    UpdateChildListeners_(null);
                }

                public void PropertyChanged_(object sender, global::System.ComponentModel.PropertyChangedEventArgs e)
                {
                    BluetoothLEHelperPage_obj18_Bindings bindings = TryGetBindingObject();
                    if (bindings != null)
                    {
                        string propName = e.PropertyName;
                        global::Microsoft.Toolkit.Uwp.Connectivity.ObservableBluetoothLEDevice obj = sender as global::Microsoft.Toolkit.Uwp.Connectivity.ObservableBluetoothLEDevice;
                        if (global::System.String.IsNullOrEmpty(propName))
                        {
                            if (obj != null)
                            {
                                bindings.Update_Name(obj.Name, DATA_CHANGED);
                            }
                        }
                        else
                        {
                            switch (propName)
                            {
                                case "Name":
                                {
                                    if (obj != null)
                                    {
                                        bindings.Update_Name(obj.Name, DATA_CHANGED);
                                    }
                                    break;
                                }
                                default:
                                    break;
                            }
                        }
                    }
                }
                public void UpdateChildListeners_(global::Microsoft.Toolkit.Uwp.Connectivity.ObservableBluetoothLEDevice obj)
                {
                    BluetoothLEHelperPage_obj18_Bindings bindings = TryGetBindingObject();
                    if (bindings != null)
                    {
                        if (bindings.dataRoot != null)
                        {
                            ((global::System.ComponentModel.INotifyPropertyChanged)bindings.dataRoot).PropertyChanged -= PropertyChanged_;
                        }
                        if (obj != null)
                        {
                            bindings.dataRoot = obj;
                            ((global::System.ComponentModel.INotifyPropertyChanged)obj).PropertyChanged += PropertyChanged_;
                        }
                    }
                }
            }
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class BluetoothLEHelperPage_obj1_Bindings :
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            IBluetoothLEHelperPage_Bindings
        {
            private global::Microsoft.Toolkit.Uwp.SampleApp.SamplePages.BluetoothLEHelperPage dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);

            // Fields for each control that has bindings.
            private global::Windows.UI.Xaml.Controls.ListView obj3;

            public BluetoothLEHelperPage_obj1_Bindings()
            {
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 3: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 14
                        this.obj3 = (global::Windows.UI.Xaml.Controls.ListView)target;
                        break;
                    default:
                        break;
                }
            }

            // IBluetoothLEHelperPage_Bindings

            public void Initialize()
            {
                if (!this.initialized)
                {
                    this.Update();
                }
            }
            
            public void Update()
            {
                this.Update_(this.dataRoot, NOT_PHASED);
                this.initialized = true;
            }

            public void StopTracking()
            {
            }

            public void DisconnectUnloadedObject(int connectionId)
            {
                throw new global::System.ArgumentException("No unloadable elements to disconnect.");
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                if (newDataRoot != null)
                {
                    this.dataRoot = (global::Microsoft.Toolkit.Uwp.SampleApp.SamplePages.BluetoothLEHelperPage)newDataRoot;
                    return true;
                }
                return false;
            }

            public void Loading(global::Windows.UI.Xaml.FrameworkElement src, object data)
            {
                this.Initialize();
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::Microsoft.Toolkit.Uwp.SampleApp.SamplePages.BluetoothLEHelperPage obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_bluetoothLEHelper(obj.bluetoothLEHelper, phase);
                    }
                }
            }
            private void Update_bluetoothLEHelper(global::Microsoft.Toolkit.Uwp.Connectivity.BluetoothLEHelper obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_bluetoothLEHelper_BluetoothLeDevices(obj.BluetoothLeDevices, phase);
                    }
                }
            }
            private void Update_bluetoothLEHelper_BluetoothLeDevices(global::System.Collections.ObjectModel.ObservableCollection<global::Microsoft.Toolkit.Uwp.Connectivity.ObservableBluetoothLEDevice> obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 14
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_ItemsControl_ItemsSource(this.obj3, obj, null);
                }
            }
        }
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 10
                {
                    this.BtEnumeration = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.BtEnumeration).Click += this.Enumeration_Click;
                }
                break;
            case 3: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 14
                {
                    this.LVDevices = (global::Windows.UI.Xaml.Controls.ListView)(target);
                    ((global::Windows.UI.Xaml.Controls.ListView)this.LVDevices).SelectionChanged += this.LVDevices_SelectionChanged;
                }
                break;
            case 4: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 31
                {
                    this.SPDevice = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 5: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 40
                {
                    this.CBServices = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.CBServices).SelectionChanged += this.CBServices_SelectionChanged;
                }
                break;
            case 6: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 55
                {
                    this.CBCharacteristic = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.CBCharacteristic).SelectionChanged += this.CBCharacteristic_SelectionChanged;
                }
                break;
            case 7: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 70
                {
                    this.BtReadCharValue = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.BtReadCharValue).Click += this.ReadCharValue_Click;
                }
                break;
            case 8: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 77
                {
                    this.TBCharValue = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 15: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 34
                {
                    this.TbDeviceName = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 16: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 36
                {
                    this.TbDeviceBtAddr = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            switch(connectionId)
            {
            case 1: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 1
                {                    
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)target;
                    BluetoothLEHelperPage_obj1_Bindings bindings = new BluetoothLEHelperPage_obj1_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(this);
                    this.Bindings = bindings;
                    element1.Loading += bindings.Loading;
                }
                break;
            case 10: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 62
                {                    
                    global::Windows.UI.Xaml.Controls.TextBlock element10 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                    BluetoothLEHelperPage_obj10_Bindings bindings = new BluetoothLEHelperPage_obj10_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(element10.DataContext);
                    element10.DataContextChanged += bindings.DataContextChangedHandler;
                    global::Windows.UI.Xaml.DataTemplate.SetExtensionInstance(element10, bindings);
                    global::Windows.UI.Xaml.Markup.XamlBindingHelper.SetDataTemplateComponent(element10, bindings);
                }
                break;
            case 13: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 47
                {                    
                    global::Windows.UI.Xaml.Controls.TextBlock element13 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                    BluetoothLEHelperPage_obj13_Bindings bindings = new BluetoothLEHelperPage_obj13_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(element13.DataContext);
                    element13.DataContextChanged += bindings.DataContextChangedHandler;
                    global::Windows.UI.Xaml.DataTemplate.SetExtensionInstance(element13, bindings);
                    global::Windows.UI.Xaml.Markup.XamlBindingHelper.SetDataTemplateComponent(element13, bindings);
                }
                break;
            case 18: // SamplePages\BluetoothLEHelper\BluetoothLEHelperPage.xaml line 25
                {                    
                    global::Windows.UI.Xaml.Controls.StackPanel element18 = (global::Windows.UI.Xaml.Controls.StackPanel)target;
                    BluetoothLEHelperPage_obj18_Bindings bindings = new BluetoothLEHelperPage_obj18_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(element18.DataContext);
                    element18.DataContextChanged += bindings.DataContextChangedHandler;
                    global::Windows.UI.Xaml.DataTemplate.SetExtensionInstance(element18, bindings);
                    global::Windows.UI.Xaml.Markup.XamlBindingHelper.SetDataTemplateComponent(element18, bindings);
                }
                break;
            }
            return returnValue;
        }
    }
}
