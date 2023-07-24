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

namespace System.Windows.Automation.Providers
{
    public static class AutomationInteropProvider
    {
        // Constants
        public const int AppendRuntimeId = 3;
        public const int InvalidateLimit = 20;
        public const int RootObjectId = -25;

        public static IRawElementProviderSimple HostProviderFromHandle(IntPtr hwnd)
        {
            Utility.ValidateArgument(hwnd != IntPtr.Zero, "HWND must not be null");
            return UiaCoreProviderApi.UiaHostProviderFromHwnd(hwnd);
        }

        public static void RaiseAutomationEvent(AutomationEventExtended eventId, IRawElementProviderSimple provider, AutomationEventArgsExtended e)
        {
            Utility.ValidateArgumentNonNull(eventId, "eventId");
            Utility.ValidateArgumentNonNull(provider, "provider");
            Utility.ValidateArgumentNonNull(e, "e");
            if (e.EventId == AutomationElementIdentifiersExtended.AsyncContentLoadedEvent)
            {
                AsyncContentLoadedEventArgs args = e as AsyncContentLoadedEventArgs;
                if (args == null)
                {
                    throw new ArgumentException("e");
                }
                UiaCoreProviderApi.UiaRaiseAsyncContentLoadedEvent(provider, args.AsyncContentLoadedState, args.PercentComplete);
            }
            else
            {
                if ((e.EventId == WindowPatternIdentifiers.WindowClosedEvent) && !(e is WindowClosedEventArgs))
                {
                    throw new ArgumentException("e");
                }
                UiaCoreProviderApi.UiaRaiseAutomationEvent(provider, eventId.Id);
            }
        }

        public static void RaiseAutomationPropertyChangedEvent(IRawElementProviderSimple element, AutomationPropertyChangedEventArgsExtended e)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(e, "e");
            UiaCoreProviderApi.UiaRaiseAutomationPropertyChangedEvent(element, e.Property.Id, e.OldValue, e.NewValue);
        }

        public static void RaiseStructureChangedEvent(IRawElementProviderSimple provider, StructureChangedEventArgsExtended e)
        {
            Utility.ValidateArgumentNonNull(provider, "provider");
            Utility.ValidateArgumentNonNull(e, "e");
            UiaCoreProviderApi.UiaRaiseStructureChangedEvent(provider, (UIAutomationClient.StructureChangeType)e.StructureChangeType, e.GetRuntimeId());
        }

        public static IntPtr ReturnRawElementProvider(IntPtr hwnd, IntPtr wParam, IntPtr lParam, IRawElementProviderSimple el)
        {
            Utility.ValidateArgument(hwnd != IntPtr.Zero, "HWND must not be null");
            Utility.ValidateArgumentNonNull(el, "el");
            return UiaCoreProviderApi.UiaReturnRawElementProvider(hwnd, wParam, lParam, el);
        }

        public static bool ClientsAreListening
        {
            get
            {
                return UiaCoreProviderApi.UiaClientsAreListening();
            }
        }
    }
}
