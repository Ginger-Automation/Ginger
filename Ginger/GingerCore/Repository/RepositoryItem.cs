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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository.BusinessFlowLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.DataSource;
using GingerCore.Repository;
using GingerCore.Variables;
using System;
using System.Reflection;

namespace GingerCore
{
    public abstract class RepositoryItem : RepositoryItemBase
    {        

        [IsSerializedForLocalRepository]
        public int Version { get; set; }

        [IsSerializedForLocalRepository]
        public string CreatedBy { get; set; }

        [IsSerializedForLocalRepository]
        public DateTime Created { get; set; }

        [IsSerializedForLocalRepository]
        public string LastUpdateBy { get; set; }

        [IsSerializedForLocalRepository]
        public DateTime LastUpdate { get; set; }        

        public bool Deleted { get; set; }

        public new string ObjFolderName { get { return FolderName(this.GetType()); } }

        public RepositoryItem()
        {
            UpdateControlFields();
        }

        
        public virtual void Save()
        {
            SaveToFile(FileName);
        }

        public void SaveToFile(string fileName, bool FlagToUpdateFileName = true)
        {            
            this.Version++;
            this.LastUpdate = DateTime.UtcNow;
            this.LastUpdateBy = Environment.UserName;
            if (FlagToUpdateFileName)
                this.FileName = fileName;
            this.ClearBackup();

            RepositorySerializer rs = new RepositorySerializer();
            rs.SaveToFile(this, fileName);
        }

        public static object LoadFromFile(Type type, string FileName)
        {
            GingerCore.Repository.RepositorySerializer RS = new RepositorySerializer();
            RepositoryItemBase ri = (RepositoryItemBase)RS.DeserializeFromFile(type, FileName);
            ri.FileName = FileName;
            return ri;
        }
        public static object LoadFromFile(string FileName)
        {
            GingerCore.Repository.RepositorySerializer RS = new RepositorySerializer();
            RepositoryItemBase ri = (RepositoryItemBase)RS.DeserializeFromFile(FileName);
            ri.FileName = FileName;
            return ri;
        }
        public static void ObjectsDeepCopy(RepositoryItem sourceObj, RepositoryItem targetObj)
        {
            RepositorySerializer repoSer = new RepositorySerializer();
            string sourceObjXml = repoSer.SerializeToString(sourceObj);

            GingerCore.Repository.RepositorySerializer RS = new RepositorySerializer();
            RS.DeserializeFromTextWithTargetObj(sourceObj.GetType(), sourceObjXml, targetObj);
        }

      

        public void InvokPropertyChanngedForAllFields()
        {
            try
            {
                Type fieldsObjType = this.GetType().GetNestedType("Fields");
                foreach (var field in fieldsObjType.GetFields())
                {
                    MethodInfo OnPropertyChangedMethod = this.GetType().GetMethod("OnPropertyChanged");
                    OnPropertyChangedMethod.Invoke(this, new object[] { field.Name });
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "Failed to InvokPropertyChanngedForAllObjectFields for the object: " + this.ToString(), ex);
            }
        }



        /// <summary>
        /// Been used for updating the Shared Repository item instance
        /// </summary>
        public virtual void UpdateInstance(RepositoryItem instanceItem, string itemPartToUpdate, RepositoryItem hostItem = null)
        {
            throw new Exception("UpdateInstance() was not implemented for this Item type");
        }


        public virtual RepositoryItem GetUpdatedRepoItem(RepositoryItem selectedItem, RepositoryItem existingItem, string itemPartToUpdate)
        {
            throw new Exception("GetUpdatedRepoItem() was not implemented for this Item type");
        }



      
    }
}
