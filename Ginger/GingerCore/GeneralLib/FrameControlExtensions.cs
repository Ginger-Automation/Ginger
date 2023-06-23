using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GingerCore.GeneralLib
{
    public static class FrameControlExtensions
    {
        /// <summary>
        /// Clear all entries from the Frame's back history.
        /// </summary>
        /// <param name="thisFrame">Frame control to clear the back entries from.</param>
        public static void ClearAllBackEntries(this Frame thisFrame)
        {
            if (!thisFrame.CanGoBack && !thisFrame.CanGoForward)
                return;

            while (thisFrame.RemoveBackEntry() != null);
        }

        /// <summary>
        /// Clear all entries from the the Frame's back history and set the new content.
        /// </summary>
        /// <param name="thisFrame">Frame control to clear the back entries and set new content.</param>
        /// <param name="content">New content that will be set to this Frame.</param>
        public static void ClearAndSetContent(this Frame thisFrame, object content)
        {
            ClearAllBackEntries(thisFrame);
            thisFrame.Content = content;
        }
    }
}
