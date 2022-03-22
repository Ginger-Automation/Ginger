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

using GingerCore.Helpers;
using GingerWPF.WizardLib;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for AddPOMIntorPage.xaml
    /// </summary>
    public partial class POMIntroductionWizardPage : Page , IWizardPage
    {
        public POMIntroductionWizardPage()
        {
            InitializeComponent();
             
            TextBlockHelper TBH = new TextBlockHelper(xNewPOMIntroTxtBlock);

            SolidColorBrush foregroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("@Skin1_ColorA")).ToString());
            //Application info
            TBH.AddFormattedText("For What POMs is Needed?", foregroundColor, true);
            TBH.AddLineBreak();
            TBH.AddFormattedText("POMs are needed for storing page elementslike: Buttons, Textboxes and DropDown lists ect.", foregroundColor);
            TBH.AddLineBreak();
            TBH.AddFormattedText("Idea is that you will be able to maintain your page elements better and keep tracking on the elements availability in any case of the page elements properties changes to minimize failures during execution causes such changes and avoid duplications while using the same control on different acrtions", foregroundColor);
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddFormattedText("How to Create a POM?", foregroundColor, true);
            TBH.AddLineBreak();
            TBH.AddFormattedText("You can create the POM by simply follow the Wizard and learn a spesific relevant page elements then you can filter founded elements list and select only the elements which neeeded to perform the automation", foregroundColor);
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddFormattedText("How to Integrate POM in Automation Flow?", foregroundColor, true);
            TBH.AddLineBreak();
            TBH.AddFormattedText("After creating the POM Use the Element Action to perform operation on a spesific Page Element", foregroundColor);
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddLineBreak();


        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {

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
