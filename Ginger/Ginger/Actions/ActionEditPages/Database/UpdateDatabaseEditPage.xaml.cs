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

namespace Ginger.Actions.ActionEditPages.Database
{
    /// <summary>
    /// Interaction logic for UpdateDatabaseEditPage.xaml
    /// </summary>
    public partial class UpdateDatabaseEditPage : Page
    {
        ActDBValidation mAct;
        public UpdateDatabaseEditPage(ActDBValidation act)
        {
            InitializeComponent();

            mAct = act;
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(CommitDB, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(nameof(ActDBValidation.CommitDB)));

            xSQLUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActDBValidation.SQL));
        }
    }
}
