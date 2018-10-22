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

using Ginger.Help.TreeViewItems;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Ginger.SolutionWindows.TreeViewItems;
using System.Diagnostics;
using System.Collections.Generic;
using GingerCore;
using Amdocs.Ginger.Common;

namespace Ginger.Help
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        string mHelpFolder = string.Empty;
        string mLibraryFolder = string.Empty;

        WebBrowser mWebBrowser = null;
        public HelpWindow(string SearchText = null)
        {
            InitializeComponent();

            MainTreeView.Tree.ItemSelected += MainTreeView_ItemSelected;

            mHelpFolder = GetGingerHelpLibraryFolder(false);
            mLibraryFolder = GetGingerHelpLibraryFolder();

            BuildLibraryTreeView();

            if (SearchText == null || SearchText== string.Empty || SearchText=="Main Window")
            {
                // Show Main Index
                ShowFile(Path.Combine(mHelpFolder, @"Index.mht"));
            }
            else
            {
                try
                {
                    ShowFile(Path.Combine(mHelpFolder, @"SearchIndex.mht"));
                    MainTreeView.SearchTree(SearchText);

                    //select first child
                    List<Type> childTypes = new List<Type>();
                    childTypes.Add(typeof(MHTHelpFileTreeItem));
                    childTypes.Add(typeof(VideoHelpFileTreeItem));
                    MainTreeView.Tree.SelectFirstVisibleChildItem(MainTreeView.Tree.TreeItemsCollection,  childTypes);
                }
                catch(Exception ex)
                {
                    ShowFile(Path.Combine(mHelpFolder, @"Index.mht")); Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                }
            }            
        }
       
        private void ShowFile(string FileName)
        {
            if (System.IO.File.Exists(FileName))
            {
                if (mWebBrowser== null)
                {
                    mWebBrowser = new WebBrowser();
                    HelpBrowserFrm.Content = mWebBrowser;
                }
                mWebBrowser.Navigate(FileName);
            }
        }

        private string GetGingerHelpLibraryFolder(bool includingLibraryFolder=true)
        {
            string folder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (includingLibraryFolder)
                folder = System.IO.Path.Combine(folder, @"Help\Library");
            else
                folder = System.IO.Path.Combine(folder, @"Help");
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            return folder;
        }

        private void BuildLibraryTreeView()
        {
            MainTreeView.TreeTitleStyle= (Style)TryFindResource("@ucTitleStyle_2");

            //add Library root folder
            HelpDocumentsFolderTreeItem DFTI = new HelpDocumentsFolderTreeItem();
            DFTI.Folder = "Library";
            DFTI.Path = mLibraryFolder;
            TreeViewItem root= MainTreeView.Tree.AddItem(DFTI);

            //expand to view the sub folders 
            root.IsExpanded = true;
        }

        private void MainTreeView_ItemSelected(object sender, EventArgs e)
        {
            BrowserGrid.Visibility = System.Windows.Visibility.Collapsed;            
            VideoPlayerGrid.Visibility = System.Windows.Visibility.Collapsed;

            TreeViewItem i = (TreeViewItem)sender;
            if (i != null)
            {
                ITreeViewItem iv = (ITreeViewItem)i.Tag;
                if (iv is MHTHelpFileTreeItem)
                {                    
                    MHTHelpFileTreeItem MHT = (MHTHelpFileTreeItem)iv;
                    BrowserGrid.Visibility = System.Windows.Visibility.Visible;
                    ShowFile(System.IO.Path.Combine(MHT.Path, MHT.FileName));
                }

                if (iv is VideoHelpFileTreeItem)
                {
                    VideoHelpFileTreeItem VHT = (VideoHelpFileTreeItem)iv;
                    VideoPlayerGrid.Visibility = System.Windows.Visibility.Visible;
                    VideoPlayer.LoadedBehavior = MediaState.Manual;
                    VideoPlayer.UnloadedBehavior = MediaState.Stop;
                    VideoPlayer.Source = new Uri(VHT.Path + @"\" + VHT.FileName);
                }
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Play();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Pause();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mWebBrowser.GoBack();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            mWebBrowser.Refresh();
        }

        private void OpenExternallyBtn_Click(object sender, RoutedEventArgs e)
        {            
            if (MainTreeView.Tree.CurrentSelectedTreeViewItem != null)
            {
                TreeViewItemBase item = (TreeViewItemBase)MainTreeView.Tree.CurrentSelectedTreeViewItem;
                if (System.IO.File.Exists(item.NodePath()))
                    Process.Start(item.NodePath());
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                mWebBrowser.Dispose();
                mWebBrowser = null;
                VideoPlayer.Pause();
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }
    }
}
