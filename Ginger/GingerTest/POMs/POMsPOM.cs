using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger;
using Ginger.Agents;
using GingerCore;
using GingerCore.Actions;
using GingerTest.POMs.Common;
using GingerTestHelper;
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

        public ApplicationPOMModel CreatePOM(string POMName, string targetApp, Agent ChromeAgent, string URL, List<eElementType> elementTypeCheckBoxToClickList)
        {
            mTreeView.AddButton.Click();
            SleepWithDoEvents(100);

            return CreatePOMOnWizard(POMName, targetApp, ChromeAgent, URL, elementTypeCheckBoxToClickList);
        }

        private ApplicationPOMModel CreatePOMOnWizard(string POMName, string targetApp, Agent agent, string URL,List<eElementType> elementTypeCheckBoxToClickList)
        {
            WizardPOM wizard = WizardPOM.CurrentWizard;
            //skip intro page
            wizard.NextButton.Click();
            //set name
            ucAgentControl ucAgentControl = (ucAgentControl)wizard.CurrentPage["ucAgentControl AID"].dependencyObject;
            ucAgentControlPOM ucAgentControlPOM = new ucAgentControlPOM(ucAgentControl);
            ucAgentControlPOM.SelectValueUCAgentControl(agent);
            ucAgentControlPOM.UCAgentControlStatusButtonClick();
            SleepWithDoEvents(10000);
            ucGrid ucGrid = (ucGrid)wizard.CurrentPage["AutoMapElementTypesGrid AID"].dependencyObject;
            ucGridPOM gridPOM = new ucGridPOM(ucGrid);
            foreach (eElementType elementType in elementTypeCheckBoxToClickList)
            {
                gridPOM.ClickOnCheckBox(nameof(UIElementFilter.Selected), nameof(UIElementFilter.ElementType), elementType.ToString());
            }
            string html = TestResources.GetTestResourcesFile(URL);
            agent.Driver.RunAction(new ActBrowserElement() { ControlAction = ActBrowserElement.eControlAction.GotoURL, ValueForDriver = html });
            SleepWithDoEvents(2000);
            wizard.NextButton.Click();
            while (agent.Driver.IsDriverBusy)
            {
                SleepWithDoEvents(20000);
            }
            wizard.NextButton.Click();
            SleepWithDoEvents(2000);
            wizard.CurrentPage["Name POMID"].SetText(POMName);
            wizard.FinishButton.Click();
            // Verify agent appear on tree, might take some time
            bool b = mTreeView.IsItemExist(POMName);
            if (!b) throw new Exception("Cannot find new POM in tree: " + POMName);
            ApplicationPOMModel POM = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>() where x.Name == POMName select x).SingleOrDefault();
            return POM;
        }

        public ApplicationPOMModel SelectPOM(string name)
        {
            mTreeView.SelectItem(name);
            ApplicationPOMModel pom = (ApplicationPOMModel)mTreeView.GetSelectedItemNodeObject();
            return pom;
        }
    }
}
