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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions
{
    public partial class ActDemoPageEditPage : Page
    {
        public ActionEditPage actp;
        public ValueExpression VE;
        private ActDemoPage mAct;

        public ActDemoPageEditPage(ActDemoPage Act)
        {
            InitializeComponent();
            mAct = Act;
            Bind();
            mAct.SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();
        }

        public void Bind()
        {
            //TextBox Example
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(ExampleTextBox, TextBox.TextProperty, mAct.GetOrCreateInputParam(ActDemoPage.Fields.RegularTextBox));

            //UCValueExpression Example
            ExampleTextBoxNoBrowser.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActDemoPage.Fields.TextBoxParamNoBrowser), true, false, UCValueExpression.eBrowserType.Folder);

            ExampleTextBoxNoVE.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActDemoPage.Fields.TextBoxParamNoVE), false, true, UCValueExpression.eBrowserType.File, "exe");

            ExampleTextBoxFile.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActDemoPage.Fields.TextBoxParamFile),true, true, UCValueExpression.eBrowserType.File , "xml", new RoutedEventHandler(BrowseButtonExample_Click));

            ExampleTextBoxFolder.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActDemoPage.Fields.TextBoxParamFolder), true, true, UCValueExpression.eBrowserType.Folder, extraBrowserSelectionHandler: new RoutedEventHandler(BrowseButtonExample_Click));
           
            List<ComboItem> comboBoxItemsList = GeneratecomboBoxItemsList();

            //UCComboBox Example  Filled with the Enum.
            ExampleUCComboBox.Init(mAct.GetOrCreateInputParam(ActDemoPage.Fields.ComboBoxDataValueType), typeof(ActDemoPage.eComboBoxDataValueType));

            //UCComboBox With VE Button Example.
            ExampleUCComboBoxWithVE.Init(mAct.GetOrCreateInputParam(ActDemoPage.Fields.ComboBoxDataValueTypeWithVE), comboBoxItemsList,true, context: Context.GetAsContext(mAct.Context));
            
            //CheckBox Example
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(ExampleCheckBox, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActDemoPage.Fields.CheckBoxParam));
            
            //UCInputValueGrid Example
            ExampleUCInputValuesGrid.Init(Context.GetAsContext(mAct.Context), mAct.ActionGrid, "Grid Title", "Property Name", "Property Value", "Property Calculated Value");
            
            //Radio Button 
            RadioButtonInit();

            //UCRadioButtons Example
            ExampleUCRadioButtons.Init(typeof(ActDemoPage.eRadioButtonValueType), RadioButtonPanel, mAct.GetOrCreateInputParam(ActDemoPage.Fields.UCRadioParam), new RoutedEventHandler(RadioButtonExample_Click));
        }

        public List<ComboItem> GeneratecomboBoxItemsList()
        {
            List<ComboItem> comboBoxItemsList = new List<ComboItem>();

            ComboItem CBI1 = new ComboItem();
            CBI1.text = "Value 1";
            CBI1.Value = "Value1";

            ComboItem CBI2 = new ComboItem();
            CBI2.text = "Value 2";
            CBI2.Value = "Value2";

            ComboItem CBI3 = new ComboItem();
            CBI3.text = "Value 3";
            CBI3.Value = "Value3";

            comboBoxItemsList.Add(CBI1);
            comboBoxItemsList.Add(CBI2);
            comboBoxItemsList.Add(CBI3);

            return comboBoxItemsList;
        }

        public void RadioButtonInit()
        {
            string currentValue = mAct.GetInputParamValue(ActDemoPage.Fields.RadioParam);
            foreach (RadioButton rdb in Panel.Children)
                if (rdb.Tag.ToString() == currentValue)
                {
                    rdb.IsChecked = true;
                    break;
                }
        }

        public void ExampleRadioButton_Click(object sender, RoutedEventArgs e)
        {
            mAct.AddOrUpdateInputParamValue(ActDemoPage.Fields.RadioParam, (((RadioButton)sender).Tag).ToString());
        }

        private void BrowseButtonExample_Click(object sender, RoutedEventArgs e)
        {
            //Extra functionality on the top of the dialog and setting the field.            
            Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Browse Button Extra functionality handler has been triggered");
        }

        private void RadioButtonExample_Click(object sender, RoutedEventArgs e)
        {
            //Extra functionality on the top of the dialog and setting the field.            
            Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Radio Button Extra functionality handler has been triggered");
        }
    }
}
