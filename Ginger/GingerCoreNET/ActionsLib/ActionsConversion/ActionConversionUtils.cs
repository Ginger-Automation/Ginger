#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Amdocs.Ginger.CoreNET.BusinessFlowToConvert;

namespace Amdocs.Ginger.CoreNET
{
    /// <summary>
    /// This class is used to add methods for action conversion helpers
    /// </summary>
    public class ActionConversionUtils
    {
        public string ActUIElementElementLocateByField
        {
            get;
            set;
        }

        public string ActUIElementLocateValueField
        {
            get;
            set;
        }

        public string ActUIElementElementLocateValueField
        {
            get;
            set;
        }

        public string ActUIElementElementTypeField
        {
            get;
            set;
        }

        public string ActUIElementClassName
        {
            get;
            set;
        }
        
        bool mStopConversion = false;

        public ObservableList<BusinessFlowToConvert> ListOfBusinessFlowsToConvert = new ObservableList<BusinessFlowToConvert>();

        /// <summary>
        /// This method stops the multiple businessflow action conversion process
        /// </summary>
        public void StopConversion()
        {
            mStopConversion = true;
        }

        /// <summary>
        /// This method stops the multiple businessflow action conversion process
        /// </summary>
        public void ContinueConversion(ObservableList<ConvertableActionDetails> actionsToBeConverted,
                                       bool addNewActivity,
                                       ObservableList<ConvertableTargetApplicationDetails> convertableTargetApplications,
                                       bool convertToPOMAction = false, ObservableList<Guid> selectedPOMs = null)
        {
            mStopConversion = false;
            ConvertActionsOfMultipleBusinessFlows(actionsToBeConverted, addNewActivity, convertableTargetApplications, convertToPOMAction, selectedPOMs);
        }

        /// <summary>
        /// This method is used to remove the Legacy actions from the businessFlows
        /// </summary>
        /// <param name="lst"></param>
        public void RemoveLegacyActionsHandler(ObservableList<BusinessFlowToConvert> lst)
        {
            try
            {
                foreach (BusinessFlowToConvert businessFlowToConvert in lst)
                {
                    for (int activityIndex = 0; activityIndex <= businessFlowToConvert.BusinessFlow.Activities.Count; activityIndex++)
                    {
                        Activity activity = businessFlowToConvert.BusinessFlow.Activities[activityIndex];
                        ePlatformType activityPlatform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == activity.TargetApplication select x.Platform).FirstOrDefault();
                        if (activity.Active)
                        {
                            for (int actIndex = 0; actIndex < activity.Acts.Count; actIndex++)
                            {
                                Act act = (Act)activity.Acts[actIndex];
                                if((act.Active &&
                                   (act is IObsoleteAction) && 
                                   (((IObsoleteAction)act).IsObsoleteForPlatform(activityPlatform)) &&
                                   (((IObsoleteAction)act).TargetAction()) != null))
                                {
                                    activity.Acts.RemoveAt(actIndex);
                                }
                            }
                            if(activity.Acts.Count <= 0 || activity.Acts.Where(x => x.Active == true).Count() <= 0)
                            {
                                businessFlowToConvert.BusinessFlow.Activities.RemoveAt(activityIndex);
                            }
                        }
                    }
                }   
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to get the count of legacy actions", ex);
            }
        }

        /// <summary>
        /// This method is used to convert actions from multiple BusinessFlows
        /// </summary>
        /// <param name="addNewActivity"></param>
        /// <param name="listOfBusinessFlow"></param>
        /// <param name="actionsToBeConverted"></param>
        /// <param name="ConvertableTargetApplications"></param>
        /// <param name="convertToPOMAction"></param>
        /// <param name="SelectedPOMs"></param>
        public void ConvertActionsOfMultipleBusinessFlows(ObservableList<ConvertableActionDetails> actionsToBeConverted,
                                                          bool addNewActivity,
                                                          ObservableList<ConvertableTargetApplicationDetails> convertableTargetApplications,
                                                          bool convertToPOMAction = false, ObservableList<Guid> selectedPOMs = null)
        {
            try
            {
                Parallel.ForEach(ListOfBusinessFlowsToConvert, (bfToConvert, state) =>
                {
                    if (!mStopConversion)
                    {
                        if (bfToConvert.ConversionStatus != eConversionStatus.Running && bfToConvert.ConversionStatus != eConversionStatus.Finish)
                        {
                            bfToConvert.ConversionStatus = eConversionStatus.Running;
                            Reporter.ToStatus(eStatusMsgKey.BusinessFlowConversion, null, bfToConvert.BusinessFlow.Name);
                            ConvertToActions(bfToConvert, addNewActivity, actionsToBeConverted, convertableTargetApplications, convertToPOMAction, selectedPOMs);
                            if (bfToConvert.ConversionStatus != eConversionStatus.Stopped && bfToConvert.ConversionStatus != eConversionStatus.NA)
                            {
                                bfToConvert.ConversionStatus = eConversionStatus.Finish;
                            }
                        }
                    }
                    else
                    {
                        state.Stop();
                    }
                });
                Reporter.HideStatusMessage();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert action", ex);
            }
        }

        /// <summary>
        /// This method is used to add the actions
        /// </summary>
        /// <param name="addNewActivity"></param>
        public void ConvertToActions(BusinessFlowToConvert businessFlowToConvert,
                                     bool addNewActivity, ObservableList<ConvertableActionDetails> actionsToBeConverted,
                                     ObservableList<ConvertableTargetApplicationDetails> convertableTargetApplications,
                                     bool convertToPOMAction = false, ObservableList<Guid> selectedPOMObjectName = null)
        {
            try
            {
                int activityIndex = 0;
                businessFlowToConvert.ConvertedActionsCount = 0;
                for (; activityIndex < businessFlowToConvert.BusinessFlow.Activities.Count(); activityIndex++)
                {
                    if (!mStopConversion)
                    {
                        if (businessFlowToConvert.TotalProcessingActionsCount > 0)
                        {
                            Activity activity = businessFlowToConvert.BusinessFlow.Activities[activityIndex];
                            if (activity != null && activity.Active && activity.SelectedForConversion && activity.Acts.OfType<IObsoleteAction>().ToList().Count > 0)
                            {
                                businessFlowToConvert.BusinessFlow.StartDirtyTracking();
                                Activity currentActivity = GetCurrentWorkingActivity(businessFlowToConvert.BusinessFlow, addNewActivity, ref activityIndex, activity);
                                ConvertSelectedActionsFromActivity(businessFlowToConvert, actionsToBeConverted, addNewActivity, convertToPOMAction, selectedPOMObjectName, currentActivity);

                                currentActivity.TargetApplication = convertableTargetApplications.Where(x => x.SourceTargetApplicationName == activity.TargetApplication).Select(x => x.TargetTargetApplicationName).FirstOrDefault();
                            } 
                        }
                    }
                    else
                    {
                        businessFlowToConvert.ConversionStatus = eConversionStatus.Stopped;
                        break;
                    }
                }
                if (businessFlowToConvert.ConvertedActionsCount == 0)
                {
                    businessFlowToConvert.ConversionStatus = eConversionStatus.NA;
                    businessFlowToConvert.SaveStatus = eConversionSaveStatus.NA;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert action", ex);
            }
        }

        /// <summary>
        /// This method is used to convert the selected actions from the activity
        /// </summary>
        /// <param name="addNewActivity"></param>
        /// <param name="actionsToBeConverted"></param>
        /// <param name="convertToPOMAction"></param>
        /// <param name="selectedPOMObjectName"></param>
        /// <param name="currentActivity"></param>
        private void ConvertSelectedActionsFromActivity(BusinessFlowToConvert businessFlowToConvert, ObservableList<ConvertableActionDetails> actionsToBeConverted, bool addNewActivity,
                                                        bool convertToPOMAction, ObservableList<Guid> selectedPOMObjectName, Activity currentActivity)
        {
            int actionIndex = 0;
            for (; actionIndex < currentActivity.Acts.Count(); actionIndex++)
            {
                Act act = (Act)currentActivity.Acts[actionIndex];
                if (!mStopConversion)
                {
                    try
                    {
                        ePlatformType activityPlatform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == currentActivity.TargetApplication select x.Platform).FirstOrDefault();
                        if (act.Active && act is IObsoleteAction 
                                       && (((IObsoleteAction)act).IsObsoleteForPlatform(activityPlatform)) 
                                       && actionsToBeConverted.Where(a => a.SourceActionType == act.GetType() &&
                                                                          a.Selected &&
                                                                          a.TargetActionType == ((IObsoleteAction)act).TargetAction()).FirstOrDefault() != null)
                        {
                            // get the index of the action that is being converted 
                            int selectedActIndex = currentActivity.Acts.IndexOf(act);

                            // convert the old action
                            Act newAct = ((IObsoleteAction)act).GetNewAction();
                            if (newAct != null)
                            {
                                newAct.Platform = ((IObsoleteAction)act).GetTargetPlatform();
                                newAct.Description = string.Format("New - {0}", newAct.Description);
                                if (convertToPOMAction && newAct.GetType().Name == ActUIElementClassName)
                                {
                                    bool isFound = false;
                                    foreach (Guid pomOj in selectedPOMObjectName)
                                    {
                                        if (!isFound)
                                        {
                                            newAct = GetMappedElementFromPOMForAction(newAct, pomOj, ref isFound);
                                        }
                                        else if (isFound)
                                        {
                                            break;
                                        }
                                    }
                                }
                                currentActivity.Acts.Insert(selectedActIndex + 1, newAct);

                                // set obsolete action in the activity as inactive
                                act.Active = false;
                                if (addNewActivity)
                                {
                                    currentActivity.Acts.Remove(act);
                                }
                                businessFlowToConvert.ConvertedActionsCount++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert action", ex);
                    }
                }
                else
                {
                    break;
                }
            }
        }
        
        /// <summary>
        /// This method will get the activity by checking the flag whether to create new or use existing activity
        /// </summary>
        /// <param name="addNewActivity"></param>
        /// <param name="businessFlow"></param>
        /// <param name="intIndex"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        private static Activity GetCurrentWorkingActivity(BusinessFlow businessFlow, bool addNewActivity, ref int intIndex, Activity activity)
        {
            Activity currentActivity;
            if (addNewActivity)
            {
                currentActivity = (Activity)activity.CreateCopy(false);
                currentActivity.ActivityName = "New - " + activity.ActivityName;
                businessFlow.Activities.Insert(intIndex + 1, currentActivity);
                activity.Active = false;
                intIndex++;
            }
            else
            {
                currentActivity = activity;
            }

            return currentActivity;
        }

        /// <summary>
        /// This method is used to find the relative element from POM for the existing action
        /// </summary>
        /// <param name="newActUIElement"></param>
        /// <param name="pomModelObject"></param>
        /// <param name="elementMatchedInPOM"></param>
        /// <returns></returns>
        public Act GetMappedElementFromPOMForAction(Act newActUIElement, Guid pomModelObject, ref bool elementMatchedInPOM)
        {
            try
            {
                elementMatchedInPOM = false;
                if (pomModelObject != default(Guid))
                {
                    ObservableList<ApplicationPOMModel> pomLst = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>();
                    ApplicationPOMModel selectedPOM = pomLst.Where(x => x.Guid == pomModelObject).SingleOrDefault();

                    ElementInfo elementInfo = null;
                    string locateValue = Convert.ToString(newActUIElement.GetType().GetProperty(ActUIElementLocateValueField).GetValue(newActUIElement, null));
                    eLocateBy elementLocateBy = GeteLocateByEnumItem(Convert.ToString(newActUIElement.GetType().GetProperty(ActUIElementElementLocateByField).GetValue(newActUIElement, null)));

                    var matchingExistingLocator = selectedPOM.MappedUIElements.Where(x => x.Locators.Any(y => y.LocateBy == elementLocateBy && y.LocateValue.ToLower().Trim().Equals(locateValue.ToLower().Trim())));
                    if (matchingExistingLocator != null)
                    {
                        elementInfo = matchingExistingLocator.FirstOrDefault();
                        if (elementInfo != null)
                        {
                            elementMatchedInPOM = true;
                            PropertyInfo pLocateBy = newActUIElement.GetType().GetProperty(ActUIElementElementLocateByField);
                            if (pLocateBy != null)
                            {
                                if (pLocateBy.PropertyType.IsEnum)
                                {
                                    pLocateBy.SetValue(newActUIElement, Enum.Parse(pLocateBy.PropertyType, "POMElement"));
                                }
                            }

                            PropertyInfo pLocateVal = newActUIElement.GetType().GetProperty(ActUIElementElementLocateValueField);
                            if (pLocateVal != null)
                            {
                                pLocateVal.SetValue(newActUIElement, string.Format("{0}_{1}", selectedPOM.Guid.ToString(), elementInfo.Guid.ToString()));
                            }

                            PropertyInfo pElementType = newActUIElement.GetType().GetProperty(ActUIElementElementTypeField);
                            if (pElementType != null && pElementType.PropertyType.IsEnum)
                            {
                                pElementType.SetValue(newActUIElement, Enum.Parse(pElementType.PropertyType, Convert.ToString(elementInfo.ElementTypeEnum)));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to Get Mapped Element From POM", ex);
            }
            return newActUIElement;
        }

        /// <summary>
        /// This method will get the eLocateBy
        /// </summary>
        /// <returns></returns>
        private eLocateBy GeteLocateByEnumItem(string sEnum)
        {
            eLocateBy item = (eLocateBy)System.Enum.Parse(typeof(eLocateBy), sEnum);
            return item;
        }

        /// <summary>
        /// This method will give the list of convertable action 
        /// </summary>
        /// <param name="lstSelectedActivities"></param>
        /// <returns></returns>
        public ObservableList<ConvertableActionDetails> GetConvertableActivityActions(List<Activity> lstSelectedActivities)
        {
            ObservableList<ConvertableActionDetails> lst = new ObservableList<ConvertableActionDetails>();
            try
            {
                foreach (Activity convertibleActivity in lstSelectedActivities)
                {
                    ePlatformType activityPlatform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == convertibleActivity.TargetApplication select x.Platform).FirstOrDefault();
                    if (convertibleActivity.Active)
                    {
                        foreach (Act act in convertibleActivity.Acts)
                        {
                            act.Platform = activityPlatform;
                            if ((act is IObsoleteAction) && (((IObsoleteAction)act).IsObsoleteForPlatform(act.Platform)) &&
                                                    (act.Active) && ((IObsoleteAction)act).TargetActionTypeName() != null)
                            {
                                ConvertableActionDetails existingConvertibleActionType = lst.Where(x => x.SourceActionType == act.GetType() && x.TargetActionTypeName == ((IObsoleteAction)act).TargetActionTypeName()).FirstOrDefault();
                                if (existingConvertibleActionType == null)
                                {
                                    ConvertableActionDetails newConvertibleActionType = new ConvertableActionDetails();
                                    newConvertibleActionType.SourceActionTypeName = act.ActionDescription.ToString();
                                    newConvertibleActionType.SourceActionType = act.GetType();
                                    newConvertibleActionType.TargetActionType = ((IObsoleteAction)act).TargetAction();
                                    if (newConvertibleActionType.TargetActionType == null)
                                        continue;
                                    newConvertibleActionType.TargetActionTypeName = ((IObsoleteAction)act).TargetActionTypeName();
                                    newConvertibleActionType.ActionCount = 1;
                                    newConvertibleActionType.Actions.Add(act);
                                    newConvertibleActionType.ActivityList.Add(convertibleActivity.ActivityName);
                                    lst.Add(newConvertibleActionType);
                                }
                                else
                                {
                                    if (!existingConvertibleActionType.Actions.Contains(act))
                                    {
                                        existingConvertibleActionType.ActionCount++;
                                        existingConvertibleActionType.Actions.Add(act);
                                        existingConvertibleActionType.ActivityList.Add(convertibleActivity.ActivityName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to Getting convertible actions from activities", ex);
            }
            return lst;
        }

        /// <summary>
        /// This method will give the list of convertable activities
        /// </summary>
        /// <param name="businessFlow"></param>
        /// <returns></returns>
        public ObservableList<Activity> GetConvertableActivitiesFromBusinessFlow(BusinessFlow businessFlow)
        {
            ObservableList<Activity> lst = new ObservableList<Activity>();
            try
            {
                foreach (Activity convertibleActivity in businessFlow.Activities)
                {
                    if (convertibleActivity.Active)
                    {
                        bool isPresent = false;
                        ePlatformType activityPlatform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == convertibleActivity.TargetApplication select x.Platform).FirstOrDefault();
                        foreach (Act act in convertibleActivity.Acts)
                        {
                            act.Platform = activityPlatform;
                            if ((act is IObsoleteAction) && (((IObsoleteAction)act).IsObsoleteForPlatform(act.Platform)) &&
                                (act.Active) && ((IObsoleteAction)act).TargetActionTypeName() != null)
                            {
                                isPresent = true;
                                break;
                            }
                        }
                        if (isPresent)
                        {
                            lst.Add(convertibleActivity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to Getting convertible actions from activities", ex);
            }
            return lst;
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
                    ePlatformType activityPlatform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == activity.TargetApplication select x.Platform).FirstOrDefault();
                    count = count + activity.Acts.Where(act => (act.Active &&
                                                               (act is IObsoleteAction) && (((IObsoleteAction)act).IsObsoleteForPlatform(activityPlatform)) &&
                                                               (((IObsoleteAction)act).TargetAction()) != null)).Count();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to get the count of legacy actions", ex);
            }
            return count;
        }
    }
}
