#pragma warning disable  
﻿#pragma checksum "C:\Users\GMateusDP\source\repos\WindowsCommunityToolkit\Microsoft.Toolkit.Uwp.SampleApp\SamplePages\Connected Animations\Pages\ThirdPage.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "62AB7B0937E88A6D3B5F7AB7557EC978AEEA4CBBC4E8F125ABD63E8FD24B285B"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Toolkit.Uwp.SampleApp.SamplePages.ConnectedAnimations.Pages
{
    partial class ThirdPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.19041.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private static class XamlBindingSetters
        {
            public static void Set_Microsoft_Toolkit_Uwp_UI_Animations_Connected_AnchorElement(global::Windows.UI.Xaml.DependencyObject obj, global::Windows.UI.Xaml.UIElement value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::Windows.UI.Xaml.UIElement) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::Windows.UI.Xaml.UIElement), targetNullValue);
                }
                global::Microsoft.Toolkit.Uwp.UI.Animations.Connected.SetAnchorElement(obj, value);
            }
            public static void Set_Windows_UI_Xaml_Controls_Image_Source(global::Windows.UI.Xaml.Controls.Image obj, global::Windows.UI.Xaml.Media.ImageSource value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::Windows.UI.Xaml.Media.ImageSource) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::Windows.UI.Xaml.Media.ImageSource), targetNullValue);
                }
                obj.Source = value;
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
        private class ThirdPage_obj1_Bindings :
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            IThirdPage_Bindings
        {
            private global::Microsoft.Toolkit.Uwp.SampleApp.SamplePages.ConnectedAnimations.Pages.ThirdPage dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);

            // Fields for each control that has bindings.
            private global::Windows.UI.Xaml.Controls.StackPanel obj3;
            private global::Windows.UI.Xaml.Controls.Image obj4;
            private global::Windows.UI.Xaml.Controls.TextBlock obj5;

            public ThirdPage_obj1_Bindings()
            {
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 3: // SamplePages\Connected Animations\Pages\ThirdPage.xaml line 13
                        this.obj3 = (global::Windows.UI.Xaml.Controls.StackPanel)target;
                        break;
                    case 4: // SamplePages\Connected Animations\Pages\ThirdPage.xaml line 21
                        this.obj4 = (global::Windows.UI.Xaml.Controls.Image)target;
                        break;
                    case 5: // SamplePages\Connected Animations\Pages\ThirdPage.xaml line 18
                        this.obj5 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    default:
                        break;
                }
            }

            // IThirdPage_Bindings

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
                    this.dataRoot = (global::Microsoft.Toolkit.Uwp.SampleApp.SamplePages.ConnectedAnimations.Pages.ThirdPage)newDataRoot;
                    return true;
                }
                return false;
            }

            public void Loading(global::Windows.UI.Xaml.FrameworkElement src, object data)
            {
                this.Initialize();
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::Microsoft.Toolkit.Uwp.SampleApp.SamplePages.ConnectedAnimations.Pages.ThirdPage obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_ItemHeroElement(obj.ItemHeroElement, phase);
                        this.Update_item(obj.item, phase);
                    }
                }
            }
            private void Update_ItemHeroElement(global::Windows.UI.Xaml.Controls.Image obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // SamplePages\Connected Animations\Pages\ThirdPage.xaml line 13
                    XamlBindingSetters.Set_Microsoft_Toolkit_Uwp_UI_Animations_Connected_AnchorElement(this.obj3, obj, null);
                }
            }
            private void Update_item(global::Microsoft.Toolkit.Uwp.SampleApp.Data.PhotoDataItem obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_item_Thumbnail(obj.Thumbnail, phase);
                        this.Update_item_Title(obj.Title, phase);
                    }
                }
            }
            private void Update_item_Thumbnail(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // SamplePages\Connected Animations\Pages\ThirdPage.xaml line 21
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_Image_Source(this.obj4, (global::Windows.UI.Xaml.Media.ImageSource) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::Windows.UI.Xaml.Media.ImageSource), obj), null);
                }
            }
            private void Update_item_Title(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // SamplePages\Connected Animations\Pages\ThirdPage.xaml line 18
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj5, obj, null);
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
            case 2: // SamplePages\Connected Animations\Pages\ThirdPage.xaml line 27
                {
                    this.Content = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 3: // SamplePages\Connected Animations\Pages\ThirdPage.xaml line 13
                {
                    this.HeroDetailsElement = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 4: // SamplePages\Connected Animations\Pages\ThirdPage.xaml line 21
                {
                    this.ItemHeroElement = (global::Windows.UI.Xaml.Controls.Image)(target);
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
            case 1: // SamplePages\Connected Animations\Pages\ThirdPage.xaml line 1
                {                    
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)target;
                    ThirdPage_obj1_Bindings bindings = new ThirdPage_obj1_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(this);
                    this.Bindings = bindings;
                    element1.Loading += bindings.Loading;
                }
                break;
            }
            return returnValue;
        }
    }
}

