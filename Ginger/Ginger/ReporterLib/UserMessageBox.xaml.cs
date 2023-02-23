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
using System.Windows.Media;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.UserControls;

namespace Ginger.ReporterLib
{
    /// <summary>
    /// Interaction logic for MessageBoxWindow.xaml
    /// </summary>
    public partial class UserMessageBox : Window
    {

        public Amdocs.Ginger.Common.eUserMsgSelection messageBoxResult { get; set; }

        
        public UserMessageBox(string txt, string caption, eUserMsgOption buttonsType,
                                            eUserMsgIcon messageImage, eUserMsgSelection defualtResualt)
        {
            InitializeComponent();

            this.Title = caption;

            xMessageTextBlock.Text = txt;
            messageBoxResult = defualtResualt;

            xOKButton.Visibility = Visibility.Collapsed;
            xYesButton.Visibility = Visibility.Collapsed;
            xNoButton.Visibility = Visibility.Collapsed;
            xCancelButton.Visibility = Visibility.Collapsed;

            switch (buttonsType)
            {                
                case Amdocs.Ginger.Common.eUserMsgOption.OKCancel:
                    xOKButton.Visibility = Visibility.Visible;
                    xCancelButton.Visibility = Visibility.Visible;
                    break;
                case Amdocs.Ginger.Common.eUserMsgOption.YesNo:
                    xYesButton.Visibility = Visibility.Visible; ;
                    xNoButton.Visibility = Visibility.Visible;
                    break;
                case Amdocs.Ginger.Common.eUserMsgOption.YesNoCancel:
                    xYesButton.Visibility = Visibility.Visible; 
                    xNoButton.Visibility = Visibility.Visible;
                    xCancelButton.Visibility = Visibility.Visible;
                    break;
                case Amdocs.Ginger.Common.eUserMsgOption.OK:
                default:
                    xOKButton.Visibility = Visibility.Visible;
                    break;
            }

            switch (messageImage)
            {
                case eUserMsgIcon.Error:
                    xMessageImage.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Error;
                    xMessageImage.ImageForeground = Brushes.DarkRed;
                    break;
                case eUserMsgIcon.None:
                    xMessageImage.Visibility = Visibility.Collapsed;
                    break;
                case eUserMsgIcon.Question:
                    xMessageImage.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Question;
                    xMessageImage.ImageForeground = Brushes.Purple;
                    break;
                case eUserMsgIcon.Warning:
                    xMessageImage.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Warn;
                    xMessageImage.ImageForeground = Brushes.DarkOrange;
                    break;
                case eUserMsgIcon.Information:
                default:
                    xMessageImage.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Info;
                    xMessageImage.ImageForeground = Brushes.DarkBlue;
                    break;
            }
        }

        //private LinearGradientBrush GetGradientBrush(Color color1, Color color2)
        //{
        //    LinearGradientBrush gradientBrush = new LinearGradientBrush();
        //    gradientBrush.StartPoint = new Point(0, 0);
        //    gradientBrush.EndPoint = new Point(0.99, 1);
        //    gradientBrush.SpreadMethod = GradientSpreadMethod.Pad;
        //    gradientBrush.ColorInterpolationMode = ColorInterpolationMode.ScRgbLinearInterpolation;
        //    GradientStop gradientStop1 = new GradientStop();
        //    gradientStop1.Color = color1;
        //    gradientStop1.Offset = 0;
        //    gradientBrush.GradientStops.Add(gradientStop1);
        //    GradientStop gradientStop2 = new GradientStop();
        //    gradientStop2.Color = color2;
        //    gradientStop2.Offset = 1;
        //    gradientBrush.GradientStops.Add(gradientStop2);
        //    return gradientBrush;
        //}
              
        private void xOKButton_Click(object sender, RoutedEventArgs e)
        {
            messageBoxResult = Amdocs.Ginger.Common.eUserMsgSelection.OK;
            this.Close();
        }

        private void XYesButton_Click(object sender, RoutedEventArgs e)
        {
            messageBoxResult = Amdocs.Ginger.Common.eUserMsgSelection.Yes;
            this.Close();
        }

        private void XNoButton_Click(object sender, RoutedEventArgs e)
        {
            messageBoxResult = Amdocs.Ginger.Common.eUserMsgSelection.No;
            this.Close();
        }

        private void XCancelButton_Click(object sender, RoutedEventArgs e)
        {
            messageBoxResult = Amdocs.Ginger.Common.eUserMsgSelection.Cancel;
            this.Close();
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (xOKButton.IsKeyboardFocusWithin)
                {
                    xOKButton_Click(null, null);
                }
                else if (xYesButton.IsKeyboardFocusWithin)
                {
                    XYesButton_Click(null, null);
                }
                else if (xNoButton.IsKeyboardFocusWithin)
                {
                    XNoButton_Click(null, null);
                }
                else if (xCancelButton.IsKeyboardFocusWithin)
                {
                    XCancelButton_Click(null, null);
                }
                else
                {
                    this.Close();
                }
            }
        }
        
    }
}
