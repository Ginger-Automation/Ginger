using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger;
using Ginger.Agents;
using GingerCore;
using GingerTest.POMs.Common;
using GingerWPF.UserControlsLib;
using GingerWPFUnitTest.POMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GingerTest.POMs
{
    public class POMsPOM : GingerPOMBase
    {
        private SingleItemTreeViewExplorerPagePOM mTreeView;

        public POMsPOM(SingleItemTreeViewExplorerPage page)
        {
            mTreeView = new SingleItemTreeViewExplorerPagePOM(page);
        }

        public SingleItemTreeViewExplorerPagePOM POMsTree { get { return mTreeView; } }

        public ApplicationPOMModel CreatePOM(string targetApp, Agent ChromeAgent, string URL)
        {
            mTreeView.AddButton.Click();
            SleepWithDoEvents(100);

            return CreatePOMOnWizard(targetApp, ChromeAgent, URL);
        }

        private ApplicationPOMModel CreatePOMOnWizard(string targetApp, Agent agentName, string URL)
        {
            WizardPOM wizard = WizardPOM.CurrentWizard;
            //skip intro page
            wizard.NextButton.Click();


            //set name
            ucAgentControl ucAgentControl = (ucAgentControl)wizard.CurrentPage["ucAgentControl AID"].dependencyObject;


            ucAgentControlPOM ucAgentControlPOM = new ucAgentControlPOM(ucAgentControl);

            ucAgentControlPOM.SelectValueUCAgentControl(agentName);
            ucAgentControlPOM.UCAgentControlStatusButtonClick();
            SleepWithDoEvents(10000);


            ucGrid ucGrid = (ucGrid)wizard.CurrentPage["AutoMapElementTypesGrid AID"].dependencyObject;

            ucGridPOM gridPOM = new ucGridPOM(ucGrid);

            gridPOM.ClickOnCheckBox(nameof(UIElementFilter.ElementType), eElementType.Button.ToString());
            


            //UIElementPOM mUcAgentControl = wizard.CurrentPage["ucAgentControl AID"];
            //SleepWithDoEvents(20000);

            //wizard.CurrentPage["AutoMapElementTypesGrid AID"].UCAgentControlAutoMapElementTypesGridCheckBoxSelect("HiperLink");


            //ucGridPOM gridPOM = new ucGridPOM((ucGrid)wizard.FindElementByAutomationID<ucGrid>(wizard.CurrentWizardPage,"AutoMapElementTypesGrid AID"));



            wizard.NextButton.Click();



            //wizard.CurrentPage["TargetApplication AID"].SelectedIndex(1);
            //wizard.CurrentPage["ucAgentControl AID"].SelectValueUCAgentControl(1);
            //ucAgentControl ucAgentControl = (ucAgentControl)FindElementByAutomationID<ucAgentControl>(wizard.CurrentWizardPage, "ucAgentControl AID");

            //wizard.CurrentPage["ucAgentControl AID"].FindElementByAutomationID<ComboBox>(ucAgentControl, "AgentsComboBox AID").SelectedIndex(1);





            //ComboBox AgentComboBox = (ComboBox)FindElementByAutomationID<ComboBox>(wizard.CurrentWizardPage, "AgentsComboBox AID");


            //AgentComboBox.Items.Where(x=>x.)

            //""

            //.FindElementByAutomationID("xAgentsComboBox");
            //wizard.CurrentPage["ucAgentControl AID"].SelectValue(agentName);
            //wizard.CurrentPage["Driver Type AID"].SelectValue(driverType);

            //// Driver config page
            //wizard.NextButton.Click();
            //wizard.Finish();

            //// Verify agent appear on tree, might take some time
            //bool b = mTreeView.IsItemExist(targetApp);
            //if (!b) throw new Exception("Cannot find new agent in tree: " + targetApp);

            //Agent agent = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Name == targetApp select x).SingleOrDefault();
            //return agent;

            return null;
        }
    }
}
