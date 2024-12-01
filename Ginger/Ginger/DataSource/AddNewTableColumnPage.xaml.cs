#region License
/*
Copyright © 2014-2024 European Support Limited

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
using GingerCore.DataSource;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.DataSource
{
    /// <summary>
    /// Interaction logic for NewAgentPage.xaml
    /// </summary>
    public partial class AddNewTableColumnPage : Page
    {
        DataSourceTableColumn mDSTableCol = new DataSourceTableColumn();
        GenericWindow _pageGenericWin = null;
        bool okClicked = false;

        public AddNewTableColumnPage()
        {
            InitializeComponent();

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DSColumnName, TextBox.TextProperty, mDSTableCol, nameof(DataSourceTableColumn.Name));
        }

        private void DSTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //validate details
            if (DSColumnName.Text.Trim() == string.Empty) { Reporter.ToUser(eUserMsgKey.MissingNewColumn, "name"); return; }
            if (DSColumnName.Text.ToLower() is "no" or "key") { Reporter.ToUser(eUserMsgKey.InvalidColumnName); return; }

            okClicked = true;
            _pageGenericWin.Close();
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button okBtn = new Button
            {
                Content = "OK"
            };
            okBtn.Click += new RoutedEventHandler(OKButton_Click);
            ObservableList<Button> winButtons = [okBtn];

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, true, "Cancel");
        }

        public DataSourceTableColumn DSTableCol
        {
            get
            {
                if (okClicked)
                {
                    return mDSTableCol;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}