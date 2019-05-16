using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.RunSetLib
{
    public class RunSetAutoRunOptions
    {
        public enum eExecutorType { GingerExe, GingerConsole}

        public bool DownloadSolutionFromSourceControl { get; set; }

        public eExecutorType ExecutorType; // Ginger.exe or dotnet GingerConsole.dll

        string mExecuterFolderPath = null;
        public string ExecuterFolderPath
        {
            get
            {
                if (mExecuterFolderPath == null)
                {
                    //defualt folder
                    mExecuterFolderPath = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("Ginger.exe", "");
                }
                return mExecuterFolderPath;
            }
            set
            {
                mExecuterFolderPath = value;
            }
        }

        public bool ShowAutoRunWindow { get; set; }

        public eAppReporterLoggingLevel AppLoggingLevel { get; set; } = eAppReporterLoggingLevel.Debug;

        public bool RunAnalyzer { get; set; }

        //public void SetGingerExecutor()
        //{
        //    WorkingDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("Ginger.exe", "");
        //    CLIExecutor = "Ginger.exe";
        //}

        //public void SetGingerConsoleExecutor()
        //{
        //    WorkingDirectory = ""; // TODO: find GingerConsole !!!!
        //    CLIExecutor = "dotnet GingerConsole.dll";
        //}
    }
}
