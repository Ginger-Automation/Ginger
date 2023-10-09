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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.SourceControlLib;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using GingerCore.GeneralLib;
using GingerWPF.TreeViewItemsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.ConflictResolve
{
    public sealed class ConflictComparisonTreeViewItem : NewTreeViewItemBase, ITreeViewItem
    {
        private readonly Comparison _comparison;
        private readonly Comparison.StateType[] _childrenStateFilter;

        public ConflictComparisonTreeViewItem(Comparison comparison, Comparison.StateType[] childrenStateFilter)
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
            if (itemSelectCheckBox != null)
            {
                headerStackPanel.Children.Add(itemSelectCheckBox);
            }

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
                case Comparison.StateType.Unmodified:
                    return Brushes.Transparent;
                case Comparison.StateType.Modified:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEFAD4"));
                case Comparison.StateType.Added:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CAE9E6"));
                case Comparison.StateType.Deleted:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCD8D6"));
                default:
                    throw new NotImplementedException();
            }
        }

        private CheckBox? GetItemSelectCheckBox()
        {
            if (_comparison.State == Comparison.StateType.Unmodified || _comparison.State == Comparison.StateType.Modified)
            {
                return null;
            }

            if (_comparison.HasParentComparison &&
                (_comparison.ParentComparison.State == Comparison.StateType.Added ||
                _comparison.ParentComparison.State == Comparison.StateType.Deleted))
            {
                return null;
            }

            CheckBox itemSelectCheckbox = new();
            itemSelectCheckbox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            BindingHandler.ObjFieldBinding(
                control: itemSelectCheckbox, 
                dependencyProperty: CheckBox.IsCheckedProperty, 
                obj: _comparison, 
                property: nameof(Comparison.Selected));
            itemSelectCheckbox.Tag = _comparison;
            itemSelectCheckbox.Checked += CheckBox_CheckedUnchecked;
            itemSelectCheckbox.Unchecked += CheckBox_CheckedUnchecked;

            return itemSelectCheckbox;
        }

        private void CheckBox_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            //not required
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
                _comparison.ChildComparisons.Any(childComparison => _childrenStateFilter.Contains(childComparison.State));
        }

        public ContextMenu Menu()
        {
            return null;
        }

        public object NodeObject()
        {
            return _comparison;
        }

        public void SetTools(ITreeView TV)
        {
            //not required
        }
    }
}
