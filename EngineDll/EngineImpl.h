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
	
	double CalculateGlare(std::string sFile,std::vector<Rect2f> rc);
	void Convert2PesudoColor(std::string srcFile, std::string destFile);
	
	int AdaptiveThreshold(uchar*, int width, int height,std::vector<uchar>& vector);
	int SearchLights(uchar* pdata, int width, int height, int min, int max, std::vector<std::vector<cv::Point>>& contours);
	std::vector<std::vector<Point>> contours;
	int max, min;
private:
	double CalculateOmega(int x, int y);
	double CalculateGuthPosition(int x, int y);
	double  GetDistance(double x1, double y1, double x2, double y2);
	void  FindContours(std::string sFile,
		std::vector<std::vector<cv::Point>
		>& contours,
		int min, int max, int cnt2Find);

	void  FindContoursRaw(uchar* pdata, int width, int height,
		std::vector<std::vector<cv::Point>
		>& contours,
		int min, int max, int cnt2Find);
	//void on_trackbar(int val, void*);
	
	int thresholdVal;
	
};

