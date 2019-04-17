using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.Run;
using Ginger.WizardLib;
using GingerCore.Environments;
using GingerWPF.WizardLib;
using IWshRuntimeLibrary;
using System;
using System.Linq;

namespace Ginger.RunSetLib.CreateCLIWizardLib
{
    public class CreateCLIWizard : WizardBase
    {
        
        public enum CLIType
        {
            ConfigFile,
            ScriptFile,
            Dynamic,
            Params
        }

        public override string Title { get { return "Create CLI"; } }

        public RunSetConfig RunSetConfig { get; set; }

        public ProjEnvironment ProjEnvironment { get; set; }

        public CLIType cLIType { get; set; }

        public string FileContent { get; set; }  // st raise event

        public ICLI iCLI;

        public CreateCLIWizard()
        {            
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "CLI Introduction", Page: new WizardIntroPage("/RunSetLib/CreateCLIWizardLib/CreateCLI.md"));            
            AddPage(Name: "CLI Type", Title: "CLI Type", SubTitle: "CLI Type", Page: new CreateCLIChooseTypePage());
            AddPage(Name: "CLI Location", Title: "CLI Location", SubTitle: "CLI Location", Page: new CreateCLILocationPage());
            AddPage(Name: "CLI Options", Title: "CLI Options", SubTitle: "CLI Options", Page: new CLIOptionsPage());
            
        }

        public override void Finish()
        {
            switch (cLIType)
            {
                case CLIType.ConfigFile:
                    CreateRunSetShortCut();
                    break;
            }
        }

        private void CreateRunSetShortCut()
        {
            string Env = ProjEnvironment.Name;

            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();

            if (RunSetConfig.Name.IndexOfAny(invalidChars) >= 0)
            {
                foreach (char value in invalidChars)
                {
                    // We should not change the Name !!!!!!!!!!!!!!!!
                    if (RunSetConfig.Name.Contains(value))
                    {
                        RunSetConfig.Name = RunSetConfig.Name.Replace(value, '_');
                    }
                }
            }



            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Ginger " + RunSetConfig + " " + Env + ".lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "Ginger Solution=" + WorkSpace.Instance.Solution.Name + ", RunSet=" + RunSetConfig + ", Env=" + Env;
            string GingerPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string SolFolder = WorkSpace.Instance.Solution.Folder;
            if (SolFolder.EndsWith(@"\"))
            {
                SolFolder = SolFolder.Substring(0, SolFolder.Length - 1);
            }
            shortcut.TargetPath = GingerPath;
            string sConfig = "Solution=" + SolFolder + Environment.NewLine;
            sConfig += "Env=" + Env + Environment.NewLine;
            sConfig += "RunSet=" + RunSetConfig + Environment.NewLine;
            string sConfigFile = SolFolder + @"\Documents\RunSetShortCuts\" + RunSetConfig + "_" + Env + ".Ginger.Config";

            if (!System.IO.Directory.Exists(SolFolder + @"\Documents\RunSetShortCuts\")) { System.IO.Directory.CreateDirectory(SolFolder + @"\Documents\RunSetShortCuts\"); }
            System.IO.File.WriteAllText(sConfigFile, sConfig);

            shortcut.Arguments = "ConfigFile=\"" + sConfigFile + "\"";
            shortcut.Save();
            Reporter.ToUser(eUserMsgKey.ShortcutCreated, shortcut.Description);
        }

    }
}
