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
using System.Runtime.InteropServices;
using UIAComWrapperInternal;

namespace System.Windows.Automation
{
    public class TransformPatternExtended : BasePattern
    {
        private UIAutomationClient.IUIAutomationTransformPattern _pattern;
        public static readonly AutomationPatternExtended Pattern = TransformPatternIdentifiers.Pattern;
        public static readonly AutomationPropertyExtended CanMoveProperty = TransformPatternIdentifiers.CanMoveProperty;
        public static readonly AutomationPropertyExtended CanResizeProperty = TransformPatternIdentifiers.CanResizeProperty;
        public static readonly AutomationPropertyExtended CanRotateProperty = TransformPatternIdentifiers.CanRotateProperty;
        
        private TransformPatternExtended(AutomationElement_Extend el, UIAutomationClient.IUIAutomationTransformPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new TransformPatternExtended(el, (UIAutomationClient.IUIAutomationTransformPattern)pattern, cached);
        }

        public void Move(double x, double y)
        {
            try
            {
                this._pattern.Move(x, y);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public void Resize(double width, double height)
        {
            try
            {
                this._pattern.Resize(width, height);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public void Rotate(double degrees)
        {
            try
            {
                this._pattern.Rotate(degrees);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }
        
        public TransformPatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new TransformPatternInformation(this._el, true);
            }
        }

        public TransformPatternInformation Current
        {
            get
            {
                return new TransformPatternInformation(this._el, false);
            }
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct TransformPatternInformation
        {
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal TransformPatternInformation(AutomationElement_Extend element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public bool CanMove
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(TransformPatternExtended.CanMoveProperty, _isCached);
                }
            }
            public bool CanResize
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(TransformPatternExtended.CanResizeProperty, _isCached);
                }
            }
            public bool CanRotate
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(TransformPatternExtended.CanRotateProperty, _isCached);
                }
            }
        }
    }
}