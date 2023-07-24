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

using System.Runtime.InteropServices;
using System.Windows.Automation.Text;
using UIAComWrapperInternal;
using Windows.Foundation;

// Provider interfaces.
// IRawElementProviderSimple is defined in the interop DLL,
// since the Client code refers to it.  Everything else is here.

namespace System.Windows.Automation.Providers
{
    [Guid("670c3006-bf4c-428b-8534-e1848f645122")]
    [ComVisible(true)]
    public enum NavigateDirection
    {
        Parent,
        NextSibling,
        PreviousSibling,
        FirstChild,
        LastChild
    }

    [Flags]
    public enum ProviderOptions
    {
        ClientSideProvider = 1,
        ServerSideProvider = 2,
        NonClientAreaProvider = 4,
        OverrideProvider = 8,
        ProviderOwnsSetFocus = 16,
        UseComThreading = 32,
    }

  
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("159bc72c-4ad3-485e-9637-d7052edf0146")]
    [ComVisible(true)]
    public interface IDockProvider
    {
        void SetDockPosition(DockPosition dockPosition);
        DockPosition DockPosition { get; }
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("d847d3a5-cab0-4a98-8c32-ecb45c59ad24")]
    [ComVisible(true)]
    public interface IExpandCollapseProvider
    {
        void Expand();
        void Collapse();
        ExpandCollapseState ExpandCollapseState { get; }
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("d02541f1-fb81-4d64-ae32-f520f8a6dbd1")]
    [ComVisible(true)]
    public interface IGridItemProvider
    {
        int Row { get; }
        int Column { get; }
        int RowSpan { get; }
        int ColumnSpan { get; }
        IRawElementProviderSimple ContainingGrid { get; }
    }

    [ComVisible(true)]
    [Guid("b17d6187-0907-464b-a168-0ef17a1572b1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IGridProvider
    {
        IRawElementProviderSimple GetItem(int row, int column);
        int RowCount { get; }
        int ColumnCount { get; }
    }

    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("54fcb24b-e18e-47a2-b4d3-eccbe77599a2")]
    public interface IInvokeProvider
    {
        void Invoke();
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    [Guid("6278cab1-b556-4a1a-b4e0-418acc523201")]
    public interface IMultipleViewProvider
    {
        string GetViewName(int viewId);
        void SetCurrentView(int viewId);
        int CurrentView { get; }
        int[] GetSupportedViews();
    }

    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("36dc7aef-33e6-4691-afe1-2be7274b3d33")]
    public interface IRangeValueProvider
    {
        void SetValue(double value);
        double Value { get; }
        bool IsReadOnly { [return: MarshalAs(UnmanagedType.Bool)] get; }
        double Maximum { get; }
        double Minimum { get; }
        double LargeChange { get; }
        double SmallChange { get; }
    }

    [Guid("a407b27b-0f6d-4427-9292-473c7bf93258")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IRawElementProviderAdviseEvents : IRawElementProviderSimple
    {
        void AdviseEventAdded(int eventId, int[] properties);
        void AdviseEventRemoved(int eventId, int[] properties);
    }

    [Guid("f7063da8-8359-439c-9297-bbc5299a7d87")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IRawElementProviderFragment : IRawElementProviderSimple
    {
        IRawElementProviderFragment Navigate(NavigateDirection direction);
        int[] GetRuntimeId();
        Rect BoundingRectangle { get; }
        IRawElementProviderSimple[] GetEmbeddedFragmentRoots();
        void SetFocus();
        IRawElementProviderFragmentRoot FragmentRoot { get; }
    }

    [ComVisible(true)]
    [Guid("620ce2a5-ab8f-40a9-86cb-de3c75599b58")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IRawElementProviderFragmentRoot : IRawElementProviderFragment, IRawElementProviderSimple
    {
        IRawElementProviderFragment ElementProviderFromPoint(double x, double y);
        IRawElementProviderFragment GetFocus();
    }

    [ComVisible(true)]
    [Guid("1d5df27c-8947-4425-b8d9-79787bb460b8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IRawElementProviderHwndOverride : IRawElementProviderSimple
    {
        IRawElementProviderSimple GetOverrideProviderForHwnd(IntPtr hwnd);
    }

    [Guid("2360c714-4bf1-4b26-ba65-9b21316127eb")]
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IScrollItemProvider
    {
        void ScrollIntoView();
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    [Guid("b38b8077-1fc3-42a5-8cae-d40c2215055a")]
    public interface IScrollProvider
    {
        void Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount);
        void SetScrollPercent(double horizontalPercent, double verticalPercent);
        double HorizontalScrollPercent { get; }
        double VerticalScrollPercent { get; }
        double HorizontalViewSize { get; }
        double VerticalViewSize { get; }
        bool HorizontallyScrollable { [return: MarshalAs(UnmanagedType.Bool)] get; }
        bool VerticallyScrollable { [return: MarshalAs(UnmanagedType.Bool)] get; }
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    [Guid("2acad808-b2d4-452d-a407-91ff1ad167b2")]
    public interface ISelectionItemProvider
    {
        void Select();
        void AddToSelection();
        void RemoveFromSelection();
        bool IsSelected { [return: MarshalAs(UnmanagedType.Bool)] get; }
        IRawElementProviderSimple SelectionContainer { get; }
    }

    [ComVisible(true)]
    [Guid("fb8b03af-3bdf-48d4-bd36-1a65793be168")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISelectionProvider
    {
        IRawElementProviderSimple[] GetSelection();
        bool CanSelectMultiple { [return: MarshalAs(UnmanagedType.Bool)] get; }
        bool IsSelectionRequired { [return: MarshalAs(UnmanagedType.Bool)] get; }
    }

    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("b9734fa6-771f-4d78-9c90-2517999349cd")]
    public interface ITableItemProvider : IGridItemProvider
    {
        IRawElementProviderSimple[] GetRowHeaderItems();
        IRawElementProviderSimple[] GetColumnHeaderItems();
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    [Guid("9c860395-97b3-490a-b52a-858cc22af166")]
    public interface ITableProvider : IGridProvider
    {
        IRawElementProviderSimple[] GetRowHeaders();
        IRawElementProviderSimple[] GetColumnHeaders();
        RowOrColumnMajor RowOrColumnMajor { get; }
    }

    [Guid("3589c92c-63f3-4367-99bb-ada653b77cf2")]
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITextProvider
    {
        ITextRangeProvider[] GetSelection();
        ITextRangeProvider[] GetVisibleRanges();
        ITextRangeProvider RangeFromChild(IRawElementProviderSimple childElement);
        ITextRangeProvider RangeFromPoint(Point screenLocation);
        ITextRangeProvider DocumentRange { get; }
        SupportedTextSelection SupportedTextSelection { get; }
    }

    [Guid("5347ad7b-c355-46f8-aff5-909033582f63")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface ITextRangeProvider
    {
        ITextRangeProvider Clone();
        [return: MarshalAs(UnmanagedType.Bool)]
        bool Compare(ITextRangeProvider range);
        int CompareEndpoints(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint);
        void ExpandToEnclosingUnit(TextUnit unit);
        ITextRangeProvider FindAttribute(int attribute, object value, [MarshalAs(UnmanagedType.Bool)] bool backward);
        ITextRangeProvider FindText(string text, [MarshalAs(UnmanagedType.Bool)] bool backward, [MarshalAs(UnmanagedType.Bool)] bool ignoreCase);
        object GetAttributeValue(int attribute);
        double[] GetBoundingRectangles();
        IRawElementProviderSimple GetEnclosingElement();
        string GetText(int maxLength);
        int Move(TextUnit unit, int count);
        int MoveEndpointByUnit(TextPatternRangeEndpoint endpoint, TextUnit unit, int count);
        void MoveEndpointByRange(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint);
        void Select();
        void AddToSelection();
        void RemoveFromSelection();
        void ScrollIntoView([MarshalAs(UnmanagedType.Bool)] bool alignToTop);
        IRawElementProviderSimple[] GetChildren();
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("56d00bd0-c4f4-433c-a836-1a52a57e0892")]
    [ComVisible(true)]
    public interface IToggleProvider
    {
        void Toggle();
        ToggleStateExtended ToggleState { get; }
    }

    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("6829ddc4-4f91-4ffa-b86f-bd3e2987cb4c")]
    public interface ITransformProvider
    {
        void Move(double x, double y);
        void Resize(double width, double height);
        void Rotate(double degrees);
        bool CanMove { [return: MarshalAs(UnmanagedType.Bool)] get; }
        bool CanResize { [return: MarshalAs(UnmanagedType.Bool)] get; }
        bool CanRotate { [return: MarshalAs(UnmanagedType.Bool)] get; }
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("c7935180-6fb3-4201-b174-7df73adbf64a")]
    [ComVisible(true)]
    public interface IValueProvider
    {
        void SetValue([MarshalAs(UnmanagedType.LPWStr)] string value);
        string Value { get; }
        bool IsReadOnly { [return: MarshalAs(UnmanagedType.Bool)] get; }
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("987df77b-db06-4d77-8f8a-86a9c3bb90b9")]
    [ComVisible(true)]
    public interface IWindowProvider
    {
        void SetVisualState(WindowVisualStateExtended state);
        void Close();
        [return: MarshalAs(UnmanagedType.Bool)]
        bool WaitForInputIdle(int milliseconds);
        bool Maximizable { [return: MarshalAs(UnmanagedType.Bool)] get; }
        bool Minimizable { [return: MarshalAs(UnmanagedType.Bool)] get; }
        bool IsModal { [return: MarshalAs(UnmanagedType.Bool)] get; }
        WindowVisualStateExtended VisualState { get; }
        WindowInteractionStateExtended InteractionState { get; }
        bool IsTopmost { [return: MarshalAs(UnmanagedType.Bool)] get; }
    }

    // New for Windows 7
    //
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("E747770B-39CE-4382-AB30-D8FB3F336F24")]
    [ComVisible(true)]
    public interface IItemContainerProvider
    {
        IRawElementProviderSimple FindItemByProperty(IRawElementProviderSimple pStartAfter, int propertyId, object Value);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("E44C3566-915D-4070-99C6-047BFF5A08F5")]
    [ComVisible(true)]
    public interface ILegacyIAccessibleProvider
    {
        void Select(int flagsSelect);
        void DoDefaultAction();
        void SetValue([MarshalAs(UnmanagedType.LPWStr)] string szValue);
        [return: MarshalAs(UnmanagedType.Interface)]
        Accessibility.IAccessible GetIAccessible();
        int ChildId {  get; }
        string Name { get; }
        string Value { get; }
        string Description { get; }
        uint Role {  get; }
        uint state {  get; }
        string Help { get; }
        string KeyboardShortcut { get; }
        IRawElementProviderSimple[] GetSelection();
        string DefaultAction { get; }
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("29DB1A06-02CE-4CF7-9B42-565D4FAB20EE")]
    [ComVisible(true)]
    public interface ISynchronizedInputProvider
    {
        void StartListening([In] SynchronizedInputType inputType);
        void Cancel();
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CB98B665-2D35-4FAC-AD35-F3C60D0C0B8B")]
    [ComVisible(true)]
    public interface IVirtualizedItemProvider
    {
        void Realize();
    }
}