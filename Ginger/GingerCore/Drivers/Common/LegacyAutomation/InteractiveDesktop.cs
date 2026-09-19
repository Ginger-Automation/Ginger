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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

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

        /// <summary>
        /// True when the desktop that currently owns input can be opened by this
        /// process, which is the precondition for physical mouse and keyboard to
        /// land on the application.
        /// </summary>
        /// <remarks>
        /// A workstation that is locked hands input to the secure desktop, which
        /// this process cannot open, so the probe returns false. A session that was
        /// detached with <c>tscon</c> still owns a usable input desktop and returns
        /// true - which is why this deliberately does not look at the terminal
        /// services connect state, where a detached-but-working session would look
        /// identical to a dead one.
        /// </remarks>
        public static bool IsAvailable()
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
            return true;
        }

        /// <summary>
        /// One line describing the session the run is happening in, written to the
        /// log when a driver starts.
        /// </summary>
        /// <remarks>
        /// These are the facts that explain an overnight run where most steps
        /// failed. An input desktop of Winlogon means the workstation is locked, so
        /// anything depending on focus or physical input was never going to work. A
        /// resolution of 1024x768 or 640x480 on a remote session is the signature of
        /// a session detached with tscon, which silently breaks coordinate and image
        /// based steps even though the desktop itself is perfectly usable.
        /// </remarks>
        public static string DescribeSession()
        {
            try
            {
                return "Session " + CurrentSessionId()
                    + (GetSystemMetrics(SM_REMOTESESSION) != 0 ? " (remote)" : " (console)")
                    + ", input desktop " + InputDesktopName()
                    + ", screen " + GetSystemMetrics(SM_CXSCREEN) + "x" + GetSystemMetrics(SM_CYSCREEN)
                    + ", physical input " + (IsAvailable() ? "available" : "unavailable");
            }
            catch (Exception ex)
            {
                return "Session details could not be determined: " + ex.Message;
            }
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
                return "not accessible (locked or secure desktop)";
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
