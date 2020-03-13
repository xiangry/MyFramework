using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CMDController
{
    public static string nextCmdFile = "";
    public static string nextCmdArgs = "";

    //        @"C:\kvgame\sot\hot_update\tools\update_flow.bat";
    public static string CMDHotUpdateFile = Application.dataPath + "/../../hot_update/tools/update_flow.bat";

    public static void ExcuteCmdFile(string cmdFile = null, string cmdArgs = null)
    {
        nextCmdFile = cmdFile;
        nextCmdArgs = cmdArgs;
        Thread t = new Thread(new ThreadStart(NewFileCmdThread));
        t.Start();
        NewFileCmdThread();
    }

    static void NewFileCmdThread()
    {
        Debug.Log("New File Thread --------------");
        Debug.Log($"CMD File Path {CMDHotUpdateFile}--------------");
        string str = RunCMD(nextCmdFile, nextCmdArgs);
        Debug.Log($"执行命令:{nextCmdFile} 参数：{nextCmdArgs}\n结果是:{str}");
    }

    private static string RunCMD(string cmdFile, string args)
    {
        if (cmdFile == null)
        {
            Debug.LogError("命令指定的文件不存在");
            return "Error 命令指定的文件不存在";
        }
        Process process = new Process();
        process.StartInfo.FileName = cmdFile;
        process.StartInfo.Arguments = args;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
//        process.WaitForExit();
        return process.StandardOutput.ReadToEnd();
    }
}