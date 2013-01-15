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
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using Alexa.Utilities;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Xml;
using System.Collections.Generic;

namespace Alexa
{
    class Program
    {
        //stores the regular expression containing the name of processes to kill
        static private string _processToKillRegEx = "";
        
        //stores the elapsed time of Al'exa execution time
        static private Stopwatch _globalTime = new Stopwatch();

        //contains all pid of the processes started before Al'exa
        static private List<UInt32> processesBeforeAlexa;
        //contains all pid of the processes started after the execution of Al'exa
        static private List<UInt32> processesAfterAlexa;

        //contain user name and user domain of the account used to run Al'exa
        private static string userName;
        private static string userDomain;

        private static long globalTimeout = 900000; //default global timeout is 15 minutes

        //in this class I don't use try catch statement, so if the user doesn't add the argument the application will be crash
        //and you can see the error from the event viewer of windows, this because I don't know yet the path of log file and I
        //cannot write any error message.
        static void Main(string[] args)
        {

            //init configuration utilities class
            ConfigUtils.Init(args[0]);

            //Hide current window calling windows API.
            //NB: To do this we can also set "Windows Application" on the output type property of Visual Studio.
            //But in that way we cannot print any message on the standard output
            IntPtr windowHandle = Process.GetCurrentProcess().MainWindowHandle;
            SystemUtils.User32.HideWindow(windowHandle);

            //get user name and user domain of the account used to run Al'exa
            userName = Environment.UserName;
            userDomain = Environment.UserDomainName;

            //get the regular expression containing the name of processes to kill
            _processToKillRegEx = ConfigUtils.ProcessesToKill;

            //get all processes (of the above user) that are running now
            if (_processToKillRegEx != "") processesBeforeAlexa = SystemUtils.ProcessUtils.GetUserProcesses(userDomain, userName);

            try
            {
                //get the global timeout attribute, it isn't mandatory.
                globalTimeout = long.Parse(ConfigUtils.Global.SelectSingleNode("performance").Attributes["timeout.value"].Value);
            }
            catch { }

            //new thread to check global timeout
            Thread _timeoutThread = new Thread(new ThreadStart(CheckTimeout));
            _timeoutThread.IsBackground = true;

            //save the start time, this will be saved in the xml output file
            //into the global node
            OutputUtils.Global.startTime = DateTime.Now;
            //start the stopwatch that measure the elapsed time of all Al'exa steps and computer vision time
            _globalTime.Start();

            //start the thread
            _timeoutThread.Start();

            //init log utilities class
            LogUtils.Init();

            //init CryptoUtils
            CryptoUtils.Init();

            //init Core utilities class
            CoreUtils.Init();

            //execute all the steps
            CoreUtils.RunSteps(ConfigUtils.AlexaSteps);

            //stop the stopwatch
            _globalTime.Stop();

            //save the output file
            Finish(false);
        }

        /// <summary>
        /// Checks the global timeout
        /// </summary>
        private static void CheckTimeout()
        {

            while (true)
            {
                //checks if a timeout has occurred
                if (_globalTime.ElapsedMilliseconds > globalTimeout)
                {
                    //call the method that save the output and exit from the program
                    Program.Finish(false);
                    break;
                }

                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Execute the method that save the output
        /// </summary>
        /// <param name="exception">set it true if you call this method on an unkown error</param>
        public static void Finish(Boolean exception)
        {
            //stop the stopwatch
            _globalTime.Stop();

            //save all info that will be used by OutputUtils to write global info
            OutputUtils.Global.xmlNode = ConfigUtils.Global;
            OutputUtils.Global.endTime = DateTime.Now;
            OutputUtils.Global.duration = _globalTime.ElapsedMilliseconds;


            //close window(s)
            Thread.Sleep(2000);
            string windowToCloseRegEx = ConfigUtils.WindowTitleToClose;
            if (windowToCloseRegEx != "")
                SystemUtils.User32.CloseWindow(windowToCloseRegEx);

            if (_processToKillRegEx != "")
            {
                processesAfterAlexa = SystemUtils.ProcessUtils.GetUserProcessesByRegEx(userDomain, userName, _processToKillRegEx);

                foreach (UInt32 pidAfterAlexa in processesAfterAlexa)
                {
                    bool pidFound = false;

                    foreach (UInt32 pidBeforeAlexa in processesBeforeAlexa)
                    {
                        if (pidAfterAlexa == pidBeforeAlexa) pidFound = true;
                    }

                    if (pidFound == false) SystemUtils.ProcessUtils.processesToKill.Add(pidAfterAlexa);
                }

                //loop through all processes to kill
                foreach (UInt32 pidToKill in SystemUtils.ProcessUtils.processesToKill)
                {
                    //kill the process
                    SystemUtils.ProcessUtils.KillProcess(pidToKill);
                }
            }

            //save the output file and exit
            OutputUtils.Finish(exception);

            //if exit is true then exit with exitcode 3
            //if (exit) Environment.Exit(3);
        }
    }
    
}
