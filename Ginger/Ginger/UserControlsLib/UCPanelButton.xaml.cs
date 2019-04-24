using Amdocs.Ginger.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for UCPanelButton.xaml
    /// </summary>
    public partial class UCPanelButton : UserControl
    {
        public UCPanelButton()
        {
            InitializeComponent();
            SetButtonDetails();
        }

        public static readonly DependencyProperty ButtonImageTypeProperty = DependencyProperty.Register("ButtonImageType", typeof(eImageType), typeof(UCPanelButton),
                      new FrameworkPropertyMetadata(OnIconPropertyChanged));

        private static void OnIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UCPanelButton ub = d as UCPanelButton;
            ub.SetButtonDetails();
        }

        public eImageType ButtonImageType
        {
            get { return (eImageType)GetValue(ButtonImageTypeProperty); }
            set
            {
                SetValue(ButtonImageTypeProperty, value);
            }
        }

        public string ButtonText
        {
            get { return xButtonLbl.Content.ToString(); }
            set { xButtonLbl.Content = value; }
        }

        public SolidColorBrush ButtonImageForground
        {
            get { return xButtonImage.ImageForeground; }
            set { xButtonImage.ImageForeground = value; }
        }

        private void SetButtonDetails()
        {
            xButtonImage.ImageType = ButtonImageType;
            //throw new NotImplementedException();
        }

        public event RoutedEventHandler Click;
        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || e.ClickCount >= 2)
            {
                e.Handled = true;
            }
            else
            {
                Click(this, e);
            }
        }

        private void XPanelButton_Click(object sender, RoutedEventArgs e)
        {
                Click(this, e);
        }
    }
}
