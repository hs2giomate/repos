#pragma warning disable  
﻿#pragma checksum "C:\Users\GMateusDP\source\repos\WindowsCommunityToolkit\Microsoft.Toolkit.Uwp.SampleApp\SamplePages\RemoteDeviceHelper\RemoteDeviceHelperPage.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "805861C852D9621A3A5CA592DB4FCFD9802E7BA288A6219F91AFAE126D507642"
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
    partial class RemoteDeviceHelperPage : 
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
            case 2: // SamplePages\RemoteDeviceHelper\RemoteDeviceHelperPage.xaml line 37
                {
                    this.DevicesList = (global::Windows.UI.Xaml.Controls.ListView)(target);
                }
                break;
            case 4: // SamplePages\RemoteDeviceHelper\RemoteDeviceHelperPage.xaml line 29
                {
                    this.filterStackPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 5: // SamplePages\RemoteDeviceHelper\RemoteDeviceHelperPage.xaml line 34
                {
                    global::Windows.UI.Xaml.Controls.Button element5 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)element5).Tapped += this.Button_Tapped;
                }
                break;
            case 6: // SamplePages\RemoteDeviceHelper\RemoteDeviceHelperPage.xaml line 30
                {
                    this.discoveryType = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                }
                break;
            case 7: // SamplePages\RemoteDeviceHelper\RemoteDeviceHelperPage.xaml line 31
                {
                    this.authorizationType = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                }
                break;
            case 8: // SamplePages\RemoteDeviceHelper\RemoteDeviceHelperPage.xaml line 32
                {
                    this.statusType = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
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

