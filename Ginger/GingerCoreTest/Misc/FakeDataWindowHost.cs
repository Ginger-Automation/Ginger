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

extern alias UIAComWrapperNetstandard;
using Accessibility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;

namespace GingerCoreTest.Misc
{
    /// <summary>
    /// One cell of a stand-in DataWindow, with the state the accessible object reports for it.
    /// </summary>
    internal sealed class FakeCell
    {
        internal const int RoleStaticText = 0x29;
        internal const int RoleText = 0x2A;
        internal const int RoleCheckButton = 0x2C;
        internal const int StateChecked = 0x10;
        internal const int StateInvisible = 0x8000;

        private readonly object mGate = new object();
        private string mValue;
        private int mState;

        /// <param name="relativeBounds">Where the cell is drawn, from the top left of the DataWindow's client area, before any scrolling.</param>
        internal FakeCell(string name, string value, Rectangle relativeBounds, int role = RoleText, int state = 0)
        {
            Name = name;
            mValue = value;
            RelativeBounds = relativeBounds;
            Role = role;
            mState = state;
        }

        internal string Name { get; }

        internal int Role { get; }

        internal Rectangle RelativeBounds { get; }

        /// <summary>
        /// True for a cell of the header, which stays where it is when the DataWindow scrolls up and down and
        /// moves with the rows when it scrolls sideways, and is drawn over the rows that scroll under it.
        /// </summary>
        internal bool Pinned { get; set; }

        /// <summary>Answers a write the way a PowerBuilder cell does: not at all.</summary>
        internal bool RefusesWrites { get; set; }

        internal bool IsCheckBox => Role == RoleCheckButton;

        internal string Value
        {
            get { lock (mGate) { return mValue; } }
        }

        internal int State
        {
            get { lock (mGate) { return mState; } }
        }

        internal bool IsChecked => (State & StateChecked) != 0;

        internal string DefaultAction => IsCheckBox ? (IsChecked ? "UnCheck" : "Check") : null;

        internal void Write(string value)
        {
            if (RefusesWrites)
            {
                throw new COMException("Member not found.", unchecked((int)0x80020003));
            }

            lock (mGate)
            {
                mValue = value;
            }
        }

        internal void DoDefaultAction()
        {
            if (!IsCheckBox)
            {
                throw new COMException("Member not found.", unchecked((int)0x80020003));
            }

            lock (mGate)
            {
                mState ^= StateChecked;
            }
        }
    }

    /// <summary>The requests a stand-in DataWindow scrolls for.</summary>
    [Flags]
    internal enum ScrollSupport
    {
        None = 0,

        /// <summary>The messages the scroll bars of a window send, which is how a DataWindow is scrolled.</summary>
        ScrollBar = 1,

        /// <summary>The message the wheel of a mouse sends.</summary>
        Wheel = 2,

        Both = ScrollBar | Wheel
    }

    /// <summary>
    /// What a stand-in DataWindow draws, and how its accessible object lists it.
    /// </summary>
    internal sealed class FakeDataWindowModel
    {
        private const int SbLineUp = 0;
        private const int SbLineDown = 1;
        private const int SbPageUp = 2;
        private const int SbPageDown = 3;
        private const int WheelDelta = 120;

        internal readonly List<FakeCell> Cells = [];

        private int mScrollX;
        private int mScrollY;
        private int mScrollMessages;

        /// <summary>
        /// The requests the DataWindow scrolls for, none unless a test asks for it: the stand-ins of the
        /// first tests never scrolled, and a cell they list outside the client area stays out of it.
        /// </summary>
        internal ScrollSupport Scrolling = ScrollSupport.None;

        /// <summary>How far one line scrolls up and down, and sideways, in pixels.</summary>
        internal int LineHeight = 22;

        internal int LineWidth = 40;

        /// <summary>How many lines one notch of the wheel scrolls.</summary>
        internal int LinesPerNotch = 3;

        /// <summary>True for a DataWindow that takes the messages for a line but not the ones for a page.</summary>
        internal bool IgnoresPageMessages;

        /// <summary>The furthest down, in pixels, the DataWindow scrolls, where that is less than its cells need.</summary>
        internal int ScrollLimitY = int.MaxValue;

        /// <summary>How far the rows have been scrolled up, and the cells sideways, in pixels.</summary>
        internal int ScrollY
        {
            get => Volatile.Read(ref mScrollY);
            set => Volatile.Write(ref mScrollY, value);
        }

        internal int ScrollX
        {
            get => Volatile.Read(ref mScrollX);
            set => Volatile.Write(ref mScrollX, value);
        }

        /// <summary>How many requests to scroll the DataWindow has received, whether or not it scrolled for them.</summary>
        internal int ScrollMessages => Volatile.Read(ref mScrollMessages);

        /// <summary>Where the client area starts on the screen. Set by the window that hosts the model.</summary>
        internal Point Origin = new Point(100, 100);

        internal Size ClientSize = new Size(420, 240);

        /// <summary>
        /// True when each cell is an accessible object of its own, false when the DataWindow answers for
        /// them by child id, which are the two shapes a control may take.
        /// </summary>
        internal bool NestedChildren = true;

        /// <summary>True for a DataWindow that fails when it is asked how many children it has.</summary>
        internal bool FailsToListChildren;

        /// <summary>
        /// True for a DataWindow that flags every one of its cells as invisible whether or not it is drawn,
        /// which is what PowerBuilder 12.5 does and what makes UI Automation list none of them. The flag has
        /// no effect on what the DataWindow answers when it is asked what lies at a point, which is how
        /// PowerBuilder 12.5 answers too.
        /// </summary>
        internal bool FlagsEveryCellInvisible;

        internal int ChildCountReads;

        /// <summary>How many times a cell, or the DataWindow, was asked for its name.</summary>
        internal int NameReads;

        internal Rectangle Area => new Rectangle(Origin, ClientSize);

        internal IAccessible Root => new FakeAccessible(this, null);

        /// <summary>
        /// Where a cell is drawn on the screen now, which is where it is whether or not that lies inside the
        /// client area: a row that has been scrolled out of view is listed with the place it would be drawn.
        /// </summary>
        internal Rectangle ScreenBoundsOf(FakeCell cell)
        {
            return new Rectangle(
                cell.RelativeBounds.X + Origin.X - ScrollX,
                cell.RelativeBounds.Y + Origin.Y - (cell.Pinned ? 0 : ScrollY),
                cell.RelativeBounds.Width,
                cell.RelativeBounds.Height);
        }

        /// <summary>Takes a message a scroll bar sends, with the code that says how far.</summary>
        internal void TakeScrollBarMessage(bool vertical, int code)
        {
            Interlocked.Increment(ref mScrollMessages);
            bool isPage = code == SbPageUp || code == SbPageDown;
            if ((Scrolling & ScrollSupport.ScrollBar) == 0 || (isPage && IgnoresPageMessages))
            {
                return;
            }

            int line = vertical ? LineHeight : LineWidth;
            int page = vertical
                ? Math.Max(line, (((ClientSize.Height - PinnedBottom()) / line) - 1) * line)
                : Math.Max(line, ClientSize.Width - line);
            int distance = code switch
            {
                SbLineUp => -line,
                SbLineDown => line,
                SbPageUp => -page,
                SbPageDown => page,
                _ => 0
            };

            Scroll(vertical, distance);
        }

        /// <summary>Takes a turn of the wheel, which is toward the user, as a positive delta, to scroll up.</summary>
        internal void TakeWheelMessage(int delta)
        {
            Interlocked.Increment(ref mScrollMessages);
            if ((Scrolling & ScrollSupport.Wheel) != 0)
            {
                Scroll(true, -(delta / WheelDelta) * LinesPerNotch * LineHeight);
            }
        }

        private void Scroll(bool vertical, int distance)
        {
            if (vertical)
            {
                ScrollY = Math.Max(0, Math.Min(Math.Min(MaxScroll(true), ScrollLimitY), ScrollY + distance));
            }
            else
            {
                ScrollX = Math.Max(0, Math.Min(MaxScroll(false), ScrollX + distance));
            }
        }

        /// <summary>How far the cells can be scrolled before the last of them is drawn at the edge of the client area.</summary>
        private int MaxScroll(bool vertical)
        {
            int extent = 0;
            foreach (FakeCell cell in Cells)
            {
                if (!(vertical && cell.Pinned))
                {
                    extent = Math.Max(extent, vertical ? cell.RelativeBounds.Bottom : cell.RelativeBounds.Right);
                }
            }

            return Math.Max(0, extent - (vertical ? ClientSize.Height : ClientSize.Width));
        }

        private int PinnedBottom()
        {
            int bottom = 0;
            foreach (FakeCell cell in Cells)
            {
                if (cell.Pinned)
                {
                    bottom = Math.Max(bottom, cell.RelativeBounds.Bottom);
                }
            }

            return bottom;
        }

        internal FakeCell Add(string name, string value, int x, int y, int width, int role = FakeCell.RoleText, int state = 0, int height = 20)
        {
            FakeCell cell = new FakeCell(name, value, new Rectangle(x, y, width, height), role, state);
            Cells.Add(cell);
            return cell;
        }

        /// <summary>
        /// The cells of a three row list as PowerBuilder names them: a header and three columns, one of
        /// them a check box, with a part that has no name among them.
        /// </summary>
        internal static FakeDataWindowModel ItemList()
        {
            FakeDataWindowModel model = new FakeDataWindowModel();
            model.Add("item_code_t", "Item Code", 10, 6, 100, FakeCell.RoleStaticText).Pinned = true;
            model.Add("item_name_t", "Item Name", 120, 6, 160, FakeCell.RoleStaticText).Pinned = true;
            model.Add("active_t", "Active", 290, 6, 60, FakeCell.RoleStaticText).Pinned = true;

            string[] codes = ["101", "102", "103"];
            string[] names = ["Alpha", "Beta", "Gamma"];
            for (int row = 0; row < codes.Length; row++)
            {
                int y = 30 + (row * 22);
                model.Add("item_code", codes[row], 10, y, 100);
                model.Add("item_name", names[row] + "      ", 120, y, 160);
                model.Add("active", "N", 290, y, 60, FakeCell.RoleCheckButton, row == 1 ? FakeCell.StateChecked : 0);
            }

            model.Add(string.Empty, "padding", 10, 180, 100);
            return model;
        }

        /// <summary>
        /// A list as PowerBuilder 12.5 lists a DataWindow: four headers and a row of five columns for each
        /// of 30 rows, every cell flagged invisible, in a client area that shows the first six rows. The
        /// fifth column has no header, as a computed column may have none.
        /// </summary>
        internal static FakeDataWindowModel FlaggedList(int rows = 30)
        {
            FakeDataWindowModel model = new FakeDataWindowModel { FlagsEveryCellInvisible = true, ClientSize = new Size(413, 160) };
            model.Add("item_code_t", "Item Code", 2, 2, 70, FakeCell.RoleStaticText).Pinned = true;
            model.Add("item_text_t", "Description", 74, 2, 150, FakeCell.RoleStaticText).Pinned = true;
            model.Add("item_day_t", "Day", 226, 2, 90, FakeCell.RoleStaticText).Pinned = true;
            model.Add("item_qty_t", "Quantity", 318, 2, 60, FakeCell.RoleStaticText).Pinned = true;

            for (int row = 0; row < rows; row++)
            {
                int y = 24 + (row * 22);
                model.Add("item_code", (101 + row).ToString(), 2, y, 70);
                model.Add("item_text", "Item " + (101 + row) + "      ", 74, y, 150);
                model.Add("item_day", ((row % 28) + 1).ToString(), 226, y, 90);
                model.Add("item_qty", (100 * (row + 1)).ToString(), 318, y, 60);
                model.Add("item_calc", "C" + ((row % 28) + 1), 380, y, 31);
            }

            return model;
        }

        /// <summary>
        /// A list too wide for its client area: a header and rows of five columns, the last two of which lie
        /// beyond the right edge until the DataWindow is scrolled sideways.
        /// </summary>
        internal static FakeDataWindowModel WideList(int rows = 3)
        {
            FakeDataWindowModel model = new FakeDataWindowModel { ClientSize = new Size(300, 120) };
            for (int column = 0; column < WideColumns.Length; column++)
            {
                model.Add(WideColumns[column] + "_t", "Caption " + column, column * 130, 2, 120, FakeCell.RoleStaticText).Pinned = true;
            }

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < WideColumns.Length; column++)
                {
                    model.Add(WideColumns[column], "r" + row + "c" + column, column * 130, 24 + (row * 22), 120);
                }
            }

            return model;
        }

        /// <summary>The columns of the model <see cref="WideList"/> builds, in the order they are listed.</summary>
        internal static readonly string[] WideColumns = ["col_a", "col_b", "col_c", "col_d", "col_e"];

        /// <summary>The columns of the model <see cref="FlaggedList"/> builds, in the order they are listed.</summary>
        internal static readonly string[] FlaggedColumns = ["item_code", "item_text", "item_day", "item_qty", "item_calc"];

        /// <summary>The cell of a column on a row of the model <see cref="FlaggedList"/> builds.</summary>
        internal FakeCell FlaggedCell(string column, int row)
        {
            const int headers = 4;
            return Cells[headers + (row * FlaggedColumns.Length) + Array.IndexOf(FlaggedColumns, column)];
        }
    }

    /// <summary>
    /// The accessible object of a stand-in DataWindow: the DataWindow itself, or one of its cells.
    /// </summary>
    [ComVisible(true)]
    internal sealed class FakeAccessible : IAccessible
    {
        private const int ChildIdSelf = 0;
        private const int RoleClient = 0x0A;

        private readonly FakeDataWindowModel mModel;
        private readonly FakeCell mCell;

        internal FakeAccessible(FakeDataWindowModel model, FakeCell cell)
        {
            mModel = model;
            mCell = cell;
        }

        public object accParent => null;

        public int accChildCount
        {
            get
            {
                if (mCell != null)
                {
                    return 0;
                }

                Interlocked.Increment(ref mModel.ChildCountReads);
                if (mModel.FailsToListChildren)
                {
                    throw new COMException("The control is not answering.", unchecked((int)0x80004005));
                }

                return mModel.Cells.Count;
            }
        }

        public object accFocus => null;

        public object accSelection => null;

        public object get_accChild(object childID)
        {
            if (mCell != null || !mModel.NestedChildren)
            {
                return null;
            }

            FakeCell cell = CellFor(childID);
            return cell == null ? null : new FakeAccessible(mModel, cell);
        }

        public string get_accName(object childID)
        {
            Interlocked.Increment(ref mModel.NameReads);
            FakeCell cell = CellFor(childID);
            return cell == null ? "dw_items" : cell.Name;
        }

        public string get_accValue(object childID) => CellFor(childID)?.Value;

        public string get_accDescription(object childID) => null;

        public object get_accRole(object childID)
        {
            FakeCell cell = CellFor(childID);
            return cell == null ? RoleClient : cell.Role;
        }

        public object get_accState(object childID)
        {
            FakeCell cell = CellFor(childID);
            if (cell == null)
            {
                return 0;
            }

            return mModel.FlagsEveryCellInvisible ? cell.State | FakeCell.StateInvisible : cell.State;
        }

        public string get_accHelp(object childID) => null;

        public int get_accHelpTopic(out string pszHelpFile, object childID)
        {
            pszHelpFile = null;
            return -1;
        }

        public string get_accKeyboardShortcut(object childID) => null;

        public string get_accDefaultAction(object childID) => CellFor(childID)?.DefaultAction;

        public void accSelect(int flagsSelect, object childID)
        {
        }

        public void accLocation(out int pxLeft, out int pyTop, out int pcxWidth, out int pcyHeight, object childID)
        {
            FakeCell cell = CellFor(childID);
            Rectangle bounds = cell == null ? mModel.Area : mModel.ScreenBoundsOf(cell);
            pxLeft = bounds.X;
            pyTop = bounds.Y;
            pcxWidth = bounds.Width;
            pcyHeight = bounds.Height;
        }

        public object accNavigate(int navDir, object childStart) => null;

        public object accHitTest(int xLeft, int yTop)
        {
            if (mCell != null)
            {
                return ChildIdSelf;
            }

            for (int index = 0; index < mModel.Cells.Count; index++)
            {
                FakeCell cell = mModel.Cells[index];
                if (mModel.ScreenBoundsOf(cell).Contains(xLeft, yTop))
                {
                    return mModel.NestedChildren ? new FakeAccessible(mModel, cell) : (object)(index + 1);
                }
            }

            return ChildIdSelf;
        }

        public void accDoDefaultAction(object childID)
        {
            FakeCell cell = CellFor(childID);
            if (cell == null)
            {
                throw new COMException("Member not found.", unchecked((int)0x80020003));
            }

            cell.DoDefaultAction();
        }

        public void set_accName(object childID, string pszName)
        {
        }

        public void set_accValue(object childID, string pszValue)
        {
            FakeCell cell = CellFor(childID);
            if (cell == null)
            {
                throw new COMException("Member not found.", unchecked((int)0x80020003));
            }

            cell.Write(pszValue);
        }

        /// <summary>
        /// The cell a child id names, or null for the object itself. A cell that is an object of its own
        /// answers for itself whatever it is asked.
        /// </summary>
        private FakeCell CellFor(object childId)
        {
            if (mCell != null)
            {
                return mCell;
            }

            int id = Convert.ToInt32(childId);
            if (id == ChildIdSelf)
            {
                return null;
            }

            if (id < 1 || id > mModel.Cells.Count)
            {
                throw new COMException("Invalid argument.", unchecked((int)0x80070057));
            }

            return mModel.Cells[id - 1];
        }
    }

    /// <summary>
    /// A window of a DataWindow class whose accessible object lists the cells of a model, standing in
    /// for a DataWindow that UI Automation lists nothing of. It also counts the clicks it receives, so a
    /// test can see where in it a click landed.
    /// </summary>
    internal sealed class FakeDataWindow : IDisposable
    {
        private const int WsPopup = unchecked((int)0x80000000);
        private const int WsChild = 0x40000000;
        private const int WsVisible = 0x10000000;
        private const int WsVScroll = 0x00200000;
        private const int WsHScroll = 0x00100000;
        private const int SbHorz = 0;
        private const int SbVert = 1;
        private const int SmCxVScroll = 2;
        private const int SmCyHScroll = 3;
        private const int SwHide = 0;
        private const uint QsAllInput = 0x04FF;
        private const int DataWindowControlId = 1000;
        private const int SbsVert = 0x0001;
        private const int SbCtl = 2;
        private const uint SifRange = 0x0001;
        private const uint SifPage = 0x0002;
        private const uint SifPos = 0x0004;
        private const int WsExToolWindow = 0x00000080;
        private const int WsExNoActivate = 0x08000000;
        private const int WmGetObject = 0x003D;
        private const int WmHScroll = 0x0114;
        private const int WmVScroll = 0x0115;
        private const int WmLButtonDown = 0x0201;
        private const int WmLButtonDblClk = 0x0203;
        private const int WmMouseWheel = 0x020A;
        private const int ObjIdClient = -4;

        private static readonly Guid IID_IAccessible = new Guid("618736e0-3c3d-11cf-810c-00aa00389b71");
        private static readonly object sGate = new object();
        private static readonly HashSet<string> sRegistered = [];
        private static WndProc sKeepAlive;

        private readonly ManualResetEventSlim mCreated = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim mRelease = new ManualResetEventSlim(false);
        private readonly Thread mOwner;
        private readonly object mClickGate = new object();
        private readonly string mClassName;
        private readonly bool mScrollBarInClientArea;
        private readonly bool mNonClientScrollBars;
        private readonly bool mInAHostWindow;
        private IntPtr mHostHandle;
        private IntPtr mScrollBarHandle;
        private int mLeftClicks;
        private int mDoubleClicks;
        private Point mLastClick;
        private volatile bool mAnswersAccessibility = true;
        private string mFailure;

        /// <param name="scrollBarInClientArea">
        /// Draws a scroll bar, an ordinary window of the system's scroll bar class, inside the client area,
        /// which is where a grid that does not use the scroll bars of its window would draw one.
        /// </param>
        /// <param name="nonClientScrollBars">
        /// Gives the window the scroll bars every window can have, down its right edge and along its bottom,
        /// outside the client area, which is where a DataWindow has them. The size of the client area is
        /// still the size the model asks for.
        /// </param>
        /// <param name="inAHostWindow">
        /// Makes the DataWindow a child window, with a control id, of a window of its own, which is how a
        /// DataWindow is found in a form.
        /// </param>
        internal FakeDataWindow(FakeDataWindowModel model, string className = "pbdw172", bool scrollBarInClientArea = false, bool nonClientScrollBars = false, bool inAHostWindow = false)
        {
            Model = model;
            mClassName = className;
            mScrollBarInClientArea = scrollBarInClientArea;
            mNonClientScrollBars = nonClientScrollBars;
            mInAHostWindow = inAHostWindow;
            mOwner = new Thread(Own) { IsBackground = true };
            mOwner.SetApartmentState(ApartmentState.STA);
            mOwner.Start();
            mCreated.Wait(10000);

            if (Handle == IntPtr.Zero)
            {
                throw new InvalidOperationException("The stand-in DataWindow was not created: " + mFailure);
            }

            Element = UIAuto.AutomationElement.FromHandle(Handle);
            if (mHostHandle != IntPtr.Zero)
            {
                Host = UIAuto.AutomationElement.FromHandle(mHostHandle);
            }

            if (mScrollBarHandle != IntPtr.Zero)
            {
                ScrollBar = UIAuto.AutomationElement.FromHandle(mScrollBarHandle);
            }
        }

        internal FakeDataWindowModel Model { get; }

        internal IntPtr Handle { get; private set; }

        internal UIAuto.AutomationElement Element { get; }

        /// <summary>The window the DataWindow is a child of, when it was asked to be in one.</summary>
        internal UIAuto.AutomationElement Host { get; }

        /// <summary>Hides the DataWindow, as a tab page that is not the selected one is hidden.</summary>
        internal void Hide()
        {
            ShowWindow(Handle, SwHide);
        }

        /// <summary>The scroll bar drawn inside the client area, when one was asked for.</summary>
        internal UIAuto.AutomationElement ScrollBar { get; }

        /// <summary>False makes the window ignore the request for its accessible object.</summary>
        internal bool AnswersAccessibility
        {
            get => mAnswersAccessibility;
            set => mAnswersAccessibility = value;
        }

        internal int LeftClicks => Volatile.Read(ref mLeftClicks);

        internal int DoubleClicks => Volatile.Read(ref mDoubleClicks);

        /// <summary>Client coordinates of the last press the window received.</summary>
        internal Point LastClick
        {
            get { lock (mClickGate) { return mLastClick; } }
        }

        /// <summary>
        /// Forgets the clicks received so far. The ones still posted to the window when this is called
        /// are counted afterwards, so wait for what was sent before calling it.
        /// </summary>
        internal void ForgetClicks()
        {
            Interlocked.Exchange(ref mLeftClicks, 0);
            Interlocked.Exchange(ref mDoubleClicks, 0);
            lock (mClickGate)
            {
                mLastClick = Point.Empty;
            }
        }

        /// <summary>
        /// Waits for the posted clicks to be taken, which they are on the window's own thread.
        /// </summary>
        internal bool WaitForLeftClicks(int count, int timeoutMs = 5000)
        {
            return WaitUntil(() => LeftClicks >= count, timeoutMs);
        }

        internal bool WaitForDoubleClicks(int count, int timeoutMs = 5000)
        {
            return WaitUntil(() => DoubleClicks >= count, timeoutMs);
        }

        private static bool WaitUntil(Func<bool> condition, int timeoutMs)
        {
            DateTime until = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (!condition())
            {
                if (DateTime.UtcNow > until)
                {
                    return false;
                }

                Thread.Sleep(10);
            }

            return true;
        }

        private void Own()
        {
            IntPtr instance = GetModuleHandleW(null);
            Register(instance);

            // The scroll bars of a window are drawn outside its client area, so the window is made as much
            // larger than the client area as they take.
            int width = Model.ClientSize.Width + (mNonClientScrollBars ? GetSystemMetrics(SmCxVScroll) : 0);
            int height = Model.ClientSize.Height + (mNonClientScrollBars ? GetSystemMetrics(SmCyHScroll) : 0);
            int scrollBars = mNonClientScrollBars ? WsVScroll | WsHScroll : 0;

            IntPtr top;
            IntPtr handle;
            if (mInAHostWindow)
            {
                top = CreateWindowExW(WsExToolWindow | WsExNoActivate, "pbxx172", "host", WsPopup | WsVisible,
                    -32000, -32000, width, height, IntPtr.Zero, IntPtr.Zero, instance, IntPtr.Zero);
                handle = top == IntPtr.Zero
                    ? IntPtr.Zero
                    : CreateWindowExW(0, mClassName, "dw", WsChild | WsVisible | scrollBars, 0, 0, width, height, top, new IntPtr(DataWindowControlId), instance, IntPtr.Zero);
                mHostHandle = top;
            }
            else
            {
                top = CreateWindowExW(WsExToolWindow | WsExNoActivate, mClassName, "dw", WsPopup | WsVisible | scrollBars,
                    -32000, -32000, width, height, IntPtr.Zero, IntPtr.Zero, instance, IntPtr.Zero);
                handle = top;
            }

            if (handle == IntPtr.Zero)
            {
                mFailure = "CreateWindowEx failed with " + Marshal.GetLastWin32Error();
                if (top != IntPtr.Zero)
                {
                    DestroyWindow(top);
                }

                mCreated.Set();
                return;
            }

            if (mNonClientScrollBars)
            {
                // A scroll bar with no range has no thumb, and the thumb is one of the things UI Automation lists.
                SCROLLINFO range = ScrollRange();
                SetScrollInfo(handle, SbVert, ref range, true);
                SetScrollInfo(handle, SbHorz, ref range, true);
            }

            // What the window leaves for the client area is what the DataWindow answers for.
            if (GetClientRect(handle, out RECT client))
            {
                Model.ClientSize = new Size(client.right - client.left, client.bottom - client.top);
            }

            POINT origin = new POINT();
            ClientToScreen(handle, ref origin);
            Model.Origin = new Point(origin.x, origin.y);

            DataWindowWindow window = new DataWindowWindow(this);
            window.AssignHandle(handle);
            Handle = handle;

            if (mScrollBarInClientArea)
            {
                mScrollBarHandle = CreateScrollBar(handle, instance);
            }

            mCreated.Set();

            // Pumped, because the requests for the accessible object and the clicks are messages the
            // window has to take on its own thread. The wait ends as soon as a message arrives, so a
            // caller is not held up for the length of a sleep on every question it asks the window.
            while (!mRelease.IsSet)
            {
                MsgWaitForMultipleObjects(0, IntPtr.Zero, false, 50, QsAllInput);
                Application.DoEvents();
            }

            window.ReleaseHandle();
            DestroyWindow(top);
        }

        public void Dispose()
        {
            mRelease.Set();
            mOwner.Join(5000);
            mCreated.Dispose();
            mRelease.Dispose();
        }

        /// <summary>
        /// A vertical scroll bar down the right edge of the client area, with a range and a page so that it
        /// has a thumb to show.
        /// </summary>
        private IntPtr CreateScrollBar(IntPtr parent, IntPtr instance)
        {
            const int width = 17;
            IntPtr bar = CreateWindowExW(0, "ScrollBar", string.Empty, WsChild | WsVisible | SbsVert,
                Model.ClientSize.Width - width, 0, width, Model.ClientSize.Height, parent, IntPtr.Zero, instance, IntPtr.Zero);
            if (bar == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            SCROLLINFO info = ScrollRange();
            SetScrollInfo(bar, SbCtl, ref info, true);
            return bar;
        }

        private static SCROLLINFO ScrollRange()
        {
            return new SCROLLINFO
            {
                cbSize = (uint)Marshal.SizeOf<SCROLLINFO>(),
                fMask = SifRange | SifPage | SifPos,
                nMin = 0,
                nMax = 100,
                nPage = 20,
                nPos = 0
            };
        }

        private static void Register(IntPtr instance)
        {
            lock (sGate)
            {
                // The class has to be registered once however many windows are made from it.
                sKeepAlive ??= DefWindowProcW;
                foreach (string className in new[] { "pbdw172", "pbxx172" })
                {
                    if (sRegistered.Contains(className))
                    {
                        continue;
                    }

                    WNDCLASS wndClass = new WNDCLASS
                    {
                        lpfnWndProc = Marshal.GetFunctionPointerForDelegate(sKeepAlive),
                        hInstance = instance,
                        lpszClassName = className
                    };

                    if (RegisterClassW(ref wndClass) == 0)
                    {
                        throw new InvalidOperationException("Could not register the window class " + className + ": " + Marshal.GetLastWin32Error());
                    }

                    sRegistered.Add(className);
                }
            }
        }

        private void RecordClick(int message, IntPtr lParam)
        {
            long packed = lParam.ToInt64();
            Point client = new Point(unchecked((short)(packed & 0xFFFF)), unchecked((short)((packed >> 16) & 0xFFFF)));
            if (message == WmLButtonDown)
            {
                lock (mClickGate)
                {
                    mLastClick = client;
                }

                Interlocked.Increment(ref mLeftClicks);
            }
            else
            {
                lock (mClickGate)
                {
                    mLastClick = client;
                }

                Interlocked.Increment(ref mDoubleClicks);
            }
        }

        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private sealed class DataWindowWindow : NativeWindow
        {
            private readonly FakeDataWindow mOwner;

            internal DataWindowWindow(FakeDataWindow owner)
            {
                mOwner = owner;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WmGetObject && mOwner.AnswersAccessibility && unchecked((int)m.LParam.ToInt64()) == ObjIdClient)
                {
                    IAccessible root = new FakeAccessible(mOwner.Model, null);
                    IntPtr unknown = Marshal.GetIUnknownForObject(root);
                    try
                    {
                        Guid iid = IID_IAccessible;
                        m.Result = LresultFromObject(ref iid, m.WParam, unknown);
                    }
                    finally
                    {
                        Marshal.Release(unknown);
                    }

                    return;
                }

                if (m.Msg == WmLButtonDown || m.Msg == WmLButtonDblClk)
                {
                    mOwner.RecordClick(m.Msg, m.LParam);
                }

                // The requests to scroll are taken on the window's own thread, as they are by the real control,
                // and are answered once the cells have moved.
                if (m.Msg == WmVScroll || m.Msg == WmHScroll)
                {
                    mOwner.Model.TakeScrollBarMessage(m.Msg == WmVScroll, (int)(m.WParam.ToInt64() & 0xFFFF));
                    m.Result = IntPtr.Zero;
                    return;
                }

                if (m.Msg == WmMouseWheel)
                {
                    mOwner.Model.TakeWheelMessage((short)((m.WParam.ToInt64() >> 16) & 0xFFFF));
                    m.Result = IntPtr.Zero;
                    return;
                }

                base.WndProc(ref m);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WNDCLASS
        {
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpszClassName;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SCROLLINFO
        {
            public uint cbSize;
            public uint fMask;
            public int nMin;
            public int nMax;
            public uint nPage;
            public int nPos;
            public int nTrackPos;
        }

        [DllImport("user32.dll")]
        private static extern int SetScrollInfo(IntPtr hWnd, int bar, ref SCROLLINFO info, [MarshalAs(UnmanagedType.Bool)] bool redraw);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern ushort RegisterClassW(ref WNDCLASS wndClass);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateWindowExW(int exStyle, string className, string windowName,
            int style, int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int command);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int index);

        [DllImport("user32.dll")]
        private static extern uint MsgWaitForMultipleObjects(uint count, IntPtr handles, [MarshalAs(UnmanagedType.Bool)] bool waitAll, uint milliseconds, uint wakeMask);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandleW(string moduleName);

        [DllImport("oleacc.dll")]
        private static extern IntPtr LresultFromObject(ref Guid riid, IntPtr wParam, IntPtr punk);
    }
}
