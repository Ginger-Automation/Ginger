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
using GingerCore;
using GingerCore.Activities;
using GingerWPF.WizardLib;
using System;

namespace Ginger.BusinessFlowPages
{
    public class AddActivityWizard: WizardBase
    {
        public Context Context;
        public ActivitiesGroup ParentActivitiesGroup { get; set; }
        public Activity ActivityToAdd;
        public bool ActivitiesGroupPreSet;

        public override string Title { get { return String.Format("Add New {0} Wizard", GingerDicser.GetTermResValue(eTermResKey.Activity)); } }


        public AddActivityWizard(Context context, ActivitiesGroup parentActivitiesGroup = null)
        {
            Context = context;
            if (parentActivitiesGroup != null)
            {
                ParentActivitiesGroup = parentActivitiesGroup;
                ActivitiesGroupPreSet = true;
            }
            else
            {
                if (Context.BusinessFlow.ActivitiesGroups.Count > 0)
                {
                    ParentActivitiesGroup = Context.BusinessFlow.ActivitiesGroups[0];
                }
                else
                {                    
                    ParentActivitiesGroup = Context.BusinessFlow.AddActivitiesGroup();
                }
            }
            
            //AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Agents Introduction", Page: new WizardIntroPage("/Agents/AddAgentWizardLib/AddAgentIntro.md"));

            AddPage(Name: "General Details", Title: "General Details", SubTitle: String.Format("Set New {0} General Details", GingerDicser.GetTermResValue(eTermResKey.Activity)), Page: new AddActivityDetailsPage());

            AddPage(Name: String.Format("{0} Configurations", GingerDicser.GetTermResValue(eTermResKey.Activity)), Title: String.Format("{0} Configurations", GingerDicser.GetTermResValue(eTermResKey.Activity)), SubTitle: String.Format("Set New {0} Configurations", GingerDicser.GetTermResValue(eTermResKey.Activity)), Page: new AddActivityConfigsPage());
        }

        public override void Finish()
        {
            Context.BusinessFlow.AddActivity(ActivityToAdd, ParentActivitiesGroup);
        }
    }
}
