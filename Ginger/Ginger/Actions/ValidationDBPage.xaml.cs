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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GingerCore.Environments;
using GingerCore.Actions;
using GingerCore.NoSqlBase;
using amdocs.ginger.GingerCoreNET;
using System.IO;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using Amdocs.Ginger.Common;
using System.Windows.Data;
using System.Text.RegularExpressions;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ValidationDBPage.xaml
    /// </summary>
    public partial class ValidationDBPage : Page
    {
        private ActDBValidation mValidationDB;

        ProjEnvironment pe;
        EnvApplication EA;
        Database db;

        public ValidationDBPage(ActDBValidation validationDB)
        {
            InitializeComponent();
        
            this.mValidationDB = validationDB;

            if (String.IsNullOrEmpty(mValidationDB.GetInputParamValue("SQL")))
            {
                mValidationDB.AddOrUpdateInputParamValue("SQL", mValidationDB.GetInputParamValue("Value"));
            }

            FillAppComboBox();

            //New UI Controls:
            //Query Type selection radio button :
            QueryTypeRadioButton.Init(typeof(ActDBValidation.eQueryType), SqlSelection, mValidationDB.GetOrCreateInputParam(ActDBValidation.Fields.QueryTypeRadioButton, ActDBValidation.eQueryType.FreeSQL.ToString()), QueryType_SelectionChanged);
            checkQueryType();

            //Free SQL
            //needs to be unmarked when fixed VE issue
            SQLUCValueExpression.Init(mValidationDB.GetOrCreateInputParam(ActDBValidation.Fields.SQL));

            //Read from sql file
            QueryFile.Init(mValidationDB.GetOrCreateInputParam(ActDBValidation.Fields.QueryFile), true, true, UCValueExpression.eBrowserType.File, "sql", BrowseQueryFile_Click);

            QueryFile.ValueTextBox.TextChanged += ValueTextBox_TextChanged;

            //Import SQL file in to solution folder
            GingerCore.General.ActInputValueBinding(ImportFile, CheckBox.IsCheckedProperty, mValidationDB.GetOrCreateInputParam(ActDBValidation.Fields.ImportFile, "True"));

            //OLD binding and UI 
            App.FillComboFromEnumVal(ValidationCfgComboBox, validationDB.DBValidationType);

            //TODO: fix hard coded
            App.ObjFieldBinding(ValidationCfgComboBox, ComboBox.SelectedValueProperty, validationDB,  "DBValidationType");

            App.ObjFieldBinding(AppNameComboBox, ComboBox.TextProperty, validationDB, ActDBValidation.Fields.AppName);
            App.ObjFieldBinding(DBNameComboBox, ComboBox.TextProperty, validationDB, ActDBValidation.Fields.DBName);
            App.ObjFieldBinding(TablesComboBox, ComboBox.TextProperty, validationDB, ActDBValidation.Fields.Table);
            App.ObjFieldBinding(KeySpaceComboBox, ComboBox.TextProperty, validationDB, ActDBValidation.Fields.Keyspace);
            App.ObjFieldBinding(ColumnComboBox, ComboBox.TextProperty, validationDB, ActDBValidation.Fields.Column);
            App.ObjFieldBinding(txtWhere, TextBox.TextProperty, validationDB, ActDBValidation.Fields.Where);
            GingerCore.General.ActInputValueBinding(CommitDB, CheckBox.IsCheckedProperty, mValidationDB.GetOrCreateInputParam(ActDBValidation.Fields.CommitDB));

            KeySpaceComboBox.Items.Add(mValidationDB.Keyspace);
            ComboAutoSelectIfOneItemOnly(KeySpaceComboBox);
            TablesComboBox.Items.Add(mValidationDB.Table);
            ComboAutoSelectIfOneItemOnly(TablesComboBox);
            ColumnComboBox.Items.Add(mValidationDB.Column);
            ComboAutoSelectIfOneItemOnly(ColumnComboBox);
            SetVisibleControlsForAction();
            SetQueryParamsGrid();
        }
                
        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string SolutionFolder = App.UserProfile.Solution.Folder.ToUpper();
            bool ImportFileFlag = false;
            string FileName = QueryFile.ValueTextBox.Text;
            Boolean.TryParse(mValidationDB.GetInputParamValue(ActDBValidation.Fields.ImportFile), out ImportFileFlag);
            if (ImportFileFlag && !FileName.StartsWith(@"~\"))
            {
                //TODO import request File
                string targetPath = System.IO.Path.Combine(SolutionFolder, @"Documents\SQL");
                if (!System.IO.Directory.Exists(targetPath))
                {
                    System.IO.Directory.CreateDirectory(targetPath);
                }
                string destFile = System.IO.Path.Combine(targetPath, FileName.Remove(0, FileName.LastIndexOf(@"\")+1));
                int fileNum = 1;
                string copySufix = "_Copy";
                while (System.IO.File.Exists(destFile))
                {
                    fileNum++;
                    string newFileName = System.IO.Path.GetFileNameWithoutExtension(destFile);
                    if (newFileName.IndexOf(copySufix) != -1)
                        newFileName = newFileName.Substring(0, newFileName.IndexOf(copySufix));
                    newFileName = newFileName + copySufix + fileNum.ToString() + System.IO.Path.GetExtension(destFile);
                    destFile = System.IO.Path.Combine(targetPath, newFileName);
                }
                System.IO.File.Copy(FileName, destFile, true);
                QueryFile.ValueTextBox.Text = @"~\Documents\SQL\" + System.IO.Path.GetFileName(destFile);
            }
            if (FileName != "" && File.Exists(FileName.Replace(@"~\", SolutionFolder)))
            {   
                mValidationDB.QueryParams.Clear();                
                string[] script = File.ReadAllLines(FileName.Replace(@"~\",SolutionFolder));                
                parseScriptHeader(script);               
                if (mValidationDB.QueryParams.Count > 0)
                    QueryParamsPanel.Visibility = Visibility.Visible;
                else
                    QueryParamsPanel.Visibility = Visibility.Collapsed;
                QueryParamsGrid.DataSourceList = mValidationDB.QueryParams;
               
            }
        }
        private void parseScriptHeader(string[] script)
        {
            foreach (string line in script)
            {
                var pattern = @"<<([^<^>].*?)>>"; // like div[1]
                                                  // Parse the XPath to extract the nodes on the path
                var matches = Regex.Matches(line, pattern);
                foreach(Match match in matches)
                {
                    ActInputValue AIV = (from aiv in mValidationDB.QueryParams where aiv.Param == match.Groups[1].Value select aiv).FirstOrDefault();
                    if (AIV == null)
                    {
                        AIV = new ActInputValue();
                        // AIV.Active = true;

                        AIV.Param = match.Groups[1].Value;
                        mValidationDB.QueryParams.Add(AIV);
                        AIV.Value = "";                        
                    }                    
                }
            }
        }
        private void SetQueryParamsGrid()
        {
            //Show/hide if needed
            //QueryParamsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddInputValue));//?? going to be hide in next line code

            QueryParamsGrid.SetTitleLightStyle = true;
            QueryParamsGrid.ClearTools();
            QueryParamsGrid.ShowDelete = System.Windows.Visibility.Visible;

            //List<GridColView> view = new List<GridColView>();
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = ActInputValue.Fields.Param, WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = ActInputValue.Fields.Value, WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["QueryParamExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = ActInputValue.Fields.ValueForDriver, Header = "Value ForDriver", WidthWeight = 150, BindingMode = BindingMode.OneWay });

            QueryParamsGrid.SetAllColumnsDefaultView(view);
            QueryParamsGrid.InitViewItems();
        }

        private void ComboAutoSelectIfOneItemOnly(ComboBox comboBox)
        {
            if (comboBox.Items.Count == 1)
            {
                comboBox.SelectedItem = comboBox.Items[0];
            }
        }

        
        /// <summary>
        /// Fill the environments Applications combo box
        /// </summary>
        private void FillAppComboBox()
        {            
            AppNameComboBox.Items.Clear();

            if (App.AutomateTabEnvironment != null)
            {                
                pe = (from e in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>() where e.Name == App.AutomateTabEnvironment.Name select e).FirstOrDefault();

                if (pe == null)
                {
                    return;
                }
                foreach (EnvApplication ea in pe.Applications)
                {
                    AppNameComboBox.Items.Add(ea.Name);
                }
                ComboAutoSelectIfOneItemOnly(AppNameComboBox);
            }
        }

        private void AppNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: if there is only one item in the combo auto select it
            DBNameComboBox.Items.Clear();
            KeySpaceComboBox.Items.Clear();
            TablesComboBox.Items.Clear();
            ColumnComboBox.Items.Clear();

            if ((((ComboBox)sender).SelectedItem) == null) return;

            string app = ((ComboBox)sender).SelectedItem.ToString();
            EA = (from a in pe.Applications where a.Name == app select a).FirstOrDefault();
            foreach (Database db in EA.Dbs)
            {
                DBNameComboBox.Items.Add(db.Name);
                
            }
            ComboAutoSelectIfOneItemOnly(DBNameComboBox);
        }

        private void DBNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeySpaceComboBox.Items.Clear();
            TablesComboBox.Items.Clear();
            ColumnComboBox.Items.Clear();
        }

        private void TablesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            ColumnComboBox.Items.Clear();
        }

        private void KeySpaceComboBox_DropDownOpened(object sender, EventArgs e)
        {
            KeySpaceComboBox.Items.Clear();
            string DBName = DBNameComboBox.Text;
            db = (from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
            if (db == null) return;
            if (db.DBType == Database.eDBTypes.Cassandra)
            {
                NoSqlBase NoSqlDriver = null;
                NoSqlDriver = new GingerCassandra(db);

                List<string> keyspace = NoSqlDriver.GetKeyspaceList();
                foreach (string s in keyspace)
                {
                    KeySpaceComboBox.Items.Add(s);
                }
            }
        }

        private void TablesComboBox_DropDownOpened(object sender, EventArgs e)
        {
            TablesComboBox.Items.Clear();
            string DBName = DBNameComboBox.Text; 
            db = (from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
            if (db == null) return;
            string KeySpace = KeySpaceComboBox.Text;
            List<string> Tables = db.GetTablesList(KeySpace);
                foreach (string s in Tables)
                {
                    TablesComboBox.Items.Add(s);
                }
        }
        
        private void ColumnComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ColumnComboBox.Items.Clear();
            string DBName = DBNameComboBox.Text;
            db = (from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
            if (db == null) return;
            string table;
            if (db.DBType == Database.eDBTypes.Cassandra)
            {
                 table = KeySpaceComboBox.Text + "." + TablesComboBox.Text;
            }
            else
            {
                table = TablesComboBox.Text;
            }
            List<string> Columns = db.GetTablesColumns(table);
            foreach (string s in Columns)
            {
                ColumnComboBox.Items.Add(s);
            }
        }

        private void ValidationCfgComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetVisibleControlsForAction();
        }

        private void SetVisibleControlsForAction()
        {
            if (ValidationCfgComboBox.SelectedItem == null)
            {
                RadioButtonsSection.Visibility = System.Windows.Visibility.Visible;
                FreeSQLStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                SqlFile.Visibility = System.Windows.Visibility.Collapsed;
                FreeSQLStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                DoCommit.Visibility = System.Windows.Visibility.Collapsed;
                Keyspace.Visibility = System.Windows.Visibility.Collapsed;
                TableColWhereStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            //Ugly code but working, find way to make it simple use the enum val from combo
            ActDBValidation.eDBValidationType validationType = (ActDBValidation.eDBValidationType)ValidationCfgComboBox.SelectedValue;

            switch (validationType)
            {
                case ActDBValidation.eDBValidationType.UpdateDB:
                    RadioButtonsSection.Visibility = System.Windows.Visibility.Visible;
                    checkQueryType();
                    TableColWhereStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    FreeSQLLabel.Content = "Update DB SQL:";
                    DoCommit.Visibility = System.Windows.Visibility.Visible;
                    Keyspace.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case ActDBValidation.eDBValidationType.FreeSQL:
                    checkQueryType();
                    RadioButtonsSection.Visibility = System.Windows.Visibility.Visible;
                    if (mValidationDB.GetInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton) == ActDBValidation.eQueryType.FreeSQL.ToString())
                    {
                        FreeSQLStackPanel.Visibility = System.Windows.Visibility.Visible;
                        SqlFile.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else
                    {
                        SqlFile.Visibility = System.Windows.Visibility.Visible;
                        FreeSQLStackPanel.Visibility = System.Windows.Visibility.Collapsed;

                        if(mValidationDB.QueryParams != null)
                        {
                            if (mValidationDB.QueryParams.Count > 0)
                                QueryParamsPanel.Visibility = Visibility.Visible;
                            else
                                QueryParamsPanel.Visibility = Visibility.Collapsed;
                            QueryParamsGrid.DataSourceList = mValidationDB.QueryParams;
                        }
                    }
                    DoCommit.Visibility = System.Windows.Visibility.Collapsed;
                    TableColWhereStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    FreeSQLLabel.Content = "Free SQL:";
                    Keyspace.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case ActDBValidation.eDBValidationType.SimpleSQLOneValue:
                    checkQueryType();
                    try
                    {
                        string DBName = DBNameComboBox.Text;
                        db = (from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
                        if (!(db == null))
                        {
                            if (db.DBType == Database.eDBTypes.Cassandra)
                            {
                                Keyspace.Visibility = System.Windows.Visibility.Visible;
                            }
                            else
                            {
                                Keyspace.Visibility = System.Windows.Visibility.Collapsed;
                            }
                        }
                    }
                    catch { }
                    FreeSQLStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    RadioButtonsSection.Visibility = System.Windows.Visibility.Collapsed;
                    TableColWhereStackPanel.Visibility = System.Windows.Visibility.Visible;
                    DoCommit.Visibility = System.Windows.Visibility.Collapsed;
                    SqlFile.Visibility = System.Windows.Visibility.Collapsed;                    
                    break;
                case ActDBValidation.eDBValidationType.RecordCount:
                    checkQueryType();
                    try
                    {
                        string DBName = DBNameComboBox.Text;
                        db = (from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
                        if (!(db == null))
                        {
                            if (db.DBType == Database.eDBTypes.Cassandra)
                            {
                                Keyspace.Visibility = System.Windows.Visibility.Visible;
                            }
                            else
                            {
                                Keyspace.Visibility = System.Windows.Visibility.Collapsed;
                            }
                        }
                    }
                    catch { }
                    RadioButtonsSection.Visibility = System.Windows.Visibility.Collapsed;
                    FreeSQLStackPanel.Visibility = System.Windows.Visibility.Visible;
                    TableColWhereStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    DoCommit.Visibility = System.Windows.Visibility.Collapsed;
                    SqlFile.Visibility = System.Windows.Visibility.Collapsed;
                    FreeSQLLabel.Content = @"Record count - SELECT COUNT(1) FROM {Table} - Enter only Table name below (+optional WHERE clause)";
                    break;

            }
        }
        
        private void DBNamVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mValidationDB, ActDBValidation.Fields.DBName);
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            DBNameComboBox.Text = mValidationDB.DBName;
        }
        
        private void AppNamVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mValidationDB, ActDBValidation.Fields.AppName);
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            AppNameComboBox.Text = mValidationDB.AppName;
        }

        private void TablesVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mValidationDB, ActDBValidation.Fields.Table);
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            TablesComboBox.Text = mValidationDB.Table;
        }

        private void ColumnsVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mValidationDB, ActDBValidation.Fields.Column);
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            ColumnComboBox.Text = mValidationDB.Column;
        }

        private void KeySpaceVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mValidationDB, ActDBValidation.Fields.Keyspace);
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            KeySpaceComboBox.Text = mValidationDB.Keyspace;
        }

        public void QueryType_SelectionChanged(object sender, RoutedEventArgs e)
        {
            mValidationDB.AddOrUpdateInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton, (((RadioButton)sender).Tag).ToString());
            if (mValidationDB.GetInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton) == ActDBValidation.eQueryType.FreeSQL.ToString())
            {

                SqlFile.Visibility = System.Windows.Visibility.Collapsed;
                FreeSQLStackPanel.Visibility = System.Windows.Visibility.Visible;

            }
            else if (mValidationDB.GetInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton) == ActDBValidation.eQueryType.SqlFile.ToString())
            {
                SqlFile.Visibility = System.Windows.Visibility.Visible;
                FreeSQLStackPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        } //TODO populate field selection changed

        public void checkQueryType()
        {
            if (mValidationDB.GetInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton) == ActDBValidation.eQueryType.FreeSQL.ToString())
            {
                FreeSQLStackPanel.Visibility = System.Windows.Visibility.Visible;
                SqlFile.Visibility = System.Windows.Visibility.Collapsed;               
            }
            else 
            {
                SqlFile.Visibility = System.Windows.Visibility.Visible;
                FreeSQLStackPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public void BrowseQueryFile_Click(object sender, RoutedEventArgs e)
        {
            string SolutionFolder = App.UserProfile.Solution.Folder.ToUpper();
            if (!String.IsNullOrEmpty(QueryFile.ValueTextBox.Text))
            {
                if(!System.IO.File.Exists(QueryFile.ValueTextBox.Text))
                {
                    return;
                }
                string FileName = QueryFile.ValueTextBox.Text.ToUpper();
                if (FileName.Contains(SolutionFolder))
                {
                    FileName = FileName.Replace(SolutionFolder, @"~\");
                }

                QueryFile.ValueTextBox.Text = FileName;                
            }
        }

        private void QueryParamGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)QueryParamsGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, ActInputValue.Fields.Value);
            VEEW.ShowAsWindow();
        }
    }
}
