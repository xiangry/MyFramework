using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SNet
{
  public  class NParam
    {

      public bool Unserial(Stream s)
      {
          if (s != null)
          {
              name = Core.Unity.Convert.ReadString(s);
              typeName = Core.Unity.Convert.ReadString(s);
              dtype = (ParamType)Core.Unity.Convert.ReadUShort(s);
              container = (ParamContainer)Core.Unity.Convert.ReadUShort(s);
              id = Core.Unity.Convert.ReadInt(s);
              isList = Core.Unity.Convert.ReadBool(s);
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
                return true;
            }
            return false;
        }

       public bool Feild(uint k, Sio.SDataBuff v)
        {
            ParamKey t = (ParamKey)k;
            switch (t)
            {
                case ParamKey.param_key_name:
                    name = v.stringValue;
                    break;
                case ParamKey.param_key_type_name:
                    typeName = v.stringValue;
                    break;
                case ParamKey.param_key_type:
                    dtype = (ParamType)v.intValue;
                    break;
                case ParamKey.param_key_container:
                    container = (ParamContainer)v.intValue;
                    break;
                case ParamKey.param_key_id:
                    id = v.intValue;
                    break;
                case ParamKey.param_key_is_list:
                    isList = v.boolValue;
                    break;
                default:
                    return false;
            }
            return true;
        }
        #region param
        String name= string.Empty;             //参数名称
        public System.String Name
        {
            get { return name; }
            set { name = value; }
        }
        String typeName= string.Empty;        //类型名称 
        public System.String TypeName
        {
            get { return typeName; }
            set { typeName = value; }
        }
        ParamType dtype =ParamType.ptype_bool;             //基本类型,只有使用其它类型的时候才会有type_name

        public ParamType DType
        {
            get { return dtype; }
            set { dtype = value; }
        }
        ParamContainer container= ParamContainer.pparam_container_no;   //容器类型

        public ParamContainer Container
        {
            get { return container; }
            set { container = value; }
        }
        int id=0;               //参数编号

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        bool isList = false;          // 是否是list

        public bool Is_list_
        {
            get { return isList; }
            set { isList = value; }
        }
        #endregion

        enum ParamKey:int
        {
            param_key_name,
            param_key_type_name,
            param_key_type,
            param_key_container,
            param_key_id,
            param_key_is_list,
        };
    }
   public enum ParamType : uint
    {
        ptype_bool = 0 << 4,
        ptype_char = 1 << 4,
        ptype_uchar = 2 << 4,
        ptype_short = 3 << 4,
        ptype_ushort = 4 << 4,
        ptype_int = 5 << 4,
        ptype_uint = 6 << 4,
        ptype_float = 7 << 4,
        ptype_double = 8 << 4,
        ptype_long = 9 << 4,
        ptype_string = 10 << 4,
        ptype_data = 11 << 4,
        ptype_map = 12 << 4,
        ptype_list = 13 << 4,
        ptype_object = 14 << 4,   //如果是其它类型
        ptype_ulong = 15 << 4,
    };
   public enum ParamContainer:uint
    {
        pparam_container_no,
        pparam_container_map,
        pparam_container_list,
    };

}
