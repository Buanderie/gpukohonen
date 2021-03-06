﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPUKohonenLib
{
    public abstract class IKohonenCore
    {
        protected KohonenSOM m_Parent;

        public IKohonenCore() { }
        public abstract void Init(KohonenSOM parent);
        public abstract void FindBMU();
        public abstract void DoEpoch(float t, float round_t);
        public abstract void Terminate();
    }
}
