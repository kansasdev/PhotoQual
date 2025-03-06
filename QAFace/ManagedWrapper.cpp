#include "ManagedWrapper.h"
#include "FaceAnalysis.h"

namespace QAFace
{
    public ref class FaceAnalysisEntity : public ManagedWrapper<FaceAnalysis>
    {
    public:
        FaceAnalysisEntity() :ManagedWrapper(new FaceAnalysis()) {
        }
        
        int CalculateQuality()
        {
            int result = m_Instance->CalculateQuality();
            return result;
        }

        String^ GetExceptionMessage()
        {
            std::string exceptionMessage = m_Instance->GetExceptionMessage();
            return gcnew String(exceptionMessage.c_str());
        }

        String^ GetQualityResults()
        {
            std::string qualityResults = m_Instance->GetQualityResults();
            return gcnew String(qualityResults.c_str());
        }

		void SetExceptionMessage()
		{
			m_Instance->SetExceptionMessage();
		}

        void SetQualityResults()
        {
			m_Instance->SetQualityResults();
        }
       
    };
}