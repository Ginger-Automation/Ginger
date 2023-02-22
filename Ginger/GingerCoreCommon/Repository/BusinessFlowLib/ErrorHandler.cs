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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;

//TODO: change add core
namespace GingerCore
{
    public enum eHandlerType
    {
        Error_Handler = 0,
        Popup_Handler = 1
    }

    public enum eErrorHandlerPostExecutionAction
    {
        [EnumValueDescription("Rerun Origin Action")]
        ReRunOriginAction,

        [EnumValueDescription("Rerun Origin Activity")]
        ReRunOriginActivity,

        [EnumValueDescription("Rerun Business Flow")]
        ReRunBusinessFlow,

        [EnumValueDescription("Continue From Next Action")]
        ContinueFromNextAction,

        [EnumValueDescription("Continue From Next Activity")]
        ContinueFromNextActivity,

        [EnumValueDescription("Continue From Next Business Flow")]
        ContinueFromNextBusinessFlow,

        [EnumValueDescription("Stop Run")]
        StopRun
    }

    public enum eTriggerType
    {
        [EnumValueDescription("Any Error")]
        AnyError,
        [EnumValueDescription("Specific Error")]
        SpecificError
    }


    //Activity can have several steps - Acts
    // The activities can come from external like: QC TC Step, vStorm    
    public class ErrorHandler : Activity, IErrorHandler
    {
        private eHandlerType mHandlerType;
        public bool IsSelected { get; set; }

        [IsSerializedForLocalRepository]
        public eHandlerType HandlerType
        {
            get{return mHandlerType;}
            set { if (mHandlerType != value) { mHandlerType = value; OnPropertyChanged(nameof(HandlerType)); } }
        }


        private eErrorHandlerPostExecutionAction mErrorHandlerPostExecutionAction;
        [IsSerializedForLocalRepository]
        public eErrorHandlerPostExecutionAction ErrorHandlerPostExecutionAction
        {
            get { return mErrorHandlerPostExecutionAction; }
            set { if (mErrorHandlerPostExecutionAction != value) { mErrorHandlerPostExecutionAction = value; OnPropertyChanged(nameof(ErrorHandlerPostExecutionAction)); } }
        }


        private eTriggerType mTriggerErrorType;
        [IsSerializedForLocalRepository]
        public eTriggerType TriggerType
        {
            get { return mTriggerErrorType; }
            set { if (mTriggerErrorType != value) { mTriggerErrorType = value; OnPropertyChanged(nameof(ErrorHandlerPostExecutionAction)); } }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<ErrorDetails> ErrorStringList = new ObservableList<ErrorDetails>();

        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.Wrench;
            }
        }

        public override string ActivityType
        {
            get
            {
                switch (HandlerType)
                {
                    case eHandlerType.Popup_Handler:
                        return GingerDicser.GetTermResValue(eTermResKey.Activity, "Pop Up Handler");               
                    default:
                        return GingerDicser.GetTermResValue(eTermResKey.Activity, "Error Handler");
                }
            }
        }
    }
}
