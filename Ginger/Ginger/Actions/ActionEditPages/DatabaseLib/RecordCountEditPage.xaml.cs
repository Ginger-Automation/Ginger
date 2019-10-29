using Amdocs.Ginger.Common;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.Actions.ActionEditPages.DatabaseLib
{
    /// <summary>
    /// Interaction logic for RecordCountEditPage.xaml
    /// </summary>
    public partial class RecordCountEditPage : Page
    {
        ActDBValidation mAct;

        public RecordCountEditPage(ActDBValidation act)
        {
            InitializeComponent();

            mAct = act;
            xSQLUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActDBValidation.SQL));

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
            //RadioButtonsSection.Visibility = Visibility.Collapsed;
            //FreeSQLStackPanel.Visibility = Visibility.Visible;
            //TableColWhereStackPanel.Visibility = Visibility.Collapsed;
            //DoCommit.Visibility = Visibility.Collapsed;
            //SqlFile.Visibility = Visibility.Collapsed;
            //FreeSQLLabel.Content = @"Record count - SELECT COUNT(1) FROM {Table} - Enter only Table name below (+optional WHERE clause)";
        }

    }
}
