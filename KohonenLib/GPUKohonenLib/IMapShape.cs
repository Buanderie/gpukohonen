using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPUKohonenLib
{
    public abstract class IMapShape
    {
        protected int m_Dimensions;
        protected int[] m_Sizes;
        protected Dictionary<int, float[]> m_SpatialPos;

        public IMapShape()
        {
            //Index <-> Spatial Positions dictionary initilization
            this.m_SpatialPos = new Dictionary<int, float[]>();
        }

        public abstract int GetFlatLength();
        public abstract float[] GetSpatialPosition( int Index );
    }
}
