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

        CLIHelper mCLIHelper = new CLIHelper();
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

        public void Execute(RunsetExecutor runsetExecutor)
        {
            WorkSpace.Instance.RunsetExecutor.InitRunners();
            runsetExecutor.RunRunset();
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
                        mCLIHelper.Solution = value;                    
                        break;
                    case "environment":
                    case "env":
                    case "e":
                        mCLIHelper.Env = value;                        
                        break;
                    case "runset":                    
                    case "r":
                        mCLIHelper.Runset = value;                        
                        break;

                    // TODO: add all the rest !!!!!!!!!!!!!

                    default:
                        Reporter.ToLog(eLogLevel.ERROR, "Unknown argument: '" + argval + "'");
                        break;
                }
            }

            mCLIHelper.ProcessArgs(runsetExecutor);


        }

        
    }
}
