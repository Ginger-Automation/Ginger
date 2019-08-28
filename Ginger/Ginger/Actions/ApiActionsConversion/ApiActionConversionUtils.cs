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
                            if (activity != null && activity.Active && (activity.TargetApplication.StartsWith("WebServices") || activity.TargetApplication.Contains("WebServices")))
                            {
                                Activity currentActivity;
                                currentActivity = new Activity() { Active = true };
                                currentActivity = (Activity)activity.CreateCopy(false);
                                currentActivity.ActivityName = "New - " + activity.ActivityName;
                                bf.BusinessFlow.Activities.Insert(intIndex + 1, currentActivity);
                                activity.Active = false;
                                intIndex++;
                                foreach (Act act in currentActivity.Acts.ToList())
                                {
                                    try
                                    {
                                        if (act.Active && act.GetType() == typeof(ActWebAPIModel))
                                        {
                                            // get the index of the action that is being converted 
                                            int selectedActIndex = currentActivity.Acts.IndexOf(act);

                                            //pull pointed API Model
                                            ApplicationAPIModel AAMB = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>().Where(x => x.Guid == ((ActWebAPIModel)act).APImodelGUID).FirstOrDefault();
                                            if (AAMB == null)
                                            {
                                                act.Error = "Failed to find the pointed API Model";
                                                act.ExInfo = string.Format("API Model with the GUID '{0}' was not found", ((ActWebAPIModel)act).APImodelGUID);
                                                return;
                                            }

                                            //init matching real WebAPI Action
                                            ActWebAPIBase actWebAPI = null;
                                            if (AAMB.APIType == ApplicationAPIUtils.eWebApiType.REST)
                                            {
                                                actWebAPI = webAPIModel.CreateActWebAPIREST((ApplicationAPIModel)AAMB, (ActWebAPIModel)act);
                                            }
                                            else if (AAMB.APIType == ApplicationAPIUtils.eWebApiType.SOAP)
                                            {
                                                actWebAPI = webAPIModel.CreateActWebAPISOAP((ApplicationAPIModel)AAMB, (ActWebAPIModel)act);
                                            }

                                            // convert the old action
                                            if (actWebAPI != null)
                                            {
                                                currentActivity.Acts.Insert(selectedActIndex + 1, actWebAPI);
                                                currentActivity.Acts.Remove(act);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert action", ex);
                                    }
                                }
                                currentActivity.TargetApplication = activity.TargetApplication;
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
        /// This method will check if the businessFlow contains the WebService targetapplication
        /// </summary>
        /// <param name="bf"></param>
        /// <returns></returns>
        private bool IsValidWebServiceBusinessFlow(BusinessFlow bf)
        {
            bool isValid = false;

            foreach (var item in bf.TargetApplications)
            {
                if (item.ItemName.StartsWith("WebServices") || item.ItemName.Contains("WebServices"))
                {
                    isValid = true;
                    break;
                } 
            }

            return isValid;
        }
    }
}
