#include "stdafx.h"
#include "EngineImpl.h"

using namespace std;
using namespace cv;
static string dbgFolder = "d:\\temp\\";
static int innerRadius = 30;
static Mat img;

EngineImpl::EngineImpl()
{
	f = 20/1000; //mm
	thresholdVal = 128;
	//1536 * 1024
	min = 20;
	max = 1000;
	pixelUnit = 9 / 1000000; //um
}

void GothroughImage(Mat& src)
{
	int height = src.rows;
	int width = src.cols;
	int channels = src.channels();
	int nc = width * channels;

	for (int y = 0; y < height; y++)
	{
		uchar *data = src.ptr(y);
		for (int x = 0; x < width; x++)
		{
			int xStart = x*channels;
			for (int i = 0; i< channels; i++)
				data[xStart + i] = 0;
		}
	}
	return;
}

void  EngineImpl::FindContours(string sFile,
	std::vector<std::vector<cv::Point>
	>& contours,
	int min, int max, int cnt2Find)
{
	std::vector< std::vector<cv::Point> > allContours;
	auto img = cv::imread(sFile,0);
	Mat thresholdImg;
	cv::adaptiveThreshold(img, thresholdImg, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY, 7, 5);
	
	cv::findContours(thresholdImg, allContours, CV_RETR_LIST, CV_CHAIN_APPROX_NONE);
	contours.clear();
	RNG rng(12345);
	for (size_t i = 0; i<allContours.size(); i++)
	{
		int contourSize = allContours[i].size();
		
		if (contourSize > min && contourSize < max)
		{
			contours.push_back(allContours[i]);
		}
	}
	Mat drawing = Mat::zeros(thresholdImg.size(), CV_8UC3);
	for (int i = 0; i< contours.size(); i++)
	{
		Scalar color = Scalar(rng.uniform(0, 255), rng.uniform(0, 255), rng.uniform(0, 255));
		drawContours(drawing, contours, i, color, 2, 8);
	}
	cv::imshow("test", drawing);
}


void EngineImpl::FindContoursRaw(uchar* pdata, int width, int height,
	std::vector<std::vector<cv::Point>
	>& contours,
	int min, int max, int cnt2Find)
{
	Mat img = Mat(height, width, CV_8UC1, pdata);
	
}

std::string WStringToString(const std::wstring &wstr)
{
	std::string str(wstr.length(), ' ');
	std::copy(wstr.begin(), wstr.end(), str.begin());
	return str;
}


double  EngineImpl::GetDistance(double x1, double y1, double x2, double y2)
{
	double xx = (x1 - x2)*(x1 - x2);
	double yy = (y1 - y2)*(y1 - y2);
	return sqrt(xx + yy);
}



double EngineImpl::CalculateGuthPosition(int x, int y)
{
	//Ｈ　＝＞　ｙ
	//Ｔ　＝＞　ｘ
	//Ｒ　＝＞　ｆ
	double H2R = y / f;
	double T2R = x / f;
	return 1;
}



void EngineImpl::Convert2PesudoColor(std::string srcFile, std::string destFile)
{
	Mat orgImg = imread(srcFile);
	Mat color;
	applyColorMap(orgImg, color, COLORMAP_JET);
	imwrite(destFile,color);
}


double EngineImpl::CalculateOmega(int x, int y)
{
	int width = img.size().width;
	int height = img.size().height;
	double xDis = abs(x - width / 2) * pixelUnit;
	double yDis = abs(y - height / 2) * pixelUnit;
	double dis = sqrt(xDis*xDis + yDis * yDis);
	double r = sqrt(xDis*xDis + yDis * yDis + f*f);
	double cosθ = f / r;
	double Ap = pixelUnit * pixelUnit * cosθ;
	double ω = Ap / (r*r);
	return ω;
}

double EngineImpl::CalculateGlare(std::string sFile, std::vector<cv::Rect2f> rects)
{
	img = imread(sFile);
	imshow(sFile, img);
	return 5;
}

void on_trackbar(int val, void* parent)
{
	int thresholdVal = val;
	string winName = "threshold";
	Mat thresholdImg;
	std::vector< std::vector<cv::Point> > allContours;
	cv::threshold(img, thresholdImg, thresholdVal, 255, CV_THRESH_BINARY);
	cv::findContours(thresholdImg, allContours, CV_RETR_LIST, CV_CHAIN_APPROX_NONE);
	EngineImpl* pEngine = (EngineImpl*)parent;
	pEngine->contours.clear();
	RNG rng(12345);
	for (size_t i = 0; i<allContours.size(); i++)
	{
		int contourSize = allContours[i].size();

		if (contourSize > pEngine->min && contourSize < pEngine->max)
		{
			std::vector<cv::Point> polygon;
			//double len = arcLength(allContours[i], true);
			approxPolyDP(allContours[i], polygon, 1, true);
			pEngine->contours.push_back(polygon);
		}
	}
	Mat drawing = Mat::zeros(thresholdImg.size(), CV_8UC3);
	for (int i = 0; i< pEngine->contours.size(); i++)
	{
		Scalar color = Scalar(rng.uniform(0, 255), rng.uniform(0, 255), rng.uniform(0, 255));
		drawContours(drawing, pEngine->contours, i, color, 2, 8);
	}
	
	imshow(winName, drawing);
	//imshow(winName, thresholdImg);
}

int EngineImpl::SearchLights(uchar* pdata, int width, int height, int min, int max, std::vector<std::vector<cv::Point>>& contours2Find)
{
	img = Mat(height, width, CV_8UC1, pdata);
	Mat thresholdImg;
	thresholdVal = cv::threshold(img, thresholdImg, 0, 255, CV_THRESH_BINARY | CV_THRESH_OTSU);
	//cv::adaptiveThreshold(img, thresholdImg, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY, 7, 5);

	string winName = "threshold";
	namedWindow(winName, WINDOW_NORMAL);
	resizeWindow(winName, 800, 800 * img.cols / img.rows);
	imshow(winName, thresholdImg);

	//vector.resize(img.rows*img.cols);
	string sliderName = "slider";
	createTrackbar(sliderName, winName, &thresholdVal, 255, on_trackbar,this);
	waitKey(0);
	cv::threshold(img, thresholdImg, thresholdVal, 255, CV_THRESH_BINARY);
	//vector.assign(thresholdImg.datastart, thresholdImg.dataend);

	contours2Find = contours;
	return thresholdVal;
}

int EngineImpl::AdaptiveThreshold(uchar* pdata,int width, int height, std::vector<uchar>& vector)
{
	img = Mat(height, width, CV_8UC1, pdata);
	Mat thresholdImg;
	thresholdVal = cv::threshold(img, thresholdImg, 0, 255, CV_THRESH_BINARY | CV_THRESH_OTSU);
	//cv::adaptiveThreshold(img, thresholdImg, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY, 7, 5);
	
	string winName = "threshold";
	namedWindow(winName, WINDOW_NORMAL);
	resizeWindow(winName, 800, 800* img.cols/img.rows);
	imshow(winName, thresholdImg);
	
	vector.resize(img.rows*img.cols);
	string sliderName = "slider";
	createTrackbar(sliderName, winName, &thresholdVal, 255, on_trackbar);
	waitKey(0);
	cv::threshold(img, thresholdImg, thresholdVal, 255, CV_THRESH_BINARY);
	vector.assign(thresholdImg.datastart, thresholdImg.dataend);
	return thresholdVal;
}