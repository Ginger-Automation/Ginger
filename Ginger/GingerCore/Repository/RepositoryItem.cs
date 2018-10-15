#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.PlugIns;
using GingerCore.Activities;
using GingerCore.DataSource;
using GingerCore.Properties;
using GingerCore.Repository;
using GingerCore.Variables;
using System;
using System.Reflection;

namespace GingerCore
{
    public abstract class RepositoryItem : RepositoryItemBase
    {        

        //[IsSerializedForLocalRepository]
        //public new Guid ParentGuid { get; set; }


        // TODO: remove or squeeze to one field

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

        //[IsSerializedForLocalRepository]
        //public new string ExternalID { get; set; }

        public bool Deleted { get; set; }

        public new string ObjFolderName { get { return FolderName(this.GetType()); } }

        public RepositoryItem()
        {
            UpdateControlFields();
        }

        

        //public void SaveParentGUID()
        //{
        //    ParentGuid = Guid;
        //    Guid = Guid.NewGuid();
        //}

        public virtual void Save()
        {
            SaveToFile(FileName);
        }

        public void SaveToFile(string fileName, bool FlagToUpdateFileName = true)
        {
            //DoPreSavePreperations;
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

        //public RepositoryItem CreateInstance(bool originFromShredRepo = false)
        //{
        //    RepositoryItem copiedItem = (RepositoryItem)this.CreateCopy();
        //    copiedItem.ParentGuid = this.Guid;
        //    if (originFromShredRepo)
        //    {
        //        copiedItem.IsSharedRepositoryInstance = true;
        //        copiedItem.ExternalID = this.ExternalID;
        //    }
        //    return copiedItem;
        //}

        //// Temp solution for backup restore until we have the obj ref tree copy
        //public void SaveBackup()
        //{
        //    //TODO: move soon to new back after testing
        //    // SaveBackup2();
        //    this.Backup = (RepositoryItem)CreateCopy();
        //}

        //public void RestoreFromBackup(bool invokFieldsPropertiesChanged = false)
        //{
        //    //TODO: move soon to new back after testing
        //    // RestoreFromBackup2();

        //    if (this.Backup != null)
        //        ObjectsDeepCopy(this.Backup, this);

        //    if (invokFieldsPropertiesChanged)
        //        InvokPropertyChanngedForAllFields();
        //}

        //TODO: fixme to use all annotated fields?! or remove

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
                Reporter.ToLog(eAppReporterLogLevel.WARN, "Failed to InvokPropertyChanngedForAllObjectFields for the object: " + this.ToString(), ex);
            }
        }

        public static string GetShortType(Type T)
        {
            //Not so much nice clean OO design but due to static on derived limitation, meanwhile it is working solution 
            // Put here only class which are saved as stand alone to file system
            // TODO: add interface to classes which are saved as files which will force them to im

            //TODO: more safe to use type of then Full Name - fix it!
            if (T == typeof(BusinessFlow)) { return "BusinessFlow"; }
            if (T == typeof(ActivitiesGroup)) { return "ActivitiesGroup"; }
            if (T == typeof(Activity)) { return "Activity"; }
            if (T == typeof(ErrorHandler)) { return "Activity"; }
            if (typeof(Act).IsAssignableFrom(T)) { return "Action"; }
            if (typeof(VariableBase).IsAssignableFrom(T)) { return "Variable"; }
            if (typeof(DataSourceBase).IsAssignableFrom(T)) return "DataSource";
            if (T.FullName == "GingerCore.Agent") return "Agent";
            if (T.FullName == "GingerCore.Environments.ProjEnvironment") return "Environment";
            if (T.FullName == "Ginger.Run.RunSetConfig") return "RunSetConfig";
            if (T.FullName == "Ginger.Run.BusinessFlowExecutionSummary") return "BusinessFlowExecutionSummary";
            if (T.FullName == "Ginger.Reports.ReportTemplate") return "ReportTemplate";
            if (T.FullName == "Ginger.Reports.HTMLReportTemplate") return "HTMLReportTemplate";
            if (T.FullName == "Ginger.Reports.HTMLReportConfiguration") return "HTMLReportConfiguration";

            if (T.FullName == "Ginger.Reports.HTMLReportConfiguration") return "HTMLReportConfiguration";
            if (T.FullName == "Ginger.TagsLib.RepositoryItemTag") return "RepsotirotyItemTag";

            if (T == typeof(ApplicationDBTableModel)) return "ApplicationDBTableModel";
            if (T == typeof(ApplicationDBModel)) return "ApplicationDBModel";
            if (T == typeof(ApplicationPOMModel)) return "ApplicationPOM";

            // Make sure we must impl or get exception
            throw new Exception("Unknown Type for Short Type Name " + T.Name);
        }

        public new string ObjFileExt { get { return FileExt(this.GetType()); } }

        public new static string FolderName(Type T)
        {
            string s = GetShortType(T) + "s";
            if (s.EndsWith("ys"))
            {
                s = s.Replace("ys", "ies");
            }

            //Special handling for Shared Repository item to be in sub folder
            if (s == "ActivitiesGroups" || s == "Activities" || s == "Actions" || s == "Variables" || s == "Validations")
            {
                s = @"SharedRepository\" + s;
            }
            return s;
        }

        /// <summary>
        /// Been used for updating the Shared Reporsitiry item instance
        /// </summary>
        public virtual void UpdateInstance(RepositoryItem instanceItem, string itemPartToUpdate, RepositoryItem hostItem = null)
        {
            throw new Exception("UpdateInstance() was not implemented for this Item type");
        }


        public virtual RepositoryItem GetUpdatedRepoItem(RepositoryItem selectedItem, RepositoryItem existingItem, string itemPartToUpdate)
        {
            throw new Exception("GetUpdatedRepoItem() was not implemented for this Item type");
        }

        public virtual Type GetTypeOfItemParts()
        {
            if (this is Activity)
                return typeof(Activity.eItemParts);

            else if (this is Act)
                return typeof(Act.eItemParts);

            else if (this is ActivitiesGroup)
                return typeof(ActivitiesGroup.eItemParts);

            else if (this is VariableBase)
                return typeof(VariableBase.eItemParts);

            else
                return null;
        }



      
    }
}
