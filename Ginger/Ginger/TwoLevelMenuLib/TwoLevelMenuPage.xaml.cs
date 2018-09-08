using Amdocs.Ginger.Common;
using Amdocs.Ginger.UserControls;
using Ginger.TwoLevelMenuLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.GeneralWindows
{
    /// <summary>
    /// Interaction logic for MenusPage.xaml
    /// </summary>
    public partial class TwoLevelMenuPage : Page
    {
        TwoLevelMenu mTwoLevelMenu;
        
        public TwoLevelMenuPage(TwoLevelMenu twoLevelMenu)
        {
            InitializeComponent();
            mTwoLevelMenu = twoLevelMenu;
            LoadMenus();            
        }        

        public void Reset()
        {
            mTwoLevelMenu.Reset();
            xSelectedItemFrame.SetContent(null);            
            SelectFirstTopMenu();
        }

        private void LoadMenus()
        {            
            foreach(TopMenuItem menu in mTwoLevelMenu.MenuList)
            {
                xMainNavigationListView.Items.Add(menu);
            }
            SelectFirstTopMenu();            
        }

        private void SelectFirstTopMenu()
        {
            xMainNavigationListView.SelectedItem = xMainNavigationListView.Items[0];            
        }

        private void xMainNavigationListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TopMenuItem selectedMainListItem = (TopMenuItem)xMainNavigationListView.SelectedItem;
            SetSelectedListItemStyle(xMainNavigationListView);

            ObservableList<ListViewItem> subItems;
            // we cahce the sub items in the Content
            if (selectedMainListItem.LoadedSubItems == null)
            {
                subItems = LoadSubNavigationList(selectedMainListItem);
                selectedMainListItem.LoadedSubItems = subItems;
            }
            else
            {
                subItems = selectedMainListItem.LoadedSubItems;
            }

            xSubNavigationListView.Items.Clear();
            foreach (ListViewItem lvi in subItems)
            {
                xSubNavigationListView.Items.Add(lvi);
            }

            // get the user back to the same sub item he had before or select the first item
            if (subItems.CurrentItem == null)
            {
                // first time click auto select first sub menu item
                subItems.CurrentItem = subItems[0];
            }

            // Get the user back to the last sub item he had
            xSubNavigationListView.SelectedItem = subItems.CurrentItem;

            // if we have only one sub item no need to show sub menu
            if (xSubNavigationListView.Items.Count > 1)
            {
                xSubNavigationListView.Visibility = Visibility.Visible;
            }
            else
            {
                xSubNavigationListView.Visibility = Visibility.Collapsed;
            }
        }

        private ObservableList<ListViewItem> LoadSubNavigationList(TopMenuItem topMenuItem)
        {
            ObservableList<ListViewItem> list = new ObservableList<ListViewItem>();
            
            foreach (SubMenuItem subMenuItem in topMenuItem.SubItems)
            {
                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Content = subMenuItem.Name;
                listViewItem.Tag = subMenuItem;
                listViewItem.Style = (Style)TryFindResource("$SubListViewItemStyle");
                listViewItem.FontSize = 11;
                listViewItem.ToolTip = subMenuItem.ToolTip;
                listViewItem.SetValue(AutomationProperties.AutomationIdProperty, subMenuItem.AutomationID);
                list.Add(listViewItem);
                //TODO: enable visible
                //Add , Visibility = Visibility.Visible
            }
                       
            return list;
        }

        // static for reuse
        static LoadingPage loadingPage  = new LoadingPage();

        private void xSubNavigationListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = (ListView)sender;
            ListViewItem lvi = (ListViewItem)lv.SelectedItem;
            if (lvi == null)
            {
                xSelectedItemFrame.SetContent(null);
                return;
            }
            SubMenuItem subMenuItem = (SubMenuItem)lvi.Tag;

            if (!subMenuItem.IsPageLoaded)
            {
                // since the page might take time to load we show Loading, will happen with source control connected                
                xSelectedItemFrame.SetContent(loadingPage);                
                GingerCore.General.DoEvents();
            }

            if (subMenuItem != null && subMenuItem.ItemPage != null)
                xSelectedItemFrame.SetContent(subMenuItem.ItemPage);            
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {            
            // MessageBox.Show(e.Key.ToString());            
        }

        private void SetSelectedListItemStyle(ListView listView)
        {
            for (int i = 0; i < listView.Items.Count; i++)
            {
                ListViewItem listViewItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(i);
                if (listViewItem != null)
                {
                    StackPanel stack = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(listViewItem, 0), 0), 0) as StackPanel;
                    if (i == xMainNavigationListView.SelectedIndex)
                    {
                        ((ImageMakerControl)stack.Children[0]).Foreground = (Brush)Application.Current.Resources["$SelectionColor_Pink"];
                        ((Label)stack.Children[1]).Foreground = (Brush)Application.Current.Resources["$SelectionColor_Pink"];
                    }
                    else
                    {
                        ((ImageMakerControl)stack.Children[0]).Foreground = Brushes.DarkGray;
                        ((Label)stack.Children[1]).Foreground = Brushes.DarkGray;
                    }
                }
            }
        }

        private void xMainNavigationListView_Loaded(object sender, RoutedEventArgs e)
        {
            SetSelectedListItemStyle(xMainNavigationListView);
        }
    }
}
