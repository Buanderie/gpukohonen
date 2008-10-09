using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Research.DataParallelArrays;

namespace GPUKohonenLib
{
    public class MSRAcceleratorKohonenCore : IKohonenCore
    {
        private DisposableFloatParallelArray m_dGPUWeight;
        private FloatParallelArray m_GPUWeight;
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
        }

        public override void DoEpoch(float t, float round_t)
        {
            float[,] test2d;
            float[] test;

            this.FindBMU();

            //Slice the pwinner row by row and do some great stuff
            m_PWinner = ParallelArrays.Evaluate(m_PWinner);

            Slice[] slices = new Slice[2];
            for (int i = 0; i < m_Parent.DataSource.PatternCount; ++i)
            {
                slices[1] = new Slice(0, m_Parent.NeuronMap.GetLength(0));
                slices[0] = new Slice(i,1);

                FloatParallelArray s = ParallelArrays.Section(m_PWinner, slices);
                s = ParallelArrays.Evaluate(s);
                FloatParallelArray bmuw = ParallelArrays.DropDimension( ParallelArrays.InnerProduct(s, m_GPUWeight), 0);
                FloatParallelArray bmuc = ParallelArrays.InnerProduct(s, m_GPUCoord);

                //Compute distances to bmu
                DisposableFloatParallelArray bmucEvaluated = ParallelArrays.Evaluate(bmuc);     //Workaround
                bmuc = ParallelArrays.Stretch(bmucEvaluated, m_Parent.NeuronMap.GetLength(0), 1);
                FloatParallelArray diff = ParallelArrays.Subtract(m_GPUCoord, bmuc);
                FloatParallelArray dist = ParallelArrays.Multiply(diff,diff);
                dist = ParallelArrays.Sum(dist, 1);
                dist = ParallelArrays.Multiply(-1.0f, dist);

                //Apply update formula
                FloatParallelArray constE = new FloatParallelArray((float)(Math.E), m_Parent.NeuronMap.GetLength(0));
                FloatParallelArray sigma = new FloatParallelArray((float)(Math.Pow(Neighborhood(t, round_t)*0.85, 2)), m_Parent.NeuronMap.GetLength(0));
                FloatParallelArray lrate = new FloatParallelArray((float)LearningRate(t, round_t), m_Parent.NeuronMap.GetLength(0), m_Parent.DataSource.PatternLength);
                FloatParallelArray omeg = ParallelArrays.Divide(dist, sigma);
                
                omeg = ParallelArrays.Pow(constE, omeg);
                DisposableFloatParallelArray domeg = ParallelArrays.Evaluate(omeg);         //Workaround
                omeg = ParallelArrays.AddDimension(domeg, 1);                               
                omeg = ParallelArrays.Stretch(omeg, 1, m_Parent.DataSource.PatternLength);
                FloatParallelArray sbmuw = ParallelArrays.AddDimension(bmuw,0);
                sbmuw = ParallelArrays.Stretch( sbmuw, m_Parent.NeuronMap.GetLength(0), 1 );

                m_GPUWeight = ((m_GPUWeight + ((sbmuw - m_GPUWeight)* omeg * lrate)));
            }
            m_GPUWeight = ParallelArrays.Evaluate(m_GPUWeight);
        }

        private float LearningRate(float t, float round_t)
        {
            float m_epsiloninitial_val = 0.1f;
            float m_theta_val = 100.0f / (float)Math.Log(25);
            return m_epsiloninitial_val * (float)Math.Exp(-(double)t / m_theta_val);
        }

        private float BMUInfluence(float t, float round_t)
        {
            return 1.0f;
        }

        private float Neighborhood(float t, float round_t)
        {
            float m_theta_val = 100.0f / (float)Math.Log(25);
            float m_epsiloninitial_val = 0.1f;
            float m_sigmainitial_val = 25;
            return m_sigmainitial_val * (float)Math.Exp(-(double)t / m_theta_val);
        }

        public override void Terminate()
        {
            float[,] tmp;
            ParallelArrays.ToArray(m_GPUWeight, out tmp);
            m_Parent.NeuronMap = tmp;
        }
    }
}
