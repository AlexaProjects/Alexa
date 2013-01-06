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

// Alexa.h

#pragma once

#include <stdio.h>
#include <iostream>
#include <cv.h>
#include <highgui.h>
#include "opencv2/core/core.hpp"
#include "opencv2/features2d/features2d.hpp"
#include "opencv2/highgui/highgui.hpp"
#include "opencv2/imgproc/imgproc.hpp"
#include "opencv2/calib3d/calib3d.hpp"

using namespace System;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::Collections;
using namespace System::Collections::Generic;

namespace Alexa {

	public ref class Core
	{

	public:
		
		ref struct Box
		{
			int x;
			int y;
			int height;
			int width;
		};

		ref struct IconBox
		{
			bool found;
			double minval;
			int x;
			int y;
			int height;
			int width;
		};

		/// <summary>
		/// Set the input image of the Alexa Core
		/// </summary>
		/// <param name="inputImage">the input image</param>
		void SetSourceImage(Bitmap^ inputImage);

		/// <summary>
		/// Enable/disable the debug level
		/// </summary>
		/// <param name="enable">set true to enable debug</param>
		void EnableDebug(bool enable);

		/// <summary>
		/// Set the debug folder to save debug image
		/// </summary>
		/// <param name="path">the path of debug folder</param>
		void SetDebugFolder(System::String^ path);
		
		/// <summary>
		/// Get the Alexa Core source image
		/// </summary>
		/// <returns> Returns the source image</returns>
		Bitmap^ GetSourceImage();

		/// <summary>
		/// Change brightness and contrast of the Alexa Core source image
		/// </summary>
		/// <param name="brightness">brightness value</param>
		/// <param name="contrast">contrast value</param>
		void SetBrightnessContrast(int brightness, int contrast);

		/// <summary>
		/// Find all the boxes (in the Alexa Core source image) that match the arguments.
		/// </summary>
		/// <param name="height">height of input box (pixel)</param>
		/// <param name="width">width of input box (pixel)</param>
		/// <param name="tollerance">tollerance of height and width (pixel)</param>
		/// <returns> Returns all input boxes found</returns>
		List<Box^>^ GetGenericBoxes(int height, int width, int tollerance);

		/// <summary>
		/// Find all the boxes (in the Alexa Core source image) that match the arguments.
		/// </summary>
		/// <param name="height">height of input box (pixel)</param>
		/// <param name="width">width of input box (pixel)</param>
		/// <param name="tollerance">tollerance of height and width (pixel)</param>
		/// <returns> Returns all input boxes found</returns>
		List<Box^>^ GetGenericBoxesV2(int height, int width, int tollerance);

		/// <summary>
		/// Find all the input boxes in the Alexa Core source image.
		/// </summary>
		/// <returns> Returns all input boxes found</returns>
		List<Box^>^ GetInputBoxes();

		/// <summary>
		/// Find all the input boxes in the Alexa Core source image.
		/// </summary>
		/// <returns> Returns all input boxes found</returns>
		List<Box^>^ GetInputBoxesV2();

		/// <summary>
		/// Find all buttons in the Alexa Core source image.
		/// </summary>
		/// <returns> Returns buttons found</returns>
		List<Box^>^ GetButtons();

		/// <summary>
		/// Find all buttons in the Alexa Core source image.
		/// </summary>
		/// <returns> Returns buttons found</returns>
		List<Box^>^ GetButtonsV2();

		/// <summary>
		/// Find all list boxes in the Alexa Core source image.
		/// </summary>
		/// <returns> Returns all List Boxes found</returns>
		List<Box^>^ GetIconListBoxes();

		/// <summary>
		/// Find all list boxes in the Alexa Core source image.
		/// </summary>
		/// <returns> Returns all List Boxes found</returns>
		List<Box^>^ GetIconListBoxesV2();

		/// <summary>
		/// Find all chars in the Alexa Core source image.
		/// </summary>
		/// <param name="lineHeight">line height</param>
		/// <param name="spaceThickness">space thickness</param>
		/// <returns> Returns all chars found</returns>
		List<Box^>^ GetChars(int lineThickness, int spaceThickness, int r, int g, int b);

		/// <summary>
		/// Find all chars in the Alexa Core source image.
		/// </summary>
		/// <param name="lineHeight">line height</param>
		/// <param name="spaceThickness">space thickness</param>
		/// <returns> Returns all chars found</returns>
		List<Box^>^ GetCharsV2(int lineThickness, int spaceThickness, int r, int g, int b);

		/// <summary>
		/// Find all chars in the Alexa Core source image.
		/// </summary>
		/// <param name="minHeight">min word height</param>
		/// <param name="maxHeight">max word height</param>
		/// <param name="minWidth">min word width</param>
		/// <param name="maxWidth">max word width</param>
		/// <returns> Returns all words found</returns>
		List<Box^>^ GetWords(int minHeight, int maxHeight, int minWidth, int maxWidth);

		/// <summary>
		/// Find all chars in the Alexa Core source image.
		/// </summary>
		/// <param name="minHeight">min word height</param>
		/// <param name="maxHeight">max word height</param>
		/// <param name="minWidth">min word width</param>
		/// <param name="maxWidth">max word width</param>
		/// <returns> Returns all words found</returns>
		List<Box^>^ GetWordsV2(int minHeight, int maxHeight, int minWidth, int maxWidth);

		/// <summary>
		/// Find all boxes in the Alexa Core source image.
		/// </summary>
		/// <returns> Returns all boxes</returns>
		List<Core::Box^>^ GetInterestPoints();

		/// <summary>
		/// Find the icon in the Alexa Core source image.
		/// </summary>
		/// <param name="icon">the icon to find</param>
		/// <param name="threshold">the threshold</param>
		/// <returns>Returns the coordinates of the icon</returns>
		IconBox^ FindIcon(Bitmap^ icon, double threshold);

		/// <summary>
		/// Binarize the Alexa Core source image
		/// </summary>
		void BinarizeImage();

		/// <summary>
		/// Replace color into the Alexa Core source image
		/// </summary>
		/// <param name="oldR">old red component</param>
		/// <param name="oldG">old green component</param>
		/// <param name="oldB">old blue component</param>
		/// <param name="newR">new red component</param>
		/// <param name="newG">new green component</param>
		/// <param name="newB">new blue component</param>
		Bitmap^ ReplaceColor(Bitmap^ bitmap, int oldR, int oldG, int oldB, int newR, int newG, int newB);

		/// <summary>
		/// Release all objects of Alexa Core
		/// </summary>
		void Release();

	private: 
		int genBoxHeight;
		int genBoxWidth;
		int genBoxTollerance;
		int boxLineThickness;
		int boxSpaceThickness;
		int boxMinHeight;
		int boxMaxHeight;
		int boxMinWidth;
		int boxMaxWidth;

		int charLinesColorR;
		int charLinesColorG;
		int charLinesColorB;

		IplImage * _inputImage;

		bool _debug;
		System::String^ _debugFolder;

		//transform a Bitmap to an IplImage
		IplImage* BitmapToIplImage(Bitmap^ bitmap);

		ImageCodecInfo^ GetEncoder(ImageFormat^ format);

		/// <summary>
		/// Find boxes in the Alexa Core source image.
		/// </summary>
		/// <returns> Returns all boxes</returns>
		List<Core::Box^>^ GetBoxes(int boxType);

		/// <summary>
		/// Find boxes in the Alexa Core source image.
		/// </summary>
		/// <returns> Returns all boxes</returns>
		List<Core::Box^>^ GetBoxes2(int boxType);

	};
}
