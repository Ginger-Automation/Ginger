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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;

namespace GingerCore.Variables
{
    public enum eVariablesLevel
    {
        BusinessFlow = 0,
        Activity = 1,
        Solution = 2
    }


    public abstract class VariableBase : RepositoryItemBase
    {

        public enum eSetValueOptions
        {
            [EnumValueDescription("Set Value")]
            SetValue,
            [EnumValueDescription("Reset Value")]
            ResetValue,
            [EnumValueDescription("Auto Generate Value")]
            AutoGenerateValue,
            [EnumValueDescription("Start Timer")]
            StartTimer,
            [EnumValueDescription("Stop Timer")]
            StopTimer,
            [EnumValueDescription("Continue Timer")]
            ContinueTimer,
            [EnumValueDescription("Clear Special Characters")]
            ClearSpecialChar
        }

        public enum eItemParts
        {
            All
            //Details
        }
        
        public new static partial class Fields
        {
            public static string Image = "Image";
            public static string Name = "Name";
            public static string Description = "Description";
            public static string VariableUIType = "VariableUIType";            
            public static string Value = "Value";
            public static string Formula = "Formula";        
            public static string ParentType = "ParentType";
            public static string ParentName = "ParentName";
            public static string VarValChanged = "VarValChanged";
            public static string DiffrentFromOrigin = "DiffrentFromOrigin";            
            public static string VariableEditPage = "VariableEditPage";
            public static string LinkedVariableName = "LinkedVariableName";
            public static string SetAsInputValue = "SetAsInputValue";
            public static string SetAsOutputValue = "SetAsOutputValue";
            public static string PossibleOutputVariables = "PossibleOutputVariables";
            public static string MappedOutputVariable = "MappedOutputVariable";
            public static string MappedOutputType = "MappedOutputType";
            public static string MappedOutputValue = "MappedOutputValue";
            public static string SupportSetValue = "SupportSetValue";
        }

        public override string ToString()
        {
            return Name;
        }
        
        public enum eOutputType
        {
            None,
            Variable,
            DataSource
        }

        private bool mSetAsInputValue=true;
        [IsSerializedForLocalRepository(true)]
        public bool SetAsInputValue
        {
            get { return mSetAsInputValue; }
            set
            {
                if (mSetAsInputValue != value)
                {
                    mSetAsInputValue = value;
                    OnPropertyChanged(Fields.SetAsInputValue);
                }
            }
        }

        private bool mSetAsOutputValue = true;
        [IsSerializedForLocalRepository(true)]
        public bool SetAsOutputValue
        {
            get { return mSetAsOutputValue; }
            set
            {
                if (mSetAsOutputValue != value)
                {
                    mSetAsOutputValue = value;
                    OnPropertyChanged(Fields.SetAsOutputValue);
                }
            }
        }

        private string mName;
        [IsSerializedForLocalRepository]
        public string Name
        {
            get { return mName; }
            set
            {
                mName = value;
                OnPropertyChanged("Name");
            }
        }

        private string mDescription;
        [IsSerializedForLocalRepository]
        public string Description
        {
            get { return mDescription; }
            set
            {
                mDescription = value;
                OnPropertyChanged("Description");
            }
        }

        private string mValue;       
        //TODO: fixme value is temp and should not be serialized
        [IsSerializedForLocalRepository]
        public virtual string Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
                OnPropertyChanged("Value");
            }
        }

        private string mFormula;
        public string Formula
        {
            get
            {
                string formula = GetFormula();                
                if (formula != mFormula)
                {
                    mFormula = formula;
                    if ((this is VariableSelectionList) == false) 
                    {
                            if(mFormula != null)
                            this.ResetValue();
                    }        
                    
                    OnPropertyChanged(Fields.Formula);
                }
                return mFormula;
            }
            set
            {
                if (value != mFormula)
                {
                    mFormula = value;
                    if ((this is VariableSelectionList) == false) 
                    {
                           if (mFormula != null)
                           this.ResetValue();
                    }
                    
                    OnPropertyChanged(Fields.Formula);
                }
            }
        }
        public abstract string GetFormula();
        public abstract string VariableType();
        public abstract void ResetValue();
        public abstract void GenerateAutoValue();
        public virtual eImageType Image { get { return eImageType.Variable; } }
        public override string GetNameForFileName() { return Name; }
        public abstract string VariableEditPage { get; }


        //all below used to describe the variable owner in a specific Business Flow
        [IsSerializedForLocalRepository]
        public string ParentType { get; set; } //BusinessFlow or Activity
        [IsSerializedForLocalRepository]
        public string ParentName { get; set; }
        private bool mVarValChanged;
        [IsSerializedForLocalRepository]
        public bool VarValChanged { get { return mVarValChanged; } set { mVarValChanged = value; OnPropertyChanged(Fields.VarValChanged); } }

        //used to identify the variables which the user customized for specific BF run
        private bool mDiffrentFromOrigin;
        [IsSerializedForLocalRepository]
        public bool DiffrentFromOrigin
        {
            get
            {
                if (mDiffrentFromOrigin == true)
                {
                    if (MappedOutputType == eOutputType.None)
                        VarValChanged = true;
                }
                return mDiffrentFromOrigin;
            }
            set { mDiffrentFromOrigin = value; OnPropertyChanged(Fields.DiffrentFromOrigin); } }

        public string NameBeforeEdit;

        public static void UpdateVariableNameChangeInItem(object item, string prevVarName, string newVarName, ref bool namechange)
        {
            var properties = item.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
            foreach (MemberInfo mi in properties)
            {
                if (Amdocs.Ginger.Common.GeneralLib.General.IsFieldToAvoidInVeFieldSearch(mi.Name))
                {
                    continue;
                }

                //Get the attr value
                PropertyInfo PI = item.GetType().GetProperty(mi.Name);
                dynamic value = null;
                try
                {
                    if (mi.MemberType == MemberTypes.Property)
                    {
                        if (PI.CanWrite)
                        {
                            value = PI.GetValue(item);
                        }
                    }
                    else if (mi.MemberType == MemberTypes.Field)
                    {
                        value = item.GetType().GetField(mi.Name).GetValue(item);
                    }
                }
                catch (Exception ex)
                {
                    AppReporter.ToLog(eAppReporterLogLevel.ERROR, "Exception during UpdateVariableNameChangeInItem", ex, true);
                }

                if (value is IObservableList)
                {
                    List<dynamic> list = new List<dynamic>();
                    foreach (object o in value)
                        UpdateVariableNameChangeInItem(o, prevVarName, newVarName, ref namechange);
                }
                else
                {
                    if (value != null)
                    {
                        if (mi.Name == "VariableName")
                        {
                            if (value == prevVarName)
                                PI.SetValue(item, newVarName);
                            namechange = true;
                        }
                        else if (mi.Name == "StoreToValue")
                        {
                            if (value == prevVarName)
                                PI.SetValue(item, newVarName);
                            else if (value.IndexOf("{Var Name=" + prevVarName + "}") > 0)
                                PI.SetValue(item, value.Replace("{Var Name=" + prevVarName + "}", "{Var Name=" + newVarName + "}"));                            
                            namechange = true;
                        }
                        else
                        {
                            try
                            {
                                if (PI.CanWrite)
                                {
                                    string stringValue = value.ToString();
                                    string variablePlaceHoler = "{Var Name=xx}";
                                    if (stringValue.Contains(variablePlaceHoler.Replace("xx", prevVarName)))
                                    {
                                        PI.SetValue(item, stringValue.Replace(variablePlaceHoler.Replace("xx", prevVarName), variablePlaceHoler.Replace("xx", newVarName)));
                                        namechange = true;
                                    }
                                }
                            }
                            catch (Exception ex) { Console.WriteLine(ex.StackTrace); }
                        }
                    }
                }
            }
        }

        public static void GetListOfUsedVariables(object item, ref List<string> usedVariables)
        {
            var properties = item.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
            foreach (MemberInfo mi in properties)
            {
                if (Amdocs.Ginger.Common.GeneralLib.General.IsFieldToAvoidInVeFieldSearch(mi.Name))
                {
                    continue;
                }

                //Get the attr value
                PropertyInfo PI = item.GetType().GetProperty(mi.Name);
                dynamic value = null;
                try
                {
                    if (mi.MemberType == MemberTypes.Property)
                        value = PI.GetValue(item);
                    else if (mi.MemberType == MemberTypes.Field)
                    {                        
                            value = item.GetType().GetField(mi.Name).GetValue(item);
                    }
                }
                catch (Exception ex)
                {
                    AppReporter.ToLog(eAppReporterLogLevel.ERROR, "Exception during GetListOfUsedVariables", ex, true);
                    value = null;
                } 
                
                if (value is IObservableList)
                {
                    foreach (object o in value)
                        GetListOfUsedVariables(o, ref usedVariables);
                }
                else
                {
                    if (value != null)
                    {
                        try
                        {
                            if (PI.CanWrite)
                            {
                                //TODO: Use nameof !!!!!
                                if (mi.Name == "StoreToValue" && mi.DeclaringType.Name=="ActReturnValue" && value.ToString().IndexOf("{DS Name") == -1)
                                {
                                    //check that it is not GUID of global model Param
                                    Guid dummyGuid = new Guid();
                                    if (!Guid.TryParse(value.ToString(), out dummyGuid))
                                    {
                                        if (value.ToString() != string.Empty)
                                            if (usedVariables.Contains(value.ToString()) == false)
                                                usedVariables.Add(value.ToString());
                                    }
                                }
                                else if(mi.Name == "ValueCalculated" && mi.DeclaringType.Name == "FlowControl") // get used variable in flow control with set variable action type.
                                {
                                    string[] vals = value.Split(new[] { '=' });
                                    const int count = 2;
                                    if (vals.Count() == count && !usedVariables.Contains(vals[0]))
                                    {                                       
                                        usedVariables.Add(vals[0]);                                     
                                    }
                                }
                                else if (mi.Name == "VariableName" && mi.DeclaringType.Name == "VariableDependency" && usedVariables!=null)
                                {
                                    if(!usedVariables.Contains(value))
                                    {
                                        usedVariables.Add(value);
                                    }
                                }
                                else
                                {
                                    string stringValue = value.ToString();
                                    string[] splitedValue = stringValue.Split(new char[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (splitedValue.Length > 0)
                                    {
                                        for (int indx = 0; indx < splitedValue.Length; indx++)
                                        {
                                            string val = splitedValue[indx];
                                            if (val.Contains("Var Name=") == true)
                                            {
                                                val = val.Replace("Var Name=", "");
                                                if (usedVariables.Contains(val) == false)
                                                    usedVariables.Add(val);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // TODO: FIXME!!! no empty exception
                        } 
                    }
                }
            }
        }

        [IsSerializedForLocalRepository]
        public string LinkedVariableName { get; set; }

        public override string ItemName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<Guid> Tags = new ObservableList<Guid>();

        public override bool FilterBy(eFilterBy filterType, object obj)
        {
            switch (filterType)
            {
                case eFilterBy.Tags:
                    foreach (Guid tagGuid in Tags)
                    {
                        Guid guid = ((List<Guid>)obj).Where(x => tagGuid.Equals(x) == true).FirstOrDefault();
                        if (!guid.Equals(Guid.Empty))
                            return true;
                    }
                    break;
            }
            return false;
        }

        public override void UpdateInstance(RepositoryItemBase instance, string partToUpdate, RepositoryItemBase hostItem = null)
        {
            VariableBase variableBaseInstance = (VariableBase)instance;

            //Create new instance of source
            VariableBase newInstance = (VariableBase)this.CreateInstance();
            newInstance.IsSharedRepositoryInstance = true;

            //update required part
            VariableBase.eItemParts ePartToUpdate = (VariableBase.eItemParts)Enum.Parse(typeof(VariableBase.eItemParts), partToUpdate);
            switch (ePartToUpdate)
            {
                case eItemParts.All:
                //case eItemParts.Details:
                    newInstance.Guid = variableBaseInstance.Guid;
                    newInstance.ParentGuid = variableBaseInstance.ParentGuid;
                    newInstance.ExternalID = variableBaseInstance.ExternalID;
                    //if (ePartToUpdate == eItemParts.Details)
                    //{
                    //}
                    if (hostItem != null)
                    {
                        //replace old instance object with new
                        int originalIndex = 0;

                        //TODO: Fix the issues
                        if (hostItem is IActivity)
                        {
                            originalIndex = ((IActivity)hostItem).GetVariables().IndexOf(variableBaseInstance);
                            ((IActivity)hostItem).GetVariables().Remove(variableBaseInstance);
                            ((IActivity)hostItem).GetVariables().Insert(originalIndex, newInstance);
                        }
                        else
                        {
                            originalIndex = ((IBusinessFlow)hostItem).GetVariables().IndexOf(variableBaseInstance);
                            ((IBusinessFlow)hostItem).GetVariables().Remove(variableBaseInstance);
                            ((IBusinessFlow)hostItem).GetVariables().Insert(originalIndex, newInstance);
                        }
                    }
                    break;
            }           
        }

        public override RepositoryItemBase GetUpdatedRepoItem(RepositoryItemBase itemToUpload, RepositoryItemBase existingRepoItem, string itemPartToUpdate)
        {

            VariableBase updatedVariable = null;       

            //update required part
            eItemParts ePartToUpdate = (eItemParts)Enum.Parse(typeof(VariableBase.eItemParts), itemPartToUpdate);
            switch (ePartToUpdate)
            {
                case eItemParts.All:               
                    updatedVariable = (VariableBase)itemToUpload.CreateCopy(false);
                
                    break;
            }

            return updatedVariable;
        }


        ObservableList<string> mPossibleOutputVariables = new ObservableList<string>();
        public ObservableList<string> PossibleOutputVariables
        {
            get
            {
                return mPossibleOutputVariables;
            }
            set
            {
                mPossibleOutputVariables = value;
                OnPropertyChanged(Fields.PossibleOutputVariables);
            }
        }

        string mMappedOutputVariable = string.Empty;
        public string MappedOutputVariable
        {
            get
            {
                return mMappedOutputVariable;
            }
            set
            {                
                if (String.IsNullOrEmpty(value) == false)
                {
                    DiffrentFromOrigin = true;
                    mMappedOutputType = eOutputType.Variable;
                    mMappedOutputValue = value;
                    OnPropertyChanged(Fields.MappedOutputType);
                    OnPropertyChanged(Fields.MappedOutputValue);
                }                    
            }
        }
        
        private eOutputType mMappedOutputType;
        [IsSerializedForLocalRepository]
        public eOutputType MappedOutputType
        {
            get { return mMappedOutputType; }

            set
            {
                mMappedOutputType = value;
                OnPropertyChanged(Fields.MappedOutputType);
            }
        }

        private string mMappedOutputValue;
        [IsSerializedForLocalRepository]
        public string MappedOutputValue
        {
            get
            {
                return mMappedOutputValue;
            }
            set
            {
                mMappedOutputValue = value;
                if (String.IsNullOrEmpty(value) == false || VarValChanged == true)                
                    DiffrentFromOrigin = true;
                else
                    DiffrentFromOrigin = false;
                OnPropertyChanged(Fields.MappedOutputValue);
            }
        }
        public abstract bool SupportSetValue { get; }

        public abstract List<eSetValueOptions> GetSupportedOperations();

        public abstract String VariableUIType { get; }

        /// <summary>
        /// Do not use, exist for backward support
        /// </summary>
        public int CycleCount { get; set; }

        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.Variable;
            }
        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.Name);
            }
        }
    }
}
