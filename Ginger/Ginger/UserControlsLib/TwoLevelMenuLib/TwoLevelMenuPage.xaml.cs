#region License
/*
Copyright © 2014-2025 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.External.Configurations;
using Amdocs.Ginger.UserControls;
using Ginger.TwoLevelMenuLib;
using GingerCoreNET.GeneralLib;
using System;
using System.Diagnostics;
using System.Windows;

using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.GeneralWindows
{
    /// <summary>
    /// Interaction logic for MenusPage.xaml
    /// </summary>
    public partial class TwoLevelMenuPage : Page
    {
        public readonly TwoLevelMenu mTwoLevelMenu;
        // static for reuse
        static LoadingPage loadingPage = new();

        TopMenuItem SelectedMainListItem
        {
            get
            {
                if (xMainNavigationListView != null && xMainNavigationListView.SelectedItem != null)
                {
                    return (TopMenuItem)xMainNavigationListView.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }

        SubMenuItem SelectedSubListItem
        {
            get
            {
                if (xSubNavigationListView != null && xSubNavigationListView.SelectedItem != null)
                {
                    return (SubMenuItem)xSubNavigationListView.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }

        public TwoLevelMenuPage(TwoLevelMenu twoLevelMenu)
        {
            InitializeComponent();
            mTwoLevelMenu = twoLevelMenu;
            LoadMenus();
        }

        public void Reset()
        {
            mTwoLevelMenu.Reset();
            xMainNavigationListView.SelectedItem = null;
            xSubNavigationListView.SelectedItem = null;
            xSubNavigationListView.Visibility = Visibility.Collapsed;
            xSelectedItemFrame.SetContent(null);

            xMainNavigationListView.Items.Clear();

            LoadMenus();
        }

        private void LoadMenus()
        {
            foreach (TopMenuItem menu in mTwoLevelMenu.MenuList)
            {
                if (!WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures && WorkSpace.Instance.Solution != null)
                {
                    if (menu.Name == WorkSpace.Instance.Solution.ExternalIntegrationsTabName)
                    {
                        continue;
                    }
                }
                xMainNavigationListView.Items.Add(menu);
            }
        }

        public void SelectFirstTopMenu()
        {
            xMainNavigationListView.SelectedItem = xMainNavigationListView.Items[0];
        }

        public void SelectTopMenu(int menuItemID)
        {
            xMainNavigationListView.SelectedItem = xMainNavigationListView.Items[menuItemID];
        }

        private void xMainNavigationListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedMainListItem == null)
            {
                xSelectedItemFrame.SetContent(null);
                return;
            }

            SetSelectedListItemStyle(xMainNavigationListView, (SolidColorBrush)FindResource("$PrimaryColor_Black"));

            ObservableList<SubMenuItem> subItems;
            subItems = SelectedMainListItem.SubItems;

            xSubNavigationListView.Items.Clear();
            foreach (SubMenuItem subItem in subItems)
            {
                xSubNavigationListView.Items.Add(subItem);
            }

            // if we have only one sub item no need to show sub menu
            if (xSubNavigationListView.Items.Count > 1)
            {
                xSubNavigationListView.Visibility = Visibility.Visible;
            }
            else
            {
                xSubNavigationListView.Visibility = Visibility.Collapsed;
            }

            // get the user back to the same sub item he had before or select the first item
            if (SelectedMainListItem.LastSubMenuItem == null)
            {
                // first time click auto select first sub menu item
                SelectedMainListItem.LastSubMenuItem = subItems[0];
            }
            // Get the user back to the last sub item he had
            if (xSubNavigationListView.SelectedItem != SelectedMainListItem.LastSubMenuItem)
            {
                xSubNavigationListView.SelectedItem = SelectedMainListItem.LastSubMenuItem;
            }
            else
            {
                if (subItems.Count > 1)
                {
                    SetSelectedListItemStyle(xSubNavigationListView, (SolidColorBrush)FindResource("$PrimaryColor_Black"));
                }
            }

        }

        private void xSubNavigationListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedSubListItem != null)
            {
                SelectedMainListItem.LastSubMenuItem = SelectedSubListItem;
            }

            if (SelectedSubListItem == null)
            {
                xSelectedItemFrame.SetContent(null);
                return;
            }

            if (!SelectedSubListItem.IsPageLoaded)
            {
                // since the page might take time to load we show Loading, will happen with source control connected                
                xSelectedItemFrame.SetContent(loadingPage);
                GingerCore.General.DoEvents();
            }

            if (SelectedSubListItem != null && SelectedSubListItem.ItemPage != null)
            {
                xSelectedItemFrame.SetContent(SelectedSubListItem.ItemPage);
            }

            if (xSubNavigationListView.Items.Count > 1)
            {
                SetSelectedListItemStyle(xSubNavigationListView, (SolidColorBrush)FindResource("$PrimaryColor_Black"));
            }
        }

        private void SetSelectedListItemStyle(ListView listView, Brush defualtForeground)
        {
            try
            {
                GingerCore.General.DoEvents();

                for (int i = 0; i < listView.Items.Count; i++)
                {
                    ListViewItem listViewItem = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(i);
                    if (listViewItem != null)
                    {
                        StackPanel stack = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(listViewItem, 0), 0), 0) as StackPanel;
                        if (i == listView.SelectedIndex)
                        {
                            ((ImageMakerControl)stack.Children[0]).Foreground = (Brush)Application.Current.Resources["$SelectionColor_Pink"];
                            ((Label)stack.Children[1]).Foreground = (Brush)Application.Current.Resources["$SelectionColor_Pink"];
                        }
                        else
                        {
                            ((ImageMakerControl)stack.Children[0]).Foreground = (Brush)Application.Current.Resources["$BackgroundColor_DarkGray"];
                            ((Label)stack.Children[1]).Foreground = defualtForeground;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Failed to Set Selected ListItem Style", ex);
            }
        }

        private void xMainNavigationListView_Loaded(object sender, RoutedEventArgs e)
        {
            SetSelectedListItemStyle(xMainNavigationListView, (SolidColorBrush)FindResource("$PrimaryColor_Black"));
        }

        private void xSubNavigationListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (xSubNavigationListView.Items.Count > 1 && xSubNavigationListView.IsVisible)
            {
                SetSelectedListItemStyle(xSubNavigationListView, (SolidColorBrush)FindResource("$PrimaryColor_Black"));
            }
        }

  
        private void ProFeatureButtonClick(object sender, RoutedEventArgs e)
        {
         
            if (GingerPlayUtils.IsGingerPlayEnabled() && !string.IsNullOrEmpty(GingerPlayUtils.GetGingerPlayGatewayURLIfConfigured()))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = GingerPlayUtils.GetGingerPlayGatewayURLIfConfigured() + "gingerplay/#/playHome",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to open the link: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Show the banner window if the gateway URL is not configured
                ShowGingerPlayBannerWindow();
            }
        }

        // Extracted banner window logic to a method for reuse
        private void ShowGingerPlayBannerWindow()
        {
            var imageUri = new Uri("pack://application:,,,/Ginger;component/UserControlsLib/ImageMakerLib/Images/GingerPlayDetailsPopUpContent.png", UriKind.Absolute);

            var image = new System.Windows.Controls.Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(imageUri),
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var viewbox = new Viewbox
            {
                Stretch = Stretch.Uniform,
                Child = image
            };

            var border = new Border
            {
                Child = viewbox,
                Background = Brushes.Transparent
            };

            border.MouseLeftButtonUp += (_, args) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://www.amdocs.com/products-services/quality-engineering-services",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to open the link: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            Window bannerWindow = new Window
            {
                Title = "Get to know Ginger Play",
                Width = 900,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ThreeDBorderWindow,
                ShowInTaskbar = false,
                Content = border,
                SizeToContent = SizeToContent.Manual,
                Owner = Application.Current.MainWindow
            };

            bannerWindow.ShowDialog();
        }
       
    }
}
