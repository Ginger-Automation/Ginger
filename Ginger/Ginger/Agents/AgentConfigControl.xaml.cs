#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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
