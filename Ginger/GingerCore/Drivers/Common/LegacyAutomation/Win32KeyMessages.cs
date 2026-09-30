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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace GingerCore.Drivers.Common.LegacyAutomation
{
    /// <summary>
    /// Delivers keystrokes through window messages instead of the physical
    /// keyboard, so Ginger can type into edit / button HWNDs without the target
    /// window being in the foreground or the desktop being unlocked.
    /// </summary>
    public static class Win32KeyMessages
    {
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_CHAR = 0x0102;

        /// <summary>
        /// Asks a dialog to move the keyboard to the next control in its tab order,
        /// which is the same work the dialog manager does when it sees a Tab.
        /// </summary>
        private const int WM_NEXTDLGCTL = 0x0028;

        /// <summary>
        /// The window class Windows gives a dialog, and the only class whose window
        /// procedure routes to <c>DefDlgProc</c>.
        /// </summary>
        private const string DialogClass = "#32770";

        private const ushort VK_TAB = 0x09;

        private const uint SMTO_BLOCK = 0x0001;
        private const uint SMTO_ABORTIFHUNG = 0x0002;

        private const int EditorPollMs = 25;

        /// <summary>
        /// The least time a queued click is given to be taken up before the window
        /// that has the keyboard is read. An application that is pumping handles it
        /// far quicker; one that is not was never going to take the text either.
        /// </summary>
        public const int EditorSettleMs = 150;

        /// <summary>
        /// How long a keystroke meant to move the keyboard is given to be seen doing
        /// it before it is called a failure.
        /// </summary>
        /// <remarks>
        /// A control need not move the focus inside the window procedure that took
        /// the key - posting itself the work and returning is just as ordinary - so
        /// reading the focus the instant the send returns can catch the application
        /// mid-step and report a Tab that did land as one that did not. Waiting ends
        /// as soon as the move is seen, so only a genuine failure pays the whole
        /// interval.
        /// </remarks>
        private const int FocusSettleMs = 100;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);

        [StructLayout(LayoutKind.Sequential)]
        private struct GUITHREADINFO
        {
            public uint cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        /// <summary>
        /// Returns the focused child HWND inside <paramref name="topLevelHwnd"/>, or
        /// <see cref="IntPtr.Zero"/> when the focus cannot be narrowed to a child. A
        /// key message posted to a frame returns success while doing nothing, so the
        /// caller must treat <see cref="IntPtr.Zero"/> as a failure rather than
        /// falling back.
        /// </summary>
        /// <remarks>
        /// The focus is read per thread, not per window, and one UI thread commonly
        /// owns several top-level windows - a main frame and a modeless dialog being
        /// the ordinary case. The thread's focus lives in whichever of them is active,
        /// so the window asked about has to be confirmed as an ancestor of the answer.
        /// Without that check a step that located window A by title typed into a field
        /// of window B and reported the keys as sent.
        /// </remarks>
        public static IntPtr ResolveFocusedChild(IntPtr topLevelHwnd)
        {
            if (topLevelHwnd == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            uint threadId = GetWindowThreadProcessId(topLevelHwnd, out _);
            if (threadId == 0)
            {
                return IntPtr.Zero;
            }

            GUITHREADINFO info = new GUITHREADINFO { cbSize = (uint)Marshal.SizeOf(typeof(GUITHREADINFO)) };
            if (GetGUIThreadInfo(threadId, ref info)
                && Win32Native.IsChildWindow(info.hwndFocus)
                && BelongsTo(topLevelHwnd, info.hwndFocus))
            {
                return info.hwndFocus;
            }

            return Win32Native.IsChildWindow(topLevelHwnd) ? topLevelHwnd : IntPtr.Zero;
        }

        /// <summary>
        /// Whether <paramref name="candidate"/> is <paramref name="topLevelHwnd"/>
        /// itself or sits somewhere beneath it.
        /// </summary>
        private static bool BelongsTo(IntPtr topLevelHwnd, IntPtr candidate)
        {
            return candidate == topLevelHwnd || IsChild(topLevelHwnd, candidate);
        }

        /// <summary>
        /// Sends VK_TAB down + up to <paramref name="hwnd"/> via window messages,
        /// reporting whether the focus actually moved.
        /// </summary>
        /// <remarks>
        /// Tab is not the focused control's to handle. It belongs to the dialog
        /// manager, which sees it through <c>IsDialogMessage</c> in the application's
        /// own message loop, and a message sent straight to a child never passes
        /// through that loop. So a standard dialog control receives the key, has no
        /// reason to act on it, and returns the same zero it would have returned for
        /// a key it acted on. Controls that read keys themselves - a PowerBuilder
        /// DataWindow moving between columns - do act on it, which is the case this
        /// is here to serve.
        ///
        /// Because neither answer distinguishes the two, success is measured by
        /// watching the focus and the caret instead, and a Tab that leaves both where
        /// they were is reported as a failure. Delivery alone used to count, which
        /// meant a field that was never committed was reported as committed and the
        /// keyboard behind it never ran.
        ///
        /// A dialog control never acts on the key, for the reason above, so the
        /// dialog is asked to advance the keyboard itself once the key has been seen
        /// to do nothing. That reaches the same code the dialog manager would have
        /// run, which is what gives the field the kill-focus it commits on. Without
        /// it every standard dialog fell through to the keyboard, pulling the
        /// application to the foreground to press a key the application could have
        /// been asked for directly.
        /// </remarks>
        public static bool SendTab(IntPtr hwnd, int timeoutMs)
        {
            if (hwnd == IntPtr.Zero)
            {
                return false;
            }

            FocusSnapshot before = CaptureFocus(hwnd);
            if (!before.Readable)
            {
                // With nothing to compare against afterwards there is no way to tell a
                // Tab that worked from one that was thrown away, and reporting the
                // unverifiable as success is the defect this guards against.
                return false;
            }

            uint timeout = NormalizeTimeout(timeoutMs);
            if (SendKey(hwnd, VK_TAB, timeout) && FocusSettled(before, hwnd))
            {
                return true;
            }

            IntPtr dialog = GetParent(hwnd);
            return IsDialog(dialog)
                && Deliver(dialog, WM_NEXTDLGCTL, IntPtr.Zero, timeout)
                && FocusSettled(before, hwnd);
        }

        /// <summary>
        /// Whether a window is a true Win32 dialog, which is the only kind that acts
        /// on <see cref="WM_NEXTDLGCTL"/>.
        /// </summary>
        /// <remarks>
        /// The message does its work in <c>DefDlgProc</c>, and only the dialog class
        /// reaches there. Sent to anything else - a WinForms form, a PowerBuilder
        /// frame - it arrives at a window procedure that never expected the number,
        /// where being ignored is the good outcome and being mistaken for one of the
        /// procedure's own messages is the bad one. Asking the class first keeps the
        /// attempt to the windows it is defined for; the rest fall through to the
        /// keyboard exactly as they did before the message was tried at all.
        /// </remarks>
        private static bool IsDialog(IntPtr hwnd)
        {
            return hwnd != IntPtr.Zero
                && string.Equals(Win32Native.GetWindowClassName(hwnd), DialogClass, StringComparison.Ordinal);
        }

        /// <summary>
        /// Waits for the keyboard to be seen somewhere other than where
        /// <paramref name="before"/> found it, up to <see cref="FocusSettleMs"/>.
        /// </summary>
        private static bool FocusSettled(FocusSnapshot before, IntPtr hwnd)
        {
            for (int waited = 0; waited < FocusSettleMs; waited += EditorPollMs)
            {
                if (FocusMoved(before, hwnd))
                {
                    return true;
                }
                Thread.Sleep(EditorPollMs);
            }

            return FocusMoved(before, hwnd);
        }

        /// <summary>
        /// Sends a single virtual-key down + up and reports whether the window took
        /// both messages.
        /// </summary>
        private static bool SendKey(IntPtr hwnd, ushort virtualKey, uint timeout)
        {
            return Deliver(hwnd, WM_KEYDOWN, (IntPtr)virtualKey, timeout)
                && Deliver(hwnd, WM_KEYUP, (IntPtr)virtualKey, timeout);
        }

        /// <summary>
        /// Sends one message and reports whether the window took it within the
        /// timeout.
        /// </summary>
        /// <remarks>
        /// Taking a message is not acting on it. The value returned here says only
        /// that the window procedure ran and returned in time; what it made of the
        /// message goes to the discarded out parameter, and the messages this class
        /// sends all document that as zero whether they acted or not. Callers that
        /// need to know something happened have to look at the application for it.
        /// </remarks>
        private static bool Deliver(IntPtr hwnd, int message, IntPtr wParam, uint timeout)
        {
            if (hwnd == IntPtr.Zero)
            {
                return false;
            }

            return SendMessageTimeout(hwnd, message, wParam, IntPtr.Zero,
                SMTO_BLOCK | SMTO_ABORTIFHUNG, timeout, out _) != IntPtr.Zero;
        }

        /// <summary>
        /// Sends one message and records in <paramref name="anyDelivered"/> whether
        /// the window took it, so a sequence that stops partway knows how much of
        /// itself already arrived.
        /// </summary>
        private static bool Deliver(IntPtr hwnd, int message, IntPtr wParam, uint timeout, ref bool anyDelivered)
        {
            bool delivered = Deliver(hwnd, message, wParam, timeout);
            anyDelivered |= delivered;
            return delivered;
        }

        /// <summary>
        /// Waits for a click already queued on <paramref name="hwnd"/> to be taken
        /// up, and answers with the window that then holds the keyboard.
        /// </summary>
        /// <remarks>
        /// A coordinate click is posted rather than delivered in line, so it is
        /// still sitting in the application's queue when the call that queued it
        /// returns. Typing at that moment addresses whatever held the keyboard
        /// before the click, and where a grid edits a cell through a window it
        /// creates on demand, that window does not exist yet.
        ///
        /// The caret is watched as well as the focused window because a control
        /// that draws its own editor keeps the same window and only shows a caret.
        ///
        /// There is a floor as well as a ceiling on the wait, and the floor is the
        /// part that matters. A control that already held the keyboard shows
        /// nothing moving when it is clicked, so returning as soon as no change is
        /// seen would hand back the state that preceded the click and the caller
        /// would type ahead of it. There is no way to ask another thread whether it
        /// has drained its queue, so the floor stands in for one.
        ///
        /// That makes the wait a guess, and it is treated as one: the caller
        /// confirms the value by reading the target back, so a floor that was too
        /// short shows up as an honest failure rather than a false success.
        /// </remarks>
        public static IntPtr WaitForEditor(IntPtr hwnd, FocusSnapshot beforeClick, int timeoutMs)
        {
            int waited = 0;
            while (waited < EditorSettleMs || (waited < timeoutMs && !FocusMoved(beforeClick, hwnd)))
            {
                Thread.Sleep(EditorPollMs);
                waited += EditorPollMs;
            }

            return ResolveFocusedChild(hwnd);
        }

        /// <summary>
        /// Where the focus and the caret sat at one moment, which is the only
        /// evidence available that a click or a keystroke meant to move them did.
        /// </summary>
        public readonly struct FocusSnapshot
        {
            internal FocusSnapshot(IntPtr focus, RECT caret)
            {
                Readable = true;
                Focus = focus;
                Caret = caret;
            }

            public bool Readable { get; }

            private IntPtr Focus { get; }

            private RECT Caret { get; }

            /// <summary>
            /// Compares the caret as well as the focused window, because a control
            /// that handles Tab internally - a DataWindow stepping to the next column
            /// - keeps the same HWND and moves only the caret.
            /// </summary>
            internal bool DiffersFrom(FocusSnapshot other)
            {
                return Focus != other.Focus
                    || Caret.left != other.Caret.left
                    || Caret.top != other.Caret.top
                    || Caret.right != other.Caret.right
                    || Caret.bottom != other.Caret.bottom;
            }
        }

        public static FocusSnapshot CaptureFocus(IntPtr hwnd)
        {
            uint threadId = GetWindowThreadProcessId(hwnd, out _);
            if (threadId == 0)
            {
                return default;
            }

            GUITHREADINFO info = new GUITHREADINFO { cbSize = (uint)Marshal.SizeOf(typeof(GUITHREADINFO)) };
            return GetGUIThreadInfo(threadId, ref info)
                ? new FocusSnapshot(info.hwndFocus, info.rcCaret)
                : default;
        }

        private static bool FocusMoved(FocusSnapshot before, IntPtr hwnd)
        {
            FocusSnapshot after = CaptureFocus(hwnd);
            return after.Readable && after.DiffersFrom(before);
        }

        /// <summary>
        /// Types a literal Unicode string through <c>WM_CHAR</c>. The text is sent
        /// verbatim, so SendKeys-style tokens and modifiers are not interpreted.
        /// </summary>
        /// <remarks>
        /// Sending stops at the first character the window refuses, so the caller
        /// sees a failure and retypes the whole value. Pushing the rest of the
        /// string after a refusal would leave the control holding text that is
        /// neither the old value nor the new one.
        /// </remarks>
        public static bool SendChars(IntPtr hwnd, string text, int timeoutMs)
        {
            if (hwnd == IntPtr.Zero)
            {
                return false;
            }
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            uint timeout = NormalizeTimeout(timeoutMs);
            foreach (char c in text)
            {
                if (!Deliver(hwnd, WM_CHAR, (IntPtr)c, timeout))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Sends a Send Keys value, so a step can press Enter or F5 rather than only
        /// type letters.
        /// </summary>
        /// <param name="failure">
        /// Why the value could not be sent, phrased to read inside a longer sentence,
        /// when this returns false.
        /// </param>
        /// <remarks>
        /// A key handled by the dialog manager rather than by the control - Tab and
        /// Enter in a standard dialog - is seen through <c>IsDialogMessage</c> in the
        /// application's own message loop, and a message sent straight to a child
        /// never passes through it. Controls that read keys themselves, which is the
        /// case this exists for, do act on them. Nothing here can tell the two apart,
        /// so callers that need to know a key was acted on have to look at the
        /// application for it.
        ///
        /// Sending stops at the first keystroke the window refuses, for the same
        /// reason <see cref="SendChars"/> does: finishing the sequence after a
        /// refusal leaves the control in a state that is neither where it started nor
        /// where the step meant to put it.
        /// </remarks>
        public static bool TrySendNotation(IntPtr hwnd, string value, int timeoutMs, out string failure)
            => TrySendNotation(hwnd, value, timeoutMs, out failure, out _);

        /// <param name="partiallyDelivered">
        /// Whether any of the value reached the window before it stopped taking
        /// messages, when this returns false. A caller with the keyboard behind it
        /// must not retry there: the window is still holding what arrived, so sending
        /// the value again leaves a second copy of that prefix in the control and
        /// presses any Enter or Tab in it twice.
        /// </param>
        public static bool TrySendNotation(IntPtr hwnd, string value, int timeoutMs, out string failure, out bool partiallyDelivered)
        {
            partiallyDelivered = false;

            if (!TryParseNotation(value, out List<KeyStroke> strokes, out failure))
            {
                return false;
            }

            if (hwnd == IntPtr.Zero)
            {
                failure = "there is no window to send the keys to";
                return false;
            }

            uint timeout = NormalizeTimeout(timeoutMs);
            bool anyDelivered = false;
            foreach (KeyStroke stroke in strokes)
            {
                if (!SendStroke(hwnd, stroke, timeout, ref anyDelivered))
                {
                    partiallyDelivered = anyDelivered;
                    failure = anyDelivered
                        ? "window " + hwnd.ToInt64() + " took part of the value and then stopped accepting keys"
                        : "window " + hwnd.ToInt64() + " did not accept the keys";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Reproduces what a message loop would have made of one key press: the key
        /// going down, the character it translates to where it makes one, then the
        /// key coming up.
        /// </summary>
        /// <param name="anyDelivered">
        /// Set once any message has been taken. One key press is several messages and
        /// a control that reads keys itself can act on the first of them, so what
        /// arrived has to be counted per message rather than per keystroke.
        /// </param>
        /// <remarks>
        /// The translation step is the one that is easy to leave out and impossible
        /// to do without. A standard edit control acts on the character, not on the
        /// key, so an Enter delivered as a key press alone reaches it and does
        /// nothing.
        /// </remarks>
        private static bool SendStroke(IntPtr hwnd, KeyStroke stroke, uint timeout, ref bool anyDelivered)
        {
            if (stroke.VirtualKey == 0)
            {
                return Deliver(hwnd, WM_CHAR, (IntPtr)stroke.Character, timeout, ref anyDelivered);
            }

            return Deliver(hwnd, WM_KEYDOWN, (IntPtr)stroke.VirtualKey, timeout, ref anyDelivered)
                && (stroke.Character == '\0' || Deliver(hwnd, WM_CHAR, (IntPtr)stroke.Character, timeout, ref anyDelivered))
                && Deliver(hwnd, WM_KEYUP, (IntPtr)stroke.VirtualKey, timeout, ref anyDelivered);
        }

        /// <summary>
        /// One thing to deliver: a character to type, or a key to press and the
        /// character it produces on the way through a message loop.
        /// </summary>
        private readonly struct KeyStroke
        {
            internal KeyStroke(char character, ushort virtualKey = 0)
            {
                Character = character;
                VirtualKey = virtualKey;
            }

            internal char Character { get; }

            internal ushort VirtualKey { get; }
        }

        /// <summary>
        /// The keys a value can name, and the character each produces. Only the few
        /// that produce one carry it.
        /// </summary>
        private static readonly Dictionary<string, KeyStroke> NamedKeys =
            new Dictionary<string, KeyStroke>(StringComparer.OrdinalIgnoreCase)
            {
                { "BACKSPACE", new KeyStroke('\b', 0x08) },
                { "BS", new KeyStroke('\b', 0x08) },
                { "BKSP", new KeyStroke('\b', 0x08) },
                { "TAB", new KeyStroke('\t', VK_TAB) },
                { "ENTER", new KeyStroke('\r', 0x0D) },
                { "RETURN", new KeyStroke('\r', 0x0D) },
                { "ESC", new KeyStroke((char)0x1B, 0x1B) },
                { "ESCAPE", new KeyStroke((char)0x1B, 0x1B) },
                { "BREAK", new KeyStroke('\0', 0x03) },
                { "CAPSLOCK", new KeyStroke('\0', 0x14) },
                { "CLEAR", new KeyStroke('\0', 0x0C) },
                { "DELETE", new KeyStroke('\0', 0x2E) },
                { "DEL", new KeyStroke('\0', 0x2E) },
                { "DOWN", new KeyStroke('\0', 0x28) },
                { "END", new KeyStroke('\0', 0x23) },
                { "HELP", new KeyStroke('\0', 0x2F) },
                { "HOME", new KeyStroke('\0', 0x24) },
                { "INSERT", new KeyStroke('\0', 0x2D) },
                { "INS", new KeyStroke('\0', 0x2D) },
                { "LEFT", new KeyStroke('\0', 0x25) },
                { "NUMLOCK", new KeyStroke('\0', 0x90) },
                { "PGDN", new KeyStroke('\0', 0x22) },
                { "PGUP", new KeyStroke('\0', 0x21) },
                { "PRTSC", new KeyStroke('\0', 0x2C) },
                { "RIGHT", new KeyStroke('\0', 0x27) },
                { "SCROLLLOCK", new KeyStroke('\0', 0x91) },
                { "UP", new KeyStroke('\0', 0x26) },
                { "F1", new KeyStroke('\0', 0x70) },
                { "F2", new KeyStroke('\0', 0x71) },
                { "F3", new KeyStroke('\0', 0x72) },
                { "F4", new KeyStroke('\0', 0x73) },
                { "F5", new KeyStroke('\0', 0x74) },
                { "F6", new KeyStroke('\0', 0x75) },
                { "F7", new KeyStroke('\0', 0x76) },
                { "F8", new KeyStroke('\0', 0x77) },
                { "F9", new KeyStroke('\0', 0x78) },
                { "F10", new KeyStroke('\0', 0x79) },
                { "F11", new KeyStroke('\0', 0x7A) },
                { "F12", new KeyStroke('\0', 0x7B) },
                { "ADD", new KeyStroke('+', 0x6B) },
                { "SUBTRACT", new KeyStroke('-', 0x6D) },
                { "MULTIPLY", new KeyStroke('*', 0x6A) },
                { "DIVIDE", new KeyStroke('/', 0x6F) },
            };

        /// <summary>
        /// Reads the notation a Send Keys value is written in and says what of it can
        /// travel as window messages.
        /// </summary>
        /// <remarks>
        /// Literal text and named keys both can. A modifier cannot: a window message
        /// leaves the target thread's key state untouched, so a control asking
        /// GetKeyState whether Ctrl is down is told it is not, and Ctrl+A arrives as
        /// a plain A. There is no way to set that state for another process.
        ///
        /// The refusal matters as much as the parsing. Typing the value verbatim
        /// would put the literal text "{ENTER}" into a field and record the step as
        /// passed, and dropping the modifier would run a different step from the one
        /// that was written, so anything not understood is handed back to the caller.
        /// </remarks>
        private static bool TryParseNotation(string value, out List<KeyStroke> strokes, out string refusal)
        {
            strokes = [];
            refusal = null;

            int at = 0;
            while (at < (value?.Length ?? 0))
            {
                char c = value[at];
                switch (c)
                {
                    case '+':
                    case '^':
                    case '%':
                        refusal = "it holds the modifier '" + c + "'";
                        return false;

                    case '(':
                    case ')':
                        // Grouping only ever qualifies a modifier.
                        refusal = "it groups keys under a modifier";
                        return false;

                    case '}':
                        refusal = "it closes a brace that was never opened";
                        return false;

                    case '~':
                        strokes.Add(NamedKeys["ENTER"]);
                        at++;
                        break;

                    case '{':
                        if (!TryReadBraced(value, ref at, strokes, out refusal))
                        {
                            return false;
                        }
                        break;

                    default:
                        strokes.Add(new KeyStroke(c));
                        at++;
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Reads one braced group: a named key, a single escaped character, or either
        /// of those followed by a repeat count.
        /// </summary>
        private static bool TryReadBraced(string value, ref int at, List<KeyStroke> strokes, out string refusal)
        {
            refusal = null;
            int close = value.IndexOf('}', at + 1);

            // "{}}" escapes a closing brace, so the first '}' is the content rather
            // than the terminator.
            if (close == at + 1 && close + 1 < value.Length && value[close + 1] == '}')
            {
                close++;
            }

            if (close < 0)
            {
                refusal = "it opens a brace that is never closed";
                return false;
            }

            string body = value.Substring(at + 1, close - at - 1);
            at = close + 1;

            int repeat = 1;
            int space = body.LastIndexOf(' ');
            if (space > 0 && int.TryParse(body.Substring(space + 1), out int count) && count > 0)
            {
                repeat = count;
                body = body.Substring(0, space);
            }

            if (!NamedKeys.TryGetValue(body, out KeyStroke stroke))
            {
                // A brace around a single character escapes it: "{+}" is a plus sign
                // rather than the Shift modifier.
                if (body.Length != 1)
                {
                    refusal = body.Length == 0
                        ? "it holds an empty {}"
                        : "it names the key '" + body + "', which is not one this recognises";
                    return false;
                }
                stroke = new KeyStroke(body[0]);
            }

            for (int i = 0; i < repeat; i++)
            {
                strokes.Add(stroke);
            }

            return true;
        }

        private static uint NormalizeTimeout(int timeoutMs)
        {
            return (uint)Math.Max(100, Math.Min(timeoutMs <= 0 ? 2000 : timeoutMs, 30000));
        }
    }
}
