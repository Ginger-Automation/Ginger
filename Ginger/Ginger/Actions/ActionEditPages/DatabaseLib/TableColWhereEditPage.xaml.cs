using Amdocs.Ginger.Common;
using GingerCore.Actions;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.Actions.ActionEditPages.DatabaseLib
{
    /// <summary>
    /// Interaction logic for TableColWhereEditPage.xaml
    /// </summary>
    public partial class TableColWhereEditPage : Page
    {
        ActDBValidation mAct;
        Database mDatabase;
        public TableColWhereEditPage(ActDBValidation act, Database database)
        {
            InitializeComponent();

            mAct = act;
            mDatabase = database;

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TablesComboBox, ComboBox.TextProperty, act, nameof(ActDBValidation.Table));
            // GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(KeySpaceComboBox, ComboBox.TextProperty, act, nameof(ActDBValidation.Keyspace));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ColumnComboBox, ComboBox.TextProperty, act, nameof(ActDBValidation.Column));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtWhere, TextBox.TextProperty, act, nameof(ActDBValidation.Where));

            //KeySpaceComboBox.Items.Add(mAct.Keyspace);
            //ComboAutoSelectIfOneItemOnly(KeySpaceComboBox);
            TablesComboBox.Items.Add(mAct.Table);
            TablesComboBox.AutoSelectIfOneItemOnly();
            ColumnComboBox.Items.Add(mAct.Column);
            ColumnComboBox.AutoSelectIfOneItemOnly();
        }

        
        private void TablesComboBox_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                if (TablesComboBox.Items.Count > 0) return;
                Mouse.OverrideCursor = Cursors.Wait;
                Reporter.ToStatus(eStatusMsgKey.StaticStatusProcess, null, "Loading Tables...");                

                List<string> Tables = mDatabase.GetTablesList();  // !!!!!!!!!!!!!!!! "KeySpace"
                if (Tables == null)
                {
                    return;
                }
                foreach (string table in Tables)
                {
                    TablesComboBox.Items.Add(table);
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
                Reporter.HideStatusMessage();
            }
        }

        private void ColumnComboBox_DropDownOpened(object sender, EventArgs e)
        {
            //ColumnComboBox.Items.Clear();
            //string DBName = DBNameComboBox.Text;
            //db = (Database)(from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
            //if (db == null) return;
            //string table;
            //if (db.DBType == Database.eDBTypes.Cassandra)
            //{
            //    table = KeySpaceComboBox.Text + "." + TablesComboBox.Text;
            //}
            //else
            //{
            //    table = TablesComboBox.Text;
            //}
            //List<string> Columns = db.GetTablesColumns(table);
            //if (Columns == null)
            //{
            //    return;
            //}
            //foreach (string s in Columns)
            //{
            //    ColumnComboBox.Items.Add(s);
            //}
        }


        private void TablesVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, nameof(ActDBValidation.Table), Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            TablesComboBox.Text = mAct.Table;
        }

        private void ColumnsVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, nameof(ActDBValidation.Column), Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            ColumnComboBox.Text = mAct.Column;
        }

        private void xRefreshButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
