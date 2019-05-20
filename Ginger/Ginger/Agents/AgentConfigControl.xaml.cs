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
    /// Interaction logic for AgentConfigControl.xaml
    /// </summary>
    public partial class AgentConfigControl : UserControl
    {
        public AgentConfigControl(DriverConfigParam Config)
        {
            InitializeComponent();
            Name.Content = Config.Parameter;
            Description.Content = Config.Description;


            if (Config.OptionalValues == null|| Config.OptionalValues.Count==0)
            {
                Ginger.Actions.UCValueExpression txtBox = new Ginger.Actions.UCValueExpression()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 600

                };

                txtBox.Visibility = Visibility.Visible;
                txtBox.Init(null, Config, "Value", isVENeeded: true);
                ControlPanel.Children.Add(txtBox);
            }
            else
            {
             ComboBox comboBox = new ComboBox()
                {
                    
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Visibility = Visibility.Visible,
                Width = 600,
                    Margin = new Thickness(10, 0, 0, 0)
                };
             //   comboBox.Init(Config, "Value");
               // ((Ginger.UserControlsLib.UCComboBox)comboBox).ComboBox.ItemsSource = Config.OptionalValues;
                ControlPanel.Children.Add(comboBox);
                ControlPanel.UpdateLayout();
            }



        }
    }
}
