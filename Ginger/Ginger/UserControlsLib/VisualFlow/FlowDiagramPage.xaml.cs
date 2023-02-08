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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static Ginger.UserControlsLib.VisualFlow.FlowElement;

namespace Ginger.UserControlsLib.VisualFlow
{
    /// <summary>
    /// this class is for Flow Diagram to show the Business flow or other flows in nice visual way for the user
    /// </summary>
    public partial class FlowDiagramPage : Page
    {
        List<FlowLink> GEList = new List<FlowLink>();

        enum CurrentAction
        {
            None,
            Move,
            Resize,
            Link
        }
        public double mCanvasHeight=10000;
        public double CanvasHeight
        {
            get
            {
                return mCanvasHeight;
            }
            set
            {
                MainCanvas.Height = mCanvasHeight = value;
            }
        }

        public ScrollViewer ScrollViewer
        {
            get
            {
                return xScrollViewer;
            }
        }
        public UserControl ZoomPanelContainer
        {
            get
            {
                return ZoomPanel;
            }
        }
        public double mCanvasWidth = 10000;
        public double CanvasWidth
        {
            get
            {
                return mCanvasWidth;
            }
            set
            {
                MainCanvas.Width = mCanvasWidth = value;
            }
        }
        public Canvas Canvas
        {
            get
            {
                return MainCanvas;
            }
        }
        public Brush BackGround { get; set; }
        public double FlowDiagramHeight { get; set; }
        public double FlowDiagramWidth { get; set; }
        public FlowElement mCurrentFlowElem;
        Thickness mCurrentFlowElemOriginalMargin;
        double mCurrentFlowElemOriginalWidth;
        double mCurrentFlowElemOriginalHeight;
        Point mRelativeMousePoint;
        string mCurrnetFlowElemControl;
        private bool mSetHeighLight = true;
        public bool SetHighLight
        {
            get
            {
                return mSetHeighLight;
            }
            set
            {
                mSetHeighLight = value;
            }
        }
        
        public HorizontalAlignment ZoomAlignment
        {
            get
            {
                return ZoomPanel.HorizontalAlignment;
            }
            set
            {
                ZoomPanel.HorizontalAlignment = value;
            }
        }
        private bool mIsMovable = true;
        public bool Ismovable
        {
            get
            {
                return mIsMovable;
            }
            set
            {
                mIsMovable = value;
            }
        }
        public double ZoomPercent
        {
            get
            {
                return ZoomPanel.ZoomSlider.Value;
            }
            set
            {
                ZoomPanel.ZoomSlider.Value = value;
            }
        }
        CurrentAction mCurrentAction = CurrentAction.None;

        public FlowDiagramPage()
        {
            InitializeComponent();
            MainCanvas.Background = BackGround;
            ZoomPanel.ZoomSlider.ValueChanged += ZoomSlider_ValueChanged;
        }

        public void AddFlowElem(FlowElement FE, int index = -1)
        {
            List<FlowElement> fe = new List<FlowElement>();
            fe = GetAllFlowElements();
            MainCanvas.Children.Clear();

            if (index != -1)
            {
                fe.Insert(index, FE);
            }
            else
                fe.Add(FE);
            ArrangeLinks(fe);
        }
        public void ClearAllFlowElement()
        {
            MainCanvas.Children.Clear();
            GEList.Clear();
            mCurrentFlowElem = null;
        }
        public void RemoveFlowElem(string e, bool removeLink)
        {            
            UIElement FlowEle = null;
            UIElement FlowLink = null;
            foreach (UIElement child in MainCanvas.Children)
            {              
                if (((FrameworkElement)child).Tag.ToString() == e)
                {                                                               
                    if (child.GetType() == typeof(FlowLink))
                        FlowLink = child;
                    else
                        FlowEle = child;                  
                }
            }
            MainCanvas.Children.Remove(FlowEle);
            MainCanvas.Children.Remove(FlowLink);
            GEList.Remove((FlowLink)FlowLink);
            ArrangeLinks();           
        }        
        public void ArrangeLinks(List<FlowElement> fe=null)
        {            
            if(fe==null)
            {
                fe = GetAllFlowElements();
                MainCanvas.Children.Clear();
            }
            FlowElement Prev = null;
            int index = 0;
            int margin = 0;
            foreach (FlowElement child in fe)
            {
                if (Prev == null)
                    Prev = child;
                else
                {
                    LinkFlowElement(Prev, child, index, margin);
                    Prev = child;
                    index = index + 1;
                }
                child.Margin = new Thickness(margin, 0, 0, 0);
                MainCanvas.Children.Add(child);
                margin = margin + 610;
            }
            UpdateConnectorsLayout();
        }
        internal void LinkFlowElement(FlowElement Prev, FlowElement Next, int index,int marginRight)
        {
            foreach (FlowLink child in GEList)
            {
                if (GEList.IndexOf(child) == index)
                {
                    child.Source = Prev;
                    child.Destination = Next;
                    child.Tag = Prev.Tag;
                    child.Margin = new Thickness(0, 0, marginRight, 0);
                    MainCanvas.Children.Add(child);
                }
            }
        }        
       
        public void MoveFlowElement(int oldindex, int newindex)
        {            
            FlowElement ue = null;
            List<FlowElement> fe = new List<FlowElement>();
            fe = GetAllFlowElements();
            MainCanvas.Children.Clear();
            foreach (FlowElement child in fe)
            {
                if (fe.IndexOf(child) == oldindex)
                    ue = child;
            }
            if (ue != null)
                fe.RemoveAt(oldindex);
            fe.Insert(newindex, ue);
            ArrangeLinks(fe);
        }
        public void AddConnector(FlowLink FL)
        {
            GEList.Add(FL);
            FL.Draw();
            MainCanvas.Children.Add(FL);
        }


        private void UpdateConnectorsLayout()
        {
            foreach (FlowLink GE in GEList)
            {
                GE.Draw();
            }
        }

        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FlowElement FE = GetFlowElemFromMosuePosition();

            if (FE != null)
            {
                if (mCurrentFlowElem!= null)
                {
                    mCurrentFlowElem.SetHighLight(false);
                }
                
                FE.SetHighLight(SetHighLight);
                // if Rect connector
                var UIElement = Mouse.DirectlyOver as UIElement;

                if (UIElement is FrameworkElement)
                {
                    FrameworkElement F1 = (FrameworkElement)UIElement;
                    mCurrnetFlowElemControl = F1.Name;
                    if (F1.Name == "TopLeftResizer")
                    {
                        mCurrentFlowElem = FE;
                        mRelativeMousePoint = Mouse.GetPosition(this.MainCanvas);
                        mCurrentFlowElemOriginalMargin = mCurrentFlowElem.Margin;
                        mCurrentFlowElemOriginalWidth = FE.Width;
                        mCurrentFlowElemOriginalHeight = FE.Height;
                        mCurrentAction = CurrentAction.Resize;
                        return;
                    }

                    if (F1.Name == "LeftConnector" || F1.Name == "RightConnector" || F1.Name == "TopConnector" || F1.Name == "BottomConnector")
                    {                        
                            // For Drag Drop 
                            UIElement element = sender as UIElement;
                            if (element == null)
                                return;
                            DragDrop.DoDragDrop(element, new DataObject(FE), DragDropEffects.Link);
                            mCurrentAction = CurrentAction.Link;
                            mCurrentFlowElem = FE;
                            return;
                    }
                }

                // else we move the FE
                mCurrentFlowElem = FE;
                mRelativeMousePoint = Mouse.GetPosition(this.MainCanvas);
                mCurrentAction = CurrentAction.Move;
                mCurrentFlowElemOriginalMargin = mCurrentFlowElem.Margin;
            }

            //TODO: IF we are on canvas and no FE then move the scroll bar like map move
        }

        private FlowElement GetFlowElemFromMosuePosition()
        {
            var UIElement = Mouse.DirectlyOver as UIElement;
            if (UIElement == null) return null;
            var v = VisualTreeHelper.GetParent(UIElement);            
            while (true)
            {
                if (v == null) return null;
                if (v is FlowElement)
                {                    
                    return (FlowElement)v;
                }
                if (v == this)
                {
                    return null;
                }                
                v = VisualTreeHelper.GetParent(v);                
            }
        }

        private void MainCanvas_Drop(object sender, DragEventArgs e)
        {
            FrameworkElement elem = sender as FrameworkElement;
            if (elem == null) return;
            IDataObject data = e.Data;
            if (!data.GetDataPresent(typeof(FlowElement))) return;
            FlowElement source = data.GetData(typeof(FlowElement)) as FlowElement;            
            if (source == null) return;
            
            if (e.Source is FlowElement)
            {
                FlowLink FL = new FlowLink(source, (FlowElement)e.Source);

                FL.LinkStyle = FlowLink.eLinkStyle.Arrow;
                GEList.Add(FL);
                SetFlowLinkConnectorsLocations(FL, e);
                FL.Draw();
                
                MainCanvas.Children.Add(FL);
            }
        }

        private void SetFlowLinkConnectorsLocations(FlowLink FL, DragEventArgs e)
        {
            // We know the source connector the user clicked on
            switch(mCurrnetFlowElemControl)
            {
                case "LeftConnector":
                    FL.SourcePosition = FlowLink.eFlowElementPosition.Left;
                    break;
                case "RightConnector":
                    FL.SourcePosition = FlowLink.eFlowElementPosition.Right;
                    break;
                case "TopConnector":
                    FL.SourcePosition = FlowLink.eFlowElementPosition.Top;
                    break;
                case "BottomConnector":
                    FL.SourcePosition = FlowLink.eFlowElementPosition.bottom;
                    break;
            }

            // TODO: if we plan to enable more than the 4 basic connectors then make a list of connectors

            //Destination - find closest connector to Mouse pos on the dest FlowElem 
            // We use the event args to get posiition since it is not reliable to use Mouse.GetPosition during drag operation
            Point mp1 = e.GetPosition(FL.Destination.TopConnector);
            double d1 = Point.Subtract(mp1, new Point(0, 0)).Length;

            Point mp2 = e.GetPosition(FL.Destination.BottomConnector);
            double d2 = Point.Subtract(mp2, new Point(0, 0)).Length;

            Point mp3 = e.GetPosition(FL.Destination.LeftConnector);
            double d3 = Point.Subtract(mp3, new Point(0, 0)).Length;

            Point mp4 = e.GetPosition(FL.Destination.RightConnector);
            double d4 = Point.Subtract(mp4, new Point(0, 0)).Length;

            double min = d1;                        
            FL.DestinationPosition = FlowLink.eFlowElementPosition.Top;

            if (d2 < min)
            {
                min = d2;
                FL.DestinationPosition = FlowLink.eFlowElementPosition.bottom;
            }

            if (d3 < min)
            {
                min = d3;
                FL.DestinationPosition = FlowLink.eFlowElementPosition.Left;
            }

            if (d4 < min)
            {
                min = d4;
                FL.DestinationPosition = FlowLink.eFlowElementPosition.Right;
            }
        }

        double GetDistanceFrom(Point p, UIElement c)
        {
            var r = VisualTreeHelper.GetContentBounds(c);
            double t = p.X - r.Left +  p.Y - r.Top;            
            return t;
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (Ismovable)
            {
                if (mCurrentAction == CurrentAction.Link)
                {
                    // TODO: highlight target elem if found under mouse cursor
                }

                if (mCurrentAction == CurrentAction.Move)
                {
                    Point MousePoint = Mouse.GetPosition(this.MainCanvas);
                    double XDiff = MousePoint.X - mRelativeMousePoint.X;
                    double YDiff = MousePoint.Y - mRelativeMousePoint.Y;
                    Thickness nm = new Thickness(mCurrentFlowElemOriginalMargin.Left + XDiff, mCurrentFlowElemOriginalMargin.Top + YDiff, mCurrentFlowElemOriginalMargin.Right, mCurrentFlowElemOriginalMargin.Bottom);
                    mCurrentFlowElem.Margin = nm;
                    UpdateConnectorsLayout();
                }

                if (mCurrentAction == CurrentAction.Resize)
                {
                    Point MousePoint = Mouse.GetPosition(this.MainCanvas);
                    double XDiff = MousePoint.X - mRelativeMousePoint.X;
                    double YDiff = MousePoint.Y - mRelativeMousePoint.Y;
                    Thickness nm = new Thickness(mCurrentFlowElemOriginalMargin.Left + XDiff, mCurrentFlowElemOriginalMargin.Top + YDiff, mCurrentFlowElemOriginalMargin.Right, mCurrentFlowElemOriginalMargin.Bottom);
                    mCurrentFlowElem.Margin = nm;

                    if (mCurrentFlowElemOriginalWidth - XDiff > 10)
                    {
                        mCurrentFlowElem.Width = mCurrentFlowElemOriginalWidth - XDiff;
                    }

                    if (mCurrentFlowElemOriginalHeight - YDiff > 10)
                    {
                        mCurrentFlowElem.Height = mCurrentFlowElemOriginalHeight - YDiff;
                    }

                    UpdateConnectorsLayout();
                }
            }
        }

        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mCurrentAction = CurrentAction.None;
        }

        private void MainCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            mCurrentAction = CurrentAction.None;
        }

        public List<FlowElement> GetAllFlowElements()
        {
            List<FlowElement> list = new List<FlowElement>();
            foreach (UIElement e in this.MainCanvas.Children)
            {
                if (e is FrameworkElement)
                {
                    if (e is FlowElement)
                    {
                        list.Add((FlowElement)e);
                    }
                }
            }
            return list;
        }

        // this function is more useful for search if we don't need the full list
        internal IEnumerable<FlowElement> GetAllFlowElem()
        {
            foreach (UIElement e in this.MainCanvas.Children)
            {
                if (e is FrameworkElement)
                {
                    if (e is FlowElement)
                    {
                        yield return (FlowElement)e;                        
                    }
                }
            }
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.MainCanvas == null) return;  // will happen only at page load

            // Set the Canvas scale based on ZoomSlider value
            ScaleTransform ST = new ScaleTransform(e.NewValue, e.NewValue);
            this.MainCanvas.LayoutTransform = ST;

            ZoomPanel.ZoomPercentLabel.Content = (int)(e.NewValue * 100) + "%";
        }

        public void SetView(Brush backgroundBrush, bool showZoomPnl = true, bool showScroll=true)            
        {
            if (!showZoomPnl)
            {
                xZoomRow.Height = new GridLength(0);
            }

            if (!showScroll)
            {
                xScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                xScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            }

            xPageGrid.Background = backgroundBrush;
        }
    }
}