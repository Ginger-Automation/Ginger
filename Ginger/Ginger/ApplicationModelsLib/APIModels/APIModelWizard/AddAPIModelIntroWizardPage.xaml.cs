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

using GingerWPF.WizardLib;
using System.Windows.Controls;

namespace GingerWPF.ApplicationModelsLib.APIModels.APIModelWizard
{
    /// <summary>
    /// Interaction logic for AddAPIModelIntroWizardPage.xaml
    /// </summary>
    public partial class AddAPIModelIntroWizardPage : Page, IWizardPage
    {
        public AddAPIModelIntroWizardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
        }        
    }
}
