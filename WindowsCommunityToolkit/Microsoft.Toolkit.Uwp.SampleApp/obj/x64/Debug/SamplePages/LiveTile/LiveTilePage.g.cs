#pragma warning disable  
﻿#pragma checksum "C:\Users\GMateusDP\source\repos\WindowsCommunityToolkit\Microsoft.Toolkit.Uwp.SampleApp\SamplePages\LiveTile\LiveTilePage.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "0DB2762FAC19FD87D34AE65CA4D302564DC95337186D7BAF348869D7B022AD8A"
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
    partial class LiveTilePage : 
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
            case 2: // SamplePages\LiveTile\LiveTilePage.xaml line 22
                {
                    this.ButtonPinTile = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ButtonPinTile).Click += this.ButtonPinTile_Click;
                }
                break;
            case 3: // SamplePages\LiveTile\LiveTilePage.xaml line 78
                {
                    this.LargePreviewTile = (global::NotificationsVisualizerLibrary.PreviewTile)(target);
                }
                break;
            case 4: // SamplePages\LiveTile\LiveTilePage.xaml line 64
                {
                    this.WidePreviewTile = (global::NotificationsVisualizerLibrary.PreviewTile)(target);
                }
                break;
            case 5: // SamplePages\LiveTile\LiveTilePage.xaml line 50
                {
                    this.MediumPreviewTile = (global::NotificationsVisualizerLibrary.PreviewTile)(target);
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

