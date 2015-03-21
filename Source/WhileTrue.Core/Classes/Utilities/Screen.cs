﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Classes.Utilities
{
    /// <summary>
    /// Retrieves information about the screen settings and provides an event to detect changes.
    /// </summary>
    public class Screen : ObservableObject
    {
        #region Win32 API

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip,
           EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        private delegate bool EnumMonitorsDelegate(
            IntPtr hMonitor, // handle to display monitor
            IntPtr hdcMonitor, // handle to monitor DC
            ref Rectangle lprcMonitor, // monitor intersection rectangle
            IntPtr dwData // data
            );

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MonitorInfoEx
        {
            // ReSharper disable InconsistentNaming
            public int cbSize;
            public Rectangle rcMonitor;
            public Rectangle rcWork;
            public UInt32 dwFlags;
            // ReSharper restore InconsistentNaming
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out Rectangle lpRect);      
        
        #endregion

        static Screen()
        {
            taskbarLocationChangePollThread = new TaskbarLocationChangePollThread(FindWindow("Shell_TrayWnd", null));
            taskbarLocationChangePollThread.Start();
        }

        private class TaskbarLocationChangePollThread : ThreadBase
        {
            private readonly IntPtr taskbarHandle;

            public TaskbarLocationChangePollThread(IntPtr taskbarHandle)
                :base("TaskBar Track Thread",true)
            {
                this.taskbarHandle = taskbarHandle;
                GetWindowRect(this.taskbarHandle, out taskbarRectangle);
            }

            protected override void Run()
            {
                while (true)
                {
                    Rectangle CurrentTaskbarLocation;
                    GetWindowRect(this.taskbarHandle, out CurrentTaskbarLocation);
                    bool TaskbarChanged = false;
                    lock (typeof(Screen))
                    {
                        if (CurrentTaskbarLocation != taskbarRectangle)
                        {
                            taskbarRectangle = CurrentTaskbarLocation;
                            TaskbarChanged = true;
                        }
                    }
                    this.Sleep(500);
                    if (TaskbarChanged)
                    {
                        InvokeMonitorDisplayChanged();
                    }
                }
// ReSharper disable FunctionNeverReturns
            }
// ReSharper restore FunctionNeverReturns
        }

        private static readonly TaskbarLocationChangePollThread taskbarLocationChangePollThread;
        private static Rectangle taskbarRectangle;
        
        /// <summary>
        /// Returns a colleaction of screen obect for each connected monitor
        /// </summary>
        public static Screen[] GetScreens()
        {
            List<Screen> Displays = new List<Screen>();

            GCHandle DisplayList = GCHandle.Alloc(Displays);
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, EnumerateDesktops, GCHandle.ToIntPtr(DisplayList));

            return Displays.ToArray();
        }

        private static bool EnumerateDesktops(IntPtr hMonitor, IntPtr hdcMonitor, ref Rectangle lprcMonitor, IntPtr dwData)
        {
            List<Screen> Displays = (List<Screen>) GCHandle.FromIntPtr(dwData).Target;
            Displays.Add( new Screen(hMonitor, hdcMonitor, lprcMonitor) );
            return true;
        }

        /// <summary>
        /// Gets the screen on which the taskbar resides
        /// </summary>
        public static Screen PrimaryScreen
        {
            get
            {
                foreach (Screen Display in GetScreens())
                {
                    if (Display.TaskbarLocation != TaskbarLocation.None)
                    {
                        return Display;
                    }
                }
                throw new InvalidOperationException("Taskbar was not found on any screen... bug?");
            }
        }

        /// <summary>
        /// Fires if the setting of screens is altered
        /// </summary>
        public static event EventHandler ScreenChanged;

        private static void InvokeMonitorDisplayChanged()
        {
            if (ScreenChanged != null)
            {
                ScreenChanged(null, new EventArgs());
            }
        }


        private readonly IntPtr monitorHandle;
        private readonly IntPtr monitorDCHandle;
        private readonly Rectangle bounds;
        private Screen(IntPtr monitorHandle, IntPtr monitorDCHandle, Rectangle bounds)
        {
            Screen.ScreenChanged += this.ScreenScreenChanged;
            this.monitorHandle = monitorHandle;
            this.monitorDCHandle = monitorDCHandle;
            this.bounds = bounds;
        }

        void ScreenScreenChanged(object sender, EventArgs e)
        {
            this.InvokePropertyChanged(()=>WorkingArea);
            this.InvokePropertyChanged(()=>TaskbarLocation);
        }
        
        /// <summary>
        /// Gets the bounds of the screen
        /// </summary>
        public Rectangle Bounds
        {
            get { return this.bounds; }
        }

        /// <summary>
        /// Gets the working area (i.e. Bounds excluding the taskbar) of the screen
        /// </summary>
        public Rectangle WorkingArea
        {
            get
            {
                MonitorInfoEx MonitorInfo = new MonitorInfoEx {cbSize = Marshal.SizeOf(typeof (MonitorInfoEx))};
                GetMonitorInfo(this.monitorHandle, ref MonitorInfo);

                return MonitorInfo.rcWork;
            }
        }

        /// <summary>
        /// Gets the current location of the taskbar.
        /// </summary>
        public TaskbarLocation TaskbarLocation
        {
            get
            {
                Rectangle CurrentTaskbarRectangle;
                lock (typeof(Screen))
                {
                    CurrentTaskbarRectangle = taskbarRectangle;
                }

                Rectangle ScreenBounds = Bounds;
                if (ScreenBounds.IntersectsWith(CurrentTaskbarRectangle))
                {
                    //taskbar is located on this screen
                    if (ScreenBounds.Top == CurrentTaskbarRectangle.Top)
                    {
                        //may be: left side, right side or top
                        if (ScreenBounds.Height == CurrentTaskbarRectangle.Height)
                        {
                            //may be: leftside, right side
                            return ScreenBounds.Left == CurrentTaskbarRectangle.Left ? TaskbarLocation.Left : TaskbarLocation.Right;
                        }
                        else
                        {
                            //is top
                            return TaskbarLocation.Top;
                        }
                    }
                    else
                    {
                        // is bottom
                        return TaskbarLocation.Bottom;
                    }
                }
                else
                {
                    //Taskbar is located on another monitor
                    return TaskbarLocation.None;
                }
            }
        }
    }
}
