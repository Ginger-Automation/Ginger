#region License
/*
Copyright © 2014-2019 European Support Limited

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

using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Common;
using GingerCore.Actions.Common;

namespace Ginger.Actions._Common
{
    /// <summary>
    /// Interaction logic for UCActionDetails.xaml
    /// </summary>
    public partial class UCActionDetails : UserControl
    {
        public UCActionDetails()
        {
            InitializeComponent();
            this.DataContextChanged += UserControl_DataContextChanged;   
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Add Property changed so grid cell will be updated while editing action
            if (e.NewValue is ActionDetails)
            {
                ActionDetails AD = (ActionDetails)e.NewValue;

                // Each action return the info and/or a list of values to show AIVs
                if (!string.IsNullOrEmpty(AD.Info))
                {                    
                    ActionDetailsLabel.Content = AD.Info;
                }
                else
                {
                    ActionDetailsLabel.Visibility = System.Windows.Visibility.Collapsed;
                }

                ObservableList<ActionParamInfo> parlist = AD.GetParamsInfo();
                if (parlist.Count > 0)
                {
                    AIVGrid.ItemsSource = (IObservableList)parlist;
                    AIVGrid.Columns[0].Width = 60;
                    AIVGrid.Columns[1].Width = new DataGridLength(100, DataGridLengthUnitType.Star);
                }
                else
                {
                    AIVGrid.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }
    }
}