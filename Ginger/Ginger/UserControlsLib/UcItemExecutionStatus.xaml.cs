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

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET.Execution;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.UserControlsLib.UCListView
{
    /// <summary>
    /// Interaction logic for UCITemExecutionStatus.xaml
    /// </summary>
    public partial class UcItemExecutionStatus : UserControl
    {
        public UcItemExecutionStatus()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(nameof(Status), typeof(eRunStatus), typeof(UcItemExecutionStatus), new FrameworkPropertyMetadata(eRunStatus.Pending, OnStatusPropertyChanged));
        public eRunStatus Status
        {
            get
            {
                return (eRunStatus)GetValue(StatusProperty);
            }
            set
            {
                SetValue(StatusProperty, value);
                SetStatus();
            }
        }
        private static void OnStatusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcItemExecutionStatus itemExecutionStatus = (UcItemExecutionStatus)d;
            itemExecutionStatus.Status = (eRunStatus)e.NewValue;
        }

        private void SetStatus()
        {
            Brush statusBrush = null;
            eImageType statusImage;
            int imageSize = 13;

            switch (Status)
            {
                case eRunStatus.Passed:
                    statusBrush = FindResource("$PassedStatusColor") as Brush;
                    statusImage = eImageType.Passed;
                    break;
                case eRunStatus.Failed:
                    statusBrush = FindResource("$FailedStatusColor") as Brush;
                    statusImage = eImageType.Failed;
                    break;
                case eRunStatus.Pending:
                    statusBrush = FindResource("$PendingStatusColor") as Brush;
                    statusImage = eImageType.Pending;
                    break;
                case eRunStatus.Running:
                    statusBrush = FindResource("$RunningStatusColor") as Brush;
                    statusImage = eImageType.Running;
                    break;
                case eRunStatus.Stopped:
                    statusBrush = FindResource("$StoppedStatusColor") as Brush;
                    statusImage = eImageType.Stop;
                    imageSize = 10;
                    break;
                case eRunStatus.Blocked:
                    statusBrush = FindResource("$BlockedStatusColor") as Brush;
                    statusImage = eImageType.Blocked;
                    break;
                case eRunStatus.Skipped:
                    statusBrush = FindResource("$SkippedStatusColor") as Brush;
                    statusImage = eImageType.Skipped;
                    break;
                default:
                    statusBrush = FindResource("$PendingStatusColor") as Brush;
                    statusImage = eImageType.Pending;
                    break;
            }

            xPolygon.Fill = statusBrush;
            xPolygon.Stroke = statusBrush;

            xStatusIcon.ImageType = statusImage;
            xStatusIcon.ImageForeground = Brushes.White;

            xStatusIcon.SetAsFontImageWithSize = imageSize;
            xStatusIcon.Width = imageSize;
            xStatusIcon.Height = imageSize;

            xPolygon.ToolTip = Status.ToString();
            xStatusIcon.ToolTip = Status.ToString();
        }
    }
}
