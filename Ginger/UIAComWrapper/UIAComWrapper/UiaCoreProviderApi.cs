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

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Automation.Providers;
using UIAutomationClient;

namespace UIAComWrapperInternal
{
    /// <summary>
    /// P/Invoke definitions for flat UI Automation Core functions
    /// </summary>
    internal static class UiaCoreProviderApi
    {
        // Methods
        private static void CheckError(int hr)
        {
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("UIAutomationCore.dll", EntryPoint = "UiaClientsAreListening", CharSet = CharSet.Unicode)]
        private static extern bool RawUiaClientsAreListening();

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("UIAutomationCore.dll", EntryPoint = "UiaHostProviderFromHwnd", CharSet = CharSet.Unicode)]
        private static extern int RawUiaHostProviderFromHwnd(IntPtr hwnd, [MarshalAs(UnmanagedType.Interface)] out IRawElementProviderSimple provider);

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("UIAutomationCore.dll", EntryPoint = "UiaRaiseAsyncContentLoadedEvent", CharSet = CharSet.Unicode)]
        private static extern int RawUiaRaiseAsyncContentLoadedEvent(IRawElementProviderSimple provider, System.Windows.Automation.AsyncContentLoadedState asyncContentLoadedState, double PercentComplete);
        
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("UIAutomationCore.dll", EntryPoint = "UiaRaiseAutomationEvent", CharSet = CharSet.Unicode)]
        private static extern int RawUiaRaiseAutomationEvent(IRawElementProviderSimple provider, int id);
        
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("UIAutomationCore.dll", EntryPoint = "UiaRaiseAutomationPropertyChangedEvent", CharSet = CharSet.Unicode)]
        private static extern int RawUiaRaiseAutomationPropertyChangedEvent(IRawElementProviderSimple provider, int id, object oldValue, object newValue);
        
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("UIAutomationCore.dll", EntryPoint = "UiaRaiseStructureChangedEvent", CharSet = CharSet.Unicode)]
        private static extern int RawUiaRaiseStructureChangedEvent(IRawElementProviderSimple provider, StructureChangeType structureChangeType, int[] runtimeId, int runtimeIdLen);
        
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("UIAutomationCore.dll", EntryPoint = "UiaReturnRawElementProvider", CharSet = CharSet.Unicode)]
        private static extern IntPtr RawUiaReturnRawElementProvider(IntPtr hwnd, IntPtr wParam, IntPtr lParam, IRawElementProviderSimple el);

        [SecurityCritical, SecuritySafeCritical]
        internal static bool UiaClientsAreListening()
        {
            return RawUiaClientsAreListening();
        }

        [SecuritySafeCritical, SecurityCritical]
        internal static IRawElementProviderSimple UiaHostProviderFromHwnd(IntPtr hwnd)
        {
            IRawElementProviderSimple simple;
            CheckError(RawUiaHostProviderFromHwnd(hwnd, out simple));
            return simple;
        }

        [SecuritySafeCritical, SecurityCritical]
        internal static void UiaRaiseAsyncContentLoadedEvent(IRawElementProviderSimple provider, System.Windows.Automation.AsyncContentLoadedState asyncContentLoadedState, double PercentComplete)
        {
            CheckError(RawUiaRaiseAsyncContentLoadedEvent(provider, asyncContentLoadedState, PercentComplete));
        }

        [SecurityCritical, SecuritySafeCritical]
        internal static void UiaRaiseAutomationEvent(IRawElementProviderSimple provider, int eventId)
        {
            CheckError(RawUiaRaiseAutomationEvent(provider, eventId));
        }

        [SecurityCritical, SecuritySafeCritical]
        internal static void UiaRaiseAutomationPropertyChangedEvent(IRawElementProviderSimple provider, int propertyId, object oldValue, object newValue)
        {
            CheckError(RawUiaRaiseAutomationPropertyChangedEvent(provider, propertyId, oldValue, newValue));
        }

        [SecuritySafeCritical, SecurityCritical]
        internal static void UiaRaiseStructureChangedEvent(IRawElementProviderSimple provider, StructureChangeType structureChangeType, int[] runtimeId)
        {
            CheckError(RawUiaRaiseStructureChangedEvent(provider, structureChangeType, runtimeId, (runtimeId == null) ? 0 : runtimeId.Length));
        }

        [SecurityCritical, SecuritySafeCritical]
        internal static IntPtr UiaReturnRawElementProvider(IntPtr hwnd, IntPtr wParam, IntPtr lParam, IRawElementProviderSimple el)
        {
            return RawUiaReturnRawElementProvider(hwnd, wParam, lParam, el);
        }
    }
}
