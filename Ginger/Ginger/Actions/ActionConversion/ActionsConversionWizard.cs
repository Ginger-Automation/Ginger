#region License
/*
Copyright © 2014-2025 European Support Limited

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
    public class ActionsConversionWizard : WizardBase, IActionsConversionProcess
    {
        public override string Title { get { return "Actions Conversion Wizard"; } }
        public Context Context;
        public ObservableList<ConvertableActionDetails> ActionToBeConverted = [];
        public ObservableList<ConvertableTargetApplicationDetails> ConvertableTargetApplications = [];
        public ObservableList<Guid> SelectedPOMs = [];

        public bool NewActivityChecked { get; set; }

        public bool IsConversionDoneOnce { get; set; }

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
            set
            {
                mListOfBusinessFlow = value;
            }
        }

        public eModelConversionType ModelConversionType
        {
            get
            {
                return eModelConversionType.ActionConversion;
            }
        }

        ConversionStatusReportPage mReportPage = null;

        public ActionsConversionWizard(eActionConversionType conversionType, Context context, ObservableList<BusinessFlow> businessFlows)
        {
            Context = context;
            ConversionType = conversionType;
            mListOfBusinessFlow = GetBusinessFlowsToConvert(businessFlows);

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Actions Conversion Introduction", Page: new WizardIntroPage("/Actions/ActionConversion/ActionConversionIntro.md"));

            if (ConversionType == eActionConversionType.MultipleBusinessFlow)
            {
                AddPage(Name: "Select Business Flow's for Conversion", Title: "Select " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + "'s for Conversion", SubTitle: "Select " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + "'s for Conversion", Page: new SelectBusinessFlowWzardPage(mListOfBusinessFlow, Context));
            }
            else if (ConversionType == eActionConversionType.SingleBusinessFlow)
            {
                AddPage(Name: "Select Activities for Conversion", Title: "Select " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " for Conversion", SubTitle: "Select " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " for Conversion", Page: new SelectActivityWzardPage());
            }

            AddPage(Name: "Select Legacy Actions Type for Conversion", Title: "Select Legacy Actions Type for Conversion", SubTitle: "Select Legacy Actions Type for Conversion", Page: new SelectActionWzardPage());

            AddPage(Name: "Conversion Configurations", Title: "Conversion Configurations", SubTitle: "Conversion Configurations", Page: new ConversionConfigurationWzardPage());

            if (ConversionType == eActionConversionType.MultipleBusinessFlow)
            {
                mReportPage = new ConversionStatusReportPage(mListOfBusinessFlow);
                AddPage(Name: "Conversion Status Report", Title: "Conversion Status Report", SubTitle: "Conversion Status Report", Page: mReportPage);
            }
        }

        /// <summary>
        /// This method is used to ge the business flows to convert
        /// </summary>
        /// <param name="businessFlows"></param>
        /// <returns></returns>
        private ObservableList<BusinessFlowToConvert> GetBusinessFlowsToConvert(ObservableList<BusinessFlow> businessFlows)
        {
            ObservableList<BusinessFlowToConvert> lst = [];
            Parallel.ForEach(businessFlows, bf =>
            {
                BusinessFlowToConvert flowToConvert = new BusinessFlowToConvert
                {
                    BusinessFlow = bf,
                    TotalProcessingActionsCount = mConversionUtils.GetConvertibleActionsCountFromBusinessFlow(bf)
                };
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
                BusinessFlowsActionsConversion(mListOfBusinessFlow);
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
        public async Task ProcessConversion(ObservableList<BusinessFlowToConvert> lst, bool isReConvert)
        {
            IsConversionDoneOnce = true;
            ProcessStarted();
            try
            {
                if (isReConvert)
                {
                    ObservableList<BusinessFlowToConvert> selectedLst = [];
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
                    await Task.Run(() => mConversionUtils.ContinueConversion(ActionToBeConverted, NewActivityChecked, ConvertableTargetApplications, ConvertToPOMAction, SelectedPOMs)).ConfigureAwait(true);
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
        public async Task BusinessFlowsActionsConversion(ObservableList<BusinessFlowToConvert> lst)
        {
            try
            {
                IsConversionDoneOnce = true;
                ProcessStarted();

                mConversionUtils.ActUIElementElementLocateByField = nameof(ActUIElement.ElementLocateBy);
                mConversionUtils.ActUIElementLocateValueField = nameof(ActUIElement.ElementLocateValue);
                mConversionUtils.ActUIElementElementLocateValueField = nameof(ActUIElement.ElementLocateValue);
                mConversionUtils.ActUIElementElementTypeField = nameof(ActUIElement.ElementType);
                mConversionUtils.ActUIElementClassName = nameof(ActUIElement);
                mConversionUtils.ListOfBusinessFlowsToConvert = lst;

                await Task.Run(() =>

                mConversionUtils.ConvertActionsOfMultipleBusinessFlows(ActionToBeConverted, NewActivityChecked, ConvertableTargetApplications, ConvertToPOMAction, SelectedPOMs)

                ).ConfigureAwait(true);

                if (ConversionType == eActionConversionType.MultipleBusinessFlow)
                {
                    mReportPage.SetButtonsVisibility(true);
                    ProcessEnded();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred during Legacy Actions conversion process.", ex);
                Reporter.ToUser(eUserMsgKey.ActivitiesConversionFailed);
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
        }

        /// <summary>
        /// This method will restore the conversion done
        /// </summary>
        public override void Cancel()
        {
            try
            {
                if (IsConversionDoneOnce)
                {
                    foreach (BusinessFlowToConvert bfToConvert in ListOfBusinessFlow)
                    {
                        try
                        {
                            bfToConvert.BusinessFlow.RestoreFromBackup();
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Error occurred while Restoring from backup - " + bfToConvert.BusinessFlowName + " - ", ex);
                        }
                    }
                }
            }
            finally
            {
                base.Cancel();
            }
        }

        public void ConversionProcessEnded()
        {
            ProcessEnded();
        }

        public void ConversionProcessStarted()
        {
            ProcessStarted();
        }
    }
}
