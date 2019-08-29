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
            string paramValuesXML = Convert.ToString(act.InputValues.Where(x => x.FileName == ActWebAPIBase.Fields.RequestBody).Select(x => x.Value).FirstOrDefault());
            if (!string.IsNullOrEmpty(paramValuesXML))
            {

            }

            return aPIModel;
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
