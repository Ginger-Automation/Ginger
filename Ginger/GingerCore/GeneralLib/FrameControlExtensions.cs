#region License
/*
Copyright © 2014-2026 European Support Limited

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

using System.Windows.Controls;
using System.Windows.Navigation;

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
            if (!thisFrame.NavigationService.CanGoBack && !thisFrame.NavigationService.CanGoForward)
            {
                return;
            }

            JournalEntry lastEntry;
            do
            {
                lastEntry = thisFrame.NavigationService.RemoveBackEntry();
            } while (lastEntry != null);
        }

        /// <summary>
        /// Clear all entries from the the Frame's back history and set the new content.
        /// </summary>
        /// <param name="thisFrame">Frame control to clear the back entries and set new content.</param>
        /// <param name="content">New content that will be set to this Frame.</param>
        public static void ClearAndSetContent(this Frame thisFrame, object content)
        {
            thisFrame.Content = content;
            ClearAllBackEntries(thisFrame);
        }
    }
}
