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
using GingerCore.Variables;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.Variables
{
    /// <summary>
    /// Interaction logic for VariableDateTimePage.xaml
    /// </summary>
    public partial class VariableDateTimePage : Page
    {
        private VariableDateTime variableDateTime;
        public VariableDateTimePage(VariableDateTime varDateTime)
        {
            variableDateTime = varDateTime;
            InitializeComponent();
            
            BindControlValue();

        }

        private void BindControlValue()
        {
            if (!string.IsNullOrEmpty(variableDateTime.InitialDateTime))
            {
                dtpInitialDate.Value = Convert.ToDateTime(variableDateTime.InitialDateTime);
            }

            dtpInitialDate.CustomFormat = variableDateTime.DateTimeFormat;
            dtpInitialDate.MinDate = Convert.ToDateTime(variableDateTime.MinDateTime);
            dtpInitialDate.MaxDate = Convert.ToDateTime(variableDateTime.MaxDateTime);
            
            dpMinDate.Value = Convert.ToDateTime(variableDateTime.MinDateTime);
            dpMinDate.CustomFormat = variableDateTime.DateTimeFormat;

            dpMaxDate.Value = Convert.ToDateTime(variableDateTime.MaxDateTime);
            dpMaxDate.CustomFormat = variableDateTime.DateTimeFormat;

            txtDateFormat.Text = variableDateTime.DateTimeFormat;

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtDateFormat, ComboBox.TextProperty,variableDateTime,nameof(VariableDateTime.DateTimeFormat), System.Windows.Data.BindingMode.TwoWay);
            txtDateFormat.AddValidationRule(new DateTimeFormatValidationRule(variableDateTime));
        }


        private void dpMinDate_TextChanged(object sender, EventArgs e)
        {
            if (dpMinDate.Value <= dtpInitialDate.MaxDate)
            {
                dtpInitialDate.MinDate = dpMinDate.Value;
                variableDateTime.MinDateTime = dpMinDate.Value.ToString();
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR,$"Minimum date :[{dpMinDate.Value}], should be <= Maximum Date:[{dtpInitialDate.MaxDate}]");
                dpMinDate.Focus();
                return;
            }
        }

        private void dpMaxDate_TextChanged(object sender, EventArgs e)
        {
            if(dpMaxDate.Value >= dtpInitialDate.MinDate)
            {
                dtpInitialDate.MaxDate = dpMaxDate.Value;
                variableDateTime.MaxDateTime = dpMaxDate.Value.ToString();
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Maximum date :[{dpMaxDate.Value}], should be >= Minimum Date:[{dtpInitialDate.MinDate}]");
                dpMaxDate.Focus();
                return;
            }
                
        }

        private void dtpInitialDate_TextChanged(object sender, EventArgs e)
        {
            if (!variableDateTime.CheckDateTimeWithInRange(dtpInitialDate.Value.ToString()))
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Input Value is not in range:- Maximum date :[{variableDateTime.MaxDateTime}], Minimum Date:[{variableDateTime.MinDateTime}]");
                dtpInitialDate.Focus();
                return;
            }
            else
            {
                variableDateTime.InitialDateTime = dtpInitialDate.Value.ToString();
            }
        }

        private void UpdateIntialDateTimePicker()
        {
            if (!String.IsNullOrEmpty(((System.Windows.Controls.ComboBoxItem)txtDateFormat.SelectedValue).Content.ToString()))
            {
                dtpInitialDate.CustomFormat = ((System.Windows.Controls.ComboBoxItem)txtDateFormat.SelectedValue).Content.ToString();
                if (dpMinDate != null || dpMaxDate != null)
                {
                    dpMinDate.CustomFormat = ((System.Windows.Controls.ComboBoxItem)txtDateFormat.SelectedValue).Content.ToString();
                    dpMaxDate.CustomFormat = ((System.Windows.Controls.ComboBoxItem)txtDateFormat.SelectedValue).Content.ToString();
                }
            }
            
        }

        private void txtDateFormat_TextChanged(object sender, SelectionChangedEventArgs e)
        {
            // this inner method checks if user is still typing
            //async Task<bool> UserKeepsTyping()
            //{
            //    var txt = txtDateFormat.Text;
            //    await Task.Delay(1000);
            //    return txt != txtDateFormat.Text;
            //}
            //if (await UserKeepsTyping() || dtpInitialDate.CustomFormat == txtDateFormat.Text) return;
            //dtpInitialDate.CustomFormat = txtDateFormat.Text;
            variableDateTime.DateTimeFormat = txtDateFormat.Text;
            UpdateIntialDateTimePicker();
        }
    }
}
