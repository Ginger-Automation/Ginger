#region License
/*
Copyright © 2014-2019 European Support Limited

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

using UIAComWrapperInternal;

namespace System.Windows.Automation
{
    public static class Automation
    {
        private static readonly UIAutomationClient.IUIAutomation factory = new UIAutomationClient.CUIAutomation();
        public static readonly Condition ContentViewCondition = Condition.Wrap(Factory.ContentViewCondition);
        public static readonly Condition ControlViewCondition = Condition.Wrap(Factory.ControlViewCondition);
        public static readonly Condition RawViewCondition = Condition.Wrap(Factory.RawViewCondition);        

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Automation()
        {
        }

        internal static UIAutomationClient.IUIAutomation Factory
        {
            get
            {
                return factory;
            }
        }

        public static void AddAutomationEventHandler(AutomationEvent eventId, AutomationElement element, TreeScope scope, AutomationEventHandler eventHandler)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(eventHandler, "eventHandler");
            Utility.ValidateArgument(eventId != AutomationElement.AutomationFocusChangedEvent, "Use FocusChange notification instead");
            Utility.ValidateArgument(eventId != AutomationElement.StructureChangedEvent, "Use StructureChange notification instead");
            Utility.ValidateArgument(eventId != AutomationElement.AutomationPropertyChangedEvent, "Use PropertyChange notification instead");

            try
            {
                BasicEventListener listener = new BasicEventListener(eventId, element, eventHandler);
                Factory.AddAutomationEventHandler(
                    eventId.Id,
                    element.NativeElement,
                    (UIAutomationClient.TreeScope)scope,
                    CacheRequest.CurrentNativeCacheRequest,
                    listener);
                ClientEventList.Add(listener);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static void AddAutomationFocusChangedEventHandler(AutomationFocusChangedEventHandler eventHandler)
        {
            Utility.ValidateArgumentNonNull(eventHandler, "eventHandler");
            try
            {
                FocusEventListener listener = new FocusEventListener(eventHandler);
                Factory.AddFocusChangedEventHandler(CacheRequest.CurrentNativeCacheRequest, listener);
                ClientEventList.Add(listener);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static void AddAutomationPropertyChangedEventHandler(AutomationElement element, TreeScope scope, AutomationPropertyChangedEventHandler eventHandler, params AutomationProperty[] properties)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(eventHandler, "eventHandler");
            Utility.ValidateArgumentNonNull(properties, "properties");
            if (properties.Length == 0)
            {
                throw new ArgumentException("AtLeastOnePropertyMustBeSpecified");
            }
            int[] propertyIdArray = new int[properties.Length];
            for (int i = 0; i < properties.Length; ++i)
            {
                Utility.ValidateArgumentNonNull(properties[i], "properties");
                propertyIdArray[i] = properties[i].Id;
            }

            try
            {
                PropertyEventListener listener = new PropertyEventListener(AutomationElement.StructureChangedEvent, element, eventHandler);
                Factory.AddPropertyChangedEventHandler(
                    element.NativeElement,
                    (UIAutomationClient.TreeScope)scope,
                    CacheRequest.CurrentNativeCacheRequest,
                    listener,
                    propertyIdArray);
                ClientEventList.Add(listener);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static void AddStructureChangedEventHandler(AutomationElement element, TreeScope scope, StructureChangedEventHandler eventHandler)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(eventHandler, "eventHandler");
            Utility.ValidateArgumentNonNull(eventHandler, "eventHandler");

            try
            {
                StructureEventListener listener = new StructureEventListener(AutomationElement.StructureChangedEvent, element, eventHandler);
                Factory.AddStructureChangedEventHandler(
                    element.NativeElement,
                    (UIAutomationClient.TreeScope)scope,
                    CacheRequest.CurrentNativeCacheRequest,
                    listener);
                ClientEventList.Add(listener);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static bool Compare(int[] runtimeId1, int[] runtimeId2)
        {
            if (runtimeId1 == null && runtimeId2 == null)
            {
                return true;
            }
            if (runtimeId1 == null || runtimeId2 == null)
            {
                return false;
            }
            try
            {
                return Factory.CompareRuntimeIds(runtimeId1, runtimeId2) != 0;
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static bool Compare(AutomationElement el1, AutomationElement el2)
        {
            if (el1 == null && el2 == null)
            {
                return true;
            }
            if (el1 == null || el2 == null)
            {
                return false;
            }
            try
            {
                return Factory.CompareElements(el1.NativeElement, el2.NativeElement) != 0;
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static string PatternName(AutomationPattern pattern)
        {
            Utility.ValidateArgumentNonNull(pattern, "pattern");
            return Factory.GetPatternProgrammaticName(pattern.Id);
        }

        public static string PropertyName(AutomationProperty property)
        {
            Utility.ValidateArgumentNonNull(property, "property");
            return Factory.GetPropertyProgrammaticName(property.Id);
        }

        public static void RemoveAllEventHandlers()
        {
            try
            {
                Factory.RemoveAllEventHandlers();
                ClientEventList.Clear();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static void RemoveAutomationEventHandler(AutomationEvent eventId, AutomationElement element, AutomationEventHandler eventHandler)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(eventHandler, "eventHandler");
            Utility.ValidateArgument(eventId != AutomationElement.AutomationFocusChangedEvent, "Use FocusChange notification instead");
            Utility.ValidateArgument(eventId != AutomationElement.StructureChangedEvent, "Use StructureChange notification instead");
            Utility.ValidateArgument(eventId != AutomationElement.AutomationPropertyChangedEvent, "Use PropertyChange notification instead");

            try
            {
                BasicEventListener listener = (BasicEventListener)ClientEventList.Remove(eventId, element, eventHandler);
                Factory.RemoveAutomationEventHandler(eventId.Id, element.NativeElement, listener);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static void RemoveAutomationFocusChangedEventHandler(AutomationFocusChangedEventHandler eventHandler)
        {
            Utility.ValidateArgumentNonNull(eventHandler, "eventHandler");

            try
            {
                FocusEventListener listener = (FocusEventListener)ClientEventList.Remove(AutomationElement.AutomationFocusChangedEvent, null, eventHandler);
                Factory.RemoveFocusChangedEventHandler(listener);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static void RemoveAutomationPropertyChangedEventHandler(AutomationElement element, AutomationPropertyChangedEventHandler eventHandler)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(eventHandler, "eventHandler");
            
            try
            {
                PropertyEventListener listener = (PropertyEventListener)ClientEventList.Remove(AutomationElement.AutomationPropertyChangedEvent, element, eventHandler);
                Factory.RemovePropertyChangedEventHandler(element.NativeElement, listener);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static void RemoveStructureChangedEventHandler(AutomationElement element, StructureChangedEventHandler eventHandler)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(eventHandler, "eventHandler");

            try
            {
                StructureEventListener listener = (StructureEventListener)ClientEventList.Remove(AutomationElement.StructureChangedEvent, element, eventHandler);
                Factory.RemoveStructureChangedEventHandler(element.NativeElement, listener);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }
    }
}
