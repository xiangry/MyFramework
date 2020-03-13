using System;

namespace Core.Util
{
	public enum MEM_TYPE
	{
		MEM_ACTOR = 1,
		MEM_SKILL,
		MEM_AUDIO,
		MEM_AMOUNT,
	}
	
	public class MemoryManager
	{
		private static MemoryManager inStance = null;
		private double[] memoryArray = new double[(int)MEM_TYPE.MEM_AMOUNT];
		
		private MemoryManager()
		{
			for (int i = 0; i < (int)MEM_TYPE.MEM_AMOUNT; ++ i)
			{
				memoryArray[i]=0.0;
			}
		}
		
		public static MemoryManager getInstance()
		{
			if (inStance == null)
			{
				inStance = new MemoryManager();
			}
			
			return inStance;
		}
		
		public void AddMemory(MEM_TYPE type, double size)
		{
			if (type >= MEM_TYPE.MEM_ACTOR && type < MEM_TYPE.MEM_AMOUNT)
			{	
				memoryArray[(int)type] += size;
			}
		}
		
		public void RemoveMemory(MEM_TYPE type, double size)
		{
			if (type >= MEM_TYPE.MEM_ACTOR && type < MEM_TYPE.MEM_AMOUNT)
			{	
				memoryArray[(int)type] -= size;
			}
		}
		
		public double getMemorySize(MEM_TYPE type) 
		{
			if (type >= MEM_TYPE.MEM_ACTOR && type < MEM_TYPE.MEM_AMOUNT)
			{	
				return memoryArray[(int)type];
			}
			return 0.0;
		}
		
//		public static int GetObjectSize(object obj)
//    	{
//        	BinaryFormatter bf = new BinaryFormatter();
//        	MemoryStream ms = new MemoryStream();
//        	byte[] Array;
//        	bf.Serialize(ms, obj);
//        	Array = ms.ToArray();
//        	return Array.Length;
//    	}
	}
}

