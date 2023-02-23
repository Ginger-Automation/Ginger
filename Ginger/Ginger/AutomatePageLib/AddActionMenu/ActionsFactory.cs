#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Amdocs.Ginger.Common.Repository.TargetLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.ApiModelsFolder;
using Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.PlugIns;
using GingerCore.Activities;
using GingerCore.Drivers.Common;
using GingerCore.Environments;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace Ginger.BusinessFlowPages
{
    /// <summary>
    /// Holds static functions for generating any Action
    /// Just call AddActionsHandler with object and Context as it's parameters 
    /// to generate the relevant action and add it to the current selected Activity
    /// </summary>
    class ActionsFactory
    {
        /// <summary>
        /// Generates relevant Action and adds to the current/selected Activity
        /// </summary>
        /// <param name="mItem"> of type object and would successfully add an action with Act/ElementInfo/ApplicationModels type object is provided</param>
        /// <param name="mContext"> required to identify the currently selected Activity, Action is to be added to </param>
        public static int AddActionsHandler(object mItem, Context mContext, int targetIndex = -1)
        {
            Act instance = null;
            ePlatformType currentActivityPlatform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == mContext.Activity.TargetApplication select x.Platform).FirstOrDefault();

            if (mContext.Activity != null)
            {
                mContext.BusinessFlow.CurrentActivity = mContext.Activity;//so new Actions will be added to correct Activity
            }
            if (!mContext.Activity.EnableEdit && mContext.Activity.IsLinkedItem)
            {
                Reporter.ToUser(eUserMsgKey.EditLinkSharedActivities);
                return -1;
            }
            if (mItem is Act)
            {
                Act selectedAction = mItem as Act;
                if (!IsValidActionPlatformForActivity(selectedAction, mContext))
                {
                    Reporter.ToUser(eUserMsgKey.MissingTargetApplication, "Activity target platform is \"" + currentActivityPlatform + "\", where as action platform is \"" + selectedAction.Platform + "\"" + System.Environment.NewLine + "Please select same platform actions only.");
                    return -1;
                }
                if (!(selectedAction is ActWithoutDriver))
                {
                    selectedAction.Platform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == mContext.Activity.TargetApplication select x.Platform).FirstOrDefault();
                }
                instance = GenerateSelectedAction(selectedAction, mContext);
            }
            else if (mItem is ElementInfo)
            {
                ElementInfo elementInfo = mItem as ElementInfo;
                instance = GeneratePOMElementRelatedAction(elementInfo, mContext);
            }
            else if (mItem is ApplicationPOMModel)
            {
                ApplicationPOMModel currentPOM = mItem as ApplicationPOMModel;
                //required to show all the actions added as selected
                int updatedTargetIndex = targetIndex;
                foreach (ElementInfo elemInfo in currentPOM.MappedUIElements)
                {
                    instance = GeneratePOMElementRelatedAction(elemInfo, mContext);
                    if (instance != null)
                    {
                        instance.Active = true;
                        if (updatedTargetIndex > -1)
                        {
                            mContext.Activity.Acts.Insert(updatedTargetIndex, instance);
                            updatedTargetIndex++;
                        }
                        else
                        {
                            mContext.BusinessFlow.AddAct(instance, true);
                        }
                    }
                }
                mContext.Activity.Acts.CurrentItem = instance;
                instance = null;
                targetIndex = updatedTargetIndex;
            }
            else if (mItem is ApplicationAPIModel || mItem is RepositoryFolder<ApplicationAPIModel>)
            {
                ObservableList<ApplicationAPIModel> apiModelsList = new ObservableList<ApplicationAPIModel>();
                if (mItem is RepositoryFolder<ApplicationAPIModel>)
                {
                    apiModelsList = (mItem as RepositoryFolder<ApplicationAPIModel>).GetFolderItems();
                    apiModelsList = new ObservableList<ApplicationAPIModel>(apiModelsList.Where(a => a.TargetApplicationKey != null && Convert.ToString(a.TargetApplicationKey.ItemName) == mContext.Target.ItemName));
                }
                else
                {
                    apiModelsList.Add(mItem as ApplicationAPIModel);
                }

                AddApiModelActionWizardPage APIModelWizPage = new AddApiModelActionWizardPage(mContext, apiModelsList);
                WizardWindow.ShowWizard(APIModelWizPage);
            }

            if (instance != null)
            {
                instance.Active = true;
                if (instance is ActWithoutDriver)
                {
                    instance.Platform = ePlatformType.NA;
                }
                else
                {
                    instance.Platform = currentActivityPlatform;
                }

                if (targetIndex > -1)
                {
                    mContext.Activity.Acts.Insert(targetIndex, instance);
                }
                else
                {
                    mContext.BusinessFlow.AddAct(instance, true);
                }
                mContext.Activity.Acts.CurrentItem = instance;
            }

            return targetIndex;
        }

        static bool ItemFromSharedRepository(Act thisAct)
        {
            return WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>().Any(a => a == thisAct);
        }

        /// <summary>
        /// In case object of type Act was passed, adds relevant action being Legacy/PlugIn/One that Adds via Wizard
        /// </summary>
        /// <param name="selectedAction"></param>
        /// <param name="mContext"></param>
        /// <returns></returns>
        static Act GenerateSelectedAction(Act selectedAction, Context mContext)
        {
            Act instance = null;

            if (ItemFromSharedRepository(selectedAction))
            {
                instance = (Act)selectedAction.CreateInstance(true);
            }
            else if (selectedAction.AddActionWizardPage != null)
            {
                string classname = selectedAction.AddActionWizardPage;
                Type t = System.Reflection.Assembly.GetExecutingAssembly().GetType(classname);
                if (t == null)
                {
                    throw new Exception("Action edit page not found - " + classname);
                }

                WizardBase wizard = (WizardBase)Activator.CreateInstance(t, mContext);
                WizardWindow.ShowWizard(wizard);

                return null;
            }
            else
            {
                instance = (Act)selectedAction.CreateCopy();
                if (selectedAction is IObsoleteAction && (selectedAction as IObsoleteAction).IsObsoleteForPlatform(mContext.Platform))
                {
                    eUserMsgSelection userSelection;
                    if (((IObsoleteAction)selectedAction).GetNewAction() == null)
                    {
                        userSelection = Reporter.ToUser(eUserMsgKey.WarnAddLegacyAction, ((IObsoleteAction)selectedAction).TargetActionTypeName());
                        if (userSelection == eUserMsgSelection.No)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        userSelection = Reporter.ToUser(eUserMsgKey.WarnAddLegacyActionAndOfferNew, ((IObsoleteAction)selectedAction).TargetActionTypeName());

                        if (userSelection == eUserMsgSelection.Yes)
                        {
                            if (selectedAction.Platform == ePlatformType.NA)
                            {
                                selectedAction.Platform = mContext.Platform;
                            }
                            instance = ((IObsoleteAction)selectedAction).GetNewAction();
                            instance.Description = instance.ActionType;
                        }
                        else if (userSelection == eUserMsgSelection.Cancel)
                        {
                            return null;            //do not add any action
                        }
                    }
                }

                for (int i = 0; i < selectedAction.InputValues.Count; i++)
                {
                    instance.InputValues[i].ParamTypeEX = selectedAction.InputValues[i].ParamTypeEX;
                }

                instance.SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();

                if (instance is ActPlugIn)
                {
                    ActPlugIn p = (ActPlugIn)instance;
                    // TODO: add per group or... !!!!!!!!!

                    //Check if target already exist else add it
                    // TODO: search only in targetplugin type
                    TargetPlugin targetPlugin = (TargetPlugin)(from x in mContext.BusinessFlow.TargetApplications where x.Name == p.ServiceId select x).SingleOrDefault();
                    if (targetPlugin == null)
                    {
                        // check if interface add it
                        // App.BusinessFlow.TargetApplications.Add(new TargetPlugin() { AppName = p.ServiceId });

                        mContext.BusinessFlow.TargetApplications.Add(new TargetPlugin() { PluginId = p.PluginId, ServiceId = p.ServiceId });

                        //Search for default agent which match 
                        mContext.Runner.UpdateApplicationAgents();
                        // TODO: update automate page target/agent

                        // if agent not found auto add or ask user 
                    }
                }
            }

            return instance;
        }

        /// <summary>
        /// Just provide the ElementInfo Object and would generate supported Action for the same according to the current selected Platform
        /// </summary>
        /// <param name="elementInfo"></param>
        /// <param name="mContext"></param>
        /// <returns></returns>
        public static Act GeneratePOMElementRelatedAction(ElementInfo elementInfo, Context mContext)
        {
            Act instance;
            IPlatformInfo mPlatform = PlatformInfoBase.GetPlatformImpl(mContext.Platform);
            string elementVal = string.Empty;
            if (elementInfo.OptionalValuesObjectsList.Count > 0)
            {
                elementVal = Convert.ToString(elementInfo.OptionalValuesObjectsList.Where(v => v.IsDefault).FirstOrDefault().Value);
            }

            ElementActionCongifuration actionConfigurations = new ElementActionCongifuration
            {
                LocateBy = eLocateBy.POMElement,
                LocateValue = elementInfo.ParentGuid.ToString() + "_" + elementInfo.Guid.ToString(),
                ElementValue = elementVal,
                AddPOMToAction = true,
                POMGuid = elementInfo.ParentGuid.ToString(),
                ElementGuid = elementInfo.Guid.ToString(),
                LearnedElementInfo = elementInfo,
                Type = elementInfo.ElementTypeEnum
            };

            instance = mPlatform.GetPlatformAction(elementInfo, actionConfigurations);
            return instance;
        }

        /// <summary>
        /// Adding Activities from Shared Repository to the Business Flow in Context
        /// </summary>
        /// <param name="sharedActivitiesToAdd">Shared Repository Activities to Add Instances from</param>
        /// <param name="businessFlow">Business Flow to add to</param>
        public static void AddActivitiesFromSRHandler(List<Activity> sharedActivitiesToAdd, BusinessFlow businessFlow, string ActivitiesGroupID = null, int insertIndex = -1, bool IsPomActivity = false)
        {
            ActivitiesGroup parentGroup = null;
            bool copyAsLink = true;
            if (!string.IsNullOrWhiteSpace(ActivitiesGroupID))
            {
                parentGroup = businessFlow.ActivitiesGroups.Where(g => g.Name == ActivitiesGroupID).FirstOrDefault();
            }

            var activitiesGroupSelectionPage = new ActivitiesGroupSelectionPage(businessFlow, parentGroup, IsPomActivity);
            parentGroup = activitiesGroupSelectionPage.ShowAsWindow();
            if (!IsPomActivity)
            {
                copyAsLink = activitiesGroupSelectionPage.xLinkedInstance.IsChecked.Value;
            }

            if (parentGroup != null)
            {
                eUserMsgSelection userSelection = eUserMsgSelection.None;
                foreach (Activity sharedActivity in sharedActivitiesToAdd)
                {
                    Activity activityIns = null;
                    if (!IsPomActivity)
                    {
                        activityIns = (Activity)sharedActivity.CreateInstance(true);
                        if (copyAsLink)
                        {
                            activityIns.Type = eSharedItemType.Link;
                        }
                    }
                    else
                    {
                        activityIns = (Activity)sharedActivity.CreateInstance(false);
                        activityIns.IsAutoLearned = true;
                    }
                    activityIns.Active = true;

                    //map activities target application to BF if missing in BF
                    userSelection = businessFlow.MapTAToBF(userSelection, activityIns, WorkSpace.Instance.Solution.ApplicationPlatforms);
                    businessFlow.SetActivityTargetApplication(activityIns);
                    if(insertIndex >= 0)
                    {
                        businessFlow.ActivitiesGroups.Move(businessFlow.ActivitiesGroups.IndexOf(parentGroup), insertIndex);
                    }
                    businessFlow.AddActivity(activityIns, parentGroup, insertIndex);


                }
            }
        }


        /// <summary>
        /// Adding Activities Groups from Shared Repository to the Business Flow in Context
        /// </summary>
        /// <param name="sharedActivitiesGroupsToAdd">Shared Repository Activities Groups to Add Instances from</param>
        /// <param name="businessFlow">Business Flow to add to</param>
        public static void AddActivitiesGroupsFromSRHandler(List<ActivitiesGroup> sharedActivitiesGroupsToAdd, BusinessFlow businessFlow)
        {
            foreach (ActivitiesGroup sharedGroup in sharedActivitiesGroupsToAdd)
            {
                ActivitiesGroup droppedGroupIns = (ActivitiesGroup)sharedGroup.CreateInstance(true);
                businessFlow.AddActivitiesGroup(droppedGroupIns);
                ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                businessFlow.ImportActivitiesGroupActivitiesFromRepository(droppedGroupIns, activities, WorkSpace.Instance.Solution.ApplicationPlatforms, false);
            }
            businessFlow.AttachActivitiesGroupsAndActivities();
        }

        public static Page GetActionEditPage(Act act, Context mContext = null)
        {
            if (act.ActionEditPage != null)
            {
                string classname = "Ginger.Actions." + act.ActionEditPage;

                Type actType = Assembly.GetExecutingAssembly().GetType(classname);
                if (actType == null)
                {
                    throw new Exception("Action edit page not found - " + classname);
                }

                Page actEditPage = (Page)Activator.CreateInstance(actType, act);
                if (actEditPage != null)
                {
                    // For no driver actions we give the BF and env - used for example in set var value.
                    if (typeof(ActWithoutDriver).IsAssignableFrom(act.GetType()))
                    {
                        if (mContext != null && mContext.Runner != null)
                        {
                            ((ActWithoutDriver)act).RunOnBusinessFlow = (BusinessFlow)mContext.Runner.CurrentBusinessFlow;
                            ((ActWithoutDriver)act).RunOnEnvironment = (ProjEnvironment)((GingerExecutionEngine)mContext.Runner).GingerRunner.ProjEnvironment;
                            ((ActWithoutDriver)act).DSList = ((GingerExecutionEngine)mContext.Runner).GingerRunner.DSList;
                        }
                    }

                    return actEditPage;
                }
            }

            return null;
        }
        /// <summary>
        /// Used to check action platform is the same as activity or NA
        /// </summary>
        /// <param name="mItem">Object(act) to check</param>
        /// <param name="context">Current context</param>
        /// <returns></returns>
        public static bool IsValidActionPlatformForActivity(object repoItem, Context context)
        {
            if (repoItem is Act)
            {
                Act selectedAction = repoItem as Act;
                ePlatformType currentActivityPlatform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == context.Activity.TargetApplication select x.Platform).FirstOrDefault();
                if (currentActivityPlatform != selectedAction.Platform && selectedAction.Platform != ePlatformType.NA)
                {
                    return false;
                }
            }
            return true;
        }
    }
}