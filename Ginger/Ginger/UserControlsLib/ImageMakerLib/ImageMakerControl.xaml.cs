#region License
/*
Copyright © 2014-2022 European Support Limited

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

using Amdocs.Ginger.Core;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FontAwesome.Sharp;
using Amdocs.Ginger.Common.Enums;
using System.Windows.Media.Animation;

namespace Amdocs.Ginger.UserControls
{
    /// <summary>
    /// Interaction logic for ImageMaker.xaml
    /// </summary>
    public partial class ImageMakerControl : UserControl
    {
        // Icon Property
        // We list all available icons for Ginger, this icons can be resized and will automatically match
        public static readonly DependencyProperty ImageTypeProperty = DependencyProperty.Register("ImageType", typeof(eImageType), typeof(ImageMakerControl),
                        new FrameworkPropertyMetadata(eImageType.Null, OnIconPropertyChanged));

        private static void OnIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageMakerControl IMC = (ImageMakerControl)d;
            IMC.SetImage();
        }

        public eImageType ImageType
        {
            get { return (eImageType)GetValue(ImageTypeProperty); }
            set
            {
                SetValue(ImageTypeProperty, value);
                SetImage();
            }
        }



        public static readonly DependencyProperty ImageToolTipProperty = DependencyProperty.Register("ImageToolTip", typeof(string), typeof(ImageMakerControl),
                       new FrameworkPropertyMetadata(string.Empty, OnToolTipPropertyChanged));

        private static void OnToolTipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageMakerControl IMC = (ImageMakerControl)d;
            IMC.SetImageToolTip();

        }

        public string ImageToolTip
        {
            get { return (string)GetValue(ImageToolTipProperty); }
            set
            {
                SetValue(ImageToolTipProperty, value);
                SetImageToolTip();
            }
        }

        public void SetImageToolTip()
        {
            if (!string.IsNullOrEmpty(ImageToolTip))
            {
                xFAImage.ToolTip = ImageToolTip;
                xFAFont.ToolTip = ImageToolTip;
            }
        }


        //Font Size Property
        //if this value is set then instead of showing image will show FAFont and set its FontSize 
        public static readonly DependencyProperty SetAsFontImageWithSizeProperty = DependencyProperty.Register("SetAsFontImageWithSize", typeof(double), typeof(ImageMakerControl), new FrameworkPropertyMetadata(0.0, OnIconPropertyChanged));
        public double SetAsFontImageWithSize
        {
            get
            {
                return (double)GetValue(SetAsFontImageWithSizeProperty);
            }
            set
            {
                SetValue(SetAsFontImageWithSizeProperty, value);
                SetImage();
            }
        }

        public static readonly DependencyProperty SetImageMakerBorderProperty = DependencyProperty.Register("SetBorder", typeof(bool), typeof(ImageMakerControl), new FrameworkPropertyMetadata(false, OnIconPropertyChanged));
        public bool SetBorder
        {
            get
            {
                return (bool)GetValue(SetImageMakerBorderProperty);
            }
            set
            {
                SetValue(SetImageMakerBorderProperty, value);
                SetImage();
            }
        }

        public static readonly DependencyProperty SetImageMakerEnableProperty = DependencyProperty.Register("Enabled", typeof(bool), typeof(ImageMakerControl), new FrameworkPropertyMetadata(true, OnIconPropertyChanged));
        public bool Enabled
        {
            get
            {
                return (bool)GetValue(SetImageMakerEnableProperty);
            }
            set
            {
                SetValue(SetImageMakerEnableProperty, value);
                SetImage();
            }
        }

        public static readonly DependencyProperty SetImageMakerForegroundProperty = DependencyProperty.Register("ImageForeground", typeof(SolidColorBrush), typeof(ImageMakerControl), new FrameworkPropertyMetadata(null, null));
        public SolidColorBrush ImageForeground
        {
            get
            {
                return (SolidColorBrush)GetValue(SetImageMakerForegroundProperty);
            }
            set
            {
                SetValue(SetImageMakerForegroundProperty, value);
                SetImage();
            }
        }

        public ImageMakerControl()
        {
            InitializeComponent();
        }

        private void SetImage()
        {
            ResetImageView();
            switch (ImageType)
            {
                #region General Images
                //############################## General Images:
                case eImageType.Empty:
                    // Do nothing and leave it empty
                    // SetAsFontAwesomeIcon(IconChar.Ban);                    
                    break;
                case eImageType.Ginger:
                    SetAsStaticImage("Ginger.png");
                    break;
                case eImageType.GingerIconWhite:
                    SetAsStaticImage("GingerIconWhite.png");
                    break;
                case eImageType.GingerIconGray:
                    SetAsStaticImage("GingerIconInGrayNoBackground.png");
                    break;
                case eImageType.GingerLogo:
                    SetAsStaticImage("GingerByAmdocsLogo.png");
                    break;
                case eImageType.GingerLogoGray:
                    SetAsStaticImage("GingerByAmdocsLogoGray.png");
                    break;
                case eImageType.GingerLogoWhiteSmall:
                    SetAsStaticImage("GingerByAmdocsLogoWhiteSmall.png");
                    break;
                case eImageType.GingerSplash:
                    SetAsStaticImage("GingerSplashImageNew.png");
                    break;
                #endregion


                #region Repository Items Images
                //############################## Repository Items Images:
                case eImageType.Solution:
                case eImageType.ApplicationModel:
                    SetAsFontAwesomeIcon(IconChar.ThLarge);
                    break;
                case eImageType.BusinessFlow:
                    SetAsFontAwesomeIcon(IconChar.Sitemap);
                    break;
                case eImageType.ActivitiesGroup:
                    SetAsFontAwesomeIcon(IconChar.ObjectGroup);
                    break;
                case eImageType.Activity:
                    SetAsFontAwesomeIcon(IconChar.Bars);
                    break;
                case eImageType.Action:
                    SetAsFontAwesomeIcon(IconChar.Bolt);
                    break;
                case eImageType.Agent:
                    SetAsFontAwesomeIcon(IconChar.User);
                    break;
                case eImageType.RunSet:
                    SetAsFontAwesomeIcon(IconChar.PlayCircle);
                    break;
                case eImageType.APIModel:
                    SetAsFontAwesomeIcon(IconChar.ExchangeAlt);
                    break;
                case eImageType.Runner:
                    SetAsFontAwesomeIcon(IconChar.PlayCircle);
                    break;
                case eImageType.Operations:
                    SetAsFontAwesomeIcon(IconChar.Cogs);
                    break;
                case eImageType.Environment:
                case eImageType.Globe:
                    SetAsFontAwesomeIcon(IconChar.Globe);
                    break;
                case eImageType.Application:
                case eImageType.ApplicationPOMModel:
                    SetAsFontAwesomeIcon(IconChar.WindowMaximize);
                    break;
                case eImageType.HtmlReport:
                    SetAsFontAwesomeIcon(IconChar.Html5);
                    break;
                case eImageType.SharedRepositoryItem:
                    SetAsFontAwesomeIcon(IconChar.Star, Brushes.Orange);
                    break;
                case eImageType.NonSharedRepositoryItem:
                    SetAsFontAwesomeIcon(IconChar.Star, Brushes.Gray);
                    break;
                case eImageType.SharedRepositoryItemDark:
                    SetAsFontAwesomeIcon(IconChar.Star);
                    break;
                case eImageType.Tag:
                case eImageType.Ticket:
                    SetAsFontAwesomeIcon(IconChar.Tag);
                    break;
                case eImageType.DataSource:
                    SetAsFontAwesomeIcon(IconChar.Database);
                    break;
                case eImageType.PluginPackage:
                    SetAsFontAwesomeIcon(IconChar.Plug);
                    break;
                #endregion


                #region Variable Item Images
                //############################## Variables Images:
                case eImageType.Variable:
                    SetAsFontAwesomeIcon(IconChar.Code);
                    break;
                case eImageType.VariableList:
                    SetAsFontAwesomeIcon(IconChar.List);
                    break;
                case eImageType.Password:
                    SetAsFontAwesomeIcon(IconChar.Key);
                    break;
                case eImageType.Random:
                    SetAsFontAwesomeIcon(IconChar.Random);
                    break;
                case eImageType.Sequence:
                    SetAsFontAwesomeIcon(IconChar.AlignJustify);
                    break;
                case eImageType.Timer:
                    SetAsFontAwesomeIcon(IconChar.Clock);
                    break;
                #endregion


                #region Execution Status Images
                //############################## Execution Status Images:
                case eImageType.Passed:
                    SetAsFontAwesomeIcon(IconChar.CheckCircle, (SolidColorBrush)FindResource("$PassedStatusColor"), 0, "Passed");
                    break;
                case eImageType.Unknown:
                    SetAsFontAwesomeIcon(IconChar.QuestionCircle, null, 0, "Unknown");
                    break;
                case eImageType.Failed:
                    SetAsFontAwesomeIcon(IconChar.TimesCircle, (SolidColorBrush)FindResource("$FailedStatusColor"), 0, "Failed");
                    break;
                case eImageType.Pending:
                    SetAsFontAwesomeIcon(IconChar.Clock, (SolidColorBrush)FindResource("$PendingStatusColor"), 0, "Pending");
                    break;
                case eImageType.Recording:
                    SetAsFontAwesomeIcon(IconChar.Camera , new SolidColorBrush(Color.FromRgb(255, 0, 0)), 0, "Recording...", true);
                    break;
                case eImageType.Processing:
                    SetAsFontAwesomeIcon(IconChar.Spinner, (LinearGradientBrush)FindResource("$amdocsLogoLinarGradientBrush_NewAmdocsColors"), 2);
                    break;
                case eImageType.Ready:
                    SetAsFontAwesomeIcon(IconChar.ThumbsUp, (SolidColorBrush)FindResource("$PendingStatusColor"));
                    break;
                case eImageType.Stopped:
                    SetAsFontAwesomeIcon(IconChar.StopCircle, (SolidColorBrush)FindResource("$StoppedStatusColor"), 0, "Stopped");
                    break;
                case eImageType.Blocked:
                    SetAsFontAwesomeIcon(IconChar.Ban, (SolidColorBrush)FindResource("$BlockedStatusColor"), 0, "Blocked");
                    break;
                case eImageType.Skipped:
                    SetAsFontAwesomeIcon(IconChar.MinusCircle, (SolidColorBrush)FindResource("$SkippedStatusColor"), 0, "Skipped");
                    break;
                case eImageType.Running:
                    SetAsFontAwesomeIcon(IconChar.Spinner, (SolidColorBrush)FindResource("$RunningStatusColor"), 2, "Running");
                    break;
                case eImageType.Mapped:
                    SetAsFontAwesomeIcon(IconChar.CheckCircle);
                    break;
                case eImageType.Partial:
                    SetAsFontAwesomeIcon(IconChar.ExclamationTriangle);
                    break;
                case eImageType.UnMapped:
                    SetAsFontAwesomeIcon(IconChar.TimesCircle);
                    break;
                #endregion


                #region Operations Images
                //############################## Operations Images:
                case eImageType.Add:
                    SetAsFontAwesomeIcon(IconChar.Plus);
                    break;
                case eImageType.Refresh:
                    SetAsFontAwesomeIcon(IconChar.Spinner);
                    break;
                case eImageType.Config:
                    SetAsFontAwesomeIcon(IconChar.Cog);
                    break;
                case eImageType.Edit:
                    SetAsFontAwesomeIcon(IconChar.PencilAlt);
                    break;
                case eImageType.Save:
                    SetAsFontAwesomeIcon(IconChar.Save);
                    break;
                case eImageType.Reply:
                    SetAsFontAwesomeIcon(IconChar.Reply);
                    break;

                case eImageType.Run:
                    SetAsFontAwesomeIcon(IconChar.Play);
                    break;
                case eImageType.RunSingle:
                    SetAsFontAwesomeIcon(IconChar.PlayCircle);
                    break;
                case eImageType.RunAll:
                    SetAsFontAwesomeIcon(IconChar.PlayCircle);
                    break;
                case eImageType.Stop:
                    SetAsFontAwesomeIcon(IconChar.Stop);
                    break;
                case eImageType.StopAll:
                    SetAsFontAwesomeIcon(IconChar.StopCircle);
                    break;
                case eImageType.Close:
                    SetAsFontAwesomeIcon(IconChar.WindowClose);
                    break;
                case eImageType.Continue:
                    SetAsFontAwesomeIcon(IconChar.FastForward);
                    break;
                case eImageType.Open:
                    SetAsFontAwesomeIcon(IconChar.FolderOpen);
                    break;
                case eImageType.New:
                    SetAsFontAwesomeIcon(IconChar.Magic);
                    break;
                case eImageType.Analyze:
                    SetAsFontAwesomeIcon(IconChar.Stethoscope);
                    break;
                case eImageType.GoBack:
                    SetAsFontAwesomeIcon(IconChar.ArrowLeft);
                    break;
                case eImageType.GoNext:
                    SetAsFontAwesomeIcon(IconChar.ArrowRight);
                    break;
                case eImageType.Finish:
                    SetAsFontAwesomeIcon(IconChar.FlagCheckered);
                    break;
                case eImageType.Cancel:
                    SetAsFontAwesomeIcon(IconChar.WindowClose);
                    break;
                case eImageType.Reset:
                    SetAsFontAwesomeIcon(IconChar.Spinner);
                    break;
                case eImageType.Undo:
                    SetAsFontAwesomeIcon(IconChar.Undo);
                    break;
                case eImageType.Simulate:
                    SetAsFontAwesomeIcon(IconChar.Android);
                    break;
                case eImageType.Copy:
                    SetAsFontAwesomeIcon(IconChar.Copy);
                    break;
                case eImageType.Cut:
                    SetAsFontAwesomeIcon(IconChar.Cut);
                    break;
                case eImageType.Paste:
                    SetAsFontAwesomeIcon(IconChar.Paste);
                    break;
                case eImageType.Delete:
                    SetAsFontAwesomeIcon(IconChar.Trash);
                    break;
                case eImageType.DeleteSingle:
                    SetAsFontAwesomeIcon(IconChar.Minus);
                    break;
                case eImageType.Minimize:
                    SetAsFontAwesomeIcon(IconChar.WindowMinimize);
                    break;
                case eImageType.MoveRight:
                    SetAsFontAwesomeIcon(IconChar.ArrowRight);
                    break;
                case eImageType.MoveLeft:
                    SetAsFontAwesomeIcon(IconChar.ArrowLeft);
                    break;
                case eImageType.MoveUp:
                    SetAsFontAwesomeIcon(IconChar.ArrowUp);
                    break;
                case eImageType.MoveDown:
                    SetAsFontAwesomeIcon(IconChar.ArrowDown);
                    break;
                case eImageType.MoveUpDown:
                    SetAsFontAwesomeIcon(IconChar.ArrowsAltV);
                    break;
                case eImageType.Reorder:
                    SetAsFontAwesomeIcon(IconChar.FirstOrder);
                    break;
                case eImageType.Retweet:
                    SetAsFontAwesomeIcon(IconChar.Retweet);
                    break;
                case eImageType.Automate:
                    SetAsFontAwesomeIcon(IconChar.Cogs);
                    break;
                case eImageType.Convert:
                    SetAsFontAwesomeIcon(IconChar.ExchangeAlt);
                    break;
                case eImageType.ParallelExecution:
                    SetAsFontAwesomeIcon(IconChar.Random);
                    break;
                case eImageType.SequentialExecution:
                    SetAsFontAwesomeIcon(IconChar.SortNumericUp);
                    break;
                case eImageType.Search:
                    SetAsFontAwesomeIcon(IconChar.Search);
                    break;
                case eImageType.Duplicate:
                    SetAsFontAwesomeIcon(IconChar.FileAlt);
                    break;
                case eImageType.Merge:
                    SetAsFontAwesomeIcon(IconChar.ObjectGroup);
                    break;
                case eImageType.Sync:
                case eImageType.InstanceLink:
                    SetAsFontAwesomeIcon(IconChar.Link);
                    break;
                case eImageType.UnSync:
                    SetAsFontAwesomeIcon(IconChar.Unlink);
                    break;
                case eImageType.Visible:
                    SetAsFontAwesomeIcon(IconChar.Eye);
                    break;
                case eImageType.Invisible:
                    SetAsFontAwesomeIcon(IconChar.EyeSlash);
                    break;
                case eImageType.View:
                    SetAsFontAwesomeIcon(IconChar.Eye);
                    break;
                case eImageType.Download:
                case eImageType.GetLatest:
                    SetAsFontAwesomeIcon(IconChar.CloudDownloadAlt);
                    break;
                case eImageType.CheckIn:
                    SetAsFontAwesomeIcon(IconChar.CloudUploadAlt);
                    break;
                case eImageType.Upload:
                    SetAsFontAwesomeIcon(IconChar.Upload);
                    break;
                case eImageType.Expand:
                    SetAsFontAwesomeIcon(IconChar.ChevronDown);
                    break;
                case eImageType.Collapse:
                    SetAsFontAwesomeIcon(IconChar.ChevronUp);
                    break;
                case eImageType.ExpandAll:
                    SetAsFontAwesomeIcon(IconChar.AngleDoubleDown);
                    break;
                case eImageType.CollapseAll:
                    SetAsFontAwesomeIcon(IconChar.AngleDoubleUp);
                    break;
                case eImageType.ActiveAll:
                    SetAsFontAwesomeIcon(IconChar.Check);
                    break;
                case eImageType.ExpandToFullScreen:
                    SetAsFontAwesomeIcon(IconChar.Expand);
                    break;
                case eImageType.Export:
                    SetAsFontAwesomeIcon(IconChar.ShareAlt);
                    break;
                case eImageType.ImportFile:
                    SetAsFontAwesomeIcon(IconChar.Download);
                    break;
                case eImageType.Times:
                    SetAsFontAwesomeIcon(IconChar.Times);
                    break;
                case eImageType.Times_Red:
                    SetAsFontAwesomeIcon(IconChar.Times, (SolidColorBrush)FindResource("$HighlightColor_Red"), 0, "ToolTip");
                    break;
                case eImageType.Exchange:
                    SetAsFontAwesomeIcon(IconChar.ExchangeAlt);
                    break;
                case eImageType.Share:
                    SetAsFontAwesomeIcon(IconChar.ShareAlt);
                    break;
                case eImageType.ShareExternal:
                    SetAsFontAwesomeIcon(IconChar.ShareAltSquare);
                    break;
                case eImageType.Filter:
                    SetAsFontAwesomeIcon(IconChar.Filter);
                    break;
                case eImageType.Upgrade:
                    SetAsFontAwesomeIcon(IconChar.ArrowCircleUp);
                    break;
                case eImageType.Recover:
                    SetAsFontAwesomeIcon(IconChar.Recycle);
                    break;
                case eImageType.Approve:
                    SetAsFontAwesomeIcon(IconChar.ThumbsUp);
                    break;
                case eImageType.Reject:
                    SetAsFontAwesomeIcon(IconChar.ThumbsDown);
                    break;
                case eImageType.Retry:
                    SetAsFontAwesomeIcon(IconChar.Spinner);
                    break;
                case eImageType.Warn:
                    SetAsFontAwesomeIcon(IconChar.ExclamationTriangle);
                    break;
                case eImageType.HighWarn:
                    SetAsFontAwesomeIcon(IconChar.ExclamationTriangle, Brushes.Red);
                    break;
                case eImageType.MediumWarn:
                    SetAsFontAwesomeIcon(IconChar.ExclamationTriangle, Brushes.Orange);
                    break;
                case eImageType.LowWarn:
                    SetAsFontAwesomeIcon(IconChar.ExclamationTriangle, Brushes.Yellow);
                    break;
                case eImageType.EditWindow:
                    SetAsFontAwesomeIcon(IconChar.Edit);
                    break;
                case eImageType.CLI:
                    SetAsFontAwesomeIcon(IconChar.ExternalLinkAlt);
                    break;
                case eImageType.WindowRestore:
                    SetAsFontAwesomeIcon(IconChar.WindowRestore);
                    break;
                case eImageType.SelfHealing:
                    SetAsFontAwesomeIcon(IconChar.Magic);
                    break;
                #endregion


                #region Items Images
                //############################## Items Images:
                case eImageType.KidsDrawing:
                    xViewBox.Visibility = Visibility.Visible;
                    xViewBox.Child = GetKidsDrawingShape();
                    break;
                case eImageType.FlowDiagram:
                    SetAsFontAwesomeIcon(IconChar.Sitemap);
                    break;
                case eImageType.DataTable:
                    SetAsFontAwesomeIcon(IconChar.Table);
                    break;
                case eImageType.Parameter:
                    SetAsFontAwesomeIcon(IconChar.SlidersH);
                    break;
                case eImageType.File:
                    SetAsFontAwesomeIcon(IconChar.FileAlt);
                    break;
                case eImageType.Folder:
                    SetAsFontAwesomeIcon(IconChar.Folder);
                    break;
                case eImageType.OpenFolder:
                    SetAsFontAwesomeIcon(IconChar.FolderOpen);
                    break;
                case eImageType.EllipsisH:
                    SetAsFontAwesomeIcon(IconChar.EllipsisH);
                    break;
                case eImageType.ListGroup:
                    SetAsFontAwesomeIcon(IconChar.ListUl);
                    break;
                case eImageType.Sitemap:
                    SetAsFontAwesomeIcon(IconChar.Sitemap);
                    break;
                case eImageType.ItemModified:
                    SetAsFontAwesomeIcon(IconChar.Asterisk, Brushes.DarkOrange, 5, "Item was modified");
                    break;
                case eImageType.Clock:
                    SetAsFontAwesomeIcon(IconChar.Clock);
                    break;
                case eImageType.Report:
                    SetAsFontAwesomeIcon(IconChar.ChartPie);
                    break;
                case eImageType.Active:
                    SetAsFontAwesomeIcon(IconChar.ToggleOn);
                    break;
                case eImageType.InActive:
                    SetAsFontAwesomeIcon(IconChar.ToggleOff);
                    break;
                case eImageType.History:
                    SetAsFontAwesomeIcon(IconChar.History);
                    break;
                case eImageType.ChevronDown:
                    SetAsFontAwesomeIcon(IconChar.ChevronCircleDown);
                    break;
                case eImageType.Question:
                    SetAsFontAwesomeIcon(IconChar.QuestionCircle);
                    break;
                case eImageType.Help:
                    SetAsFontAwesomeIcon(IconChar.LifeRing);
                    break;
                case eImageType.Screen:
                    SetAsFontAwesomeIcon(IconChar.Desktop);
                    break;
                case eImageType.Info:
                    SetAsFontAwesomeIcon(IconChar.InfoCircle);
                    break;
                case eImageType.Service:
                    SetAsFontAwesomeIcon(IconChar.Headphones);
                    break;
                case eImageType.FileVideo:
                    SetAsFontAwesomeIcon(IconChar.FileVideo);
                    break;
                case eImageType.Email:
                    SetAsFontAwesomeIcon(IconChar.Envelope);
                    break;
                case eImageType.SourceControl:
                    SetAsFontAwesomeIcon(IconChar.CodeBranch);
                    break;
                case eImageType.SourceControlNew:
                    SetAsFontAwesomeIcon(IconChar.Plus, Brushes.Green, toolTip: "New");
                    break;
                case eImageType.SourceControlModified:
                    SetAsFontAwesomeIcon(IconChar.PencilAlt, Brushes.OrangeRed, toolTip: "Modified");
                    break;
                case eImageType.SourceControlDeleted:
                    SetAsFontAwesomeIcon(IconChar.Minus, Brushes.Red, toolTip: "Deleted");
                    break;
                case eImageType.SourceControlEquel:
                    SetAsFontAwesomeIcon(IconChar.Check, Brushes.Gray, toolTip: "Same as Source");
                    break;
                case eImageType.SourceControlLockedByAnotherUser:
                    SetAsFontAwesomeIcon(IconChar.Lock, Brushes.Purple, toolTip: "Locked by Other User");
                    break;
                case eImageType.SourceControlLockedByMe:
                    SetAsFontAwesomeIcon(IconChar.Lock, Brushes.SaddleBrown, toolTip: "Locked by You");
                    break;
                case eImageType.SourceControlError:
                    SetAsFontAwesomeIcon(IconChar.ExclamationCircle, Brushes.Red, toolTip: "Error in checking status");
                    break;
                case eImageType.Check:
                    SetAsFontAwesomeIcon(IconChar.CheckCircle);
                    break;
                case eImageType.Bug:
                    SetAsFontAwesomeIcon(IconChar.Bug);
                    break;
                case eImageType.Power:
                    SetAsFontAwesomeIcon(IconChar.PowerOff);
                    break;
                case eImageType.Pointer:
                    SetAsFontAwesomeIcon(IconChar.HandPointer);
                    break;
                case eImageType.Camera:
                    SetAsFontAwesomeIcon(IconChar.Camera);
                    break;
                case eImageType.ExcelFile:
                    SetAsFontAwesomeIcon(IconChar.FileExcel);
                    break;
                case eImageType.PlusSquare:
                    SetAsFontAwesomeIcon(IconChar.PlusSquare);
                    break;
                case eImageType.Wrench:
                case eImageType.Fix:
                    SetAsFontAwesomeIcon(IconChar.Wrench);
                    break;

                case eImageType.Eraser:
                    SetAsFontAwesomeIcon(IconChar.Eraser);
                    break;

                case eImageType.ArrowDown:
                    SetAsFontAwesomeIcon(IconChar.AngleDown);
                    break;
                case eImageType.ArrowRight:
                    SetAsFontAwesomeIcon(IconChar.ArrowRight);
                    break;
                case eImageType.User:
                    SetAsFontAwesomeIcon(IconChar.UserCircle);
                    break;
                case eImageType.UserProfile:
                    SetAsFontAwesomeIcon(IconChar.UserMd);
                    break;
                case eImageType.Forum:
                    SetAsFontAwesomeIcon(IconChar.Comment);
                    break;
                case eImageType.Website:
                    SetAsFontAwesomeIcon(IconChar.Laptop);
                    break;
                case eImageType.Beta:
                    SetAsFontAwesomeIcon(IconChar.Android, Brushes.Orange);
                    break;
                case eImageType.Error:
                    SetAsFontAwesomeIcon(IconChar.TimesCircle);
                    break;
                case eImageType.Coffee:
                    SetAsFontAwesomeIcon(IconChar.Coffee, Brushes.Red);
                    break;
                case eImageType.MapSigns:
                    SetAsFontAwesomeIcon(IconChar.MapSigns);
                    break;
                case eImageType.Elements:
                    SetAsFontAwesomeIcon(IconChar.Th);
                    break;
                case eImageType.LocationPointer:
                    SetAsFontAwesomeIcon(IconChar.LocationArrow);
                    break;
                case eImageType.GitHub:
                    SetAsFontAwesomeIcon(IconChar.Github);
                    break;
                case eImageType.Ping:
                    SetAsFontAwesomeIcon(IconChar.ExchangeAlt);
                    break;
                case eImageType.Database:
                    SetAsFontAwesomeIcon(IconChar.Database);
                    break;
                case eImageType.Output:
                    SetAsFontAwesomeIcon(IconChar.Upload);
                    break;
                case eImageType.Input:
                    SetAsFontAwesomeIcon(IconChar.Download);
                    break;
                case eImageType.Spy:
                    SetAsFontAwesomeIcon(IconChar.UserSecret);
                    break;

                case eImageType.CodeFile:
                    SetAsFontAwesomeIcon(IconChar.FileCode);
                    break;

                case eImageType.Rows:
                    SetAsFontAwesomeIcon(IconChar.ArrowsAltH);
                    break;

                case eImageType.Column:
                    SetAsFontAwesomeIcon(IconChar.ArrowsAltV);
                    break;

                case eImageType.Columns:
                    SetAsFontAwesomeIcon(IconChar.Columns);
                    break;

                case eImageType.Browser:
                    SetAsFontAwesomeIcon(IconChar.Chrome);
                    break;
               case eImageType.Java:
                    SetAsFontAwesomeIcon(IconChar.Coffee);
                    break;
               case eImageType.KeyboardLayout:
                    SetAsFontAwesomeIcon(IconChar.Keyboard);
                    break;

                case eImageType.Linux:
                    SetAsFontAwesomeIcon(IconChar.Linux);
                    break;

                case eImageType.BatteryThreeQuarter:
                    SetAsFontAwesomeIcon(IconChar.BatteryThreeQuarters);
                    break;

                case eImageType.Mobile:
                    SetAsFontAwesomeIcon(IconChar.Mobile);
                    break;

                case eImageType.Codepen:
                    SetAsFontAwesomeIcon(IconChar.Codepen);
                    break;

                case eImageType.MousePointer:
                    SetAsFontAwesomeIcon(IconChar.MousePointer);
                    break;

                case eImageType.AudioFileOutline:
                    SetAsFontAwesomeIcon(IconChar.FileAudio);
                    break;

                case eImageType.ChartLine:
                    SetAsFontAwesomeIcon(IconChar.ChartLine);
                    break;

                case eImageType.Suitcase:
                    SetAsFontAwesomeIcon(IconChar.Suitcase);
                    break;

                case eImageType.Paragraph:
                    SetAsFontAwesomeIcon(IconChar.Paragraph);
                    break;

                case eImageType.Graph:
                    SetAsFontAwesomeIcon(IconChar.ChartLine);
                    break;

                case eImageType.BullsEye:
                    SetAsFontAwesomeIcon(IconChar.Bullseye);
                    break;

                case eImageType.WindowsIcon:
                    SetAsFontAwesomeIcon(IconChar.Windows);
                    break;

                case eImageType.PDFFile:
                    SetAsFontAwesomeIcon(IconChar.FilePdf);
                    break;

                case eImageType.CSS3Text:
                    SetAsFontAwesomeIcon(IconChar.Css3);
                    break;

                case eImageType.Languages:
                    SetAsFontAwesomeIcon(IconChar.Language);
                    break;

                case eImageType.MinusSquare:
                    SetAsFontAwesomeIcon(IconChar.MinusSquare);
                    break;
                case eImageType.Mandatory:
                    SetAsFontAwesomeIcon(IconChar.ExclamationCircle);
                    break;
                case eImageType.ALM:
                    SetAsFontAwesomeIcon(IconChar.Qrcode);//need to find better image type
                    break;
                case eImageType.MapALM:
                    SetAsFontAwesomeIcon(IconChar.MapSigns);
                    break;
                case eImageType.CSV:
                    SetAsFontAwesomeIcon(IconChar.FileCsv);
                    break;
                case eImageType.Clipboard:
                    SetAsFontAwesomeIcon(IconChar.Clipboard);
                    break;
                case eImageType.ID:
                    SetAsFontAwesomeIcon(IconChar.IdCardAlt);
                    break;
                case eImageType.RegularExpression:
                    SetAsFontAwesomeIcon(IconChar.Registered);
                    break;
                case eImageType.DataManipulation:
                    SetAsFontAwesomeIcon(IconChar.Th);
                    break;
                case eImageType.General:
                    SetAsFontAwesomeIcon(IconChar.Gg);
                    break;
                case eImageType.SignIn:
                    SetAsFontAwesomeIcon(IconChar.SignInAlt);
                    break;
                case eImageType.SignOut:
                    SetAsFontAwesomeIcon(IconChar.SignOutAlt);
                    break;
                case eImageType.AngleArrowUp:
                    SetAsFontAwesomeIcon(IconChar.AngleUp);
                    break;
                case eImageType.AngleArrowDown:
                    SetAsFontAwesomeIcon(IconChar.AngleDown);
                    break;
                case eImageType.AngleArrowLeft:
                    SetAsFontAwesomeIcon(IconChar.AngleLeft);
                    break;
                case eImageType.AngleArrowRight:
                    SetAsFontAwesomeIcon(IconChar.AngleRight);
                    break;
                case eImageType.Support:
                    SetAsFontAwesomeIcon(IconChar.Headphones);
                    break;
                case eImageType.Asterisk:
                    SetAsFontAwesomeIcon(IconChar.Asterisk);
                    break;
                #endregion

                #region ElementType Images
                case eImageType.Button:
                    SetAsFontAwesomeIcon(IconChar.HandPointer);
                    break;
                case eImageType.TextBox:
                    SetAsFontAwesomeIcon(IconChar.PenSquare);
                    break;
                case eImageType.Image:
                    SetAsFontAwesomeIcon(IconChar.Image);
                    break;
                case eImageType.CheckBox:
                    SetAsFontAwesomeIcon(IconChar.CheckSquare);
                    break;
                case eImageType.RadioButton:
                    SetAsFontAwesomeIcon(IconChar.DotCircle);
                    break;
                case eImageType.Link:
                    SetAsFontAwesomeIcon(IconChar.ExternalLinkAlt);
                    break;
                case eImageType.Element:
                    SetAsFontAwesomeIcon(IconChar.Square);
                    break;
                case eImageType.Menu:
                    SetAsFontAwesomeIcon(IconChar.Bars);
                    break;
                case eImageType.Label:
                    SetAsFontAwesomeIcon(IconChar.Font);
                    break;
                case eImageType.DropList:
                    SetAsFontAwesomeIcon(IconChar.ListAlt);
                    break;
                case eImageType.List:
                    SetAsFontAwesomeIcon(IconChar.ListOl);
                    break;
                case eImageType.Window:
                    SetAsFontAwesomeIcon(IconChar.WindowMaximize);
                    break;
                case eImageType.Toggle:
                    SetAsFontAwesomeIcon(IconChar.ToggleOn);
                    break;
                case eImageType.Table:
                    SetAsFontAwesomeIcon(IconChar.Table);
                    break;
                case eImageType.Text:
                    SetAsFontAwesomeIcon(IconChar.TextWidth);
                    break;
                case eImageType.LinkSquare:
                    SetAsFontAwesomeIcon(IconChar.ExternalLinkSquareAlt);
                    break;
                case eImageType.DatePicker:
                    SetAsFontAwesomeIcon(IconChar.Calendar);
                    break;
                case eImageType.TreeView:
                    SetAsFontAwesomeIcon(IconChar.Sitemap);
                    break;
                case eImageType.Pin:
                    SetAsFontAwesomeIcon(IconChar.Thumbtack);
                    break;
                case eImageType.Square:
                    SetAsFontAwesomeIcon(IconChar.Square);
                    break;
                case eImageType.Triangle:
                    SetAsFontAwesomeIcon(IconChar.Play);
                    break;
                case eImageType.Circle:
                    SetAsFontAwesomeIcon(IconChar.Circle);
                    break;
                case eImageType.Ios:
                    SetAsStaticImage("ios.png");
                    break;
                case eImageType.IosOutline:
                    SetAsStaticImage("iosOutline.png");
                    break;
                case eImageType.IosWhite:
                    SetAsStaticImage("iosWhite.png");
                    break;
                case eImageType.Android:
                    SetAsStaticImage("android.png");
                    break;
                case eImageType.AndroidOutline:
                    SetAsStaticImage("androidOutline.png");
                    break;
                case eImageType.AndroidWhite:
                    SetAsStaticImage("androidWhite.png");
                    break;
                #endregion

                #region Comparison Status Images
                case eImageType.Unchanged:
                    SetAsFontAwesomeIcon(IconChar.CheckCircle, toolTip: "Unchanged");
                    break;
                case eImageType.Changed:
                    SetAsFontAwesomeIcon(IconChar.ExclamationTriangle, toolTip: "Changed");
                    break;
                case eImageType.Deleted:
                    SetAsFontAwesomeIcon(IconChar.MinusCircle, toolTip: "Deleted");
                    break;
                case eImageType.Added:
                    SetAsFontAwesomeIcon(IconChar.PlusCircle, toolTip: "Added");
                    break;
                case eImageType.Avoided:
                    SetAsFontAwesomeIcon(IconChar.EyeSlash, toolTip: "Avoided");
                    break;
                #endregion

                default:
                    SetAsFontAwesomeIcon(IconChar.Question, Brushes.Red);
                    this.Background = Brushes.Yellow;
                    break;
            }
        }

        private void ResetImageView()
        {
            // Reset All do defaults
            xFAImage.Visibility = Visibility.Collapsed;
            //xFAImage.Spin = false;
            //xFAImage.StopSpin();
            //xFAImage.Rotation = 0;
            xFAFont.Visibility = Visibility.Collapsed;
            //xFAFont.Spin = false;
            //xFAFont.StopSpin();
            //xFAFont.Rotation = 0;
            xStaticImage.Visibility = Visibility.Collapsed;
            xViewBox.Visibility = Visibility.Collapsed;
            this.Background = null;
        }

        private void SetAsFontAwesomeIcon(IconChar fontAwesomeIcon, Brush foreground = null, double spinDuration = 0, string toolTip = null, bool blinkingIcon = false)
        {
            //set the icon
            xFAFont.Icon = fontAwesomeIcon;
            xFAImage.Icon = fontAwesomeIcon;
            if (SetAsFontImageWithSize > 0)
            {
                xFAFont.Visibility = Visibility.Visible;
                xFAFont.FontSize = SetAsFontImageWithSize;
            }
            else
            {
                xFAImage.Visibility = Visibility.Visible;
            }

            //set Foreground
            if (this.ImageForeground != null)
            {
                foreground = (SolidColorBrush)this.ImageForeground;
            }
            else if (foreground == null)
                foreground = (SolidColorBrush)FindResource("$BackgroundColor_DarkBlue");
            xFAImage.Foreground = foreground;
            if (this.ImageForeground != null)
                xFAFont.Foreground = foreground;

            if (spinDuration != 0)
            {
                //xFAImage.Spin = true;
                //xFAImage.SpinDuration = spinDuration;
                //xFAFont.Spin = true;
                //xFAFont.SpinDuration = spinDuration;
            }

            if(blinkingIcon)
            {
                DoubleAnimation blinkAnimation = new DoubleAnimation()
                {
                    From = 1,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(1)),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };

                xFAImage.BeginAnimation(OpacityProperty, blinkAnimation);
            }

            if (!string.IsNullOrEmpty(toolTip) && string.IsNullOrEmpty(ImageToolTip))
            {
                xFAImage.ToolTip = toolTip;
                xFAFont.ToolTip = toolTip;
            }

            if (SetBorder)
            {
                ImageMakerBorder.BorderThickness = new Thickness(1);
                ImageMakerBorder.BorderBrush = (SolidColorBrush)FindResource("$BackgroundColor_DarkBlue");
            }
            else
            {
                ImageMakerBorder.BorderThickness = new Thickness(0);
            }
        }

        private BitmapImage GetImageBitMap(string imageName)
        {
            return new BitmapImage(new Uri(@"/Images/" + imageName, UriKind.RelativeOrAbsolute));
        }

        public static ImageSource GetImageSource(eImageType imageType, SolidColorBrush foreground = null, double spinDuration = 0, string toolTip = null, double width = 0.0, bool SetBorder = false)
        {
            ImageMakerControl IM = new ImageMakerControl();
            IM.ImageType = imageType;
            IM.SetBorder = SetBorder;
            if (foreground != null || spinDuration > 0 || toolTip != null)//default design change is required
            {
                IM.SetAsFontAwesomeIcon(IM.xFAImage.Icon, foreground, spinDuration, toolTip);
            }
            IM.Width = width;

            if (IM.xFAImage.Visibility == Visibility.Visible)
            {
                return IM.xFAImage.Source;
            }
            else if (IM.xStaticImage.Visibility == Visibility.Visible)
            {
                return IM.xStaticImage.Source;
            }
            return null;
        }

        private void SetAsStaticImage(string imageName = "", BitmapImage imageBitMap = null)
        {
            xStaticImage.Visibility = Visibility.Visible;
            if (imageBitMap != null)
                xStaticImage.Source = imageBitMap;
            else
                xStaticImage.Source = GetImageBitMap(imageName);
        }

        Shape GetKidsDrawingShape()
        {
            Path path = new Path();

            path.Width = 500;
            path.Height = 500;

            path.StrokeThickness = 5;
            path.Stroke = Brushes.Purple;

            string PathData = "M 100,200 C 100,25 400,350 400,175 H 280";

            path.Data = Geometry.Parse(PathData);

            return path;
        }

        internal void StopImageSpin()
        {
            //Used for visual compare, we stop the spinner so image compare will be able to compare
            //xFAImage.Spin = false;
            //xFAFont.Spin = false;
        }
    }
}
