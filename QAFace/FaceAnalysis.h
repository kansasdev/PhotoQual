#undef _HAS_STD_BYTE
#include "ofiq_lib.h"
#include "image_io.h"
#include "utils.h"

#include <cstring>
#include <fstream>
#include <iostream>
#include <sstream>
#include <iterator>
#include <algorithm>
#include <cmath>
//#include <magic_enum.hpp>
#include <filesystem>


class FaceAnalysis
{
private:
	std::string ExceptionMessage;
	std::string QualityResultsScalar;
	std::string QualityResultsRaw;
	std::string MeasureAttributeWithValue;
	int getQualityAssessmentResults(
		std::shared_ptr<OFIQ::Interface>& implPtr,
		std::string& imageFile,
		OFIQ::FaceImageQualityAssessment& assessments);
	std::string exportAssessmentResultsToString(const OFIQ::FaceImageQualityAssessment& assessments, bool doExportScalar);
	bool isInitialized = false;
	std::shared_ptr<OFIQ::Interface> faceLib;

public:
	FaceAnalysis();
	int CalculateQuality();
	std::string GetExceptionMessage();
	std::string GetQualityResults();
	void SetQualityResults();
	void SetExceptionMessage();
};
