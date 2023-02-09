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

using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using GingerCore.ALM;
using GingerCore.ALM.ZephyrEnt.Bll;
using ZephyrEntStdSDK.Models.Base;

namespace Ginger.ALM.ZephyrEnt.TreeViewItems
{
    public class ZephyrEntFolderTreeItem : ZephyrEntTreeItem, ITreeViewItem
    {
        public static bool IsCreateBusinessFlowFolder { get; set; }
        public ZephyrEntFolderTreeItem(BaseResponseItem node) : base(node)
        {
        }

        public ZephyrEntFolderTreeItem()
        {
        }

        public string Description { get; set; }
        public bool FolderPath { get; set; }
        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(Name, "@Folder_16x16.png");
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            if (entityType == EntityFolderType.Cycle)
            {
                List<ITreeViewItem> tsChildrens = new List<ITreeViewItem>();
                CurrentChildrens.ForEach(cc =>
                {
                    List<BaseResponseItem> phase = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetZephyrEntPhaseById(Convert.ToInt32(((ZephyrEntTreeItem)cc).Id));
                    ((ZephyrEntTreeItem)cc).Id = phase[0].TryGetItem("id").ToString();
                    if (Convert.ToInt32(phase[0].TryGetItem("testcaseCount")) > 0 && FolderPath)
                    {
                        ZephyrEntPhaseTreeItem zeTS = new ZephyrEntPhaseTreeItem(phase[0]);
                        tsChildrens.Add(zeTS);
                    }
                });
                CurrentChildrens.AddRange(tsChildrens);
            }
            else
            {
                CurrentChildrens = new List<ITreeViewItem>();
                List<BaseResponseItem> module = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetTreeByCretiria(entityType.ToString(), Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), 180, Convert.ToInt32(Id));
                 
                module.ForEach(p =>
                {
                    if (((JArray)p.TryGetItem("categories")).Count > 0)
                    {
                        ZephyrEntFolderTreeItem zeFolder = new ZephyrEntFolderTreeItem(p);
                        zeFolder.CurrentChildrens = new List<ITreeViewItem>();
                        foreach (var item in (JArray)p.TryGetItem("categories"))
                        {
                            dynamic d = JObject.Parse(item.ToString());
                            zeFolder.CurrentChildrens.Add(new ZephyrEntFolderTreeItem()
                            { Id = d.id.ToString(), Name = d.name.ToString(), entityType = EntityFolderType.Module, FolderPath = FolderPath });
                            zeFolder.entityType = EntityFolderType.Module;
                            zeFolder.FolderPath = FolderPath;
                        }
                        CurrentChildrens.Add(zeFolder);
                    }
                    if (Convert.ToInt32(p.TryGetItem("testcaseCount")) > 0 && !FolderPath)
                    {
                        ZephyrEntPhaseTreeItem zeTS = new ZephyrEntPhaseTreeItem(p);
                        CurrentChildrens.Add(zeTS);
                    }
                });
            }

            return CurrentChildrens;
        }
    }
}

