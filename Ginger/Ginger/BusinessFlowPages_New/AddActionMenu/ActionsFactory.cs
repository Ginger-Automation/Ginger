using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository.TargetLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.ApiModelsFolder;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.PlugIns;
using GingerCore.Drivers.Common;
using GingerCore.Platforms.PlatformsInfo;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.BusinessFlowPages_New.AddActionMenu
{
    class ActionsFactory
    {
        public static bool IsLegacyTabSelected = false;

        public static void AddActionsHandler(object mItem, Context mContext)
        {
            Act instance = null;

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
                foreach (ElementInfo elemInfo in currentPOM.MappedUIElements)
                {
                    HTMLElementInfo htmlElementInfo = elemInfo as HTMLElementInfo;
                    instance = GeneratePOMElementRelatedAction(htmlElementInfo, mContext);
                    if (instance != null)
                    {
                        instance.Active = true;
                        mContext.BusinessFlow.AddAct(instance, true);
                    }
                }
                instance = null;
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
                mContext.BusinessFlow.AddAct(instance, true);
                mContext.Activity.Acts.CurrentItem = instance;
            }
        }

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
                Act instance = (Act)selectedAction.CreateCopy();
                if (selectedAction is IObsoleteAction && IsLegacyTabSelected)
                {
                    eUserMsgSelection userSelection = Reporter.ToUser(eUserMsgKey.WarnAddLegacyActionAndOfferNew, ((IObsoleteAction)selectedAction).TargetActionTypeName());
                    if (userSelection == eUserMsgSelection.Yes)
                    {
                        instance = ((IObsoleteAction)selectedAction).GetNewAction();
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
        static Act GeneratePOMElementRelatedAction(ElementInfo elementInfo, Context mContext)
        {
            Act instance;
            IPlatformInfo mPlatform = PlatformInfoBase.GetPlatformImpl(mContext.Platform);
            ElementActionCongifuration actionConfigurations = new ElementActionCongifuration
            {
                LocateBy = eLocateBy.POMElement,
                LocateValue = elementInfo.ParentGuid.ToString() + "_" + elementInfo.Guid.ToString(),
                ElementValue = string.Empty,
                AddPOMToAction = true,
                POMGuid = elementInfo.ParentGuid.ToString(),
                ElementGuid = elementInfo.Guid.ToString(),
                LearnedElementInfo = elementInfo,
            };

            instance = mPlatform.GetPlatformAction(elementInfo, actionConfigurations);
            return instance;
        }

    }
}
