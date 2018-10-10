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
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GingerCore.Actions
{
    public class ActWebService : Act
    {
        public override string ActionDescription { get { return "Web Service Action"; } }
        public override string ActionUserDescription { get { return string.Empty; } }

        public override List<ePlatformType> LegacyActionPlatformsList { get { return Platforms; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
        }

        public override string ActionEditPage { get { return "WebServices.ActWebServiceEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {                    
                    mPlatforms.Add(ePlatformType.WebServices);
                }
                return mPlatforms;
            }
        }

        public new static partial class Fields
        {
            public static string URL = "URL";
            public static string SOAPAction = "SOAPAction";
            public static string XMLfileName = "XMLfileName";
            public static string URLUser = "URLUser";
            public static string URLPass = "URLPass";
            public static string URLDomain = "URLDomain";
            public static string DoValidationChkbox = "DoValidationChkbox";
        }

        public override List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing()
        {
            List<ObservableList<ActInputValue>> list = new List<ObservableList<ActInputValue>>();
            list.Add(DynamicXMLElements);
            return list;
        }
                
        public ActInputValue URL { get { return GetOrCreateInputParam(Fields.URL); } }
            
        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> DynamicXMLElements = new ObservableList<ActInputValue>();
             
        public ActInputValue SOAPAction { get { return GetOrCreateInputParam(Fields.SOAPAction); } }
         
        public ActInputValue XMLfileName { get { return GetOrCreateInputParam(Fields.XMLfileName); } }
        
        public ActInputValue URLUser { get { return GetOrCreateInputParam(Fields.URLUser); } }
       
        public ActInputValue URLPass { get { return GetOrCreateInputParam(Fields.URLPass); } }
       
        public ActInputValue URLDomain { get { return GetOrCreateInputParam(Fields.URLDomain); } }

        [IsSerializedForLocalRepository]
        public bool DoValidationChkbox { get; set; }

        public override String ActionType
        {
            get
            {
                return "ActWebService";
            }
        }

        public override System.Drawing.Image Image { get { return Resources.Act; } }
    }
}
