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

using Amdocs.Ginger.Repository;
using NJsonSchema.Infrastructure;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for UCRadioButtons.xaml
    /// </summary>
    public partial class UCRadioButtons : UserControl
    {
        object bindingObject;
        string propertyName;

        Type enumType;
        public UCRadioButtons()
        {
            InitializeComponent();
        }

       public enum eUCYesNoRadioButton
        {
            Yes,
            No
        }

        /// <summary>
        /// Generate the Radio Buttons on run time according to the Enum configured under the Action and initilized the checked Radio Button.
        /// </summary>
        /// <param name="type">Enum type for setting the titles of the Radio Buttons</param>
        /// <param name="panel">The Panel Name which the Radio Buttons will be generated in</param>
        /// <param name="act">The Created Action incstance</param>
        /// <param name="radioParam">The Field which the current selection will be save</param>
        /// <param name="extraRadioClickedHandler">The Handler for extra functionality to be implemented under the EditPage</param>
        public void Init(Type enumType,Panel panel,ActInputValue actValue, RoutedEventHandler extraRadioClickedHandler = null)
        {
            bindingObject = actValue;
            Array.ForEach((int[])Enum.GetValues(enumType),
                val =>
                {
                    
                    var button = new RadioButton() { Tag = Enum.GetName(enumType, val), Name = Enum.GetName(enumType, val), Content = GingerCore.General.GetEnumValueDescription(enumType, Enum.GetName(enumType, val)), Margin= new Thickness(5,5,0,0), Style= (Style)TryFindResource("@InputRadioButtonStyle") };
                    button.AddHandler(Button.ClickEvent, new RoutedEventHandler(RadioButton_Click));//default handler
                    if (extraRadioClickedHandler != null)
                          button.Click += extraRadioClickedHandler;//extra handler
                    panel.Children.Add(button);
                });

            string currentValue = actValue.Value;

            string[] radioTitles = Enum.GetNames(enumType);

            if (!string.IsNullOrEmpty(currentValue))
            {
                int i = 1;

                foreach (string RB in radioTitles)
                {
                    if (currentValue.Equals(RB))
                    {
                        ((RadioButton)panel.Children[i]).IsChecked = true;
                        return;
                    }
                    i++;
                }
            }
        }


        public void Init(Type enumType, Panel panel, object obj, string property, RoutedEventHandler extraRadioButtonCheckedHandler = null)
        {
            bindingObject = obj;
            propertyName = property;
            this.enumType = enumType;
            Array.ForEach((int[])Enum.GetValues(enumType),
                val =>
                {

                    var button = new RadioButton() { Tag = Enum.GetName(enumType, val), Name = Enum.GetName(enumType, val), Content = GingerCore.General.GetEnumValueDescription(enumType, Enum.GetName(enumType, val)), Margin = new Thickness(5, 5, 0, 0), Style = (Style)TryFindResource("@InputRadioButtonStyle") };
                    button.AddHandler(Button.ClickEvent, new RoutedEventHandler(RadioButton_Click));//default handler
                    if (extraRadioButtonCheckedHandler != null)
                    {
                        button.Checked += extraRadioButtonCheckedHandler;//extra handler
                    }                       
                        
                    panel.Children.Add(button);


                });

            string currentValue = obj.GetType().GetProperty(property)?.GetValue(obj)?.ToString();

            string[] radioTitles = Enum.GetNames(enumType);

            if (!string.IsNullOrEmpty(currentValue))
            {
                int i = 1;

                foreach (string RB in radioTitles)
                {
                    if (currentValue.Equals(RB))
                    {
                        ((RadioButton)panel.Children[i]).IsChecked = true;
                        return;
                    }
                    i++;
                }
            }
        }







        public void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if(bindingObject is ActInputValue)
            {
                ((ActInputValue)bindingObject).Value = (((RadioButton)sender).Tag).ToString();
            }                
            else
            {
                string value = ((RadioButton)sender).Tag?.ToString();              
                bindingObject.GetType().GetProperty(propertyName).SetValue(bindingObject, Enum.Parse(enumType, value));
            }                
        }
    }
}
