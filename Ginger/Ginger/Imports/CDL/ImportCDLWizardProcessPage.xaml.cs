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

using GingerWPF.WizardLib;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Imports.CDL
{
    /// <summary>
    /// Interaction logic for ImportCDLWizardProcessPage.xaml
    /// </summary>
    public partial class ImportCDLWizardProcessPage : Page, IWizardPage
    {
        ImportCDLWizard wiz;
        public ImportCDLWizardProcessPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    wiz = (ImportCDLWizard)WizardEventArgs.Wizard;
                    break;
                case EventType.Active:

                    break;
            }
        }

        private void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            LogTextBlock.Text += "Processing started" + Environment.NewLine;
            wiz.ImportCDL.Run();
            LogTextBlock.Text += "Processing completed";
        }
    }
}
