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

using System.Diagnostics;
using UIAComWrapperInternal;

namespace System.Windows.Automation
{
    public sealed class TreeWalkerExtended
    {
        private UIAutomationClient.IUIAutomationTreeWalker _obj;
        public static readonly TreeWalkerExtended ContentViewWalker = new TreeWalkerExtended(System.Windows.Automation.AutomationExtended.ContentViewCondition);
        public static readonly TreeWalkerExtended ControlViewWalker = new TreeWalkerExtended(System.Windows.Automation.AutomationExtended.ControlViewCondition);
        public static readonly TreeWalkerExtended RawViewWalker = new TreeWalkerExtended(System.Windows.Automation.AutomationExtended.RawViewCondition);
        
        public TreeWalkerExtended(ConditionExtended condition)
        {
            // This is an unusual situation - a direct constructor.
            // We have to go create the native tree walker, which might throw.
            Utility.ValidateArgumentNonNull(condition, "condition");
            try
            {
                this._obj = AutomationExtended.Factory.CreateTreeWalker(condition.NativeCondition);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        internal TreeWalkerExtended(UIAutomationClient.IUIAutomationTreeWalker obj)
        {
            Debug.Assert(obj != null);
            _obj = obj;
        }

        internal TreeWalkerExtended Wrap(UIAutomationClient.IUIAutomationTreeWalker obj)
        {
            return (obj == null) ? null : Wrap(obj);
        }

        public AutomationElement_Extend GetFirstChild(AutomationElement_Extend element)
        {
            return GetFirstChild(element, CacheRequest.Current);
        }

        public AutomationElement_Extend GetFirstChild(AutomationElement_Extend element, CacheRequest request)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(request, "request");
            try
            {
                return AutomationElement_Extend.Wrap(this._obj.GetFirstChildElementBuildCache(
                    element.NativeElement,
                    request.NativeCacheRequest));
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public AutomationElement_Extend GetLastChild(AutomationElement_Extend element)
        {
            return GetLastChild(element, CacheRequest.Current);
        }

        public AutomationElement_Extend GetLastChild(AutomationElement_Extend element, CacheRequest request)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(request, "request");
            try
            {
                return AutomationElement_Extend.Wrap(this._obj.GetLastChildElementBuildCache(
                    element.NativeElement,
                    request.NativeCacheRequest));
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public AutomationElement_Extend GetNextSibling(AutomationElement_Extend element)
        {
            return GetNextSibling(element, CacheRequest.Current);
        }

        public AutomationElement_Extend GetNextSibling(AutomationElement_Extend element, CacheRequest request)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(request, "request");
            try
            {
                return AutomationElement_Extend.Wrap(this._obj.GetNextSiblingElementBuildCache(
                    element.NativeElement,
                    request.NativeCacheRequest));
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public AutomationElement_Extend GetParent(AutomationElement_Extend element)
        {
            return GetParent(element, CacheRequest.Current);
        }

        public AutomationElement_Extend GetParent(AutomationElement_Extend element, CacheRequest request)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(request, "request");
            try
            {
                return AutomationElement_Extend.Wrap(this._obj.GetParentElementBuildCache(
                    element.NativeElement,
                    request.NativeCacheRequest));
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public AutomationElement_Extend GetPreviousSibling(AutomationElement_Extend element)
        {
            return GetPreviousSibling(element, CacheRequest.Current);
        }

        public AutomationElement_Extend GetPreviousSibling(AutomationElement_Extend element, CacheRequest request)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(request, "request");
            try
            {
                return AutomationElement_Extend.Wrap(this._obj.GetPreviousSiblingElementBuildCache(
                    element.NativeElement,
                    request.NativeCacheRequest));
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public AutomationElement_Extend Normalize(AutomationElement_Extend element)
        {
            return Normalize(element, CacheRequest.Current);
        }

        public AutomationElement_Extend Normalize(AutomationElement_Extend element, CacheRequest request)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(request, "request");
            try
            {
                return AutomationElement_Extend.Wrap(this._obj.NormalizeElementBuildCache(
                    element.NativeElement,
                    request.NativeCacheRequest));
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        
        public ConditionExtended Condition
        {
            get
            {
                try
                {
                    return ConditionExtended.Wrap(_obj.condition);
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
                }
            }
        }
    }
}