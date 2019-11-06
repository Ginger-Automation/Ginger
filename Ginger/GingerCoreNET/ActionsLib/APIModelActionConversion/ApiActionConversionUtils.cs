using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.APIModelLib;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.WebServices;
using GingerCore.Actions.WebServices.WebAPI;
using System;
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
        private string WEB_SERVICE = "WebServices";
        bool mStopConversion = false;

        /// <summary>
        /// This method is used to convert the legacy service actions to api actions from the businessflows provided
        /// </summary>
        /// <param name="businessFlows"></param>
        public void ConvertToApiActionsFromBusinessFlows(ObservableList<BusinessFlowToConvert> businessFlows)
        {
            try
            {
                ActWebAPIModel webAPIModel = new ActWebAPIModel();
                //Parallel.ForEach(businessFlows, (bf, state) =>
                foreach(BusinessFlowToConvert bf in businessFlows)
                {
                    if (!mStopConversion)
                    {
                        bf.ConversionStatus = eConversionStatus.Pending;
                        if (IsValidWebServiceBusinessFlow(bf.BusinessFlow))
                        {
                            for (int intIndex = 0; intIndex < bf.BusinessFlow.Activities.Count(); intIndex++)
                            {
                                Activity activity = bf.BusinessFlow.Activities[intIndex];
                                if (activity != null && activity.Active && (activity.TargetApplication.StartsWith(WEB_SERVICE) || activity.TargetApplication.Contains(WEB_SERVICE)))
                                {
                                    foreach (Act act in activity.Acts.ToList())
                                    {
                                        try
                                        {
                                            if (act.Active && (act.GetType() == typeof(ActWebAPIRest) || act.GetType() == typeof(ActWebAPISoap)))
                                            {
                                                // get the index of the action that is being converted 
                                                int selectedActIndex = activity.Acts.IndexOf(act);

                                                //Create/Update API Model
                                                bool isModelExists = true;
                                                ApplicationAPIModel applicationModel = GetAPIModelIfExists(act);

                                                if(applicationModel == null)
                                                {
                                                    isModelExists = false;
                                                    applicationModel = new ApplicationAPIModel();
                                                    applicationModel.TargetApplicationKey = bf.BusinessFlow.TargetApplications.Where(x => x.ItemName == activity.TargetApplication).FirstOrDefault().Key;
                                                }
                                                SetApplicationAPIModel(ref applicationModel, act, bf.BusinessFlow.ContainingFolderFullPath);
                                                
                                                //Create WebAPIModel action
                                                ActWebAPIModel actApiModel = GetNewAPIModelAction(applicationModel.Guid, act);                                                
                                                AddAppParameters(applicationModel, act, actApiModel);
                                                
                                                activity.Acts.Insert(selectedActIndex + 1, actApiModel);
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
                            }
                        }
                        bf.ConversionStatus = eConversionStatus.Finish; 
                    }
                }
                //);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert action", ex);
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
            ActWebAPIModel actModel = new ActWebAPIModel();
            actModel.ItemName = string.Format("New - {0}", act.ItemName);
            actModel.Description = string.Format("New - {0}", act.Description);
            actModel.APImodelGUID = applicationModelGuid;
            actModel.ReturnValues = act.ActReturnValues;
            actModel.MaxNumberOfRetries = act.MaxNumberOfRetries;
            actModel.Active = true;            
            return actModel;
        }

        /// <summary>
        /// This method is used to check if the app parameter is already added in the list if not then it will add the parameter
        /// </summary>
        /// <param name="aPIModel"></param>
        /// <param name="act"></param>
        /// <param name="actApiModel"></param>
        private void AddAppParameters(ApplicationAPIModel aPIModel, Act act, ActWebAPIModel actApiModel)
        {
            try
            {
                string contentType = string.Empty;
                PropertyInfo actProp = act.GetType().GetProperty(nameof(Act.ActInputValues));
                if (actProp != null)
                {
                    ObservableList<ActInputValue> inputValues = (ObservableList<ActInputValue>)actProp.GetValue(act);
                    if (inputValues != null)
                    {
                        var val = inputValues.Where(x => x.FileName == nameof(ActWebAPIRest.Fields.ContentType)).Select(x => x.Value).FirstOrDefault();
                        contentType = Convert.ToString(val);
                    }
                }

                string requestBody = string.Empty;
                if(aPIModel.RequestBodyType == ApplicationAPIUtils.eRequestBodyType.TemplateFile)
                {
                    string apiModelPath = aPIModel.ContainingFolder.Replace("\\Applications Models\\API Models\\", "");
                    string fileUri = aPIModel.TemplateFileNameFileBrowser.Replace("~\\", apiModelPath);
                    if (File.Exists(fileUri))
                    {
                        requestBody = System.IO.File.ReadAllText(fileUri); 
                    }
                }
                else
                {
                    SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.RequestBody), nameof(ApplicationAPIModel.RequestBody), act);
                    requestBody = Convert.ToString(aPIModel.RequestBody);
                }
                ObservableList<AppModelParameter> lstParameters = null;                
                if (!string.IsNullOrEmpty(requestBody) && requestBody.StartsWith("{"))
                {
                    JSONTemplateParser jsonTemplate = new JSONTemplateParser();
                    lstParameters = jsonTemplate.GetAppModelParametersFromJson(requestBody);
                }
                else if (!string.IsNullOrEmpty(requestBody) && requestBody.StartsWith("<"))
                {
                    XMLTemplateParser parser = new XMLTemplateParser();
                    lstParameters = parser.GetAppParameterFromXML(requestBody);
                }

                if (lstParameters != null && lstParameters.Count > 0)
                {
                    if(actApiModel.ActAppModelParameters == null)
                    {
                        actApiModel.ActAppModelParameters = new ObservableList<AppModelParameter>();
                    }

                    foreach (var param in lstParameters)
                    {
                        if (!CheckParameterExistsIfExistsThenAddValues(aPIModel.AppModelParameters, param))
                        {
                            aPIModel.AppModelParameters.Add(param); 
                        }

                        actApiModel.ActAppModelParameters.Add(param);
                    }
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
                if(item.ItemName.Contains(param.ItemName))
                {
                    isExists = true;
                    foreach (var val in param.OptionalValuesList)
                    {
                        item.OptionalValuesList.Add(val);
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
        private void SetApplicationAPIModel(ref ApplicationAPIModel aPIModel, Act act, string path)
        {
            string folderPath = Path.Combine(path, @"Applications Models\API Models\");
            try
            {
                aPIModel.ItemName = act.ItemName;
                aPIModel.ContainingFolder = folderPath;
                aPIModel.ContainingFolderFullPath = Path.Combine(folderPath, string.Format("{0}.xml", act.ItemName));
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
            if(aPIModel != null && act != null)
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
            if (!string.IsNullOrEmpty(endPointURL))
            {
                aPIModel = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>().Where(x => x.EndpointURL == endPointURL).FirstOrDefault();
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
            bool isValid = string.IsNullOrEmpty(Convert.ToString(bf.TargetApplications.Where(x => x.ItemName.StartsWith(WEB_SERVICE) || x.ItemName.Contains(WEB_SERVICE)).FirstOrDefault())) ? false : true;
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
