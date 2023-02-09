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

using Ginger.SolutionGeneral;
using GingerWPF.WizardLib;
using System;
using System.Windows.Controls;

namespace GingerWPF.SolutionLib
{
    /// <summary>
    /// Interaction logic for SaveNewSolutionWizardPage.xaml
    /// </summary>
    public partial class SaveNewSolutionWizardPage : Page, IWizardPage
    {
        Solution mSolution;
        NewSolutionWizard mWizard;
        public SaveNewSolutionWizardPage(Solution solution)
        {
            InitializeComponent();
            mSolution = solution;            
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (NewSolutionWizard)WizardEventArgs.Wizard;
                    break;
                
                case EventType.Active:
                    UpdateSummary();
                    break;
            }
        }

        

        private void UpdateSummary()
        {
            string summary = "Solution Name: " + mSolution.Name + Environment.NewLine;
            summary += "Solution folder: " + mSolution.Folder + Environment.NewLine;

            //TODO: add platform, plugins etc.

            SummaryTextBlock.Text = summary;
        }
    }
}