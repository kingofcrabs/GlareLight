#pragma once
#include "EngineImpl.h"


namespace EngineDll
{

	public ref class Circle
	{
	public:
		int x;
		int y;
		int radius;
		
		Circle(int xx, int yy, int rr)
		{
			x = xx;
			y = yy;
			radius = rr;
		}
	};
	public ref class MPoint
	{
	public:
		int x;
		int y;
		MPoint(int xx, int yy)
		{
			x = xx;
			y = yy;
		}

	};

	public ref class MRect
	{
	public:
		MPoint^ ptStart;
		MPoint^ ptEnd;
		MRect(MPoint^ ptS, MPoint^ ptE)
		{
			ptStart = gcnew MPoint(ptS->x,ptS->y);
			ptEnd = gcnew MPoint(ptE->x,ptE->y);
		}
	};

	public ref class MSize
	{
	public:
		int x;
		int y;
	
		MSize(int xx, int yy)
		{
			x = xx;
			y = yy;
		}
	};


	public ref class AnalysisResult
	{
	public: 
		double val;
		AnalysisResult(double v)
		{
			val = v;
		}
	};




	public ref class IEngine
	{
	public:
		IEngine();
		~IEngine();
		cv::Rect2f Convert2Rect2f(MRect^ rc);
		AnalysisResult^ Analysis(System::String^ sFile, array<MRect^>^ rects);
	private :
		std::string IEngine::WStringToString(const std::wstring &wstr);
		EngineImpl* m_EngineImpl;
	};



	
	

}

