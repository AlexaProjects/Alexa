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
using System.Reflection;
using System.Diagnostics;

namespace Alexa.Utilities
{
    /// <summary>
    /// Provides utilities to write log
    /// </summary>
    public class LogUtils
    {
        //private variables
        static private string _fileName = "";

        public enum ErrorLevel
        {
            Error= 2,
            Warning = 1,
            Debug= 0,
        }


        /// <summary>
        /// Initialize Log
        /// </summary>
        public static void Init()
        {
            //get the log filename
            _fileName = ConfigUtils.LogFolder + @"\Al'exa.log";
        }


        /// <summary>
        /// Write a string into the log file
        /// </summary>
        /// <param name="ex">The method Exception</param>
        public static void Write(Exception ex)
        {
            // Get stack trace for the exception with source file information 
            StackTrace stackTrace = new StackTrace(ex, true);
            // Get the top stack frame 
            StackFrame frame = stackTrace.GetFrame(0);
            // Get the line number from the stack frame 
            int line = frame.GetFileLineNumber();
            //get the method name
            MethodBase methodBase = frame.GetMethod();
            string methodName = methodBase.Name;
            //get the class base
            string className;
            try
            {
                className = methodBase.DeclaringType.Name;
            }
            catch
            {
                className = "unknown";
            }

            //write the error
            //Write(className,methodName,line,ErrorLevel.Error,ex.Message);
            try
            {
                //get the log folder
                DirectoryInfo dir = new DirectoryInfo(ConfigUtils.LogFolder);

                //if log folder doesn't exist then create it
                if (!dir.Exists) dir.Create();

                if (ConfigUtils.LogIsEnabled == true)
                {
                    using (StreamWriter sw = new StreamWriter(_fileName, true))
                    {
                        //write the string into the log file
                        sw.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + " " + "ERROR MESSAGE: " + ex.ToString());
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Write a string into the log file
        /// </summary>
        /// <param name="stackFrame">The StackFrame of  method</param>
        /// <param name="errorLevel">The error level</param>
        /// <param name="message">The message</param>
        public static void Write(StackFrame stackFrame,ErrorLevel errorLevel, string message)
        {
            // Get the line number from the stack frame 
            int line = stackFrame.GetFileLineNumber();
            //get the method name
            MethodBase methodBase = stackFrame.GetMethod();
            string methodName = methodBase.Name;
            //get the class base
            string className = methodBase.DeclaringType.Name;

            //write the error
            Write(className, methodName, line, errorLevel, message);
        }

        /// <summary>
        /// Write a string into the log file
        /// </summary>
        /// <param name="className">The class that returned the error</param>
        /// <param name="methodName">The method that returned the error</param>
        /// <param name="line">Theline of error</param>
        /// <param name="errorLevel">The error level</param>
        /// <param name="message">The error message</param>
        public static void Write(string className, string methodName, int line, ErrorLevel errorLevel, string message)
        {
            try
            {
                //get the log folder
                DirectoryInfo dir = new DirectoryInfo(ConfigUtils.LogFolder);

                //if log folder doesn't exist then create it
                if (!dir.Exists) dir.Create();

                if (ConfigUtils.LogIsEnabled == true)
                {
                    using (StreamWriter sw = new StreamWriter(_fileName, true))
                    {
                        //write the string into the log file
                        sw.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + " " + errorLevel.ToString().ToUpper() + " MESSAGE:  "
                            + message.Replace("\r", "").Replace("\n", "") + "  CLASS: " + className + "  METHOD: " + methodName + "  LINE: " + line);
                    }
                }
            }
            catch { }

        }

    }
}
