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
using GingerCore;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ginger.UserControlsLib.VisualFlow
{
    /// <summary>
    /// Interaction logic for GraphEdge.xaml
    /// </summary>
    public partial class FlowLink : UserControl
    {
        public enum eLinkStyle
        {
            Arrow,
            Line,
            DottedArrow,
            DataArrow
        }

        public enum eFlowElementPosition
        {
            Left,
            Top,
            Right,
            bottom,
            ByName
        }

        public eLinkStyle LinkStyle { get; set; }

        public eFlowElementPosition FlowElementPosition { get; set; }

        public FlowElement Source { get; set; }
        public eFlowElementPosition SourcePosition { get; set; }

        public FlowElement Destination { get; set; }
        public eFlowElementPosition DestinationPosition { get; set; }
        public string DestinationConnectorName { get; set; }

        Path path = new Path();

        public FlowLink(FlowElement Source, FlowElement Destination,bool fromrunTab=false)
        {
            this.Source = Source;
            this.Destination = Destination;
            if(!fromrunTab)
            {
                this.MouseLeftButtonDown += GraphEdge_MouseLeftButtonDown;
            }            
        }

        private void GraphEdge_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //TODO: move to Canvas to handle
            SetHighlight(true);                    
            Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Conn click");
        }

        public void Draw()
        {
            switch (LinkStyle)
            {
                case eLinkStyle.Line:
                    this.Content = DrawLine();
                    break;
                case eLinkStyle.Arrow:
                    this.Content = DrawLinkArrow();
                    break;
                case eLinkStyle.DottedArrow:
                    Shape shape = DrawLinkArrow();
                    DoubleCollection da = new DoubleCollection();
                    da.Add(5);
                    da.Add(2);
                    shape.StrokeDashArray = da;
                    
                    this.Content = path;
                    break;
                case eLinkStyle.DataArrow:
                    Shape shape2 = DrawLinkArrow();
                    DoubleCollection da2 = new DoubleCollection();
                    // shape2.Fill = Brushes.Orange;
                    shape2.Stroke = Brushes.Orange;
                    da2.Add(10);
                    da2.Add(3);
                    shape2.StrokeDashArray = da2;
                    shape2.StrokeDashCap = PenLineCap.Triangle;
                    this.Content = path;
                    break;
                default:
                    LinkStyle = eLinkStyle.Arrow;
                    DrawLine();
                    break;
            }
        }

        private Shape DrawLine()
        {
            Point p1 = GetP1(); //Point(Source.Margin.Left, Source.Margin.Top);
            Point p2 = GetP2(); //new Point(Destination.Margin.Left, Destination.Margin.Top);

            path.StrokeThickness = 2;
            path.Stroke = Brushes.Black;
            path.MinWidth = 1;
            path.MinHeight = 1;
            
            //// Now we set the path to draw a line from source to destination
            string PathData = "";
            PathData += "M " + p1.X + "," + p1.Y;   // M is Move to abs point
            PathData += " L " + p2.X + "," + p2.Y;  // L is draw a line
            path.Data = Geometry.Parse(PathData);  
            return path;
        }

        private Shape DrawLinkArrow()
        {
            Point p1 = GetP1(); //Point(Source.Margin.Left, Source.Margin.Top);
            Point p2 = GetP2(); //new Point(Destination.Margin.Left, Destination

            GeometryGroup lineGroup = new GeometryGroup();
            double theta = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            Point p = new Point(p1.X + ((p2.X - p1.X) / 1.15), p1.Y + ((p2.Y - p1.Y) / 1.15));
            pathFigure.StartPoint = p;

            Point lpoint = new Point(p.X + 6, p.Y + 15);
            Point rpoint = new Point(p.X - 6, p.Y + 15);
            LineSegment seg1 = new LineSegment();
            seg1.Point = lpoint;
            pathFigure.Segments.Add(seg1);

            LineSegment seg2 = new LineSegment();
            seg2.Point = rpoint;
            pathFigure.Segments.Add(seg2);

            LineSegment seg3 = new LineSegment();
            seg3.Point = p;
            pathFigure.Segments.Add(seg3);

            pathGeometry.Figures.Add(pathFigure);
            RotateTransform transform = new RotateTransform();
            transform.Angle = theta + 90;
            transform.CenterX = p.X;
            transform.CenterY = p.Y;
            pathGeometry.Transform = transform;
            lineGroup.Children.Add(pathGeometry);

            LineGeometry connectorGeometry = new LineGeometry();
            connectorGeometry.StartPoint = p1;
            connectorGeometry.EndPoint = p2;
            lineGroup.Children.Add(connectorGeometry);
            path.Data = lineGroup;
            path.StrokeThickness = 2;
            path.Stroke = path.Fill = Brushes.Black;

            return path;
        }

        Point GetP1()
        {
            Point p1 = new Point(Source.Margin.Left, Source.Margin.Top);
            switch (SourcePosition)
            {
                case eFlowElementPosition.Top:
                    p1.X += Source.Width / 2;
                    break;
                case eFlowElementPosition.Left:
                    p1.Y += Source.Height / 2;
                    break;
                case eFlowElementPosition.bottom:
                    p1.X += Source.Width / 2;
                    p1.Y += Source.Height;
                    break;
                case eFlowElementPosition.Right:
                    p1.X += Source.Width;
                    p1.Y += Source.Height / 2;
                    break;
            }
            return p1;
        }

        Point GetP2()
        {
            if (Destination == null) return new Point(0,0); // temp to fix null dest
            Point p2 = new Point(Destination.Margin.Left, Destination.Margin.Top);
            switch (DestinationPosition)
            {
                case eFlowElementPosition.Top:
                    p2.X += Destination.Width / 2;
                    break;
                case eFlowElementPosition.Left:
                    p2.Y += Destination.Height / 2;
                    break;
                case eFlowElementPosition.bottom:
                    p2.X += Destination.Width / 2;
                    p2.Y += Destination.Height;
                    break;
                case eFlowElementPosition.Right:
                    p2.X += Destination.Width;
                    p2.Y += Destination.Height / 2;
                    break;

                case eFlowElementPosition.ByName:
                    Point pc = Destination.GetConnectorPointByTagName(DestinationConnectorName);
                    p2.X += pc.X;
                    p2.Y += pc.Y;
                    break;
            }
            return p2;
         }

        public void SetHighlight(bool visible)
        {
            if (visible)
            {
                path.Stroke = Brushes.Purple;
                path.StrokeThickness = 3;
            }
        }
       }
   }
