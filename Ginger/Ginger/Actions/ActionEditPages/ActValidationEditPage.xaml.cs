using Amdocs.Ginger.Common;
using GingerCore.Actions;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions.ActionEditPages
{
    /// <summary>
    /// Interaction logic for ActValidationEditPage.xaml
    /// </summary>
    public partial class ActValidationEditPage : Page
    {
        ActValidation mActValidation;
        public ActValidationEditPage(ActValidation actValidation)
        {
            mActValidation = actValidation;
            InitializeComponent();
            xCalcEngineUCRadioButtons.Init(typeof(ActValidation.eCalcEngineType), xCalcEngineRBsPanel, mActValidation.GetOrCreateInputParam(nameof(ActValidation.CalcEngineType), ActValidation.eCalcEngineType.VBS.ToString()), CalcEngineType_SelectionChanged);          
            xValidationUCValueExpression.Init(Context.GetAsContext(mActValidation.Context), mActValidation, nameof(ActValidation.Condition));
            SetWarnMsgView();
        }
        private void CalcEngineType_SelectionChanged(object sender, RoutedEventArgs e)
        {
            SetWarnMsgView();
        }

        private void SetWarnMsgView()
        {
            if (mActValidation.CalcEngineType == ActValidation.eCalcEngineType.CS)
            {
                xVBSWarnHelpLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                xVBSWarnHelpLabel.Visibility = Visibility.Visible;
            }
        }

    }
}
