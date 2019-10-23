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

using Amdocs.Ginger.Common;
using System.Windows.Controls;
using GingerCore;
using GingerCore.Actions;
using GingerWPF.UserControlsLib.UCTreeView;
using Amdocs.Ginger.Common.Enums;

namespace Ginger.WindowExplorer.Appium
{
    public class AppiumWindowTreeItem : AppiumElementTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        AppiumWindowPage mAppiumWindowPage =null;

        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "@Window_16x16.png";
            string Title = "Page"; 
            return TreeViewUtils.CreateItemHeader(Title, ImageFileName);
        }

        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();
            return list;
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {            
            if (mAppiumWindowPage == null)
            {
                mAppiumWindowPage = new AppiumWindowPage(AppiumElementInfo);
            }
            return mAppiumWindowPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return null;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            TV.AddToolbarTool(eImageType.Refresh, "Refresh", TV.Tree.RefreshSelectedTreeNodeChildrens);
        }
    }
}
