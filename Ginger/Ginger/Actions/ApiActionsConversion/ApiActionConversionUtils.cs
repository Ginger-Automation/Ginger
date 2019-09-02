using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.WebServices;
using GingerCore.Actions.WebServices.WebAPI;
using GingerCore.Drivers.WebServicesDriverLib;
using System;
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

        /// <summary>
        /// This method is used to convert the legacy service actions to api actions from the businessflows provided
        /// </summary>
        /// <param name="businessFlows"></param>
        public void ConvertToApiActionsFromBusinessFlows(ObservableList<BusinessFlowToConvert> businessFlows)
        {
            try
            {
                ActWebAPIModel webAPIModel = new ActWebAPIModel();
                foreach (var bf in businessFlows)
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
                                            ApplicationAPIModel applicationModel = GetAPIModelIfExists(act);

                                            if (applicationModel == null && act.GetType() == typeof(ActWebAPIRest))
                                            {
                                                applicationModel = GetWebAPIRestModel((ActWebAPIRest)act); 
                                            }
                                            else if(applicationModel == null && act.GetType() == typeof(ActWebAPISoap))
                                            {
                                                applicationModel = GetWebAPISOAPModel((ActWebAPISoap)act);
                                            }

                                            if(applicationModel != null)
                                            {

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
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert action", ex);
            }
        }

        /// <summary>
        /// This method is used to create or update API Model from Rest action
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        private ApplicationAPIModel GetWebAPIRestModel(ActWebAPIRest act)
        {
            ApplicationAPIModel aPIModel = new ApplicationAPIModel();
            aPIModel.APIType = ApplicationAPIUtils.eWebApiType.REST;
            string paramValuesXML = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.RequestBody).Select(x => x.Value).FirstOrDefault());
            if (!string.IsNullOrEmpty(paramValuesXML))
            {
                XMLTemplateParser parser = new XMLTemplateParser();
                aPIModel.AppModelParameters = parser.GetAppParameterFromXML(paramValuesXML);
            }

            return aPIModel;
        }

        /// <summary>
        /// This method is used to create or update API Model from Soap action
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        private ApplicationAPIModel GetWebAPISOAPModel(ActWebAPISoap act)
        {
            ApplicationAPIModel aPIModel = new ApplicationAPIModel();
            aPIModel.APIType = ApplicationAPIUtils.eWebApiType.SOAP;
            aPIModel.RequestBody = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.RequestBody).Select(x => x.Value).FirstOrDefault());


            //SetPropertyValue(aPIModel, RequestType, act);

            aPIModel.RequestType = (ApplicationAPIUtils.eRequestType)(Enum.Parse(typeof(ApplicationAPIUtils.eRequestType), Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIRest.Fields.RequestType).Select(x => x.Value).FirstOrDefault()), true));
            aPIModel.ReqHttpVersion = (ApplicationAPIUtils.eHttpVersion)(Enum.Parse(typeof(ApplicationAPIUtils.eHttpVersion), Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIRest.Fields.ReqHttpVersion).Select(x => x.Value).FirstOrDefault()), true));
            aPIModel.ResponseContentType = (ApplicationAPIUtils.eContentType)(Enum.Parse(typeof(ApplicationAPIUtils.eContentType), Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIRest.Fields.ResponseContentType).Select(x => x.Value).FirstOrDefault()), true));
            aPIModel.CookieMode = (ApplicationAPIUtils.eCookieMode)(Enum.Parse(typeof(ApplicationAPIUtils.eCookieMode), Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIRest.Fields.CookieMode).Select(x => x.Value).FirstOrDefault()), true));
            aPIModel.ContentType = (ApplicationAPIUtils.eContentType)(Enum.Parse(typeof(ApplicationAPIUtils.eContentType), Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIRest.Fields.ContentType).Select(x => x.Value).FirstOrDefault()), true));
            aPIModel.SOAPAction = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPISoap.Fields.SOAPAction).Select(x => x.Value).FirstOrDefault());
            //aPIModel.EndPointURL = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.EndPointURL).Select(x => x.Value).FirstOrDefault());
            //aPIModel.NetworkCredentialsRadioButton = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.NetworkCredentialsRadioButton).Select(x => x.Value).FirstOrDefault());
            aPIModel.URLUser = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.URLUser).Select(x => x.Value).FirstOrDefault());
            aPIModel.URLDomain = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.URLDomain).Select(x => x.Value).FirstOrDefault());
            aPIModel.URLPass = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.URLPass).Select(x => x.Value).FirstOrDefault());
            aPIModel.DoNotFailActionOnBadRespose = Convert.ToBoolean(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.DoNotFailActionOnBadRespose).Select(x => x.Value).FirstOrDefault());
            //aPIModel.RequestBodyTypeRadioButton = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.RequestBodyTypeRadioButton).Select(x => x.Value).FirstOrDefault());
            //aPIModel.CertificateTypeRadioButton = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.CertificateTypeRadioButton).Select(x => x.Value).FirstOrDefault());
            aPIModel.CertificatePath = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.CertificatePath).Select(x => x.Value).FirstOrDefault());
            aPIModel.ImportCetificateFile = Convert.ToBoolean(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.ImportCetificateFile).Select(x => x.Value).FirstOrDefault());
            aPIModel.CertificatePassword = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.CertificatePassword).Select(x => x.Value).FirstOrDefault());
            aPIModel.SecurityType = (ApplicationAPIUtils.eSercurityType)(Enum.Parse(typeof(ApplicationAPIUtils.eSercurityType), Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.SecurityType).Select(x => x.Value).FirstOrDefault()), true));
            aPIModel.AuthorizationType = (ApplicationAPIUtils.eAuthType)(Enum.Parse(typeof(ApplicationAPIUtils.eAuthType), Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.AuthorizationType).Select(x => x.Value).FirstOrDefault()), true));
            aPIModel.TemplateFileNameFileBrowser = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.TemplateFileNameFileBrowser).Select(x => x.Value).FirstOrDefault());
            aPIModel.AuthUsername = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.AuthUsername).Select(x => x.Value).FirstOrDefault());
            aPIModel.AuthPassword = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.AuthPassword).Select(x => x.Value).FirstOrDefault());

            if (!string.IsNullOrEmpty(aPIModel.RequestBody))
            {
                XMLTemplateParser parser = new XMLTemplateParser();
                aPIModel.AppModelParameters = parser.GetAppParameterFromXML(aPIModel.RequestBody);
            }

            return aPIModel;
        }

        /// <summary>
        /// This method will set the Property value in API model's property by reading the value from action
        /// </summary>
        /// <param name="aPIModel"></param>
        /// <param name="propName"></param>
        /// <param name="act"></param>
        private void SetPropertyValue(ApplicationAPIModel aPIModel, string propName, ActWebAPISoap act)
        {
            if(aPIModel != null && act != null)
            {
                PropertyInfo p = aPIModel.GetType().GetProperty(propName);
                if (p.PropertyType.IsEnum)
                {
                    //Type a = p.GetType() Enum.Parse(typeof(p.GetType()), value);
                    //p.SetValue(this, a);
                }
            }
            //(ApplicationAPIUtils.eRequestType)(Enum.Parse(typeof(ApplicationAPIUtils.eRequestType), Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIRest.Fields.RequestType).Select(x => x.Value).FirstOrDefault()), true));
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
    }
}
