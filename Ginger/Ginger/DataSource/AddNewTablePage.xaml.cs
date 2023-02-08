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
using System.Windows;
using System.Windows.Controls;
using GingerCore;
using GingerCore.DataSource;

namespace Ginger.DataSource
{
    /// <summary>
    /// Interaction logic for NewAgentPage.xaml
    /// </summary>
    public partial class AddNewTablePage : Page
    {
        DataSourceTable mDSTableDetails = new DataSourceTable();
        GenericWindow _pageGenericWin = null;
        bool okClicked = false;

        public AddNewTablePage()
        {
            InitializeComponent();
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TableName, TextBox.TextProperty, mDSTableDetails, DataSourceTable.Fields.Name);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DSTableTypeComboBox, ComboBox.SelectedValueProperty, mDSTableDetails, DataSourceTable.Fields.DSTableType);
            DSTableTypeComboBox.SelectionChanged += DSTypeComboBox_SelectionChanged;
            GingerCore.General.FillComboFromEnumObj(DSTableTypeComboBox, mDSTableDetails.DSTableType);
            DSTableTypeComboBox.SelectedIndex = 1;
        }       

        private void DSTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //validate details
            if (TableName.Text.Trim() == string.Empty) { Reporter.ToUser(eUserMsgKey.MissingNewTableDetails, "name"); return; }
            else if (TableName.Text.IndexOf(" ") > 0) { Reporter.ToUser(eUserMsgKey.InvalidTableDetails); return; }
            else if (DSTableTypeComboBox.SelectedItem == null) { Reporter.ToUser(eUserMsgKey.MissingNewTableDetails, "Type"); return; }

            okClicked = true;
            _pageGenericWin.Close();
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button okBtn = new Button();
            okBtn.Content = "OK";
            okBtn.Click += new RoutedEventHandler(OKButton_Click);
            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(okBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, true, "Cancel");
        }
       
        public DataSourceTable DSTableDetails
        {
            get
            {
                if (okClicked)
                {
                    return mDSTableDetails;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
