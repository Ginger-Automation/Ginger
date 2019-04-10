#region License
/*
Copyright © 2014-2019 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger
{    
    public enum eWindowShowStyle
    {
        Free,
        FreeMaximized,
        Dialog        
    }

    /// <summary>
    /// Interaction logic for GenericWindow.xaml
    /// </summary>
    public partial class GenericWindow : Window
    {

        // Used for Ginger Automator
        public static GenericWindow CurrentWindow;

        private RoutedEventHandler mCloseEventHandler;

        public eWindowShowStyle CurrentWinStyle { get; set; }
        public bool NeedToReShow { get; set; }
        public eWindowShowStyle ReShowStyle { get; set; }

        double mPageOriginalWidth = -1;
        double mPageOriginalHeight = -1;
        private bool OwnerWindowClosing = false;

        public GenericWindow(Window Owner, eWindowShowStyle windowStyle, string windowTitle,
                                Page windowPage, ObservableList<Button> windowBtnsList = null, bool showClosebtn = true, string closeBtnText = "Close", RoutedEventHandler closeEventHandler = null)
        {
            InitializeComponent();   
            this.Owner = Owner;
            if (this.Owner != null)
            {
                this.Owner.Closing += Owner_Closing;
            }
            CurrentWindow = this;

            //set style
            CurrentWinStyle = windowStyle;
            if (CurrentWinStyle == eWindowShowStyle.Dialog)
                PinBtn.Visibility = System.Windows.Visibility.Visible;
            else
                PinBtn.Visibility = System.Windows.Visibility.Collapsed;
            NeedToReShow = false;

            //set title
            winTitle.Content = windowTitle;

            //set size
            if (windowStyle == eWindowShowStyle.FreeMaximized)
            {
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                if (windowPage != null)
                {
                    double widthDelta = 20;
                    double heightDelta = 100;
                    
                    //keep window min width & height
                    if (windowPage.MinWidth > 0)
                        mPageOriginalWidth = windowPage.MinWidth;
                    else if (windowPage.Width > 0)
                        mPageOriginalWidth = windowPage.Width;
                    if (windowPage.MinHeight > 0)
                        mPageOriginalHeight = windowPage.MinHeight;
                    else if (windowPage.Height > 0)
                        mPageOriginalHeight = windowPage.Height;

                    if (mPageOriginalWidth == -1)
                    {
                        windowPage.Width = 600;
                        mPageOriginalWidth = 600;
                    }
                    if (mPageOriginalHeight == -1)
                    {
                        windowPage.Height = this.Height;
                        mPageOriginalHeight = this.Height;
                    }

                    //doing some user screen calculations
                    double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                    double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                    List<System.Windows.Forms.Screen> allScreens = System.Windows.Forms.Screen.AllScreens.ToList();
                    if (allScreens.Count > 1)
                    {
                        //take smallest screen size
                        foreach (System.Windows.Forms.Screen scr in allScreens)
                        {
                            if (scr.Bounds.Width < screenWidth)
                                screenWidth = scr.Bounds.Width;
                            if (scr.Bounds.Height < screenHeight)
                                screenHeight = scr.Bounds.Height;
                        }
                    }

                    //fit page size to screen size
                    if (Convert.ToString(windowPage.Tag) != "PageSizeWasModified")
                    {
                        if (windowPage.Width > 0)
                            windowPage.Width = ((windowPage.Width / 1280) * screenWidth);//relative to screen size
                        if (windowPage.Width > screenWidth)
                            windowPage.Width = screenWidth - widthDelta - 100;
                        
                        if (windowPage.Height > 0)
                            windowPage.Height = ((windowPage.Height / 1024) * screenHeight);//relative to screen size
                        if (windowPage.Height > screenHeight)
                            windowPage.Height = screenHeight - heightDelta - 100;

                        if (windowPage.Tag == null)
                            windowPage.Tag = "PageSizeWasModified";
                    }

                    //set min height and width
                    if (windowPage.MinWidth > 0)
                    {
                        if (windowPage.Width < windowPage.MinWidth)
                        {
                            if (windowPage.MinWidth > screenWidth)
                                windowPage.MinWidth = screenWidth - widthDelta - 100;
                            windowPage.Width = windowPage.MinWidth;
                        }
                    }
                    if (windowPage.MinHeight > 0)
                    {
                        if (windowPage.Height < windowPage.MinHeight)
                        {
                            if (windowPage.MinHeight > screenHeight)
                                windowPage.MinHeight = screenHeight - heightDelta - 100;
                            windowPage.Height = windowPage.MinHeight;
                        }
                    }

                    //set the window size based on page size
                    this.Width = windowPage.Width + widthDelta;
                    this.MinWidth = windowPage.MinWidth + widthDelta;

                    this.Height = windowPage.Height + heightDelta;
                    this.MinHeight = windowPage.MinHeight + heightDelta;
                }
            }

            //set page content
            if (windowPage != null)
            {
                PageFrame.Content = windowPage;
            }


            //set window buttons

            //close buttons handling
            mCloseEventHandler = closeEventHandler;
            if (!showClosebtn)
            {
                CloseBtn.Visibility = System.Windows.Visibility.Collapsed;
                if (mCloseEventHandler == null)
                {
                    UpperCloseBtn.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            if (!string.IsNullOrEmpty(closeBtnText))
            {
                CloseBtn.Content = closeBtnText;
                UpperCloseBtn.ToolTip = closeBtnText;
            }            

            if (windowBtnsList != null)
            {
                foreach (Button btn in windowBtnsList)
                {
                    btn.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    Thickness margin = btn.Margin;
                    if (margin.Right < 10)
                        margin.Right = 10;
                    btn.Margin = margin;
                    btn.Style = this.FindResource("$WindowButtonStyle") as Style;
                    DockPanel.SetDock(btn, Dock.Right);
                    BottomPanel.Children.Add(btn);
                }
            }            
        }

        private void Owner_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OwnerWindowClosing = true;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow((Button)sender);
            parentWindow.IsEnabled = false;
            if (mCloseEventHandler != null)
            {
                mCloseEventHandler.Invoke(this, new RoutedEventArgs());                
            }
            else
                this.Close();
            parentWindow.IsEnabled = true;            
        }

        private void PinBtn_Click(object sender, RoutedEventArgs e)
        {
            NeedToReShow = true;
            if (CurrentWinStyle == eWindowShowStyle.Dialog)
                ReShowStyle = eWindowShowStyle.Free;
            else
                ReShowStyle = eWindowShowStyle.Dialog;
            this.Close();
        }

        private void MaximizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Maximized;            
        }

        private void RestoreBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Normal; 
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;            
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {            
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                MaximizeBtn.Visibility = System.Windows.Visibility.Collapsed;
                RestoreBtn.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                MaximizeBtn.Visibility = System.Windows.Visibility.Visible;
                RestoreBtn.Visibility = System.Windows.Visibility.Collapsed;
            }

            FixContentPageSize();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FixContentPageSize();
        }

        private void FixContentPageSize()
        {
            if (PageFrame.Content != null)
            {
                Page contentPage = (Page)PageFrame.Content;

                if (this.ActualWidth- 20 > mPageOriginalWidth)
                    contentPage.Width = this.ActualWidth - 20;
                if (mPageOriginalWidth > 0 && this.ActualWidth < mPageOriginalWidth)
                {                 
                    WindowScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
                else
                {
                    WindowScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;                    
                }

                if (this.ActualHeight - 100 > mPageOriginalHeight)
                    contentPage.Height = this.ActualHeight - 100;
                if (mPageOriginalHeight > 0 && this.Height < mPageOriginalHeight)
                {
                    WindowScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
                else
                {
                    WindowScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }
            }
        }

        private void HeaderPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }


        public static void LoadGenericWindow(ref GenericWindow genWindow, System.Windows.Window owner, eWindowShowStyle windowStyle, string windowTitle, Page windowPage,
                                        ObservableList<Button> windowBtnsList = null, bool showClosebtn = true, string closeBtnText = "Close", RoutedEventHandler closeEventHandler = null, bool startupLocationWithOffset = false)
        {
            //GenericWindow win = null;
            genWindow = null;
            eWindowShowStyle winStyle;
            do
            {
                if (genWindow != null)
                {
                    winStyle = genWindow.ReShowStyle;
                    genWindow.BottomPanel.Children.Clear();
                    genWindow = null;
                }
                else
                {
                    winStyle = windowStyle;
                }
                genWindow = new GenericWindow(owner, winStyle, windowTitle, windowPage, windowBtnsList, showClosebtn, closeBtnText, closeEventHandler);
                genWindow.Title = windowPage.Title;
                if (startupLocationWithOffset)
                {
                    genWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    genWindow.Left = 50;
                    genWindow.Top = 200;
                }
                if (winStyle == eWindowShowStyle.Dialog)
                    genWindow.ShowDialog();
                else
                    genWindow.Show();
            }
            while (genWindow.NeedToReShow);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //to make sure the parent window will be showen
            if (this.Owner != null)
            {
                if (!this.Owner.IsVisible & !OwnerWindowClosing)
                {
                    this.Owner.Show();
                }
                if (this.Owner.WindowState == WindowState.Minimized)
                {
                    this.Owner.WindowState = WindowState.Normal;
                }
                this.Owner.Activate();
                this.Owner.Topmost = true;  
                this.Owner.Topmost = false; 
                this.Owner.Focus();
            }
        }
    }
}
