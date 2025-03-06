
#include "stdafx.h"
#include "FaceAnalysis.h"
#include "image_io.h"
#include "utils.h"
#include <ShlObj.h>
#include <magic_enum.hpp>
#include <string_view>


// ...
FaceAnalysis::FaceAnalysis()
{

}

int FaceAnalysis::CalculateQuality()
{
	try
	{
		OFIQ::ReturnStatus ret = OFIQ::ReturnCode::Success;
		if (!FaceAnalysis::isInitialized)
		{
			FaceAnalysis::faceLib = OFIQ::Interface::getImplementation();


			string configDir = "data";
			string configvalue = "ofiq_config.jaxn";
			int zmienna = 5;
			ret = faceLib->initialize(configDir, configvalue);
		}
		
		if (ret.code == OFIQ::ReturnCode::Success)
		{
			FaceAnalysis::isInitialized = true;
			wchar_t mypicturespath[MAX_PATH] = L"";
			HRESULT result = SHGetFolderPathW(NULL, CSIDL_MYPICTURES, NULL, SHGFP_TYPE_CURRENT, mypicturespath);
			wchar_t currentDirAndFile[MAX_PATH] = L"";
			wcscpy(currentDirAndFile, mypicturespath);
			wcscat(currentDirAndFile, L"\\");
			wcscat(currentDirAndFile, L"face.bmp");

			wstring ws(currentDirAndFile);
			
			string fstr(ws.begin(), ws.end());

			
				OFIQ::FaceImageQualityAssessment assessment;
				int resultCode = getQualityAssessmentResults(FaceAnalysis::faceLib, fstr, assessment);
				if (resultCode == 1)
				{
					QualityResultsScalar = exportAssessmentResultsToString(assessment, true);
					QualityResultsRaw = exportAssessmentResultsToString(assessment, false);
					
				}
				else
				{
					throw std::runtime_error("Error in quality assessment");
				}
				for (const auto& [measure, measure_result] : assessment.qAssessments)
				{
					auto mName = static_cast<std::string>(magic_enum::enum_name(measure));
					auto rawScore = measure_result.rawScore;
					auto scalarScore = measure_result.scalar;
					if (measure_result.code != OFIQ::QualityMeasureReturnCode::Success)
					{
						scalarScore = -1;
					}

					MeasureAttributeWithValue = MeasureAttributeWithValue  + std::to_string(scalarScore) + "% score "+std::to_string(rawScore)+"|";
				}
			

			return 1;
		}
		else
		{
			FaceAnalysis::isInitialized = false;
			std::string str = std::string(magic_enum::enum_name(ret.code));
			throw std::runtime_error(str);
		}

	}
	catch (exception& e)
	{
		ExceptionMessage = std::string(e.what());
		
		return -1;
	}
}


int FaceAnalysis::getQualityAssessmentResults(
	std::shared_ptr<OFIQ::Interface>& implPtr,
	std::string& imageFile,
OFIQ::FaceImageQualityAssessment& assessments)
{
	OFIQ::Image image;
	OFIQ::ReturnStatus retStatus = OFIQ_LIB::readImage(imageFile, image);

	OFIQ::ReturnStatus retStatus2 = implPtr->vectorQuality(image, assessments);

	double quality;
	OFIQ::ReturnStatus retStatus3 = implPtr->scalarQuality(image, quality);
	//std::cout << "Image file: '" << inputFile << "' scalar quality: " << quality << std::endl;
	if (retStatus2.code == OFIQ::ReturnCode::Success)
	{
		QualityResultsScalar = QualityResultsScalar + "Quality: " + std::to_string(quality);
		return retStatus.code == OFIQ::ReturnCode::Success ? 1 : 0;
	}
	else
	{
		return 0;
	}

}

string FaceAnalysis::exportAssessmentResultsToString(
	const OFIQ::FaceImageQualityAssessment& assessments,
	bool doExportScalar)
{
	std::string resultStr;
	char buf[200] = {};
	for (auto const& aResult : assessments.qAssessments)
	{
		//const QualityMeasure& measure = aResult.first;
		const OFIQ::QualityMeasureResult& qaResult = aResult.second;
		double val = doExportScalar ? qaResult.scalar : qaResult.rawScore;
		if (round(val) == val)
			snprintf(buf, 200, "%d;", (int)val);
		else
			snprintf(buf, 200, "%f;", val);
		resultStr += std::string(buf);
	}
	return resultStr;
}


string FaceAnalysis::GetExceptionMessage()
{
	
	return ExceptionMessage;
	
}

string FaceAnalysis::GetQualityResults()
{
	return MeasureAttributeWithValue;//QualityResultsScalar + "|" + QualityResultsRaw;
}

void FaceAnalysis::SetExceptionMessage()
{
	ExceptionMessage = "";
}

void FaceAnalysis::SetQualityResults()
{
	QualityResultsScalar = "";
	QualityResultsRaw = "";
	MeasureAttributeWithValue = "";
}