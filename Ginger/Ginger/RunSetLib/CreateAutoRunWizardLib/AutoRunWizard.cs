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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.Run;
using Ginger.WizardLib;
using GingerCore;
using GingerWPF.WizardLib;
using IWshRuntimeLibrary;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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
        public bool ResetCLIContent;

        public AutoRunWizard(RunSetConfig runSetConfig, Context context)
        {
            RunsetConfig = runSetConfig;
            mContext = context;
            CliHelper = new CLIHelper();           
            AutoRunConfiguration = new RunSetAutoRunConfiguration(WorkSpace.Instance.Solution, WorkSpace.Instance.RunsetExecutor, CliHelper);
            AutoRunShortcut = new RunSetAutoRunShortcut(AutoRunConfiguration);

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Auto Run Configuration Introduction", Page: new WizardIntroPage("/RunSetLib/CreateAutoRunWizardLib/AutoRunIntroduction.md"));
            AddPage(Name: "General Options", Title: "General Options", SubTitle: "Set Auto Run General Options", Page: new AutoRunWizardOptionsPage());                        
            AddPage(Name: "Execution Configurations", Title: "Execution Configurations", SubTitle: "Execution Configurations", Page: new AutoRunWizardCLITypePage());
            AddPage(Name: "Execute", Title: "Execute", SubTitle: "Execute", Page: new AutoRunWizardShortcutPage());
        }

        public override void Finish()
        {
            _ = Task.Run(() =>
            {
                try
                {
                    var userMsg = "";

                    // Write Configuration file
                    if (AutoRunConfiguration.SelectedCLI.IsFileBasedConfig)
                    {
                        AutoRunConfiguration.CreateContentFile();
                        userMsg += "Configuration file created successfully." + Environment.NewLine;
                    }

                    // Create windows shortcut
                    if (AutoRunShortcut.CreateShortcut && AutoRunConfiguration.AutoRunEexecutorType != eAutoRunEexecutorType.Remote)
                    {
                        userMsg += SaveShortcut();
                    }
                    if (AutoRunShortcut.StartExecution)
                    {
                        userMsg = ExecuteCommand(userMsg);
                    }
                    if (!string.IsNullOrEmpty(userMsg))
                    {
                        Reporter.ToUser(eUserMsgKey.RunsetAutoRunResult, "Please find auto run wizard outcomes:" + Environment.NewLine + userMsg);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Error occurred while creating the Auto Run Configuration/Shortcut/Execution." + Environment.NewLine + "Error: " + ex.Message);
                }
            });
            
        }

        private string ExecuteCommand(string userMsg)
        {
            try
            {
                var count = 1;
                var successCount = 0;
                var failCount = 0;
                if(AutoRunConfiguration.AutoRunEexecutorType == eAutoRunEexecutorType.Remote)
                {
                    Reporter.ToStatus(eStatusMsgKey.StaticStatusMessage, null, "Sending the execution requests is in progress...");
                }
                else
                {
                    Reporter.ToStatus(eStatusMsgKey.StaticStatusProcess, null, "Starting execution process is in progress...");
                }
                while (count <= AutoRunConfiguration.ParallelExecutionCount)
                {
                    try
                    {
                        if (AutoRunConfiguration.AutoRunEexecutorType == eAutoRunEexecutorType.Remote)
                        {
                            var responseString = new RemoteExecutionRequestConfig().ExecuteFromRunsetShortCutWizard(AutoRunConfiguration.ExecutionServiceUrl, AutoRunConfiguration.CLIContent);

                            if (responseString == "Created")
                            {
                                successCount++;
                            }
                            else
                            {
                                failCount++;
                                userMsg += "Failed to start remote execution, error: " + responseString + Environment.NewLine;
                            }
                        }
                        else
                        {
                            var args = AutoRunConfiguration.CLIContent;

                            if (AutoRunConfiguration.AutoRunEexecutorType == eAutoRunEexecutorType.DynamicFile)
                            {
                                args = "dynamic --filename " + AutoRunConfiguration.ConfigFileFullPath;
                            }
                            Process.Start(new ProcessStartInfo() { FileName = AutoRunShortcut.ExecuterFullPath, Arguments = args, UseShellExecute = true });
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        userMsg += "Execution process failed to start, error: " + ex.Message + Environment.NewLine;
                        Reporter.ToLog(eLogLevel.ERROR, "Execution process starting failed.", ex);
                        failCount++;
                    }

                    count++;
                }
                if (failCount == 0 && successCount > 0)
                {
                    userMsg += "Total " + successCount + " process/es started successfully." + Environment.NewLine;
                }
                else if (failCount > 0)
                {
                    userMsg += "Total " + successCount + " process/es started successfully." + Environment.NewLine + "Total " + failCount + " process failed to start." + Environment.NewLine;
                }

                return userMsg;
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
           
        }

        public string SaveShortcut()
        {
            var msg = string.Empty;
            try
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
                msg= "Execution shortcut file created successfully." + Environment.NewLine;
            }
            catch (Exception ex)
            {
                msg = "Execution shortcut file creation failed, error:" + ex + Environment.NewLine;
            }
            return msg;
        }

    }
}
