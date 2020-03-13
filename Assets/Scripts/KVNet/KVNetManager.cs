using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core.Net;
using UnityEngine;
using Core.Resource;
using SNet;
using UnityEngine.Experimental.PlayerLoop;
using XLua;

[CSharpCallLua]
public class KVNetManager : Singleton<KVNetManager>
{
    
    public PackageFactory PackageFactory = new PackageFactory();
   
    
    public bool Prepare()
    {
        byte[] data = LoadResource("LuaScripts/SnkFramework/net/des/netdes.snd");
        if (data != null)
        {
            return Prepare(data);
        }
        return true;
    }
    
    public bool Prepare(TextAsset asset)
    {
        return Prepare(asset.bytes);
    }
    
    public bool Prepare(Byte[] data)
    {
        using (System.IO.MemoryStream ms = new System.IO.MemoryStream(data))
        {
            SNet.NModuleManager.GetInstance().Load(ms, true);
            SNet.NModuleManager.GetInstance().Pprocess = net_process.GetInstance();
            return true;
        }

        return false;
    }
    // 同步读取资源
    public byte[] LoadResource(string url)
    {
        //string fullPath = "";
        //if (FileUtil.FileExist(url, ref fullPath) == false)
        string fullPath = FileUtil.GetFilePathGrayAll(url);
        if (fullPath == "")
        {
            Debug.LogError("[ResourceManager LoadResource] LoadResource failed. file not exist. path:" + url);
            return null;
        }

        if (fullPath.Length > 0)
        {
            try
            {
                //Debug.LogWarning("lua path=" + fullPath);
                // load original resource data
                FileStream fs = File.OpenRead(fullPath);
                byte[] data = new byte[fs.Length];
                int length = fs.Read(data, 0, Convert.ToInt32(fs.Length));
                fs.Close();

                //Debug.WatchEnd();

                if (length <= 0)
                {
                    //UnityEngine.Debug.LogWarning(String.Format("[ResourceManager LoadResource] Load file error: length={0} path={1}", length, fullPath));
                    return null;
                }

                byte[] decryptData = Process(data);
                if (decryptData == null)
                {
                    //Debug.LogError(String.Format("[ResourceManager LoadResource] Process file failed: length={0} path={1}", length, fullPath));
                    return data;
                }

                return decryptData;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.ToString());
                return null;
            }
        }

        return null;
    }
    
    
    public byte[] Process(byte[] encryptData)
    {
        byte[] decryptData = null;
        ResourceHeader resourceHeader = new ResourceHeader();
        
        MemoryStream ms = new MemoryStream(encryptData);
        
        if(!resourceHeader.Unserial(ms))
        {
            //Debug.Log(String.Format("[ResourceManager Process] Unserial file header error"));
            //Unity.Debug.Log(String.Format("[ResourceManager Process] Unserial file header error"));
            return null;
        }

        if (resourceHeader.herderVersion == ResourceHeader.HEADER_VERSION
            && (ResourceHeader.FILE_TYPE)resourceHeader.fileType == ResourceHeader.FILE_TYPE.SCRIPT
            && resourceHeader.fileSize == ms.Length - resourceHeader.headerSize)
        {
            decryptData = new byte[resourceHeader.fileSize];
            ms.Read(decryptData, 0, resourceHeader.fileSize);

            ProcessNor(decryptData, resourceHeader.encryptKey);
        }

        return decryptData;
    }

    bool ProcessNor(byte[] bs, byte[] encryptKey)
    {
        int destLen = bs.Length / 4;
        int keyLen = encryptKey.Length / 4;
      
        int ki = 0;
        int value = 0;
        int keyValue = 0;

        for (int i = 0; i < destLen; ++i)
        {
            value = BitConverter.ToInt32(bs, i * 4);
            keyValue = BitConverter.ToInt32(encryptKey, ki * 4);

            if ((i & 0x03) > 0)
            {   
                value = ~value;
            }

            value = value ^ keyValue;
            BitConverter.GetBytes(value).CopyTo(bs, i * 4);

            ki++;
            ki = ki % keyLen;
        }

        return true;
    }

    public override void Dispose()
    {
        throw new System.NotImplementedException();
    }

    public void Update()
    {
        SNet.NModuleManager.GetInstance().Process();
    }
}
