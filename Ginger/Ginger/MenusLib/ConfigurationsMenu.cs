#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger.Common.Enums;
using Ginger.GeneralWindows;
using Ginger.Reports;
using Ginger.SolutionWindows;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.TagsLib;
using Ginger.TwoLevelMenuLib;
using GingerWPF.UserControlsLib;
using System;
using System.Windows.Controls;

namespace Ginger.ConfigurationsLib
{
    public static class ConfigurationsMenu
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
                    App.UserProfile.PropertyChanged += UserProfile_PropertyChanged;
                }
                return mMenusPage;
            }
        }

        private static void UserProfile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserProfile.Solution))
            {
                MenusPage.Reset();
            }
        }

        private static TwoLevelMenu GetMenu()
        {
            TwoLevelMenu twoLevelMenu = new TwoLevelMenu();

            TopMenuItem targetApplicationsMenu = new TopMenuItem(eImageType.Application, "Target Applications", ConsoleKey.T, "Target Applications AID", "Name & Platforms of the Applications which been tested in current Solution");
            targetApplicationsMenu.Add(eImageType.Application, "", GetTargetApplicationsPage, ConsoleKey.T, "", "AID");
            twoLevelMenu.Add(targetApplicationsMenu);

            TopMenuItem agentsMenu = new TopMenuItem(eImageType.Agent, "Agents", ConsoleKey.A, "Agents AID", "Agents are the drivers which communicates with the tested application");
            agentsMenu.Add(eImageType.Agent, "", AgentsList, ConsoleKey.A, "", "AID");
            twoLevelMenu.Add(agentsMenu);

            TopMenuItem reportsMenu = new TopMenuItem(eImageType.Report, "Reports", ConsoleKey.R, "Reports_AID", "Reports Templates and Configurations");
            reportsMenu.Add(eImageType.Report, "Reports Templates", ReportsList, ConsoleKey.R, "Reports Templates are used to define the HTML report content and design", "Reports AID");
            reportsMenu.Add(eImageType.Config, "General Reports Configurations", ReportsConfig, ConsoleKey.R, "Global Reports Configurations", "Reports Config AID");
            reportsMenu.Add(eImageType.Config, "Execution Logger Configurations", ExecutionLoggerConfig, ConsoleKey.R, "Execution Logger Configurations", "Execution Logger Config AID");
            twoLevelMenu.Add(reportsMenu);

            TopMenuItem tagsMenu = new TopMenuItem(eImageType.Tag, "Tags", ConsoleKey.T, "Tags AID", "List of Tags to be used for marking any of the Solution items with");
            tagsMenu.Add(eImageType.Tag, "", GetTagsPage, ConsoleKey.T, "", "AID");
            twoLevelMenu.Add(tagsMenu);

            return twoLevelMenu;
        }

        private static Page GetTargetApplicationsPage()
        {
            return (new TargetApplicationsPage());
        }


        private static Page ReportsConfig()
        {
            return new HTMLReportsConfigurationPage();            
        }

        private static Page ExecutionLoggerConfig()
        {           
            return ExecutionResultsConfiguration.Instance;
        }

        private static Page AgentsList()
        {            
            AgentsFolderTreeItem AgentsRoot = new AgentsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<GingerCore.Agent>());
            AgentsRoot.IsGingerDefualtFolder = true;
            SingleItemTreeViewExplorerPage agentsPage = new SingleItemTreeViewExplorerPage("Agents", eImageType.Agent, AgentsRoot, AgentsRoot.SaveAllTreeFolderItemsHandler, AgentsRoot.AddItemHandler);                        
            return agentsPage;
        }


        private static Page GetTagsPage()
        {
             return new TagsPage(TagsPage.eViewMode.Solution);            
        }

        private static Page ReportsList()
        {          
            HTMLGingerReportsTreeItem reportsRoot = new HTMLGingerReportsTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<HTMLReportConfiguration>());
            reportsRoot.IsGingerDefualtFolder = true;
            SingleItemTreeViewExplorerPage reportsPage = new SingleItemTreeViewExplorerPage("Reports Templates", eImageType.Report, reportsRoot, reportsRoot.SaveAllTreeFolderItemsHandler, reportsRoot.AddItemHandler);
            return reportsPage;
        }



    }
}
