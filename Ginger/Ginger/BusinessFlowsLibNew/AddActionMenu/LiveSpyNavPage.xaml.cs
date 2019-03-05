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

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for LiveSpyNavAction.xaml
    /// </summary>
    public partial class LiveSpyNavPage : Page
    {
        bool isSpying = false;
        public LiveSpyNavPage()
        {
            InitializeComponent();
        }
        private void SpyingButton_Click(object sender, RoutedEventArgs e)
        {
            isSpying = !isSpying;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (isSpying)
            {
                xSpyingButton.ToolTip = "Stop Spying";
                xSpyingButton.Background = Brushes.White;
                lblRecording.Foreground = Brushes.DeepSkyBlue;
                lblRecording.Content = "Stop Spying";
                xSpyingButton.BorderThickness = new Thickness(2);
                xSpyingButton.BorderBrush = Brushes.DeepSkyBlue;
                //                xSpyingButton.Background = Brushes.White;
            }
            else
            {
                xSpyingButton.ToolTip = "Start Spying";
                xSpyingButton.Background = Brushes.DeepSkyBlue;
                lblRecording.Foreground = Brushes.White;
                lblRecording.Content = "Start Spying";
            }
        }

        private void WindowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void RefreshWindowsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddSwitchWindowActionButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
