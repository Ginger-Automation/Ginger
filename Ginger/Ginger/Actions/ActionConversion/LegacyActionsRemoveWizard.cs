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
using System.Threading.Tasks;

namespace Ginger.Actions.ActionConversion
{
    public class LegacyActionsRemoveWizard : WizardBase
    {
        public override string Title { get { return "Legacy Actions Removal Wizard"; } }
        public Context Context;

        public ObservableList<BusinessFlowToConvert> ListOfBusinessFlow = null;
        ActionConversionUtils mConversionUtils = new ActionConversionUtils();

        public LegacyActionsRemoveWizard(Context context, ObservableList<BusinessFlow> businessFlows)
        {
            Context = context;
            ListOfBusinessFlow = GetBusinessFlowsToConvert(businessFlows);
            
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Legacy Actions Removal Introduction", Page: new WizardIntroPage("/Actions/ActionConversion/ActionConversionIntro.md"));
            AddPage(Name: "Select Business Flow's for Conversion", Title: "Select Business Flow's for Conversion", SubTitle: "Select Business Flow's for Conversion", Page: new SelectBusinessFlowWzardPage(ListOfBusinessFlow, Context));
        }

        /// <summary>
        /// This method is used to ge the businessflows to convert
        /// </summary>
        /// <param name="businessFlows"></param>
        /// <returns></returns>
        private ObservableList<BusinessFlowToConvert> GetBusinessFlowsToConvert(ObservableList<BusinessFlow> businessFlows)
        {
            ObservableList<BusinessFlowToConvert> lst = new ObservableList<BusinessFlowToConvert>();
            foreach (BusinessFlow bf in businessFlows)
            {
                BusinessFlowToConvert flowToConvert = new BusinessFlowToConvert();
                flowToConvert.BusinessFlow = bf;
                flowToConvert.TotalProcessingActionsCount = mConversionUtils.GetConvertibleActionsCountFromBusinessFlow(bf);
                lst.Add(flowToConvert);
            }
            return lst;
        }
        
        /// <summary>
        /// This is finish method which does the finish the wizard functionality
        /// </summary>
        public override void Finish()
        {
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

                await Task.Run(() => mConversionUtils.RemoveLegacyActionsHandler(lst));

                ProcessEnded();
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
