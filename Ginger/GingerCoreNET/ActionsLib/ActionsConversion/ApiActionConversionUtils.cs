using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.CoreNET.ActionsLib.ActionsConversion
{
    /// <summary>
    /// This class is used to convert the actions to Api Actions
    /// </summary>
    public class ApiActionConversionUtils
    {
        /// <summary>
        /// This method is used to add the actions
        /// </summary>
        /// <param name="businessFlows"></param>
        public void ConvertToApiActionsFromBusinessFlow(ObservableList<BusinessFlow> businessFlows)
        {
            try
            {
                foreach (var bf in businessFlows)
                {
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
                                    if (act.Active)
                                    {
                                        // get the index of the action that is being converted 
                                        int selectedActIndex = currentActivity.Acts.IndexOf(act);

                                        // convert the old action
                                        Act newAct = null; //= ((IObsoleteAction)act).GetNewAction();
                                        if (newAct != null)
                                        {
                                            newAct.Platform = ((IObsoleteAction)act).GetTargetPlatform();
                                            currentActivity.Acts.Insert(selectedActIndex + 1, newAct);
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
