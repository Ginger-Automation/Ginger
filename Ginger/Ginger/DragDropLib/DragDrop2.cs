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

using GingerWPF.DragDropLib;
using System;
using System.Windows;
using System.Windows.Input;

namespace GingerWPF.DragDropLib
{
    public static class DragDrop2
    {
        private static Point _startPoint;
        private static bool IsDragging;
        public static DragInfo DragInfo;

        private static DragDropWindow DDW = new DragDropWindow();

        private static void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);            
        }

      

        public static void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !IsDragging)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    DragInfo = new DragInfo();                    
                    DragInfo.OriginalSource = e.OriginalSource;
                    ((IDragDrop)sender).StartDrag(DragInfo);                    
                    // We start drag only of control put data in the drag data
                    if (DragInfo.Data != null)
                    {
                        StartDrag(e);
                    }
                }
            }

        }

        private static void StartDrag(MouseEventArgs e)
        {
            if (DragInfo.Header == null) return;
            IsDragging = true;
            DataObject data = new DataObject("Header", DragInfo.Header);
            var v = e.OriginalSource; // The UI element that was clicked can be sub element in grid for example            

            DDW.SetHeader(DragInfo.Header);
            DDW.MoveToMousePosition();
            DDW.Show();            
            
            //TODO decide effects
            DragDropEffects de = DragDrop.DoDragDrop(DragInfo.DragSource, data, DragDropEffects.Move | DragDropEffects.Copy);
            
            IsDragging = false;
            DDW.Hide();
        }
        

        private static void DragSource_Drop(object sender, DragEventArgs e)
        {
            if (DragInfo.DragIcon == DragDropLib.DragInfo.eDragIcon.Copy || DragInfo.DragIcon == DragDropLib.DragInfo.eDragIcon.Move)
            {
                ((IDragDrop)DragInfo.DragTarget).Drop(DragInfo);
            }
        }

      
        private static void DragTarget_DragEnter(object sender, DragEventArgs e)
        {
             DragInfo.DragIcon = DragDropLib.DragInfo.eDragIcon.Unknown;
             ((IDragDrop)sender).DragEnter(DragInfo);             
             DDW.SetDragIcon(DragInfo.DragIcon);
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
