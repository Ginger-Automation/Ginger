using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using GingerCore.GeneralLib;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

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
            StackPanel headerStackPanel = new();
            headerStackPanel.Orientation = Orientation.Horizontal;
           
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
                RepositoryItemBase? rib = (RepositoryItemBase?)Activator.CreateInstance(_comparison.DataType);
                if (rib != null)
                    itemImage.ImageType = rib.ItemImageType;
                else
                    itemImage.ImageType = eImageType.Info;
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
                itemHeaderLabel.Content = _comparison.Name;
            else
                itemHeaderLabel.Content = _comparison.Name + ": " + _comparison.DataAsString;

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

        }
    }
}
