using System;
using System.Collections;
using UnityEngine;
using System.IO;
 
 
public class SaveLog : MonoBehaviour
{
    private float length;
    public string filepath; //给模拟器用的文件存储路径
    Queue queue;

    private void Start()
    {
#if UNITY_EDITOR
        var utcNow = DateTime.UtcNow;
        var timeSpan = utcNow - new DateTime(1970, 1, 1, 0, 0, 0);
        var tname = (int)timeSpan.TotalSeconds;
        filepath = Application.dataPath + "/../Logs/"+"log"+tname+".txt";
        StreamWriter writer =new StreamWriter(filepath, true, System.Text.Encoding.Default);
        writer.WriteLine("start------");
        writer.Close();
#else

#endif

    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
        LogToFile("Version of the runtime: " + Application.unityVersion, true, false);
        Application.logMessageReceivedThreaded += OnReceiveLogMsg;
        queue = new Queue();
    }
    // Start is called before the first frame update
    
    void OnReceiveLogMsg(string condition, string stackTrace, LogType type) {
        string _type = "";
        switch (type)
        {
            case LogType.Error:
                _type = "error";
                break;
            case LogType.Assert:
                _type = "Assert";
                break;
            case LogType.Warning:
                _type = "Warning";
                break;
            case LogType.Log:
                _type = "Log";
                break;
            case LogType.Exception:
                _type = "Exception";
                break;
            default:
                break;
        }
        string msg = "[MSG]:" + condition + "--" + "[station]:" + stackTrace + "-" + "[LogType]:" + _type;
        queue.Enqueue(msg);
    }
    // Update is called once per frame
    void Update()
    {
        CheckLogs();
    }
    void CheckLogs() {
        if (queue.Count != 0) {
            LogToFile(queue.Peek().ToString(), true, true,()=> { queue.Dequeue(); });           
        }
    }
    public  void LogToFile(string str, bool bwithTime, bool bAppendLineFeed,System.Action callback = null) {
        if (str == null) return;
        try
        {
#if UNITY_EDITOR
            string fname = filepath;
#else
            string fname = Application.persistentDataPath+ "/Unitylog.txt";
#endif
 
 
            if (fname == "" || fname == null) return;
 
            StreamWriter writer = new StreamWriter(fname, true, System.Text.Encoding.Default);
            
            if (bwithTime) writer.WriteLine("\r\n\r\n---------" + System.DateTime.Now.ToString());
            if (bAppendLineFeed) writer.WriteLine(str);
            else writer.Write(str);
            writer.Close();
            callback?.Invoke();
        }
        catch
        {
 
            throw;
        }
    }
}