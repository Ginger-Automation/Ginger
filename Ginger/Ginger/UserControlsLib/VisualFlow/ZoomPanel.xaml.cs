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

using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.UserControlsLib.VisualFlow
{
    /// <summary>
    /// Interaction logic for ZoomPanel.xaml
    /// </summary>
    public partial class ZoomPanel : UserControl
    {
        public ZoomPanel()
        {
            InitializeComponent();
        }
        public double ZoomPercent
        {
            get
            {
                return ZoomSlider.Value;
            }
            set
            {
                ZoomSlider.Value = value;
            }
        }
        public Slider ZoomSliderContainer
        {
            get
            {
                return ZoomSlider;
            }
        }
        public Label PercentLabel
        {
            get
            {
                return ZoomPercentLabel;
            }
        }
        private void ZoomMinus_Click(object sender, RoutedEventArgs e)
        {
            // We reduce 0.1 and round it nicely to the nearest 10% - so it will go from 57% to 50% instead of 47%
            ZoomSlider.Value = Math.Round(ZoomPercent * 10 - 1) / 10;
        }

        private void ZoomPlus_Click(object sender, RoutedEventArgs e)
        {
            ZoomSlider.Value = Math.Round(ZoomPercent * 10 + 1) / 10;
        }
    }
}
