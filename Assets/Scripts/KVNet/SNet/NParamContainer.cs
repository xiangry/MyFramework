using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SNet
{
    public class NParamContainer
    {

        public virtual bool Unserial(Stream s)
        {
            if (s != null)
            {
                uint size = Core.Unity.Convert.ReadUint(s);
                for (uint index = 0; index < size; ++index)
                {
                    NParam parmaTemp = new NParam();
                    if (parmaTemp.Unserial(s))
                    {
                        paramList.Add(parmaTemp);
                    }
                }
                return true; 
            }
            return false; 
        }

        public virtual bool Unsrial(Sio.SMapReader map)
        {
            if (map != null)
            {
                Sio.SDataBuff k = new Sio.SDataBuff();
                Sio.SDataBuff v = new Sio.SDataBuff();
                while (map.Next(k, v))
                {
                    Feild(k.uintValue, v);
                }
                return true; 
            }
            return false; 
        }



        public virtual bool Feild(uint k, Sio.SDataBuff d)
        {

            ParamContainerKey t = (ParamContainerKey)k; 
            switch (t)
            {
                case ParamContainerKey.ParamKeyList:
                    {
                        Sio.SListReader plist = new Sio.SListReader();
                        if (plist !=null)
                        {
                           Sio.SDataBuff o = new Sio.SDataBuff();
                            while (plist.Next(o))
                            {
                               Sio.SMapReader r = o.mapReader;
                               NParam p = new NParam();
                                if (r != null && p.Unserial(r))
                                {
                                    paramList.Add(p);
                                }
                            }
                        }
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }
        public bool Add(NParam param)
        {
            if (param == null || param.Name == null)
                return false; 
            for (int i = 0; i < paramList.Count;i++)
            {
                NParam o = paramList[i];
                if (o.Name != null && o.Name.CompareTo(param.Name) == 0)
                {
                    return false;
                }
            }
            param.Id = paramList.Count + 1; 
            return true; 
        }
        public bool Remove(NParam param)
        {
            return paramList.Remove(param);
        }
        public bool Remove(int id)
        {
            for (int i = 0; i < paramList.Count;i++)
            {
                NParam o = paramList[i];
                if (o.Id == id)
                {
                    paramList.Remove(o);
                    return true;
                }
            }
            return false; 
        }
        public bool Remove(string name)
        {
            if (name==null || name.Length==0)
            {
                return false; 
            }
            for (int i = 0; i < paramList.Count;i++)
            {
                NParam o = paramList[i];
                if (o.Name.CompareTo(name) == 0)
                {
                    paramList.Remove(o);
                    return true;
                }
            }
            return false; 
        }
        public NParam Get(string name)
        {
            if (name == null || name.Length == 0 )
                return null; 
            for (int i = 0; i < paramList.Count;i++)
            {
                NParam o = paramList[i];
                if (o.Name.CompareTo(name) == 0)
                {

                    return o;
                }
            }
            return null;
        }
        public NParam Get(int id)
        {
            for (int i = 0; i < paramList.Count;i++)
            {
                NParam o = paramList[i];
                if (o.Id == id)
                {
                    return o; 
                }
            }
            return null; 
        }
#region param
        IList<NParam> paramList = new List<NParam>();

        public IList<NParam> ParamList
        {
            get { return paramList; }
            set { paramList = value; }
        }
#endregion

    }
    enum ParamContainerKey:int
    {
        ParamKeyList,
        ParamKeyEnd,
    }

}
