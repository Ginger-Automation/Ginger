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
using GingerCore.Actions;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Ginger.Drivers.PowerBuilder;
using System.Windows.Automation;

namespace Ginger.WindowExplorer.PowerBuilder
{
    /// <summary>
    /// Interaction logic for DataGridInfoPage.xaml
    /// </summary>
    public partial class DataGridInfoPage : Page
    {
        private System.Windows.Automation.AutomationElement AEControl;

        public DataGridInfoPage(System.Windows.Automation.AutomationElement AEControl)
        {
            InitializeComponent();
            this.AEControl = AEControl;
            ShowGridData();
            ShowDetails();
        }

        private void ShowGridData()
        {             
            DataTable tempTable = new DataTable("table");
            AutomationElement AEHeader = TreeWalker.RawViewWalker.GetFirstChild(AEControl);
            
            //Calculate total cells of Grid
            while (AEHeader != null) 
            {
                string ColName = AEHeader.GetCurrentPropertyValue(ValuePatternIdentifiers.ValueProperty).ToString();
                ColName += " (" + AEHeader.Current.Name + ")";
                tempTable.Columns.Add(ColName);
                AEHeader = TreeWalker.RawViewWalker.GetNextSibling(AEHeader);
                if (AEHeader.Current.ControlType != ControlType.Text) break;
            }

            AutomationElement AECell = AEHeader;
            while (AECell != null)
            {
                DataRow dr = tempTable.NewRow();
                for (int j = 0; j < tempTable.Columns.Count; j++)
                {
                    //TODO: based on cell type get the value for check box get is checked
                    dr[j] = AECell.GetCurrentPropertyValue(ValuePatternIdentifiers.ValueProperty).ToString();
                    AECell = TreeWalker.RawViewWalker.GetNextSibling(AECell);
                    if (AECell == null) break;
                }
                tempTable.Rows.Add(dr);
            }
            GridData.AutoGenerateColumns = true;
            GridData.ItemsSource = tempTable.DefaultView;
        }
        
        public void ShowDetails()
        {
            grdCondition.Visibility = Visibility.Collapsed;
            grdIndex.Visibility = Visibility.Collapsed;
        }
      /// <summary>
        /// Action on selecting a value from the drop down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectByCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rowFilter.Text = "";
            colFilter.Text = "";
            rowFilterIndex.Text = "";
            colFilterIndex.Text = "";
            colFilterRandom.Text = "";
            string selectBy = SelecColumnByComboBox.SelectedItem.ToString();
            if (selectBy.Equals("Condition"))
            {
                grdCondition.Visibility = Visibility.Visible;
                grdIndex.Visibility = Visibility.Collapsed;
                grdRandom.Visibility = Visibility.Collapsed;
            }
            else if (selectBy.Equals("Index"))
            {
                grdCondition.Visibility = Visibility.Collapsed;
                grdIndex.Visibility = Visibility.Visible;
                grdRandom.Visibility = Visibility.Collapsed;
            }
            else if (selectBy.Equals("Random"))
            {
                grdCondition.Visibility = Visibility.Collapsed;
                grdIndex.Visibility = Visibility.Collapsed;
                grdRandom.Visibility = Visibility.Visible;
            }
        }
              
        /// <summary>
        /// Filtering the grid based on condition button action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_FilterByCondition(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Filtering the grid based on only column button action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_FilterByColumns(object sender, RoutedEventArgs e)
        {
        }
    }
}
