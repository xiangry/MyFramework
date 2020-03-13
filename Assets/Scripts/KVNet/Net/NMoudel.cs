using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Net
{
    public class NModule
    {
        public virtual bool Dispatch(Client c, Package p) { return false;  }
#region  param
        protected uint startid = 0;
        public uint Startid
        {
            get { return startid; }
            set { startid = value; }
        }
        protected uint endid = 0;

        public uint Endid
        {
            get { return endid; }
            set { endid = value; }
        } 
#endregion
     
    }
}
