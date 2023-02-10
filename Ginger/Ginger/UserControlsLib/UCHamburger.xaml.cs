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

using System.Windows;
using System.Windows.Controls;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for UCHamburger.xaml
    /// </summary>
    public partial class UCHamburger : UserControl
    {
        ColumnDefinition mColumnDefinition;
        int OriginalColumn;
        GridLength CurrentColumnLength;
        public UCHamburger()
        {
            InitializeComponent();
        }

        private void HamburegrButton_Click(object sender, RoutedEventArgs e)
        {
            //if this is first time click we keep the original data and ref to grid etc.
            if (mColumnDefinition == null)
            {
                Grid grd = (Grid)this.Parent;
                OriginalColumn = (int)this.GetValue(Grid.ColumnProperty);
                mColumnDefinition = grd.ColumnDefinitions[OriginalColumn];
            }

            //check if minimize or expand
            if (mColumnDefinition.ActualWidth > 0)
            {
                CurrentColumnLength = mColumnDefinition.Width;
                // We shrink, keep the original values
                mColumnDefinition.Width = new GridLength(0);                
                this.SetValue(Grid.ColumnProperty, OriginalColumn + 1);  // TODO: if left add one else -1 - auto check if col 0 or cols count
                this.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                // restore 
                mColumnDefinition.Width = CurrentColumnLength;
                this.SetValue(Grid.ColumnProperty, OriginalColumn);
                this.HorizontalAlignment = HorizontalAlignment.Right;
            }
        }
    }
}
