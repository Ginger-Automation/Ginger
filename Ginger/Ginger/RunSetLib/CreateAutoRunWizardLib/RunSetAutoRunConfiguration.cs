using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.Run;
using GingerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.RunSetLib
{
    public class RunSetAutoRunConfiguration
    {
        public RunSetConfig mRunsetConfig;

        public ICLI SelectedCLI;

        public string ConfigFileContent { get; set; }

        string mConfigFileFolderPath = null;
        public string ConfigFileFolderPath
        {
            get
            {
                if (mConfigFileFolderPath == null)
                {
                    //defualt folder
                    string SolFolder = WorkSpace.Instance.Solution.Folder;
                    if (SolFolder.EndsWith(@"\"))
                    {
                        SolFolder = SolFolder.Substring(0, SolFolder.Length - 1);
                    }
                    mConfigFileFolderPath = SolFolder + @"\Documents\RunSetShortCuts\";
                    if (!System.IO.Directory.Exists(mConfigFileFolderPath))
                    {
                        System.IO.Directory.CreateDirectory(SolFolder + @"\Documents\RunSetShortCuts\");
                    }
                }
                return mConfigFileFolderPath;
            }
            set
            {
                mConfigFileFolderPath = value;
            }
        }

        public string ConfigFileName
        {
            get
            {
                return FileUtils.RemoveInvalidChars(mRunsetConfig.Name) + ".Ginger.AutoRunConfigs." + SelectedCLI.FileExtension;
            }
        }

        public RunSetAutoRunConfiguration(RunSetConfig runSetConfig)
        {
            mRunsetConfig = runSetConfig;
        }
    }
}
