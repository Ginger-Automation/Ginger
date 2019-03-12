#region License
/*
Copyright © 2014-2019 European Support Limited

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

using Ginger.SolutionWindows.TreeViewItems;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace Ginger.Help.TreeViewItems
{
    public class HelpDocumentsFolderTreeItem : TreeViewItemBase, ITreeViewItem
    {
        public string Folder { get; set; }
        public string Path { get; set; }

      //  private ContextMenu mContextMenu = new ContextMenu();
        public eBusinessFlowsTreeViewMode mViewMode;

        public HelpDocumentsFolderTreeItem()
        {
            
        }

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {            
            return TreeViewUtils.CreateItemHeader(Folder, "@Folder2_16x16.png");                        
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();
            
            //collect files & folders
            string[] folders = System.IO.Directory.GetDirectories(Path);           
            string[] files = System.IO.Directory.GetFiles(Path);
            List<string> filesAndFolders = new List<string>();
            foreach (string folder in folders)
                filesAndFolders.Add(folder);
            foreach (string file in files)
                filesAndFolders.Add(file);
            filesAndFolders.Sort();//sort alphabetically

            foreach (string itemPath in filesAndFolders)
            {
                //check if it a file or folder
                if (System.IO.File.GetAttributes(itemPath).HasFlag(FileAttributes.Directory))
                {
                    //--add folder
                    HelpDocumentsFolderTreeItem DFTI = new HelpDocumentsFolderTreeItem();
                    DFTI.Folder = itemPath.Replace(Path + @"\", "");
                    DFTI.Path = itemPath;
                    Childrens.Add(DFTI);
                }
                else
                {
                    //--add file

                    ////avoid the index files
                    string fileName = System.IO.Path.GetFileName(itemPath).ToLower();
                    if (fileName == "index.mht" || fileName == "searchindex.mht")
                        continue;

                    //decide based on extension what TV item to display
                    string ext = System.IO.Path.GetExtension(itemPath).ToUpper();
                    switch (ext)
                    {
                        case ".MHT":
                            MHTHelpFileTreeItem MHT = new MHTHelpFileTreeItem();
                            MHT.FileName = itemPath.Replace(Path + @"\", "");
                            MHT.Path = Path;
                            Childrens.Add(MHT);
                            break;

                        case ".MP4":
                        case ".MPEG":
                        case ".MPG":
                            VideoHelpFileTreeItem VHT = new VideoHelpFileTreeItem();
                            VHT.FileName = itemPath.Replace(Path + @"\", "");
                            VHT.Path = Path;
                            Childrens.Add(VHT);
                            break;

                        //TODO: other types
                        case ".TXT":
                            break;

                        default:
                            break;

                    }
                }
            }

            return Childrens;
        }

        
        bool ITreeViewItem.IsExpandable()
        {
            // Folder is expandable if it has sub folders or files.
            if (System.IO.Directory.GetDirectories(Path).Length >0 || System.IO.Directory.GetFiles(Path).Length > 0 )
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }

        Page ITreeViewItem.EditPage()
        {
            return null;            
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;            
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
        }
    }
}
