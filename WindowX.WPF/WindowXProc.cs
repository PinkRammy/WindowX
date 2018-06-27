using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace WindowX.WPF
{
    /// <summary>
    /// Contains methods and properties to aid with the Windows Messages handling sent to the WindowX.
    /// </summary>
    internal class WindowXProc
    {

        #region Windows API Enums

        /// <summary>
        /// Windows messages.
        /// </summary>
        private enum WindowsMessages : uint
        {
            Null = 0x0000,
            Destroy = 0x0002,
            Close = 0x0010,
            GETMINMAXINFO = 0x0024,
            NCCREATE = 0x0081,
            NCCALCSIZE = 0x0083,
            NCHITTEST = 0x0084,
            NCACTIVATE = 0x0086,
            SysCommand = 0x0112,
            SysMenu = 0xa4,
            WindowPositionChanging = 0x0046,
        }

        /// <summary>
        /// System commands.
        /// </summary>
        internal enum SystemCommand
        {
            Minimize = 0xF020,
            Maximize = 0xF030,
            Close = 0xF060,
            Restore = 0xF120
        }

        #endregion

        #region Windows API Structures

        /// <summary>
        /// Used with the DwmExtendFrameIntoClientArea function to extend the Aero composition further into a given window.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        /// <summary>
        /// Contains information about a window's maximized size and position and its minimum and maximum tracking size.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        /// <summary>
        /// Contains information about a display monitor.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public uint dwFlags = 0;
        }

        /// <summary>
        /// Defines the x- and y- coordinates of a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
            public POINT(System.Drawing.Point pt) : this(pt.X, pt.Y) { }

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }
            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }

        /// <summary>
        /// Defines the coordinates of the upper-left and lower-right corners of a rectangle.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }
            public int Y
            {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }
            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }
            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }
            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }
            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }
            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }
            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }
            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }
            public override int GetHashCode() { return ((System.Drawing.Rectangle)this).GetHashCode(); }
            public override string ToString() { return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom); }
        }

        /// <summary>
        /// Contains information about the size and position of a window.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPOS
        {
            /// <summary>
            /// A handle to the window.
            /// </summary>
            public IntPtr hwnd;

            /// <summary>
            /// The position of the window in Z order (front-to-back position).
            /// This member can be a handle to the window behind which this window is placed.
            /// </summary>
            public IntPtr hwndInsertAfter;

            /// <summary>
            /// The position of the left edge of the window.
            /// </summary>
            public int x;

            /// <summary>
            /// The position of the top edge of the window.
            /// </summary>
            public int y;

            /// <summary>
            /// The window width, in pixels.
            /// </summary>
            public int cx;

            /// <summary>
            /// The window height, in pixels.
            /// </summary>
            public int cy;

            /// <summary>
            /// The window position additional flags.
            /// </summary>
            public int flags;
        }

        #endregion

        #region Windows API Methods

        /// <summary>
        /// Extends the window frame into the client area.
        /// </summary>
        /// <param name="hwnd">The handle to the window in which the frame will be extended into the client area.</param>
        /// <param name="margins"> pointer to a <b>MARGINS</b> structure that describes the margins to use when extending the frame into the client area.</param>
        /// <returns>f this function succeeds, it returns 0. Otherwise, it returns an error code.</returns>
        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        /// <summary>
        /// Retrieves information about a display monitor.
        /// </summary>
        /// <param name="hMonitor">A handle to the display monitor of interest.</param>
        /// <param name="lpmi">A pointer to a <b>MONITORINFO</b> structure that receives information about the specified display monitor.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        /// <summary>
        /// Retrieves information about the specified window.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The zero-based offset to the value to be retrieved.</param>
        /// <returns>If the function succeeds, the return value is the requested value, otherwise zero.</returns>
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Retrieves information about the specified window.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The zero-based offset to the value to be retrieved.</param>
        /// <returns>If the function succeeds, the return value is the requested value, otherwise zero.</returns>
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLong64(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Retrieves information about the specified window.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The zero-based offset to the value to be retrieved.</param>
        /// <returns>If the function succeeds, the return value is the requested value, otherwise zero.</returns>
        private static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLong64(hWnd, nIndex);
            else
                return GetWindowLong32(hWnd, nIndex);
        }

        /// <summary>
        /// Retrieves a handle to the display monitor that has the largest area of intersection with the bounding rectangle of a specified window.
        /// </summary>
        /// <param name="hwnd">A handle to the window of interest.</param>
        /// <param name="dwFlags">Determines the function's return value if the window does not intersect any display monitor.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        /// <summary>
        /// Places (posts) a message in the message queue associated with the thread that created the specified window and returns without waiting for the thread to process the message.
        /// </summary>
        /// <param name="hwnd">A handle to the window whose window procedure is to receive the message.</param>
        /// <param name="msg">The message to be posted.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Sets the 32-bit (long) value at the specified offset into the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The zero-based offset to the value to be set.</param>
        /// <param name="dwNewLong">The replacement value.</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// Sets a value at the specified offset in the extra window memory
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The zero-based offset to the value to be set.</param>
        /// <param name="dwNewLong">The replacement value.</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        /// <summary>
        /// Sets a value at the specified offset in the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The zero-based offset to the value to be set.</param>
        /// <param name="dwNewLong">The replacement value.</param>
        /// <returns>If the function succeeds, the return value is the previous value of the specified offset, otherwise zero.</returns>
        private static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Create a new WindowX Windows API messages processor.
        /// </summary>
        /// <param name="wndx">The WindowX to process.</param>
        public WindowXProc(WindowX wndx)
        {
            // Check the given window
            if (wndx == null)
                throw new ArgumentNullException(nameof(wndx), "The given WindowX instance is null.");

            // Set the SwarmWindow border size
            _wndxBorderSize = (int)wndx.BorderSize;
        }

        #endregion

        #region Fields

        /// <summary>
        /// The WindowX border size.
        /// </summary>
        private int _wndxBorderSize;

        #endregion

        /// <summary>
        /// A delegate for window procedure callbacks.
        /// </summary>
        /// <param name="hwnd">The handle of the window.</param>
        /// <param name="msg">The window message code.</param>
        /// <param name="wParam">Additional message information. The contents of this parameter depend on the value of the msg parameter.</param>
        /// <param name="lParam">Additional message information. The contents of this parameter depend on the value of the msg parameter.</param>
        /// <param name="handled">A value that indicates whether the message was handled. Set the value to true if the message was handled; otherwise, false.</param>
        /// <returns>The appropriate return value depends on the particular message</returns>
        public IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Process the messages
            switch (msg)
            {
                // Get the window maximum size
                case (int)WindowsMessages.GETMINMAXINFO:
                    GetMinMaxInfo(hwnd, lParam, _wndxBorderSize);
                    handled = true;
                    break;

                // Handle window position and size changes
                case (int)WindowsMessages.WindowPositionChanging:
                    if (WindowChangePos(hwnd, lParam) == IntPtr.Zero) break;
                    handled = true;
                    break;
            }

            // Return result
            return IntPtr.Zero;
        }

        #region SwarmWindow MinMaxInfo

        /// <summary>
        /// Returns the WINDOWPOS for the window with the specified handle, when being maximized.
        /// </summary>
        /// <param name="hwnd">The handle of window to get the information for.</param>
        /// <returns>The WindowPos for the window with the given handle.</returns>
        private static WINDOWPOS GetMaximizedWindowInfo(IntPtr hwnd)
        {
            // Check the given handle
            if (hwnd == IntPtr.Zero)
                throw new ArgumentOutOfRangeException("The window handle should be different than zero.");

            // Get the monitor information
            var monitorInformation = GetMonitorInfo(hwnd);

            // Get the maximum size
            var width = Math.Abs(monitorInformation.rcWork.Right - monitorInformation.rcWork.Left);
            var height = Math.Abs(monitorInformation.rcWork.Bottom - monitorInformation.rcWork.Top);
            var left = Math.Abs(monitorInformation.rcWork.Left - monitorInformation.rcMonitor.Left);
            var top = Math.Abs(monitorInformation.rcWork.Top - monitorInformation.rcMonitor.Top);

            // Return the result
            return new WINDOWPOS
            {
                hwnd = hwnd,
                hwndInsertAfter = IntPtr.Zero,
                x = left,
                y = top,
                cx = width,
                cy = height
            };
        }

        /// <summary>
        /// Returns the MonitorInfo for the specified window.
        /// </summary>
        /// <param name="hwnd">The handle of window to get the information for.</param>
        /// <returns>The MonitorInfo for the given window.</returns>
        private static MONITORINFO GetMonitorInfo(IntPtr hwnd)
        {
            // Check the given handle
            if (hwnd == IntPtr.Zero)
                throw new ArgumentOutOfRangeException("The window handle should be different than zero.");

            // Get the monitor the window is on (nearest one)
            var monitor = MonitorFromWindow(hwnd, 0x00000002);

            // Check the monitor
            if (monitor == IntPtr.Zero) return null;

            // Initialize the monitor information
            var monitorInformation = new MONITORINFO();

            // Get the monitor information
            GetMonitorInfo(monitor, monitorInformation);

            // Return the result
            return monitorInformation;
        }

        /// <summary>
        /// Calculates the maximized window size.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="lParam">Additional information.</param>
        /// <param name="borderSize">The size of the WindowX border.</param>
        private static void GetMinMaxInfo(IntPtr hwnd, IntPtr lParam, int borderSize)
        {
            // Initialize the MINMAXINFO
            var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Get the maximum window size
            var maximumWindowInformation = GetMaximizedWindowInfo(hwnd);
            if (maximumWindowInformation.Equals(default(WINDOWPOS)))
                throw new NullReferenceException("Could not get the maximized window information.");

            // Set the maximum left and top positions
            mmi.ptMaxPosition.X = maximumWindowInformation.x;
            mmi.ptMaxPosition.Y = maximumWindowInformation.y;

            // Set the maximum work area size
            mmi.ptMaxSize.X = maximumWindowInformation.cx;
            mmi.ptMaxSize.Y = maximumWindowInformation.cy;

            // Set the information
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        #endregion

        #region WindowX WindowChangePos

        /// <summary>
        /// The previous window position and size information.
        /// </summary>
        private static WINDOWPOS _prevPos;

        /// <summary>
        /// Handles the given window position and size change.
        /// </summary>
        /// <param name="hwnd">The handle to the window.</param>
        /// <param name="lParam">Additional information.</param>
        private static IntPtr WindowChangePos(IntPtr hwnd, IntPtr lParam)
        {
            // Get the current position
            var pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

            // Check the position
            // 2 represents no move
            if ((pos.flags & 2) != 0) return IntPtr.Zero;

            // Get the window
            var window = (System.Windows.Window)HwndSource.FromHwnd(hwnd)?.RootVisual;

            // Check the window
            if (window == null) return IntPtr.Zero;

            // Get the DPI
            var presentationSource = PresentationSource.FromVisual(window);
            var dpiMatrix = presentationSource?.CompositionTarget?.TransformToDevice;

            // Initialize the flag
            var changedPosition = false;

            // Get the minimum values
            var minWidth = window.MinWidth * (dpiMatrix?.M11 ?? 1);
            var minHeight = window.MinHeight * (dpiMatrix?.M22 ?? 1);

            // Check for minimum width
            if (pos.cx < minWidth)
            {
                // Set the position
                pos.cx = (int)minWidth;

                // Set the x position
                pos.x = _prevPos.x;

                // Set the flag
                changedPosition = true;
            }
            if (pos.cy < minHeight)
            {
                // Set the position
                pos.cy = (int)minHeight;

                // Set the y position
                pos.y = _prevPos.y;

                // Set the flag
                changedPosition = true;
            }

            // Keep the position
            _prevPos = pos;

            // Check the flag
            if (!changedPosition) return IntPtr.Zero;

            // Set the new position
            Marshal.StructureToPtr(pos, lParam, true);
            return (IntPtr)1;
        }

        #endregion

        #region WindowX Manipulation

        /// <summary>
        /// Closes the window with the specified handle.
        /// </summary>
        /// <param name="hWnd">The handle of the window to close.</param>
        public void CloseWindow(IntPtr hWnd)
        {
            // Check the given handle
            if (hWnd.Equals(IntPtr.Zero)) return;

            // Close the window
            PostMessage(hWnd, (uint)WindowsMessages.SysCommand, new IntPtr((int)SystemCommand.Close), IntPtr.Zero);
        }

        /// <summary>
        /// Enables the Windows Aero animations on the window.
        /// </summary>
        public void EnableAeroAnimations(IntPtr hWnd)
        {
            // Check the given handle
            if (hWnd.Equals(IntPtr.Zero)) return;

            // The style index offset value
            const int GWL_STYLE = -16;

            // Get the window style
            var currentWindowStyle = GetWindowLong(hWnd, GWL_STYLE);

            // The titlebar window style value
            const int WS_CAPTION = 0x00800000 | 0x00400000;

            // Set the new window style value
            var newWindowStyle = new IntPtr(currentWindowStyle.ToInt64() + WS_CAPTION);

            // Set the window style to add animations
            SetWindowLong(hWnd, GWL_STYLE, newWindowStyle);
        }

        /// <summary>
        /// Enables or disables the Aero shadow to the window with the given handle
        /// </summary>
        /// <param name="hWnd">The handle to the window.</param>
        /// <param name="enabled">Determines if the shadow is enabled or disabled.</param>
        public void SetAeroShadow(IntPtr hWnd, bool enabled)
        {
            // Set the shadow state
            var shadow_state = new MARGINS
            {
                topHeight = enabled ? 1 : 0,
                bottomHeight = enabled ? 1 : 0,
                leftWidth = enabled ? 1 : 0,
                rightWidth = enabled ? 1 : 0
            };

            // Set the shadow
            DwmExtendFrameIntoClientArea(hWnd, ref shadow_state);
        }

        /// <summary>
        /// Maximizes the window with the specified handle.
        /// </summary>
        /// <param name="hWnd">The handle of the window to maximize.</param>
        public void MaximizeWindow(IntPtr hWnd)
        {
            // Check the given handle
            if (hWnd.Equals(IntPtr.Zero)) return;

            // Maximize the window
            PostMessage(hWnd, (uint)WindowsMessages.SysCommand, new IntPtr((int)SystemCommand.Maximize), IntPtr.Zero);
        }

        /// <summary>
        /// Minimizes the window with the specified handle.
        /// </summary>
        /// <param name="hWnd">The handle of the window to minimize.</param>
        public void MinimizeWindow(IntPtr hWnd)
        {
            // Check the given handle
            if (hWnd.Equals(IntPtr.Zero)) return;

            // Minimize the window
            PostMessage(hWnd, (uint)WindowsMessages.SysCommand, new IntPtr((int)SystemCommand.Minimize), IntPtr.Zero);
        }

        /// <summary>
        /// Restores the window with the specified handle.
        /// </summary>
        /// <param name="hWnd">The handle of the window to restore.</param>
        public void RestoreWindow(IntPtr hWnd)
        {
            // Check the given handle
            if (hWnd.Equals(IntPtr.Zero)) return;

            // Restore the window
            PostMessage(hWnd, (uint)WindowsMessages.SysCommand, new IntPtr((int)SystemCommand.Restore), IntPtr.Zero);
        }

        #endregion
    }
}
