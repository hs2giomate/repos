#pragma warning disable  
﻿#pragma checksum "C:\Users\GMateusDP\source\repos\WindowsCommunityToolkit\Microsoft.Toolkit.Uwp.SampleApp\SamplePages\ViewportBehavior\ViewportBehaviorPage.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "462E7D88A4DD554ABF8FC59FD882863591492B9B7DDDE79BBA28C52C2B7AB7CD"
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
    partial class ViewportBehaviorPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // SamplePages\ViewportBehavior\ViewportBehaviorPage.xaml line 19
                {
                    this.XamlRoot = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 3: // SamplePages\ViewportBehavior\ViewportBehaviorPage.xaml line 40
                {
                    this.LogsScrollViewer = (global::Windows.UI.Xaml.Controls.ScrollViewer)(target);
                }
                break;
            case 4: // SamplePages\ViewportBehavior\ViewportBehaviorPage.xaml line 43
                {
                    this.LogsItemsControl = (global::Windows.UI.Xaml.Controls.ItemsControl)(target);
                }
                break;
            case 5: // SamplePages\ViewportBehavior\ViewportBehaviorPage.xaml line 36
                {
                    global::Windows.UI.Xaml.Controls.Button element5 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)element5).Click += this.ClearLogsButton_Click;
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
            return returnValue;
        }
    }
}

