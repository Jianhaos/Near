﻿#pragma checksum "C:\Users\songj_000\documents\visual studio 2013\Projects\Near\Near\ChatRoom.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "E98F3E4B177B5DCB9B620B3C41F826EA"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34011
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace Near {
    
    
    public partial class ChatRoom : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.TextBlock Receiver;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.ScrollViewer MessageScroll;
        
        internal System.Windows.Controls.StackPanel MessageStack;
        
        internal Coding4Fun.Toolkit.Controls.RoundButton Clear;
        
        internal System.Windows.Controls.TextBox MessageTextBox;
        
        internal Coding4Fun.Toolkit.Controls.RoundButton Send;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/Near;component/ChatRoom.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.Receiver = ((System.Windows.Controls.TextBlock)(this.FindName("Receiver")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.MessageScroll = ((System.Windows.Controls.ScrollViewer)(this.FindName("MessageScroll")));
            this.MessageStack = ((System.Windows.Controls.StackPanel)(this.FindName("MessageStack")));
            this.Clear = ((Coding4Fun.Toolkit.Controls.RoundButton)(this.FindName("Clear")));
            this.MessageTextBox = ((System.Windows.Controls.TextBox)(this.FindName("MessageTextBox")));
            this.Send = ((Coding4Fun.Toolkit.Controls.RoundButton)(this.FindName("Send")));
        }
    }
}

