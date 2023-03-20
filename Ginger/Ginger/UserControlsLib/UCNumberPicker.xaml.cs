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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for UCNumberPicker.xaml
    /// </summary>
    public partial class UCNumberPicker : UserControl
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int MinCount { get; set; } = 1;
        public int MaxCount { get; set; } = 10;

        private int mSelectedNumber;
        public int SelectedNumber
        {
            get
            {
                return mSelectedNumber;
            }
            private set
            {
                if (mSelectedNumber != value)
                {
                    mSelectedNumber = value;
                }
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedNumber)));
                }
            }
        }

        public UCNumberPicker()
        {
            InitializeComponent();
            SelectedNumber = MinCount;
        }

        private void xDecreaseCounterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedNumber > 1)
            {
                SelectedNumber--;
                SetExecutionCount();
            }
        }

        private void SetExecutionCount()
        {
            xCountTextBox.Text = SelectedNumber.ToString();
        }

        private void xIncreaseCounterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedNumber < 10 || xCountTextBox.Text == "1")
            {
                SelectedNumber++;
                SetExecutionCount();
            }
        }

        private void xCountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int number;
            var isNumber = int.TryParse(xCountTextBox.Text, out number);
            if (!isNumber || (isNumber && (number < MaxCount) && (number > MinCount)))
            {
                SelectedNumber = number;
                SetExecutionCount();
            }
            else if (number > MaxCount)
            {
                SetExecutionCount();
            }
        }
    }
}
