using Ginger.AdornerLib;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace GingerTest
{
    public class HighlightAdorner : Adorner
    {
        // UIElement mAdornedElement;
        // To store and manage the adorner's visual children
        VisualCollection visualChildren;

        // Custom UIElement to add to the target UIElement
        HighlightAdornerControl highlighterAdorner = new HighlightAdornerControl();

        public HighlightAdorner(UIElement adornedElement) :
        base(adornedElement)
        {            
            visualChildren = new VisualCollection(this);
            highlighterAdorner.Opacity = 0.20;         
            visualChildren.Add(highlighterAdorner);            
            var layer = AdornerLayer.GetAdornerLayer(adornedElement);
            layer.Add(this);
            adornedElement.InvalidateVisual();          
        }


        // Arrange the Adorner
        protected override Size ArrangeOverride(Size finalSize)
        {
            double desiredWidth = AdornedElement.DesiredSize.Width + 10;
            double desiredHeight = AdornedElement.DesiredSize.Height + 10;

            highlighterAdorner.Arrange(new Rect(-10, -10, desiredWidth, desiredHeight));
            
            return finalSize;
        }

        protected override int VisualChildrenCount { get { return visualChildren.Count; } }
        protected override Visual GetVisualChild(int index) { return visualChildren[index]; }

    }
}
