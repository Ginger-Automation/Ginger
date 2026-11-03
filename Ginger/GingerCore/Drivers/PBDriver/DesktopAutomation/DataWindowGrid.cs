#region License
/*
Copyright © 2014-2026 European Support Limited

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

using Accessibility;
using Amdocs.Ginger.Common;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GingerCore.Drivers.PBDriver.DesktopAutomation
{
    /// <summary>
    /// One cell of a PowerBuilder DataWindow, as the DataWindow's accessible object reports it.
    /// </summary>
    /// <remarks>
    /// A cell is only a handle on the control, so what it shows is read when it is asked for.
    /// </remarks>
    public sealed class DataWindowCell
    {
        private const int StateChecked = 0x10;
        private const int RoleCheckButton = 0x2C;

        private readonly IAccessible mAccessible;
        private readonly object mChildId;

        private DataWindowCell(string name, IAccessible accessible, object childId)
        {
            Name = name;
            mAccessible = accessible;
            mChildId = childId;
        }

        /// <summary>
        /// The DataWindow's name for the cell, which is the column it belongs to, such as item_code.
        /// It is not the caption drawn in the header.
        /// </summary>
        public string Name { get; }

        /// <summary>What the cell shows. Null when it reports nothing.</summary>
        public string Value => Read(() => mAccessible.get_accValue(mChildId));

        /// <summary>What the cell shows, and for a check box whether it is checked.</summary>
        public string Text => IsCheckBox ? (IsChecked ? "Checked" : "Unchecked") : Value ?? string.Empty;

        public bool IsCheckBox => Flags(Read(() => mAccessible.get_accRole(mChildId))) == RoleCheckButton;

        public bool IsChecked => (Flags(Read(() => mAccessible.get_accState(mChildId))) & StateChecked) != 0;

        /// <summary>
        /// The cell for an accessible object, or null when it is not one to automate: one with no name
        /// to find it by, or one that does not answer.
        /// </summary>
        /// <remarks>
        /// A cell is not left out for the state the DataWindow reports. PowerBuilder 12.5 flags every
        /// cell as invisible, drawn or not, so the flag does not say which cells are shown.
        /// </remarks>
        public static DataWindowCell TryCreate(IAccessible accessible, object childId)
        {
            try
            {
                if (accessible == null)
                {
                    return null;
                }

                string name = accessible.get_accName(childId);
                return string.IsNullOrWhiteSpace(name) ? null : new DataWindowCell(name, accessible, childId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>The cell's rectangle on the screen. False when it has none.</summary>
        public bool TryGetBounds(out Rectangle bounds)
        {
            try
            {
                mAccessible.accLocation(out int left, out int top, out int width, out int height, mChildId);
                bounds = new Rectangle(left, top, width, height);
                return width > 0 && height > 0;
            }
            catch (Exception)
            {
                bounds = Rectangle.Empty;
                return false;
            }
        }

        public bool TryDoDefaultAction()
        {
            try
            {
                mAccessible.accDoDefaultAction(mChildId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static int Flags(object value)
        {
            try
            {
                return value == null ? 0 : unchecked((int)Convert.ToInt64(value));
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static T Read<T>(Func<T> read)
        {
            try
            {
                return read();
            }
            catch (Exception)
            {
                return default;
            }
        }
    }

    /// <summary>
    /// The cells of a PowerBuilder DataWindow, read through its accessible object and grouped the
    /// way a table action addresses them: by column, then by row.
    /// </summary>
    /// <remarks>
    /// Used where UI Automation does not list the cells at all, which is the case for PowerBuilder 12.5:
    /// it reaches UI Automation through the MSAA proxy of Windows, the proxy leaves out every child
    /// flagged invisible, and PowerBuilder 12.5 flags every cell. The DataWindow still answers MSAA,
    /// which names every cell after its column and carries what it shows as the value, and none of
    /// that needs focus, the foreground window or an unlocked desktop.
    /// </remarks>
    public sealed class DataWindowGrid
    {
        private const int ChildIdSelf = 0;
        private const int MaxCells = 20000;
        private const int SampledCells = 8;
        private const string HeaderSuffix = "_t";

        // How a DataWindow is scrolled: by the messages its scroll bars send, or, where it does not take
        // those, by the ones the wheel of a mouse sends. The codes are the ones of the scroll bar messages,
        // which are the same whichever way the bar lies.
        private const uint WmHScroll = 0x0114;
        private const uint WmVScroll = 0x0115;
        private const uint WmMouseWheel = 0x020A;
        private const int SbLineUp = 0;
        private const int SbLineDown = 1;
        private const int SbPageUp = 2;
        private const int SbPageDown = 3;
        private const int WheelDelta = 120;
        private const uint SmtoAbortIfHung = 0x0002;
        private const uint ScrollMessageTimeoutMs = 2000;
        private const int KindLine = 0;
        private const int KindPage = 1;
        private const int KindWheel = 2;
        private const int ScrollKinds = 3;
        private const int MaxScrollPasses = 12;
        private const int MaxScrollBatch = 200;
        private const int SettleChecks = 3;
        private const int SettleStepMs = 50;

        // The header of a DataWindow lies along the top of its client area, and is no taller than half of it.
        private const int HeaderSlack = 8;
        private const int MaxHeaderCells = 64;

        /// <summary>
        /// The most time a read may take, which is far more than a DataWindow needs to answer and keeps
        /// a control that has stopped answering from holding the run.
        /// </summary>
        private static readonly TimeSpan ReadBudget = TimeSpan.FromSeconds(15);

        /// <summary>The most time scrolling a cell into view may take.</summary>
        private static readonly TimeSpan ScrollBudget = TimeSpan.FromSeconds(20);

        private readonly Dictionary<string, List<DataWindowCell>> mByName = new Dictionary<string, List<DataWindowCell>>(StringComparer.Ordinal);
        private readonly List<DataWindowCell> mCells;
        private readonly List<string> mColumnNames;

        [DllImport("oleacc.dll")]
        private static extern int AccessibleChildren(IAccessible paccContainer, int iChildStart, int cChildren, [Out] object[] rgvarChildren, out int pcObtained);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessageTimeout(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hwnd, out RECT rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ClientToScreen(IntPtr hwnd, ref POINT point);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDpiAwarenessContext(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        /// <param name="listingCutShort">True where the DataWindow lists more cells than were read.</param>
        public DataWindowGrid(IEnumerable<DataWindowCell> cells, bool listingCutShort = false)
        {
            mCells = new List<DataWindowCell>(cells);
            List<string> names = new List<string>(mCells.Count);
            foreach (DataWindowCell cell in mCells)
            {
                if (!mByName.TryGetValue(cell.Name, out List<DataWindowCell> sameName))
                {
                    sameName = new List<DataWindowCell>();
                    mByName.Add(cell.Name, sameName);
                }

                sameName.Add(cell);
                names.Add(cell.Name);
            }

            Describe(names, out List<string> columns, out int rowCount, listingCutShort);
            mColumnNames = columns;
            RowCount = rowCount;
        }

        /// <summary>Every cell, in the order the DataWindow lists them.</summary>
        public IReadOnlyList<DataWindowCell> Cells => mCells;

        /// <summary>
        /// The names that have a cell on every row, in the order they are first listed. These are the
        /// grid's columns, as UI Automation lists them for a grid with several rows that it can see into,
        /// with any column the DataWindow does not draw as well, which UI Automation leaves out.
        /// </summary>
        public IReadOnlyList<string> ColumnNames => mColumnNames;

        /// <summary>
        /// The number of rows the DataWindow lists, which is the most cells any one name has. A row the
        /// DataWindow has scrolled out of view is listed, and counted, as well.
        /// </summary>
        public int RowCount { get; }

        public bool HasColumn(string name)
        {
            return name != null && mByName.ContainsKey(name);
        }

        /// <summary>The cells with this name, one for each row they appear on.</summary>
        public IReadOnlyList<DataWindowCell> Column(string name)
        {
            if (name != null && mByName.TryGetValue(name, out List<DataWindowCell> cells))
            {
                return cells;
            }

            return [];
        }

        /// <summary>
        /// The caption the header of a column shows, such as Item Code for item_code, or null when the column
        /// has no header. PowerBuilder names the header of a column after it with _t added.
        /// </summary>
        public string CaptionOf(string column)
        {
            if (column != null
                && mByName.TryGetValue(column + HeaderSuffix, out List<DataWindowCell> headers)
                && headers.Count == 1)
            {
                string caption = Normalize(headers[0].Value);
                return caption.Length == 0 ? null : caption;
            }

            return null;
        }

        /// <summary>
        /// The columns whose header shows this caption, which is how a person reads a column off the screen.
        /// The case and the white space are not compared, as a header may break its caption over lines.
        /// </summary>
        public IReadOnlyList<string> ColumnsCaptioned(string caption)
        {
            List<string> columns = [];
            string wanted = Normalize(caption);
            if (wanted.Length > 0)
            {
                foreach (string column in mColumnNames)
                {
                    if (string.Equals(CaptionOf(column), wanted, StringComparison.OrdinalIgnoreCase))
                    {
                        columns.Add(column);
                    }
                }
            }

            return columns;
        }

        /// <summary>
        /// The columns by name, each with the caption its header shows where it has one, as in
        /// item_code (Item Code).
        /// </summary>
        public string DescribeColumns()
        {
            if (mColumnNames.Count == 0)
            {
                return "none, as the grid lists no rows";
            }

            List<string> described = new List<string>(mColumnNames.Count);
            foreach (string column in mColumnNames)
            {
                string caption = CaptionOf(column);
                described.Add(caption == null ? column : column + " (" + caption + ")");
            }

            return string.Join(", ", described);
        }

        private static string Normalize(string text)
        {
            return string.Join(" ", (text ?? string.Empty).Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Which of the names are columns, and how many rows there are, for the cell names in the
        /// order the DataWindow lists them. A header or footer appears once and a column appears once
        /// for every row, so the columns are the names that appear most.
        /// </summary>
        /// <remarks>
        /// A grid with a single row cannot be told that way, as its headers appear as often as its columns
        /// do. A header is named after its column with _t added, so with one row a name that is the header
        /// of another name listed is left out, and the columns stay the same whether the grid shows one row
        /// or many. A grid with no rows lists its headers alone, so where every name is listed once and
        /// ends in _t there are no columns and no rows.
        ///
        /// A listing cut short ends part way through a row, so the columns that row did not reach have one
        /// cell fewer than the others; they are columns all the same, and that row is left out.
        /// </remarks>
        public static void Describe(IReadOnlyList<string> cellNames, out List<string> columns, out int rowCount, bool listingCutShort = false)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);
            List<string> order = [];
            foreach (string name in cellNames)
            {
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                if (counts.TryGetValue(name, out int count))
                {
                    counts[name] = count + 1;
                }
                else
                {
                    counts.Add(name, 1);
                    order.Add(name);
                }
            }

            rowCount = 0;
            foreach (int count in counts.Values)
            {
                if (count > rowCount)
                {
                    rowCount = count;
                }
            }

            columns = [];
            if (rowCount == 1 && order.TrueForAll(name => name.EndsWith(HeaderSuffix, StringComparison.Ordinal)))
            {
                rowCount = 0;
                return;
            }

            int fewest = listingCutShort && rowCount > 2 ? rowCount - 1 : rowCount;
            foreach (string name in order)
            {
                if (counts[name] >= fewest && !(rowCount == 1 && IsHeaderOfListedName(name, counts)))
                {
                    columns.Add(name);
                }
            }

            rowCount = fewest;
        }

        private static bool IsHeaderOfListedName(string name, Dictionary<string, int> counts)
        {
            return name.Length > HeaderSuffix.Length
                && name.EndsWith(HeaderSuffix, StringComparison.Ordinal)
                && counts.ContainsKey(name.Substring(0, name.Length - HeaderSuffix.Length));
        }

        /// <summary>
        /// The first row whose cell satisfies the comparison, or -1.
        /// </summary>
        /// <remarks>
        /// A DataWindow pads what a cell shows out to the column width, so both sides are compared
        /// without the padding.
        /// </remarks>
        public static int FindRow(IReadOnlyList<string> values, ActTableElement.eRunColOperator comparison, string expected)
        {
            string wanted = (expected ?? string.Empty).Trim();
            for (int row = 0; row < values.Count; row++)
            {
                string value = (values[row] ?? string.Empty).Trim();
                bool matches;
                switch (comparison)
                {
                    case ActTableElement.eRunColOperator.Equals:
                        matches = string.Equals(value, wanted, StringComparison.Ordinal);
                        break;
                    case ActTableElement.eRunColOperator.NotEquals:
                        matches = !string.Equals(value, wanted, StringComparison.Ordinal);
                        break;
                    case ActTableElement.eRunColOperator.Contains:
                        matches = value.Contains(wanted, StringComparison.Ordinal);
                        break;
                    case ActTableElement.eRunColOperator.NotContains:
                        matches = !value.Contains(wanted, StringComparison.Ordinal);
                        break;
                    case ActTableElement.eRunColOperator.StartsWith:
                        matches = value.StartsWith(wanted, StringComparison.Ordinal);
                        break;
                    case ActTableElement.eRunColOperator.NotStartsWith:
                        matches = !value.StartsWith(wanted, StringComparison.Ordinal);
                        break;
                    case ActTableElement.eRunColOperator.EndsWith:
                        matches = value.EndsWith(wanted, StringComparison.Ordinal);
                        break;
                    case ActTableElement.eRunColOperator.NotEndsWith:
                        matches = !value.EndsWith(wanted, StringComparison.Ordinal);
                        break;
                    default:
                        throw new NotImplementedException("Selected operator " + comparison + " not implemented.");
                }

                if (matches)
                {
                    return row;
                }
            }

            return -1;
        }

        /// <summary>
        /// The area of the DataWindow that holds its cells, in screen coordinates. The scroll bars lie
        /// outside it.
        /// </summary>
        public static bool TryGetClientArea(IntPtr dataWindow, out Rectangle area)
        {
            area = Rectangle.Empty;
            POINT origin = new POINT();
            if (dataWindow == IntPtr.Zero || !GetClientRect(dataWindow, out RECT client) || !ClientToScreen(dataWindow, ref origin))
            {
                return false;
            }

            area = new Rectangle(origin.x, origin.y, client.right - client.left, client.bottom - client.top);
            return area.Width > 0 && area.Height > 0;
        }

        /// <summary>
        /// Whether the DataWindow is shown, which it is not on a tab page that is not the selected one, or
        /// in a window that is hidden. Nothing of a DataWindow that is not shown is drawn, so it is left
        /// alone rather than asked about cells nobody can see.
        /// </summary>
        public static bool IsShown(IntPtr dataWindow)
        {
            return dataWindow != IntPtr.Zero && IsWindowVisible(dataWindow);
        }

        /// <summary>
        /// Has the calling thread measure the screen in the pixels of the DataWindow's application until the
        /// result is disposed, which are the pixels the DataWindow says where its cells are in.
        /// </summary>
        /// <remarks>
        /// A PowerBuilder application does not follow the scaling of the display, so on a display scaled past
        /// 100% it places its cells in pixels of its own, while Ginger measures windows, moves the mouse and
        /// resolves the points of window messages in the pixels of the display. At 100% the two are the same.
        /// Windows before Windows 10 1607 measures every thread as its process does, and is left measuring so.
        /// </remarks>
        public static IDisposable InCoordinatesOf(IntPtr dataWindow)
        {
            try
            {
                IntPtr context = dataWindow == IntPtr.Zero ? IntPtr.Zero : GetWindowDpiAwarenessContext(dataWindow);
                return new ThreadCoordinates(context == IntPtr.Zero ? IntPtr.Zero : SetThreadDpiAwarenessContext(context));
            }
            catch (EntryPointNotFoundException)
            {
                return new ThreadCoordinates(IntPtr.Zero);
            }
        }

        private sealed class ThreadCoordinates : IDisposable
        {
            private IntPtr mPrevious;

            internal ThreadCoordinates(IntPtr previous)
            {
                mPrevious = previous;
            }

            public void Dispose()
            {
                if (mPrevious != IntPtr.Zero)
                {
                    SetThreadDpiAwarenessContext(mPrevious);
                    mPrevious = IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// Brings a cell to where the DataWindow shows it, scrolling the DataWindow when the cell is not
        /// drawn there, and says where the cell is drawn then. False, with the reason in
        /// <paramref name="details"/>, when the DataWindow is not shown or does not scroll to the cell.
        /// </summary>
        /// <remarks>
        /// A DataWindow lists the rows it has scrolled out of view as well, and PowerBuilder 12.5 does not
        /// flag them as off screen, so where a cell is drawn is the only thing that says whether a click or
        /// a keystroke aimed at it would reach it. Without this a click aimed at a row below the last one
        /// shown would land on the scroll bar, or on whatever lies beyond the DataWindow.
        ///
        /// The DataWindow is scrolled with the messages its scroll bars send, which need no mouse, no focus
        /// and no foreground window, and which it answers at once. It says where it has put the cell as it
        /// scrolls, so the messages are sent until the cell is where it shows and no further, and a
        /// DataWindow that does not move for them is reported rather than waited for.
        ///
        /// PowerBuilder lists the row below the last one it shows as lying inside the client area, at its
        /// foot, which its footer may cover or which it leaves empty, and a click there selects nothing. A
        /// row of a list is therefore in view only with the height of a row free below it. A DataWindow that
        /// scrolls no further down may show its last row only in part, above its footer, and there the part
        /// of the cell above the foot is what is aimed at.
        /// </remarks>
        public bool TryBringIntoView(IntPtr dataWindow, DataWindowCell cell, out Rectangle bounds, out string details)
        {
            details = null;
            if (!cell.TryGetBounds(out bounds))
            {
                details = "The grid does not say where the cell is.";
                return false;
            }

            if (!IsShown(dataWindow) || !TryGetClientArea(dataWindow, out Rectangle client))
            {
                details = "The grid is not shown.";
                return false;
            }

            Rectangle view = ViewOf(cell, bounds, client);
            int room = RoomBelow(cell, bounds, view);
            if (IsInside(view, bounds) && ExcessOf(bounds, view, true, room) == 0)
            {
                return true;
            }

            Stopwatch clock = Stopwatch.StartNew();
            int sent = 0;
            bool inView = ScrollAxis(dataWindow, cell, view, true, room, clock, ref sent, out bounds, out details)
                && ScrollAxis(dataWindow, cell, view, false, 0, clock, ref sent, out bounds, out details);
            if (inView && ExcessOf(bounds, view, true, room) != 0 && bounds.Top < view.Bottom - room)
            {
                bounds = Rectangle.FromLTRB(bounds.Left, bounds.Top, bounds.Right, view.Bottom - room);
            }

            Reporter.ToLog(eLogLevel.INFO, inView
                ? "Scrolled the DataWindow with " + sent + " scroll messages in " + clock.ElapsedMilliseconds + " ms, so that the cell " + cell.Name + " is drawn in view"
                : "The DataWindow did not bring the cell " + cell.Name + " into view (" + sent + " scroll messages sent in " + clock.ElapsedMilliseconds + " ms): " + details);
            return inView;
        }

        /// <summary>
        /// The part of the client area a cell shows in. A column shows below the header, which does not scroll
        /// up and down, so the row just above the first one in view lies inside the client area and is hidden
        /// by the header: a click there would reach the header, and sort the grid. Any other cell shows
        /// anywhere in the client area.
        /// </summary>
        /// <remarks>
        /// The header is made of the cells that are not columns and lie above the cell, along the top of the
        /// client area. A cell beside the one asked about, such as the label of a field in a form, is not part
        /// of it, and neither is anything that would take up more than half of the client area.
        /// </remarks>
        private Rectangle ViewOf(DataWindowCell cell, Rectangle bounds, Rectangle client)
        {
            int top = client.Top;
            if (mColumnNames.Contains(cell.Name))
            {
                int centre = bounds.X + (bounds.Width / 2);
                int inspected = 0;
                foreach (KeyValuePair<string, List<DataWindowCell>> named in mByName)
                {
                    if (mColumnNames.Contains(named.Key))
                    {
                        continue;
                    }

                    foreach (DataWindowCell other in named.Value)
                    {
                        if (++inspected > MaxHeaderCells)
                        {
                            return Rectangle.FromLTRB(client.Left, top, client.Right, client.Bottom);
                        }

                        if (other.TryGetBounds(out Rectangle header)
                            && header.Left <= centre && centre < header.Right
                            && Math.Abs(header.Top - client.Top) <= HeaderSlack
                            && header.Bottom <= client.Top + (client.Height / 2))
                        {
                            top = Math.Max(top, header.Bottom);
                        }
                    }
                }
            }

            return Rectangle.FromLTRB(client.Left, top, client.Right, client.Bottom);
        }

        /// <summary>
        /// The room a cell on a row of a list needs below it in the view to be drawn: the height of a row, from
        /// the top of the cell to the top of the one on the next row, or on the row before for the last row.
        /// 0 for any other cell, and where the view is too short to keep that room below a row.
        /// </summary>
        private int RoomBelow(DataWindowCell cell, Rectangle bounds, Rectangle view)
        {
            if (!mColumnNames.Contains(cell.Name) || !mByName.TryGetValue(cell.Name, out List<DataWindowCell> column) || column.Count < 2)
            {
                return 0;
            }

            int row = column.IndexOf(cell);
            DataWindowCell neighbour = row < 0 ? null : column[row + 1 < column.Count ? row + 1 : row - 1];
            if (neighbour == null || !neighbour.TryGetBounds(out Rectangle next))
            {
                return 0;
            }

            int pitch = Math.Abs(next.Y - bounds.Y);
            return pitch > 0 && view.Height - pitch > bounds.Height ? pitch : 0;
        }

        /// <summary>
        /// Scrolls along one axis until the centre of the cell lies inside the view on that axis, with
        /// <paramref name="room"/> to spare below it.
        /// </summary>
        /// <remarks>
        /// How far one message moves the cell is not known, so one is sent first and the cell is asked where it
        /// is, and the rest of what the distance takes is sent as one batch. A page is used while the cell lies
        /// further off than the view is long, so a page never carries it past the view, and a line after that.
        /// A kind of message the DataWindow does not move for is not sent again, and the next is tried: a page,
        /// then a line, then, up and down, the wheel of a mouse.
        /// </remarks>
        private static bool ScrollAxis(IntPtr dataWindow, DataWindowCell cell, Rectangle view, bool vertical, int room, Stopwatch clock, ref int sent, out Rectangle bounds, out string details)
        {
            int[] stepOfKind = new int[ScrollKinds];
            bool[] refused = new bool[ScrollKinds];
            bool moved = false;
            bool forward = false;
            bounds = Rectangle.Empty;
            for (int pass = 0; pass < MaxScrollPasses && clock.Elapsed < ScrollBudget; pass++)
            {
                if (!cell.TryGetBounds(out bounds))
                {
                    details = "The grid no longer says where the cell is.";
                    return false;
                }

                int excess = ExcessOf(bounds, view, vertical, room);
                if (excess == 0)
                {
                    details = null;
                    return true;
                }

                forward = excess > 0;
                int distance = Math.Abs(excess);
                int viewSize = vertical ? view.Height : view.Width;
                int kind;
                if (distance > viewSize && !refused[KindPage])
                {
                    kind = KindPage;
                    distance -= viewSize;
                }
                else if (!refused[KindLine])
                {
                    kind = KindLine;
                }
                else if (vertical && !refused[KindWheel] && ExcessOf(bounds, view, vertical) != 0)
                {
                    // Only for a cell out of view, as a window that does not take the wheel hands it on to the window it lies in.
                    kind = KindWheel;
                }
                else
                {
                    break;
                }

                int count = stepOfKind[kind] == 0 ? 1 : Math.Min(MaxScrollBatch, (distance + stepOfKind[kind] - 1) / stepOfKind[kind]);
                int from = vertical ? bounds.Y : bounds.X;
                int sentNow = 0;
                while (sentNow < count && (sentNow == 0 || clock.Elapsed < ScrollBudget))
                {
                    if (!SendScroll(dataWindow, vertical, forward, kind, view))
                    {
                        details = "The grid did not take a scroll message.";
                        return false;
                    }

                    sentNow++;
                    sent++;
                }

                // The DataWindow answers a scroll message once it has scrolled, so the wait is only for one that
                // does not move it at all, and it is as long as it takes a slow one to catch up.
                int gained = 0;
                for (int wait = 0; wait <= SettleChecks && gained <= 0; wait++)
                {
                    if (wait > 0)
                    {
                        System.Threading.Thread.Sleep(SettleStepMs);
                    }

                    if (!cell.TryGetBounds(out bounds))
                    {
                        details = "The grid no longer says where the cell is.";
                        return false;
                    }

                    int now = vertical ? bounds.Y : bounds.X;
                    gained = forward ? from - now : now - from;
                }

                if (gained > 0)
                {
                    stepOfKind[kind] = Math.Max(1, gained / sentNow);
                    moved = true;
                }
                else
                {
                    refused[kind] = true;
                    Reporter.ToLog(eLogLevel.DEBUG, "The DataWindow did not move " + DirectionOf(vertical, forward) + " for " + sentNow + " scroll messages of a "
                        + (kind == KindPage ? "page" : kind == KindLine ? "line" : "wheel notch"));
                }
            }

            // A DataWindow that scrolls no further down shows the cell where it is, if only in part above its footer.
            if (room > 0 && ExcessOf(bounds, view, vertical) == 0)
            {
                details = null;
                return true;
            }

            details = "The grid did not scroll " + DirectionOf(vertical, forward) + (moved ? " far enough" : string.Empty)
                + " when it was sent scroll messages (" + sent + " sent), so the cell stays out of view.";
            return false;
        }

        private static string DirectionOf(bool vertical, bool forward)
        {
            return vertical ? (forward ? "down" : "up") : (forward ? "right" : "left");
        }

        /// <summary>
        /// How far the centre of a cell lies from the view: past the far edge as a positive distance, before
        /// the near edge as a negative one, and 0 where it lies inside. Up and down, the far edge is
        /// <paramref name="room"/> above the bottom of the view.
        /// </summary>
        private static int ExcessOf(Rectangle cell, Rectangle view, bool vertical, int room = 0)
        {
            int centre = vertical ? cell.Y + (cell.Height / 2) : cell.X + (cell.Width / 2);
            int near = vertical ? view.Top : view.Left;
            int far = (vertical ? view.Bottom - room : view.Right) - 1;
            return centre < near ? centre - near : (centre > far ? centre - far : 0);
        }

        /// <summary>
        /// Sends the DataWindow one scroll message of a kind. False when it does not take it.
        /// </summary>
        private static bool SendScroll(IntPtr dataWindow, bool vertical, bool forward, int kind, Rectangle view)
        {
            uint message;
            IntPtr wParam;
            IntPtr lParam = IntPtr.Zero;
            if (kind == KindWheel)
            {
                // The wheel is turned toward the user to scroll up, at a point of the DataWindow on the screen.
                message = WmMouseWheel;
                wParam = Pack(0, forward ? -WheelDelta : WheelDelta);
                lParam = Pack(view.Left + (view.Width / 2), view.Top + (view.Height / 2));
            }
            else
            {
                message = vertical ? WmVScroll : WmHScroll;
                int code = kind == KindPage ? (forward ? SbPageDown : SbPageUp) : (forward ? SbLineDown : SbLineUp);
                wParam = new IntPtr(code);
            }

            return SendMessageTimeout(dataWindow, message, wParam, lParam, SmtoAbortIfHung, ScrollMessageTimeoutMs, out _) != IntPtr.Zero;
        }

        /// <summary>Two numbers in the halves of one, as the parameters of a message carry them.</summary>
        private static IntPtr Pack(int low, int high)
        {
            return unchecked((nint)(((uint)(high & 0xFFFF) << 16) | (uint)(low & 0xFFFF)));
        }

        /// <summary>
        /// A point in a cell as the offset from the top left of the DataWindow, which is what the
        /// coordinate clicks take.
        /// </summary>
        /// <param name="inCell">
        /// The point as x,y from the top left of the cell, or nothing for the centre of the cell.
        /// </param>
        public static string OffsetInGrid(Rectangle gridBounds, Rectangle cellBounds, string inCell)
        {
            if (!TryParseOffset(inCell, out int x, out int y))
            {
                x = cellBounds.Width / 2;
                y = cellBounds.Height / 2;
            }

            return (cellBounds.X - gridBounds.X + x) + "," + (cellBounds.Y - gridBounds.Y + y);
        }

        /// <summary>Reads a point written as x,y.</summary>
        public static bool TryParseOffset(string offset, out int x, out int y)
        {
            x = 0;
            y = 0;
            string[] parts = (offset ?? string.Empty).Split(',');
            return parts.Length == 2 && int.TryParse(parts[0].Trim(), out x) && int.TryParse(parts[1].Trim(), out y);
        }

        /// <summary>Whether the centre of an element lies inside the area.</summary>
        public static bool IsInside(Rectangle area, Rectangle element)
        {
            return element.Width > 0
                && element.Height > 0
                && area.Contains(element.X + (element.Width / 2), element.Y + (element.Height / 2));
        }

        /// <summary>
        /// Whether the DataWindow lists cells that have a name, asked for as lightly as it can be: the
        /// first few children are looked at, where reading the cells goes through every one of them.
        /// </summary>
        public static bool ListsNamedCells(IntPtr dataWindow)
        {
            try
            {
                if (dataWindow == IntPtr.Zero || !DataWindowMsaaLayer.TryGetAccessible(dataWindow, out IAccessible root))
                {
                    return false;
                }

                int listed = root.accChildCount;
                if (listed <= 0)
                {
                    return false;
                }

                object[] children = AskForChildren(root, Math.Min(listed, SampledCells), out int obtained);
                for (int index = 0; index < obtained; index++)
                {
                    if (CellOf(root, children[index]) != null)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Asking a DataWindow whether it lists cells failed", ex);
            }

            return false;
        }

        /// <summary>
        /// Reads the cells of a DataWindow. False when it lists none, with the reason in
        /// <paramref name="details"/>.
        /// </summary>
        public static bool TryRead(IntPtr dataWindow, out DataWindowGrid grid, out string details)
        {
            grid = null;
            if (dataWindow == IntPtr.Zero || !DataWindowMsaaLayer.TryGetAccessible(dataWindow, out IAccessible root))
            {
                details = "the control did not hand out an accessible object";
                return false;
            }

            return TryRead(root, out grid, out details);
        }

        /// <summary>
        /// Reads the cells from the DataWindow's own listing of its children.
        /// </summary>
        public static bool TryRead(IAccessible root, out DataWindowGrid grid, out string details)
        {
            grid = null;
            int listed = 0;
            try
            {
                List<DataWindowCell> cells = ReadChildren(root, out listed);
                details = "the control lists " + listed + " children, " + cells.Count + " of them named";
                if (cells.Count == 0)
                {
                    return false;
                }

                bool cutShort = listed > MaxCells;
                grid = new DataWindowGrid(cells, cutShort);
                if (cutShort)
                {
                    Reporter.ToLog(eLogLevel.WARN, "The DataWindow lists " + listed + " children, more than the " + MaxCells + " that are read, so only its first "
                        + grid.RowCount + " rows can be worked");
                }

                return true;
            }
            catch (Exception ex)
            {
                details = "reading the cells failed (the control lists " + listed + " children): " + ex.Message;
                Reporter.ToLog(eLogLevel.DEBUG, "Reading the cells of a DataWindow through MSAA failed", ex);
                return false;
            }
        }

        private static List<DataWindowCell> ReadChildren(IAccessible root, out int listed)
        {
            List<DataWindowCell> cells = [];
            listed = root.accChildCount;
            if (listed <= 0)
            {
                return cells;
            }

            object[] children = AskForChildren(root, Math.Min(listed, MaxCells), out int obtained);

            Stopwatch budget = Stopwatch.StartNew();
            for (int index = 0; index < obtained; index++)
            {
                EnsureWithinBudget(budget);
                DataWindowCell cell = CellOf(root, children[index]);
                if (cell != null)
                {
                    cells.Add(cell);
                }
            }

            return cells;
        }

        /// <summary>The first children the DataWindow lists, and how many of them it handed out.</summary>
        private static object[] AskForChildren(IAccessible root, int count, out int obtained)
        {
            object[] children = new object[count];
            obtained = 0;
            try
            {
                if (AccessibleChildren(root, 0, count, children, out obtained) < 0)
                {
                    obtained = 0;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "AccessibleChildren failed on a DataWindow", ex);
                obtained = 0;
            }

            return children;
        }

        /// <summary>The cell a listed child stands for, which is an accessible object or a child id, or null.</summary>
        private static DataWindowCell CellOf(IAccessible root, object child)
        {
            return child switch
            {
                IAccessible nested => DataWindowCell.TryCreate(nested, ChildIdSelf),
                int childId when childId != ChildIdSelf => DataWindowCell.TryCreate(root, childId),
                _ => null
            };
        }

        private static void EnsureWithinBudget(Stopwatch budget)
        {
            if (budget.Elapsed > ReadBudget)
            {
                throw new TimeoutException("the control took more than " + (int)ReadBudget.TotalSeconds + " seconds to answer");
            }
        }
    }
}
