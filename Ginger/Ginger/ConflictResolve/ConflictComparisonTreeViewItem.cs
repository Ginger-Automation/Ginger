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
    public sealed class ConflictComparisonTreeViewItem : NewTreeViewItemBase, ITreeViewItem
    {
        private readonly Comparison _comparison;
        private readonly State[] _childrenStateFilter;

        public ConflictComparisonTreeViewItem(Comparison comparison, State[] childrenStateFilter)
        {
            _comparison = comparison;
            _childrenStateFilter = childrenStateFilter;
        }

        public List<ITreeViewItem> Childrens()
        {
            if (_comparison.HasChildComparisons)
            {
                return _comparison.ChildComparisons
                    .Where(childComparison => _childrenStateFilter.Contains(childComparison.State))
                    .Select(childComparison => (ITreeViewItem)new ConflictComparisonTreeViewItem(childComparison, _childrenStateFilter))
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

            SolidColorBrush itemColor = GetItemColor();
            headerStackPanel.Background = itemColor;

            CheckBox? itemSelectCheckBox = GetItemSelectCheckBox();
            if(itemSelectCheckBox != null)
                headerStackPanel.Children.Add(itemSelectCheckBox);
           
            ImageMakerControl itemImage = GetItemImage();
            headerStackPanel.Children.Add(itemImage);

            Label itemHeader = GetItemHeader();
            headerStackPanel.Children.Add(itemHeader);

            return headerStackPanel;
        }

        private SolidColorBrush GetItemColor()
        {
            switch (_comparison.State)
            {
                case State.Unmodified:
                    return Brushes.Transparent;
                case State.Modified:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEFAD4"));
                case State.Added:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CAE9E6"));
                case State.Deleted:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCD8D6"));
                default:
                    throw new NotImplementedException();
            }
        }

        private CheckBox? GetItemSelectCheckBox()
        {
            if (_comparison.State == State.Unmodified || _comparison.State == State.Modified)
                return null;

            CheckBox itemSelectCheckbox = new();
            itemSelectCheckbox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            BindingHandler.ObjFieldBinding(
                control: itemSelectCheckbox, 
                dependencyProperty: CheckBox.IsCheckedProperty, 
                obj: _comparison, 
                property: nameof(Comparison.Selected));
            return itemSelectCheckbox;
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
                _comparison.ChildComparisons.Any(childComparison => _childrenStateFilter.Contains(childComparison.State));
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
