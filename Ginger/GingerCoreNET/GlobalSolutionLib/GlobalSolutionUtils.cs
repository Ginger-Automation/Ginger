using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Amdocs.Ginger.Common.OS;
using Amdocs.Ginger.Repository;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Activities;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Amdocs.Ginger.CoreNET.GlobalSolutionLib
{
    public class GlobalSolutionUtils
    {
        NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();

        public string EncryptionKey { get; set; }
        public string SolutionFolder { get; set; }

        private static readonly GlobalSolutionUtils _instance = new GlobalSolutionUtils();
        public static GlobalSolutionUtils Instance
        {
            get
            {
                return _instance;
            }
        }
        public string GetRepositoryItemName(string sourceFile)
        {
            try
            {
                RepositoryItemBase repositoryItem = newRepositorySerializer.DeserializeFromFile(sourceFile);
                return repositoryItem.ItemName;
            }
            catch (Exception ex)
            {
                return System.IO.Path.GetFileNameWithoutExtension(sourceFile);
            }
        }
        public bool CheckForItemWithDuplicateGUID(GlobalSolutionItem itemToCheck)
        {
            bool isDuplicate = false;
            try
            {
                RepositoryItemBase repositoryItem = newRepositorySerializer.DeserializeFromFile(itemToCheck.ItemFullPath);
                RepositoryItemBase duplicateItem = null;
                switch (itemToCheck.ItemType)
                {
                    case GlobalSolution.eImportItemType.Environments:
                        duplicateItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => x.Guid == repositoryItem.Guid).FirstOrDefault();
                        break;
                    case GlobalSolution.eImportItemType.DataSources:
                        duplicateItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>().Where(x => x.Guid == repositoryItem.Guid).FirstOrDefault();
                        break;
                    case GlobalSolution.eImportItemType.SharedRepositoryActivitiesGroup:
                        duplicateItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>().Where(x => x.Guid == repositoryItem.Guid).FirstOrDefault();
                        break;
                    case GlobalSolution.eImportItemType.SharedRepositoryActivities:
                        duplicateItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>().Where(x => x.Guid == repositoryItem.Guid).FirstOrDefault();
                        break;
                    case GlobalSolution.eImportItemType.SharedRepositoryActions:
                        duplicateItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>().Where(x => x.Guid == repositoryItem.Guid).FirstOrDefault();
                        break;
                    case GlobalSolution.eImportItemType.SharedRepositoryVariables:
                        duplicateItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>().Where(x => x.Guid == repositoryItem.Guid).FirstOrDefault();
                        break;
                    case GlobalSolution.eImportItemType.APIModels:
                        duplicateItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>().Where(x => x.Guid == repositoryItem.Guid).FirstOrDefault();
                        break;
                    case GlobalSolution.eImportItemType.POMModels:
                        duplicateItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>().Where(x => x.Guid == repositoryItem.Guid).FirstOrDefault();
                        break;
                    default:
                        //Nothing to do
                        break;
                }
                if (duplicateItem != null)
                {
                    isDuplicate = true;
                }
            }
            catch (Exception ex)
            {
                isDuplicate = false;
            }
            return isDuplicate;
        }
        public bool IsGingerRepositoryItem(string filePath)
        {
            RepositoryItemBase repositoryItem = null;
            bool isRepoItem = false;
            try
            {
                repositoryItem = newRepositorySerializer.DeserializeFromFile(filePath);
                isRepoItem = true;
            }
            catch (Exception ex)
            {
                isRepoItem = false;
            }
            return isRepoItem;
        }
        public RepositoryItemBase GetRepositoryItemByGUID(GlobalSolutionItem itemToCheck, RepositoryItemBase repoItemToCheck)
        {
            RepositoryItemBase repositoryItem = null;
            try
            {
                switch (itemToCheck.ItemType)
                {
                    case GlobalSolution.eImportItemType.Environments:
                        repositoryItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => x.Guid == repoItemToCheck.Guid).FirstOrDefault();
                        break;
                    case GlobalSolution.eImportItemType.DataSources:
                        repositoryItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>().Where(x => x.Guid == repoItemToCheck.Guid).FirstOrDefault();
                        break;
                    case GlobalSolution.eImportItemType.SharedRepositoryActivitiesGroup:
                        repositoryItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>().Where(x => x.Guid == repoItemToCheck.Guid).FirstOrDefault();
                        break;
                    case GlobalSolution.eImportItemType.SharedRepositoryActivities:
                        repositoryItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>().Where(x => x.Guid == repoItemToCheck.Guid).FirstOrDefault();
                        break;
                    case GlobalSolution.eImportItemType.SharedRepositoryActions:
                        repositoryItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>().Where(x => x.Guid == repoItemToCheck.Guid).FirstOrDefault();
                        break;
                    case GlobalSolution.eImportItemType.SharedRepositoryVariables:
                        repositoryItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>().Where(x => x.Guid == repoItemToCheck.Guid).FirstOrDefault();
                        break;
                    case GlobalSolution.eImportItemType.APIModels:
                        repositoryItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>().Where(x => x.Guid == repoItemToCheck.Guid).FirstOrDefault();
                        break;
                    case GlobalSolution.eImportItemType.POMModels:
                        repositoryItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>().Where(x => x.Guid == repoItemToCheck.Guid).FirstOrDefault();
                        break;
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
            return repositoryItem;
        }
        public List<RepositoryItemBase> GetAllRepositoryItemsByItemType(GlobalSolutionItem itemToCheck)
        {
            List<RepositoryItemBase> repositoryItems = null;
            try
            {
                switch (itemToCheck.ItemType)
                {
                    case GlobalSolution.eImportItemType.Environments:
                        ObservableList<ProjEnvironment> repoEnvList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
                        repositoryItems = repoEnvList.OfType<RepositoryItemBase>().ToList();
                        break;
                    case GlobalSolution.eImportItemType.DataSources:
                        ObservableList<DataSourceBase> repoDataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
                        repositoryItems = repoDataSourceList.OfType<RepositoryItemBase>().ToList();
                        break;
                    default:
                        //Nothing to do
                        break;
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
            return repositoryItems;
        }

        public string GetUniqFileName(string fileFullPath, bool returnWithExt = true)
        {
            //validate no other file with same name
            //find first file which doesn't exist, add _counter until found

            RepositoryItemBase repositoryItem = newRepositorySerializer.DeserializeFromFile(fileFullPath);
            string ext = ".Ginger." + repositoryItem.GetItemType() + ".xml";

            int counter = 0;

            string OriginalName = System.IO.Path.GetFileName(fileFullPath);
            OriginalName = OriginalName.Replace(ext, "");
            string newName = OriginalName;

            while (true)
            {
                counter++;
                newName = OriginalName + "_" + counter;
                fileFullPath = fileFullPath.Replace(OriginalName + ext, newName + ext);
                if (File.Exists(fileFullPath))
                {
                    fileFullPath = fileFullPath.Replace(newName + ext, OriginalName + ext);
                    continue;
                }
                else 
                {
                    break;
                }
            }
            if (returnWithExt)
            {
                return newName + ext;
            }
            return newName;
        }
        //keep the backup to PrevVersion folder
        public bool KeepBackupAndDeleteFile(string filePath)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);
                string bkpDateTime = System.Text.RegularExpressions.Regex.Replace(DateTime.Now.ToString(), @"[^0-9a-zA-Z]+", "");
                string bkpPath = Path.Combine(directory, "PrevVersions");
                string bkpFilePath = Path.Combine(bkpPath, fileName);
                if (!Directory.Exists(bkpPath))
                {
                    Directory.CreateDirectory(bkpPath);
                }
                File.Copy(filePath, bkpFilePath + "." + bkpDateTime + ".bak");
                File.Delete(filePath);
            }
            catch(Exception ex) 
            {
                return false;
            }
            return true;
        }

        public void AddDependaciesForEnvironment(GlobalSolutionItem itemEnv, ref ObservableList<GlobalSolutionItem> SelectedItemsListToImport, ref List<VariableBase> VariableListToImport)
        {
            AddDependaciesForDataSource(itemEnv.ItemFullPath, ref SelectedItemsListToImport);
            AddDependaciesForGlobalVariable(itemEnv.ItemFullPath, ref SelectedItemsListToImport, ref VariableListToImport);
        }

        public void AddDependaciesForSharedActivityGroup(GlobalSolutionItem itemActivitiesGroup, ref ObservableList<GlobalSolutionItem> SelectedItemsListToImport, ref List<VariableBase> VariableListToImport, ref List<EnvApplication> EnvAppListToImport)
        {
            ActivitiesGroup activitiesGroup = (ActivitiesGroup)newRepositorySerializer.DeserializeFromFile(itemActivitiesGroup.ItemFullPath);
            string[] filePaths = Directory.GetFiles(Path.Combine(SolutionFolder, "SharedRepository", "Activities"), "*.xml", SearchOption.AllDirectories);
            foreach (string activityFile in filePaths)
            {
                Activity activity = (Activity)newRepositorySerializer.DeserializeFromFile(activityFile);
                ActivityIdentifiers actIdent = activitiesGroup.ActivitiesIdentifiers.Where(x => x.ActivityGuid == activity.Guid).FirstOrDefault();
                if (actIdent != null)
                {
                    GlobalSolutionItem newItem = new GlobalSolutionItem(GlobalSolution.eImportItemType.SharedRepositoryActivities, activity.FilePath, ConvertToRelativePath(activity.FilePath), true, "", activitiesGroup.Name);
                    AddItemToSelectedItemsList(newItem, ref SelectedItemsListToImport);
                    foreach (Act act in activity.Acts)
                    {
                        string filePath = GetApplicationPOMModelFilePathForAction(act);
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            newItem = new GlobalSolutionItem(GlobalSolution.eImportItemType.POMModels, filePath,ConvertToRelativePath(filePath), true, "", activitiesGroup.Name);
                            AddItemToSelectedItemsList(newItem, ref SelectedItemsListToImport);

                            //Add dependancies for POM
                            AddDependaciesForPOMModel(newItem, ref SelectedItemsListToImport, ref VariableListToImport, ref EnvAppListToImport);
                        }
                    }
                }
            }

        }
        public void AddDependaciesForSharedActivity(GlobalSolutionItem itemActivity, ref ObservableList<GlobalSolutionItem> SelectedItemsListToImport, ref List<VariableBase> VariableListToImport, ref List<EnvApplication> EnvAppListToImport)
        {
            Activity importedActivity = (Activity)newRepositorySerializer.DeserializeFromFile(itemActivity.ItemFullPath);
            foreach (Act act in importedActivity.Acts)
            {
                string filePath = GetApplicationPOMModelFilePathForAction(act);
                if (!string.IsNullOrEmpty(filePath))
                {
                    GlobalSolutionItem newItem = new GlobalSolutionItem(GlobalSolution.eImportItemType.POMModels, filePath, ConvertToRelativePath(filePath), true, "", importedActivity.ActivityName);
                    AddItemToSelectedItemsList(newItem, ref SelectedItemsListToImport);

                    //Add dependancies for POM
                    AddDependaciesForPOMModel(newItem, ref SelectedItemsListToImport, ref VariableListToImport, ref EnvAppListToImport);
                }
            }
            //Add dependancies for Env
            AddDependaciesForEnvParam(itemActivity.ItemFullPath, ref SelectedItemsListToImport, ref VariableListToImport, ref EnvAppListToImport);
            //Add dependancies for GlobalVariables
            AddDependaciesForGlobalVariable(itemActivity.ItemFullPath, ref SelectedItemsListToImport, ref VariableListToImport);
            //Add dependancies for DS
            AddDependaciesForDataSource(itemActivity.ItemFullPath, ref SelectedItemsListToImport);
        }
        public void AddDependaciesForSharedAction(GlobalSolutionItem itemAct, ref ObservableList<GlobalSolutionItem> SelectedItemsListToImport, ref List<VariableBase> VariableListToImport, ref List<EnvApplication> EnvAppListToImport)
        {
            Act importedAct = (Act)newRepositorySerializer.DeserializeFromFile(itemAct.ItemFullPath);
            string filePath = GetApplicationPOMModelFilePathForAction(importedAct);
            if (!string.IsNullOrEmpty(filePath))
            {
                GlobalSolutionItem newItem = new GlobalSolutionItem(GlobalSolution.eImportItemType.POMModels, filePath, ConvertToRelativePath(filePath), true, "", importedAct.ActionDescription);
                AddItemToSelectedItemsList(newItem, ref SelectedItemsListToImport);

                //Add dependancies for POM
                AddDependaciesForPOMModel(newItem, ref SelectedItemsListToImport, ref VariableListToImport, ref EnvAppListToImport);
            }
            //Add dependancies for Env
            AddDependaciesForEnvParam(itemAct.ItemFullPath, ref SelectedItemsListToImport, ref VariableListToImport, ref EnvAppListToImport);
            //Add dependancies for GlobalVariables
            AddDependaciesForGlobalVariable(itemAct.ItemFullPath, ref SelectedItemsListToImport, ref VariableListToImport);
            //Add dependancies for DS
            AddDependaciesForDataSource(itemAct.ItemFullPath, ref SelectedItemsListToImport);
        }

        private string GetApplicationPOMModelFilePathForAction(Act act)
        {
            string filePath = string.Empty;
            if (act is ActUIElement)
            {
                ActUIElement actUIElement = (ActUIElement)act;
                if (actUIElement.ElementLocateBy == Common.UIElement.eLocateBy.POMElement)
                {
                    string[] pOMandElementGUIDs = actUIElement.ElementLocateValue.Split('_');
                    filePath = GetPOMFilePathByGUID(pOMandElementGUIDs[0]);
                }
            }
            else if (act is ActBrowserElement)
            {
                if (act.GetInputParamValue(ActBrowserElement.Fields.URLSrc) == ActBrowserElement.eURLSrc.UrlPOM.ToString())
                {
                    string POMGuid = act.GetInputParamValue(ActBrowserElement.Fields.PomGUID);
                    filePath = GetPOMFilePathByGUID(POMGuid);
                }
            }
            else
            {
            }
            return filePath;

        }
        private string GetPOMFilePathByGUID(string GUID)
        {
            string filePath = string.Empty;
            Guid selectedPOMGUID = new Guid(GUID);
            if (!string.IsNullOrEmpty(GUID))
            {
                string[] filePaths = Directory.GetFiles(Path.Combine(SolutionFolder, "Applications Models", "POM Models"), "*.xml", SearchOption.AllDirectories);
                foreach (string pomFile in filePaths)
                {
                    ApplicationPOMModel pomModel = (ApplicationPOMModel)newRepositorySerializer.DeserializeFromFile(pomFile);
                    if (selectedPOMGUID == pomModel.Guid)
                    {
                        filePath = pomModel.FilePath;
                        break;
                    }
                }
            }
            return filePath;
        }
        public void AddDependaciesForPOMModel(GlobalSolutionItem itemPOM, ref ObservableList<GlobalSolutionItem> SelectedItemsListToImport, ref List<VariableBase> VariableListToImport, ref List<EnvApplication> EnvAppListToImport)
        {
            AddDependaciesForEnvParam(itemPOM.ItemFullPath, ref SelectedItemsListToImport, ref VariableListToImport, ref EnvAppListToImport);
            AddDependaciesForGlobalVariable(itemPOM.ItemFullPath, ref SelectedItemsListToImport, ref VariableListToImport);
            AddDependaciesForDataSource(itemPOM.ItemFullPath, ref SelectedItemsListToImport);
        }
        public void AddDependaciesForAPIModel(GlobalSolutionItem itemAPI, ref ObservableList<GlobalSolutionItem> SelectedItemsListToImport, ref List<VariableBase> VariableListToImport, ref List<EnvApplication> EnvAppListToImport)
        {
            AddDependaciesForEnvParam(itemAPI.ItemFullPath, ref SelectedItemsListToImport, ref VariableListToImport, ref EnvAppListToImport);
            AddDependaciesForGlobalVariable(itemAPI.ItemFullPath, ref SelectedItemsListToImport, ref VariableListToImport);
            AddDependaciesForDataSource(itemAPI.ItemFullPath, ref SelectedItemsListToImport);
        }
        public void AddDependaciesForEnvParam(string filePath, ref ObservableList<GlobalSolutionItem> SelectedItemsListToImport, ref List<VariableBase> VariableListToImport, ref List<EnvApplication> EnvAppListToImport)
        {
            string allText = File.ReadAllText(filePath);
            Regex rxPattern = new Regex(@"({.*?})", RegexOptions.Compiled);
            MatchCollection matches = rxPattern.Matches(allText);
            if (matches.Count == 0)
            {
                return;
            }
            foreach (Match matchValue in matches)
            {
                string match = matchValue.Value;
                if (match.Contains("{EnvParam App="))
                {
                    string AppName = null;
                    string GlobalParamName = null;

                    match = match.Replace("\r\n", "vbCrLf");
                    string appStr = " App=";
                    string paramStr = " Param=";
                    int indxOfApp = match.IndexOf(appStr);
                    int indexOfParam = match.IndexOf(paramStr);
                    AppName = match.Substring(indxOfApp + appStr.Length, indexOfParam - (indxOfApp + appStr.Length));
                    GlobalParamName = match.Substring(indexOfParam + paramStr.Length, (match.Length - 1) - (indexOfParam + paramStr.Length));

                    string[] filePaths = Directory.GetFiles(Path.Combine(SolutionFolder, "Environments"), "*.xml", SearchOption.AllDirectories);
                    foreach (string envFile in filePaths)
                    {
                        ProjEnvironment projEnvironment = (ProjEnvironment)newRepositorySerializer.DeserializeFromFile(envFile);
                        EnvApplication envApplication = projEnvironment.GetApplication(AppName);
                        if (envApplication != null)
                        {
                            //decrypt and reencrypt
                            foreach (Database db in envApplication.Dbs)
                            {
                                if (!string.IsNullOrEmpty(db.Pass))
                                {
                                    db.Pass = EncryptValueWithCurrentSolutionKey(db.Pass);
                                }
                            }

                            GeneralParam gp = envApplication.GetParam(GlobalParamName);
                            if (gp != null)
                            {
                                if (gp.Encrypt)
                                {
                                    gp.Value = EncryptValueWithCurrentSolutionKey(gp.Value);
                                }

                                EnvApplication isAlreadyAddedApp = EnvAppListToImport.Where(x => x.Name == AppName).FirstOrDefault();
                                if (isAlreadyAddedApp == null)
                                {
                                    EnvApplication envApp = new EnvApplication() { Name = AppName };
                                    envApp.GeneralParams.Add(gp);
                                    EnvAppListToImport.Add(envApp);
                                }
                                else 
                                {
                                    isAlreadyAddedApp.GeneralParams.Add(gp);
                                }

                                GlobalSolutionItem newItem = new GlobalSolutionItem(GlobalSolution.eImportItemType.Environments, envFile, ConvertToRelativePath(envFile), true, "", GetRepositoryItemName(filePath));
                                AddItemToSelectedItemsList(newItem, ref SelectedItemsListToImport);
                                //
                                AddDependaciesForEnvironment(newItem, ref SelectedItemsListToImport, ref VariableListToImport);
                            }
                        }

                    }
                }
            }
        }

        public void AddDependaciesForGlobalVariable(string filePath, ref ObservableList<GlobalSolutionItem> SelectedItemsListToImport, ref List<VariableBase> VariableListToImport)
        {
            string allText = File.ReadAllText(filePath);
            Regex rxPattern = new Regex(@"({.*?})", RegexOptions.Compiled);
            MatchCollection matches = rxPattern.Matches(allText);
            if (matches.Count == 0)
            {
                return;
            }
            string[] filePaths = Directory.GetFiles(Path.Combine(SolutionFolder), "Ginger.Solution.xml", SearchOption.AllDirectories);
            Solution solution = (Solution)newRepositorySerializer.DeserializeFromFile(filePaths[0]);
            foreach (Match matchValue in matches)
            {
                string match = matchValue.Value;
                if (match.Contains("{Var Name="))
                {
                    string VarName = match.Replace("{Var Name=", "");
                    VarName = VarName.Replace("}", "");
                    VariableBase isAlreadyAddedVB = VariableListToImport.Where(x => x.Name == VarName).FirstOrDefault();
                    if (isAlreadyAddedVB != null)
                    {
                        continue;
                    }
                    VariableBase vb = (from v1 in solution.Variables where v1.Name == VarName select v1).FirstOrDefault();
                    if (vb != null)
                    {
                        
                        string varValue = string.Empty;
                        if (vb is VariablePasswordString)
                        {
                            VariablePasswordString vp = (VariablePasswordString)vb;
                            vp.Password = EncryptValueWithCurrentSolutionKey(vb.Value);
                            VariableListToImport.Add(vp);
                        }
                        else
                        {
                            VariableListToImport.Add(vb);
                        }
                    }
                }
            }
            if (VariableListToImport.Count > 0)
            {
                GlobalSolutionItem newItem = new GlobalSolutionItem(GlobalSolution.eImportItemType.Variables, solution.FilePath, ConvertToRelativePath(solution.FilePath), true, "", GetRepositoryItemName(filePath));
                newItem.ItemName = string.Join(",", VariableListToImport);
                AddItemToSelectedItemsList(newItem, ref SelectedItemsListToImport);
            }
        }

        public void AddDependaciesForDataSource(string filePath, ref ObservableList<GlobalSolutionItem> SelectedItemsListToImport)
        {
            string allText = File.ReadAllText(filePath);
            Regex rxPattern = new Regex(@"({.*?})", RegexOptions.Compiled);
            MatchCollection matches = rxPattern.Matches(allText);
            if (matches.Count == 0)
            {
                return;
            }
            var dsList = new List<string>();
            foreach (Match matchValue in matches)
            {
                string match = matchValue.Value;
                if (match.Contains("{DS Name="))
                {
                    string[] Token = match.Split(new[] { "{DS Name=", " " }, StringSplitOptions.None);
                    string DSName = Token[1];
                    if (!dsList.Contains(Token[1]))
                    {
                        dsList.Add(Token[1]);
                    }
                }
            }

            string[] filePaths = Directory.GetFiles(Path.Combine(SolutionFolder, "DataSources"), "*.xml", SearchOption.AllDirectories);
            foreach (string file in filePaths)
            {
                if (dsList.Contains(Path.GetFileNameWithoutExtension(file).Replace(".Ginger.DataSource", "")))
                {
                    //check if datasource is already added to list
                    if (SelectedItemsListToImport.Where(x => x.ItemFullPath == file).ToList().Count == 0)
                    {
                        GlobalSolutionItem newItem = new GlobalSolutionItem(GlobalSolution.eImportItemType.DataSources, file, ConvertToRelativePath(file), true, "", GetRepositoryItemName(filePath));
                        AddItemToSelectedItemsList(newItem, ref SelectedItemsListToImport);
                    }
                }
            }
        }

        public void AddItemToSelectedItemsList(GlobalSolutionItem itemToAdd, ref ObservableList<GlobalSolutionItem> SelectedItemsListToImport)
        {
            bool skipAdd = false;
            //check if item already exist in the list -> if duplicate keep dependant item
            GlobalSolutionItem listItem = SelectedItemsListToImport.Where(x => x.ItemFullPath == itemToAdd.ItemFullPath).FirstOrDefault();

            if (listItem != null)
            {
                if (string.IsNullOrEmpty(listItem.RequiredFor))
                {
                    SelectedItemsListToImport.Remove(listItem);
                }
                else
                {
                    skipAdd = true;
                }
            }
            
            //Check if GUID is already exist
            bool isDuplicateGUID = GlobalSolutionUtils.Instance.CheckForItemWithDuplicateGUID(itemToAdd);
            if (isDuplicateGUID)
            {
                itemToAdd.ItemImportSetting = GlobalSolution.eImportSetting.Replace;
                itemToAdd.Comments = "Item already exist, with same GUID";
            }

            if (itemToAdd.ItemType == GlobalSolution.eImportItemType.Variables)
            {
                itemToAdd.Comments = "Only Used Global Variables will be added to your solution.";
            }
            else
            {
                itemToAdd.ItemName = GlobalSolutionUtils.Instance.GetRepositoryItemName(itemToAdd.ItemFullPath);
                //check if file already exist
                string path = Path.GetDirectoryName(itemToAdd.ItemFullPath);
                string folderPath = path.Replace(SolutionFolder, "");
                string targetFile = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, folderPath, Path.GetFileName(itemToAdd.ItemFullPath));
                if (File.Exists(targetFile) && !isDuplicateGUID)
                {
                    if (IsGingerRepositoryItem(targetFile))
                    {
                        itemToAdd.ItemImportSetting = GlobalSolution.eImportSetting.New;
                        string newFileName = GetUniqFileName(targetFile, false);
                        itemToAdd.ItemNewName = newFileName;
                        itemToAdd.Comments = "Item already exist, importing item with new name " + newFileName;
                    }
                    else
                    {
                        itemToAdd.ItemImportSetting = GlobalSolution.eImportSetting.Replace;
                        itemToAdd.Comments = "Item already exist, with same filename";
                    }
                }
            }
            
            
            if (!skipAdd)
            {
                SelectedItemsListToImport.Add(itemToAdd);
            }
        }

        public void AddEnvDependanciesToSolution(List<EnvApplication> EnvAppListToImport)
        {
            //Add env params and dbs to this solution
            ObservableList<ProjEnvironment> repoEnvList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
            foreach (ProjEnvironment projEnv in repoEnvList)
            {
                foreach (EnvApplication envApplicationToImport in EnvAppListToImport)
                {
                    EnvApplication envApp = projEnv.Applications.Where(x => x.Name == envApplicationToImport.Name).FirstOrDefault();
                    if (envApp == null)
                    {
                        projEnv.Applications.Add(envApplicationToImport);
                    }
                    else
                    {
                        //add env params
                        foreach (GeneralParam gpToImport in envApplicationToImport.GeneralParams)
                        {
                            GeneralParam gp = envApp.GeneralParams.Where(x => x.Name == gpToImport.Name).FirstOrDefault();
                            if (gp == null)
                            {
                                envApp.GeneralParams.Add(gpToImport);
                            }
                        }
                        //add db
                        foreach (Database dbToImport in envApplicationToImport.Dbs)
                        {
                            Database db = (Database)envApp.Dbs.Where(x => x.Name == dbToImport.Name).FirstOrDefault();
                            if (db == null)
                            {
                                envApp.Dbs.Add(db);
                            }
                        }
                    }
                }
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(projEnv);
            }
        }

        public void VariablesToReEncrypt(string sourceFile, GlobalSolutionItem itemToImport)
        {
            List<VariableBase> variablePasswords = new List<VariableBase>();
            switch (itemToImport.ItemType)
            {
                case GlobalSolution.eImportItemType.SharedRepositoryActivities:
                    Activity activity= (Activity)newRepositorySerializer.DeserializeFromFile(sourceFile);
                    variablePasswords = activity.Variables.Where(x => x.VariableType == "PasswordString").ToList();
                    break;
                //case GlobalSolution.eImportItemType.BusinessFlows:
                //    BusinessFlow businessFlow = (BusinessFlow)newRepositorySerializer.DeserializeFromFile(sourceFile);
                //    variablePasswords = businessFlow.Variables.Where(x => x.VariableType == "PasswordString").ToList();
                //    break;
                default:
                    //Nothing to do
                    break;
            }

            foreach (VariablePasswordString vbs in variablePasswords)
            {
                vbs.Password = EncryptValueWithCurrentSolutionKey(vbs.Value);
            }
        }

        public void EnvParamsToReEncrypt(string sourceFile, GlobalSolutionItem itemToImport)
        {
            ProjEnvironment projEnvironment = (ProjEnvironment)newRepositorySerializer.DeserializeFromFile(sourceFile);
            foreach (EnvApplication ea in projEnvironment.Applications)
            {
                foreach (GeneralParam gp in ea.GeneralParams.Where(param => param.Encrypt))
                {
                    gp.Value = EncryptValueWithCurrentSolutionKey(gp.Value);
                }

                foreach (Database db in ea.Dbs)
                {
                    if (!string.IsNullOrEmpty(db.Pass))
                    {
                        db.Pass = EncryptValueWithCurrentSolutionKey(db.Pass);
                    }
                }
            }
        }
        public string GetEncryptionKey()
        {
            string[] filePaths = Directory.GetFiles(Path.Combine(SolutionFolder), "Ginger.Solution.xml", SearchOption.TopDirectoryOnly);
            Solution solution = (Solution)newRepositorySerializer.DeserializeFromFile(filePaths[0]);
            solution.EncryptionKey = Solution.GetEncryptionKey(solution.Guid.ToString());
            
            return solution.EncryptionKey;
        }

        public Solution GetSolution()
        {
            string[] filePaths = Directory.GetFiles(Path.Combine(SolutionFolder), "Ginger.Solution.xml", SearchOption.TopDirectoryOnly);
            if (filePaths.Length > 0)
            {
                Solution solution = (Solution)newRepositorySerializer.DeserializeFromFile(filePaths[0]);
                solution.EncryptionKey = Solution.GetEncryptionKey(solution.Guid.ToString());

                return solution;
            }
            else
            { return null; }
        }

        public string EncryptValueWithCurrentSolutionKey(string oldEncryptedValue)
        {
            string varValue = string.Empty;
            string strValuetoPass = EncryptionHandler.DecryptwithKey(oldEncryptedValue, EncryptionKey);
            if (!string.IsNullOrEmpty(strValuetoPass))
            {
                varValue = strValuetoPass;
            }
            else
            {
                varValue = oldEncryptedValue;
            }
            return EncryptionHandler.EncryptwithKey(varValue);
        }

        public string ConvertToRelativePath(string fullPath)
        {
            string relative = fullPath;
            if (fullPath != null && fullPath.ToUpper().Contains(SolutionFolder.ToUpper()))
            {
                relative = @"~\" + fullPath.Remove(0, SolutionFolder.Length);
            }
            return relative;
        }

        public string ConvertSolutionFullPath(string relativePath)
        {
            if (String.IsNullOrWhiteSpace(relativePath))
            {
                return relativePath;
            }
            try
            {
                if (relativePath.TrimStart().StartsWith("~"))
                {
                    string fullPath = relativePath.TrimStart(new char[] { '~', '\\', '/' });
                    fullPath = Path.Combine(SolutionFolder, fullPath);
                    return OperatingSystemBase.CurrentOperatingSystem.AdjustFilePath(fullPath);

                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Failed to replace relative path sign '~' with Solution path for the path: '" + relativePath + "'", ex);
            }
            return OperatingSystemBase.CurrentOperatingSystem.AdjustFilePath(relativePath);
        }
    }
}
