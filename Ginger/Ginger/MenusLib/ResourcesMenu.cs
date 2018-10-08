using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.GeneralWindows;
using Ginger.GingerGridLib;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using Ginger.TwoLevelMenuLib;
using GingerCore.DataSource;
using GingerWPF.ApplicationModelsLib.ModelParams_Pages;
using GingerWPF.PluginsLib;
using GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems;
using GingerWPF.UserControlsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.MenusLib
{
    public static class ResourcesMenu
    {
        public static TwoLevelMenu twoLevelMenu;

        private static TwoLevelMenuPage mMenusPage = null;
        public static TwoLevelMenuPage menusPage
        {
            get
            {
                if (mMenusPage == null)
                {
                    mMenusPage = new TwoLevelMenuPage(GetMenu());
                    App.UserProfile.PropertyChanged += UserProfile_PropertyChanged;
                }
                return mMenusPage;
            }
        }

        private static void UserProfile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Solution")
            {
                menusPage.Reset();
            }
        }

        private static TwoLevelMenu GetMenu()
        {
            TwoLevelMenu twoLevelMenu = new TwoLevelMenu();
            TopMenuItem ApplicationModelsMenu = new TopMenuItem("Application Models", ConsoleKey.A, "Application Models AID");
            ApplicationModelsMenu.Add("API Models", APIModels, ConsoleKey.L, "Applications Web Service Requests Templates Repository","AID");

            //TODO: bind visible to Beta feature or use WorkSpace.Instance.BetaFeatures.PropertyChanged
            // meanwhile show/hide per current status
            if (WorkSpace.Instance.BetaFeatures.ShowPOMInResourcesTab)
            {
                ApplicationModelsMenu.Add("Page Objects Models", POMModels, ConsoleKey.P, "Page Object Modeling", "AID");                
            }
            ApplicationModelsMenu.Add("Models Global Parameters", ModelsGlobalParameters, ConsoleKey.G, "Add or Edit Models Global Parameters", "AID");
            twoLevelMenu.Add(ApplicationModelsMenu);

            // TODO: use visible show/hide instead of not adding
            if (App.UserProfile.UserTypeHelper.IsSupportAutomate)
            {
                TopMenuItem DataSourceMenu = new TopMenuItem("Data Sources", ConsoleKey.D, "Data Sources AID");
                DataSourceMenu.Add("List", DataSources, ConsoleKey.D, "Add and Edit data source", "AID");
                twoLevelMenu.Add(DataSourceMenu);
            }

            TopMenuItem DocumentsMenu = new TopMenuItem("Documents", ConsoleKey.D, "Documents AID");
            DocumentsMenu.Add("List", Documents, ConsoleKey.L, "Solution documents like: text, excel, js scripts and any type of file", "AID");
            twoLevelMenu.Add(DocumentsMenu);

            TopMenuItem PluginsMenu = new TopMenuItem("Plugins", ConsoleKey.P, "Plugins AID");
            PluginsMenu.Add("Installed", PluginsList, ConsoleKey.L, "Installed Plugins", "Installed AID");
            PluginsMenu.Add("GingerGrid", GingerGrid, ConsoleKey.G, "Ginger Grid", "Ginger Grid AID");
            twoLevelMenu.Add(PluginsMenu);

            return twoLevelMenu;
        }

        
        private static Page Documents()
        {
            DocumentsFolderTreeItem documentsFolderRoot = new DocumentsFolderTreeItem();
            documentsFolderRoot.IsGingerDefualtFolder = true;
            documentsFolderRoot.Path = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, "Documents");
            documentsFolderRoot.Folder = "Documents";
            SingleItemTreeViewExplorerPage dataSourcesRootPage = new SingleItemTreeViewExplorerPage("Documents", eImageType.File, documentsFolderRoot, saveAllHandler:null, addHandler:null);
            return dataSourcesRootPage;
        }

        private static Page DataSources()
        {
            DataSourceFolderTreeItem dataSourcesRoot = new DataSourceFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<DataSourceBase>());
            dataSourcesRoot.IsGingerDefualtFolder = true;
            SingleItemTreeViewExplorerPage dataSourcesRootPage = new SingleItemTreeViewExplorerPage("Data Sources", eImageType.DataSource, dataSourcesRoot, dataSourcesRoot.SaveAllTreeFolderItemsHandler, dataSourcesRoot.AddDataSource);
            return dataSourcesRootPage;            
        }

        private static Page ModelsGlobalParameters()
        {
            return new ModelsGlobalParamsPage();            
        }

        private static Page POMModels()
        {
            ApplicationPOMsTreeItem POMsRoot = new ApplicationPOMsTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>());
            SingleItemTreeViewExplorerPage pomModelPage = new SingleItemTreeViewExplorerPage("POM", eImageType.Application, POMsRoot, POMsRoot.SaveAllTreeFolderItemsHandler, POMsRoot.AddPOM);
            return pomModelPage;
        }

        private static Page APIModels()
        {
            AppApiModelsFolderTreeItem apiRoot = new AppApiModelsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationAPIModel>());
            SingleItemTreeViewExplorerPage apiModelPage = new SingleItemTreeViewExplorerPage("API Models", eImageType.APIModel32, apiRoot, apiRoot.SaveAllTreeFolderItemsHandler, apiRoot.AddAPIModelFromDocument);
            return apiModelPage;
        }

        private static Page PluginsList()
        {            
            PlugInsFolderTreeItem pluginsFolderTreeItem = new PlugInsFolderTreeItem();
            pluginsFolderTreeItem.IsGingerDefualtFolder = true;
            pluginsFolderTreeItem.Path = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, "Plugins");
            pluginsFolderTreeItem.Folder = "Plugins";
            SingleItemTreeViewExplorerPage PluginsRootPage = new SingleItemTreeViewExplorerPage("Plugins", eImageType.PluginPackage , pluginsFolderTreeItem, saveAllHandler: null, addHandler: pluginsFolderTreeItem.AddPlugIn);
            return PluginsRootPage;
        }

        private static Page GingerGrid()
        {
            return new GingerGridPage(WorkSpace.Instance.LocalGingerGrid);
        }
    }
}
