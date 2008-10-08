﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GPUKohonenLib
{
    public class CPUKohonenCore : IKohonenCore
    {
        private float[] m_BMUWeight;
        private float[] m_BMUCoord;
        private float[] m_CurrentPattern;

        public CPUKohonenCore() : base()
        { }

        public override void Init(KohonenSOM parent)
        {
            this.m_Parent = parent;
            m_BMUCoord = new float[this.m_Parent.NeuronMapShape.Dimension];
            m_BMUWeight = new float[this.m_Parent.DataSource.GetPatternLength()];
        }

        public override void FindBMU()
        {
            //Useful locals
            int alen = this.m_Parent.NeuronMapShape.GetFlatLength();
            int m_PatternLength = m_CurrentPattern.Length;
            float[] Pattern = m_CurrentPattern;

            //Create distances array
            float[] distances = new float[alen];
            for (int i = 0; i < alen; ++i)
            {
                double sum = 0;
                for (int j = 0; j < m_PatternLength; ++j)
                    sum += Math.Pow(m_Parent.NeuronMap[i, j] - Pattern[j], 2.0f);
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
            m_BMUCoord = m_Parent.NeuronMapShape.GetSpatialPosition(min_ind);
            for (int i = 0; i < m_PatternLength; ++i)
                m_BMUWeight[i] = m_Parent.NeuronMap[min_ind, i];
        }

        public override void DoEpoch(float t, float round_t)
        {
            int alen = this.m_Parent.NeuronMapShape.GetFlatLength();

            //For all patterns
            for (int a = 0; a < m_Parent.DataSource.PatternCount; ++a)
            {
                m_CurrentPattern = m_Parent.DataSource.GetPattern(a);
                this.FindBMU();

                //For each unit
                for (int i = 0; i < alen; ++i)
                {
                    float distsq = 0;
                    for (int k = 0; k < this.m_Parent.NeuronMapShape.Dimension; ++k)
                        distsq += (m_Parent.NeuronMapShape.GetSpatialPosition(i)[k] - this.m_BMUCoord[k]) * (m_Parent.NeuronMapShape.GetSpatialPosition(i)[k] - this.m_BMUCoord[k]);
                    for (int k = 0; k < m_Parent.DataSource.GetPatternLength(); ++k)
                    {
                        this.m_Parent.NeuronMap[i, k] = this.m_Parent.NeuronMap[i, k] + (float)Math.Exp(-distsq / (2 * Math.Pow(this.Neighborhood(t, round_t), 2))) * this.LearningRate(t, round_t) * (m_CurrentPattern[k] - this.m_Parent.NeuronMap[i, k]);
                    }
                }
            }
            float testou = (float)(2 * Math.Pow(this.Neighborhood(t, round_t), 2));
            int lol = 345;
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
            
        }
    }
}
