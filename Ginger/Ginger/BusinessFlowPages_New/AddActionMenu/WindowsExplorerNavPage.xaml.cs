using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Drivers.Common;
using Ginger.WindowExplorer;
using Ginger.WindowExplorer.Common;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.UserControlsLib.UCTreeView;
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
    /// Interaction logic for WindowsExplorerNavPage.xaml
    /// </summary>
    public partial class WindowsExplorerNavPage : Page
    {
        Context mContext;
        ITreeViewItem mCurrentControlTreeViewItem;
        Page mControlFrameContentPage = null;
        bool mFirstElementSelectionDone = false;
        private Act mAction;
        ITreeViewItem mRootItem;
        ObservableList<ElementInfo> VisibleElementsInfoList = new ObservableList<ElementInfo>();

        public WindowsExplorerNavPage(Context context)
        {
            InitializeComponent();
            mContext = context;
            xWinGridUC.mContext = mContext;

            WindowControlsTreeView.TreeGrid.RowDefinitions[0].Height = new GridLength(0);

            WindowControlsTreeView.SearchStarted += (object sender,EventArgs e) => StatusTextBlock.Text = "Searching...";
            WindowControlsTreeView.SearchCancelled += (object sender, EventArgs e) => StatusTextBlock.Text = "Ready";
            WindowControlsTreeView.SearchCompleted += (object sender, EventArgs e) => StatusTextBlock.Text = "Ready";

            xWinGridUC.WindowsComboBox.SelectionChanged += WindowsComboBox_SelectionChanged;
        }

        private void WindowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WindowControlsGridView.Visibility == System.Windows.Visibility.Visible)
            {
                RefreshControlsGrid();
            }
        }

        private void ControlsViewsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            ControlsViewRow.Height = new GridLength(35);
            ControlsViewRow.MaxHeight = 35;
        }

        private void ControlsViewsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            ControlsViewRow.Height = new GridLength(200, GridUnitType.Star);
            ControlsViewRow.MaxHeight = Double.PositiveInfinity;
        }

        private void ControlsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            ElementInfo EI = WindowControlsGridView.CurrentItem as ElementInfo;
            mCurrentControlTreeViewItem = WindowExplorerCommon.GetTreeViewItemForElementInfo(EI);
            ShowCurrentControlInfo();
        }

        private void ShowCurrentControlInfo()
        {
            if (mCurrentControlTreeViewItem == null) return;
            ElementInfo EI = (ElementInfo)mCurrentControlTreeViewItem.NodeObject();
            try
            {
                if (xWinGridUC.mWindowExplorerDriver.IsElementObjectValid(EI.ElementObject))
                {
                    EI.WindowExplorer = xWinGridUC.mWindowExplorerDriver;
                    xWinGridUC.mWindowExplorerDriver.HighLightElement(EI);

                    //General tab will show the generic element info page, customized page will be in Data tab
                    mControlFrameContentPage = new ElementInfoPage(EI);
                    ControlFrame.Content = mControlFrameContentPage;
                    SetDetailsExpanderDesign(true, EI);
                    if (mCurrentControlTreeViewItem is IWindowExplorerTreeItem)
                    {
                        ControlPropertiesGrid.DataSourceList = ((IWindowExplorerTreeItem)mCurrentControlTreeViewItem).GetElementProperties();
                        ControlPropertiesGrid.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        ControlPropertiesGrid.Visibility = System.Windows.Visibility.Collapsed;
                    }

                    ShowControlActions(mCurrentControlTreeViewItem);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.ObjectUnavailable);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception in ShowCurrentControlInfo", ex);
                Reporter.ToUser(eUserMsgKey.ObjectLoad);
            }
        }

        private void SetDetailsExpanderDesign(bool detailsExist, ElementInfo selectedElementInfo)
        {
            if (detailsExist == false)
            {
                SelectedControlDetailsExpanderLable.Content = "Selected Element Details & Actions";
                SelectedControlDetailsExpanderLable.Foreground = Brushes.Gray;
                SelectedControlDetailsExpander.IsEnabled = false;
                SelectedControlDetailsExpander.IsExpanded = false;
            }
            else
            {
                SelectedControlDetailsExpanderLable.Content = "'" + selectedElementInfo.ElementTitle + "' Element Details & Actions";
                SelectedControlDetailsExpanderLable.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$BackgroundColor_LightGray")).ToString()); ;
                SelectedControlDetailsExpander.IsEnabled = true;
                if (mFirstElementSelectionDone == false)
                {
                    SelectedControlDetailsExpander.IsExpanded = true;
                    mFirstElementSelectionDone = true;
                }
            }
        }

        private void ShowControlActions(ITreeViewItem iv)
        {
            //We show action if we didn't came from Edit page
            if (iv is IWindowExplorerTreeItem)
            {
                ElementInfo EI = (ElementInfo)iv.NodeObject();

                ControlActionsPage CAP = null;
                // We came from Action EditPage
                if (mAction != null)
                {
                    CAP = new ControlActionsPage(mAction, EI);
                }
                else
                {
                    ObservableList<Act> list = new ObservableList<Act>();
                    ObservableList<ActInputValue> actInputValuelist = new ObservableList<ActInputValue>();
                    if (xWinGridUC.mPlatform.PlatformType() == GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Web)
                    {
                        list = xWinGridUC.mPlatform.GetPlatformElementActions(EI);
                    }
                    else
                    {                                                               // this "else" is temporary. Currently only ePlatformType.Web is overided
                        list = ((IWindowExplorerTreeItem)iv).GetElementActions();   // case will be removed once all platforms will be overrided
                    }                                                               //

                    //If no element actions returned then no need to get locator's. 
                    if (list == null || list.Count == 0)
                    {
                        SetActionsTabDesign(false);
                        return;
                    }
                    else
                    {
                        Page DataPage = mCurrentControlTreeViewItem.EditPage();
                        actInputValuelist = ((IWindowExplorerTreeItem)iv).GetItemSpecificActionInputValues();
                        CAP = new ControlActionsPage(xWinGridUC.mWindowExplorerDriver, EI, list, DataPage, actInputValuelist, mContext);
                    }
                }
                ControlActionsFrame.Content = CAP;
                SetActionsTabDesign(true);
            }
            else
            {
                SetActionsTabDesign(false);
            }
        }

        private void SetActionsTabDesign(bool actionsExist)
        {
            if (actionsExist == false)
            {
                ActionsTab.IsEnabled = false;
                ControlActionsFrame.Visibility = System.Windows.Visibility.Collapsed;
                GeneralTab.IsSelected = true;
            }
            else
            {
                ActionsTab.IsEnabled = true;
                ActionsTab.IsSelected = true;
                ControlActionsFrame.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void ControlsRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = "Searching Elements";
            GingerCore.General.DoEvents();

            if (WindowControlsTreeView.Visibility == System.Windows.Visibility.Visible)
                RefreshTreeControls();
            //else
            //    RefreshFilteredElements();
            StatusTextBlock.Text = "Ready";
        }

        private void RefreshTreeControls()
        {
            WindowControlsTreeView.Tree.ClearTreeItems();
            if (xWinGridUC.comboBoxSelectedValue != null && mRootItem != null)
            {
                TreeViewItem TVI = WindowControlsTreeView.Tree.AddItem(mRootItem);
                TVI.IsExpanded = false;
            }
        }

        private void ControlTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SyncWithLiveSpyButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();

            if (((Button)sender).ToolTip.ToString().Contains("Cancel") == true)
            {
                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@WithoutSpyWhite_24x24.png"));
                SyncWithLiveSpyButton.Content = image;
                SyncWithLiveSpyButton.ToolTip = "Click to Locate Live Spy Found Element";

                //mSyncControlsViewWithLiveSpy = false;
            }
            else
            {
                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@WithSpyWhite_24x24.png"));
                SyncWithLiveSpyButton.Content = image;
                SyncWithLiveSpyButton.ToolTip = "Click to Cancel Locate Live Spy Found Element";

                //mSyncControlsViewWithLiveSpy = true;
                //FocusSpyItemOnControlTree();//try to locate last find element by live spy
            }
        }

        private void GridTreeViewButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();

            if (((Button)sender).ToolTip.ToString().Contains("Tree") == true)
            {
                //switch to tree view
                WindowControlsTreeView.Visibility = System.Windows.Visibility.Visible;
                WindowControlsGridView.Visibility = System.Windows.Visibility.Collapsed;

                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Grid_24x24.png"));
                GridTreeViewButton.Content = image;
                GridTreeViewButton.ToolTip = "Switch to Grid View";
            }
            else
            {
                //switch to grid view
                WindowControlsTreeView.Visibility = System.Windows.Visibility.Collapsed;
                WindowControlsGridView.Visibility = System.Windows.Visibility.Visible;
                if (WindowControlsGridView.DataSourceList == null || WindowControlsGridView.DataSourceList.Count == 0)
                    ShowFilterElementsPage();

                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@TreeView_24x24.png"));
                GridTreeViewButton.Content = image;
                GridTreeViewButton.ToolTip = "Switch to Tree View";
            }
        }

        private void SelectedControlDetailsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            if (mControlFrameContentPage == null)
            {
                SelectedControlDetailsExpander.IsExpanded = false;
                return;
            }
            ControlDetailsRow.Height = new GridLength(200, GridUnitType.Star);
            ControlDetailsRow.MaxHeight = Double.PositiveInfinity;
        }

        private void SelectedControlDetailsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            ControlDetailsRow.Height = new GridLength(35);
            ControlDetailsRow.MaxHeight = 35;
        }

        ObservableList<UIElementFilter> CheckedFilteringCreteriaList = new ObservableList<UIElementFilter>();
        ObservableList<UIElementFilter> FilteringCreteriaList = new ObservableList<UIElementFilter>();

        private void ShowFilterElementsPage()
        {
            if (FilteringCreteriaList.Count == 0)
                SetAutoMapElementTypes();
            if (FilteringCreteriaList.Count != 0)
            {
                CheckedFilteringCreteriaList = new ObservableList<UIElementFilter>();
                FilterElementsPage FEW = new FilterElementsPage(FilteringCreteriaList, CheckedFilteringCreteriaList, null, this);
                FEW.ShowAsWindow(eWindowShowStyle.Dialog);

                foreach (UIElementFilter filter in FilteringCreteriaList)
                    if (filter.Selected)
                    {
                        if (!CheckedFilteringCreteriaList.Contains(filter))
                            CheckedFilteringCreteriaList.Add(filter);
                    }
            }
        }
        private void SetAutoMapElementTypes()
        {
            List<eElementType> UIElementsTypeList = null;
            switch (xWinGridUC.mPlatform.PlatformType())
            {
                case ePlatformType.Web:
                    WebPlatform webPlatformInfo = new WebPlatform();
                    UIElementsTypeList = webPlatformInfo.GetPlatformUIElementsType();
                    break;
            }
            if (UIElementsTypeList != null)
            {
                foreach (eElementType eET in UIElementsTypeList)
                {
                    FilteringCreteriaList.Add(new UIElementFilter(eET, string.Empty));
                }
            }
        }

        public bool DoSearchControls()
        {
            bool isSearched = false;
            foreach (UIElementFilter filter in FilteringCreteriaList)
                if (filter.Selected)
                {
                    if (!CheckedFilteringCreteriaList.Contains(filter))
                        CheckedFilteringCreteriaList.Add(filter);
                }

            StatusTextBlock.Text = "Searching Elements...";
            GingerCore.General.DoEvents();

            isSearched = RefreshFilteredElements();

            return isSearched;
        }

        private bool RefreshFilteredElements()
        {
            if (FilteringCreteriaList.Count != 0)
            {
                if (CheckedFilteringCreteriaList.Count == 0)
                {
                    Amdocs.Ginger.Common.eUserMsgSelection result = Reporter.ToUser(eUserMsgKey.FilterNotBeenSet);
                    if (result == Amdocs.Ginger.Common.eUserMsgSelection.OK)
                    {
                        RefreshControlsGrid();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    RefreshControlsGrid();
                    return true;
                }
            }
            else
            {
                Amdocs.Ginger.Common.eUserMsgSelection result = Reporter.ToUser(eUserMsgKey.RetreivingAllElements);
                if (result == Amdocs.Ginger.Common.eUserMsgSelection.OK)
                {
                    RefreshControlsGrid();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private async void RefreshControlsGrid()
        {
            if (xWinGridUC.comboBoxSelectedValue != null && xWinGridUC.mWindowExplorerDriver != null)
            {
                List<ElementInfo> list = await Task.Run(() => xWinGridUC.mWindowExplorerDriver.GetVisibleControls(CheckedFilteringCreteriaList.Select(x => x.ElementType).ToList()));

                StatusTextBlock.Text = "Ready";
                // Convert to obserable for the grid
                VisibleElementsInfoList.Clear();
                foreach (ElementInfo EI in list)
                {
                    VisibleElementsInfoList.Add(EI);
                }

                WindowControlsGridView.DataSourceList = VisibleElementsInfoList;

            }
        }

    }
}
