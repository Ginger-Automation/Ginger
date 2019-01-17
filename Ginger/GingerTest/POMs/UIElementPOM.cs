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

using Amdocs.Ginger.UserControls;
using Ginger.Agents;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                ((ComboBox)dependencyObject).SelectedValue = obj;
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
