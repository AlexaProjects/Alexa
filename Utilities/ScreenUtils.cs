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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace Alexa.Utilities
{
    /// <summary>
    /// Provides functions to capture single windows or the entire desktop.
    /// </summary>
    static public class ScreenUtils
    {

        /// <summary>
        /// User32 API functions
        /// </summary>
        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);
            [DllImport("user32.dll")]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        }


        /// <summary>
        /// Gdi32 API functions
        /// </summary>
        private class Gdi32
        {

            public const int SRCC = 0x00CC0020;

            [DllImport("Gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);
            [DllImport("Gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("Gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
            [DllImport("Gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,int nHeight);
            [DllImport("Gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("Gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);

        }


        /// <summary>
        /// Creates a Bitmap containing the entire desktop image
        /// </summary>
        /// <returns>Bitmap containing the entire desktop image</returns>
        static public Bitmap CaptureDesktop()
        {
            return CaptureWindowByHandle(User32.GetDesktopWindow());
        }


        /// <summary>
        /// Creates a Bitmap containing the screen of the window
        /// </summary>
        /// <param name="handle">The handle of the window.</param>
        /// <returns>Bitmap containing the screen of the window</returns>
        static public Bitmap CaptureWindow(IntPtr handle)
        {
            //call CaptureWindowByHandle and return the screenshot
            return CaptureWindowByHandle(handle);
        }


        /// <summary>
        /// Creates a Bitmap containing the screen of the window
        /// </summary>
        /// <param name="handle">The handle to the window.</param>
        /// <returns>Bitmap containing the screen of the window</returns>
        static private Bitmap CaptureWindowByHandle(IntPtr handle)
        {
            IntPtr hSrc;
            IntPtr hDest;
            IntPtr hBitmap;
            IntPtr hOld;

            //create the rectangle and get image size
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int height = windowRect.bottom - windowRect.top;
            int width = windowRect.right - windowRect.left;

            //get all handle device context
            hSrc = User32.GetWindowDC(handle);
            hDest = Gdi32.CreateCompatibleDC(hSrc);
            hBitmap = Gdi32.CreateCompatibleBitmap(hSrc, width, height);
            hOld = Gdi32.SelectObject(hDest,hBitmap);

            //get the image
            Gdi32.BitBlt(hDest, 0, 0, width, height, hSrc, 0, 0, Gdi32.SRCC);
            Gdi32.SelectObject(hDest,hOld);
            Bitmap img = Image.FromHbitmap(hBitmap);

            //free the memory
            Gdi32.DeleteDC(hDest);
            User32.ReleaseDC(handle, hSrc);
            Gdi32.DeleteObject(hBitmap);

            //return the image
            return img;
        }
    }
}
