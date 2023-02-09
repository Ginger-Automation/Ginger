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

using System.Windows;
using System.Windows.Input;
using Ginger.Help;

namespace Ginger.GeneralLib
{
    /// <summary>
    /// Interaction logic for ToolTipPopUpWindow.xaml
    /// </summary>
    public partial class ToolTipPopUpWindow : Window
    {
        FrameworkElement mElem;

        public ToolTipPopUpWindow()
        {
            InitializeComponent();
        }

        public void ShowTip(FrameworkElement elem, string txt)
        {
            mElem = elem;

            Point position = new Point();
            double Height = 0;

            elem.MouseLeave += elem_MouseLeave;

            //Get elem position and height, must be on dispatcher for STA window
            elem.Dispatcher.Invoke(() =>
            {
                position = elem.PointToScreen(new Point(0d, 0d));
                Height = elem.ActualHeight;
            });

            // Set our window of tooltip popup, must be on dispatcher
            ToolTipPopupTextBlock.Dispatcher.Invoke(() =>
            {
                ToolTipPopupTextBlock.Text = txt;                
                this.Topmost = true;
                this.Top = position.Y + Height;
                this.Left = position.X;
                this.Show();
                                    
                //Hide the toolTip after 1.5 sec
                var timer = new System.Timers.Timer(1500) { AutoReset = false };
                timer.Elapsed += delegate
                {
                    HideToolTip();
                };

                timer.Enabled = true;    
            });
        }

        public void HideToolTip()
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    mElem.MouseLeave -= elem_MouseLeave;
                    this.Hide();
                    this.Topmost = false;
                });
            }
            catch
            {
                //do nothing
            }
        }

        private void ToolTipF1Button_Click(object sender, RoutedEventArgs e)
        {
            string HS = GingerHelpProvider.GetHelpString((FrameworkElement)sender);
            General.ShowGingerHelpWindow(HS);
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            //Enable the user to press the F1 button            
            this.Show();
        }

        private void elem_MouseLeave(object sender, MouseEventArgs e)
        {
             HideToolTip();
        }
    }
}