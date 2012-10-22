﻿#pragma checksum "..\..\..\VideoDisplayWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "DA154C088FAD72420F16B023A526E5A3"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using EERIL.ControlSystem;
using EERIL.DeviceControls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace EERIL.ControlSystem {
    
    
    /// <summary>
    /// VideoDisplayWindow
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class VideoDisplayWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 6 "..\..\..\VideoDisplayWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid canvas;
        
        #line default
        #line hidden
        
        
        #line 7 "..\..\..\VideoDisplayWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image videoImage;
        
        #line default
        #line hidden
        
        
        #line 8 "..\..\..\VideoDisplayWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal EERIL.DeviceControls.HeadUpDisplay headsUpDisplay;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/EERIL.ControlSystem;component/videodisplaywindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\VideoDisplayWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 5 "..\..\..\VideoDisplayWindow.xaml"
            ((EERIL.ControlSystem.VideoDisplayWindow)(target)).Closed += new System.EventHandler(this.WindowClosed);
            
            #line default
            #line hidden
            return;
            case 2:
            this.canvas = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.videoImage = ((System.Windows.Controls.Image)(target));
            return;
            case 4:
            this.headsUpDisplay = ((EERIL.DeviceControls.HeadUpDisplay)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

