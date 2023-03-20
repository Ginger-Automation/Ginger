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

using UIAComWrapperInternal;

namespace System.Windows.Automation
{
    public static class AutomationExtended
    {
        private static readonly UIAutomationClient.IUIAutomation factory = new UIAutomationClient.CUIAutomation();
        public static readonly ConditionExtended ContentViewCondition = ConditionExtended.Wrap(Factory.ContentViewCondition);
        public static readonly ConditionExtended ControlViewCondition = ConditionExtended.Wrap(Factory.ControlViewCondition);
        public static readonly ConditionExtended RawViewCondition = ConditionExtended.Wrap(Factory.RawViewCondition);        

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static AutomationExtended()
        {
        }

        internal static UIAutomationClient.IUIAutomation Factory
        {
            get
            {
                return factory;
            }
        }

        public static void AddAutomationEventHandler(AutomationEventExtended eventId, AutomationElement_Extend element, TreeScopeExtended scope, AutomationEventHandler eventHandler)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(eventHandler, "eventHandler");
            Utility.ValidateArgument(eventId != AutomationElement_Extend.AutomationFocusChangedEvent, "Use FocusChange notification instead");
            Utility.ValidateArgument(eventId != AutomationElement_Extend.StructureChangedEvent, "Use StructureChange notification instead");
            Utility.ValidateArgument(eventId != AutomationElement_Extend.AutomationPropertyChangedEvent, "Use PropertyChange notification instead");

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

        public static void AddAutomationPropertyChangedEventHandler(AutomationElement_Extend element, TreeScopeExtended scope, AutomationPropertyChangedEventHandler eventHandler, params AutomationPropertyExtended[] properties)
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
                PropertyEventListener listener = new PropertyEventListener(AutomationElement_Extend.StructureChangedEvent, element, eventHandler);
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

        public static void AddStructureChangedEventHandler(AutomationElement_Extend element, TreeScopeExtended scope, StructureChangedEventHandler eventHandler)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(eventHandler, "eventHandler");
            Utility.ValidateArgumentNonNull(eventHandler, "eventHandler");

            try
            {
                StructureEventListener listener = new StructureEventListener(AutomationElement_Extend.StructureChangedEvent, element, eventHandler);
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

        public static bool Compare(AutomationElement_Extend el1, AutomationElement_Extend el2)
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

        public static string PatternName(AutomationPatternExtended pattern)
        {
            Utility.ValidateArgumentNonNull(pattern, "pattern");
            return Factory.GetPatternProgrammaticName(pattern.Id);
        }

        public static string PropertyName(AutomationPropertyExtended property)
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

        public static void RemoveAutomationEventHandler(AutomationEventExtended eventId, AutomationElement_Extend element, AutomationEventHandler eventHandler)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(eventHandler, "eventHandler");
            Utility.ValidateArgument(eventId != AutomationElement_Extend.AutomationFocusChangedEvent, "Use FocusChange notification instead");
            Utility.ValidateArgument(eventId != AutomationElement_Extend.StructureChangedEvent, "Use StructureChange notification instead");
            Utility.ValidateArgument(eventId != AutomationElement_Extend.AutomationPropertyChangedEvent, "Use PropertyChange notification instead");

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
                FocusEventListener listener = (FocusEventListener)ClientEventList.Remove(AutomationElement_Extend.AutomationFocusChangedEvent, null, eventHandler);
                Factory.RemoveFocusChangedEventHandler(listener);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static void RemoveAutomationPropertyChangedEventHandler(AutomationElement_Extend element, AutomationPropertyChangedEventHandler eventHandler)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(eventHandler, "eventHandler");
            
            try
            {
                PropertyEventListener listener = (PropertyEventListener)ClientEventList.Remove(AutomationElement_Extend.AutomationPropertyChangedEvent, element, eventHandler);
                Factory.RemovePropertyChangedEventHandler(element.NativeElement, listener);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static void RemoveStructureChangedEventHandler(AutomationElement_Extend element, StructureChangedEventHandler eventHandler)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(eventHandler, "eventHandler");

            try
            {
                StructureEventListener listener = (StructureEventListener)ClientEventList.Remove(AutomationElement_Extend.StructureChangedEvent, element, eventHandler);
                Factory.RemoveStructureChangedEventHandler(element.NativeElement, listener);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }
    }
}
