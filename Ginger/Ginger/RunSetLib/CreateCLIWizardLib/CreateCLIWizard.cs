using System;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.WizardLib;
using GingerUtils;
using GingerWPF.WizardLib;
using IWshRuntimeLibrary;

namespace Ginger.RunSetLib.CreateCLIWizardLib
{
    public class CreateCLIWizard : WizardBase
    {       
        public override string Title { get { return "Create CLI"; } }

        //public RunSetConfig RunSetConfig { get; set; }
        public string FileContent { get; set; }  
        public bool DownloadSolutionFromSourceControl { get; set; }
        public bool RunAnalyzer { get; set; }

        public ICLI SelectedCLI;

        public string CLIExecutor { get; set; }  // Ginger.exe or dotnet GingerConsole.dll

        public string ShortcutDescription { get; set; }
        public string CLIFileName
        {
            get
            {
                string SolFolder = WorkSpace.Instance.Solution.Folder;
                if (SolFolder.EndsWith(@"\"))
                {
                    SolFolder = SolFolder.Substring(0, SolFolder.Length - 1);
                }
                
                string fileName = SolFolder + @"\Documents\RunSetShortCuts\" + FileUtils.RemoveInvalidChars(ShortcutDescription) + ".Ginger." + SelectedCLI.FileExtension;

                if (!System.IO.Directory.Exists(SolFolder + @"\Documents\RunSetShortCuts\"))
                {
                    System.IO.Directory.CreateDirectory(SolFolder + @"\Documents\RunSetShortCuts\");
                }

                return fileName;
            }
        }
        

        public CreateCLIWizard()
        {            
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "CLI Introduction", Page: new WizardIntroPage("/RunSetLib/CreateCLIWizardLib/CreateCLI.md"));
            AddPage(Name: "CLI Options", Title: "CLI Options", SubTitle: "CLI Options", Page: new CLIOptionsPage());                        
            AddPage(Name: "CLI Type", Title: "CLI Type", SubTitle: "CLI Type", Page: new CreateCLIChooseTypePage());
            AddPage(Name: "CLI Location", Title: "CLI Location", SubTitle: "CLI Location", Page: new CreateCLILocationPage());
        }


        public void SetGingerExecutor()
        {
            CLIExecutor = System.Reflection.Assembly.GetExecutingAssembly().Location;
        }

        public void SetGingerConsoleExecutor()
        {
            CLIExecutor = "dotnet GingerConsole.dll";
        }

        string CLIFolder;
        public override void Finish()
        {            
            WshShell shell = new WshShell();
            if (string.IsNullOrEmpty(CLIFolder))
            {
                object shDesktop = (object)"Desktop";
                CLIFolder = (string)shell.SpecialFolders.Item(ref shDesktop);
            }            

            string shortcutAddress = CLIFolder + @"\" + ShortcutDescription + ".lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = ShortcutDescription;             
            shortcut.TargetPath = CLIExecutor;
            System.IO.File.WriteAllText(CLIFileName, FileContent);
            shortcut.Arguments = SelectedCLI.Identifier + "=\"" + CLIFileName + "\"";   
            shortcut.Save();
            Reporter.ToUser(eUserMsgKey.ShortcutCreated, shortcut.Description);
        }
        internal void SetCLIFolder(string text = null)
        {
            CLIFolder = text;
        }



    }
}
