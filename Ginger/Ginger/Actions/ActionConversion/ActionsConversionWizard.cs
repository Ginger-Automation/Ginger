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
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Repository;
using Ginger.SolutionGeneral;
using Ginger.WizardLib;
using GingerCore;
using GingerCore.Actions;
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
        public Context Context;
        public ObservableList<ConvertableActionDetails> ActionToBeConverted = new ObservableList<ConvertableActionDetails>();

        public bool NewActivityChecked { get; set; }

        private bool mSameActivityChecked = true;
        public bool SameActivityChecked
        {
            get {
                return mSameActivityChecked;
            }
            set
            {
                mSameActivityChecked = value;
                NewActivityChecked = !value;
            }
        }

        public bool DefaultTargetAppChecked { get; set; }

        public string SelectedTargetApp { get; set; }

        public bool ConvertToPOMAction { get; set; }

        private string mSelectedPOMObjectName = string.Empty;
        public string SelectedPOMObjectName
        {
            get
            {
                return mSelectedPOMObjectName;
            }
            set
            {
                mSelectedPOMObjectName = value;
            }
        }

        public ActionsConversionWizard(Context context)
        {
            Context = context;

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Actions Conversion Introduction", Page: new WizardIntroPage("/Actions/ActionConversion/ActionConversionIntro.md"));

            AddPage(Name: "Select Activities for Conversion", Title: "Select Activities for Conversion", SubTitle: "Select Activities for Conversion", Page: new SelectActivityWzardPage());

            AddPage(Name: "Select Legacy Actions Type for Conversion", Title: "Select Legacy Actions Type for Conversion", SubTitle: "Select Legacy Actions Type for Conversion", Page: new SelectActionWzardPage());

            AddPage(Name: "Conversion Configurations", Title: "Conversion Configurations", SubTitle: "Conversion Configurations", Page: new ConversionConfigurationWzardPage());
        }
        
        public override void Finish()
        {
            ConverToActions();
        }

        private void ConverToActions()
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
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    Reporter.ToStatus(eStatusMsgKey.BusinessFlowConversion, null, Context.BusinessFlow.Name);

                    // create a new converted activity
                    ActionConversionUtils utils = new ActionConversionUtils();
                    utils.ActUIElementElementLocateByField = nameof(ActUIElement.ElementLocateBy);
                    utils.ActUIElementLocateValueField = nameof(ActUIElement.LocateValue);
                    utils.ActUIElementElementLocateValueField = nameof(ActUIElement.ElementLocateValue);
                    utils.ActUIElementElementTypeField = nameof(ActUIElement.ElementType);
                    utils.ActUIElementClassName = nameof(ActUIElement);
                    utils.ConvertToActions(NewActivityChecked, Context.BusinessFlow, ActionToBeConverted, DefaultTargetAppChecked, SelectedTargetApp, ConvertToPOMAction, SelectedPOMObjectName);                    
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " - ", ex);
                    Reporter.ToUser(eUserMsgKey.ActivitiesConversionFailed);
                }
                finally
                {
                    Reporter.HideStatusMessage();
                    Mouse.OverrideCursor = null;
                }
            }
        }

        public override void Cancel()
        {
            base.Cancel();
        }

        private bool DoExistingPlatformCheck(ObservableList<ConvertableActionDetails> lstActionToBeConverted)
        {
            // fetch list of existing platforms in the business flow
            List<ePlatformType> lstExistingPlatform = WorkSpace.Instance.Solution.ApplicationPlatforms
                                                      .Where(x => Context.BusinessFlow.TargetApplications
                                                      .Any(a => a.Name == x.AppName))
                                                      .Select(x => x.Platform).ToList();

            Dictionary<ePlatformType, string> lstMissingPlatform = new Dictionary<ePlatformType, string>();
            // create list of missing platforms
            foreach (ConvertableActionDetails ACH in lstActionToBeConverted)
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
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
