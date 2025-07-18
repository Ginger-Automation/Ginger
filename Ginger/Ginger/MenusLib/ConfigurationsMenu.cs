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
using Amdocs.Ginger.Common.Enums;
using Ginger.Configurations;
using Ginger.ExternalConfigurations;
using Ginger.GeneralWindows;
using Ginger.Reports;
using Ginger.SolutionWindows;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.TagsLib;
using Ginger.TwoLevelMenuLib;
using GingerCore;
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
                    WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;
                    WorkSpace.Instance.UserProfile.PropertyChanged += WorkSpacePropertyChanged;
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
            if (e.PropertyName == nameof(WorkSpace.UserProfile.ShowEnterpriseFeatures))
            {
                MenusPage.Reset();
            }
        }

        private static TwoLevelMenu GetMenu()
        {
            TwoLevelMenu twoLevelMenu = new TwoLevelMenu();

            TopMenuItem targetApplicationsMenu = new TopMenuItem(eImageType.Application, $"{GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}s", ConsoleKey.T, $"{GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} AID", "Name & Platforms of the Applications which been tested in current Solution");
            targetApplicationsMenu.Add(eImageType.Application, "", GetTargetApplicationsPage, ConsoleKey.T, "", "AID");
            twoLevelMenu.Add(targetApplicationsMenu);

            TopMenuItem agentsMenu = new TopMenuItem(eImageType.Agent, "Agents", ConsoleKey.A, "Agents AID", "Agents are the drivers which communicates with the tested application");
            agentsMenu.Add(eImageType.Agent, "", AgentsList, ConsoleKey.A, "", "AID");
            twoLevelMenu.Add(agentsMenu);

            TopMenuItem reportsMenu = new TopMenuItem(eImageType.Report, "Reports", ConsoleKey.R, "Reports_AID", "Reports Templates and Configurations");
            reportsMenu.Add(eImageType.Report, "Reports Templates", ReportsList, ConsoleKey.R, "Reports Templates are used to define the HTML report content and design", "Reports AID");
            reportsMenu.Add(eImageType.Config, "General Reports Configurations", ReportsConfig, ConsoleKey.R, "Global Reports Configurations", "Reports Configurations AID");
            reportsMenu.Add(eImageType.Config, "Execution Logger Configurations", ExecutionLoggerConfig, ConsoleKey.R, "Execution Logger Configurations", "Execution Logger Configurations AID");
            twoLevelMenu.Add(reportsMenu);

            TopMenuItem tagsMenu = new TopMenuItem(eImageType.Tag, "Tags", ConsoleKey.T, "Tags AID", "List of Tags to be used for marking any of the Solution items with");
            tagsMenu.Add(eImageType.Tag, "", GetTagsPage, ConsoleKey.T, "", "AID");
            twoLevelMenu.Add(tagsMenu);

            TopMenuItem externalConfigMenu = new TopMenuItem(eImageType.Building, WorkSpace.Instance.Solution.ExternalIntegrationsTabName, ConsoleKey.X, "External Configurations AID", "List of External Configurations to be used");
            externalConfigMenu.Add(eImageType.VRT, "VRT", GetVRTExteranalConfigsPage, ConsoleKey.X, "Visual Regression Testing External Configurations", "VRT Configuration AID");
            externalConfigMenu.Add(eImageType.Applitools, "Applitools", GetApplitoolsExteranalConfigsPage, ConsoleKey.X, "Applitools External Configurations", "Applitools Configuration AID");
            externalConfigMenu.Add(eImageType.Sealights, "Sealights", GetSealightsExteranalConfigsPage, ConsoleKey.X, "Sealights External Configurations", "Sealights Configuration AID");
            externalConfigMenu.Add(eImageType.Exchange, "Ask Lisa", GetAskLisaConfigsPage, ConsoleKey.X, "Ask Lisa Configurations", "Ask Lisa Configuration AID");
            externalConfigMenu.Add(eImageType.GingerAnalytics, "GingerOps", GetGingerOpsPage, ConsoleKey.X, "GingerOps Configuration", "GingerOps Configuration AID");
            externalConfigMenu.Add(eImageType.WireMockLogo, "WireMock", GetWireMockPage, ConsoleKey.X, "WireMock Configuration", "WireMock Configuration AID");
            externalConfigMenu.Add(eImageType.GingerPlayLogo, "GingerPlay", GetGingerPlayPage, ConsoleKey.X, "GingerPlay Configuration", "GingerPlay Configuration AID");
            twoLevelMenu.Add(externalConfigMenu);

            TopMenuItem accessiblityRulesMenu = new TopMenuItem(eImageType.Accessibility, $"{GingerCore.General.GetEnumValueDescription(typeof(eTermResKey), nameof(eTermResKey.AccessibilityRules))}", ConsoleKey.T, $"{GingerCore.General.GetEnumValueDescription(typeof(eTermResKey), nameof(eTermResKey.AccessibilityRules))}", "Name & rules of the Accessibility which been present current json");
            accessiblityRulesMenu.Add(eImageType.Application, "", GetAccessibilityRulePage, ConsoleKey.T, "", "AID");
            twoLevelMenu.Add(accessiblityRulesMenu);

            TopMenuItem selfHealingConfigMenu = new TopMenuItem(eImageType.SelfHealing, name: "Self Healing", ConsoleKey.U, automationID: "selfHealingMenuAutoId", "Solution Self Healing Configuration");
            selfHealingConfigMenu.Add(eImageType.SelfHealing, "", CreateSolutionSelfHealingConfigPage, ConsoleKey.U, "", AutomationID: "selfHealingSubMenuAutoId");
            twoLevelMenu.Add(selfHealingConfigMenu);

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
            return new ExecutionResultsConfiguration();
            //return ExecutionResultsConfiguration.Instance;
        }

        private static Page AgentsList()
        {
            AgentsFolderTreeItem AgentsRoot = new AgentsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<GingerCore.Agent>())
            {
                IsGingerDefualtFolder = true
            };
            SingleItemTreeViewExplorerPage agentsPage = new SingleItemTreeViewExplorerPage("Agents", eImageType.Agent, AgentsRoot, AgentsRoot.SaveAllTreeFolderItemsHandler, AgentsRoot.AddItemHandler, isSaveButtonHidden: true, showTitle: true);
            return agentsPage;
        }


        private static Page GetTagsPage()
        {
            return new TagsPage(TagsPage.eViewMode.Solution);
        }

        private static Page ReportsList()
        {
            HTMLGingerReportsTreeItem reportsRoot = new HTMLGingerReportsTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<HTMLReportConfiguration>())
            {
                IsGingerDefualtFolder = true
            };
            SingleItemTreeViewExplorerPage reportsPage = new SingleItemTreeViewExplorerPage("Reports Templates", eImageType.Report, reportsRoot, reportsRoot.SaveAllTreeFolderItemsHandler, reportsRoot.AddItemHandler, isSaveButtonHidden: true, showTitle: true);
            return reportsPage;
        }

        private static Page GetVRTExteranalConfigsPage()
        {
            return new VRTExternalConfigurationsPage();
        }
        private static Page GetApplitoolsExteranalConfigsPage()
        {
            return new ApplitoolsExternalConfigurationsPage();
        }
        private static Page GetSealightsExteranalConfigsPage()
        {
            return new SealightsExternalConfigurationsPage();
        }
        private static Page GetAskLisaConfigsPage()
        {
            return new AskLisaConfigurationsPage();
        }

        private static Page GetGingerOpsPage()
        {
            return new GingerOpsConfigurationPage();
        }

        private static Page GetWireMockPage()
        {
            return new WireMockConfigurationPage();
        }

        private static Page GetGingerPlayPage()
        {
            return new GingerPlayConfigurationpage();
        }

        //Remove when we add other pages
        private static Page OthersPage()
        {
            return new Page();
        }

        private static Page GetAccessibilityRulePage()
        {
            return (new AccessibilityRulePage());
        }

        private static SolutionSelfHealingConfigPage CreateSolutionSelfHealingConfigPage()
        {
            return new SolutionSelfHealingConfigPage(WorkSpace.Instance.Solution);
        }
    }
}
