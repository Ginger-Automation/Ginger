using System;
using System.IO;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.Run;
using Ginger.WizardLib;
using GingerCore;
using GingerUtils;
using GingerWPF.WizardLib;
using IWshRuntimeLibrary;

namespace Ginger.RunSetLib.CreateCLIWizardLib
{
    public class AutoRunWizard : WizardBase
    {
        public override string Title { get { return string.Format("Create {0} Auto Run Configuration", GingerDicser.GetTermResValue(eTermResKey.RunSet)); } }

        public RunSetConfig mRunsetConfig;

        public Context mContext;

        public CLIHelper mCLIHelper;

        public RunSetAutoRunOptions AutoRunOptions;

        public RunSetAutoRunConfiguration AutoRunConfiguration;

        public RunSetAutoRunShortcut AutoRunShortcut;


        public AutoRunWizard(RunSetConfig runSetConfig, Context context)
        {
            mRunsetConfig = runSetConfig;
            mContext = context;
            mCLIHelper = new CLIHelper();
            AutoRunOptions = new RunSetAutoRunOptions();
            AutoRunConfiguration = new RunSetAutoRunConfiguration(runSetConfig);
            AutoRunShortcut = new RunSetAutoRunShortcut();

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Auto Run Configuration Introduction", Page: new WizardIntroPage("/RunSetLib/CreateCLIWizardLib/CreateCLI.md"));
            AddPage(Name: "Auto Run Options", Title: "Set Auto Run Configuration Options", SubTitle: "Set Auto Run Configuration Options", Page: new AutoRunWizardOptionsPage());                        
            AddPage(Name: "Auto Run Type", Title: "Set Auto Run Configuration Type", SubTitle: "Set Auto Run Configuration Type", Page: new AutoRunWizardCLITypePage());
            AddPage(Name: "Auto Run Shortcut", Title: "Create Auto Run Configuration Execution Shortcut", SubTitle: "Create Auto Run Configuration Execution Shortcut", Page: new AutoRunWizardShortcutPage());
        }

        public override void Finish()
        {
            try
            {
                // Write the content file with runset data
                System.IO.File.WriteAllText(AutoRunConfigurationFileName, AutoRunConfigurationFileContent);

                // Create windows shortcut
                WshShell shell = new WshShell();
                string shortcutAddress = Path.Combine(CLIFolder, ShortcutFileName + ".lnk");
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
                shortcut.Description = ShortcutFileName;
                shortcut.WorkingDirectory = WorkingDirectory;
                shortcut.TargetPath = Path.Combine(WorkingDirectory, CLIExecutor);
                shortcut.Arguments = SelectedCLI.Identifier + "=\"" + AutoRunConfigurationFileName + "\"";

                shortcut.Save();

                Reporter.ToUser(eUserMsgKey.ShortcutCreated, shortcut.Description);
            }
            catch(Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.ShortcutCreationFailed, ex.Message);
            }
        }

    }
}
