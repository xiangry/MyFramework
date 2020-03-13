using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using FileUtil = Core.Resource.FileUtil;

public class KVFileHelper : MonoSingleton<KVFileHelper>
{
    class FileWriteMsg
    {
        public string file;
        public string msg;

        public FileWriteMsg(string filePath, string writeMsg)
        {
            file = filePath;
            msg = writeMsg;
        }
    }
    
    private float length;
    public string filepath; //给模拟器用的文件存储路径
    private string fileName = "debug.log";
    Queue queue;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        queue = new Queue();
    }
    // Start is called before the first frame update

    public void WriteLine(string filepath, string line)
    {
        queue.Enqueue(new FileWriteMsg(filepath, line));
    }
    // Update is called once per frame
    void Update()
    {
        CheckWriteData();
    }
    
    void CheckWriteData() {
        if (queue.Count != 0) {
            WriteToFile(queue.Peek() as FileWriteMsg,()=> { queue.Dequeue(); });           
        }
    }
    private void WriteToFile(FileWriteMsg msg, System.Action callback = null) {
        if (msg == null)
        {
            callback?.Invoke();
            return;
        }
        
        try
        {
            filepath = FileUtil.GetWritePath(msg.file);
            StreamWriter writer = new StreamWriter(filepath, true, System.Text.Encoding.Default);
            writer.WriteLine(msg.msg);
            writer.Close();
        }
        finally
        {
            callback?.Invoke();
        }
    }
}