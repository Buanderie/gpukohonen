using System;
using System.Collections.Generic;
using System.Text;

namespace GPUKohonenLib
{
    public class SquareShape : IMapShape
    {
        public SquareShape(int[] Sizes) : base()
        {
            this.m_Sizes = Sizes;
            this.m_Dimensions = Sizes.GetLength(0);

            //Filling dictionary
            for( int i = 0; i < m_Sizes[0]; ++i )
                for( int j = 0; j < m_Sizes[1]; ++j )
                {
                    float[] temp = new float[this.m_Dimensions];
                    temp[0] = i;
                    temp[1] = j;
                    this.m_SpatialPos[m_Sizes[1] * i + j] = temp;
                }
        }

        public override int GetFlatLength()
        {
            return m_SpatialPos.Count;
        }

        public override float[] GetSpatialPosition(int Index)
        {
            return m_SpatialPos[Index];
        }
    }
}
