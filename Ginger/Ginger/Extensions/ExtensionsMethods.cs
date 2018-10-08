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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Amdocs.Ginger.ValidationRules;
using Ginger.Actions;
using Ginger.GeneralValidationRules;
using GingerCore;
using GingerWPF.BindingLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using System.ComponentModel;
using GingerCore.Actions.Common;
using Ginger.Agents;

namespace Ginger
{
    //TODO: Rename to Controls Extension Methods
    public static class ExtensionMethods
        {
            private static Action EmptyDelegate = delegate() { };

            // all Controls of type UIElement
            public static void Refresh(this UIElement uiElement)
            {
                try
                {
                    uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
                }
                catch(Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Refresh progress bar failed", ex); // !!!!!!!!!!!!!!!!!!!!!!
                }

            }

            // ------------------------------------------------------------
            // Combo Box
            // ------------------------------------------------------------
            
            // Bind ComboBox to enum type field, list of valid values are all enum of the field selected
            public static void BindControl(this ComboBox ComboBox, Object obj, string Field)
            {
                // Get the current value so we can make it selected
                PropertyInfo PI = obj.GetType().GetProperty(Field);
                object CurrentFieldEnumValue = PI.GetValue(obj);
                if (CurrentFieldEnumValue == null || CurrentFieldEnumValue.GetType() == typeof(string))
                {
                    // if it's string like in Excel Sheet name combo then do binding to the text
                    // or we can ask for function call to return list of values - TODO: later if we need Val/text
                    GingerCore.General.ObjFieldBinding(ComboBox, ComboBox.TextProperty, obj, Field, BindingMode.TwoWay);
                }
                else
                {
                    App.FillComboFromEnumVal(ComboBox, CurrentFieldEnumValue);
                    GingerCore.General.ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty, obj, Field, BindingMode.TwoWay);
                }
            }

            // Bind Combo for enum type, but provide the subset list of enums/valid values to show
            public static void BindControl(this ComboBox ComboBox, Object obj, string Field, dynamic enumslist)
            {
                GingerCore.General.ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty, obj, Field, BindingMode.TwoWay);
                List<object> l = new List<object>();
                foreach (var v in enumslist)
                {
                    l.Add(v);
                }
                
                // Get yhe current value so it will be sleected in the combo after the list created
                PropertyInfo PI = obj.GetType().GetProperty(Field);
                object CurrentFieldEnumValue = PI.GetValue(obj);                

                App.FillComboFromEnumVal(ComboBox, CurrentFieldEnumValue, l);                 
            }

            // Bind Combo for enum type, but provide the subset list of enums/valid values to show
            // also using grouping on results, according to 
            public static void BindControlWithGrouping(this ComboBox ComboBox, Object obj, string Field, dynamic enumslist)
            {
                GingerCore.General.ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty, obj, Field, BindingMode.TwoWay);
                List<GingerCore.General.ComboGroupedEnumItem> l = new List<GingerCore.General.ComboGroupedEnumItem>();
                foreach (var v in enumslist)
                {
                    GingerCore.General.ComboGroupedEnumItem item = new GingerCore.General.ComboGroupedEnumItem();
                    item.text = GingerCore.General.GetEnumValueDescription(v.GetType(), v);
                    item.Category = GingerCore.General.GetEnumDescription(v.GetType(), v); ;
                    item.Value = v;

                    l.Add(item);
                }

                // Get yhe current value so it will be sleected in the combo after the list created
                PropertyInfo PI = obj.GetType().GetProperty(Field);
                object CurrentFieldEnumValue = PI.GetValue(obj);

                ListCollectionView lcv = new ListCollectionView(l);
                lcv.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                lcv.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
                
                App.FillComboFromEnumVal(ComboBox, CurrentFieldEnumValue, null, true, lcv);
            }

        /// <summary>
        /// Bind the combo box to ObservableList 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ComboBox"></param>
        /// <param name="obj">Object to bind to</param>
        /// <param name="Field">Object field to bind</param>
        /// <param name="list">List of Observable items to display in the combo box</param>
        /// <param name="DisplayMemberPath">list item field to display</param>
        /// <param name="SelectedValuePath">list item value to to return when selected</param>
        public static void BindControl<T>(this ComboBox ComboBox, Object obj, string Field, ObservableList<T> list, string DisplayMemberPath, string SelectedValuePath, BindingMode bindingMode = BindingMode.TwoWay)
        {
            ComboBox.ItemsSource = list;
            ComboBox.DisplayMemberPath = DisplayMemberPath;
            ComboBox.SelectedValuePath = SelectedValuePath;

            //ControlsBinding.ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty, obj, Field, bindingMode);   
            GingerCore.General.ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty, obj, Field, bindingMode);
        }
     
        // ------------------------------------------------------------
        // Validation rules
        // ------------------------------------------------------------

        public enum eValidationRule
        {
            CannotBeEmpty,
            FileExist
        }

        // ------------------------------------------------------------
        // Combo Box
        // ------------------------------------------------------------
        public static void AddValidationRule(this ComboBox comboBox, ValidationRule validationRule)
        {
            BindingExpression bd = null;
            //Check if Selected value is binded
            bd = comboBox.GetBindingExpression(ComboBox.SelectedValueProperty);
            if (bd != null)
            {
                AddValidation(comboBox, ComboBox.SelectedValueProperty, validationRule);
                return;
            }

            //Check if text is binded
            bd = comboBox.GetBindingExpression(ComboBox.TextProperty);
            if (bd != null)
            {
                AddValidation(comboBox, ComboBox.TextProperty, validationRule);
                return;
            }

            throw new Exception("trying to add rule to control which is not binded - " + comboBox.Name);
        }

        public static void AddValidationRule(this ComboBox comboBox, eValidationRule validationRule)
        {
            if (validationRule == eValidationRule.CannotBeEmpty)
            {
                comboBox.AddValidationRule(new EmptyValidationRule());
            }
        }


        // ------------------------------------------------------------
        // Text Box
        // ------------------------------------------------------------
        public static void BindControl(this ComboBox ComboBox, dynamic enumslist)
        {
                List<object> l = new List<object>();
                foreach (var v in enumslist)
                {
                    l.Add(v);
                }
                // Get yhe current value so it will be sleected in the combo after the list created
                App.FillComboFromEnumVal(ComboBox, l[0], l);
        }
        public static void BindControl(this TextBox TextBox, Object obj, string Field, BindingMode bm = BindingMode.TwoWay)
        {
            GingerCore.General.ObjFieldBinding(TextBox, TextBox.TextProperty, obj, Field, bm);
        }

        public static void BindControl(this TextBox TextBox, ActInputValue AIV)
        {                
            TextBox.BindControl(AIV, ActInputValue.Fields.Value);
        }


        public static void AddValidationRule(this TextBox textBox, ValidationRule validationRule)
        {
            AddValidation(textBox, TextBox.TextProperty, validationRule);
        }

        public static void AddValidationRule(this TextBox textBox, eValidationRule validationRule)
        {
            if (validationRule == eValidationRule.CannotBeEmpty)
            {
                AddValidation(textBox, TextBox.TextProperty, new EmptyValidationRule());
            }
        }

        // ------------------------------------------------------------
        // ucAgentControl
        // ------------------------------------------------------------
        public static void AddValidationRule(this ucAgentControl agentControl, ValidationRule validationRule)
        {
            BindingExpression bd = null;
            
            bd = agentControl.GetBindingExpression(ucAgentControl.SelectedAgentProperty);
            if (bd != null)
            {
                AddValidation(agentControl, ucAgentControl.SelectedAgentProperty, validationRule);
                return;
            }           

            throw new Exception("trying to add rule to AgentControl user control which is not binded - " + agentControl.Name);
        }

        // ------------------------------------------------------------
        // ucAgentControl
        // ------------------------------------------------------------

        public static void AddValidationRule(this Frame frame, ValidationRule validationRule)
        {
            AddValidation(frame, Frame.ContentProperty, validationRule);
        }

        // ------------------------------------------------------------
        // UCValue Expression
        // ------------------------------------------------------------

        public static void BindControl(this UCValueExpression UCValueExpression, Object obj, string Field)
            {
                GingerCore.General.ObjFieldBinding(UCValueExpression, TextBox.TextProperty, obj, "Value");
                UCValueExpression.Init(obj, Field);                
            }

        // ------------------------------------------------------------
        // check box
        // ------------------------------------------------------------
        public static void BindControl(this CheckBox checkBox, Object obj, string field)
        {
            GingerCore.General.ObjFieldBinding(checkBox, CheckBox.IsCheckedProperty, obj,  field);
            
        }


        // ------------------------------------------------------------
        // Label
        // ------------------------------------------------------------

        public static void BindControl(this Label Label, Object obj, string Field)
        {
            ControlsBinding.ObjFieldBinding(Label, Label.ContentProperty, obj, Field, BindingMode.OneWay);
        }



        // ------------------------------------------------------------
        //Image Maker
        // ------------------------------------------------------------
        public static void BindControl(this ImageMakerControl Label, Object obj, string Field)
        {
            ControlsBinding.ObjFieldBinding(Label, ImageMakerControl.ImageTypeProperty, obj, Field, BindingMode.OneWay);
        }



        // ------------------------------------------------------------
        // Frame
        // ------------------------------------------------------------
        public static void SetContent(this Frame Frame, Page Page)
        {
            // Clear history first
            if (!Frame.CanGoBack && !Frame.CanGoForward)
            {
                // do nothing    
            }
            else
            {
                // clear frame history
                var entry = Frame.RemoveBackEntry();
                while (entry != null)
                {
                    entry = Frame.RemoveBackEntry();
                }
            }

            // Set the frame content
            Frame.Content = Page;
        }

        // ------------------------------------------------------------
        // ucGrid
        // ------------------------------------------------------------

        //public static void AddValidationRule(this ucGrid ucgrid , ValidationRule validationRule)
        //{
        //    AddValidation(ucgrid, ucGrid.RowsCountProperty, validationRule);
        //}

        //public static void AddValidationRule(this ucGrid ucgrid, eValidationRule validationRule)
        //{
        //    if (validationRule == eValidationRule.CannotBeEmpty)
        //    {
        //        AddValidation(ucgrid, ucGrid.RowsCountProperty, new GridValidationRule());
        //    }
        //    //TODO: throw...
        //}

        // ------------------------------------------------------------
        // Validations
        // ------------------------------------------------------------

        public static void RemoveValidations(this FrameworkElement frameworkElement, DependencyProperty SelectedProperty)
        {
            BindingExpression bd = frameworkElement.GetBindingExpression(SelectedProperty);

            //if (frameworkElement is ucAgentControl)
            //{
            //    bd = frameworkElement.GetBindingExpression(ucAgentControl.SelectedAgentProperty);
            //}
            //else if(frameworkElement is ComboBox)
            //{
            //    bd = frameworkElement.GetBindingExpression(ComboBox.SelectedValueProperty);
            //}

            if (bd != null)
            {
                bd.ParentBinding.ValidationRules.Clear();
            }
        }

        private static void AddValidation(this FrameworkElement frameworkElement, DependencyProperty dependencyProperty, ValidationRule validationRule)
        {
            BindingExpression bd = frameworkElement.GetBindingExpression(dependencyProperty);
            bd.ParentBinding.ValidationRules.Add(validationRule);

            if (bd.ParentBinding.ValidationRules.Count > 1)
            {
                // no need to recreate the controlTemplate
                return;
            }

            bd.ParentBinding.NotifyOnValidationError = true;
          

            // This Xaml is being created in code
            //
            //  <TextBox x:Name="AgentNameTextBox"  TextWrapping="Wrap" Text="" Style="{StaticResource @TextBoxStyle}" FontWeight="Bold" Margin="10">
            //    <Validation.ErrorTemplate>
            //        <ControlTemplate>
            //            <StackPanel>
            //                <!--Placeholder for the TextBox itself-->
            //                <AdornedElementPlaceholder x:Name = "textBox" />
            //                <TextBlock Text = "{Binding [0].ErrorContent}" Foreground = "Red" />
            //            </StackPanel>
            //        </ControlTemplate>
            //    </Validation.ErrorTemplate>
            //  </TextBox >

            // Generate the above xaml programmatically

            ControlTemplate controlTemplate = new ControlTemplate();
            var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
            var stackPanelPlaceHolder = new FrameworkElementFactory(typeof(AdornedElementPlaceholder));
            stackPanel.AppendChild(stackPanelPlaceHolder);
            var textBlock = new FrameworkElementFactory(typeof(TextBlock));

            //Bind the textblock to validation error
            Binding b2 = new Binding();
            b2.Source = frameworkElement;
            //b2.Path = new PropertyPath("(Validation.Errors)[0].ErrorContent"); // OK

            // '/' = CurrentItem which is better than [0] as the list might be empty
            b2.Path = new PropertyPath("(Validation.Errors)/ErrorContent");
            b2.Mode = BindingMode.OneWay;
            b2.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            textBlock.SetBinding(TextBlock.TextProperty, b2);

            textBlock.SetValue(TextBlock.ForegroundProperty, Brushes.Red);
            stackPanel.AppendChild(textBlock);
            controlTemplate.VisualTree = stackPanel;

            // attach the control template to the text box
            Validation.SetErrorTemplate(frameworkElement, controlTemplate);
        }

        public static void ClearValidations(this FrameworkElement frameworkElement, DependencyProperty dependencyProperty)
        {
            //try
            //{
                BindingExpression bd = frameworkElement.GetBindingExpression(dependencyProperty);
                bd.ParentBinding.ValidationRules.Clear();
            //}
            //catch (Exception ex)
            //{
            //    Reporter.ToLog(eLogLevel.WARN, "Failed to clear control validations", ex, true, true)
            // }
        }

    }
}