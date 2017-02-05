#pragma once
#include "EngineImpl.h"
using namespace System::Collections::Generic;

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
		void Convert2PseudoColor(System::String^ sOrgFile, System::String^ sDestFile);
		AnalysisResult^ Analysis(System::String^ sFile, array<MRect^>^ rects);
		//int AdaptiveThreshold(array<uchar>^ src, int width, int height, List<uchar>^% threshold);
		int SearchLights(array<uchar>^ src, int width, int height, List<List<MPoint^>^>^% contours);
		//void FindContours(array<uchar>^ arr, int width, int height, int cnt2Find);
		//void FindContours(System::String^ sFile, int cnt2Find);
	private :
		std::string IEngine::WStringToString(const std::wstring &wstr);
		template<typename T>  List<T>^  Copy2List(std::vector<T> vector);
		EngineImpl* m_EngineImpl;
		
	};



	
	

}

