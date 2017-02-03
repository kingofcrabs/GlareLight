
#include "stdafx.h"
#include "IEngine.h"
#include <msclr\marshal_cppstd.h>
using namespace System;
using namespace System::IO;

namespace EngineDll
{
	IEngine::IEngine()
	{
		m_EngineImpl = new EngineImpl();
	}

	IEngine::~IEngine()
	{
		delete m_EngineImpl;
	}

	std::string IEngine::WStringToString(const std::wstring &wstr)
	{
		std::string str(wstr.length(), ' ');
		std::copy(wstr.begin(), wstr.end(), str.begin());
		return str;
	}

	cv::Rect2f IEngine::Convert2Rect2f(MRect^ rc)
	{
		cv::Point2f ptStart(rc->ptStart->x, rc->ptStart->y);
		cv::Point2f ptEnd(rc->ptEnd->x, rc->ptEnd->y);
		return cv::Rect2f(ptStart, ptEnd);
	}

	AnalysisResult^ IEngine::Analysis(System::String^ sFile, array<MRect^>^ rects)
	{
		std::string nativeFileName = msclr::interop::marshal_as< std::string >(sFile);
		std::vector<cv::Rect2f> nativeRects;
		for (int i = 0; i < rects->Length; i++)
		{
			cv::Rect2f nativeRect = Convert2Rect2f(rects[i]);
			nativeRects.push_back(nativeRect);
		}
			
		double gVal = m_EngineImpl->CalculateGlare(nativeFileName, nativeRects);
		return gcnew AnalysisResult(gVal);
	}

	void IEngine::Convert2PseudoColor(System::String^ sOrgFile, System::String^ sDestFile)
	{
		std::string nativeSourceFileName = msclr::interop::marshal_as< std::string >(sOrgFile);
		std::string nativeDestFileName = msclr::interop::marshal_as< std::string >(sDestFile);
		m_EngineImpl->Convert2PesudoColor(nativeSourceFileName, nativeDestFileName);
	}


	void IEngine::FindContours(System::String^ sFile, int cnt2Find)
	{
		std::string nativeSourceFileName = msclr::interop::marshal_as< std::string >(sFile);
		std::vector<std::vector<cv::Point>> contours;
		m_EngineImpl->FindContours(nativeSourceFileName, contours, 100, 1000, 3);
	}

}
