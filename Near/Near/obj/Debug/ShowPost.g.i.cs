﻿#pragma checksum "C:\Users\songj_000\documents\visual studio 2013\Projects\Near\Near\ShowPost.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "3BF4AB6E005B3EF2370EAB2EA98F6B1C"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34011
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

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
    
    
    public partial class ShowPost : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.TextBox PostContent;
        
        internal System.Windows.Controls.TextBox Location;
        
        internal System.Windows.Controls.TextBox PostTime;
        
        internal System.Windows.Controls.ScrollViewer CommentScroll;
        
        internal System.Windows.Controls.Image PostImage;
        
        internal System.Windows.Controls.StackPanel CommentStack;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/Near;component/ShowPost.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.PostContent = ((System.Windows.Controls.TextBox)(this.FindName("PostContent")));
            this.Location = ((System.Windows.Controls.TextBox)(this.FindName("Location")));
            this.PostTime = ((System.Windows.Controls.TextBox)(this.FindName("PostTime")));
            this.CommentScroll = ((System.Windows.Controls.ScrollViewer)(this.FindName("CommentScroll")));
            this.PostImage = ((System.Windows.Controls.Image)(this.FindName("PostImage")));
            this.CommentStack = ((System.Windows.Controls.StackPanel)(this.FindName("CommentStack")));
        }
    }
}
