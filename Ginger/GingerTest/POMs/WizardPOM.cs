#region License
/*
Copyright © 2014-2018 European Support Limited

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
using GingerTest.POMs.Common;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPFUnitTest.POMs
{
    public class WizardPOM : GingerPOMBase
    {
        WizardWindow wiz;

        public WizardPOM()
        {
            wiz = WizardWindow.CurrentWizardWindow;
            int i = 0;
            while (wiz == null && i < 100)
            {
                wiz = WizardWindow.CurrentWizardWindow;
                SleepWithDoEvents(100);
                i++;
            }
        }

        public static WizardPOM CurrentWizard { get { return new WizardPOM(); } }

        public static bool IsWizardOpen
        {
            get
            {
                if (WizardWindow.CurrentWizardWindow != null)                
                    return true;
                else
                    return false;
            }
        }

        Frame PageFrame
        {
            get
            {
                return (Frame)FindElementByName(wiz, "PageFrame");
            }
        }

        private UCButtonPOM GetButton(string name)
        {
            UCButtonPOM pom = null;
            Execute(() =>
            {
                ucButton b = (ucButton)FindElementByName(wiz, name);
                pom = new UCButtonPOM(b);
            });
            return pom;
        }

        public UCButtonPOM NextButton
        {
            get
            {
                return GetButton("xNextButton");
            }
        }

        public UCButtonPOM PrevButton
        {
            get
            {
                return GetButton("xPrevButton");
            }
        }

        public UCButtonPOM FinishButton
        {
            get
            {
                return GetButton("xFinishButton");
            }
        }

        public UCButtonPOM CancelButton
        {
            get
            {
                return GetButton("xCancelButton");
            }
        }



        public Page CurrentWizardPage
        {
            get
            {
                Page p = (Page)PageFrame.Content;
                return p;
            }
        }

        internal void Finish()
        {
            FinishButton.Click();
            Execute(() => {                 
                // Wait for the wizard to close properly max 10 seconds
                int i = 0;
                while (wiz.IsActive && i<100)
                {
                    SleepWithDoEvents(100);
                    i++;
                }
            });

        }

        public PagePOM CurrentPage { 
            get
            {        
                Page p = null;
                Execute(() => {
                    p = (Page)PageFrame.Content;
                });
                return new PagePOM(p);
            }
        }

        public WindowPOM WindowPOM { get { return new WindowPOM(wiz); } }

        

    }
}
