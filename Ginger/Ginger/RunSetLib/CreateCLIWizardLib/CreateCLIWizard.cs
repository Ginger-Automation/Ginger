using System;
using System.IO;
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
        public override string Title { get { return "Create Ginger Auto Run Configuration"; } }

        //public RunSetConfig RunSetConfig { get; set; }
        public string FileContent { get; set; }  
        public bool DownloadSolutionFromSourceControl { get; set; }        

        public ICLI SelectedCLI;

        public string CLIExecutor { get; set; }
        // Ginger.exe or dotnet GingerConsole.dll
        bool mShowAutoRunWindow;
        public bool ShowAutoRunWindow
        {
            get
            {
                return mShowAutoRunWindow;
            }
            set
            {
                mShowAutoRunWindow = value;
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("ShowAutoRunWindow {0}", value));
            }
        }

        public string ShortcutDescription { get; set; }

        public eAppReporterLoggingLevel AppLoggingLevel { get; set; } =  eAppReporterLoggingLevel.Debug;

        public bool RunAnalyzer { get;set; }

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
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Auto Run Configuration Introduction", Page: new WizardIntroPage("/RunSetLib/CreateCLIWizardLib/CreateCLI.md"));
            AddPage(Name: "Auto Run Configuration Options", Title: "Auto Run Configuration Options", SubTitle: "CLI Options", Page: new CLIOptionsPage());                        
            AddPage(Name: "Auto Run Configuration Type", Title: "Auto Run Configuration Type", SubTitle: "CLI Type", Page: new CreateCLIChooseTypePage());
            AddPage(Name: "Auto Run Configuration Location", Title: "Auto Run Configuration Location", SubTitle: "CLI Location", Page: new CreateCLILocationPage());
        }


        public void SetGingerExecutor()
        {
            WorkingDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("Ginger.exe", "");
            CLIExecutor = "Ginger.exe";
        }

        public void SetGingerConsoleExecutor()
        {
            WorkingDirectory = ""; // TODO: find GingerConsole !!!!
            CLIExecutor = "dotnet GingerConsole.dll";
        }

        string CLIFolder;
        internal void SetCLIFolder(string text = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                CLIFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else
            {
                CLIFolder = text;
            }
        }

        
        string WorkingDirectory;
        public override void Finish()
        {
            try
            {
                // Write the content file with runset data
                System.IO.File.WriteAllText(CLIFileName, FileContent);

                // Create windows shortcut
                WshShell shell = new WshShell();
                string shortcutAddress = Path.Combine(CLIFolder, ShortcutDescription + ".lnk");
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
                shortcut.Description = ShortcutDescription;
                shortcut.WorkingDirectory = WorkingDirectory;
                shortcut.TargetPath = Path.Combine(WorkingDirectory, CLIExecutor);
                shortcut.Arguments = SelectedCLI.Identifier + "=\"" + CLIFileName + "\"";

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
