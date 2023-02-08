#region License
/*
Copyright © 2014-2023 European Support Limited

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
