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
using System.Collections;
using AutoItX3Lib;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using OCR.TesseractWrapper;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Windows.Automation;
using System.Text.RegularExpressions;

namespace Alexa.Utilities
{
    /// <summary>
    /// Provides functions to interact with the Core of Al'exa
    /// </summary>
    static public class CoreUtils
    {
        //check if we have to send an e-mail
        static public bool sendEmail = false;

        #region private variables

        #region variables for debug
        //set the naming convention for the debug log folders and debug images
        static private string _debugFolderDateFormat = "dd_MM_yyyy_HH.mm.ss.fff";
        static private string _debugImageDateFormat = "HH.mm.ss.fff";
        static private string _debugHomeFolder = "";
        static private string _debugCoreFolder = "";
        #endregion

        static private AutoItX3 _autoIt; //Declare an AutoIT Interpreter object type
        static private Alexa.Core _core; //Declare an Alexa.Core type variable
        static private Bitmap desktopScreen2;
        static private int _stepNumber = 0;
        static private bool _coreInitialized = false;

        //stores configuration properties
        static private int _afterClickDelay = 600;

        //stores OCR informations
        static private string _ocrLangData = "";
        static private string _ocrLanguageSelected = "";

        //stores the LogLevel
        static private bool _debugLogLevel = false;
        static private bool _warningLogLevel = false;
        static private Stopwatch _stepTime; //stores the elapsed time of a single Al'exa step
        static private Stopwatch _otherDelayTime; //stores the elapsed time of mouse movement and/or Key digits

        //flag that indicates if we run any executable
        static private bool _executableStarted = false;

        //stores the home folder of Al'exa
        static private string _homeFolder = "";

        //this struct will contain stepName or stepNumber
        //where we will have to jump (if user has set this option) on timeout event
        private struct JumpToStep
        {
            static public string stepName;
            static public int stepNumber;
        }

        //this enum contains the definition for all Type of Step
        private enum StepType
        {
            RunExe = 0,
            InteractInputBox = 1,
            InteractGenericBox = 2,
            InteractIcon = 3,
            InteractButton = 4,
            InteractIconList = 5,
            InteractText = 6,
            InteractDropDownList = 7
        }

        //this enum contains the definition for all Type of Action
        private enum ActionType
        {
            FindWindow = 0,
        }

        #endregion

        #region common step variables
        //user can disable mouse click and mouse movements
        static private bool _mouseClick = true;
        static private bool _mouseMove = true;

        //store debug properties
        static private string _debugFullPath = "";
        static private string _debugPath = "";
        static private string _debugImageName = "";

        //flag that indicates if the input box was found
        static private bool _found = false;

        //step configuration properties
        static private string _stepName = "";
        static private string _labelPosition = "";
        static private string _label = "";
        static private string _textToInsert = "";
        static private int _boxBrightness = -999; //default value, if it doesn't change I don't change brightness and contrast
        static private int _boxContrast = -999;
        static private int _labelBrightness = -999;
        static private int _labelContrast = -999;

        //offset coordinate, the origin point of these coordinats
        //are the top left point of the box that will be found
        static private int _clickOffsetX = 0;
        static private int _clickOffsetY = 0;

        //store the properties of the label related to the inputbox 
        //that the user wants find
        static private int _labelBoxHeight = 0;
        static private int _labelBoxWidth = 0;

        //store the properties of the box that the user wants find
        static private int _boxHeight = -1;
        static private int _boxWidth = -1;
        static private int _boxTollerance = 0;

        //label offset, the origin point of these coordinats
        //are the top left point of the box that will be found
        static private int _labelBoxOffsetX = 0;
        static private int _labelBoxOffsetY = 0;

        //get the text to insert into the Drop Down List
        static private string _selectItem = "";

        //they will contain the desktop area to crop
        //if user doesn't set these options then all desktop image will be analyzed
        static private int _cropRectX = -1;
        static private int _cropRectY = -1;
        static private int _cropRectHeight = -1;
        static private int _cropRectWidth = -1;

        //word properties
        static private int _charRectMinHeight = 6;
        static private int _charRectMaxHeight = 50;
        static private int _charRectMinWidth = 50;
        static private int _charRectMaxWidth = 300;
        static private int _charRectThickness = 2;
        static private int _charRectExtendLeft = 2;
        //r,g,b
        static private int[] _charRectColor = {255, 0, 0};

        static private bool _performanceEnable = false;

        //this flag indicates if we have to add mouse (and keyboard) time to the step timing
        static private bool _performanceMouseKeyboardEnable = true;

        //word text
        static private string _textValue = "";

        //stores the points of the check region
        static private List<Alexa.Core.Box> _BindRegionOldPoints = new List<Core.Box>();

        //variables for interrupt
        static private int _cropInterruptRegionRectX = -1;
        static private int _cropInterruptRegionRectY = -1;
        static private int _cropInterruptRegionRectHeight = -1;
        static private int _cropInterruptRegionRectWidth = -1;

        static private bool _mustCheckInterruptRegion = false;
        static private bool _updateInterruptBindPoint = false;

        static private int _interruptBindPointX = -1;
        static private int _interruptBindPointY = -1;

        static private int _interruptBrightness = -999;
        static private int _interruptContrast = -999;

        //variable for mouse wheel
        static private bool _enableScroll = false;
        static private int _scrollStep = 1;
        static private string _scrollDirection = "down";
        static private int _scrollLastdelay = 2000;

        //variables for the color change
        static private int[] _oldColor = {-1,-1,-1};
        static private int[] _newColor = {-1,-1,-1};

        //flag that indicates if we have to binarize the image
        static private bool _binarizeImage = false;

        //flag that indicates if we have to binarize the label
        static private bool _binarizeLabel = false;
        #endregion

        /// <summary>
        /// Initialize the CoreUtils class
        /// </summary>
        static public void Init()
        {

            //init the class if it's not already done
            if (_coreInitialized == false)
            {
                //Init the AutoIT language interpreter
                _autoIt = new AutoItX3();
                //Init the Al'exa Core
                _core = new Alexa.Core();
                //Set the OCR Language data folder
                _ocrLangData = ConfigUtils.OcrLanguageData;
                //Set the OCR Language
                _ocrLanguageSelected = ConfigUtils.OcrSelectedLanguage;
                //Set the debug folder
                _debugHomeFolder = Path.Combine(ConfigUtils.LogFolder, "steps");
                //Init the Stopwatch to measure elapsed time of a single Al'exa step
                _stepTime = new Stopwatch();
                //Init the Stopwatch to measure elapsed time of mouse movement and/or Key digits
                _otherDelayTime = new Stopwatch();

                //Init the JumpToStep struct, this will contain stepName or stepNumber
                //where we will have to jump (if user has set this option) on timeout event
                JumpToStep.stepName = "";
                JumpToStep.stepNumber = 0;

                //get the home folder
                _homeFolder = ConfigUtils.HomeFolder;

                //set the loglevel
                if (ConfigUtils.ErrorLevel == LogUtils.ErrorLevel.Debug && ConfigUtils.LogIsEnabled == true)
                {
                    _debugLogLevel = true;
                    _warningLogLevel = true;
                    //enable debug on Alexa.Core
                    _core.EnableDebug(true);

                    //if user has set Debug mode, then create the Debug Directory and then compress it
                    DirectoryInfo debugDir = new DirectoryInfo(_debugHomeFolder);
                    if (!debugDir.Exists) debugDir.Create();
                    //compress the folder
                    SystemUtils.SetDirectoryCompression(debugDir, true);
                }
                else if (ConfigUtils.ErrorLevel == LogUtils.ErrorLevel.Warning && ConfigUtils.LogIsEnabled == true)
                {
                    _warningLogLevel = true;
                }
                //NOTE: we don't have to set error level, this because Al'exa always writes error message.
                //Al'exa will not write any message if user sets the option

                //the core is now initialized
                _coreInitialized = true;
            }
        }

        
        /// <summary>
        /// Runs the steps that are written into the config file
        /// </summary>
        /// <param name="steps">Steps that are written into the config file</param>
        static public void RunSteps(XmlNodeList steps)
        {
            _autoIt.AutoItSetOption("SendKeyDelay", 50); //set AutoIT delay key, I have set 50ms to simulate a human
            _autoIt.MouseMove(0,0);  //set mouse position on top of the screen

            //loop through all steps
            foreach (XmlNode alexaStep in steps)
            {

                //set the number of current step
                _stepNumber++;

                //check if we have to jump to another step
                if (JumpToStep.stepName != "" || JumpToStep.stepNumber != 0)
                {
                    try //the attributes name for a step is optional
                    {
                        //if user has set a step name and it isn't equal to current step name, then continue to the next step
                        if (alexaStep.Attributes["name"].Value.ToLower() != JumpToStep.stepName) continue;
                    }
                    catch { continue; }

                    //if user has set a step number and it isn't equal to current step number, then continue to the next step
                    if (_stepNumber != JumpToStep.stepNumber) continue;
                }

                //always reset JumpToStep.stepName and JumpToStep.stepNumber
                JumpToStep.stepName = "";
                JumpToStep.stepNumber = 0;

                #region get the step type, the control to bind and other step properties

                string stepType = "";
                string controlToBind = "";

                //get the bind attribute. It is not mandatory for all step.
                try { controlToBind = alexaStep.Attributes["bind"].Value.ToLower(); }
                catch { }

                try //the type attribute is mandatory, so if I don't find it I write the message to the Log file
                {
                    stepType = alexaStep.Attributes["type"].Value.ToLower();
                }
                catch(Exception ex)
                {
                    LogUtils.Write(ex); //write the exception to the Log file
                }
                #endregion


                //executes the step type "run"
                if (stepType == "run")
                {
                    ExecStepMethod(StepType.RunExe, alexaStep);
                }
                //executes the step type "digit"
                else if(stepType == "insert")
                {
                    InsertText(alexaStep);
                }
                else if (stepType == "putclipboard")
                {
                    PutToClipboard(alexaStep);
                }
                //executes the step type "mousemove"
                else if (stepType == "mousemove")
                {
                    StepMouseMove(alexaStep);
                }
                //executes the step type "mouseclick"
                else if (stepType == "mouseclick")
                {
                    StepMouseClick(alexaStep);
                }
                //executes the step type "delay"
                else if (stepType == "delay")
                {
                    ExecDelay(alexaStep);
                }
                //executes the step type "interact" on an inputbox
                else if (stepType == "interact" && controlToBind == "inputbox")
                {
                    ExecStepMethod(StepType.InteractInputBox, alexaStep);
                }
                //executes the step type "interact" on a drop down list
                else if (stepType == "interact" && controlToBind == "dropdownlist")
                {
                    ExecStepMethod(StepType.InteractDropDownList, alexaStep);
                }
                //executes the step type "interact" on a generic box
                else if (stepType == "interact" && controlToBind == "genericbox")
                {
                    ExecStepMethod(StepType.InteractGenericBox, alexaStep);
                }
                //executes the step type "interact" on a generic box
                else if (stepType == "interact" && controlToBind == "icon")
                {
                    ExecStepMethod(StepType.InteractIcon, alexaStep);
                }
                //executes the step type "interact" on a button
                else if (stepType == "interact" && controlToBind == "button")
                {
                    ExecStepMethod(StepType.InteractButton, alexaStep);
                }
                //executes the step type "interact" on an iconlist
                else if (stepType == "interact" && controlToBind == "iconlist")
                {
                    ExecStepMethod(StepType.InteractIconList, alexaStep);
                }
                //executes the step type "interact" on a text
                else if (stepType == "interact" && controlToBind == "word")
                {
                    ExecStepMethod(StepType.InteractText, alexaStep);
                }
                //change the configuration
                else if (stepType == "configuration")
                {
                    ChangeStepBehavior(alexaStep);
                }
                //I have a click step type but I don't have any "bind" attribute
                else if (stepType == "interact" && controlToBind == "")
                {
                    //write the error
                    LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Error, "cannot find the bind attribute for the step " + GetStepNameNumber(alexaStep) + ". This step is an \"interact\" type and you have to define a bind attribute.");
                }
                else //The step type doesn't exist
                {
                    //write the error
                    LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Error, "step type \"" + stepType + "\" of step " + GetStepNameNumber(alexaStep) + " doesn't exist.");
                }

                //stop the stopwatch
                _stepTime.Stop();
            }
        }

        #region executes the "run" type step
        /// <summary>
        /// Runs the method that executes the run type step
        /// </summary>
        /// <param name="alexaStep">the xml node that contains the step</param>
        /// <returns>return true if the executables was execute</returns>
        private static bool StepRunExe(XmlNode alexaStep)
        {
            try
            {
                //contains the processname
                string processName = "";

                //contains the full path (plus name) of the program that we have to run
                string executable = "";

                //contains the arguments of the program
                string arguments = "";

                //contains the name of the window that we have to wait
                string windowRegEx = "";

                //contains the flag that indicates if we have to maximize the window
                Boolean maximize = true; //default is true

                //get the executable
                executable = alexaStep.SelectSingleNode("executable").InnerText;

                //get the arguments
                foreach (XmlNode argument in alexaStep.SelectNodes("argument"))
                {
                    if (arguments == "") arguments = argument.InnerText;
                    else
                        arguments = arguments + " " + argument.InnerText;
                }

                try
                {
                    //get the name of the window that we have to wait, it isn't mandatory.
                    windowRegEx = alexaStep.SelectSingleNode("window").InnerText;
                }
                catch { }

                try
                {
                    string maximizeString = "";
                    //check if we have to maximize the window, it isn't mandatory.
                    maximizeString = alexaStep.SelectSingleNode("window").Attributes["maximize"].Value;

                    if (maximizeString.ToLower() == "no") maximize = false;
                }
                catch { }

                //if we don't have already launched the executable
                if (_executableStarted == false)
                {
                    //register current time
                    _otherDelayTime.Reset();
                    _otherDelayTime.Start();

                    //create the process object
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;

                    //set the filename and the arguments
                    p.StartInfo.FileName = executable;
                    if(arguments != "") p.StartInfo.Arguments = arguments;

                    //this doesn't work for IE, FireFox and other programs, so later we will have to call also the windows api
                    if (maximize) p.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;

                    //start the process
                    p.Start();
                    processName = p.ProcessName;
                    _executableStarted = true;

                    //stop the stopwatch
                    _otherDelayTime.Stop();

                    //To understand these two lines of code I do a small example: Suppose for example to run FireFox passing a new url as argument.
                    //If FireFox is already open then we will obtain a new tab and not a new istance of FireFox. So to close the page that has loaded our url we have to kill
                    //the first process of FireFox. To do this I call GetUserProcessesByName that performs a WMI query to get all processes that in their name contain 
                    //p.ProcessName. In our example I will kill all FireFox processes. Note that also Internet Explorer and other programs work like FireFox.
                    string processToKillRegEx = "";
                    try
                    {
                        //get the regular expression containing the name of processes to kill
                        processToKillRegEx = ConfigUtils.ProcessesToKill;


                        if (processToKillRegEx != "" & Regex.IsMatch(processName, processToKillRegEx))
                        {
                            List<UInt32> executableProcesses = SystemUtils.ProcessUtils.GetUserProcessesByName(Environment.UserDomainName, Environment.UserName, processName);
                            SystemUtils.ProcessUtils.processesToKill.AddRange(executableProcesses);
                        }
                    }
                    catch { }


                    //write a debug message
                    if (_debugLogLevel)
                        LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "run program: " + executable + " with argument(s): " + arguments);
                }

                //if windowRegEx is not empty then we have to wait the window
                if (windowRegEx != "")
                {
                    IntPtr handle;

                    //check if the window has appeared
                    if (SystemUtils.User32.GetWindow(windowRegEx,out handle) == true)
                    {
                        if (maximize)
                            //maximize the window
                            MaximizeWindow(handle);
                        else
                            //show window on top
                            SystemUtils.User32.ShowWindowOnTop(handle, false);

                        //if window has appeared set _executableStarted to false
                        _executableStarted = false;

                        return true;
                    }
                }
                else
                {
                    return true;
                }


            }
            catch (Exception ex)
            {
                //write the error
                LogUtils.Write(ex);
                Program.Finish(true);
            }

            //if we are here then we have to return false
            return false;
        }
        #endregion

        #region execute the "interact" type step on an inputbox
        /// <summary>
        /// Runs the method that does a click type step on an input box
        /// </summary>
        /// <returns>return true if the input box was found</returns>
        private static bool StepInteractInputBox()
        {
            //store the label images
            Bitmap left = null;
            Bitmap right = null;
            Bitmap top = null;
            Bitmap inside = null;
            Bitmap desktopScreen = ScreenUtils.CaptureDesktop();

            int mouseX = 0;
            int mouseY = 0;

            try
            {
                //if user has set all crop rectangle attributes then crop the rectangle from the desktop screenshot
                if (_cropRectHeight >= 0 && _cropRectWidth >= 0 && _cropRectX >= 0 && _cropRectY >= 0)
                {
                    //Alexa.Core will analyze the cropped image
                    desktopScreen = CropRect(desktopScreen, new Rectangle(_cropRectX, _cropRectY, _cropRectWidth, _cropRectHeight));
                }

                //save the screenshot of the desktop
                if (_debugLogLevel) desktopScreen.Save(Path.Combine(_debugFullPath, DateTime.Now.ToString(_debugImageDateFormat) + "_DesktopScreenshot.bmp"));

                //set the source image for the Alexa.Core
                SetCoreSourceImage(desktopScreen);

                //check if we have to change the color
                if (_oldColor[0] != -1 && _oldColor[1] != -1 && _oldColor[2] != -1 && _newColor[0] != -1 && _newColor[1] != -1 && _newColor[2] != -1)
                {
                    Bitmap clone = new Bitmap(desktopScreen.Width, desktopScreen.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    using (Graphics gr = Graphics.FromImage(clone))
                    {
                        gr.DrawImage(desktopScreen, new Rectangle(0, 0, clone.Width, clone.Height));
                    }
                    Bitmap c = _core.ReplaceColor(clone, _oldColor[0], _oldColor[1], _oldColor[2], _newColor[0], _newColor[1], _newColor[2]);
                    clone.Dispose();
                    _core.SetSourceImage(c);

                    c.Dispose();
                }

                if (_binarizeImage)
                {
                    //if user has set brightness and contrast value then change it
                    if (_boxBrightness != -999 && _boxContrast != -999)
                        _core.SetBrightnessContrast(_boxBrightness, _boxContrast);
                    else
                        //otherwise use a value that is ok for almost all application
                        _core.SetBrightnessContrast(-100, 85);
                }



                //get all input boxes that are present into the Alexa.Core source image
                List<Alexa.Core.Box> boxes;
                if (_binarizeImage)
                {
                    boxes = _core.GetInputBoxes();
                }
                else
                {
                    boxes = _core.GetInputBoxesV2();
                }

                boxes.Reverse();

                //looking for the label
                int boxCnt = 0;

                foreach (Alexa.Core.Box box in boxes)
                {

                    #region check the top label
                    if (_labelPosition == "top")
                    {
                        //if we are in top position and user has set labelBoxHeight
                        if (_labelBoxHeight != 0)
                        {
                            //crop an image on top of the inputbox
                            top = CropRect(desktopScreen, new Rectangle(box.x - 10, (box.y - _labelBoxHeight), box.width + 10, _labelBoxHeight));
                            top = (Bitmap)ResizeImage(top, new Size(top.Width * 3, top.Height * 3));
                        }
                        else
                        {
                            //crop an image on top of the inputbox
                            top = CropRect(desktopScreen, new Rectangle(box.x - 10, (box.y - box.height), box.width + 10, box.height));
                            top = (Bitmap)ResizeImage(top, new Size(top.Width * 3, top.Height * 3));
                        }

                        //set the source image for the Alexa.Core
                        SetCoreSourceImage(top);

                        if (_binarizeLabel)
                        {
                            //if user has set brightness and contrast value then change it
                            if (_labelBrightness != -999 && _labelContrast != -999)
                                _core.SetBrightnessContrast(_labelBrightness, _labelContrast);
                            else
                                //otherwise use a value that is ok for almost all application
                                _core.SetBrightnessContrast(-70, 100);

                            //Binarize the Alexa.Core source image
                            _core.BinarizeImage();
                        }


                        #region debug message
                        //save the binarized image if we are in debug
                        if (_debugLogLevel)
                        {
                            _debugImageName = DateTime.Now.ToString(_debugImageDateFormat) + "_InputBox" + boxCnt + "_LabelPos_Top.bmp";
                            _core.GetSourceImage().Save(Path.Combine(_debugFullPath, _debugImageName));
                            LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + _debugPath + "\\" + _debugImageName);
                        }
                        #endregion

                        //if the OCR engine has found the label text then click on the input box and insert the text
                        if (checkStringByOCR(_core.GetSourceImage(), _label) == true && _found == false)
                        {
                            //if this step must update the interrupt region origin point...
                            if (_updateInterruptBindPoint == true)
                            {
                                // ...then update the points
                                _interruptBindPointX = box.x;
                                _interruptBindPointY = box.y;
                                _updateInterruptBindPoint = false;
                            }

                            //get the coordinates of where we have to click
                            mouseX = box.x + (box.width / 2) + _clickOffsetX;
                            mouseY = box.y + (box.height / 2) + _clickOffsetY;
                            ClickAndInsert(mouseX, mouseY, _textToInsert);
                            _found = true;
                            //exit from the foreach loop
                            break;
                        }
                    }
                    #endregion

                    #region check the left label
                    if (_labelPosition == "left")
                    {
                        //if we are in top position and user has set labelBoxHeight
                        if (_labelBoxWidth != 0)
                        {
                            //crop an image on top of the inputbox
                            left = CropRect(desktopScreen, new Rectangle((box.x - _labelBoxWidth), box.y, _labelBoxWidth, box.height));
                            left = (Bitmap)ResizeImage(left, new Size(left.Width * 3, left.Height * 3));
                        }
                        else
                        {
                            //crop an image on top of the inputbox
                            left = CropRect(desktopScreen, new Rectangle((box.x - box.width * 2), box.y, box.width * 2, box.height));
                            left = (Bitmap)ResizeImage(left, new Size(left.Width * 3, left.Height * 3));
                        }

                        //set the source image for the Alexa.Core
                        SetCoreSourceImage(left);

                        if (_binarizeLabel)
                        {
                            //if user has set brightness and contrast value then change it
                            if (_labelBrightness != -999 && _labelContrast != -999)
                                _core.SetBrightnessContrast(_labelBrightness, _labelContrast);
                            else
                                //otherwise use a value that is ok for almost all application
                                _core.SetBrightnessContrast(-70, 100);

                            //Binarize the Alexa.Core source image
                            _core.BinarizeImage();
                        }

                        #region debug message
                        //save the binarized image if we are in debug
                        if (_debugLogLevel)
                        {
                            _debugImageName = DateTime.Now.ToString(_debugImageDateFormat) + "_InputBox" + boxCnt + "_LabelPos_Left.bmp";
                            _core.GetSourceImage().Save(Path.Combine(_debugFullPath, _debugImageName));
                            LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + _debugPath + "\\" + _debugImageName);
                        }
                        #endregion

                        //if the OCR engine has found the label text then click on the input box and insert the text
                        if (checkStringByOCR(_core.GetSourceImage(), _label) == true && _found == false)
                        {
                            //if this step must update the interrupt region origin point...
                            if (_updateInterruptBindPoint == true)
                            {
                                // ...then update the points
                                _interruptBindPointX = box.x;
                                _interruptBindPointY = box.y;
                                _updateInterruptBindPoint = false;
                            }

                            //get the coordinates of where we have to click
                            mouseX = box.x + (box.width / 2) + _clickOffsetX;
                            mouseY = box.y + (box.height / 2) + _clickOffsetY;
                            ClickAndInsert(mouseX, mouseY, _textToInsert);
                            _found = true;
                            //exit from the foreach loop
                            break;
                        }
                    }
                    #endregion

                    #region check the right label
                    if (_labelPosition == "right")
                    {
                        if (_labelBoxWidth != 0)
                        {
                            //crop an image on top of the inputbox
                            right = CropRect(desktopScreen, new Rectangle((box.x + box.width), box.y, _labelBoxWidth, box.height));
                            right = (Bitmap)ResizeImage(right, new Size(right.Width * 3, right.Height * 3));
                        }
                        else
                        {
                            //crop an image on top of the inputbox
                            right = CropRect(desktopScreen, new Rectangle((box.x + box.width), box.y, box.width * 2, box.height));
                            right = (Bitmap)ResizeImage(right, new Size(right.Width * 3, right.Height * 3));
                        }

                        //set the source image for the Alexa.Core
                        SetCoreSourceImage(right);

                        if (_binarizeLabel)
                        {
                            //if user has set brightness and contrast value then change it
                            if (_labelBrightness != -999 && _labelContrast != -999)
                                _core.SetBrightnessContrast(_labelBrightness, _labelContrast);
                            else
                                //otherwise use a value that is ok for almost all application
                                _core.SetBrightnessContrast(-70, 100);

                            //Binarize the Alexa.Core source image
                            _core.BinarizeImage();
                        }

                        #region debug message
                        //save the binarized image if we are in debug
                        if (_debugLogLevel)
                        {
                            _debugImageName = DateTime.Now.ToString(_debugImageDateFormat) + "_InputBox" + boxCnt + "_LabelPos_Right.bmp";
                            _core.GetSourceImage().Save(Path.Combine(_debugFullPath, _debugImageName));
                            LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + _debugPath + "\\" + _debugImageName);
                        }
                        #endregion

                        //if the OCR engine has found the label text then click on the input box and insert the text
                        if (checkStringByOCR(_core.GetSourceImage(), _label) == true && _found == false)
                        {
                            //if this step must update the interrupt region origin point...
                            if (_updateInterruptBindPoint == true)
                            {
                                // ...then update the points
                                _interruptBindPointX = box.x;
                                _interruptBindPointY = box.y;
                                _updateInterruptBindPoint = false;
                            }

                            //get the coordinates of where we have to click
                            mouseX = box.x + (box.width / 2) + _clickOffsetX;
                            mouseY = box.y + (box.height / 2) + _clickOffsetY;
                            ClickAndInsert(mouseX, mouseY, _textToInsert);
                            _found = true;
                            //exit from the foreach loop
                            break;
                        }
                    }
                    #endregion

                    #region check the inside label
                    if (_labelPosition == "" || _labelPosition == "inside")
                    {
                        //crop an image on top of the inputbox
                        inside = CropRect(desktopScreen, new Rectangle(box.x, box.y, box.width, box.height));
                        inside = (Bitmap)ResizeImage(inside, new Size(inside.Width * 3, inside.Height * 3));
                        //set the source image for the Alexa.Core
                        SetCoreSourceImage(inside);

                        if (_binarizeLabel)
                        {
                            //if user has set brightness and contrast value then change it
                            if (_labelBrightness != -999 && _labelContrast != -999)
                                _core.SetBrightnessContrast(_labelBrightness, _labelContrast);
                            else
                                //otherwise use a value that is ok for almost all application
                                _core.SetBrightnessContrast(-70, 100);

                            //Binarize the Alexa.Core source image
                            _core.BinarizeImage();
                        }

                        #region debug message
                        //save the binarized image if we are in debug
                        if (_debugLogLevel)
                        {
                            _debugImageName = DateTime.Now.ToString(_debugImageDateFormat) + "_InputBox" + boxCnt + "_LabelPos_Inside.bmp";
                            _core.GetSourceImage().Save(Path.Combine(_debugFullPath, _debugImageName));
                            LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + _debugPath + "\\" + _debugImageName);
                        }
                        #endregion

                        //if the OCR engine has found the label text then click on the input box and insert the text
                        if (checkStringByOCR(_core.GetSourceImage(), _label) == true && _found == false)
                        {
                            //if this step must update the interrupt region origin point...
                            if (_updateInterruptBindPoint == true)
                            {
                                // ...then update the points
                                _interruptBindPointX = box.x;
                                _interruptBindPointY = box.y;
                                _updateInterruptBindPoint = false;
                            }

                            //get the coordinates of where we have to click
                            mouseX = box.x + (box.width / 2) + _clickOffsetX;
                            mouseY = box.y + (box.height / 2) + _clickOffsetY;
                            ClickAndInsert(mouseX, mouseY, _textToInsert);
                            _found = true;
                            //exit from the foreach loop
                            break;
                        }
                    }
                    #endregion

                    boxCnt++;


                    #region check interrupt region image
                    if (_mustCheckInterruptRegion == true && _interruptBindPointX >= 0 && _interruptBindPointY >= 0)
                    {
                        int boxesEquals = 0;
                        List<Core.Box> BindRegionPoints = GetBindRegionPoints();

                        foreach (Alexa.Core.Box oldBox in _BindRegionOldPoints)
                        {
                            foreach (Alexa.Core.Box newBox in BindRegionPoints)
                            {
                                if (oldBox.height == newBox.height && oldBox.width == newBox.width && oldBox.x == newBox.x && oldBox.y == newBox.y)
                                {
                                    boxesEquals++;
                                }
                            }
                        }

                        if (_BindRegionOldPoints.Count < BindRegionPoints.Count)
                        {
                            UpdateRegionPoints();
                            return false;
                        }
                        else if (boxesEquals < _BindRegionOldPoints.Count)
                        {
                            UpdateRegionPoints();
                            return false;
                        }

                        UpdateRegionPoints();
                    }
                    #endregion

                }

                //if the input box was not found save the error message
                if (_found == false)
                {
                    //write the error message
                    if (_warningLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Warning, "the input box (with \"" + _label + "\" as label value) was not found, the step is " + _stepName);
                    //exit from the method and return false
                    return false;
                }
                else
                    return true;
            }
            catch(Exception ex)
            {
                //write the error
                LogUtils.Write(ex);
                Program.Finish(true);
                return false;
            }
            finally
            {
                //release all images
                if (inside != null) inside.Dispose();
                if (top != null) top.Dispose();
                if (right != null) right.Dispose();
                if (left != null) left.Dispose();
            }
        }
        #endregion

        #region execute the "interact" type step on an DropDownList
        /// <summary>
        /// Runs the method that does a click type step on an Drop Down List
        /// </summary>
        /// <returns>return true if the Drop Down List was found</returns>
        private static bool StepInteractDropDownList()
        {

            //store the label images
            Bitmap left = null;
            Bitmap right = null;
            Bitmap top = null;
            Bitmap inside = null;
            Bitmap desktopScreen = ScreenUtils.CaptureDesktop();

            int mouseX = 0;
            int mouseY = 0;

            //if user has set all crop rectangle attributes then crop the rectangle from the desktop screenshot
            if (_cropRectHeight >= 0 && _cropRectWidth >= 0 && _cropRectX >= 0 && _cropRectY >= 0)
            {
                //Alexa.Core will analyze the cropped image
                desktopScreen = CropRect(desktopScreen, new Rectangle(_cropRectX, _cropRectY, _cropRectWidth, _cropRectHeight));
            }

            try
            {

                //save the screenshot of the desktop
                if (_debugLogLevel) desktopScreen.Save(Path.Combine(_debugFullPath, DateTime.Now.ToString(_debugImageDateFormat) + "_DesktopScreenshot.bmp"));

                //set the source image for the Alexa.Core
                SetCoreSourceImage(desktopScreen);

                //check if we have to change the color
                if (_oldColor[0] != -1 && _oldColor[1] != -1 && _oldColor[2] != -1 && _newColor[0] != -1 && _newColor[1] != -1 && _newColor[2] != -1)
                {
                    Bitmap clone = new Bitmap(desktopScreen.Width, desktopScreen.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    using (Graphics gr = Graphics.FromImage(clone))
                    {
                        gr.DrawImage(desktopScreen, new Rectangle(0, 0, clone.Width, clone.Height));
                    }
                    Bitmap c = _core.ReplaceColor(clone, _oldColor[0], _oldColor[1], _oldColor[2], _newColor[0], _newColor[1], _newColor[2]);
                    clone.Dispose();
                    _core.SetSourceImage(c);

                    c.Dispose();
                }

                if (_binarizeImage)
                {
                    //if user has set brightness and contrast value then change it
                    if (_boxBrightness != -999 && _boxContrast != -999)
                        _core.SetBrightnessContrast(_boxBrightness, _boxContrast);
                    else
                        //otherwise use a value that is ok for almost all application
                        _core.SetBrightnessContrast(-100, 85);
                }

                //get all Drop Down Listes that are present into the Alexa.Core source image
                List<Alexa.Core.Box> boxes;
                if (_binarizeImage)
                {
                    boxes = _core.GetInputBoxes();
                }
                else
                {
                    boxes = _core.GetInputBoxesV2();
                }
                boxes.Reverse();

                //looking for the label
                int boxCnt = 0;
                foreach (Alexa.Core.Box box in boxes)
                {

                    #region check the top label
                    if (_labelPosition == "top")
                    {
                        //if we are in top position and user has set labelBoxHeight
                        if (_labelBoxHeight != 0)
                        {
                            //crop an image on top of the dropdownlist
                            top = CropRect(desktopScreen, new Rectangle(box.x - 10, (box.y - _labelBoxHeight), box.width + 10, _labelBoxHeight));
                            top = (Bitmap)ResizeImage(top, new Size(top.Width * 3, top.Height * 3));
                        }
                        else
                        {
                            //crop an image on top of the dropdownlist
                            top = CropRect(desktopScreen, new Rectangle(box.x - 10, (box.y - box.height), box.width + 10, box.height));
                            top = (Bitmap)ResizeImage(top, new Size(top.Width * 3, top.Height * 3));
                        }

                        //set the source image for the Alexa.Core
                        SetCoreSourceImage(top);

                        if (_binarizeLabel)
                        {
                            //if user has set brightness and contrast value then change it
                            if (_labelBrightness != -999 && _labelContrast != -999)
                                _core.SetBrightnessContrast(_labelBrightness, _labelContrast);
                            else
                                //otherwise use a value that is ok for almost all application
                                _core.SetBrightnessContrast(-70, 100);

                            //Binarize the Alexa.Core source image
                            _core.BinarizeImage();
                        }

                        #region debug message
                        //save the binarized image if we are in debug
                        if (_debugLogLevel)
                        {
                            _debugImageName = DateTime.Now.ToString(_debugImageDateFormat) + "_DropDownList" + boxCnt + "_LabelPos_Top.bmp";
                            _core.GetSourceImage().Save(Path.Combine(_debugFullPath, _debugImageName));
                            LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + _debugPath + "\\" + _debugImageName);
                        }
                        #endregion

                        //if the OCR engine has found the label text then click on the Drop Down List and insert the text
                        if (checkStringByOCR(_core.GetSourceImage(), _label) == true && _found == false)
                        {
                            //if this step must update the interrupt region origin point...
                            if (_updateInterruptBindPoint == true)
                            {
                                // ...then update the points
                                _interruptBindPointX = box.x;
                                _interruptBindPointY = box.y;
                                _updateInterruptBindPoint = false;
                            }

                            //get the coordinates of where we have to click
                            mouseX = box.x + (box.width / 2) + _clickOffsetX;
                            mouseY = box.y + (box.height / 2) + _clickOffsetY;
                            SelectListItem(mouseX, mouseY, _selectItem);
                            _found = true;
                            //exit from the foreach loop
                            break;
                        }
                    }
                    #endregion

                    #region check the left label
                    if (_labelPosition == "left")
                    {
                        //if we are in top position and user has set labelBoxHeight
                        if (_labelBoxWidth != 0)
                        {
                            //crop an image on top of the dropdownlist
                            left = CropRect(desktopScreen, new Rectangle((box.x - _labelBoxWidth), box.y, _labelBoxWidth, box.height));
                            left = (Bitmap)ResizeImage(left, new Size(left.Width * 3, left.Height * 3));
                        }
                        else
                        {
                            //crop an image on top of the dropdownlist
                            left = CropRect(desktopScreen, new Rectangle((box.x - box.width * 2), box.y, box.width * 2, box.height));
                            left = (Bitmap)ResizeImage(left, new Size(left.Width * 3, left.Height * 3));
                        }

                        //set the source image for the Alexa.Core
                        SetCoreSourceImage(left);

                        if (_binarizeLabel)
                        {
                            //if user has set brightness and contrast value then change it
                            if (_labelBrightness != -999 && _labelContrast != -999)
                                _core.SetBrightnessContrast(_labelBrightness, _labelContrast);
                            else
                                //otherwise use a value that is ok for almost all application
                                _core.SetBrightnessContrast(-70, 100);

                            //Binarize the Alexa.Core source image
                            _core.BinarizeImage();
                        }

                        #region debug message
                        //save the binarized image if we are in debug
                        if (_debugLogLevel)
                        {
                            _debugImageName = DateTime.Now.ToString(_debugImageDateFormat) + "_DropDownList" + boxCnt + "_LabelPos_Left.bmp";
                            _core.GetSourceImage().Save(Path.Combine(_debugFullPath, _debugImageName));
                            LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + _debugPath + "\\" + _debugImageName);
                        }
                        #endregion

                        //if the OCR engine has found the label text then click on the Drop Down List and insert the text
                        if (checkStringByOCR(_core.GetSourceImage(), _label) == true && _found == false)
                        {
                            //if this step must update the interrupt region origin point...
                            if (_updateInterruptBindPoint == true)
                            {
                                // ...then update the points
                                _interruptBindPointX = box.x;
                                _interruptBindPointY = box.y;
                                _updateInterruptBindPoint = false;
                            }

                            //get the coordinates of where we have to click
                            mouseX = box.x + (box.width / 2) + _clickOffsetX;
                            mouseY = box.y + (box.height / 2) + _clickOffsetY;
                            SelectListItem (mouseX, mouseY, _selectItem);
                            _found = true;
                            //exit from the foreach loop
                            break;
                        }
                    }
                    #endregion

                    #region check the right label
                    if (_labelPosition == "right")
                    {
                        if (_labelBoxWidth != 0)
                        {
                            //crop an image on top of the dropdownlist
                            right = CropRect(desktopScreen, new Rectangle((box.x + box.width), box.y, _labelBoxWidth, box.height));
                            right = (Bitmap)ResizeImage(right, new Size(right.Width * 3, right.Height * 3));
                        }
                        else
                        {
                            //crop an image on top of the dropdownlist
                            right = CropRect(desktopScreen, new Rectangle((box.x + box.width), box.y, box.width * 2, box.height));
                            right = (Bitmap)ResizeImage(right, new Size(right.Width * 3, right.Height * 3));
                        }

                        //set the source image for the Alexa.Core
                        SetCoreSourceImage(right);

                        if (_binarizeLabel)
                        {
                            //if user has set brightness and contrast value then change it
                            if (_labelBrightness != -999 && _labelContrast != -999)
                                _core.SetBrightnessContrast(_labelBrightness, _labelContrast);
                            else
                                //otherwise use a value that is ok for almost all application
                                _core.SetBrightnessContrast(-70, 100);

                            //Binarize the Alexa.Core source image
                            _core.BinarizeImage();
                        }

                        #region debug message
                        //save the binarized image if we are in debug
                        if (_debugLogLevel)
                        {
                            _debugImageName = DateTime.Now.ToString(_debugImageDateFormat) + "_DropDownList" + boxCnt + "_LabelPos_Right.bmp";
                            _core.GetSourceImage().Save(Path.Combine(_debugFullPath, _debugImageName));
                            LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + _debugPath + "\\" + _debugImageName);
                        }
                        #endregion

                        //if the OCR engine has found the label text then click on the Drop Down List and insert the text
                        if (checkStringByOCR(_core.GetSourceImage(), _label) == true && _found == false)
                        {
                            //if this step must update the interrupt region origin point...
                            if (_updateInterruptBindPoint == true)
                            {
                                // ...then update the points
                                _interruptBindPointX = box.x;
                                _interruptBindPointY = box.y;
                                _updateInterruptBindPoint = false;
                            }

                            //get the coordinates of where we have to click
                            mouseX = box.x + (box.width / 2) + _clickOffsetX;
                            mouseY = box.y + (box.height / 2) + _clickOffsetY;
                            SelectListItem(mouseX, mouseY, _selectItem);
                            _found = true;
                            //exit from the foreach loop
                            break;
                        }
                    }
                    #endregion

                    #region check the inside label
                    if (_labelPosition == "" || _labelPosition == "inside")
                    {
                        //crop an image on top of the DropDownList
                        inside = CropRect(desktopScreen, new Rectangle(box.x, box.y, box.width, box.height));
                        inside = (Bitmap)ResizeImage(inside, new Size(inside.Width * 3, inside.Height * 3));
                        //set the source image for the Alexa.Core
                        SetCoreSourceImage(inside);

                        if (_binarizeLabel)
                        {
                            //if user has set brightness and contrast value then change it
                            if (_labelBrightness != -999 && _labelContrast != -999)
                                _core.SetBrightnessContrast(_labelBrightness, _labelContrast);
                            else
                                //otherwise use a value that is ok for almost all application
                                _core.SetBrightnessContrast(-70, 100);

                            //Binarize the Alexa.Core source image
                            _core.BinarizeImage();
                        }

                        #region debug message
                        //save the binarized image if we are in debug
                        if (_debugLogLevel)
                        {
                            _debugImageName = DateTime.Now.ToString(_debugImageDateFormat) + "_DropDownList" + boxCnt + "_LabelPos_Inside.bmp";
                            _core.GetSourceImage().Save(Path.Combine(_debugFullPath, _debugImageName));
                            LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + _debugPath + "\\" + _debugImageName);
                        }
                        #endregion

                        //if the OCR engine has found the label text then click on the Drop Down List and insert the text
                        if (checkStringByOCR(_core.GetSourceImage(), _label) == true && _found == false)
                        {
                            //if this step must update the interrupt region origin point...
                            if (_updateInterruptBindPoint == true)
                            {
                                // ...then update the points
                                _interruptBindPointX = box.x;
                                _interruptBindPointY = box.y;
                                _updateInterruptBindPoint = false;
                            }

                            //get the coordinates of where we have to click
                            mouseX = box.x + (box.width / 2) + _clickOffsetX;
                            mouseY = box.y + (box.height / 2) + _clickOffsetY;
                            SelectListItem(mouseX, mouseY, _selectItem);
                            _found = true;
                            //exit from the foreach loop
                            break;
                        }
                    }
                    #endregion

                    boxCnt++;

                    #region check interrupt region image
                    if (_mustCheckInterruptRegion == true && _interruptBindPointX >= 0 && _interruptBindPointY >= 0)
                    {
                        int boxesEquals = 0;
                        List<Core.Box> BindRegionPoints = GetBindRegionPoints();

                        foreach (Alexa.Core.Box oldBox in _BindRegionOldPoints)
                        {
                            foreach (Alexa.Core.Box newBox in BindRegionPoints)
                            {
                                if (oldBox.height == newBox.height && oldBox.width == newBox.width && oldBox.x == newBox.x && oldBox.y == newBox.y)
                                {
                                    boxesEquals++;
                                }
                            }
                        }

                        if (_BindRegionOldPoints.Count < BindRegionPoints.Count)
                        {
                            UpdateRegionPoints();
                            return false;
                        }
                        else if (boxesEquals < _BindRegionOldPoints.Count)
                        {
                            UpdateRegionPoints();
                            return false;
                        }

                        UpdateRegionPoints();
                    }
                    #endregion
                }

                //if the Drop Down List was not found save the error message
                if (_found == false)
                {
                    //write the error message
                    if (_warningLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Warning, "the Drop Down List (with \"" + _label + "\" as label value) was not found, the step is " + _stepName);
                    //exit from the method and return false
                    return false;
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                //write the error
                LogUtils.Write(ex);
                Program.Finish(true);
                return false;
            }
            finally
            {
                //release all images
                if (inside != null) inside.Dispose();
                if (top != null) top.Dispose();
                if (right != null) right.Dispose();
                if (left != null) left.Dispose();
            }
        }
        #endregion

        #region execute the "interact" type step on a generic box
        /// <summary>
        /// Runs the method that does a click type step on a generic box
        /// </summary>
        /// <returns>return true if the generic box was found</returns>
        private static bool StepInteractGenericBox()
        {
            //store the desktop screenshot
            Bitmap desktopScreen;

            //store the label images
            Bitmap labelImg = null;

            //capture desktop image
            desktopScreen = ScreenUtils.CaptureDesktop();

            int mouseX = 0;
            int mouseY = 0;

            //if user has set all crop rectangle attributes then crop the rectangle from the desktop screenshot
            if (_cropRectHeight >= 0 && _cropRectWidth >= 0 && _cropRectX >= 0 && _cropRectY >= 0)
            {
                //Alexa.Core will analyze the cropped image
                desktopScreen = CropRect(desktopScreen, new Rectangle(_cropRectX, _cropRectY, _cropRectWidth, _cropRectHeight));
            }

            try
            {

                //save the screenshot of the desktop
                if (_debugLogLevel) desktopScreen.Save(Path.Combine(_debugFullPath, DateTime.Now.ToString(_debugImageDateFormat) + "_DesktopScreenshot.bmp"));

                //set the source image for the core
                SetCoreSourceImage(desktopScreen);

                //check if we have to change the color
                if (_oldColor[0] != -1 && _oldColor[1] != -1 && _oldColor[2] != -1 && _newColor[0] != -1 && _newColor[1] != -1 && _newColor[2] != -1)
                {
                    Bitmap clone = new Bitmap(desktopScreen.Width, desktopScreen.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    using (Graphics gr = Graphics.FromImage(clone))
                    {
                        gr.DrawImage(desktopScreen, new Rectangle(0, 0, clone.Width, clone.Height));
                    }
                    Bitmap c = _core.ReplaceColor(clone, _oldColor[0], _oldColor[1], _oldColor[2], _newColor[0], _newColor[1], _newColor[2]);
                    clone.Dispose();
                    _core.SetSourceImage(c);

                    c.Dispose();
                }

                //desktopScreen = (Bitmap)ReplaceColor((Image)desktopScreen, Color.White, Color.Black, 1);

                if (_binarizeImage)
                {

                    //if user has set brightness and contrast value then change it
                    if (_boxBrightness != -999 && _boxContrast != -999)
                        _core.SetBrightnessContrast(_boxBrightness, _boxContrast);
                    else
                        //otherwise use a value that is ok for almost all application
                        _core.SetBrightnessContrast(-100, 85);
                }

                //get all input boxes that are present in source image
                List<Alexa.Core.Box> boxes;
                if (_binarizeImage)
                {
                    boxes = _core.GetGenericBoxes(_boxHeight, _boxWidth, _boxTollerance);
                }
                else
                {
                    boxes = _core.GetGenericBoxesV2(_boxHeight, _boxWidth, _boxTollerance);
                }
                boxes.Reverse();

                //looking for the label
                int boxCnt = 0;
                foreach (Alexa.Core.Box box in boxes)
                {

                    #region looking for the label
                    //if the user has set a label related to the box, search that label
                    if (_label != "")
                    {
                        //if the user has set the label box height and box width then crop a rectangle to find the label
                        if (_labelBoxHeight != 0 && _labelBoxWidth != 0)
                        {
                            labelImg = CropRect(desktopScreen, new Rectangle(box.x + _labelBoxOffsetX, box.y + _labelBoxOffsetY, _labelBoxWidth, _labelBoxHeight));
                            labelImg = (Bitmap)ResizeImage(labelImg, new Size(labelImg.Width * 3, labelImg.Height * 3));
                        }
                        else //else the user wants to search the label inside the box
                        {
                            labelImg = CropRect(desktopScreen, new Rectangle(box.x, box.y, box.width, box.height));
                            labelImg = (Bitmap)ResizeImage(labelImg, new Size(labelImg.Width * 3, labelImg.Height * 3));
                        }

                        //set Alexa.Core source image
                        SetCoreSourceImage(labelImg);

                        if (_binarizeLabel)
                        {
                            //if user has set brightness and contrast value then change it
                            if (_labelBrightness != -999 && _labelContrast != -999)
                                _core.SetBrightnessContrast(_labelBrightness, _labelContrast);
                            else
                                //otherwise use a value that is ok for almost all application
                                _core.SetBrightnessContrast(-70, 100);

                            //binarize the image
                            _core.BinarizeImage();
                        }

                        #region debug message
                        //save the binarized image if we are in debug
                        if (_debugLogLevel)
                        {
                            _debugImageName = DateTime.Now.ToString(_debugImageDateFormat) + "_GenericBox_Label_" + boxCnt + ".bmp";
                            _core.GetSourceImage().Save(Path.Combine(_debugFullPath, _debugImageName));
                            LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + _debugPath + "\\" + _debugImageName);
                        }
                        #endregion

                        //if the OCR engine has found the label text then click on the input box and insert the text
                        if (checkStringByOCR(_core.GetSourceImage(), _label) == true && _found == false)
                        {

                            //get the coordinates of where we have to click
                            if (_cropRectX != -1 && _cropRectY != -1)
                            {
                                mouseX = box.x + _cropRectX + _clickOffsetX;
                                mouseY = box.y + _cropRectY + _clickOffsetY;
                            }
                            else
                            {
                                mouseX = box.x + _clickOffsetX;
                                mouseY = box.y + _clickOffsetY;
                            }

                            //if this step must update the interrupt region origin point...
                            if (_updateInterruptBindPoint == true && _cropRectX != -1 && _cropRectY != -1)
                            {
                                // ...then update the points
                                _interruptBindPointX = box.x + _cropRectX;
                                _interruptBindPointY = box.y + _cropRectX;
                                _updateInterruptBindPoint = false;
                            }
                            else if (_updateInterruptBindPoint == true)
                            {
                                // ...then update the points
                                _interruptBindPointX = box.x;
                                _interruptBindPointY = box.y;
                                _updateInterruptBindPoint = false;
                            }

                            //if user has set a text to insert into the generic box
                            if (_textToInsert != "")
                                //then click and insert the text
                                ClickAndInsert(mouseX, mouseY, _textToInsert);
                            else
                                //otherwise only click
                                Click(mouseX, mouseY);
                            _found = true;
                            //exit from the foreach loop
                            break;
                        }
                    }
                    //if the user doesn't have set a label related to the box then click on the first generic box
                    //that Alexa.Core has found according to the search option
                    else
                    {
                        //get the coordinates of where we have to click
                        if (_cropRectX != -1 && _cropRectY != -1)
                        {
                            mouseX = box.x + _cropRectX + _clickOffsetX;
                            mouseY = box.y + _cropRectY + _clickOffsetY;
                        }
                        else
                        {
                            mouseX = box.x + _clickOffsetX;
                            mouseY = box.y + _clickOffsetY;
                        }

                        //if this step must update the interrupt region origin point...
                        if (_updateInterruptBindPoint == true && _cropRectX != -1 && _cropRectY != -1)
                        {
                            // ...then update the points
                            _interruptBindPointX = box.x + _cropRectX;
                            _interruptBindPointY = box.y + _cropRectX;
                            _updateInterruptBindPoint = false;
                        }
                        else if (_updateInterruptBindPoint == true)
                        {
                            // ...then update the points
                            _interruptBindPointX = box.x;
                            _interruptBindPointY = box.y;
                            _updateInterruptBindPoint = false;
                        }

                        //if user has set a text to insert into the generic box
                        if (_textToInsert != "")
                            //then click and insert the text
                            ClickAndInsert(mouseX, mouseY, _textToInsert);
                        else
                            //otherwise only click
                            Click(mouseX, mouseY);
                        _found = true;
                        //exit from the foreach loop
                        break;

                    }
                    #endregion
                    boxCnt++;

                    #region check interrupt region image
                    if (_mustCheckInterruptRegion == true && _interruptBindPointX >= 0 && _interruptBindPointY >= 0)
                    {
                        int boxesEquals = 0;
                        List<Core.Box> BindRegionPoints = GetBindRegionPoints();

                        foreach (Alexa.Core.Box oldBox in _BindRegionOldPoints)
                        {
                            foreach (Alexa.Core.Box newBox in BindRegionPoints)
                            {
                                if (oldBox.height == newBox.height && oldBox.width == newBox.width && oldBox.x == newBox.x && oldBox.y == newBox.y)
                                {
                                    boxesEquals++;
                                }
                            }
                        }

                        if (_BindRegionOldPoints.Count < BindRegionPoints.Count)
                        {
                            UpdateRegionPoints();
                            return false;
                        }
                        else if (boxesEquals < _BindRegionOldPoints.Count)
                        {
                            UpdateRegionPoints();
                            return false;
                        }

                        UpdateRegionPoints();
                    }
                    #endregion

                }

                //if the input box was not found save the error message
                if (_found == false)
                {
                    if (_label != "")
                    {
                        //write the error message
                        if (_warningLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Warning, "the generic box (with \"" + _label + "\" as label value) was not found, the step is " + _stepName);
                    }
                    else
                    {
                        //write the error message
                        if (_warningLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Warning, "the generic box was not found, the step is " + _stepName);
                    }
                    //exit from the method and return false
                    return false;
                }
                else return true;
            }
            catch(Exception ex)
            {
                //write the error
                LogUtils.Write(ex);
                Program.Finish(true);
                return false;
            }
            finally
            {
                //release all images
                desktopScreen.Dispose();
                if (labelImg != null) labelImg.Dispose();
            }
        }
        #endregion

        #region execute the "interact" type step on an icon
        /// <summary>
        /// Runs the method that does a click type step on an icon
        /// </summary>
        /// <param name="alexaStep">the xml node that contains the step</param>
        /// <returns>return true if the generic box was found</returns>
        private static bool StepInteractIcon(XmlNode alexaStep)
        {
            //store the icon image
            Bitmap iconImage = null;

            //store a screenshot of the desktop
            Bitmap desktopScreen = ScreenUtils.CaptureDesktop();
            
            //store the icon path
            string iconPath = "";

            //store the threshold, set it lower if the icon must must be very similar with the icon
            //on the Alexa.Core source image.
            double threshold = 0.00001;

            //store the culutre info for the threshold (that is a double type)
            CultureInfo culture = new CultureInfo("en-US");

            int mouseX = 0;
            int mouseY = 0;

            try
            {
                //it is not a mandatory option
                threshold = Double.Parse(alexaStep.Attributes["threshold"].Value,culture);
            }
            catch { }

            try
            {
                if (_debugLogLevel)
                {
                    string imageName = DateTime.Now.ToString(_debugImageDateFormat) + "_DesktopScreenshot.bmp";

                    //save the screenshot of the desktop
                    desktopScreen.Save(Path.Combine(_debugFullPath, imageName));

                    //LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + _debugPath + "\\" + imageName);
                }


                //get the icon image
                iconPath = Path.Combine(_homeFolder,alexaStep.Attributes["path"].Value);
                iconImage = new Bitmap(iconPath);

                //set the Al'exa core source image
                SetCoreSourceImage(desktopScreen);

                Bitmap clone = new Bitmap(iconImage.Width, iconImage.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                using (Graphics gr = Graphics.FromImage(clone))
                {
                    gr.DrawImage(iconImage, new Rectangle(0, 0, clone.Width, clone.Height));
                }

                //find the icon
                Alexa.Core.IconBox iconbox = _core.FindIcon(clone, threshold);

                clone.Dispose();



                if (iconbox.found == true)
                {
                    //if this step must update the interrupt region origin point...
                    if (_updateInterruptBindPoint == true)
                    {
                        // ...then update the points
                        _interruptBindPointX = iconbox.x;
                        _interruptBindPointY = iconbox.y;
                        _updateInterruptBindPoint = false;
                    }

                    //if we are in debug then write 
                    if (_debugLogLevel)
                    {
                        LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + _debugCoreFolder + "iconFound.bmp");
                        LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "a similar icon was found. Icon minval is: " + iconbox.minval);
                    }

                    //get the coordinates of where we have to click
                    mouseX = iconbox.x + _clickOffsetX;
                    mouseY = iconbox.y + _clickOffsetY;
                    #region debug message
                    //if we are in debug
                    if (_debugLogLevel)
                    {
                        LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "icon found on x: " + iconbox.x + " and y: " + iconbox.y + ", step is " + GetStepNameNumber(alexaStep));
                        LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "click on the icon");
                        if (_textToInsert != "") LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "insert text: " + _textToInsert);
                    }
                    #endregion
                    //if user has set a text to insert into the generic box
                    if (_textToInsert != "")
                        //then click and insert the text
                        ClickAndInsert(mouseX, mouseY, _textToInsert);
                    else
                        //otherwise only click
                        Click(mouseX, mouseY);
                }

                //if the icon was not found save the error message
                if (iconbox.found == false)
                {
                    //write the error message
                    if (_warningLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Warning, "the icon was not found (step number " + _stepNumber + "), returned icon minval is: " + iconbox.minval);
                    //exit from the method and return false
                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch(Exception ex)
            {
                //write the error
                LogUtils.Write(ex);
                Program.Finish(true);
                return false;
            }
            finally
            {
                //release all images
                desktopScreen.Dispose();
                if (iconImage != null) iconImage.Dispose();
            }
        }
        #endregion

        #region execute the "interact" type step on a button
        /// <summary>
        /// Runs the method that does a click type step on a button
        /// </summary>
        /// <returns>return true if the button was found</returns>
        private static bool StepInteractButton()
        {
            //store the label images
            Bitmap inside = null;

            //store the position where we want to click
            int mouseX = 0;
            int mouseY = 0;

            //capture desktop image
            Bitmap desktopScreen = ScreenUtils.CaptureDesktop();

            try
            {
                //save the screenshot of the desktop
                if(_debugLogLevel)desktopScreen.Save(Path.Combine(_debugFullPath, DateTime.Now.ToString(_debugImageDateFormat) + "_DesktopScreenshot.bmp"));

                //set the source image for the Alexa.Core
                SetCoreSourceImage(desktopScreen);

                //check if we have to change the color
                if (_oldColor[0] != -1 && _oldColor[1] != -1 && _oldColor[2] != -1 && _newColor[0] != -1 && _newColor[1] != -1 && _newColor[2] != -1)
                {
                    Bitmap clone = new Bitmap(desktopScreen.Width, desktopScreen.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    using (Graphics gr = Graphics.FromImage(clone))
                    {
                        gr.DrawImage(desktopScreen, new Rectangle(0, 0, clone.Width, clone.Height));
                    }
                    Bitmap c = _core.ReplaceColor(clone, _oldColor[0], _oldColor[1], _oldColor[2], _newColor[0], _newColor[1], _newColor[2]);
                    clone.Dispose();
                    _core.SetSourceImage(c);

                    c.Dispose();
                }

                if (_binarizeImage)
                {
                    //if user has set brightness and contrast value then change it
                    if (_boxBrightness != -999 && _boxContrast != -999)
                        _core.SetBrightnessContrast(_boxBrightness, _boxContrast);
                    else
                        //otherwise use a value that is ok for almost all application
                        _core.SetBrightnessContrast(-100, 85);
                }


                //get all buttons that are present into the Alexa.Core source image
                List<Alexa.Core.Box> boxes;
                if (_binarizeImage)
                {
                    boxes = _core.GetButtons();
                }
                else
                {
                    boxes = _core.GetButtonsV2();
                }

                boxes.Reverse();

                //looking for the label
                int boxCnt = 0;
                foreach (Alexa.Core.Box box in boxes)
                {

                    #region check the inside label
                    //crop an image on top of the inputbox
                    inside = CropRect(desktopScreen, new Rectangle(box.x + 3, box.y + 3, box.width-6, box.height-6));
                    inside = (Bitmap)ResizeImage(inside, new Size(inside.Width * 3, inside.Height * 3));
                    //set the source image for the Alexa.Core
                    SetCoreSourceImage(inside);

                    if (_binarizeLabel)
                    {

                        //if user has set brightness and contrast value then change it
                        if (_labelBrightness != -999 && _labelContrast != -999)
                            _core.SetBrightnessContrast(_labelBrightness, _labelContrast);
                        else
                            //otherwise use a value that is ok for almost all application
                            _core.SetBrightnessContrast(-70, 100);

                        //Binarize the Alexa.Core source image
                        _core.BinarizeImage();
                    }

                    #region debug message
                    //save the binarized image if we are in debug
                    if (_debugLogLevel)
                    {
                        _debugImageName = DateTime.Now.ToString(_debugImageDateFormat) + "_Box" + boxCnt + "_LabelPos_Inside.bmp";
                        _core.GetSourceImage().Save(Path.Combine(_debugFullPath, _debugImageName));
                        LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + _debugPath + "\\" + _debugImageName);
                    }
                    #endregion

                    //if the OCR engine has found the label text then click on the input box and insert the text
                    if (checkStringByOCR(_core.GetSourceImage(), _label) == true && _found == false)
                    {
                        //if this step must update the interrupt region origin point...
                        if (_updateInterruptBindPoint == true)
                        {
                            // ...then update the points
                            _interruptBindPointX = box.x;
                            _interruptBindPointY = box.y;
                            _updateInterruptBindPoint = false;
                        }

                        //get the coordinates of where we have to click
                        mouseX = box.x + (box.width / 2) + _clickOffsetX;
                        mouseY = box.y + (box.height / 2) + _clickOffsetY;
                        Click(mouseX, mouseY);
                        _found = true;
                        //exit from the foreach loop
                        break;
                    }
                    #endregion

                    boxCnt++;

                    #region check interrupt region image
                    if (_mustCheckInterruptRegion == true && _interruptBindPointX >= 0 && _interruptBindPointY >= 0)
                    {
                        int boxesEquals = 0;
                        List<Core.Box> BindRegionPoints = GetBindRegionPoints();

                        foreach (Alexa.Core.Box oldBox in _BindRegionOldPoints)
                        {
                            foreach (Alexa.Core.Box newBox in BindRegionPoints)
                            {
                                if (oldBox.height == newBox.height && oldBox.width == newBox.width && oldBox.x == newBox.x && oldBox.y == newBox.y)
                                {
                                    boxesEquals++;
                                }
                            }
                        }

                        if (_BindRegionOldPoints.Count < BindRegionPoints.Count)
                        {
                            UpdateRegionPoints();
                            return false;
                        }
                        else if (boxesEquals < _BindRegionOldPoints.Count)
                        {
                            UpdateRegionPoints();
                            return false;
                        }

                        UpdateRegionPoints();
                    }
                    #endregion
                }

                //if the input box was not found save the error message
                if (_found == false)
                {
                    //write the error message
                    if (_warningLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Warning, "the button \"" + _label + "\" was not found, the step is " + _stepName);
                    //exit from the method and return false
                    return false;
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                //write the error
                LogUtils.Write(ex);
                Program.Finish(true);
                return false;
            }
            finally
            {
                //release all images
                desktopScreen.Dispose();
                if (inside != null) inside.Dispose();
            }
        }
        #endregion

        #region execute the "interact" type step on an IconListMenuItem
        /// <summary>
        /// Runs the method that does a click type step on a List Menu Item
        /// </summary>
        /// <returns>return true if the List Menu Item was found</returns>
        private static bool StepInteractIconListItem()
        {
            //store the label images
            Bitmap left = null;
            Bitmap right = null;

            //store the position where we want to click
            int mouseX = 0;
            int mouseY = 0;

            //capture desktop image
            Bitmap desktopScreen = ScreenUtils.CaptureDesktop();

            try
            {

                //save the screenshot of the desktop
                if (_debugLogLevel) desktopScreen.Save(Path.Combine(_debugFullPath, DateTime.Now.ToString(_debugImageDateFormat) + "_DesktopScreenshot.bmp"));

                //set the source image for the Alexa.Core
                SetCoreSourceImage(desktopScreen);

                //check if we have to change the color
                if (_oldColor[0] != -1 && _oldColor[1] != -1 && _oldColor[2] != -1 && _newColor[0] != -1 && _newColor[1] != -1 && _newColor[2] != -1)
                {
                    Bitmap clone = new Bitmap(desktopScreen.Width, desktopScreen.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    using (Graphics gr = Graphics.FromImage(clone))
                    {
                        gr.DrawImage(desktopScreen, new Rectangle(0, 0, clone.Width, clone.Height));
                    }
                    Bitmap c = _core.ReplaceColor(clone, _oldColor[0], _oldColor[1], _oldColor[2], _newColor[0], _newColor[1], _newColor[2]);
                    clone.Dispose();
                    _core.SetSourceImage(c);

                    c.Dispose();
                }

                if (_binarizeImage)
                {
                    //if user has set brightness and contrast value then change it
                    if (_boxBrightness != -999 && _boxContrast != -999)
                        _core.SetBrightnessContrast(_boxBrightness, _boxContrast);
                    else
                        //otherwise use a value that is ok for almost all application
                        _core.SetBrightnessContrast(-100, 85);
                }


                //get all List Menu Itemes that are present into the Alexa.Core source image
                List<Alexa.Core.Box> boxes;
                if (_binarizeImage)
                {
                    boxes = _core.GetIconListBoxes();
                }
                else
                {
                    boxes = _core.GetIconListBoxesV2();
                }

                boxes.Reverse();

                //looking for the label
                int boxCnt = 0;
                foreach (Alexa.Core.Box box in boxes)
                {

                    #region check the left label
                    if (_labelPosition == "" || _labelPosition == "left")
                    {
                        //if we are in top position and user has set labelBoxHeight
                        if (_labelBoxWidth != 0)
                        {
                            //crop an image on top of the ListMenuItem
                            left = CropRect(desktopScreen, new Rectangle((box.x - _labelBoxWidth), box.y, _labelBoxWidth, box.height));
                            left = (Bitmap)ResizeImage(left, new Size(left.Width * 3, left.Height * 3));
                        }
                        else
                        {
                            //crop an image on top of the ListMenuItem
                            left = CropRect(desktopScreen, new Rectangle((box.x - 160), box.y, 160, box.height));
                            left = (Bitmap)ResizeImage(left, new Size(left.Width * 3, left.Height * 3));
                        }

                        //set the source image for the Alexa.Core
                        SetCoreSourceImage(left);

                        if (_binarizeLabel)
                        {
                            //if user has set brightness and contrast value then change it
                            if (_labelBrightness != -999 && _labelContrast != -999)
                                _core.SetBrightnessContrast(_labelBrightness, _labelContrast);
                            else
                                //otherwise use a value that is ok for almost all application
                                _core.SetBrightnessContrast(-70, 100);

                            //Binarize the Alexa.Core source image
                            _core.BinarizeImage();
                        }

                        #region debug message
                        //save the binarized image if we are in debug
                        if (_debugLogLevel)
                        {
                            _debugImageName = DateTime.Now.ToString(_debugImageDateFormat) + "_Box" + boxCnt + "_LabelPos_Left.bmp";
                            _core.GetSourceImage().Save(Path.Combine(_debugFullPath, _debugImageName));
                            LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + _debugPath + "\\" + _debugImageName);
                        }
                        #endregion

                        //if the OCR engine has found the label text then click on the List Menu Item and insert the text
                        if (checkStringByOCR(_core.GetSourceImage(), _label) == true && _found == false)
                        {
                            //if this step must update the interrupt region origin point...
                            if (_updateInterruptBindPoint == true)
                            {
                                // ...then update the points
                                _interruptBindPointX = box.x;
                                _interruptBindPointY = box.y;
                                _updateInterruptBindPoint = false;
                            }

                            //get the coordinates of where we have to click
                            mouseX = box.x + (box.width / 2) + _clickOffsetX;
                            mouseY = box.y + (box.height / 2) + _clickOffsetY;
                            #region debug message
                            //if we are in debug then write debug message
                            //if (_debugLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "insert text \"" + textToInsert + "\"");
                            #endregion
                            Click(mouseX, mouseY);
                            _found = true;
                            //exit from the foreach loop
                            break;
                        }
                    }
                    #endregion

                    #region check the right label
                    if (_labelPosition == "" || _labelPosition == "right")
                    {
                        if (_labelBoxWidth != 0)
                        {
                            //crop an image on top of the ListMenuItem
                            right = CropRect(desktopScreen, new Rectangle((box.x + box.width), box.y, _labelBoxWidth, box.height));
                            right = (Bitmap)ResizeImage(right, new Size(right.Width * 3, right.Height * 3));
                        }
                        else
                        {
                            //crop an image on top of the ListMenuItem
                            right = CropRect(desktopScreen, new Rectangle((box.x + box.width), box.y, 160, box.height));
                            right = (Bitmap)ResizeImage(right, new Size(right.Width * 3, right.Height * 3));
                        }

                        //set the source image for the Alexa.Core
                        SetCoreSourceImage(right);

                        if (_binarizeLabel)
                        {
                            //if user has set brightness and contrast value then change it
                            if (_labelBrightness != -999 && _labelContrast != -999)
                                _core.SetBrightnessContrast(_labelBrightness, _labelContrast);
                            else
                                //otherwise use a value that is ok for almost all application
                                _core.SetBrightnessContrast(-70, 100);

                            //Binarize the Alexa.Core source image
                            _core.BinarizeImage();
                        }

                        #region debug message
                        //save the binarized image if we are in debug
                        if (_debugLogLevel)
                        {
                            _debugImageName = DateTime.Now.ToString(_debugImageDateFormat) + "_Box" + boxCnt + "_LabelPos_right.bmp";
                            _core.GetSourceImage().Save(Path.Combine(_debugFullPath, _debugImageName));
                            LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + _debugPath + "\\" + _debugImageName);
                        }
                        #endregion

                        //if the OCR engine has found the label text then click on the List Menu Item and insert the text
                        if (checkStringByOCR(_core.GetSourceImage(), _label) == true && _found == false)
                        {
                            //if this step must update the interrupt region origin point...
                            if (_updateInterruptBindPoint == true)
                            {
                                // ...then update the points
                                _interruptBindPointX = box.x;
                                _interruptBindPointY = box.y;
                                _updateInterruptBindPoint = false;
                            }

                            //get the coordinates of where we have to click
                            mouseX = box.x + _clickOffsetX;
                            mouseY = box.y + _clickOffsetY;
                            #region debug message
                            //if we are in debug then write debug message
                            //if (_debugLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "insert text \"" + textToInsert + "\"");
                            #endregion
                            Click(mouseX, mouseY);
                            _found = true;
                            //exit from the foreach loop
                            break;
                        }
                    }
                    #endregion

                    boxCnt++;

                    #region check interrupt region image
                    if (_mustCheckInterruptRegion == true && _interruptBindPointX >= 0 && _interruptBindPointY >= 0)
                    {
                        int boxesEquals = 0;
                        List<Core.Box> BindRegionPoints = GetBindRegionPoints();

                        foreach (Alexa.Core.Box oldBox in _BindRegionOldPoints)
                        {
                            foreach (Alexa.Core.Box newBox in BindRegionPoints)
                            {
                                if (oldBox.height == newBox.height && oldBox.width == newBox.width && oldBox.x == newBox.x && oldBox.y == newBox.y)
                                {
                                    boxesEquals++;
                                }
                            }
                        }

                        if (_BindRegionOldPoints.Count < BindRegionPoints.Count)
                        {
                            UpdateRegionPoints();
                            return false;
                        }
                        else if (boxesEquals < _BindRegionOldPoints.Count)
                        {
                            UpdateRegionPoints();
                            return false;
                        }

                        UpdateRegionPoints();
                    }
                    #endregion
                }

                //if the List Menu Item was not found save the error message
                if (_found == false)
                {
                    //write the error message
                    if (_warningLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Warning, "The Icon List Menu Item \"" + _label + "\" was not found, the step is " + _stepName);
                    //exit from the method and return false
                    return false;
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                //write the error
                LogUtils.Write(ex);
                Program.Finish(true);
                return false;
            }
            finally
            {
                //release all images
                desktopScreen.Dispose();
                if (right != null) right.Dispose();
                if (left != null) left.Dispose();
            }
        }
        #endregion

        #region execute the "interact" type step on a word
        /// <summary>
        /// Runs the method that does a click type step on an input box
        /// </summary>
        /// <returns>return true if the text was found</returns>
        private static bool StepInteractText()
        {
            //store the text image
            Bitmap textImage = null;

            //store the position where we want to click
            int mouseX = 0;
            int mouseY = 0;

            //capture desktop image
            Bitmap desktopScreen = ScreenUtils.CaptureDesktop();

            try
            {
                //if user has set all crop rectangle attributes then crop the rectangle from the desktop screenshot
                if (_cropRectHeight >= 0 && _cropRectWidth >= 0 && _cropRectX >= 0 && _cropRectY >= 0)
                {
                    //Alexa.Core will analyze the cropped image
                    desktopScreen = CropRect(desktopScreen, new Rectangle(_cropRectX, _cropRectY, _cropRectWidth, _cropRectHeight));
                }

                //save the screenshot of the desktop
                if(_debugLogLevel)desktopScreen.Save(Path.Combine(_debugFullPath, DateTime.Now.ToString(_debugImageDateFormat) + "_DesktopScreenshot.bmp"));


                //set the source image for the Alexa.Core
                SetCoreSourceImage(desktopScreen);

                //check if we have to change the color
                if (_oldColor[0] != -1 && _oldColor[1] != -1 && _oldColor[2] != -1 && _newColor[0] != -1 && _newColor[1] != -1 && _newColor[2] != -1)
                {
                    Bitmap clone = new Bitmap(desktopScreen.Width, desktopScreen.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    using (Graphics gr = Graphics.FromImage(clone))
                    {
                        gr.DrawImage(desktopScreen, new Rectangle(0, 0, clone.Width, clone.Height));
                    }
                    Bitmap c = _core.ReplaceColor(clone, _oldColor[0], _oldColor[1], _oldColor[2], _newColor[0], _newColor[1], _newColor[2]);
                    clone.Dispose();
                    _core.SetSourceImage(c);

                    c.Dispose();
                }

                if (_binarizeImage)
                {
                    //if user has set brightness and contrast value then change it
                    if (_boxBrightness != -999 && _boxContrast != -999)
                        _core.SetBrightnessContrast(_boxBrightness, _boxContrast);
                    else
                        //otherwise use a value that is ok for almost all application
                        _core.SetBrightnessContrast(-70, 100);
                }

                //get all chars that are present into the Alexa.Core source image
                List<Alexa.Core.Box> chars;

                //get all words that are present into the Alexa.Core source image.
                //Call this method only after GetChars().
                List<Alexa.Core.Box> words;

                if (_binarizeImage)
                {
                    chars = _core.GetChars(_charRectThickness, _charRectExtendLeft, _charRectColor[0], _charRectColor[1], _charRectColor[2]);
                    words = _core.GetWords(_charRectMinHeight, _charRectMaxHeight, _charRectMinWidth, _charRectMaxWidth);
                }
                else
                {
                    chars = _core.GetCharsV2(_charRectThickness, _charRectExtendLeft, _charRectColor[0], _charRectColor[1], _charRectColor[2]);
                    words = _core.GetWordsV2(_charRectMinHeight, _charRectMaxHeight, _charRectMinWidth, _charRectMaxWidth);
                }

                words.Reverse();

                //looking for the text
                int boxCnt = 0;

                foreach (Alexa.Core.Box box in words)
                {
                    //crop an image on top of the Text
                    textImage = CropRect(desktopScreen, new Rectangle(box.x , box.y, box.width, box.height));
                    textImage = (Bitmap)ResizeImage(textImage, new Size(textImage.Width * 3, textImage.Height * 3));
                    //set the source image for the Alexa.Core
                    SetCoreSourceImage(textImage);

                    if (_binarizeImage)
                    {
                        //if user has set brightness and contrast value then change it
                        if (_boxBrightness != -999 && _boxContrast != -999)
                            _core.SetBrightnessContrast(_boxBrightness, _boxContrast);
                        else
                            //otherwise use a value that is ok for almost all application
                            _core.SetBrightnessContrast(-70, 100);

                        //Binarize the Alexa.Core source image
                        _core.BinarizeImage();
                    }

                    #region debug message
                    //save the binarized image if we are in debug
                    if (_debugLogLevel)
                    {
                        _debugImageName = DateTime.Now.ToString(_debugImageDateFormat) + "_Word_" + boxCnt + ".bmp";
                        _core.GetSourceImage().Save(Path.Combine(_debugFullPath, _debugImageName));
                        LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + _debugPath + "\\" + _debugImageName);
                    }
                    #endregion

                    //check if we have found the text
                    if (checkStringByOCR(_core.GetSourceImage(), _textValue) == true && _found == false)
                    {

                        if (_cropRectX != -1 && _cropRectY != -1)
                        {
                            //get the coordinates of where we have to click
                            mouseX = box.x + _cropRectX + _clickOffsetX;
                            mouseY = box.y + _cropRectY + _clickOffsetY;
                        }
                        else
                        {
                            //get the coordinates of where we have to click
                            mouseX = box.x + _clickOffsetX;
                            mouseY = box.y + _clickOffsetY;
                        }

                        //if this step must update the interrupt region origin point...
                        if (_updateInterruptBindPoint == true && _cropRectX != -1 && _cropRectY != -1)
                        {
                            // ...then update the points
                            _interruptBindPointX = box.x + _cropRectX;
                            _interruptBindPointY = box.y + _cropRectX;
                            _updateInterruptBindPoint = false;
                        }
                        else if (_updateInterruptBindPoint == true)
                        {
                            // ...then update the points
                            _interruptBindPointX = box.x;
                            _interruptBindPointY = box.y;
                            _updateInterruptBindPoint = false;
                        }

                        if (_textToInsert == "") Click(mouseX, mouseY);
                        else
                            ClickAndInsert(mouseX, mouseY, _textToInsert);

                        _found = true;
                        //exit from the foreach loop
                        break;
                    }

                    boxCnt++;

                    #region check interrupt region image
                    if (_mustCheckInterruptRegion == true && _interruptBindPointX >= 0 && _interruptBindPointY >= 0)
                    {
                        int boxesEquals = 0;
                        List<Core.Box> BindRegionPoints = GetBindRegionPoints();

                        foreach (Alexa.Core.Box oldBox in _BindRegionOldPoints)
                        {
                            foreach (Alexa.Core.Box newBox in BindRegionPoints)
                            {
                                if (oldBox.height == newBox.height && oldBox.width == newBox.width && oldBox.x == newBox.x && oldBox.y == newBox.y)
                                {
                                    boxesEquals++;
                                }
                            }
                        }


                        if (_BindRegionOldPoints.Count < BindRegionPoints.Count)
                        {
                            UpdateRegionPoints();
                            return false;
                        }
                        else if(boxesEquals < _BindRegionOldPoints.Count)
                        {
                            UpdateRegionPoints();
                            return false;
                        }

                        UpdateRegionPoints();
                    }
                    #endregion
                }


                //if the input box was not found save the error message
                if (_found == false)
                {
                    //write the error message
                    if (_warningLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Warning, "the text \"" + _textValue + "\" was not found, the step is " + _stepName);
                    //exit from the method and return false
                    return false;
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                //write the error
                LogUtils.Write(ex);
                Program.Finish(true);
                return false;
            }
            finally
            {
                //release all images
                desktopScreen.Dispose();
                if (textImage != null) textImage.Dispose();
            }
        }
        #endregion

        #region execute the "insert" type step
        /// <summary>
        /// Runs the method that insert text
        /// </summary>
        /// <param name="alexaStep">the xml node that contains the step</param>
        private static void InsertText(XmlNode alexaStep)
        {

            try
            {
                //Create a new StepTiming object, it will be used by OutputUtils to write
                //current step info to the output file
                OutputUtils.StepTiming stepTiming = new OutputUtils.StepTiming();

                //save start date
                stepTiming.startTime = DateTime.Now;

                //create a stopwatch to mesea the elapsed time of this step
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                string key = alexaStep.SelectSingleNode("text").InnerText;
                _autoIt.Send(key);

                stopWatch.Stop();

                //save all info that will be used by OutputUtils to write current
                //step info to the output file
                stepTiming.stepNumber = _stepNumber;
                stepTiming.stepNode = alexaStep;
                stepTiming.endTime = DateTime.Now;
                stepTiming.stepDuration = stopWatch.ElapsedMilliseconds;

                //add above info to StepTimingsCollection
                OutputUtils.StepTimingsCollection.Add(stepTiming);
            }
            catch (Exception ex)
            {
                //write the error
                LogUtils.Write(ex);
                Program.Finish(true);
            }
        }
        #endregion

        #region execute the "mousemove" type step
        /// <summary>
        /// Runs the method that insert text
        /// </summary>
        /// <param name="alexaStep">the xml node that contains the step</param>
        private static void StepMouseMove(XmlNode alexaStep)
        {

            try
            {
                //Create a new StepTiming object, it will be used by OutputUtils to write
                //current step info to the output file
                OutputUtils.StepTiming stepTiming = new OutputUtils.StepTiming();

                //save start date
                stepTiming.startTime = DateTime.Now;

                //create a stopwatch to mesea the elapsed time of this step
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                int x = 0;
                int y = 0;
                int speed = 10;

                x = Int32.Parse( alexaStep.Attributes["x"].Value);
                y = Int32.Parse(alexaStep.Attributes["y"].Value);
                try
                {

                    speed = Int32.Parse(alexaStep.Attributes["speed"].Value);
                }
                catch { }
                _autoIt.MouseMove(x,y,speed);

                stopWatch.Stop();

                //save all info that will be used by OutputUtils to write current
                //step info to the output file
                stepTiming.stepNumber = _stepNumber;
                stepTiming.stepNode = alexaStep;
                stepTiming.endTime = DateTime.Now;
                stepTiming.stepDuration = stopWatch.ElapsedMilliseconds;

                //add above info to StepTimingsCollection
                OutputUtils.StepTimingsCollection.Add(stepTiming);
            }
            catch (Exception ex)
            {
                //write the error
                LogUtils.Write(ex);
                Program.Finish(true);
            }
        }
        #endregion

        #region execute the "mouseclick" type step
        /// <summary>
        /// Runs the method that click the mouse
        /// </summary>
        /// <param name="alexaStep">the xml node that contains the step</param>
        private static void StepMouseClick(XmlNode alexaStep)
        {

            try
            {
                //Create a new StepTiming object, it will be used by OutputUtils to write
                //current step info to the output file
                OutputUtils.StepTiming stepTiming = new OutputUtils.StepTiming();

                //save start date
                stepTiming.startTime = DateTime.Now;

                //create a stopwatch to mesea the elapsed time of this step
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                bool doubleClick = false;
                bool rightClick = false;
                int delay = 80;

                try
                {
                    if (alexaStep.Attributes["double.click"].Value == "enable")
                    {
                        doubleClick = true;
                    }
                }
                catch { }

                try
                {
                    if (alexaStep.Attributes["right.click"].Value == "enable")
                    {
                        rightClick = true;
                    }
                }
                catch { }

                try
                {
                    delay = Int32.Parse(alexaStep.Attributes["double.click.delay"].Value);
                }
                catch { }

                
                if(rightClick == true)
                {
                    _autoIt.MouseClick("RIGHT");
                    Thread.Sleep(_afterClickDelay);
                }
                else if (doubleClick)
                {
                    _autoIt.MouseClick("LEFT");
                    Thread.Sleep(delay);
                    _autoIt.MouseClick("LEFT");
                    Thread.Sleep(_afterClickDelay);

                }
                else
                {
                    _autoIt.MouseClick("LEFT");
                    Thread.Sleep(_afterClickDelay);
                }

                stopWatch.Stop();

                //save all info that will be used by OutputUtils to write current
                //step info to the output file
                stepTiming.stepNumber = _stepNumber;
                stepTiming.stepNode = alexaStep;
                stepTiming.endTime = DateTime.Now;
                stepTiming.stepDuration = stopWatch.ElapsedMilliseconds;

                //add above info to StepTimingsCollection
                OutputUtils.StepTimingsCollection.Add(stepTiming);
            }
            catch (Exception ex)
            {
                //write the error
                LogUtils.Write(ex);
                Program.Finish(true);
            }
        }
        #endregion

        #region execute the "delay" type step
        /// <summary>
        /// Runs the method that executes Delay
        /// </summary>
        /// <param name="alexaStep">the xml node that contains the step</param>
        private static void ExecDelay(XmlNode alexaStep)
        {

            try
            {
                //Create a new StepTiming object, it will be used by OutputUtils to write
                //current step info to the output file
                OutputUtils.StepTiming stepTiming = new OutputUtils.StepTiming();

                //save start date
                stepTiming.startTime = DateTime.Now;

                int delay = Int32.Parse(alexaStep.SelectSingleNode("value").InnerText);
                Thread.Sleep(delay);

                //save all info that will be used by OutputUtils to write current
                //step info to the output file
                stepTiming.stepNumber = _stepNumber;
                stepTiming.stepNode = alexaStep;
                stepTiming.endTime = DateTime.Now;
                stepTiming.stepDuration = delay;

                //add above info to StepTimingsCollection
                OutputUtils.StepTimingsCollection.Add(stepTiming);
            }
            catch (Exception ex)
            {
                //write the error
                LogUtils.Write(ex);
                Program.Finish(true);
            }
        }
        #endregion

        #region execute the "configuration" type step
        /// <summary>
        /// Runs the method that can change some option
        /// </summary>
        /// <param name="alexaStep">the xml node that contains the step</param>
        /// <returns>return true if the text was found</returns>
        private static void ChangeStepBehavior(XmlNode alexaStep)
        {

            try
            {

                try
                {
                    //this is not a mandatory option
                    _afterClickDelay = Int32.Parse(alexaStep.SelectSingleNode("mouse").Attributes["click.delay"].Value);
                }
                catch { }

                try
                {
                    //this is not a mandatory option
                    int sendKeyDelay = Int32.Parse(alexaStep.SelectSingleNode("keyboard").Attributes["key.delay"].Value);
                    _autoIt.AutoItSetOption("SendKeyDelay", sendKeyDelay);
                }
                catch { }

                try
                {
                    //this is not a mandatory option
                    if (alexaStep.Attributes["IO.timing"].Value == "disable")
                        _performanceMouseKeyboardEnable = false;
                    else if (alexaStep.Attributes["IO.timing"].Value == "enable")
                        _performanceMouseKeyboardEnable = true;
                }
                catch { _performanceMouseKeyboardEnable = true; }

                try
                {
                    //this is not a mandatory option
                    _ocrLanguageSelected = alexaStep.SelectSingleNode("ocr").Attributes["language"].Value;
                }
                catch{ }
            }
            catch(Exception ex)
            {
                //write the error
                LogUtils.Write(ex);
                Program.Finish(true);
            }
        }
        #endregion

        #region execute the "puttoclipboard" type step
        /// <summary>
        /// Runs the method that will copy text to clipBoard
        /// </summary>
        /// <param name="alexaStep">the xml node that contains the step</param>
        private static void PutToClipboard(XmlNode alexaStep)
        {

            try
            {
                string text = alexaStep.SelectSingleNode("text").InnerText;
                _autoIt.ClipPut(text);
            }
            catch (Exception ex)
            {
                //write the error
                LogUtils.Write(ex);
                Program.Finish(true);
            }
        }
        #endregion

        #region test memory leak
        private static bool TestMemoryLeak()
        {
            desktopScreen2 = ScreenUtils.CaptureDesktop();
            Bitmap icon = new Bitmap("C:\\test_ocr\\start.bmp");

            try
            {
                _core.SetSourceImage(desktopScreen2);
                //_core.SetBrightnessContrast(-100, 85);
                _core.EnableDebug(false);
                _core.SetDebugFolder("c:\\work\\");

                //get all input boxes that are present in source image
                //List<Alexa.Core.Box> boxes = _core.GetGenericBoxes(50,1000,10);
                //List<Alexa.Core.Box> boxes = _core.GetInputBoxes();


                Alexa.Core.IconBox iconbox = _core.FindIcon(icon, 0.05);

                //_core.Release();
                return false;
            }
            catch
            {
                return false;
            }
            finally
            {
                desktopScreen2.Dispose();
                icon.Dispose();
            }

        }
        #endregion

       
        static private void UpdateRegionPoints()
        {
            Bitmap desktopScreen = ScreenUtils.CaptureDesktop();

            //if user has set all crop rectangle attributes then crop the rectangle from the desktop screenshot
            if (_cropInterruptRegionRectHeight >= 0 && _cropInterruptRegionRectWidth >= 0)
            {
                //Alexa.Core will analyze the cropped image
                desktopScreen = CropRect(desktopScreen, new Rectangle(_interruptBindPointX + _cropInterruptRegionRectX, _interruptBindPointY + _cropInterruptRegionRectY, _cropInterruptRegionRectWidth, _cropInterruptRegionRectHeight));
            }

            //set the source image for the core
            SetCoreSourceImage(desktopScreen);

            //if user has set brightness and contrast value then change it
            if (_interruptBrightness != -999 && _interruptContrast != -999)
                _core.SetBrightnessContrast(_interruptBrightness, _interruptContrast);
            else
                //otherwise use a value that is ok for almost all application
                _core.SetBrightnessContrast(-100, 85);

            //get all input boxes that are present in source image
            _BindRegionOldPoints = _core.GetInterestPoints();

            //if (_debugLogLevel) desktopScreen.Save(Path.Combine(_debugFullPath, DateTime.Now.ToString(_debugImageDateFormat) + "_DesktopScreenshot.bmp"));

            desktopScreen.Dispose();

        }

        /// <summary>
        /// Get the points of interest from the core source image
        /// </summary>
        /// <returns>the points of interest</returns>
        static private List<Core.Box> GetBindRegionPoints()
        {
            Bitmap desktopScreen = ScreenUtils.CaptureDesktop();

            //if user has set all crop rectangle attributes then crop the rectangle from the desktop screenshot
            if (_cropInterruptRegionRectHeight >= 0 && _cropInterruptRegionRectWidth >= 0)
            {
                //Alexa.Core will analyze the cropped image
                desktopScreen = CropRect(desktopScreen, new Rectangle(_interruptBindPointX + _cropInterruptRegionRectX, _interruptBindPointY + _cropInterruptRegionRectY, _cropInterruptRegionRectWidth, _cropInterruptRegionRectHeight));
            }

            //set the source image for the core
            SetCoreSourceImage(desktopScreen);

            //if user has set brightness and contrast value then change it
            if (_interruptBrightness != -999 && _interruptContrast != -999)
                _core.SetBrightnessContrast(_interruptBrightness, _interruptContrast);
            else
                //otherwise use a value that is ok for almost all application
                _core.SetBrightnessContrast(-100, 85);

            desktopScreen.Dispose();

            //get all input boxes that are present in source image
            return _core.GetInterestPoints();

        }

        /// <summary>
        /// Move the mouse, click and write text
        /// </summary>
        /// <param name="x">x coordinate of the mouse</param>
        /// <param name="y">y coordinate of the mouse</param>
        /// <param name="text">the text to write</param>
        private static void ClickAndInsert(int x, int y, string text)
        {
            //contains the plain text to insert into the application control
            string plainText = "";

            //try to decrypt the text
            plainText = CryptoUtils.DecryptString(text);

            //if plainText is null then the text isn't crypted
            if (plainText == null) plainText = text;

            //start stopwatch to save the time elapsed to move the mouse 
            //and digit the keys
            _otherDelayTime.Reset();
            _otherDelayTime.Start();

            if (_mouseClick == true && _mouseMove == true)
            {
                //if we are in debug then write debug message
                if (_debugLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug,
                    "click on x: " + x + ", y: " + y);

                //AutoIT mouse click function
                _autoIt.MouseClick("left", x, y);

                Thread.Sleep(_afterClickDelay);

                //AutoIT send text function
                _autoIt.Send(plainText);
            }
            else if (_mouseClick == false && _mouseMove == true)
            {
                //if we are in debug then write debug message
                if (_debugLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug,
                    "move on x: " + x + ", y: " + y);

                //AutoIT mouse move function
                _autoIt.MouseMove(x, y);

                _mouseClick = true;
            }
            else if (_mouseClick == true && _mouseMove == false)
            {
                //if we are in debug then write debug message
                if (_debugLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug,
                    "click on previous coordinates");

                //AutoIT mouse move function
                _autoIt.MouseClick("left");
                Thread.Sleep(_afterClickDelay);

                _mouseMove = true;
            }
            else
            {
                _mouseClick = true;
                _mouseMove = true;
            }

            //if we are in debug then write debug message
            if (_debugLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, 
                "click on x: " + x + ", y: " + y + " and insert text \"" + text + "\"");

            //stop the stopwatcher
            _otherDelayTime.Stop();
        }

        /// <summary>
        /// Move the mouse, click and write text
        /// </summary>
        /// <param name="x">x coordinate of the mouse</param>
        /// <param name="y">y coordinate of the mouse</param>
        /// <param name="text">the text to write</param>
        private static void SelectListItem(int x, int y, string text)
        {
            //start stopwatch to save the time elapsed to move the mouse 
            //and digit the keys
            _otherDelayTime.Reset();
            _otherDelayTime.Start();

            //if we are in debug then write debug message
            if (_debugLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug,
                "click on x: " + x + ", y: " + y + " and select item \"" + text + "\"");

            //AutoIT mouse click function
            _autoIt.MouseClick("left", x, y);
            //AutoIT send text function
            Thread.Sleep(_afterClickDelay);
            _autoIt.MouseClick("left", x, y);
            Thread.Sleep(_afterClickDelay);
            _autoIt.Send(text);

            //stop the stopwatcher
            _otherDelayTime.Stop();
        }

        /// <summary>
        /// Move the mouse and click
        /// </summary>
        /// <param name="x">x coordinate of the mouse</param>
        /// <param name="y">y coordinate of the mouse</param>
        private static void Click(int x, int y)
        {
            //start stopwatch to save the time elapsed to move the mouse
            _otherDelayTime.Reset();
            _otherDelayTime.Start();

            if (_mouseClick == true && _mouseMove == true)
            {
                //if we are in debug then write debug message
                if (_debugLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug,
                    "click on x: " + x + ", y: " + y);

                //AutoIT mouse click function
                _autoIt.MouseClick("left", x, y);
                Thread.Sleep(_afterClickDelay);

            }
            else if (_mouseClick == false && _mouseMove == true)
            {
                //if we are in debug then write debug message
                if (_debugLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug,
                    "move on x: " + x + ", y: " + y);

                //AutoIT mouse move function
                _autoIt.MouseMove(x, y);

                _mouseClick = true;
            }
            else if (_mouseClick == true && _mouseMove == false)
            {
                //if we are in debug then write debug message
                if (_debugLogLevel) LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug,
                    "click on previous coordinates");

                //AutoIT mouse move function
                _autoIt.MouseClick("left");
                Thread.Sleep(_afterClickDelay);

                _mouseMove = true;
            }
            else
            {
                _mouseClick = true;
                _mouseMove = true;
            }

            //stop the stopwatcher
            _otherDelayTime.Stop();
        }

        /// <summary>
        /// Crops a portion of the image
        /// </summary>
        /// <param name="original">the original image</param>
        /// <param name="rect">the rectangle to crop</param>
        /// <returns>return the cropped image</returns>
        private static Bitmap CropRect(Bitmap original, Rectangle rect)
        {
            int rectX = rect.X;
            int rectWidth = rect.Width;
            if (rectX < 0)
            {
                rectWidth = rectWidth + rectX;
                rectX = 0;
            }

            if (rectX + rectWidth > SystemUtils.ScreenWidth)
            {
                rectWidth = SystemUtils.ScreenWidth - rectX - 5;
                if (rectWidth <= 0) rectWidth = 1;
            }

            Bitmap cutted = new Bitmap(rectWidth, rect.Height);
            Graphics graphic = Graphics.FromImage(cutted);
            graphic.DrawImage(original, -rectX, -rect.Y);
            graphic.Dispose();
            return cutted;
        }
        //private static Bitmap CropRect(Bitmap original, Rectangle rect)
        //{
        //    Bitmap cutted = new Bitmap(rect.Width, rect.Height);
        //    Graphics graphic = Graphics.FromImage(cutted);
        //    graphic.DrawImage(original, -rect.X, -rect.Y);
        //    graphic.Dispose();
        //    return cutted;
        //}


        /// <summary>
        /// Resize an image
        /// </summary>
        /// <param name="image">the original image</param>
        /// <param name="size">the new size</param>
        /// <returns>return the resized image</returns>
        private static Image ResizeImage(Image image, Size size)
        {
            int newWidth = size.Width;
            int newHeight = size.Height;

            Image newImage = new Bitmap(size.Width, size.Height);
            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            if (_binarizeLabel == true || _binarizeImage == true)
            {
                newImage = CropRect((Bitmap)newImage,new Rectangle(0,0,newWidth,newHeight-2));
            }

            return newImage;
        }

        /// <summary>
        /// Search a string into an image
        /// </summary>
        /// <param name="image">the image</param>
        /// <param name="inputString">the string to search</param>
        /// <returns>return true if the string was found</returns>
        private static bool checkStringByOCR(Bitmap image, string inputString)
        {
            Stopwatch a = new Stopwatch();
            a.Start();

            using (var bmp = image)
            {
                try
                {
                    //init the OCR engine, I use tessaract as OCR engine.
                    TesseractProcessor processor = new TesseractProcessor();
                    //verify if tessaract has been successfully loaded
                    var success = processor.Init(_ocrLangData, _ocrLanguageSelected, (int)eOcrEngineMode.OEM_DEFAULT);
                    if (!success)
                    {
                        //if tessaract has not been successfully loaded then write the error
                        LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Error, "Failed to start OCR engine");
                        Program.Finish(true);
                        return false;
                    }
                    else
                    {
                        //extract the string from the image
                        string textInImage = processor.Recognize(bmp);
                        //textInImage = textInImage.Replace('\n', ' ');
                        //textInImage = textInImage.Replace('\r', ' ');

                        //write a debug message
                        if (_debugLogLevel)
                            LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "text found: " + textInImage);

                        //check if the string extracted from the image is equal to the string that we want to find.
                        //NOTE that tesseract occasionally swaps a letter with another. For example, it may change
                        //an "m" with two letters "rm" or an "l" with a "|" and so on. So i try to solve some of these errors.
                        //You can set a debug level on the log option and search into the log file the string "text found: " and
                        //then see text that tesseract has found
                        if (Regex.IsMatch(textInImage, inputString) ||
                            Regex.IsMatch(textInImage.Replace("m", "rn"), inputString) || Regex.IsMatch(textInImage.Replace("rn", "m"), inputString) ||
                            Regex.IsMatch(textInImage.Replace("l", "1"), inputString) || Regex.IsMatch(textInImage.Replace("1", "l"), inputString) ||
                            Regex.IsMatch(textInImage.Replace("l", "i"), inputString) || Regex.IsMatch(textInImage.Replace("i", "l"), inputString) ||
                            Regex.IsMatch(textInImage.Replace("t", "l"), inputString) || Regex.IsMatch(textInImage.Replace("l", "t"), inputString) ||
                            Regex.IsMatch(textInImage.Replace("1", "i"), inputString) || Regex.IsMatch(textInImage.Replace("i", "1"), inputString) ||
                            Regex.IsMatch(textInImage.Replace("I", "l"), inputString) || Regex.IsMatch(textInImage.Replace("l", "I"), inputString) ||
                            Regex.IsMatch(textInImage.Replace("I", "1"), inputString) || Regex.IsMatch(textInImage.Replace("1", "I"), inputString) ||
                            Regex.IsMatch(textInImage.Replace("I", "t"), inputString) || Regex.IsMatch(textInImage.Replace("t", "I"), inputString) ||
                            Regex.IsMatch(textInImage.Replace("0", "o"), inputString) || Regex.IsMatch(textInImage.Replace("o", "0"), inputString) ||
                            Regex.IsMatch(textInImage.Replace("0", "O"), inputString) || Regex.IsMatch(textInImage.Replace("O", "0"), inputString) ||
                            Regex.IsMatch(textInImage.Replace("l", "|"), inputString) || Regex.IsMatch(textInImage.Replace("|", "l"), inputString) ||
                            Regex.IsMatch(textInImage.Replace("I", "|"), inputString) || Regex.IsMatch(textInImage.Replace("|", "I"), inputString) ||
                            Regex.IsMatch(textInImage.Replace("t", "|"), inputString) || Regex.IsMatch(textInImage.Replace("|", "t"), inputString) ||
                            Regex.IsMatch(textInImage.Replace("i", "|"), inputString) || Regex.IsMatch(textInImage.Replace("|", "i"), inputString) ||
                            Regex.IsMatch(textInImage.Replace("M", "II"), inputString) || Regex.IsMatch(textInImage.Replace("II", "M"), inputString) ||
                            Regex.IsMatch(textInImage.Replace("ni", "m"), inputString) || Regex.IsMatch(textInImage.Replace("m", "ni"), inputString))
                        {

                            a.Stop();
                            return true;
                        }
                        else
                            return false;
                    }
                }
                catch (Exception ex)
                {
                    //write the error
                    LogUtils.Write(ex);
                    Program.Finish(true);
                    return false;
                }
                
            }
        }

        /// <summary>
        /// Runs the method that executes a step and then calculate the execution time
        /// </summary>
        /// <param name="stepType">the type of the step</param>
        /// <param name="alexaStep">the xml node that contains the step</param>
        private static void ExecStepMethod(StepType stepType, XmlNode alexaStep)
        {
            long stepDuration = 0;
            int stepTimeout = 30000; //default timeout is 30 seconds

            //Init a Stopwatch to measure elapsed time of a method that executes a single step
            Stopwatch stepExecutionTime = new Stopwatch();

            //Create a new StepTiming object, it will be used by OutputUtils to write
            //current step info to the output file
            OutputUtils.StepTiming stepTiming = new OutputUtils.StepTiming();

            #region common private variables
            _mouseClick = true;
            _mouseMove = true;

            //store debug properties
            _debugFullPath = "";
            _debugPath = "";
            _debugImageName = "";
            _debugCoreFolder = "";

            //flag that indicates if the input box was found
            _found = false;

            //step configuration properties
            _stepName = "";
            _labelPosition = "";
            _label = "";
            _textToInsert = "";
            _boxBrightness = -999; //default value, if it doesn't change I don't change brightness and contrast
            _boxContrast = -999;
            _labelBrightness = -999;
            _labelContrast = -999;

            //offset coordinate, the origin point of these coordinats
            //are the top left point of the box that will be found
            _clickOffsetX = 0;
            _clickOffsetY = 0;

            //store the properties of the label related to the inputbox 
            //that the user wants find
            _labelBoxHeight = 0;
            _labelBoxWidth = 0;

            //store the properties of the box that the user wants find
            _boxHeight = -1;
            _boxWidth = -1;
            _boxTollerance = 0;

            //label offset, the origin point of these coordinats
            //are the top left point of the box that will be found
            _labelBoxOffsetX = 0;
            _labelBoxOffsetY = 0;

            //cotains the element to select from a drop down list
            _selectItem = "";

            //they will contain the desktop area to crop
            //if user doesn't set these options then all desktop image will be analyzed
            _cropRectX = -1;
            _cropRectY = -1;
            _cropRectHeight = -1;
            _cropRectWidth = -1;

            //char bounding rect properties
            _charRectMinHeight = 6;
            _charRectMaxHeight = 50;
            _charRectMinWidth = 50;
            _charRectMaxWidth = 300;
            _charRectThickness = 2;
            _charRectExtendLeft = 2;
            _charRectColor[0] = 255;
            _charRectColor[1] = _charRectColor[2] = 0;

            //word text
            _textValue = "";

            _performanceEnable = false;

            //variables for scroll
            _scrollStep = 1;
            _enableScroll = false;
            _scrollDirection = "down";
            _scrollLastdelay = 2000;

            //variables for the color change
            //   Red           Blue          Green
            _oldColor[0] = _oldColor[1] = _oldColor[2] = -1;
            _newColor[0] = _newColor[1] = _newColor[2] = -1;

            #endregion

            #region get step node attributes
            //get the timeout attribute, it isn't mandatory.
            try { stepTimeout = Int32.Parse(alexaStep.SelectSingleNode("performance").Attributes["timeout.value"].Value.ToLower()); }
            catch { }

            try
            {
                //this is not a mandatory option
                if (alexaStep.Attributes["mouse.click"].Value == "off") _mouseClick = false;
            }
            catch { }

            try
            {
                //this is not a mandatory option
                if (alexaStep.Attributes["mouse.move"].Value == "off") _mouseMove = false;
            }
            catch { }

            try //get the step name attribute, it isn't mandatory.
            {
                _stepName = GetStepNameNumber(alexaStep);
            }
            catch { }

            try
            {
                //get the label position attribute, it isn't mandatory.
                _labelPosition = alexaStep.SelectSingleNode("label").Attributes["position"].Value;
            }
            catch { }

            try
            {
                //this is not a mandatory option
                _boxBrightness = Int32.Parse(alexaStep.Attributes["brightness"].Value);
            }
            catch { _boxBrightness = -999; }

            try
            {
                //this is not a mandatory option
                _boxContrast = Int32.Parse(alexaStep.Attributes["contrast"].Value);
            }
            catch { _boxContrast = -999; }

            try
            {
                //this is not a mandatory option
                _labelBrightness = Int32.Parse(alexaStep.SelectSingleNode("label").Attributes["brightness"].Value);
            }
            catch { _labelBrightness = -999; }

            try
            {
                //this is not a mandatory option
                _labelContrast = Int32.Parse(alexaStep.SelectSingleNode("label").Attributes["contrast"].Value);
            }
            catch { _labelContrast = -999; }

            try
            {
                //it isn't a mandatory option
                _labelBoxHeight = Int32.Parse(alexaStep.SelectSingleNode("label").Attributes["height"].Value);
            }
            catch { }

            try
            {
                //it isn't a mandatory option
                _labelBoxWidth = Int32.Parse(alexaStep.SelectSingleNode("label").Attributes["width"].Value);
            }
            catch { }

            
            try
            {
                //get the label of the input box
                _label = alexaStep.SelectSingleNode("label").InnerText;
            }
            catch{}

            try
            {

                //get the text to insert into the input box
                _textToInsert = alexaStep.SelectSingleNode("insert").InnerText;
            }
            catch{}

            try
            {
                //get the text to insert into the Drop Down List
                _selectItem = alexaStep.SelectSingleNode("select").InnerText;
            }
            catch { }

            try
            {
                //they are not mandatory options
                _cropRectX = Int32.Parse(alexaStep.Attributes["croprect.x"].Value);
                _cropRectY = Int32.Parse(alexaStep.Attributes["croprect.y"].Value);
                _cropRectHeight = Int32.Parse(alexaStep.Attributes["croprect.height"].Value);
                _cropRectWidth = Int32.Parse(alexaStep.Attributes["croprect.width"].Value);
            }
            catch
            {
                _cropRectX = _cropRectY = _cropRectHeight = _cropRectWidth = -1;
            }

            try
            {
                _boxHeight = Int32.Parse(alexaStep.Attributes["height"].Value);
            }
            catch { }

            try
            {
                _boxWidth = Int32.Parse(alexaStep.Attributes["width"].Value);
            }
            catch { }

            try
            {
                _boxTollerance = Int32.Parse(alexaStep.Attributes["tollerance"].Value);
            }
            catch { }

            try
            {
                _labelBoxOffsetX = Int32.Parse(alexaStep.SelectSingleNode("label").Attributes["offset.x"].Value);
            }
            catch { }

            try
            {
                _labelBoxOffsetY = Int32.Parse(alexaStep.SelectSingleNode("label").Attributes["offset.y"].Value);
            }
            catch { }

            try
            {
                //it is not a mandatory options
                _clickOffsetX = Int32.Parse(alexaStep.Attributes["click.add.x"].Value);
            }
            catch { }

            try
            {
                //it is not a mandatory options
                _clickOffsetY = Int32.Parse(alexaStep.Attributes["click.add.y"].Value);
            }
            catch { }

            try
            {
                //they are not a mandatory options
                _charRectThickness = Int32.Parse(alexaStep.Attributes["rectbound.thickness"].Value);
            }
            catch { }

            try
            {
                //they are not a mandatory options
                _charRectExtendLeft = Int32.Parse(alexaStep.Attributes["rectbound.extend.left"].Value);
            }
            catch { }

            try
            {
                //they are not a mandatory options
                _charRectMinHeight = Int32.Parse(alexaStep.Attributes["rectbound.min.height"].Value);
                _charRectMaxHeight = Int32.Parse(alexaStep.Attributes["rectbound.max.height"].Value) + (_charRectThickness * 2);
                _charRectMinWidth = Int32.Parse(alexaStep.Attributes["rectbound.min.width"].Value);
                _charRectMaxWidth = Int32.Parse(alexaStep.Attributes["rectbound.max.width"].Value) + (_charRectThickness * 2);
            }
            catch { }

            try
            {
                string rectColor = alexaStep.Attributes["rectbound.color"].Value.Replace(" ", "");
                rectColor = rectColor.Replace("(", "");
                rectColor = rectColor.Replace(")", "");

                string[] colorArr = rectColor.Split(',');

                _charRectColor[0] = Int32.Parse(colorArr[0]);
                _charRectColor[1] = Int32.Parse(colorArr[1]);
                _charRectColor[2] = Int32.Parse(colorArr[2]);
            }
            catch { }

            try
            {
                _textValue = alexaStep.SelectSingleNode("text").InnerText;
            }
            catch { }

            if (alexaStep.SelectSingleNode("performance") != null) _performanceEnable = true;

            try
            {
                //they are not mandatory options
                _cropInterruptRegionRectX = Int32.Parse(alexaStep.SelectSingleNode("interrupt").Attributes["region.x"].Value);
                _cropInterruptRegionRectY = Int32.Parse(alexaStep.SelectSingleNode("interrupt").Attributes["region.y"].Value);
                _cropInterruptRegionRectHeight = Int32.Parse(alexaStep.SelectSingleNode("interrupt").Attributes["region.height"].Value);
                _cropInterruptRegionRectWidth = Int32.Parse(alexaStep.SelectSingleNode("interrupt").Attributes["region.width"].Value);
                _updateInterruptBindPoint = true;
            }
            catch
            {
            }

            try
            {
                //it's are not a mandatory options. It turns off interrupt on region
                if (alexaStep.SelectSingleNode("interrupt").Attributes["enable"].Value == "yes" && _performanceEnable == true) _mustCheckInterruptRegion = true;
                else
                    _mustCheckInterruptRegion = false;
            }
            catch { _mustCheckInterruptRegion = false; }

            try
            {
                //this is not a mandatory option
                _interruptBrightness = Int32.Parse(alexaStep.SelectSingleNode("interrupt").Attributes["brightness"].Value);
            }
            catch { }

            try
            {
                //this is not a mandatory option
                _interruptContrast = Int32.Parse(alexaStep.SelectSingleNode("interrupt").Attributes["contrast"].Value);
            }
            catch { }

            try
            {
                //this is not a mandatory option
                if (alexaStep.Attributes["scroll.enable"].Value == "yes")
                    _enableScroll = true;
            }
            catch { }

            try
            {
                //this is not a mandatory option
                _scrollStep = Int32.Parse(alexaStep.Attributes["scroll.step"].Value);
            }
            catch { }

            try
            {
                //this is not a mandatory option
                _scrollDirection = alexaStep.Attributes["scroll.direction"].Value;
            }
            catch { }

            try
            {
                //this is not a mandatory option
                _scrollLastdelay = Int32.Parse(alexaStep.Attributes["scroll.lastdelay"].Value);
            }
            catch { }

            try
            {
                string replaceColor = alexaStep.Attributes["color.replace"].Value.Replace(" ","");
                string oldColorStr = replaceColor.Split(')')[0];
                oldColorStr = oldColorStr.Replace("((", "");
                string[] oldColorArr = oldColorStr.Split(',');

                _oldColor[0] = Int32.Parse(oldColorArr[0]);
                _oldColor[1] = Int32.Parse(oldColorArr[1]);
                _oldColor[2] = Int32.Parse(oldColorArr[2]);

                string newColorStr = oldColorStr = replaceColor.Split(')')[1];
                newColorStr = newColorStr.Replace(",(", "");
                string[] newColorArr = newColorStr.Split(',');

                _newColor[0] = Int32.Parse(newColorArr[0]);
                _newColor[1] = Int32.Parse(newColorArr[1]);
                _newColor[2] = Int32.Parse(newColorArr[2]);
            }
            catch { }

            try
            {
                //check if we have to binarize the image
                if (alexaStep.Attributes["binarize"].Value == "yes")
                    _binarizeImage = true;
                else
                    _binarizeImage = false;
            }
            catch { _binarizeImage = false; }

            try
            {
                //check if we have to binarize the label
                if (alexaStep.SelectSingleNode("label").Attributes["binarize"].Value == "yes")
                    _binarizeLabel = true;
                else
                    _binarizeLabel = false;
            }
            catch { _binarizeLabel = false; }

            #endregion

            #region handle configuration error
            //write the error
            if (_textToInsert == "" && stepType == StepType.InteractInputBox && _mouseClick == true && _mouseMove == true)
            {
                LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Error, "You have to add the node \"insert\" on a \"interact\" step with \"inputbox\" as bind value. The step is " + GetStepNameNumber(alexaStep));
                Program.Finish(true);
            }

            //write the error
            if (_label == "" && (stepType == StepType.InteractInputBox || stepType == StepType.InteractButton || stepType == StepType.InteractDropDownList || stepType == StepType.InteractIconList))
            {
                LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Error, "You have to add the node \"label\" on a \"interact\" step with \"inputbox\" (or \"dropdownlist\" or \"button\" or \"iconlist\") as bind value. The step is " + GetStepNameNumber(alexaStep));
                Program.Finish(true);
            }

            //write the error
            if (_selectItem == "" && stepType == StepType.InteractDropDownList)
            {
                LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Error, "You have to add the node \"select\" on a \"interact\" step with \"dropdownlist\" as bind value. The step is " + GetStepNameNumber(alexaStep));
                Program.Finish(true);
            }


            //write the error
            if ((_boxHeight == -1 || _boxWidth == -1) && (stepType == StepType.InteractGenericBox))
            {
                LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Error, "You have to add the attributes \"height\" and \"width\" on a \"interact\" step with \"genericbox\" as bind value. The step is " + GetStepNameNumber(alexaStep));
                Program.Finish(true);
            }

            //write the error
            if (_textValue == "" && stepType == StepType.InteractText)
            {
                LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Error, "You have to add the node \"text\" on a \"interact\" step with \"word\" as bind value. The step is " + GetStepNameNumber(alexaStep));
                Program.Finish(true);
            }
            #endregion

            if (_enableScroll)
            {
                _mouseClick = false;
                _mouseMove = false;
            }

            //reset stopwatch
            _stepTime.Reset();
            //start stopwatch
            _stepTime.Start();

            stepTiming.startTime = DateTime.Now;

            while (true) //waits until we will find the input box or a timeout will occur
            {
                //reset stopwatch
                stepExecutionTime.Reset();
                //start stopwatch
                stepExecutionTime.Start();


                //if debug is active
                if (_debugLogLevel && stepType != StepType.RunExe)
                {
                    //get the log folder path for this step
                    _debugPath = Path.Combine(_debugHomeFolder, DateTime.Now.ToString(_debugFolderDateFormat) + "_Step_" + RemoveIllegalChars(_stepName.Replace(" ", "_")));

                    _debugFullPath = _debugPath;

                    //get the full path (otherwise the method Image.Save does not work)
                    DirectoryInfo dir = new DirectoryInfo(_debugFullPath);
                    _debugFullPath = dir.FullName;

                    //if the directory doesn't exist then create it
                    if (!Directory.Exists(dir.FullName)) dir.Create();

                    //set the debug folder for the Alexa.Core
                    _debugCoreFolder = Path.Combine(_debugFullPath, DateTime.Now.ToString(_debugImageDateFormat) + "_");
                    _core.SetDebugFolder(_debugCoreFolder);
                }

                //check what method we have to execute and then execute it
                if (stepType == StepType.InteractInputBox && StepInteractInputBox() == true ||
                    stepType == StepType.InteractGenericBox && StepInteractGenericBox() == true ||
                    stepType == StepType.InteractIcon && StepInteractIcon(alexaStep) == true ||
                    stepType == StepType.RunExe && StepRunExe(alexaStep) == true ||
                    stepType == StepType.InteractButton && StepInteractButton() == true ||
                    stepType == StepType.InteractIconList && StepInteractIconListItem() == true ||
                    stepType == StepType.InteractText && StepInteractText() == true ||
                    stepType == StepType.InteractDropDownList && StepInteractDropDownList() == true)
                {

                    if (_enableScroll)
                    {
                        Thread.Sleep(_scrollLastdelay);
                        _mouseClick = true;
                        _mouseMove = true;
                        _found = false;
                        if (stepType == StepType.InteractInputBox) StepInteractInputBox();
                        if (stepType == StepType.InteractGenericBox) StepInteractGenericBox();
                        if (stepType == StepType.InteractIcon) StepInteractIcon(alexaStep);
                        if (stepType == StepType.InteractButton) StepInteractButton();
                        if (stepType == StepType.InteractIconList) StepInteractIconListItem();
                        if (stepType == StepType.InteractText) StepInteractText();
                        if (stepType == StepType.InteractDropDownList) StepInteractDropDownList();
                    }

                    //stop all stopwatch
                    stepExecutionTime.Stop();
                    _stepTime.Stop();

                    //Save the execution time!!!
                    //How do I make the measurement of the time may seems strange, but I have to measure only the time taken by the control to appear
                    //WITHOUT THE TIME THAT THE Alexa.Core TAKE TO ANALYZE THE IMAGES. So, this is the way that I could find to measure the time
                    //efficiently.
                    if (_performanceMouseKeyboardEnable)
                        stepDuration = _stepTime.ElapsedMilliseconds - stepExecutionTime.ElapsedMilliseconds + _otherDelayTime.ElapsedMilliseconds;
                    else
                        stepDuration = _stepTime.ElapsedMilliseconds - stepExecutionTime.ElapsedMilliseconds;

                    //save all info that will be used by OutputUtils to write current
                    //step info to the output file
                    stepTiming.stepNumber = _stepNumber;
                    stepTiming.stepNode = alexaStep;
                    stepTiming.endTime = DateTime.Now;
                    stepTiming.stepDuration = stepDuration;

                    //add above info to StepTimingsCollection
                    OutputUtils.StepTimingsCollection.Add(stepTiming);

                    //LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "step " + _stepName + " method duration is: " + stepExecutionTime.ElapsedMilliseconds + "ms");

                    break;
                }

                //move mouse wheel
                if (_enableScroll) _autoIt.MouseWheel(_scrollDirection,_scrollStep);

                //stop the timer and save the execution time of the method that has execute the step
                stepExecutionTime.Stop();

                //LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "step " + _stepName + " method duration is: " + stepExecutionTime.ElapsedMilliseconds + "ms");

                if (_performanceEnable)
                {
                    //if a timeout has occurred...
                    if (_stepTime.ElapsedMilliseconds >= stepTimeout)
                    {

                        //set to false in case of next run
                        _executableStarted = false;

                        // ...write the error message
                        LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Error, "a timeout has occurred with the step number " + _stepNumber);

                        //save all info that will be used by OutputUtils to write current
                        //step info to the output file
                        stepTiming.stepNumber = _stepNumber;
                        stepTiming.stepNode = alexaStep;
                        stepTiming.endTime = DateTime.Now;
                        stepTiming.stepDuration = _stepTime.ElapsedMilliseconds;


                        //add above info to StepTimingsCollection
                        OutputUtils.StepTimingsCollection.Add(stepTiming);

                        //save the screenshot of the error
                        SaveScreenshotAfterTimeOut(alexaStep);

                        //Analyze all windows to find the control
                        if (AnalyzeWindow(stepType, alexaStep) == true) return;

                        try //check if we have to Jump to another step or if we have to execute a specific function
                        {
                            //Get timout action
                            string timeOutAction = alexaStep.SelectSingleNode("performance").Attributes["timeout.action"].Value;

                            //if user has defined a "continue" action, then break current loop so
                            //we can execute another step
                            if (timeOutAction.ToLower() == "continue") break;

                            //if user has defined a "break" action, then call the "Finish" method with "true" argument
                            //in this way the program will write the output file and then exit
                            if (timeOutAction.ToLower() == "break") Program.Finish(false);


                            //if we are here then we have to execute a function or a jump
                            //so, first of all check if we have to execute a jump
                            timeOutAction = timeOutAction.Replace("Jump(", "");
                            timeOutAction = timeOutAction.Replace(")", "");

                            //check if we have to jump to another step by its name
                            if (timeOutAction.IndexOf("step.name") != -1)
                            {
                                timeOutAction = timeOutAction.Replace("step.name='", "");
                                JumpToStep.stepName = timeOutAction.Remove(timeOutAction.Length - 1);
                                break;
                            }
                            //check if we have to jump to another step by its number
                            else if (timeOutAction.IndexOf("step.number") != -1)
                            {
                                timeOutAction = timeOutAction.Replace("step.number=", "");
                                JumpToStep.stepNumber = Int32.Parse(timeOutAction);
                                break;
                            }

                            //if we are here then we have to execute a function instead of a jump
                            timeOutAction = alexaStep.SelectSingleNode("performance").Attributes["timeout.action"].Value;
                            timeOutAction = timeOutAction.Replace("Exec(", "");
                            timeOutAction = timeOutAction.Replace(")", "");

                            //eseguire la funzione e brekkare
                            if (timeOutAction.IndexOf("action.name") != -1)
                            {
                                timeOutAction = timeOutAction.Replace("action.name='", "");
                                break;
                            }

                        }
                        //if an exception has occurred then we don't have to jump to another step or execute a function
                        catch
                        {
                            JumpToStep.stepName = "";
                            JumpToStep.stepNumber = 0;
                            break;
                        }

                    }
                }
            }

        }

        /// <summary>
        /// Runs the method that executes a step and then calculate the execution time
        /// </summary>
        /// <param name="actionName">the name of the action</param>
        /// <param name="stepType">the type of the step</param>
        /// <param name="alexaStep">the xml node that contains the step</param>
        private static void ExecActionMethod(string actionName, StepType stepType, XmlNode alexaStep)
        {
            XmlNode actionNode = ConfigUtils.GetAction(actionName);
        }

        /// <summary>
        /// Takes a screenshot of desktop if timeout has occurred
        /// </summary>
        /// <param name="alexaStep">the xml node that contains the step</param>
        private static void SaveScreenshotAfterTimeOut(XmlNode alexaStep)
        {
            if (ConfigUtils.LogIsEnabled == true)
            {
                sendEmail = true;

                DirectoryInfo dir = new DirectoryInfo(Path.Combine(_debugHomeFolder, @"..\Error_Screenshots"));
                if (!Directory.Exists(dir.FullName)) dir.Create();
                Bitmap desktopScreen;
                //capture desktop image
                desktopScreen = ScreenUtils.CaptureDesktop();
                
                //--
                var newImage = new Bitmap(desktopScreen.Width, desktopScreen.Height + 50);

                var gr = Graphics.FromImage(newImage);
                gr.FillRectangle(new SolidBrush(Color.Black), 0, 0, desktopScreen.Width, desktopScreen.Height + 50);

                gr.DrawImageUnscaled(desktopScreen, 0, 50);

                Font TextFont = new Font("Tahoma", 15);
                SolidBrush TextBrush = new SolidBrush(Color.GreenYellow);

                gr.TextRenderingHint = TextRenderingHint.AntiAlias;
                gr.DrawString("Step number " + _stepNumber + ". Step name: " + RemoveIllegalChars(GetStepNameNumber(alexaStep)), TextFont, TextBrush,
                    new RectangleF(15,12, desktopScreen.Width, 50));

                newImage.Save(dir + "\\" + _stepNumber + ".bmp");
                newImage.Dispose();
                gr.Dispose();
                //--

                //desktopScreen.Save(dir + "\\step_" + RemoveIllegalChars(GetStepNameNumber(alexaStep)).Replace(" ", "_") + ".bmp");
                desktopScreen.Dispose();
            }
        }

        /// <summary>
        /// Returns a path without illegal characters
        /// </summary>
        /// <param name="path">the path string</param>
        /// <returns>return the path without illegal characters</returns>
        private static string RemoveIllegalChars(string path)
        {
            //remove all illegal characters
            return path.Replace("<", "").Replace(">", "").Replace("\"", "").Replace(":", "").Replace("/", "").Replace("\\", "").Replace("|", "").Replace("?", "").Replace("*", "");
        }

        /// <summary>
        /// Returns the step name or the step number
        /// </summary>
        /// <param name="stepNode">the xml node that contains the step</param>
        /// <returns>the step name or step number</returns>
        private static string GetStepNameNumber(XmlNode stepNode)
        {
            try{ return stepNode.Attributes["name"].Value; }
            catch{ return _stepNumber.ToString();}
        }

        /// <summary>
        /// Maximize the window
        /// </summary>
        /// <param name="windowHandle">the handle to the window</param>
        private static void MaximizeWindow(IntPtr windowHandle)
        {
            //show window on top and try to maximize it using windows api
            SystemUtils.User32.ShowWindowOnTop(windowHandle, true);

            SystemUtils.User32.WindowProperties windowProperties = new SystemUtils.User32.WindowProperties();
            windowProperties = SystemUtils.User32.GetWindowProperties(windowHandle);


            //if the window is not maximized then try to maximize it using the mouse
            if ((windowProperties.Height <= SystemUtils.ScreenHeight - 50 || windowProperties.Width <= SystemUtils.ScreenWidth - 50 
                || windowProperties.X > 100 || windowProperties.Y > 100) && windowProperties.X > 0)
            {
                _autoIt.MouseClick("left",windowProperties.X + windowProperties.Width - 50, windowProperties.Y + 15);
            }  
        }

        /// <summary>
        /// Analyze all windows to find what is written into the step
        /// </summary>
        /// <param name="stepType">the step type</param>
        /// <param name="alexaStep">the Al'exa step node</param>
        /// <returns>true if what is written into the step is found</returns>
        private static Boolean AnalyzeWindow(StepType stepType, XmlNode alexaStep)
        {
            #region private variables
            //store the hanlde of current active window
            IntPtr currHandle;

            //store debug properties
            string debugWindowFullPath = "";
            string debugWindowPath = "";
            string debugImageName = "";

            int mouseX = 0;
            int mouseY = 0;

            bool wasClickOff = false;
            bool wasMouseMoveOff = false;

            //store the tab title image
            Bitmap tabTitleImg = null;
            //store the desktop screenshot
            Bitmap desktopScreen = null;

            //contains the regular expression used to search the window(s) title
            string windowRegEx = "";

            //conatins the regular expression used to used to search the tab title
            string tabTitle = "";

            //tab properties
            int tabHeight = 10;
            int tabWidth = 215;
            int tabSizeTollerance = 60;

            //store brightness and constrast values
            int tabBoxBrightness = -999; //default value, if it doesn't change I don't change brightness and contrast
            int tabBoxContrast = -999;
            int tabTitleBrightness = -999;
            int tabTitleContrast = -999;

            //used for icon in tabs
            int subtractLeft = 0;
            int subtractRight = 0;

            bool binarizeImage = false;
            bool binarizeTab = false;

            //offset coordinate, the origin point of these coordinats
            //are the top left point of the box that will be found
            int clickOffsetX = 30;
            int clickOffsetY = 10;

            //variables for the color change
            int[] oldColor = {-1,-1,-1};
            int[] newColor = {-1,-1,-1};

            bool maximizeWindow = true;

            #endregion

            #region get node attributes
            try
            {
                //check if we have to looking for a window, it is mandatory.
                windowRegEx = alexaStep.SelectSingleNode("window").Attributes["title"].Value;
            }
            catch { return false; }

            try
            {
                //check if we have to looking for a tab, it isn't mandatory.
                tabTitle = alexaStep.SelectSingleNode("window").SelectSingleNode("tab").Attributes["title"].Value;
            }
            catch {}

            try
            {
                //take tab height, it is not mandatory.
                tabHeight = Int32.Parse(alexaStep.SelectSingleNode("window").SelectSingleNode("tab").Attributes["height"].Value);
            }
            catch {}

            try
            {
                //take tab width, it is not mandatory.
                tabWidth = Int32.Parse(alexaStep.SelectSingleNode("window").SelectSingleNode("tab").Attributes["width"].Value);
            }
            catch { }

            try
            {
                //take tab tollerance, it isn't mandatory.
                tabSizeTollerance = Int32.Parse(alexaStep.SelectSingleNode("window").SelectSingleNode("tab").Attributes["tollerance"].Value);
            }
            catch { }

            try
            {
                //take tab brightness, it is not mandatory.
                tabBoxBrightness = Int32.Parse(alexaStep.SelectSingleNode("window").Attributes["brightness"].Value);
            }
            catch { }

            try
            {
                //take tab contrast, it is not mandatory.
                tabBoxContrast = Int32.Parse(alexaStep.SelectSingleNode("window").Attributes["contrast"].Value);
            }
            catch { }

            try
            {
                //check if we have to binarize the label
                if (alexaStep.SelectSingleNode("window").Attributes["binarize"].Value == "yes")
                    binarizeImage = true;
                else
                    binarizeImage = false;
            }
            catch { binarizeImage = false; }

            try
            {
                //take tab brightness, it is not mandatory.
                tabTitleBrightness = Int32.Parse(alexaStep.SelectSingleNode("window").SelectSingleNode("tab").Attributes["brightness"].Value);
            }
            catch { }

            try
            {
                //take tab contrast, it is not mandatory.
                tabTitleContrast = Int32.Parse(alexaStep.SelectSingleNode("window").SelectSingleNode("tab").Attributes["contrast"].Value);
            }
            catch { }

            try
            {
                //check if we have to binarize the label
                if (alexaStep.SelectSingleNode("window").SelectSingleNode("tab").Attributes["binarize"].Value == "yes")
                    binarizeTab = true;
                else
                    binarizeTab = false;
            }
            catch { binarizeTab = false; }

            try
            {
                //take tab contrast, it is not mandatory.
                subtractRight = Int32.Parse(alexaStep.SelectSingleNode("window").SelectSingleNode("tab").Attributes["right.subtraction"].Value);
            }
            catch { }

            try
            {
                //take tab contrast, it is not mandatory.
                subtractLeft = Int32.Parse(alexaStep.SelectSingleNode("window").SelectSingleNode("tab").Attributes["left.subtraction"].Value);
            }
            catch { }

            try
            {
                //it is not a mandatory options
                clickOffsetX = Int32.Parse(alexaStep.SelectSingleNode("window").SelectSingleNode("tab").Attributes["click.add.x"].Value);
            }
            catch { }

            try
            {
                //it is not a mandatory options
                clickOffsetY = Int32.Parse(alexaStep.SelectSingleNode("window").SelectSingleNode("tab").Attributes["click.add.y"].Value);
            }
            catch { }

            try
            {
                //they are not a mandatory options
                if (alexaStep.SelectSingleNode("window").Attributes["maximize"].Value == "yes")
                    maximizeWindow = true;
                else if (alexaStep.SelectSingleNode("window").Attributes["maximize"].Value == "no")
                    maximizeWindow = false;
            }
            catch { }

            try
            {
                string replaceColor = alexaStep.SelectSingleNode("window").Attributes["color.replace"].Value.Replace(" ", "");
                string oldColorStr = replaceColor.Split(')')[0];
                oldColorStr = oldColorStr.Replace("((", "");
                string[] oldColorArr = oldColorStr.Split(',');

                oldColor[0] = Int32.Parse(oldColorArr[0]);
                oldColor[1] = Int32.Parse(oldColorArr[1]);
                oldColor[2] = Int32.Parse(oldColorArr[2]);

                string newColorStr = oldColorStr = replaceColor.Split(')')[1];
                newColorStr = newColorStr.Replace(",(", "");
                string[] newColorArr = newColorStr.Split(',');

                newColor[0] = Int32.Parse(newColorArr[0]);
                newColor[1] = Int32.Parse(newColorArr[1]);
                newColor[2] = Int32.Parse(newColorArr[2]);
            }
            catch { }


            #endregion

            //salvo l'attuale handle
            currHandle = SystemUtils.User32.GetActiveWindow();

            try
            {
                if (_debugLogLevel)
                {
                    //get the log folder for the genericbox step type
                    debugWindowPath = Path.Combine(_debugHomeFolder, DateTime.Now.ToString(_debugFolderDateFormat) + "_Step_" + RemoveIllegalChars(GetStepNameNumber(alexaStep)).Replace(" ","_") + "_AnalyzeWindows");

                    debugWindowFullPath = debugWindowPath;

                    //get the full path (otherwise the method Image.Save does not work)
                    DirectoryInfo dir = new DirectoryInfo(debugWindowFullPath);
                    debugWindowFullPath = dir.FullName;

                    //if log folder doesn't exist then create it
                    if (!Directory.Exists(dir.FullName)) dir.Create();
                }

                //loop through all windows that matches our regular expression
                foreach (IntPtr windowHandle in SystemUtils.User32.GetWindowsCollection(windowRegEx))
                {
                    //brings the window to the foreground
                    SystemUtils.User32.ShowWindowOnTop(windowHandle, maximizeWindow);
                    Thread.Sleep(1500);


                    if (stepType == StepType.InteractInputBox && StepInteractInputBox() == true ||
                        stepType == StepType.InteractGenericBox && StepInteractGenericBox() == true ||
                        stepType == StepType.InteractIcon && StepInteractIcon(alexaStep) == true ||
                        stepType == StepType.RunExe && StepRunExe(alexaStep) == true ||
                        stepType == StepType.InteractButton && StepInteractButton() == true ||
                        stepType == StepType.InteractIconList && StepInteractIconListItem() == true ||
                        stepType == StepType.InteractText && StepInteractText() == true ||
                        stepType == StepType.InteractDropDownList && StepInteractDropDownList() == true)
                    {
                        return true;
                    }


                    if (tabTitle != "")
                    {
                        desktopScreen = ScreenUtils.CaptureDesktop();

                        //save the screenshot of the desktop
                        if (_debugLogLevel)
                        {

                            //set the debug folder for the core
                            _core.SetDebugFolder(Path.Combine(debugWindowFullPath, DateTime.Now.ToString(_debugImageDateFormat) + "_"));
                            desktopScreen.Save(Path.Combine(debugWindowFullPath, DateTime.Now.ToString(_debugImageDateFormat) + "_DesktopScreenshot.bmp"));
                        }

                        //set the source image for the core
                        SetCoreSourceImage(desktopScreen);

                        //check if we have to change the color
                        if (oldColor[0] != -1 && oldColor[1] != -1 && oldColor[2] != -1 && newColor[0] != -1 && newColor[1] != -1 && newColor[2] != -1)
                        {
                            Bitmap clone = new Bitmap(desktopScreen.Width, desktopScreen.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                            using (Graphics gr = Graphics.FromImage(clone))
                            {
                                gr.DrawImage(desktopScreen, new Rectangle(0, 0, clone.Width, clone.Height));
                            }
                            Bitmap c = _core.ReplaceColor(clone, oldColor[0], oldColor[1], oldColor[2], newColor[0], newColor[1], newColor[2]);
                            clone.Dispose();
                            _core.SetSourceImage(c);

                            c.Dispose();
                        }


                        if (binarizeImage)
                        {
                            //if user has set brightness and contrast value then change it
                            if (tabBoxBrightness != -999 && tabBoxContrast != -999)
                                _core.SetBrightnessContrast(tabBoxBrightness, tabBoxContrast);
                            else
                                //otherwise use a value that is ok for almost all application
                                _core.SetBrightnessContrast(-60, 50);
                        }

                        //get the boxes
                         List<Alexa.Core.Box> tabBoxes;
                        if (binarizeImage)
                        {
                            tabBoxes = _core.GetGenericBoxes(tabHeight, tabWidth, tabSizeTollerance);
                        }
                        else
                        {
                            tabBoxes = _core.GetGenericBoxesV2(tabHeight, tabWidth, tabSizeTollerance);
                        }

                        int boxCnt = 0;
                        foreach (Core.Box box in tabBoxes)
                        {
                            if (subtractLeft != 0 || subtractRight != 0)
                            {
                                tabTitleImg = CropRect(desktopScreen, new Rectangle(box.x + subtractLeft, box.y, box.width - subtractRight - subtractLeft, box.height));
                                tabTitleImg = (Bitmap)ResizeImage(tabTitleImg, new Size(tabTitleImg.Width * 3, tabTitleImg.Height * 3));
                            }
                            else
                            {
                                tabTitleImg = CropRect(desktopScreen, new Rectangle(box.x, box.y, box.width, box.height));
                                tabTitleImg = (Bitmap)ResizeImage(tabTitleImg, new Size(tabTitleImg.Width * 3, tabTitleImg.Height * 3));
                            }

                            //set Alexa.Core source image
                            SetCoreSourceImage(tabTitleImg);

                            if (binarizeTab)
                            {
                                //if user has set brightness and contrast value then change it
                                if (tabTitleBrightness != -999 && tabTitleContrast != -999)
                                    _core.SetBrightnessContrast(tabTitleBrightness, tabTitleContrast);
                                else
                                    //otherwise use a value that is ok for almost all application
                                    _core.SetBrightnessContrast(0, 50);

                                //binarize the image
                                _core.BinarizeImage();
                            }

                            #region debug message
                            //save the binarized image if we are in debug
                            if (_debugLogLevel)
                            {
                                debugImageName = DateTime.Now.ToString(_debugImageDateFormat) + "_Box" + boxCnt + ".bmp";
                                _core.GetSourceImage().Save(Path.Combine(debugWindowFullPath, debugImageName));
                                LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "save debug image: " + debugWindowPath + "\\" + debugImageName);
                            }
                            #endregion

                            //if the OCR engine has found the label text then click
                            if (checkStringByOCR(_core.GetSourceImage(), tabTitle) == true)
                            {
                                //get the coordinates of where we have to click
                                mouseX = box.x + clickOffsetX;
                                mouseY = box.y + clickOffsetY;
                                #region debug message
                                if (_debugLogLevel) //if we are in debug write the message
                                    LogUtils.Write(new StackFrame(0, true), LogUtils.ErrorLevel.Debug, "click on the generic box");
                                #endregion

                                if (_mouseClick == false)
                                {
                                    wasClickOff = true;
                                    _mouseClick = true;
                                }

                                if (_mouseMove == false)
                                {
                                    wasMouseMoveOff = true;
                                    _mouseMove = true;
                                }


                                //click
                                Click(mouseX, mouseY);

                                if (wasClickOff == true)
                                {
                                    wasClickOff = false;
                                    _mouseClick = false;
                                }

                                if (wasMouseMoveOff == true)
                                {
                                    wasMouseMoveOff = false;
                                    _mouseMove = false;
                                }

                                Thread.Sleep(1000);

                                if (stepType == StepType.InteractInputBox && StepInteractInputBox() == true ||
                                    stepType == StepType.InteractGenericBox && StepInteractGenericBox() == true ||
                                    stepType == StepType.InteractIcon && StepInteractIcon(alexaStep) == true ||
                                    stepType == StepType.RunExe && StepRunExe(alexaStep) == true ||
                                    stepType == StepType.InteractButton && StepInteractButton() == true ||
                                    stepType == StepType.InteractIconList && StepInteractIconListItem() == true ||
                                    stepType == StepType.InteractText && StepInteractText() == true ||
                                    stepType == StepType.InteractDropDownList && StepInteractDropDownList() == true)
                                {
                                    return true;
                                }
                            }

                            boxCnt++;
                        }
                        desktopScreen.Dispose();
                    }
                    else
                    {
                        //if (stepType == StepType.InteractInputBox && StepInteractInputBox() == true ||
                        //    stepType == StepType.InteractGenericBox && StepInteractGenericBox() == true ||
                        //    stepType == StepType.InteractIcon && StepInteractIcon(alexaStep) == true ||
                        //    stepType == StepType.RunExe && StepRunExe(alexaStep) == true ||
                        //    stepType == StepType.InteractButton && StepInteractButton() == true ||
                        //    stepType == StepType.InteractIconList && StepInteractIconListItem() == true ||
                        //    stepType == StepType.InteractText && StepInteractText() == true ||
                        //    stepType == StepType.InteractDropDownList && StepInteractDropDownList() == true)
                        //{
                        //    return true;
                        //}
                    }

                    //desktopScreen.Dispose();
                }

                //brings the original window to the foreground
                SystemUtils.User32.ShowWindowOnTop(currHandle, maximizeWindow);
                Thread.Sleep(1500);


                return false;
            }
            catch(Exception ex)
            {
                return false;
            }
            finally
            {
                if (desktopScreen != null) desktopScreen.Dispose();
                if (tabTitleImg != null) tabTitleImg.Dispose();
            }
        }

        public static void SetCoreSourceImage(Bitmap image)
        {
            try
            {
                Bitmap clone = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                using (Graphics gr = Graphics.FromImage(clone))
                {
                    gr.DrawImage(image, new Rectangle(0, 0, clone.Width, clone.Height));
                }

                _core.SetSourceImage(clone);
                clone.Dispose();
            }
            catch(Exception ex)
            {
            }
        }
    }
}
