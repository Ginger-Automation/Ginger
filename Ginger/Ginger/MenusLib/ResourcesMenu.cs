using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.GeneralWindows;
using Ginger.GingerGridLib;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using Ginger.TwoLevelMenuLib;
using Ginger.Variables;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Variables;
using GingerWPF.ApplicationModelsLib.ModelParams_Pages;
using GingerWPF.PluginsLib;
using GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems;
using GingerWPF.TreeViewItemsLib.NewEnvironmentsTreeItems;
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
            if (e.PropertyName == nameof(UserProfile.Solution))
            {
                menusPage.Reset();
            }
        }

        private static TwoLevelMenu GetMenu()
        {
            TwoLevelMenu twoLevelMenu = new TwoLevelMenu();

            TopMenuItem SharedRepositoryMenu = new TopMenuItem(eImageType.SharedRepositoryItem, "Shared Repository", ConsoleKey.S, "Shared Repository AID", "Flow Elements which can be shared between multiple " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows));
            SharedRepositoryMenu.Add(GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.ActivitiesGroups), SharedActivitiesGroups, ConsoleKey.S, GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.ActivitiesGroups, "Shared "), "AID");
            SharedRepositoryMenu.Add(GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.Activities), SharedActivities, ConsoleKey.S, GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.Activities, "Shared "), "AID");
            SharedRepositoryMenu.Add("Actions", SharedActions, ConsoleKey.S, "Shared Actions", "AID");
            SharedRepositoryMenu.Add(GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.Variables), SharedVariables, ConsoleKey.S, GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.Variables, "Shared "), "AID");
            twoLevelMenu.Add(SharedRepositoryMenu);

            TopMenuItem ApplicationModelsMenu = new TopMenuItem(eImageType.APIModel16, "Applications Models", ConsoleKey.A, "Application Models AID", "Applications Layers Templates" );
            ApplicationModelsMenu.Add("API Models", APIModels, ConsoleKey.L, "API Templates Repository","AID");
            //TODO: bind visible to Beta feature or use WorkSpace.Instance.BetaFeatures.PropertyChanged
            // meanwhile show/hide per current status
            if (WorkSpace.Instance.BetaFeatures.ShowPOMInResourcesTab)            
                ApplicationModelsMenu.Add("Page Objects Models", POMModels, ConsoleKey.P, "Page UI elemetns Modeling", "AID");                            
            ApplicationModelsMenu.Add("Models Global Parameters", ModelsGlobalParameters, ConsoleKey.G, "Add or Edit Models Global Parameters", "AID");
            twoLevelMenu.Add(ApplicationModelsMenu);

            TopMenuItem environemntsMenu = new TopMenuItem(eImageType.Environment, "Environments", ConsoleKey.E, "Environemnts_AID", "Environments are been used for storing environment level parameters and DB/Unix connections details");
            environemntsMenu.Add("Environments", GetEnvsPage, ConsoleKey.L, "Environments are been used for storing environment level parameters and DB / Unix connections details", "Envs List");
            //environemntsMenu.Add("Compare", EnvsCompare, ConsoleKey.C, "Compare Environments", "compare Envs AID");
            twoLevelMenu.Add(environemntsMenu);

            TopMenuItem GlobalVariabelsMenu = new TopMenuItem(eImageType.Variable, GingerDicser.GetTermResValue(eTermResKey.Variables,"Global "), ConsoleKey.G, "Global Variables AID", GingerDicser.GetTermResValue(eTermResKey.Variables, suffixString:" which can be used cross " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows)));
            GlobalVariabelsMenu.Add("Grid", GetGlobalVariabelsPage, ConsoleKey.G, "", "AID");
            twoLevelMenu.Add(GlobalVariabelsMenu);

            TopMenuItem DocumentsMenu = new TopMenuItem(eImageType.File, "Documents", ConsoleKey.D, "Documents AID", "Solution documents like: text, excel, js scripts and any type of file");
            DocumentsMenu.Add("List", Documents, ConsoleKey.L, "", "AID");
            twoLevelMenu.Add(DocumentsMenu);

            TopMenuItem DataSourceMenu = new TopMenuItem(eImageType.DataSource, "Data Sources", ConsoleKey.D, "Data Sources AID", "Add and Edit Data Sources");
                DataSourceMenu.Add("List", DataSources, ConsoleKey.D, "", "AID");
                twoLevelMenu.Add(DataSourceMenu);

            TopMenuItem PluginsMenu = new TopMenuItem(eImageType.PluginPackage, "Plugins", ConsoleKey.P, "Plugins AID", "Ginger extention Add-ons");
            PluginsMenu.Add("Installed", PluginsList, ConsoleKey.L, "Installed Plugins", "Installed AID");           
            twoLevelMenu.Add(PluginsMenu);

            return twoLevelMenu;
        }

        private static Page GetGlobalVariabelsPage()
        {
            return (new VariablesPage(eVariablesLevel.Solution));
        }

        private static Page SharedActivitiesGroups()
        {
            SharedActivitiesGroupsFolderTreeItem activitiesGroupsRoot = new SharedActivitiesGroupsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ActivitiesGroup>());
            SingleItemTreeViewExplorerPage activitiesGroupsPage = new SingleItemTreeViewExplorerPage(GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.ActivitiesGroups), eImageType.ActivitiesGroup, activitiesGroupsRoot, saveAllHandler: activitiesGroupsRoot.SaveAllTreeFolderItemsHandler);
            return activitiesGroupsPage;
        }
        private static Page SharedActivities()
        {
            SharedActivitiesFolderTreeItem activitiesRoot = new SharedActivitiesFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Activity>());
            SingleItemTreeViewExplorerPage activitiesPage = new SingleItemTreeViewExplorerPage(GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.Activities), eImageType.Activity, activitiesRoot, saveAllHandler: activitiesRoot.SaveAllTreeFolderItemsHandler);
            return activitiesPage;
        }
        private static Page SharedActions()
        {
            SharedActionsFolderTreeItem actionsRoot = new SharedActionsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Act>());
            SingleItemTreeViewExplorerPage actionsPage = new SingleItemTreeViewExplorerPage("Actions", eImageType.Action, actionsRoot, actionsRoot.SaveAllTreeFolderItemsHandler);
            return actionsPage;
        }
        private static Page SharedVariables()
        {
            SharedVariablesFolderTreeItem variablesRoot = new SharedVariablesFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<VariableBase>());
            SingleItemTreeViewExplorerPage variablesPage = new SingleItemTreeViewExplorerPage(GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.Variables), eImageType.Variable, variablesRoot, variablesRoot.SaveAllTreeFolderItemsHandler);
            return variablesPage;
        }

        private static Page GetEnvsPage()
        {
            EnvironmentsFolderTreeItem EnvsRoot = new EnvironmentsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ProjEnvironment>());
            SingleItemTreeViewExplorerPage p = new SingleItemTreeViewExplorerPage("Environments", eImageType.Environment, EnvsRoot, EnvsRoot.SaveAllTreeFolderItemsHandler, EnvsRoot.AddItemHandler);
            EnvsRoot.IsGingerDefualtFolder = true;
            return p;
        }
        private static Page EnvsCompare()
        {
            return new Page() { Content = "Env Compare coming soon..." };
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


    }
}
