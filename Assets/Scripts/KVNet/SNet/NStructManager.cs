using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SNet
{
  public  class NStructManager
    {
        private static NStructManager instance = null;
        public static NStructManager GetInstance()
        {
            if (instance == null)
            {
                instance = new NStructManager();
            }
            return instance;
        }
        private NStructManager()
        {
        }
        #region  serial

      public bool ReadBinary(Stream s)
      {

          if (s != null)
          {
              uint size = Core.Unity.Convert.ReadUint(s);
              for (uint index = 0; index < size; ++index)
              {
                  NStruct ps = new NStruct();
                  if (ps.Unserial(s))
                  {
                      Add(ps);
                  }
              }
              return true; 
          }
          return false; 
      }
      public bool ReadSio(Stream instream)
      {
           Sio.SDataBuff d = new Sio.SDataBuff();
            if (d.UnSerializ(instream))
            {
                Sio.SListReader l = d.listReader;
                if (l!=null)
                {
                    Sio.SDataBuff b = new Sio.SDataBuff();
                    while (l.Next(b))
                    {
                        Sio.SMapReader mr = b.mapReader;
                        if (mr != null)
                        {
                            NStruct n = new NStruct();
                            if (n.Unsrial(mr))
                            {
                                Add(n);
                            }
                        }
                    }
                    return true; 
                }
                
            }
          return false; 
      }

        public bool Read(Stream instream, bool bin)
        {
            if (bin == false)
            {
              return   ReadSio(instream);
            }
            else
            {
                return ReadBinary(instream);
            }
        }
        public bool init()
        {
            foreach (KeyValuePair<string, NStruct> p in cache)
            {
                p.Value.Init();
            }
            return true; 
        }
        #endregion
        public NStruct Find(string pname)
        {
            if (cache.ContainsKey(pname))
            {
                return cache[pname];
            }
            return null;
        }
        public bool Add(NStruct ps)
        {
			if (cache.ContainsKey(ps.Name))
			{
				cache[ps.Name] = ps;
			} 
			else
			{
				cache.Add(ps.Name, ps);
			}
            
            return true;
        }
        public bool remove(NStruct ps)
        {
            return cache.Remove(ps.Name);
        }
        public bool remove(string name)
        {
            return cache.Remove(name);
        }
        public void clear()
        {
            cache.Clear();
        }
        public int GetSize() { return cache.Count; }


        #region  param
        Dictionary<String, NStruct> cache = new Dictionary<String, NStruct>();
        #endregion
    }
}
