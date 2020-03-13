using System;
using System.Collections.Generic;
using System.Text;

namespace SNet
{
    public class NFunction : NParamContainer
    {

        public override bool Unserial(System.IO.Stream s)
        {
            if (s != null && base.Unserial(s))
            {
                name = Core.Unity.Convert.ReadString(s);
                id = Core.Unity.Convert.ReadUint(s);
                isClient = Core.Unity.Convert.ReadBool(s);
                return true; 
            }
            return false; 
        }

        public override bool Feild(uint k, Sio.SDataBuff d)
        {
            if (base.Feild(k, d))
            {
                return true;
            }
            function_name t = (function_name)k;
            switch (t)
            {
                case function_name.func_key_name:
                    name = d.stringValue;
                    break;
                case function_name.func_key_id:
                    id = d.uintValue;
                    break;
                case function_name.func_key_is_client:
                    isClient = d.boolValue;
                    break;

                default:
                    return false;
            }
            return true;

        }
        #region param
        string name= string.Empty; //方法名称

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        uint id =0;           //方法编号

        public uint Id
        {
            get { return id; }
            set { id = value; }
        }
        bool isClient= false;   //是否是客户端

        public bool IsClient
        {
            get { return isClient; }
            set { isClient = value; }
        }
        private NModule nm = null;

        public NModule Nm
        {
            get { return nm; }
            set { nm = value; }
        } 
        #endregion

       protected enum function_name : int
        {
            func_key_start = ParamContainerKey.ParamKeyEnd,
            func_key_name,
            func_key_id,
            func_key_is_client,
        };
    }
}
