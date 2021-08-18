using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Amdocs.Ginger.Repository;
using GingerCore.DataSource;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.CoreNET.GlobalSolutionLib
{
    public class GlobalSolutionUtils
    {
        NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();

        #region singlton
        private static readonly GlobalSolutionUtils _instance = new GlobalSolutionUtils();
        public static GlobalSolutionUtils Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion singlton
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
                RepositoryItemBase repositoryItem = newRepositorySerializer.DeserializeFromFile(itemToCheck.ItemExtraInfo);
                RepositoryItemBase duplicateItem = null;
                switch (itemToCheck.ItemType)
                {
                    case GlobalSolution.eImportItemType.Environments:
                        duplicateItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => x.Guid == repositoryItem.Guid).FirstOrDefault();
                        break;
                    case GlobalSolution.eImportItemType.DataSources:
                        duplicateItem = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>().Where(x => x.Guid == repositoryItem.Guid).FirstOrDefault();
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
                }

            }
            catch (Exception ex)
            {
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
                }

            }
            catch (Exception ex)
            {
            }
            return repositoryItems;
        }

        public string GetUniqFileName(string fileFullPath, bool returnWithExt = true)
        {
            //validate no other file with same name
            //find first file which doesn't exist, add ~counter until found

            RepositoryItemBase repositoryItem = newRepositorySerializer.DeserializeFromFile(fileFullPath);
            string ext = ".Ginger." + repositoryItem.GetItemType() + ".xml";

            int counter = 0;

            string OriginalName = System.IO.Path.GetFileName(fileFullPath);
            OriginalName = OriginalName.Replace(ext, "");
            string newName = OriginalName;

            while (File.Exists(fileFullPath))
            {
                counter++;
                newName = OriginalName + "~" + counter;
                fileFullPath = fileFullPath.Replace(OriginalName + ext, newName + ext);

                if (counter > 100)
                {
                    return "Cannot find unique file after 100 tries...";
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
    }
}
