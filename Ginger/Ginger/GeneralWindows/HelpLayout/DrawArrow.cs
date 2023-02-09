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

using System;
using System.Windows;
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
            
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            Point p = new Point(p2.X, p2.Y);
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

            double theta = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;
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
    }
}
