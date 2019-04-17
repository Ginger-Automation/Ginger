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
        
       
        public override string Title { get { return "Create CLI"; } }

        public RunSetConfig RunSetConfig { get; set; }

        public string FileContent { get; set; }  
        public bool DownloadSolutionFromSourceControl { get; set; }

        public ICLI SelectedCLI;

        public CreateCLIWizard()
        {            
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "CLI Introduction", Page: new WizardIntroPage("/RunSetLib/CreateCLIWizardLib/CreateCLI.md"));            
            AddPage(Name: "CLI Type", Title: "CLI Type", SubTitle: "CLI Type", Page: new CreateCLIChooseTypePage());
            AddPage(Name: "CLI Location", Title: "CLI Location", SubTitle: "CLI Location", Page: new CreateCLILocationPage());
            AddPage(Name: "CLI Options", Title: "CLI Options", SubTitle: "CLI Options", Page: new CLIOptionsPage());
            
        }

        public override void Finish()
        {
            // SelectedCLIType.CreateCLI();            
            string gingerStarter = "ginger.exe";  // or GingerConsole
            string fileName = @"c:\1234";
            string Args = string.Format("ConfigFile={0}", fileName);
            string content = FileContent;

            // System.IO.File.WriteAllText(fileName, content);
            CreateRunSetShortCut();
        }

        private void CreateRunSetShortCut()
        {
            string Env = WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Name;

            //char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();

            //if (RunSetConfig.Name.IndexOfAny(invalidChars) >= 0)
            //{
            //    foreach (char value in invalidChars)
            //    {
            //        // We should not change the Name !!!!!!!!!!!!!!!!
            //        if (RunSetConfig.Name.Contains(value))
            //        {
            //            RunSetConfig.Name = RunSetConfig.Name.Replace(value, '_');
            //        }
            //    }
            //}



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
            //string sConfig = "Solution=" + SolFolder + Environment.NewLine;
            //sConfig += "Env=" + Env + Environment.NewLine;
            //sConfig += "RunSet=" + RunSetConfig + Environment.NewLine;

            // TODO: create extension per type !!!!!!!!!!!!!!!!!!

            string fileName = SolFolder + @"\Documents\RunSetShortCuts\" + WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name + "_" + Env + ".Ginger.Config";

            if (!System.IO.Directory.Exists(SolFolder + @"\Documents\RunSetShortCuts\"))
            {
                System.IO.Directory.CreateDirectory(SolFolder + @"\Documents\RunSetShortCuts\");
            }
            System.IO.File.WriteAllText(fileName, FileContent);

            shortcut.Arguments = SelectedCLI.Identifier + "=\"" + fileName + "\"";
            shortcut.Save();
            Reporter.ToUser(eUserMsgKey.ShortcutCreated, shortcut.Description);
        }



    }
}
