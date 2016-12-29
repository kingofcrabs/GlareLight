#pragma once
#include "stdafx.h"




class EngineImpl
{
public:
	double f;         //len's focus
	double pixelUnit; //camera's pixel unit um

	EngineImpl();
	Mat img;
	double CalculateGlare(std::string sFile,std::vector<Rect2f> rc);
	
private:
	double CalculateOmega(int x, int y);
	double CalculateGuthPosition(int x, int y);
	double  GetDistance(double x1, double y1, double x2, double y2);
};

