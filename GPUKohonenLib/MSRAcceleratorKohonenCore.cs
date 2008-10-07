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
        private DisposableFloatParallelArray m_GPUCoord;
        private FloatParallelArray m_PWinner;

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
            m_GPUCoord = new DisposableFloatParallelArray(m_Parent.NeuronMapCoordArray);
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

            FloatParallelArray pacc = ParallelArrays.InnerProduct(inputnorm, weightnorm);

            //Replication bug here...
            FloatParallelArray bmxval = ParallelArrays.MaxVal(pacc, 1);
            //MSR Vivian Swelson workaround
            DisposableFloatParallelArray bmxvalEvaluated = ParallelArrays.Evaluate(bmxval);
            bmxval = ParallelArrays.AddDimension(bmxvalEvaluated, 1);
            bmxval = ParallelArrays.Stretch(bmxval, 1, m_Parent.NeuronMap.GetLength(0));
            
            //Winner matrix (0 = winner)
            FloatParallelArray pwinner = ParallelArrays.Subtract(pacc, bmxval);

            //Convert to 1 = winner, 0 otherwise
            FloatParallelArray zero = new FloatParallelArray(0.0f, pwinner.Shape);
            FloatParallelArray one = new FloatParallelArray(1.0f, pwinner.Shape);
            BoolParallelArray bmask = ParallelArrays.CompareEqual(pwinner, zero);
            m_PWinner = ParallelArrays.Cond(bmask, one, zero);

            //ParallelArrays.ToArray(pwinner, out test2d);

            int popopo = 34;

        }

        public override void DoEpoch(float t, float round_t)
        {
            float[,] test2d;
            float[] test;
            this.FindBMU();

            //Slice the pwinner row by row and do some great stuff
            Slice[] slices = new Slice[2];
            for (int i = 0; i < m_Parent.DataSource.PatternCount; ++i)
            {
                slices[1] = new Slice(0, m_Parent.NeuronMap.GetLength(0));
                slices[0] = new Slice(i,1);
                FloatParallelArray s = ParallelArrays.Section(m_PWinner, slices);
                FloatParallelArray bmuw = ParallelArrays.DropDimension( ParallelArrays.InnerProduct(s, m_GPUWeight), 0);
                FloatParallelArray bmuc = ParallelArrays.InnerProduct(s, m_GPUCoord);
                
                //Compute distances to bmu
                bmuc = ParallelArrays.Stretch(bmuc, m_Parent.NeuronMap.GetLength(0), 1);
                FloatParallelArray diff = ParallelArrays.Subtract(m_GPUCoord, bmuc);
                FloatParallelArray dist = ParallelArrays.Pow2(diff);
                dist = ParallelArrays.Sum(dist, 1);
                dist = ParallelArrays.Sqrt(dist);

                //Apply update formula

                //Debug output
                ParallelArrays.ToArray(dist, out test);
                int popopo = 34;
            }
        }
    }
}
