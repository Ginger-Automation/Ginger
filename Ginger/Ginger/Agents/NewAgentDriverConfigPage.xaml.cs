using GingerCore;
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

namespace Ginger.Agents
{
    /// <summary>
    /// Interaction logic for NewAgentDriverConfigPage.xaml
    /// </summary>
    public partial class NewAgentDriverConfigPage : Page
    {
        Agent mAgent;
        public NewAgentDriverConfigPage(Agent agent)
        {
            mAgent = agent;
            InitializeComponent();
            agent.SetDriverConfiguration();
            StackPanel dynamicPanel = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center };

            foreach (DriverConfigParam DCp in mAgent.DriverConfiguration)
            {
                AgentConfigControl AGC = new AgentConfigControl(DCp);
                AGC.Margin = new Thickness(10, 0, 0, 0);

                dynamicPanel.Children.Add(AGC);
            }
            dynamicPanel.UpdateLayout();
           Cntent.Content = dynamicPanel;
        }
    }
}
