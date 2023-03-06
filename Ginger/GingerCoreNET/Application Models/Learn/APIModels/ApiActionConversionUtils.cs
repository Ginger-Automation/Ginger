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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.APIModelLib;
using Amdocs.Ginger.Repository;
using GingerAutoPilot.APIModelLib;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.WebServices;
using GingerCore.Actions.WebServices.WebAPI;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Amdocs.Ginger.CoreNET.ActionsLib.ActionsConversion
{
    /// <summary>
    /// This class is used to convert the actions to Api Actions
    /// </summary>
    public class ApiActionConversionUtils
    {
        bool mStopConversion = false;

        /// <summary>
        /// This method is used to convert the legacy service actions to api actions from the businessflows provided
        /// </summary>
        /// <param name="businessFlows"></param>
        /// <param name="parameterizeRequestBody"></param>
        /// <param name="configuredValidationRequired"></param>
        public void ConvertToApiActionsFromBusinessFlows(ObservableList<BusinessFlowToConvert> businessFlows, bool parameterizeRequestBody, bool pullValidations, RepositoryFolder<ApplicationAPIModel> apiModelFolder)
        {
            ActWebAPIModel webAPIModel = new ActWebAPIModel();
            foreach (var bf in businessFlows)
            {
                try
                {
                    if (!mStopConversion)
                    {
                        bf.ConversionStatus = eConversionStatus.Pending;
                        if (IsValidWebServiceBusinessFlow(bf.BusinessFlow))
                        {
                            for (int activityIndex = 0; activityIndex < bf.BusinessFlow.Activities.Count(); activityIndex++)
                            {
                                Activity activity = bf.BusinessFlow.Activities[activityIndex];
                                RepositoryItemKey ta = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.ItemName == activity.TargetApplication).FirstOrDefault().Key;
                                if (activity != null && activity.Active && WorkSpace.Instance.Solution.GetApplicationPlatformForTargetApp(ta.ItemName) == ePlatformType.WebServices)
                                {
                                  bf.ConvertedActionsCount +=  ConvertActivity(parameterizeRequestBody, pullValidations, activity, ta, apiModelFolder);
                                }
                            }
                        }
                        bf.ConversionStatus = eConversionStatus.Finish;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert action", ex);
                }
            }
        }

        /// <summary>
        /// This method will convert the Activity Actions
        /// </summary>
        /// <param name="parameterizeRequestBody"></param>
        /// <param name="pullValidations"></param>
        /// <param name="activity"></param>
        /// <param name="ta"></param>
        private int ConvertActivity(bool parameterizeRequestBody, bool pullValidations, Activity activity, RepositoryItemKey ta, RepositoryFolder<ApplicationAPIModel> apiModelFolder)
        {
            int convertionConter = 0;
            for (int actionIndex = 0; actionIndex < activity.Acts.Count; actionIndex++)
            {
                try
                {
                    Act act = (Act)activity.Acts[actionIndex];
                    if (act.Active && (act.GetType() == typeof(ActWebAPIRest) || act.GetType() == typeof(ActWebAPISoap)))
                    {
                        // get the index of the action that is being converted 
                        int selectedActIndex = activity.Acts.IndexOf(act);

                        //Create/Update API Model
                        bool isModelExists = true;
                        ApplicationAPIModel applicationModel = GetAPIModelIfExists(act);

                        if (applicationModel == null)
                        {
                            isModelExists = false;
                            applicationModel = new ApplicationAPIModel();
                            applicationModel.TargetApplicationKey = ta;
                            CreateAPIModelFromWebserviceAction(ref applicationModel, act, pullValidations, parameterizeRequestBody);
                        }

                        //Parse optional values
                        Dictionary<System.Tuple<string, string>, List<string>> optionalValuesPulledFromConvertedAction = new Dictionary<Tuple<string, string>, List<string>>();
                        if (applicationModel.AppModelParameters != null && applicationModel.AppModelParameters.Count > 0)
                        {
                            optionalValuesPulledFromConvertedAction = ParseParametersOptionalValues(applicationModel, (ActWebAPIBase)act);
                        }

                        //Create WebAPIModel action
                        ActWebAPIModel actApiModel = GetNewAPIModelAction(applicationModel, act, optionalValuesPulledFromConvertedAction);

                        activity.Acts.Insert(selectedActIndex + 1, actApiModel);
                        actionIndex++;
                        act.Active = false;
                        if (!isModelExists)
                        {
                            apiModelFolder.AddRepositoryItem(applicationModel);
                        }
                        else
                        {
                            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(applicationModel);
                        }

                        convertionConter++;
                    }                    
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert the action", ex);
                }
            }

            return convertionConter;
        }

        /// <summary>
        /// This method will create the new api model action
        /// </summary>
        /// <param name="applicationModel"></param>
        /// <param name="act"></param>
        /// <returns></returns>
        private ActWebAPIModel GetNewAPIModelAction(ApplicationAPIModel applicationModel, Act act, Dictionary<System.Tuple<string, string>, List<string>> optionalValuesPulledFromConvertedAction)
        {
            AutoMapper.MapperConfiguration mapConfigUIElement = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<Act, ActWebAPIModel>(); });
            ActWebAPIModel newActModel = mapConfigUIElement.CreateMapper().Map<Act, ActWebAPIModel>(act);
            newActModel.APImodelGUID = applicationModel.Guid;
            newActModel.Active = true;
            newActModel.Reset();
            newActModel.APIModelParamsValue = GetAPIActionParams(applicationModel.AppModelParameters, optionalValuesPulledFromConvertedAction);
            //newActModel.ReturnValues = GetAPIModelActionReturnValues(applicationModel.ReturnValues); //For now we want to keep original validations on action
            return newActModel;
        }

        private ObservableList<EnhancedActInputValue> GetAPIActionParams(ObservableList<AppModelParameter> paramsList, Dictionary<System.Tuple<string, string>, List<string>> optionalValuesPulledFromConvertedAction)
        {
            ObservableList<EnhancedActInputValue> enhancedParamsList = new ObservableList<EnhancedActInputValue>();
            foreach (AppModelParameter apiModelParam in paramsList)
            {
                if (apiModelParam.RequiredAsInput == true)
                {
                    EnhancedActInputValue actAPIModelParam = new EnhancedActInputValue();
                    actAPIModelParam.ParamGuid = apiModelParam.Guid;
                    actAPIModelParam.Param = apiModelParam.PlaceHolder;
                    actAPIModelParam.Description = apiModelParam.Description;                    
                    foreach (OptionalValue optionalValue in apiModelParam.OptionalValuesList)
                    {
                        actAPIModelParam.OptionalValues.Add(optionalValue.Value);
                    }

                    //set value came from Action 
                    foreach (System.Tuple<string, string> key in optionalValuesPulledFromConvertedAction.Keys)
                    {
                        if (key.Item2 == apiModelParam.Path)
                        {
                            if (optionalValuesPulledFromConvertedAction[key] != null && optionalValuesPulledFromConvertedAction[key].Count > 0)
                            {
                                actAPIModelParam.Value = optionalValuesPulledFromConvertedAction[key][0];
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(actAPIModelParam.Value))
                    {
                        //set defualt value
                        OptionalValue ov = apiModelParam.OptionalValuesList.Where(x => x.IsDefault == true).FirstOrDefault();
                        if (ov != null)
                        {
                            actAPIModelParam.Value = ov.Value;
                        }
                    }

                    enhancedParamsList.Add(actAPIModelParam);
                }
            }
            return enhancedParamsList;
        }

        //private ObservableList<ActReturnValue> GetAPIModelActionReturnValues(ObservableList<ActReturnValue> modelReturnValues)
        //{
        //    ObservableList<ActReturnValue> returnValuesList = new ObservableList<ActReturnValue>();
        //    foreach (ActReturnValue modelRV in modelReturnValues)
        //    {
        //        ActReturnValue rv = new ActReturnValue();
        //        rv.AddedAutomatically = true;
        //        rv.Guid = modelRV.Guid;
        //        rv.Active = modelRV.Active;
        //        rv.Param = modelRV.Param;
        //        rv.Path = modelRV.Path;
        //        rv.Expected = modelRV.Expected;
        //        if (!string.IsNullOrEmpty(modelRV.StoreToValue))
        //        {
        //            rv.StoreTo = ActReturnValue.eStoreTo.ApplicationModelParameter;
        //            rv.StoreToValue = modelRV.StoreToValue;
        //        }
        //        returnValuesList.Add(rv);
        //    }
        //    return returnValuesList;
        //}


        /// <summary>
        /// This method is used to check if the app parameter is already added in the list if not then it will add the parameter
        /// </summary>
        /// <param name="aPIModel"></param>
        /// <param name="act"></param>
        /// <param name="actApiModel"></param>
        private Dictionary<System.Tuple<string, string>, List<string>> ParseParametersOptionalValues(ApplicationAPIModel aPIModel, ActWebAPIBase actionToConvert)
        {
            Dictionary<System.Tuple<string, string>, List<string>> optionalValuesPerParameterDict = new Dictionary<Tuple<string, string>, List<string>>();
            try
            {
                string requestBody = null;
                if (!string.IsNullOrEmpty(actionToConvert.GetInputParamValue(ActWebAPIBase.Fields.TemplateFileNameFileBrowser)))
                {
                    string fileUri = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(actionToConvert.GetInputParamValue(ActWebAPIBase.Fields.TemplateFileNameFileBrowser));
                    if (File.Exists(fileUri))
                    {
                        requestBody = System.IO.File.ReadAllText(fileUri);
                    }
                }
                else if (!string.IsNullOrEmpty(actionToConvert.GetInputParamValue(ActWebAPIBase.Fields.RequestBody)))
                {
                    requestBody = actionToConvert.GetInputParamValue(ActWebAPIBase.Fields.RequestBody);
                }

                if (requestBody != null)
                {                   
                    ImportParametersOptionalValues ImportOptionalValues = new ImportParametersOptionalValues();
                    if (!string.IsNullOrEmpty(requestBody) && requestBody.StartsWith("{"))
                    {
                        ImportOptionalValues.GetJSONAllOptionalValuesFromExamplesFile(requestBody, optionalValuesPerParameterDict);
                        ImportOptionalValues.PopulateJSONOptionalValuesForAPIParameters(aPIModel, optionalValuesPerParameterDict);
                    }
                    else if (!string.IsNullOrEmpty(requestBody) && requestBody.StartsWith("<"))
                    {
                        ImportOptionalValues.GetXMLAllOptionalValuesFromExamplesFile(requestBody, optionalValuesPerParameterDict);
                        ImportOptionalValues.PopulateXMLOptionalValuesForAPIParameters(aPIModel, optionalValuesPerParameterDict);
                    }
                }
                return optionalValuesPerParameterDict;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while parsing the app parameter", ex);
                return optionalValuesPerParameterDict;
            }
        }

        /// <summary>
        /// This method will parse the request body and add the app parameters to model
        /// </summary>
        /// <param name="aPIModel"></param>
        /// <param name="act"></param>
        /// <param name="parameterizeRequestBody"></param>
        private void ParameterizeApiModellBody(ApplicationAPIModel aPIModel, Act act, bool parameterizeRequestBody)
        {
            ObservableList<ActInputValue> actInputs = ((ActWebAPIBase)act).DynamicElements;
            if (actInputs == null || actInputs.Count == 0)
            {
                string requestBody = string.Empty;
                if (aPIModel.RequestBodyType == ApplicationAPIUtils.eRequestBodyType.TemplateFile)
                {
                    string fileUri = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(aPIModel.TemplateFileNameFileBrowser);
                    if (File.Exists(fileUri))
                    {
                        aPIModel.RequestBody = System.IO.File.ReadAllText(fileUri);
                        aPIModel.RequestBodyType = ApplicationAPIUtils.eRequestBodyType.FreeText;
                    }
                }

                if (!string.IsNullOrEmpty(aPIModel.RequestBody))
                {
                    requestBody = aPIModel.RequestBody;
                }
                
                if (!string.IsNullOrEmpty(requestBody))
                {
                    ObservableList<ApplicationAPIModel> applicationAPIModels = new ObservableList<ApplicationAPIModel>();
                    if (aPIModel.RequestBody.StartsWith("{"))//TODO: find better way to identify JSON format
                    {
                        JSONTemplateParser jsonTemplate = new JSONTemplateParser();
                        jsonTemplate.ParseDocumentWithJsonContent(requestBody, applicationAPIModels);
                    }
                    else if (aPIModel.RequestBody.StartsWith("<"))//TODO: find better way to identify XML format
                    {
                        XMLTemplateParser parser = new XMLTemplateParser();
                        parser.ParseDocumentWithXMLContent(requestBody, applicationAPIModels);
                    }

                    if (applicationAPIModels[0].AppModelParameters != null && applicationAPIModels[0].AppModelParameters.Count > 0)
                    {
                        aPIModel.RequestBody = applicationAPIModels[0].RequestBody;
                        aPIModel.AppModelParameters = applicationAPIModels[0].AppModelParameters;
                    }
                }
            }
            else
            {
                aPIModel.AppModelParameters = ParseParametersFromRequestBodyInputs(aPIModel, actInputs);
            }
        }

        /// <summary>
        /// This method will get the appparameters from the requestbody inputs
        /// </summary>
        /// <param name="aPIModel"></param>
        /// <param name="actInputs"></param>
        /// <returns></returns>
        private ObservableList<AppModelParameter> ParseParametersFromRequestBodyInputs(ApplicationAPIModel aPIModel, ObservableList<ActInputValue> actInputs)
        {
            ObservableList<AppModelParameter> lstParameters = new ObservableList<AppModelParameter>();
            foreach (var inptVal in actInputs)
            {
                AppModelParameter param = new AppModelParameter();
                param.ItemName = inptVal.ItemName;
                OptionalValue opVal = new OptionalValue()
                {
                    Value = inptVal.Value,
                    IsDefault = true
                };
                param.OptionalValuesList = new ObservableList<OptionalValue>() { opVal };
                lstParameters.Add(param);
            }
            foreach (var par in lstParameters)
            {
                if (!CheckParameterExistsIfExistsThenAddValues(aPIModel.AppModelParameters, par))
                {
                    aPIModel.AppModelParameters.Add(par);
                }
            }

            return lstParameters;
        }

        /// <summary>
        /// This method is used to check if the Parameter exists in the model, if exists then add the values to model parameter
        /// </summary>
        /// <param name="lstParameters"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private bool CheckParameterExistsIfExistsThenAddValues(ObservableList<AppModelParameter> lstParameters, AppModelParameter param)
        {
            bool isExists = false;
            foreach (var item in lstParameters)
            {
                if (item.ItemName.Contains(param.ItemName))
                {
                    isExists = true;
                    foreach (var val in param.OptionalValuesList)
                    {
                        var paramOptionalValue = item.OptionalValuesList.Where(x => x.Value == val.Value).FirstOrDefault();
                        if (!string.IsNullOrEmpty(Convert.ToString(val.Value)) && Convert.ToString(val.Value) != "?" && (paramOptionalValue == null || paramOptionalValue.Value != val.Value))
                        {
                            item.OptionalValuesList.Add(val);
                        }
                    }
                    break;
                }
            }
            return isExists;
        }

        /// <summary>
        /// This method is used to create or update API Model from Soap action
        /// </summary>
        /// <param name="actionToConvert"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private void CreateAPIModelFromWebserviceAction(ref ApplicationAPIModel aPIModel, Act actionToConvert, bool pullValidations, bool parameterizeRequestBody)
        {
            try
            {
                aPIModel.ItemName = actionToConvert.ItemName;

                if (actionToConvert.GetType() == typeof(ActWebAPIRest))
                {
                    aPIModel.APIType = ApplicationAPIUtils.eWebApiType.REST;
                }
                else
                {
                    aPIModel.APIType = ApplicationAPIUtils.eWebApiType.SOAP;
                }

                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.RequestBody), nameof(ApplicationAPIModel.RequestBody), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIRest.Fields.RequestType), nameof(ApplicationAPIModel.RequestType), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIRest.Fields.ReqHttpVersion), nameof(ApplicationAPIModel.ReqHttpVersion), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIRest.Fields.ResponseContentType), nameof(ApplicationAPIModel.ResponseContentType), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIRest.Fields.CookieMode), nameof(ApplicationAPIModel.CookieMode), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIRest.Fields.ContentType), nameof(ApplicationAPIModel.ContentType), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPISoap.Fields.SOAPAction), nameof(ApplicationAPIModel.SOAPAction), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.EndPointURL), nameof(ApplicationAPIModel.EndpointURL), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.URLUser), nameof(ApplicationAPIModel.URLUser), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.URLDomain), nameof(ApplicationAPIModel.URLDomain), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.URLPass), nameof(ApplicationAPIModel.URLPass), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.CertificatePath), nameof(ApplicationAPIModel.CertificatePath), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.DoNotFailActionOnBadRespose), nameof(ApplicationAPIModel.DoNotFailActionOnBadRespose), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.ImportCetificateFile), nameof(ApplicationAPIModel.ImportCetificateFile), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.CertificatePassword), nameof(ApplicationAPIModel.CertificatePassword), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.SecurityType), nameof(ApplicationAPIModel.SecurityType), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.AuthorizationType), nameof(ApplicationAPIModel.AuthorizationType), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.TemplateFileNameFileBrowser), nameof(ApplicationAPIModel.TemplateFileNameFileBrowser), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.AuthUsername), nameof(ApplicationAPIModel.AuthUsername), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.AuthPassword), nameof(ApplicationAPIModel.AuthPassword), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.CertificateTypeRadioButton), nameof(ApplicationAPIModel.CertificateType), actionToConvert);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.NetworkCredentialsRadioButton), nameof(ApplicationAPIModel.NetworkCredentials), actionToConvert);

                if (!string.IsNullOrEmpty(Convert.ToString(aPIModel.TemplateFileNameFileBrowser)))
                {
                    aPIModel.RequestBodyType = ApplicationAPIUtils.eRequestBodyType.TemplateFile;
                }

                if (((ActWebAPIBase)actionToConvert).HttpHeaders != null)
                {
                    if (aPIModel.HttpHeaders == null)
                    {
                        aPIModel.HttpHeaders = new ObservableList<APIModelKeyValue>();
                    }
                    foreach (var header in ((ActWebAPIBase)actionToConvert).HttpHeaders)
                    {
                        APIModelKeyValue keyVal = new APIModelKeyValue();
                        keyVal.ItemName = header.ItemName;
                        keyVal.Param = header.ItemName;
                        keyVal.FileName = header.ItemName;
                        keyVal.Value = header.Value;
                        aPIModel.HttpHeaders.Add(keyVal);
                    }
                }

                if (parameterizeRequestBody)
                {
                    ParameterizeApiModellBody(aPIModel, actionToConvert, parameterizeRequestBody);
                }

                if (pullValidations)
                {
                    foreach (var exitingRV in actionToConvert.ReturnValues)
                    {
                        ActReturnValue actR = new ActReturnValue();
                                              
                        if (string.IsNullOrEmpty(exitingRV.Expected) == false)
                        {
                            actR.Expected = exitingRV.Expected;
                        }
                        if (exitingRV.StoreTo == ActReturnValue.eStoreTo.ApplicationModelParameter)
                        {
                            actR.StoreTo = exitingRV.StoreTo;
                            actR.StoreToValue = exitingRV.StoreToValue;
                        }
                        if (actR.Expected != null || actR.StoreTo != ActReturnValue.eStoreTo.None )
                        {
                            actR.Active = true;
                            actR.Param = exitingRV.Param;
                            actR.Path = exitingRV.Path;
                            aPIModel.ReturnValues.Add(actR);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while creating the API Model from webservice Action", ex);
            }
        }

        /// <summary>
        /// This method will set the Property value in API model's property by reading the value from action
        /// </summary>
        /// <param name="aPIModel"></param>
        /// <param name="propName"></param>
        /// <param name="act"></param>
        private void SetPropertyValue(ApplicationAPIModel aPIModel, string propName, string modelPropertyName, Act act)
        {
            if (aPIModel != null && act != null)
            {
                try
                {
                    PropertyInfo actProp = act.GetType().GetProperty(nameof(Act.ActInputValues));
                    if (actProp != null)
                    {
                        ObservableList<ActInputValue> inputValues = (ObservableList<ActInputValue>)actProp.GetValue(act);
                        if (inputValues != null)
                        {
                            var val = inputValues.Where(x => x.FileName == propName).Select(x => x.Value).FirstOrDefault();
                            if (val != null)
                            {
                                PropertyInfo apiProp = aPIModel.GetType().GetProperty(modelPropertyName);
                                var modelVal = apiProp.GetValue(aPIModel);
                                if (apiProp.PropertyType.IsEnum)
                                {
                                    apiProp.SetValue(aPIModel, Enum.Parse(apiProp.PropertyType, Convert.ToString(val)));
                                }
                                else if (apiProp.PropertyType.Equals(typeof(Boolean)))
                                {
                                    apiProp.SetValue(aPIModel, Convert.ToBoolean(val));
                                }
                                else
                                {
                                    if (modelVal == null || string.IsNullOrEmpty(Convert.ToString(modelVal)))
                                    {
                                        apiProp.SetValue(aPIModel, val);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while Setting property value", ex);
                }
            }
        }

        /// <summary>
        /// This method is used to check if the apimodel exists or not
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        private ApplicationAPIModel GetAPIModelIfExists(Act act)
        {
            ApplicationAPIModel aPIModel = null;
            string endPointURL = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.EndPointURL).Select(x => x.Value).FirstOrDefault());
            string soapAction = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPISoap.Fields.SOAPAction).Select(x => x.Value).FirstOrDefault());
            ApplicationAPIUtils.eWebApiType apiType = act.GetType().Name.Equals(typeof(ActWebAPISoap).Name) ? ApplicationAPIUtils.eWebApiType.SOAP : ApplicationAPIUtils.eWebApiType.REST;
            if (!string.IsNullOrEmpty(soapAction))
            {
                aPIModel = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>().Where(x => x.EndpointURL == endPointURL && x.SOAPAction == soapAction && x.APIType == apiType).FirstOrDefault();
            }
            else
            {
                aPIModel = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>().Where(x => x.EndpointURL == endPointURL && x.APIType == apiType).FirstOrDefault();
            }
            return aPIModel;
        }

        /// <summary>
        /// This method will check if the businessFlow contains the WebService targetapplication
        /// </summary>
        /// <param name="bf"></param>
        /// <returns></returns>
        private bool IsValidWebServiceBusinessFlow(BusinessFlow bf)
        {
            bool isValid = false;
            foreach (var ta in bf.TargetApplications)
            {
                isValid = WorkSpace.Instance.Solution.GetApplicationPlatformForTargetApp(ta.ItemName) == ePlatformType.WebServices;
                if (isValid)
                {
                    break;
                }
            }
            return isValid;
        }

        /// <summary>
        /// This method is used to get the Convertible Actions Count From BusinessFlow
        /// </summary>
        /// <param name="bf"></param>
        /// <returns></returns>
        public int GetConvertibleActionsCountFromBusinessFlow(BusinessFlow bf)
        {
            int count = 0;
            try
            {
                foreach (Activity activity in bf.Activities.Where(x => x.Active))
                {
                    count = count + activity.Acts.Where(act => (act.Active &&
                                                               (act.GetType() == typeof(ActWebAPIRest) || act.GetType() == typeof(ActWebAPISoap)))).Count();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to get the count of legacy actions", ex);
            }
            return count;
        }

        /// <summary>
        /// This method stops the multiple businessflow action conversion process
        /// </summary>
        public void StopConversion()
        {
            mStopConversion = true;
        }
    }
}
