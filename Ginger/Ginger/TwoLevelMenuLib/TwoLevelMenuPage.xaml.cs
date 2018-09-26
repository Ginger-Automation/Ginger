using Amdocs.Ginger.Common;
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
                ListViewItem topMenu = new ListViewItem() {Content = menu.Name };
                topMenu.Style = (Style)TryFindResource("$ListViewItemStyle");
                topMenu.SetValue(AutomationProperties.AutomationIdProperty, menu.AutomationID);
                xMainNavigationListView.Items.Add(topMenu);
            }
            SelectFirstTopMenu();            
        }

        private void SelectFirstTopMenu()
        {
            xMainNavigationListView.SelectedItem = xMainNavigationListView.Items[0];
            UpdateMainFrame();
        }

        private void xMainNavigationListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMainFrame();         
        }

        private void UpdateMainFrame()
        {
            ListViewItem mainListItem = (ListViewItem)xMainNavigationListView.SelectedItem;
            ObservableList<ListViewItem> subItems;
            // we cahce the sub items in the Tag
            if (mainListItem.Tag == null)
            {
                TopMenuItem topMenuItem = (from x in mTwoLevelMenu.MenuList where x.Name == mainListItem.Content.ToString() select x).SingleOrDefault();
                subItems = LoadSubNavigationList(topMenuItem);
                mainListItem.Tag = subItems;
            }
            else
            {
                subItems = (ObservableList<ListViewItem>)mainListItem.Tag;
            }
            xSubNavigationListView.Items.Clear();
            foreach (ListViewItem lvi in subItems)
            {
                lvi.Style = (Style)TryFindResource("$ListViewItemStyle");
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



            // if we have one sub item no need to show sub menu
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
                listViewItem.Style = (Style)TryFindResource("$ListViewItemStyle");
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

            xSelectedItemFrame.SetContent(subMenuItem.ItemPage);            
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {            
            
        }
    }
}
