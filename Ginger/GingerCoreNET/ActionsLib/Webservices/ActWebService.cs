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
using Amdocs.Ginger.Repository;

using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET;
using GingerCore.Actions.WebServices;

using Amdocs.Ginger.Common.Enums;

namespace GingerCore.Actions
{
    public class ActWebService : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "Web Service Action"; } }
        public override string ActionUserDescription { get { return "Web Service Action"; } }

        public override List<ePlatformType> LegacyActionPlatformsList { get { return Platforms; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
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

        bool IObsoleteAction.IsObsoleteForPlatform(ePlatformType actionPlatform)
        {
            if (actionPlatform == ePlatformType.WebServices || actionPlatform == ePlatformType.NA)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        Act IObsoleteAction.GetNewAction()
        {
            AutoMapper.MapperConfiguration mapperConfiguration = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<Act, ActWebAPISoap>(); });
            ActWebAPISoap convertedActWebAPISoap = mapperConfiguration.CreateMapper().Map<Act, ActWebAPISoap>(this);

            convertedActWebAPISoap.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.EndPointURL, this.URL.Value);

            convertedActWebAPISoap.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.AuthorizationType, ApplicationAPIUtils.eAuthType.NoAuthentication.ToString());
            convertedActWebAPISoap.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.SecurityType, ApplicationAPIUtils.eSercurityType.None.ToString());
            convertedActWebAPISoap.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());

            if (!string.IsNullOrEmpty(this.URLDomain.Value) || (!string.IsNullOrEmpty(this.URLUser.Value) && !string.IsNullOrEmpty(this.URLPass.Value)))
            {
                convertedActWebAPISoap.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.NetworkCredentialsRadioButton, ApplicationAPIUtils.eNetworkCredentials.Custom.ToString());
                convertedActWebAPISoap.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.URLDomain, this.URLDomain.Value);
                convertedActWebAPISoap.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.URLUser, this.URLUser.Value);
                convertedActWebAPISoap.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.URLPass, this.URLPass.Value);
            }
            else
            {
                convertedActWebAPISoap.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.NetworkCredentialsRadioButton, ApplicationAPIUtils.eNetworkCredentials.Default.ToString());
            }


            if (!string.IsNullOrEmpty(this.XMLfileName.ToString()))
            {
                convertedActWebAPISoap.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton, ApplicationAPIUtils.eRequestBodyType.TemplateFile.ToString());
                convertedActWebAPISoap.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.TemplateFileNameFileBrowser, this.XMLfileName.Value);
            }

            if (convertedActWebAPISoap.ReturnValues != null && convertedActWebAPISoap.ReturnValues.Count != 0)
            {
                //Old web service action add response as --> FullReponseXML
                //And new adds it as Response:
                // so we update it when converting from old action to new
                ActReturnValue ARC = convertedActWebAPISoap.ReturnValues.Where(x => x.Param == "FullReponseXML").FirstOrDefault();
                if (ARC != null)
                {
                    ARC.Param = "Response:";
                    if (!string.IsNullOrEmpty(ARC.Expected))
                    {
                        ARC.Expected = XMLDocExtended.PrettyXml(ARC.Expected);
                    }
                }
            }
            convertedActWebAPISoap.DynamicElements = this.DynamicXMLElements;

            return convertedActWebAPISoap;
        }

        Type IObsoleteAction.TargetAction()
        {
            return typeof(ActWebAPISoap);
        }

        string IObsoleteAction.TargetActionTypeName()
        {
            ActWebAPISoap newActApiSoap = new ActWebAPISoap();
            return newActApiSoap.ActionDescription;
        }

        ePlatformType IObsoleteAction.GetTargetPlatform()
        {
            return ePlatformType.WebServices;
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

        public override eImageType Image { get { return eImageType.Exchange; } }
    }
}
