using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.Run;
using Ginger.WizardLib;
using GingerCore;
using GingerWPF.WizardLib;
using IWshRuntimeLibrary;
using System;

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
                AutoRunConfiguration.CreateConfigFile();

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
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Error occured while creating the Auto Run Configuration/Shortcut." + Environment.NewLine + "Error: " + ex.Message);
            }
        }

        public void SaveShortcut()
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(AutoRunShortcut.ShortcutFileFullPath);
            shortcut.Description = AutoRunShortcut.ShortcutFileName;
            shortcut.WorkingDirectory = AutoRunShortcut.ExecuterFolderPath;
            shortcut.TargetPath = AutoRunShortcut.ExecuterFullPath;
            shortcut.Arguments = AutoRunConfiguration.SelectedCLI.Identifier + "=\"" + AutoRunConfiguration.ConfigArgs + "\"";
            shortcut.Save();
        }

    }
}
