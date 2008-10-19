using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.DataParallelArrays;

namespace AcceleratorUtils
{
    public class ParallelArraysUtils
    {
        static public FloatParallelArray Append(FloatParallelArray f1, FloatParallelArray f2, int Dimension)
        {
            FloatParallelArray a = ParallelArrays.Replicate(f1, f2.Shape[0]+f1.Shape[0]);
            a = ParallelArrays.ShiftDefault(a, 0.0f, new int[] { -f2.Shape[0] });
            a = ParallelArrays.ShiftDefault(a, 0.0f, new int[] { f2.Shape[0] });
            FloatParallelArray b = ParallelArrays.Replicate(f2, f2.Shape[0] + f1.Shape[0]);
            b = ParallelArrays.ShiftDefault(b, 0.0f, new int[] { f1.Shape[0] });
            b = ParallelArrays.ShiftDefault(b, 0.0f, new int[] { -f1.Shape[0] });
            return (a + b);
        }
    }

    public class ParallelStack
    {
        private FloatParallelArray m_ContArray;
        private bool IsEmpty;

        public ParallelStack()
        {
            IsEmpty = true;
        }

        public void Push(float Element)
        {
            FloatParallelArray FPAElem = new FloatParallelArray(Element,new int[]{1});
            if (IsEmpty)
            {
                m_ContArray = FPAElem;
                IsEmpty = false;
            }
            else
            {
                m_ContArray = ParallelArraysUtils.Append(m_ContArray, FPAElem, 0);
                m_ContArray = ParallelArrays.Evaluate(m_ContArray);
            }
            
        }

        public FloatParallelArray Pop()
        {
            if (IsEmpty)
                throw new UnexpectedOperation();

            if (m_ContArray.Shape[0] - 1 == 0)
            {
                IsEmpty = true;
                return m_ContArray;
            }

            Slice slc = new Slice(0, m_ContArray.Shape[0] - 1);
            FloatParallelArray popelem = ParallelArrays.Section(m_ContArray, slc);
            m_ContArray = ParallelArrays.Replicate(m_ContArray, m_ContArray.Shape[0] - 1);
            return popelem;
        }

        public FloatParallelArray GetStackArray()
        {
            if (IsEmpty)
                throw new UnexpectedOperation();
            return m_ContArray;
        }
    }
}
