using System.Windows;

namespace Ginger.ReporterLib
{
    /// <summary>
    /// Interaction logic for MessageBoxWindow.xaml
    /// </summary>
    public partial class MessageBoxWindow : Window
    {

        public Amdocs.Ginger.Common.MessageBoxResult messageBoxResult { get; set; }

        
        public MessageBoxWindow(string txt, string caption, Amdocs.Ginger.Common.MessageBoxButton buttonsType,
                                            GingerCoreNET.ReporterLib.MessageBoxImage messageImage, Amdocs.Ginger.Common.MessageBoxResult defualtResualt)
        {
            InitializeComponent();

            xMessageTextLabel.Content = txt;
            this.Title = caption;
            messageBoxResult = defualtResualt;

            xOKButton.Visibility = Visibility.Collapsed;
            xYesButton.Visibility = Visibility.Collapsed;
            xNoButton.Visibility = Visibility.Collapsed;
            xCancelButton.Visibility = Visibility.Collapsed;

            switch (buttonsType)
            {
                case Amdocs.Ginger.Common.MessageBoxButton.OK:
                       xOKButton.Visibility = Visibility.Visible;
                    break;
                case Amdocs.Ginger.Common.MessageBoxButton.OKCancel:
                    xOKButton.Visibility = Visibility.Visible;
                    xCancelButton.Visibility = Visibility.Visible;
                    break;
                case Amdocs.Ginger.Common.MessageBoxButton.YesNo:
                    xYesButton.Visibility = Visibility.Visible; ;
                    xNoButton.Visibility = Visibility.Visible;
                    break;
                case Amdocs.Ginger.Common.MessageBoxButton.YesNoCancel:
                    xYesButton.Visibility = Visibility.Visible; ;
                    xNoButton.Visibility = Visibility.Visible;
                    xCancelButton.Visibility = Visibility.Visible;
                    break;
            }

        }
        
      
        private void xOKButton_Click(object sender, RoutedEventArgs e)
        {
            messageBoxResult = Amdocs.Ginger.Common.MessageBoxResult.OK;
            this.Close();
        }


        private void XYesButton_Click(object sender, RoutedEventArgs e)
        {
            messageBoxResult = Amdocs.Ginger.Common.MessageBoxResult.Yes;
            this.Close();
        }

        private void XNoButton_Click(object sender, RoutedEventArgs e)
        {
            messageBoxResult = Amdocs.Ginger.Common.MessageBoxResult.No;
            this.Close();
        }

        private void XCancelButton_Click(object sender, RoutedEventArgs e)
        {
            messageBoxResult = Amdocs.Ginger.Common.MessageBoxResult.Cancel;
            this.Close();
        }
    }
}
