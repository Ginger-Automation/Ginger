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
            switch (Status)
            {
                case eRunStatus.Passed:
                    xPolygon.Fill = FindResource("$PassedStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Passed;
                    break;
                case eRunStatus.Failed:
                    xPolygon.Fill = FindResource("$FailedStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Failed;
                    break;
                case eRunStatus.Pending:
                    xPolygon.Fill = FindResource("$PendingStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Pending;
                    break;
                case eRunStatus.Running:
                    xPolygon.Fill = FindResource("$RunningStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Running;
                    break;
                case eRunStatus.Stopped:
                    xPolygon.Fill = FindResource("$StoppedStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Stop;
                    break;
                case eRunStatus.Blocked:
                    xPolygon.Fill = FindResource("$BlockedStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Blocked;
                    break;
                case eRunStatus.Skipped:
                    xPolygon.Fill = FindResource("$SkippedStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Skipped;
                    break;
                default:
                    xPolygon.Fill = FindResource("$PendingStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Pending;
                    break;
            }

            xStatusIcon.ImageForeground = Brushes.White;
        }
    }
}
