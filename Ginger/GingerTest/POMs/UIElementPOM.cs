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

using GingerCore.GeneralLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GingerWPFUnitTest.POMs
{
    public class UIElementPOM : GingerPOMBase
    {
        public DependencyObject dependencyObject { get; set; }

        public UIElementPOM(DependencyObject dependencyObject)
        {
            this.dependencyObject = dependencyObject;
        }


        /// <summary>
        /// Set TextBox text value
        /// </summary>
        /// <param name="txt"></param>
        public void SetText(string txt)
        {
            Execute(() => { 
                ((TextBox)dependencyObject).Text = txt;
            });
        }



        /// <summary>
        /// Get TextBlock text value
        /// </summary>
        /// <param name="txt"></param>
        public string Text
        {
            get
            {
                string text = null;
                Execute(() =>
                {
                    text = ((TextBlock)dependencyObject).Text;
                });
                return text;
            }
        }

        /// <summary>
        /// Set the selected item of combo box 
        /// </summary>
        /// <param name="obj"></param>
        public void SelectValue(object obj)
        {
            Execute(() => {
                ComboBox combo = (ComboBox)dependencyObject;
                foreach (var x in combo.Items)
                {
                    ComboEnumItem item = (ComboEnumItem)x;
                    //TODO:  Not working check why
                    //if (item.Value == obj)
                    //{
                    //}
                    // Meanwhile use toString which works
                    if (item.Value.ToString() == obj.ToString())
                    {
                        combo.SelectedValue = item;
                    }
                }
                // TODO: fail back to find by string if not found by obj so use the .ToString in 2nd option
                
            });
        }

        /// <summary>
        /// Set the Checked Value of check box 
        /// </summary>
        /// <param name="obj"></param>
        public void SetCheckedValue(bool value)
        {
            Execute(() => {
                ((CheckBox)dependencyObject).IsChecked = value;
            });
        }



        /// <summary>
        /// Set the selected item of combo box 
        /// </summary>
        /// <param name="obj"></param>
        public void SelectedIndex(int index)
        {
            Execute(() => {
                ((ComboBox)dependencyObject).SelectedIndex = index;
            });
        }


        public void DoMouseEnterEvent()
        {
            UIElement b = (UIElement)dependencyObject; 
            MouseEventArgs e = new MouseEventArgs(Mouse.PrimaryDevice, 0);
            e.RoutedEvent = Mouse.MouseEnterEvent;
            b.RaiseEvent(e);
            SleepWithDoEvents(100);
        }

    }
}
