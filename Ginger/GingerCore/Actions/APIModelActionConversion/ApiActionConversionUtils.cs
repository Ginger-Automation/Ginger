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

                                                if (applicationModel == null)
                                                {
                                                    applicationModel = GetApplicationAPIModel(act, bf.BusinessFlow.ContainingFolderFullPath);
                                                    isModelExists = false;
                                                }

                                                AddAppParameters(applicationModel, act);
                                                if (!isModelExists)
                                                {
                                                    WorkSpace.Instance.SolutionRepository.AddRepositoryItem(applicationModel);
                                                }
                                                else
                                                {
                                                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(applicationModel);
                                                }

                                                //Add WebAPIModel action
                                                ActWebAPIModel actWebAPI = new ActWebAPIModel();
                                                if (applicationModel.APIType == ApplicationAPIUtils.eWebApiType.REST)
                                                {
                                                    ActWebAPIRest webAPIRest = actWebAPI.CreateActWebAPIREST(applicationModel, actWebAPI);
                                                    webAPIRest.ItemName = act.ItemName;
                                                    webAPIRest.Active = true;
                                                    activity.Acts.Insert(selectedActIndex + 1, webAPIRest);
                                                }
                                                else
                                                {
                                                    ActWebAPISoap webAPISoap = actWebAPI.CreateActWebAPISOAP(applicationModel, actWebAPI);
                                                    webAPISoap.ItemName = act.ItemName;
                                                    webAPISoap.Active = true;
                                                    activity.Acts.Insert(selectedActIndex + 1, webAPISoap);
                                                }
                                                act.Active = false;
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
        /// This method is used to check if the app parameter is already added in the list if not then it will add the parameter
        /// </summary>
        /// <param name="aPIModel"></param>
        /// <param name="act"></param>
        private void AddAppParameters(ApplicationAPIModel aPIModel, Act act)
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

                SetPropertyValue(aPIModel, nameof(ActWebAPIBase.Fields.RequestBody), nameof(ApplicationAPIModel.RequestBody), act);
                ObservableList<AppModelParameter> lstParameters = null;
                JSONTemplateParser jsonTemplate = new JSONTemplateParser();
                if (contentType != null && contentType.ToLower() == "xml" && !string.IsNullOrEmpty(aPIModel.RequestBody))
                {
                    XMLTemplateParser parser = new XMLTemplateParser();
                    lstParameters = parser.GetAppParameterFromXML(aPIModel.RequestBody);
                }
                else if (contentType != null && contentType.ToLower() == "json" && !string.IsNullOrEmpty(aPIModel.RequestBody))
                {
                    lstParameters = jsonTemplate.GetAppModelParametersFromJson(aPIModel.RequestBody);
                }

                if (lstParameters != null && lstParameters.Count > 0)
                {
                    foreach (var param in lstParameters)
                    {
                        if (!CheckParameterExistsIfExistsThenAddValues(aPIModel.AppModelParameters, param))
                        {
                            aPIModel.AppModelParameters.Add(param); 
                        }
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
        private ApplicationAPIModel GetApplicationAPIModel(Act act, string path)
        {
            string folderPath = Path.Combine(path, @"Applications Models\API Models\");
            ApplicationAPIModel aPIModel = new ApplicationAPIModel();
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

                
                //aPIModel.RequestBody = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.RequestBody).Select(x => x.Value).FirstOrDefault());
                //aPIModel.RequestType = (ApplicationAPIUtils.eRequestType)(Enum.Parse(typeof(ApplicationAPIUtils.eRequestType), Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIRest.Fields.RequestType).Select(x => x.Value).FirstOrDefault()), true));
                //aPIModel.ReqHttpVersion = (ApplicationAPIUtils.eHttpVersion)(Enum.Parse(typeof(ApplicationAPIUtils.eHttpVersion), Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIRest.Fields.ReqHttpVersion).Select(x => x.Value).FirstOrDefault()), true));
                //aPIModel.ResponseContentType = (ApplicationAPIUtils.eContentType)(Enum.Parse(typeof(ApplicationAPIUtils.eContentType), Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIRest.Fields.ResponseContentType).Select(x => x.Value).FirstOrDefault()), true));
                //aPIModel.CookieMode = (ApplicationAPIUtils.eCookieMode)(Enum.Parse(typeof(ApplicationAPIUtils.eCookieMode), Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIRest.Fields.CookieMode).Select(x => x.Value).FirstOrDefault()), true));
                //aPIModel.ContentType = (ApplicationAPIUtils.eContentType)(Enum.Parse(typeof(ApplicationAPIUtils.eContentType), Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIRest.Fields.ContentType).Select(x => x.Value).FirstOrDefault()), true));
                //aPIModel.SOAPAction = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPISoap.Fields.SOAPAction).Select(x => x.Value).FirstOrDefault());
                //aPIModel.EndPointURL = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.EndPointURL).Select(x => x.Value).FirstOrDefault());
                //aPIModel.URLUser = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.URLUser).Select(x => x.Value).FirstOrDefault());
                //aPIModel.URLDomain = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.URLDomain).Select(x => x.Value).FirstOrDefault());
                //aPIModel.URLPass = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.URLPass).Select(x => x.Value).FirstOrDefault());
                //aPIModel.CertificatePath = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.CertificatePath).Select(x => x.Value).FirstOrDefault());
                //aPIModel.DoNotFailActionOnBadRespose = Convert.ToBoolean(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.DoNotFailActionOnBadRespose).Select(x => x.Value).FirstOrDefault());
                //aPIModel.ImportCetificateFile = Convert.ToBoolean(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.ImportCetificateFile).Select(x => x.Value).FirstOrDefault());
                //aPIModel.CertificatePassword = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.CertificatePassword).Select(x => x.Value).FirstOrDefault());
                //aPIModel.SecurityType = (ApplicationAPIUtils.eSercurityType)(Enum.Parse(typeof(ApplicationAPIUtils.eSercurityType), Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.SecurityType).Select(x => x.Value).FirstOrDefault()), true));
                //aPIModel.AuthorizationType = (ApplicationAPIUtils.eAuthType)(Enum.Parse(typeof(ApplicationAPIUtils.eAuthType), Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.AuthorizationType).Select(x => x.Value).FirstOrDefault()), true));
                //aPIModel.TemplateFileNameFileBrowser = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.TemplateFileNameFileBrowser).Select(x => x.Value).FirstOrDefault());
                //aPIModel.AuthUsername = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.AuthUsername).Select(x => x.Value).FirstOrDefault());
                //aPIModel.AuthPassword = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.AuthPassword).Select(x => x.Value).FirstOrDefault());

                //aPIModel.NetworkCredentialsRadioButton = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.NetworkCredentialsRadioButton).Select(x => x.Value).FirstOrDefault());
                //aPIModel.RequestBodyTypeRadioButton = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.RequestBodyTypeRadioButton).Select(x => x.Value).FirstOrDefault());
                //aPIModel.CertificateTypeRadioButton = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.CertificateTypeRadioButton).Select(x => x.Value).FirstOrDefault());
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while creating the api model", ex);
            }
            return aPIModel;
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
                                if (apiProp.PropertyType.IsEnum)
                                {
                                    apiProp.SetValue(aPIModel, Enum.Parse(apiProp.PropertyType, Convert.ToString(val)));
                                }
                                else if(apiProp.PropertyType.Equals(typeof(Boolean)))
                                {
                                    apiProp.SetValue(aPIModel, Convert.ToBoolean(val));
                                }
                                else
                                {
                                    apiProp.SetValue(aPIModel, val);
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
