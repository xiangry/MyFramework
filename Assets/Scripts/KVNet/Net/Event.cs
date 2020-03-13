using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Core
{
    namespace Net
    {
        ///TODO:  网络状态(连接，断开，错误) 的事件派发对象
        public class Event
        {
            public Event(Client c, Type s, int ex)
            {
                client = c;
                state_type = s;
                er = ex;
            }

            public enum Type : uint
            {
                CONNECT = 0x01,
                CLOSE = 0x02,
                ERROR = 0x04,
            };

            private Type state_type;

            public Type ST
            {
                get { return state_type; }
                set { state_type = value; }
            }

            private Client client;

            public Client CT
            {
                get { return client; }
                set { client = value; }
            }


            private int er;

            public int ER
            {
                get { return er; }
                set { er = value; }
            }
        }
    }
}
