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
            ClearSpecialChar,
            [EnumValueDescription("Delete Specific Optional Value")]
            DynamicValueDeletion,
            [EnumValueDescription("Delete All Optional Values")]
            DeleteAllValues
        }

        public enum eItemParts
        {
            All
            //Details
        }

        public override string ToString()
        {
            return Name;
        }

        public enum eOutputType
        {
            None,
            Variable,
            GlobalVariable,
            OutputVariable,
            ApplicationModelParameter,
            DataSource
        }

        private bool mSetAsInputValue = true;
        [IsSerializedForLocalRepository(true)]
        public bool SetAsInputValue
        {
            get { return mSetAsInputValue; }
            set
            {
                if (mSetAsInputValue != value)
                {
                    mSetAsInputValue = value;
                    OnPropertyChanged(nameof(SetAsInputValue));
                }
            }
        }

        private bool mMandatoryInput = false;
        [IsSerializedForLocalRepository(false)]
        public bool MandatoryInput
        {
            get { return mMandatoryInput; }
            set
            {
                if (mMandatoryInput != value)
                {
                    mMandatoryInput = value;
                    OnPropertyChanged(nameof(MandatoryInput));
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
                    OnPropertyChanged(nameof(SetAsOutputValue));
                }
            }
        }

        private string mName;
        [IsSerializedForLocalRepository]
        public string Name { get { return mName; } set { if (mName != value) { mName = value; OnPropertyChanged(nameof(Name)); } } }

        public string MandatoryIndication
        {
            get
            {
                if (mMandatoryInput)
                {
                    return "*";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private string mDescription;
        [IsSerializedForLocalRepository]
        public string Description { get { return mDescription; } set { if (mDescription != value) { mDescription = value; OnPropertyChanged(nameof(Description)); } } }

        private string mValue;
        public virtual string Value { get { return mValue; } set { if (mValue != value) { mValue = value; OnPropertyChanged(nameof(Value)); } } }


        public override void PostDeserialization()
        {
                ResetValue();
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

                    OnPropertyChanged(formula);
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

                    OnPropertyChanged(nameof(Formula));
                }
            }
        }
        public abstract string GetFormula();
        public abstract string VariableType { get; }
        public abstract void ResetValue();
        public abstract bool GenerateAutoValue(ref string errorMsg);
        public virtual eImageType Image { get { return eImageType.Variable; } }
        public override string GetNameForFileName() { return Name; }
        public abstract string VariableEditPage { get; }
        public virtual bool IsObsolete { get { return false; } }

        public virtual string GetValueWithParam(Dictionary<string,string> extraParamDict) {return Value;}

        public virtual List<string> GetExtraParamsList() { return null; }

        public abstract bool SupportResetValue { get; }
        public abstract bool SupportAutoValue { get; }


        //all below used to describe the variable owner in a specific Business Flow
        [IsSerializedForLocalRepository]
        public string ParentType { get; set; } //BusinessFlow or Activity
        [IsSerializedForLocalRepository]
        public string ParentName { get; set; }
        private bool mVarValChanged;
        [IsSerializedForLocalRepository]
        public bool VarValChanged
        {
            get
            {
                return mVarValChanged;
            }
            set
            {
                if (mVarValChanged != value)
                {
                    mVarValChanged = value;
                    OnPropertyChanged(nameof(VarValChanged));
                }
            }
        }

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
            set { if (mDiffrentFromOrigin != value) { mDiffrentFromOrigin = value; OnPropertyChanged(nameof(DiffrentFromOrigin)); } }
        }

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
                    Reporter.ToLog(eLogLevel.ERROR, "Exception during UpdateVariableNameChangeInItem", ex);
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
                                if (PI != null && PI.CanWrite)
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
            if (item != null)
            {
                // TODO: cache the reflection item needed
                var properties = item.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
                if (properties == null)
                { return; }
                foreach (MemberInfo mi in properties)
                {
                    if (Amdocs.Ginger.Common.GeneralLib.General.IsFieldToAvoidInVeFieldSearch(mi.Name))
                    {
                        continue;
                    }

                    //Get the attr value
                    PropertyInfo PI = item.GetType().GetProperty(mi.Name);
                    object value = null;
                    try
                    {
                        if (mi.MemberType == MemberTypes.Property && PI != null && PI.CanRead)
                        {
                            value = PI.GetValue(item);
                        }
                        else if (mi.MemberType == MemberTypes.Field)
                        {
                            value = item.GetType().GetField(mi.Name).GetValue(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, string.Format("Exception occurred during Action Analyze of Used Variables, object='{0}', field='{1}'", item, mi.Name), ex);
                        value = null;
                    }

                    if (value is IObservableList)
                    {
                        foreach (object o in (IObservableList)value)
                        {
                            GetListOfUsedVariables(o, ref usedVariables);
                        }
                    }
                    else
                    {
                        if (value != null && PI != null)
                        {
                            try
                            {
                                //TODO: Use nameof !!!!!
                                if ((PI.DeclaringType).FullName == "GingerCore.Actions.ActSetVariableValue" && mi.Name == "VariableName")
                                {
                                    usedVariables.Add(value.ToString());
                                }

                                if (PI.CanWrite)
                                {
                                    //TODO: Use nameof !!!!!
                                    if (mi.Name == "StoreToValue" && mi.DeclaringType.Name == "ActReturnValue" && value.ToString().IndexOf("{DS Name") == -1)
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
                                    else if (mi.Name == "FlowControlAction" && value.ToString() == "SetVariableValue") //get used variable in flow control with set variable action type.
                                    {
                                        string[] vals = ((string)item.GetType().GetRuntimeProperty("ValueCalculated").GetValue(item)).Split(new[] { '=' });
                                        const int count = 2;
                                        if (vals.Count() == count && !usedVariables.Contains(vals[0]))
                                        {
                                            usedVariables.Add(vals[0]);
                                        }
                                    }
                                    else if (mi.Name == "VariableName" && mi.DeclaringType.Name == "VariableDependency" && usedVariables != null)
                                    {
                                        if (!usedVariables.Contains(value))
                                        {
                                            usedVariables.Add((string)value);
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
                                Reporter.ToLog(eLogLevel.DEBUG, string.Format("Exception occurred during Action Analyze of Used Variables, object='{0}', field='{1}'", item, mi.Name), ex);
                            }
                        }
                    }
                }
            }
        }

        string mLinkedVariableName;
        [IsSerializedForLocalRepository]
        public string LinkedVariableName { get { return mLinkedVariableName; } set { if (mLinkedVariableName != value) { mLinkedVariableName = value; OnPropertyChanged(nameof(this.LinkedVariableName)); } } }

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

        public override void UpdateInstance(RepositoryItemBase instance, string partToUpdate, RepositoryItemBase hostItem = null, object extraDetails = null)
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
                        if (hostItem is Activity)
                        {
                            originalIndex = ((Activity)hostItem).GetVariables().IndexOf(variableBaseInstance);
                            ((Activity)hostItem).GetVariables().Remove(variableBaseInstance);
                            ((Activity)hostItem).GetVariables().Insert(originalIndex, newInstance);
                        }
                        else
                        {
                            originalIndex = ((BusinessFlow)hostItem).GetVariables().IndexOf(variableBaseInstance);
                            ((BusinessFlow)hostItem).GetVariables().Remove(variableBaseInstance);
                            ((BusinessFlow)hostItem).GetVariables().Insert(originalIndex, newInstance);
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


        ObservableList<string> mPossibleVariables = new ObservableList<string>();
        public ObservableList<string> PossibleVariables
        {
            get
            {
                return mPossibleVariables;
            }
            set
            {
                mPossibleVariables = value;
                OnPropertyChanged(nameof(PossibleVariables));
            }
        }

        ObservableList<VariableBase> mPossibleOutputVariables = new ObservableList<VariableBase>();
        public ObservableList<VariableBase> PossibleOutputVariables
        {
            get
            {
                return mPossibleOutputVariables;
            }
            set
            {
                mPossibleOutputVariables = value;
                OnPropertyChanged(nameof(PossibleOutputVariables));
            }
        }

        /// <summary>
        /// This field is used to design the variable path 
        /// e.g. when showing possible output values on the Runset business flow configuration page
        /// for output variable possible values we will show <<VariableName>>[<<BusinessFlowName(<<BusinessflowRunInstanceNumber>>)>>]
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// This field is used to store the variable instance info in the runset.
        /// And this is used for output variable mapping on the runset configuration.
        /// e.g. if the variables parent business flow is added in Runset 
        /// then this field have the value <<BusinessflowinstanceGUID>>_<<VaruabileGUID>>
        /// </summary>
        public string VariableInstanceInfo { get; set; }


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
                    OnPropertyChanged(nameof(MappedOutputType));
                    OnPropertyChanged(nameof(MappedOutputValue));
                }
            }
        }

        private eOutputType mMappedOutputType;
        [IsSerializedForLocalRepository]
        public eOutputType MappedOutputType { get { return mMappedOutputType; }  set { if (mMappedOutputType != value) { mMappedOutputType = value; OnPropertyChanged(nameof(MappedOutputType)); } } }

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
                if (mMappedOutputValue != value)
                {
                    mMappedOutputValue = value;
                    OnPropertyChanged(nameof(MappedOutputValue));
                }
                if (String.IsNullOrEmpty(value) == false || VarValChanged == true)
                    DiffrentFromOrigin = true;
                else
                    DiffrentFromOrigin = false;
            }
        }
        public abstract bool SupportSetValue { get; }

        public virtual bool SetValue(string value)
        {
            if (this.SupportSetValue)
            {
                this.Value = value;
                return true;
            }
            else
            {
                return false;
            }
        }

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

        public virtual void SetInitialSetup()
        {
            Name = string.Format("New {0}", VariableUIType);
            SetAsInputValue = false;
            SetAsOutputValue = false;
        }

        public override void PrepareItemToBeCopied()
        {
            this.IsSharedRepositoryInstance = TargetFrameworkHelper.Helper.IsSharedRepositoryItem(this);
        }

        public override string GetItemType()
        {
            return "Variable";
        }

        public static ObservableList<VariableBase> SortByMandatoryInput(ObservableList<VariableBase> variables)
        {
            int movedNum = 0;               
            for (int indx=0; indx<variables.Count; indx++)
            {
                if (variables[indx].MandatoryInput)
                {
                    variables.Move(indx, 0);
                    movedNum++;
                }
            }
            for (int indx = 0; indx < movedNum; indx++)//keep original order
            {
                variables.Move(indx, 0);
            }
            return variables;
        }
    }
}
