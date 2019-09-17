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
using Amdocs.Ginger.Common.Repository.TargetLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.ApiModelsFolder;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.PlugIns;
using GingerCore.Activities;
using GingerCore.Drivers.Common;
using GingerCore.Platforms.PlatformsInfo;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;

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

            if (mContext.Activity != null)
            {
                mContext.BusinessFlow.CurrentActivity = mContext.Activity;//so new Actions will be added to correct Activity
            }

            if (mItem is Act)
            {
                Act selectedAction = mItem as Act;
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

        /// <summary>
        /// In case object of type Act was passed, adds relevant action being Legacy/PlugIn/One that Adds via Wizard
        /// </summary>
        /// <param name="selectedAction"></param>
        /// <param name="mContext"></param>
        /// <returns></returns>
        static Act GenerateSelectedAction(Act selectedAction, Context mContext)
        {
            if (selectedAction.AddActionWizardPage != null)
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
                Act instance = null;

                if (selectedAction.IsSharedRepositoryInstance || selectedAction.ContainingFolder.Contains("SharedRepository"))
                {
                    instance = (Act)selectedAction.CreateInstance(true);
                }
                else
                {
                    instance = (Act)selectedAction.CreateCopy();
                }

                if (selectedAction is IObsoleteAction && (selectedAction as IObsoleteAction).IsObsoleteForPlatform(mContext.Platform))
                {
                    eUserMsgSelection userSelection = Reporter.ToUser(eUserMsgKey.WarnAddLegacyActionAndOfferNew, ((IObsoleteAction)selectedAction).TargetActionTypeName());
                    if (userSelection == eUserMsgSelection.Yes)
                    {
                        instance = ((IObsoleteAction)selectedAction).GetNewAction();
                        instance.Description = instance.ActionType;
                    }
                    else if (userSelection == eUserMsgSelection.Cancel)
                    {
                        return null;            //do not add any action
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

                return instance;
            }
        }

        /// <summary>
        /// Just provide the ElementInfo Object and would generate supported Action for the same according to the current selected Platform
        /// </summary>
        /// <param name="elementInfo"></param>
        /// <param name="mContext"></param>
        /// <returns></returns>
        static Act GeneratePOMElementRelatedAction(ElementInfo elementInfo, Context mContext)
        {
            Act instance;
            IPlatformInfo mPlatform = PlatformInfoBase.GetPlatformImpl(mContext.Platform);
            string elementVal = string.Empty;
            if(elementInfo.OptionalValuesObjectsList.Count > 0)
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
            };

            instance = mPlatform.GetPlatformAction(elementInfo, actionConfigurations);
            return instance;
        }

        /// <summary>
        /// Adding Activities from Shared Repository to the Business Flow in Context
        /// </summary>
        /// <param name="sharedActivitiesToAdd">Shared Repository Activities to Add Instances from</param>
        /// <param name="businessFlow">Business Flow to add to</param>
        public static void AddActivitiesFromSRHandler(List<Activity> sharedActivitiesToAdd, BusinessFlow businessFlow, string ActivitiesGroupID = null, int insertIndex=-1)
        {
            ActivitiesGroup parentGroup = null;
            if (!string.IsNullOrWhiteSpace(ActivitiesGroupID))
            {
                parentGroup = businessFlow.ActivitiesGroups.Where(g => g.Name == ActivitiesGroupID).FirstOrDefault();
            }
            else
            {
                parentGroup = (new ActivitiesGroupSelectionPage(businessFlow)).ShowAsWindow();
            }

            if (parentGroup != null)
            {
                foreach (Activity sharedActivity in sharedActivitiesToAdd)
                {
                    Activity activityIns = (Activity)sharedActivity.CreateInstance(true);
                    activityIns.Active = true;
                    businessFlow.SetActivityTargetApplication(activityIns);                    
                    businessFlow.AddActivity(activityIns, parentGroup,insertIndex);
                    //mBusinessFlow.CurrentActivity = droppedActivityIns;
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
                businessFlow.ImportActivitiesGroupActivitiesFromRepository(droppedGroupIns, activities, false);
            }
            businessFlow.AttachActivitiesGroupsAndActivities();
        }

    }
}
