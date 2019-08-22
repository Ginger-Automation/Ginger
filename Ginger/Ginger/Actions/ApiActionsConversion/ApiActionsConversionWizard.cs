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
using Ginger.Actions.ActionConversion;
using Ginger.WizardLib;
using GingerCore;
using GingerWPF.WizardLib;

namespace Ginger.Actions.ApiActionsConversion
{
    public class ApiActionsConversionWizard : WizardBase
    {
        public override string Title { get { return "Convert Webservices Actions"; } }
        public Context Context;
        public ObservableList<ConvertableActionDetails> ActionToBeConverted = new ObservableList<ConvertableActionDetails>();
        public ObservableList<BusinessFlow> ListOfBusinessFlow = null;
        ConversionStatusReportPage mReportPage = null;

        public ApiActionsConversionWizard(Context context)
        {
            Context = context;
            ListOfBusinessFlow = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Webservices Actions Conversion Introduction", Page: new WizardIntroPage("/Actions/ApiActionsConversion/ApiActionsConversionIntro.md"));
            AddPage(Name: "Select Business Flow's for Conversion", Title: "Select Business Flow's for Conversion", SubTitle: "Select Business Flow's for Conversion", Page: new SelectBusinessFlowWzardPage(ListOfBusinessFlow, context));

            mReportPage = new ConversionStatusReportPage();
            AddPage(Name: "Conversion Status Report", Title: "Conversion Status Report", SubTitle: "Conversion Status Report", Page: mReportPage);
        }
        
        public override void Finish()
        {
            
        }
        
        public override void Cancel()
        {
            base.Cancel();
        }
    }
}
