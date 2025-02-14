#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common.External.Configurations;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.WebServices;
using GingerCore.Actions.WebServices.WebAPI;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.ActionsLib.Webservices
{
    public class ActWebAPIModelOperation : IActWebAPIModelOperation
    {
        public WireMockConfiguration mockConfiguration;
        public void FillAPIBaseFields(ApplicationAPIModel AAMB, ActWebAPIBase actWebAPIBase, ActWebAPIModel actWebAPIModel)
        {
            ApplicationAPIModel AAMBDuplicate = SetAPIModelData(AAMB, actWebAPIModel);
            mockConfiguration = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<WireMockConfiguration>().Count == 0 ? new WireMockConfiguration() : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<WireMockConfiguration>();

            //Initializing Act Properties
            actWebAPIBase.AddNewReturnParams = actWebAPIModel.AddNewReturnParams;
            actWebAPIBase.SolutionFolder = actWebAPIModel.SolutionFolder;
            actWebAPIBase.SupportSimulation = actWebAPIModel.SupportSimulation;
            actWebAPIBase.AddOrUpdateInputParamValue(nameof(ActWebAPIBase.UseLegacyJSONParsing), "False");
            actWebAPIBase.Description = actWebAPIModel.Description;
            actWebAPIBase.Timeout = actWebAPIModel.Timeout;
            actWebAPIBase.UseLiveAPI = AAMBDuplicate.UseLiveAPI;
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.RequestType, AAMBDuplicate.RequestType.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.ReqHttpVersion, AAMBDuplicate.ReqHttpVersion.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.ResponseContentType, AAMBDuplicate.ResponseContentType.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.CookieMode, AAMBDuplicate.CookieMode.ToString());
            actWebAPIBase.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.ContentType, AAMBDuplicate.RequestContentType.ToString());
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
        private ObservableList<WebAPIKeyBodyValues> ConvertAPIModelBodyKeyValueToWebAPIKeyBodyValue(ObservableList<APIModelBodyKeyValue> aPIModelBodyKeyValueHeaders, ActWebAPIModel actWebAPIModel)
        {
            ObservableList<WebAPIKeyBodyValues> webAPIKeyBodyValues = [];

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
    }
}
