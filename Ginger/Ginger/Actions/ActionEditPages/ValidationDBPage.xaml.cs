#region License
/*
Copyright © 2014-2025 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using Ginger.UserControlsLib.TextEditor.Common;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerCore.Helpers;
using GingerCore.NoSqlBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using static GingerCore.Actions.ActDBValidation;
using static GingerCore.Environments.Database;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ValidationDBPage.xaml
    /// </summary>
    public partial class ValidationDBPage : Page
    {
        private ActDBValidation mAct;

        ProjEnvironment pe;
        EnvApplication EA;
        Database db;

        public ValidationDBPage(ActDBValidation act)
        {
            InitializeComponent();

            this.mAct = act;

            if (String.IsNullOrEmpty(mAct.GetInputParamValue("SQL")))
            {
                mAct.AddOrUpdateInputParamValue("SQL", mAct.GetInputParamValue("Value"));
            }

            FillAppComboBox();

            //New UI Controls:
            //Query Type selection radio button :
            QueryTypeRadioButton.Init(typeof(ActDBValidation.eQueryType), SqlSelection, mAct.GetOrCreateInputParam(ActDBValidation.Fields.QueryTypeRadioButton, ActDBValidation.eQueryType.FreeSQL.ToString()), QueryType_SelectionChanged);
            checkQueryType();

            //Free SQL
            //needs to be unmarked when fixed VE issue
            SQLUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActDBValidation.Fields.SQL));

            //Read from sql file
            QueryFile.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActDBValidation.Fields.QueryFile), true, true, UCValueExpression.eBrowserType.File, "sql", BrowseQueryFile_Click, WorkSpace.Instance.SolutionRepository.SolutionFolder);

            WeakEventManager<TextBoxBase, TextChangedEventArgs>.AddHandler(source: QueryFile.ValueTextBox, eventName: nameof(TextBoxBase.TextChanged), handler: ValueTextBox_TextChanged);

            //OLD binding and UI
            GingerCore.General.FillComboFromEnumObj(ValidationCfgComboBox, act.DBValidationType);

            //TODO: fix hard coded
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ValidationCfgComboBox, ComboBox.SelectedValueProperty, act, "DBValidationType");

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(AppNameComboBox, ComboBox.TextProperty, act, ActDBValidation.Fields.AppName);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DBNameComboBox, ComboBox.TextProperty, act, ActDBValidation.Fields.DBName);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TablesComboBox, ComboBox.TextProperty, act, ActDBValidation.Fields.Table);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(KeySpaceComboBox, ComboBox.TextProperty, act, ActDBValidation.Fields.Keyspace);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ColumnComboBox, ComboBox.TextProperty, act, ActDBValidation.Fields.Column);

            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(CommitDB, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActDBValidation.Fields.CommitDB));

            txtInsertJson.ValueTextBox.Text = string.Empty;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtInsertJson, TextBox.TextProperty, act, nameof(ActDBValidation.InsertJson), BindingMode.TwoWay);
            txtInsertJson.BindControl(Context.GetAsContext(act.Context), act, nameof(ActDBValidation.InsertJson));
            txtInsertJson.Init(Context.GetAsContext(act.Context), act.GetOrCreateInputParam(nameof(act.InsertJson), string.Empty), true, false);
            txtInsertJson.ValueTextBox.AddValidationRule(new RunSetLib.CreateCLIWizardLib.ValidateJsonFormat());

            txtInsertJson.AdjustHight(200);

            txtPrimaryKey.Init(Context.GetAsContext(act.Context), act.GetOrCreateInputParam(nameof(act.PrimaryKey),
                string.Empty), true, false);

            txtPartitionKey.Init(Context.GetAsContext(act.Context), act.GetOrCreateInputParam(nameof(act.PartitionKey),
                string.Empty), true, false);

            KeySpaceComboBox.Items.Add(mAct.Keyspace);
            ComboAutoSelectIfOneItemOnly(KeySpaceComboBox);
            TablesComboBox.Items.Add(mAct.Table);
            ComboAutoSelectIfOneItemOnly(TablesComboBox);
            ColumnComboBox.Items.Add(mAct.Column);
            ComboAutoSelectIfOneItemOnly(ColumnComboBox);
            SetVisibleControlsForAction();
            SetQueryParamsGrid();
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtWhere, TextBox.TextProperty, act, ActDBValidation.Fields.Where);
        }
    
        SelectedContentArgs mSelectedContentArgs;
        public ValidationDBPage(SelectedContentArgs selectedContentArgs,ActDBValidation act)
      {
            InitializeComponent();
            mSelectedContentArgs = selectedContentArgs;
            string selValueExpression = selectedContentArgs.TextEditor.Text.Substring(selectedContentArgs.StartPos, selectedContentArgs.Length);

            mAct = act; 
            string envAppPattern = @"EnvApp=([^}\s]+)";
            string dbNamePattern = @"EnvAppDB=([^}\s]+)";
            string queryPattern = @"Query=([^}]+)";

            string EnvApplication = Regex.Match(selValueExpression, envAppPattern).Groups[1].Value;
            string dbName = Regex.Match(selValueExpression, dbNamePattern).Groups[1].Value;
            string query = Regex.Match(selValueExpression, queryPattern).Groups[1].Value;


            GingerCore.General.FillComboFromEnumObj(ValidationCfgComboBox, act.DBValidationType);
            QueryTypeRadioButton.Init(typeof(ActDBValidation.eQueryType), SqlSelection, mAct.GetOrCreateInputParam(ActDBValidation.Fields.QueryTypeRadioButton, ActDBValidation.eQueryType.FreeSQL.ToString()), QueryType_SelectionChanged);
            checkQueryType();
            AppNameComboBox.SelectionChanged -= AppNameComboBox_SelectionChanged;
            DBNameComboBox.SelectionChanged -= DBNameComboBox_SelectionChanged;
            RadioButtonsSection.IsVisibleChanged += RadioButtonsSection_IsVisibleChanged;
            AppNameComboBox.ItemsSource = new object[] { EnvApplication };
            AppNameComboBox.SelectedIndex = 0;
            DBNameComboBox.ItemsSource = new object[] { dbName };
            DBNameComboBox.SelectedIndex = 0;
            AppNameComboBox.IsEditable = false;
            DBNameComboBox.IsEditable = false;
            ValidationCfgComboBox.SelectedIndex = 0;
            ValidationCfgComboBox.IsEnabled = false;
            SQLUCValueExpression.ValueTextBox.Text= query.TrimEnd(' ');
            SetVisibleControlsForAction();
            SQLUCValueExpression.OpenExpressionEditorButton.Visibility = Visibility.Collapsed;
        }
        public void UpdateContent()
        {
            string txt = mSelectedContentArgs.TextEditor.Text[..mSelectedContentArgs.StartPos];
            txt += GetValue();
            txt += mSelectedContentArgs.TextEditor.Text[(mSelectedContentArgs.EndPos + 1)..];
            mSelectedContentArgs.TextEditor.Text = txt;
        }
        private void RadioButtonsSection_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is UIElement element && element.Visibility == Visibility.Visible)
            {
                element.Visibility = Visibility.Collapsed;
            }
        }

        private string GetValue()
        {
            string envApp = AppNameComboBox?.SelectedValue?.ToString();
            string envAppDB = DBNameComboBox?.SelectedValue?.ToString();
            string query = SQLUCValueExpression?.ValueTextBox?.Text;

            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(envApp)) 
            { 
                errors.Add("EnvApp"); 
            }
            if (string.IsNullOrWhiteSpace(envAppDB)) 
            { 
                errors.Add("EnvAppDB"); 
            }
            if (string.IsNullOrWhiteSpace(query)) 
            {
                errors.Add("Query"); 
            }

            if (errors.Any())
            {
                errors.ForEach(param => Reporter.ToLog(eLogLevel.ERROR, $"{param} is null or empty."));
                return "";
            }

            // Simplified TextBlockHelper usage
            var a = new TextBlockHelper(new TextBlock());
            a.AddText("{");
            a.AddBoldText("EnvApp=");
            a.AddText($"{envApp} ");
            a.AddBoldText("EnvAppDB=");
            a.AddText($"{envAppDB} ");
            a.AddBoldText("Query=");
            a.AddText($"{query}");
            a.AddText("}");
           return a.GetText();
        }


        private async void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(QueryFile.ValueTextBox.Text))
            {
                async Task<bool> UserKeepsTyping()
                {
                    string txt = QueryFile.ValueTextBox.Text;
                    await Task.Delay(2000);
                    return txt != QueryFile.ValueTextBox.Text;
                }
                if (await UserKeepsTyping() || QueryFile.ValueTextBox.Text == null)
                {
                    return;
                }
            }
            string FileName = QueryFile.ValueTextBox.Text;
            if (FileName != "" && File.Exists(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(FileName)))
            {
                parseScriptHeader(FileName);
            }
        }

        public void parseScriptHeader(string FileName)
        {
            mAct.QueryParams.Clear();
            string[] script = File.ReadAllLines(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(FileName));

            foreach (string line in script)
            {
                var pattern = @"<<([^<^>].*?)>>"; // like div[1]
                                                  // Parse the XPath to extract the nodes on the path
                var matches = Regex.Matches(line, pattern);
                foreach (Match match in matches)
                {
                    ActInputValue AIV = (from aiv in mAct.QueryParams where aiv.Param == match.Groups[1].Value select aiv).FirstOrDefault();
                    if (AIV == null)
                    {
                        AIV = new ActInputValue
                        {
                            // AIV.Active = true;

                            Param = match.Groups[1].Value
                        };
                        mAct.QueryParams.Add(AIV);
                        AIV.Value = "";
                    }
                }
            }

            if (mAct.QueryParams.Count > 0)
            {
                QueryParamsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                QueryParamsPanel.Visibility = Visibility.Collapsed;
            }

            QueryParamsGrid.DataSourceList = mAct.QueryParams;
        }
        private void SetQueryParamsGrid()
        {
            //Show/hide if needed
            //QueryParamsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddInputValue));//?? going to be hide in next line code

            QueryParamsGrid.SetTitleLightStyle = true;
            QueryParamsGrid.ClearTools();
            QueryParamsGrid.ShowDelete = System.Windows.Visibility.Visible;

            //List<GridColView> view = new List<GridColView>();
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ActInputValue.Param), WidthWeight = 150 },
                new GridColView() { Field = nameof(ActInputValue.Value), WidthWeight = 150 },
                new GridColView() { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["QueryParamExpressionButton"] },
                new GridColView() { Field = nameof(ActInputValue.ValueForDriver), Header = "Value ForDriver", WidthWeight = 150, BindingMode = BindingMode.OneWay },
            ]
            };

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

            if (Context.GetAsContext(mAct.Context) != null && Context.GetAsContext(mAct.Context).Environment != null)
            {
                pe = (from e in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>() where e.Name == Context.GetAsContext(mAct.Context).Environment.Name select e).FirstOrDefault();

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
            txtWhere.Clear();

            if ((((ComboBox)sender).SelectedItem) == null)
            {
                return;
            }

            string app = ((ComboBox)sender).SelectedItem.ToString();
            EA = (from a in pe.Applications where a.Name == app select a).FirstOrDefault();
            foreach (Database db in EA.Dbs)
            {
                DBNameComboBox.Items.Add(db.Name);

            }
            ComboAutoSelectIfOneItemOnly(DBNameComboBox);
        }

        private void AddDBOperationTypeInsert(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender) != null && ((ComboBox)sender).SelectedItem != null)
            {
                string dbName = ((ComboBox)sender).SelectedItem.ToString();
                db = (Database)EA.Dbs.First(m => m.Name == dbName);
                if (db.DBType.Equals(eDBTypes.CosmosDb))
                {
                    if (ValidationCfgComboBox.Items.Cast<ComboEnumItem>().Where(m => m.text.ToString().Equals(eDBValidationType.Insert.ToString())) == null
                        || !ValidationCfgComboBox.Items.Cast<ComboEnumItem>().Any(m => m.text.ToString().Equals(eDBValidationType.Insert.ToString())))
                    {
                        ValidationCfgComboBox.Items.Add(new ComboEnumItem() { text = "Insert", Value = eDBValidationType.Insert });
                    }
                }
                else
                {
                    if (ValidationCfgComboBox.Items.Cast<ComboEnumItem>().Where(m => m.text.ToString().Equals(eDBValidationType.Insert.ToString())) != null
                        && ValidationCfgComboBox.Items.Cast<ComboEnumItem>().Any(m => m.text.ToString().Equals(eDBValidationType.Insert.ToString())))
                    {
                        if (ValidationCfgComboBox.SelectedValue != null && ValidationCfgComboBox.SelectedValue.ToString() == eDBValidationType.Insert.ToString())
                        {
                            ValidationCfgComboBox.SelectedIndex = 0;
                        }
                        ValidationCfgComboBox.Items.Remove(ValidationCfgComboBox.Items.Cast<ComboEnumItem>().First(m => m.text.ToString().Equals(eDBValidationType.Insert.ToString())));
                    }
                }
            }
        }

        private void NewAutomatePage_RaiseEnvComboBoxChanged(object sender, EventArgs e)
        {
            string selectedAppName = mAct.AppName;
            string selectedDBName = mAct.DBName;

            object selectAppNameObject = AppNameComboBox.SelectedItem;
            object selectDBNameObject = DBNameComboBox.SelectedItem;

            AppNameComboBox.Items.Clear();
            DBNameComboBox.Items.Clear();

            if (Context.GetAsContext(mAct.Context) != null && Context.GetAsContext(mAct.Context).Environment != null)
            {
                pe = (from env in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>() where env.Name == Context.GetAsContext(mAct.Context).Environment.Name select env).FirstOrDefault();

                if (pe == null)
                {
                    return;
                }
                foreach (EnvApplication ea in pe.Applications)
                {
                    AppNameComboBox.Items.Add(ea.Name);
                }
            }

            AppNameComboBox.SelectedItem = selectAppNameObject;
            mAct.AppName = selectedAppName;
            AppNameComboBox.Text = selectedAppName;

            DBNameComboBox.SelectedItem = selectDBNameObject;
            mAct.DBName = selectedDBName;
            DBNameComboBox.Text = selectedDBName;
        }

        private void DBNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeySpaceComboBox.Items.Clear();
            TablesComboBox.Items.Clear();
            ColumnComboBox.Items.Clear();
            txtWhere.Clear();
            AddDBOperationTypeInsert(sender, e);
            SetVisibleControlsForAction();
        }

        private void TablesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColumnComboBox.Items.Clear();
            txtWhere.Clear();
        }

        private void KeySpaceComboBox_DropDownOpened(object sender, EventArgs e)
        {
            KeySpaceComboBox.Items.Clear();
            string DBName = DBNameComboBox.Text;
            db = (Database)(from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
            if (db == null)
            {
                return;
            }

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
            else if (db.DBType == Database.eDBTypes.Couchbase)
            {
                NoSqlBase NoSqlDriver = null;
                NoSqlDriver = new GingerCouchbase(db);

                List<string> keyspace = NoSqlDriver.GetKeyspaceList();
                foreach (string s in keyspace)
                {
                    KeySpaceComboBox.Items.Add(s);
                }
            }
        }

        private async void TablesComboBox_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                Reporter.ToStatus(eStatusMsgKey.StaticStatusProcess, null, "Loading Tables...");
                TablesComboBox.Items.Clear();
                string DBName = DBNameComboBox.Text;
                if (EA == null)
                {
                    EA = pe.Applications.FirstOrDefault(a => string.Equals(a.Name, AppNameComboBox.Text));
                }
                if (EA == null)
                {
                    return;
                }
                db = (Database)EA.Dbs.FirstOrDefault(db => string.Equals(db.Name, DBName));
                if (db == null)
                {
                    return;
                }

                string KeySpace = KeySpaceComboBox.Text;
                if (db.DatabaseOperations == null)
                {
                    db.DatabaseOperations = new DatabaseOperations(db);
                }

                await Task.Run(async () =>
                   {
                       try
                       {
                           List<string> tables = await db.DatabaseOperations.GetTablesListAsync(KeySpace);
                           if (tables != null)
                           {
                               Dispatcher.Invoke(() =>
                               {
                                   foreach (string s in tables)
                                   {
                                       TablesComboBox.Items.Add(s);
                                   }
                               });
                           }
                           Reporter.HideStatusMessage();
                       }
                       catch (Exception ex)
                       {
                           Reporter.ToLog(eLogLevel.ERROR, $"{db.DatabaseOperations} failed to get tables", ex);
                       }
                   });

            }
            finally
            {
                Mouse.OverrideCursor = null;
                Reporter.HideStatusMessage();
            }
        }

        private async void ColumnComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ColumnComboBox.Items.Clear();
            string DBName = DBNameComboBox.Text;
            if (EA == null)
            {
                EA = pe.Applications.FirstOrDefault(a => string.Equals(a.Name, AppNameComboBox.Text));
            }
            if (EA == null)
            {
                return;
            }
            db = (Database)(from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
            if (db == null)
            {
                return;
            }

            string table;
            if (db.DBType == Database.eDBTypes.Cassandra)
            {
                table = KeySpaceComboBox.Text + "." + TablesComboBox.Text;
            }
            else
            {
                table = TablesComboBox.Text;
            }
            if (table != "")
            {
                if (db.DatabaseOperations == null)
                {
                    db.DatabaseOperations = new DatabaseOperations(db);
                }

                await Task.Run(async () =>
                {
                    try
                    {
                        List<string> Columns = await db.DatabaseOperations.GetTablesColumns(table);
                        if (Columns != null)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                foreach (string s in Columns)
                                {
                                    ColumnComboBox.Items.Add(s);
                                }
                            });
                        }
                        Reporter.HideStatusMessage();
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"{db.DatabaseOperations} failed to get tables", ex);
                    }
                });


            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectTable);
            }
        }

        private void ValidationCfgComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetVisibleControlsForAction();
        }

        private void SetVisibleControlsForAction()
        {

            // Whenever no Database Operation is selected then the Input Type and the Radio buttons related to the input type should be hidden.
            if (ValidationCfgComboBox.SelectedItem == null)
            {
                RadioButtonsSection.Visibility = Visibility.Collapsed;
                FreeSQLStackPanel.Visibility = Visibility.Collapsed;
                SqlFile.Visibility = Visibility.Collapsed;
                DoCommit.Visibility = Visibility.Collapsed;
                Keyspace.Visibility = Visibility.Collapsed;
                TableColWhereStackPanel.Visibility = Visibility.Collapsed;
                txtWhere.Clear();
                return;
            }
            if (pe != null)
            {
                if (!string.IsNullOrEmpty(AppNameComboBox.Text))
                {
                    EA = pe.Applications.FirstOrDefault(m => m.Name.Equals(AppNameComboBox.Text));
                }
            }


            //Ugly code but working, find way to make it simple use the enum val from combo
            ActDBValidation.eDBValidationType validationType = (ActDBValidation.eDBValidationType)ValidationCfgComboBox.SelectedValue;

            txtInsertJson.Visibility = Visibility.Collapsed;
            lblInsertJson.Visibility = Visibility.Collapsed;
            gridInsertJson.Visibility = Visibility.Collapsed;
            imgHelpSql.Visibility = Visibility.Collapsed;
            xPrimaryKeyStackPanel.Visibility = Visibility.Collapsed;
            xPartitionKeyStackPanel.Visibility = Visibility.Collapsed;
            //SQLUCValueExpression.ValueTextBox.Text = string.Empty;
            UpdateDbParametersHeadersGrid.Visibility = Visibility.Collapsed;

            switch (validationType)
            {
                case ActDBValidation.eDBValidationType.UpdateDB:
                    string dbName = DBNameComboBox.Text;
                    if (EA != null)
                    {
                        db = (Database)(from d in EA.Dbs where d.Name == dbName select d).FirstOrDefault();
                    }
                    RadioButtonsSection.Visibility = System.Windows.Visibility.Visible;
                    checkQueryType();
                    TableColWhereStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    txtWhere.Clear();
                    FreeSQLLabel.Content = "Update DB SQL:";
                    DoCommit.Visibility = Visibility.Visible;
                    Keyspace.Visibility = Visibility.Collapsed;
                    if (db != null && db.DBType == Database.eDBTypes.CosmosDb)
                    {
                        FreeSQLStackPanel.Visibility = Visibility.Collapsed;
                        TableColWhereStackPanel.Visibility = Visibility.Visible;
                        txtWhere.Visibility = Visibility.Collapsed;
                        lblWhere.Visibility = Visibility.Hidden;
                        TableColWhereStackPanel.Height = 40;
                        DoCommit.Visibility = Visibility.Collapsed;
                        DoUpdate.Visibility = Visibility.Visible;
                        RadioButtonsSection.Visibility = Visibility.Collapsed;
                        UpdateDbParametersGrid.Visibility = Visibility.Visible;
                        xPrimaryKeyStackPanel.Visibility = Visibility.Visible;
                        xPartitionKeyStackPanel.Visibility = Visibility.Visible;
                        UpdateDbParametersHeadersGrid.Visibility = Visibility.Visible;
                        SetGridView();
                    }
                    else
                    {
                        string queryTypeRadioValue = mAct.GetInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton);
                        if (queryTypeRadioValue == ActDBValidation.eQueryType.FreeSQL.ToString())
                        {
                            FreeSQLStackPanel.Visibility = Visibility.Visible;
                            SqlFile.Visibility = Visibility.Collapsed;
                        }
                        TableColWhereStackPanel.Visibility = Visibility.Collapsed;
                        txtWhere.Visibility = Visibility.Visible;
                        lblWhere.Visibility = Visibility.Visible;
                        TableColWhereStackPanel.Height = 244;
                        DoCommit.Visibility = Visibility.Visible;
                        DoUpdate.Visibility = Visibility.Collapsed;
                        RadioButtonsSection.Visibility = Visibility.Visible;
                    }
                    break;
                case ActDBValidation.eDBValidationType.FreeSQL:
                    TableColWhereStackPanel.Visibility = Visibility.Collapsed;
                    txtWhere.Visibility = Visibility.Visible;
                    lblWhere.Visibility = Visibility.Visible;
                    TableColWhereStackPanel.Height = 244;
                    checkQueryType();
                    RadioButtonsSection.Visibility = System.Windows.Visibility.Visible;
                    if (mAct.GetInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton) == ActDBValidation.eQueryType.FreeSQL.ToString())
                    {
                        FreeSQLStackPanel.Visibility = System.Windows.Visibility.Visible;
                        SqlFile.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else
                    {
                        SqlFile.Visibility = System.Windows.Visibility.Visible;
                        FreeSQLStackPanel.Visibility = System.Windows.Visibility.Collapsed;

                        if (mAct.QueryParams != null)
                        {
                            if (mAct.QueryParams.Count > 0)
                            {
                                QueryParamsPanel.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                QueryParamsPanel.Visibility = Visibility.Collapsed;
                            }

                            QueryParamsGrid.DataSourceList = mAct.QueryParams;
                        }
                    }
                    DoCommit.Visibility = System.Windows.Visibility.Collapsed;
                    DoUpdate.Visibility = Visibility.Collapsed;
                    TableColWhereStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    FreeSQLLabel.Content = "Free SQL:";
                    Keyspace.Visibility = System.Windows.Visibility.Collapsed;
                    txtWhere.Clear();
                    break;
                case ActDBValidation.eDBValidationType.SimpleSQLOneValue:
                    checkQueryType();
                    Keyspace.Visibility = System.Windows.Visibility.Collapsed;
                    try
                    {
                        string DBName = DBNameComboBox.Text;
                        if (EA != null)
                        {
                            db = (Database)(from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
                        }
                        if (db != null && db.DBType == Database.eDBTypes.Cassandra)
                        {
                            Keyspace.Visibility = System.Windows.Visibility.Visible;
                        }
                    }
                    catch { }
                    FreeSQLStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    RadioButtonsSection.Visibility = System.Windows.Visibility.Collapsed;
                    TableColWhereStackPanel.Visibility = System.Windows.Visibility.Visible;
                    DoCommit.Visibility = System.Windows.Visibility.Collapsed;
                    DoUpdate.Visibility = Visibility.Collapsed;
                    SqlFile.Visibility = System.Windows.Visibility.Collapsed;
                    txtWhere.Visibility = Visibility.Visible;
                    lblWhere.Visibility = Visibility.Visible;
                    TableColWhereStackPanel.Height = 244;
                    break;
                case ActDBValidation.eDBValidationType.RecordCount:
                    TableColWhereStackPanel.Visibility = Visibility.Collapsed;
                    txtWhere.Visibility = Visibility.Visible;
                    lblWhere.Visibility = Visibility.Visible;
                    TableColWhereStackPanel.Height = 244;
                    imgHelpSql.Visibility = Visibility.Visible;
                    checkQueryType();
                    try
                    {
                        string DBName = DBNameComboBox.Text;
                        if (EA != null)
                        {
                            db = (Database)(from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
                        }
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
                    DoUpdate.Visibility = Visibility.Collapsed;
                    SqlFile.Visibility = System.Windows.Visibility.Collapsed;
                    FreeSQLLabel.Content = @"Record count";
                    txtWhere.Clear();
                    break;
                case eDBValidationType.Insert:
                    DoUpdate.Visibility = Visibility.Visible;
                    txtInsertJson.Visibility = Visibility.Visible;
                    lblInsertJson.Visibility = Visibility.Visible;
                    gridInsertJson.Visibility = Visibility.Visible;
                    lblColumn.Visibility = Visibility.Collapsed;
                    Keyspace.Visibility = Visibility.Collapsed;
                    KeyspaceCmbStack.Visibility = Visibility.Collapsed;
                    ColumnStack.Visibility = Visibility.Collapsed;
                    UpdateDbParametersGrid.Visibility = Visibility.Collapsed;
                    FreeSQLStackPanel.Visibility = Visibility.Collapsed;
                    TableColWhereStackPanel.Visibility = Visibility.Visible;
                    TableColWhereStackPanel.Height = 40;
                    txtWhere.Visibility = Visibility.Collapsed;
                    lblWhere.Visibility = Visibility.Hidden;
                    txtWhere.Clear();
                    DoCommit.Visibility = Visibility.Collapsed;
                    FreeSQLStackPanel.Visibility = Visibility.Collapsed;
                    RadioButtonsSection.Visibility = Visibility.Collapsed;
                    break;
            }
            if (db != null)
            {
                if (db.DBType == Database.eDBTypes.CosmosDb)
                {
                    SQLUCValueExpression.ToolTip = "Container Name is case-sensitive";
                    lblColumn.Visibility = Visibility.Collapsed;
                    if (ColumnsVEButton != null)
                    {
                        ColumnsVEButton.Visibility = Visibility.Collapsed;
                    }
                    ColumnComboBox.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SQLUCValueExpression.ToolTip = string.Empty;
                    lblColumn.Visibility = Visibility.Visible;
                    if (ColumnsVEButton != null)
                    {
                        ColumnsVEButton.Visibility = Visibility.Visible;
                    }
                    ColumnComboBox.Visibility = Visibility.Visible;
                }
            }
        }

        private void DBNamVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActDBValidation.Fields.DBName, Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            DBNameComboBox.Text = mAct.DBName;
        }

        private void AppNamVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActDBValidation.Fields.AppName, Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            AppNameComboBox.Text = mAct.AppName;
        }

        private void TablesVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActDBValidation.Fields.Table, Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            TablesComboBox.Text = mAct.Table;
        }

        private void ColumnsVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActDBValidation.Fields.Column, Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            ColumnComboBox.Text = mAct.Column;
        }

        private void KeySpaceVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, ActDBValidation.Fields.Keyspace, Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            KeySpaceComboBox.Text = mAct.Keyspace;
        }

        public void QueryType_SelectionChanged(object sender, RoutedEventArgs e)
        {
            mAct.AddOrUpdateInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton, (((RadioButton)sender).Tag).ToString());
            if (mAct.GetInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton) == ActDBValidation.eQueryType.FreeSQL.ToString())
            {

                SqlFile.Visibility = System.Windows.Visibility.Collapsed;
                FreeSQLStackPanel.Visibility = System.Windows.Visibility.Visible;

            }
            else if (mAct.GetInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton) == ActDBValidation.eQueryType.SqlFile.ToString())
            {
                SqlFile.Visibility = System.Windows.Visibility.Visible;
                FreeSQLStackPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        } //TODO populate field selection changed

        public void checkQueryType()
        {
            if (mAct.GetInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton) == ActDBValidation.eQueryType.FreeSQL.ToString())
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
            string SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();
            if (!String.IsNullOrEmpty(QueryFile.ValueTextBox.Text))
            {
                if (!System.IO.File.Exists(QueryFile.ValueTextBox.Text))
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
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), Context.GetAsContext(mAct.Context));
            VEEW.ShowAsWindow();
        }

        private void TablesComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ColumnComboBox.Items.Clear();
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ActInputValue.Param), Header = "Path", WidthWeight = 150 },
                new GridColView() { Field = "...", Header = "...", WidthWeight = 5, MaxWidth = 35, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.UpdateDbParametersHeadersGrid.Resources["UpdateDbParametersPathValueExpressionButton"] },
                new GridColView() { Field = nameof(ActInputValue.Value), Header = "Value", WidthWeight = 150 },
                new GridColView() { Field = "....", Header = "...", WidthWeight = 5, MaxWidth = 35, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.UpdateDbParametersHeadersGrid.Resources["UpdateDbParametersValueExpressionButton"] },
                new GridColView() { Field = nameof(ActInputValue.ValueForDriver), Header = "Value For Driver", WidthWeight = 150, BindingMode = BindingMode.OneWay },
            ]
            };

            UpdateDbParametersGrid.SetAllColumnsDefaultView(view);
            UpdateDbParametersGrid.InitViewItems();
            UpdateDbParametersGrid.SetTitleLightStyle = true;
            UpdateDbParametersGrid.btnAdd.RemoveHandler(Button.ClickEvent, new RoutedEventHandler(AddPatchOperationForCosmos));
            UpdateDbParametersGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddPatchOperationForCosmos));
            UpdateDbParametersGrid.btnDown.Visibility = Visibility.Collapsed;
            UpdateDbParametersGrid.btnUp.Visibility = Visibility.Collapsed;
            UpdateDbParametersGrid.btnClearAll.Visibility = Visibility.Collapsed;
            UpdateDbParametersGrid.btnRefresh.Visibility = Visibility.Collapsed;

            UpdateDbParametersGrid.DataSourceList = mAct.UpdateOperationInputValues;
        }

        private void UpdateDbParametersGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue cosmosPatchInput = (ActInputValue)UpdateDbParametersGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(cosmosPatchInput, nameof(ActInputValue.Value), Context.GetAsContext(mAct.Context));
            VEEW.ShowAsWindow();
        }
        private void UpdateDbParametersGridPathVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue cosmosPatchInput = (ActInputValue)UpdateDbParametersGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(cosmosPatchInput, nameof(ActInputValue.Param), Context.GetAsContext(mAct.Context));
            VEEW.ShowAsWindow();
        }

        private void AddPatchOperationForCosmos(object sender, RoutedEventArgs e)
        {
            ActInputValue cosmosPatchInput = new ActInputValue();
            mAct.UpdateOperationInputValues.Add(cosmosPatchInput);
        }
    }
}
