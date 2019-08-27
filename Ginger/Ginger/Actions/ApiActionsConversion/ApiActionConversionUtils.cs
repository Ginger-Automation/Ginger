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
        public void ConvertToApiActionsFromBusinessFlows(ObservableList<BusinessFlow> businessFlows)
        {
            try
            {                
                foreach (var bf in businessFlows)
                {
                    WebServicesDriver driver = new WebServicesDriver(bf);
                    for (int intIndex = 0; intIndex < bf.Activities.Count(); intIndex++)
                    {
                        Activity activity = bf.Activities[intIndex];
                        if (activity != null && activity.SelectedForConversion && activity.Acts.OfType<IObsoleteAction>().ToList().Count > 0)
                        {
                            Activity currentActivity;
                            currentActivity = new Activity() { Active = true };
                            currentActivity = (Activity)activity.CreateCopy(false);
                            currentActivity.ActivityName = "New - " + activity.ActivityName;
                            bf.Activities.Insert(intIndex + 1, currentActivity);
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
                                            actWebAPI = driver.CreateActWebAPIREST((ApplicationAPIModel)AAMB, (ActWebAPIModel)act);
                                        }
                                        else if (AAMB.APIType == ApplicationAPIUtils.eWebApiType.SOAP)
                                        {
                                            actWebAPI = driver.CreateActWebAPISOAP((ApplicationAPIModel)AAMB, (ActWebAPIModel)act);
                                        }

                                        // convert the old action
                                        if (actWebAPI != null)
                                        {
                                            actWebAPI.Platform = ((IObsoleteAction)act).GetTargetPlatform();
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
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert action", ex);
            }
        }
    }
}
