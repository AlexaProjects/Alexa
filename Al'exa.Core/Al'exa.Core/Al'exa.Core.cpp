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

// This is the main DLL file.
#include "stdafx.h"

#include "Al'exa.Core.h"

#include <stdio.h>
#include <iostream>
#include <cv.h>
#include <highgui.h>
#include "opencv2/core/core.hpp"
#include "opencv2/features2d/features2d.hpp"
#include "opencv2/highgui/highgui.hpp"
#include "opencv2/imgproc/imgproc.hpp"
#include "opencv2/calib3d/calib3d.hpp"

using namespace cv;
using namespace System;
using namespace System::IO;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

namespace Alexa {

	/*void Core::SetSourceImage(Bitmap^ inputImage)
	{
		//if the image already exists then release it
		if(_inputImage)
		{
			//Release the input image
			pin_ptr<IplImage*> pInputImage = &_inputImage; 
			cvReleaseImage(pInputImage);
		}

		ImageCodecInfo^ jgpEncoder = GetEncoder(ImageFormat::Jpeg);

		System::Drawing::Imaging::Encoder^ myEncoder =
                        System::Drawing::Imaging::Encoder::Quality;

		EncoderParameters^ myEncoderParameters = gcnew EncoderParameters(1);

		EncoderParameter^ myEncoderParameter = gcnew EncoderParameter(myEncoder, (__int64)100);

        myEncoderParameters->Param[0] = myEncoderParameter;

		MemoryStream^ stream = gcnew MemoryStream();

		inputImage->Save(stream, jgpEncoder, myEncoderParameters);

		_inputImage = BitmapToIplImage((Bitmap^)Bitmap::FromStream(stream));

		 stream->~MemoryStream();
	}*/

	void Core::SetSourceImage(Bitmap^ inputImage)
	{
		//if the image already exists then release it
		if(_inputImage)
		{
			//Release the input image
			pin_ptr<IplImage*> pInputImage = &_inputImage; 
			cvReleaseImage(pInputImage);
		}

		_inputImage = BitmapToIplImage(inputImage);

	}

	//enable/disable debug
	void Core::EnableDebug(bool enable)
	{
		_debug = enable;
	}

	void Core::SetDebugFolder(System::String^ path)
	{
		_debugFolder = path;
	}

	List<Core::Box^>^ Core::GetInputBoxes()
	{
		return GetBoxes(0);
	}

	List<Core::Box^>^ Core::GetInputBoxesV2()
	{
		return GetBoxes2(0);
	}

	List<Core::Box^>^ Core::GetGenericBoxes(int height, int width, int tollerance)
	{
		genBoxHeight = height;
		genBoxWidth = width;
		genBoxTollerance = tollerance;

		return GetBoxes(1);
	}

	List<Core::Box^>^ Core::GetGenericBoxesV2(int height, int width, int tollerance)
	{
		genBoxHeight = height;
		genBoxWidth = width;
		genBoxTollerance = tollerance;

		return GetBoxes2(1);
	}

	//get all buttons in the Alexa Core source image
	List<Core::Box^>^ Core::GetButtons()
	{
		return GetBoxes(2);
	}

	//get all buttons in the Alexa Core source image
	List<Core::Box^>^ Core::GetButtonsV2()
	{
		return GetBoxes2(2);
	}

	//get all chars in the Alexa Core source image
	List<Core::Box^>^ Core::GetChars(int lineThickness, int spaceThickness, int r, int g, int b)
	{
		charLinesColorR = r;
		charLinesColorG = g;
		charLinesColorB = b;

		boxLineThickness = lineThickness;
		boxSpaceThickness = spaceThickness;
		return GetBoxes(3);
	}

	//get all chars in the Alexa Core source image
	List<Core::Box^>^ Core::GetCharsV2(int lineThickness, int spaceThickness, int r, int g, int b)
	{
		charLinesColorR = r;
		charLinesColorG = g;
		charLinesColorB = b;

		boxLineThickness = lineThickness;
		boxSpaceThickness = spaceThickness;
		return GetBoxes2(3);
	}

	//get all chars in the Alexa Core source image
	List<Core::Box^>^ Core::GetWords(int minHeight, int maxHeight, int minWidth, int maxWidth)
	{
		boxMinHeight = minHeight;
		boxMaxHeight = maxHeight;
		boxMinWidth = minWidth;
		boxMaxWidth = maxWidth;
		return GetBoxes(4);
	}

	//get all chars in the Alexa Core source image
	List<Core::Box^>^ Core::GetWordsV2(int minHeight, int maxHeight, int minWidth, int maxWidth)
	{
		boxMinHeight = minHeight;
		boxMaxHeight = maxHeight;
		boxMinWidth = minWidth;
		boxMaxWidth = maxWidth;
		return GetBoxes2(4);
	}

	//get all icon list boxes in the Alexa Core source image
	List<Core::Box^>^ Core::GetIconListBoxes()
	{
		return GetBoxes(5);
	}

	//get all icon list boxes in the Alexa Core source image
	List<Core::Box^>^ Core::GetIconListBoxesV2()
	{
		return GetBoxes2(5);
	}

	//get all boxes in the Alexa Core source image
	List<Core::Box^>^ Core::GetInterestPoints()
	{
		return GetBoxes(6);
	}

	List<Core::Box^>^ Core::GetBoxes(int boxType)
	{
		//contains input boxes coordinates
		//ArrayList^ inputBoxCoordinates = gcnew ArrayList;
		List<Core::Box^>^ boxes = gcnew List<Core::Box^>;

		//init images
		IplImage * tmp = _inputImage;
		Mat image;
		Mat imageGray;
		Mat imageBlackWhite;
		Mat result;
		//for debug
		Mat clone;
		Mat clone2;
		Mat clone3;
		Mat clone4;

		std::vector<std::vector<cv::Point>> contours;
		Rect rect;
		cv::Point pt1, pt2;

		//adjust brightness and contrast
		//image = ContrastBrightness(_inputImage,50,-100);
		image = tmp;

		//for debug:
		if(_debug)
		{
			clone = image.clone();
			clone4 = image.clone();
		}

		//convert image in grayscale
		cvtColor(image,imageGray,CV_RGB2GRAY);

		//i want a black and white image
		imageBlackWhite = imageGray > 128; 
		//for debug
		if(_debug) clone2 = imageBlackWhite.clone();

		vector<Vec4i> hierarchy;

		//thi will find all contours in our image
		findContours( imageBlackWhite, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_SIMPLE, cv::Point(0, 0) );
		//for debug
		if(_debug) clone3 = imageBlackWhite.clone();

		result = Mat(image.size(),CV_8U,cv::Scalar(255));

		//loop all contours
		for(int i = 0 ; i < contours.size() ; i++ )
		{

			rect = boundingRect(contours[i]); 

			pt1.x = rect.x; 
			pt1.y = rect.y; 
			pt2.x = rect.x + rect.width; 
			pt2.y = rect.y + rect.height; 

			//for debug
			if(_debug && boxType == 4)
				rectangle(clone4, pt1, pt2, Scalar(255, 0, 0), 2); 
			else if (_debug)
				rectangle(clone4, pt1, pt2, Scalar(0, 0, 255), 2); 

			//save only the contours according to the thresholds sets
			/*
				if( (boxType == 0 && (rect.width >= (85)  && rect.width <= (700)) && (rect.height >= 15 && rect.height <= 40)) ||
				(boxType == 1 && (rect.width >= (genBoxWidth - genBoxTollerance)  && rect.width <= (genBoxWidth + genBoxTollerance))
					&& (rect.height >= (genBoxHeight - genBoxTollerance)  && rect.height <= (genBoxHeight + genBoxTollerance))) ||
				(boxType ==2 && (rect.width >= 30  && rect.width <= 250) && (rect.height >= 15 && rect.height <= 50)) ||
				(boxType == 5 && (rect.width >= 10  && rect.width <= 35) && (rect.height >= 10 && rect.height <= 35)) ||
				(boxType == 6 && (rect.width >= 2  && rect.width <= 2000) && (rect.height >= 2 && rect.height <= 2000)) )
			*/
			/*if( (boxType == 0 && (rect.width >= (85)  && rect.width <= (700)) && (rect.height >= 10 && rect.height <= 40)) ||
				(boxType == 1 && (rect.width >= (genBoxWidth - genBoxTollerance)  && rect.width <= (genBoxWidth + genBoxTollerance))
					&& (rect.height >= (genBoxHeight - genBoxTollerance)  && rect.height <= (genBoxHeight + genBoxTollerance))) ||
				(boxType ==2 && (rect.width >= 30  && rect.width <= 250) && (rect.height >= 10 && rect.height <= 50)) ||
				(boxType == 5 && (rect.width >= 10  && rect.width <= 35) && (rect.height >= 10 && rect.height <= 35)) ||
				(boxType == 6 && (rect.width >= 2  && rect.width <= 2000) && (rect.height >= 2 && rect.height <= 2000)) )*/
			if( (boxType == 0 && (rect.width >= (85)  && rect.width <= (700)) && (rect.height >= 15 && rect.height <= 40)) ||
				(boxType == 1 && (rect.width >= (genBoxWidth - genBoxTollerance)  && rect.width <= (genBoxWidth + genBoxTollerance))
					&& (rect.height >= (genBoxHeight - genBoxTollerance)  && rect.height <= (genBoxHeight + genBoxTollerance))) ||
				(boxType ==2 && (rect.width >= 30  && rect.width <= 250) && (rect.height >= 15 && rect.height <= 50)) ||
				(boxType == 5 && (rect.width >= 10  && rect.width <= 35) && (rect.height >= 10 && rect.height <= 35)) ||
				(boxType == 6 && (rect.width >= 2  && rect.width <= 2000) && (rect.height >= 2 && rect.height <= 2000)) )
			{
				Core::Box^ box = gcnew Core::Box;
				box->x = pt1.x;
				box->y = pt1.y;
				box->height = rect.height;
				box->width = rect.width;

				boxes->Add(box);

				//for debug:
				if(_debug) rectangle(clone, pt1, pt2, Scalar(0, 0 , 255), 2); 

			}
			else if(boxType == 3 && rect.width <= 50 && rect.height <= 50)
			{
				Core::Box^ box = gcnew Core::Box;
				box->x = pt1.x;
				box->y = pt1.y;
				box->height = rect.height;
				box->width = rect.width;

				boxes->Add(box);

				cv::Point pt3, pt4;

				//pt3.x = rect.x + rect.width + boxLineThickness - 2; 
				//pt3.y = rect.y; 

				//pt4.x = pt2.x + boxLineThickness - 2; 
				//pt4.y = pt2.y; 

				pt3.x = rect.x + rect.width - 2; 
				pt3.y = rect.y - (boxLineThickness/2);
				pt4.x = rect.x + rect.width + boxSpaceThickness; 
				pt4.y = rect.y + rect.height + (boxLineThickness) ; 

				//for debug:
				if(_debug)
				{
					rectangle(clone, pt1, pt2, Scalar(charLinesColorB, charLinesColorG , charLinesColorR), -1); 
					rectangle(clone, pt1, pt2, Scalar(charLinesColorB, charLinesColorG , charLinesColorR), boxLineThickness); 
					rectangle(clone, pt3, pt4, Scalar(charLinesColorB, charLinesColorG , charLinesColorR), -1); 
					//line(clone, pt3, pt4 , Scalar(0, 0 , 255), boxSpaceThickness); 
				}

				cvRectangle( _inputImage, pt1, pt2, cvScalar(charLinesColorB, charLinesColorG, charLinesColorR, 0), -1);   
				cvRectangle( _inputImage, pt1, pt2, cvScalar(charLinesColorB, charLinesColorG, charLinesColorR, 0), boxLineThickness);  
				cvRectangle( _inputImage, pt3, pt4, cvScalar(charLinesColorB, charLinesColorG, charLinesColorR, 0), -1);
				//cvLine(_inputImage, pt3, pt4 , cvScalar(0, 0, 255, 0), boxSpaceThickness);
			}
			else if(boxType == 4 && (rect.width >= boxMinWidth  && rect.width <= boxMaxWidth) && (rect.height >= boxMinHeight && rect.height <= boxMaxHeight))
			{
				Core::Box^ box = gcnew Core::Box;
				box->x = pt1.x;
				box->y = pt1.y;
				box->height = rect.height;
				box->width = rect.width;

				boxes->Add(box);

				//for debug:
				if(_debug) rectangle(clone, pt1, pt2, Scalar(255, 0 , 0), 2); 

			}

		}

		//for debug:
		if(_debug)
		{
			if(boxType== 3)
			{
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_1_Chars_BlackAndWhite.bmp"))).ToPointer(),clone2);
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_2_Chars_Borders.bmp"))).ToPointer(),clone3);
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_3_Chars_FoundAll.bmp"))).ToPointer(),clone4);
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_4_Chars_FoundThatMatch.bmp"))).ToPointer(),clone);
			}
			else if(boxType == 4)
			{
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_1_Words_BlackAndWhite.bmp"))).ToPointer(),clone2);
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_2_Words_Borders.bmp"))).ToPointer(),clone3);
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_3_Words_FoundAll.bmp"))).ToPointer(),clone4);
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_4_Words_FoundThatMatch.bmp"))).ToPointer(),clone);
			}
			else if(boxType == 6)
			{
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_InterestPoints.bmp"))).ToPointer(),clone);
			}
			else
			{
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_1_BlackAndWhite.bmp"))).ToPointer(),clone2);
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_2_Borders.bmp"))).ToPointer(),clone3);
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_3_FoundAll.bmp"))).ToPointer(),clone4);
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_4_FoundThatMatch.bmp"))).ToPointer(),clone);
			}
			
		}

		//release all object
		image.release();
		imageGray.release();
		imageBlackWhite.release();
		result.release();
		//for debug
		clone.release();
		clone2.release();
		clone3.release();
		clone4.release();
		//cvReleaseImage(&tmp);

		return boxes;
	}

	List<Core::Box^>^ Core::GetBoxes2(int boxType)
	{
		//contains input boxes coordinates
		//ArrayList^ inputBoxCoordinates = gcnew ArrayList;
		List<Core::Box^>^ boxes = gcnew List<Core::Box^>;

		//init images
		IplImage * tmp = _inputImage;
		Mat image;
		Mat imageGray;
		//Mat imageBlackWhite;
		Mat result;
		//for debug
		Mat clone;
		//Mat clone2;
		Mat clone3;
		Mat clone4;

		Mat cannyOutput;

		std::vector<std::vector<cv::Point>> contours;
		Rect rect;
		cv::Point pt1, pt2;

		//adjust brightness and contrast
		//image = ContrastBrightness(_inputImage,50,-100);
		image = tmp;

		//for debug:
		if(_debug)
		{
			clone = image.clone();
			clone4 = image.clone();
		}

		//convert image in grayscale
		cvtColor(image,imageGray,CV_RGB2GRAY);

		//i want a black and white image
		//imageBlackWhite = imageGray > 128; 
		//for debug
		//if(_debug) clone2 = imageBlackWhite.clone();

		vector<Vec4i> hierarchy;

		/// Detect edges using canny
		Canny( imageGray, cannyOutput, 40, 40*2, 3 );

		//thi will find all contours in our image
		findContours( cannyOutput, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_SIMPLE, cv::Point(0, 0) );
		//for debug
		if(_debug) clone3 = cannyOutput.clone();

		result = Mat(image.size(),CV_8U,cv::Scalar(255));

		//loop all contours
		for(int i = 0 ; i < contours.size() ; i++ )
		{

			rect = boundingRect(contours[i]); 

			pt1.x = rect.x; 
			pt1.y = rect.y; 
			pt2.x = rect.x + rect.width; 
			pt2.y = rect.y + rect.height; 

			//for debug
			if(_debug && boxType == 4)
				rectangle(clone4, pt1, pt2, Scalar(255, 0, 0), 2); 
			else if (_debug)
				rectangle(clone4, pt1, pt2, Scalar(0, 0, 255), 2); 

			//save only the contours according to the thresholds sets
			/*
				if( (boxType == 0 && (rect.width >= (85)  && rect.width <= (700)) && (rect.height >= 15 && rect.height <= 40)) ||
				(boxType == 1 && (rect.width >= (genBoxWidth - genBoxTollerance)  && rect.width <= (genBoxWidth + genBoxTollerance))
					&& (rect.height >= (genBoxHeight - genBoxTollerance)  && rect.height <= (genBoxHeight + genBoxTollerance))) ||
				(boxType ==2 && (rect.width >= 30  && rect.width <= 250) && (rect.height >= 15 && rect.height <= 50)) ||
				(boxType == 5 && (rect.width >= 10  && rect.width <= 35) && (rect.height >= 10 && rect.height <= 35)) ||
				(boxType == 6 && (rect.width >= 2  && rect.width <= 2000) && (rect.height >= 2 && rect.height <= 2000)) )
			*/
			if( (boxType == 0 && (rect.width >= (85)  && rect.width <= (700)) && (rect.height >= 15 && rect.height <= 40)) ||
				(boxType == 1 && (rect.width >= (genBoxWidth - genBoxTollerance)  && rect.width <= (genBoxWidth + genBoxTollerance))
					&& (rect.height >= (genBoxHeight - genBoxTollerance)  && rect.height <= (genBoxHeight + genBoxTollerance))) ||
				(boxType ==2 && (rect.width >= 30  && rect.width <= 250) && (rect.height >= 15 && rect.height <= 50)) ||
				(boxType == 5 && (rect.width >= 10  && rect.width <= 35) && (rect.height >= 10 && rect.height <= 35)) ||
				(boxType == 6 && (rect.width >= 2  && rect.width <= 2000) && (rect.height >= 2 && rect.height <= 2000)) )
			{
				Core::Box^ box = gcnew Core::Box;
				box->x = pt1.x;
				box->y = pt1.y;
				box->height = rect.height;
				box->width = rect.width;

				boxes->Add(box);

				//for debug:
				if(_debug) rectangle(clone, pt1, pt2, Scalar(0, 0 , 255), 2); 

			}
			else if(boxType == 3 && rect.width <= 50 && rect.height <= 50)
			{
				Core::Box^ box = gcnew Core::Box;
				box->x = pt1.x;
				box->y = pt1.y;
				box->height = rect.height;
				box->width = rect.width;

				boxes->Add(box);

				cv::Point pt3, pt4;

				//pt3.x = rect.x + rect.width + boxLineThickness - 2; 
				//pt3.y = rect.y; 

				//pt4.x = pt2.x + boxLineThickness - 2; 
				//pt4.y = pt2.y; 

				pt3.x = rect.x + rect.width - 2; 
				pt3.y = rect.y - (boxLineThickness/2);
				pt4.x = rect.x + rect.width + boxSpaceThickness; 
				pt4.y = rect.y + rect.height + (boxLineThickness) ; 

				//for debug:
				if(_debug)
				{
					rectangle(clone, pt1, pt2, Scalar(charLinesColorB, charLinesColorG , charLinesColorR), -1); 
					rectangle(clone, pt1, pt2, Scalar(charLinesColorB, charLinesColorG , charLinesColorR), boxLineThickness); 
					rectangle(clone, pt3, pt4, Scalar(charLinesColorB, charLinesColorG , charLinesColorR), -1); 
					//line(clone, pt3, pt4 , Scalar(0, 0 , 255), boxSpaceThickness); 
				}

				cvRectangle( _inputImage, pt1, pt2, cvScalar(charLinesColorB, charLinesColorG, charLinesColorR, 0), -1);   
				cvRectangle( _inputImage, pt1, pt2, cvScalar(charLinesColorB, charLinesColorG, charLinesColorR, 0), boxLineThickness);  
				cvRectangle( _inputImage, pt3, pt4, cvScalar(charLinesColorB, charLinesColorG, charLinesColorR, 0), -1);
				//cvLine(_inputImage, pt3, pt4 , cvScalar(0, 0, 255, 0), boxSpaceThickness);
			}
			else if(boxType == 4 && (rect.width >= boxMinWidth  && rect.width <= boxMaxWidth) && (rect.height >= boxMinHeight && rect.height <= boxMaxHeight))
			{
				Core::Box^ box = gcnew Core::Box;
				box->x = pt1.x;
				box->y = pt1.y;
				box->height = rect.height;
				box->width = rect.width;

				boxes->Add(box);

				//for debug:
				if(_debug) rectangle(clone, pt1, pt2, Scalar(255, 0 , 0), 2); 

			}

		}

		//for debug:
		if(_debug)
		{
			if(boxType== 3)
			{
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_1_Chars_Borders.bmp"))).ToPointer(),clone3);
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_2_Chars_FoundAll.bmp"))).ToPointer(),clone4);
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_3_Chars_FoundThatMatch.bmp"))).ToPointer(),clone);
			}
			else if(boxType == 4)
			{
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_1_Words_Borders.bmp"))).ToPointer(),clone3);
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_2_Words_FoundAll.bmp"))).ToPointer(),clone4);
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_3_Words_FoundThatMatch.bmp"))).ToPointer(),clone);
			}
			else
			{
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_1_Borders.bmp"))).ToPointer(),clone3);
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_2_FoundAll.bmp"))).ToPointer(),clone4);
				imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("CoreProc_3_FoundThatMatch.bmp"))).ToPointer(),clone);	
			}

		}

		//release all object
		image.release();
		imageGray.release();
		//imageBlackWhite.release();
		result.release();
		//for debug
		clone.release();
		//clone2.release();
		clone3.release();
		clone4.release();
		cannyOutput.release();
		//cvReleaseImage(&tmp);

		return boxes;
	}


	// Convert .NET Bitmap to an OpenCV IplImage.
	IplImage* Core::BitmapToIplImage(Bitmap^ bitmap)
	{
		IplImage* tmp;


		BitmapData^ bmData = bitmap->LockBits(System::Drawing::Rectangle(0, 0,
			bitmap->Width, bitmap->Height),
			System::Drawing::Imaging::ImageLockMode::ReadWrite, bitmap->PixelFormat);
		if(bitmap->PixelFormat ==
			System::Drawing::Imaging::PixelFormat::Format8bppIndexed)
		{
			tmp = cvCreateImage(cvSize(bitmap->Width , bitmap->Height) , IPL_DEPTH_8U ,
				1);
			tmp->imageData = (char*)bmData->Scan0.ToPointer();
		}

		else if (bitmap->PixelFormat ==
			System::Drawing::Imaging::PixelFormat::Format24bppRgb)
		{
			tmp = cvCreateImage(cvSize(bitmap->Width , bitmap->Height) , IPL_DEPTH_8U ,
				3);
			tmp->imageData = (char*)bmData->Scan0.ToPointer();
		}

		bitmap->UnlockBits(bmData);

		//Release the input image
		//pin_ptr<IplImage*> pInputImage = &_inputImage; 
		//cvReleaseImage(pInputImage);
		
				IplImage* returnImage = cvCreateImage(cvGetSize(tmp),tmp->depth, tmp->nChannels);

		cvCopy(tmp,returnImage);
		cvReleaseImage(&tmp);
		return returnImage;
	}

	//set brightness and contrast values
	void Core::SetBrightnessContrast(int Brightness, int Contrast)
	{
		int i;
		double j;
		double y;
		double diff;
		int tmp;

		IplImage * red;
		IplImage * green;
		IplImage * blue;

		uchar table[256];
		CvMat* table_matx;

		if(Contrast > 100) Contrast = 100;
		if(Contrast < -100) Contrast = -100;
		if(Brightness > 150) Brightness = 150;
		if(Brightness < -150) Brightness = -150;

		table_matx = cvCreateMatHeader( 1, 256, CV_8UC1 );
		cvSetData( table_matx, table, 0 );

		diff = 127 * (Contrast/100);
		j = 255/(255 - (diff*2));
		y = j * (Brightness - diff);

		if( Contrast < 0 )
		{
			diff = -128 * (Contrast/100);
			j = (256 - (diff*2))/255;
			y = j * (Brightness + diff);
		}

		for( i = 0; i < 256; i++ )
		{
			tmp = cvRound(j*i + y);
			if( tmp < 0 ) tmp = 0;
			if( tmp > 255 ) tmp = 255;
			table[i] = tmp;
		}

		blue = cvCreateImage(cvGetSize(_inputImage),_inputImage->depth,1);
		green = cvCreateImage(cvGetSize(_inputImage),_inputImage->depth,1);
		red = cvCreateImage(cvGetSize(_inputImage),_inputImage->depth,1);

		cvCvtPixToPlane(_inputImage,red,green,blue,NULL);

		cvLUT( blue, blue, table_matx );
		cvLUT( green, green, table_matx );
		cvLUT( red, red, table_matx );

		cvCvtPlaneToPix(red,green,blue,NULL,_inputImage);

		cvReleaseImage(&blue);
		cvReleaseImage(&green);
		cvReleaseImage(&red);

		cvReleaseMat( &table_matx);
	}

	void Core::Release()
	{
		//if the image already exists then release it
		if(_inputImage)
		{
			//Release the input image
			pin_ptr<IplImage*> pInputImage = &_inputImage; 
			cvReleaseImage(pInputImage);
		}
	}

	Bitmap ^ Core::GetSourceImage()
	{

		Bitmap ^bitmap;

		if(_inputImage->nChannels == 1)
		{
			bitmap = gcnew Bitmap( _inputImage->width, _inputImage->height, PixelFormat::Format8bppIndexed);
 
			ColorPalette  ^palette = bitmap->Palette;
			for( int i=0; i<256; i++)
				palette->Entries[i] = Color::FromArgb(i,i,i);
			bitmap->Palette = palette;
		}
		else
			bitmap = gcnew Bitmap( _inputImage->width, _inputImage->height, PixelFormat::Format24bppRgb);
 
		System::Drawing::Imaging::BitmapData ^data = bitmap->LockBits(System::Drawing::Rectangle(0, 0, bitmap->Width, bitmap->Height), System::Drawing::Imaging::ImageLockMode::ReadWrite,  bitmap->PixelFormat);
		memcpy( data->Scan0.ToPointer(), _inputImage->imageData, _inputImage->imageSize );
		bitmap->UnlockBits( data );
 
		return bitmap;
	}

	void Core::BinarizeImage()
	{
		/*Mat image = _inputImage;
		Mat image_bw;
		Mat imageGray;
			
		cvtColor(image,imageGray,CV_RGB2GRAY);

		image_bw = imageGray > 128;

		IplImage temp = image_bw;

		_inputImage = &temp; 
		//cvSaveImage("c:\\work\\zzzzzzzzzz.bmp",_inputImage);*/

		IplImage *im_rgb  = _inputImage;

		IplImage *im_gray = cvCreateImage(cvGetSize(im_rgb),IPL_DEPTH_8U,1); 

		cvCvtColor(im_rgb,im_gray,CV_RGB2GRAY);

		IplImage* im_bw = cvCreateImage(cvGetSize(im_gray),IPL_DEPTH_8U,1); 

		//cvThreshold(im_gray, im_bw, 128, 255, CV_THRESH_BINARY | CV_THRESH_OTSU); 
		cvThreshold(im_gray, im_bw, 128, 255, CV_THRESH_BINARY); 

		_inputImage = im_bw;

		cvReleaseImage(&im_rgb);
		cvReleaseImage(&im_gray);
		//cvReleaseImage(&im_bw);

	}

	ImageCodecInfo^ Core::GetEncoder(ImageFormat^ format)
    {

        array<ImageCodecInfo^>^  codecs = ImageCodecInfo::GetImageDecoders();

        for each (ImageCodecInfo^ codec in codecs)
        {
            if (codec->FormatID == format->Guid)
            {
                return codec;
            }
        }
        return nullptr;
    }

	//Find the icon in the Alexa Core source image.
	Core::IconBox^ Core::FindIcon(Bitmap^ icon, double threshold)
	{
		IplImage * tmpIcon;

		//convert icon to jpg format
		//ImageCodecInfo^ jgpEncoder = GetEncoder(ImageFormat::Jpeg);

		//System::Drawing::Imaging::Encoder^ myEncoder =
  //                      System::Drawing::Imaging::Encoder::Quality;

		//EncoderParameters^ myEncoderParameters = gcnew EncoderParameters(1);

		//EncoderParameter^ myEncoderParameter = gcnew EncoderParameter(myEncoder, (__int64)100);

  //      myEncoderParameters->Param[0] = myEncoderParameter;

		//MemoryStream^ stream = gcnew MemoryStream();

		//icon->Save(stream, jgpEncoder, myEncoderParameters);

		//convert bitmap to IplImage
		//tmpIcon = BitmapToIplImage((Bitmap^)Bitmap::FromStream(stream));
		tmpIcon = BitmapToIplImage(icon);
		
		//release the MemoryStream
		//stream->~MemoryStream();
		
		IplImage *tmp = cvCreateImage(cvGetSize(_inputImage),_inputImage->depth, _inputImage->nChannels);
		cvCopy(_inputImage,tmp);

		Mat src;
		Mat tpl;
		//for debug
		Mat clone;

		
		src = tmp;
		//for debug
		if(_debug) clone = src.clone();

		//do directly tpl = BitmapToIplImage((Bitmap^)Bitmap::FromStream(stream)) will cause a memory leak.
		//even if You release all resources at the end of the method.
		tpl = tmpIcon;

		Mat src_gray, tpl_gray;
		cvtColor(src, src_gray, CV_BGR2GRAY);
		cvtColor(tpl, tpl_gray, CV_BGR2GRAY);

		int width  = src.cols - tpl.cols + 1;
		int height = src.rows - tpl.rows + 1;

		Mat res = Mat(height, width, CV_8U);

		//do match
		matchTemplate(src_gray, tpl_gray, res, CV_TM_SQDIFF_NORMED);

		double minval, maxval;
		cv::Point  minloc, maxloc;

		//calc minval and maxval
		minMaxLoc(res, &minval, &maxval, &minloc, &maxloc);
		
		Core::IconBox^ box = gcnew Core::IconBox;

		//for debug
		if(_debug) 
			rectangle(clone, cv::Point(minloc.x, minloc.y), cv::Point(minloc.x + tpl.cols, minloc.y + tpl.rows), CV_RGB(255,0,0), 3);

		//check if minval is less than the threshold
		if(minval <= threshold)
		{
			box->x = minloc.x;
			box->y = minloc.y; 
			box->height = tpl.rows;
			box->width = tpl.cols;
			box->found = true;
			box->minval = minval;
		}
		else //if minval isn't less than threshold (so Alexa.Core has not found the icon)...
		{
			//...fill box.x with minval and other properties with -1,
			box->x = -1;
			box->y = -1; 
			box->height = -1;
			box->width = -1;
			box->found = false;
			box->minval = minval;
		}

		//for debug:
		if(_debug) imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("iconFound.bmp"))).ToPointer(),clone);

		//release all resources
		src_gray.release();
		tpl_gray.release();
		res.release();
		cvReleaseImage(&tmp);
		cvReleaseImage(&tmpIcon);
		//debug
		clone.release();

		 return box;
	}

	Bitmap^ Core::ReplaceColor(Bitmap^ bitmapInput, int oldR, int oldG, int oldB, int newR, int newG, int newB)
	{
		IplImage * tmp;

		BitmapData^ bmData = bitmapInput->LockBits(System::Drawing::Rectangle(0, 0, bitmapInput->Width, bitmapInput->Height),
		
		System::Drawing::Imaging::ImageLockMode::ReadWrite, bitmapInput->PixelFormat);


		tmp = cvCreateImage(cvSize(bitmapInput->Width , bitmapInput->Height) , IPL_DEPTH_8U , 3);
		tmp->imageData = (char*)bmData->Scan0.ToPointer();

		bitmapInput->UnlockBits(bmData);


		//IplImage * tmp = _inputImage;

		Mat3b src = tmp;
		for (Mat3b::iterator it = src.begin(); it != src.end(); it++)
		{
			if (*it == Vec3b(oldB, oldG, oldR))
			{
				*it = Vec3b(newB, newG, newR);
			}
		}

		imwrite((const char*)(Marshal::StringToHGlobalAnsi(_debugFolder + gcnew System::String("colorReplaced.bmp"))).ToPointer(),src);

		
		Bitmap^ bitmap;
		
		bitmap = gcnew Bitmap( tmp->width, tmp->height, PixelFormat::Format24bppRgb);
 
		System::Drawing::Imaging::BitmapData ^data = bitmap->LockBits(System::Drawing::Rectangle(0, 0, bitmap->Width, bitmap->Height), System::Drawing::Imaging::ImageLockMode::ReadWrite,  bitmap->PixelFormat);
		memcpy( data->Scan0.ToPointer(), tmp->imageData, tmp->imageSize );
		bitmap->UnlockBits( data );

		src.release();
		cvReleaseImage(&tmp);

		return bitmap;
	}
	
	/*Core::Box^ Core::FindIcon(Bitmap^ icon)
	{
		//IplImage * tmp = _inputImage;
		//IplImage *tmp = cvCreateImage( cvSize(_inputImage.width, _inputImage.height), _inputImage.depth, _inputImage.nChannels );
		IplImage *tmp = cvCreateImage(cvGetSize(_inputImage),_inputImage->depth, _inputImage->nChannels);
		cvCopy(_inputImage,tmp);
		Mat src;
		Mat tpl;

		ImageCodecInfo^ jgpEncoder = GetEncoder(ImageFormat::Jpeg);

		System::Drawing::Imaging::Encoder^ myEncoder =
                        System::Drawing::Imaging::Encoder::Quality;

		EncoderParameters^ myEncoderParameters = gcnew EncoderParameters(1);

		EncoderParameter^ myEncoderParameter = gcnew EncoderParameter(myEncoder, (__int64)100);

        myEncoderParameters->Param[0] = myEncoderParameter;

		MemoryStream^ stream = gcnew MemoryStream();

		icon->Save(stream, jgpEncoder, myEncoderParameters);


		tpl = BitmapToIplImage((Bitmap^)Bitmap::FromStream(stream));

						
		 stream->~MemoryStream();
		src = tmp;

		 Mat src_gray, tpl_gray;
		cvtColor(src, src_gray, CV_BGR2GRAY);
		cvtColor(tpl, tpl_gray, CV_BGR2GRAY);


		int width  = src.cols - tpl.cols + 1;
		int height = src.rows - tpl.rows + 1;

		Mat res = Mat(height, width, CV_8U);
		matchTemplate(src_gray, tpl_gray, res, CV_TM_SQDIFF_NORMED);

		double minval, maxval;
		cv::Point  minloc, maxloc;

		minMaxLoc(res, &minval, &maxval, &minloc, &maxloc);
		
		 Core::Box^ box = gcnew Core::Box;

		 box->x = minloc.x;
		 box->y = minloc.y; 
		box->height = tpl.rows;
		box->width = tpl.cols;

		src.release();
		src_gray.release();
		tpl_gray.release();
		tpl.release();
		res.release();
		cvReleaseImage(&tmp);


		 return box;
	}*/
	
}