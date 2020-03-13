using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using Core.Net;

namespace SNet
{
   public class NModuleManager: Core.Net.SampleHandler
    {
        private static NModuleManager instance= null;
        public static NModuleManager GetInstance()
        {
            if (instance == null)
            {
                instance = new NModuleManager();
            }
            return instance; 
        }
        public bool Load(Stream s, bool bin = false )
        {
            bool ans = false; 
            if (s != null)
            {
                ans = NStructManager.GetInstance().Read(s, bin)
                    && Read(s, bin);
            }
            return ans; 
        }

        private bool ReadBinary(Stream s)
        {
            if (s != null)
            {
                uint size = Core.Unity.Convert.ReadUint(s);
                for (uint index = 0; index < size; ++index)
                {
                    NModule m = new NModule();
                    if (m.Unserial(s))
                    {
                        Add(m);
                    }
                }
                return true; 
            }
            return false; 
        }
        private bool ReadSio(Stream s)
        {
            Sio.SDataBuff d = new Sio.SDataBuff();
            if (d.UnSerializ(s))
            {
                Sio.SListReader lr = d.listReader;
                if (lr != null)
                {
                    Sio.SDataBuff b = new Sio.SDataBuff();
                    while (lr.Next(b))
                    {
                        Sio.SMapReader mr = b.mapReader;
                        if (mr != null)
                        {
                            NModule tm = new NModule();
                            if (tm.Unserial(mr))
                            {
                                Add(tm);
                            }
                        }
                    }
                    return true;
                }
            }
            return false; 
        }

        public bool Read(Stream s, bool binary)
        {
            if (binary)
            {
                return ReadBinary(s);
            }
            else
            {
                return ReadSio(s);
            }
        }
        public int getModuleSize()
        {
            return moduelCache.Count;
        }
        public void clear()
        {
            for (int i = 0; i < moduelCache.Count;i++)
            {
                Clear((NModule)moduelCache[i]);
            }
        }
        private void Clear(NModule o)
        {
            IList<NFunction> functionList = o.GetFunctionList();
            for (int i = 0; i < functionList.Count;i++)
            {
                Clear(functionList[i]);
            }
        }
        private void Clear(NFunction f)
        {
            if (f != null)
            {
                functionCache.Remove(f.Id);
            }
        }
        public bool Remove(NModule m)
        {
            for (int index = 0; index < moduelCache.Count;++index )
            {
                if (moduelCache[index].Equals(m))
                {
                    Clear(m);
                    moduelCache.RemoveAt(index);
                    return true; 
                }
            }
            return false; 
        }
        public bool Add(NModule m)
        {
            if (m != null)
            {
                Remove(m);
                moduelCache.Add(m);
                IList<NFunction> functionList = m.GetFunctionList();
                for (int i = 0; i < functionList.Count;i++)
                {
                    NFunction f = functionList[i];
                    if (functionCache.ContainsKey(f.Id))
                    {
                        functionCache[f.Id] = f;
                    }
                    else
                    {
                        functionCache.Add(f.Id, f);
                    }

                }
                return true; 
            }
            return false; 
        }

        public override bool ProcessEvent(Client c, Event.Type p, int error)
        {
            return pprocess.ProcessEvent(c, p, error);
        }

        public override String GetMessageFuncName(int mid) 
        {
            NFunction f = GetFunctin(mid);
            string name = "";
            if(f != null)
            {
                name = f.Name;
            }
            return name;
        }

        public override void Process(Package p)
        {
            int mid = p.MessageID;
            NFunction f = GetFunctin(mid);
            if (f != null&& pprocess!=null)
            {
                pprocess.Process(p.C, p, mid, f);
            }
            else
            {
                Logger.LogError("not find functioin msgid:" + mid);
            }
        }

        public NFunction GetFunctin(int mid)
        {
            uint id = (uint)mid;
            if (functionCache.ContainsKey(id))
            {
                return functionCache[id];
            }
            return null; 
        }
#region  param
        private ArrayList moduelCache = new ArrayList();

        private Dictionary<uint, NFunction> functionCache = new Dictionary<uint, NFunction>();
        
        private NIprocess pprocess = null;

       public  NIprocess Pprocess
        {
            get { return pprocess; }
            set { pprocess = value; }
        }
#endregion
      
    }
}
