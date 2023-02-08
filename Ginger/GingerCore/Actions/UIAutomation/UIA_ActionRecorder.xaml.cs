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

extern alias UIAComWrapperNetstandard;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;
using Amdocs.Ginger.Common.UIElement;
using System;
using System.Windows;


namespace GingerCore.Actions
{
    /// <summary>
    /// Interaction logic for UIA_ActRecorder.xaml
    /// </summary>
    public partial class UIA_ActRecorder : Window
    {
        public BusinessFlow mBusinessFlow { get; set; }

        public UIA_ActRecorder(BusinessFlow BizFlow)
        {
            InitializeComponent();
            mBusinessFlow = BizFlow;
        }
        
        private void HighLightButton_Click(object sender, RoutedEventArgs e)
        {
           // mPBDriver.SetHighLightMode(true);                
        }

        private void GetRecordingButton_Click(object sender, RoutedEventArgs e)
        {
           // GetRecording();  
        }

        
        private void btnStartRecord_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
        }

        internal UIAuto.AutomationElement TryGetActElementByLocator(Act act)
        {
            if (act == null) return null;
            try
            {
                UIAuto.AutomationElement e = TryGetActElementByLocator(act, act.LocateBy, act.LocateValue);
                if (e == null)
                {
                    act.Error = "Element not found - " + act.LocateBy + " - " + act.LocateValue + Environment.NewLine; //TODO add to accommodate multiple locator's need to be done for UIA
                }
                return e;
            }
            catch (Exception e)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                act.Error = "Element not found - " + act.LocateBy + " - " + act.LocateValue + Environment.NewLine + e.Message; //TODO add to accommodate multiple locator's need to be done for UIA
                return null;
            }
        }

        internal UIAuto.AutomationElement TryGetActElementByLocator(Act A, eLocateBy LocateBy, string LocateValue)
        {
            UIAuto.AutomationElement e = null;
            switch (LocateBy)
            {
                case eLocateBy.ByID:

                    break;

                case eLocateBy.ByName:
      
                    break;

                case eLocateBy.ByLinkText:

                    break;

                case eLocateBy.ByValue:
                    break;
                case eLocateBy.ByHref:
                    break;
                case eLocateBy.ByCSS:

                    return null;
                //TODO:
                case eLocateBy.ByXPath:
                     break;
                case eLocateBy.NA:
                case eLocateBy.Unknown:
                    //Do nothing
                    break;
                default:
                    A.Error = "Locator Not implemented yet - " + LocateBy;
                    break;
            }
            return e;
        }
    }
}
