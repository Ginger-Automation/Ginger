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
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GingerCore.Actions.ScreenCapture
{
    /// <summary>
    /// Interaction logic for WebServicesDriverWindow.xaml
    /// </summary>
    /// 
    public partial class LocatorImageCaptureWindow : Window
    {
        #region Data Members
        private Rect dragRect = new Rect();

        private ActImageCaptureSupport actImageCaptureSupport;
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
        private System.Windows.Point origMouseDownPoint;
        private System.Windows.Point clickMousePoint;
        private bool bCapturedOrigCoordinates = false;
        private bool bCapturedTargetCoordinates = false;
        private string mScreenImageDirectory = string.Empty;
        public string ScreenImageDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(mScreenImageDirectory))
                {
                    //TODO: need to find a way to hold the image in the Act so it will go to shared repo have version and more
                    // Need to think if good or not
                    mScreenImageDirectory = Path.Combine("~", actImageCaptureSupport.SolutionFolder + actImageCaptureSupport.ImagePath);

                    if (!Directory.Exists(mScreenImageDirectory))
                    {
                        Directory.CreateDirectory(mScreenImageDirectory);
                    }
                    mScreenImageDirectory = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(ScreenImageDirectory);
                }

                return mScreenImageDirectory;
            }
        }

        public string mScreenImageName;
        public string ScreenImageName
        {
            get
            {
                if (string.IsNullOrEmpty(mScreenImageName))
                {
                    mScreenImageName = Guid.NewGuid().ToString() + ".JPG";
                }
                return mScreenImageName;
            }
        }

        /// <summary>
        /// The threshold distance the mouse-cursor must move before drag-selection begins.
        /// </summary>
        private static readonly double DragThreshold = 5;

        #endregion Data Members

        public LocatorImageCaptureWindow(ActImageCaptureSupport act)
        {
            InitializeComponent();
            actImageCaptureSupport = act;
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

            dragSelectionCanvas1.Visibility = Visibility.Visible;
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
            Canvas.SetLeft(dragSelectionBorder1, x);
            Canvas.SetTop(dragSelectionBorder1, y);
            dragSelectionBorder1.Width = width;
            dragSelectionBorder1.Height = height;
        }

        /// <summary>
        /// Select all nodes that are in the drag selection rectangle.
        /// </summary>
        private void ApplyDragSelectionRect()
        {
            double x = Canvas.GetLeft(dragSelectionBorder1);
            double y = Canvas.GetTop(dragSelectionBorder1);
            double width = dragSelectionBorder1.Width;
            double height = dragSelectionBorder1.Height;
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

        public void CaptureImage(System.Drawing.Point SourcePoint, System.Drawing.Rectangle SelectionRectangle)
        {
            this.Hide();
            try
            {
                using (Bitmap bitmap = new Bitmap(SelectionRectangle.Width, SelectionRectangle.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(SourcePoint, System.Drawing.Point.Empty, SelectionRectangle.Size);
                    }

                    bitmap.Save(GetPathToExpectedImage(), ImageFormat.Jpeg);
                }
            }
            catch (Exception exc)
            {
                Reporter.ToLog(eLogLevel.ERROR, exc.Message, exc);
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please select image to capture");
            }

        }

        public string GetPathToExpectedImage()
        {
            return Path.Combine(ScreenImageDirectory, ScreenImageName);
        }

        public void SaveSelection(System.Drawing.Point clickdp)
        {
            if (ScreenImageDirectory != "")
            {
                //Allow 250 milliseconds for the screen to repaint itself (we don't want to include this form in the capture)
                System.Threading.Thread.Sleep(250);

                System.Drawing.Point dp = new System.Drawing.Point((int)origMouseDownPoint.X, (int)origMouseDownPoint.Y);
                System.Drawing.Rectangle rc = new System.Drawing.Rectangle() { Width = (int)dragRect.Width, Height = (int)dragRect.Height };
                CaptureImage(dp, rc);
                actImageCaptureSupport.ClickX = clickdp.X;
                actImageCaptureSupport.ClickY = clickdp.Y;
                actImageCaptureSupport.StartX = (int)origMouseDownPoint.X;
                actImageCaptureSupport.StartY = (int)origMouseDownPoint.Y;
                actImageCaptureSupport.EndX = actImageCaptureSupport.StartX + (int)dragRect.Width;
                actImageCaptureSupport.EndY = actImageCaptureSupport.StartY + (int)dragRect.Height;
                actImageCaptureSupport.LocatorImgFile = GetPathToExpectedImage();
            }
        }
    }
}

