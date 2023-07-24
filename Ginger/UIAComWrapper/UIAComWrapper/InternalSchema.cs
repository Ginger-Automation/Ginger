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
using System.Globalization;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using System.Windows;
using Windows.Foundation;

namespace UIAComWrapperInternal
{
    internal delegate object PropertyConverter(object valueAsObject);
    internal delegate object PatternWrapper(AutomationElement_Extend el, object pattern, bool cached);

    internal class PropertyTypeInfo
    {
        
        private PropertyConverter _converter;
        private AutomationIdentifier _id;
        private Type _type;

        
        internal PropertyTypeInfo(PropertyConverter converter, AutomationIdentifier id, Type type)
        {
            this._id = id;
            this._type = type;
            this._converter = converter;
        }

        
        internal AutomationIdentifier ID
        {
            get
            {
                return this._id;
            }
        }

        internal PropertyConverter ObjectConverter
        {
            get
            {
                return this._converter;
            }
        }

        internal Type Type
        {
            get
            {
                return this._type;
            }
        }
    }

    internal class PatternTypeInfo
    {
        
        private PatternWrapper _clientSideWrapper;
        private AutomationPatternExtended _id;

        
        public PatternTypeInfo(AutomationPatternExtended id, PatternWrapper clientSideWrapper)
        {
            this._id = id;
            this._clientSideWrapper = clientSideWrapper;
        }

        
        internal PatternWrapper ClientSideWrapper
        {
            get
            {
                return this._clientSideWrapper;
            }
        }

        internal AutomationPatternExtended ID
        {
            get
            {
                return this._id;
            }
        }
    }

    internal class Schema
    {
        
        private static PropertyConverter convertToBool = new PropertyConverter(Schema.ConvertToBool);
        private static PropertyConverter convertToControlType = new PropertyConverter(Schema.ConvertToControlType);
        private static PropertyConverter convertToCultureInfo = new PropertyConverter(Schema.ConvertToCultureInfo);
        private static PropertyConverter convertToDockPosition = new PropertyConverter(Schema.ConvertToDockPosition);
        private static PropertyConverter convertToElement = new PropertyConverter(Schema.ConvertToElement);
        private static PropertyConverter convertToElementArray = new PropertyConverter(Schema.ConvertToElementArray);
        private static PropertyConverter convertToExpandCollapseState = new PropertyConverter(Schema.ConvertToExpandCollapseState);
        private static PropertyConverter convertToOrientationType = new PropertyConverter(Schema.ConvertToOrientationType);
        private static PropertyConverter convertToPoint = new PropertyConverter(Schema.ConvertToPoint);
        private static PropertyConverter convertToRect = new PropertyConverter(Schema.ConvertToRect);
        private static PropertyConverter convertToRowOrColumnMajor = new PropertyConverter(Schema.ConvertToRowOrColumnMajor);
        private static PropertyConverter convertToToggleState = new PropertyConverter(Schema.ConvertToToggleState);
        private static PropertyConverter convertToWindowInteractionState = new PropertyConverter(Schema.ConvertToWindowInteractionState);
        private static PropertyConverter convertToWindowVisualState = new PropertyConverter(Schema.ConvertToWindowVisualState);

        private static readonly PropertyTypeInfo[] _propertyInfoTable = new PropertyTypeInfo[] { 
            // Properties requiring conversion
            new PropertyTypeInfo(convertToRect, AutomationElement_Extend.BoundingRectangleProperty, typeof(Rect)), 
            new PropertyTypeInfo(convertToControlType, AutomationElement_Extend.ControlTypeProperty, typeof(ControlTypeExtended)), 
            new PropertyTypeInfo(convertToPoint, AutomationElement_Extend.ClickablePointProperty, typeof(Point)), 
            new PropertyTypeInfo(convertToCultureInfo, AutomationElement_Extend.CultureProperty, typeof(CultureInfo)), 
            new PropertyTypeInfo(convertToOrientationType, AutomationElement_Extend.OrientationProperty, typeof(OrientationTypeExtended)), 
            new PropertyTypeInfo(convertToDockPosition, DockPattern.DockPositionProperty, typeof(DockPosition)), 
            new PropertyTypeInfo(convertToExpandCollapseState, ExpandCollapsePatternExtended.ExpandCollapseStateProperty, typeof(ExpandCollapseState)), 
            new PropertyTypeInfo(convertToWindowVisualState, WindowPatternExtended.WindowVisualStateProperty, typeof(WindowVisualStateExtended)), 
            new PropertyTypeInfo(convertToWindowInteractionState, WindowPatternExtended.WindowInteractionStateProperty, typeof(WindowInteractionStateExtended)), 
            new PropertyTypeInfo(convertToRowOrColumnMajor, TablePattern.RowOrColumnMajorProperty, typeof(RowOrColumnMajor)), 
            new PropertyTypeInfo(convertToToggleState, TogglePatternExtended.ToggleStateProperty, typeof(ToggleStateExtended)), 

            // Text attributes 
            new PropertyTypeInfo(null, TextPatternExtended.AnimationStyleAttribute, typeof(AnimationStyle)), 
            new PropertyTypeInfo(null, TextPatternExtended.BackgroundColorAttribute, typeof(int)), 
            new PropertyTypeInfo(null, TextPatternExtended.BulletStyleAttribute, typeof(BulletStyle)), 
            new PropertyTypeInfo(null, TextPatternExtended.CapStyleAttribute, typeof(CapStyle)), 
            new PropertyTypeInfo(convertToCultureInfo, TextPatternExtended.CultureAttribute, typeof(CultureInfo)), 
            new PropertyTypeInfo(null, TextPatternExtended.FontNameAttribute, typeof(string)), 
            new PropertyTypeInfo(null, TextPatternExtended.FontSizeAttribute, typeof(double)), 
            new PropertyTypeInfo(null, TextPatternExtended.FontWeightAttribute, typeof(int)), 
            new PropertyTypeInfo(null, TextPatternExtended.ForegroundColorAttribute, typeof(int)), 
            new PropertyTypeInfo(null, TextPatternExtended.HorizontalTextAlignmentAttribute, typeof(HorizontalTextAlignment)), 
            new PropertyTypeInfo(null, TextPatternExtended.IndentationFirstLineAttribute, typeof(double)), 
            new PropertyTypeInfo(null, TextPatternExtended.IndentationLeadingAttribute, typeof(double)), 
            new PropertyTypeInfo(null, TextPatternExtended.IndentationTrailingAttribute, typeof(double)), 
            new PropertyTypeInfo(null, TextPatternExtended.IsHiddenAttribute, typeof(bool)), new PropertyTypeInfo(null, TextPatternExtended.IsItalicAttribute, typeof(bool)), 
            new PropertyTypeInfo(null, TextPatternExtended.IsReadOnlyAttribute, typeof(bool)), 
            new PropertyTypeInfo(null, TextPatternExtended.IsSubscriptAttribute, typeof(bool)), 
            new PropertyTypeInfo(null, TextPatternExtended.IsSuperscriptAttribute, typeof(bool)), 
            new PropertyTypeInfo(null, TextPatternExtended.MarginBottomAttribute, typeof(double)), 
            new PropertyTypeInfo(null, TextPatternExtended.MarginLeadingAttribute, typeof(double)), 
            new PropertyTypeInfo(null, TextPatternExtended.MarginTopAttribute, typeof(double)), 
            new PropertyTypeInfo(null, TextPatternExtended.MarginTrailingAttribute, typeof(double)), 
            new PropertyTypeInfo(null, TextPatternExtended.OutlineStylesAttribute, typeof(OutlineStyles)), 
            new PropertyTypeInfo(null, TextPatternExtended.OverlineColorAttribute, typeof(int)), 
            new PropertyTypeInfo(null, TextPatternExtended.OverlineStyleAttribute, typeof(TextDecorationLineStyle)), 
            new PropertyTypeInfo(null, TextPatternExtended.StrikethroughColorAttribute, typeof(int)), 
            new PropertyTypeInfo(null, TextPatternExtended.StrikethroughStyleAttribute, typeof(TextDecorationLineStyle)), 
            new PropertyTypeInfo(null, TextPatternExtended.TabsAttribute, typeof(double[])), 
            new PropertyTypeInfo(null, TextPatternExtended.TextFlowDirectionsAttribute, typeof(FlowDirections)), 
            new PropertyTypeInfo(null, TextPatternExtended.UnderlineColorAttribute, typeof(int)), 
            new PropertyTypeInfo(null, TextPatternExtended.UnderlineStyleAttribute, typeof(TextDecorationLineStyle))
        };

        private static readonly PatternTypeInfo[] _patternInfoTable = new PatternTypeInfo[] { 
            new PatternTypeInfo(InvokePatternExtended.Pattern, new PatternWrapper(InvokePatternExtended.Wrap)), 
            new PatternTypeInfo(SelectionPattern.Pattern, new PatternWrapper(SelectionPattern.Wrap)), 
            new PatternTypeInfo(ValuePatternExtended.Pattern, new PatternWrapper(ValuePatternExtended.Wrap)), 
            new PatternTypeInfo(RangeValuePatternExtended.Pattern, new PatternWrapper(RangeValuePatternExtended.Wrap)), 
            new PatternTypeInfo(ScrollPatternExtended.Pattern, new PatternWrapper(ScrollPatternExtended.Wrap)), 
            new PatternTypeInfo(ExpandCollapsePatternExtended.Pattern, new PatternWrapper(ExpandCollapsePatternExtended.Wrap)), 
            new PatternTypeInfo(GridPatternExtended.Pattern, new PatternWrapper(GridPatternExtended.Wrap)), 
            new PatternTypeInfo(GridItemPattern.Pattern, new PatternWrapper(GridItemPattern.Wrap)), 
            new PatternTypeInfo(MultipleViewPattern.Pattern, new PatternWrapper(MultipleViewPattern.Wrap)), 
            new PatternTypeInfo(WindowPatternExtended.Pattern, new PatternWrapper(WindowPatternExtended.Wrap)), 
            new PatternTypeInfo(SelectionItemPatternExtended.Pattern, new PatternWrapper(SelectionItemPatternExtended.Wrap)), 
            new PatternTypeInfo(DockPattern.Pattern, new PatternWrapper(DockPattern.Wrap)), 
            new PatternTypeInfo(TablePattern.Pattern, new PatternWrapper(TablePattern.Wrap)), 
            new PatternTypeInfo(TableItemPattern.Pattern, new PatternWrapper(TableItemPattern.Wrap)), 
            new PatternTypeInfo(TextPatternExtended.Pattern, new PatternWrapper(TextPatternExtended.Wrap)), 
            new PatternTypeInfo(TogglePatternExtended.Pattern, new PatternWrapper(TogglePatternExtended.Wrap)), 
            new PatternTypeInfo(TransformPatternExtended.Pattern, new PatternWrapper(TransformPatternExtended.Wrap)), 
            new PatternTypeInfo(ScrollItemPatternExtended.Pattern, new PatternWrapper(ScrollItemPatternExtended.Wrap)),
            new PatternTypeInfo(ItemContainerPattern.Pattern, new PatternWrapper(ItemContainerPattern.Wrap)),
            new PatternTypeInfo(VirtualizedItemPattern.Pattern, new PatternWrapper(VirtualizedItemPattern.Wrap)),
            new PatternTypeInfo(LegacyIAccessiblePatternExtended.Pattern, new PatternWrapper(LegacyIAccessiblePatternExtended.Wrap)),
            new PatternTypeInfo(SynchronizedInputPattern.Pattern, new PatternWrapper(SynchronizedInputPattern.Wrap))
     };

        
        private Schema()
        {
        }

        private static object ConvertToBool(object value)
        {
            return value;
        }

        private static object ConvertToControlType(object value)
        {
            if (value is ControlTypeExtended)
            {
                return value;
            }
            return ControlTypeExtended.LookupById((int)value);
        }

        private static object ConvertToCultureInfo(object value)
        {
            if (value is int)
            {
                if ((int)value == 0)
                {
                    // Some providers return 0 to mean Invariant
                    return CultureInfo.InvariantCulture;
                }
                else
                {
                    return new CultureInfo((int)value);
                }
            }
            return null;
        }

        private static object ConvertToDockPosition(object value)
        {
            return (DockPosition)value;
        }

        private static object ConvertToElement(object value)
        {
            return AutomationElement_Extend.Wrap((UIAutomationClient.IUIAutomationElement)value);
        }

        internal static object ConvertToElementArray(object value)
        {
            return Utility.ConvertToElementArray((UIAutomationClient.IUIAutomationElementArray)value);
        }

        private static object ConvertToExpandCollapseState(object value)
        {
            return (ExpandCollapseState)value;
        }

        private static object ConvertToOrientationType(object value)
        {
            return (OrientationTypeExtended)value;
        }

        private static object ConvertToPoint(object value)
        {
            double[] numArray = (double[])value;
            return new Point(numArray[0], numArray[1]);
        }

        private static object ConvertToRect(object value)
        {
            double[] numArray = (double[])value;
            double x = numArray[0];
            double y = numArray[1];
            double width = numArray[2];
            return new Rect(x, y, width, numArray[3]);
        }

        private static object ConvertToRowOrColumnMajor(object value)
        {
            return (RowOrColumnMajor)value;
        }

        private static object ConvertToToggleState(object value)
        {
            return (ToggleStateExtended)value;
        }

        private static object ConvertToWindowInteractionState(object value)
        {
            return (WindowInteractionStateExtended)value;
        }

        private static object ConvertToWindowVisualState(object value)
        {
            return (WindowVisualStateExtended)value;
        }

        internal static bool GetPatternInfo(AutomationPatternExtended id, out PatternTypeInfo info)
        {
            foreach (PatternTypeInfo info2 in _patternInfoTable)
            {
                if (info2.ID == id)
                {
                    info = info2;
                    return true;
                }
            }
            info = null;
            return false;
        }

        internal static bool GetPropertyTypeInfo(AutomationIdentifier id, out PropertyTypeInfo info)
        {
            foreach (PropertyTypeInfo info2 in _propertyInfoTable)
            {
                if (info2.ID == id)
                {
                    info = info2;
                    return true;
                }
            }
            info = null;
            return false;
        }
    }
}
