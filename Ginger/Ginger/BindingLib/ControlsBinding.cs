using Amdocs.Ginger.Common;
using GingerCoreNET;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GingerWPF.BindingLib
{
    public static class ControlsBinding
    {
        //#region ENUM
        /// <summary>
        /// 
        /// </summary>
        /// <param name="comboBox"></param>
        /// <param name="EnumObj"></param>
        /// <param name="values"> leave values empty will take all possible vals, or pass a list to limit selection </param>
        public static void FillComboFromEnumObj(ComboBox comboBox, Object EnumObj, List<object> values = null, bool sortValues = true)
        {
            comboBox.Items.Clear();
            comboBox.SelectedValuePath = "Value";
            Type Etype = EnumObj.GetType();

            if (values == null)
            {
                // Get all possible enum vals
                foreach (object item in Enum.GetValues(Etype))
                {
                    ComboEnumItem CEI = new ComboEnumItem();
                    CEI.text = GetEnumValueDescription(Etype, item);
                    CEI.Value = item;
                    comboBox.Items.Add(CEI);
                }
            }
            else
            {
                // get only subset from selected enum vals - used in Edit Action locate by to limit to valid values
                foreach (object item in values)
                {
                    ComboEnumItem CEI = new ComboEnumItem();
                    CEI.text = GetEnumValueDescription(Etype, item);
                    CEI.Value = item;
                    comboBox.Items.Add(CEI);
                }
            }


            if (sortValues)
            {
                // Get the combo to be sorted
                comboBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("text", System.ComponentModel.ListSortDirection.Ascending));
            }

            // comboBox.Text = EnumObj.ToString();
            comboBox.SelectedItem = EnumObj;
            // comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        public static void FillComboFromEnumType(ComboBox comboBox, Type Etype, List<object> values = null)
        {
            comboBox.Items.Clear();
            comboBox.SelectedValuePath = "Value";
            //Type Etype = EnumValue.GetType();

            if (values == null)
            {
                // Get all possible enum vals
                foreach (object item in Enum.GetValues(Etype))
                {
                    ComboEnumItem CEI = new ComboEnumItem();
                    CEI.text = GetEnumValueDescription(Etype, item);
                    CEI.Value = item;
                    comboBox.Items.Add(CEI);
                }
            }
            else
            {
                // get only subset from selected enum vals - used in Edit Action locate by to limit to valid values
                foreach (object item in values)
                {
                    ComboEnumItem CEI = new ComboEnumItem();
                    CEI.text = GetEnumValueDescription(Etype, item);
                    CEI.Value = item;
                    comboBox.Items.Add(CEI);
                }
            }


            // Get the combo to be sorted
            comboBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("text", System.ComponentModel.ListSortDirection.Ascending));

            //comboBox.Text = EnumValue.ToString();
            // comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        public static List<string> GetEnumValues(Type EnumType)
        {
            List<string> l = new List<string>();
            foreach (object item in Enum.GetValues(EnumType))
            {
                // l.Add(item.ToString());                
                l.Add(GetEnumValueDescription(EnumType, item));
            }
            return l;
        }

        public static string GetEnumValueDescription(Type EnumType, object EnumValue)
        {
            try
            {
                EnumValueDescriptionAttribute[] attributes = (EnumValueDescriptionAttribute[])EnumType.GetField(EnumValue.ToString()).GetCustomAttributes(typeof(EnumValueDescriptionAttribute), false);
                //string s = attributes.Length > 0 ? attributes[0].Description : EnumValue.ToString();
                string s;
                if (attributes.Length > 0)
                {
                    s = attributes[0].ValueDescription;


                    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!FIXME
                    //for supporting multi Terminology types
                    //s = s.Replace("Business Flow", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    //s = s.Replace("Business Flows", GingerDicser.GetTermResValue(eTermResKey.BusinessFlows));
                    //s = s.Replace("Activities Group", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup));
                    //s = s.Replace("Activities Groups", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups));
                    //s = s.Replace("Activity", GingerDicser.GetTermResValue(eTermResKey.Activity));
                    //s = s.Replace("Conversion Mechanism", GingerDicser.GetTermResValue(eTermResKey.ConversionMechanism));
                    //s = s.Replace("Activities", GingerDicser.GetTermResValue(eTermResKey.Activities));
                    //s = s.Replace("Variable", GingerDicser.GetTermResValue(eTermResKey.Variable));
                    //s = s.Replace("Variables", GingerDicser.GetTermResValue(eTermResKey.Variables));
                    //s = s.Replace("Run Set", GingerDicser.GetTermResValue(eTermResKey.RunSet));
                    //s = s.Replace("Run Sets", GingerDicser.GetTermResValue(eTermResKey.RunSets));
                }
                else
                {
                    s = EnumValue.ToString();
                }
                return s;
            }
            catch
            {
                //TODO: fixme ugly catch - check why we come here and fix it, todo later
                return EnumValue.ToString();
            }

        }

        public static void ObjFieldBinding(System.Windows.Controls.Control control, DependencyProperty dependencyProperty, object obj, string property, BindingMode BindingMode = BindingMode.TwoWay, IValueConverter bindingConvertor=null)
        {
            //TODO: add Inotify on the obj.attr - so code changes to property will be reflected
            //TODO: check perf impact + reuse exisitng binding on same obj.prop
            try
            {
                System.Windows.Data.Binding b = new System.Windows.Data.Binding();
                b.Source = obj;
                b.Path = new PropertyPath(property);
                b.Mode = BindingMode;
                if (bindingConvertor != null)
                    b.Converter = bindingConvertor;
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                control.SetBinding(dependencyProperty, b);
            }
            catch (Exception ex)
            {
                //it is possible we load an old enum or something else which will cause the binding to fail
                // Can happen also if the bind field name is incorrect
                // mark the control in red, instead of not openning the Page
                // Set a tool tip with the error

                // control.IsEnabled = false; // Do not disable as the red will not show
                control.Style = null; // remove style so red will show
                //control.Foreground = System.Windows.Media.Brushes.Red;
                control.Background = System.Windows.Media.Brushes.LightPink;
                control.BorderThickness = new Thickness(2);
                control.BorderBrush = System.Windows.Media.Brushes.Red;

                control.ToolTip = "Error binding control to property: " + Environment.NewLine + property + " Please open a defect with all information,  " + Environment.NewLine + ex.Message;
            }
        }
    }
}
