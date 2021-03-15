using Ginger.SolutionWindows.TreeViewItems;
using GingerCore;
using GingerCore.ALM.ZephyrEnt.Bll;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using ZephyrEntSDK.Models.Base;

namespace Ginger.ALM.ZephyrEnt.TreeViewItems
{
    public class ZephyrEntTreeItem : TreeViewItemBase, ITreeViewItem
    {
        private BaseResponseItem treeItemData = new BaseResponseItem();
        private bool isExpandable = true;
        public EntityFolderType entityType { get; set; }
        public BusinessFlow QCExplorer { get; set; }
        public List<ITreeViewItem> CurrentChildrens = null;
        public string Id { get; set; }
        public string Name { get; set; }
        public string VersionId { get; set; }
        public BaseResponseItem TreeItemData
        {
            get { return treeItemData; }
            set { treeItemData = value; }
        }
        public ZephyrEntTreeItem()
        {

        }
        public ZephyrEntTreeItem(BaseResponseItem node)
        {
            this.treeItemData = node;
            this.Name = node.TryGetItem("name").ToString();
            this.Id = node.TryGetItem("id").ToString();
        }

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(Name, "@WorkFlow_16x16.png");
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return CurrentChildrens;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return isExpandable;
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
            // there is not tools needed at this stage
        }
    }
}

