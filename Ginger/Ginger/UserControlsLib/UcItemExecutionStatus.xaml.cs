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
                    imageSize = 11;
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
