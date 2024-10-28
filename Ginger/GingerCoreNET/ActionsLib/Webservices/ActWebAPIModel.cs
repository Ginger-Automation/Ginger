#region License
/*
Copyright © 2014-2024 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using GingerCore.Platforms;
using GingerCoreNET.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GingerCore.Actions.WebServices.WebAPI
{
    public class ActWebAPIModel : Act, IActPluginExecution, IActPluginPostRun
    {
        public ActWebAPIModel()
        {
            //Disable Auto Screenshot on failure by default. User can override it if needed
            AutoScreenShotOnFailure = false;
        }

        public override String ActionType
        {
            get
            {
                return ActionDescription;
            }
        }

        public override string ActionDescription { get { return "Web API Model Action"; } }

        public override eImageType Image { get { return eImageType.Exchange; } }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }

        public override bool ValueConfigsNeeded { get { return false; } }

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

        public override string ActionEditPage { get { return "WebServices.ActWebAPIModelEditPage"; } }

        public override string AddActionWizardPage { get { return "Ginger.ApiModelsFolder.AddApiModelActionWizardPage"; } }

        public override string ActionUserDescription { get { return "Uses Application API Model template to performs SOAP/REST action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you want to use Application API Model template to perform a SOAP/REST Action.");
            TBH.AddLineBreak();
            TBH.AddText("Add your Application API Model you want to use and populate the placeholder grid according to the action you want to run");
            TBH.AddLineBreak();
            TBH.AddText("In order to create API Model please navigate to 'Resources' tab then select 'Application Models' Sub Option then select 'API Models' Sub Option then right click on the 'Application API Models' folder and select one of the option to add API's manually or from a document.");
        }

        public Guid APImodelGUID
        {
            get
            {
                if (string.IsNullOrEmpty(GetOrCreateInputParam(nameof(APImodelGUID)).Value))
                {
                    return new Guid();
                }
                else
                {
                    return Guid.Parse(GetOrCreateInputParam(nameof(APImodelGUID)).Value);
                }
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(APImodelGUID), value.ToString());

            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<EnhancedActInputValue> APIModelParamsValue = [];

        public override List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing()
        {
            List<ObservableList<ActInputValue>> list = [];
            List<ActInputValue> AIVList = APIModelParamsValue.Cast<ActInputValue>().ToList();
            list.Add(General.ConvertListToObservableList(AIVList));
            return list;
        }

        public ActWebAPIBase WebApiAction;

        public ObservableList<AppModelParameter> ActAppModelParameters;

        public override void CalculateModelParameterExpectedValue(ActReturnValue actReturnValue)
        {
            if (actReturnValue.ExpectedCalculated.Contains("AppModelParam"))
            {
                List<AppModelParameter> usedParams = ActAppModelParameters.Where(x => actReturnValue.ExpectedCalculated.Contains(x.PlaceHolder)).ToList();
                foreach (AppModelParameter param in usedParams)
                {
                    actReturnValue.ExpectedCalculated = actReturnValue.ExpectedCalculated.Replace(("{AppModelParam Name = " + param.PlaceHolder + "}"), param.ExecutionValue);
                }
            }
        }
        public string GetName()
        {
            return "ActWebAPIModel";
        }


        public PlatformAction GetAsPlatformAction()
        {
            ApplicationAPIModel AAMB = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationAPIModel>(APImodelGUID);
            if (AAMB == null)
            {
                Error = "Failed to find the pointed API Model";
                ExInfo = string.Format("API Model with the GUID '{0}' was not found", APImodelGUID);
                throw new InvalidOperationException("Application Modal Not Found");
            }
            ActWebAPIBase actWebAPI = null;
            if (AAMB.APIType == ApplicationAPIUtils.eWebApiType.REST)
            {
                actWebAPI = CreateActWebAPIREST(AAMB, this);

            }
            else if (AAMB.APIType == ApplicationAPIUtils.eWebApiType.SOAP)
            {
                actWebAPI = CreateActWebAPISOAP(AAMB, this);
            }
            WebApiAction = actWebAPI;
            return ((IActPluginExecution)actWebAPI).GetAsPlatformAction();
        }



        public ActWebAPIRest CreateActWebAPIREST(ApplicationAPIModel AAMB, ActWebAPIModel ActWebAPIModel)
        {
            ActWebAPIRest actWebAPIBase = new ActWebAPIRest();
            FillAPIBaseFields(AAMB, actWebAPIBase, ActWebAPIModel);
            return actWebAPIBase;
        }

        public ActWebAPISoap CreateActWebAPISOAP(ApplicationAPIModel AAMB, ActWebAPIModel ActWebAPIModel)
        {
            ActWebAPISoap actWebAPISoap = new ActWebAPISoap();
            FillAPIBaseFields(AAMB, actWebAPISoap, ActWebAPIModel);
            return actWebAPISoap;
        }
        private void FillAPIBaseFields(ApplicationAPIModel AAMB, ActWebAPIBase actWebAPIBase, ActWebAPIModel actWebAPIModel)
        {
            ApplicationAPIModel AAMBDuplicate = SetAPIModelData(AAMB, actWebAPIModel);

            //Initializing Act Properties
            actWebAPIBase.AddNewReturnParams = actWebAPIModel.AddNewReturnParams;
            actWebAPIBase.SolutionFolder = actWebAPIModel.SolutionFolder;
            actWebAPIBase.SupportSimulation = actWebAPIModel.SupportSimulation;
            actWebAPIBase.AddOrUpdateInputParamValue(nameof(ActWebAPIBase.UseLegacyJSONParsing), "False");
            actWebAPIBase.Description = actWebAPIModel.Description;
            actWebAPIBase.Timeout = actWebAPIModel.Timeout;
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.RequestType, AAMBDuplicate.RequestType.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.ReqHttpVersion, AAMBDuplicate.ReqHttpVersion.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.ResponseContentType, AAMBDuplicate.ResponseContentType.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.CookieMode, AAMBDuplicate.CookieMode.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.ContentType, AAMBDuplicate.ContentType.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPISoap.Fields.SOAPAction, AAMBDuplicate.SOAPAction);
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.EndPointURL, AAMBDuplicate.EndpointURL);
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.NetworkCredentialsRadioButton, AAMBDuplicate.NetworkCredentials.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.URLUser, AAMBDuplicate.URLUser);
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.URLDomain, AAMBDuplicate.URLDomain);
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.URLPass, AAMBDuplicate.URLPass);
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.DoNotFailActionOnBadRespose, AAMBDuplicate.DoNotFailActionOnBadRespose.ToString());
            actWebAPIBase.HttpHeaders = ConvertAPIModelKeyValueToActInputValues(AAMBDuplicate.HttpHeaders, actWebAPIModel);
            actWebAPIBase.RequestKeyValues = ConvertAPIModelBodyKeyValueToWebAPIKeyBodyValue(AAMBDuplicate.APIModelBodyKeyValueHeaders, actWebAPIModel);
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton, AAMBDuplicate.RequestBodyType.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.RequestBody, AAMBDuplicate.RequestBody);
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, AAMBDuplicate.CertificateType.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.CertificatePath, AAMBDuplicate.CertificatePath);
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.ImportCetificateFile, AAMBDuplicate.ImportCetificateFile.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.CertificatePassword, AAMBDuplicate.CertificatePassword);
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.SecurityType, AAMBDuplicate.SecurityType.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.AuthorizationType, AAMBDuplicate.AuthorizationType.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.TemplateFileNameFileBrowser, AAMBDuplicate.TemplateFileNameFileBrowser.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.AuthUsername, AAMBDuplicate.AuthUsername.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.AuthPassword, AAMBDuplicate.AuthPassword.ToString());

            actWebAPIBase.ReturnValues = actWebAPIModel.ReturnValues;
            actWebAPIBase.Context = actWebAPIModel.Context;

        }

        private ObservableList<WebAPIKeyBodyValues> ConvertAPIModelBodyKeyValueToWebAPIKeyBodyValue(ObservableList<APIModelBodyKeyValue> aPIModelBodyKeyValueHeaders, ActWebAPIModel actWebAPIModel)
        {
            ObservableList<WebAPIKeyBodyValues> webAPIKeyBodyValues = new ObservableList<WebAPIKeyBodyValues>();

            if (aPIModelBodyKeyValueHeaders != null)
            {
                foreach (APIModelBodyKeyValue AMKV in aPIModelBodyKeyValueHeaders)
                {
                    WebAPIKeyBodyValues AIV = new WebAPIKeyBodyValues
                    {
                        Param = AMKV.Param,
                        Value = AMKV.Value,
                        ValueType = ConvertToWebAPIKeyBodyValueType(AMKV.ValueType),
                        ValueForDriver = ReplacePlaceHolderParameterWithActual(AMKV.Value, actWebAPIModel.APIModelParamsValue)
                    };
                    webAPIKeyBodyValues.Add(AIV);
                }
            }

            return webAPIKeyBodyValues;
        }

        private static WebAPIKeyBodyValues.eValueType ConvertToWebAPIKeyBodyValueType(APIModelBodyKeyValue.eValueType valueType)
        {
            if (valueType == APIModelBodyKeyValue.eValueType.File)
                return WebAPIKeyBodyValues.eValueType.File;

            return WebAPIKeyBodyValues.eValueType.Text;
        }

        private ObservableList<ActInputValue> ConvertAPIModelKeyValueToActInputValues(ObservableList<APIModelKeyValue> GingerCoreNETHttpHeaders, ActWebAPIModel actWebAPIModel)
        {
            ObservableList<ActInputValue> GingerCoreHttpHeaders = [];

            if (GingerCoreNETHttpHeaders != null)
            {
                foreach (APIModelKeyValue AMKV in GingerCoreNETHttpHeaders)
                {
                    ActInputValue AIV = new ActInputValue
                    {
                        Param = AMKV.Param,
                        Value = AMKV.Value,
                        ValueForDriver = ReplacePlaceHolderParameterWithActual(AMKV.Value, actWebAPIModel.APIModelParamsValue)
                    };
                    GingerCoreHttpHeaders.Add(AIV);
                }
            }

            return GingerCoreHttpHeaders;
        }

        private ApplicationAPIModel SetAPIModelData(ApplicationAPIModel AAMB, ActWebAPIModel actWebAPIModel)
        {
            WorkSpace.Instance.RefreshGlobalAppModelParams(AAMB);
            //Duplicate Model for not changing on cache
            ApplicationAPIModel AAMBDuplicate = (ApplicationAPIModel)AAMB.CreateCopy(true);

            //Set model params with actual execution value
            foreach (AppModelParameter modelParam in AAMBDuplicate.AppModelParameters)
            {
                SetExecutionValue(modelParam, actWebAPIModel);
            }

            foreach (GlobalAppModelParameter globalParam in AAMBDuplicate.GlobalAppModelParameters)
            {
                SetExecutionValue(globalParam, actWebAPIModel);
            }

            actWebAPIModel.ActAppModelParameters = AAMBDuplicate.MergedParamsList;

            //Replace Placeholders with Execution Values
            AAMBDuplicate.SetModelConfigsWithExecutionData();
            return AAMBDuplicate;
        }

        private string ReplacePlaceHolderParameterWithActual(string ValueBeforeReplacing, ObservableList<EnhancedActInputValue> APIModelDynamicParamsValue)
        {
            if (string.IsNullOrEmpty(ValueBeforeReplacing))
            {
                return string.Empty;
            }

            foreach (EnhancedActInputValue EAIV in APIModelDynamicParamsValue)
            {
                ValueBeforeReplacing = ValueBeforeReplacing.Replace(EAIV.Param, EAIV.ValueForDriver);
            }

            return ValueBeforeReplacing;
        }

        private void SetExecutionValue<T>(T param, ActWebAPIModel actWebAPIModel)
        {
            AppModelParameter p = param as AppModelParameter;
            EnhancedActInputValue enhanceInput = actWebAPIModel.APIModelParamsValue.FirstOrDefault(x => x.ParamGuid == p.Guid);
            if (enhanceInput != null)
            {
                p.ExecutionValue = enhanceInput.ValueForDriver;
            }
            else
            {
                p.ExecutionValue = p.GetDefaultValue();
            }

            if (p is GlobalAppModelParameter && p.ExecutionValue.Equals(GlobalAppModelParameter.CURRENT_VALUE))
            {
                p.ExecutionValue = ((GlobalAppModelParameter)p).CurrentValue;
            }
        }

        public void ParseOutput()
        {
            WebApiAction.ParseOutput();
        }
    }
}
