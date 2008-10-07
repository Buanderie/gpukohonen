using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPUKohonenLib
{
    public abstract class IMapShape
    {
        protected int m_Dimensions;
        protected float m_Extent;
        protected Dictionary<int, float[]> m_SpatialPos;

        public IMapShape()
        {
            //Index <-> Spatial Positions dictionary initilization
            this.m_SpatialPos = new Dictionary<int, float[]>();
        }

        public int Dimension
        {
            get
            {
                return m_Dimensions;
            }
            set
            {
            }
        }

        public float Extent
        {
            get
            {
                return m_Extent;
            }
            set
            {
            }
        }

        public float[,] UnitCoordinates
        {
            get
            {
                float[,] spapos = new float[m_SpatialPos.Count, m_Dimensions];
                for (int i = 0; i < m_SpatialPos.Count; ++i)
                for( int j = 0; j < m_Dimensions; ++j )
                {
                    spapos[i, j] = m_SpatialPos[i][j];    
                }
                return spapos;
            }
            set
            {
            }
        }

        public abstract int GetFlatLength();
        public abstract float[] GetSpatialPosition( int Index );
    }
}
