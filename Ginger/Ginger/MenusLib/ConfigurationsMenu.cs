using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;
using Ginger.GeneralWindows;
using Ginger.Reports;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.TwoLevelMenuLib;
using GingerCore.Environments;
using GingerWPF.TreeViewItemsLib.NewEnvironmentsTreeItems;
using GingerWPF.UserControlsLib;
using System;
using System.Windows.Controls;

namespace Ginger.ConfigurationsLib
{
    public static class ConfigurationsMenu
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
            TopMenuItem environemntsMenu = new TopMenuItem("Environments", ConsoleKey.E, "Environemnts_AID", "Environments are been used for storing environment level parameters and DB/Unix connections details");
            environemntsMenu.Add("Environments List",  EnvsList, ConsoleKey.L, "Environments are been used for storing environment level parameters and DB / Unix connections details", "Envs List");
            //environemntsMenu.Add("Compare", EnvsCompare, ConsoleKey.C, "Compare Environments", "compare Envs AID");
            twoLevelMenu.Add(environemntsMenu);

            TopMenuItem agentsMenu = new TopMenuItem("Agents", ConsoleKey.A, "Agents AID", "Agents are the drivers which comunicates with the tested application");
            agentsMenu.Add("List", AgentsList, ConsoleKey.L, "", "AgentsList");            
            twoLevelMenu.Add(agentsMenu);

            TopMenuItem reportsMenu = new TopMenuItem("Reports", ConsoleKey.R, "Reports_AID");
            reportsMenu.Add("Reports Templates", ReportsList, ConsoleKey.L, "Reports Templates are used to define the HTML report content and design", "Reports AID");
            reportsMenu.Add("General Configurations", ReportsConfig, ConsoleKey.C, "Global Reports Configurations", "Reports Config AID");
            
            // reportsMenu.Add("Templates", ReportsTemplates, ConsoleKey.T, "Edit and Create report templates", "AID");
            twoLevelMenu.Add(reportsMenu);
            
            return twoLevelMenu;
        }
        
        private static Page ReportsConfig()
        {
            return new HTMLReportsConfigurationPage();
            
        }

        private static Page AgentsList()
        {            
            AgentsFolderTreeItem AgentsRoot = new AgentsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<GingerCore.Agent>());
            AgentsRoot.IsGingerDefualtFolder = true;
            SingleItemTreeViewExplorerPage agentsPage = new SingleItemTreeViewExplorerPage("Agents", eImageType.Agent, AgentsRoot, AgentsRoot.SaveAllTreeFolderItemsHandler, AgentsRoot.AddItemHandler);                        
            return agentsPage;
        }

        private static Page EnvsCompare()
        {            
            return new Page() { Content = "Env Compare coming soon..." };
        }

        private static Page ReportsTemplates()
        {
             return new Page() { Content = "Reports templates coming soon..." };            
        }

        private static Page ReportsList()
        {          
            HTMLGingerReportsTreeItem reportsRoot = new HTMLGingerReportsTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<HTMLReportConfiguration>());
            reportsRoot.IsGingerDefualtFolder = true;
            SingleItemTreeViewExplorerPage reportsPage = new SingleItemTreeViewExplorerPage("Reports Templates", eImageType.Report, reportsRoot, reportsRoot.SaveAllTreeFolderItemsHandler, reportsRoot.AddItemHandler);
            return reportsPage;
        }

        private static Page EnvsList()
        {
            // cache
            EnvironmentsFolderTreeItem EnvsRoot = new EnvironmentsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ProjEnvironment>());
            SingleItemTreeViewExplorerPage p = new SingleItemTreeViewExplorerPage("Environments", eImageType.Environment, EnvsRoot, EnvsRoot.SaveAllTreeFolderItemsHandler, EnvsRoot.AddItemHandler);
            //xEnvironmentsItem.Tag = p;
            //xSelectedItemFrame.Content = p;
            EnvsRoot.IsGingerDefualtFolder = true;

            return p;
        }

        

    }
}
