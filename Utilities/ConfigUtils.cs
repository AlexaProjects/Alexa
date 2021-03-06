﻿/*  
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
using System.Xml;
using System.IO;

namespace Alexa.Utilities
{
    /// <summary>
    /// Provides functions to read from the config file (aka test case file).
    /// </summary>
    
    //in this class I don't use try catch statement, so if the config file contains some errors the application will be crash
    //and you can see the error from the event viewer of windows.
    //This because I don't know yet the path of log file and I cannot write any error message.
    static public class ConfigUtils
    {
        //private var
        static private XmlDocument _configFile = new XmlDocument();
        static private string _runDate = DateTime.Now.ToString("dd_MM_yyyy_HH.mm.ss");


        /// <summary>
        /// Initialize the config class
        /// </summary>
        /// <param name="filename">The full path of the config file</param>
        static public void Init(string filename)
        {
            _configFile.Load(filename);
        }

        /// <summary>
        /// Get the home folder
        /// </summary>
        /// <returns>The home folder</returns>
        static public string HomeFolder
        {
            get { return Global.SelectSingleNode("home").InnerText; }
        }

        /// <summary>
        /// Get if Log is enabled 
        /// </summary>
        /// <returns>true or false</returns>
        static public bool LogIsEnabled
        {
            get 
            {
                if (Global.SelectSingleNode("log").Attributes["enable"].Value.ToLower() == "yes") return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Get the Log ErrorLevel
        /// </summary>
        /// <returns>Error Level</returns>
        static public LogUtils.ErrorLevel ErrorLevel
        {
            get 
            {
                switch (Global.SelectSingleNode("log/level").InnerText.ToLower())
                {
                    case "error":
                        return LogUtils.ErrorLevel.Error;
                    case "warning":
                        return LogUtils.ErrorLevel.Warning;
                    case "debug":
                        return LogUtils.ErrorLevel.Debug;
                    default:
                        return LogUtils.ErrorLevel.Error;
                }
            }
        }

        /// <summary>
        /// Get the Log folder
        /// </summary>
        /// <returns>The log folder</returns>
        static public string LogFolder
        {
            get 
            {
                string logFolder = Global.SelectSingleNode("log/folder").InnerText;
                try
                {
                    if (Global.SelectSingleNode("log").Attributes["split"].Value.ToLower() == "yes")
                        logFolder = Path.Combine(logFolder, "run_" + _runDate);
                }
                catch { }
                return Path.GetFullPath(Path.Combine(HomeFolder,logFolder));
            }
        }

        /// <summary>
        /// Get the Log folder user name
        /// </summary>
        /// <returns>The log user name</returns>
        static public string LogFolderUserName
        {
            get
            {
                try
                {
                    return Global.SelectSingleNode("log/folder").Attributes["username"].Value;
                }
                catch 
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Get the Log folder user password
        /// </summary>
        /// <returns>The log user password</returns>
        static public string LogFolderPassword
        {
            get
            {
                try
                {
                    string nodeText = Global.SelectSingleNode("log/folder").Attributes["password"].Value;
                    string plainText = "";

                    //try to decrypt the text
                    plainText = CryptoUtils.DecryptString(nodeText);

                    //if plainText is null then the text isn't crypted
                    if (plainText == null) plainText = nodeText;
                    
                    return plainText;
                }
                catch
                {
                    return "";
                }
            }
        }



        /// <summary>
        /// Get the steps of the test case
        /// </summary>
        /// <returns>The steps</returns>
        static public XmlNodeList AlexaSteps
        {
            get { return _configFile.SelectNodes("/config/steps/step"); }
        }

        /// <summary>
        /// Get the timeout actions of the test case
        /// </summary>
        /// <param name="actionName">the name of the action that you have to execute</param>
        /// <returns>The action node</returns>
        static public XmlNode GetAction(string actionName)
        {
            try
            {
                return _configFile.SelectSingleNode("/config/actions/action[@name='" + actionName + "']");
            }
            catch
            {
                return null;
            }

        }

        /// <summary>
        /// Get the global node
        /// </summary>
        /// <returns>The global node</returns>
        static public XmlNode Global
        {
            get 
            {
                try { return _configFile.SelectSingleNode("/config/global"); }
                catch { return null; }
            }
        }

        /// <summary>
        /// Get the folder of the OCR language data
        /// </summary>
        /// <returns>Language data folder</returns>
        static public string OcrLanguageData
        {
            get 
            {
                string ocrLangFolder = Global.SelectSingleNode("ocr/folder").InnerText;
                return Path.GetFullPath(Path.Combine(HomeFolder, ocrLangFolder));
            }
        }

        /// <summary>
        /// Get the selected OCR language
        /// </summary>
        /// <returns>The selected OCR language</returns>
        static public string OcrSelectedLanguage
        {
            get { return Global.SelectSingleNode("ocr").Attributes["language"].Value; }
        }

        /// <summary>
        /// Check if output on file is enabled
        /// </summary>
        /// <returns>true if output on file is enabled</returns>
        static public bool OutputOnFile
        {
            get
            {
                try
                {
                    if (Global.SelectSingleNode("end/output").Attributes["file"].Value.ToLower() == "yes")
                        return true;
                    else
                        return false;
                }
                catch
                {
                    return false;
                }

            }
        }


        /// <summary>
        /// Get the Output folder
        /// </summary>
        /// <returns>the output folder</returns>
        static public string OutputFolder
        {
            get
            {   
                string outputFolder = "";
                try
                {
                    outputFolder = Global.SelectSingleNode("end/output").Attributes["folder"].Value;

                    if (Global.SelectSingleNode("end/output").Attributes["split"].Value.ToLower() == "yes")
                        outputFolder = Path.Combine(outputFolder, "run_" + _runDate);
                }
                catch { }
                return Path.GetFullPath(Path.Combine(HomeFolder, outputFolder));
            }
        }

        /// <summary>
        /// Get the output timeout handler mode
        /// </summary>
        /// <returns>the handler mode</returns>
        static public int OutputTimeoutHandler
        {
            get
            {
                try
                {
                    return Int32.Parse(Global.SelectSingleNode("end/output").Attributes["timeout.printmode"].Value);
                }
                catch
                {
                    return 0;
                }

            }
        }


        /// <summary>
        /// Get the regular expression containing the title of window(s) to close
        /// </summary>
        /// <returns>the regular expression</returns>
        static public string WindowTitleToClose
        {
            get
            {
                try
                {
                    return Global.SelectSingleNode("end").Attributes["window.close"].Value;
                }
                catch
                {
                    return "";
                }
            }
        }


        /// <summary>
        /// Get the regular expression containing the name of processes to kill
        /// </summary>
        /// <returns>the regular expression</returns>
        static public string ProcessesToKill
        {
            get
            {
                try
                {
                    return Global.SelectSingleNode("end").Attributes["processes.kill"].Value;
                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Get the the program to run after Al'exa
        /// </summary>
        /// <returns>the run nodes</returns>
        static public XmlNodeList GetProgramsToRun
        {
            get
            {
                try
                {
                    return Global.SelectSingleNode("end").SelectNodes("run");
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the process to kill before the execution of the steps
        /// </summary>
        /// <returns>the proccess node list</returns>
        static public XmlNodeList GetProcessesToKillAtStartTime
        {
            get
            {
                try
                {
                    return Global.SelectSingleNode("wormup").SelectNodes("process.kill");
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the windows to close before the execution of the steps
        /// </summary>
        /// <returns>the proccess node list</returns>
        static public XmlNodeList GetWindowsToCloseAtStartTime
        {
            get
            {
                try
                {
                    return Global.SelectSingleNode("wormup").SelectNodes("window.close");
                }
                catch
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// Get the smtp server
        /// </summary>
        /// <returns>the smtp server</returns>
        static public String GetSmtpServer
        {
            get
            {
                try
                {
                    return Global.SelectSingleNode("smtp/host").InnerText ;
                }
                catch
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// Get the smtp port
        /// </summary>
        /// <returns>the smtp port</returns>
        static public int GetSmtpPort
        {
            get
            {
                try
                {
                    return Int32.Parse(Global.SelectSingleNode("smtp/port").InnerText);
                }
                catch
                {
                    return 25;
                }
            }
        }


        /// <summary>
        /// Get the smtp "from" address
        /// </summary>
        /// <returns>the smtp server</returns>
        static public string GetSmtpFromAddress
        {
            get
            {
                try
                {
                    return Global.SelectSingleNode("smtp/from").InnerText;
                }
                catch
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// Get the smtp "to" address(es)
        /// </summary>
        /// <returns>the smtp server</returns>
        static public XmlNodeList GetSmtpToAddresses
        {
            get
            {
                try
                {
                    return Global.SelectNodes("smtp/to");
                }
                catch
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// Get the username for smtp server
        /// </summary>
        /// <returns>the username for smtp server</returns>
        static public string GetSmtpUser
        {
            get
            {
                try
                {
                    return Global.SelectSingleNode("smtp/username").InnerText;
                }
                catch
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// Get the password for smtp server
        /// </summary>
        /// <returns>the password for smtp server</returns>
        static public string GetSmtpPassword
        {
            get
            {
                try
                {
                    return Global.SelectSingleNode("smtp/password").InnerText;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the Image Quality for the attachment of smtp
        /// </summary>
        /// <returns>the Image Quality for the attachment of smtp</returns>
        static public string GetSmtpImageQuality
        {
            get
            {
                try
                {
                    return Global.SelectSingleNode("smtp/attachment.quality").InnerText;
                }
                catch
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// Get the Image resize factor for the attachment of smtp
        /// </summary>
        /// <returns>the Image resize factor for the attachment of smtp</returns>
        static public float GetSmtpImageSizeFactor
        {
            get
            {
                try
                {
                    return float.Parse(Global.SelectSingleNode("smtp/attachment.resize").InnerText.Replace("%", "")) / 100;
                }
                catch
                {
                    return -1;
                }
            }
        }


        /// <summary>
        /// Get Smtp Timeout
        /// </summary>
        /// <returns>Smtp timeout</returns>
        static public int GetSmtpTimeout
        {
            get
            {
                try
                {
                    return Int32.Parse(Global.SelectSingleNode("smtp/timeout").InnerText);
                }
                catch
                {
                    return 10000;
                }
            }
        }


        /// <summary>
        /// Get the Image Quality for the attachment of smtp
        /// </summary>
        /// <returns>the Image Quality for the attachment of smtp</returns>
        static public string GetSmtpExe
        {
            get
            {
                try
                {
                    return Global.SelectSingleNode("smtp/smtp.tool").InnerText;
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
