using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.MenusLib;
using Ginger.Repository;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using Ginger.WindowExplorer;
using GingerCore;
using GingerCore.Actions;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.Platforms;
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
        IWindowExplorer WindowExplorerDriver;
        Context mContext;
        public MainAddActionsNavigationPage(Context context)
        {
            mContext = context;
            InitializeComponent();
            navigationBar.Visibility = Visibility.Collapsed;
            navPnlActionFrame.ContentRendered += NavPnlActionFrame_ContentRendered;
        }

        private void NavPnlActionFrame_ContentRendered(object sender, EventArgs e)
        {
            if ((sender as Frame).Content == null)
            {
                (sender as Frame).Visibility = Visibility.Collapsed;
                navigationBar.Visibility = Visibility.Collapsed;
                navHomeStackPnl.Visibility = Visibility.Visible;
            }
            else
            {
                (sender as Frame).Visibility = Visibility.Visible;
                navigationBar.Visibility = Visibility.Visible;
                navHomeStackPnl.Visibility = Visibility.Collapsed;
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

        private void UcButton_Click(object sender, RoutedEventArgs e)
        {
            LoadActionFrame(null);
        }

        private void LoadActionFrame(Page navigationPage, string titleText = "", eImageType titleImage = eImageType.Empty)
        {
            navPnlActionFrame.Content = navigationPage;
            xTitleImage.ImageType = titleImage;
            xTitleText.Content = titleText;
            //            this.NavigationService.GoBack();
        }
    }
}
