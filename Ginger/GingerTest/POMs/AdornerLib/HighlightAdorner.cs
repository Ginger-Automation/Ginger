using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace GingerTest.POMs.AdornerLib
{
    public class HighlightAdorner : Adorner
    {
        // UIElement mAdornedElement;
        // To store and manage the adorner's visual children
        VisualCollection visualChildren;

        // Custom UIElement to add to the target UIElement
        Button button = new Button();

        public HighlightAdorner(UIElement adornedElement) :
        base(adornedElement)
        {
            
            //// ... other init
            visualChildren = new VisualCollection(this);
            button.Height = 50.0;
            button.Width = 50.0;
            button.Content = "<--------------------";
            button.Background = Brushes.Red;

            visualChildren.Add(button);


            var layer = AdornerLayer.GetAdornerLayer(adornedElement);
            layer.Add(this);

            adornedElement.InvalidateVisual();
            // InvalidateVisual();
            //mAdornedElement = adornedElement;
        }

        //protected override void OnRender(DrawingContext drawingContext)
        //{
        //    base.OnRender(drawingContext);

        //    Pen pen = new Pen(Brushes.Transparent, 3);

        //    drawingContext.DrawRoundedRectangle(Brushes.Yellow, pen, new Rect(0, 0, AdornedElement.DesiredSize.Width + 5, AdornedElement.DesiredSize.Height + 5), 3, 3);



        //    //// ... add custom rendering code here ...
        //}


        // Arrange the Adorner
        protected override Size ArrangeOverride(Size finalSize)
        {
            double desiredWidth = AdornedElement.DesiredSize.Width;
            double desiredHeight = AdornedElement.DesiredSize.Height;

            button.Arrange(new Rect((button.Width + desiredWidth) / 2, -desiredHeight,
                                 desiredWidth, desiredHeight));
            return finalSize;
        }

        protected override int VisualChildrenCount { get { return visualChildren.Count; } }
        protected override Visual GetVisualChild(int index) { return visualChildren[index]; }

    }
}
