using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SNet
{
    public class NModule
    {

        public void SetId(uint startid, uint endid)
        {
            startID = Math.Min(startid, endid);
            endID = Math.Max(startid, endid);
        }
        public IList<NFunction> GetFunctionList() { return functionList; }
        public NFunction GetFunction(uint id)
        {
            for (int i = 0; i < functionList.Count;i++)
            {
                NFunction o = functionList[i];
                if (o.Id == id)
                    return o;
            }
            return null;
        }
        public NFunction GetFunction(string pname)
        {
           if(pname==null)
               return null; 
           for (int i = 0; i < functionList.Count; i++)
           {
               NFunction o = functionList[i];
               if (o.Name.CompareTo(pname) == 0)
                   return o;
           }
            return null;
        }
        public bool Add(NFunction pf)
        {
            pf.Nm = this; 
            functionList.Remove(pf);
            functionList.Add(pf);
            return true;
        }
        public bool Remove(NFunction pf)
        {
            pf.Nm = null; 
            functionList.Remove(pf);
            return true;
        }
        public bool remove(uint id)
        {
            for (int i = 0; i < functionList.Count;i++)
            {
                NFunction o = functionList[i];
                if (o.Id == id)
                {
                    functionList.Remove(o);
                }
            }
            return false;
        }
        public void Clear()
        {
            functionList.Clear();
        }


        public bool Unserial(Stream s)
        {
            if (s != null)
            {
                name = Core.Unity.Convert.ReadString(s);
                startID = Core.Unity.Convert.ReadUint(s);
                endID = Core.Unity.Convert.ReadUint(s);
                uint funsize = 0;
                funsize = Core.Unity.Convert.ReadUint(s);
                for (uint index = 0; index < funsize; ++index)
                {
                    NFunction f = new NFunction();
                    if (f.Unserial(s))
                    {
                        Add(f);
                    }
                }
                return true; 
            }
            return false; 
        }

        public bool Unserial(Sio.SMapReader pmap)
        {
            if (pmap != null)
            {
                Sio.SDataBuff k = new Sio.SDataBuff();
                Sio.SDataBuff v = new Sio.SDataBuff();
                while (pmap.Next(k, v))
                {
                    Feild(k.uintValue, v);
                }
            }
            return false;
        }
        bool Feild(uint k, Sio.SDataBuff d)
        {
            moduleKey t = (moduleKey)k; 
            switch (t)
            {
                case moduleKey.module_name_key:
                    name = d.stringValue;
                    break;
                case moduleKey.module_startid:
                    startID = d.uintValue;
                    break;
                case moduleKey.module_endid:
                    endID = d.uintValue;
                    break;
                case moduleKey.module_function_list:
                    {
                        Sio.SListReader plist = d.listReader;
                        Sio.SDataBuff tn = new Sio.SDataBuff();
                        while (plist.Next(tn))
                        {
                            Sio.SMapReader pr = tn.mapReader;
                            if (pr != null)
                            {
                                NFunction pf = new NFunction();
                                if (pf.Unsrial(pr))
                                {
                                    Add(pf);
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
        #region param
        String name = string.Empty;

        public String Name
        {
            get { return name; }
            set { name = value; }
        }
        uint startID= 0;

        public uint StartID
        {
            get { return startID; }
            set { startID = value; }
        }
        uint endID=0;

        public uint EndID
        {
            get { return endID; }
            set { endID = value; }
        }
        IList<NFunction> functionList = new List<NFunction>();
        #endregion

        enum moduleKey : int
        {
            module_name_key,
            module_startid,
            module_endid,
            module_function_list,
        };
    }
}
