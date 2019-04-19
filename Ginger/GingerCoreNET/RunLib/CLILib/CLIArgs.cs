using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Run;
using GingerCore;
using GingerCore.Environments;
using System;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIArgs : ICLI
    {

        string mSolution;
        string mEnv;
        string mRunset;

        string ICLI.Identifier
        {
            get
            {
                return "Args";
            }
        }

        string ICLI.FileExtension
        {
            get
            {
                return null;
            }
        }

        public string CreateContent(RunsetExecutor runsetExecutor)
        {
            string Args = string.Format("--soluion {0}", WorkSpace.Instance.Solution.Folder);
            Args += string.Format(" --runset {0}", runsetExecutor.RunSetConfig.Name);
            Args += string.Format(" --environemnt:{0}", runsetExecutor.RunsetExecutionEnvironment.Name);
            return Args;
        }

        public bool Execute(RunsetExecutor runsetExecutor)
        {
            WorkSpace.Instance.RunsetExecutor.InitRunners();
            runsetExecutor.RunRunset();
            return true;
        }
        

        public void LoadContent(string args, RunsetExecutor runsetExecutor)
        {
            //TODO: parse all args then process

            //TODO: make -s --solution  work  but not -solution or -Solution !!!!!!!!!!!!!!!!!!!!!!!!!!!!

            string[] argsList = args.Split('-');
            foreach(string argval in argsList)
            {
                if (string.IsNullOrEmpty(argval.Trim()))
                {
                    continue;
                }
                string[] aargval = argval.Split(new[] {' '}, 2);  // split on the first space
                string arg = aargval[0];
                string value =aargval[1];
                switch (arg)
                {
                    case "solution":
                    case "s":                    
                        mSolution = value;                    
                        break;
                    case "environment":
                    case "env":
                    case "e":                    
                        mEnv = value;                        
                        break;
                    case "runset":                    
                    case "r":                    
                        mRunset = value;                        
                        break;
                    default:
                        Reporter.ToLog(eLogLevel.ERROR, "Unknown argument: '" + argval + "'");
                        break;
                }
            }

            ProcessArgs(runsetExecutor);


        }

        private void ProcessArgs(RunsetExecutor runsetExecutor)
        {
            // TODO: create CLIHelper with generic functions !!!!!!!!!!!!!!!!!!!!!!!
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            if (WorkSpace.Instance.OpenSolution(mSolution) == false)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to load the Solution");
                // TODO: throw
                return;
            }

            // Dup with config externlize !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Reporter.ToLog(eLogLevel.DEBUG, "Selected Environment: '" + mEnv + "'");
            ProjEnvironment env = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => x.Name.ToLower().Trim() == mEnv.ToLower().Trim()).FirstOrDefault();
            if (env != null)
            {
                runsetExecutor.RunsetExecutionEnvironment = env;
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to find matching Environment in the Solution");
                // TODO: throw
                // return false;
            }

            // Dup with config externlize !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Selected {0}: '{1}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), mRunset));
            ObservableList<RunSetConfig> RunSets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
            RunSetConfig runSetConfig = RunSets.Where(x => x.Name.ToLower().Trim() == mRunset.ToLower().Trim()).FirstOrDefault();
            if (runSetConfig != null)
            {
                runsetExecutor.RunSetConfig = runSetConfig;
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to find matching {0} in the Solution", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                // TODO: throw
                // return false;
            }

        }
    }
}
