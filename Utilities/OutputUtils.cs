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
using System.Xml;
using System.IO;
using System.Collections;

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

        //structure for all steps that have not been executed
        private struct UnknownStepStruct
        {
            //public long stepDuration;
            public string stepName;
        }

        /// <summary>
        /// Print the output and exit from the program
        /// </summary>
        public static void Finish(Boolean exception)
        {
            int executedStepCounter = StepTimingsCollection.Count();

            bool enableGlobalOutput = false;

            //contains the message to send into the standard output
            string outString = "all steps are ok";

            string serviceName = "";

            //contains the nagios performance string
            string nagiosPerformance = "";

            //contains the exit code of Al'exa
            int standardOutputExitCode = 0; //default exit code is 0, all ok

            //they will contain step with an error state
            List<String> OkStep = new List<String>();
            List<String> CriticalStep = new List<String>();
            List<String> WarningStep = new List<String>();
            List<String> TimeoutStep = new List<String>();

            //list for all steps that have not been executed
            List<UnknownStepStruct> unknownStepList = new List<UnknownStepStruct>();

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
                //globalPerformance.SetAttribute("start", DateTimeToUnixTimestamp(Global.startTime).ToString());d_MM_yyyy_HH.mm.ss
                globalPerformance.SetAttribute("start", Global.startTime.ToString("HH:mm:ss"));
                //globalPerformance.SetAttribute("end", DateTimeToUnixTimestamp(Global.endTime).ToString());
                globalPerformance.SetAttribute("end", Global.endTime.ToString("HH:mm:ss"));
                globalPerformance.SetAttribute("duration", Global.duration.ToString());
                XmlElement globalDate = outputFile.CreateElement("date");
                globalDate.InnerText = Global.startTime.ToString("dd/MM/yyyy");
                //set the exit code of the global node
                XmlElement globalExitcode = outputFile.CreateElement("exitcode");
                globalExitcode.InnerText = "0";

                try
                {
                    //set the name of global node
                    string globalName = Global.xmlNode.SelectSingleNode("name").InnerText;
                    global.SetAttribute("name", globalName);

                    serviceName = Global.xmlNode.SelectSingleNode("name").InnerText;
                    outString = "'" + serviceName + "' service is ok, all steps are ok";

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
                }
                catch { }

                //add current step to the OkStep Array
                try
                {
                    OkStep.Add(global.Attributes["name"].Value + " (global step);" + globalPerformance.Attributes["duration"].Value);
                }
                catch
                {
                    OkStep.Add("global step;" + globalPerformance.Attributes["duration"].Value);
                }

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

                        //change the standard output exit code
                        standardOutputExitCode = 1;

                        //add this step to the warning steps
                        try
                        {
                            WarningStep.Add(global.Attributes["name"].Value + " (global step);" + globalPerformance.Attributes["duration"].Value);

                            OkStep.Remove(global.Attributes["name"].Value + " (global step);" + globalPerformance.Attributes["duration"].Value);
                        }
                        catch
                        {
                            WarningStep.Add("global step;" + globalPerformance.Attributes["duration"].Value);

                            OkStep.Remove("global step;" + globalPerformance.Attributes["duration"].Value);
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

                        //set the standard output exit code
                        standardOutputExitCode = 2;

                        //add this step to the critical steps
                        try
                        {
                            CriticalStep.Add(global.Attributes["name"].Value + " (global step);" + globalPerformance.Attributes["duration"].Value);

                            OkStep.Remove(global.Attributes["name"].Value + " (global step);" + globalPerformance.Attributes["duration"].Value);
                            WarningStep.Remove(global.Attributes["name"].Value + " (global step);" + globalPerformance.Attributes["duration"].Value);
                        }
                        catch
                        {
                            CriticalStep.Add("global step;" + globalPerformance.Attributes["duration"].Value);

                            OkStep.Remove("global step;" + globalPerformance.Attributes["duration"].Value);
                            WarningStep.Remove("global step;" + globalPerformance.Attributes["duration"].Value);
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
                    //set global timeout value into the output file
                    long timeout = long.Parse(Global.xmlNode.SelectSingleNode("performance").Attributes["timeout.value"].Value);
                    globalPerformance.SetAttribute("timeout", timeout.ToString());

                    //check if the global step has exceeded the timeout value
                    if (Global.duration >= timeout)
                    {
                        //change global exit code
                        globalExitcode.InnerText = "3";

                        //set the standard output exit code
                        standardOutputExitCode = 3;

                        //add this step to the unknown steps
                        try
                        {
                            TimeoutStep.Add(global.Attributes["name"].Value + " (global step);" + globalPerformance.Attributes["duration"].Value);

                            OkStep.Remove(global.Attributes["name"].Value + " (global step);" + globalPerformance.Attributes["duration"].Value);
                            WarningStep.Remove(global.Attributes["name"].Value + " (global step);" + globalPerformance.Attributes["duration"].Value);
                            CriticalStep.Remove(global.Attributes["name"].Value + " (global step);" + globalPerformance.Attributes["duration"].Value);
                        }
                        catch
                        {
                            TimeoutStep.Add("global step;" + globalPerformance.Attributes["duration"].Value);

                            OkStep.Remove("global step;" + globalPerformance.Attributes["duration"].Value);
                            WarningStep.Remove("global step;" + globalPerformance.Attributes["duration"].Value);
                            CriticalStep.Remove("global step;" + globalPerformance.Attributes["duration"].Value);
                        }

                    }
                }
                catch { }

                //we don't have a min or max value for the performance data.
                //So we I to add two semicolon to avoid any kind of error on the graph
                nagiosPerformance = nagiosPerformance + ";;";

                //put together global node and global child nodes
                global.AppendChild(globalDate);
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

                    performance.SetAttribute("start", stepTiming.startTime.ToString("HH:mm:ss"));
                    performance.SetAttribute("end", stepTiming.endTime.ToString("HH:mm:ss"));
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

                    //add current step to the OkStep Array
                    try
                    {
                        OkStep.Add(step.Attributes["name"].Value + " (step " + step.Attributes["number"].Value + ");" + performance.Attributes["duration"].Value);
                    }
                    catch
                    {
                        OkStep.Add("Step " + step.Attributes["number"].Value + ";" + performance.Attributes["duration"].Value);
                    }

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

                            //add this step to the warning steps
                            try
                            {
                                WarningStep.Add(step.Attributes["name"].Value + " (step " + step.Attributes["number"].Value + ");" + performance.Attributes["duration"].Value);

                                OkStep.Remove(step.Attributes["name"].Value + " (step " + step.Attributes["number"].Value + ");" + performance.Attributes["duration"].Value);
                            }
                            catch
                            {
                                WarningStep.Add("Step " + step.Attributes["number"].Value + ";" + performance.Attributes["duration"].Value);

                                OkStep.Remove("Step " + step.Attributes["number"].Value + ";" + performance.Attributes["duration"].Value);
                            }

                            if (standardOutputExitCode <= 1)
                            {
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

                            //add this step to the critical steps
                            try
                            {
                                CriticalStep.Add(step.Attributes["name"].Value + " (step " + step.Attributes["number"].Value + ");" + performance.Attributes["duration"].Value);

                                OkStep.Remove(step.Attributes["name"].Value + " (step " + step.Attributes["number"].Value + ");" + performance.Attributes["duration"].Value);
                                WarningStep.Remove(step.Attributes["name"].Value + " (step " + step.Attributes["number"].Value + ");" + performance.Attributes["duration"].Value);
                            }
                            catch
                            {
                                CriticalStep.Add("Step " + step.Attributes["number"].Value + ";" + performance.Attributes["duration"].Value);

                                OkStep.Remove("Step " + step.Attributes["number"].Value + ";" + performance.Attributes["duration"].Value);
                                WarningStep.Remove("Step " + step.Attributes["number"].Value + ";" + performance.Attributes["duration"].Value);
                            }

                            if (standardOutputExitCode <= 2)
                            {
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
                            //add this step to the timeout steps
                            try
                            {
                                TimeoutStep.Add(step.Attributes["name"].Value + " (step " + step.Attributes["number"].Value + ");" + performance.Attributes["duration"].Value);

                                OkStep.Remove(step.Attributes["name"].Value + " (step " + step.Attributes["number"].Value + ");" + performance.Attributes["duration"].Value);
                                CriticalStep.Remove(step.Attributes["name"].Value + " (step " + step.Attributes["number"].Value + ");" + performance.Attributes["duration"].Value);
                                WarningStep.Remove(step.Attributes["name"].Value + " (step " + step.Attributes["number"].Value + ");" + performance.Attributes["duration"].Value);
                            }
                            catch
                            {
                                TimeoutStep.Add("Step " + step.Attributes["number"].Value + ";" + performance.Attributes["duration"].Value);

                                OkStep.Remove("Step " + step.Attributes["number"].Value + ";" + performance.Attributes["duration"].Value);
                                CriticalStep.Remove("Step " + step.Attributes["number"].Value + ";" + performance.Attributes["duration"].Value);
                                WarningStep.Remove("Step " + step.Attributes["number"].Value + ";" + performance.Attributes["duration"].Value);
                            }

                            //performance.SetAttribute("duration", duration.ToString());

                            //change global exit code
                            exitcode.InnerText = "3";

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

            int cnt = 0;
            foreach(XmlNode notExecutedStep in ConfigUtils.AlexaSteps)
            {
                UnknownStepStruct unknownStepStruct = new UnknownStepStruct();

                if (cnt < executedStepCounter)
                {
                    cnt++;
                    continue;
                }
                else
                {
                    bool enableOutput = false;

                    try
                    {
                        //check if we have to write the output of current step into the output file
                        if (notExecutedStep.SelectSingleNode("performance").Attributes["output"].Value.ToLower() == "yes")
                            enableOutput = true;
                    }
                    catch { }

                    if (enableOutput)
                    {
                        standardOutputExitCode = 3;

                        //declare the step node (and its child node) that will be written to the output file
                        XmlElement step = outputFile.CreateElement("step");
                        XmlElement performance = outputFile.CreateElement("performance");
                        XmlElement exitcode = outputFile.CreateElement("exitcode");

                        performance.SetAttribute("start", "n.a.");
                        performance.SetAttribute("end", "n.a.");
                        performance.SetAttribute("duration", "n.a.");

                        //set the exit code of the step
                        exitcode.InnerText = "3";

                        //set the step number
                        step.SetAttribute("number", (cnt + 1).ToString());


                        try
                        {
                            //set the step name
                            string stepName = notExecutedStep.Attributes["name"].Value;
                            step.SetAttribute("name", stepName);

                            unknownStepStruct.stepName = stepName + " (Step " + (cnt + 1).ToString() + ")";

                            //add the duration of current step to the nagios performance string
                            nagiosPerformance = nagiosPerformance + " " + stepName + "=0ms";
                        }
                        catch
                        {
                            //add the duration of current step to the nagios performance string
                            nagiosPerformance = nagiosPerformance + " Step " + (cnt + 1).ToString() + "=0ms";
                            unknownStepStruct.stepName = "Step " + (cnt + 1).ToString();
                        }

                        try
                        {
                            //set warning value into the output file for the current step
                            long warningLevel = long.Parse(notExecutedStep.SelectSingleNode("performance").Attributes["warning"].Value);
                            performance.SetAttribute("warning", warningLevel.ToString());

                            //add warning threshold to the nagios performance string
                            nagiosPerformance = nagiosPerformance + ";" + warningLevel.ToString();
                        }
                        catch
                        {
                            //if no warning threshold is set then add only a semicolon to the nagios performance string
                            nagiosPerformance = nagiosPerformance + ";";
                        }

                        try
                        {
                            //set global critical value into the output file fo the current step
                            long errorLevel = long.Parse(notExecutedStep.SelectSingleNode("performance").Attributes["critical"].Value);
                            performance.SetAttribute("critical", errorLevel.ToString());

                            //add critical threshold to the nagios performance string
                            nagiosPerformance = nagiosPerformance + ";" + errorLevel.ToString();
                        }
                        catch
                        {
                            //if no warning threshold is set then add only a semicolon to the nagios performance string
                            nagiosPerformance = nagiosPerformance + ";";
                        }


                        try
                        {
                            //set the timeout value into the output file for the current step
                            long timeout = long.Parse(notExecutedStep.SelectSingleNode("performance").Attributes["timeout.value"].Value);
                            performance.SetAttribute("timeout", timeout.ToString());
                        }
                        catch { }

                        //we don't have a min or max value for the performance data.
                        //So we I to add two semicolon to avoid any kind of error on the graph
                        nagiosPerformance = nagiosPerformance + ";;";

                        //add current unknownStepStruct element to the unknownStepList
                        unknownStepList.Add(unknownStepStruct);

                        //put together step node and step child nodes
                        step.AppendChild(performance);
                        step.AppendChild(exitcode);
                        steps.AppendChild(step);
                        cnt++;
                    }
                }
            }

            //append all steps to the root node
            xRoot.AppendChild(steps);

            string CriticalString = "critical state step(s):";

            foreach (String critStep in CriticalStep)
            {
                CriticalString = CriticalString + " " + critStep.Split(';')[0] + ",";
            }

            CriticalString = CriticalString.Remove(CriticalString.Length - 1);

            string WarningString = "warning state step(s):";

            foreach (String warnStep in WarningStep)
            {
                WarningString = WarningString + " " + warnStep.Split(';')[0] + ",";
            }

            WarningString = WarningString.Remove(WarningString.Length - 1);

            string TimeoutString = "timeout state step(s):";

            foreach (String timeoutStep in TimeoutStep)
            {
                TimeoutString = TimeoutString + " " + timeoutStep.Split(';')[0] + ",";
            }

            TimeoutString = TimeoutString.Remove(TimeoutString.Length - 1);

            string unknownStepsString = "unknown state step(s):";

            foreach (UnknownStepStruct unknownStepElement in unknownStepList)
            {
                unknownStepsString = unknownStepsString + " " + unknownStepElement.stepName + ",";
            }

            unknownStepsString = unknownStepsString.Remove(unknownStepsString.Length - 1);


            if (standardOutputExitCode != 0)
            {
                outString = "";
            }

            if (exception == true)
            {
                outString = "An internal exception has occurred. Some steps may not have been executed. Please read the Al'exa.log file";
                standardOutputExitCode = 3;
            }


            if (TimeoutString != "timeout state step(s)")
            {
                outString = outString + TimeoutString + ", ";
            }

            if (CriticalString != "critical state step(s)")
            {
                outString = outString + CriticalString + ", ";
            }

            if (WarningString != "warning state step(s)")
            {
                outString = outString + WarningString + ", ";
            }

            if (unknownStepsString != "unknown state step(s)")
            {
                outString = outString + unknownStepsString + ", ";
            }

            outString = outString.Remove(outString.Length - 2);
            //print to the console the message
            outString = outString + "|" + nagiosPerformance;

            if (standardOutputExitCode == 0)
            {
                outString = "OK: " + outString;
            }
            else if (standardOutputExitCode == 1)
            {
                if(serviceName != "")
                    outString = "WARNING: '" + serviceName + "' service has some problems: " + outString;
                else
                    outString = "WARNING: " + outString;
            }
            else if (standardOutputExitCode == 2)
            {
                if (serviceName != "")
                    outString = "CRITICAL: '" + serviceName + "' service has some problems: " + outString;
                else
                    outString = "CRITICAL: " + outString;
            }
            else if (standardOutputExitCode == 3)
            {
                if (serviceName != "")
                    outString = "UNKNOWN: '" + serviceName + "' service has some problems: " + outString;
                else
                    outString = "UNKNOWN: " + outString;
            }

            Console.WriteLine(outString);


            string detailString = "";
            foreach (String critStep in CriticalStep)
            {
                Console.WriteLine("CRITICAL: " + critStep.Split(';')[0] + ", duration is " + critStep.Split(';')[1] + " ms");
                detailString = detailString + "CRITICAL: " + critStep.Split(';')[0] + ", duration is " + critStep.Split(';')[1] + " ms\r\n";
            }

            foreach (String warningStep in WarningStep)
            {
                Console.WriteLine("WARNING: " + warningStep.Split(';')[0] + ", duration is " + warningStep.Split(';')[1] + " ms");
                detailString = detailString + "WARNING: " + warningStep.Split(';')[0] + ", duration is " + warningStep.Split(';')[1] + " ms\r\n";
            }

            foreach (String timeoutStep in TimeoutStep)
            {
                Console.WriteLine("TIMEOUT: " + timeoutStep.Split(';')[0] + ", duration is " + timeoutStep.Split(';')[1] + " ms");
                detailString = detailString + "TIMEOUT: " + timeoutStep.Split(';')[0] + ", duration is " + timeoutStep.Split(';')[1] + " ms\r\n";
            }

            foreach (UnknownStepStruct unknownStepElement in unknownStepList)
            {
                Console.WriteLine("UNKNOWN: " + unknownStepElement.stepName);
                detailString = detailString + "UNKNOWN: " + unknownStepElement.stepName + "\r\n";
            }

            foreach (String okStep in OkStep)
            {
                Console.WriteLine("OK: " + okStep.Split(';')[0] + ", duration is " + okStep.Split(';')[1] + " ms");
                detailString = detailString + "OK: " + okStep.Split(';')[0] + ", duration is " + okStep.Split(';')[1] + " ms\r\n";
            }

            //create a node that contains the data formatted for nagios
            XmlElement nagiosService = outputFile.CreateElement("nagios");
            XmlElement nagiosMessage = outputFile.CreateElement("message");
            XmlElement nagiosExitCode = outputFile.CreateElement("exitcode");

            //set the values of the nagios nodes
            nagiosMessage.InnerText = outString + "|" + nagiosPerformance + "\r\n" + detailString;
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

            //run external scripts
            SystemUtils.RunExternalScript();

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
