/*  
Copyright (C) 2013 Alan Pipitone
    
Alexa is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Alexa is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Alexa.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace Alexa.Utilities
{
    class OutputUtils
    {
        static public List<StepTiming> StepTimingsCollection = new List<StepTiming>();
        static public GlobalAttributes Global = new GlobalAttributes();

        public struct GlobalAttributes
        {
            public DateTime startTime;
            public DateTime endTime;
            public long duration;
            public XmlNode xmlNode;
        }

        public struct StepTiming
        {
            public int stepNumber;
            public DateTime startTime;
            public DateTime endTime;
            public long stepDuration;
            public XmlNode stepNode;
        }

        /// <summary>
        /// Print the output and exit from the program
        /// </summary>
        public static void Finish(Boolean exception)
        {
            bool enableGlobalOutput = false;

            //contains the message to send into the standard output
            string standardOutputString = "Alexa service is ok"; //default message

            //contains the nagios performance string
            string nagiosPerformance = "";

            //contains the exit code of Alexa
            int standardOutputExitCode = 0; //default exit code is 0, all ok

            //declare the output file
            XmlDocument outputFile = new XmlDocument();

            //set the xml encoding
            XmlDeclaration xDecl = outputFile.CreateXmlDeclaration("1.0", System.Text.Encoding.UTF8.WebName, null);
            outputFile.AppendChild(xDecl);

            //add root node
            XmlElement xRoot = outputFile.CreateElement("", "output", "");
            outputFile.AppendChild(xRoot);

            try
            {
                //check if we have to write the global output into the output file
                if (Global.xmlNode.SelectSingleNode("performance").Attributes["output"].Value.ToLower() == "yes")
                    enableGlobalOutput = true;
            }
            catch{}

            if (enableGlobalOutput)
            {
                //declare the global node (and its child node) that will be written to the output file
                XmlElement global = outputFile.CreateElement("global");
                XmlElement globalPerformance = outputFile.CreateElement("performance");
                globalPerformance.SetAttribute("start", DateTimeToUnixTimestamp(Global.startTime).ToString());
                globalPerformance.SetAttribute("end", DateTimeToUnixTimestamp(Global.endTime).ToString());
                globalPerformance.SetAttribute("duration", Global.duration.ToString());

                //set the exit code of the global node
                XmlElement globalExitcode = outputFile.CreateElement("exitcode");
                globalExitcode.InnerText = "0";

                try
                {
                    //set the name of global node
                    string globalName = Global.xmlNode.SelectSingleNode("name").InnerText;
                    global.SetAttribute("name", globalName);

                    //add the duration of global step to the nagios performance string
                    nagiosPerformance = nagiosPerformance + globalName + "=" + Global.duration.ToString() + "ms";
                }
                catch
                {
                    //add the duration of global step to the nagios performance string
                    nagiosPerformance = nagiosPerformance + "Global=" + Global.duration.ToString() + "ms";
                }

                try
                {
                    //set the description of global node
                    string description = Global.xmlNode.SelectSingleNode("description").InnerText;
                    global.SetAttribute("description", description);

                    //set the message for the standard output
                    standardOutputString = description + " service is ok";
                }
                catch { }

                try
                {
                    //set global warning value into the output file
                    long warningValue = long.Parse(Global.xmlNode.SelectSingleNode("performance").Attributes["warning"].Value);
                    globalPerformance.SetAttribute("warning", warningValue.ToString());

                    //add warning threshold to the nagios performance string
                    nagiosPerformance = nagiosPerformance + ";" + warningValue.ToString();

                    //check if the global step has exceeded the warning threshold
                    if (Global.duration >= warningValue)
                    {
                        //change global exit code
                        globalExitcode.InnerText = "1";

                        //set the standard output message
                        standardOutputString = "one or more steps have exceeded the warning threshold";

                        //change the standard output exit code
                        standardOutputExitCode = 1;
                    }
                }
                catch
                {
                    //if no warning threshold is set then add only a semicolon to the nagios performance string
                    nagiosPerformance = nagiosPerformance + ";";
                }

                try
                {
                    //set global critical value into the output file
                    long criticalValue = long.Parse(Global.xmlNode.SelectSingleNode("performance").Attributes["critical"].Value);
                    globalPerformance.SetAttribute("critical", criticalValue.ToString());

                    //add critical threshold to the nagios performance string
                    nagiosPerformance = nagiosPerformance + ";" + criticalValue.ToString();

                    //check if the global step has exceeded the critical threshold
                    if (Global.duration >= criticalValue)
                    {
                        //change global exit code
                        globalExitcode.InnerText = "2";

                        //set the standard output message
                        standardOutputString = "one or more steps have exceeded the critical threshold";

                        //set the standard output exit code
                        standardOutputExitCode = 2;
                    }
                }
                catch
                {
                    //if no warning threshold is set then add only a semicolon to the nagios performance string
                    nagiosPerformance = nagiosPerformance + ";";
                }

                try
                {
                    //set global timeout value into the output file
                    long timeout = long.Parse(Global.xmlNode.SelectSingleNode("performance").Attributes["timeout.value"].Value);
                    globalPerformance.SetAttribute("timeout", timeout.ToString());

                    //check if the global step has exceeded the timeout value
                    if (Global.duration >= timeout)
                    {
                        //change global exit code
                        globalExitcode.InnerText = "3";

                        //set the standard output message
                        standardOutputString = "a timeout has occurred while executing one or more steps";

                        //set the standard output exit code
                        standardOutputExitCode = 3;
                    }
                }
                catch { }

                //we don't have a min or max value for the performance data.
                //So we I to add two semicolon to avoid any kind of error on the graph
                nagiosPerformance = nagiosPerformance + ";;";

                //put together global node and global child nodes
                global.AppendChild(globalPerformance);
                global.AppendChild(globalExitcode);
                xRoot.AppendChild(global);
            }
            
            //create a node that will contain all the steps
            XmlElement steps = outputFile.CreateElement("steps");

            //loop through all steps timings collection
            foreach (StepTiming stepTiming in StepTimingsCollection)
            {
                bool enableOutput = false;

                try
                {
                    //check if we have to write the output of current step into the output file
                    if (stepTiming.stepNode.SelectSingleNode("performance").Attributes["output"].Value.ToLower() == "yes")
                        enableOutput = true;
                }
                catch{}

                if (enableOutput)
                {
                    //declare the step node (and its child node) that will be written to the output file
                    XmlElement step = outputFile.CreateElement("step");
                    XmlElement performance = outputFile.CreateElement("performance");
                    XmlElement exitcode = outputFile.CreateElement("exitcode");
                    long duration = stepTiming.stepDuration;

                    performance.SetAttribute("start", DateTimeToUnixTimestamp(stepTiming.startTime).ToString());
                    performance.SetAttribute("end", DateTimeToUnixTimestamp(stepTiming.endTime).ToString());
                    performance.SetAttribute("duration", duration.ToString());

                    //set the exit code of the step
                    exitcode.InnerText = "0";

                    //set the step number
                    step.SetAttribute("number", stepTiming.stepNumber.ToString());

                    try
                    {
                        //set the step name
                        string stepName = stepTiming.stepNode.Attributes["name"].Value;
                        step.SetAttribute("name", stepName);

                        //add the duration of current step to the nagios performance string
                        nagiosPerformance = nagiosPerformance + " " + stepName + "=" + duration.ToString() + "ms";
                    }
                    catch
                    {
                        //add the duration of current step to the nagios performance string
                        nagiosPerformance = nagiosPerformance + " Step " + stepTiming.stepNumber.ToString() + "=" + duration.ToString() + "ms";
                    }

                    try
                    {
                        //set the description for the current step
                        step.SetAttribute("description", stepTiming.stepNode.Attributes["description"].Value);
                    }
                    catch { }

                    try
                    {
                        //set warning value into the output file for the current step
                        long warningLevel = long.Parse(stepTiming.stepNode.SelectSingleNode("performance").Attributes["warning"].Value);
                        performance.SetAttribute("warning", warningLevel.ToString());

                        //add warning threshold to the nagios performance string
                        nagiosPerformance = nagiosPerformance + ";" + warningLevel.ToString();

                        //check if the current step has exceeded the warning threshold
                        if (duration >= warningLevel)
                        {
                            //change the exit code of current step
                            exitcode.InnerText = "1";

                            if (standardOutputExitCode <= 1)
                            {
                                //set the standard output message
                                standardOutputString = "one or more steps have exceeded the warning threshold";

                                //set the standard output exit code
                                standardOutputExitCode = 1;
                            }

                        }
                    }
                    catch
                    {
                        //if no warning threshold is set then add only a semicolon to the nagios performance string
                        nagiosPerformance = nagiosPerformance + ";";
                    }

                    try
                    {
                        //set global critical value into the output file fo the current step
                        long errorLevel = long.Parse(stepTiming.stepNode.SelectSingleNode("performance").Attributes["critical"].Value);
                        performance.SetAttribute("critical", errorLevel.ToString());

                        //add critical threshold to the nagios performance string
                        nagiosPerformance = nagiosPerformance + ";" + errorLevel.ToString();

                        //check if the current step has exceeded the critical threshold
                        if (duration >= errorLevel)
                        {
                            //change the exit code of current step
                            exitcode.InnerText = "2";

                            if (standardOutputExitCode <= 2)
                            {
                                //set the standard output message
                                standardOutputString = "one or more steps have exceeded the critical threshold";

                                //set the standard output exit code
                                standardOutputExitCode = 2;
                            }
                        }
                    }
                    catch
                    {
                        //if no warning threshold is set then add only a semicolon to the nagios performance string
                        nagiosPerformance = nagiosPerformance + ";";
                    }


                    try
                    {
                        //set the timeout value into the output file for the current step
                        long timeout = long.Parse(stepTiming.stepNode.SelectSingleNode("performance").Attributes["timeout.value"].Value);
                        performance.SetAttribute("timeout", timeout.ToString());

                        //check if the global step has exceeded the timeout value
                        if (duration >= timeout)
                        {
                            //change global exit code
                            exitcode.InnerText = "3";

                            //set the standard output message
                            standardOutputString = "a timeout has occurred while executing one or more steps";

                            //set the standard output exit code
                            standardOutputExitCode = 3;
                        }
                    }
                    catch { }

                    //we don't have a min or max value for the performance data.
                    //So we I to add two semicolon to avoid any kind of error on the graph
                    nagiosPerformance = nagiosPerformance + ";;";

                    //put together step node and step child nodes
                    step.AppendChild(performance);
                    step.AppendChild(exitcode);
                    steps.AppendChild(step);
                }
                
            }

            if (exception == true)
            {
                standardOutputString = "An internal exception has occurred. Some steps may not have been executed. Please read the Alexa.log file";
                standardOutputExitCode = 3;
            }

            //append all steps to the root node
            xRoot.AppendChild(steps);

            //create a node that contains the data formatted for nagios
            XmlElement nagiosService = outputFile.CreateElement("nagios");
            XmlElement nagiosMessage = outputFile.CreateElement("message");
            XmlElement nagiosExitCode = outputFile.CreateElement("exitcode");

            //set the values of the nagios nodes
            nagiosMessage.InnerText = standardOutputString + "|" + nagiosPerformance;
            nagiosExitCode.InnerText = standardOutputExitCode.ToString();

            //append nagios child nodes to nagios root node
            nagiosService.AppendChild(nagiosMessage);
            nagiosService.AppendChild(nagiosExitCode);

            //append nagios root node to the xml root node
            xRoot.AppendChild(nagiosService);

            //save the output if user has set this option
            if (ConfigUtils.OutputOnFile == true)
            {
                DirectoryInfo dir = new DirectoryInfo(ConfigUtils.OutputFolder);

                //if the output directory doesn't exist then create it
                if (!Directory.Exists(dir.FullName)) dir.Create();

                //save the xml
                outputFile.Save(Path.Combine(ConfigUtils.OutputFolder, "output.xml"));
            }

            //print to the console the message
            string outString = standardOutputString + "|" + nagiosPerformance;
            Console.Write(outString);

            Program.RunExternalScript();

            //exit with current exit code
            Environment.Exit(standardOutputExitCode);
        }

        /// <summary>
        /// Converts a DateTime into a Unix Timestamp
        /// </summary>
        /// <param name="dateTime">The DateTime to convert</param>
        /// <returns>The Unix Timestamp</returns>
        private static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            TimeSpan _UnixTimeSpan = (dateTime - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)_UnixTimeSpan.TotalSeconds; 

        }
    }
}
