using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.GlobalSolutionLib
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

    }
}
