using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;
using Core.Net;
using SNet;
using XLua;

/// <summary>
/// xlua自定义导出
/// </summary>
[LuaCallCSharp]
public class KVDeviceInfo
{
    /// <summary>
    /// 获取设备信息
    /// </summary>

    public static List<string> DeviceInfoList = new List<string>()
    {
        "设备模型：" + SystemInfo.deviceModel,
        "设备名称：" + SystemInfo.deviceName,
        "设备类型：" + SystemInfo.deviceType,
        "设备唯一标识符：" + SystemInfo.deviceUniqueIdentifier,
        "是否支持纹理复制：" + SystemInfo.copyTextureSupport,
        "显卡ID：" + SystemInfo.graphicsDeviceID,
        "显卡名称：" + SystemInfo.graphicsDeviceName,
        "显卡类型：" + SystemInfo.graphicsDeviceType,
        "显卡供应商：" + SystemInfo.graphicsDeviceVendor,
        "显卡供应商ID：" + SystemInfo.graphicsDeviceVendorID,
        "显卡版本号：" + SystemInfo.graphicsDeviceVersion,
        "显存大小（单位：MB）：" + SystemInfo.graphicsMemorySize,
        "是否支持多线程渲染：" + SystemInfo.graphicsMultiThreaded,
        "支持的渲染目标数量：" + SystemInfo.supportedRenderTargetCount,
        "系统内存大小（单位：MB）：" + SystemInfo.systemMemorySize,
        "操作系统：" + SystemInfo.operatingSystem
    };
    /// <summary>
    /// 获取mac地址
    /// </summary>
    public static string GetMacAddress()
    {
        string macAddress = "0";
        NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface ni in nis)
        {
            Debug.Log("Name = " + ni.Name);
            Debug.Log("Des = " + ni.Description);
            Debug.Log("Type = " + ni.NetworkInterfaceType.ToString());
            Debug.Log("Mac地址 = " + ni.GetPhysicalAddress().ToString());
            macAddress = "mac地址：" + ni.GetPhysicalAddress().ToString();
        }
        return  macAddress;
    }
    
    /// <summary>
    /// 手机序列号是IMEI码的俗称。
    /// IMEI为TAC + FAC + SNR + SP。
    /// IMEI(International Mobile Equipment Identity)是国际移动设备身份码的缩写，
    /// 国际移动装备辨识码，是由15位数字组成的"电子串号"，
    /// 它与每台移动电话机一一对应，而且该码是全世界唯一的。
    /// </summary>
    #region 获得安卓手机上的IMEI号
    public static string imei0 = "";
    public static string imei1 = "";
    public static string meid = "";
    public static List<string> GetDeviceIMEI()
    {
        List<string> files = new List<string>();
        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var telephoneyManager = context.Call<AndroidJavaObject>("getSystemService", "phone");
        imei0 = telephoneyManager.Call<string>("getImei", 0);//如果手机双卡 双待  就会有两个MIEI号
        imei1 = telephoneyManager.Call<string>("getImei", 1);
        meid = telephoneyManager.Call<string>("getMeid");//电信的手机 是MEID
        string IMEI0 = "IMEI0:" + imei0;
        string IMEI1 = "IMEI1:" + imei1;
        string MEID = "MEID:" + meid;
        files.Add(IMEI0);
        files.Add(IMEI1);
        files.Add(MEID);
        return  files;
    }
    #endregion
    
}