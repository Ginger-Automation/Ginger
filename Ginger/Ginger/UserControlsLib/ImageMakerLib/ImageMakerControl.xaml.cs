#region License
/*
Copyright © 2014-2025 European Support Limited

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
using FontAwesome6;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
                case eImageType.Ginger: //new
                    SetAsStaticImage("Ginger.png");
                    break;
                case eImageType.GingerIconWhite: //new
                    SetAsStaticImage("GingerIconWhite.png");
                    break;
                case eImageType.GingerLogo: //new
                    SetAsStaticImage("GingerByAmdocsLogo.png");
                    break;
                case eImageType.GingerLogoGray: //new
                    SetAsStaticImage("GingerByAmdocsLogoGray.png");
                    break;
                case eImageType.GingerLogoWhite: //new
                    SetAsStaticImage("GingerByAmdocsLogoWhiteSmall.png");
                    break;
                case eImageType.GingerSplash://new
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
                case eImageType.Environment:
                    //SetAsStaticImage("Environment.ico");
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_LayerGroup);
                    break;
                case eImageType.Chatbot:
                    SetAsStaticImage("bot.png");
                    break;
                case eImageType.GingerAnalytics:
                    SetAsStaticImage("GingerAnalytics.png");
                    break;
                case eImageType.WireMockLogo:
                    SetAsStaticImage("WireMockLogo.png");
                    break;
                case eImageType.WireMock_Logo:
                    SetAsStaticImage("WireMock_Logo.png");
                    break;
                case eImageType.WireMockLogo16x16:
                    SetAsStaticImage("WireMockLogo16x16.png");
                    break;
                case eImageType.SendArrow:
                    SetAsStaticImage("sendArrow.png");
                    break;
                case eImageType.Medical:
                    SetAsStaticImage("medical.png");
                    break;
                case eImageType.Chat:
                    SetAsStaticImage("chat.png");
                    break;
                #endregion
                #region Repository Items Images
                //############################## Repository Items Images:
                case eImageType.Solution:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Flask);
                    break;
                case eImageType.ApplicationModel:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_TableCellsLarge);
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
                case eImageType.AIActivity:
                    SetAsStaticImage("AIBrain.png");
                    break;
                case eImageType.Action:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Bolt);
                    break;
                case eImageType.Agent:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_User);
                    break;
                case eImageType.RunSet:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CirclePlay);
                    break;
                case eImageType.APIModel:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowRightArrowLeft);
                    break;
                case eImageType.Runner:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CirclePlay);
                    break;
                case eImageType.Operations:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Gears);
                    break;
                case eImageType.Settings:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Gears);//Gears
                    break;
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Building);
                    break;

                case eImageType.Accessibility:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_UniversalAccess);
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Shuffle);
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CircleCheck, (SolidColorBrush)FindResource("$PassedStatusColor"), 0, "Passed");
                    break;
                case eImageType.Unknown:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CircleQuestion, null, 0, "Unknown");
                    break;
                case eImageType.Failed:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CircleXmark, (SolidColorBrush)FindResource("$FailedStatusColor"), 0, "Failed");
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
                case eImageType.LisaProcessing:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Spinner, Brushes.MediumPurple, 2);
                    break;
                case eImageType.Ready:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_ThumbsUp, (SolidColorBrush)FindResource("$PendingStatusColor"));
                    break;
                case eImageType.Stopped:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CircleStop, (SolidColorBrush)FindResource("$StoppedStatusColor"), 0, "Stopped");
                    break;
                case eImageType.Blocked:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Ban, (SolidColorBrush)FindResource("$BlockedStatusColor"), 0, "Blocked");
                    break;
                case eImageType.Skipped:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_CircleMinus, (SolidColorBrush)FindResource("$SkippedStatusColor"), 0, "Skipped");
                    break;
                case eImageType.Running:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Spinner, (SolidColorBrush)FindResource("$RunningStatusColor"), 2, "Running");
                    break;
                case eImageType.Mapped:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CircleCheck);
                    break;
                case eImageType.Partial:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_TriangleExclamation);
                    break;
                case eImageType.UnMapped:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CircleStop);
                    break;
                #endregion


                #region Operations Images
                //############################## Operations Images:
                case eImageType.Add:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Plus);
                    break;
                case eImageType.Refresh:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowsRotate);
                    break;
                case eImageType.Config:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Gear);
                    break;
                case eImageType.Edit:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Pencil);
                    break;
                case eImageType.Save:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_FloppyDisk);
                    break;
                case eImageType.Reply:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Reply);
                    break;

                case eImageType.Run:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Play);
                    break;
                case eImageType.RunSingle:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_CirclePlay);
                    break;
                case eImageType.RunAll:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CirclePlay);
                    break;
                case eImageType.Stop:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Stop);
                    break;
                case eImageType.StopAll:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CircleStop);
                    break;
                case eImageType.Close:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Xmark);
                    break;
                case eImageType.Close2:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Xmark);
                    break;
                case eImageType.CloseWhite:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Xmark, Brushes.White);
                    break;
                case eImageType.Continue:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ForwardFast);
                    break;
                case eImageType.Open:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_FolderOpen);
                    break;
                case eImageType.New:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_WandMagicSparkles);
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
                case eImageType.Home:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_House);
                    break;
                case eImageType.Cancel:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Xmark);
                    break;
                case eImageType.Reset:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Retweet);
                    break;
                case eImageType.Undo:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_RotateLeft);
                    break;
                case eImageType.Simulate:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Android);
                    break;
                case eImageType.Copy:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Copy);
                    break;
                case eImageType.Cut:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Scissors);
                    break;
                case eImageType.Paste:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Paste);
                    break;
                case eImageType.Delete:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_TrashCan);
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowsUpDown);
                    break;
                case eImageType.Category:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_StackExchange);
                    break;
                case eImageType.Reorder:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_FirstOrder);
                    break;
                case eImageType.Retweet:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Retweet);
                    break;
                case eImageType.Automate:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Gears);
                    break;
                case eImageType.Convert:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowRightArrowLeft);
                    break;
                case eImageType.ParallelExecution:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Shuffle);
                    break;
                case eImageType.SequentialExecution:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowDown19);
                    break;
                case eImageType.Search:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_MagnifyingGlass);
                    break;
                case eImageType.Duplicate:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_File);
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_LinkSlash);
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_CloudArrowDown);
                    break;
                case eImageType.CheckIn:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_CloudArrowUp);
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_AnglesDown);
                    break;
                case eImageType.CollapseAll:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_AnglesUp);
                    break;
                case eImageType.ActiveAll:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Check);
                    break;
                case eImageType.ExpandToFullScreen:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Expand);
                    break;
                case eImageType.Export:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Share);
                    break;
                case eImageType.ImportFile:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Download);
                    break;
                case eImageType.Times:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Xmark);
                    break;
                case eImageType.Times_Red:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Xmark, (SolidColorBrush)FindResource("$HighlightColor_Red"), 0, "ToolTip");
                    break;
                case eImageType.Exchange:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowRightArrowLeft);
                    break;
                case eImageType.Share:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Share);
                    break;
                case eImageType.ShareExternal:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ShareFromSquare);
                    break;
                case eImageType.Filter:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Filter);
                    break;
                case eImageType.Upgrade:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_CircleUp);
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_TriangleExclamation);
                    break;
                case eImageType.HighWarn:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_TriangleExclamation, Brushes.Red);
                    break;
                case eImageType.MediumWarn:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_TriangleExclamation, Brushes.Orange);
                    break;
                case eImageType.LowWarn:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_TriangleExclamation, Brushes.Yellow);
                    break;
                case eImageType.EditWindow:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_PenToSquare);
                    break;
                case eImageType.CLI:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowUpRightFromSquare);
                    break;
                case eImageType.WindowRestore:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_WindowRestore);
                    break;
                case eImageType.SelfHealing:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_WandMagicSparkles);
                    break;
                case eImageType.Rules:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Gavel);
                    break;
                case eImageType.AdminUser:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_UserGear, Brushes.OrangeRed);
                    break;
                case eImageType.NormalUser:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_UserGear);
                    break;
                case eImageType.AnglesArrowLeft:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_AnglesLeft);
                    break;
                case eImageType.AnglesArrowRight:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_AnglesRight);
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
                case eImageType.Robot:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Robot);
                    break;
                case eImageType.Parameter:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Sliders);
                    break;
                case eImageType.File:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_File);
                    break;
                case eImageType.Folder:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Folder);
                    break;
                case eImageType.OpenFolder:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_FolderOpen);
                    break;
                case eImageType.EllipsisH:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Ellipsis);
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ClockRotateLeft);
                    break;
                case eImageType.ChevronDown:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_CircleChevronDown);
                    break;
                case eImageType.Question:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CircleQuestion);
                    break;
                case eImageType.Help:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_LifeRing);
                    break;
                case eImageType.Screen:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Desktop);
                    break;
                case eImageType.Info:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_CircleInfo);
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Pencil, Brushes.OrangeRed, toolTip: "Modified");
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_CircleExclamation, Brushes.Red, toolTip: "Error in checking status");
                    break;
                case eImageType.Check:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CircleCheck);
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
                case eImageType.WordFile:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_FileWord);
                    break;
                case eImageType.FileXML:
                    SetAsStaticImage("xml.png", null, 50);
                    break;
                case eImageType.FileJSON:
                    SetAsStaticImage("json.png", null, 50);
                    break;
                case eImageType.FileJavascript:
                    SetAsStaticImage("javascript.png", null, 50);
                    break;
                case eImageType.FilePowerpoint:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_FilePowerpoint);
                    break;
                case eImageType.FileArchive:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_FileZipper);
                    break;
                case eImageType.PlusSquare:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_SquarePlus);
                    break;
                case eImageType.Wrench:
                case eImageType.Fix:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Wrench);
                    break;

                case eImageType.Eraser:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Eraser);
                    break;
                case eImageType.Broom:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Broom);
                    break;

                case eImageType.ArrowDown:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_AngleDown);
                    break;
                case eImageType.ArrowRight:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowRight);
                    break;
                case eImageType.User:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CircleUser);
                    break;
                case eImageType.UserProfile:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_UserDoctor);
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CircleXmark);
                    break;
                case eImageType.Coffee:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_MugHot, Brushes.Red);
                    break;
                case eImageType.MapSigns:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_SignsPost);
                    break;
                case eImageType.Elements:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_TableCells);
                    break;
                case eImageType.LocationPointer:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_LocationArrow);
                    break;
                case eImageType.GitHub:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Github);
                    break;
                case eImageType.Ping:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Shuffle);
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowsLeftRight);
                    break;

                case eImageType.Column:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_TableColumns);
                    break;

                case eImageType.Columns:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_TableColumns);
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

                case eImageType.Smile:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_FaceSmileWink);
                    break;

                case eImageType.Linux:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Linux);
                    break;

                case eImageType.BatteryThreeQuarter:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_BatteryThreeQuarters);
                    break;

                case eImageType.Mobile:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_MobileScreenButton);
                    break;

                case eImageType.Codepen:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Codepen);
                    break;

                case eImageType.Code:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Code);
                    break;

                case eImageType.Runing:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_PersonRunning);
                    break;

                case eImageType.Dos:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_SquareFull);
                    break;

                case eImageType.Server:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Server);
                    break;

                case eImageType.MousePointer:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowPointer);
                    break;

                case eImageType.Target:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowPointer);
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_SquareMinus);
                    break;
                case eImageType.Mandatory:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_CircleExclamation);
                    break;
                case eImageType.ALM:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Qrcode);//need to find better image type
                    break;
                case eImageType.MapALM:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_SignsPost);
                    break;
                case eImageType.CSV:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_FileCsv);
                    break;
                case eImageType.Clipboard:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Clipboard);
                    break;
                case eImageType.ID:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_IdCard);
                    break;
                case eImageType.RegularExpression:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Registered);
                    break;
                case eImageType.DataManipulation:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_TableCells);
                    break;
                case eImageType.General:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Brands_Gg);
                    break;
                case eImageType.SignIn:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_RightToBracket);
                    break;
                case eImageType.SignOut:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_RightFromBracket);
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
                case eImageType.MoneyCheckDollar:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_MoneyCheckDollar);
                    break;
                case eImageType.AddressCard:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_AddressCard);
                    break;
                case eImageType.IdCard:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_IdCard);
                    break;
                case eImageType.IdBadge:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_IdBadge);
                    break;
                case eImageType.Phone:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Phone);
                    break;
                case eImageType.Microchip:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Microchip);
                    break;
                #endregion

                #region ElementType Images
                case eImageType.Button:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_HandPointer);
                    break;
                case eImageType.TextBox:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_PenToSquare);
                    break;
                case eImageType.Image:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_Image);
                    break;
                case eImageType.CheckBox:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_SquareCheck);
                    break;
                case eImageType.RadioButton:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CircleDot);
                    break;
                case eImageType.Link:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowUpRightFromSquare);
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_List);
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
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_ArrowUpRightFromSquare);
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
                case eImageType.Katalon:
                    SetAsStaticImage("katalon.png");
                    break;
                #endregion

                #region Comparison Status Images
                case eImageType.Unchanged:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_CircleCheck, toolTip: "Unchanged");
                    break;
                case eImageType.Changed:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_TriangleExclamation, toolTip: "Changed");
                    break;
                case eImageType.Deleted:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_CircleMinus, toolTip: "Deleted");
                    break;
                case eImageType.Added:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_CirclePlus, toolTip: "Added");
                    break;
                case eImageType.Avoided:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Regular_EyeSlash, toolTip: "Avoided");
                    break;
                #endregion

                case eImageType.VerticalBars:
                    SetAsFontAwesomeIcon(EFontAwesomeIcon.Solid_Bars, rotation: 90);
                    break;

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

        private void SetAsFontAwesomeIcon(EFontAwesomeIcon fontAwesomeIcon, Brush foreground = null, double spinDuration = 0, string toolTip = null, bool blinkingIcon = false, double rotation = 0)
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
                foreground = ImageForeground;
            }
            else if (foreground == null)
            {
                foreground = (SolidColorBrush)FindResource("$BackgroundColor_Black");
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

            xFAImage.Rotation = rotation;

            if (!string.IsNullOrEmpty(toolTip) && string.IsNullOrEmpty(ImageToolTip))
            {
                xFAImage.ToolTip = toolTip;
                xFAFont.ToolTip = toolTip;
            }

            if (SetBorder)
            {
                ImageMakerBorder.BorderThickness = new Thickness(1);
                ImageMakerBorder.BorderBrush = (SolidColorBrush)FindResource("$BackgroundColor_Black");
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
            ImageMakerControl IM = new ImageMakerControl
            {
                ImageType = imageType,
                SetBorder = SetBorder
            };
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

        private void SetAsStaticImage(string imageName = "", BitmapImage imageBitMap = null, double Width = 0, double Height = 0)
        {
            xStaticImage.Visibility = Visibility.Visible;
            if (Width > 0)
            {
                xStaticImage.Width = Width;
            }
            if (Height > 0)
            {
                xStaticImage.Height = Height;
            }
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
            Path path = new Path
            {
                Width = 500,
                Height = 500,

                StrokeThickness = 5,
                Stroke = Brushes.Purple
            };

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
