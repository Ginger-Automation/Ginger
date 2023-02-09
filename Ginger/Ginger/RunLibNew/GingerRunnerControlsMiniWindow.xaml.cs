#region License
/*
Copyright © 2014-2023 European Support Limited

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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GingerWPF.RunLib
{
    /// <summary>
    /// Interaction logic for GingerRunnerControlsMiniWindow.xaml
    /// </summary>
    public partial class GingerRunnerControlsMiniWindow : Window
    {
        Window mBigWindow;        
        ContentControl mGingerRunnerControlsPageContentControl;
        Grid mGrid;
        private Action<bool> setMiniView;

        public GingerRunnerControlsMiniWindow(Grid grid, ContentControl CC, Action<bool> setMiniView)
        {
            InitializeComponent();
            this.Top = 0;
            this.Left = 0;

            MainFrame.Content = grid;
            mGingerRunnerControlsPageContentControl = CC;
            mGrid = grid;
            this.setMiniView = setMiniView;

            mBigWindow = Window.GetWindow(CC);
            mBigWindow.WindowState = WindowState.Minimized;
            mBigWindow.ShowInTaskbar = false;
        }

        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            mGingerRunnerControlsPageContentControl.Content = mGrid;
                
            //restore to previous status
            mBigWindow.WindowState = WindowState.Maximized;
            mBigWindow.ShowInTaskbar = true;
            setMiniView(false);
            this.Close();
        }

        //TODO: move to globals/helpers
        public static T GetParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject dependencyObject = VisualTreeHelper.GetParent(child);
            if (dependencyObject != null)
            {
                T parent = dependencyObject as T;

                if (parent != null)
                {
                    return parent;
                }
                else
                {
                    return GetParent<T>(dependencyObject);
                }
            }
            else
            {
                return null;
            }
        }
    }
}