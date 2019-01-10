using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GingerControls
{
    public static class TreeViewItemExtensions
    {
        // Get depth of tree view item
        public static int GetDepth(this TreeViewItem item)
        {
            DependencyObject target = item;
            var depth = 0;
            while (target != null)
            {
                if (target is TreeView)
                    return depth;
                if (target is TreeViewItem)
                    depth++;

                target = VisualTreeHelper.GetParent(target);
            }
            return 0;
        }
    }
}
