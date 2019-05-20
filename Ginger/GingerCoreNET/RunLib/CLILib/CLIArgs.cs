using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Run;
using System;
using System.Collections.Generic;
using System.IO;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIArgs : ICLI
    {
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

        public string CreateContent(RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            string Args = string.Format("--solution {0}", WorkSpace.Instance.Solution.Folder);
            Args += string.Format(" --runset {0}", runsetExecutor.RunSetConfig.Name);
            Args += string.Format(" --environemnt {0}", runsetExecutor.RunsetExecutionEnvironment.Name);

            // TODO: add all the rest source control, run analyzer...

            return Args;
        }

        public void Execute(RunsetExecutor runsetExecutor)
        {
            runsetExecutor.RunRunset();
        }
        

        public void LoadContent(string args, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
        {
            //TODO: make -s --solution  work  but not -solution or -Solution !!!!!!!!!!!!!!!!!!!!!!!!!!!!

            List<Arg> argsList = SplitArgs(args);
            

            // - SeekOrigin -- split keep -

            foreach(Arg arg in argsList)
            {             
                
                switch (arg.ArgName)
                {                        
                    case "-s":
                    case "--solution":
                        cliHelper.Solution = arg.ArgValue;
                        break;                     
                    case "-e":
                    case "--env":
                    case "--environment":
                        cliHelper.Env = arg.ArgValue;
                        break;                        
                    case "-r":
                    case "--runset":
                        cliHelper.Runset = arg.ArgValue;
                        break;

                    // TODO: add all the rest !!!!!!!!!!!!!
                    default:
                        Reporter.ToLog(eLogLevel.ERROR, "Unknown argument with '-' prefix: '" + arg + "'");
                        throw new ArgumentException("Unknown argument: ", arg.ArgName);
                }
            }                
        }

        public struct Arg
        {            
            public string ArgName;
            public string ArgValue;
        }


        // Handle args which are passed as -- (long) or - (short)
        public List<Arg> SplitArgs(string sArgs)
        {
            List<Arg> args = new List<Arg>();            
            string[] argsList = sArgs.Split('-');

            string parampref = "";
            foreach (string argval in argsList)
            {                
                if (string.IsNullOrEmpty(argval.Trim()))
                {
                    parampref += "-";
                    continue;
                }
                string[] aargval = argval.Split(new[] { ' ' }, 2);  // split on the first space
                string arg = parampref + aargval[0].Trim();
                string value = aargval[1].Trim();

                args.Add(new Arg() { ArgName = arg, ArgValue = value  });
                parampref = "-";
            }

            return args;
        }
    }
}
