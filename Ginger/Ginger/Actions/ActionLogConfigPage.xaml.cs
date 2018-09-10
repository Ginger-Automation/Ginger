using Amdocs.Ginger.Common.Actions;
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

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActionLogConfigPage.xaml
    /// </summary>
    public partial class ActionLogConfigPage : Page
    {
        public ActionLogConfigPage(ActionLogConfig actionLogConfig)
        {
            InitializeComponent();

            ActionLogTextBox.BindControl(actionLogConfig, nameof(ActionLogConfig.ActionLogText));
            LogInputVariablesCheckBox.BindControl(actionLogConfig, nameof(ActionLogConfig.LogInputVariables));
            LogOuputVariablesCheckBox.BindControl(actionLogConfig, nameof(ActionLogConfig.LogOutputVariables));
            LogRunStatusCheckBox.BindControl(actionLogConfig, nameof(ActionLogConfig.LogRunStatus));
            LogErrorCheckBox.BindControl(actionLogConfig, nameof(ActionLogConfig.LogError));
            LogElapsedTimeCheckBox.BindControl(actionLogConfig, nameof(ActionLogConfig.LogElapsedTime));
        }

    }
}
