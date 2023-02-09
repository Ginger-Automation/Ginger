#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerUtils;
using System.ComponentModel;
using System.IO;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public enum eAutoRunEexecutorType
    {
        Run,
        DynamicFile,
        Remote
    }
    public class RunSetAutoRunConfiguration: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        Solution mSolution;
        RunsetExecutor mRunsetExecutor;
        CLIHelper mCLIHelper;
        
        public int ParallelExecutionCount;
        public eAutoRunEexecutorType AutoRunEexecutorType { get; set; }


        public ICLI SelectedCLI;

        private string mCLIContent;
        public string CLIContent
        {
            get
            {
                if (mCLIContent == null)
                {
                    mCLIContent = GetCLIContent();
                }
                return mCLIContent;
            }
            set
            {
                if (mCLIContent != value)
                {
                    mCLIContent = value;
                }
            }
        }

        public string GetCLIContent()
        {
            return SelectedCLI.CreateConfigurationsContent(mSolution, mRunsetExecutor, mCLIHelper);
        }

        string mConfigFileFolderPath = null;
        public string ConfigFileFolderPath
        {
            get
            {
                if (mConfigFileFolderPath == null)
                {
                    //default folder
                    string SolFolder = mSolution.Folder;
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
                OnPropertyChanged(nameof(ConfigFileFolderPath));
            }
        }

        string mArtifactsPath = null;
        public string ArtifactsPath
        {
            get
            {
                if (mArtifactsPath == null)
                {
                    mArtifactsPath = WorkSpace.Instance.TestArtifactsFolder;                         
                }
                return mArtifactsPath;
            }
            set
            {
                mArtifactsPath = value;
                mCLIHelper.TestArtifactsFolder = value;
                OnPropertyChanged(nameof(ArtifactsPath));
            }
        }


        string mConfigName = null;
        public string ConfigName
        {
            get
            {
                if (mConfigName == null)
                {
                    //default name
                    mConfigName = FileUtils.RemoveInvalidChars(mSolution.Name + "-" + mRunsetExecutor.RunSetConfig.Name);
                }
                return mConfigName;
            }
            set
            {
                mConfigName = value;
            }
        }

        public string ExecutionServiceUrl
        {
            get
            {
                return mRunsetExecutor.RunSetConfig.ExecutionServiceURLUsed;
            }
            set
            {
                if (mRunsetExecutor.RunSetConfig.ExecutionServiceURLUsed != value)
                {
                    mRunsetExecutor.RunSetConfig.ExecutionServiceURLUsed = value;
                }
            }
        }
        public string ConfigFileName
        {
            get
            {
                return System.Text.RegularExpressions.Regex.Replace(FileUtils.RemoveInvalidChars(ConfigName) + ".Ginger.AutoRunConfigs." + SelectedCLI.FileExtension, @"\s+", ""); ;
            }
        }

        public RunSetAutoRunConfiguration(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            mSolution = solution;
            mRunsetExecutor = runsetExecutor;
            mCLIHelper = cliHelper;
        }

        public string ConfigFileFullPath
        {
            get
            {
               return Path.Combine(ConfigFileFolderPath, ConfigFileName);
            }
        }

        public void CreateContentFile()
        {
            if (SelectedCLI.IsFileBasedConfig)
            {
                System.IO.File.WriteAllText(ConfigFileFullPath, CLIContent);
            }
        }

        public string ConfigArgs
        {
            get
            {
                if (SelectedCLI.IsFileBasedConfig)
                {
                    return SelectedCLI.Verb +  " --" + CLIOptionClassHelper.FILENAME + " \"" + ConfigFileFullPath + "\"";
                }
                else
                {
                    return CLIContent;
                }
            }
        }
    }
}
