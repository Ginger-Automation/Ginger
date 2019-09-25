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
        public enum eStatusViewMode { Polygon, Image}

        public static readonly DependencyProperty StatusViewModeProperty = DependencyProperty.Register("StatusViewMode", typeof(eStatusViewMode), typeof(UcItemExecutionStatus),
                             new FrameworkPropertyMetadata(OnStatusViewModePropertyChanged));
        public eStatusViewMode StatusViewMode
        {
            get { return (eStatusViewMode)GetValue(StatusViewModeProperty); }
            set
            {
                SetValue(StatusViewModeProperty, value);
                SetViewModeControls();
                SetStatus();
            }
        }
        private static void OnStatusViewModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcItemExecutionStatus uc = (UcItemExecutionStatus)d;
            uc.StatusViewMode = (eStatusViewMode)e.NewValue;
        }
        
        public double StatusImageSize
        {
            get
            {
                if (StatusViewMode == eStatusViewMode.Polygon)
                {
                    return xPolygonStatusImage.SetAsFontImageWithSize;
                }
                else
                {
                    return xStatusImage.SetAsFontImageWithSize;
                }
            }
            set
            {                
                if (StatusViewMode == eStatusViewMode.Polygon)
                {
                    xPolygonStatusImage.SetAsFontImageWithSize = value;
                }
                else
                {
                    xStatusImage.SetAsFontImageWithSize = value;
                }
            }
        }

        Brush mStatusBrush = null;
        eImageType mStatusImage = eImageType.Pending;


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

        public UcItemExecutionStatus()
        {
            InitializeComponent();

            Init();
        }

        public void Init()
        {
            SetViewModeControls();
            SetStatus();
        }

        private void SetViewModeControls()
        {
            if (StatusViewMode == eStatusViewMode.Polygon)
            {
                xPolygonStatusPnl.Visibility = Visibility.Visible;
                xStatusImage.Visibility = Visibility.Collapsed;
            }
            else//image view mode
            {
                xPolygonStatusPnl.Visibility = Visibility.Collapsed;
                xStatusImage.Visibility = Visibility.Visible;
            }
        }

        private void SetStatus()
        {
            switch (Status)
            {
                case eRunStatus.Passed:
                    mStatusBrush = FindResource("$PassedStatusColor") as Brush;
                    mStatusImage = eImageType.Passed;
                    break;
                case eRunStatus.Failed:
                    mStatusBrush = FindResource("$FailedStatusColor") as Brush;
                    mStatusImage = eImageType.Failed;
                    break;
                case eRunStatus.Pending:
                    mStatusBrush = FindResource("$PendingStatusColor") as Brush;
                    mStatusImage = eImageType.Pending;
                    break;
                case eRunStatus.Running:
                    mStatusBrush = FindResource("$RunningStatusColor") as Brush;
                    mStatusImage = eImageType.Running;
                    break;
                case eRunStatus.Stopped:
                    mStatusBrush = FindResource("$StoppedStatusColor") as Brush;
                    mStatusImage = eImageType.Stop;
                    break;
                case eRunStatus.Blocked:
                    mStatusBrush = FindResource("$BlockedStatusColor") as Brush;
                    mStatusImage = eImageType.Blocked;
                    break;
                case eRunStatus.Skipped:
                    mStatusBrush = FindResource("$SkippedStatusColor") as Brush;
                    mStatusImage = eImageType.Skipped;
                    break;
                default:
                    mStatusBrush = FindResource("$PendingStatusColor") as Brush;
                    mStatusImage = eImageType.Pending;
                    break;
            }

            if (StatusViewMode == eStatusViewMode.Polygon)
            {
                xPolygon.Fill = mStatusBrush;
                xPolygon.Stroke = mStatusBrush;
                xPolygon.ToolTip = Status.ToString();

                if (Status == eRunStatus.Stopped)
                {
                    StatusImageSize = 10;
                }

                xPolygonStatusImage.ImageType = mStatusImage;
                xPolygonStatusImage.Width = StatusImageSize;
                xPolygonStatusImage.Height = StatusImageSize;
                xPolygonStatusImage.ToolTip = Status.ToString();
            }
            else//image view mode
            {
                xStatusImage.ImageForeground = (SolidColorBrush)mStatusBrush;
                xStatusImage.ImageType = mStatusImage;
                xStatusImage.Width = StatusImageSize;
                xStatusImage.Height = StatusImageSize;
                xStatusImage.ToolTip = Status.ToString();
            }
        }
    }
}
