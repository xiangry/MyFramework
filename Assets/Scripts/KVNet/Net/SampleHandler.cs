using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
/************************************************************************/
/* 网络hand
 * 
 */
/************************************************************************/
namespace Core
{
    namespace Net
    {
        public class SampleHandler : NetHandle
        {
            private static readonly object syncLock = new object();

            static int HEART_MESSAGE_REQUEST_ID = 0;
            static int HEART_MESSAGE_RESPONSE_ID = 0;
            public Action OnHeartReceive;
 
            //TODO: 测试一下C#Queue性能表现
            private LinkedList<Package> m_reciveList = new LinkedList<Package>();
            private LinkedList<Package> m_standbyReciveList = new LinkedList<Package>();

            private LinkedList<Package> m_pendingProcessList = new LinkedList<Package>();
            private LinkedList<NModule> moduleList = new LinkedList<NModule>();

            private LinkedList<Event> m_stateEventList = new LinkedList<Event>();
            private LinkedList<Event> m_standbyStateEventList = new LinkedList<Event>();
            private LinkedList<Event> m_processingEventList = new LinkedList<Event>();

            public virtual bool Init()
            {
                return true;
            }

            private static void SwapList<T>(ref T l1, ref T l2)
            {
                T t = l1;
                l1 = l2;
                l2 = t;
            }
 

            public bool Event(Client c, Event.Type type, int ex)
            {
                Event p = new Event(c, type, ex);

                lock (syncLock)
                {
                    m_stateEventList.AddLast(p);
                }

                return true;
            }

            public int Packet(Client c, Package p)
            {
                if (p != null)
                {
                    if (p.MessageID == HEART_MESSAGE_REQUEST_ID)
                    {
                        p.MessageID = HEART_MESSAGE_RESPONSE_ID;
                        //Debug.LogWarning("[SampleHandler Packet] heart_message " + p.Header.Length);
                        c.Send(p); //copy and send   
                    }
                    else
                    {
                        p.C = c;
                        lock (syncLock)
                        {
                            m_reciveList.AddLast(p);
                        }
                    }
                    return p.AllSize;
                }
                return 0;
            }

            public virtual String GetMessageFuncName(int mid) { return ""; }


            private void ProcessPackageList(LinkedList<Package> packageList)
            {
                if (packageList.Count == 0)
                    return;

                LinkedListNode<Package> ln = packageList.First;
                while (ln != packageList.Last)
                { 
                    Process(ln.Value);
                    ln = ln.Next;
                }

                ln = packageList.Last;
                Process(ln.Value);
            }

            private void AddPendingProcessPackage(LinkedList<Package> packageList)
            {
                LinkedListNode<Package> ln = packageList.First;
                while (ln != packageList.Last)
                {
                    m_pendingProcessList.AddLast(ln.Value);
                } 
            }

            private void ProcessEventList(LinkedList<Event> eventList)
            {
                if (eventList.Count == 0)
                    return;

                LinkedListNode<Event> ln = eventList.First;
                Event p = null;
                while (ln != eventList.Last)
                {
                    p = ln.Value;
                    ProcessEvent(p.CT, p.ST, p.ER);
                    ln = ln.Next;
                }

                ln = eventList.Last;
                p = ln.Value;
                ProcessEvent(p.CT, p.ST, p.ER); ;
            }


            public virtual void Process()
            {
                if (m_reciveList.Count != 0 || m_pendingProcessList.Count != 0)
                {
//                    App3 app = App3.GetInstance();

                    //优先处理手动被堆积的数据
//                    if (app.get_enable_net_dispatch())
                    {
                        if (m_pendingProcessList.Count > 0)
                        {
                            ProcessPackageList(m_pendingProcessList);
                            m_pendingProcessList.Clear();
                        }
                    }

                    LinkedList<Package> tmpList = null;
                    if (m_reciveList.Count > 0)
                    {
                        lock (syncLock)
                        {
                            tmpList = m_reciveList;
                            SwapList(ref m_reciveList, ref m_standbyReciveList);
                        }

//                        if (app.get_enable_net_dispatch())
                        if (true)
                        {
                            ProcessPackageList(tmpList);
                            tmpList.Clear();
                        }
                    }
                }
                if (m_stateEventList.Count > 0)
                {
                    LinkedList<Event> tmpEventList = null;
                    lock (syncLock)
                    {
                        tmpEventList = m_stateEventList;
                        SwapList(ref m_stateEventList, ref m_standbyStateEventList);
                    }
 
                    ProcessEventList(tmpEventList);
                    tmpEventList.Clear();
                }
            }

            public virtual void Process(Package p)
            {
                int mid = p.MessageID;
                //Debug.Log("mid:"+mid);
                foreach (NModule o in moduleList)
                {
                    if (o.Startid <= mid && mid <= o.Endid)
                    {
                        o.Dispatch(p.C, p);
                    }
                }
            }

            public virtual bool ProcessEvent(Client c, Event.Type p, int error)
            {
                return true;
            }

            protected bool AddModule(NModule o)
            {
                if (o == null)
                    return false;
                foreach (NModule item in moduleList)
                {
                    if (item == o)
                        return false;
                }
                moduleList.AddLast(o);
                return true;
            }
        }
    }
}
