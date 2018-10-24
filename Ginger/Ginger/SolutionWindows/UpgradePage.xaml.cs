#region License
/*
Copyright © 2014-2018 European Support Limited

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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Environments;
using GingerCore;
using GingerCore.Repository;
using GingerCore.XMLConverters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.SolutionWindows
{
    public enum SolutionUpgradePageViewMode { UpgradeSolution, UpgradeGinger, FailedUpgradeSolution }

    /// <summary>
    /// Interaction logic for SolutionUpgradePage.xaml
    /// </summary>
    public partial class UpgradePage : Page
    {
        GenericWindow _pageGenericWin = null;

        List<string> mFilesToShow;
        List<string> mFailedFiles;
        string mSolutionFolder;
        string mSolutionName;
        SolutionUpgradePageViewMode mViewMode;
        Button mUpgradeButton;

        public UpgradePage(SolutionUpgradePageViewMode viewMode, string solutionFolder, string solutionName, List<string> filesToShow)
        {
            InitializeComponent();

            mViewMode = viewMode;
            mSolutionFolder = solutionFolder;
            mSolutionName = solutionName;           
            mFilesToShow = filesToShow;

            SetControls();
        }

        private void SetControls()
        {
            switch (mViewMode)
            {
                case SolutionUpgradePageViewMode.UpgradeSolution:
                    ExplanationLable.Text = @"The Solution '" + mSolutionName + "' contains items which were created with older version/s of Ginger (see below), it is recommended to upgrade them to current version (" + RepositorySerializer.GetCurrentGingerVersion() + ") before continuing.";

                    string BackupFolder = Path.Combine(mSolutionFolder, @"Backups\Backup_" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm"));
                    BackupFolderTextBox.Text = BackupFolder;
                    FilesListBox.ItemsSource = mFilesToShow;
                    App.ObjFieldBinding(DoNotAskAgainChkbox, CheckBox.IsCheckedProperty, App.UserProfile, nameof(UserProfile.DoNotAskToUpgradeSolutions));
                    break;

                case SolutionUpgradePageViewMode.FailedUpgradeSolution:
                    ExplanationLable.Text = @"Upgrade failed for below files so they were not changed.";
                    ExplanationLable.Foreground = Brushes.Red;
                    BackupFolderTextBox.Visibility = Visibility.Collapsed;
                    BackupFolderPanel.Visibility = Visibility.Collapsed;
                    FilesListLable.Content = "Items which Failed to be Upgraded:";
                    FilesListBox.ItemsSource = mFailedFiles;                   
                    break;

                case SolutionUpgradePageViewMode.UpgradeGinger:
                    ExplainRow.Height = new GridLength(80);
                    ExplanationLable.Text = @"The Solution in '" + mSolutionFolder + "' contains items which were created with higher Ginger version/s (see below), for loading this Solution you must upgrade Ginger version to the highest version which was used.";
                    BackupFolderPanel.Visibility = System.Windows.Visibility.Collapsed;
                    DoNotAskAgainChkbox.Visibility = System.Windows.Visibility.Collapsed;
                    FilesListLable.Content = "Items Created with Higher Version:";
                    FilesListBox.ItemsSource = mFilesToShow;
                    break;
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            switch (mViewMode)
            {
                case SolutionUpgradePageViewMode.UpgradeSolution:
                    mUpgradeButton = new Button();
                    mUpgradeButton.Content = "Upgrade";
                    mUpgradeButton.Click += new RoutedEventHandler(Upgrade);
                    ObservableList<Button> winButtons = new ObservableList<Button>();
                    winButtons.Add(mUpgradeButton);
                    GingerCore.General.LoadGenericWindow(ref _pageGenericWin, null, windowStyle, "Upgrade Solution Files to Latest Ginger Version", this, winButtons);
                    break;

                case SolutionUpgradePageViewMode.UpgradeGinger:
                    GingerCore.General.LoadGenericWindow(ref _pageGenericWin, null, windowStyle, "Ginger Version Upgrade Required", this);
                    break;
            }
        }

        private void Upgrade(object sender, RoutedEventArgs e)
        {
            try
            {
                xProcessingImage.Visibility = Visibility.Visible;
                GingerCore.General.DoEvents();
                NewRepositorySerializer newSerilizer = new NewRepositorySerializer();
                mFailedFiles = new List<string>();
                string backupFolderPath = BackupFolderTextBox.Text;
                //make sure back direcroty exist if not create
                if (!Directory.Exists(backupFolderPath))
                {
                    MakeSurePathExistforBakFile(backupFolderPath + @"\");
                }

                // now do the upgrade file by file
                foreach (string filePathToConvert in mFilesToShow)
                {
                    string filePath = filePathToConvert;
                    //remove info extention
                    if (filePath.Contains("-->"))
                    {
                        filePath = filePath.Remove(filePath.IndexOf("-->"));
                    }

                    //do upgrade
                    try
                    {
                        //first copy to backup folder
                        string BakFile = filePath.Replace(mSolutionFolder, BackupFolderTextBox.Text + @"\");
                        MakeSurePathExistforBakFile(BakFile);
                        System.IO.File.Copy(filePath, BakFile, true);

                        //make sure backup was created
                        if (File.Exists(BakFile) == true)
                        {
                            //Do Upgrade by unserilize and serlize the item using new serilizer
                            //unserilize
                            string itemXML = File.ReadAllText(filePath);
                            RepositoryItemBase itemObject = (RepositoryItemBase)NewRepositorySerializer.DeserializeFromText(itemXML);
                            itemObject.FilePath = filePath;
                            //serlize
                            newSerilizer.SaveToFile(itemObject, filePath);
                        }
                        else
                        {
                            mFailedFiles.Add(filePathToConvert);
                        }
                    }
                    catch (Exception ex)
                    {


                        Reporter.ToLog(eAppReporterLogLevel.WARN, string.Format("Failed to upgrade the solution file '{0}'", filePath), ex);
                        mFailedFiles.Add(filePathToConvert);
                    }
                }

                if (mFailedFiles.Count == 0)
                {
                    Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "Upgrade ended successfully.");
                    _pageGenericWin.Close();
                }
                else
                {
                    Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Upgrade failed for some of the files.");
                    mViewMode = SolutionUpgradePageViewMode.FailedUpgradeSolution;
                    SetControls();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to upgrade the solution files", ex);
                Reporter.ToUser(eUserMsgKeys.StaticErrorMessage, "Error occurred during upgrade, details: " + ex.Message);
                _pageGenericWin.Close();
            }
            finally
            {
                xProcessingImage.Visibility = Visibility.Collapsed;
            }
        }

        private void MakeSurePathExistforBakFile(string BakFile)
        {
            string fileFolderPath = string.Empty;
            if (System.IO.Path.GetFileName(BakFile) != string.Empty)
                fileFolderPath = BakFile.Replace(System.IO.Path.GetFileName(BakFile), string.Empty);
            else
                fileFolderPath = BakFile;
            fileFolderPath = fileFolderPath.TrimEnd('\\');
            string path = string.Empty;            
            if (fileFolderPath[0] == '\\')//for supporting shared folders paths
            {
                fileFolderPath = fileFolderPath.TrimStart('\\');
                path = @"\\";
            }
            string[] PathFolders = fileFolderPath.Split('\\');
            path += PathFolders[0] + "\\";//driver or machine name
            //process all paths folders
            for (int i=1; i<PathFolders.Length; i++)
            {                
                path += PathFolders[i] + "\\";
                if (!Directory.Exists(path))                
                    Directory.CreateDirectory(path);                
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Select Backup Folder";
            dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            if (mSolutionFolder != string.Empty)
                dlg.SelectedPath = mSolutionFolder;
            dlg.ShowNewFolderButton = true;
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                BackupFolderTextBox.Text = dlg.SelectedPath;
            }
        }
    }
}