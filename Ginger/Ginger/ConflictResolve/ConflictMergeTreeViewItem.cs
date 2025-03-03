#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.SourceControlLib;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Ginger.ConflictResolve
{
    public sealed class ConflictMergeTreeViewItem : NewTreeViewItemBase, ITreeViewItem
    {
        private readonly Comparison _comparison;

        public ConflictMergeTreeViewItem(Comparison comparison)
        {
            _comparison = comparison;
        }

        public List<ITreeViewItem> Childrens()
        {
            if (_comparison.HasChildComparisons)
            {
                return _comparison.ChildComparisons
                    .Select(childComparison => (ITreeViewItem)new ConflictMergeTreeViewItem(childComparison))
                    .ToList();
            }
            return Array.Empty<ITreeViewItem>().ToList();
        }

        public Page EditPage(Context mContext = null)
        {
            return null;
        }

        public StackPanel Header()
        {
            StackPanel headerStackPanel = new()
            {
                Orientation = Orientation.Horizontal
            };

            ImageMakerControl itemImage = GetItemImage();
            headerStackPanel.Children.Add(itemImage);

            Label itemHeader = GetItemHeader();
            headerStackPanel.Children.Add(itemHeader);

            return headerStackPanel;
        }

        private ImageMakerControl GetItemImage()
        {
            ImageMakerControl itemImage = new ImageMakerControl();
            if (_comparison.HasData && _comparison.Data is RepositoryItemBase ribData)
            {
                itemImage.ImageType = ribData.ItemImageType;
            }
            else if (_comparison.DataType != null && typeof(RepositoryItemBase).IsAssignableFrom(_comparison.DataType))
            {
                if (_comparison.DataType.IsAbstract || _comparison.DataType.GetConstructor(Type.EmptyTypes) == null)
                {
                    itemImage.ImageType = eImageType.Unknown;
                }
                else
                {
                    RepositoryItemBase? rib = (RepositoryItemBase?)Activator.CreateInstance(_comparison.DataType);
                    if (rib != null)
                    {
                        itemImage.ImageType = rib.ItemImageType;
                    }
                    else
                    {
                        itemImage.ImageType = eImageType.Info;
                    }
                }
            }
            else
            {
                itemImage.ImageType = eImageType.Info;
            }

            itemImage.Width = 16;
            itemImage.Height = 16;

            return itemImage;
        }

        private Label GetItemHeader()
        {
            Label itemHeaderLabel = new();

            if (_comparison.HasChildComparisons)
            {
                itemHeaderLabel.Content = _comparison.Name;
            }
            else
            {
                itemHeaderLabel.Content = _comparison.Name + ": " + _comparison.DataAsString;
            }
            return itemHeaderLabel;
        }

        public bool IsExpandable()
        {
            return
                _comparison.HasChildComparisons &&
                _comparison.ChildComparisons.Any();
        }

        public ContextMenu Menu()
        {
            return null;
        }

        public object NodeObject()
        {
            return null;
        }

        public void SetTools(ITreeView TV)
        {
            //this method is not required
        }
    }
}
