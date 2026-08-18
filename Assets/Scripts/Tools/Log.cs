//
// Log.cs

using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;


    public class Log
    {
        //日志输出线程
        static LogOutPutThread s_LogOutPutThread = new LogOutPutThread();
        public static void Init(bool isOpenLog = true) {
            Debug.unityLogger.logEnabled = isOpenLog;
            if (isOpenLog)
            {
                s_LogOutPutThread.Init();
                Application.logMessageReceivedThreaded += UnityLogCallBackThread;
                Application.logMessageReceived += UnityLogCallBack;
            }

        }

        static void UnityLogCallBackThread(string log, string track, LogType type)
        {
            LogInfo l_logInfo = new LogInfo
            {
                m_logContent = log,
                m_logTrack = track,
                m_logType = type
            };
            s_LogOutPutThread.Log(l_logInfo);
        }

        static void UnityLogCallBack(string log, string track, LogType type)
        {
            //LogInfo l_logInfo = new LogInfo
            //{
            //    m_logContent = log,
            //    m_logTrack = track,
            //    m_logType = type
            //};
        }

        public static void Print(object message)
        {
            Debug.Log("[Log]" + message);
        }

        public static void Print(string message)
        {
            Debug.Log("[Log]" + message);
        }

        public static void Print(object message, Color color)
        {
            Debug.Log("<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">[Log]"+ message + "</color> ");
        }

        public static void Print(string message, Color color)
        {
            Debug.Log("<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">[Log]" + message + "</color> ");
        }

        public static void PrintWarning(object message)
        {
            Debug.LogWarning("<color=#ffa673>[Warnning]</color> " + message);
        }
        public static void PrintWarning(string message)
        {
            Debug.LogWarning("<color=#ffa673>[Warnning]</color> " + message);
        }

        public static void PrintError(object message)
        {
            Debug.LogError("<color=#db4259>[Error]</color> " + message);
        }

        public static void PrintError(string message)
        {
            Debug.LogError("<color=#db4259>[Error]</color> " + message);
        }


        public static void PrintObject<T>(T Obj, string exstr = "")
        {
            string str = "NULL";
            //if (Obj != null)
            //{
            //    str = JsonMapper.ToJson(Obj);
            //}
            Debug.Log("[LogObject] " + exstr + str);
        }
    }
