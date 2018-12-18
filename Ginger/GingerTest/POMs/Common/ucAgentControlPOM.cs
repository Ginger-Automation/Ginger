using Amdocs.Ginger.UserControls;
using Ginger.Agents;
using GingerCore;
using GingerWPFUnitTest.POMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GingerTest.POMs.Common
{
    public class ucAgentControlPOM : GingerPOMBase
    {
    
        ucAgentControl mUCAgentControl;
        public ucAgentControlPOM(ucAgentControl ucAgentControl)
        {
            mUCAgentControl = ucAgentControl;
        }

        public void SelectValueUCAgentControl(Agent agent)
        {
            Execute(() => {
                ComboBox comboBox = (ComboBox)FindElementByAutomationID<ComboBox>(mUCAgentControl, "AgentsComboBox AID");
                comboBox.SelectedValue = agent;
            });
        }


        //public void SelectValueUCAgentControl(Agent agent)
        //{
        //    Execute(() => {
        //        ((ComboBox)mUCAgentControl.FindName("xAgentsComboBox")).SelectedValue = agent;
        //    });
        //}




        public void UCAgentControlStatusButtonClick()
        {
            Task.Factory.StartNew(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    Execute(() =>
                    {

                        ucButton b = (ucButton)FindElementByAutomationID<ucButton>(mUCAgentControl, "AgentStatusButton AID");
                        UCButtonPOM p = new UCButtonPOM(b);
                        p.Click();
                    });
                });
            });
        }
    }
}
