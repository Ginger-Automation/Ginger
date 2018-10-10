#region License
/*
Copyright © 2014-2018 European Support Limited

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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GingerCore.Actions.ScreenCapture
{
    /// <summary>
    /// Interaction logic for WebServicesDriverWindow.xaml
    /// </summary>
    /// 
    public partial class ScreenCaptureWindow : Window
    {
        #region Data Members
        private Rect dragRect=new Rect();
        private ActCompareImgs f;
        /// <summary>
        /// Set to 'true' when the left mouse-button is down.
        /// </summary>
        private bool isLeftMouseButtonDownOnWindow = false;

        /// <summary>
        /// Set to 'true' when dragging the 'selection rectangle'.
        /// Dragging of the selection rectangle only starts when the left mouse-button is held down and the mouse-cursor
        /// is moved more than a threshold distance.
        /// </summary>
        private bool isDraggingSelectionRect = false;

        /// <summary>
        /// Records the location of the mouse (relative to the window) when the left-mouse button has pressed down.
        /// </summary>
        private  System.Windows.Point origMouseDownPoint;
        private System.Windows.Point clickMousePoint;
        private bool bCapturedOrigCoordinates = false;
        private bool bCapturedTargetCoordinates = false;
        private string ScreenPath="";
        /// <summary>
        /// The threshold distance the mouse-cursor must move before drag-selection begins.
        /// </summary>
        private static readonly double DragThreshold = 5;

        #endregion Data Members

        public ScreenCaptureWindow(ActCompareImgs act)
        {
            InitializeComponent();
            f = act;
        }


        /// <summary>
        /// Event raised when the user presses down the left mouse-button.
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !bCapturedOrigCoordinates)
            {
                isLeftMouseButtonDownOnWindow = true;
                origMouseDownPoint = e.GetPosition(this);
                this.CaptureMouse();
                bCapturedOrigCoordinates = true;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised when the user releases the left mouse-button.
        /// </summary>
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !bCapturedTargetCoordinates)
            {
                if (isDraggingSelectionRect)
                {
                    //
                    // Drag selection has ended, apply the 'selection rectangle'.
                    //

                    isDraggingSelectionRect = false;
                    ApplyDragSelectionRect();
                    bCapturedTargetCoordinates = true;
                    e.Handled = true;
                }

                if (isLeftMouseButtonDownOnWindow)
                {
                    isLeftMouseButtonDownOnWindow = false;
                    this.ReleaseMouseCapture();

                    e.Handled = true;
                }
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Event raised when the user moves the mouse button.
        /// </summary>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingSelectionRect)
            {
                //
                // Drag selection is in progress.
                //
                System.Windows.Point curMouseDownPoint = e.GetPosition(this);
                UpdateDragSelectionRect(origMouseDownPoint, curMouseDownPoint);

                e.Handled = true;
            }
            else if (isLeftMouseButtonDownOnWindow)
            {
                //
                // The user is left-dragging the mouse,
                // but don't initiate drag selection until
                // they have dragged past the threshold value.
                //
                System.Windows.Point curMouseDownPoint = e.GetPosition(this);
                var dragDelta = curMouseDownPoint - origMouseDownPoint;
                double dragDistance = Math.Abs(dragDelta.Length);
                if (dragDistance > DragThreshold)
                {
                    //
                    // When the mouse has been dragged more than the threshold value commence drag selection.
                    //
                    isDraggingSelectionRect = true;



                    InitDragSelectionRect(origMouseDownPoint, curMouseDownPoint);
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Initialize the rectangle used for drag selection.
        /// </summary>
        private void InitDragSelectionRect(System.Windows.Point pt1, System.Windows.Point pt2)
        {
            UpdateDragSelectionRect(pt1, pt2);

            dragSelectionCanvas.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Update the position and size of the rectangle used for drag selection.
        /// </summary>
        private void UpdateDragSelectionRect(System.Windows.Point pt1, System.Windows.Point pt2)
        {
            double x, y, width, height;

            //
            // Determine x,y,width and height of the rect inverting the points if necessary.
            // 

            if (pt2.X < pt1.X)
            {
                x = pt2.X;
                width = pt1.X - pt2.X;
            }
            else
            {
                x = pt1.X;
                width = pt2.X - pt1.X;
            }

            if (pt2.Y < pt1.Y)
            {
                y = pt2.Y;
                height = pt1.Y - pt2.Y;
            }
            else
            {
                y = pt1.Y;
                height = pt2.Y - pt1.Y;
            }

            //
            // Update the coordinates of the rectangle used for drag selection.
            //
            Canvas.SetLeft(dragSelectionBorder, x);
            Canvas.SetTop(dragSelectionBorder, y);
            dragSelectionBorder.Width = width;
            dragSelectionBorder.Height = height;
        }

        /// <summary>
        /// Select all nodes that are in the drag selection rectangle.
        /// </summary>
        private void ApplyDragSelectionRect()
        {
            double x = Canvas.GetLeft(dragSelectionBorder);
            double y = Canvas.GetTop(dragSelectionBorder);
            double width = dragSelectionBorder.Width;
            double height = dragSelectionBorder.Height;
            dragRect = new Rect(x, y, width, height);
        }

        private void Window_DClick(object sender, MouseEventArgs e)
        {
            bCapturedOrigCoordinates = false;
            bCapturedTargetCoordinates = false;
            clickMousePoint = e.GetPosition(this);
            System.Drawing.Point dp = new System.Drawing.Point((int)(clickMousePoint.X - origMouseDownPoint.X), (int)(clickMousePoint.Y - origMouseDownPoint.Y));
            SaveSelection(dp);
            this.Close();
        }

        public void CaptureImage(System.Drawing.Point SourcePoint, System.Drawing.Rectangle SelectionRectangle, string FilePath)
        {
            this.Hide();

            using (Bitmap bitmap = new Bitmap(SelectionRectangle.Width, SelectionRectangle.Height))
            {

                using (Graphics g = Graphics.FromImage(bitmap))
                {

                    g.CopyFromScreen(SourcePoint, System.Drawing.Point.Empty, SelectionRectangle.Size);

                }

                //if (FilePath.StartsWith("~\\") == true) FilePath = FilePath.Replace("~\\", "");
                FilePath = FilePath.Replace("~\\", f.SolutionFolder);
                bitmap.Save(FilePath, ImageFormat.Png);
            }
        }

        public string GetPathToExpectedImage()
        {
            try
            {
                //TODO: need to find a way to hold the image in the Act so it will go to shared repo have version and more
                // Need to think if good or not
                if (!Directory.Exists(System.IO.Path.Combine(f.SolutionFolder, @"Documents\ExpectedImages\")))
                    Directory.CreateDirectory(System.IO.Path.Combine(f.SolutionFolder, @"Documents\ExpectedImages\"));
            }
            catch (Exception e)
            {                
                Reporter.ToUser(eUserMsgKeys.FolderOperationError, e.Message);
            }
            //return f.SolutionFolder + @"Documents\ExpectedImages\"+Guid.NewGuid().ToString()+".png";
            return @"~\Documents\ExpectedImages\" + Guid.NewGuid().ToString() + ".png";
        }

        public string GetCordinates()
        {
            try
            {
                //TODO: need to find a way to hold the image in the Act so it will go to shared repo have version and more
                // Need to think if good or not
                System.Drawing.Point dp = new System.Drawing.Point((int)origMouseDownPoint.X, (int)origMouseDownPoint.Y);
                System.Drawing.Rectangle rc = new System.Drawing.Rectangle() { Width = (int)dragRect.Width, Height = (int)dragRect.Height };
                //CaptureImage(dp, rc, ScreenPath);
                //f.ClickX = clickdp.X;
                //f.ClickY = clickdp.Y;
                f.StartX = (int)origMouseDownPoint.X;
                f.StartY = (int)origMouseDownPoint.Y;
                f.EndX = f.StartX + (int)dragRect.Width;
                f.EndY = f.StartY + (int)dragRect.Height;
            }
            catch (Exception e)
            {                
                Reporter.ToUser(eUserMsgKeys.StaticErrorMessage, e.Message);
            }
            
            return (f.StartX + ", " + f.StartY + ", " + f.EndX + ", " + f.EndY); 
        }
        public void SaveSelection(System.Drawing.Point clickdp)
        {
            ScreenPath = GetPathToExpectedImage();

            if (ScreenPath != "")
            {

                //Allow 250 milliseconds for the screen to repaint itself (we don't want to include this form in the capture)
                System.Threading.Thread.Sleep(250);

                //Rectangle bounds = new Rectangle(CurrentTopLeft.X, CurrentTopLeft.Y, CurrentBottomRight.X - CurrentTopLeft.X, CurrentBottomRight.Y - CurrentTopLeft.Y);

                System.Drawing.Point dp = new System.Drawing.Point((int)origMouseDownPoint.X, (int)origMouseDownPoint.Y);
                System.Drawing.Rectangle rc = new System.Drawing.Rectangle() { Width = (int)dragRect.Width, Height = (int)dragRect.Height };
                CaptureImage(dp, rc, ScreenPath);
                //f.ClickX = clickdp.X;
                //f.ClickY = clickdp.Y;
                f.StartX = (int)origMouseDownPoint.X;
                f.StartY = (int)origMouseDownPoint.Y;
                f.EndX = f.StartX + (int)dragRect.Width;
                f.EndY = f.StartY + (int)dragRect.Height;
                
                f.ExpectedImgFile = ScreenPath;
            }
        }
    }
}

