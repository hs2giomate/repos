#pragma warning disable  
﻿#pragma checksum "C:\Users\GMateusDP\source\repos\WindowsCommunityToolkit\Microsoft.Toolkit.Uwp.SampleApp\SamplePages\StringExtensions\StringExtensionsPage.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "2E59F024E773A7EF18DEAF6769CF1258D3C8635F57CBC93AB0BAA85252F8CBC5"
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
    partial class StringExtensionsPage : 
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
            case 2: // SamplePages\StringExtensions\StringExtensionsPage.xaml line 13
                {
                    this.InputTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                    ((global::Windows.UI.Xaml.Controls.TextBox)this.InputTextBox).TextChanged += this.OnTextChanged;
                }
                break;
            case 3: // SamplePages\StringExtensions\StringExtensionsPage.xaml line 27
                {
                    this.IsValidEmailResult = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 4: // SamplePages\StringExtensions\StringExtensionsPage.xaml line 30
                {
                    this.IsValidNumberResult = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 5: // SamplePages\StringExtensions\StringExtensionsPage.xaml line 33
                {
                    this.IsValidDecimalResult = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 6: // SamplePages\StringExtensions\StringExtensionsPage.xaml line 36
                {
                    this.IsValidStringResult = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 7: // SamplePages\StringExtensions\StringExtensionsPage.xaml line 39
                {
                    this.IsValidPhoneNumberResult = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
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

