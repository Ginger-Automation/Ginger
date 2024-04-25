using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.EnumsLib;
using Amdocs.Ginger.UserControls;
using Ginger.BusinessFlowWindows;
using Ginger.UserControlsLib.TextEditor;
using GingerCore;
using GingerWPF.UserControlsLib.UCTreeView;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Ginger.Actions.UserControls
{
    /// <summary>
    /// Interaction logic for UCArtifact.xaml
    /// </summary>
    public partial class UCArtifact : UserControl
    {
        public eImageType ArtifactImage { get; set; }

        public string ArtifactName { get; set; }

        public string ArtifactPath { get; set; }
       
        public UCArtifact()
        {
            InitializeComponent();
            this.PreviewMouseRightButtonDown -=UCArtifact_PreviewMouseRightButtonDown;
            this.PreviewMouseRightButtonDown +=UCArtifact_PreviewMouseRightButtonDown;
        }

        private void UCArtifact_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {           
            if (this == null)
            {
                e.Handled = true;
            }
            else
            {
                this.Focus();               
                this.ContextMenu = new ContextMenu();
                AddMenuItem(this.ContextMenu, "View Content", ViewContent, null, eImageType.View);
                AddMenuItem(this.ContextMenu, "Open by System", OpenbySystem, null, eImageType.Open);
                AddMenuItem(this.ContextMenu, "Open File Location", OpenFileLocation, null, eImageType.OpenFolder);               
            }
        }

        private void OpenbySystem(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenBySystem();
        }

        private void OpenFileLocation(object sender, System.Windows.RoutedEventArgs e)
        {           
            string lastFolderName = System.IO.Path.GetDirectoryName(ArtifactPath);
            if (string.IsNullOrEmpty(lastFolderName))
            {
                return;
            }

            if (!Directory.Exists(lastFolderName))
            {
                Directory.CreateDirectory(lastFolderName);
            }
            Process.Start(new ProcessStartInfo() { FileName = lastFolderName, UseShellExecute = true });
        }
       
        private void ViewContent(object sender, System.Windows.RoutedEventArgs e)
        {
            
            string tempFilePath = GingerCoreNET.GeneralLib.General.CreateTempTextFile(ArtifactPath);
            if (System.IO.File.Exists(tempFilePath))
            {
                DocumentEditorPage docPage = new DocumentEditorPage(ArtifactPath, enableEdit: false, UCTextEditorTitle: string.Empty);
                docPage.Width = 800;
                docPage.Height = 800;
                docPage.ShowAsWindow(ArtifactName);                
                return;
            }          
            Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to load raw response view, see log for details.");
        }

        public void OpenBySystem()
        {
            new Process
            {
                StartInfo = new ProcessStartInfo(ArtifactPath)
                {
                    UseShellExecute = true
                }
            }.Start();
        }
        private static MenuItem CreateMenuItem(string Header, RoutedEventHandler RoutedEventHandler, object CommandParameter = null, eImageType imageType = eImageType.Null)
        {
            MenuItem mnuItem = new MenuItem();
            if (imageType != eImageType.Null)
            {
                ImageMakerControl actionIcon = new ImageMakerControl();
                actionIcon.ImageType = imageType;
                actionIcon.Height = 16;
                actionIcon.Width = 16;
                mnuItem.Icon = actionIcon;
            }
            mnuItem.Header = Header;
            mnuItem.Click += RoutedEventHandler;
            mnuItem.CommandParameter = CommandParameter;
            return mnuItem;

        }

        public static void AddMenuItem(ContextMenu menu, string Header, RoutedEventHandler RoutedEventHandler, object CommandParameter = null, eImageType imageType = eImageType.Null)
        {
            MenuItem mnuItem = CreateMenuItem(Header, RoutedEventHandler, CommandParameter, imageType);
            menu.Items.Add(mnuItem);
        }

        public void IntiArtifact()
        {
            string FileExtention = string.Empty;            
            FileExtention = System.IO.Path.GetExtension(ArtifactPath).ToLower().Remove(0,1);
            eFileTypes fileType;
            Enum.TryParse(FileExtention, out fileType);

            switch(fileType)
            {
                case eFileTypes.xls:
                case eFileTypes.xlsx :
                case eFileTypes.csv:
                    ArtifactImage = eImageType.ExcelFile;
                    Foreground = new SolidColorBrush(Colors.Green);
                    break;
                case eFileTypes.ppt:
                    ArtifactImage= eImageType.FilePowerpoint;
                    Foreground = new SolidColorBrush(Colors.Orange);
                    break;
                case eFileTypes.docx:
                case eFileTypes.doc:
                    ArtifactImage = eImageType.WordFile;
                    Foreground = new SolidColorBrush(Colors.Blue);
                    break;
                case eFileTypes.bmp:
                case eFileTypes.gif:
                case eFileTypes.jpeg:
                case eFileTypes.jpg:
                case eFileTypes.png:
                    ArtifactImage = eImageType.Image;
                    Foreground = new SolidColorBrush(Colors.Green);
                    break;
                case eFileTypes.htm:
                case eFileTypes.html:
                    ArtifactImage = eImageType.HtmlReport;
                    Foreground = new SolidColorBrush(Colors.Orange);
                    break;
                case eFileTypes.pdf:
                    ArtifactImage = eImageType.PDFFile;
                    Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case eFileTypes.txt:
                    ArtifactImage = eImageType.Text;
                    Foreground = new SolidColorBrush(Colors.Gray);
                    break;
                case eFileTypes.None:
                default:
                    ArtifactImage = eImageType.File;
                    Foreground = new SolidColorBrush(Colors.Black);
                    break;
            }
           
        }

        private void ImageMakerControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenBySystem();
        }
    }
}
