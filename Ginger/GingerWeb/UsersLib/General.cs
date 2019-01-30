using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.GingerConsole;
using Amdocs.Ginger.GingerConsole.ReporterLib;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.IO;

namespace GingerWeb.UsersLib
{

    public class General
    {
        public static SolutionRepository SR;
        public static void init()
        {
            InitClassTypesDictionary();

            Reporter.WorkSpaceReporter = new GingerConsoleWorkspaceReporter();
            // NewRepositorySerializer RS = new NewRepositorySerializer();

            GingerConsoleWorkSpace ws = new GingerConsoleWorkSpace();
            WorkSpace.Init(ws);

            // WorkSpace.Instance.OpenSolution(@"C:\yaron\GingerSolution\Plugins\Plugins");
            //OpenSolution(@"C:\yaron\GingerSolution\Plugins\Plugins");
            if (GingerUtils.GingerOS.IsWindows())
            {
                OpenSolution(@"C:\Work\GINGER_WS_TEST");
            }
            else if (GingerUtils.GingerOS.IsLinux())
            {
                OpenSolution(@"/home/ginger/ginger_tests/ginger_solutions/TestSolution");
            }
            WorkSpace.Instance.Solution = (Solution)(ISolution)SR.RepositorySerializer.DeserializeFromFile(Path.Combine(SR.SolutionFolder, "Ginger.Solution.xml"));

            var gg = WorkSpace.Instance.LocalGingerGrid;
            var nodes = gg.NodeList;

            var v = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();            
        }


        // Combine to one in core !!!!!!!!!!!!!!!!!!!!!

        private static void OpenSolution(string sFolder)
        {    
            
            Console.WriteLine("Inside OpenSolution.. Opening Solution at folder: " + sFolder);

            if (Directory.Exists(sFolder))
            {
                SR = new SolutionRepository();
                SR.AddItemInfo<BusinessFlow>("*.Ginger.BusinessFlow.xml", @"~\BusinessFlows", true, GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), PropertyNameForFileName: nameof(BusinessFlow.Name));

                SR.AddItemInfo<ApplicationAPIModel>("*.Ginger.ApplicationAPIModel.xml", @"~\Applications Models\API Models", true, "API Models", PropertyNameForFileName: nameof(ApplicationAPIModel.Name));
                SR.AddItemInfo<GlobalAppModelParameter>("*.Ginger.GlobalAppModelParameter.xml", @"~\Applications Models\Global Models Parameters", true, "Global Model Parameters", PropertyNameForFileName: nameof(GlobalAppModelParameter.PlaceHolder));
                SR.AddItemInfo<ApplicationPOMModel>("*.Ginger.ApplicationPOMModel.xml", @"~\Applications Models\POM Models", true, "POM Models", PropertyNameForFileName: nameof(ApplicationPOMModel.Name));

                SR.AddItemInfo<ProjEnvironment>("*.Ginger.Environment.xml", @"~\Environments", true, "Environments", PropertyNameForFileName: nameof(ProjEnvironment.Name));
                SR.AddItemInfo<ALMDefectProfile>("*.Ginger.ALMDefectProfile.xml", @"~\ALMDefectProfiles", true, "ALM Defect Profiles", PropertyNameForFileName: nameof(ALMDefectProfile.Name));

                SR.AddItemInfo<Agent>("*.Ginger.Agent.xml", @"~\Agents", true, "Agents", PropertyNameForFileName: nameof(Agent.Name));

                //TODO: check if below 2 reports folders are realy needed
                SR.AddItemInfo<HTMLReportConfiguration>("*.Ginger.HTMLReportConfiguration.xml", @"~\HTMLReportConfigurations", true, "HTMLReportConfigurations", PropertyNameForFileName: nameof(HTMLReportsConfiguration.Name));
                SR.AddItemInfo<HTMLReportTemplate>("*.Ginger.HTMLReportTemplate.xml", @"~\HTMLReportConfigurations\HTMLReportTemplate", true, "HTMLReportTemplate", PropertyNameForFileName: nameof(HTMLReportTemplate.Name));

                SR.AddItemInfo<ReportTemplate>("*.Ginger.ReportTemplate.xml", @"~\HTMLReportConfigurations\ReportTemplates", true, "ReportTemplates", PropertyNameForFileName: nameof(ReportTemplate.Name));

                SR.AddItemInfo<DataSourceBase>("*.Ginger.DataSource.xml", @"~\DataSources", true, "Data Sources", PropertyNameForFileName: nameof(DataSourceBase.Name));

                SR.AddItemInfo<PluginPackage>("*.Ginger.PluginPackage.xml", @"~\Plugins", true, "Plugins", PropertyNameForFileName: nameof(PluginPackage.PluginId));

                SR.AddItemInfo<ActivitiesGroup>("*.Ginger.ActivitiesGroup.xml", @"~\SharedRepository\ActivitiesGroup", true, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups, "Shared "), PropertyNameForFileName: nameof(ActivitiesGroup.Name));
                SR.AddItemInfo<GingerCore.Activity>("*.Ginger.Activity.xml", @"~\SharedRepository\Activities", true, GingerDicser.GetTermResValue(eTermResKey.Activities, "Shared "), PropertyNameForFileName: nameof(GingerCore.Activity.ActivityName));
                SR.AddItemInfo<Act>("*.Ginger.Action.xml", @"~\SharedRepository\Actions", true, "Shared Actions", PropertyNameForFileName: nameof(Act.Description));
                SR.AddItemInfo<VariableBase>("*.Ginger.Variable.xml", @"~\SharedRepository\Variables", true, GingerDicser.GetTermResValue(eTermResKey.Variables, "Shared "), PropertyNameForFileName: nameof(VariableBase.Name));

                SR.AddItemInfo<RunSetConfig>("*.Ginger.RunSetConfig.xml", @"~\RunSetConfigs", true, GingerDicser.GetTermResValue(eTermResKey.RunSets), PropertyNameForFileName: nameof(RunSetConfig.Name));

                WorkSpace.Instance.SolutionRepository = SR;
                SR.Open(sFolder);
            }
            else
            {
                Console.WriteLine("Directory not found: " + sFolder);
            }
        }

        // COMBINE to one for all !!!!!!!!!!!!!!
        public static void InitClassTypesDictionary()
        {
            //TODO: cleanup after all RIs moved to GingerCoreCommon

            //if (bDone) return;
            //bDone = true;

            // TODO: remove after we don't need old serializer to load old repo items
            // NewRepositorySerializer.NewRepositorySerializerEvent += RepositorySerializer.NewRepositorySerializer_NewRepositorySerializerEvent;

            // Add all RI classes from GingerCoreCommon
            NewRepositorySerializer.AddClassesFromAssembly(typeof(RepositoryItemBase).Assembly);
            NewRepositorySerializer.AddClassesFromAssembly(typeof(Solution).Assembly);

            // corenet
            // NewRepositorySerializer.AddClassesFromAssembly(typeof(Agent).Assembly);

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


    }
}
