This folder contains all the references that you have to add in Visual Studio.


You have to add libtesseract.dll


libtesseract.dll is the reference that you need to use Tesseract OCR with the Framework .NET
I download it from here: https://github.com/charlesw/tesseract-ocr-dotnet
The home page of Tesseract project is: http://code.google.com/p/tesseract-ocr/


The folder OpenCV contains all files that allow you to use OpenCV functions, you should add
that folder to your windows environment path (or copy all files into the bin folder of the solution).
The home page of OpenCV project is: http://opencv.willowgarage.com/wiki/


You need also to install the AutoIT interpreter engine from here: http://www.autoitscript.com/site/autoit/
You have to register the AutoItX3.dll that comes with the AutoIT installation package and then add that dll
as new reference in Visual Studio. However you can only register the AutoItX3.dll without install AutoIT
(I've copy that dll into the folder "AutoIT"). You can register the dll with the command:
regsvr32 your_dll_path\AutoItX3.dll


You have to install also the Microsoft Visual C++ 2010 Redistributable Package (x86).
You need it (the x86 version) also on a 64 bit machine.


Al'exa is released under GPL v3.0 license.

OpenCV is licensed under the terms of the BSD License.
More details on the project page: http://opencv.willowgarage.com/wiki/

Tesseract is licensed under the Apache License 2.0 license.
More details on the project page: http://code.google.com/p/tesseract-ocr/

OpenCV BSD License and Apache License 2.0 are compatible with the General Public License Version 3.

Ragarding Microsoft Framework .NET, Microsoft Visual C++ 2010 Redistributable Package and
AutoIT, the General Public License Version 3 considers them as System Libraries and/or compiler
used to produce the work and/or an object code interpreter. So they are not to be considered
part of Al'exa source code or binaries.

You can find more details about Al'exa here: http://www.alan-pipitone.com/alexa/docs/
