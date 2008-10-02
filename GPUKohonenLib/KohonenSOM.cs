using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GPUKohonenLib
{
    public class KohonenSOM
    {
        public enum KOHONEN_SOM_MAPINIT_TYPE { MAPINIT_RANDOM, MAPINIT_GRADIENT };

        public IKohonenCore m_Core;
        private IMapShape m_MapShape;
        private IDataSource m_DataSource;
        private float[,] m_NeuronMap;

        private KOHONEN_SOM_MAPINIT_TYPE m_NeuronMapInitType;
        private float m_NeuronMapInitMin;
        private float m_NeuronMapInitMax;

        private int m_Time;

        public KohonenSOM(IKohonenCore Core,
                            IMapShape MapShape,
                            IDataSource DataSource
                          )
        {
            m_Core = Core;
            m_MapShape = MapShape;
            m_DataSource = DataSource;
            m_Core.Init(this);
        }

        public KOHONEN_SOM_MAPINIT_TYPE NeuronMapInitType
        {
            get
            {
                return m_NeuronMapInitType;
            }
            set
            {
                m_NeuronMapInitType = value;
            }
        }

        public float NeuronMapInitMin
        {
            get
            {
                return m_NeuronMapInitMin;
            }
            set
            {
                m_NeuronMapInitMin = value;
            }
        }

        public float NeuronMapInitMax
        {
            get
            {
                return m_NeuronMapInitMax;
            }
            set
            {
                m_NeuronMapInitMax = value;
            }
        }

        public float[,] NeuronMap
        {
            get
            {
                return m_NeuronMap;
            }
            set
            {
            }
        }

        public IMapShape NeuronMapShape
        {
            get
            {
                return m_MapShape;
            }
            set
            {
            }
        }

        public IDataSource DataSource
        {
            get
            {
                return m_DataSource;
            }
            set
            {
            }
        }

        private void InitNeuronMap(KOHONEN_SOM_MAPINIT_TYPE InitType, float min, float max)
        {
            Random RandomNumber = new Random();
            for( int i = 0; i < m_MapShape.GetFlatLength(); ++i )
                for( int j = 0; j < m_DataSource.GetPatternLength(); ++j )
                {
                    m_NeuronMap[i,j] = RandomNumber.Next(0, 255);
                }
        }

        public void DoRound(int nb)
        {
            for (int i = 0; i < nb; ++i)
            {
                //Update the output neuron map
                m_Core.DoEpoch(i, nb);
            }
        }

        public void Init()
        {
            //Create Output Neuron Map
            m_NeuronMap = new float[m_MapShape.GetFlatLength(), m_DataSource.GetPatternLength()];

            //Set time = 0
            m_Time = 0;

            //Initialize the output neuron map to some values
            InitNeuronMap(m_NeuronMapInitType, m_NeuronMapInitMin, m_NeuronMapInitMax);
        }

        public System.Drawing.Bitmap ToWeightBitmap()
        {
            Bitmap bm = new Bitmap( (int)Math.Sqrt(m_MapShape.GetFlatLength()), (int)Math.Sqrt(m_MapShape.GetFlatLength() ));
            for (int i = 0; i < Math.Sqrt(m_MapShape.GetFlatLength()); ++i)
                for (int j = 0; j < Math.Sqrt(m_MapShape.GetFlatLength()); ++j)
                    bm.SetPixel(j, i, Color.FromArgb((int)(Math.Floor(NeuronMap[50 * i + j, 0])),
                                                     (int)(NeuronMap[50 * i + j, 1]),
                                                     (int)(NeuronMap[50 * i + j, 2]))
                                                     );
            return bm;
        }
    }
}
