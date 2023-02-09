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
using GingerCore.DataSource;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.DataSource
{
    /// <summary>
    /// Interaction logic for NewAgentPage.xaml
    /// </summary>
    public partial class RemoveTableColumnPage : Page
    {
        DataSourceTableColumn mDSTableCol = new DataSourceTableColumn();
        GenericWindow _pageGenericWin = null;
        bool okClicked = false;

        public RemoveTableColumnPage(List<string> mColNameList)
        {
            InitializeComponent();
            // mandatory fields which cannot be removed
            mColNameList.Remove("GINGER_ID");
            mColNameList.Remove("GINGER_LAST_UPDATED_BY");
            mColNameList.Remove("GINGER_LAST_UPDATE_DATETIME");

            if (mColNameList.Contains("GINGER_USED"))
                mColNameList.Remove("GINGER_USED");           

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DSColNameComboBox, ComboBox.TextProperty, mDSTableCol, nameof(DataSourceTableColumn.Name));
            DSColNameComboBox.ItemsSource = mColNameList;
        }

        private void DSTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //validate details
           if (DSColNameComboBox.SelectedItem == null) { Reporter.ToUser(eUserMsgKey.MissingNewColumn, "Column type"); return; }
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
