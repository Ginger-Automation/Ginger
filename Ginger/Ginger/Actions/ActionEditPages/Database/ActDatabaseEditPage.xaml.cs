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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Actions.ActionEditPages.Database;
using GingerCore.Actions;
using GingerCore.Environments;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ValidationDBPage.xaml
    /// </summary>
    public partial class ActDatabaseEditPage : Page
    {
        private ActDBValidation mAct;

        ProjEnvironment pe;
        EnvApplication EA;
        Database db;

        public ActDatabaseEditPage(ActDBValidation act)
        {
            InitializeComponent();
        
            this.mAct = act;

            if (String.IsNullOrEmpty(mAct.GetInputParamValue("SQL")))
            {
                mAct.AddOrUpdateInputParamValue("SQL", mAct.GetInputParamValue("Value"));
            }

            FillAppComboBox();


            GingerCore.General.FillComboFromEnumObj(xDBOperationComboBox, act.DBValidationType);            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xDBOperationComboBox, ComboBox.SelectedValueProperty, act, nameof(ActDBValidation.DBValidationType));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAppNameComboBox, ComboBox.TextProperty, act, nameof( ActDBValidation.AppName));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xDBNameComboBox, ComboBox.TextProperty, act, nameof (ActDBValidation.DBName));            
            
            
            SetVisibleControlsForAction();
            SetQueryParamsGrid();
        }

        //private async void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    string SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();
        //    bool ImportFileFlag = false;
        //    string FileName = QueryFile.ValueTextBox.Text;
        //    Boolean.TryParse(nameof (ActDBValidation.ImportFile), out ImportFileFlag);
        //    if (ImportFileFlag && !FileName.StartsWith(@"~\"))
        //    {
        //        async Task<bool> UserKeepsTyping()
        //        {
        //            string txt = QueryFile.ValueTextBox.Text;
        //            await Task.Delay(2000);
        //            return txt != QueryFile.ValueTextBox.Text;
        //        }
        //        if (await UserKeepsTyping() || QueryFile.ValueTextBox.Text == null) return;
        //    }            
        //    if (FileName != "" && File.Exists(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(FileName)))
        //    {  
        //        parseScriptHeader(FileName);  
        //    }
        //}

       
        private void SetQueryParamsGrid()
        {
            ////Show/hide if needed
            ////QueryParamsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddInputValue));//?? going to be hide in next line code

            //QueryParamsGrid.SetTitleLightStyle = true;
            //QueryParamsGrid.ClearTools();
            //QueryParamsGrid.ShowDelete = Visibility.Visible;

            ////List<GridColView> view = new List<GridColView>();
            //GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            //view.GridColsView = new ObservableList<GridColView>();

            //view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), WidthWeight = 150 });
            //view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), WidthWeight = 150 });
            //view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["QueryParamExpressionButton"] });
            //view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.ValueForDriver), Header = "Value ForDriver", WidthWeight = 150, BindingMode = BindingMode.OneWay });

            //QueryParamsGrid.SetAllColumnsDefaultView(view);
            //QueryParamsGrid.InitViewItems();
        }


        

        
        /// <summary>
        /// Fill the environments Applications combo box
        /// </summary>
        private void FillAppComboBox()
        {            
            xAppNameComboBox.Items.Clear();

            if (Context.GetAsContext(mAct.Context) != null && Context.GetAsContext(mAct.Context).Environment != null)
            {                
                pe = (from e in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>() where e.Name == Context.GetAsContext(mAct.Context).Environment.Name select e).FirstOrDefault();

                if (pe == null)
                {
                    return;
                }
                foreach (EnvApplication ea in pe.Applications)
                {
                    xAppNameComboBox.Items.Add(ea.Name);
                }
                xAppNameComboBox.AutoSelectIfOneItemOnly();
            }
        }

        private void AppNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: if there is only one item in the combo auto select it
            xDBNameComboBox.Items.Clear();
            //KeySpaceComboBox.Items.Clear();
            //TablesComboBox.Items.Clear();
            //ColumnComboBox.Items.Clear();

            if ((((ComboBox)sender).SelectedItem) == null) return;

            string app = ((ComboBox)sender).SelectedItem.ToString();
            EA = (from a in pe.Applications where a.Name == app select a).FirstOrDefault();
            foreach (Database db in EA.Dbs)
            {
                xDBNameComboBox.Items.Add(db.Name);
                
            }
            xDBNameComboBox.AutoSelectIfOneItemOnly();
        }

        private void DBNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //KeySpaceComboBox.Items.Clear();
            //TablesComboBox.Items.Clear();
            //ColumnComboBox.Items.Clear();
        }

        private void TablesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            // ColumnComboBox.Items.Clear();
        }

        private void KeySpaceComboBox_DropDownOpened(object sender, EventArgs e)
        {
            // FIXME !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            // KeySpaceComboBox.Items.Clear();
            string DBName = xDBNameComboBox.Text;
            db = (Database)(from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
            if (db == null) return;
            //if (db.DBType == Database.eDBTypes.Cassandra)
            //{
            //    //NoSqlBase NoSqlDriver = null;
            //    //NoSqlDriver = new GingerCassandra(db);

            //    //List<string> keyspace = NoSqlDriver.GetKeyspaceList();
            //    //foreach (string s in keyspace)
            //    //{
            //    //    KeySpaceComboBox.Items.Add(s);
            //    //}
            //}
            //else if (db.DBType == Database.eDBTypes.Couchbase)
            //{
            //    CouchBaseConnection couchBaseConnection = new CouchBaseConnection();
               
            //    List<string> keyspace = couchBaseConnection.GetKeyspaceList();
            //    foreach (string s in keyspace)
            //    {
            //        KeySpaceComboBox.Items.Add(s);
            //    }
            //}
        }

       

        private void ValidationCfgComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetVisibleControlsForAction();
        }

        private void SetVisibleControlsForAction()
        {
            OperationConfigFrame.SetContent(null);

            if (xDBOperationComboBox.SelectedItem == null)
            {                
                return;
            }
            
            ActDBValidation.eDBValidationType validationType = (ActDBValidation.eDBValidationType)xDBOperationComboBox.SelectedValue;

            switch (validationType)
            {
                case ActDBValidation.eDBValidationType.UpdateDB:
                    OperationConfigFrame.SetContent(new UpdateDatabaseEditPage(mAct));
                    // checkQueryType();                    
                    break;
                case ActDBValidation.eDBValidationType.FreeSQL:
                    OperationConfigFrame.SetContent(new DatabaseQueryEditPage(mAct));                    
                    // checkQueryType();

                    //{
                    //    if (mAct.QueryParams.Count > 0)
                    //        QueryParamsPanel.Visibility = Visibility.Visible;
                    //    else
                    //        QueryParamsPanel.Visibility = Visibility.Collapsed;
                    //    QueryParamsGrid.DataSourceList = mAct.QueryParams;
                    //}
            // }
                    //DoCommit.Visibility = Visibility.Collapsed;
                    //TableColWhereStackPanel.Visibility = Visibility.Collapsed;
                    //FreeSQLLabel.Content = "Free SQL:";
                    //Keyspace.Visibility = Visibility.Collapsed;
                    break;
                case ActDBValidation.eDBValidationType.SimpleSQLOneValue:
                    OperationConfigFrame.SetContent(new TableColWhereEditPage(mAct));
                    
                    //checkQueryType();
                    //try
                    //{
                    //    string DBName = DBNameComboBox.Text;
                    //    db = (Database) (from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
                    //    if (!(db == null))
                    //    {
                    //        if (db.DBType == Database.eDBTypes.Cassandra)
                    //        {
                    //            Keyspace.Visibility = Visibility.Visible;
                    //        }
                    //        else
                    //        {
                    //            Keyspace.Visibility = Visibility.Collapsed;
                    //        }
                    //    }
                    //}
                    //catch { }
                    //FreeSQLStackPanel.Visibility = Visibility.Collapsed;
                    //RadioButtonsSection.Visibility = Visibility.Collapsed;
                    //TableColWhereStackPanel.Visibility = Visibility.Visible;
                    //DoCommit.Visibility = Visibility.Collapsed;
                    //SqlFile.Visibility = Visibility.Collapsed;                    
                    break;
                case ActDBValidation.eDBValidationType.RecordCount:
                    OperationConfigFrame.SetContent(new RecordCountEditPage(mAct));
                    
                    
                    break;

            }
        }
        
        private void DBNamVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, nameof(ActDBValidation.DBName), Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            xDBNameComboBox.Text = mAct.DBName;
        }
        
        private void AppNamVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct,nameof(ActDBValidation.AppName), Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            xAppNameComboBox.Text = mAct.AppName;
        }

        
        private void KeySpaceVEButton_Click(object sender, RoutedEventArgs e)
        {
            //ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, nameof(ActDBValidation.Keyspace), Context.GetAsContext(mAct.Context));
            //w.ShowAsWindow(eWindowShowStyle.Dialog);
            //KeySpaceComboBox.Text = mAct.Keyspace;
        }

       //TODO populate field selection changed

        //public void checkQueryType()
        //{
        //    if (nameof(ActDBValidation.QueryTypeRadioButton) == ActDBValidation.eQueryType.FreeSQL.ToString())
        //    {
        //        FreeSQLStackPanel.Visibility = Visibility.Visible;
        //        SqlFile.Visibility = Visibility.Collapsed;               
        //    }
        //    else 
        //    {
        //        SqlFile.Visibility = Visibility.Visible;
        //        FreeSQLStackPanel.Visibility = Visibility.Collapsed;
        //    }
        //}

      

        private void QueryParamGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            //ActInputValue AIV = (ActInputValue)QueryParamsGrid.CurrentItem;
            //ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), Context.GetAsContext(mAct.Context));
            //VEEW.ShowAsWindow();
        }

        private void XViewButton_Click(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = mAct.GetResultView();
            if (dataTable != null)
            {
                xDataGrid.ItemsSource = dataTable.DefaultView;
                xDataGrid.IsReadOnly = true;
                xDataGrid.AutoGenerateColumns = true;                
            }

        }
    }
}
