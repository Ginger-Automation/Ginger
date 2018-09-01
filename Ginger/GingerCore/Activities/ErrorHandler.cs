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

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using GingerCore.Properties;

//TODO: chang add core
namespace GingerCore
{
    //Activity can have several steps - Acts
    // The activities can come from external like: QC TC Step, vStorm    
    public class ErrorHandler : Activity
    {
        public new static class Fields
        {
            public static string HandlerType = "HandlerType";
            public static string Description = "Description";
            public static string HandlerMapping = "HandlerMapping";
            public static string IsSelected = "IsSelected";
        }

        public enum eHandlerType
        {             
            Error_Handler = 0,
            Popup_Handler = 1
        }       

        private eHandlerType mHandlerType;
        public bool IsSelected { get; set; }

        [IsSerializedForLocalRepository]
        public eHandlerType HandlerType
        {
            get{return mHandlerType;}
            set { if (mHandlerType != value) { mHandlerType = value; OnPropertyChanged(Fields.HandlerType); } }
        }

        //public override System.Drawing.Image Image { get { return (mHandlerType == eHandlerType.Error_Handler) ? Resources.Handler_16x16 : Resources.PopUpHandler_16x16; } }

        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.Wrench;
            }
        }
    }
}
