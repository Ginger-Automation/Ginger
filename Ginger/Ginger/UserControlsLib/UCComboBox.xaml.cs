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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for UCValueExpressionComboBox.xaml
    /// </summary>
    public partial class UCComboBox : UserControl
    {
        private object obj;
        private string AttrName;

        public object ComboBoxSelectedValue
        {
            get
            {
                return ComboBox.SelectedValue;
            }
        }

        public ComboBox ComboBoxObject
        {
            get
            {
                return ComboBox;
            }
        }

        public UCComboBox()
        {
            InitializeComponent();

            this.DataContextChanged += UCComboBox_DataContextChanged;

        }

        private void UCComboBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // If the VE is in Grid, we call this function:
            if (e.NewValue.GetType() == typeof(ValueExpression))
            {
                ValueExpression ve = (ValueExpression)e.NewValue;
                Init(ve.Obj, ve.ObjAttr);
            }
        }

        public void Init(object bindedObject, string bindedObjField, Type optionalEnumType, bool isVENeeded = false, SelectionChangedEventHandler UCselectionChange = null)
        {
            // If the VE is on stand alone form:
            this.obj = bindedObject;
            this.AttrName = bindedObjField;

            GingerCore.General.FillComboFromEnumType(ComboBox, optionalEnumType);

            if (UCselectionChange != null)
            {
                ComboBox.SelectionChanged += UCselectionChange;
            }

            if (isVENeeded)
            {
                Col.Width = new GridLength(22);
                VEButton.Visibility = Visibility.Visible;
                ComboBox.IsEditable = true;
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ComboBox, ComboBox.TextProperty, bindedObject, bindedObjField);
            }
            else
            {
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty, bindedObject, bindedObjField);
            }
        }

        public void Init(object bindedObject, dynamic comboBoxEnumItemsList, string AttrName, bool isVENeeded = false)
        {
            this.obj = bindedObject;
            this.AttrName = AttrName;

            if (comboBoxEnumItemsList != null && comboBoxEnumItemsList.Count > 0)
            {
                ComboBox.Items.Clear();
                ComboBox.SelectedValuePath = "Value";
                Type Etype = comboBoxEnumItemsList[0].GetType();
                foreach (object item in comboBoxEnumItemsList)
                {
                    GingerCore.General.ComboItem CEI = new GingerCore.General.ComboItem();
                    CEI.text = GingerCore.General.GetEnumValueDescription(Etype, item);
                    CEI.Value = item;
                    ComboBox.Items.Add(CEI);
                }
            }

            if (isVENeeded)
            {
                Col.Width = new GridLength(22);
                VEButton.Visibility = Visibility.Visible;
                ComboBox.IsEditable = true;
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ComboBox, ComboBox.TextProperty, bindedObject, AttrName);
            }
            else
            {
                ComboBox.IsEditable = false;
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty, bindedObject, AttrName);
            }
        }


        public void Init(object obj, string AttrName)
        {
            // If the VE is on stand alone form:
            this.obj = obj;
            this.AttrName = AttrName;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty, obj, AttrName);
        }

        public void Init(ActInputValue AIV, bool isVENeeded = false, SelectionChangedEventHandler UCselectionChange = null)
        {
            // If the VE is on stand alone form:
            this.obj = AIV;
            this.AttrName = ActInputValue.Fields.Value;

            if (UCselectionChange != null)
            {
                ComboBox.SelectionChanged += UCselectionChange;
            }

            if (isVENeeded)
            {
                Col.Width = new GridLength(22);
                VEButton.Visibility = Visibility.Visible;
                ComboBox.IsEditable = true;
                GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(ComboBox, ComboBox.TextProperty, AIV);
            }
            else
            {
                GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(ComboBox, ComboBox.SelectedValueProperty, AIV);
            }
        }

        /// <summary>
        /// Initilized and Bind the UCComboBox.
        /// </summary>
        /// <param name="AIV">The ActInputValue related to the Param Name saved on the Action Field</param>
        /// <param name="optionalEnumType">Type of the Enum created under the Action represents the values on the ComboBox</param>
        public void Init(ActInputValue AIV,Type optionalEnumType, bool isVENeeded = false,  SelectionChangedEventHandler UCselectionChange= null)
        {
            // If the VE is on stand alone form:
            this.obj = AIV;
            this.AttrName = ActInputValue.Fields.Value;
            GingerCore.General.FillComboFromEnumType(ComboBox, optionalEnumType);
            if (UCselectionChange != null)
            {
                ComboBox.SelectionChanged += UCselectionChange;
            }


            if (isVENeeded)
            {
                Col.Width = new GridLength(22);
                VEButton.Visibility = Visibility.Visible;
                ComboBox.IsEditable = true;
                GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(ComboBox, ComboBox.TextProperty, AIV);
            }
            else
            {
                GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(ComboBox, ComboBox.SelectedValueProperty, AIV);
            }
        }

        public void Init(ActInputValue AIV, Type optionalEnumType, int DefaultIndexValue, bool isVENeeded = false, SelectionChangedEventHandler UCselectionChange = null)
        {
            // If the VE is on stand alone form:
            this.obj = AIV;
            this.AttrName = ActInputValue.Fields.Value;

            GingerCore.General.FillComboFromEnumType(ComboBox, optionalEnumType);

            if (UCselectionChange != null)
            {
                ComboBox.SelectionChanged += UCselectionChange;
            }

            if (isVENeeded)
            {
                Col.Width = new GridLength(22);
                VEButton.Visibility = Visibility.Visible;
                ComboBox.IsEditable = true;
                GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(ComboBox, ComboBox.TextProperty, AIV);
            }
            else
            {
                GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(ComboBox, ComboBox.SelectedValueProperty, AIV);
            }

            if (ComboBox.Items.Count > 0 && DefaultIndexValue != -1)
            {
                ComboBox.SelectedIndex = DefaultIndexValue;
            }
        }

        public void Init(ActInputValue AIV, dynamic comboBoxEnumItemsList, bool isVENeeded = false, SelectionChangedEventHandler UCselectionChange = null)
        {
            List<GingerCore.General.ComboItem> comboBoxItemsList = new List<GingerCore.General.ComboItem>();
            if (comboBoxEnumItemsList != null && comboBoxEnumItemsList.Count > 0)
            {
                Type Etype = comboBoxEnumItemsList[0].GetType();
                foreach (object item in comboBoxEnumItemsList)
                {
                    GingerCore.General.ComboItem CEI = new GingerCore.General.ComboItem();
                    CEI.text = GingerCore.General.GetEnumValueDescription(Etype, item);
                    CEI.Value = item;
                    comboBoxItemsList.Add(CEI);
                }

                Init(AIV, comboBoxItemsList, isVENeeded, UCselectionChange);
            }
        }
        public void Init(ActInputValue AIV, List<GingerCore.General.ComboItem> comboBoxItemsList, bool isVENeeded = false, SelectionChangedEventHandler UCselectionChange = null)
        {
            // If the VE is on stand alone form:
            this.obj = AIV;
            this.AttrName = ActInputValue.Fields.Value;

            //fill items
            ComboBox.Items.Clear();
            ComboBox.SelectedValuePath = "Value";
            foreach (GingerCore.General.ComboItem item in comboBoxItemsList)
                ComboBox.Items.Add(item);

            if (UCselectionChange != null)
            {
                ComboBox.SelectionChanged += UCselectionChange;
            }

            if (isVENeeded)
            {
                Col.Width = new GridLength(22);
                VEButton.Visibility = Visibility.Visible;
                ComboBox.IsEditable = true;
                GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(ComboBox, ComboBox.TextProperty, AIV);
            }
            else
            {
                GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(ComboBox, ComboBox.SelectedValueProperty, AIV);
            }
        }

        public void VEButton_Click(object sender, RoutedEventArgs e)
        {
            Context context = null;
            if(obj is Act)
            {
                context = Context.GetAsContext(((Act)obj).Context);
            }
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(obj, AttrName, context);
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            ComboBox.Text = w.ValueUCTextEditor.textEditor.Text;
        }

        public void UpdateComboItems(List<GingerCore.General.ComboItem> comboBoxItemsList)
        {
            ComboBox.Items.Clear();
            ComboBox.SelectedValuePath = "Value";
            foreach (GingerCore.General.ComboItem item in comboBoxItemsList)
                ComboBox.Items.Add(item);
        }
    }
}