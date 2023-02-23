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

using GingerUtils;
using System;
using System.ComponentModel;
using System.IO;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class RunSetAutoRunShortcut : INotifyPropertyChanged
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

        public enum eExecutorType { GingerExe, GingerConsole }

        RunSetAutoRunConfiguration mAutoRunConfiguration;

        public RunSetAutoRunShortcut(RunSetAutoRunConfiguration autoRunConfiguration)
        {
            mAutoRunConfiguration = autoRunConfiguration;
        }

        public bool CreateShortcut;

        public bool StartExecution;
        public string ShortcutFileName{ get; set; }


        string mShortcutFolderPath = null;
        public string ShortcutFolderPath
        {
            get
            {
                if (mShortcutFolderPath == null)
                {
                    //defualt folder
                    mShortcutFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }
                return mShortcutFolderPath;
            }
            set
            {
                mShortcutFolderPath = value;
                OnPropertyChanged(nameof(ShortcutFolderPath));
            }
        }

        eExecutorType mExecutorType;
        public eExecutorType ExecutorType// Ginger.exe or dotnet GingerConsole.dll
        {
            get
            {
                return mExecutorType;
            }
            set
            {
                mExecutorType = value;
                OnPropertyChanged(nameof(ExecuterFolderPath));
                OnPropertyChanged(nameof(ShortcutContent));
            }
        }


        string mExecuterFolderPath = null;
        public string ExecuterFolderPath
        {
            get
            {
                if (mExecuterFolderPath == null)
                {
                    //defualt folder
                    mExecuterFolderPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                }
                return mExecuterFolderPath;
            }
            set
            {
                mExecuterFolderPath = value;
                OnPropertyChanged(nameof(ExecuterFolderPath));
                OnPropertyChanged(nameof(ShortcutContent));
            }
        }

        public string ShortcutFileFullPath
        {
            get
            {
                return Path.Combine(ShortcutFolderPath, FileUtils.RemoveInvalidChars(ShortcutFileName) + ".lnk");
            }
        }

        public string ExecuterFullPath
        {
            get
            {
                return Path.Combine(ExecuterFolderPath, "Ginger.exe");
            }
        }

        public string ShortcutContent
        {
            get
            {
                string args = mAutoRunConfiguration.ConfigArgs;
                string prefix = "";
                string command;
                if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("GINGER_HOME")))
                {
                    command = "ginger " + args;
                }
                else
                {
                    command = "\"" + ExecuterFullPath + "\" " + args;
                }
                    return command;
                
            }
        }
    }
}
