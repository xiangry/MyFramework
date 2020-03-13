using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Core;
using XLua;

namespace Core
{
    namespace Net
    {
        
        [LuaCallCSharp()]
        [CSharpCallLua]
        public class Client
        {

			[DllImport("__Internal")]
            private static extern string getIPv6(string host, string port);

            public string GetIPv6(string host, string port)
            {
#if UNITY_IPHONE && !UNITY_EDITOR
		        string mIPv6 = getIPv6(host, port);
		        return mIPv6;
#else
                return host + "&&ipv4";
#endif
            }

            public void getIPType(String serverIp, String serverPorts, out String newServerIp, out AddressFamily ipType)
            {
                ipType = AddressFamily.InterNetwork;
                newServerIp = serverIp;
                try
                {
                    string ipv6 = GetIPv6(serverIp, serverPorts);
                    if (!string.IsNullOrEmpty(ipv6))
                    {
                        string[] strTemp = System.Text.RegularExpressions.Regex.Split(ipv6, "&&");
                        if (strTemp != null && strTemp.Length >= 2)
                        {
                            string IPType = strTemp[1];
                            if (IPType == "ipv6")
                            {
                                newServerIp = strTemp[0];
                                ipType = AddressFamily.InterNetworkV6;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError("GetIPv6 error:" + e);
                }
            }
            //static long time1,time2;
            //static float rate;
            static int CLIENT_KEY_MESSAGE_REQUEST_ID = 2;
            static int CLIENT_KEY_MESSAGE_RESPONSE_ID = 3;

            public bool Send(Package p)
            {
                if (p != null)
                {
                    p.Header.SetSeqNum(++sendSeqId);
                    try
                    {
                        lock (sendlock)
                            sendDataList.AddLast(p);
                        StartWrite();
                    }
                    catch (System.Exception ex)
                    {
                        Logger.LogError("SendException:" + ex);
                    }

                    return true;
                }
                else
                    return false;

            }
            public bool Connect(string ip, int port)
            {
#if DEBUG_CORE
                Logger.Log("[Client Connect] connect start, ip:" + ip + " port:" + port);
#endif

                if (c != null)
                {
                    Close();
                }
                try
                {
					String newServerIp = "";
					AddressFamily newAddressFamily = AddressFamily.InterNetwork;
					getIPType(ip, port.ToString(), out newServerIp, out newAddressFamily);
					if (!string.IsNullOrEmpty(newServerIp))
						ip = newServerIp;

					c = new Socket(newAddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    Logger.LogError("Connect c ======== " + c.ToString());
                    socketID = Core.Util.Utils.GenID();
                    c.NoDelay = true;
                    IPEndPoint remoteEp = new IPEndPoint(IPAddress.Parse(ip), port);
                    c.Connect(remoteEp);              
                    if (c.Connected)
                        return true;
                }
                catch (System.Exception ex)
                {
                    Logger.LogError("[Client Connect] ConnectException:" + ex);
                    return false;
                }
                return true;
            }
            public bool AsycConnect(string ip, int port)
            {
#if DEBUG_CORE
                Logger.Log("[Client AsycConnect] connect start, ip:" + ip + " port:" + port);
#endif

				if (c != null)
				{
					Close();
				}
				try
				{
					String newServerIp = "";
					AddressFamily newAddressFamily = AddressFamily.InterNetwork;
					getIPType(ip, port.ToString(), out newServerIp, out newAddressFamily);
					if (!string.IsNullOrEmpty(newServerIp))
						ip = newServerIp;

					c = new Socket(newAddressFamily, SocketType.Stream, ProtocolType.Tcp);
					socketID = Core.Util.Utils.GenID();
					c.NoDelay = true;
                  
					IPEndPoint remoteEp = new IPEndPoint(IPAddress.Parse(ip), port);
					c.BeginConnect(remoteEp, new AsyncCallback(ConnectCallBack), c);
				}
				catch (System.Exception ex)
				{
					Logger.LogError("[Client AsycConnect] AsycConnectException:" + ex);
				}
				return true;
            }
            public void ConnectCallBack(IAsyncResult ir)
            {
                sendData = null;
                sendDataPos = 0;
                Array.Clear(receiveData, 0, MEM_SIZE);
                currentReceivePos = 0;
                isSend = false;
                isReceive = false;
                sendDataList = new LinkedList<Package>();//发送消息包

                Socket s = (Socket)ir.AsyncState;
                try
                {
                    s.EndConnect(ir);
#if DEBUG_CORE
                    Logger.Log("[Client ConnectCallBack] connect success");
#endif
                    ///TODO:
                    ///修改多线程bug.(网络状态回调，采用与处理接受消息机制一样，由该处抛出事件，在游戏主线程进行处理。)
                    ///fix by : liu guirong (oxrusher@gmail.com)
                    //handle.Packet(this, receiveData, 0, 0);

                    StartRead();
                    StartWrite();

                    if (IsNeedExchangeKey == false)
                    {
                        if (handle != null) handle.Event(this, Event.Type.CONNECT);
                    }
                    else if (IsNeedExchangeKey && !isExchenagedKey) //发送key交换request
                    {
                        SendExchangeKey();
                    }
                }
                catch (System.Exception ex)
                {
                    Logger.LogError("[Client ConnectCallBack] ConnectCallBackException:" + ex);
                    if (handle != null)
                        handle.Event(this, Event.Type.ERROR, Convert.ToInt32(NetError.NETERROR_CONNECTION));
                    Close();
                }
            }

            private void SendExchangeKey()
            {
                Package p = PackageFactory.CreatePackage();
                p.MessageID = CLIENT_KEY_MESSAGE_REQUEST_ID;
                this.Send(p);
            }

            public void Close()
            {
                lock (receiveLock)
                {
                    if (c != null && c.Connected)
                    {
#if !UNITY_EDITOR
						//在编辑器里面会导致卡死
                        c.Disconnect(false);
#endif
                        c.Close();
                        if (handle != null)
                            handle.Event(this, Event.Type.CLOSE);
                    }                
                }
            }
            private void StartRead()
            {
#if DEBUG_CORE
                Logger.Log("[Client StartRead] start receive socketID:" + socketID);
#endif

                lock (receiveLock)
                {
                    if (isReceive == false && c.Connected)
                    {
                        isReceive = true;
                        //time1 = DateTime.Now.Ticks;
#if DEBUG_CORE
                        Logger.Log("[Client ReceiveCallBack] will receive bytes:" + (receiveData.Length - currentReceivePos).ToString());
#endif
                        c.BeginReceive(receiveData, currentReceivePos, receiveData.Length - currentReceivePos,
                            0, new AsyncCallback(ReceiveCallBack), c);
                        //Logger.Log("receive pos:" + currentReceivePos);
                        //time2 = DateTime.Now.Ticks;
                        //UnityEngine.Logger.Log("======================read time11111:" + (time2 - time1) / 10000.0f + ", NoDelay:" + c.NoDelay + ", ReceiveBufferSize:" + c.ReceiveBufferSize + ", cur size:" + (receiveData.Length - currentReceivePos));
                    }
                }
            }

            //int a = 0;
            public void ReceiveCallBack(IAsyncResult ir)
            { 
#if DEBUG_CORE
                Logger.Log(string.Format("OnRcve: {0}, {1}, {2}, {3}", ir.AsyncState, ir.AsyncWaitHandle, ir.CompletedSynchronously, ir.IsCompleted));
#endif
                Socket s = (Socket)ir.AsyncState;            
                try
                {
                    lock (receiveLock)
                    {
                        isReceive = false;
                        if (s.Connected)
                        {
#if DEBUG_CORE
                            Logger.Log("[Client ReceiveCallBack] start receive socketID:" + socketID);
#endif
                            int receiveBytes = s.EndReceive(ir);
//                            SocketError socketError;                         
//                            int receiveBytes = s.EndReceive(ir,out socketError);
//                            Logger.Log("EndReceive:" +Enum.GetName(typeof(SocketError), socketError)+":"+(int)socketError+" lenght:"+receiveBytes);
                            if (receiveBytes != 0)
                            {
                                currentReceivePos += receiveBytes;
#if DEBUG_CORE
                                Logger.Log("[Client ReceiveCallBack] receive success, receive bytes:" + receiveBytes.ToString() + " currentReceivePos:" + currentReceivePos.ToString());
#endif
                                if (handle != null)
                                {
                                    int startPos = 0;
                                    while ((currentReceivePos - startPos) > 0)
                                    {
                                        Package p = PackageFactory.CreatePackage(receiveData, startPos, currentReceivePos - startPos);
                                        if (p == null)
                                        {
#if DEBUG_CORE
                                            Logger.Log("[Client ReceiveCallBack] package p = null");
#endif
                                            break;
                                        }
                                        startPos += p.AllSize;
#if DEBUG_CORE
                                        Logger.Log("[Client ReceiveCallBack] create Package success, startPos:" + startPos.ToString());
#endif
//                                        Logger.LogError("Recive Package p.Header.MessageId:" + p.Header.MessageId.ToString());
                                        //if (p.Header.MessageId == 200007) ++a;
                                        //Logger.Log(string.Format("[Client ReceiveCallBack] receive detail:{0} baseLength:{1} datalength:{2}, messageId:{3}, header.Seq:{4} mySeq:{5}", 0, p.Header.Length, p.Data.Length, p.Header.MessageId, p.Header.GetSeqNum(), receiveSeqId));
                                        if (p.Header.CheckSeqNum(receiveSeqId + 1) == false)
                                        {
                                            throw new Exception(String.Format("[Client ReceiveCallBack] error seq message {0}.", p.Header.MessageId));
                                        }
                                        p.SetKey(packageKey);
                                        if (p.decode() == false)
                                        {
                                            throw new Exception(String.Format("[Client ReceiveCallBack] client decode failed, message:{0}", p.Header.MessageId));
                                        }
                                        receiveSeqId++;
                                        if (p.Header.MessageId == CLIENT_KEY_MESSAGE_RESPONSE_ID) //这是在处理连接验证吧
                                        {
                                            Sio.SData keyData = new Sio.SData();
                                            keyData.UnSerializ(new System.IO.MemoryStream(p.Data));
                                            packageKey = keyData.uintValue;
                                            handle.Event(this, Event.Type.CONNECT);
                                            break;
                                        }
                                        //服务器心跳特殊处理
                                        if (p.Header.MessageId == 0)
                                        {
                                            if (SNet.NModuleManager.GetInstance() != null && SNet.NModuleManager.GetInstance().OnHeartReceive != null)
                                            {
                                                SNet.NModuleManager.GetInstance().OnHeartReceive();
                                            }
                                        }

                                        handle.Packet(this, p);
                                    }

                                    if (startPos != 0)
                                    {
                                        // 把未处理的字节数组放到最前面去
                                        Array.Copy(receiveData, startPos, receiveData, 0, currentReceivePos - startPos);
                                        currentReceivePos = currentReceivePos - startPos;
                                    }
                                }
                                StartRead();
                            }
                            else
                            {
#if DEBUG_CORE
                                Logger.Log("[Client ReceiveCallBack] receiveBytes = 0 socketID:" + socketID);
                                Logger.Log(handle);
#endif
                                if (handle != null)
                                {
                                    handle.Event(this, Event.Type.CLOSE, Convert.ToInt32(NetError.NETERROR_RECEIVE));
                                    Close();
                                }
                            }
                        }
                    }
                }
             
                catch (System.Exception ex)
                {
                    //Logger.LogError("[Client ReceiveCallBack] 消息接收异常:" + ex + " 异常 socketID:" + socketID);

                    if (handle != null)
                    {
                        handle.Event(this, Event.Type.ERROR, Convert.ToInt32(NetError.NETERROR_RECEIVE));
                        Close();
                    }
                } 
             
            }


            private void StartWrite()
            {
#if DEBUG_CORE
//                Logger.Log("[Client StartWrite] start send socketID:" + socketID);
#endif
                lock (sendlock)
                {
                    try
                    {
                        if (isSend == false && sendDataList.Count != 0 && c.Connected)
                        {
                            isSend = true;
                            sendData = null;
                            do
                            {
                                if (sendDataList.First == null)
                                    continue;
                                Package p = sendDataList.First.Value;
                                sendDataList.RemoveFirst();
                                //
                                p.SetKey(1);
                                p.encode();                                
                                sendData = p.GetBytes();
                                var x = 0;
                                x = x + 1;
                                break;
                            } while (sendDataList.Count != 0);

                            sendDataPos = 0;
                            if (sendData != null)
                            {
#if DEBUG_CORE
                                Logger.Log("[Client StartWrite] send bytes:" + sendData.Length.ToString());
#endif
//                                Logger.Log("sendData ======= " + sendData.Length);
//                                Logger.Log(sendData.ToString());
                                c.BeginSend(sendData, sendDataPos, sendData.Length - sendDataPos, 0, new AsyncCallback(SendCallBack), c);
                            }
                        }
                        else
                        {
                            //if (isSend == true)
                            //{
                            //    Logger.Log("issend equal true");
                            //}
                            //else if (sendDataList.Count == 0)
                            //{
                            //    Logger.Log("send data list count{0}", sendDataList.Count);
                            //}
                            //else if (c.Connected == false)
                            //{
                            //    Logger.Log("current connected is false");
                            //}
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Logger.LogError("[Client StartWrite] 消息发送异常:" + ex);
                        if (handle != null)
                            handle.Event(this, Event.Type.ERROR, Convert.ToInt32(NetError.NETERROR_SEND));
                        Close();
                    }
                }
            }

            public void SendCallBack(IAsyncResult ir)
            {
                Socket s = (Socket)ir.AsyncState;

                try
                {
                    int bytesSend = s.EndSend(ir);
                    if (bytesSend > 0)
                    {
                        sendDataPos += bytesSend;
                        if (sendDataPos < sendData.Length)
                        {
#if DEBUG_CORE
                            Logger.Log("[Client SendCallBack] send success, bytes:" + bytesSend.ToString());
#endif

                            s.BeginSend(sendData, sendDataPos, sendData.Length - sendDataPos, 0, new AsyncCallback(SendCallBack), s);
                        }
                        else
                        {
                            lock (sendlock)
                            {
                                isSend = false;
                                sendData = null;
                            }
                            StartWrite();
                        }
                    }
                    else
                    {
                        Logger.LogError("[Client SendCallBack] 消息发送异常, bytesSend = 0");
                        if (handle != null)
                        {
                            handle.Event(this, Event.Type.ERROR, Convert.ToInt32(NetError.NETERROR_SEND));
                            //handle.Event(this, Event.Type.CLOSE);
                            //handle.Error(this, Convert.ToInt32(NetError.NETERROR_SEND));
                            //handle.Close(this);
                            Close();
                        }
                    }

                }
                catch (System.Exception ex)
                {
                    Logger.LogError("[Client SendCallBack] 消息发送异常:" + ex);
                    if (handle != null)
                    {
                        handle.Event(this, Event.Type.ERROR, Convert.ToInt32(NetError.NETERROR_SEND));
                        //handle.Event(this, Event.Type.CLOSE);
                        //handle.Error(this, Convert.ToInt32(NetError.NETERROR_SEND));
                        //handle.Close(this);
                        Close();
                    }
                }
            }


            #region 属性
            NetHandle handle = null;

            public NetHandle Handle
            {
                get { return handle; }
                set { handle = value; }
            }

            public PackageFactory PackageFactory = new PackageFactory();

            int socketID = 0;
            public int SocketID
            {
                get { return socketID; }
            }

            private Socket c = null;
            private byte[] sendData = null;
            private int sendDataPos = 0;
            const int MEM_SIZE = 1024 * 1024;
            private byte[] receiveData = new byte[MEM_SIZE];
            private int currentReceivePos = 0;
            private bool isSend = false;
            private object sendlock = new object();
            private bool isReceive = false;
            private object receiveLock = new object();
            private LinkedList<Package> sendDataList = new LinkedList<Package>();//发送消息包
            private uint sendSeqId = 0;
            private uint receiveSeqId = 0;

            public bool IsNeedExchangeKey = false;
            private bool isExchenagedKey = false;
            private uint packageKey = 0;

            private uint nextSendPackageExtData = 0;
            private string dispatchFunc = null;

            public bool isConnected
            {
                get
                {
                    if (c != null)
                    {
                        return c.Connected;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            public uint NextSendPackageExtData
            {
                get { return nextSendPackageExtData; }
                set { nextSendPackageExtData = value; }
            } 

            public string DispatchFunc
            {
                get { return dispatchFunc; }
                set { dispatchFunc = value; }
            }
            #endregion
        }
    }

}
