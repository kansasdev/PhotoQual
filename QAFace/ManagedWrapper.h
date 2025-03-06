#pragma once
#pragma once
using namespace System;
namespace QAFace {

    template<class T>
    public ref class ManagedWrapper
    {
    protected:
        T* m_Instance;
    public:
        ManagedWrapper(T* instance)
            : m_Instance(instance)
        {
        }
        virtual ~ManagedWrapper()
        {
            if (m_Instance != nullptr)
            {
                delete m_Instance;
            }
        }
        !ManagedWrapper()
        {
            if (m_Instance != nullptr)
            {
                delete m_Instance;
            }
        }
        T* GetInstance()
        {
            return m_Instance;
        }
    };
}

