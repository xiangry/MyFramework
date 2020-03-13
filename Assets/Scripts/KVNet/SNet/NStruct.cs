using System;
using System.Collections.Generic;
using System.Text;

namespace SNet
{
   public class NStruct : NParamContainer
    {

        public void ChangePartent(NStruct partent)
        {
            partentName = partent.Name;
            this.partent = partent; 

        }

        public override bool Unserial(System.IO.Stream s)
        {
            if (base.Unserial(s))
            {
                name = Core.Unity.Convert.ReadString(s);
                partentName = Core.Unity.Convert.ReadString(s);
                return true; 
            }
            return false; 
        }
        public override bool Feild(uint k, Sio.SDataBuff v)
        {
            if (base.Feild(k, v) == true)
                return true;
            nstruct_key t = (nstruct_key)k;
            switch (t)
            {
                case nstruct_key.nstruct_key_partent:
                    partentName = v.stringValue; 
                    break;
                case nstruct_key.nstruct_key_name:
                    name = v.stringValue;
                    break;
                default:
                    return false; 
            }
            return true; 
        }
        public bool Init()
        {
            if (partentName != null && partentName.Length != 0)
            {
                NStruct p = NStructManager.GetInstance().Find(partentName);
                if (p != null)
                {
                    partent = p;
                }
            }
            return true;
        }
        public void SetPartentName(String name)
        {
            partentName = name;
            if (partent != null && partent.Name.CompareTo(name) == 0)
            {
                return; 
            }
            else
            {
                NStruct p = NStructManager.GetInstance().Find(partentName);
                if (p != null)
                {
                    partent = p;
                }
            }
        }

        String name = String.Empty; //名称
        public String Name
        {
            get { return name; }
            set { name = value; }
        }
        NStruct partent = null;           //继承的父结构体
        internal NStruct Partent
        {
            get { return partent; }
            set { partent = value; }
        }
        String partentName = String.Empty;     //上线结构体

        public String PartentName
        {
            get { return partentName; }
            set { partentName = value; }
        }
        enum nstruct_key
        {
            nstruct_key_start = ParamContainerKey.ParamKeyEnd,
            nstruct_key_name,
            nstruct_key_partent,
            nstruct_key_end,
        };
    }
}
