﻿using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.Repository;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.Variables
{
    public class InputVariableRule : RepositoryItemBase
    {    

        public enum eInputVariableOperation
        {
            [EnumValueDescription("Set Visibility")]
            SetVisibility,
            [EnumValueDescription("Set Value")]
            SetValue,
            [EnumValueDescription("Set Optional Values")]
            SetOptionalValues
        }

        ObservableList<VariableBase> mSourceVariableList = new ObservableList<VariableBase>();
        public ObservableList<VariableBase> SourceVariableList
        {
            get
            {
                return mSourceVariableList;
            }
            set
            {
                mSourceVariableList = value;
                OnPropertyChanged(nameof(SourceVariableList));
            }
        }
        
        public ObservableList<VariableBase> TargetVariableList
        {
            get
            {
                if(SourceVariableGuid!=null)
                {
                    return new ObservableList<VariableBase>(SourceVariableList.Where(x => x.Guid != SourceVariableGuid));
                }  
                else
                {
                    return null;
                }    
            }           
        }

        ObservableList<string> mVariableValuesList = new ObservableList<string>();
        public ObservableList<string> variableValueList
        {
            get
            {
                return mVariableValuesList;
            }
            set
            {
                mVariableValuesList = value;
                OnPropertyChanged(nameof(variableValueList));
            }
        }
        
        public VariableBase SelectedSourceVariable
        {
            get
            {
                return SourceVariableList.Where(x=> x.Guid == SourceVariableGuid).FirstOrDefault();
            }           
        }

        public VariableBase mSelectedTargetVariable;
        public VariableBase SelectedTargetVariable
        {
            get
            {
                return TargetVariableList.Where(x => x.Guid== TargetVariableGuid).FirstOrDefault();                
            }
            set
            {
                mSelectedTargetVariable = value;
            }
        }


        [IsSerializedForLocalRepository]
        public bool Active { get; set; }

        public Guid mSourceVariableGuid;
        [IsSerializedForLocalRepository]
        public Guid SourceVariableGuid 
        {
            get
            {
                return mSourceVariableGuid;
            }

            set
            {
                mSourceVariableGuid = value;
                OnPropertyChanged(nameof(SelectedSourceVariable));
                OnPropertyChanged(nameof(TargetVariableList));
            }
        } 

        [IsSerializedForLocalRepository]
        public string TriggerValue { get; set; }

        public Guid mTargetVariableGuid;
        [IsSerializedForLocalRepository]
        public Guid TargetVariableGuid 
        {
            get
            {
                return mTargetVariableGuid;
            }

            set
            {
                mTargetVariableGuid = value;                
                OnPropertyChanged(nameof(SelectedTargetVariable));
            } 
        }

        private eInputVariableOperation mOperationType;
        [IsSerializedForLocalRepository]
        public eInputVariableOperation OperationType 
        { 
            get
            {
                return mOperationType;
            }
            set
            {
                mOperationType = value;

                OnPropertyChanged(nameof(OperationType));
            }
        }

        private string mOperationValue;
        [IsSerializedForLocalRepository]
        public string OperationValue 
        { 
            get
            {
                return mOperationValue;
            }
            set
            {
                mOperationValue = value;
                OnPropertyChanged(nameof(OperationValue));
            }
        }

        private ObservableList<OperationValues> mOperationValueList;
        [IsSerializedForLocalRepository]
        public ObservableList<OperationValues> OperationValueList 
        { 
            get
            {
                return mOperationValueList;
            }
            set
            {
                mOperationValueList = value;
                OnPropertyChanged(nameof(OperationValueList));
            }
        }
        
       // public ObservableList<SelectableObject<string>> OperationSelectedValues { get; set; }        


        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                return;
            }
        }
    }
}
