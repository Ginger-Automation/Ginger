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
using System.Diagnostics;
using System.Windows.Controls;

namespace Ginger.Help.TreeViewItems
{
    public class VideoHelpFileTreeItem : TreeViewItemBase, ITreeViewItem
    {
        public string FileName { get; set; }
        public string Path { get; set; }

        private ContextMenu mContextMenu = new ContextMenu();

        public VideoHelpFileTreeItem()
        {
            
        }
        override public string NodePath()
        {
            return System.IO.Path.Combine(Path, FileName);
        }

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {            
            return TreeViewUtils.CreateItemHeader(System.IO.Path.GetFileNameWithoutExtension(FileName), "@Video_16x16.png");                        
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return null;
        }
        
        bool ITreeViewItem.IsExpandable()
        {
            return false;
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
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            TreeViewUtils.AddMenuItem(mContextMenu, "Open by External Player", OpenInExternal, null, "@CreateShorCut_16x16.png");
            mTreeView.AddToolbarTool("@CreateShorCut_16x16.png", "Open by External Player", OpenInExternal);
        }

        private void OpenInExternal(object sender, System.Windows.RoutedEventArgs e)
        {
            string fullFileName = System.IO.Path.Combine(Path, FileName);
            if (System.IO.File.Exists(fullFileName))
                Process.Start(fullFileName);
        }
    }
}
