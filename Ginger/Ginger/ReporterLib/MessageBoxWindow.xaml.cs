using System.Windows;
using Amdocs.Ginger.Common;

namespace Ginger.ReporterLib
{
    /// <summary>
    /// Interaction logic for MessageBoxWindow.xaml
    /// </summary>
    public partial class MessageBoxWindow : Window
    {

        public Amdocs.Ginger.Common.eUserMsgSelection messageBoxResult { get; set; }

        
        public MessageBoxWindow(string txt, string caption, eUserMsgOption buttonsType,
                                            eUserMsgIcon messageImage, eUserMsgSelection defualtResualt)
        {
            InitializeComponent();

            xMessageTextLabelBlock.Text = txt;
            this.Title = caption;
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
            }

            switch (messageImage)
            {
                case eUserMsgIcon.Error:
                    xImage.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Error;
                    break;
                case eUserMsgIcon.Information:
                    xImage.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Info;
                    break;
                case eUserMsgIcon.None:
                    xImage.Visibility = Visibility.Collapsed;
                    break;
                case eUserMsgIcon.Question:
                    xImage.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Question;
                    break;
                case eUserMsgIcon.Warning:
                    xImage.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Info; //FIXME !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
