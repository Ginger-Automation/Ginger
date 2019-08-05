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
using Amdocs.Ginger.CoreNET;
using Ginger.WizardLib;
using GingerCore;
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Ginger.Actions.ActionConversion
{
    public class ActionsConversionWizard : WizardBase
    {
        public override string Title { get { return "Actions Conversion Wizard"; } }
        public Context Context;
        public ObservableList<ConvertableActionDetails> ActionToBeConverted = new ObservableList<ConvertableActionDetails>();
        public ObservableList<ConvertableTargetApplicationDetails> ConvertableTargetApplications = new ObservableList<ConvertableTargetApplicationDetails>();
        public ObservableList<string> SelectedPOMs = new ObservableList<string>();

        private bool mNewActivityChecked = true;
        public bool NewActivityChecked
        {
            get
            {
                return mNewActivityChecked;
            }
            set
            {
                mNewActivityChecked = value;
            }
        }

        public ObservableList<BusinessFlow> ListOfBusinessFlow = null;

        public enum eActionConversionType
        {
            SingleBusinessFlow,
            MultipleBusinessFlow
        }

        public eActionConversionType ConversionType { get; set; }

        public object BusinessFlowFolder { get; set; }
        
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

        public ActionsConversionWizard(Context context, object businessFlowFolder = null, eActionConversionType conversionType = eActionConversionType.SingleBusinessFlow)
        {
            Context = context;
            ConversionType = conversionType;
            BusinessFlowFolder = businessFlowFolder;

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Actions Conversion Introduction", Page: new WizardIntroPage("/Actions/ActionConversion/ActionConversionIntro.md"));

            if (ConversionType == eActionConversionType.MultipleBusinessFlow && businessFlowFolder != null)
            {
                AddPage(Name: "Select Business Flow's for Conversion", Title: "Select Business Flow's for Conversion", SubTitle: "Select Business Flow's for Conversion", Page: new SelectBusinessFlowWzardPage());
            }
            else if (ConversionType == eActionConversionType.SingleBusinessFlow)
            {
                AddPage(Name: "Select Activities for Conversion", Title: "Select Activities for Conversion", SubTitle: "Select Activities for Conversion", Page: new SelectActivityWzardPage());
            }

            AddPage(Name: "Select Legacy Actions Type for Conversion", Title: "Select Legacy Actions Type for Conversion", SubTitle: "Select Legacy Actions Type for Conversion", Page: new SelectActionWzardPage());

            AddPage(Name: "Conversion Configurations", Title: "Conversion Configurations", SubTitle: "Conversion Configurations", Page: new ConversionConfigurationWzardPage());
        }
        
        public override void Finish()
        {
            ConverToActions();
        }

        private void ConverToActions()
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                
                // create a new converted activity
                ActionConversionUtils utils = new ActionConversionUtils();
                utils.ActUIElementElementLocateByField = nameof(ActUIElement.ElementLocateBy);
                utils.ActUIElementLocateValueField = nameof(ActUIElement.LocateValue);
                utils.ActUIElementElementLocateValueField = nameof(ActUIElement.ElementLocateValue);
                utils.ActUIElementElementTypeField = nameof(ActUIElement.ElementType);
                utils.ActUIElementClassName = nameof(ActUIElement);

                utils.ConvertActionsOfMultipleBusinessFlows(NewActivityChecked, ListOfBusinessFlow, ActionToBeConverted, ConvertableTargetApplications, ConvertToPOMAction, SelectedPOMs);
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

        public override void Cancel()
        {
            base.Cancel();
        }
    }
}
