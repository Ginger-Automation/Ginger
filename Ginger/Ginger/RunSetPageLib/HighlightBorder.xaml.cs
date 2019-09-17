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

using Amdocs.Ginger.CoreNET.Execution;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.MoveToGingerWPF.Run_Set_Pages
{
    /// <summary>
    /// Interaction logic for HighlightBorder.xaml
    /// </summary>
    public partial class BorderStatus : UserControl
    {
        public static readonly DependencyProperty HighlightProperty = DependencyProperty.Register("Status", typeof(eRunStatus), typeof(BorderStatus),
                        new FrameworkPropertyMetadata(eRunStatus.Pending, OnIconPropertyChanged));
        public eRunStatus Status
        {
            get { return (eRunStatus)GetValue(HighlightProperty); }
            set
            {
                SetValue(HighlightProperty, value);
                SetStatus();
            }

        }
        private static void OnIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BorderStatus HB = (BorderStatus)d;
            HB.SetStatus();
        }
      
        private void SetStatus()
        {
            switch (Status)
            {
                case eRunStatus.Running:
                    SelectedBorder.Visibility = Visibility.Visible;
                    break;
                default:
                    SelectedBorder.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        public BorderStatus()
        {
            InitializeComponent();
        }
    }
}
