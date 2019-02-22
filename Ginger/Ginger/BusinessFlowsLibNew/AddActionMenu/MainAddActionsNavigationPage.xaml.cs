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
    /// Interaction logic for MainAddActionsNavigationPage.xaml
    /// </summary>
    public partial class MainAddActionsNavigationPage : Page
    {
        public MainAddActionsNavigationPage()
        {
            InitializeComponent();
            this.ShowsNavigationUI = false;
        }

        private void XNavSharedRepo_Click(object sender, RoutedEventArgs e)
        {
            this.ShowsNavigationUI = true;
            this.NavigationService.Navigate(new SharedRepositoryNavAction());
        }

        private void XNavPOM_Click(object sender, RoutedEventArgs e)
        {
            this.ShowsNavigationUI = true;
            this.NavigationService.Navigate(new POMNavAction());
        }

        private void XRecord_Click(object sender, RoutedEventArgs e)
        {
            this.ShowsNavigationUI = true;
            this.NavigationService.Navigate(new RecordNavAction());
        }

        private void SHAddActionPanel_Click(object sender, RoutedEventArgs e)
        {
            AddActionsGrid.ColumnDefinitions[2].Width = AddActionsGrid.ColumnDefinitions[2].Width == GridLength.Auto ? new GridLength(0) : new GridLength(1, GridUnitType.Auto);
            //ActionSPanel.Visibility = ActionSPanel.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            //ActionSPanel.Width = ActionSPanel.Width == 0 ? 200 : 0;
            this.Width = this.Width == 60 ? 300 : 60;
        }

    }
}
