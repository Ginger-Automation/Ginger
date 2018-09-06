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
            TBH.AddFormattedText("What are POMs used for?", foregroundColor, true);
            TBH.AddLineBreak();
            TBH.AddFormattedText("POMs are models used to store a given GUI page’s objects such as buttons, text boxes, drop-down lists, etc.", foregroundColor);
            TBH.AddLineBreak();
            TBH.AddFormattedText("By using POMs you will be able to maintain relevant page objects better and keep track of the objects’ availability within the pages, any time there are changes made to them. This will enable you to minimize potential failures encountered during execution and avoid duplications whenever using the same controls in different actions on the same page.", foregroundColor);
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddFormattedText("How to create a POM?", foregroundColor, true);
            TBH.AddLineBreak();
            TBH.AddFormattedText("You can create a POM by simply following the POM wizard, which will automatically learn a page’s relevant objects and properties. Once learned, you will be able to filter the objects and select only those needed to create the automation required.", foregroundColor);
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            //Deleting until we will know how exactly to integrate POM in Automation Flow
            //TBH.AddFormattedText("How to Integrate POM in Automation Flow?", foregroundColor, true);
            //TBH.AddLineBreak();
            //TBH.AddFormattedText("After creating the POM Use the Element Action to perform operation on a spesific Page Element", foregroundColor);
            //TBH.AddLineBreak();
            //TBH.AddLineBreak();
            //TBH.AddLineBreak();


        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            
        }
    }
}
