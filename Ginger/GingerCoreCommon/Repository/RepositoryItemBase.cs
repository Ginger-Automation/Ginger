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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.WorkSpaceLib;
using GingerCore.GeneralLib;

namespace Amdocs.Ginger.Repository
{

    public class GuidMapper
    {
        public Guid Original { get; set; }
        public Guid newGuid { get; set; } // pointer to obj
    }

    public enum SerializationErrorType
    {
        PropertyNotFound,
        SetValueException   // if type changed, and we can add more handling...
    }
    public enum eSharedItemType
    {
        [EnumValueDescription("Regular Item")]
        Regular = 0,
        [EnumValueDescription("Linked Item")]
        Link = 1
    }

    public abstract class RepositoryItemBase : INotifyPropertyChanged, ISearchFilter
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public RepositoryItemHeader RepositoryItemHeader { get; set; }

        private Guid mParentGuid;
        [IsSerializedForLocalRepository]
        public Guid ParentGuid { get { return mParentGuid; } set { if (mParentGuid != value) { mParentGuid = value; OnPropertyChanged(nameof(ParentGuid)); } } }

        private string mExternalID;
        [IsSerializedForLocalRepository]
        public string ExternalID { get { return mExternalID; } set { if (mExternalID != value) { mExternalID = value; OnPropertyChanged(nameof(ExternalID)); } } }

        private string mExternalID2;
        [IsSerializedForLocalRepository]
        public string ExternalID2 { get { return mExternalID2; } set { if (mExternalID2 != value) { mExternalID2 = value; OnPropertyChanged(nameof(ExternalID2)); } } }

        public LiteDB.ObjectId LiteDbId { get; set; }

        public string ObjFolderName { get { return FolderName(this.GetType()); } }

        public bool ItemBeenReloaded;

        //DO Not save
        protected ConcurrentDictionary<string, object> mBackupDic;
        protected bool mBackupInProgress = false;

        public bool IsBackupExist 
        {
            get
            {
                if (mBackupDic != null && mBackupInProgress == false)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        protected ConcurrentDictionary<string, object> mLocalBackupDic;

        public bool IsLocalBackupExist
        {
            get
            {
                if (mLocalBackupDic != null && mBackupInProgress == false)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public virtual string ObjFileExt
        {
            get
            {
                // We can override if we want different extension for example un sub class 
                // like APIModel - SOAP/REST we want both file name to be with same extension - ApplicationAPIModel
                return RepositorySerializer.FileExt(this);
            }
        }

        public static string FolderName(Type T)
        {
            // string s =  GetClassShortName(T) + "s";
            string s = T.Name + "s";


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

        public override string ToString()
        {
            return ItemName;
        }


        // TypeName cache
        private static ConcurrentDictionary<string, string> ShortNameDictionary = new ConcurrentDictionary<string, string>();



        public static string GetClassShortName(Type t)
        {
            string ClassName = t.FullName;

            //TODO: make it generic using RS classes dic
            // For speed and in order to to waste mem by creating everytime obj to get name we cache it

            string ShortName = null;
            ShortNameDictionary.TryGetValue(ClassName, out ShortName);
            if (ShortName == null)
            {
                RepositoryItemBase obj = (RepositoryItemBase)(t.Assembly.CreateInstance(ClassName));
                if (obj != null)
                {
                    ShortName = obj.ObjFileExt;
                    ShortNameDictionary.TryAdd(ClassName, ShortName);
                }
                else
                {
                    throw new Exception("GetClassShortName - Unable to create class - " + ClassName);
                }
            }
            return ShortName;
        }

        /// <summary>
        /// Returns the type of Repository item. Respective Repository item should override this method
        /// </summary>
        /// <returns></returns>
        public virtual string GetItemType()
        {
            throw new Exception("Unknown Type for GetItemType " + this.GetType().Name);
        }


        static NewRepositorySerializer mRepositorySerializer = new NewRepositorySerializer();
        public IRepositorySerializer RepositorySerializer
        {
            get
            {
                return mRepositorySerializer;
            }
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
                DirtyCheck(name);
            }
        }

        public bool AllowAutoSave { get; set; }

        Guid mGuid = Guid.Empty;
        [IsSerializedForLocalRepository]
        public Guid Guid
        {
            get
            {
                if (mGuid == Guid.Empty)
                {
                    mGuid = Guid.NewGuid();
                }
                return mGuid;
            }
            set
            {
                if (mGuid != value)
                {
                    mGuid = value;
                    OnPropertyChanged(nameof(Guid));
                }
            }
        }

        public async Task SaveBackupAsync()
        {
            await Task.Run(() =>
            {
                SaveBackup();
            });
        }

        public bool SaveBackup()
        {
            if (DirtyStatus != eDirtyStatus.NoChange)
            {
                return CreateBackup();
            }
            else
            {
                return CreateBackup(true);
            }
        }

        // Deep backup keep obj ref and all prop, restore to real original situation
        public bool CreateBackup(bool isLocalBackup = false)
        {
            if (mBackupInProgress)
            {
                return false;
            }

            try
            {
                mBackupInProgress = true;

                if (!isLocalBackup)
                {
                    mBackupDic = new ConcurrentDictionary<string, object>();
                }
                mLocalBackupDic = new ConcurrentDictionary<string, object>();

                var properties = this.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
                Parallel.ForEach(properties, mi =>
                {
                    if (IsDoNotBackupAttr(mi))
                    {
                        return;
                    }

                    if (!isLocalBackup)
                    {
                        if (mi.Name == nameof(mBackupDic))
                        {
                            return; // since we are running on repo item which contain the dic we need to ignore trying to save it...
                        }

                    }
                    if (mi.Name == nameof(mLocalBackupDic))
                    {
                        return;
                    }
                    object v = null;
                    if (mi.MemberType == MemberTypes.Property)
                    {
                        //Make sure we can do set - not all props have set, so do not save if there is only get
                        PropertyInfo PI = this.GetType().GetProperty(mi.Name);
                        if (PI.CanWrite)
                        {
                            //TODO: mark with no backup
                            //TODO: find better way, make it generic
                            if (mi.Name != nameof(FileName) && mi.Name != nameof(FilePath) && mi.Name != nameof(ObjFolderName) && mi.Name != nameof(ObjFileExt) && mi.Name != nameof(ContainingFolder) && mi.Name != nameof(ContainingFolderFullPath)) // Will cause err to get filename on each repo item
                            {
                                v = PI.GetValue(this);
                            }
                        }
                    }
                    else if (mi.MemberType == MemberTypes.Field)
                    {
                        v = this.GetType().GetField(mi.Name).GetValue(this);
                    }

                    if (!isLocalBackup)
                    {
                        mBackupDic.TryAdd(mi.Name, v);
                    }

                    if (v is IObservableList)
                    {
                        BackupList(mi.Name, (IObservableList)v, isLocalBackup);
                    }
                    else
                    {
                        mLocalBackupDic.TryAdd(mi.Name, v);
                    }
                });

                return true;
            }
            finally
            {
                mBackupInProgress = false;
            }
        }

        private bool IsDoNotBackupAttr(MemberInfo mi)
        {
            var IsSerializedAttr = mi.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute));
            if (IsSerializedAttr == null)
            {
                return true;
            }

            var IsDoNotBackupAttr = mi.GetCustomAttribute(typeof(DoNotBackupAttribute));
            if (IsDoNotBackupAttr != null)
            {
                return true;
            }
            return false;
        }

        public void BackupList(string Name, IObservableList v, bool isLocalBackup = false)
        {
            //TODO: if v is Lazy bak the text without drill down
            List<object> list = new List<object>();
            foreach (object o in v)
            {
                // Run back on each item, so will drill down the hierarchy
                if (o is RepositoryItemBase)
                {
                    ((RepositoryItemBase)o).CreateBackup(isLocalBackup);
                }
                list.Add(o);
            }
            // we keep the original list of items in special name like: Activities~List
            if (!isLocalBackup)
            {
                mBackupDic.TryAdd(Name + "~List", list);
            }
            mLocalBackupDic.TryAdd(Name + "~List", list);
        }

        // Item which will not be saved to the XML - for example dynamic activities or temp output values - no expected or store to
        // Only when in Observable list 
        public virtual bool IsTempItem
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsLinkedItem
        {
            get
            {
                return false;
            }
        }

        public bool ClearBackup(bool isLocalBackup = false)
        {
            if (mBackupInProgress)
            {
                return false;
            }

            var properties = this.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Field);
            foreach (MemberInfo mi in properties)
            {
                if (!isLocalBackup)
                {
                    if (mi.Name == nameof(mBackupDic)) continue;
                }
                if (mi.Name == nameof(mLocalBackupDic)) continue;
                object v = null;
                v = this.GetType().GetField(mi.Name).GetValue(this);
                if (v is IObservableList)
                {
                    foreach (object o in (IObservableList)v)
                    {
                        if (o is RepositoryItemBase)
                        {
                            ((RepositoryItemBase)o).ClearBackup(isLocalBackup);
                        }
                    }
                }
            }
            if (!isLocalBackup)
            {
                mBackupDic = null;
            }
            mLocalBackupDic = null;

            return true;
        }

        private bool RestoreBackup(bool isLocalBackup = false)
        {
            if (mBackupInProgress)
            {
                return false;
            }

            if (isLocalBackup)
            {
                if (mLocalBackupDic == null || mLocalBackupDic.Count == 0)
                {
                    return false;
                }
            }
            else
            {
                if (mBackupDic == null || mBackupDic.Count == 0)
                {
                    return false;
                }
            }

            var properties = this.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
            foreach (MemberInfo mi in properties)
            {
                if (IsDoNotBackupAttr(mi)) continue;
                object v;
                bool b;
                if (isLocalBackup)
                {
                    b = mLocalBackupDic.TryGetValue(mi.Name, out v);
                }
                else
                {
                    b = mBackupDic.TryGetValue(mi.Name, out v);
                }
                if (!b)
                {
                    //TODO: handle Error
                }
                if (mi.MemberType == MemberTypes.Property)
                {
                    // check that we have set method, TODO: do not save it in first place
                    try
                    {
                        //Make sure we can do set - not all props have set
                        PropertyInfo PI = this.GetType().GetProperty(mi.Name);

                        if (typeof(IObservableList).IsAssignableFrom(PI.PropertyType))
                        {
                            IObservableList list = (IObservableList)PI.GetValue(this);
                            if (list != null && list.Count > 0)
                            {
                                RestoreList(mi.Name, list, isLocalBackup);
                            }
                        }
                        else
                        {
                            if (PI.CanWrite)
                            {
                                PI.SetValue(this, v);
                            }
                        }
                    }
                    catch (Exception ex)
                    {// temp fix me 
                        Reporter.ToLog(eLogLevel.DEBUG, "Undo- restoring values from back up", ex);
                    }
                }
                else if (mi.MemberType == MemberTypes.Field)
                {

                    // Do reverse + restore each obj
                    // Do set only if we can really do set, some attrs are get only
                    // FieldInfo fi = this.GetType().GetField(mi.Name, BindingFlags.SetProperty);
                    FieldInfo fi = this.GetType().GetField(mi.Name);


                    if (typeof(IObservableList).IsAssignableFrom(fi.FieldType))
                    {
                        IObservableList list = (IObservableList)fi.GetValue(this);
                        if (list != null && list.Count > 0)
                        {
                            RestoreList(mi.Name, list, isLocalBackup);
                        }
                    }
                    else
                    {

                        if (fi != null && fi.IsStatic == false)
                        {
                            fi.SetValue(this, v);
                        }
                    }
                }

                object item = null;
                if (isLocalBackup)
                {

                    mLocalBackupDic.TryRemove(mi.Name, out item);
                }
                else
                {
                    mBackupDic.TryRemove(mi.Name, out item);
                }
                // Console.WriteLine(mi.MemberType + " : " + mi.ToString() + " " + mi.Name + "=" + v);                
            }
            // make sure we cleared all bak items = full restore
            if (isLocalBackup)
            {
                if (mLocalBackupDic.Count() != 0)
                {
                    // TODO: err handler
                    return false;
                }
            }
            else
            {
                if (mBackupDic.Count() != 0)
                {
                    // TODO: err handler 
                    return false;
                }
            }

            return true;
        }

        private void RestoreList(string Name, IObservableList v, bool isLocalBackup = false)
        {

            try
            {
                v.Clear();
            }
            catch (Exception ex)
            {
                //This is Temporary fix- Inputvalues list throwing observable collection cannot be modified exception
                Reporter.ToLog(eLogLevel.DEBUG, "Clearing list values for restoring from back up", ex);
            }


            object Backuplist;
            bool b;
            b = isLocalBackup ? mLocalBackupDic.TryGetValue(Name + "~List", out Backuplist) : mBackupDic.TryGetValue(Name + "~List", out Backuplist);

            if (b)
            {
                if (Backuplist != null)
                {
                    foreach (object o in ((IList)Backuplist))
                    {
                        v.Add(o);
                        RepositoryItemBase repoItem = o as RepositoryItemBase;
                        repoItem?.RestoreBackup(isLocalBackup);   // Drill down the restore

                    }

                    object item = null;
                    if (isLocalBackup)
                    {
                        mLocalBackupDic.TryRemove(Name + "~List", out item);
                    }
                    else
                    {
                        mBackupDic.TryRemove(Name + "~List", out item);
                    }
                }
                else
                {
                    v = null;
                }
            }
            else
            {
                // TODO: handle err 
            }
        }

        public bool RestoreFromBackup(bool isLocalBackup = false, bool clearBackup = true)
        {
            bool isRestored = false;
            try
            {
                isRestored = RestoreBackup(isLocalBackup);
            }
            catch (Exception exc)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error occurred in the Undo Process", exc);
            }
            finally
            {
                if (isRestored && clearBackup)
                {
                    ClearBackup(isLocalBackup);
                }
            }

            return isRestored;
        }

        private string mFileName = null;
        public string FileName
        {
            get
            {
                if (mFileName == null)
                {
                    return GetNameForFileName();
                }
                else
                {
                    return mFileName;
                }
            }
            set { mFileName = value; }
        }

        public virtual string GetNameForFileName()
        {
            //Only the Repository items which are stored as XML should override this.
            //For other we just return null instead of exception
            return null;
            // In case no override impl then throw
            //throw new Exception("Please override this method in class - " + this.GetType().ToString());
        }

        public abstract string ItemName
        {
            get;
            set;
        }

        public virtual string ItemNameField
        {
            get
            {
                throw new NotImplementedException("Repository Item didn't implement ItemNameField - " + this.GetType().FullName);
            }
        }

        public void InitHeader()
        {
            RepositoryItemHeader = new RepositoryItemHeader()
            {
                Created = GetUTCDateTime(),
                CreatedBy = Environment.UserName,
                GingerVersion = Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationMajorVersion,
                Version = 1,
                LastUpdateBy = Environment.UserName,
                LastUpdate = GetUTCDateTime()

                //TODO: other fields
            };
        }

        public void UpdateHeader()
        {
            RepositoryItemHeader.Version++;
            RepositoryItemHeader.GingerVersion = Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationMajorVersion;
            RepositoryItemHeader.LastUpdateBy = Environment.UserName;
            RepositoryItemHeader.LastUpdate = DateTime.UtcNow;
        }

        private DateTime GetUTCDateTime()
        {
            // We remove the seconds and millis as we don't save them and we want the load date time to match exactly when parsed          
            DateTime dt = DateTime.UtcNow;
            DateTime dt2 = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
            return dt2;
        }

        private RepositoryItemBase CopyRIObject(RepositoryItemBase repoItemToCopy, List<GuidMapper> guidMappingList, bool setNewGUID)
        {
            Type objType = repoItemToCopy.GetType();
            var targetObj = Activator.CreateInstance(objType) as RepositoryItemBase;

            var objMembers = repoItemToCopy.GetType().GetMembers().Where(x => (x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field));

            repoItemToCopy.PrepareItemToBeCopied();
            //targetObj.PreDeserialization();
            Parallel.ForEach(objMembers, mi =>
            {
                try
                {
                    if (IsDoNotBackupAttr(mi))
                    {
                        return;
                    }

                    object memberValue = null;

                    if (mi.MemberType == MemberTypes.Property)
                    {
                        var propInfo = repoItemToCopy.GetType().GetProperty(mi.Name);

                        if (propInfo.CanWrite)
                        {
                            memberValue = propInfo.GetValue(repoItemToCopy);
                            if (memberValue is IObservableList && typeof(IObservableList).IsAssignableFrom(propInfo.PropertyType))
                            {
                                var copiedList = (IObservableList)propInfo.GetValue(targetObj);

                                if (copiedList == null)
                                {
                                    Type listItemType = memberValue.GetType().GetGenericArguments().SingleOrDefault();
                                    var listOfType = typeof(ObservableList<>).MakeGenericType(listItemType);
                                    copiedList = (IObservableList)Activator.CreateInstance(listOfType);
                                }

                                CopyRIList((IObservableList)memberValue, copiedList, guidMappingList, setNewGUID);
                                propInfo.SetValue(targetObj, copiedList);
                            }
                            else
                            {
                                propInfo.SetValue(targetObj, memberValue);
                            }
                        }
                    }
                    else
                    {
                        FieldInfo fieldInfo = repoItemToCopy.GetType().GetField(mi.Name);
                        memberValue = fieldInfo.GetValue(repoItemToCopy);
                        fieldInfo.SetValue(targetObj, memberValue);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Error occurred during object copy of the item: '{0}', type: '{1}', property/field: '{2}'", this.ItemName, this.GetType(), mi.Name), ex);
                }
            });
            //targetObj.PostDeserialization();


            return targetObj;
        }

        private void CopyRIList(IObservableList sourceList, IObservableList targetList, List<GuidMapper> guidMappingList, bool setNewGUID)
        {
            for (int i = 0; i < sourceList.Count; i++)
            {
                object item = sourceList[i];
                if (item is RepositoryItemBase)
                {

                    RepositoryItemBase RI = CopyRIObject(item as RepositoryItemBase, guidMappingList, setNewGUID);
                    if (setNewGUID)
                    {
                        GuidMapper mapping = new GuidMapper();
                        mapping.Original = RI.Guid;
                        RI.Guid = Guid.NewGuid();
                        mapping.newGuid = RI.Guid;
                        guidMappingList.Add(mapping);

                        if ((item as RepositoryItemBase).IsSharedRepositoryInstance && (item as RepositoryItemBase).ParentGuid == Guid.Empty)
                        {
                            RI.ParentGuid = mapping.Original;
                        }

                    }
                    targetList.Add(RI);
                }
                else
                {
                    targetList.Add(item);
                }
            }
        }

        protected bool ItemCopyIsInProgress = false;
        public RepositoryItemBase CreateCopy(bool setNewGUID = true)
        {
            try
            {
                ItemCopyIsInProgress = true;

                List<GuidMapper> guidMappingList = new List<GuidMapper>();
                var duplicatedItem = CopyRIObject(this, guidMappingList, setNewGUID);
                //change the GUID of duplicated item
                if (duplicatedItem != null)
                {
                    if (setNewGUID)
                    {
                        duplicatedItem.ParentGuid = Guid.Empty;   // TODO: why we don't keep parent GUID?                     
                        duplicatedItem.ExternalID = string.Empty;
                        duplicatedItem.Guid = Guid.NewGuid();
                        if (duplicatedItem.IsLinkedItem && duplicatedItem is GingerCore.Activity)
                        {
                            ((GingerCore.Activity)duplicatedItem).Type = eSharedItemType.Regular;
                        }
                        duplicatedItem = duplicatedItem.ReplaceOldGuidUsages(guidMappingList);
                    }
                    duplicatedItem.UpdateCopiedItem();
                    duplicatedItem.DirtyStatus = eDirtyStatus.Modified;
                }
                return duplicatedItem;
            }
            finally
            {
                ItemCopyIsInProgress = false;
            }
        }


        /// <summary>
        /// Update flow control and other places with new guid of entities.
        /// If FC is GoTo action and action got a new guid this method update flow control value with new guid
        /// </summary>
        /// <param name="list">mapping of old and new guids</param>
        /// <returns></returns>
        private RepositoryItemBase ReplaceOldGuidUsages(List<GuidMapper> list)
        {
            string s = RepositorySerializer.SerializeToString(this);

            foreach (GuidMapper mapper in list)
            {
                s = s.Replace(mapper.Original.ToString(), mapper.newGuid.ToString());
                s = s.Replace("ParentGuid=" + "\"" + mapper.newGuid.ToString() + "\"", "ParentGuid=" + "\"" + mapper.Original.ToString() + "\"");
            }

            return (RepositoryItemBase)RepositorySerializer.DeserializeFromText(this.GetType(), s, filePath: string.Empty);
        }



        private RepositoryItemKey mRepositoryItemKey;

        public RepositoryItemKey Key
        {
            get
            {
                if (mRepositoryItemKey == null)
                {
                    mRepositoryItemKey = new RepositoryItemKey();
                }
                // we keep it updated just in case, to get the latest and greatest key any time requested
                mRepositoryItemKey.Guid = this.Guid;
                mRepositoryItemKey.ItemName = this.ItemName;
                return mRepositoryItemKey;
            }
        }

        public virtual string RelativeFilePath { get; set; }

        public virtual void UpdateBeforeSave()
        {
            this.ClearBackup();
        }

        public string GetContainingFolder()
        {
            string containingFolder = string.Empty;
            try
            {
                int startIndx = this.FileName.ToUpper().IndexOf(this.ObjFolderName.ToUpper());
                int endIndx = this.FileName.LastIndexOf('\\');
                if (endIndx > startIndx)
                    containingFolder = this.FileName.Substring(startIndx, endIndx - startIndx) + "\\";
                return containingFolder;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return containingFolder;
            }
        }

        public virtual bool FilterBy(eFilterBy filterType, object obj)
        {
            return false;
        }

        ///Do not use,This field will be removed. All the folder paths, Solution repository should know based on repo item type
        private string mContainingFolder = null;
        public string ContainingFolder
        {
            get
            {
                if (mContainingFolder == null)
                {
                    mContainingFolder = GetContainingFolder();
                    return mContainingFolder;
                }
                else
                {
                    return mContainingFolder;
                }
            }
            set { mContainingFolder = value; }
        }

        private string mContainingFolderFullPath = null;
        public string ContainingFolderFullPath
        {
            get
            {
                if (mContainingFolderFullPath == null)
                {
                    if (!string.IsNullOrEmpty(mFileName))
                    {
                        //TODO: cleanup later - use only below
                        mContainingFolderFullPath = System.IO.Path.GetDirectoryName(mFileName);
                    }
                    else
                    {
                        mContainingFolderFullPath = System.IO.Path.GetDirectoryName(mFilePath);
                    }
                }
                return mContainingFolderFullPath;
            }
            set { mContainingFolderFullPath = value; }
        }

        private string mFilePath = null;
        public string FilePath
        {
            get
            {
                if (mFilePath == null)
                {
                    return GetNameForFileName();
                }
                else
                {
                    return mFilePath;
                }
            }
            set { if (mFilePath != value) { mFilePath = value; OnPropertyChanged(nameof(FilePath)); } }
        }

        public virtual eImageType ItemImageType
        {
            get
            {
                //throw new NotImplementedException("ItemImageType not defined for: " + this.GetType().FullName); 
                return eImageType.Null;
            }
        }

        #region SourceControl

        private static ISourceControl SourceControl;

        private eImageType mSourceControlStatus = eImageType.Null;
        public eImageType SourceControlStatus
        {
            get
            {
                if (mSourceControlStatus == eImageType.Null)
                {
                    mSourceControlStatus = eImageType.Pending;
                }
                return mSourceControlStatus;
            }
        }

        public async void RefreshSourceControlStatus()
        {
            if (SourceControl != null && mSourceControlStatus != eImageType.Null)
            {
                mSourceControlStatus = await SourceControl.GetFileStatusForRepositoryItemPath(mFilePath);
                OnPropertyChanged(nameof(SourceControlStatus));
            }
        }

        public static void SetSourceControl(ISourceControl sourceControl)
        {
            SourceControl = sourceControl;
        }
        #endregion SourceControl

        #region Dirty


        eDirtyStatus mDirtyStatus;
        public eDirtyStatus DirtyStatus
        {
            get
            {
                return mDirtyStatus;
            }
            set
            {
                if (mDirtyStatus != value)
                {
                    mDirtyStatus = value;
                    if (value == eDirtyStatus.Modified)
                    {
                        RaiseDirtyChangedEvent();
                        // check that path is really valid path to a file contaning both drive at the start & a target with extension, also in order to keep the list unique, check if the value has already been added to the list.
                        if (!String.IsNullOrEmpty(FilePath) && Path.IsPathFullyQualified(FilePath) && !GingerCoreCommonWorkSpace.Instance.SolutionRepository.ModifiedFiles.Contains(this))
                        {
                            GingerCoreCommonWorkSpace.Instance.SolutionRepository.ModifiedFiles.Add(this);
                        }
                    }
                    OnPropertyChanged(nameof(DirtyStatus));
                    OnPropertyChanged(nameof(DirtyStatusImage));
                }
            }
        }

        public eImageType? DirtyStatusImage
        {
            get
            {

                if (mDirtyStatus == eDirtyStatus.Modified)
                {
                    return eImageType.ItemModified;
                }
                else
                {
                    return eImageType.Empty;
                }
            }
        }

        public eDirtyTracking DirtyTracking = eDirtyTracking.NotStarted;


        public event EventHandler OnDirtyStatusChanged;

        void RaiseDirtyChangedEvent()
        {
            if (OnDirtyStatusChanged != null)
            {
                OnDirtyStatusChanged(this, new EventArgs());
            }
        }


        private void DirtyCheck(string name)
        {
            if (DirtyStatus != eDirtyStatus.NoTracked && DirtyTrackingFields != null && DirtyTrackingFields.Contains(name) && DirtyTracking != eDirtyTracking.Paused)
            {
                DirtyStatus = eDirtyStatus.Modified;
                // RaiseDirtyChangedEvent();
            }
        }


        public void RaiseDirtyChanged(object sender, EventArgs e)
        {
            if (DirtyTracking != eDirtyTracking.Paused)
            {
                DirtyStatus = eDirtyStatus.Modified;
            }
            // RaiseDirtyChangedEvent();
        }



        internal void ChildCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // each change in Observavle will mark the item modified - all NotifyCollectionChangedAction.*
            if (DirtyTracking != eDirtyTracking.Paused)
            {
                DirtyStatus = eDirtyStatus.Modified;
                #region Collection Action - Add
                // if item added set tracking too
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (object obj in e.NewItems)
                    {
                        if (obj is RepositoryItemBase)
                        {
                            RepositoryItemBase repositoryItemBase = (RepositoryItemBase)obj;
                            repositoryItemBase.StartDirtyTracking();
                            repositoryItemBase.OnDirtyStatusChanged += this.RaiseDirtyChanged;
                            repositoryItemBase.DirtyStatus = eDirtyStatus.Modified;
                        }
                        else
                        {
                            // not RI no tracking...
                        }
                    }
                }
                #endregion
                #region Collection Action - Remove Or Move
                else if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Move)
                {
                    foreach (object obj in e.OldItems)
                    {
                        if (obj is RepositoryItemBase)
                        {
                            ((RepositoryItemBase)obj).DirtyStatus = eDirtyStatus.Modified;
                        }
                    }
                }
                #endregion
                #region Collection Action - Replace
                else if (e.Action == NotifyCollectionChangedAction.Replace)
                {
                    foreach (object obj in e.NewItems)
                    {
                        if (obj is RepositoryItemBase)
                        {
                            ((RepositoryItemBase)obj).DirtyStatus = eDirtyStatus.Modified;
                        }
                    }
                    foreach (object obj in e.OldItems)
                    {
                        if (obj is RepositoryItemBase)
                        {
                            ((RepositoryItemBase)obj).DirtyStatus = eDirtyStatus.Modified;
                        }
                    }
                }
                #endregion
            }
        }

        public void PauseDirtyTracking()
        {
            if (DirtyTracking != eDirtyTracking.NotStarted)
            {
                DirtyTracking = eDirtyTracking.Paused;
                PropertyChanged -= ItmePropertyChanged;
            }
        }

        public void ResumeDirtyTracking()
        {

            if (DirtyTracking == eDirtyTracking.Paused)
            {
                DirtyTracking = eDirtyTracking.Started;
                PropertyChanged += ItmePropertyChanged;
            }
        }

        ConcurrentBag<string> DirtyTrackingFields;
        public void StartDirtyTracking()
        {
            if (DirtyStatus != eDirtyStatus.NoTracked && DirtyTracking != eDirtyTracking.NotStarted)
            {
                // Nothing to do
                return;
            }

            DirtyTrackingFields = new ConcurrentBag<string>();
            DirtyStatus = eDirtyStatus.NoChange;
            //first track self item changes
            if (PropertyChanged == null)
            {
                PropertyChanged += ItmePropertyChanged;
            }
            //set dirty tracking to started
            DirtyTracking = eDirtyTracking.Started;

            // now track all children which are marked with isSerizalized...
            // throw err if item is serialized but dindn't impl IsDirty

            // Properties
            Parallel.ForEach(this.GetType().GetProperties(), PI =>
            {
                var token = PI.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute));
                if (token == null)
                {
                    return;
                }
                DirtyTrackingFields.Add(PI.Name);

                // We track observable list which are seriazlized - drill down recursivley in obj tree
                if (typeof(IObservableList).IsAssignableFrom(PI.PropertyType))
                {
                    //skip list if it is LazyLoad and was not loaded yet
                    var lazyLoadtoken = PI.GetCustomAttribute(typeof(IsLazyLoadAttribute));
                    if (lazyLoadtoken != null)
                    {
                        string lazyStatusProp = PI.Name + nameof(IObservableList.LazyLoad);
                        if (this.GetType().GetProperty(lazyStatusProp) != null)
                        {
                            if (bool.Parse(this.GetType().GetProperty(PI.Name + nameof(IObservableList.LazyLoad)).GetValue(this).ToString()) == true)
                            {
                                return;//skip doing dirty tracking for observableList which is LazyLoad and not loaded yet
                            }
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to check if to start DirtyTracking for Lazy Load ObservabelList called '{0}' because the property '{1}' is missing", PI.Name, lazyStatusProp));
                        }
                    }

                    IObservableList obj = (IObservableList)PI.GetValue(this);
                    if (obj == null)
                    {
                        return;
                    }
                    TrackObservableList((IObservableList)obj);
                }
                // track changes in childern which are RepositoryItemBase
                if (typeof(RepositoryItemBase).IsAssignableFrom(PI.PropertyType))
                {
                    RepositoryItemBase obj = (RepositoryItemBase)PI.GetValue(this);

                    if (obj == null)
                    {
                        return;
                    }
                    obj.OnDirtyStatusChanged += this.RaiseDirtyChanged;
                }
            });

            // Fields
            Parallel.ForEach(this.GetType().GetFields(), FI =>
            {
                var token = FI.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute));
                if (token == null)
                {
                    return;
                }

                DirtyTrackingFields.Add(FI.Name);

                // We track observable list which are seriazlized - drill down recursivley in obj tree
                if (typeof(IObservableList).IsAssignableFrom(FI.FieldType))
                {
                    IObservableList obj = (IObservableList)FI.GetValue(this);
                    if (obj == null)
                    {
                        return;
                    }
                    TrackObservableList((IObservableList)obj);

                }
            });

        }

        public void TrackObservableList(IObservableList obj)
        {
            // No need to track items which are lazy load            
            List<object> items = ((IObservableList)obj).ListItems;

            ((INotifyCollectionChanged)obj).CollectionChanged += ((RepositoryItemBase)this).ChildCollectionChanged;

            Parallel.ForEach(items, item =>
            {
                if (item is RepositoryItemBase)
                {
                    RepositoryItemBase RI = ((RepositoryItemBase)item);
                    // Do start tracking only for item which are not already tracked
                    if (RI.DirtyStatus == eDirtyStatus.NoTracked)
                    {
                        RI.StartDirtyTracking();
                    }
                    RI.OnDirtyStatusChanged += this.RaiseDirtyChanged;
                }
                else
                {

                    // for now we ignore list of Guids - like Agents.Tags as user cannot change the value, but if he add/remove it will be tracked
                    if (item is Guid || item is RepositoryItemKey)
                    {
                        return;
                    }
                    throw new Exception("Error: trying to track object which is Serialized in a list but is not RepositoryItemBase " + this.GetType().FullName + " " + item.ToString());
                }
            });
        }

        private void ItmePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (DirtyTrackingFields.Contains(e.PropertyName))
            {
                if (((RepositoryItemBase)sender).DirtyStatus != eDirtyStatus.Modified)
                {
                    ((RepositoryItemBase)sender).DirtyStatus = eDirtyStatus.Modified;
                    // ((RepositoryItemBase)sender).OnPropertyChanged(nameof(DirtyStatus));
                }
            }
        }

        /// <summary>
        /// This method is used to set the DirtyStatus to NoChange to item and it's child items
        /// </summary>
        public void SetDirtyStatusToNoChange()
        {
            DirtyStatus = eDirtyStatus.NoChange;

            // Properties
            foreach (PropertyInfo PI in this.GetType().GetProperties())
            {
                var token = PI.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute));
                if (token == null) continue;

                if (typeof(IObservableList).IsAssignableFrom(PI.PropertyType))
                {
                    IObservableList obj = (IObservableList)PI.GetValue(this);
                    if (obj == null) continue;
                    foreach (object o in obj)
                        if (o is RepositoryItemBase)
                            ((RepositoryItemBase)o).SetDirtyStatusToNoChange();
                }
                if (typeof(RepositoryItemBase).IsAssignableFrom(PI.PropertyType))
                {
                    RepositoryItemBase obj = (RepositoryItemBase)PI.GetValue(this);

                    if (obj == null)
                    {
                        continue;
                    }
                    obj.SetDirtyStatusToNoChange();
                }
            }

            // Fields
            foreach (FieldInfo FI in this.GetType().GetFields())
            {
                var token = FI.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute));
                if (token == null) continue;
                if (typeof(IObservableList).IsAssignableFrom(FI.FieldType))
                {
                    IObservableList obj = (IObservableList)FI.GetValue(this);
                    if (obj == null) return;
                    foreach (object o in obj)
                        if (o is RepositoryItemBase)
                            ((RepositoryItemBase)o).SetDirtyStatusToNoChange();
                }
            }
        }

        // test after save dirt should be reset to no change
        // undo shoudl reset to - restpre from bak

        #endregion Dirty


        public RepositoryItemBase CreateInstance(bool originFromSharedRepository = false)
        {
            RepositoryItemBase copiedItem = this.CreateCopy();
            copiedItem.ParentGuid = this.Guid;
            if (originFromSharedRepository)
            {
                copiedItem.IsSharedRepositoryInstance = true;
                copiedItem.ExternalID = this.ExternalID;
            }
            return copiedItem;
        }

        bool mIsSharedRepositoryInstance = false;
        public bool IsSharedRepositoryInstance
        {
            get
            {
                return mIsSharedRepositoryInstance;
            }
            set
            {
                if (mIsSharedRepositoryInstance != value)
                {
                    mIsSharedRepositoryInstance = value;

                    OnPropertyChanged(nameof(SharedRepoInstanceImage));
                    OnPropertyChanged(nameof(IsSharedRepositoryInstance));
                }
            }
        }

        public eImageType SharedRepoInstanceImage
        {
            get
            {
                if (IsSharedRepositoryInstance)
                {
                    return eImageType.SharedRepositoryItem;
                }
                else
                {
                    return eImageType.NonSharedRepositoryItem;
                }
            }
        }

        public virtual RepositoryItemBase GetUpdatedRepoItem(RepositoryItemBase selectedItem, RepositoryItemBase existingItem, string itemPartToUpdate)
        {
            throw new NotImplementedException("GetUpdatedRepoItem() was not implemented for this Item type");
        }

        public virtual void UpdateInstance(RepositoryItemBase instanceItem, string itemPartToUpdate, RepositoryItemBase hostItem = null, object extraDetails = null)
        {
            throw new NotImplementedException("UpdateInstance() was not implemented for this Item type");
        }

        public static void ObjectsDeepCopy(RepositoryItemBase sourceObj, RepositoryItemBase targetObj)
        {
            NewRepositorySerializer repoSer = new NewRepositorySerializer();

            string sourceObjXml = repoSer.SerializeToString(sourceObj);
            NewRepositorySerializer RS = new NewRepositorySerializer();

            RS.DeserializeFromTextWithTargetObj(sourceObj.GetType(), sourceObjXml, targetObj);
        }

        public virtual void UpdateItemFieldForReposiotryUse()
        {
            UpdateControlFields();
        }

        public void UpdateControlFields()
        {
            // from old RI
        }

        /// <summary>
        /// This method is being called when object type is read in xml which is being deserialzied and before the properties/fields are updated
        /// Overrid this method if you need to initial repository item as soon as it is created to set default for example
        /// </summary>
        public virtual void PreDeserialization()
        {
        }

        /// <summary>
        /// This method is being called afetr object type is read from xml and all properties/fields been deserialzied
        /// Use this method to do updates to the object being serialzied 
        /// </summary>
        public virtual void PostDeserialization()
        {
        }

        /// <summary>
        // When xml contain field/property which doesn't exist in the object being deserialzed to, then this method will be called
        // Use it when you need to convert old property to new name or type
        // if handled return true
        /// </summary>
        /// <param name="errorType"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>        
        public virtual bool SerializationError(SerializationErrorType errorType, string name, string value)
        {
            // override method in sub class need to impelment and return true if handled
            return false;
        }

        /// <summary>
        /// This method is being called on original item to be copied before copy process is starting
        /// Overrid this method if you need to modify some object member before it been copied
        /// </summary>
        public virtual void PrepareItemToBeCopied()
        {
        }

        /// <summary>
        /// This method is being called on copied item after copy process is ended
        /// Overrid this method if you need to modify some object member after it been copied
        /// </summary>
        public virtual void UpdateCopiedItem()
        {
        }

        public virtual void PostSaveHandler()
        {
        }
        public virtual bool PreSaveHandler()
        {
            return false;
        }

        bool mPublish = false;
        /// <summary>
        /// Flag used to mark if item is ready to be published on third party applications which enhancing Ginger framework
        /// </summary>
        [IsSerializedForLocalRepository]
        public bool Publish
        {
            get
            {
                return mPublish;
            }
            set
            {
                if (mPublish != value)
                {
                    mPublish = value;
                    OnPropertyChanged(nameof(Publish));
                }
            }
        }

        public Guid ExecutionParentGuid { get; set; } = Guid.Empty;

    }
}
