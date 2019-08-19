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

using System.Windows.Controls;
using Ginger.WindowExplorer;
using GingerCore.Drivers.ASCF;
using GingerWPF.UserControlsLib.UCTreeView;

namespace Ginger.Actions.Locators.ASCF
{
    class ASCFGridTreeItem : ASCFControlTreeItem, ITreeViewItem, IWindowExplorerTreeItem
    {
        public ASCFDriver ASCFDriver { get; set; }

        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "@Grid_16x16.png";
            return TreeViewUtils.CreateItemHeader(Name, ImageFileName);
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            return null;
        }
        
        ContextMenu ITreeViewItem.Menu()
        {
            return null;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
        }
    }
}
