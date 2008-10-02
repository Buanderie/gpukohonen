using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.DataParallelArrays;
using PA = Microsoft.Research.DataParallelArrays.ParallelArrays;
using FPA = Microsoft.Research.DataParallelArrays.FloatParallelArray;
using BPA = Microsoft.Research.DataParallelArrays.BoolParallelArray;
using DFPA = Microsoft.Research.DataParallelArrays.DisposableFloatParallelArray;
using F4PA = Microsoft.Research.DataParallelArrays.Float4ParallelArray;
using DF4PA = Microsoft.Research.DataParallelArrays.DisposableFloat4ParallelArray;
using System.Drawing;

namespace GPUKohonenLib
{
    public class KohonenMap
    {
        private int m_Width;
        private int m_Height;
        private int m_PatternLength;
        public FPA m_Weights;
        private GPUK_COMPUTATION_TYPE m_CompType;

        //GPU Data
        private FPA m_Shape;            
        private FPA m_Distances;
        private FPA m_BMUCoord;
        private FPA m_BMUCodeVector;
        private FPA m_CurrentPatternGPU;
        private FPA m_Theta;
        private FPA m_SigmaInitial;
        private FPA m_EpsilonInitial;
        private FPA m_MaxTime;
        private FPA m_Time;
        private FPA LogBase;

        //CPU Counterparts
        private float[,] weight_vals;
        private float[,] shape_vals;
        private float[] m_bmucoord_vals;
        private float[] m_bmucodevector_vals;
        private float[] m_CurrentPattern;
        private float m_theta_val;
        private float m_sigmainitial_val;
        private float m_epsiloninitial_val;
        private float m_time_val;
        private float m_maxtime_val;

        //Intermediate data 
        //  (FindBMU)
        float[,] zerocv_vals;
        float[,] zero_vals;
        DFPA zeros;
        DFPA zeroscv;

        public enum GPUK_COMPUTATION_TYPE { CPU_COMPUTATION_TYPE, GPU_COMPUTATION_TYPE };

        public KohonenMap(int width, int height, int pattern_length, GPUK_COMPUTATION_TYPE CompType)
        {
            m_Width = width;
            m_Height = height;
            m_PatternLength = pattern_length;
            m_CompType = CompType;
            PA.InitGPU();
            InitMap();
        }

        private void InitMap()
        {
            // MAP VALUES AND SHAPE
            Random RandomNumber = new Random();
            weight_vals = new float[m_PatternLength,m_Width * m_Height];
            for (int i = 0; i < m_Width * m_Height; ++i)
            {
                for(int k = 0; k < m_PatternLength; ++k )
                weight_vals[k, i] = (float)(RandomNumber.NextDouble()*255.0f);
            }
            
            shape_vals = new float[2,m_Width*m_Height];
            for( int i = 0; i < m_Height; i++ )
                for (int j = 0; j < m_Width; j++)
                {
                    shape_vals[0,m_Width*i + j] = j+1;
                    shape_vals[1,m_Width*i + j] = i+1;
                }

            m_Weights = new DisposableFloatParallelArray(weight_vals);
            m_Shape = new DFPA(shape_vals);
            //

            //BMU INIT
            m_bmucoord_vals = new float[2];
            m_bmucodevector_vals = new float[m_PatternLength];

            //ZEROS FOR FINDBMU
            zero_vals = new float[2, m_Width * m_Height];
            for (int i = 0; i < m_Width * m_Height; ++i)
            {
                zero_vals[0, i] = 0;
                zero_vals[1, i] = 0;
            }
            zerocv_vals = new float[m_PatternLength, m_Width * m_Height];
            for (int i = 0; i < m_Width * m_Height; ++i)
                for (int k = 0; k < m_PatternLength; ++k)
                {
                    zerocv_vals[k, i] = 0;
                    zerocv_vals[k, i] = 0;
                }

            zeroscv = new DFPA(zerocv_vals);
            zeros = new DFPA(zero_vals);
            //

            //INIT TIME
            //Don't use this
            m_Time = new DFPA(new float[] { 1 });

            //NEIGHBORHOOD VARIABLE INIT
            m_maxtime_val = 100.0f;
            m_theta_val = 100.0f / (float)Math.Log(25);
            m_sigmainitial_val = 25;
            m_epsiloninitial_val = 0.1f;
            m_Theta = new DFPA(new float[] { m_theta_val });
            m_SigmaInitial = new DFPA(new float[] { m_sigmainitial_val });
            m_EpsilonInitial = new DFPA(new float[] { m_epsiloninitial_val });
            m_MaxTime = new DFPA(new float[] { m_maxtime_val });

            //LOG BASE
            LogBase = new DFPA(new float[] { (float)(Math.E) });
            
        }//Init

        public void FindBMU()
        {
            //Useful locals
            int alen = m_Height * m_Width;

                //Compute the distances from pattern to code vectors
                FPA a = PA.AddDimension(m_CurrentPatternGPU, 1);
                FPA x = PA.Stretch(a, 1, alen);
                FPA pol = PA.Subtract(m_Weights, x);
                FPA pol2 = PA.Multiply(pol, pol);
                FPA pol3 = PA.Sum(pol2, 0);
                m_Distances = PA.Sqrt(pol3);

                //Find the minimal distance
                FPA dist2 = PA.AddDimension(m_Distances, 1);
                FPA minval = PA.MinVal(dist2, 0);

                FPA xxx = PA.Stretch(minval, alen);

                //Prepare trigger array
                BPA trigger = PA.CompareEqual(xxx, m_Distances);

                //BMU Coord ZEROS
                BPA trigger2 = PA.AddDimension(trigger, 0);
                BPA trigger3 = PA.Stretch(trigger2, 2, 1);

                //Extract BMU Coord
                FPA lol = PA.Cond(trigger3, m_Shape, zeros);
                m_BMUCoord = PA.Sum(lol, 1);
                //m_BMUCoord = PA.Evaluate(m_BMUCoord);

                //BMU Code Vector ZEROS
                BPA triggercv2 = PA.AddDimension(trigger, 0);
                BPA triggercv3 = PA.Stretch(triggercv2, m_PatternLength, 1);

                //Extract BMU code vector
                FPA mdr = PA.Cond(triggercv3, m_Weights, zeroscv);
                m_BMUCodeVector = PA.Sum(mdr, 1);
                //m_BMUCodeVector = PA.Evaluate(m_BMUCodeVector);
            

            //Begin computation ! ^^ AND OUTPUT
            PA.Evaluate(m_BMUCodeVector);
            PA.Evaluate(m_BMUCoord);
        }

        public void _CPU_FindBMU()
        {
                //Useful locals
                int alen = m_Height * m_Width;

                //Create distances array
                float[] distances = new float[alen];
                for (int i = 0; i < alen; ++i)
                {
                    double sum = 0;
                    for (int j = 0; j < m_PatternLength; ++j)
                        sum += Math.Pow(weight_vals[j, i] - m_CurrentPattern[j], 2.0f);
                    distances[i] = (float)(Math.Sqrt(sum));
                }

                //Find the minimal distance
                int min_ind = 0;
                for (int i = 0; i < alen; ++i)
                {
                    if (distances[i] < distances[min_ind])
                        min_ind = i;
                }
                //Return the BMU coords and value
                m_bmucoord_vals[0] = shape_vals[0, min_ind];
                m_bmucoord_vals[1] = shape_vals[1, min_ind];
                for (int i = 0; i < m_PatternLength; ++i)
                    m_bmucodevector_vals[i] = weight_vals[i, min_ind];
            
        }//_CPU_FindBMU

        public void SetCurrentPattern(float[] Pattern)
        {
            m_CurrentPattern = Pattern;
            m_CurrentPatternGPU = new DisposableFloatParallelArray(Pattern);
        }

        public void DoEpoch(int nb)
        {
                int alen = m_Width * m_Height;
                for (int i = 0; i < nb; ++i)
                {
                    //Neighborhood Function
                    FPA sbmuc = PA.AddDimension(m_BMUCoord, 1);
                    sbmuc = PA.Stretch(sbmuc, 1, alen);
                    sbmuc = sbmuc - m_Shape;
                    sbmuc = PA.Pow2(sbmuc);
                    FPA sqdist = PA.Sum(sbmuc, 0);
                    sqdist = PA.AddDimension(sqdist, 0);
                    sqdist = PA.Stretch(sqdist, m_PatternLength, 1);
                    //PA.Evaluate(sqdist);
                    //

                    //Learning Rate
                    FPA lrate = new FPA((float)_CPU_LearningRate(m_time_val), m_PatternLength, alen);
                    /*FPA sLearningRate = PA.AddDimension(LearningRate(m_Time), 1);
                    sLearningRate = PA.Stretch(sLearningRate, m_PatternLength, alen);*/

                    //Difference between units and current pattern
                    FPA a = PA.AddDimension(m_CurrentPatternGPU, 1);
                    FPA x = PA.Stretch(a, 1, alen);
                    FPA pol = x - m_Weights;

                    //Calcul des deltas
                    FPA deltaW = lrate * pol;
                    
                    //Mise à jour des poids
                    m_Weights = m_Weights + deltaW;

                    //Incrémente le compteur de temps
                    //m_Time = PA.Add(m_Time, 1.0f);
                    m_time_val += 1;
                }
        }

        public void _CPU_DoEpoch(int nb)
        {
            int alen = m_Width * m_Height;
            for (int g = 0; g < nb; ++g)
            {
                //For each unit
                for (int i = 0; i < alen; ++i)
                {
                    float distsq = 0;
                    for (int k = 0; k < 2; ++k)
                        distsq += (shape_vals[k, i] - m_bmucoord_vals[k]) * (shape_vals[k, i] - m_bmucoord_vals[k]);
                    for (int k = 0; k < m_PatternLength; ++k)
                    {
                        weight_vals[k, i] = weight_vals[k, i] + (float)Math.Exp(-distsq / (2 * Math.Pow(_CPU_NeighborhoodRatio(m_time_val), 2))) * _CPU_LearningRate(m_time_val) * (m_CurrentPattern[k] - weight_vals[k, i]);
                    }
                }
                m_time_val += 1;
            }
        }

        public FPA NeighborhoodRatio(FPA t)          //Sigma(t)
        {
            FPA exponent = -t / m_Theta;
            FPA Sigmat = m_SigmaInitial * PA.Pow2(PA.Log2(LogBase) * exponent);
            return Sigmat;
        }

        public float _CPU_NeighborhoodRatio(float t)
        {
            return m_sigmainitial_val * (float)Math.Exp(-(double)t / m_theta_val);
        }

        public FPA LearningRate(FPA t)              //Epsilon(t)
        {
            FPA exponent = -t / m_Theta;
            FPA Epsilont = m_EpsilonInitial * PA.Pow2(PA.Log2(LogBase) * exponent);
            return Epsilont;
        }

        public float _CPU_LearningRate(float t)
        {
            return m_epsiloninitial_val * (float)Math.Exp(-(double)t / m_theta_val);
        }

        public float[] GetBMUCodeVector()
        {
            PA.ToArray(m_BMUCodeVector, out m_bmucodevector_vals);
            return m_bmucodevector_vals;
        }

        public float[] _CPU_GetBMUCodeVector()
        {
            return m_bmucodevector_vals;
        }

        public Bitmap GetBitmap()
        {
            Bitmap bm = new Bitmap(m_Height, m_Width);
            float[,] polbak = new float[80, 80];
            PA.ToArray(m_Weights, out polbak);
            for( int i = 0; i < m_Height; ++i )
                for( int j = 0; j < m_Width; ++j )
                    bm.SetPixel(j, i, Color.FromArgb(   (int)Math.Floor(Math.Min(Math.Max(polbak[0,m_Width*i+j],0),255)), 
                                                        (int)Math.Floor(Math.Min(Math.Max(polbak[1,m_Width*i+j],0),255)),
                                                        (int)Math.Floor(Math.Min(Math.Max(polbak[2, m_Width * i + j],0), 255))
                                                     ));
            return bm;
        }

        public Bitmap _CPU_GetBitmap()
        {
            Bitmap bm = new Bitmap(m_Height, m_Width);
            for (int i = 0; i < m_Height; ++i)
                for (int j = 0; j < m_Width; ++j)
                    bm.SetPixel(j, i, Color.FromArgb((int)(Math.Floor(weight_vals[0, m_Width * i + j])),
                                                        (int)(weight_vals[1, m_Width * i + j]),
                                                        (int)(weight_vals[2, m_Width * i + j]))
                                                     );
            return bm;
        }
    }
}
