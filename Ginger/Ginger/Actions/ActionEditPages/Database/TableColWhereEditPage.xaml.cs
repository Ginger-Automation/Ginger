using Amdocs.Ginger.Common;
using GingerCore.Actions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.Actions.ActionEditPages.Database
{
    /// <summary>
    /// Interaction logic for TableColWhereEditPage.xaml
    /// </summary>
    public partial class TableColWhereEditPage : Page
    {
        ActDBValidation mAct;
        public TableColWhereEditPage(ActDBValidation act)
        {
            InitializeComponent();

            mAct = act;


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

        //private void ComboAutoSelectIfOneItemOnly(ComboBox comboBox)
        //{
        //    if (comboBox.Items.Count == 1)
        //    {
        //        comboBox.SelectedItem = comboBox.Items[0];
        //    }
        //}

        private void TablesComboBox_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                //Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                //Reporter.ToStatus(eStatusMsgKey.StaticStatusProcess, null, "Loading Tables...");
                //// TablesComboBox.Items.Clear();
                //string DBName = mAct.DBName;
                //db = (Database)(from d in EA.Dbs where d.Name == DBName select d).FirstOrDefault();
                //if (db == null) return;
                //string KeySpace = KeySpaceComboBox.Text;
                //List<string> Tables = db.GetTablesList(KeySpace);
                //if (Tables == null)
                //{
                //    return;
                //}
                //foreach (string s in Tables)
                //{
                //    TablesComboBox.Items.Add(s);
                //}
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



    }
}
