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
using System.Windows;
using System.Windows.Controls;
using GingerCore;
using Ginger.Actions;

namespace Ginger.DataSource
{
    /// <summary>
    /// Interaction logic for NewAgentPage.xaml
    /// </summary>
    public partial class DataSourceExportToExcel : Page
    {
        GenericWindow _pageGenericWin = null;
        bool okClicked = false;

        public DataSourceExportToExcel(string sTableName="")
        {
            InitializeComponent();

            ExcelFilePath.Init(null, null, false, true, UCValueExpression.eBrowserType.File, "xlsx", new RoutedEventHandler(BrowseButton_Click));

            if (sTableName == "")
            {
                SheetRow.Height = new GridLength(0);
                this.Height = 30;
            }                
            else 
            {
                OutSheetName.Text = sTableName;
                SheetRow.Height = new GridLength(30);
                this.Height = 60;
            }           
        }       

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //validate details
            if (ExcelFilePath.ValueTextBox.Text.Trim() == string.Empty) { Reporter.ToUser(eUserMsgKey.MissingExcelDetails); return; }
            if (!ExcelFilePath.ValueTextBox.Text.ToLower().EndsWith(".xlsx")) { Reporter.ToUser(eUserMsgKey.InvalidExcelDetails); return; }            
            //else if (OutSheetName.Text.IndexOf(" ") > 0) { Reporter.ToUser(eUserMsgKey.InValidExportSheetDetails); return; }

            okClicked = true;

            // WorkSpace.Instance.Solution.Agents.Add(mNewAgent);

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

       
        public string SheetName
        {
            get
            {
                if (okClicked)
                {
                    return OutSheetName.Text;
                }
                else
                {
                    return "";
                }
            }
        }

        public string ExcelPath
        {
            get
            {
                if (okClicked)
                {
                    return ExcelFilePath.ValueTextBox.Text;
                }
                else
                {
                    return "";
                }
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
