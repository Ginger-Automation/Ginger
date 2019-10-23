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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Amdocs.Ginger.Common;
using Ginger.Drivers.WindowsAutomation;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.UIAutomation;
using GingerCore.Drivers.Common;

namespace Ginger.Drivers.UIA
{
    /// <summary>
    /// Interaction logic for EditControlPage.xaml
    /// </summary>
    public partial class UIAElementPage : Page
    {
        UIAElementInfo mUIAElementInfo;
        public ActPBControl.eControlAction mAction { get; set; }

        public UIAElementPage(UIAElementInfo EI)
        {
            InitializeComponent();  
            mUIAElementInfo = EI;
            ShowInfo();
        }

        private void ShowInfo()
        {            
            ShowAESupportedPatterns();
        }

        private void ShowAESupportedPatterns()
        {
            object[] Patterns = ((UIAutomationDriverBase)mUIAElementInfo.WindowExplorer).mUIAutomationHelper
                .GetSupportedPatterns(mUIAElementInfo.ElementObject);

            List<ControlAutomationPatterm> CAPs = new List<ControlAutomationPatterm>();
            foreach (var AP in Patterns)
            {
                ControlAutomationPatterm CAP = new ControlAutomationPatterm();
                CAP.Pattern = AP;
                CAP.Name = ((UIAutomationDriverBase)mUIAElementInfo.WindowExplorer).mUIAutomationHelper.GetPatternName(AP);
                CAPs.Add(CAP);
            }

            SupportedPatternsGrid.ItemsSource = CAPs;
        }


        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            int selCount = SupportedPatternsGrid.SelectedItems.Count;
            if (selCount == 0)
            {                
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "No Pattern selected for testing");
                return;
            }

            // Call the selected pattern
            object AP = ((ControlAutomationPatterm)SupportedPatternsGrid.SelectedItem).Pattern;
            ((UIAutomationDriverBase) mUIAElementInfo.WindowExplorer).mUIAutomationHelper.TestPattern(
                mUIAElementInfo.ElementObject, AP);
        }

        private const UInt32 MOUSEEVENTF_LEFTDOWN = 0x0002;
            private const UInt32 MOUSEEVENTF_LEFTUP = 0x0004;

        [DllImport("user32.dll")]
        private static extern void mouse_event(UInt32 dwFlags, UInt32 dx, UInt32 dy, UInt32 dwData, IntPtr dwExtraInfo);
        
        private void MouseClickElement(AutomationElement AE)
        {            
            System.Windows.Point clickpoint;
            bool b = AE.TryGetClickablePoint(out clickpoint);
            if (!b) return;

            System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)clickpoint.X, (int)clickpoint.Y);

            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, new IntPtr());
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, new IntPtr());
        }
    }
}
