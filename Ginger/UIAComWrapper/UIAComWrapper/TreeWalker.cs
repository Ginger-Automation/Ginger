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

using System.Diagnostics;
using UIAComWrapperInternal;

namespace System.Windows.Automation
{
    public sealed class TreeWalker
    {
        private UIAutomationClient.IUIAutomationTreeWalker _obj;
        public static readonly TreeWalker ContentViewWalker = new TreeWalker(System.Windows.Automation.Automation.ContentViewCondition);
        public static readonly TreeWalker ControlViewWalker = new TreeWalker(System.Windows.Automation.Automation.ControlViewCondition);
        public static readonly TreeWalker RawViewWalker = new TreeWalker(System.Windows.Automation.Automation.RawViewCondition);
        
        public TreeWalker(Condition condition)
        {
            // This is an unusual situation - a direct constructor.
            // We have to go create the native tree walker, which might throw.
            Utility.ValidateArgumentNonNull(condition, "condition");
            try
            {
                this._obj = Automation.Factory.CreateTreeWalker(condition.NativeCondition);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        internal TreeWalker(UIAutomationClient.IUIAutomationTreeWalker obj)
        {
            Debug.Assert(obj != null);
            _obj = obj;
        }

        internal TreeWalker Wrap(UIAutomationClient.IUIAutomationTreeWalker obj)
        {
            return (obj == null) ? null : Wrap(obj);
        }

        public AutomationElement GetFirstChild(AutomationElement element)
        {
            return GetFirstChild(element, CacheRequest.Current);
        }

        public AutomationElement GetFirstChild(AutomationElement element, CacheRequest request)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(request, "request");
            try
            {
                return AutomationElement.Wrap(this._obj.GetFirstChildElementBuildCache(
                    element.NativeElement,
                    request.NativeCacheRequest));
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public AutomationElement GetLastChild(AutomationElement element)
        {
            return GetLastChild(element, CacheRequest.Current);
        }

        public AutomationElement GetLastChild(AutomationElement element, CacheRequest request)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(request, "request");
            try
            {
                return AutomationElement.Wrap(this._obj.GetLastChildElementBuildCache(
                    element.NativeElement,
                    request.NativeCacheRequest));
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public AutomationElement GetNextSibling(AutomationElement element)
        {
            return GetNextSibling(element, CacheRequest.Current);
        }

        public AutomationElement GetNextSibling(AutomationElement element, CacheRequest request)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(request, "request");
            try
            {
                return AutomationElement.Wrap(this._obj.GetNextSiblingElementBuildCache(
                    element.NativeElement,
                    request.NativeCacheRequest));
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public AutomationElement GetParent(AutomationElement element)
        {
            return GetParent(element, CacheRequest.Current);
        }

        public AutomationElement GetParent(AutomationElement element, CacheRequest request)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(request, "request");
            try
            {
                return AutomationElement.Wrap(this._obj.GetParentElementBuildCache(
                    element.NativeElement,
                    request.NativeCacheRequest));
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public AutomationElement GetPreviousSibling(AutomationElement element)
        {
            return GetPreviousSibling(element, CacheRequest.Current);
        }

        public AutomationElement GetPreviousSibling(AutomationElement element, CacheRequest request)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(request, "request");
            try
            {
                return AutomationElement.Wrap(this._obj.GetPreviousSiblingElementBuildCache(
                    element.NativeElement,
                    request.NativeCacheRequest));
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public AutomationElement Normalize(AutomationElement element)
        {
            return Normalize(element, CacheRequest.Current);
        }

        public AutomationElement Normalize(AutomationElement element, CacheRequest request)
        {
            Utility.ValidateArgumentNonNull(element, "element");
            Utility.ValidateArgumentNonNull(request, "request");
            try
            {
                return AutomationElement.Wrap(this._obj.NormalizeElementBuildCache(
                    element.NativeElement,
                    request.NativeCacheRequest));
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        
        public Condition Condition
        {
            get
            {
                try
                {
                    return Condition.Wrap(_obj.condition);
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
                }
            }
        }
    }
}