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
            //TODO: make -s --solution  work  but not -solution or -Solution !!!!!!!!!!!!!!!!!!!!!!!!!!!!

            List<Arg> argsList = SplitArgs(args);
            
            foreach(Arg arg in argsList)
            {             
                if(arg.prefix == "-") // short param name - one letter for most common
                {
                    switch (arg.param)
                    {                        
                        case "s":
                            mCLIHelper.Solution = arg.value;
                            break;                     
                        case "e":                        
                            mCLIHelper.Env = arg.value;
                            break;                        
                        case "r":
                            mCLIHelper.Runset = arg.value;
                            break;

                        // TODO: add all the rest !!!!!!!!!!!!!
                        default:
                            Reporter.ToLog(eLogLevel.ERROR, "Unknown argument with '-' prefix: '" + arg + "'");
                            throw new ArgumentException("Unknown argument: ", arg.param);
                    }
                }
                else if(arg.prefix == "--") // Long param name
                {
                    switch (arg.param)
                    {
                        case "solution":                        
                            mCLIHelper.Solution = arg.value;
                            break;
                        case "environment":
                        case "env":                        
                            mCLIHelper.Env = arg.value;
                            break;
                        case "runset":                        
                            mCLIHelper.Runset = arg.value;
                            break;

                        // TODO: add all the rest !!!!!!!!!!!!!

                        default:
                            Reporter.ToLog(eLogLevel.ERROR, "Unknown argument with '--' prefix: '" + arg + "'");
                            throw new ArgumentException("Unknown argument: ", arg.param);
                    }
                }
                else
                {
                    throw new ArgumentException("Unknown prefix: ", arg.prefix);
                }
                
            }

            mCLIHelper.ProcessArgs(runsetExecutor);
        }

        public struct Arg
        {
            public string prefix;
            public string param;
            public string value;
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
                string arg = aargval[0].Trim();
                string value = aargval[1].Trim();

                args.Add(new Arg() { prefix = parampref, param = arg, value = value  });
                parampref = "-";
            }

            return args;
        }
    }
}
