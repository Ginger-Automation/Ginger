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

using Amdocs.Ginger.Core;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FontAwesome5;
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
                case eImageType.VRT:
                    SetAsStaticImage("VRTLogo.png");
                    break;
                case eImageType.Applitools:
                    SetAsStaticImage("ApplitoolsLogo.png");
                    break;
                case eImageType.Sealights:
                    SetAsStaticImage("SealightsLogo.png");
                    break;
                case eImageType.SaveAll:
                    SetAsStaticImage("save-all-regular-light-grey.png");
                    break;
                case eImageType.SaveAllGradient:
                    SetAsStaticImage("save-all-regular-gradient-amdocs.png");
                    break;
                case eImageType.SaveLightGrey:
                    SetAsStaticImage("save-regular-light-grey.svg");
                    break;
                case eImageType.SaveGradient:
                    SetAsStaticImage("save-regular-amdocs-gradient.svg");
                    break;
                #endregion


                #region Repository Items Images
                //############################## Repository Items Images:
                case eImageType.Solution:
                case eImageType.ApplicationModel:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ThLarge);
                    break;
                case eImageType.BusinessFlow:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Sitemap);
                    break;
                case eImageType.ActivitiesGroup:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_ObjectGroup);
                    break;
                case eImageType.Activity:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Bars);
                    break;
                case eImageType.Action:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Bolt);
                    break;
                case eImageType.Agent:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_User);
                    break;
                case eImageType.RunSet:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_PlayCircle);
                    break;
                case eImageType.APIModel:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ExchangeAlt);
                    break;
                case eImageType.Runner:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_PlayCircle);
                    break;
                case eImageType.Operations:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Cogs);
                    break;
                case eImageType.Settings:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Cogs);//Gears
                    break;
                case eImageType.Environment:
                case eImageType.Globe:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Globe);
                    break;
                case eImageType.Application:
                case eImageType.ApplicationPOMModel:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_WindowMaximize);
                    break;
                case eImageType.HtmlReport:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Html5);
                    break;
                case eImageType.SharedRepositoryItem:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Star, Brushes.Orange);
                    break;
                case eImageType.NonSharedRepositoryItem:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Star, Brushes.Gray);
                    break;
                case eImageType.SharedRepositoryItemDark:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Star);
                    break;
                case eImageType.Tag:
                case eImageType.Ticket:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Tag);
                    break;
                case eImageType.DataSource:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Database);
                    break;
                case eImageType.PluginPackage:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Plug);
                    break;

                case eImageType.Building:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Building);
                    break;
                #endregion


                #region Variable Item Images
                //############################## Variables Images:
                case eImageType.Variable:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Code);
                    break;
                case eImageType.VariableList:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_List);
                    break;
                case eImageType.Password:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Key);
                    break;
                case eImageType.Random:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Random);
                    break;
                case eImageType.Sequence:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_AlignJustify);
                    break;
                case eImageType.Timer:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Clock);
                    break;
                #endregion


                #region Execution Status Images
                //############################## Execution Status Images:
                case eImageType.Passed:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CheckCircle, (SolidColorBrush)FindResource("$PassedStatusColor"), 0, "Passed");
                    break;
                case eImageType.Unknown:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_QuestionCircle, null, 0, "Unknown");
                    break;
                case eImageType.Failed:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_TimesCircle, (SolidColorBrush)FindResource("$FailedStatusColor"), 0, "Failed");
                    break;
                case eImageType.Pending:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Clock, (SolidColorBrush)FindResource("$PendingStatusColor"), 0, "Pending");
                    break;
                case eImageType.Recording:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Camera, new SolidColorBrush(Color.FromRgb(255, 0, 0)), 0, "Recording...", true);
                    break;
                case eImageType.Processing:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Spinner, (LinearGradientBrush)FindResource("$amdocsLogoLinarGradientBrush_NewAmdocsColors"), 2);
                    break;
                case eImageType.Ready:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_ThumbsUp, (SolidColorBrush)FindResource("$PendingStatusColor"));
                    break;
                case eImageType.Stopped:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_StopCircle, (SolidColorBrush)FindResource("$StoppedStatusColor"), 0, "Stopped");
                    break;
                case eImageType.Blocked:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Ban, (SolidColorBrush)FindResource("$BlockedStatusColor"), 0, "Blocked");
                    break;
                case eImageType.Skipped:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_MinusCircle, (SolidColorBrush)FindResource("$SkippedStatusColor"), 0, "Skipped");
                    break;
                case eImageType.Running:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Spinner, (SolidColorBrush)FindResource("$RunningStatusColor"), 2, "Running");
                    break;
                case eImageType.Mapped:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CheckCircle);
                    break;
                case eImageType.Partial:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ExclamationTriangle);
                    break;
                case eImageType.UnMapped:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_TimesCircle);
                    break;
                #endregion


                #region Operations Images
                //############################## Operations Images:
                case eImageType.Add:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Plus);
                    break;
                case eImageType.Refresh:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_SyncAlt);
                    break;
                case eImageType.Config:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Cog);
                    break;
                case eImageType.Edit:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_PencilAlt);
                    break;
                case eImageType.Save:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Save);
                    break;
                case eImageType.Reply:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Reply);
                    break;

                case eImageType.Run:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Play);
                    break;
                case eImageType.RunSingle:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_PlayCircle);
                    break;
                case eImageType.RunAll:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_PlayCircle);
                    break;
                case eImageType.Stop:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Stop);
                    break;
                case eImageType.StopAll:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_StopCircle);
                    break;
                case eImageType.Close:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_WindowClose);
                    break;
                case eImageType.Continue:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_FastForward);
                    break;
                case eImageType.Open:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_FolderOpen);
                    break;
                case eImageType.New:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Magic);
                    break;
                case eImageType.Analyze:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Stethoscope);
                    break;
                case eImageType.GoBack:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowLeft);
                    break;
                case eImageType.GoNext:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowRight);
                    break;
                case eImageType.Finish:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_FlagCheckered);
                    break;
                case eImageType.Cancel:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_WindowClose);
                    break;
                case eImageType.Reset:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_RedoAlt);
                    break;
                case eImageType.Undo:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_UndoAlt);
                    break;
                case eImageType.Simulate:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Android);
                    break;
                case eImageType.Copy:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Copy);
                    break;
                case eImageType.Cut:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Cut);
                    break;
                case eImageType.Paste:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Paste);
                    break;
                case eImageType.Delete:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_TrashAlt);
                    break;
                case eImageType.DeleteSingle:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Minus);
                    break;
                case eImageType.Minimize:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_WindowMinimize);
                    break;
                case eImageType.MoveRight:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowRight);
                    break;
                case eImageType.MoveLeft:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowLeft);
                    break;
                case eImageType.MoveUp:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowUp);
                    break;
                case eImageType.MoveDown:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowDown);
                    break;
                case eImageType.MoveUpDown:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowsAltV);
                    break;
                case eImageType.Reorder:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_FirstOrder);
                    break;
                case eImageType.Retweet:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Retweet);
                    break;
                case eImageType.Automate:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Cogs);
                    break;
                case eImageType.Convert:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ExchangeAlt);
                    break;
                case eImageType.ParallelExecution:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Random);
                    break;
                case eImageType.SequentialExecution:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_SortNumericDown);
                    break;
                case eImageType.Search:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Search);
                    break;
                case eImageType.Duplicate:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_FileAlt);
                    break;
                case eImageType.Merge:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_ObjectGroup);
                    break;
                case eImageType.Sync:
                case eImageType.InstanceLink:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Link);
                    break;
                case eImageType.InstanceLinkOrange:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Link, Brushes.Orange);
                    break;
                case eImageType.UnSync:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Unlink);
                    break;
                case eImageType.Visible:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Eye);
                    break;
                case eImageType.Invisible:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_EyeSlash);
                    break;
                case eImageType.View:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Eye);
                    break;
                case eImageType.Download:
                case eImageType.GetLatest:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_CloudDownloadAlt);
                    break;
                case eImageType.CheckIn:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_CloudUploadAlt);
                    break;
                case eImageType.Upload:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Upload);
                    break;
                case eImageType.Expand:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ChevronDown);
                    break;
                case eImageType.Collapse:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ChevronUp);
                    break;
                case eImageType.ExpandAll:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_AngleDoubleDown);
                    break;
                case eImageType.CollapseAll:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_AngleDoubleUp);
                    break;
                case eImageType.ActiveAll:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Check);
                    break;
                case eImageType.ExpandToFullScreen:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Expand);
                    break;
                case eImageType.Export:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ShareAlt);
                    break;
                case eImageType.ImportFile:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Download);
                    break;
                case eImageType.Times:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Times);
                    break;
                case eImageType.Times_Red:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Times, (SolidColorBrush)FindResource("$HighlightColor_Red"), 0, "ToolTip");
                    break;
                case eImageType.Exchange:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ExchangeAlt);
                    break;
                case eImageType.Share:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ShareAlt);
                    break;
                case eImageType.ShareExternal:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ShareAltSquare);
                    break;
                case eImageType.Filter:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Filter);
                    break;
                case eImageType.Upgrade:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowCircleUp);
                    break;
                case eImageType.Recover:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Recycle);
                    break;
                case eImageType.Approve:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_ThumbsUp);
                    break;
                case eImageType.Reject:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_ThumbsDown);
                    break;
                case eImageType.Retry:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Retweet);
                    break;
                case eImageType.Warn:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ExclamationTriangle);
                    break;
                case eImageType.HighWarn:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ExclamationTriangle, Brushes.Red);
                    break;
                case eImageType.MediumWarn:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ExclamationTriangle, Brushes.Orange);
                    break;
                case eImageType.LowWarn:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ExclamationTriangle, Brushes.Yellow);
                    break;
                case eImageType.EditWindow:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Edit);
                    break;
                case eImageType.CLI:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ExternalLinkAlt);
                    break;
                case eImageType.WindowRestore:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_WindowRestore);
                    break;
                case eImageType.SelfHealing:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Magic);
                    break;
                #endregion


                #region Items Images
                //############################## Items Images:
                case eImageType.KidsDrawing:
                    xViewBox.Visibility = Visibility.Visible;
                    xViewBox.Child = GetKidsDrawingShape();
                    break;
                case eImageType.FlowDiagram:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Sitemap);
                    break;
                case eImageType.DataTable:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Table);
                    break;
                case eImageType.Parameter:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_SlidersH);
                    break;
                case eImageType.File:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_FileAlt);
                    break;
                case eImageType.Folder:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Folder);
                    break;
                case eImageType.OpenFolder:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_FolderOpen);
                    break;
                case eImageType.EllipsisH:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_EllipsisH);
                    break;
                case eImageType.ListGroup:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ListUl);
                    break;
                case eImageType.Sitemap:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Sitemap);
                    break;
                case eImageType.ItemModified:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Asterisk, Brushes.DarkOrange, 5, "Item was modified");
                    break;
                case eImageType.Clock:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Clock);
                    break;
                case eImageType.Report:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ChartPie);
                    break;
                case eImageType.Active:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ToggleOn);
                    break;
                case eImageType.InActive:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ToggleOff);
                    break;
                case eImageType.History:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_History);
                    break;
                case eImageType.ChevronDown:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ChevronCircleDown);
                    break;
                case eImageType.Question:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_QuestionCircle);
                    break;
                case eImageType.Help:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_LifeRing);
                    break;
                case eImageType.Screen:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Desktop);
                    break;
                case eImageType.Info:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_InfoCircle);
                    break;
                case eImageType.Service:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Headphones);
                    break;
                case eImageType.FileVideo:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_FileVideo);
                    break;
                case eImageType.Email:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Envelope);
                    break;
                case eImageType.SourceControl:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_CodeBranch);
                    break;
                case eImageType.SourceControlNew:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Plus, Brushes.Green, toolTip: "New");
                    break;
                case eImageType.SourceControlModified:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_PencilAlt, Brushes.OrangeRed, toolTip: "Modified");
                    break;
                case eImageType.SourceControlDeleted:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Minus, Brushes.Red, toolTip: "Deleted");
                    break;
                case eImageType.SourceControlEquel:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Check, Brushes.Gray, toolTip: "Same as Source");
                    break;
                case eImageType.SourceControlLockedByAnotherUser:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Lock, Brushes.Purple, toolTip: "Locked by Other User");
                    break;
                case eImageType.SourceControlLockedByMe:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Lock, Brushes.SaddleBrown, toolTip: "Locked by You");
                    break;
                case eImageType.SourceControlError:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ExclamationCircle, Brushes.Red, toolTip: "Error in checking status");
                    break;
                case eImageType.Check:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CheckCircle);
                    break;
                case eImageType.Bug:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Bug);
                    break;
                case eImageType.Power:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_PowerOff);
                    break;
                case eImageType.Pointer:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_HandPointer);
                    break;
                case eImageType.Camera:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Camera);
                    break;
                case eImageType.ExcelFile:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_FileExcel);
                    break;
                case eImageType.PlusSquare:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_PlusSquare);
                    break;
                case eImageType.Wrench:
                case eImageType.Fix:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Wrench);
                    break;

                case eImageType.Eraser:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Eraser);
                    break;

                case eImageType.ArrowDown:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_AngleDown);
                    break;
                case eImageType.ArrowRight:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowRight);
                    break;
                case eImageType.User:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_UserCircle);
                    break;
                case eImageType.UserProfile:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_UserMd);
                    break;
                case eImageType.Forum:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Comment);
                    break;
                case eImageType.Website:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Laptop);
                    break;
                case eImageType.Beta:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Android, Brushes.Orange);
                    break;
                case eImageType.Error:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_TimesCircle);
                    break;
                case eImageType.Coffee:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_MugHot, Brushes.Red);
                    break;
                case eImageType.MapSigns:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_MapSigns);
                    break;
                case eImageType.Elements:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Th);
                    break;
                case eImageType.LocationPointer:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_LocationArrow);
                    break;
                case eImageType.GitHub:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Github);
                    break;
                case eImageType.Ping:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ExchangeAlt);
                    break;
                case eImageType.Database:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Database);
                    break;
                case eImageType.Output:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Upload);
                    break;
                case eImageType.Input:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Download);
                    break;
                case eImageType.Spy:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_UserSecret);
                    break;

                case eImageType.CodeFile:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_FileCode);
                    break;

                case eImageType.Rows:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowsAltH);
                    break;

                case eImageType.Column:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowsAltV);
                    break;

                case eImageType.Columns:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Columns);
                    break;

                case eImageType.Browser:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Chrome);
                    break;
                case eImageType.Java:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Java);
                    break;
                case eImageType.KeyboardLayout:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Keyboard);
                    break;

                case eImageType.Linux:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Linux);
                    break;

                case eImageType.BatteryThreeQuarter:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_BatteryThreeQuarters);
                    break;

                case eImageType.Mobile:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_MobileAlt);
                    break;

                case eImageType.Codepen:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Codepen);
                    break;

                case eImageType.Code:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Code);
                    break;

                case eImageType.Runing:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Running);
                    break;

                case eImageType.Dos:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_SquareFull);
                    break;

                case eImageType.Server:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Server);
                    break;

                case eImageType.MousePointer:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_MousePointer);
                    break;

                case eImageType.AudioFileOutline:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_FileAudio);
                    break;

                case eImageType.ChartLine:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ChartLine);
                    break;

                case eImageType.Suitcase:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Suitcase);
                    break;

                case eImageType.Paragraph:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Paragraph);
                    break;

                case eImageType.Graph:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ChartLine);
                    break;

                case eImageType.BullsEye:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Bullseye);
                    break;

                case eImageType.WindowsIcon:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Windows);
                    break;

                case eImageType.PDFFile:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_FilePdf);
                    break;

                case eImageType.CSS3Text:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Css3);
                    break;

                case eImageType.Languages:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Language);
                    break;

                case eImageType.MinusSquare:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_MinusSquare);
                    break;
                case eImageType.Mandatory:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ExclamationCircle);
                    break;
                case eImageType.ALM:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Qrcode);//need to find better image type
                    break;
                case eImageType.MapALM:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_MapSigns);
                    break;
                case eImageType.CSV:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_FileCsv);
                    break;
                case eImageType.Clipboard:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Clipboard);
                    break;
                case eImageType.ID:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_IdCardAlt);
                    break;
                case eImageType.RegularExpression:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Registered);
                    break;
                case eImageType.DataManipulation:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Th);
                    break;
                case eImageType.General:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Gg);
                    break;
                case eImageType.SignIn:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_SignInAlt);
                    break;
                case eImageType.SignOut:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_SignOutAlt);
                    break;
                case eImageType.AngleArrowUp:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_AngleUp);
                    break;
                case eImageType.AngleArrowDown:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_AngleDown);
                    break;
                case eImageType.AngleArrowLeft:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_AngleLeft);
                    break;
                case eImageType.AngleArrowRight:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_AngleRight);
                    break;
                case eImageType.Support:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Headphones);
                    break;
                case eImageType.Asterisk:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Asterisk);
                    break;
                #endregion

                #region ElementType Images
                case eImageType.Button:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_HandPointer);
                    break;
                case eImageType.TextBox:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_PenSquare);
                    break;
                case eImageType.Image:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Image);
                    break;
                case eImageType.CheckBox:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CheckSquare);
                    break;
                case eImageType.RadioButton:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_DotCircle);
                    break;
                case eImageType.Link:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ExternalLinkAlt);
                    break;
                case eImageType.Element:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Square);
                    break;
                case eImageType.Menu:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Bars);
                    break;
                case eImageType.Label:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Font);
                    break;
                case eImageType.DropList:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_ListAlt);
                    break;
                case eImageType.List:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ListOl);
                    break;
                case eImageType.Window:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_WindowMaximize);
                    break;
                case eImageType.ToggleOn:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ToggleOn);
                    break;
                case eImageType.ToggleOff:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ToggleOff);
                    break;
                case eImageType.Table:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Table);
                    break;
                case eImageType.Text:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_TextWidth);
                    break;
                case eImageType.LinkSquare:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ExternalLinkSquareAlt);
                    break;
                case eImageType.DatePicker:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Calendar);
                    break;
                case eImageType.TreeView:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Sitemap);
                    break;
                case eImageType.Pin:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Thumbtack);
                    break;
                case eImageType.Square:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Square);
                    break;
                case eImageType.Triangle:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Play);
                    break;
                case eImageType.Circle:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Circle);
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CheckCircle, toolTip: "Unchanged");
                    break;
                case eImageType.Changed:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ExclamationTriangle, toolTip: "Changed");
                    break;
                case eImageType.Deleted:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_MinusCircle, toolTip: "Deleted");
                    break;
                case eImageType.Added:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_PlusCircle, toolTip: "Added");
                    break;
                case eImageType.Avoided:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_EyeSlash, toolTip: "Avoided");
                    break;
                #endregion

                default:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Question, Brushes.Red);
                    this.Background = Brushes.Yellow;
                    break;
            }
        }

        private void ResetImageView()
        {
            // Reset All do defaults
            xFAImage.Visibility = Visibility.Collapsed;
            xFAImage.Spin = false;
            xFAImage.Rotation = 0;
            xFAFont.Visibility = Visibility.Collapsed;
            xFAFont.Spin = false;
            xFAFont.Rotation = 0;
            xStaticImage.Visibility = Visibility.Collapsed;
            xViewBox.Visibility = Visibility.Collapsed;
            this.Background = null;
        }

        private void SetAsFontAwesomeIcon(EFontAwesomeIcon fontAwesomeIcon, Brush foreground = null, double spinDuration = 0, string toolTip = null, bool blinkingIcon = false)
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
            {
                foreground = (SolidColorBrush)FindResource("$BackgroundColor_DarkBlue");
            }
            xFAImage.Foreground = foreground;
            if (this.ImageForeground != null)
            {
                xFAFont.Foreground = foreground;
            }

            if (spinDuration != 0)
            {
                xFAImage.Spin = true;
                xFAImage.SpinDuration = spinDuration;
                xFAFont.Spin = true;
                xFAFont.SpinDuration = spinDuration;
            }

            if (blinkingIcon)
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
            return new BitmapImage(new Uri("pack://application:,,,/Ginger;component/UserControlsLib/ImageMakerLib/Images/" + imageName, UriKind.RelativeOrAbsolute));
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
            {
                xStaticImage.Source = imageBitMap;
            }
            else
            {
                xStaticImage.Source = GetImageBitMap(imageName);
            }
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
            xFAImage.Spin = false;
            xFAFont.Spin = false;
        }
    }
}
