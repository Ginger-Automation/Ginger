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

using System;
using System.Collections.Generic;
using System.Linq;
using Amdocs.Ginger.Common;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GingerCore;
using Amdocs.Ginger.Repository;
using GingerCore.GeneralLib;
using Amdocs.Ginger.Common.UIElement;
using Ginger.Actions._Common.ActUIElementLib;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for UCLocateByAndValue.xaml
    /// </summary>
    public partial class UCLocateByAndValue : UserControl
    {
        private object obj;
        private string AttrName;
        private Context mContext;

        public object ComboBoxSelectedValue
        {
            get
            {
                return xElementLocateByComboBox.SelectedValue;
            }
        }

        public ComboBox ComboBoxObject
        {
            get
            {
                return xElementLocateByComboBox;
            }
        }
        public UCLocateByAndValue()
        {
            InitializeComponent();
            this.DataContextChanged += UCLocateByAndValue_DataContextChanged;
        }

        private void UCLocateByAndValue_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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

            GingerCore.General.FillComboFromEnumType(xElementLocateByComboBox, optionalEnumType);
            BindVEAndSelectionChangedEvent(false, UCselectionChange);
        }

        public void Init(object bindedObject, string AttrName, dynamic comboBoxEnumItemsList)
        {
            this.obj = bindedObject;
            this.AttrName = AttrName;

            FillComboBoxFromDynamicList(comboBoxEnumItemsList);
            xElementLocateByComboBox.IsEditable = false;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xElementLocateByComboBox, ComboBox.SelectedValueProperty, bindedObject, AttrName);

        }
        public void Init(object obj, string AttrName)
        {
            // If the VE is on stand alone form:
            this.obj = obj;
            this.AttrName = AttrName;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xElementLocateByComboBox, ComboBox.SelectedValueProperty, obj, AttrName);
        }

        public void Init(ActInputValue AIV, Type optionalEnumType = null, bool isVENeeded = false, SelectionChangedEventHandler UCselectionChange = null, Context context = null)
        {
            // If the VE is on stand alone form:
            this.obj = AIV;
            this.AttrName = nameof(ActInputValue.Value);
            mContext = context;
            if (optionalEnumType != null)
            {
                GingerCore.General.FillComboFromEnumType(xElementLocateByComboBox, optionalEnumType);
            }

            BindVEAndSelectionChangedEvent(isVENeeded, UCselectionChange);

        }

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
                xElementLocateByComboBox.Items.Clear();
                xElementLocateByComboBox.SelectedValuePath = "Value";
                Type Etype = comboBoxEnumItemsList[0].GetType();
                foreach (object item in comboBoxEnumItemsList)
                {
                    ComboItem CEI = new ComboItem();
                    CEI.text = GingerCore.General.GetEnumValueDescription(Etype, item);
                    CEI.Value = item;
                    xElementLocateByComboBox.Items.Add(CEI);
                }
            }
        }

        private void BindVEAndSelectionChangedEvent(bool isVENeeded, SelectionChangedEventHandler UCselectionChange)
        {
            if (UCselectionChange != null)
            {
                xElementLocateByComboBox.SelectionChanged += UCselectionChange;
            }

            if (isVENeeded)
            {
                xElementLocateByComboBox.IsEditable = true;
                if (obj.GetType() == typeof(ActInputValue))
                {
                    GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(xElementLocateByComboBox, ComboBox.TextProperty, (ActInputValue)obj);
                }
                else
                {
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xElementLocateByComboBox, ComboBox.TextProperty, obj, AttrName);
                }

            }
            else
            {
                if (obj.GetType() == typeof(ActInputValue))
                {
                    GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(xElementLocateByComboBox, ComboBox.SelectedValueProperty, (ActInputValue)obj);
                }
                else
                {
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xElementLocateByComboBox, ComboBox.SelectedValueProperty, obj, AttrName);
                }
            }
        }


    }
}
