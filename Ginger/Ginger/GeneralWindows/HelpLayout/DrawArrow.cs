using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ginger.GeneralWindows.HelpLayout
{
    public sealed class DrawArrow 
    {
        /// <summary>
        /// Draw arrow between 2 points
        /// </summary>
        /// <param name="p1">Source Point</param>
        /// <param name="p2">Target Point</param>
        /// <returns></returns>
        public static Shape DrawLinkArrow(Point p1, Point p2)
        {
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
            Path path = new Path();
            path.Data = lineGroup;
            path.StrokeThickness = 2;
            path.Stroke = path.Fill = Brushes.White;

            return path;
        }

        //Point GetP1(Control sourceControl, Control targetControl)
        //{
        //    Point p1 = new Point(sourceControl.Margin.Left, sourceControl.Margin.Top);
        //  if (sourceControl.)
        //            p1.X += Source.Width / 2;
        //            break;
        //        case eFlowElementPosition.Left:
        //            p1.Y += Source.Height / 2;
        //            break;
        //        case eFlowElementPosition.bottom:
        //            p1.X += Source.Width / 2;
        //            p1.Y += Source.Height;
        //            break;
        //        case eFlowElementPosition.Right:
        //            p1.X += Source.Width;
        //            p1.Y += Source.Height / 2;
        //            break;
        //    }
        //    return p1;
        //}

        //Point GetP2(Control sourceControl, Control targetControl)
        //{
        //    if (Destination == null) return new Point(0, 0); // temp to fix null dest
        //    Point p2 = new Point(Destination.Margin.Left, Destination.Margin.Top);
        //    switch (DestinationPosition)
        //    {
        //        case eFlowElementPosition.Top:
        //            p2.X += Destination.Width / 2;
        //            break;
        //        case eFlowElementPosition.Left:
        //            p2.Y += Destination.Height / 2;
        //            break;
        //        case eFlowElementPosition.bottom:
        //            p2.X += Destination.Width / 2;
        //            p2.Y += Destination.Height;
        //            break;
        //        case eFlowElementPosition.Right:
        //            p2.X += Destination.Width;
        //            p2.Y += Destination.Height / 2;
        //            break;

        //        case eFlowElementPosition.ByName:
        //            Point pc = Destination.GetConnectorPointByTagName(DestinationConnectorName);
        //            p2.X += pc.X;
        //            p2.Y += pc.Y;
        //            break;
        //    }
        //    return p2;
        //}
    }
}
