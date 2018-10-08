//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.Common.Repository;
//using amdocs.ginger.GingerCoreNET;
//using Amdocs.Ginger.CoreNET.SolutionRepositoryLib.RepositoryObjectsLib;
//using GingerCoreNET.GeneralLib;
//using System;
//using System.ComponentModel;
//using System.Reflection;
//using Amdocs.Ginger.Common;
//using Amdocs.Ginger.Repository;

//namespace GingerCoreNET.SolutionRepositoryLib
//{
//    public abstract class RepositoryItem : RepositoryItemBase, INotifyPropertyChanged
//    {
//        public string ObjFolderName { get { return FolderName(this.GetType()); } }
//        public Guid ParentGuid { get; set; }
//        public bool Deleted { get; set; }
        
//        public static string FolderName(Type T)
//        {
//            string s = GetClassShortName(T) + "s";
//            if (s.EndsWith("ys"))
//            {
//                s = s.Replace("ys", "ies");
//            }

//            //Special handling for Shared Repository item to be in sub folder
//            if (s == "ActivitiesGroups" || s == "Activities" || s == "Actions" || s == "Variables" || s == "Validations")
//            {
//                s = @"SharedRepository\" + s;
//            }
//            return s;
//        }

//        public RepositoryItem()
//        {
//            UpdateControlFields();
//        }

//        public void UpdateControlFields()
//        {
//            Deleted = false;
//        }

//        public void SaveParentGUID()
//        {
//            ParentGuid = Guid;
//            Guid = Guid.NewGuid();
//        }

//        //TODO: fixme to use all annotated fields?! or remove
//        public void InvokPropertyChanngedForAllFields()
//        {
//            try
//            {
//                Type fieldsObjType = this.GetType().GetNestedType("Fields");
//                foreach (var field in fieldsObjType.GetFields())
//                {
//                    MethodInfo OnPropertyChangedMethod = this.GetType().GetMethod("OnPropertyChanged");
//                    OnPropertyChangedMethod.Invoke(this, new object[] { field.Name });
//                }
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }

//        public string RelativeFilePath
//        {
//            get
//            {
//                return System.IO.Path.Combine(ContainingFolder, System.IO.Path.GetFileName(FilePath));
//            }
//        }

//        public static string FileExt(string Ext)
//        {
//            return "Ginger." + Ext;
//        }
        
//        public virtual string ObjFileExt
//        {
//            get
//            {
//                // We can override if we want differnet extension for example un sub class 
//                // like APIModel - SOAP/REST we want both file name to be with same extension - ApplicationAPIModel
//                return FileExt(this.GetType());
//            }
//        }

//        /// <summary>
//        /// Been used for updating the Shared Reporsitiry item instance
//        /// </summary>
//        public virtual void UpdateInstance(RepositoryItem instanceItem, string itemPartToUpdate, RepositoryItem hostItem = null)
//        {
//            throw new Exception("UpdateInstance() was not implemented for this Item type");
//        }
//    }
//}
