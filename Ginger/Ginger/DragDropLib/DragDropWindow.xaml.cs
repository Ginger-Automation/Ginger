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

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.UserControls;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GingerWPF.DragDropLib
{
    /// <summary>
    /// Interaction logic for DragDropWindow.xaml
    /// </summary>
    public partial class DragDropWindow : Window
    {
        int i = 1;
        public DragDropWindow()
        {
            InitializeComponent();
        }

        public void SetHeader(string Header)
        {
            xHeaderTextBlock.Text = Header;
        }

        internal void MoveToMousePosition()
        {            
            Point p = GetMousePosition();
            i++;
            this.Top = p.Y + 10;
            this.Left = p.X + 10;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        internal void SetDragIcon(DragInfo.eDragIcon eDragIcon)
        {
            switch (eDragIcon)
            {
                case DragInfo.eDragIcon.Unknown:
                    xDragOperationImage.ImageForeground = new SolidColorBrush(Colors.Red);
                    xDragOperationImage.ImageType = eImageType.Blocked;
                    break;

                case DragInfo.eDragIcon.Add:
                    xDragOperationImage.ImageForeground = new SolidColorBrush(Colors.Green);
                    xDragOperationImage.ImageType = eImageType.PlusSquare;
                    break;

                case DragInfo.eDragIcon.DoNotDrop:
                    xDragOperationImage.ImageForeground = new SolidColorBrush(Colors.Red);
                    xDragOperationImage.ImageType = eImageType.Blocked;
                    break;

                case DragInfo.eDragIcon.Move:
                    xDragOperationImage.ImageForeground = new SolidColorBrush(Colors.Green);
                    xDragOperationImage.ImageType = eImageType.MoveUpDown;
                    break;

                case DragInfo.eDragIcon.MultiAdd:
                    xDragOperationImage.ImageForeground = new SolidColorBrush(Colors.Green);
                    xDragOperationImage.ImageType = eImageType.WindowRestore;
                    break;
            }
        }
    }
}
