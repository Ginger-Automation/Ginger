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
using GingerCore.Variables;

namespace Ginger.Variables
{
    /// <summary>
    /// Interaction logic for VariableStopWatchEditPage.xaml
    /// </summary>
    public partial class VariableTimerPage : Page
    {
        
        public VariableTimerPage(VariableTimer variableTimer)
        {
            InitializeComponent();

            xTimerUnitCombo.Init(variableTimer, nameof(variableTimer.TimerUnit), typeof(VariableTimer.eTimerUnit));
        }
    }
}
