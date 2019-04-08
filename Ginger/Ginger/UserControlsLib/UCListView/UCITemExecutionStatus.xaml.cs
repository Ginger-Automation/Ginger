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
    public partial class UCITemExecutionStatus : UserControl
    {
        public UCITemExecutionStatus()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(nameof(Status), typeof(eRunStatus), typeof(UCITemExecutionStatus), new FrameworkPropertyMetadata(eRunStatus.Pending, OnStatusPropertyChanged));
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
            UCITemExecutionStatus itemExecutionStatus = (UCITemExecutionStatus)d;
            itemExecutionStatus.Status = (eRunStatus)e.NewValue;
        }

        private void SetStatus()
        {
            switch (Status)
            {
                case eRunStatus.Passed:
                    xStatusIcon.Foreground = FindResource("$PassedStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Passed;
                    break;
                case eRunStatus.Failed:
                    xStatusIcon.Foreground = FindResource("$FailedStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Failed;
                    break;
                case eRunStatus.Pending:
                    xStatusIcon.Foreground = FindResource("$PendingStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Pending;
                    break;
                case eRunStatus.Running:
                    xStatusIcon.Foreground = FindResource("$RunningStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Running;
                    break;
                case eRunStatus.Stopped:
                    xStatusIcon.Foreground = FindResource("$StoppedStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Stop;
                    break;
                case eRunStatus.Blocked:
                    xStatusIcon.Foreground = FindResource("$BlockedStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Blocked;
                    break;
                case eRunStatus.Skipped:
                    xStatusIcon.Foreground = FindResource("$SkippedStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Skipped;
                    break;
                default:
                    xStatusIcon.Foreground = FindResource("$PendingStatusColor") as Brush;
                    xStatusIcon.ImageType = eImageType.Pending;
                    break;
            }
        }
    }
}
