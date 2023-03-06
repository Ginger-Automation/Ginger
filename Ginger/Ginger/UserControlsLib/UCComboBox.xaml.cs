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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.GeneralLib;
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
        private Context mContext;
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

        public void Init(object bindedObject, string bindedObjField, Type optionalEnumType, SelectionChangedEventHandler UCselectionChange = null)
        {
            // If the VE is on stand alone form:
            this.obj = bindedObject;
            this.AttrName = bindedObjField;         

            GingerCore.General.FillComboFromEnumType(ComboBox, optionalEnumType);
            BindVEAndSelectionChangedEvent(false, UCselectionChange);
        }

        public void Init(object bindedObject, string AttrName, dynamic comboBoxEnumItemsList, DependencyProperty dependencyProperty)
        {
            this.obj = bindedObject;
            this.AttrName = AttrName;

            FillComboBoxFromDynamicList(comboBoxEnumItemsList);
            ComboBox.IsEditable = false;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ComboBox, dependencyProperty, bindedObject, AttrName);
           
        }


        public void Init(object obj, string AttrName)
        {
            // If the VE is on stand alone form:
            this.obj = obj;
            this.AttrName = AttrName;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty, obj, AttrName);
        }


        /// <summary>
        /// Initilized and Bind the UCComboBox with ActInputValue based on the enum type.
        /// </summary>
        /// <param name="AIV">The ActInputValue related to the Param Name saved on the Action Field</param>
        /// <param name="optionalEnumType">Type of the Enum created under the Action represents the values on the ComboBox</param>
        /// <param name="isVENeeded">boolean value to specify is value expression is needed</param>
        /// <param name="UCselectionChange">Selection changed event to attach to the field</param>
        /// <param name="context">Context of the input value. If Value expression is needed then context is must</param>
        public void Init(ActInputValue AIV,Type optionalEnumType=null, bool isVENeeded = false,  SelectionChangedEventHandler UCselectionChange= null, Context context=null)
        {
            // If the VE is on stand alone form:
            this.obj = AIV;
            this.AttrName = nameof(ActInputValue.Value);
            mContext = context;
            if (optionalEnumType!=null)
            {
                GingerCore.General.FillComboFromEnumType(ComboBox, optionalEnumType);
            }

            BindVEAndSelectionChangedEvent(isVENeeded, UCselectionChange);

        }

        /// <summary>
        /// Initilized and Bind the UCComboBox with ActInputValue based on the dynamic list
        /// </summary>
        /// <param name="AIV">The ActInputValue related to the Param Name saved on the Action Field</param>
        /// <param name="comboBoxEnumItemsList"> List of items to be binded with combo box values</param>
        /// <param name="isVENeeded">boolean value to specify is value expression is needed</param>
        /// <param name="UCselectionChange">Selection changed event to attach to the field</param>
        /// <param name="context">Context of the input value. If Value expression is needed then context is must</param>
        public void Init(ActInputValue AIV, dynamic comboBoxEnumItemsList, bool isVENeeded = false, SelectionChangedEventHandler UCselectionChange = null, Context context = null)
        {
            // If the VE is on stand alone form:
            this.obj = AIV;
            this.AttrName = nameof(ActInputValue.Value);
            mContext = context;
            FillComboBoxFromDynamicList(comboBoxEnumItemsList);
            BindVEAndSelectionChangedEvent(isVENeeded, UCselectionChange);
        }


        private void FillComboBoxFromDynamicList(dynamic comboBoxEnumItemsList)
        {
            if (comboBoxEnumItemsList != null && comboBoxEnumItemsList.Count > 0)
            {
                ComboBox.Items.Clear();
                ComboBox.SelectedValuePath = "Value";
                Type Etype = comboBoxEnumItemsList[0].GetType();                
                foreach (object item in comboBoxEnumItemsList)
                {
                    ComboItem CEI = new ComboItem();
                    CEI.text = GingerCore.General.GetEnumValueDescription(Etype, item);
                    CEI.Value = item;
                    ComboBox.Items.Add(CEI);
                }
            }
        }
        private void BindVEAndSelectionChangedEvent(bool isVENeeded, SelectionChangedEventHandler UCselectionChange)
        {
            if (UCselectionChange != null)
            {
                ComboBox.SelectionChanged += UCselectionChange;
            }

            if (isVENeeded)
            {
                Col.Width = new GridLength(22);
                VEButton.Visibility = Visibility.Visible;
                ComboBox.IsEditable = true;
                if(obj.GetType()==typeof(ActInputValue))
                {
                    GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(ComboBox, ComboBox.TextProperty, (ActInputValue)obj);
                }
                else
                {
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ComboBox, ComboBox.TextProperty, obj, AttrName);
                }
                
            }
            else
            {
                if (obj.GetType() == typeof(ActInputValue))
                {
                    GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(ComboBox, ComboBox.SelectedValueProperty, (ActInputValue)obj);
                }
                else
                {
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty, obj, AttrName);
                }
            }
        }

        public void VEButton_Click(object sender, RoutedEventArgs e)
        {           
            if(obj is Act)
            {
                mContext = Context.GetAsContext(((Act)obj).Context);
            }
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(obj, AttrName, mContext);
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            ComboBox.Text = w.xExpressionUCTextEditor.textEditor.Text;
        }

        public void UpdateComboItems(List<ComboItem> comboBoxItemsList)
        {
            ComboBox.Items.Clear();
            ComboBox.SelectedValuePath = "Value";
            foreach (ComboItem item in comboBoxItemsList)
                ComboBox.Items.Add(item);
        }
    }
}