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

namespace Amdocs.Ginger.Repository
{
    public abstract class SolutionRepositoryItemInfoBase
    {      
        public Type ItemType;

        public string ItemFileSystemRootFolder { get; set; }
        
        public abstract RepositoryFolderBase ItemRootRepositoryFolder { get;  }

        /// <summary>
        /// Get the parent folder of the Repository Item
        /// </summary>
        /// <param name="repositoryItem"></param>
        /// <returns></returns>
        public abstract RepositoryFolderBase GetItemRepositoryFolder(RepositoryItemBase repositoryItem);


        /// <summary>
        /// Delete the Repository Item folder and it sub folders from file system and cache
        /// </summary>
        /// <param name="repositoryFolder"></param>
        public abstract void DeleteRepositoryItemFolder(RepositoryFolderBase repositoryFolder);


        ///// <summary>
        ///// Define which property of the Repository Item to use when saving it as a file
        ///// for example: in BusinessFlow it will be: 'nameof(BusinessFlow.Name)'
        ///// so a Business flow with name 'Create Customer' will be saved to file with the name: 'Create Csutomer.Ginger.BusinessFlow.xml'
        ///// </summary>
        public string PropertyForFileName { get; set; }

        public string Pattern { get; set; }
        public string DisplayName { get; set; }

    }
}
