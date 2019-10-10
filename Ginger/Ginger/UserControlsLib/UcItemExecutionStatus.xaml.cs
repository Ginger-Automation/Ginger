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
        public enum eStatusViewMode { Polygon, Image, Label}

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
        
        public double StatusSize
        {
            get
            {
                if (StatusViewMode == eStatusViewMode.Polygon)
                {
                    return xPolygonStatusImage.SetAsFontImageWithSize;
                }
                else if (StatusViewMode == eStatusViewMode.Image)
                {
                    return xStatusImagePnl.Height;
                }
                else//label
                {
                    return xStatusLbl.FontSize;
                }
            }
            set
            {                
                if (StatusViewMode == eStatusViewMode.Polygon)
                {
                    xPolygonStatusImage.SetAsFontImageWithSize = value;
                }
                else if (StatusViewMode == eStatusViewMode.Image)
                {
                    xStatusImagePnl.Height = value;
                }
                else//label
                {
                    xStatusLbl.FontSize = value;
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
            switch(StatusViewMode)
            {
                case eStatusViewMode.Polygon:
                    xPolygonStatusPnl.Visibility = Visibility.Visible;
                    xStatusImagePnl.Visibility = Visibility.Collapsed;
                    xStatusLbl.Visibility = Visibility.Collapsed;
                    break;

                case eStatusViewMode.Image:
                    xPolygonStatusPnl.Visibility = Visibility.Collapsed;
                    xStatusImagePnl.Visibility = Visibility.Visible;
                    xStatusLbl.Visibility = Visibility.Collapsed;
                    break;

                case eStatusViewMode.Label:
                    xPolygonStatusPnl.Visibility = Visibility.Collapsed;
                    xStatusImagePnl.Visibility = Visibility.Collapsed;
                    xStatusLbl.Visibility = Visibility.Visible;
                    break;
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

            switch (StatusViewMode)
            {
                case eStatusViewMode.Polygon:
                    xPolygon.Fill = mStatusBrush;
                    xPolygon.Stroke = mStatusBrush;
                    xPolygon.ToolTip = Status.ToString();
                    if (Status == eRunStatus.Stopped)
                    {
                        StatusSize = 10;
                    }
                    xPolygonStatusImage.ImageType = mStatusImage;
                    xPolygonStatusImage.Width = StatusSize;
                    xPolygonStatusImage.Height = StatusSize;
                    xPolygonStatusImage.ToolTip = Status.ToString();
                    break;

                case eStatusViewMode.Image:
                    xStatusImagePnl.Children.Clear();
                    Amdocs.Ginger.UserControls.ImageMakerControl xExecutionStatusImage = new Amdocs.Ginger.UserControls.ImageMakerControl(); //creating new each time due to Spin issue
                    xExecutionStatusImage.ImageType = mStatusImage;
                    xExecutionStatusImage.ImageForeground = (SolidColorBrush)mStatusBrush;
                    xExecutionStatusImage.SetAsFontImageWithSize = StatusSize;
                    xExecutionStatusImage.Width = StatusSize;
                    xExecutionStatusImage.Height = StatusSize;
                    xExecutionStatusImage.HorizontalAlignment = HorizontalAlignment.Center;
                    xExecutionStatusImage.VerticalAlignment = VerticalAlignment.Center;
                    xExecutionStatusImage.ToolTip = Status.ToString();
                    xStatusImagePnl.Children.Add(xExecutionStatusImage);
                    break;

                case eStatusViewMode.Label:
                    xStatusLbl.Content = Status.ToString();
                    xStatusLbl.Foreground = (SolidColorBrush)mStatusBrush;
                    xStatusLbl.FontSize = StatusSize;
                    break;
            }
        }
    }
}
