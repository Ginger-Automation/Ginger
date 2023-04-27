using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Ginger.ConflictResolve
{
    public static class GetListOfChildControls
    {
        public static List<FrameworkElement> GetAllControls(DependencyObject parent)
        {
            var controls = new List<FrameworkElement>();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is FrameworkElement)
                {
                    controls.Add(child as FrameworkElement);
                }

                controls.AddRange(GetAllControls(child));
            }

            return controls;
        }
    }
}
