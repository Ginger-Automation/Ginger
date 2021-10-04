#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.Run;
using Ginger.WizardLib;
using GingerCore;
using GingerWPF.WizardLib;
using IWshRuntimeLibrary;
using System;
using System.IO;

namespace Ginger.RunSetLib.CreateCLIWizardLib
{
    public class AutoRunWizard : WizardBase
    {
        public override string Title { get { return string.Format("Create {0} Auto Run Configuration", GingerDicser.GetTermResValue(eTermResKey.RunSet)); } }

        public RunSetConfig RunsetConfig;

        public Context mContext;

        public CLIHelper CliHelper;

        public RunSetAutoRunConfiguration AutoRunConfiguration;

        public RunSetAutoRunShortcut AutoRunShortcut;


        public AutoRunWizard(RunSetConfig runSetConfig, Context context)
        {
            RunsetConfig = runSetConfig;
            mContext = context;
            CliHelper = new CLIHelper();           
            AutoRunConfiguration = new RunSetAutoRunConfiguration(WorkSpace.Instance.Solution, WorkSpace.Instance.RunsetExecutor, CliHelper);
            AutoRunShortcut = new RunSetAutoRunShortcut(AutoRunConfiguration);

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Auto Run Configuration Introduction", Page: new WizardIntroPage("/RunSetLib/CreateAutoRunWizardLib/AutoRunIntroduction.md"));
            AddPage(Name: "General Options", Title: "General Options", SubTitle: "Set Auto Run General Options", Page: new AutoRunWizardOptionsPage());                        
            AddPage(Name: "Configuration Type", Title: "Configuration Type", SubTitle: "Set Auto Run Configuration Type", Page: new AutoRunWizardCLITypePage());
            AddPage(Name: "Execution Shortcut", Title: "Execution Shortcut", SubTitle: "Create Auto Run Configuration Execution Shortcut", Page: new AutoRunWizardShortcutPage());
        }

        public override void Finish()
        {
            try
            {
                // Write Configuration file
                AutoRunConfiguration.CreateContentFile();

                // Create windows shortcut
                if (AutoRunShortcut.CreateShortcut)
                {
                    SaveShortcut();
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Auto Run Configuration and Shortcut were created successfully.");
                    return;
                }

                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Auto Run Configuration was created successfully.");
            }
            catch(Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Error occurred while creating the Auto Run Configuration/Shortcut." + Environment.NewLine + "Error: " + ex.Message);
            }
        }

        public void SaveShortcut()
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(AutoRunShortcut.ShortcutFileFullPath);
            shortcut.Description = AutoRunShortcut.ShortcutFileName;
            shortcut.WorkingDirectory = AutoRunShortcut.ExecuterFolderPath;
            if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("GINGER_HOME")))
            {                                
                shortcut.TargetPath = "ginger"; 
                shortcut.Arguments = AutoRunConfiguration.ConfigArgs;
            }
            else
            {
                shortcut.TargetPath = AutoRunShortcut.ExecuterFullPath;
                shortcut.Arguments = AutoRunConfiguration.ConfigArgs;
            }
            
            string iconPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "GingerIconNew.ico");
            if (System.IO.File.Exists(iconPath))
            {
                shortcut.IconLocation = iconPath;
            }

            shortcut.Save();
        }

    }
}
