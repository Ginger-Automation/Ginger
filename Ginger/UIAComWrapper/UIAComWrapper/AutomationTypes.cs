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

using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using UIAComWrapperInternal;

namespace System.Windows.Automation
{
    #region Well-known properties

    public static class AutomationElementIdentifiersExtended
    {
        
        public static readonly AutomationPropertyExtended AcceleratorKeyProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.AcceleratorKey_Property, "AutomationElementIdentifiers.AcceleratorKeyProperty");
        public static readonly AutomationPropertyExtended AccessKeyProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.AccessKey_Property, "AutomationElementIdentifiers.AccessKeyProperty");
        public static readonly AutomationEventExtended AsyncContentLoadedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.AsyncContentLoaded_Event, "AutomationElementIdentifiers.AsyncContentLoadedEvent");
        public static readonly AutomationEventExtended AutomationFocusChangedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.AutomationFocusChanged_Event, "AutomationElementIdentifiers.AutomationFocusChangedEvent");
        public static readonly AutomationPropertyExtended AutomationIdProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.AutomationId_Property, "AutomationElementIdentifiers.AutomationIdProperty");
        public static readonly AutomationEventExtended AutomationPropertyChangedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.AutomationPropertyChanged_Event, "AutomationElementIdentifiers.AutomationPropertyChangedEvent");
        public static readonly AutomationPropertyExtended BoundingRectangleProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.BoundingRectangle_Property, "AutomationElementIdentifiers.BoundingRectangleProperty");
        public static readonly AutomationPropertyExtended ClassNameProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.ClassName_Property, "AutomationElementIdentifiers.ClassNameProperty");
        public static readonly AutomationPropertyExtended ClickablePointProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.ClickablePoint_Property, "AutomationElementIdentifiers.ClickablePointProperty");
        public static readonly AutomationPropertyExtended ControlTypeProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.ControlType_Property, "AutomationElementIdentifiers.ControlTypeProperty");
        public static readonly AutomationPropertyExtended CultureProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Culture_Property, "AutomationElementIdentifiers.CultureProperty");
        public static readonly AutomationPropertyExtended FrameworkIdProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.FrameworkId_Property, "AutomationElementIdentifiers.FrameworkIdProperty");
        public static readonly AutomationPropertyExtended HasKeyboardFocusProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.HasKeyboardFocus_Property, "AutomationElementIdentifiers.HasKeyboardFocusProperty");
        public static readonly AutomationPropertyExtended HelpTextProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.HelpText_Property, "AutomationElementIdentifiers.HelpTextProperty");
        public static readonly AutomationPropertyExtended IsContentElementProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsContentElement_Property, "AutomationElementIdentifiers.IsContentElementProperty");
        public static readonly AutomationPropertyExtended IsControlElementProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsControlElement_Property, "AutomationElementIdentifiers.IsControlElementProperty");
        public static readonly AutomationPropertyExtended IsDockPatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsDockPatternAvailable_Property, "AutomationElementIdentifiers.IsDockPatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsEnabledProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsEnabled_Property, "AutomationElementIdentifiers.IsEnabledProperty");
        public static readonly AutomationPropertyExtended IsExpandCollapsePatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsExpandCollapsePatternAvailable_Property, "AutomationElementIdentifiers.IsExpandCollapsePatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsGridItemPatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsGridItemPatternAvailable_Property, "AutomationElementIdentifiers.IsGridItemPatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsGridPatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsGridPatternAvailable_Property, "AutomationElementIdentifiers.IsGridPatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsInvokePatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsInvokePatternAvailable_Property, "AutomationElementIdentifiers.IsInvokePatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsKeyboardFocusableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsKeyboardFocusable_Property, "AutomationElementIdentifiers.IsKeyboardFocusableProperty");
        public static readonly AutomationPropertyExtended IsMultipleViewPatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsMultipleViewPatternAvailable_Property, "AutomationElementIdentifiers.IsMultipleViewPatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsOffscreenProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsOffscreen_Property, "AutomationElementIdentifiers.IsOffscreenProperty");
        public static readonly AutomationPropertyExtended IsPasswordProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsPassword_Property, "AutomationElementIdentifiers.IsPasswordProperty");
        public static readonly AutomationPropertyExtended IsRangeValuePatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsRangeValuePatternAvailable_Property, "AutomationElementIdentifiers.IsRangeValuePatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsRequiredForFormProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsRequiredForForm_Property, "AutomationElementIdentifiers.IsRequiredForFormProperty");
        public static readonly AutomationPropertyExtended IsScrollItemPatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsScrollItemPatternAvailable_Property, "AutomationElementIdentifiers.IsScrollItemPatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsScrollPatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsScrollPatternAvailable_Property, "AutomationElementIdentifiers.IsScrollPatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsSelectionItemPatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsSelectionItemPatternAvailable_Property, "AutomationElementIdentifiers.IsSelectionItemPatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsSelectionPatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsSelectionPatternAvailable_Property, "AutomationElementIdentifiers.IsSelectionPatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsTableItemPatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsTableItemPatternAvailable_Property, "AutomationElementIdentifiers.IsTableItemPatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsTablePatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsTablePatternAvailable_Property, "AutomationElementIdentifiers.IsTablePatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsTextPatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsTextPatternAvailable_Property, "AutomationElementIdentifiers.IsTextPatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsTogglePatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsTogglePatternAvailable_Property, "AutomationElementIdentifiers.IsTogglePatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsTransformPatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsTransformPatternAvailable_Property, "AutomationElementIdentifiers.IsTransformPatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsValuePatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsValuePatternAvailable_Property, "AutomationElementIdentifiers.IsValuePatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsWindowPatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsWindowPatternAvailable_Property, "AutomationElementIdentifiers.IsWindowPatternAvailableProperty");
        public static readonly AutomationPropertyExtended ItemStatusProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.ItemStatus_Property, "AutomationElementIdentifiers.ItemStatusProperty");
        public static readonly AutomationPropertyExtended ItemTypeProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.ItemType_Property, "AutomationElementIdentifiers.ItemTypeProperty");
        public static readonly AutomationPropertyExtended LabeledByProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.LabeledBy_Property, "AutomationElementIdentifiers.LabeledByProperty");
        public static readonly AutomationEventExtended LayoutInvalidatedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.LayoutInvalidated_Event, "AutomationElementIdentifiers.LayoutInvalidatedEvent");
        public static readonly AutomationPropertyExtended LocalizedControlTypeProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.LocalizedControlType_Property, "AutomationElementIdentifiers.LocalizedControlTypeProperty");
        public static readonly AutomationEventExtended MenuClosedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.MenuClosed_Event, "AutomationElementIdentifiers.MenuClosedEvent");
        public static readonly AutomationEventExtended MenuOpenedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.MenuOpened_Event, "AutomationElementIdentifiers.MenuOpenedEvent");
        public static readonly AutomationPropertyExtended NameProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Name_Property, "AutomationElementIdentifiers.NameProperty");
        public static readonly AutomationPropertyExtended NativeWindowHandleProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.NewNativeWindowHandle_Property, "AutomationElementIdentifiers.NativeWindowHandleProperty");
        public static readonly object NotSupported = AutomationExtended.Factory.ReservedNotSupportedValue;
        public static readonly AutomationPropertyExtended OrientationProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Orientation_Property, "AutomationElementIdentifiers.OrientationProperty");
        public static readonly AutomationPropertyExtended ProcessIdProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.ProcessId_Property, "AutomationElementIdentifiers.ProcessIdProperty");
        public static readonly AutomationPropertyExtended RuntimeIdProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.RuntimeId_Property, "AutomationElementIdentifiers.RuntimeIdProperty");
        public static readonly AutomationEventExtended StructureChangedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.StructureChanged_Event, "AutomationElementIdentifiers.StructureChangedEvent");
        public static readonly AutomationEventExtended ToolTipClosedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.ToolTipClosed_Event, "AutomationElementIdentifiers.ToolTipClosedEvent");
        public static readonly AutomationEventExtended ToolTipOpenedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.ToolTipOpened_Event, "AutomationElementIdentifiers.ToolTipOpenedEvent");

        // New for Windows 7
        //

        public static readonly AutomationPropertyExtended IsLegacyIAccessiblePatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsLegacyIAccessiblePatternAvailable_Property, "AutomationElementIdentifiers.IsLegacyIAccessiblePatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsItemContainerPatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsItemContainerPatternAvailable_Property, "AutomationElementIdentifiers.IsItemContainerPatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsVirtualizedItemPatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsVirtualizedItemPatternAvailable_Property, "AutomationElementIdentifiers.IsVirtualizedItemPatternAvailableProperty");
        public static readonly AutomationPropertyExtended IsSynchronizedInputPatternAvailableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsSynchronizedInputPatternAvailable_Property, "AutomationElementIdentifiers.IsSynchronizedInputPatternAvailableProperty");

        public static readonly AutomationPropertyExtended AriaRoleProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.AriaRole_Property, "AutomationElementIdentifiers.AriaRoleProperty");
        public static readonly AutomationPropertyExtended AriaPropertiesProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.AriaProperties_Property, "AutomationElementIdentifiers.AriaPropertiesProperty");
        public static readonly AutomationPropertyExtended IsDataValidForFormProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.IsDataValidForForm_Property, "AutomationElementIdentifiers.IsDataValidForFormProperty");
        public static readonly AutomationPropertyExtended ControllerForProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.ControllerFor_Property, "AutomationElementIdentifiers.ControllerForProperty");
        public static readonly AutomationPropertyExtended DescribedByProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.DescribedBy_Property, "AutomationElementIdentifiers.DescribedByProperty");
        public static readonly AutomationPropertyExtended FlowsToProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.FlowsTo_Property, "AutomationElementIdentifiers.FlowsToProperty");
        public static readonly AutomationPropertyExtended ProviderDescriptionProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.ProviderDescription_Property, "AutomationElementIdentifiers.Property");

        public static readonly AutomationEventExtended MenuModeStartEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.MenuModeStart_Event, "AutomationElementIdentifiers.MenuModeStartEvent");
        public static readonly AutomationEventExtended MenuModeEndEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.MenuModeEnd_Event, "AutomationElementIdentifiers.MenuModeEndEvent");
    }

    public static class DockPatternIdentifiers
    {
        
        public static readonly AutomationPropertyExtended DockPositionProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Dock_Position_Property, "DockPatternIdentifiers.DockPositionProperty");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.Dock_Pattern, "DockPatternIdentifiers.Pattern");
    }

    public static class ExpandCollapsePatternIdentifiersExtended
    {
        
        public static readonly AutomationPropertyExtended ExpandCollapseStateProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.ExpandCollapse_State_Property, "ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.ExpandCollapse_Pattern, "ExpandCollapsePatternIdentifiers.Pattern");
    }

    public static class GridItemPatternIdentifiers
    {
        
        public static readonly AutomationPropertyExtended ColumnProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.GridItem_Column_Property, "GridItemPatternIdentifiers.ColumnProperty");
        public static readonly AutomationPropertyExtended ColumnSpanProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.GridItem_ColumnSpan_Property, "GridItemPatternIdentifiers.ColumnSpanProperty");
        public static readonly AutomationPropertyExtended ContainingGridProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.GridItem_Parent_Property, "GridItemPatternIdentifiers.ContainingGridProperty");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.GridItem_Pattern, "GridItemPatternIdentifiers.Pattern");
        public static readonly AutomationPropertyExtended RowProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.GridItem_Row_Property, "GridItemPatternIdentifiers.RowProperty");
        public static readonly AutomationPropertyExtended RowSpanProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.GridItem_RowSpan_Property, "GridItemPatternIdentifiers.RowSpanProperty");
    }

    public static class GridPatternIdentifiers
    {
        
        public static readonly AutomationPropertyExtended ColumnCountProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Grid_ColumnCount_Property, "GridPatternIdentifiers.ColumnCountProperty");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.Grid_Pattern, "GridPatternIdentifiers.Pattern");
        public static readonly AutomationPropertyExtended RowCountProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Grid_RowCount_Property, "GridPatternIdentifiers.RowCountProperty");
    }

    public static class InvokePatternIdentifiersExtended
    {
        
        public static readonly AutomationEventExtended InvokedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.Invoke_Invoked_Event, "InvokePatternIdentifiersExtended.InvokedEvent");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.Invoke_Pattern, "InvokePatternIdentifiersExtended.Pattern");
    }

    public static class MultipleViewPatternIdentifiers
    {
        
        public static readonly AutomationPropertyExtended CurrentViewProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.MultipleView_CurrentView_Property, "MultipleViewPatternIdentifiers.CurrentViewProperty");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.MultipleView_Pattern, "MultipleViewPatternIdentifiers.Pattern");
        public static readonly AutomationPropertyExtended SupportedViewsProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.MultipleView_SupportedViews_Property, "MultipleViewPatternIdentifiers.SupportedViewsProperty");
    }

    public static class RangeValuePatternIdentifiers
    {
        
        public static readonly AutomationPropertyExtended IsReadOnlyProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.RangeValue_IsReadOnly_Property, "RangeValuePatternIdentifiers.IsReadOnlyProperty");
        public static readonly AutomationPropertyExtended LargeChangeProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.RangeValue_LargeChange_Property, "RangeValuePatternIdentifiers.LargeChangeProperty");
        public static readonly AutomationPropertyExtended MaximumProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.RangeValue_Maximum_Property, "RangeValuePatternIdentifiers.MaximumProperty");
        public static readonly AutomationPropertyExtended MinimumProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.RangeValue_Minimum_Property, "RangeValuePatternIdentifiers.MinimumProperty");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.RangeValue_Pattern, "RangeValuePatternIdentifiers.Pattern");
        public static readonly AutomationPropertyExtended SmallChangeProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.RangeValue_SmallChange_Property, "RangeValuePatternIdentifiers.SmallChangeProperty");
        public static readonly AutomationPropertyExtended ValueProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.RangeValue_Value_Property, "RangeValuePatternIdentifiers.ValueProperty");
    }

    public static class ScrollItemPatternIdentifiers
    {
        
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.ScrollItem_Pattern, "ScrollItemPatternIdentifiers.Pattern");
    }

    public static class ScrollPatternIdentifiersExtended
    {
        
        public static readonly AutomationPropertyExtended HorizontallyScrollableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Scroll_HorizontallyScrollable_Property, "ScrollPatternIdentifiers.HorizontallyScrollableProperty");
        public static readonly AutomationPropertyExtended HorizontalScrollPercentProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Scroll_HorizontalScrollPercent_Property, "ScrollPatternIdentifiers.HorizontalScrollPercentProperty");
        public static readonly AutomationPropertyExtended HorizontalViewSizeProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Scroll_HorizontalViewSize_Property, "ScrollPatternIdentifiers.HorizontalViewSizeProperty");
        public const double NoScroll = -1.0;
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.Scroll_Pattern, "ScrollPatternIdentifiers.Pattern");
        public static readonly AutomationPropertyExtended VerticallyScrollableProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Scroll_VerticallyScrollable_Property, "ScrollPatternIdentifiers.VerticallyScrollableProperty");
        public static readonly AutomationPropertyExtended VerticalScrollPercentProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Scroll_VerticalScrollPercent_Property, "ScrollPatternIdentifiers.VerticalScrollPercentProperty");
        public static readonly AutomationPropertyExtended VerticalViewSizeProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Scroll_VerticalViewSize_Property, "ScrollPatternIdentifiers.VerticalViewSizeProperty");
    }

    public static class SelectionItemPatternIdentifiersExtended
    {
        
        public static readonly AutomationEventExtended ElementAddedToSelectionEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.SelectionItem_ElementAddedToSelection_Event, "SelectionItemPatternIdentifiers.ElementAddedToSelectionEvent");
        public static readonly AutomationEventExtended ElementRemovedFromSelectionEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.SelectionItem_ElementRemovedFromSelection_Event, "SelectionItemPatternIdentifiers.ElementRemovedFromSelectionEvent");
        public static readonly AutomationEventExtended ElementSelectedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.SelectionItem_ElementSelected_Event, "SelectionItemPatternIdentifiers.ElementSelectedEvent");
        public static readonly AutomationPropertyExtended IsSelectedProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.SelectionItem_IsSelected_Property, "SelectionItemPatternIdentifiers.IsSelectedProperty");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.SelectionItem_Pattern, "SelectionItemPatternIdentifiers.Pattern");
        public static readonly AutomationPropertyExtended SelectionContainerProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.SelectionItem_SelectionContainer_Property, "SelectionItemPatternIdentifiers.SelectionContainerProperty");
    }

    public static class SelectionPatternIdentifiersExtended
    {
        
        public static readonly AutomationPropertyExtended CanSelectMultipleProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Selection_CanSelectMultiple_Property, "SelectionPatternIdentifiers.CanSelectMultipleProperty");
        public static readonly AutomationEventExtended InvalidatedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.Selection_Invalidated_Event, "SelectionPatternIdentifiers.InvalidatedEvent");
        public static readonly AutomationPropertyExtended IsSelectionRequiredProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Selection_IsSelectionRequired_Property, "SelectionPatternIdentifiers.IsSelectionRequiredProperty");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.Selection_Pattern, "SelectionPatternIdentifiers.Pattern");
        public static readonly AutomationPropertyExtended SelectionProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Selection_Selection_Property, "SelectionPatternIdentifiers.SelectionProperty");
    }

    public static class TableItemPatternIdentifiers
    {
        
        public static readonly AutomationPropertyExtended ColumnHeaderItemsProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.TableItem_ColumnHeaderItems_Property, "TableItemPatternIdentifiers.ColumnHeaderItemsProperty");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.TableItem_Pattern, "TableItemPatternIdentifiers.Pattern");
        public static readonly AutomationPropertyExtended RowHeaderItemsProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.TableItem_RowHeaderItems_Property, "TableItemPatternIdentifiers.RowHeaderItemsProperty");
    }

    public static class TablePatternIdentifiers
    {
        
        public static readonly AutomationPropertyExtended ColumnHeadersProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Table_ColumnHeaders_Property, "TablePatternIdentifiers.ColumnHeadersProperty");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.Table_Pattern, "TablePatternIdentifiers.Pattern");
        public static readonly AutomationPropertyExtended RowHeadersProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Table_RowHeaders_Property, "TablePatternIdentifiers.RowHeadersProperty");
        public static readonly AutomationPropertyExtended RowOrColumnMajorProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Table_RowOrColumnMajor_Property, "TablePatternIdentifiers.RowOrColumnMajorProperty");
    }

    public static class TextPatternIdentifiers
    {
        
        public static readonly AutomationTextAttribute AnimationStyleAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_AnimationStyle_Attribute, "TextPatternIdentifiers.AnimationStyleAttribute");
        public static readonly AutomationTextAttribute BackgroundColorAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_BackgroundColor_Attribute, "TextPatternIdentifiers.BackgroundColorAttribute");
        public static readonly AutomationTextAttribute BulletStyleAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_BulletStyle_Attribute, "TextPatternIdentifiers.BulletStyleAttribute");
        public static readonly AutomationTextAttribute CapStyleAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_CapStyle_Attribute, "TextPatternIdentifiers.CapStyleAttribute");
        public static readonly AutomationTextAttribute CultureAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_Culture_Attribute, "TextPatternIdentifiers.CultureAttribute");
        public static readonly AutomationTextAttribute FontNameAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_FontName_Attribute, "TextPatternIdentifiers.FontNameAttribute");
        public static readonly AutomationTextAttribute FontSizeAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_FontSize_Attribute, "TextPatternIdentifiers.FontSizeAttribute");
        public static readonly AutomationTextAttribute FontWeightAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_FontWeight_Attribute, "TextPatternIdentifiers.FontWeightAttribute");
        public static readonly AutomationTextAttribute ForegroundColorAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_ForegroundColor_Attribute, "TextPatternIdentifiers.ForegroundColorAttribute");
        public static readonly AutomationTextAttribute HorizontalTextAlignmentAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_HorizontalTextAlignment_Attribute, "TextPatternIdentifiers.HorizontalTextAlignmentAttribute");
        public static readonly AutomationTextAttribute IndentationFirstLineAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_IndentationFirstLine_Attribute, "TextPatternIdentifiers.IndentationFirstLineAttribute");
        public static readonly AutomationTextAttribute IndentationLeadingAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_IndentationLeading_Attribute, "TextPatternIdentifiers.IndentationLeadingAttribute");
        public static readonly AutomationTextAttribute IndentationTrailingAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_IndentationTrailing_Attribute, "TextPatternIdentifiers.IndentationTrailingAttribute");
        public static readonly AutomationTextAttribute IsHiddenAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_IsHidden_Attribute, "TextPatternIdentifiers.IsHiddenAttribute");
        public static readonly AutomationTextAttribute IsItalicAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_IsItalic_Attribute, "TextPatternIdentifiers.IsItalicAttribute");
        public static readonly AutomationTextAttribute IsReadOnlyAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_IsReadOnly_Attribute, "TextPatternIdentifiers.IsReadOnlyAttribute");
        public static readonly AutomationTextAttribute IsSubscriptAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_IsSubscript_Attribute, "TextPatternIdentifiers.IsSubscriptAttribute");
        public static readonly AutomationTextAttribute IsSuperscriptAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_IsSuperscript_Attribute, "TextPatternIdentifiers.IsSuperscriptAttribute");
        public static readonly AutomationTextAttribute MarginBottomAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_MarginBottom_Attribute, "TextPatternIdentifiers.MarginBottomAttribute");
        public static readonly AutomationTextAttribute MarginLeadingAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_MarginLeading_Attribute, "TextPatternIdentifiers.MarginLeadingAttribute");
        public static readonly AutomationTextAttribute MarginTopAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_MarginTop_Attribute, "TextPatternIdentifiers.MarginTopAttribute");
        public static readonly AutomationTextAttribute MarginTrailingAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_MarginTrailing_Attribute, "TextPatternIdentifiers.MarginTrailingAttribute");
        public static readonly object MixedAttributeValue = AutomationExtended.Factory.ReservedMixedAttributeValue;
        public static readonly AutomationTextAttribute OutlineStylesAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_OutlineStyles_Attribute, "TextPatternIdentifiers.OutlineStylesAttribute");
        public static readonly AutomationTextAttribute OverlineColorAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_OverlineColor_Attribute, "TextPatternIdentifiers.OverlineColorAttribute");
        public static readonly AutomationTextAttribute OverlineStyleAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_OverlineStyle_Attribute, "TextPatternIdentifiers.OverlineStyleAttribute");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.Text_Pattern, "TextPatternIdentifiers.Pattern");
        public static readonly AutomationTextAttribute StrikethroughColorAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_StrikethroughColor_Attribute, "TextPatternIdentifiers.StrikethroughColorAttribute");
        public static readonly AutomationTextAttribute StrikethroughStyleAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_StrikethroughStyle_Attribute, "TextPatternIdentifiers.StrikethroughStyleAttribute");
        public static readonly AutomationTextAttribute TabsAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_Tabs_Attribute, "TextPatternIdentifiers.TabsAttribute");
        public static readonly AutomationEventExtended TextChangedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.Text_TextChanged_Event, "TextPatternIdentifiers.TextChangedEvent");
        public static readonly AutomationTextAttribute TextFlowDirectionsAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_FlowDirections_Attribute, "TextPatternIdentifiers.TextFlowDirectionsAttribute");
        public static readonly AutomationEventExtended TextSelectionChangedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.Text_TextSelectionChanged_Event, "TextPatternIdentifiers.TextSelectionChangedEvent");
        public static readonly AutomationTextAttribute UnderlineColorAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_UnderlineColor_Attribute, "TextPatternIdentifiers.UnderlineColorAttribute");
        public static readonly AutomationTextAttribute UnderlineStyleAttribute = AutomationTextAttribute.Register(AutomationIdentifierGuids.Text_UnderlineStyle_Attribute, "TextPatternIdentifiers.UnderlineStyleAttribute");
    }

    public static class TogglePatternIdentifiersExtended
    {
        
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.Toggle_Pattern, "TogglePatternIdentifiers.Pattern");
        public static readonly AutomationPropertyExtended ToggleStateProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Toggle_State_Property, "TogglePatternIdentifiers.ToggleStateProperty");
    }

    public static class TransformPatternIdentifiers
    {
        
        public static readonly AutomationPropertyExtended CanMoveProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Transform_CanMove_Property, "TransformPatternIdentifiers.CanMoveProperty");
        public static readonly AutomationPropertyExtended CanResizeProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Transform_CanResize_Property, "TransformPatternIdentifiers.CanResizeProperty");
        public static readonly AutomationPropertyExtended CanRotateProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Transform_CanRotate_Property, "TransformPatternIdentifiers.CanRotateProperty");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.Transform_Pattern, "TransformPatternIdentifiers.Pattern");
    }

    public static class ValuePatternIdentifiersExtended
    {
        
        public static readonly AutomationPropertyExtended IsReadOnlyProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Value_IsReadOnly_Property, "ValuePatternIdentifiers.IsReadOnlyProperty");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.Value_Pattern, "ValuePatternIdentifiers.Pattern");
        public static readonly AutomationPropertyExtended ValueProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Value_Property, "ValuePatternIdentifiers.ValueProperty");
    }

    public static class WindowPatternIdentifiers
    {
        
        public static readonly AutomationPropertyExtended CanMaximizeProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Window_CanMaximize_Property, "WindowPatternIdentifiers.CanMaximizeProperty");
        public static readonly AutomationPropertyExtended CanMinimizeProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Window_CanMinimize_Property, "WindowPatternIdentifiers.CanMinimizeProperty");
        public static readonly AutomationPropertyExtended IsModalProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Window_IsModal_Property, "WindowPatternIdentifiers.IsModalProperty");
        public static readonly AutomationPropertyExtended IsTopmostProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Window_IsTopmost_Property, "WindowPatternIdentifiers.IsTopmostProperty");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.Window_Pattern, "WindowPatternIdentifiers.Pattern");
        public static readonly AutomationEventExtended WindowClosedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.Window_Closed_Event, "WindowPatternIdentifiers.WindowClosedProperty");
        public static readonly AutomationPropertyExtended WindowInteractionStateProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Window_InteractionState_Property, "WindowPatternIdentifiers.WindowInteractionStateProperty");
        public static readonly AutomationEventExtended WindowOpenedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.Window_Opened_Event, "WindowPatternIdentifiers.WindowOpenedProperty");
        public static readonly AutomationPropertyExtended WindowVisualStateProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.Window_VisualState_Property, "WindowPatternIdentifiers.WindowVisualStateProperty");
    }

    // New for Windows 7
    //
    public static class LegacyIAccessiblePatternIdentifiersExtended
    {
        public static readonly AutomationPropertyExtended ChildIdProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.LegacyIAccessible_ChildId_Property, "LegacyIAccessiblePatternIdentifiers.ChildIdProperty");
        public static readonly AutomationPropertyExtended NameProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.LegacyIAccessible_Name_Property, "LegacyIAccessiblePatternIdentifiers.NameProperty");
        public static readonly AutomationPropertyExtended ValueProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.LegacyIAccessible_Value_Property, "LegacyIAccessiblePatternIdentifiers.ValueProperty");
        public static readonly AutomationPropertyExtended DescriptionProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.LegacyIAccessible_Description_Property, "LegacyIAccessiblePatternIdentifiers.DescriptionProperty");
        public static readonly AutomationPropertyExtended RoleProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.LegacyIAccessible_Role_Property, "LegacyIAccessiblePatternIdentifiers.RoleProperty");
        public static readonly AutomationPropertyExtended StateProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.LegacyIAccessible_State_Property, "LegacyIAccessiblePatternIdentifiers.StateProperty");
        public static readonly AutomationPropertyExtended HelpProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.LegacyIAccessible_Help_Property, "LegacyIAccessiblePatternIdentifiers.HelpProperty");
        public static readonly AutomationPropertyExtended KeyboardShortcutProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.LegacyIAccessible_KeyboardShortcut_Property, "LegacyIAccessiblePatternIdentifiers.KeyboardShortcutProperty");
        public static readonly AutomationPropertyExtended SelectionProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.LegacyIAccessible_Selection_Property, "LegacyIAccessiblePatternIdentifiers.SelectionProperty");
        public static readonly AutomationPropertyExtended DefaultActionProperty = AutomationPropertyExtended.Register(AutomationIdentifierGuids.LegacyIAccessible_DefaultAction_Property, "LegacyIAccessiblePatternIdentifiers.DefaultActionProperty");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.LegacyIAccessible_Pattern, "LegacyIAccessiblePatternIdentifiers.Pattern");
    }

    public static class ItemContainerPatternIdentifiers
    {
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.ItemContainer_Pattern, "ItemContainerPatternIdentifiers.Pattern");
    }

    public static class VirtualizedItemPatternIdentifiers
    {
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.VirtualizedItem_Pattern, "VirtualizedItemPatternIdentifiers.Pattern");
    }

    public static class SynchronizedInputPatternIdentifiers
    {
        public static readonly AutomationEventExtended InputReachedTargetEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.SynchronizedInput_InputReachedTarget_Event, "SynchronizedInputPatternIdentifiers.InputReachedTargetEvent");
        public static readonly AutomationEventExtended InputReachedOtherElementEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.SynchronizedInput_InputReachedOtherElement_Event, "SynchronizedInputPatternIdentifiers.InputReachedOtherElementEvent");
        public static readonly AutomationEventExtended InputDiscardedEvent = AutomationEventExtended.Register(AutomationIdentifierGuids.SynchronizedInput_InputDiscarded_Event, "SynchronizedInputPatternIdentifiers.InputDiscardedEvent");
        public static readonly AutomationPatternExtended Pattern = AutomationPatternExtended.Register(AutomationIdentifierGuids.SynchronizedInput_Pattern, "SynchronizedInputPatternIdentifiers.Pattern");
    }



    #endregion

    #region Identifier classes
    
    /// <summary>
    /// Core Automation Identifier - essentially a wrapped integer
    /// </summary>
    public class AutomationIdentifier : IComparable
    {
        
        private Guid _guid;
        private int _id;
        private static Hashtable _identifierDirectory = new Hashtable(200, 1f);
        private string _programmaticName;
        private UiaCoreIds.AutomationIdType _type;

        
        internal AutomationIdentifier(UiaCoreIds.AutomationIdType type, int id, Guid guid, string programmaticName)
        {
            this._id = id;
            this._type = type;
            this._guid = guid;
            this._programmaticName = programmaticName;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            return (this.GetHashCode() - obj.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return (obj == this);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal static AutomationIdentifier LookupById(UiaCoreIds.AutomationIdType type, int id)
        {
            AutomationIdentifier identifier;
            lock (_identifierDirectory)
            {
                identifier = (AutomationIdentifier)_identifierDirectory[id];
            }
            if (identifier == null)
            {
                return null;
            }
            if (identifier._type != type)
            {
                return null;
            }
            return identifier;
        }

        internal static AutomationIdentifier Register(UiaCoreIds.AutomationIdType type, Guid guid, string programmaticName)
        {
            int id = UiaCoreIds.UiaLookupId(type, ref guid);
            if (id == 0)
            {
                return null;
            }
            lock (_identifierDirectory)
            {
                AutomationIdentifier identifier = (AutomationIdentifier)_identifierDirectory[guid];
                if (identifier == null)
                {
                    switch (type)
                    {
                        case UiaCoreIds.AutomationIdType.Property:
                            identifier = new AutomationPropertyExtended(id, guid, programmaticName);
                            break;

                        case UiaCoreIds.AutomationIdType.Pattern:
                            identifier = new AutomationPatternExtended(id, guid, programmaticName);
                            break;

                        case UiaCoreIds.AutomationIdType.Event:
                            identifier = new AutomationEventExtended(id, guid, programmaticName);
                            break;

                        case UiaCoreIds.AutomationIdType.ControlType:
                            identifier = new ControlTypeExtended(id, guid, programmaticName);
                            break;

                        case UiaCoreIds.AutomationIdType.TextAttribute:
                            identifier = new AutomationTextAttribute(id, guid, programmaticName);
                            break;

                        default:
                            throw new InvalidOperationException("Invalid type specified for AutomationIdentifier");
                    }
                    _identifierDirectory[id] = identifier;
                }
                return identifier;
            }
        }

        
        public int Id
        {
            get
            {
                return this._id;
            }
        }

        public string ProgrammaticName
        {
            get
            {
                return this._programmaticName;
            }
        }
    }

    public class AutomationEventExtended : AutomationIdentifier
    {
        
        internal AutomationEventExtended(int id, Guid guid, string programmaticName)
            : base(UiaCoreIds.AutomationIdType.Event, id, guid, programmaticName)
        {
        }

        public static AutomationEventExtended LookupById(int id)
        {
            return (AutomationEventExtended)AutomationIdentifier.LookupById(UiaCoreIds.AutomationIdType.Event, id);
        }

        internal static AutomationEventExtended Register(Guid guid, string programmaticName)
        {
            return (AutomationEventExtended)AutomationIdentifier.Register(UiaCoreIds.AutomationIdType.Event, guid, programmaticName);
        }
    }

    public class AutomationPatternExtended : AutomationIdentifier
    {
        
        internal AutomationPatternExtended(int id, Guid guid, string programmaticName)
            : base(UiaCoreIds.AutomationIdType.Pattern, id, guid, programmaticName)
        {
        }

        public static AutomationPatternExtended LookupById(int id)
        {
            return (AutomationPatternExtended)AutomationIdentifier.LookupById(UiaCoreIds.AutomationIdType.Pattern, id);
        }

        internal static AutomationPatternExtended Register(Guid guid, string programmaticName)
        {
            return (AutomationPatternExtended)AutomationIdentifier.Register(UiaCoreIds.AutomationIdType.Pattern, guid, programmaticName);
        }
    }


    public class AutomationPropertyExtended : AutomationIdentifier
    {
        
        internal AutomationPropertyExtended(int id, Guid guid, string programmaticName)
            : base(UiaCoreIds.AutomationIdType.Property, id, guid, programmaticName)
        {
        }

        public static AutomationPropertyExtended LookupById(int id)
        {
            return (AutomationPropertyExtended)AutomationIdentifier.LookupById(UiaCoreIds.AutomationIdType.Property, id);
        }

        internal static AutomationPropertyExtended Register(Guid guid, string programmaticName)
        {
            return (AutomationPropertyExtended)AutomationIdentifier.Register(UiaCoreIds.AutomationIdType.Property, guid, programmaticName);
        }
    }

    public class AutomationTextAttribute : AutomationIdentifier
    {
        
        internal AutomationTextAttribute(int id, Guid guid, string programmaticName)
            : base(UiaCoreIds.AutomationIdType.TextAttribute, id, guid, programmaticName)
        {
        }

        public static AutomationTextAttribute LookupById(int id)
        {
            return (AutomationTextAttribute)AutomationIdentifier.LookupById(UiaCoreIds.AutomationIdType.TextAttribute, id);
        }

        internal static AutomationTextAttribute Register(Guid guid, string programmaticName)
        {
            return (AutomationTextAttribute)AutomationIdentifier.Register(UiaCoreIds.AutomationIdType.TextAttribute, guid, programmaticName);
        }
    }

    public class ControlTypeExtended : AutomationIdentifier
    {
        
        private AutomationPatternExtended[] _neverSupportedPatterns;
        private AutomationPatternExtended[][] _requiredPatternsSets;
        private AutomationPropertyExtended[] _requiredProperties;
        public static readonly ControlTypeExtended Button = Register(AutomationIdentifierGuids.Button_Control, "ControlType.Button", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { InvokePatternIdentifiersExtended.Pattern } });
        public static readonly ControlTypeExtended Calendar = Register(AutomationIdentifierGuids.Calendar_Control, "ControlType.Calendar", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { GridPatternIdentifiers.Pattern, ValuePatternIdentifiersExtended.Pattern, SelectionPatternIdentifiersExtended.Pattern } });
        public static readonly ControlTypeExtended CheckBox = Register(AutomationIdentifierGuids.CheckBox_Control, "ControlType.CheckBox", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { TogglePatternIdentifiersExtended.Pattern } });
        public static readonly ControlTypeExtended ComboBox = Register(AutomationIdentifierGuids.ComboBox_Control, "ControlType.ComboBox", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { SelectionPatternIdentifiersExtended.Pattern, ExpandCollapsePatternIdentifiersExtended.Pattern } });
        public static readonly ControlTypeExtended Custom = Register(AutomationIdentifierGuids.Custom_Control, "ControlType.Custom");
        public static readonly ControlTypeExtended DataGrid = Register(AutomationIdentifierGuids.DataGrid_Control, "ControlType.DataGrid", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { GridPatternIdentifiers.Pattern }, new AutomationPatternExtended[] { SelectionPatternIdentifiersExtended.Pattern }, new AutomationPatternExtended[] { TablePatternIdentifiers.Pattern } });
        public static readonly ControlTypeExtended DataItem = Register(AutomationIdentifierGuids.DataItem_Control, "ControlType.DataItem", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { SelectionItemPatternIdentifiersExtended.Pattern } });
        public static readonly ControlTypeExtended Document = Register(AutomationIdentifierGuids.Document_Control, "ControlType.Document", new AutomationPropertyExtended[0], new AutomationPatternExtended[] { ValuePatternIdentifiersExtended.Pattern }, new AutomationPatternExtended[][] { new AutomationPatternExtended[] { ScrollPatternIdentifiersExtended.Pattern }, new AutomationPatternExtended[] { TextPatternIdentifiers.Pattern } });
        public static readonly ControlTypeExtended Edit = Register(AutomationIdentifierGuids.Edit_Control, "ControlType.Edit", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { ValuePatternIdentifiersExtended.Pattern } });
        public static readonly ControlTypeExtended Group = Register(AutomationIdentifierGuids.Group_Control, "ControlType.Group");
        public static readonly ControlTypeExtended Header = Register(AutomationIdentifierGuids.Header_Control, "ControlType.Header");
        public static readonly ControlTypeExtended HeaderItem = Register(AutomationIdentifierGuids.HeaderItem_Control, "ControlType.HeaderItem");
        public static readonly ControlTypeExtended Hyperlink = Register(AutomationIdentifierGuids.Hyperlink_Control, "ControlType.Hyperlink", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { InvokePatternIdentifiersExtended.Pattern } });
        public static readonly ControlTypeExtended Image = Register(AutomationIdentifierGuids.Image_Control, "ControlType.Image");
        public static readonly ControlTypeExtended List = Register(AutomationIdentifierGuids.List_Control, "ControlType.List", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { SelectionPatternIdentifiersExtended.Pattern, TablePatternIdentifiers.Pattern, GridPatternIdentifiers.Pattern, MultipleViewPatternIdentifiers.Pattern } });
        public static readonly ControlTypeExtended ListItem = Register(AutomationIdentifierGuids.ListItem_Control, "ControlType.ListItem", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { SelectionItemPatternIdentifiersExtended.Pattern } });
        public static readonly ControlTypeExtended Menu = Register(AutomationIdentifierGuids.Menu_Control, "ControlType.Menu");
        public static readonly ControlTypeExtended MenuBar = Register(AutomationIdentifierGuids.MenuBar_Control, "ControlType.MenuBar");
        public static readonly ControlTypeExtended MenuItem = Register(AutomationIdentifierGuids.MenuItem_Control, "ControlType.MenuItem", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { InvokePatternIdentifiersExtended.Pattern }, new AutomationPatternExtended[] { ExpandCollapsePatternIdentifiersExtended.Pattern }, new AutomationPatternExtended[] { TogglePatternIdentifiersExtended.Pattern } });
        public static readonly ControlTypeExtended Pane = Register(AutomationIdentifierGuids.Pane_Control, "ControlType.Pane", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { TransformPatternIdentifiers.Pattern } });
        public static readonly ControlTypeExtended ProgressBar = Register(AutomationIdentifierGuids.ProgressBar_Control, "ControlType.ProgressBar", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { ValuePatternIdentifiersExtended.Pattern } });
        public static readonly ControlTypeExtended RadioButton = Register(AutomationIdentifierGuids.RadioButton_Control, "ControlType.RadioButton");
        public static readonly ControlTypeExtended ScrollBar = Register(AutomationIdentifierGuids.ScrollBar_Control, "ControlType.ScrollBar");
        public static readonly ControlTypeExtended Separator = Register(AutomationIdentifierGuids.Separator_Control, "ControlType.Separator");
        public static readonly ControlTypeExtended Slider = Register(AutomationIdentifierGuids.Slider_Control, "ControlType.Slider", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { RangeValuePatternIdentifiers.Pattern }, new AutomationPatternExtended[] { SelectionPatternIdentifiersExtended.Pattern } });
        public static readonly ControlTypeExtended Spinner = Register(AutomationIdentifierGuids.Spinner_Control, "ControlType.Spinner", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { RangeValuePatternIdentifiers.Pattern }, new AutomationPatternExtended[] { SelectionPatternIdentifiersExtended.Pattern } });
        public static readonly ControlTypeExtended SplitButton = Register(AutomationIdentifierGuids.SplitButton_Control, "ControlType.SplitButton", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { InvokePatternIdentifiersExtended.Pattern }, new AutomationPatternExtended[] { ExpandCollapsePatternIdentifiersExtended.Pattern } });
        public static readonly ControlTypeExtended StatusBar = Register(AutomationIdentifierGuids.StatusBar_Control, "ControlType.StatusBar");
        public static readonly ControlTypeExtended Tab = Register(AutomationIdentifierGuids.Tab_Control, "ControlType.Tab");
        public static readonly ControlTypeExtended TabItem = Register(AutomationIdentifierGuids.TabItem_Control, "ControlType.TabItem");
        public static readonly ControlTypeExtended Table = Register(AutomationIdentifierGuids.Table_Control, "ControlType.Table", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { GridPatternIdentifiers.Pattern }, new AutomationPatternExtended[] { SelectionPatternIdentifiersExtended.Pattern }, new AutomationPatternExtended[] { TablePatternIdentifiers.Pattern } });
        public static readonly ControlTypeExtended Text = Register(AutomationIdentifierGuids.Text_Control, "ControlType.Text");
        public static readonly ControlTypeExtended Thumb = Register(AutomationIdentifierGuids.Thumb_Control, "ControlType.Thumb");
        public static readonly ControlTypeExtended TitleBar = Register(AutomationIdentifierGuids.TitleBar_Control, "ControlType.TitleBar");
        public static readonly ControlTypeExtended ToolBar = Register(AutomationIdentifierGuids.ToolBar_Control, "ControlType.ToolBar");
        public static readonly ControlTypeExtended ToolTip = Register(AutomationIdentifierGuids.ToolTip_Control, "ControlType.ToolTip");
        public static readonly ControlTypeExtended Tree = Register(AutomationIdentifierGuids.Tree_Control, "ControlType.Tree");
        public static readonly ControlTypeExtended TreeItem = Register(AutomationIdentifierGuids.TreeItem_Control, "ControlType.TreeItem");
        public static readonly ControlTypeExtended Window = Register(AutomationIdentifierGuids.Window_Control, "ControlType.Window", new AutomationPatternExtended[][] { new AutomationPatternExtended[] { TransformPatternIdentifiers.Pattern }, new AutomationPatternExtended[] { WindowPatternIdentifiers.Pattern } });

        
        internal ControlTypeExtended(int id, Guid guid, string programmaticName)
            : base(UiaCoreIds.AutomationIdType.ControlType, id, guid, programmaticName)
        {
        }

        public AutomationPatternExtended[] GetNeverSupportedPatterns()
        {
            return (AutomationPatternExtended[])this._neverSupportedPatterns.Clone();
        }

        public AutomationPatternExtended[][] GetRequiredPatternSets()
        {
            int length = this._requiredPatternsSets.Length;
            AutomationPatternExtended[][] patternArray = new AutomationPatternExtended[length][];
            for (int i = 0; i < length; i++)
            {
                patternArray[i] = (AutomationPatternExtended[])this._requiredPatternsSets[i].Clone();
            }
            return patternArray;
        }

        public AutomationPropertyExtended[] GetRequiredProperties()
        {
            return (AutomationPropertyExtended[])this._requiredProperties.Clone();
        }

        public static ControlTypeExtended LookupById(int id)
        {
            return (ControlTypeExtended)AutomationIdentifier.LookupById(UiaCoreIds.AutomationIdType.ControlType, id);
        }

        internal static ControlTypeExtended Register(Guid guid, string programmaticName)
        {
            return (ControlTypeExtended)AutomationIdentifier.Register(UiaCoreIds.AutomationIdType.ControlType, guid, programmaticName);
        }

        internal static ControlTypeExtended Register(Guid guid, string programmaticName, AutomationPropertyExtended[] requiredProperties)
        {
            return Register(guid, programmaticName, requiredProperties, new AutomationPatternExtended[0], new AutomationPatternExtended[0][]);
        }

        internal static ControlTypeExtended Register(Guid guid, string programmaticName, AutomationPatternExtended[][] requiredPatternsSets)
        {
            return Register(guid, programmaticName, new AutomationPropertyExtended[0], new AutomationPatternExtended[0], requiredPatternsSets);
        }

        internal static ControlTypeExtended Register(Guid guid, string programmaticName, AutomationPropertyExtended[] requiredProperties, AutomationPatternExtended[] neverSupportedPatterns, AutomationPatternExtended[][] requiredPatternsSets)
        {
            ControlTypeExtended type = (ControlTypeExtended)AutomationIdentifier.Register(UiaCoreIds.AutomationIdType.ControlType, guid, programmaticName);
            type._requiredPatternsSets = requiredPatternsSets;
            type._neverSupportedPatterns = neverSupportedPatterns;
            type._requiredProperties = requiredProperties;
            return type;
        }

        
        public string LocalizedControlType
        {
            get
            {
                throw new NotImplementedException("UI Automation COM API does not have a matching method");
            }
        }
    }

    #endregion

    #region Exceptions

    
    public class ElementNotAvailableExceptionExtended : SystemException
    {
        public ElementNotAvailableExceptionExtended()
            : base("Element not available")
        {
            base.HResult = UiaCoreIds.UIA_E_ELEMENTNOTAVAILABLE;
        }

        public ElementNotAvailableExceptionExtended(Exception innerException)
            : base("Element not available", innerException)
        {
            base.HResult = UiaCoreIds.UIA_E_ELEMENTNOTAVAILABLE;
        }

        public ElementNotAvailableExceptionExtended(string message)
            : base(message)
        {
            base.HResult = UiaCoreIds.UIA_E_ELEMENTNOTAVAILABLE;
        }

        protected ElementNotAvailableExceptionExtended(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            base.HResult = UiaCoreIds.UIA_E_ELEMENTNOTAVAILABLE;
        }

        public ElementNotAvailableExceptionExtended(string message, Exception innerException)
            : base(message, innerException)
        {
            base.HResult = UiaCoreIds.UIA_E_ELEMENTNOTAVAILABLE;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }


    
    public class ElementNotEnabledException : InvalidOperationException
    {
        public ElementNotEnabledException()
            : base("Element not enabled")
        {
            base.HResult = UiaCoreIds.UIA_E_ELEMENTNOTENABLED;
        }

        public ElementNotEnabledException(Exception innerException)
            : base("Element not enabled", innerException)
        {
            base.HResult = UiaCoreIds.UIA_E_ELEMENTNOTENABLED;
        }

        public ElementNotEnabledException(string message)
            : base(message)
        {
            base.HResult = UiaCoreIds.UIA_E_ELEMENTNOTENABLED;
        }

        protected ElementNotEnabledException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            base.HResult = UiaCoreIds.UIA_E_ELEMENTNOTENABLED;
        }

        public ElementNotEnabledException(string message, Exception innerException)
            : base(message, innerException)
        {
            base.HResult = UiaCoreIds.UIA_E_ELEMENTNOTENABLED;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    
    public class NoClickablePointException : Exception
    {
        
        public NoClickablePointException()
        {
        }

        public NoClickablePointException(Exception innerException) :
            base(String.Empty, innerException)
        {
        }

        public NoClickablePointException(string message)
            : base(message)
        {
        }

        protected NoClickablePointException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public NoClickablePointException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    
    public class ProxyAssemblyNotLoadedException : Exception
    {
        
        public ProxyAssemblyNotLoadedException()
        {
        }

        public ProxyAssemblyNotLoadedException(Exception innerException) :
            base(String.Empty, innerException)
        {
        }

        public ProxyAssemblyNotLoadedException(string message)
            : base(message)
        {
        }

        protected ProxyAssemblyNotLoadedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ProxyAssemblyNotLoadedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    #endregion

    #region Enums

    [Guid("70d46e77-e3a8-449d-913c-e30eb2afecdb"), ComVisible(true)]
    public enum DockPosition
    {
        Top,
        Left,
        Bottom,
        Right,
        Fill,
        None
    }

    [Guid("76d12d7e-b227-4417-9ce2-42642ffa896a"), ComVisible(true)]
    public enum ExpandCollapseState
    {
        Collapsed,
        Expanded,
        PartiallyExpanded,
        LeafNode
    }

    [Guid("5F8A77B4-E685-48c1-94D0-8BB6AFA43DF9"), ComVisible(true)]
    public enum OrientationTypeExtended
    {
        None,
        Horizontal,
        Vertical
    }

    [ComVisible(true), Guid("15fdf2e2-9847-41cd-95dd-510612a025ea")]
    public enum RowOrColumnMajor
    {
        RowMajor,
        ColumnMajor,
        Indeterminate
    }

    [ComVisible(true), Guid("bd52d3c7-f990-4c52-9ae3-5c377e9eb772")]
    public enum ScrollAmount
    {
        LargeDecrement,
        SmallDecrement,
        NoAmount,
        LargeIncrement,
        SmallIncrement
    }

    [Flags, ComVisible(true), Guid("3d9e3d8f-bfb0-484f-84ab-93ff4280cbc4")]
    public enum SupportedTextSelection
    {
        None,
        Single,
        Multiple
    }

    [Guid("ad7db4af-7166-4478-a402-ad5b77eab2fa"), ComVisible(true)]
    public enum ToggleStateExtended
    {
        Off,
        On,
        Indeterminate
    }

    [Flags]
    public enum TreeScopeExtended
    {
        Element = 1,
        Children = 2,
        Descendants = 4,
        Subtree = 7,
        Parent = 8,
        Ancestors = 16,
    }

    [Guid("65101cc7-7904-408e-87a7-8c6dbd83a18b"), ComVisible(true)]
    public enum WindowInteractionStateExtended
    {
        Running,
        Closing,
        ReadyForUserInteraction,
        BlockedByModalWindow,
        NotResponding
    }

    [ComVisible(true), Guid("fdc8f176-aed2-477a-8c89-ea04cc5f278d")]
    public enum WindowVisualStateExtended
    {
        Normal,
        Maximized,
        Minimized
    }

    // New for Windows 7
    //

    [Flags]
    public enum SynchronizedInputType
    {
        KeyUp =         0x01,
        KeyDown =       0x02,
        LeftMouseUp =   0x04,
        LeftMouseDown = 0x08,
        RightMouseUp =  0x10,
        RightMouseDown =0x20
    };

    #endregion

}