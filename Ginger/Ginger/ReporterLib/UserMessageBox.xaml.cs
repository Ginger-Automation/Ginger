using System.Windows;
using System.Windows.Media;
using Amdocs.Ginger.Common;

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
                case Amdocs.Ginger.Common.eUserMsgOption.OK:
                    xOKButton.Visibility = Visibility.Visible;
                    break;
                case Amdocs.Ginger.Common.eUserMsgOption.OKCancel:
                    xOKButton.Visibility = Visibility.Visible;
                    xCancelButton.Visibility = Visibility.Visible;
                    break;
                case Amdocs.Ginger.Common.eUserMsgOption.YesNo:
                    xYesButton.Visibility = Visibility.Visible; ;
                    xNoButton.Visibility = Visibility.Visible;
                    break;
                case Amdocs.Ginger.Common.eUserMsgOption.YesNoCancel:
                    xYesButton.Visibility = Visibility.Visible; ;
                    xNoButton.Visibility = Visibility.Visible;
                    xCancelButton.Visibility = Visibility.Visible;
                    break;
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
                case eUserMsgIcon.Information:
                    xMessageImage.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Info;
                    xMessageImage.ImageForeground = Brushes.DarkBlue;
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
            }

        }
              
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
    }
}
