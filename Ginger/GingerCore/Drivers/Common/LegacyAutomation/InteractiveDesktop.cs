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

using Amdocs.Ginger.Common;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace GingerCore.Drivers.Common.LegacyAutomation
{
    /// <summary>
    /// Reports whether the physical mouse and keyboard can currently reach the
    /// application. The desktop engine and the driver helpers use this to decide
    /// whether physical input is still worth attempting, so the quiet window-message
    /// paths engage only where a click or keystroke would have been swallowed in
    /// silence. It also describes the session itself, so a run that failed overnight
    /// carries the evidence of why.
    /// </summary>
    public static class InteractiveDesktop
    {
        private const int DESKTOP_READOBJECTS = 0x0001;
        private const int DESKTOP_SWITCHDESKTOP = 0x0100;
        private const int ERROR_ACCESS_DENIED = 5;
        private const int UOI_NAME = 2;
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private const int SM_REMOTESESSION = 0x1000;

        /// <summary>
        /// Desktops that own input while no user is at the keyboard. Winlogon owns it
        /// from the moment a workstation locks until the credentials are accepted;
        /// Screen-saver owns it while a secure screen saver runs.
        /// </summary>
        private const string WinlogonDesktop = "Winlogon";
        private const string ScreenSaverDesktop = "Screen-saver";
        private const string DesktopNotAccessible = "not accessible (locked or secure desktop)";

        /// <summary>
        /// Terminal services session information. WTSSessionInfoEx carries the lock
        /// state, which is the one fact the desktop cannot be asked for.
        /// </summary>
        private const int WTSSessionInfoEx = 25;
        private const int WTSInfoExLevel1 = 1;
        private const int WTS_SESSIONSTATE_LOCK = 0;
        private static readonly IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

        /// <summary>
        /// Last state written to the log: -1 before the first probe, 0 unavailable,
        /// 1 available. Only transitions are logged, so a run that locks halfway
        /// through carries one line saying so instead of one line per action.
        /// </summary>
        private static int mLastLoggedState = -1;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr OpenInputDesktop(uint flags, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint desiredAccess);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetUserObjectInformationW(IntPtr handle, int index, StringBuilder info, uint length, out uint lengthNeeded);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int index);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("wtsapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WTSQuerySessionInformationW(IntPtr server, int sessionId, int infoClass, out IntPtr buffer, out int bytesReturned);

        [DllImport("wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr memory);

        /// <summary>
        /// Leading fields of WTSINFOEX. The four bytes after <c>Level</c> are the
        /// padding the native union carries to reach the alignment its 64-bit members
        /// need; the fields beyond <c>SessionFlags</c> are names and timestamps that
        /// are of no use here, and leaving them out is safe because the buffer is only
        /// ever read, never written back.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct SessionInfoExHeader
        {
            public int Level;
            public int Padding;
            public int SessionId;
            public int SessionState;
            public int SessionFlags;
        }

        /// <summary>
        /// True when physical mouse and keyboard can currently land on the
        /// application, which holds only while an ordinary user desktop owns input.
        /// </summary>
        /// <remarks>
        /// A workstation that is locked hands input to the secure desktop, so the
        /// probe returns false. It is identified by name rather than by whether the
        /// handle opens: a process running in the user's own session can still open
        /// the secure desktop on some configurations, and checking only the handle
        /// made a locked workstation indistinguishable from a user at the keyboard.
        /// A session that was detached with <c>tscon</c> still owns a usable input
        /// desktop and returns true - which is why this deliberately does not look at
        /// the terminal services connect state, where a detached-but-working session
        /// would look identical to a dead one.
        /// </remarks>
        public static bool IsAvailable()
        {
            bool available = Probe();
            LogStateChange(available);
            return available;
        }

        /// <summary>
        /// The probe itself, without the logging, so <see cref="DescribeSession"/> can
        /// report the state without counting as a state change.
        /// </summary>
        private static bool Probe()
        {
            IntPtr desktop;
            try
            {
                desktop = OpenInputDesktop(0, false, DESKTOP_SWITCHDESKTOP);
            }
            catch (Exception)
            {
                // Never let a probe failure block an action that would have worked.
                return true;
            }

            if (desktop == IntPtr.Zero)
            {
                // Only a denied handle proves the input desktop is out of reach.
                // Anything else (a restricted token, an unexpected error) fails open.
                return Marshal.GetLastWin32Error() != ERROR_ACCESS_DENIED;
            }

            CloseDesktop(desktop);

            try
            {
                // Asked first because it is the only one of the three that is always
                // conclusive. Neither holding a handle nor reading the desktop name
                // proves input lands on the application: a process in the user's own
                // session can open the secure desktop on some configurations, and
                // OpenInputDesktop can answer Default while the lock screen is up.
                if (IsSessionLocked())
                {
                    return false;
                }

                return !IsSecureDesktop(InputDesktopName());
            }
            catch (Exception)
            {
                return true;
            }
        }

        /// <summary>
        /// True while this session is locked, taken from the terminal services session
        /// flags rather than inferred from the desktop that owns input.
        /// </summary>
        /// <remarks>
        /// This is the fact the desktop cannot supply. A locked workstation was seen
        /// reporting an input desktop of Default, which made it indistinguishable from
        /// a user at the keyboard and left every quiet window-message path switched
        /// off for exactly the runs that needed it. Anything other than an explicit
        /// lock counts as unlocked, so a query that fails never blocks a step that
        /// would have worked. The session id is compared with the one asked about
        /// because that is what proves the buffer was read at the right offsets - a
        /// mismatch means the layout is wrong and the flags cannot be trusted.
        /// </remarks>
        private static bool IsSessionLocked()
        {
            return ReadSessionLockState() == SessionLockState.Locked;
        }

        /// <summary>
        /// The four answers the session flags can give. All three of the non-Locked
        /// ones behave identically - none of them blocks a step - but they are kept
        /// apart because they mean very different things when a run is being explained
        /// afterwards.
        /// </summary>
        /// <remarks>
        /// <see cref="Unavailable"/> and <see cref="Unreadable"/> were a single
        /// "unknown" until it became clear the two could not be told apart: a machine
        /// that cannot report its session lock at all and a build that reads the
        /// buffer at the wrong offsets both looked like a machine nobody had locked.
        /// Only the second is a defect, and it is the one that has to be noticed,
        /// because wrong offsets would report it on every run while the lock went
        /// undetected.
        /// </remarks>
        private enum SessionLockState
        {
            /// <summary>Windows was asked and would not answer, so there is nothing to read.</summary>
            Unavailable,

            /// <summary>Windows answered, but the buffer did not describe the session asked about.</summary>
            Unreadable,

            Unlocked,
            Locked
        }

        private static SessionLockState ReadSessionLockState()
        {
            int sessionId = CurrentSessionId();
            bool answered;
            IntPtr buffer;
            int bytesReturned;

            try
            {
                answered = WTSQuerySessionInformationW(WTS_CURRENT_SERVER_HANDLE, sessionId, WTSSessionInfoEx, out buffer, out bytesReturned);
            }
            catch (DllNotFoundException)
            {
                // Terminal services is not present at all, as on the trimmed server
                // installations, so there is no session lock to read here.
                return SessionLockState.Unavailable;
            }
            catch (EntryPointNotFoundException)
            {
                return SessionLockState.Unavailable;
            }

            if (!answered || buffer == IntPtr.Zero)
            {
                return SessionLockState.Unavailable;
            }

            try
            {
                if (bytesReturned < Marshal.SizeOf<SessionInfoExHeader>())
                {
                    return SessionLockState.Unreadable;
                }

                SessionInfoExHeader info = Marshal.PtrToStructure<SessionInfoExHeader>(buffer);
                if (info.Level != WTSInfoExLevel1 || info.SessionId != sessionId)
                {
                    return SessionLockState.Unreadable;
                }

                return info.SessionFlags == WTS_SESSIONSTATE_LOCK
                    ? SessionLockState.Locked
                    : SessionLockState.Unlocked;
            }
            finally
            {
                WTSFreeMemory(buffer);
            }
        }

        /// <summary>
        /// True while some window holds the foreground, which no process on the user
        /// desktop can see once the workstation is locked.
        /// </summary>
        /// <remarks>
        /// Reported alongside the desktop name rather than used to decide anything.
        /// It is the fact that explains a click the application never received: a step
        /// can fail for want of the foreground on a perfectly unlocked desktop, which
        /// the desktop name alone gives no way to tell.
        /// </remarks>
        private static bool HasForegroundWindow()
        {
            return GetForegroundWindow() != IntPtr.Zero;
        }

        /// <summary>
        /// True for the desktops that own input when nobody is at the keyboard. A name
        /// that cannot be read is treated as usable, so an unexpected failure never
        /// blocks a step that would have worked.
        /// </summary>
        private static bool IsSecureDesktop(string name)
        {
            return string.Equals(name, WinlogonDesktop, StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, ScreenSaverDesktop, StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, DesktopNotAccessible, StringComparison.Ordinal);
        }

        /// <summary>
        /// Writes one line the first time the state is probed and one more each time it
        /// flips, so a run that was locked can be proved from its log afterwards
        /// instead of being inferred from the driver start-up line.
        /// </summary>
        private static void LogStateChange(bool available)
        {
            int current = available ? 1 : 0;
            if (Interlocked.Exchange(ref mLastLoggedState, current) == current)
            {
                return;
            }

            try
            {
                // Describes the value that was actually returned. Probing a second time
                // here let the line report a state the caller was never given, and the
                // flip is the one moment where a second probe is most likely to differ.
                Reporter.ToLog(eLogLevel.INFO, "Ginger desktop input state: " + Describe(available));
            }
            catch (Exception)
            {
                // Logging must never be the reason an action fails.
            }
        }

        /// <summary>
        /// One line describing the session the run is happening in, written to the
        /// log when a driver starts.
        /// </summary>
        /// <remarks>
        /// These are the facts that explain an overnight run where most steps
        /// failed. An input desktop of Winlogon means the workstation is locked, so
        /// anything depending on focus or physical input was never going to work. The
        /// session lock is reported next to it rather than folded into it, because the
        /// two disagree: a locked workstation has been seen naming its input desktop
        /// Default, and reading only the name is what let a locked run be recorded as
        /// a user at the keyboard. No
        /// foreground window says a step could not have been given focus even on a
        /// desktop that was never locked, which the desktop name cannot show. A
        /// resolution of 1024x768 or 640x480 on a remote session is the signature of
        /// a session detached with tscon, which silently breaks coordinate and image
        /// based steps even though the desktop itself is perfectly usable.
        /// </remarks>
        public static string DescribeSession()
        {
            return Describe(Probe());
        }

        /// <summary>
        /// Describes the session around a state that has already been probed, so the
        /// line always reports the value its caller was given.
        /// </summary>
        private static string Describe(bool available)
        {
            try
            {
                return "Session " + CurrentSessionId()
                    + (GetSystemMetrics(SM_REMOTESESSION) != 0 ? " (remote)" : " (console)")
                    + ", input desktop " + InputDesktopName()
                    + ", session " + DescribeLockState(ReadSessionLockState())
                    + ", screen " + GetSystemMetrics(SM_CXSCREEN) + "x" + GetSystemMetrics(SM_CYSCREEN)
                    + ", foreground window " + (HasForegroundWindow() ? "present" : "absent")
                    + ", physical input " + (available ? "available" : "unavailable");
            }
            catch (Exception ex)
            {
                return "Session details could not be determined: " + ex.Message;
            }
        }

        private static string DescribeLockState(SessionLockState state)
        {
            return state switch
            {
                SessionLockState.Locked => "locked",
                SessionLockState.Unlocked => "unlocked",

                // Worded apart so a run can be explained without the source: the first
                // is a fact about the machine and the second is a defect in this code.
                SessionLockState.Unreadable => "lock state unreadable",
                _ => "lock state unavailable"
            };
        }

        /// <summary>
        /// Name of the desktop that currently owns input: Default when a user is
        /// working, Winlogon once the workstation locks.
        /// </summary>
        private static string InputDesktopName()
        {
            IntPtr desktop = OpenInputDesktop(0, false, DESKTOP_READOBJECTS);
            if (desktop == IntPtr.Zero)
            {
                return DesktopNotAccessible;
            }

            try
            {
                StringBuilder name = new StringBuilder(256);
                return GetUserObjectInformationW(desktop, UOI_NAME, name, (uint)(name.Capacity * sizeof(char)), out _)
                    ? name.ToString()
                    : "unknown";
            }
            finally
            {
                CloseDesktop(desktop);
            }
        }

        private static int CurrentSessionId()
        {
            using Process current = Process.GetCurrentProcess();
            return current.SessionId;
        }
    }
}
