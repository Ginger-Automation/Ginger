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
using System.Threading.Tasks;

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
        public void ConvertToApiActionsFromBusinessFlows(ObservableList<BusinessFlowToConvert> businessFlows, bool parameterizeRequestBody, bool pullValidations)
        {
            ActWebAPIModel webAPIModel = new ActWebAPIModel();
            foreach(var bf in businessFlows)
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
                                    ConvertAction(parameterizeRequestBody, pullValidations, activity, ta);
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
        /// This method will convert the action
        /// </summary>
        /// <param name="parameterizeRequestBody"></param>
        /// <param name="pullValidations"></param>
        /// <param name="activity"></param>
        /// <param name="ta"></param>
        private void ConvertAction(bool parameterizeRequestBody, bool pullValidations, Activity activity, RepositoryItemKey ta)
        {
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
                            SetApplicationAPIModel(ref applicationModel, act, pullValidations);
                        }

                        //Create WebAPIModel action
                        ActWebAPIModel actApiModel = GetNewAPIModelAction(applicationModel.Guid, act);
                        AddAppParameters(applicationModel, act, actApiModel, parameterizeRequestBody, isModelExists);

                        activity.Acts.Insert(selectedActIndex + 1, actApiModel);
                        actionIndex++;
                        act.Active = false;
                        if (!isModelExists)
                        {
                            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(applicationModel);
                        }
                        else
                        {
                            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(applicationModel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert action", ex);
                }
            }
        }

        /// <summary>
        /// This method will create the new api model action
        /// </summary>
        /// <param name="applicationModel"></param>
        /// <param name="act"></param>
        /// <returns></returns>
        private ActWebAPIModel GetNewAPIModelAction(Guid applicationModelGuid, Act act)
        {
            AutoMapper.MapperConfiguration mapConfigUIElement = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<Act, ActWebAPIModel>(); });
            ActWebAPIModel newActModel = mapConfigUIElement.CreateMapper().Map<Act, ActWebAPIModel>(act);
            newActModel.APImodelGUID = applicationModelGuid;
            newActModel.Active = true;
            return newActModel;
        }

        /// <summary>
        /// This method is used to check if the app parameter is already added in the list if not then it will add the parameter
        /// </summary>
        /// <param name="aPIModel"></param>
        /// <param name="act"></param>
        /// <param name="actApiModel"></param>
        private void AddAppParameters(ApplicationAPIModel aPIModel, Act act, ActWebAPIModel actApiModel, bool parameterizeRequestBody, bool isModelExists)
        {
            try
            {
                ObservableList<AppModelParameter> lstParameters = null;
                ObservableList<ActInputValue> actInputs = ((ActWebAPIBase)act).DynamicElements;
                if (actInputs == null || actInputs.Count == 0)
                {
                    string requestBody = string.Empty;
                    if (aPIModel.RequestBodyType == ApplicationAPIUtils.eRequestBodyType.TemplateFile)
                    {
                        string fileUri = WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(aPIModel.TemplateFileNameFileBrowser);
                        if (File.Exists(fileUri))
                        {
                            requestBody = System.IO.File.ReadAllText(fileUri);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(aPIModel.RequestBody))
                        {
                            SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.RequestBody), nameof(ApplicationAPIModel.RequestBody), act);
                            requestBody = Convert.ToString(aPIModel.RequestBody);
                        }
                        else
                        {
                            requestBody = Convert.ToString(aPIModel.RequestBody);
                        }
                    }

                    ObservableList<ApplicationAPIModel> applicationAPIModels = new ObservableList<ApplicationAPIModel>();
                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        if (aPIModel.RequestBody.StartsWith("{"))
                        {
                            JSONTemplateParser jsonTemplate = new JSONTemplateParser();
                            jsonTemplate.ParseDocumentWithJsonContent(requestBody, applicationAPIModels);
                        }
                        else if (aPIModel.RequestBody.StartsWith("<"))
                        {
                            XMLTemplateParser parser = new XMLTemplateParser();
                            parser.ParseDocumentWithXMLContent(requestBody, applicationAPIModels);
                        }

                        if (applicationAPIModels[0].AppModelParameters != null && applicationAPIModels[0].AppModelParameters.Count > 0)
                        {
                            foreach(var par in applicationAPIModels[0].AppModelParameters)
                            {
                                if (!CheckParameterExistsIfExistsThenAddValues(aPIModel.AppModelParameters, par))
                                {
                                    aPIModel.AppModelParameters.Add(par); 
                                }
                            }
                        }
                    }

                    if (!isModelExists && parameterizeRequestBody && applicationAPIModels.Count > 0 && !string.IsNullOrEmpty(applicationAPIModels[0].RequestBody))
                    {
                        aPIModel.RequestBody = applicationAPIModels[0].RequestBody;
                    }

                    if (!isModelExists)
                    {
                        Dictionary<System.Tuple<string, string>, List<string>> OptionalValuesPerParameterDict = new Dictionary<Tuple<string, string>, List<string>>();
                        ImportParametersOptionalValues ImportOptionalValues = new ImportParametersOptionalValues();
                        if (!string.IsNullOrEmpty(requestBody) && requestBody.StartsWith("{"))
                        {
                            ImportOptionalValues.GetJSONAllOptionalValuesFromExamplesFile(requestBody, OptionalValuesPerParameterDict);
                            ImportOptionalValues.PopulateJSONOptionalValuesForAPIParameters(aPIModel, OptionalValuesPerParameterDict);
                        }
                        else if (!string.IsNullOrEmpty(requestBody) && requestBody.StartsWith("<"))
                        {
                            ImportOptionalValues.GetXMLAllOptionalValuesFromExamplesFile(requestBody, OptionalValuesPerParameterDict);
                            ImportOptionalValues.PopulateXMLOptionalValuesForAPIParameters(aPIModel, OptionalValuesPerParameterDict);
                        } 
                    }

                    actApiModel.ActAppModelParameters = aPIModel.AppModelParameters;
                }
                else
                {
                    lstParameters = new ObservableList<AppModelParameter>();
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
                    aPIModel.AppModelParameters = lstParameters;
                    actApiModel.ActAppModelParameters = lstParameters;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while parsing the app parameter", ex);
            }
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
        /// <param name="act"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private void SetApplicationAPIModel(ref ApplicationAPIModel aPIModel, Act act, bool pullValidations)
        {
            string folderPath = Path.Combine(WorkSpace.Instance.Solution.ContainingFolderFullPath, @"Applications Models\API Models\");
            try
            {
                aPIModel.ItemName = act.ItemName;
                aPIModel.ContainingFolder = folderPath;
                aPIModel.ContainingFolderFullPath = Path.Combine(folderPath, string.Format("{0}.xml", act.ItemName));
                if (((ActWebAPIBase)act).HttpHeaders != null)
                {
                    if (aPIModel.HttpHeaders == null)
                    {
                        aPIModel.HttpHeaders = new ObservableList<APIModelKeyValue>();
                    }
                    foreach (var header in ((ActWebAPIBase)act).HttpHeaders)
                    {
                        APIModelKeyValue keyVal = new APIModelKeyValue();
                        keyVal.ItemName = header.ItemName;
                        keyVal.Param = header.ItemName;
                        keyVal.FileName = header.ItemName;
                        keyVal.Value = header.Value;
                        aPIModel.HttpHeaders.Add(keyVal);
                    }
                }

                if (pullValidations)
                {
                    foreach (var item in act.ReturnValues)
                    {
                        if ((!string.IsNullOrEmpty(item.Expected) && 
                            (item.StoreTo == ActReturnValue.eStoreTo.None || 
                             item.StoreTo == ActReturnValue.eStoreTo.ApplicationModelParameter)))
                         {
                            ActReturnValue actR = new ActReturnValue();
                            actR.ItemName = item.ItemName;
                            actR.FileName = item.FileName;
                            actR.FilePath = item.FilePath;
                            actR.Param = item.Param;
                            actR.ParamCalculated = item.ParamCalculated;
                            actR.Expected = item.Expected;
                            aPIModel.ReturnValues.Add(actR);
                        }
                    }
                }

                if (act.GetType() == typeof(ActWebAPIRest))
                {
                    aPIModel.APIType = ApplicationAPIUtils.eWebApiType.REST;
                }
                else
                {
                    aPIModel.APIType = ApplicationAPIUtils.eWebApiType.SOAP;
                }

                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.RequestBody), nameof(ApplicationAPIModel.RequestBody), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIRest.Fields.RequestType), nameof(ApplicationAPIModel.RequestType), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIRest.Fields.ReqHttpVersion), nameof(ApplicationAPIModel.ReqHttpVersion), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIRest.Fields.ResponseContentType), nameof(ApplicationAPIModel.ResponseContentType), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIRest.Fields.CookieMode), nameof(ApplicationAPIModel.CookieMode), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIRest.Fields.ContentType), nameof(ApplicationAPIModel.ContentType), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPISoap.Fields.SOAPAction), nameof(ApplicationAPIModel.SOAPAction), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.EndPointURL), nameof(ApplicationAPIModel.EndpointURL), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.URLUser), nameof(ApplicationAPIModel.URLUser), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.URLDomain), nameof(ApplicationAPIModel.URLDomain), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.URLPass), nameof(ApplicationAPIModel.URLPass), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.CertificatePath), nameof(ApplicationAPIModel.CertificatePath), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.DoNotFailActionOnBadRespose), nameof(ApplicationAPIModel.DoNotFailActionOnBadRespose), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.ImportCetificateFile), nameof(ApplicationAPIModel.ImportCetificateFile), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.CertificatePassword), nameof(ApplicationAPIModel.CertificatePassword), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.SecurityType), nameof(ApplicationAPIModel.SecurityType), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.AuthorizationType), nameof(ApplicationAPIModel.AuthorizationType), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.TemplateFileNameFileBrowser), nameof(ApplicationAPIModel.TemplateFileNameFileBrowser), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.AuthUsername), nameof(ApplicationAPIModel.AuthUsername), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.AuthPassword), nameof(ApplicationAPIModel.AuthPassword), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.CertificateTypeRadioButton), nameof(ApplicationAPIModel.CertificateType), act);
                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.NetworkCredentialsRadioButton), nameof(ApplicationAPIModel.NetworkCredentials), act);

                if (!string.IsNullOrEmpty(Convert.ToString(aPIModel.TemplateFileNameFileBrowser)))
                {
                    aPIModel.RequestBodyType = ApplicationAPIUtils.eRequestBodyType.TemplateFile;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while creating the api model", ex);
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
                if(isValid)
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
