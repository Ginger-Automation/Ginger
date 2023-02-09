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

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ginger.UserControlsLib.TextEditor.Common
{
    public class BackgroundRenderer : IBackgroundRenderer
    {
        public List<ISegment> Segments = new List<ISegment>();

        private ICSharpCode.AvalonEdit.TextEditor _editor;
        
        public DocumentLine HighLightLine {get; set;}

        public KnownLayer Layer { get { return KnownLayer.Caret; } }

        public BackgroundRenderer(ICSharpCode.AvalonEdit.TextEditor editor)
        {
            _editor = editor;
        }

        Pen mRedPen;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (textView.ActualWidth == 0) return;

            textView.EnsureVisualLines();
            if (HighLightLine != null)
            {
                try
                {
                    foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, HighLightLine))
                    {
                        drawingContext.DrawRectangle(
                            new SolidColorBrush(Color.FromArgb(0x40, 0, 0, 0xFF)), null,
                            new Rect(rect.Location, new Size(textView.ActualWidth, rect.Height)));
                    }
                }
                catch
                {
                    //Do Nothing - can happen when lines are deleted but grid still exist and shows rows
                }
            }

            // Draw line marker - this one is simple red line below the text
            foreach (ISegment seg in Segments)
            {                
                IEnumerable<Rect> rects = BackgroundGeometryBuilder.GetRectsForSegment(textView, seg);
                try
                {
                    foreach (var rect in rects)
                    {
                        drawingContext.DrawLine(GetRedPen(), rect.BottomLeft, rect.BottomRight);
                        // We can draw other shapes too like below 
                    }
                }
                catch
                {
                    //do nothing - can happen when lines are deleted but grid still exist and shows rows
                }
            }
        }

        private Pen GetRedPen()
        {
            if (mRedPen == null)
            {
                mRedPen = new Pen(Brushes.Red, 2);
                mRedPen.Brush = getVB();
            }
            return mRedPen;
        }

        VisualBrush getVB()
        {
            //TODO: improve it to look like zigzag wabbely line
            Path p = new Path();
            p.Data = Geometry.Parse("M 0,2 L 2,0 4,2 6,0 8,2 10,0 12,2");                
            p.Stroke = Brushes.Red;
            VisualBrush VB = new VisualBrush(p);            
            return VB;
        }
    }
}
