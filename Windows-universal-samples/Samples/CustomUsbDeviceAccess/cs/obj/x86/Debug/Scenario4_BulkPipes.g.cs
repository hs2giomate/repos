﻿#pragma checksum "C:\Users\GMateusDP\Source\Repos\Windows-universal-samples\Samples\CustomUsbDeviceAccess\cs\Scenario4_BulkPipes.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "B630B523027F75C24FCC26D13EB6A39F"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CustomUsbDeviceAccess
{
    partial class Scenario4_BulkPipes : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // Scenario4_BulkPipes.xaml line 85
                {
                    this.StatusBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 3: // Scenario4_BulkPipes.xaml line 36
                {
                    this.LayoutRoot = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 4: // Scenario4_BulkPipes.xaml line 41
                {
                    this.Input = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 5: // Scenario4_BulkPipes.xaml line 67
                {
                    this.Output = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 6: // Scenario4_BulkPipes.xaml line 71
                {
                    this.OutputFullScreenLandscape = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 7: // Scenario4_BulkPipes.xaml line 72
                {
                    this.OutputFilled = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 8: // Scenario4_BulkPipes.xaml line 73
                {
                    this.OutputFullScreenPortrait = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 9: // Scenario4_BulkPipes.xaml line 74
                {
                    this.OutputSnapped = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 10: // Scenario4_BulkPipes.xaml line 46
                {
                    this.DeviceScenarioContainer = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 11: // Scenario4_BulkPipes.xaml line 47
                {
                    this.GeneralScenario = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 12: // Scenario4_BulkPipes.xaml line 48
                {
                    this.GeneralScenarioText = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 13: // Scenario4_BulkPipes.xaml line 53
                {
                    this.ButtonBulkReadWrite = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ButtonBulkReadWrite).Click += this.BulkReadWrite_Click;
                }
                break;
            case 14: // Scenario4_BulkPipes.xaml line 54
                {
                    this.ButtonCancelAllIoTasks = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ButtonCancelAllIoTasks).Click += this.CancelAllIoTasks_Click;
                }
                break;
            case 15: // Scenario4_BulkPipes.xaml line 50
                {
                    this.ButtonBulkRead = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ButtonBulkRead).Click += this.BulkRead_Click;
                }
                break;
            case 16: // Scenario4_BulkPipes.xaml line 51
                {
                    this.ButtonBulkWrite = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ButtonBulkWrite).Click += this.BulkWrite_Click;
                }
                break;
            case 17: // Scenario4_BulkPipes.xaml line 60
                {
                    this.InputFullScreenLandscape = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 18: // Scenario4_BulkPipes.xaml line 61
                {
                    this.InputFilled = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 19: // Scenario4_BulkPipes.xaml line 62
                {
                    this.InputFullScreenPortrait = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 20: // Scenario4_BulkPipes.xaml line 63
                {
                    this.InputSnapped = (global::Windows.UI.Xaml.VisualState)(target);
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
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

