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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Amdocs.Ginger.GingerConsole
{
    public class SolutionMenu
    {
        SolutionRepository SR;
        MenuItem OpenSolutionMenuItem;
        MenuItem CloseSolutionMenuItem;
        MenuItem BusinessFlowsCountMenuItem;
        MenuItem BusinessFlowsListMenuItem;
        MenuItem EnvironmentsListMenuItem;
        MenuItem RunBusinessFlowMenuItem;
        MenuItem RunSetMenuItem;

        public MenuItem GetMenu()
        {
            OpenSolutionMenuItem = new MenuItem(ConsoleKey.O, "Open", () => OpenSolution(), true);
            CloseSolutionMenuItem = new MenuItem(ConsoleKey.C, "Close", CloseSolution, false);
            BusinessFlowsCountMenuItem = new MenuItem(ConsoleKey.B, "Business Flows Count", BusinessFlowsCount, false);
            BusinessFlowsListMenuItem = new MenuItem(ConsoleKey.L, "Business Flows List", BusinessFlowsList, false);
            EnvironmentsListMenuItem = new MenuItem(ConsoleKey.E, "Environments List", EnvironmentsList, false);
            RunBusinessFlowMenuItem = new MenuItem(ConsoleKey.R, "Run Business Flow", RunBusinessFlow, false);
            RunSetMenuItem = new MenuItem(ConsoleKey.S, "Run set", RunSet, false);

            MenuItem SolutionMenu = new MenuItem(ConsoleKey.S, "Solution");
            SolutionMenu.SubItems.Add(OpenSolutionMenuItem);
            SolutionMenu.SubItems.Add(CloseSolutionMenuItem);
            SolutionMenu.SubItems.Add(BusinessFlowsCountMenuItem);
            SolutionMenu.SubItems.Add(BusinessFlowsListMenuItem);
            SolutionMenu.SubItems.Add(EnvironmentsListMenuItem);
            SolutionMenu.SubItems.Add(RunBusinessFlowMenuItem);
            SolutionMenu.SubItems.Add(RunSetMenuItem);

            return SolutionMenu;
        }

        private void RunSet()
        {
            Console.WriteLine("Run Set Name?");
            string runSetName = Console.ReadLine();
            RunSetConfig runSetConfig = (from x in SR.GetAllRepositoryItems<RunSetConfig>() where x.Name == runSetName select x).SingleOrDefault();
            if (runSetConfig == null)
            {
                Console.WriteLine("RunSetConfig not found");
                return;
            }


            Console.WriteLine("starting RunSetConfig execution");
            RunsetExecutor runsetExecuto = new RunsetExecutor();
            runsetExecuto.RunSetConfig = runSetConfig;
            runsetExecuto.RunRunset();            

            Console.WriteLine("Execution completed");            
        }

        private void RunBusinessFlow()
        {
            Console.WriteLine("Business Flow Name?");
            string BizFlowName = Console.ReadLine();
            BusinessFlow businessFlow = (from x in SR.GetAllRepositoryItems<BusinessFlow>() where x.Name == BizFlowName select x).SingleOrDefault();
            if (businessFlow == null)
            {
                Console.WriteLine("Business Flow not found");
                return;
            }
            GingerRunner gingerRunner = new GingerRunner();            
            gingerRunner.RunBusinessFlow(businessFlow, true);

            Console.WriteLine("Execution Completed");
            Console.WriteLine("----------------------------");
            Console.WriteLine("Business Flow: " + businessFlow.Name);
            Console.WriteLine("Business Flow Description: " + businessFlow.Description);
            Console.WriteLine("Business Flow Status: " + businessFlow.RunStatus);
            Console.WriteLine("Activities Count: " + businessFlow.Activities.Count);
            Console.WriteLine("----------------------------");
            foreach (Activity activity in businessFlow.Activities)
            {
                Console.WriteLine("Activity: " + activity.ActivityName + " Status: " + activity.Status);

                Console.WriteLine("Actions Found:" + activity.Acts.Count);
                foreach (Act act in activity.Acts )
                {
                    Console.WriteLine("------");
                    Console.WriteLine("Action:" + act.Description);
                    Console.WriteLine("Description:" + act.ActionDescription);
                    Console.WriteLine("Type:" + act.ActionType);
                    Console.WriteLine("Class:" + act.ActClass );
                    Console.WriteLine("Status:" + act.Status );
                    Console.WriteLine("Error:" + act.Error);
                    Console.WriteLine("ExInfo:" + act.ExInfo);
                }
                Console.WriteLine("----------");
            }


        }

        public static void InitClassTypesDictionary()
        {
            //TODO: cleanup after all RIs moved to GingerCoreCommon

            //if (bDone) return;
            //bDone = true;

            // TODO: remove after we don't need old serializer to load old repo items
            // NewRepositorySerializer.NewRepositorySerializerEvent += RepositorySerializer.NewRepositorySerializer_NewRepositorySerializerEvent;

            // Add all RI classes from GingerCoreCommon
            NewRepositorySerializer.AddClassesFromAssembly(typeof(RepositoryItemBase).Assembly);

            // Add all RI classes from GingerCore
            // NewRepositorySerializer.AddClassesFromAssembly(typeof(GingerCore.Actions.ActButton).Assembly); // GingerCore.dll

            // add  old Plugins - TODO: remove later when we change to new plugins
            // NewRepositorySerializer.AddClassesFromAssembly(typeof(GingerPlugIns.ActionsLib.PlugInActionsBase).Assembly);


            // add from Ginger - items like RunSetConfig
            // NewRepositorySerializer.AddClassesFromAssembly(typeof(Ginger.App).Assembly);

            // Each class which moved from GingerCore to GingerCoreCommon needed to be added here, so it will auto translate
            // For backward compatibility of loading old object name in xml
            Dictionary<string, Type> list = new Dictionary<string, Type>();
            list.Add("GingerCore.Actions.ActInputValue", typeof(ActInputValue));
            list.Add("GingerCore.Actions.ActReturnValue", typeof(ActReturnValue));
            list.Add("GingerCore.Actions.EnhancedActInputValue", typeof(EnhancedActInputValue));
            list.Add("GingerCore.Environments.GeneralParam", typeof(GeneralParam));

            // Put back for Lazy load of BF.Acitvities
            NewRepositorySerializer.AddLazyLoadAttr(nameof(BusinessFlow.Activities)); // TODO: add RI type, and use attr on field


            // Verify the old name used in XML
            //list.Add("GingerCore.Actions.RepositoryItemTag", typeof(RepositoryItemTag));
            //list.Add("GingerCore.Actions.EnhancedActInputValue", typeof(EnhancedActInputValue));

            // TODO: change to SR2  if we want the files to be loaded convert and save with the new SR2

            //if (WorkSpace.Instance.BetaFeatures.UseNewRepositorySerializer)
            //{
            //RepositorySerializer2 RS2 = new RepositorySerializer2();

            //SolutionRepository.mRepositorySerializer = RS2;
            //RepositoryFolderBase.mRepositorySerializer = RS2;
            //    ObservableListSerializer.RepositorySerializer = RS2;

            //}
            //else
            //{
            //        SolutionRepository.mRepositorySerializer = new RepositorySerializer();
            //        RepositoryFolderBase.mRepositorySerializer = new RepositorySerializer();
            //}

            NewRepositorySerializer.AddClasses(list);

        }


        private void OpenSolution()
        {
            InitClassTypesDictionary();

            Console.WriteLine("Solution Folder?");
            string sFolder = Console.ReadLine();
            if (Directory.Exists(sFolder))
            {

                Console.WriteLine("Opening Solution at folder: " + sFolder);

                SR = new SolutionRepository();
                SR.AddItemInfo<BusinessFlow>("*.Ginger.BusinessFlow.xml", @"~\BusinessFlows", true, GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), PropertyNameForFileName: nameof(BusinessFlow.Name));

                SR.AddItemInfo<ApplicationAPIModel>("*.Ginger.ApplicationAPIModel.xml", @"~\Applications Models\API Models", true, "API Models", PropertyNameForFileName: nameof(ApplicationAPIModel.Name));
                SR.AddItemInfo<GlobalAppModelParameter>("*.Ginger.GlobalAppModelParameter.xml", @"~\Applications Models\Global Models Parameters", true, "Global Model Parameters", PropertyNameForFileName: nameof(GlobalAppModelParameter.PlaceHolder));
                SR.AddItemInfo<ApplicationPOMModel>("*.Ginger.ApplicationPOMModel.xml", @"~\Applications Models\POM Models", true, "POM Models", PropertyNameForFileName: nameof(ApplicationPOMModel.Name));

                SR.AddItemInfo<ProjEnvironment>("*.Ginger.Environment.xml", @"~\Environments", true, "Environments", PropertyNameForFileName: nameof(ProjEnvironment.Name));
                SR.AddItemInfo<ALMDefectProfile>("*.Ginger.ALMDefectProfile.xml", @"~\ALMDefectProfiles", true, "ALM Defect Profiles", PropertyNameForFileName: nameof(ALMDefectProfile.Name));

                // SR.AddItemInfo<Agent>("*.Ginger.Agent.xml", @"~\Agents", true, "Agents", PropertyNameForFileName: nameof(Agent.Name));

                //TODO: check if below 2 reports folders are realy needed
                SR.AddItemInfo<HTMLReportConfiguration>("*.Ginger.HTMLReportConfiguration.xml", @"~\HTMLReportConfigurations", true, "HTMLReportConfigurations", PropertyNameForFileName: nameof(HTMLReportsConfiguration.Name));
                SR.AddItemInfo<HTMLReportTemplate>("*.Ginger.HTMLReportTemplate.xml", @"~\HTMLReportConfigurations\HTMLReportTemplate", true, "HTMLReportTemplate", PropertyNameForFileName: nameof(HTMLReportTemplate.Name));

                // SR.AddItemInfo<ReportTemplate>("*.Ginger.ReportTemplate.xml", @"~\HTMLReportConfigurations\ReportTemplates", true, "ReportTemplates", PropertyNameForFileName: nameof(ReportTemplate.Name));

                SR.AddItemInfo<DataSourceBase>("*.Ginger.DataSource.xml", @"~\DataSources", true, "Data Sources", PropertyNameForFileName: nameof(DataSourceBase.Name));

                SR.AddItemInfo<PluginPackage>("*.Ginger.PluginPackage.xml", @"~\Plugins", true, "Plugins", PropertyNameForFileName: nameof(PluginPackage.PluginId));

                SR.AddItemInfo<ActivitiesGroup>("*.Ginger.ActivitiesGroup.xml", @"~\SharedRepository\ActivitiesGroup", true, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups, "Shared "), PropertyNameForFileName: nameof(ActivitiesGroup.Name));
                SR.AddItemInfo<Activity>("*.Ginger.Activity.xml", @"~\SharedRepository\Activities", true, GingerDicser.GetTermResValue(eTermResKey.Activities, "Shared "), PropertyNameForFileName: nameof(Activity.ActivityName));
                SR.AddItemInfo<Act>("*.Ginger.Action.xml", @"~\SharedRepository\Actions", true, "Shared Actions", PropertyNameForFileName: nameof(Act.Description));
                SR.AddItemInfo<VariableBase>("*.Ginger.Variable.xml", @"~\SharedRepository\Variables", true, GingerDicser.GetTermResValue(eTermResKey.Variables, "Shared "), PropertyNameForFileName: nameof(VariableBase.Name));

                SR.AddItemInfo<RunSetConfig>("*.Ginger.RunSetConfig.xml", @"~\RunSetConfigs", true, GingerDicser.GetTermResValue(eTermResKey.RunSets), PropertyNameForFileName: nameof(RunSetConfig.Name));

                WorkSpace.Instance.SolutionRepository = SR;
                SR.Open(sFolder);
                IEnumerable<RepositoryFile> files = SR.GetAllSolutionRepositoryFiles().ToList();
                Console.WriteLine("Files count = " + files.Count());

                Console.WriteLine("Ginger Menu - Solution: " + sFolder);

                OpenSolutionMenuItem.Active = false;
                CloseSolutionMenuItem.Active = true;
                BusinessFlowsCountMenuItem.Active = true;
                BusinessFlowsListMenuItem.Active = true;
                EnvironmentsListMenuItem.Active = true;
                RunBusinessFlowMenuItem.Active = true;
                RunSetMenuItem.Active = true;
            }
            else
            {
                Console.WriteLine("Directory not found: " + sFolder);
            }
        }

        private void BusinessFlowsCount()
        {
            Console.WriteLine("Executing BFs count");
            ObservableList<BusinessFlow> BFs = SR.GetAllRepositoryItems<BusinessFlow>();
            Console.WriteLine("BFs count = " + BFs.Count());
        }

        private void CloseSolution()
        {
            SR = null;
            OpenSolutionMenuItem.Active = true;
            CloseSolutionMenuItem.Active = false;
            BusinessFlowsCountMenuItem.Active = false;
            BusinessFlowsListMenuItem.Active = false;
            EnvironmentsListMenuItem.Active = false;
            RunSetMenuItem.Active = false;
        }

        private void BusinessFlowsList()
        {
            Console.WriteLine("Executing BFs List");
            ObservableList<BusinessFlow> BFs = SR.GetAllRepositoryItems<BusinessFlow>();
            int count = 0;
            foreach (BusinessFlow BF in BFs)
            {
                count++;
                Console.WriteLine("# " + count + " - " + BF.Name);
                foreach (Activity activity in BF.Activities)
                {                    
                    Console.WriteLine("Activity: " + activity.ActivityName);
                    foreach (Act action in activity.Acts)
                    {
                        Console.WriteLine("Action: " + action.Description);
                    }
                }
            }
        }

        void EnvironmentsList()
        {            
            //ObservableList<ProjEnvironment> v = SR.GetAllRepositoryItems<ProjEnvironment>();
            //foreach (ProjEnvironment e in v)
            //{
            //    Console.WriteLine(e.Name);
            //}
        }
    }
}
