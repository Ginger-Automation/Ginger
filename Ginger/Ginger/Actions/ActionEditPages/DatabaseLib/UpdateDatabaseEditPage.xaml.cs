using Amdocs.Ginger.Common;
using GingerCore.Actions;
using System.Windows.Controls;

namespace Ginger.Actions.ActionEditPages.DatabaseLib
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
