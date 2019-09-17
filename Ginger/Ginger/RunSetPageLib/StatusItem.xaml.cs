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

namespace Ginger.MoveToGingerWPF.Run_Set_Pages
{
    /// <summary>
    /// Interaction logic for StatusItem.xaml
    /// </summary>
    public partial class StatusItem : UserControl
    {
        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register("Status", typeof(eRunStatus), typeof(StatusItem),
                        new FrameworkPropertyMetadata(eRunStatus.Pending, OnIconPropertyChanged));
        public eRunStatus Status
        {
            get { return (eRunStatus)GetValue(StatusProperty); }
            set
            {
                SetValue(StatusProperty, value);
                SetStatus();
            }
        }

        public static readonly DependencyProperty StatusIConProperty = DependencyProperty.Register("SetStatusIcon", typeof(bool), typeof(StatusItem),
                        new FrameworkPropertyMetadata(false, OnIconPropertyChanged));
        public bool SetStatusIcon
        {
            get { return (bool)GetValue(StatusIConProperty); }
            set
            {
                SetValue(StatusIConProperty, value);
                SetStatus();
            }
        }

        private static void OnIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StatusItem SI = (StatusItem)d;
            SI.SetStatus();
        }

        public StatusItem()
        {
            InitializeComponent();          
        }

        private void SetStatus()
        {
            if (SetStatusIcon)
            {
                xStatusTri.Visibility = Visibility.Collapsed;
                xStatusIcon.Visibility = Visibility.Visible;
            }
            else
            {
                xStatusTri.Visibility = Visibility.Visible;
                xStatusIcon.Visibility = Visibility.Collapsed;               
            }
            switch (Status)
            {

                case eRunStatus.Passed:
                    if(SetStatusIcon)
                    {
                      xStatusIcon.Foreground = FindResource("$PassedStatusColor") as Brush;
                      xStatusIcon.ImageType = eImageType.Passed;
                    }
                    else
                    {                        
                        xStatusTri.Fill = xStatusTri.Stroke = FindResource("$PassedStatusColor") as Brush;
                    }                    
                    break;
                case eRunStatus.Failed:
                    if (SetStatusIcon)
                    {
                        xStatusIcon.Foreground = FindResource("$FailedStatusColor") as Brush;
                        xStatusIcon.ImageType = eImageType.Failed;
                    }
                    else
                    {
                        xStatusTri.Fill = xStatusTri.Stroke = FindResource("$FailedStatusColor") as Brush;
                    }
                    break;
                case eRunStatus.Pending:
                    if (SetStatusIcon)
                    {
                        xStatusIcon.Foreground = FindResource("$PendingStatusColor") as Brush;
                        xStatusIcon.ImageType = eImageType.Pending;
                    }
                    else
                    {
                        xStatusTri.Fill = xStatusTri.Stroke = FindResource("$PendingStatusColor") as Brush;
                    }
                    break;
                case eRunStatus.Running:
                    if (SetStatusIcon)
                    {
                        xStatusIcon.Foreground = FindResource("$RunningStatusColor") as Brush;
                        xStatusIcon.ImageType = eImageType.Running;
                    }
                    else
                    {
                        xStatusTri.Fill = xStatusTri.Stroke = FindResource("$RunningStatusColor") as Brush;
                    }
                    break;
                case eRunStatus.Stopped:
                    if (SetStatusIcon)
                    {
                        xStatusIcon.Foreground = FindResource("$StoppedStatusColor") as Brush;
                        xStatusIcon.ImageType = eImageType.Stop;
                    }
                    else
                    {
                        xStatusTri.Fill = xStatusTri.Stroke = FindResource("$StoppedStatusColor") as Brush;
                    }
                    break;
                case eRunStatus.Blocked:
                    if (SetStatusIcon)
                    {
                        xStatusIcon.Foreground = FindResource("$BlockedStatusColor") as Brush;
                        xStatusIcon.ImageType = eImageType.Blocked;
                    }
                    else
                    {
                        xStatusTri.Fill = xStatusTri.Stroke = FindResource("$BlockedStatusColor") as Brush;
                    }
                    break;
                case eRunStatus.Skipped:
                    if (SetStatusIcon)
                    {
                        xStatusIcon.Foreground = FindResource("$SkippedStatusColor") as Brush;
                        xStatusIcon.ImageType = eImageType.Skipped;
                    }
                    else
                    {
                        xStatusTri.Fill = xStatusTri.Stroke = FindResource("$SkippedStatusColor") as Brush;
                    }
                    break;
                default:
                    if (SetStatusIcon)
                    {
                        xStatusIcon.Foreground = FindResource("$PendingStatusColor") as Brush;
                        xStatusIcon.ImageType = eImageType.Pending;
                    }
                    else
                    {
                        xStatusTri.Fill = xStatusTri.Stroke = FindResource("$PendingStatusColor") as Brush;
                    }
                    break;
            }
        }
    }
}
