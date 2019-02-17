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

        public ApplicationPOMModel CreatePOM(string POMName, string POMDescription, string targetApp, Agent ChromeAgent, string URL, List<eElementType> elementTypeCheckBoxToClickList, List<ElementLocator> prioritizedLocatorsList)
        {
            mTreeView.AddButton.Click();
            SleepWithDoEvents(100);

            return CreatePOMOnWizard(POMName, POMDescription, targetApp, ChromeAgent, URL, elementTypeCheckBoxToClickList, prioritizedLocatorsList);
        }

        private ApplicationPOMModel CreatePOMOnWizard(string POMName,string POMDescription, string targetApp, Agent agent, string URL,List<eElementType> elementTypeCheckBoxToClickList, List<ElementLocator> prioritizedLocatorsList)
        {
            WizardPOM wizard = WizardPOM.CurrentWizard;
            wizard.NextButton.Click();
            ucAgentControl ucAgentControl = (ucAgentControl)wizard.CurrentPage["ucAgentControl AID"].dependencyObject;
            ucAgentControlPOM ucAgentControlPOM = new ucAgentControlPOM(ucAgentControl);
            ucAgentControlPOM.SelectValueUCAgentControl(agent);
            ucAgentControlPOM.UCAgentControlStatusButtonClick();
            SleepWithDoEvents(10000);

            //Process AutoMap Element Locators Grid
            ucGrid ucElementLocatorsGrid = (ucGrid)wizard.CurrentPage["AutoMapElementLocatorsGrid AID"].dependencyObject;
            ucGridPOM ucElementLocatorsGridPOM = new ucGridPOM(ucElementLocatorsGrid);
            int locatorIndex = 0;
            foreach (ElementLocator elemLocator in prioritizedLocatorsList)
            {
                if(!elemLocator.Active)
                    ucElementLocatorsGridPOM.ClickOnCheckBox(nameof(ElementLocator.Active), nameof(ElementLocator.LocateBy), elemLocator.LocateBy.ToString());

                ucElementLocatorsGridPOM.ReOrderGridRows(nameof(ElementLocator.LocateBy), elemLocator.LocateBy.ToString(), locatorIndex);

                locatorIndex++;
            }

            //Process AutoMap Element Types Grid
            ucGrid ucElementTypesGrid = (ucGrid)wizard.CurrentPage["AutoMapElementTypesGrid AID"].dependencyObject;
            ucGridPOM ucElementTypesGridPOM = new ucGridPOM(ucElementTypesGrid);
            foreach (eElementType elementType in elementTypeCheckBoxToClickList)
            {
                ucElementTypesGridPOM.ClickOnCheckBox(nameof(UIElementFilter.Selected), nameof(UIElementFilter.ElementType), elementType.ToString());
            }
            string html = TestResources.GetTestResourcesFile(URL);
            agent.Driver.RunAction(new ActBrowserElement() { ControlAction = ActBrowserElement.eControlAction.GotoURL, ValueForDriver = html });
            SleepWithDoEvents(10000);
            wizard.NextButton.Click();
            SleepWithDoEvents(5000);
            while (agent.Driver.IsDriverBusy)
            {
                SleepWithDoEvents(20000);
            }
            wizard.NextButton.Click();
            SleepWithDoEvents(2000);
            wizard.CurrentPage["Name POMID"].SetText(POMName);
            wizard.CurrentPage["Description POMID"].SetText(POMDescription);
            wizard.FinishButton.Click();
            SleepWithDoEvents(2000);

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
