#pragma once
#include "stdafx.h"
#include <opencv2/opencv.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/imgproc/imgproc.hpp>
using namespace cv;



class EngineImpl
{
public:
	double f;         //len's focus
	double pixelUnit; //camera's pixel unit um

	EngineImpl();
	Mat img;
	double CalculateGlare(std::string sFile,std::vector<Rect2f> rc);
	void Convert2PesudoColor(std::string srcFile, std::string destFile);
	void  FindContours(std::string sFile,
		std::vector<std::vector<cv::Point>
		>& contours,
		int min, int max, int cnt2Find);
private:
	double CalculateOmega(int x, int y);
	double CalculateGuthPosition(int x, int y);
	double  GetDistance(double x1, double y1, double x2, double y2);
	
};

