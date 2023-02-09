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


namespace GingerCore.Actions.VisualTesting
{
    public enum eVisualTestingVisibility : byte
    {
        //
        // Summary:
        //     Display the element.
        Visible = 0,
        //
        // Summary:
        //     Do not display the element, but reserve space for the element in layout.
        Hidden = 1,
        //
        // Summary:
        //     Do not display the element, and do not reserve space for it in layout.
        Collapsed = 2
    }

    public class VisualCompareAnalyzerIntegration
    {        
        public event VisualTestingEventHandler VisualTestingEvent;
        public delegate void VisualTestingEventHandler(VisualTestingEventArgs EventArgs);

        public void OnVisualTestingEvent(VisualTestingEventArgs.eEventType EvType, eVisualTestingVisibility visibility)
        {
            VisualTestingEventHandler handler = VisualTestingEvent;
            if (handler != null)
            {
                handler(new VisualTestingEventArgs(EvType, visibility));
            }
        }
    }

    public class VisualTestingEventArgs
    {
        public eEventType EventType;
        public eVisualTestingVisibility visibility;
        public enum eEventType
        {
            SetTargetSectionVisibility,
            SetBaselineSectionVisibility,
            SetResultsSectionVisibility,
            SetScreenSizeSelectionVisibility,
        }

        public VisualTestingEventArgs(eEventType EventType, eVisualTestingVisibility visibility)
        {
            this.EventType = EventType;
            this.visibility = visibility;
        }
    }
}
