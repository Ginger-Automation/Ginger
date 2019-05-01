using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for MainAddActionsNavigationPage.xaml
    /// </summary>
    public partial class MainAddActionsNavigationPage : Page
    {
        IWindowExplorer WindowExplorerDriver;
        Context mContext;
        public MainAddActionsNavigationPage(Context context)
        {
            mContext = context;
            InitializeComponent();
            xNavigationBarPnl.Visibility = Visibility.Collapsed;
            xSelectedItemFrame.ContentRendered += NavPnlActionFrame_ContentRendered;
        }

        private void NavPnlActionFrame_ContentRendered(object sender, EventArgs e)
        {
            if ((sender as Frame).Content == null)
            {
                (sender as Frame).Visibility = Visibility.Collapsed;
                xNavigationBarPnl.Visibility = Visibility.Collapsed;
                xAddActionsOptionsPnl.Visibility = Visibility.Visible;
            }
            else
            {
                (sender as Frame).Visibility = Visibility.Visible;
                xNavigationBarPnl.Visibility = Visibility.Visible;
                xAddActionsOptionsPnl.Visibility = Visibility.Collapsed;
                //navBarTitle.Content = (navPnlActionFrame.Content as Page).Title;
            }
        }

        private void XNavSharedRepo_Click(object sender, RoutedEventArgs e)
        {
            //LoadActionFrame(new RepositoryPage(mContext.BusinessFlow));
            LoadActionFrame(new SharedRepositoryNavPage(mContext), "Shared Repository", eImageType.SharedRepositoryItem); // WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Act>()));
        }

        private void XNavPOM_Click(object sender, RoutedEventArgs e)
        {
            ApplicationPOMsTreeItem POMsRoot = new ApplicationPOMsTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>());
            LoadActionFrame(new POMNavPage("Page Objects Models", eImageType.Application, POMsRoot, POMsRoot.SaveAllTreeFolderItemsHandler, POMsRoot.AddPOM), "Page Objects Model", eImageType.ApplicationPOMModel);
        }

        private void XRecord_Click(object sender, RoutedEventArgs e)
        {
            LoadActionFrame(new RecordNavPage(mContext), "Record", eImageType.Camera);
        }

        private void XNavActLib_Click(object sender, RoutedEventArgs e)
        {
            LoadActionFrame(new ActionsLibraryNavPage(mContext), "Actions Library", eImageType.Action);
        }

        private void XNavSpy_Click(object sender, RoutedEventArgs e)
        {
            LoadActionFrame(new LiveSpyNavPage(mContext), "Live Spy", eImageType.Spy);
        }

        private void XNavWinExp_Click(object sender, RoutedEventArgs e)
        {
            LoadActionFrame(new WindowsExplorerNavPage(mContext), "Windows Explorer", eImageType.Search);
        }

        private void xGoBackBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadActionFrame(null);
        }

        private void LoadActionFrame(Page navigationPage, string titleText = "", eImageType titleImage = eImageType.Empty)
        {            
            xSelectedItemFrame.Content = navigationPage;

            if (navigationPage != null)
            {
                xSelectedItemTitlePnl.Visibility = Visibility.Visible;
                xSelectedItemTitleImage.ImageType = titleImage;
                xSelectedItemTitleText.Content = titleText;
            }
            else
            {
                xSelectedItemTitlePnl.Visibility = Visibility.Collapsed;
            }
        }
    }
}
