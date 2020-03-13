using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Core.Net
{
    /************************************************************************/
    /*                                                                      */
    /************************************************************************/
    public class NetManager
    {
        private static object lockOjbect = new object();
        private static NetManager instance = null;
        public static NetManager GetInstance()
        {
            lock (lockOjbect)
            {
                if (instance == null)
                {
                    instance = new NetManager();
                }
            }
            return instance; 
        }
        public Client CreateClient(NetHandle handle)
        {
            Client c = new Client();
            c.Handle = handle;
            Debug.Log("CreateClient Handle ==== ");
            Debug.Log(handle);
            return c; 
        }
    }
}
