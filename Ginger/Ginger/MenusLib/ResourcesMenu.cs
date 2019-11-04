#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowPages;
using Ginger.GeneralWindows;
using Ginger.PlugInsWindows;
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
using GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems;
using GingerWPF.TreeViewItemsLib.NewEnvironmentsTreeItems;
using GingerWPF.UserControlsLib;
using System;
using System.IO;
using System.Windows.Controls;

namespace Ginger.MenusLib
{
    public static class ResourcesMenu
    {
        public static TwoLevelMenu twoLevelMenu;

        private static TwoLevelMenuPage mMenusPage = null;
        public static TwoLevelMenuPage MenusPage
        {
            get
            {
                if (mMenusPage == null)
                {
                    mMenusPage = new TwoLevelMenuPage(GetMenu());
                     WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;
                }
                return mMenusPage;
            }
        }

        private static void WorkSpacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkSpace.Solution))
            {
                MenusPage.Reset();
            }
        }

        private static TwoLevelMenu GetMenu()
        {
            TwoLevelMenu twoLevelMenu = new TwoLevelMenu();

            TopMenuItem SharedRepositoryMenu = new TopMenuItem(eImageType.SharedRepositoryItem, "Shared Repository", ConsoleKey.S, "Shared Repository AID", "Flow Elements which can be shared between multiple " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows));
            SharedRepositoryMenu.Add(eImageType.ActivitiesGroup, GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.ActivitiesGroups), SharedActivitiesGroups, ConsoleKey.S, GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.ActivitiesGroups, "Shared "), "SR ActivitiesGroup AID");
            SharedRepositoryMenu.Add(eImageType.Activity, GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.Activities), SharedActivities, ConsoleKey.S, GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.Activities, "Shared "), "SR Activity AID");
            SharedRepositoryMenu.Add(eImageType.Action, "Actions", SharedActions, ConsoleKey.S, "Shared Actions", "SR Actions AID");
            SharedRepositoryMenu.Add(eImageType.Variable, GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.Variables), SharedVariables, ConsoleKey.S, GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.Variables, "Shared "), "SR Variables AID");
            twoLevelMenu.Add(SharedRepositoryMenu);

            TopMenuItem ApplicationModelsMenu = new TopMenuItem(eImageType.ApplicationModel, "Applications Models", ConsoleKey.A, "Application Models AID", "Applications Layers Templates" );
            ApplicationModelsMenu.Add(eImageType.APIModel, "API Models", APIModels, ConsoleKey.A, "API Templates Repository", "API Models AID");
            ApplicationModelsMenu.Add(eImageType.ApplicationPOMModel, "Page Objects Models", POMModels, ConsoleKey.P, "Page UI Elements Repository", "POM Menu AID");         
            ApplicationModelsMenu.Add(eImageType.Parameter, "Models Global Parameters", ModelsGlobalParameters, ConsoleKey.P, "Add or Edit Models Global Parameters", "Global Params AID");
            twoLevelMenu.Add(ApplicationModelsMenu);

            TopMenuItem environemntsMenu = new TopMenuItem(eImageType.Environment, "Environments", ConsoleKey.E, "Environemnts_AID", "Environments are been used for storing environment level parameters and DB/Unix connections details");
            environemntsMenu.Add(eImageType.Environment, "Environments", GetEnvsPage, ConsoleKey.E, "Environments are been used for storing environment level parameters and DB / Unix connections details", "Envs List");            
            twoLevelMenu.Add(environemntsMenu);

            TopMenuItem GlobalVariabelsMenu = new TopMenuItem(eImageType.Variable, GingerDicser.GetTermResValue(eTermResKey.Variables,"Global "), ConsoleKey.G, "Global Variables AID", GingerDicser.GetTermResValue(eTermResKey.Variables, suffixString:" which can be used cross " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows)));
            GlobalVariabelsMenu.Add(eImageType.Variable, "", GetGlobalVariabelsPage, ConsoleKey.G, "", "Global Variable AID");
            twoLevelMenu.Add(GlobalVariabelsMenu);

            TopMenuItem DataSourceMenu = new TopMenuItem(eImageType.DataSource, "Data Sources", ConsoleKey.D, "Data Sources AID", "Add and Edit Data Sources");
                DataSourceMenu.Add(eImageType.DataSource, "", DataSources, ConsoleKey.D, "", "DS AID");
                twoLevelMenu.Add(DataSourceMenu);

            TopMenuItem DocumentsMenu = new TopMenuItem(eImageType.File, "Documents", ConsoleKey.D, "Documents AID", "Solution documents like: text, excel, js scripts and any type of file");
            DocumentsMenu.Add(eImageType.File, "", Documents, ConsoleKey.D, "", "Dcouments AID");
            twoLevelMenu.Add(DocumentsMenu);

            TopMenuItem PluginsMenu = new TopMenuItem(eImageType.PluginPackage, "Plugins", ConsoleKey.P, "Plugins AID", "Ginger extension Add-ons");
            PluginsMenu.Add(eImageType.PluginPackage, "Installed", PluginsList, ConsoleKey.P, "Plugins which are installed in the solution", "Plugins AID");
            PluginsMenu.Add(eImageType.Download, "Download Plugins", OnlinePlugins, ConsoleKey.O, "Online plugins which can be downloaded", "Online Plugins");
            //PluginsMenu.Add(eImageType.PluginPackage, "Local", LocalPlugins, ConsoleKey.L, "Local plugins which are already downloaded on the user machine", "Online Plugins");
            twoLevelMenu.Add(PluginsMenu);

            return twoLevelMenu;
        }

        private static Page GetGlobalVariabelsPage()
        {
            VariabelsListViewPage variabelsPage = new VariabelsListViewPage(WorkSpace.Instance.Solution, null, General.eRIPageViewMode.Standalone);

            return (variabelsPage);
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
            SingleItemTreeViewExplorerPage pomModelPage = new SingleItemTreeViewExplorerPage("Page Objects Models", eImageType.Application, POMsRoot, POMsRoot.SaveAllTreeFolderItemsHandler, POMsRoot.AddPOM);
            return pomModelPage;
        }

        private static Page APIModels()
        {
            AppApiModelsFolderTreeItem apiRoot = new AppApiModelsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationAPIModel>());
            SingleItemTreeViewExplorerPage apiModelPage = new SingleItemTreeViewExplorerPage("API Models", eImageType.APIModel, apiRoot, apiRoot.SaveAllTreeFolderItemsHandler, apiRoot.AddAPIModelFromDocument);
            return apiModelPage;
        }

        private static Page PluginsList()
        {
            if (IsPluginsBeenDownloaded())
            {
                return null;
            }

            PlugInsFolderTreeItem pluginsRoot = new PlugInsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<PluginPackage>());          
            SingleItemTreeViewExplorerPage PluginsRootPage = new SingleItemTreeViewExplorerPage("Plugins", eImageType.PluginPackage, pluginsRoot, saveAllHandler: pluginsRoot.SaveAllTreeFolderItemsHandler, addHandler: pluginsRoot.AddPlugIn);
            return PluginsRootPage;
        }

        //private static Page LocalPlugins()
        //{
        //    return new LocalPluginsPage();
        //}

        private static Page OnlinePlugins()
        {
            if (IsPluginsBeenDownloaded())
            {
                return null;
            }

            return new PluginsIndexPage();
        }

        private static bool IsPluginsBeenDownloaded()
        {
            if (WorkSpace.Instance.PlugInsManager.BackgroudDownloadInprogress)
            {
                Reporter.ToUser(eUserMsgKey.PluginDownloadInProgress);

                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
