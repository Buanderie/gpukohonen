using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Research.DataParallelArrays;

namespace GPUKohonenLib
{
    public class MSRAcceleratorKohonenCore : IKohonenCore
    {
        private DisposableFloatParallelArray m_GPUWeight;
        private DisposableFloatParallelArray m_GPUInput;
        
        //Constructor
        public MSRAcceleratorKohonenCore():base()
        {
            
        }

        public override void Init(KohonenSOM parent)
        {
            this.m_Parent = parent;
            float[,] globalInput = new float[m_Parent.DataSource.GetPatternCount(), m_Parent.DataSource.GetPatternLength()];
            for (int i = 0; i < m_Parent.DataSource.PatternCount; ++i)
                for (int j = 0; j < m_Parent.DataSource.GetPatternLength(); ++j)
                {
                    globalInput[i, j] = m_Parent.DataSource.GetPattern(i)[j];
                }

            m_GPUInput = new DisposableFloatParallelArray(globalInput);
            m_GPUWeight = new DisposableFloatParallelArray(m_Parent.NeuronMap);
        }

        public override void FindBMU()
        {
            //How do the vectors look like ? :/
            float[,] test2d;
            float[] test;
            ParallelArrays.ToArray(m_GPUInput, out test2d);

            //Normalize the weight vector
            FloatParallelArray transpose = ParallelArrays.Transpose(m_GPUWeight, 1, 0);
            FloatParallelArray weightsq = ParallelArrays.InnerProduct(m_GPUWeight, ParallelArrays.Transpose(m_GPUWeight, 1, 0));
            FloatParallelArray weightsum = ParallelArrays.Sum(weightsq, 0);
            FloatParallelArray weightlength = ParallelArrays.Sqrt(weightsum);
            weightlength = ParallelArrays.Stretch(ParallelArrays.AddDimension(weightlength, 1), 1, m_Parent.DataSource.PatternLength);
            FloatParallelArray weightnorm = ParallelArrays.Divide(m_GPUWeight, weightlength);
            weightnorm = ParallelArrays.Transpose(weightnorm, 1, 0);

            //Normalize the input vector
            FloatParallelArray inputsq = ParallelArrays.InnerProduct(m_GPUInput, ParallelArrays.Transpose(m_GPUInput,1,0));
            FloatParallelArray inputsum = ParallelArrays.Sum(inputsq, 0);
            FloatParallelArray inputlength = ParallelArrays.Sqrt(inputsum);
            inputlength = ParallelArrays.Stretch(ParallelArrays.AddDimension(inputlength, 1), 1, m_Parent.DataSource.PatternLength);
            FloatParallelArray inputnorm = ParallelArrays.Divide(m_GPUInput, inputlength);
            //inputnorm = ParallelArrays.Transpose(inputnorm, 1, 0);

            FloatParallelArray pacc = ParallelArrays.InnerProduct(inputnorm, weightnorm);
            FloatParallelArray bmxval = ParallelArrays.MaxVal(pacc, 1);
            bmxval = ParallelArrays.AddDimension(bmxval, 1);
            bmxval = ParallelArrays.Stretch(bmxval, 1, 2500);

            //ParallelArrays.ToArray(pwinner, out test2d);
            ParallelArrays.ToArray(bmxval, out test2d);
            int popopo = 34;

        }

        public override void DoEpoch(float t, float round_t)
        {
            float[] pol = null;
            this.FindBMU();
        }
    }
}
