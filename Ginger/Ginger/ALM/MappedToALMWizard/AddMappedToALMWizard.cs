using Amdocs.Ginger.Common;
using Ginger.ALM.MappedToALMWizard;
using Ginger.ALM.ZephyrEnt.TreeViewItems;
using Ginger.WizardLib;
using GingerCore;
using GingerWPF.WizardLib;
using System;

namespace Ginger.ALM.MappedToALMWizard
{
    class AddMappedToALMWizard : WizardBase
    {
        public Context Context;
        //public ActivitiesGroup ParentActivitiesGroup { get; set; }
        //public Activity ActivityToAdd;
        public bool ActivitiesGroupPreSet;

        public override string Title { get { return String.Format("Add New {0} Wizard", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)); } }


        public AddMappedToALMWizard(/*Context context, ActivitiesGroup parentActivitiesGroup = null*/)
        {
            //ZephyrEntRepositoryExplorerPage win = new ZephyrEntRepositoryExplorerPage();
            //win.ShowAsWindow(eWindowShowStyle.Dialog);
            //Context = context;
            //if (parentActivitiesGroup != null)
            //{
            //    ParentActivitiesGroup = parentActivitiesGroup;
            //    ActivitiesGroupPreSet = true;
            //}
            //else
            //{
            //    if (Context.BusinessFlow.ActivitiesGroups.Count > 0)
            //    {
            //       // ParentActivitiesGroup = Context.BusinessFlow.ActivitiesGroups[0];
            //    }
            //    else
            //    {
            //        //ParentActivitiesGroup = Context.BusinessFlow.AddActivitiesGroup();
            //    }
            //}

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "ALM Export Introduction", Page: new WizardIntroPage("/ALM/MappedToALMWizard/MappedToALMIntro.md"));

            AddPage(Name: "Test Set Mapping", Title: "Business Flow Name", SubTitle: String.Format("Set New {0} General Details", GingerDicser.GetTermResValue(eTermResKey.Activity)), Page: new TestSetMappingPage());

            AddPage(Name: "Test Case Mapping", Title: String.Format("{0} Configurations", GingerDicser.GetTermResValue(eTermResKey.Activity)), SubTitle: String.Format("Set New {0} Configurations", GingerDicser.GetTermResValue(eTermResKey.Activity)), Page: new TestCaseMappingPage());
            AddPage(Name: "Test Case Mapping", Title: String.Format("{0} Configurations", GingerDicser.GetTermResValue(eTermResKey.Activity)), SubTitle: String.Format("Set New {0} Configurations", GingerDicser.GetTermResValue(eTermResKey.Activity)), Page: new TestStepMappingPage());
        }

        public override void Finish()
        {
            //Context.BusinessFlow.AddActivity(ActivityToAdd, ParentActivitiesGroup);
        }
    }
}
