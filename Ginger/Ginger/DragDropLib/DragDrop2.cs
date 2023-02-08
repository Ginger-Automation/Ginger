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
using Amdocs.Ginger.Repository;
using Ginger.UserControlsLib.UCListView;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GingerWPF.DragDropLib
{
    public static class DragDrop2
    {
        private static Point _startPoint;
        private static bool IsDragging;
        public static DragInfo DrgInfo;
        private static Point _DroppedPoint;

        private static DragDropWindow DDW = new DragDropWindow();

        private static void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        /// <summary>
        /// SetDragIcon to set an appropriate Icon for Drag Drop events
        /// value = true : allows item to be dropped/added on the target region with "+" icon
        /// value = false : means "Do Not Drop"
        /// </summary>
        /// <param name="isDraggable"></param>
        public static void SetDragIcon(bool isDraggable, bool multipleItems = false)
        {
            if (isDraggable == true)
            {
                if (multipleItems)
                {
                    DrgInfo.DragIcon = DragInfo.eDragIcon.MultiAdd;
                }
                else
                {
                    DrgInfo.DragIcon = DragInfo.eDragIcon.Add;
                }
            }
            else
            {
                DrgInfo.DragIcon = DragInfo.eDragIcon.DoNotDrop;
            }
        }

        public static void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !IsDragging)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    DrgInfo = new DragInfo();
                    DrgInfo.OriginalSource = e.OriginalSource;
                    ((IDragDrop)sender).StartDrag(DrgInfo);
                    // We start drag only of control put data in the drag data
                    if (DrgInfo.Data != null)
                    {
                        StartDrag(e);
                    }
                }
            }

        }

        private static void StartDrag(MouseEventArgs e)
        {
            if (DrgInfo.Header == null) return;
            IsDragging = true;
            DataObject data = new DataObject("Header", DrgInfo.Header);
            var v = e.OriginalSource; // The UI element that was clicked can be sub element in grid for example            

            DDW.SetHeader(DrgInfo.Header);
            DDW.MoveToMousePosition();
            DDW.Show();

            //TODO decide effects
            DragDropEffects de = DragDrop.DoDragDrop(DrgInfo.DragSource, data, DragDropEffects.Move | DragDropEffects.Copy);

            ResetDragDrop();
        }

        public static void ResetDragDrop()
        {
            IsDragging = false;
            DDW.Hide();
        }

        private static void DragSource_Drop(object sender, DragEventArgs e)
        {
            if (DrgInfo.DragIcon == DragInfo.eDragIcon.Add || DrgInfo.DragIcon == DragInfo.eDragIcon.Move || DrgInfo.DragIcon == DragInfo.eDragIcon.MultiAdd)
            {
                try
                {
                    if (sender is UcListView)
                    {
                        _DroppedPoint = e.GetPosition(sender as UcListView);
                    }
                    if(sender is Ginger.ucGrid)
                    {
                        _DroppedPoint = e.GetPosition(sender as Ginger.ucGrid);
                    }
                    object droppedItem = DrgInfo.Data;
                    if (droppedItem is ObservableList<RepositoryItemBase>)
                    {
                        ObservableList<RepositoryItemBase> repoItemsList = droppedItem as ObservableList<RepositoryItemBase>;
                        if (repoItemsList != null && repoItemsList.Count > 0)
                        {
                            foreach (RepositoryItemBase listItem in repoItemsList)
                            {
                                DragInfo newDragInfo = new DragInfo()
                                {
                                    DragIcon = DrgInfo.DragIcon,
                                    DragSource = DrgInfo.DragSource,
                                    DragTarget = DrgInfo.DragTarget,
                                    Header = DrgInfo.Header,
                                    OriginalSource = DrgInfo.OriginalSource,
                                    Data = listItem
                                };

                                ((IDragDrop)DrgInfo.DragTarget).Drop(newDragInfo);
                            }
                        }
                    }
                    else
                    {
                        ((IDragDrop)DrgInfo.DragTarget).Drop(DrgInfo);
                    }

                }
                catch (Exception ex)
                {
                    ResetDragDrop();
                }
            }
        }

        public static void ShuffleControlsItems(RepositoryItemBase draggedItem, RepositoryItemBase itemDroppedOver, UcListView xUCListView)
        {
            int newIndex = xUCListView.DataSourceList.IndexOf(itemDroppedOver);
            int oldIndex = xUCListView.DataSourceList.IndexOf(draggedItem);

            xUCListView.DataSourceList.Move(oldIndex, newIndex);
        }

        public static object GetRepositoryItemHit(UcListView xUCListView)
        {
            if (_DroppedPoint != null)
            {
                HitTestResult htResult = VisualTreeHelper.HitTest(xUCListView, _DroppedPoint);

                if (htResult != null)
                {
                    FrameworkElement fwElem = htResult.VisualHit as FrameworkElement;
                    if (fwElem != null)
                    {
                        if (fwElem.DataContext != null && fwElem.DataContext is RepositoryItemBase)
                        {
                            return fwElem.DataContext;
                        }
                    }
                }

            }

            return null;
        }
        public static object GetGridItemHit(Ginger.ucGrid xUCGrid)
        {
            if (_DroppedPoint != null)
            {
                HitTestResult htResult = VisualTreeHelper.HitTest(xUCGrid, _DroppedPoint);

                if (htResult != null)
                {
                    FrameworkElement fwElem = htResult.VisualHit as FrameworkElement;
                    if (fwElem != null)
                    {
                        if (fwElem.DataContext != null)
                        {
                            return fwElem.DataContext;
                        }
                    }
                }

            }

            return null;
        }
        private static void DragTarget_DragEnter(object sender, DragEventArgs e)
        {
            DrgInfo.DragIcon = DragDropLib.DragInfo.eDragIcon.Unknown;
            ((IDragDrop)sender).DragEnter(DrgInfo);
            DDW.SetDragIcon(DrgInfo.DragIcon);
        }


        private static void DragSource_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            DDW.MoveToMousePosition();
            e.Handled = true;
        }


        // TODO: add param if allow Drag/Drop or both, based on it hook the events
        public static void HookEventHandlers(UIElement DragDropControl)
        {
            DragDropControl.PreviewMouseLeftButtonDown += DragSource_PreviewMouseLeftButtonDown;
            DragDropControl.PreviewMouseMove += DragSource_PreviewMouseMove;
            DragDropControl.Drop += DragSource_Drop;
            DragDropControl.DragEnter += DragTarget_DragEnter;
            DragDropControl.GiveFeedback += DragSource_GiveFeedback;
        }

        public static void UnHookEventHandlers(UIElement DragDropControl)
        {
            DragDropControl.PreviewMouseLeftButtonDown -= DragSource_PreviewMouseLeftButtonDown;
            DragDropControl.PreviewMouseMove -= DragSource_PreviewMouseMove;
            DragDropControl.Drop -= DragSource_Drop;
            DragDropControl.DragEnter -= DragTarget_DragEnter;
            DragDropControl.GiveFeedback -= DragSource_GiveFeedback;
        }

        private static void DragDropControl_DragOver(object sender, DragEventArgs e)
        {
        }
    }
}
