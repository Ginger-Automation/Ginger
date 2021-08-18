using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Amdocs.Ginger.CoreNET.GlobalSolutionLib;
using Amdocs.Ginger.Repository;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.WizardLib;
using GingerCore;
using GingerCore.DataSource;
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
        NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();
        RepositoryItemBase repoItemToImport = null;
        public ImportItemWizard()
        {
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Global Solution Introduction", Page: new WizardIntroPage("/GlobalSolutionLib/ImportItemWizardLib/ImportItemIntro.md"));
            
            AddPage(Name: "Select Item Source Type", Title: "Select Item Source Type", SubTitle: "Choose ...", Page: new SelectItemImportTypePage());

            AddPage(Name: "Select Item Type To Import", Title: "Select Item Type To Import", SubTitle: "Choose ...", Page: new SelectItemTypesToImportPage());

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
                    foreach (GlobalSolutionItem itemToAdd in SelectedItemTypeListToImport.Where(x => x.Selected).ToList())
                    {
                        string sourceFile = itemToAdd.ItemExtraInfo;
                        string targetFile = string.Empty;
                        //Get subdirectory path
                        string path = Path.GetDirectoryName(itemToAdd.ItemExtraInfo);
                        string folderPath = path.Replace(SolutionFolder + "\\", "");

                        //if (string.IsNullOrEmpty(itemToAdd.Comments))
                        if (string.IsNullOrEmpty(itemToAdd.ItemNewName))
                        {
                            targetFile = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, folderPath, Path.GetFileName(itemToAdd.ItemExtraInfo));
                        }
                        else
                        {
                            string newFileName = GlobalSolutionUtils.Instance.GetUniqFileName(sourceFile);
                            targetFile = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, folderPath, newFileName);
                        }

                        //targetFile = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, folderPath, Path.GetFileName(item.ItemExtraInfo));

                        switch (itemToAdd.ItemType)
                        {
                            case GlobalSolution.eImportItemType.Documents:
                                AddItemToSolution(sourceFile, targetFile, false, itemToAdd);
                                break;
                            case GlobalSolution.eImportItemType.Environments:
                                AddItemToSolution(sourceFile, targetFile, true, itemToAdd);
                                break;
                            case GlobalSolution.eImportItemType.DataSources:
                                AddItemToSolution(sourceFile, targetFile, true, itemToAdd);

                                //DataSourceBase dataSource = (DataSourceBase)newRepositorySerializer.DeserializeFromFile(sourceFile);
                                //sourceFile = dataSource.FilePath.Replace("~", SolutionFolder);
                                //targetFile = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, folderPath, Path.GetFileName(sourceFile));
                                //GlobalSolutionItem newItem = new GlobalSolutionItem(GlobalSolution.eImportItemType.DataSources, sourceFile, true, Path.GetFileName(sourceFile), true);
                                //AddItemToSolution(sourceFile, targetFile, false, newItem);

                                //if (Path.GetExtension(targetFile) != ".xml")
                                //{
                                //    AddItemToSolution(sourceFile, targetFile, false, itemToAdd);
                                //}
                                //else
                                //{
                                //    AddItemToSolution(sourceFile, targetFile, true, itemToAdd);
                                //}
                                break;
                        }


                    }
                }
            }
        }

        void AddItemToSolution(string sourceFile, string targetFile, bool saveAsRepo, GlobalSolutionItem itemToImport)
        {
            switch (itemToImport.ItemImportSetting)
            {
                case GlobalSolution.eImportSetting.ReplaceExsiting:
                    //if (File.Exists(targetFile))
                    {
                        if (GlobalSolutionUtils.Instance.IsGingerRepositoryItem(sourceFile))
                        {
                            RepositoryItemBase repositoryItem = newRepositorySerializer.DeserializeFromFile(sourceFile);
                            RepositoryItemBase repoItem = GlobalSolutionUtils.Instance.GetRepositoryItemByGUID(itemToImport, repositoryItem);
                            if (repoItem != null)
                            {
                                //repositoryItem.ContainingFolder = Path.GetDirectoryName(targetFile);
                                //repositoryItem.FilePath = targetFile;
                                //repositoryItem.FileName = targetFile;//Needed inside MoveSharedRepositoryItemToPrevVersion
                                WorkSpace.Instance.SolutionRepository.MoveSharedRepositoryItemToPrevVersion(repoItem);
                                if (itemToImport.ItemType == GlobalSolution.eImportItemType.DataSources)
                                {
                                    DataSourceBase dataSource = (DataSourceBase)repoItem;
                                    string dsFile = WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(dataSource.FilePath);
                                    GlobalSolutionUtils.Instance.KeepBackupAndDeleteFile(dsFile);
                                    //string directory = Path.GetDirectoryName(dsFile);
                                    ////string ext = Path.GetExtension(dsFile);
                                    //string fileName = Path.GetFileName(dsFile);
                                    //string bkpDateTime = System.Text.RegularExpressions.Regex.Replace(DateTime.Now.ToString(), @"[^0-9a-zA-Z]+", "");
                                    ////keep the backup to PrevVersion folder
                                    //string bkpPath = Path.Combine(directory, "PrevVersions",fileName);
                                    //File.Copy(dsFile, bkpPath + "." + bkpDateTime + ".bak");
                                    //File.Delete(dsFile);

                                    //File.Copy(dsFile, dsFile + "." + bkpDateTime + ".bak");
                                    //File.Delete(dsFile);
                                }
                            }
                        }
                        else
                        {
                            //string bkpDateTime = System.Text.RegularExpressions.Regex.Replace(DateTime.Now.ToString(), @"[^0-9a-zA-Z]+", "");
                            ////keep the backup 
                            //File.Copy(targetFile, targetFile + "." + bkpDateTime + ".bak");
                            //File.Delete(targetFile);
                            GlobalSolutionUtils.Instance.KeepBackupAndDeleteFile(targetFile);
                        }

                    }
                    break;
            }

            if (saveAsRepo)
            {
                repoItemToImport = newRepositorySerializer.DeserializeFromFile(sourceFile);
                repoItemToImport.ContainingFolder = Path.GetDirectoryName(targetFile);
                repoItemToImport.FilePath = targetFile;
                if (!string.IsNullOrEmpty(itemToImport.ItemNewName))
                {
                    repoItemToImport.ItemName = itemToImport.ItemNewName;
                }
                if (itemToImport.ItemType == GlobalSolution.eImportItemType.DataSources)
                {
                    DataSourceBase dataSource = (DataSourceBase)repoItemToImport;
                    sourceFile = dataSource.FilePath.Replace("~", SolutionFolder);
                    string dsFile = WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(dataSource.FilePath);
                    string directory = Path.GetDirectoryName(dsFile);
                    string ext = Path.GetExtension(dsFile);
                    string fileName = Path.GetFileName(dsFile);

                    string newFile = string.Empty;
                    if (!string.IsNullOrEmpty(itemToImport.ItemNewName))
                    {
                        newFile = Path.Combine(directory, itemToImport.ItemNewName + ext);
                    }
                    else
                    {
                        newFile = Path.Combine(directory, fileName);
                    }
                    //
                    File.Copy(sourceFile, newFile);
                }
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(repoItemToImport);
            }
            else
            {
                File.Copy(sourceFile, targetFile);
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
