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
                if (ExecutorType == eExecutorType.GingerExe)
                {
                    return Path.Combine(ExecuterFolderPath, "Ginger.exe");
                }
                else
                {
                    return Path.Combine(ExecuterFolderPath, "GingerConsole.dll");
                }
            }
        }

        public string ShortcutContent
        {
            get
            {
                return ExecuterFullPath + " " + mAutoRunConfiguration.SelectedCLI.Identifier + "=\"" + mAutoRunConfiguration.ConfigArgs + "\"";
            }
        }
    }
}
