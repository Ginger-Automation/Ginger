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
using System.Collections.Generic;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.DataSource
{
    /// <summary>
    /// Interaction logic for NewAgentPage.xaml
    /// </summary>
    public partial class DataSourceTablesListPage : Page
    {
        ObservableList<DataSourceBase> mDSList = new ObservableList<DataSourceBase>();
        ObservableList<DataSourceTable> mDSTableList = new ObservableList<DataSourceTable>();
        DataSourceTable.eDSTableType mDSTableType;

        string mDataSourceName;
        string mDataSourceTableName;
        GenericWindow _pageGenericWin = null;
        bool okClicked = false;

        public DataSourceTablesListPage(DataSourceTable.eDSTableType DSTableType = DataSourceTable.eDSTableType.Customized)
        {
            InitializeComponent();
            mDSTableType = DSTableType;

            mDSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            if (mDSList.Count == 0)
            { 
                return;
            }

            List<string> mDSNames = new List<string>();
            foreach (DataSourceBase ds in mDSList)
            { 
                mDSNames.Add(ds.Name);
            }

            GingerCore.General.FillComboFromList(cmbDataSourceName, mDSNames);
            cmbDataSourceName.SelectedIndex = 0;
            mDataSourceName = mDSNames[0];            
        }       

        private void DSTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Implement DS Type Change
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //validate details
            if (cmbDataSourceName.Text.Trim() == string.Empty || cmbDataSourceTableName.Text.Trim() == string.Empty) { Reporter.ToUser(eUserMsgKey.InvalidDataSourceDetails); return; }            

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
       
        public string DSName
        {
            get
            {
                if (okClicked)
                {
                    return mDataSourceName;
                }
                else
                {
                    return "";
                }
            }
        }

        public string DSTableName
        {
            get
            {
                if (okClicked)
                {
                    return mDataSourceTableName;
                }
                else
                {
                    return "";
                }
            }
        }

        private void cmbDataSourceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (DataSourceBase ds in mDSList)
            {
                if (ds.Name == cmbDataSourceName.SelectedValue.ToString())
                {
                    mDataSourceName = cmbDataSourceName.SelectedValue.ToString();
                    //if (ds.FilePath.StartsWith("~"))
                    //{
                    //    ds.FileFullPath = ds.FilePath.Replace(@"~\", "").Replace("~", "");
                    //    ds.FileFullPath = System.IO.Path.Combine( WorkSpace.Instance.Solution.Folder, ds.FileFullPath);
                    //}
                    //
                    ds.FileFullPath = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(ds.FilePath);

                    List<string> dsTableNames = new List<string>();
                    mDSTableList = ds.GetTablesList();
                    if (mDSTableList != null)
                    {
                        ObservableList<DataSourceTable> custTableList = new ObservableList<DataSourceTable>();
                        foreach (DataSourceTable dst in mDSTableList)
                        {
                            if (dst.DSTableType == mDSTableType)
                            { 
                                dsTableNames.Add(dst.Name);
                                custTableList.Add(dst);
                            }
                        }
                        mDSTableList = custTableList;
                    }
                    GingerCore.General.FillComboFromList(cmbDataSourceTableName, dsTableNames);
                    cmbDataSourceTableName.SelectedIndex = 0;
                    if (mDSTableList != null && mDSTableList.Count > 0)
                    { 
                        mDataSourceTableName = mDSTableList[0].Name;
                    }
                    else
                    {
                        mDataSourceTableName = null;
                    }
                    break;
                }
            }
        }

        private void cmbDataSourceTableName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDataSourceTableName == null || cmbDataSourceTableName.Items.Count == 0)
            { 
                return;
            }

            foreach (DataSourceTable dst in mDSTableList)
            {
                if (dst.Name == cmbDataSourceTableName.SelectedValue.ToString())
                {
                    mDataSourceTableName = dst.Name;
                    break;
                }
            }
        }
    }
}
