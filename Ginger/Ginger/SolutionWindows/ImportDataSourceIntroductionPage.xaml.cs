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

using System.Windows.Controls;
using GingerWPF.WizardLib;

namespace Ginger.DataSource
{
    /// <summary>
    /// Interaction logic for ImportDataSourceIntroductionPage.xaml
    /// </summary>
    public partial class ImportDataSourceIntroductionPage : Page, IWizardPage
    {                
        /// <summary>
        /// This method is default wizard action event
        /// </summary>
        /// <param name="WizardEventArgs"></param>
        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    break;
                case EventType.Active:
                    break;
            }
        }

        /// <summary>
        /// Constrtuctor for ImportDataSourceBrowseFile class
        /// </summary>
        public ImportDataSourceIntroductionPage()
        {           
            InitializeComponent();
        }

        /// <summary>
        /// This method is used to cehck whether alternate page is required to load
        /// </summary>
        /// <returns></returns>
        public bool IsAlternatePageToLoad()
        {
            return false;
        }
    }
}
