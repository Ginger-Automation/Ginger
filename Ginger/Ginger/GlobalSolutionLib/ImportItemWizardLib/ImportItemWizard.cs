using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Amdocs.Ginger.Repository;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.WizardLib;
using GingerCore;
using GingerCore.Environments;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
namespace Ginger.GlobalSolutionLib.ImportItemWizardLib
{
    class ImportItemWizard : WizardBase
    {
        public GlobalSolution.eImportFromType ImportFromType = GlobalSolution.eImportFromType.LocalFolder;
        public string SolutionFolder { get; set; }
        public List<object> SelectedItems { get; set; }

        public List<string> ItemTypesList = Enum.GetNames(typeof(GlobalSolution.eImportItemType)).ToList();

        public ObservableList<GlobalSolutionItem> ItemTypeListToImport = null;
        public ObservableList<GlobalSolutionItem> ItemsListToImport = null;
        public ObservableList<GlobalSolutionItem> SelectedItemTypeListToImport = null;

        public ImportItemWizard()
        {
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Global Solution Introduction", Page: new WizardIntroPage("/GlobalSolutionLib/ImportItemWizardLib/ImportItemIntro.md"));
            
            AddPage(Name: "Select Item Source Type", Title: "Select Item Source Type", SubTitle: "Choose ...", Page: new SelectItemImportTypePage());

            AddPage(Name: "Select Solution Item", Title: "Select Solution Item", SubTitle: "Select Solution Item...", Page: new SelectItemFromSolutionPage());

            AddPage(Name: "Item Dependancy Mapping", Title: "Item Dependancy Mapping", SubTitle: "Item Dependancy Mapping...", Page: new ItemDependancyPage());

        }

        public override string Title { get { return "Import Global Solution Wizard"; } }

        public override void Finish()
        {
            if (ImportFromType == GlobalSolution.eImportFromType.LocalFolder)
            {
                if (!string.IsNullOrEmpty(SolutionFolder))
                {
                    foreach (GlobalSolutionItem item in SelectedItemTypeListToImport.Where(x => x.Selected).ToList())
                    {
                        string sourceFile = item.ItemExtraInfo; ;
                        string targetFile = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, item.ItemType.ToString(), Path.GetFileName(item.ItemExtraInfo));

                        switch (item.ItemType)
                        {
                            case GlobalSolution.eImportItemType.Documents:
                                AddItemToSolution(sourceFile, targetFile, false, item.ItemImportSetting);
                                break;
                            case GlobalSolution.eImportItemType.Environments:
                                AddItemToSolution(sourceFile, targetFile, true, item.ItemImportSetting);
                                break;
                            case GlobalSolution.eImportItemType.DataSources:
                                if (Path.GetExtension(targetFile) != ".xml")
                                {
                                    AddItemToSolution(sourceFile, targetFile, false, item.ItemImportSetting);
                                }
                                else
                                {
                                    AddItemToSolution(sourceFile, targetFile, true, item.ItemImportSetting);
                                }
                                break;
                        }


                    }
                }
            }
        }
        NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();
        RepositoryItemBase importedItem = null;
        void AddItemToSolution(string sourceFile, string targetFile, bool saveAsRepo, GlobalSolution.eImportSetting eImportSetting)
        {
            switch (eImportSetting)
            {
                case GlobalSolution.eImportSetting.CreateNew:
                    //
                    break;
                case GlobalSolution.eImportSetting.ReplaceExsiting:
                    if (File.Exists(targetFile))
                    {
                        string bkpDateTime = System.Text.RegularExpressions.Regex.Replace(DateTime.Now.ToString(), @"[^0-9a-zA-Z]+", "");
                        //keep the backup 
                        File.Copy(targetFile, targetFile + "." +bkpDateTime + ".bak");
                        File.Delete(targetFile);

                        //File.WriteAllText(targetFile, File.ReadAllText(sourceFile));
                        File.Copy(sourceFile, targetFile);
                        return;
                    }
                    
                    break;

                case GlobalSolution.eImportSetting.KeepLocal:
                    // Nothing to do 
                    return;
                    break;
            }

            if (saveAsRepo)
            {
                importedItem = newRepositorySerializer.DeserializeFromFile(sourceFile);
                importedItem.ContainingFolder = Path.GetDirectoryName(targetFile);
                importedItem.FilePath = targetFile;
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(importedItem);
            }
            else
            {
                if (!File.Exists(targetFile))
                {
                    File.Copy(sourceFile, targetFile);
                }
            }


            //if (!File.Exists(targetFile))
            //{
            //    File.Copy(sourceFile, targetFile);
            //}
            //if (saveAsRepo)
            //{
            //    importedItem = newRepositorySerializer.DeserializeFromFile(sourceFile);
            //    importedItem.ContainingFolder = Path.GetDirectoryName(targetFile);
            //    importedItem.FilePath = targetFile;
            //    //WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(importedItem);
            //    WorkSpace.Instance.SolutionRepository.AddRepositoryItem(importedItem);
            //}
        }

    }
}
