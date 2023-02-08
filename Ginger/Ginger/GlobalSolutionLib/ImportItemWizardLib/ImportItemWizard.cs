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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.GlobalSolutionLib;
using Amdocs.Ginger.Repository;
using Ginger.SolutionGeneral;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.WizardLib;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
namespace Ginger.GlobalSolutionLib.ImportItemWizardLib
{
    class ImportItemWizard : WizardBase
    {
        public GlobalSolution.eImportFromType ImportFromType = GlobalSolution.eImportFromType.LocalFolder;
        public string SolutionFolder { get; set; } 
        public string EncryptionKey { get; set; } 
        public List<object> SelectedItems { get; set; }

        public List<string> ItemTypesList = Enum.GetNames(typeof(GlobalSolution.eImportItemType)).ToList();

        public ObservableList<GlobalSolutionItem> ItemTypeListToImport = null;
        public ObservableList<GlobalSolutionItem> ItemsListToImport = new ObservableList<GlobalSolutionItem>();
        public ObservableList<GlobalSolutionItem> SelectedItemsListToImport = new ObservableList<GlobalSolutionItem>();
        public List<VariableBase> VariableListToImport = new List<VariableBase>();
        public List<EnvApplication> EnvAppListToImport = new List<EnvApplication>();

        NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();
        public ImportItemWizard()
        {
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Global Solution Introduction", Page: new WizardIntroPage("/GlobalSolutionLib/ImportItemWizardLib/ImportItemIntro.md"));
            
            AddPage(Name: "Select Item Source Type", Title: "Select Item Source Type", SubTitle: "Choose ...", Page: new SelectItemImportTypePage());

            AddPage(Name: "Select Item Types", Title: "Select Item Types", SubTitle: "Choose ...", Page: new SelectItemTypesToImportPage());

            AddPage(Name: "Select Solution Items", Title: "Select Solution Items", SubTitle: "Select Solution Items...", Page: new SelectItemFromSolutionPage());

            AddPage(Name: "Solution Items Dependency Validation", Title: "Solution Items Dependency Validation", SubTitle: "Solution Items Dependency Validation...", Page: new ItemDependancyPage());

        }

        public override string Title { get { return "Import Global Cross Solution Wizard"; } }

        public override void Finish()
        {
            try
            {
                ProcessStarted();
                ValidateAndAddItemToSolution().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
            finally
            {
                ProcessEnded();
            }
        }

        private async Task ValidateAndAddItemToSolution()
        {
            if (!string.IsNullOrEmpty(SolutionFolder))
            {
                foreach (GlobalSolutionItem itemToAdd in SelectedItemsListToImport.Where(x => x.Selected).ToList())
                {
                    try
                    {
                        string sourceFile = itemToAdd.ItemFullPath;
                        string targetFile = string.Empty;
                        //Get subdirectory path
                        string path = Path.GetDirectoryName(itemToAdd.ItemFullPath);
                        string folderPath = path.Replace(SolutionFolder, "");

                        if (string.IsNullOrEmpty(itemToAdd.ItemNewName))
                        {
                            targetFile = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, folderPath, Path.GetFileName(itemToAdd.ItemExtraInfo));
                        }
                        else
                        {
                            string newFileName = GlobalSolutionUtils.Instance.GetUniqFileName(Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, folderPath, Path.GetFileName(itemToAdd.ItemExtraInfo)));
                            targetFile = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, folderPath, newFileName);
                        }

                        switch (itemToAdd.ItemType)
                        {
                            case GlobalSolution.eImportItemType.Documents:
                                AddItemToSolution(sourceFile, targetFile, false, itemToAdd);
                                break;
                            case GlobalSolution.eImportItemType.Environments:
                                GlobalSolutionUtils.Instance.EnvParamsToReEncrypt(sourceFile, itemToAdd);
                                AddItemToSolution(sourceFile, targetFile, true, itemToAdd);
                                break;
                            case GlobalSolution.eImportItemType.BusinessFlows:
                            case GlobalSolution.eImportItemType.SharedRepositoryActivities:
                                GlobalSolutionUtils.Instance.VariablesToReEncrypt(sourceFile, itemToAdd);
                                AddItemToSolution(sourceFile, targetFile, true, itemToAdd);
                                break;

                            case GlobalSolution.eImportItemType.DataSources:
                            case GlobalSolution.eImportItemType.SharedRepositoryActivitiesGroup:
                            case GlobalSolution.eImportItemType.SharedRepositoryActions:
                            case GlobalSolution.eImportItemType.SharedRepositoryVariables:
                            case GlobalSolution.eImportItemType.APIModels:
                            case GlobalSolution.eImportItemType.POMModels:
                            case GlobalSolution.eImportItemType.Agents:
                            case GlobalSolution.eImportItemType.TargetApplication:
                            case GlobalSolution.eImportItemType.Variables:
                            case GlobalSolution.eImportItemType.ExtrnalIntegrationConfigurations:
                                AddItemToSolution(sourceFile, targetFile, true, itemToAdd);
                                break;

                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                    }
                }

                //Add env params and dbs to this solution from the list
                if (EnvAppListToImport.Count > 0)
                {
                    GlobalSolutionUtils.Instance.AddEnvDependanciesToSolution(EnvAppListToImport);
                }
                //Save solution
                WorkSpace.Instance.Solution.SolutionOperations.SaveSolution();
            }
        }

        void AddItemToSolution(string sourceFile, string targetFile, bool saveAsRepo, GlobalSolutionItem itemToImport)
        {
            if (itemToImport.ItemType == GlobalSolution.eImportItemType.Variables)
            {
                //Add Global Variables from the list
                if (VariableListToImport.Count > 0)
                {
                    foreach (VariableBase vb in VariableListToImport)
                    {
                        if (WorkSpace.Instance.Solution.Variables.Where(x => x.Name == vb.Name).FirstOrDefault() == null)
                        {
                            WorkSpace.Instance.Solution.AddVariable(vb);
                        }
                    }
                }
                return;
            }
            if (itemToImport.ItemType == GlobalSolution.eImportItemType.TargetApplication)
            {
                string[] filePaths = Directory.GetFiles(Path.Combine(SolutionFolder), "Ginger.Solution.xml", SearchOption.AllDirectories);
                Solution solution = (Solution)newRepositorySerializer.DeserializeFromFile(filePaths[0]);
                ApplicationPlatform applicationPlatform  = solution.ApplicationPlatforms.Where(x=>x.AppName == itemToImport.ItemName).FirstOrDefault();

                ApplicationPlatform appPlatform = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.AppName == applicationPlatform.AppName && x.Platform == applicationPlatform.Platform).FirstOrDefault();
                if (appPlatform == null)
                {
                    WorkSpace.Instance.Solution.ApplicationPlatforms.Add(applicationPlatform);
                }
                return;
            }
            if (itemToImport.ItemType == GlobalSolution.eImportItemType.ExtrnalIntegrationConfigurations)
            {
                string[] filePaths = Directory.GetFiles(Path.Combine(SolutionFolder), "Ginger.Solution.xml", SearchOption.AllDirectories);
                Solution solution = (Solution)newRepositorySerializer.DeserializeFromFile(filePaths[0]);
                if (itemToImport.ItemName == ActVisualTesting.eVisualTestingAnalyzer.VRT.ToString())
                {
                    WorkSpace.Instance.Solution.VRTConfiguration = solution.VRTConfiguration;
                }
                else if (itemToImport.ItemName == ActVisualTesting.eVisualTestingAnalyzer.Applitools.ToString())
                {
                    WorkSpace.Instance.Solution.ApplitoolsConfiguration = solution.ApplitoolsConfiguration;
                }
            }
            RepositoryItemBase repoItemToImport = null;
            if (itemToImport.ItemImportSetting == GlobalSolution.eImportSetting.Replace)
            {
                if (GlobalSolutionUtils.Instance.IsGingerRepositoryItem(sourceFile))
                {
                    RepositoryItemBase repositoryItem = newRepositorySerializer.DeserializeFromFile(sourceFile);
                    RepositoryItemBase repoItem = GlobalSolutionUtils.Instance.GetRepositoryItemByGUID(itemToImport, repositoryItem);
                    if (repoItem != null)
                    {
                        WorkSpace.Instance.SolutionRepository.MoveSharedRepositoryItemToPrevVersion(repoItem);
                        if (itemToImport.ItemType == GlobalSolution.eImportItemType.DataSources)
                        {
                            DataSourceBase dataSource = (DataSourceBase)repoItem;
                            string dsFile = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(dataSource.FilePath);
                            GlobalSolutionUtils.Instance.KeepBackupAndDeleteFile(dsFile);
                        }
                    }
                }
                else
                {
                    GlobalSolutionUtils.Instance.KeepBackupAndDeleteFile(targetFile);
                }
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
                    string dsFile = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(dataSource.FilePath);
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
                    dataSource.FilePath = WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(newFile);
                    //
                    File.Copy(sourceFile, newFile);
                }
                //Create repository (sub) folder before adding
                AddRepositoryItem(itemToImport, repoItemToImport, targetFile);
            }
            else
            {
                if (!Directory.Exists(Path.GetDirectoryName(targetFile)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
                }
                File.Copy(sourceFile, targetFile);
            }
        }
        void AddRepositoryItem(GlobalSolutionItem itemToImport, RepositoryItemBase repoItemToImport, string targetFile)
        {
            //Get subdirectory path
            string path = Path.GetDirectoryName(targetFile);
            RepositoryFolderBase repoFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryFolderByPath(path);
            if (repoFolder != null)
            {
                repoFolder.AddRepositoryItem(repoItemToImport);
                return;
            }

            string folderPath = string.Empty;
            RepositoryFolderBase rootFolder = null;
            RepositoryFolderBase subFolder = null;
            switch (itemToImport.ItemType)
            {
                case GlobalSolution.eImportItemType.Environments:
                    rootFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ProjEnvironment>();
                    break;
                case GlobalSolution.eImportItemType.DataSources:
                    rootFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<DataSourceBase>();
                    break;
                case GlobalSolution.eImportItemType.BusinessFlows:
                    rootFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>();
                    break;
                case GlobalSolution.eImportItemType.SharedRepositoryActivities:
                    rootFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Activity>();
                    break;
                case GlobalSolution.eImportItemType.SharedRepositoryActivitiesGroup:
                    rootFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ActivitiesGroup>();
                    break;
                case GlobalSolution.eImportItemType.SharedRepositoryActions:
                    rootFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Act>();
                    break;
                case GlobalSolution.eImportItemType.SharedRepositoryVariables:
                    rootFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<VariableBase>();
                    break;
                case GlobalSolution.eImportItemType.APIModels:
                    rootFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationAPIModel>();
                    break;
                case GlobalSolution.eImportItemType.POMModels:
                    rootFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>();
                    break;
                case GlobalSolution.eImportItemType.Agents:
                    rootFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Agent>();
                    break;

                default:
                    break;
            }

            folderPath = path.Replace(rootFolder.FolderFullPath + "\\", "");
            subFolder = rootFolder.AddSubFolder(folderPath);
            subFolder.FolderRelativePath = rootFolder.FolderRelativePath + "\\" + folderPath;
            subFolder.AddRepositoryItem(repoItemToImport);
        }

    }
}
