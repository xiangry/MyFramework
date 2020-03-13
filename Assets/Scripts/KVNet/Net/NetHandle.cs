using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    namespace Net
    {


        public enum NetError:int
        {
            NETERROR_CONNECTION= 1 << 0,
            NETERROR_RECEIVE   = 1 << 1,
            NETERROR_SEND      = 1 << 2,
            NETERROR_CLOSE     = 1 << 3,
        }
        public interface NetHandle
        {
            bool ProcessEvent(Client c, Event.Type type, int error);
            bool Event(Client c, Event.Type type, int ex = 0);
            int  Packet(Client c, Package p);
            void Process();
        }
    }
}
