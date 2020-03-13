using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Core.Net;
using SNet;
using XLua;

[LuaCallCSharp()]
[CSharpCallLua]
public class ClientObject : Singleton<ClientObject>
{
    private int pid = Core.Util.Utils.GenID();
    public int GetPid()
    {
        return pid;
    }
    
    #region create and destroy
    public static Dictionary<int, ClientObject> cache = new Dictionary<int, ClientObject>();
    public static Dictionary<Client, ClientObject> cache_by_client = new Dictionary<Client, ClientObject>();
    public static ClientObject CreateInstance()
    {
        ClientObject clientObject = new ClientObject();
        Client c = NetManager.GetInstance().CreateClient(NModuleManager.GetInstance());
        c.PackageFactory = new Core.Net.PackageFactoryEx();
        clientObject.c = c;
        cache.Add(clientObject.GetPid(), clientObject);
        cache_by_client.Add(c, clientObject);

//        #if DEBUG_CORE
        Logger.Log("client object create. pid:"+clientObject.GetPid());
//        #endif

        return clientObject;
    }
    
    public static void DestroyInstance(ClientObject clientObject)
    {
        if(clientObject != null)
        {
//            #if DEBUG_CORE
            Logger.Log("client object destroy. pid:" + clientObject.GetPid());
//            #endif

            cache.Remove(clientObject.GetPid());
            cache_by_client.Remove(clientObject.c);
            if (clientObject.c != null)
            {
                clientObject.c.Close();
            }
            clientObject.c = null;
        }
    }
    #endregion

    #region property

    public Client client
    {
        get { return c; }
    }

    private Client c = null;

    #endregion

    public override void Dispose()
    {
        throw new NotImplementedException();
    }
}
