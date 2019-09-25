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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;
using Ginger.WizardLib;
using GingerCore;
using GingerCore.Actions.Common;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginger.Actions.ActionConversion
{
    public class ActionsConversionWizard : WizardBase
    {
        public override string Title { get { return "Actions Conversion Wizard"; } }
        public Context Context;
        public ObservableList<ConvertableActionDetails> ActionToBeConverted = new ObservableList<ConvertableActionDetails>();
        public ObservableList<ConvertableTargetApplicationDetails> ConvertableTargetApplications = new ObservableList<ConvertableTargetApplicationDetails>();
        public ObservableList<Guid> SelectedPOMs = new ObservableList<Guid>();

        public bool NewActivityChecked { get; set; }

        ActionConversionUtils mConversionUtils = new ActionConversionUtils();

        public List<Activity> LstSelectedActivities { get; set; }
        public enum eActionConversionType
        {
            SingleBusinessFlow,
            MultipleBusinessFlow
        }

        public eActionConversionType ConversionType { get; set; }
                
        public bool ConvertToPOMAction { get; set; }

        ObservableList<BusinessFlowToConvert> mListOfBusinessFlow = null;
        public ObservableList<BusinessFlowToConvert> ListOfBusinessFlow
        {
            get
            {
                return mListOfBusinessFlow;
            }
            set {
                mListOfBusinessFlow = value;
            }
        }

        ConversionStatusReportPage mReportPage = null;

        public ActionsConversionWizard(eActionConversionType conversionType, Context context, ObservableList<BusinessFlow> businessFlows)
        {
            Context = context;
            ConversionType = conversionType;
            ListOfBusinessFlow = GetBusinessFlowsToConvert(businessFlows);
            
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Actions Conversion Introduction", Page: new WizardIntroPage("/Actions/ActionConversion/ActionConversionIntro.md"));

            if (ConversionType == eActionConversionType.MultipleBusinessFlow)
            {
                AddPage(Name: "Select Business Flow's for Conversion", Title: "Select Business Flow's for Conversion", SubTitle: "Select Business Flow's for Conversion", Page: new SelectBusinessFlowWzardPage(ListOfBusinessFlow, Context));
            }
            else if (ConversionType == eActionConversionType.SingleBusinessFlow)
            {
                AddPage(Name: "Select Activities for Conversion", Title: "Select Activities for Conversion", SubTitle: "Select Activities for Conversion", Page: new SelectActivityWzardPage());
            }

            AddPage(Name: "Select Legacy Actions Type for Conversion", Title: "Select Legacy Actions Type for Conversion", SubTitle: "Select Legacy Actions Type for Conversion", Page: new SelectActionWzardPage());

            AddPage(Name: "Conversion Configurations", Title: "Conversion Configurations", SubTitle: "Conversion Configurations", Page: new ConversionConfigurationWzardPage());

            if (ConversionType == eActionConversionType.MultipleBusinessFlow)
            {
                mReportPage = new ConversionStatusReportPage(ListOfBusinessFlow);
                AddPage(Name: "Conversion Status Report", Title: "Conversion Status Report", SubTitle: "Conversion Status Report", Page: mReportPage); 
            }
        }

        /// <summary>
        /// This method is used to ge the businessflows to convert
        /// </summary>
        /// <param name="businessFlows"></param>
        /// <returns></returns>
        private ObservableList<BusinessFlowToConvert> GetBusinessFlowsToConvert(ObservableList<BusinessFlow> businessFlows)
        {
            ObservableList<BusinessFlowToConvert> lst = new ObservableList<BusinessFlowToConvert>();
            Parallel.ForEach(businessFlows, bf =>
            {
                BusinessFlowToConvert flowToConvert = new BusinessFlowToConvert();
                flowToConvert.BusinessFlow = bf;
                flowToConvert.TotalProcessingActionsCount = mConversionUtils.GetConvertibleActionsCountFromBusinessFlow(bf);
                lst.Add(flowToConvert);
            });
            return lst;
        }

        /// <summary>
        /// This method is used to get the Convertible Actions Count From BusinessFlow
        /// </summary>
        /// <param name="bf"></param>
        /// <returns></returns>
        public int GetConvertibleActionsCountFromBusinessFlow(BusinessFlow bf)
        {
            return mConversionUtils.GetConvertibleActionsCountFromBusinessFlow(bf);
        }

        /// <summary>
        /// This is finish method which does the finish the wizard functionality
        /// </summary>
        public override void Finish()
        {
            if (ConversionType == eActionConversionType.SingleBusinessFlow)
            {               
                BusinessFlowsActionsConversion(ListOfBusinessFlow);
            }
        }

        /// <summary>
        /// This method is used to Stop the conversion process in between conversion process
        /// </summary>
        public void StopConversion()
        {
            mConversionUtils.StopConversion();
        }

        /// <summary>
        /// This method is used to convert the action in case of Continue & Re-Convert
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="isReConvert"></param>
        public async void ProcessConversion(ObservableList<BusinessFlowToConvert> lst, bool isReConvert)
        {
            ProcessStarted();
            try
            {
                if (isReConvert)
                {
                    ObservableList<BusinessFlowToConvert> selectedLst = new ObservableList<BusinessFlowToConvert>();
                    foreach (var bf in lst)
                    {
                        if (bf.IsSelected)
                        {                            
                            bf.BusinessFlow.RestoreFromBackup(clearBackup: false);
                            bf.ConversionStatus = eConversionStatus.Pending;
                            bf.SaveStatus = eConversionSaveStatus.Pending;
                            selectedLst.Add(bf);
                        }
                    }
                    mConversionUtils.ListOfBusinessFlowsToConvert = selectedLst;
                }
                else
                {
                    mConversionUtils.ListOfBusinessFlowsToConvert = lst;
                }

                if (mConversionUtils.ListOfBusinessFlowsToConvert.Count > 0)
                {
                    await Task.Run(() => mConversionUtils.ContinueConversion(ActionToBeConverted, NewActivityChecked, ConvertableTargetApplications, ConvertToPOMAction, SelectedPOMs));
                }
                mReportPage.SetButtonsVisibility(true);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " - ", ex);
            }
            finally
            {
                ProcessEnded();
            }
        }

        /// <summary>
        /// This method is used to convert the actions
        /// </summary>
        /// <param name="lst"></param>
        public async void BusinessFlowsActionsConversion(ObservableList<BusinessFlowToConvert> lst)
        {
            try
            {
                ProcessStarted();

                mConversionUtils.ActUIElementElementLocateByField = nameof(ActUIElement.ElementLocateBy);
                mConversionUtils.ActUIElementLocateValueField = nameof(ActUIElement.ElementLocateValue);
                mConversionUtils.ActUIElementElementLocateValueField = nameof(ActUIElement.ElementLocateValue);
                mConversionUtils.ActUIElementElementTypeField = nameof(ActUIElement.ElementType);
                mConversionUtils.ActUIElementClassName = nameof(ActUIElement);
                mConversionUtils.ListOfBusinessFlowsToConvert = lst;

                await Task.Run(() => mConversionUtils.ConvertActionsOfMultipleBusinessFlows(ActionToBeConverted, NewActivityChecked, ConvertableTargetApplications, ConvertToPOMAction, SelectedPOMs));

                if (ConversionType == eActionConversionType.MultipleBusinessFlow)
                {
                    mReportPage.SetButtonsVisibility(true);
                    ProcessEnded();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to convert " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " - ", ex);
                Reporter.ToUser(eUserMsgKey.ActivitiesConversionFailed);
            }
            finally
            {
                Reporter.HideStatusMessage();               
            }
        }
        
        /// <summary>
        /// This method is used to cancle the wizard
        /// </summary>
        public override void Cancel()
        {
            base.Cancel();
        }
    }
}
