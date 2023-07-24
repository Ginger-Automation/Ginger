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

using System.Collections.Generic;
using System.Diagnostics;
using UIAComWrapperInternal;
using Windows.Foundation;

namespace System.Windows.Automation
{
    public enum AutomationElementMode
    {
        None,
        Full
    }

    public sealed class AutomationElement_Extend
    {
        private UIAutomationClient.IUIAutomationElement _obj;
        public static readonly AutomationPropertyExtended AcceleratorKeyProperty = AutomationElementIdentifiersExtended.AcceleratorKeyProperty;
        public static readonly AutomationPropertyExtended AccessKeyProperty = AutomationElementIdentifiersExtended.AccessKeyProperty;
        public static readonly AutomationEventExtended AsyncContentLoadedEvent = AutomationElementIdentifiersExtended.AsyncContentLoadedEvent;
        public static readonly AutomationEventExtended AutomationFocusChangedEvent = AutomationElementIdentifiersExtended.AutomationFocusChangedEvent;
        public static readonly AutomationPropertyExtended AutomationIdProperty = AutomationElementIdentifiersExtended.AutomationIdProperty;
        public static readonly AutomationEventExtended AutomationPropertyChangedEvent = AutomationElementIdentifiersExtended.AutomationPropertyChangedEvent;
        public static readonly AutomationPropertyExtended BoundingRectangleProperty = AutomationElementIdentifiersExtended.BoundingRectangleProperty;
        public static readonly AutomationPropertyExtended ClassNameProperty = AutomationElementIdentifiersExtended.ClassNameProperty;
        public static readonly AutomationPropertyExtended ClickablePointProperty = AutomationElementIdentifiersExtended.ClickablePointProperty;
        public static readonly AutomationPropertyExtended ControlTypeProperty = AutomationElementIdentifiersExtended.ControlTypeProperty;
        public static readonly AutomationPropertyExtended CultureProperty = AutomationElementIdentifiersExtended.CultureProperty;
        public static readonly AutomationPropertyExtended FrameworkIdProperty = AutomationElementIdentifiersExtended.FrameworkIdProperty;
        public static readonly AutomationPropertyExtended HasKeyboardFocusProperty = AutomationElementIdentifiersExtended.HasKeyboardFocusProperty;
        public static readonly AutomationPropertyExtended HelpTextProperty = AutomationElementIdentifiersExtended.HelpTextProperty;
        public static readonly AutomationPropertyExtended IsContentElementProperty = AutomationElementIdentifiersExtended.IsContentElementProperty;
        public static readonly AutomationPropertyExtended IsControlElementProperty = AutomationElementIdentifiersExtended.IsControlElementProperty;
        public static readonly AutomationPropertyExtended IsDockPatternAvailableProperty = AutomationElementIdentifiersExtended.IsDockPatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsEnabledProperty = AutomationElementIdentifiersExtended.IsEnabledProperty;
        public static readonly AutomationPropertyExtended IsExpandCollapsePatternAvailableProperty = AutomationElementIdentifiersExtended.IsExpandCollapsePatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsGridItemPatternAvailableProperty = AutomationElementIdentifiersExtended.IsGridItemPatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsGridPatternAvailableProperty = AutomationElementIdentifiersExtended.IsGridPatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsInvokePatternAvailableProperty = AutomationElementIdentifiersExtended.IsInvokePatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsKeyboardFocusableProperty = AutomationElementIdentifiersExtended.IsKeyboardFocusableProperty;
        public static readonly AutomationPropertyExtended IsMultipleViewPatternAvailableProperty = AutomationElementIdentifiersExtended.IsMultipleViewPatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsOffscreenProperty = AutomationElementIdentifiersExtended.IsOffscreenProperty;
        public static readonly AutomationPropertyExtended IsPasswordProperty = AutomationElementIdentifiersExtended.IsPasswordProperty;
        public static readonly AutomationPropertyExtended IsRangeValuePatternAvailableProperty = AutomationElementIdentifiersExtended.IsRangeValuePatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsRequiredForFormProperty = AutomationElementIdentifiersExtended.IsRequiredForFormProperty;
        public static readonly AutomationPropertyExtended IsScrollItemPatternAvailableProperty = AutomationElementIdentifiersExtended.IsScrollItemPatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsScrollPatternAvailableProperty = AutomationElementIdentifiersExtended.IsScrollPatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsSelectionItemPatternAvailableProperty = AutomationElementIdentifiersExtended.IsSelectionItemPatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsSelectionPatternAvailableProperty = AutomationElementIdentifiersExtended.IsSelectionPatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsTableItemPatternAvailableProperty = AutomationElementIdentifiersExtended.IsTableItemPatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsTablePatternAvailableProperty = AutomationElementIdentifiersExtended.IsTablePatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsTextPatternAvailableProperty = AutomationElementIdentifiersExtended.IsTextPatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsTogglePatternAvailableProperty = AutomationElementIdentifiersExtended.IsTogglePatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsTransformPatternAvailableProperty = AutomationElementIdentifiersExtended.IsTransformPatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsValuePatternAvailableProperty = AutomationElementIdentifiersExtended.IsValuePatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsWindowPatternAvailableProperty = AutomationElementIdentifiersExtended.IsWindowPatternAvailableProperty;
        public static readonly AutomationPropertyExtended ItemStatusProperty = AutomationElementIdentifiersExtended.ItemStatusProperty;
        public static readonly AutomationPropertyExtended ItemTypeProperty = AutomationElementIdentifiersExtended.ItemTypeProperty;
        public static readonly AutomationPropertyExtended LabeledByProperty = AutomationElementIdentifiersExtended.LabeledByProperty;
        public static readonly AutomationEventExtended LayoutInvalidatedEvent = AutomationElementIdentifiersExtended.LayoutInvalidatedEvent;
        public static readonly AutomationPropertyExtended LocalizedControlTypeProperty = AutomationElementIdentifiersExtended.LocalizedControlTypeProperty;
        public static readonly AutomationEventExtended MenuClosedEvent = AutomationElementIdentifiersExtended.MenuClosedEvent;
        public static readonly AutomationEventExtended MenuOpenedEvent = AutomationElementIdentifiersExtended.MenuOpenedEvent;
        public static readonly AutomationPropertyExtended NameProperty = AutomationElementIdentifiersExtended.NameProperty;
        public static readonly AutomationPropertyExtended NativeWindowHandleProperty = AutomationElementIdentifiersExtended.NativeWindowHandleProperty;
        public static readonly object NotSupported = AutomationElementIdentifiersExtended.NotSupported;
        public static readonly AutomationPropertyExtended OrientationProperty = AutomationElementIdentifiersExtended.OrientationProperty;
        public static readonly AutomationPropertyExtended ProcessIdProperty = AutomationElementIdentifiersExtended.ProcessIdProperty;
        public static readonly AutomationPropertyExtended RuntimeIdProperty = AutomationElementIdentifiersExtended.RuntimeIdProperty;
        public static readonly AutomationEventExtended StructureChangedEvent = AutomationElementIdentifiersExtended.StructureChangedEvent;
        public static readonly AutomationEventExtended ToolTipClosedEvent = AutomationElementIdentifiersExtended.ToolTipClosedEvent;
        public static readonly AutomationEventExtended ToolTipOpenedEvent = AutomationElementIdentifiersExtended.ToolTipOpenedEvent;

        public static readonly AutomationPropertyExtended IsLegacyIAccessiblePatternAvailableProperty = AutomationElementIdentifiersExtended.IsLegacyIAccessiblePatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsItemContainerPatternAvailableProperty = AutomationElementIdentifiersExtended.IsItemContainerPatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsVirtualizedItemPatternAvailableProperty = AutomationElementIdentifiersExtended.IsVirtualizedItemPatternAvailableProperty;
        public static readonly AutomationPropertyExtended IsSynchronizedInputPatternAvailableProperty = AutomationElementIdentifiersExtended.IsSynchronizedInputPatternAvailableProperty;

        public static readonly AutomationPropertyExtended AriaRoleProperty = AutomationElementIdentifiersExtended.AriaRoleProperty;
        public static readonly AutomationPropertyExtended AriaPropertiesProperty = AutomationElementIdentifiersExtended.AriaPropertiesProperty;
        public static readonly AutomationPropertyExtended IsDataValidForFormProperty = AutomationElementIdentifiersExtended.IsDataValidForFormProperty;
        public static readonly AutomationPropertyExtended ControllerForProperty = AutomationElementIdentifiersExtended.ControllerForProperty;
        public static readonly AutomationPropertyExtended DescribedByProperty = AutomationElementIdentifiersExtended.DescribedByProperty;
        public static readonly AutomationPropertyExtended FlowsToProperty = AutomationElementIdentifiersExtended.FlowsToProperty;
        public static readonly AutomationPropertyExtended ProviderDescriptionProperty = AutomationElementIdentifiersExtended.ProviderDescriptionProperty;

        public static readonly AutomationEventExtended MenuModeStartEvent = AutomationElementIdentifiersExtended.MenuModeStartEvent;
        public static readonly AutomationEventExtended MenuModeEndEvent = AutomationElementIdentifiersExtended.MenuModeEndEvent;
        
        internal AutomationElement_Extend(UIAutomationClient.IUIAutomationElement obj)
        {
            Debug.Assert(obj != null);
            this._obj = obj;
        }

        internal static AutomationElement_Extend Wrap(UIAutomationClient.IUIAutomationElement obj)
        {
            return (obj == null) ? null : new AutomationElement_Extend(obj);
        }

        public override bool Equals(object obj)
        {
            AutomationElement_Extend element = obj as AutomationElement_Extend;
            return (((obj != null) && (element != null)) && (AutomationExtended.Factory.CompareElements(this._obj, element._obj) != 0));
        }

        ~AutomationElement_Extend()
        {
        }

        public AutomationElementCollectionExtended FindAll(TreeScopeExtended scope, ConditionExtended condition)
        {
            Utility.ValidateArgumentNonNull(condition, "condition");

            try
            {
                UIAutomationClient.IUIAutomationElementArray elemArray =
                    this._obj.FindAllBuildCache(
                        (UIAutomationClient.TreeScope)scope,
                        condition.NativeCondition,
                        CacheRequest.CurrentNativeCacheRequest);
                return AutomationElementCollectionExtended.Wrap(elemArray);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public AutomationElement_Extend FindFirst(TreeScopeExtended scope, ConditionExtended condition)
        {
            Utility.ValidateArgumentNonNull(condition, "condition");
            try
            {
                UIAutomationClient.IUIAutomationElement elem =
                    this._obj.FindFirstBuildCache(
                        (UIAutomationClient.TreeScope)scope,
                        condition.NativeCondition,
                        CacheRequest.CurrentNativeCacheRequest);
                return AutomationElement_Extend.Wrap(elem);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static AutomationElement_Extend FromHandle(IntPtr hwnd)
        {
            Utility.ValidateArgument(hwnd != IntPtr.Zero, "Hwnd cannot be null");
            try
            {
                UIAutomationClient.IUIAutomationElement element =
                    AutomationExtended.Factory.ElementFromHandleBuildCache(hwnd, CacheRequest.CurrentNativeCacheRequest);
                return AutomationElement_Extend.Wrap(element);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static AutomationElement_Extend FromIAccessible(Accessibility.IAccessible acc, int childId)
        {
            Utility.ValidateArgumentNonNull(acc, "acc");

            try
            {
                UIAutomationClient.IUIAutomationElement element =
                    AutomationExtended.Factory.ElementFromIAccessibleBuildCache(
                        (UIAutomationClient.IAccessible)acc, 
                        childId,
                        CacheRequest.CurrentNativeCacheRequest);
                return AutomationElement_Extend.Wrap(element);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static AutomationElement_Extend FromLocalProvider(object /* IRawElementProviderSimple */ localImpl)
        {
            Utility.ValidateArgumentNonNull(localImpl, "localImpl");

            // It's not clear how you'd do this -- COM API doesn't give you
            // the chance to wrap a provider like this.
            throw new NotImplementedException();
        }

        public static AutomationElement_Extend FromPoint(Point pt)
        {
            try
            {
                UIAutomationClient.IUIAutomationElement element =
                    AutomationExtended.Factory.ElementFromPointBuildCache(
                        Utility.PointManagedToNative(pt),
                        CacheRequest.CurrentNativeCacheRequest);
                return AutomationElement_Extend.Wrap(element);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public object GetCachedPattern(AutomationPatternExtended pattern)
        {
            object patternObj;
            if (!this.TryGetCachedPattern(pattern, out patternObj))
            {
                throw new InvalidOperationException("Unsupported pattern");
            }
            return patternObj;
        }

        public object GetCachedPropertyValue(AutomationPropertyExtended property)
        {
            return this.GetCachedPropertyValue(property, false);
        }

        public object GetCachedPropertyValue(AutomationPropertyExtended property, bool ignoreDefaultValue)
        {
            Utility.ValidateArgumentNonNull(property, "property");

            try
            {
                object obj = this._obj.GetCachedPropertyValueEx(property.Id, (ignoreDefaultValue) ? 1 : 0);
                return Utility.WrapObjectAsProperty(property, obj);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public Point GetClickablePoint()
        {
            Point point;
            if (!this.TryGetClickablePoint(out point))
            {
                throw new NoClickablePointException();
            }
            return point;
        }

        public object GetCurrentPattern(AutomationPatternExtended pattern)
        {
            object patternObj;
            if (!this.TryGetCurrentPattern(pattern, out patternObj))
            {
                throw new InvalidOperationException("Unsupported pattern");
            }
            return patternObj;
        }

        public object GetCurrentPropertyValue(AutomationPropertyExtended property)
        {
            return this.GetCurrentPropertyValue(property, false);
        }

        public object GetCurrentPropertyValue(AutomationPropertyExtended property, bool ignoreDefaultValue)
        {
            Utility.ValidateArgumentNonNull(property, "property");
            try
            {
                object obj = this._obj.GetCurrentPropertyValueEx(property.Id, (ignoreDefaultValue) ? 1 : 0);
                return Utility.WrapObjectAsProperty(property, obj);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }

        }

        internal object GetPropertyValue(AutomationPropertyExtended property, bool cached)
        {
            if (cached)
            {
                return GetCachedPropertyValue(property);
            }
            else
            {
                return GetCurrentPropertyValue(property);
            }
        }

        public override int GetHashCode()
        {
            int[] runtimeId = GetRuntimeId();
            int num = 0;
            if (runtimeId == null)
            {
                throw new InvalidOperationException("Operation cannot be performed");
            }
            foreach (int i in runtimeId)
            {
                num = (num * 4) ^ i;
            }
            return num;
        }

        internal object GetRawPattern(AutomationPatternExtended pattern, bool isCached)
        {
            try
            {

                if (isCached)
                {
                    return this._obj.GetCachedPattern(pattern.Id);
                }
                else
                {
                    return this._obj.GetCurrentPattern(pattern.Id);
                }
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }

        }

        public int[] GetRuntimeId()
        {
            try
            {
                return (int[])this._obj.GetRuntimeId();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public AutomationPatternExtended[] GetSupportedPatterns()
        {
            Array rawPatternIds;
            Array rawPatternNames;
            try
            {
                AutomationExtended.Factory.PollForPotentialSupportedPatterns(this._obj, out rawPatternIds, out rawPatternNames);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
            int[] patternIds = (int[])rawPatternIds;


            // This element may support patterns that are not registered for this 
            // client.  Filter them out.
            List<AutomationPatternExtended> patterns = new List<AutomationPatternExtended>();
            foreach (int patternId in patternIds)
            {
                AutomationPatternExtended pattern = AutomationPatternExtended.LookupById(patternId);
                if (pattern != null)
                {
                    patterns.Add(pattern);
                }
            }
            return patterns.ToArray();
        }

        public AutomationPropertyExtended[] GetSupportedProperties()
        {
            Array rawPropertyIds;
            Array rawPropertyNames;
            try
            {
                AutomationExtended.Factory.PollForPotentialSupportedProperties(this._obj, out rawPropertyIds, out rawPropertyNames);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
            int[] propertyIds = (int[])rawPropertyIds;

            // This element may support properties that are not registered for this 
            // client.  Filter them out.
            List<AutomationPropertyExtended> properties = new List<AutomationPropertyExtended>();
            foreach (int propertyId in propertyIds)
            {
                AutomationPropertyExtended property = AutomationPropertyExtended.LookupById(propertyId);
                if (property != null)
                {
                    properties.Add(property);
                }
            }
            return properties.ToArray();
        }

        public AutomationElement_Extend GetUpdatedCache(CacheRequest request)
        {
            try
            {
                return AutomationElement_Extend.Wrap(this._obj.BuildUpdatedCache(request.NativeCacheRequest));
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public static bool operator ==(AutomationElement_Extend left, AutomationElement_Extend right)
        {
            if (object.Equals(left, null))
            {
                return (object.Equals(right, null));
            }
            if (object.Equals(right, null))
            {
                return (object.Equals(left, null));
            }
            return left.Equals(right);
        }

        public static bool operator !=(AutomationElement_Extend left, AutomationElement_Extend right)
        {
            return !(left == right);
        }

        public void SetFocus()
        {
            try
            {
                this._obj.SetFocus();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }

        }

        public bool TryGetCachedPattern(AutomationPatternExtended pattern, out object patternObject)
        {
            patternObject = null;
            Utility.ValidateArgumentNonNull(pattern, "pattern");
            try
            {
                object nativePattern = this._obj.GetCachedPattern(pattern.Id);
                patternObject = Utility.WrapObjectAsPattern(this, nativePattern, pattern, true /* cached */);
                return (patternObject != null);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }

        }

        public bool TryGetClickablePoint(out Point pt)
        {
            pt = new Point(0.0, 0.0);
            UIAutomationClient.tagPOINT nativePoint = new UIAutomationClient.tagPOINT();
            try
            {
                bool success = this._obj.GetClickablePoint(out nativePoint) != 0;
                if (success)
                {
                    pt.X = nativePoint.x;
                    pt.Y = nativePoint.y;
                }
                return success;
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }

        }

        public bool TryGetCurrentPattern(AutomationPatternExtended pattern, out object patternObject)
        {
            patternObject = null;
            Utility.ValidateArgumentNonNull(pattern, "pattern");
            try
            {
                object nativePattern = this._obj.GetCurrentPattern(pattern.Id);
                patternObject = Utility.WrapObjectAsPattern(this, nativePattern, pattern, false /* cached */);
                return (patternObject != null);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }

        }

        
        public AutomationElementInformation Cached
        {
            get
            {
                return new AutomationElementInformation(this, true);
            }
        }

        public AutomationElementCollectionExtended CachedChildren
        {
            get
            {
                try
                {
                    return AutomationElementCollectionExtended.Wrap(this._obj.GetCachedChildren());
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
                }

            }
        }

        public AutomationElement_Extend CachedParent
        {
            get
            {
                try
                {

                    return AutomationElement_Extend.Wrap(this._obj.GetCachedParent());
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
                }

            }
        }

        public AutomationElementInformation Current
        {
            get
            {
                return new AutomationElementInformation(this, false);
            }
        }

        public UIAutomationClient.IUIAutomationElement NativeElement
        {
            get
            {
                return this._obj;
            }
        }

        public static AutomationElement_Extend FocusedElement
        {
            get
            {
                try
                {
                    return AutomationElement_Extend.Wrap(AutomationExtended.Factory.GetFocusedElement());
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
                }

            }
        }

        public static AutomationElement_Extend RootElement
        {
            get
            {
                try
                {
                    UIAutomationClient.IUIAutomationElement element =
                        AutomationExtended.Factory.GetRootElementBuildCache(
                            CacheRequest.CurrentNativeCacheRequest);
                    return AutomationElement_Extend.Wrap(element);
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
                }

            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct AutomationElementInformation
        {
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal AutomationElementInformation(AutomationElement_Extend el, bool isCached)
            {
                this._el = el;
                this._isCached = isCached;
            }

            public ControlTypeExtended ControlType
            {
                get
                {
                    return (ControlTypeExtended)this._el.GetPropertyValue(AutomationElement_Extend.ControlTypeProperty, _isCached);
                }
            }
            public string LocalizedControlType
            {
                get
                {
                    return (string)this._el.GetPropertyValue(AutomationElement_Extend.LocalizedControlTypeProperty, _isCached);
                }
            }
            public string Name
            {
                get
                {
                    return (string)this._el.GetPropertyValue(AutomationElement_Extend.NameProperty, _isCached);
                }
            }
            public string AcceleratorKey
            {
                get
                {
                    return (string)this._el.GetPropertyValue(AutomationElement_Extend.AcceleratorKeyProperty, _isCached);
                }
            }
            public string AccessKey
            {
                get
                {
                    return (string)this._el.GetPropertyValue(AutomationElement_Extend.AccessKeyProperty, _isCached);
                }
            }
            public bool HasKeyboardFocus
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(AutomationElement_Extend.HasKeyboardFocusProperty, _isCached);
                }
            }
            public bool IsKeyboardFocusable
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(AutomationElement_Extend.IsKeyboardFocusableProperty, _isCached);
                }
            }
            public bool IsEnabled
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(AutomationElement_Extend.IsEnabledProperty, _isCached);
                }
            }
            public Rect BoundingRectangle
            {
                get
                {
                    return (Rect)this._el.GetPropertyValue(AutomationElement_Extend.BoundingRectangleProperty, _isCached);
                }
            }
            public string HelpText
            {
                get
                {
                    return (string)this._el.GetPropertyValue(AutomationElement_Extend.HelpTextProperty, _isCached);
                }
            }
            public bool IsControlElement
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(AutomationElement_Extend.IsControlElementProperty, _isCached);
                }
            }
            public bool IsContentElement
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(AutomationElement_Extend.IsContentElementProperty, _isCached);
                }
            }
            public AutomationElement_Extend LabeledBy
            {
                get
                {
                    return (AutomationElement_Extend)this._el.GetPropertyValue(AutomationElement_Extend.LabeledByProperty, _isCached);
                }
            }
            public string AutomationId
            {
                get
                {
                    return (string)this._el.GetPropertyValue(AutomationElement_Extend.AutomationIdProperty, _isCached);
                }
            }
            public string ItemType
            {
                get
                {
                    return (string)this._el.GetPropertyValue(AutomationElement_Extend.ItemTypeProperty, _isCached);
                }
            }
            public bool IsPassword
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(AutomationElement_Extend.IsPasswordProperty, _isCached);
                }
            }
            public string ClassName
            {
                get
                {
                    return (string)this._el.GetPropertyValue(AutomationElement_Extend.ClassNameProperty, _isCached);
                }
            }
            public int NativeWindowHandle
            {
                get
                {
                    return (int)this._el.GetPropertyValue(AutomationElement_Extend.NativeWindowHandleProperty, _isCached);
                }
            }
            public int ProcessId
            {
                get
                {
                    return (int)this._el.GetPropertyValue(AutomationElement_Extend.ProcessIdProperty, _isCached);
                }
            }
            public bool IsOffscreen
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(AutomationElement_Extend.IsOffscreenProperty, _isCached);
                }
            }
            public OrientationTypeExtended Orientation
            {
                get
                {
                    return (OrientationTypeExtended)this._el.GetPropertyValue(AutomationElement_Extend.OrientationProperty, _isCached);
                }
            }
            public string FrameworkId
            {
                get
                {
                    return (string)this._el.GetPropertyValue(AutomationElement_Extend.FrameworkIdProperty, _isCached);
                }
            }
            public bool IsRequiredForForm
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(AutomationElement_Extend.IsRequiredForFormProperty, _isCached);
                }
            }
            public string ItemStatus
            {
                get
                {
                    return (string)this._el.GetPropertyValue(AutomationElement_Extend.ItemStatusProperty, _isCached);
                }
            }
            public string AriaRole
            {
                get
                {
                    return (string)this._el.GetPropertyValue(AutomationElement_Extend.AriaRoleProperty, _isCached);
                }
            }
            public string AriaProperties
            {
                get
                {
                    return (string)this._el.GetPropertyValue(AutomationElement_Extend.AriaPropertiesProperty, _isCached);
                }
            }
            public bool IsDataValidForForm
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(AutomationElement_Extend.IsDataValidForFormProperty, _isCached);
                }
            }
            public AutomationElement_Extend ControllerFor
            {
                get
                {
                    return (AutomationElement_Extend)this._el.GetPropertyValue(AutomationElement_Extend.ControllerForProperty, _isCached);
                }
            }
            public AutomationElement_Extend DescribedBy
            {
                get
                {
                    return (AutomationElement_Extend)this._el.GetPropertyValue(AutomationElement_Extend.DescribedByProperty, _isCached);
                }
            }
            public AutomationElement_Extend FlowsTo
            {
                get
                {
                    return (AutomationElement_Extend)this._el.GetPropertyValue(AutomationElement_Extend.FlowsToProperty, _isCached);
                }
            }
            public string ProviderDescription
            {
                get
                {
                    return (string)this._el.GetPropertyValue(AutomationElement_Extend.ProviderDescriptionProperty, _isCached);
                }
            }
        }
    }
}
