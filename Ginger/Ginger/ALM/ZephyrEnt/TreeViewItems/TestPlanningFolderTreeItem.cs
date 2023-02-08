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

using GingerCore.ALM;
using GingerCore.ALM.ZephyrEnt.Bll;
using GingerWPF.UserControlsLib.UCTreeView;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using ZephyrEntStdSDK.Models.Base;

namespace Ginger.ALM.ZephyrEnt.TreeViewItems
{
    class TestPlanningFolderTreeItem : ZephyrEntTreeItem, ITreeViewItem
    {
        public string Folder { get; set; }
        public string Path { get; set; }
        public string TestSetName { get; set; }
        public bool FolderOnly { get; set; }
        public int CycleId { get; set; }
        public int ParentId { get; set; }
        public int RevisionId { get; set; }

        private new ContextMenu mContextMenu = new ContextMenu();

        public TestPlanningFolderTreeItem()
        {
        }
        public TestPlanningFolderTreeItem(BaseResponseItem node) : base(node)
        {
        }

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }

        StackPanel ITreeViewItem.Header()
        {
            //if root item
            if (Folder == "Root")
            {
                return TreeViewUtils.CreateItemHeader(Folder, "@WorkFlow_16x16.png");
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(TestSetName))
                { return TreeViewUtils.CreateItemHeader(TestSetName, "@TestSet_16x16.png"); }
                else { return TreeViewUtils.CreateItemHeader(Folder, "@Folder_16x16.png"); }
            }

        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            if (entityType == EntityFolderType.Cycle)
            {
                List<ITreeViewItem> tsChildrens = new List<ITreeViewItem>();
                CurrentChildrens.ForEach(cc =>
                {
                    List<BaseResponseItem> phase = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetZephyrEntPhaseById(Convert.ToInt32(((TestPlanningFolderTreeItem)cc).Id));
                    ((ZephyrEntTreeItem)cc).Id = phase[0].TryGetItem("id").ToString();
                    if (Convert.ToInt32(phase[0].TryGetItem("testcaseCount")) > 0 && !FolderOnly)
                    {
                        ZephyrEntPhaseTreeItem zeTS = new ZephyrEntPhaseTreeItem(phase[0]);
                        zeTS.Path = ((TestPlanningFolderTreeItem)cc).Path;
                        if(!String.IsNullOrEmpty(((TestPlanningFolderTreeItem)cc).CycleId.ToString()))
                        {
                            zeTS.TestSetID = ((TestPlanningFolderTreeItem)cc).CycleId.ToString();
                        }
                        zeTS.FatherId = Id;
                        zeTS.entityType = ((ZephyrEntTreeItem)cc).entityType;
                        zeTS.TestSetStatuses.AddRange(((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetTCsDataSummary(Convert.ToInt32(zeTS.TestSetID)));
                        tsChildrens.Add(zeTS);
                    }
                });
                CurrentChildrens.AddRange(tsChildrens);
                this.entityType = EntityFolderType.CyclePhase;
            }
            else
            {
                CurrentChildrens = new List<ITreeViewItem>();
                List <BaseResponseItem> module = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetTreeByCretiria(entityType.ToString(), Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), 180, Convert.ToInt32(Id));   
                module.ForEach(p =>
                {

                    TestPlanningFolderTreeItem zeFolder = new TestPlanningFolderTreeItem(p);
                    zeFolder.entityType = EntityFolderType.Module;
                    zeFolder.Folder = zeFolder.Name;
                    zeFolder.Path = this.Path + '\\' + zeFolder.Name;
                    zeFolder.FolderOnly = this.FolderOnly;
                    zeFolder.CycleId = CycleId;
                    zeFolder.ParentId = Convert.ToInt32(p.TryGetItem("parentId"));
                    zeFolder.CurrentChildrens = new List<ITreeViewItem>();
                    zeFolder.RevisionId = Convert.ToInt32(p.TryGetItem("revision"));
                    foreach (var item in (JArray)p.TryGetItem("categories"))
                    {
                        dynamic d = JObject.Parse(item.ToString());
                        zeFolder.CurrentChildrens.Add(new TestPlanningFolderTreeItem()
                        {
                            Id = d.id.ToString(),
                            Name = d.name.ToString(),
                            entityType = EntityFolderType.Module,
                            Folder = d.name.ToString(),
                            Path = zeFolder.Path + '\\' + d.name.ToString(),
                            FolderOnly = this.FolderOnly,
                            ParentId = Convert.ToInt32(zeFolder.Id),
                            CycleId = CycleId
                        });
                    }
                    CurrentChildrens.Add(zeFolder);

                    if (Convert.ToInt32(p.TryGetItem("testcaseCount")) > 0 && !FolderOnly)
                    {
                        ZephyrEntPhaseTreeItem zeTS = new ZephyrEntPhaseTreeItem(p);
                        zeTS.Path = this.Path + '\\' + zeTS.Name;
                        zeTS.TestSetStatuses.AddRange(((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetTCsDataSummary(Convert.ToInt32(zeTS.TestSetID)));
                        zeTS.FatherId = zeFolder.CycleId.ToString();
                        zeTS.entityType = zeFolder.entityType;
                        CurrentChildrens.Add(zeTS);
                    }
                });
            }
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

