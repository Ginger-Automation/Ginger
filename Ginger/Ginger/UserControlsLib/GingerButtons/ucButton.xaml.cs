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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Core;

namespace Amdocs.Ginger.UserControls
{
    /// <summary>
    /// Interaction logic for ucButton.xaml
    /// </summary>
    public partial class ucButton : UserControl
    {
        public ucButton()
        {
            InitializeComponent();
            SetButtonLook();
        }

        public static readonly DependencyProperty ButtonTypeProperty = DependencyProperty.Register("ButtonType", typeof(eButtonType), typeof(ucButton),
                              new FrameworkPropertyMetadata(OnIconPropertyChanged));

        public static readonly DependencyProperty ButtonImageTypeProperty = DependencyProperty.Register("ButtonImageType", typeof(eImageType), typeof(ucButton),
                              new FrameworkPropertyMetadata(OnIconPropertyChanged));

        public static readonly DependencyProperty ButtonImageForgroundProperty = DependencyProperty.Register("ButtonImageForground", typeof(SolidColorBrush), typeof(ucButton),
                      new FrameworkPropertyMetadata(ButtonImageForgroundPropertyChanged));

        public eButtonType ButtonType
        {
            get { return (eButtonType)GetValue(ButtonTypeProperty); }
            set
            {
                SetValue(ButtonTypeProperty, value);
            }
        }
        private static void OnIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ucButton ub = (ucButton)d;
            ub.SetButtonLook();
        }

        public SolidColorBrush ButtonImageForground
        {
            get { return xButtonImage.ImageForeground; }
            set { xButtonImage.ImageForeground = value; }
        }
        private static void ButtonImageForgroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ucButton;
            if (control != null && e.NewValue != null)
            {
                control.ButtonImageForground = ((SolidColorBrush)e.NewValue);
            }
        }

        public string ButtonText
        {
            get { return xButtonText.Text.ToString(); }
            set { xButtonText.Text = value; }
        }      
        
        public eImageType ButtonImageType
        {
            get { return (eImageType)GetValue(ButtonImageTypeProperty); }
            set
            {
                SetValue(ButtonImageTypeProperty, value);               
            }
        }
        public bool SetBorder
        {
            get { return xButtonImage.SetBorder; }
            set { xButtonImage.SetBorder = value; }
        }

        public Style ButtonStyle
        {
            get { return xButton.Style; }
            set { xButton.Style = value; }
        }

        public double ButtonFontImageSize
        {
            get { return xButtonImage.SetAsFontImageWithSize; }
            set { xButtonImage.SetAsFontImageWithSize = value; }
        }

        public double ButtonImageHeight
        {
            get { return xButtonImage.Height; }
            set { xButtonImage.Height = value; }
        }
        public double ButtonImageWidth
        {
            get { return xButtonImage.Width; }
            set { xButtonImage.Width = value; }
        }



        private void SetButtonLook()
        {
            xButtonImage.ImageType = ButtonImageType;
            switch (ButtonType)
            {
                case (Core.eButtonType.ImageButton):
                    xButtonText.Visibility = Visibility.Collapsed;
                    xButton.Style = FindResource("$ImageButtonStyle") as Style;                    
                    break;
                case (Core.eButtonType.RoundTextAndImageButton):
                    xButtonText.Visibility = Visibility.Visible;
                    xButtonText.Margin = new Thickness(10, 0, 10, 0);
                    xButtonImage.Margin = new Thickness(0, 0, 10, 0);
                    xButton.Style = FindResource("$RoundTextAndImageButtonStyle") as Style;                  
                    break;
                case (Core.eButtonType.CircleImageButton):
                    xButtonText.Visibility = Visibility.Collapsed;
                    xButton.Style = FindResource("$CircleImageButtonStyle") as Style;
                    xButton.Width = ButtonImageWidth + (ButtonImageWidth / 2);
                    xButton.Height = ButtonImageHeight + (ButtonImageHeight / 2);
                    xButtonImage.Margin = new Thickness(3);                    
                    break;
                case (Core.eButtonType.CircleImageLightButton):
                    xButtonText.Visibility = Visibility.Collapsed;
                    xButton.Style = FindResource("$CircleImageLightButtonStyle") as Style;
                    xButton.Width = ButtonImageWidth + (ButtonImageWidth / 2);
                    xButton.Height = ButtonImageHeight + (ButtonImageHeight / 2);
                    xButtonImage.Margin = new Thickness(3);
                    break;
                case (Core.eButtonType.PanelButton):
                    xButtonText.Visibility = Visibility.Visible;
                    xButtonText.Margin = new Thickness(10, 0, 10, 0);
                    xButtonImage.Margin = new Thickness(0, 0, 10, 0);
                    xButton.Style = FindResource("$PanelButtonStyle") as Style;
                    break;
            }
        }     
              

        public event RoutedEventHandler Click;
  

        private void xButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Click(this, e);
        }

        public void DoClick()
        {
            Click(this, null);
        }

        Style mPreviousStyle = null;
        private void IsEnabledChangeHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!xButton.IsEnabled)
            {
                mPreviousStyle = xButton.Style;
                switch (ButtonType)
                {
                    case (Core.eButtonType.ImageButton):                       
                        xButton.Style = FindResource("$ImageButtonStyle_Disabled") as Style;
                        break;
                    case (Core.eButtonType.RoundTextAndImageButton):
                        xButton.Style = FindResource("$RoundTextAndImageButtonStyle_Disabled") as Style;
                        break;
                    case (Core.eButtonType.CircleImageButton):
                        xButton.Style = FindResource("$CircleImageButtonStyle_Disabled") as Style;
                        break;
                    case (Core.eButtonType.PanelButton):
                        xButton.Style = FindResource("$PanelButtonStyle") as Style;

                        break;
                }
            }
            else
            {
                xButton.Style = mPreviousStyle;
            }
        }     
    }
}
