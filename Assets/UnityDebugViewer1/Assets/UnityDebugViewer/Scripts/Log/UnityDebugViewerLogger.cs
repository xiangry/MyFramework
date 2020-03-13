﻿/// Copyright (C) 2020 AsanCai   
/// All rights reserved
/// Email: 969850420@qq.com

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using UnityEngine;

namespace UnityDebugViewer
{
    /// <summary>
    /// socket用于传递log数据的structure
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)] //按1字节对齐
    public struct TransferLogData
    {
        public int logType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string info;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string stack;

        public TransferLogData(string _info, string _stack, LogType type)
        {
            var infoLength = _info.Length > 512 ? 512 : _info.Length;
            info = _info.Substring(0, infoLength);
            var stackLength = _stack.Length > 1024 ? 1024 : _stack.Length;
            stack = _stack.Substring(0, stackLength);
            logType = (int)type;
        }
    }

    [Serializable]
    public struct CollapsedLogData
    {
        public LogData log;
        public int count;
    }

    /// <summary>
    /// 保存log数据
    /// </summary>
    [Serializable]
    public class LogData  
    {
        /// <summary>
        /// Regular expression for all the stack message generated by unity
        /// example: 
        /// UnityDebugViewer.TestLog:Awake() (at Assets/UnityDebugViewer/Test/TestLog.cs:12)
        /// UnityEngine.<_loading>c__Iterator0:.ctor(ILIntepreter, StackObject*, IList`1, Boolean,Boolean&)
        /// </summary>
        public const string UNITY_STACK_REGEX = @"(?m)^(?<className>[\w]+(\.[\<\>\w\s\,\`]+)*)[\s]*[\.:][\s]*(?<methodName>[\<\>\w\s\,\`\.]+[\s]*\([\w\s\,\.\[\]\<\>\&\*\`]*\))[\s]*(\([at]*[\s]*(?<filePath>(([\w\.\s]+:)?[\\/])?([\w\.\s]+[\\/])*[\w\.\s]*\.[\w]+)[\s]*:[\s]*(?<lineNumber>[\d]+)\))*\r*$";

        /// <summary>
        /// Regular expression for all the stack message generated by android system
        /// example:
        /// at com.android.server.am.ProcessManagerService$8.run(ProcessManagerService.java:1693)
        /// </summary>
        public const string ANDROID_STACK_REGEX = @"[at]*[\s]*(?<className>([\w\$]+\.)+([\w\$]+))\.(?<methodName>[\w\$]+)\((?<filePath>[\w]+\.[\w]+)\:(?<lineNumber>[\d]+)*\)";
        public const string ANDROID_STACK_REGEX_WITH_PARAM = @"[at]*[\s]*(?<className>([\w\$]+\.)+([\w\$]+))\.(?<methodName>[\w\$]+\([\w\s\,\.\[\]\<\>\&\*\`]*\))";
        

        /// <summary>
        /// Regular expression for the stack message generated by unity when compiling
        /// </summary>
        public const string UNITY_COMPILE_LOG_REGEX = @"(?<filePath>(.+:[\\/])?(.+[\\/])*[\w]+\.[\w]+)\((?<lineNumber>[\d]+).+\):";

        public string info { get; private set; }
        public string extraInfo { get; private set; }
        public string time { get; private set; }
        public LogType type { get; private set; }
        public string stackMessage { get; private set; }

        private List<LogStackData> _stackList;
        public List<LogStackData> stackList
        {
            get
            {
                if(_stackList == null)
                {
                    _stackList = new List<LogStackData>();
                }

                return _stackList;
            }
        }

        public LogData(string info, string stack, LogType type) : this(info, stack, DateTime.Now.ToString("HH:mm:ss.fff"), type) { }

        public LogData(string info, string stack, string time, LogType type)
        {
            this.info = info ?? string.Empty;
            this.type = type;
            this.stackMessage = stack ?? string.Empty;
            this.time = time ?? string.Empty;

            /// stack message is null means that it is generated by compilation
            if (string.IsNullOrEmpty(stack))
            {
                if(string.IsNullOrEmpty(info) == false)
                {
                    Match compileMatch = Regex.Match(info, UNITY_COMPILE_LOG_REGEX);
                    if (compileMatch.Success)
                    {
                        var logStack = new LogStackData(compileMatch);
                        this.stackList.Add(logStack);
                        this.info = Regex.Replace(info, UNITY_COMPILE_LOG_REGEX, "").Trim();
                        this.stackMessage = logStack.fullStackMessage;
                    }
                }
                
                return;
            }

            
            try
            {
                string[] stackArray = stack.Split('\n');

                if(stackArray != null)
                {
                    for (int i = 0; i < stackArray.Length; i++)
                    {
                        var match = Regex.Match(stackArray[i], UNITY_STACK_REGEX);
                        if (match.Success)
                        {
                            this.stackList.Add(new LogStackData(match));
                            match = match.NextMatch();
                            continue;
                        }

                        match = Regex.Match(stackArray[i], ANDROID_STACK_REGEX);
                        if (match.Success)
                        {
                            this.stackList.Add(new LogStackData(match));
                            match = match.NextMatch();
                            continue;
                        }

                        match = Regex.Match(stackArray[i], ANDROID_STACK_REGEX_WITH_PARAM);
                        if (match.Success)
                        {
                            this.stackList.Add(new LogStackData(match));
                            match = match.NextMatch();
                            continue;
                        }

                        this.extraInfo = string.Format("{0}{1}\n", this.extraInfo, stackArray[i]);
                    }
                }

                this.extraInfo = this.extraInfo.Trim();
                if (string.IsNullOrEmpty(extraInfo))
                {
                    if (stackList.Count > 0 && stackList[0].lineNumber == -1)
                    {
                        var stackData = stackList[0];
                        stackList.RemoveAt(0);
                        this.extraInfo = stackData.ToString();
                    }
                }
            }
            catch
            {
                /// get the extraInfo of log
                this.extraInfo = stack.Trim();
            }
        }

        public LogData(string info, string extraInfo, List<StackFrame> stackFrameList, LogType logType) : this(info, extraInfo, stackFrameList, DateTime.Now.ToString("HH:mm:ss.fff"), logType) { }

        public LogData(string info, string extraInfo, List<StackFrame> stackFrameList, string time, LogType logType)
        {
            this.info = info ?? string.Empty;
            this.type = logType;
            this.extraInfo = extraInfo ?? string.Empty;
            this.time = time ?? string.Empty;
            this.stackMessage = extraInfo ?? string.Empty;

            if (stackFrameList == null)
            {
                return;
            }

            for(int i = 0; i < stackFrameList.Count; i++)
            {
                var logStackData = new LogStackData(stackFrameList[i]);
                this.stackMessage = string.Format("{0}\n{1}", this.stackMessage, logStackData.fullStackMessage);
                this.stackList.Add(logStackData);
            }
        }

        public LogData(string info, string extraInfo, string stackMessage, List<LogStackData> stackList, string time, LogType type)
        {
            this.info = info ?? string.Empty;
            this.extraInfo = extraInfo ?? string.Empty;
            this.time = time ?? string.Empty;
            this.stackMessage = stackMessage ?? string.Empty;
            this.type = type;

            if (stackList != null)
            {
                this.stackList.AddRange(stackList);
            }
        }

        public string GetKey()
        {
            string key = string.Format("{0}{1}{2}", info, stackMessage, type);
            return key;
        }

        public string GetContent(bool showTime = false)
        {
            string content = showTime ? string.Format("[{0}] {1}", this.time, this.info) : this.info;
            return content;
        }

        public bool Equals(LogData data)
        {
            if (data == null)
            {
                return false;
            }

            return this.info.Equals(data.info) 
                && this.stackMessage.Equals(data.stackMessage) 
                && this.type == data.type;
        }

        public LogData Clone()
        {
            LogData log = new LogData(
                this.info, 
                this.extraInfo, 
                this.stackMessage, 
                this.stackList, 
                this.time, 
                this.type);

            return log;
        }

        public override string ToString()
        {
            string logType = string.Empty;
            switch (this.type)
            {
                case LogType.Log:
                    logType = "log";
                    break;
                case LogType.Warning:
                    logType = "warning";
                    break;
                case LogType.Error:
                    logType = "error";
                    break;
                default:
                    logType = "error";
                    break;

            }

            string str = string.Format("[{0}] {1} {2}\n{3}", logType, this.time, this.info, this.extraInfo);
            for(int i =0;i < stackList.Count; i++)
            {
                if(stackList[i] == null)
                {
                    continue;
                }

                str = string.Format("{0}\n{1}", str, stackList[i].ToString());
            }

            return str;
        }
    }


    [Serializable]
    public class LogStackData
    {
        public string className { get; private set; }
        public string methodName { get; private set; }
        public string filePath { get; private set; }
        public int lineNumber { get; private set; }

        public string fullStackMessage { get; private set; }
        public string sourceContent;

        public LogStackData(Match match)
        {
            if(match == null)
            {
                SetDefaultValue();
                return;
            }

            this.sourceContent = String.Empty;

            this.className = match.Result("${className}");
            this.methodName = match.Result("${methodName}");
            

            this.filePath = UnityDebugViewerEditorUtility.ConvertToSystemFilePath(match.Result("${filePath}"));
            string lineNumberStr = match.Result("${lineNumber}");
            int lineNumber;
            this.lineNumber = int.TryParse(lineNumberStr, out lineNumber) ? lineNumber : -1;


            if (this.filePath.Equals("${filePath}") || this.lineNumber == -1)
            {
                this.fullStackMessage = string.Format("{0}:{1}", this.className, this.methodName);
            }
            else
            {
                if (this.className.Equals("${className}") || this.methodName.Equals("${methodName}"))
                {
                    if (this.className.Equals("${className}"))
                    {
                        this.className = "UnknowClass";
                    }

                    if (this.methodName.Equals("${methodName}"))
                    {
                        this.methodName = "UnknowMethod()";
                    }
                }

                if (this.methodName.Contains("(") == false)
                {
                    this.methodName += "()";
                }

                this.fullStackMessage = string.Format("{0}:{1} (at {2}:{3})", this.className, this.methodName, this.filePath, this.lineNumber);
            }
        }

        public LogStackData(StackFrame stackFrame)
        {
            if(stackFrame == null)
            {
                SetDefaultValue();
                return;
            }

            var method = stackFrame.GetMethod();

            string methodParam = string.Empty;
            var paramArray = method.GetParameters();
            if (paramArray != null)
            {
                string[] paramType = new string[paramArray.Length];
                for (int index = 0; index < paramArray.Length; index++)
                {
                    paramType[index] = paramArray[index].ParameterType.Name;
                }
                methodParam = string.Join(", ", paramType);
            }

            this.className = method.DeclaringType.Name;
            this.methodName = string.Format("{0}({1})", method.Name, methodParam); ;
            this.filePath = stackFrame.GetFileName();
            this.lineNumber = stackFrame.GetFileLineNumber();

            this.fullStackMessage = string.Format("{0}:{1} (at {2}:{3})", this.className, this.methodName, this.filePath, this.lineNumber);
            this.sourceContent = String.Empty;
        }

        private void SetDefaultValue()
        {
            className = string.Empty;
            methodName = string.Empty;
            filePath = string.Empty;
            lineNumber = -1;
            fullStackMessage = string.Empty;
            sourceContent = string.Empty;
        }

        public bool Equals(LogStackData data)
        {
            if(data == null)
            {
                return false;
            }

            return fullStackMessage.Equals(data.fullStackMessage);
        }

        public override string ToString()
        {
            return this.fullStackMessage;
        }
    }

    /// <summary>
    /// Attribute to mark whether the stack message of target method should be ignore when parsing the system stack trace message
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class IgnoreStackTrace : Attribute
    {
        /// <summary>
        /// Decide if the target method is displayed as extraInfo
        /// </summary>
        public bool showAsExtraInfo { get; private set; }

        public IgnoreStackTrace(bool show)
        {
            showAsExtraInfo = show;
        }

        public IgnoreStackTrace()
        {
            /// don't diaplay as extraInfo in default
            showAsExtraInfo = false;
        }
    }


    public class UnityDebugViewerLogger
    {
        public static void AddLog(string info, string stack, LogType type, string modeName)
        {
            var logData = new LogData(info, stack, type);
            AddLog(logData, modeName);
        }

        public static void AddLog(string info, string extraMessage, List<StackFrame> stackFrameList, LogType type, string modeName)
        {
            var logData = new LogData(info, extraMessage, stackFrameList, type);
            AddLog(logData, modeName);
        }

        public static void AddLog(LogData data, string modeName)
        {
            UnityDebugViewerEditorManager.GetEditor(modeName).AddLog(data);
        }

        [IgnoreStackTrace(true)]
        public static void Log(string str, string modeName = UnityDebugViewerDefaultMode.Editor)
        {
            AddSystemLog(str, LogType.Log, modeName);
        }

        [IgnoreStackTrace(true)]
        public static void LogWarning(string str, string modeName = UnityDebugViewerDefaultMode.Editor)
        {
            AddSystemLog(str, LogType.Warning, modeName);
        }

        [IgnoreStackTrace(true)]
        public static void LogError(string str, string modeName = UnityDebugViewerDefaultMode.Editor)
        {
            AddSystemLog(str, LogType.Error, modeName);
        }

        [IgnoreStackTrace]
        private static void AddSystemLog(string str, LogType logType, string modeName)
        {
            string extraInfo = string.Empty;
            var stackList = ParseSystemStackTrace(ref extraInfo);
            AddLog(str, extraInfo, stackList, logType, modeName);
        }

        [IgnoreStackTrace]
        private static List<StackFrame> ParseSystemStackTrace(ref string extraInfo)
        {
            List<StackFrame> stackFrameList = new List<StackFrame>();

            StackTrace stackTrace = new StackTrace(true);
            StackFrame[] stackFrames = stackTrace.GetFrames();

            for (int i = 0; i < stackFrames.Length; i++)
            {
                StackFrame stackFrame = stackFrames[i];
                var method = stackFrame.GetMethod();

                if (!method.IsDefined(typeof(IgnoreStackTrace), true))
                {
                    /// ignore all the stack message generated by Unity internal method
                    if (method.Name.Equals("InternalInvoke"))
                    {
                        break;
                    }

                    stackFrameList.Add(stackFrame);
                }
                else
                {
                    foreach (object attributes in method.GetCustomAttributes(false))
                    {
                        IgnoreStackTrace ignoreAttr = (IgnoreStackTrace)attributes;
                        /// check and display corresponding method as extraInfo
                        if (ignoreAttr != null && ignoreAttr.showAsExtraInfo)
                        {
                            string methodParam = string.Empty;
                            var paramArray = method.GetParameters();
                            if (paramArray != null)
                            {
                                string[] paramType = new string[paramArray.Length];
                                for (int index = 0; index < paramArray.Length; index++)
                                {
                                    paramType[index] = paramArray[index].ParameterType.Name;
                                }
                                methodParam = string.Join(", ", paramType);
                            }

                            extraInfo = string.Format("{0}:{1}({2})", method.DeclaringType.FullName, method.Name, methodParam);
                        }
                    }
                }
            }

            return stackFrameList;
        }
    }
}
