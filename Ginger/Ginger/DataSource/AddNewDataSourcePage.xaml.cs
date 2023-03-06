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

using Amdocs.Ginger.Common;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Ginger.Repository;
using GingerCore;
using GingerCore.DataSource;
using Ginger.UserControls;
using Ginger.UserControlsLib;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;
using GingerCoreNET.DataSource;

namespace Ginger.DataSource
{
    /// <summary>
    /// Interaction logic for NewAgentPage.xaml
    /// </summary>
    public partial class AddNewDataSourcePage : Page
    {

        DataSourceBase mDSDetails;
        GenericWindow _pageGenericWin = null;
        string mFileType = "";
        bool okClicked = false;
        RepositoryFolder<DataSourceBase> mTargetFolder;

        public AddNewDataSourcePage(RepositoryFolder<DataSourceBase> folder)
        {           
            InitializeComponent();

            mTargetFolder = folder;
            mDSDetails = new AccessDataSource();

            mDSDetails.FilePath = mTargetFolder.FolderRelativePath + @"\GingerDataSource.mdb";
            mDSDetails.Name = "GingerDataSource";

            FilePathTextBox.IsEnabled = false;
            FileBrowseButton.IsEnabled = false;
            DSName.IsEnabled = false;
            DSTypeComboBox.IsEnabled = true;

            GingerCore.General.FillComboFromEnumType(DSTypeComboBox, typeof(DataSourceBase.eDSType));
            DSTypeComboBox.SelectionChanged += DSTypeComboBox_SelectionChanged;
            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(FilePathTextBox, TextBox.TextProperty, mDSDetails, DataSourceBase.Fields.FilePath);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DSName, TextBox.TextProperty, mDSDetails, DataSourceBase.Fields.Name);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DSTypeComboBox, ComboBox.SelectedValueProperty, mDSDetails, DataSourceBase.Fields.DSType); 
            
        }
        
        void SetAccessDataSource()
        {
            if (!(mDSDetails is AccessDataSource))
            {
                mDSDetails = new AccessDataSource();
            }

            mDSDetails.DSType = DataSourceBase.eDSType.MSAccess;
            mFileType = "mdb";
            mDSDetails.FilePath = mTargetFolder.FolderRelativePath + @"\GingerDataSource.mdb";
            FilePathTextBox.Text = mDSDetails.FilePath;
            DSName.Text = "GingerDataSource";

        }

        void SetLiteDBDataSource()
        {
            if (!(mDSDetails is GingerLiteDB))
            {
                mDSDetails = new GingerLiteDB();
            }

            mDSDetails.DSType = DataSourceBase.eDSType.LiteDataBase;
            mFileType = "db";
            mDSDetails.FilePath = mTargetFolder.FolderRelativePath + @"\LiteDB.db";
            FilePathTextBox.Text = mDSDetails.FilePath;
            DSName.Text = "LiteDB";
        }

        private void DSTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DSTypeComboBox.SelectedValue.ToString() == DataSourceBase.eDSType.MSAccess.ToString())
            {
                SetAccessDataSource();
            }
            else if (DSTypeComboBox.SelectedValue.ToString() == DataSourceBase.eDSType.LiteDataBase.ToString())
            {
                SetLiteDBDataSource();
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //validate details
            if (FilePathTextBox.Text.Trim() == string.Empty) { Reporter.ToUser(eUserMsgKey.MissingNewDSDetails, "File Path"); return; }
            else if (DSTypeComboBox.SelectedItem == null) { Reporter.ToUser(eUserMsgKey.MissingNewDSDetails, "DB type"); return; }

            mDSDetails.FilePath = FilePathTextBox.Text;
            mDSDetails.FileFullPath = mDSDetails.FilePath.Replace("~",  WorkSpace.Instance.Solution.Folder);
            mDSDetails.Name = DSName.Text;
            if (!Directory.Exists(Path.GetDirectoryName(mDSDetails.FileFullPath)))
            { Reporter.ToUser(eUserMsgKey.InvalidDSPath, Path.GetDirectoryName(mDSDetails.FileFullPath)); return; }

            mDSDetails.FilePath = mDSDetails.FilePath.Replace( WorkSpace.Instance.Solution.Folder, "~");//Pending                       
            
           

            ObservableList<DataSourceBase> DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            foreach(DataSourceBase ds in DSList)
            {
                ds.FileFullPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(ds.FilePath);
                if (ds.FileFullPath.Trim() == mDSDetails.FileFullPath.Trim())
                {
                    Reporter.ToUser(eUserMsgKey.DuplicateDSDetails, FilePathTextBox.Text.Trim());
                    return;
                }
            }
             
            
            okClicked = true;           
            
            if (File.Exists(mDSDetails.FileFullPath.Replace("~", WorkSpace.Instance.Solution.Folder)) == false)
            {
                if (mDSDetails.DSType == DataSourceBase.eDSType.MSAccess)
                {
                    byte[] obj = Properties.Resources.GingerDataSource;
                    System.IO.FileStream fs = new System.IO.FileStream(mDSDetails.FileFullPath.Replace("~", WorkSpace.Instance.Solution.Folder), System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    fs.Write(obj, 0, obj.Count());
                    fs.Close();
                    fs.Dispose();
                }
                else if (mDSDetails.DSType == DataSourceBase.eDSType.LiteDataBase)
                {
                    byte[] obj = Properties.Resources.LiteDB;
                    System.IO.FileStream fs = new System.IO.FileStream(mDSDetails.FileFullPath.Replace("~", WorkSpace.Instance.Solution.Folder), System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    fs.Write(obj, 0, obj.Count());
                    fs.Close();
                    fs.Dispose();
                }
            }

            mTargetFolder.AddRepositoryItem(mDSDetails);


            _pageGenericWin.Close();
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button okBtn = new Button();
            okBtn.Content = "OK";
            okBtn.Click += new RoutedEventHandler(OKButton_Click);
            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(okBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, true, "Cancel");
        }

        // Handles browsing of Script File from user desktop
        private void FileBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = FilePathTextBox.Text.Replace("~",  WorkSpace.Instance.Solution.Folder);
            var dlg = new System.Windows.Forms.OpenFileDialog();
            if (path != "")
                dlg.InitialDirectory = Path.GetDirectoryName(path);
            else
                dlg.InitialDirectory = System.IO.Path.Combine( WorkSpace.Instance.Solution.Folder, "DataSources");
            dlg.Title = "Select DB File";
            if(mFileType != "")
                dlg.Filter = mFileType.ToUpper() + " Files (*." + mFileType + ")|*." + mFileType + "|All Files (*.*)|*.*";
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                FilePathTextBox.Text = dlg.FileName;
                path = FilePathTextBox.Text;
            }
        }

        public DataSourceBase DSDetails
        {
            get
            {
                if (okClicked)
                {
                    return mDSDetails;
                }
                else
                {
                    return null;
                }
            }
        }

        private void Default_Checked(object sender, RoutedEventArgs e)
        {
            if (DSTypeComboBox == null)
                return;               
            
            DSTypeComboBox.IsEnabled = true;            
            FilePathTextBox.IsEnabled = false;
            FileBrowseButton.IsEnabled = false;
            DSName.IsEnabled = false;

            DSTypeComboBox.SelectedValue = DataSourceBase.eDSType.MSAccess;
            SetAccessDataSource();

            DSName.Text = "GingerDataSource";            
            FilePathTextBox.Text = mTargetFolder.FolderRelativePath + @"\GingerDataSource.mdb";
            
        }

        private void New_Checked(object sender, RoutedEventArgs e)
        {
            if (DSTypeComboBox == null)
                return;

            DSTypeComboBox.IsEnabled = true;
            DSName.IsEnabled = true;
            FilePathTextBox.IsEnabled = true;
            FileBrowseButton.IsEnabled = true;

            DSTypeComboBox.SelectedValue = DataSourceBase.eDSType.MSAccess;
            SetAccessDataSource();

            DSName.Text = "";
            FilePathTextBox.Text = "";
        }

        private void DSName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(Default.IsChecked == false)
            {
                FilePathTextBox.Clear();
                string FilePath = FilePathTextBox.Text;
                if(FilePath == "" || FilePath.StartsWith(@"~DataSources\") || FilePath.StartsWith(@"~\DataSources\"))
                {
                    if (DSName.Text != "")
                    {
                        string FileName = Amdocs.Ginger.Common.GeneralLib.General.RemoveInvalidFileNameChars(DSName.Text);
                        FilePathTextBox.Text = mTargetFolder.FolderRelativePath + @"\" + FileName + mDSDetails.GetExtension();
                        mDSDetails.Name = DSName.Text;
                    }
                    else
                        FilePathTextBox.Text = "";                        
                }               
            }            
        }
    }
}
