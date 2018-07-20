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

//using Amdocs.Ginger.Common;
//using Amdocs.Ginger.Common.Repository;
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.GeneralLib;
//using GingerCoreNET.ReporterLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;

//namespace GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.VariablesLib
//{
//    public enum eVariablesLevel
//    {
//        BusinessFlow = 0,
//        Activity = 1,
//        Solution = 2
//    }
    
//    public abstract class VariableBase : RepositoryItem
//    {
//        public enum eItemParts
//        {
//            All,
//            Details
//        }
        
//        public static partial class Fields
//        {
//            public static string Image = "Image";
//            public static string Name = "Name";
//            public static string Description = "Description";
//            public static string VariableUIType = "VariableUIType";            
//            public static string Value = "Value";
//            public static string Formula = "Formula";        
//            public static string ParentType = "ParentType";
//            public static string ParentName = "ParentName";
//            public static string VarValChanged = "VarValChanged";
//            public static string DiffrentFromOrigin = "DiffrentFromOrigin";            
//            public static string VariableEditPage = "VariableEditPage";
//            public static string LinkedVariableName = "LinkedVariableName";
//            public static string SetAsInputValue = "SetAsInputValue";
//            public static string SetAsOutputValue = "SetAsOutputValue";
//            public static string PossibleOutputVariables = "PossibleOutputVariables";
//            public static string MappedOutputVariable = "MappedOutputVariable";
//            public static string MappedOutputType = "MappedOutputType";
//            public static string MappedOutputValue = "MappedOutputValue";
//            public static string SupportSetValue = "SupportSetValue";
//        }

//        public override string ToString()
//        {
//            return Name;
//        }
        
//        public enum eOutputType
//        {
//            None,
//            Variable,
//            DataSource
//        }
        
//        private bool mSetAsInputValue=true;
//        [IsSerializedForLocalRepository]
//        public bool SetAsInputValue
//        {
//            get { return mSetAsInputValue; }
//            set
//            {
//                if (mSetAsInputValue != value)
//                {
//                    mSetAsInputValue = value;
//                    OnPropertyChanged(Fields.SetAsInputValue);
//                }
//            }
//        }

//        private bool mSetAsOutputValue = true;
//        [IsSerializedForLocalRepository]
//        public bool SetAsOutputValue
//        {
//            get { return mSetAsOutputValue; }
//            set
//            {
//                if (mSetAsOutputValue != value)
//                {
//                    mSetAsOutputValue = value;
//                    OnPropertyChanged(Fields.SetAsOutputValue);
//                }
//            }
//        }

//        private string mName;
//        [IsSerializedForLocalRepository]
//        public string Name
//        {
//            get { return mName; }
//            set
//            {
//                mName = value;
//                OnPropertyChanged("Name");
//            }
//        }

//        private string mDescription;
//        [IsSerializedForLocalRepository]
//        public string Description
//        {
//            get { return mDescription; }
//            set
//            {
//                mDescription = value;
//                OnPropertyChanged("Description");
//            }
//        }

//        private string mValue;       
//        //TODO: fixme value is temp and should not be serialized
//        [IsSerializedForLocalRepository]
//        public virtual string Value
//        {
//            get
//            {
//                return mValue;
//            }
//            set
//            {
//                mValue = value;
//                OnPropertyChanged("Value");
//            }
//        }

        
//        //TODO: VariableBase should not know on VariableSelectionList. handle in sub class when requesting formula do override
//        private string mFormula;
//        public string Formula
//        {
//            get
//            {
//                string formula = GetFormula();
//                if (formula != mFormula)
//                {
//                    mFormula = formula;
//                    if ((this is VariableSelectionList) == false)
//                        this.ResetValue();
//                    OnPropertyChanged(Fields.Formula);
//                }
//                return mFormula;
//            }
//            set
//            {
//                if (value != mFormula)
//                {
//                    mFormula = value;
//                    if ((this is VariableSelectionList) == false)
//                        this.ResetValue();
//                    OnPropertyChanged(Fields.Formula);
//                }
//            }
//        }

//        public abstract string GetFormula();
//        public abstract string VariableType();
//        public abstract void ResetValue();
//        public abstract void GenerateAutoValue();
//        public override string GetNameForFileName() { return Name; }
//        public abstract string VariableEditPage { get; }

//        //all below used to describe the variable owner in a specific Business Flow
//        [IsSerializedForLocalRepository]
//        public string ParentType { get; set; } //BusinessFlow or Activity
//        [IsSerializedForLocalRepository]
//        public string ParentName { get; set; }

//        private bool mVarValChanged;
//        [IsSerializedForLocalRepository]
//        public bool VarValChanged { get { return mVarValChanged; } set { mVarValChanged = value; OnPropertyChanged(Fields.VarValChanged); } }

//        //used to identify the variables which the user customized for specific BF run
//        private bool mDiffrentFromOrigin;
//        [IsSerializedForLocalRepository]
//        public bool DiffrentFromOrigin
//        {
//            get
//            {
//                if (mDiffrentFromOrigin == true)
//                {
//                    if (MappedOutputType == eOutputType.None)
//                        VarValChanged = true;
//                }
//                return mDiffrentFromOrigin;
//            }
//            set { mDiffrentFromOrigin = value; OnPropertyChanged(Fields.DiffrentFromOrigin); } }
        
//        public static void UpdateVariableNameChangeInItem(object item, string prevVarName, string newVarName, ref bool namechange)
//        {
//            var properties = item.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
//            foreach (MemberInfo mi in properties)
//            {
//                if (mi.Name == "BackupDic" || mi.Name == "FileName" ||
//                    mi.Name == "ObjFolderName" || mi.Name == "ObjFileExt" ||
//                    mi.Name == "ActInputValues" || mi.Name == "ActReturnValues" || mi.Name == "ActFlowControls" ||
//                    mi.Name == "ContainingFolder" || mi.Name == "ContainingFolderFullPath") continue;

//                //Get the attr value
//                PropertyInfo PI = item.GetType().GetProperty(mi.Name);
//                dynamic value = null;
//                if (mi.MemberType == MemberTypes.Property)
//                    value = PI.GetValue(item);
//                else if (mi.MemberType == MemberTypes.Field)
//                    value = item.GetType().GetField(mi.Name).GetValue(item);

//                if (value is IObservableList)
//                {
//                    foreach (object o in value)
//                        UpdateVariableNameChangeInItem(o, prevVarName, newVarName, ref namechange);
//                }
//                else
//                {
//                    if (value != null)
//                    {
//                        if (mi.Name == "VariableName")
//                        {
//                            if (value == prevVarName)
//                                PI.SetValue(item, newVarName);
//                            namechange = true;
//                        }
//                        else if (mi.Name == "StoreToValue")
//                        {
//                            if (value == prevVarName)
//                                PI.SetValue(item, newVarName);
//                            else if (value.IndexOf("{Var Name=" + prevVarName + "}") > 0)
//                                PI.SetValue(item, value.Replace("{Var Name=" + prevVarName + "}", "{Var Name=" + newVarName + "}"));                            
//                            namechange = true;
//                        }
//                        else
//                        {
//                            try
//                            {
//                                if (PI.CanWrite)
//                                {
//                                    string stringValue = value.ToString();
//                                    string variablePlaceHoler = "{Var Name=xx}";
//                                    if (stringValue.Contains(variablePlaceHoler.Replace("xx", prevVarName)))
//                                    {
//                                        PI.SetValue(item, stringValue.Replace(variablePlaceHoler.Replace("xx", prevVarName), variablePlaceHoler.Replace("xx", newVarName)));
//                                        namechange = true;
//                                    }
//                                }
//                            }
//                            catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
//                        }
//                    }
//                }
//            }
//        }

//        public static void GetListOfUsedVariables(object item, ref List<string> usedVariables)
//        {
//            var properties = item.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
//            foreach (MemberInfo mi in properties)
//            {
//                if (mi.Name == "BackupDic" || mi.Name == "FileName" ||
//                    mi.Name == "ObjFolderName" || mi.Name == "ObjFileExt" ||
//                    mi.Name == "ActInputValues" || mi.Name == "ActReturnValues" || mi.Name == "ActFlowControls" || mi.Name == "ScreenShots" ||
//                    mi.Name == "ContainingFolder" || mi.Name == "ContainingFolderFullPath") continue;

//                //Get the attr value
//                PropertyInfo PI = item.GetType().GetProperty(mi.Name);
//                dynamic value = null;
//                if (mi.MemberType == MemberTypes.Property)
//                    value = PI.GetValue(item);
//                else if (mi.MemberType == MemberTypes.Field)
//                    value = item.GetType().GetField(mi.Name).GetValue(item);

//                if (value is IObservableList)
//                {
//                    List<dynamic> list = new List<dynamic>();
//                    foreach (object o in value)
//                        GetListOfUsedVariables(o, ref usedVariables);
//                }
//                else
//                {
//                    if (value != null)
//                    {
//                        try
//                        {
//                            if (PI.CanWrite)
//                            {
//                                if (mi.Name == "StoreToValue" && mi.DeclaringType.Name=="ActReturnValue" && value.ToString().IndexOf("{DS Name") == -1)
//                                {
//                                    if (value.ToString() != string.Empty)
//                                        if (usedVariables.Contains(value.ToString()) == false)
//                                            usedVariables.Add(value.ToString());
//                                }
//                                else
//                                {
//                                    string stringValue = value.ToString();
//                                    string[] splitedValue = stringValue.Split(new char[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
//                                    if (splitedValue.Length > 0)
//                                    {
//                                        for (int indx = 0; indx < splitedValue.Length; indx++)
//                                        {
//                                            string val = splitedValue[indx];
//                                            if (val.Contains("Var Name=") == true)
//                                            {
//                                                val = val.Replace("Var Name=", "");
//                                                if (usedVariables.Contains(val) == false)
//                                                    usedVariables.Add(val);
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                        catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
//                    }
//                }
//            }
//        }

//        [IsSerializedForLocalRepository]
//        public string LinkedVariableName { get; set; }

//        public override string ItemName
//        {
//            get
//            {
//                return this.Name;
//            }
//            set
//            {
//                this.Name = value;
//            }
//        }

//        [IsSerializedForLocalRepository]
//        public ObservableList<Guid> Tags = new ObservableList<Guid>();
        
//        private eOutputType mMappedOutputType;
//        [IsSerializedForLocalRepository]
//        public eOutputType MappedOutputType
//        {
//            get { return mMappedOutputType; }

//            set
//            {
//                mMappedOutputType = value;
//                OnPropertyChanged(Fields.MappedOutputType);
//            }
//        }

//        private string mMappedOutputValue;
//        [IsSerializedForLocalRepository]
//        public string MappedOutputValue
//        {
//            get
//            {
//                return mMappedOutputValue;
//            }
//            set
//            {
//                mMappedOutputValue = value;
//                if (String.IsNullOrEmpty(value) == false || VarValChanged == true)                
//                    DiffrentFromOrigin = true;
//                else
//                    DiffrentFromOrigin = false;
//                OnPropertyChanged(Fields.MappedOutputValue);
//            }
//        }
//        public abstract bool SupportSetValue { get; }
        
//        public abstract String VariableUIType { get; }
//    }
//}
