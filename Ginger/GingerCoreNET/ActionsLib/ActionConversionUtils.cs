using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Amdocs.Ginger.CoreNET
{
    /// <summary>
    /// This class is used to add methods for action conversion helpers
    /// </summary>
    public class ActionConversionUtils
    {
        /// <summary>
        /// private constructor
        /// </summary>
        public ActionConversionUtils()
        {
        }

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

        /// <summary>
        /// This method is used to add the actions
        /// </summary>
        /// <param name="addNewActivity"></param>
        public void ConvertToActions(bool addNewActivity, BusinessFlow businessFlow, 
                                     ObservableList<ActionConversionHandler> actionsToBeConverted,
                                     bool isDefaultTargetApp, string strTargetApp,
                                     bool convertToPOMAction = false, string selectedPOMObjectName = "")
        {
            try
            {
                foreach (Activity activity in businessFlow.Activities)
                {
                    if (activity.SelectedForConversion && activity.Acts.OfType<IObsoleteAction>().ToList().Count > 0)
                    {
                        Activity currentActivity;
                        if (addNewActivity)
                        {
                            currentActivity = new Activity() { Active = true };
                            currentActivity = (Activity)activity.CreateCopy(false);
                            currentActivity.ActivityName = "New - " + activity.ActivityName;
                            businessFlow.Activities.Insert(businessFlow.Activities.IndexOf(activity) + 1, currentActivity);
                            activity.Active = false;
                        }
                        else
                        {
                            currentActivity = activity;
                        }
                        foreach (Act act in currentActivity.Acts.ToList())
                        {
                            if (act.Active && act is IObsoleteAction &&
                                actionsToBeConverted.Where(a => a.SourceActionType == act.GetType() && 
                                                          a.Selected && 
                                                          a.TargetActionType == ((IObsoleteAction)act).TargetAction()).FirstOrDefault() != null)
                            {
                                // get the index of the action that is being converted 
                                int selectedActIndex = currentActivity.Acts.IndexOf(act);

                                // convert the old action
                                Act newAct = ((IObsoleteAction)act).GetNewAction();
                                if (convertToPOMAction && newAct.GetType().Name == ActUIElementClassName)
                                {
                                    newAct = GetMappedElementFromPOMForAction(newAct, selectedPOMObjectName);
                                }
                                currentActivity.Acts.Insert(selectedActIndex + 1, newAct);

                                // set obsolete action in the activity as inactive
                                act.Active = false;
                            }
                        }

                        // if the user has not chosen any target application in the combobox then, we set it as empty
                        if (isDefaultTargetApp && !string.IsNullOrEmpty(strTargetApp))
                        {
                            currentActivity.TargetApplication = strTargetApp;
                        }
                        else
                        {
                            currentActivity.TargetApplication = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert action", ex);
            }
        }
        
        /// <summary>
        /// This method is used to find the relative element from POM for the existing action
        /// </summary>
        /// <param name="newActUIElement"></param>
        /// <param name="pomModelObject"></param>
        /// <returns></returns>
        public Act GetMappedElementFromPOMForAction(Act newActUIElement, string pomModelObject)
        {
            ObservableList<ApplicationPOMModel> pomLst = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>();
            ApplicationPOMModel selectedPOM = pomLst.Where(x => x.Guid.ToString() == pomModelObject).SingleOrDefault();

            ElementInfo elementInfo = null;
            string locateValue = Convert.ToString(newActUIElement.GetType().GetProperty(ActUIElementLocateValueField).GetValue(newActUIElement, null));
            eLocateBy elementLocateBy = GeteLocateByEnumItem(Convert.ToString(newActUIElement.GetType().GetProperty(ActUIElementElementLocateByField).GetValue(newActUIElement, null)));

            var matchingExistingLocator = selectedPOM.MappedUIElements.Where(x => x.Locators.Any(y => y.LocateBy == elementLocateBy && y.LocateValue == locateValue));
            if(matchingExistingLocator != null)
            {
                elementInfo = matchingExistingLocator.FirstOrDefault();
                if (elementInfo != null)
                {
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
    }
}
