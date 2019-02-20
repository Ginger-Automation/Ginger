#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Repository;
using Ginger.SolutionGeneral;
using Ginger.WizardLib;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.ActionConversion;
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Ginger.Actions.ActionConversion
{
    public class ActionsConversionWizard : WizardBase
    {
        public override string Title { get { return "Actions Conversion Wizard"; } }
        public BusinessFlow BusinessFlow;
        public Solution Solution;
        public ObservableList<ActionConversionHandler> ActionToBeConverted = new ObservableList<ActionConversionHandler>();

        public bool RadNewActivity { get; set; }

        public bool ChkDefaultTargetApp { get; set; }

        public string CmbTargetApp { get; set; }

        public bool ConvertToPOMAction { get; set; }

        public string SelectedPOMObjectName { get; set; }

        public ActionsConversionWizard(Solution solution, BusinessFlow businessFlow)
        {
            BusinessFlow = businessFlow;
            Solution = solution;

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Actions Conversion Introduction", Page: new WizardIntroPage("/Actions/ActionConversion/ActionConversionIntro.md"));

            AddPage(Name: "Select Activity for Conversion", Title: "Select Activity for Conversion", SubTitle: "Select Activity for Conversion", Page: new SelectActivityWzardPage());

            AddPage(Name: "Select Action for Conversion", Title: "Select Action for Conversion", SubTitle: "Select Action for Conversion", Page: new SelectActionWzardPage());

            AddPage(Name: "Conversion Configurations", Title: "Conversion Configurations", SubTitle: "Conversion Configurations", Page: new ConversionConfigurationWzardPage());
        }
        
        public override void Finish()
        {
            ExistingActionConversion();
        }

        private void ExistingActionConversion()
        {
            if (!DoExistingPlatformCheck(ActionToBeConverted))
            {
                //missing target application so stop the conversion
                return;
            }
            else
            {
                try
                {
                    Reporter.ToStatus(eStatusMsgKey.BusinessFlowConversion, null, BusinessFlow.Name);

                    // create a new converted activity
                    if (RadNewActivity)
                    {
                        Activity newActivity = new Activity() { Active = true };
                        foreach (Activity oldActivity in BusinessFlow.Activities.ToList())
                        {
                            // check if the activity is selected for conversion and it contains actions that are obsolete (of type, IObsolete)
                            if (oldActivity.SelectedForConversion && oldActivity.Acts.OfType<IObsoleteAction>().ToList().Count > 0)
                            {
                                newActivity = (Activity)oldActivity.CreateCopy(false);
                                newActivity.ActivityName = "New - " + oldActivity.ActivityName;
                                BusinessFlow.Activities.Add(newActivity);
                                BusinessFlow.Activities.Move(BusinessFlow.Activities.Count() - 1, BusinessFlow.Activities.IndexOf(oldActivity) + 1);

                                foreach (Act oldAct in oldActivity.Acts.ToList())
                                {
                                    if (oldAct is IObsoleteAction && ActionToBeConverted.Where(act => act.SourceActionType == oldAct.GetType() && act.Selected && act.TargetActionType == ((IObsoleteAction)oldAct).TargetAction()).FirstOrDefault() != null)
                                    {                                        
                                        // convert the old action
                                        Act newAct = ((IObsoleteAction)oldAct).GetNewAction();
                                        if (ConvertToPOMAction && newAct.GetType().Name == typeof(ActUIElement).Name)
                                        {
                                            ActionConversionUtils.Instance.GetMappedElementFromPOMForAction(newAct, SelectedPOMObjectName);
                                        }
                                        int oldActionIndex = newActivity.Acts.IndexOf(newActivity.Acts.Where(x => x.Guid == oldAct.Guid).FirstOrDefault());
                                        newActivity.Acts.RemoveAt(oldActionIndex);
                                        newActivity.Acts.Add(newAct);
                                        newActivity.Acts.Move(newActivity.Acts.Count() - 1, oldActionIndex);
                                    }
                                }

                                // check if the old activity was active or not and accordingly set Active field for the new activity
                                if (!oldActivity.Active)
                                {
                                    newActivity.Active = false;
                                }
                                else
                                    newActivity.Active = true;

                                // by default, set the old activity as inactive
                                oldActivity.Active = false;

                                // if the user has not chosen any target application in the combobox then, we set it as empty
                                if (ChkDefaultTargetApp && !string.IsNullOrEmpty(CmbTargetApp))
                                {
                                    newActivity.TargetApplication = CmbTargetApp;
                                }
                                else
                                {
                                    newActivity.TargetApplication = string.Empty;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (Activity activity in BusinessFlow.Activities)
                        {
                            if (activity.SelectedForConversion && activity.Acts.OfType<IObsoleteAction>().ToList().Count > 0)
                            {

                                foreach (Act act in activity.Acts.ToList())
                                {
                                    if (act.Active && act is IObsoleteAction && ActionToBeConverted.Where(a => a.SourceActionType == act.GetType() && a.Selected && a.TargetActionType == ((IObsoleteAction)act).TargetAction()).FirstOrDefault() != null)
                                    {
                                        // get the index of the action that is being converted 
                                        int selectedActIndex = activity.Acts.IndexOf(act);

                                        // convert the old action
                                        Act newAct = ((IObsoleteAction)act).GetNewAction();                                        
                                        if (ConvertToPOMAction && newAct.GetType().Name == typeof(ActUIElement).Name)
                                        {
                                            ActionConversionUtils.Instance.GetMappedElementFromPOMForAction(newAct, SelectedPOMObjectName);
                                        }
                                        activity.Acts.Add(newAct);
                                        if (selectedActIndex >= 0)
                                        {
                                            activity.Acts.Move(activity.Acts.Count - 1, selectedActIndex + 1);
                                        }

                                        // set obsolete action in the activity as inactive
                                        act.Active = false;
                                    }
                                }

                                // if the user has not chosen any target application in the combobox then, we set it as empty
                                if (ChkDefaultTargetApp && !string.IsNullOrEmpty(CmbTargetApp))
                                {
                                    activity.TargetApplication = CmbTargetApp;
                                }
                                else
                                { activity.TargetApplication = string.Empty; }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " - ", ex);
                    Reporter.ToUser(eUserMsgKey.ActivitiesConversionFailed);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        public override void Cancel()
        {
            base.Cancel();
        }

        private bool DoExistingPlatformCheck(ObservableList<ActionConversionHandler> lstActionToBeConverted)
        {
            // fetch list of existing platforms in the business flow
            List<ePlatformType> lstExistingPlatform = Solution.ApplicationPlatforms
                                                      .Where(x => BusinessFlow.TargetApplications
                                                      .Any(a => a.Name == x.AppName))
                                                      .Select(x => x.Platform).ToList();

            Dictionary<ePlatformType, string> lstMissingPlatform = new Dictionary<ePlatformType, string>();
            // create list of missing platforms
            foreach (ActionConversionHandler ACH in lstActionToBeConverted)
            {
                if (ACH.Selected && !lstExistingPlatform.Contains(ACH.TargetPlatform)
                    && !lstMissingPlatform.ContainsKey(ACH.TargetPlatform))
                {
                    lstMissingPlatform.Add(ACH.TargetPlatform, ACH.TargetActionTypeName);
                }
            }

            // if there are any missing platforms
            if (lstMissingPlatform.Count > 0)
            {
                foreach (var item in lstMissingPlatform)
                {
                    // ask the user if he wants to continue with the conversion, if there are missing target platforms
                    if (Reporter.ToUser(eUserMsgKey.MissingTargetPlatformForConversion, item.Value, item.Key) == Amdocs.Ginger.Common.eUserMsgSelection.No)
                        return false;
                }
            }
            return true;
        }
    }
}
