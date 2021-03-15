using GingerCore.ALM.ZephyrEnt.Bll;
using GingerWPF.UserControlsLib.UCTreeView;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using ZephyrEntSDK.Models.Base;

namespace Ginger.ALM.ZephyrEnt.TreeViewItems
{
    class TestRepositoryFolderTreeItem : ZephyrEntTreeItem, ITreeViewItem
    {
        public string Folder { get; set; }
        public string Path { get; set; }
        public TestRepositoryFolderTreeItem()
        {
        }
        public TestRepositoryFolderTreeItem(BaseResponseItem node) : base(node)
        {
            entityType = EntityFolderType.Phase;
            if (((JArray)node.TryGetItem("categories")).Count > 0)
            {
                CurrentChildrens = new List<ITreeViewItem>();
            }
        }
        public static bool IsCreateBusinessFlowFolder { get; set; }

        private new ContextMenu mContextMenu = new ContextMenu();

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            if (Folder == "Subject")
            {
                return TreeViewUtils.CreateItemHeader(Folder, "@WorkFlow_16x16.png");
            }
            else
            {
                return TreeViewUtils.CreateItemHeader(Folder, "@Folder_16x16.png");
            }
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return CurrentChildrens;
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
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            //Set Context Menu
            mContextMenu = new ContextMenu();
        }
    }
}

