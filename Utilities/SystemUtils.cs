/*  
Copyright (C) 2013 Alan Pipitone
    
Al'exa is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Al'exa is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Al'exa.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Management;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Xml;

namespace Alexa.Utilities
{
    class SystemUtils
    {
        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        /// <summary>
        /// Provides functions to interact with window
        /// </summary>
        public class User32
        {
            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags);

            [DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

            [DllImport("user32.dll")]
            public static extern int ShowWindow(IntPtr hWnd, WindowAppearanceStyle nCmdShow);

            [DllImport("user32.dll")]
            private static extern IntPtr GetForegroundWindow();

            [DllImport("User32.dll")]
            public static extern Int32 SetForegroundWindow(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr SetActiveWindow(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern int FindWindow(string lpClassName, string lpWindowName);

            [DllImport("user32.dll", EntryPoint = "GetWindowText",
            ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

            [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
            ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsWindowVisible(IntPtr hWnd);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("user32.dll", SetLastError = true)]
            static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

            [StructLayout(LayoutKind.Sequential)]
            /// <summary>
            /// Struct for rectangle
            /// </summary>
            private struct RECT
            {
                public int Left;        // x position of upper-left corner
                public int Top;         // y position of upper-left corner
                public int Right;       // x position of lower-right corner
                public int Bottom;      // y position of lower-right corner
            }

            /// <summary>
            /// Window Properties
            /// </summary>
            public struct WindowProperties
            {
                public int X;         // x position of upper-left corner
                public int Y;         // y position of upper-left corner
                public int Height;    // Height
                public int Width;     // Width
            }

            /// <summary>
            /// Window Struct
            /// </summary>
            private struct WindowStruct
            {
                public IntPtr Ptr;
                public string Title;
            }

            public const int WM_SYSCOMMAND = 0x0112;
            public const int SC_CLOSE = 0xF060;

            /// <summary>
            /// Window show style
            /// </summary>
            public enum WindowAppearanceStyle : uint
            {
                Hide = 0,
                ShowNormal = 1,
                ShowMinimized = 2,
                ShowMaximized = 3,
                ShowNormalNoActivate = 4,
                Show = 5,
                Minimize = 6,
                ShowMinNoActivate = 7,
                ShowNoActivate = 8,
                Restore = 9,
                ShowDefault = 10,
                ForceMinimized = 11
            }

            /// <summary>
            /// Get the handler of active window
            /// </summary>
            /// <returns>The handler of active window</returns>
            public static IntPtr GetActiveWindow()
            {
                try
                {
                    return GetForegroundWindow();
                }
                catch
                {
                    return IntPtr.Zero;
                }
            }

            /// <summary>
            /// Checks if a window is present
            /// </summary>
            /// <param name="regularExpression">The regular expression that is used to find the window</param>
            /// <param name="windowHandle">returns the handle of the window</param>
            /// <returns>True if window is present</returns>
            public static bool GetWindow(string regularExpression, out IntPtr windowHandle)
            {
                windowHandle = IntPtr.Zero;

                List<WindowStruct> windowsCollection = new List<WindowStruct>();

                EnumDelegate enumDelegate = delegate(IntPtr hWnd, int lParam)
                {
                    StringBuilder strBuilderTitle = new StringBuilder(255);
                    int nLength = GetWindowText(hWnd, strBuilderTitle, strBuilderTitle.Capacity + 1);
                    string strTitle = strBuilderTitle.ToString();

                    if (IsWindowVisible(hWnd) == true && string.IsNullOrEmpty(strTitle) == false)
                    {
                        WindowStruct windowStruct = new WindowStruct();
                        windowStruct.Title = strTitle;
                        windowStruct.Ptr = hWnd;
                        windowsCollection.Add(windowStruct);
                    }
                    return true;
                };

                if (EnumDesktopWindows(IntPtr.Zero, enumDelegate, IntPtr.Zero))
                {
                    foreach (WindowStruct window in windowsCollection)
                    {
                        if (Regex.IsMatch(window.Title, regularExpression))
                        {
                            windowHandle = window.Ptr;
                            return true;
                        }
                    }
                }

                return false;
            }

            /// <summary>
            /// Returns all windows that their title matches the regular expression
            /// </summary>
            /// <param name="regularExpression">The regular expression that is used to find the windows</param>
            /// <returns>All handle of the  windows</returns>
            public static List<IntPtr> GetWindowsCollection(string regularExpression)
            {
                List<IntPtr> windowsHandleCollection = new List<IntPtr>();

                EnumDelegate enumDelegate = delegate(IntPtr hWnd, int lParam)
                {
                    StringBuilder strBuilderTitle = new StringBuilder(255);
                    int nLength = GetWindowText(hWnd, strBuilderTitle, strBuilderTitle.Capacity + 1);
                    string strTitle = strBuilderTitle.ToString();

                    if (IsWindowVisible(hWnd) == true && string.IsNullOrEmpty(strTitle) == false)
                    {
                        if (Regex.IsMatch(strTitle, regularExpression))
                        {
                            windowsHandleCollection.Add(hWnd);
                        }
                    }

                    return true;
                };

                EnumDesktopWindows(IntPtr.Zero, enumDelegate, IntPtr.Zero);

                return windowsHandleCollection;
            }

            /// <summary>
            /// Brings to the foreground the window
            /// </summary>
            /// <param name="hWnd">Handle of window</param>
            /// <param name="maximize">Set it true if you want also to maximize the window</param>
            public static void ShowWindowOnTop(IntPtr hWnd, bool maximize)
            {

                //if user has set the option to maximize the window
                if (maximize == true)
                {
                    //then show the window and maximize it
                    ShowWindow(hWnd, WindowAppearanceStyle.ShowMaximized);
                    //set window to the foreground
                    SetForegroundWindow(hWnd);
                }
                else
                {
                    //otherwise show window only
                    ShowWindow(hWnd, WindowAppearanceStyle.Restore);
                    //set window to the foreground
                    SetForegroundWindow(hWnd);
                }
            }

            public static void MoveWindow(IntPtr hWnd, int x, int y)
            {
                WindowProperties winProp = new WindowProperties();
                winProp = GetWindowProperties(hWnd);
                SetWindowPos(hWnd, new IntPtr(0), x, y, winProp.Width, winProp.Height, 0x0040);
            }

            /// <summary>
            /// Close window
            /// </summary>
            /// <param name="regularExpression">The regular expression that is used to find the window</param>
            public static void CloseWindow(string regularExpression)
            {
                EnumDelegate enumDelegate = delegate(IntPtr hWnd, int lParam)
                {
                    try
                    {
                        StringBuilder strBuilderTitle = new StringBuilder(255);
                        int nLength = GetWindowText(hWnd, strBuilderTitle, strBuilderTitle.Capacity + 1);
                        string strTitle = strBuilderTitle.ToString();

                        if (IsWindowVisible(hWnd) == true && string.IsNullOrEmpty(strTitle) == false)
                        {
                            // close the window using API        
                            if (Regex.IsMatch(strTitle, regularExpression)) 
                                SendMessage(hWnd, WM_SYSCOMMAND, SC_CLOSE, 0);
                        }
                    }
                    catch { }

                    return true;

                };

                EnumDesktopWindows(IntPtr.Zero, enumDelegate, IntPtr.Zero);
            }

            /// <summary>
            /// Close window
            /// </summary>
            /// <param name="regularExpression">The regular expression that is used to find the window</param>
            public static void CloseWindowNew(string regularExpression)
            {
                EnumDelegate enumDelegate = delegate(IntPtr hWnd, int lParam)
                {
                    try
                    {
                        StringBuilder strBuilderTitle = new StringBuilder(255);
                        int nLength = GetWindowText(hWnd, strBuilderTitle, strBuilderTitle.Capacity + 1);
                        string strTitle = strBuilderTitle.ToString();

                        if (IsWindowVisible(hWnd) == true && string.IsNullOrEmpty(strTitle) == false)
                        {
                            // close the window using API        
                            if (Regex.IsMatch(strTitle, regularExpression))
                            {
                                SendMessage(hWnd, WM_SYSCOMMAND, SC_CLOSE, 0);
                            }
                        }
                    }
                    catch { }

                    return true;

                };

                EnumDesktopWindows(IntPtr.Zero, enumDelegate, IntPtr.Zero);
            }

            /// <summary>
            /// Gets the handle of the window.
            /// </summary>
            /// <param name="regularExpression">The regular expression that is used to find the window</param>
            public static IntPtr GetWindowHandle(string regularExpression)
            {
                List<IntPtr> collection = new List<IntPtr>();

                EnumDelegate enumDelegate = delegate(IntPtr hWnd, int lParam)
                {
                    StringBuilder strBuilderTitle = new StringBuilder(255);
                    int nLength = GetWindowText(hWnd, strBuilderTitle, strBuilderTitle.Capacity + 1);
                    string strTitle = strBuilderTitle.ToString();

                    if (IsWindowVisible(hWnd) == true && string.IsNullOrEmpty(strTitle) == false)
                    {
                        if (Regex.IsMatch(strTitle, regularExpression)) collection.Add(hWnd);
                    }
                    return true;
                };

                if (EnumDesktopWindows(IntPtr.Zero, enumDelegate, IntPtr.Zero))
                {
                    //return the first handle
                    foreach (IntPtr handle in collection)
                    {
                        return handle;
                    }
                }

                return IntPtr.Zero;
            }

            /// <summary>
            /// Hide window
            /// </summary>
            /// <param name="windowHandle">The handle of the window</param>
            public static void HideWindow(IntPtr windowHandle)
            {
                ShowWindow(windowHandle, WindowAppearanceStyle.Hide);
            }

            /// <summary>
            /// Gets the properties of the window. It returns the window coordinates, height and width.
            /// </summary>
            /// <param name="windowHandle">The handle of the window</param>
            public static WindowProperties GetWindowProperties(IntPtr windowHandle)
            {
                RECT rectangle;
                WindowProperties windowProperties = new WindowProperties();

                if (GetWindowRect(windowHandle, out rectangle))
                {
                    windowProperties.X = rectangle.Left;
                    windowProperties.Y = rectangle.Top;
                    windowProperties.Width = rectangle.Right - rectangle.Left + 1;
                    windowProperties.Height = rectangle.Bottom - rectangle.Top + 1;
                }

                return windowProperties;
            }


        }


        /// <summary>
        /// Provides functions to interact with system processes
        /// </summary>
        public class ProcessUtils
        {
            //contains pid of all processes to kill after the execution of Al'exa
            public static List<UInt32> processesToKill = new List<UInt32>();

            public static bool anotherAlexaInstance = false;

            public static bool CheckAlexaInstances()
            {
                int alexaInstancesCnt = 0;
                Process[] processlist = Process.GetProcesses();
                
                //string fileName = System.Reflection.Assembly.GetEntryAssembly().Location;

                //get the program name
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
                string programName = System.IO.Path.GetFileNameWithoutExtension(a.Location);
 
                foreach(Process process in processlist)
                {
                    if (process.ProcessName.ToLower().IndexOf(programName.ToLower()) != -1)
                    {
                        alexaInstancesCnt++;
                    }
                }

                if (alexaInstancesCnt >= 2)
                {
                    anotherAlexaInstance = true;
                    return true;
 
                }
                else
                {
                    return false;
                }

            }

            /// <summary>
            /// Get all processes that belong to specific user
            /// </summary>
            /// <param name="userDomain">The domain of the user</param>
            /// <param name="userName">The user name</param>
            /// <returns>The list that contains the pid of the process</returns>
            public static List<UInt32> GetUserProcesses(string userDomain, string userName)
            {
                //this contains the pid of the processes
                List<UInt32> pIds = new List<UInt32>();

                //set the WMI query to get all processes
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process"))
                {
                    //loop through all results
                    foreach (ManagementObject mngObject in searcher.Get())
                    {
                        try
                        {

                            //this object array will contain username and user domain
                            Object[] argObj = new Object[2];

                            //Get the user name and user domain of current process
                            mngObject.InvokeMethod("GetOwner", argObj);

                            string processUserName = (string)argObj[0];
                            string processUserDomain = (string)argObj[1];

                            //if the process user name and user domain are equal to the arguments
                            if (processUserName == userName && processUserDomain == userDomain)
                            {
                                //then add the pid of current process to the List of pid
                                UInt32 pid = (UInt32)mngObject["ProcessId"];
                                pIds.Add(pid);
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                //return the list
                return pIds;
            }

            /// <summary>
            /// Get all processes that belong to specific user and their name matches the regular expression
            /// </summary>
            /// <param name="userDomain">The domain of the user</param>
            /// <param name="userName">The user name</param>
            /// <param name="regularExpression">The regular expression</param>
            /// <returns>The list that contains the pid of the process</returns>
            public static List<UInt32> GetUserProcessesByRegEx(string userDomain, string userName, string regularExpression)
            {
                //this contains the pid of the processes
                List<UInt32> pIds = new List<UInt32>();

                //set the WMI query to get all processes
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process"))
                {
                    //loop through all results
                    foreach (ManagementObject mngObject in searcher.Get())
                    {
                        try
                        {

                            //this object array will contain username and user domain
                            Object[] argObj = new Object[2];

                            //Get the user name and user domain of current process
                            mngObject.InvokeMethod("GetOwner", argObj);

                            string processUserName = (string)argObj[0];
                            string processUserDomain = (string)argObj[1];
                            string processName = (string)mngObject["Name"];

                            //if the process user name and user domain are equal to the arguments
                            if (processUserName == userName && processUserDomain == userDomain && Regex.IsMatch(processName, regularExpression))
                            {
                                //then add the pid of current process to the List of pid
                                UInt32 pid = (UInt32)mngObject["ProcessId"];
                                pIds.Add(pid);
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                //return the list
                return pIds;
            }

            /// <summary>
            /// Get all processes that belong to a specific user and their name is like process name passed as argument
            /// </summary>
            /// <param name="userDomain">The domain of the user</param>
            /// <param name="userName">The user name</param>
            /// <param name="procName">The name (or a part of the name) of the processes to seach</param>
            /// <returns>The list that contains the pid of the process</returns>
            public static List<UInt32> GetUserProcessesByName(string userDomain, string userName, string procName)
            {
                //this contains the pid of the processes
                List<UInt32> pIds = new List<UInt32>();

                //set the WMI query to get all processes
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE Name LIKE '%" + procName + "%'"))
                {
                    //loop through all results
                    foreach (ManagementObject mngObject in searcher.Get())
                    {
                        try
                        {
                            //this object array will contain username and user domain
                            Object[] argObj = new Object[2];

                            //Get the user name and user domain of current process
                            mngObject.InvokeMethod("GetOwner", argObj);

                            string processUserName = (string)argObj[0];
                            string processUserDomain = (string)argObj[1];

                            //if the process user name and user domain are equal to the arguments
                            if (processUserName == userName && processUserDomain == userDomain)
                            {
                                //then add the pid of current process to the List of pid
                                UInt32 pid = (UInt32)mngObject["ProcessId"];
                                pIds.Add(pid);
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                //return the list
                return pIds;
            }

            /// <summary>
            /// Kill a process by Pid
            /// </summary>
            /// <param name="pid">The process pid</param>
            public static void KillProcess(UInt32 pid)
            {
                //this contains the pid of the processes
                List<UInt32> pIds = new List<UInt32>();

                //set the WMI query to get the process
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ProcessId=" + pid))
                {
                    //loop through all results
                    foreach (ManagementObject mngObject in searcher.Get())
                    {
                        try
                        {
                            mngObject.InvokeMethod("Terminate", null);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Set folder compression
        /// </summary>
        /// <param name="path">The path of folder to compress</param>
        /// <param name="compress">set true to compress the folder</param>
        public static void SetDirectoryCompression(DirectoryInfo path, Boolean compress)
        {
            if (Directory.Exists(path.FullName)) //check if the path exist
            {
                //set the ManagementObject
                using (ManagementObject dir = new ManagementObject("Win32_Directory.Name=\"" + path.FullName.Replace(@"\", @"\\") + "\""))
                {
                    if (compress == true)
                        //compress the folder
                        dir.InvokeMethod("Compress", null, null);
                    else
                        //uncompress the folder
                        dir.InvokeMethod("Uncompress", null, null);
                }
            }
        }

        /// <summary>
        /// Get the Height of screen resolution
        /// </summary>
        public static int ScreenHeight
        {
            get
            {
                var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                return screen.Height;
            }
        }

        /// <summary>
        /// Get the Width of screen resolution
        /// </summary>
        public static int ScreenWidth
        {
            get
            {
                var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                return screen.Width;
            }
        }

        /// <summary>
        /// Runs external script
        /// </summary>
        public static void RunExternalScript()
        {
            try
            {
                if (ConfigUtils.GetProgramsToRun != null)
                {
                    foreach (XmlNode node in ConfigUtils.GetProgramsToRun)
                    {

                        //contains the full path (plus name) of the program that we have to run
                        string executable = "";

                        //contains the arguments of the program
                        string arguments = "\"" + ConfigUtils.OutputFolder + "\"";

                        //get the executable
                        executable = node.SelectSingleNode("executable").InnerText;

                        //get the arguments
                        foreach (XmlNode argument in node.SelectNodes("argument"))
                        {
                            arguments = arguments + " " + argument.InnerText;
                        }

                        //create the process object
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;

                        //set the filename and the arguments
                        p.StartInfo.FileName = executable;
                        if (arguments != "") p.StartInfo.Arguments = arguments;

                        //start the process
                        p.Start();
                    }
                }
            }
            catch(Exception ex)
            {
                //write the exception
                LogUtils.Write(ex);
            }
        }
    }
}
