using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.WizardLib;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
namespace Ginger.GlobalSolutionLib.ImportItemWizardLib
{
    class ImportItemWizard : WizardBase
    {
        public GlobalSolution.ImportFromType ImportFromType = GlobalSolution.ImportFromType.LocalFolder;
        public string SolutionFolder { get; set; }
        public List<object> SelectedItems { get; set; }

        //public List<string> ItemTypeListToImport = Enum.GetNames(typeof(GlobalSolution.ImportItemType)).ToList();

        public ObservableList<GlobalSolutionItem> ItemTypeListToImport = null;

        public ObservableList<GlobalSolutionItem> SelectedItemTypeListToImport = null;

        public ImportItemWizard()
        {
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Global Solution Introduction", Page: new WizardIntroPage("/GlobalSolutionLib/ImportItemWizardLib/ImportItemIntro.md"));
            
            AddPage(Name: "Select Item Source Type", Title: "Select Item Source Type", SubTitle: "Choose ...", Page: new SelectItemImportTypePage());

            AddPage(Name: "Select Solution Item", Title: "Select Solution Item", SubTitle: "Select Solution Item...", Page: new SelectItemFromSolutionPage());

            AddPage(Name: "Item Dependancy Mapping", Title: "Item Dependancy Mapping", SubTitle: "Item Dependancy Mapping...", Page: new ItemDependancyPage());

            //AddPage(Name: "Select Solution From Source Control", Title: "Select Solution From Source Control", SubTitle: "Select Solution From Source Control...", Page: new SelectImportFromSourceControlPage());

        }

        public override string Title { get { return "Import Global Solution Wizard"; } }

        public override void Finish()
        {
            if (ImportFromType == GlobalSolution.ImportFromType.LocalFolder)
            {
                if (!string.IsNullOrEmpty(SolutionFolder))
                {
                    //foreach (object item in SelectedItems)
                    //{
                    //    //check itemType
                    //    string sourceFile = Path.Combine(SolutionFolder, "Documents", item.ToString());
                    //    string targetFile = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, "Documents", item.ToString());

                    //    if (!File.Exists(targetFile))
                    //    {
                    //        File.Copy(sourceFile, targetFile);
                    //    }
                    //}

                    foreach (GlobalSolutionItem item in SelectedItemTypeListToImport)
                    {
                        //check itemType
                        string sourceFile = string.Empty;
                        string targetFile = string.Empty;
                        //sourceFile = Path.Combine(SolutionFolder, item.ItemType.ToString(), item.ItemExtraInfo);
                        sourceFile = item.ItemExtraInfo;
                        targetFile = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, item.ItemType.ToString(), Path.GetFileName(item.ItemExtraInfo));
                        if (!File.Exists(targetFile))
                        {
                            File.Copy(sourceFile, targetFile);
                        }
                        //switch (item.ItemType)
                        //{
                        //    case GlobalSolution.ImportItemType.Documents:
                        //    case GlobalSolution.ImportItemType.Environments:
                        //    case GlobalSolution.ImportItemType.DataSources:
                        //        sourceFile = Path.Combine(SolutionFolder, item.ItemType.ToString(), item.ToString());
                        //        targetFile = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, item.ItemType.ToString(), item.ToString());
                        //        if (!File.Exists(targetFile))
                        //        {
                        //            File.Copy(sourceFile, targetFile);
                        //        }
                        //        break;


                        //}


                    }
                }
            }
        }

        
    }
}
