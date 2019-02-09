using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.GingerConsole;
using Amdocs.Ginger.GingerConsole.ReporterLib;
using Amdocs.Ginger.Repository;
using Ginger.SolutionGeneral;
using GingerCore;
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
            if (GingerUtils.OperatingSystem.IsWindows())
            {
                OpenSolution(@"C:\Work\GINGER_WS_TEST");
            }
            else if (GingerUtils.OperatingSystem.IsLinux())
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
            if (Directory.Exists(sFolder))
            {
                Console.WriteLine("Opening Solution at folder: " + sFolder);
                SR = GingerSolutionRepository.CreateGingerSolutionRepository();
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
