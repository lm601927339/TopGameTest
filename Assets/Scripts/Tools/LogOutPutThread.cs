using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;
using System.Text;

/// <summary>
/// 日志输出线程
/// 这里借鉴了QLog
/// </summary>
public class LogOutPutThread
{
    public const string LogPath = "Log";
    public const string expandName = "txt";

    private StreamWriter mLogWriter = null;

    public void Init()
    {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
        try
        {
            //ApplicationManager.s_OnApplicationQuit += Close;

            DateTime now = DateTime.Now;
            string logName = string.Format("Log{0}{1}{2}#{3}_{4}_{5}",
                now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            string logPath = PathTool.GetAbsolutePath(ResLoadLocation.Persistent, PathTool.GetRelativelyPath(LogPath, logName, expandName));
            Debug.Log("LogPath: "+logPath);

            UpLoadLogic(logPath);
            if (File.Exists(logPath))
                File.Delete(logPath);
            string logDir = Path.GetDirectoryName(logPath);

            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            mLogWriter = new StreamWriter(logPath);
            mLogWriter.AutoFlush = true;

        }
        catch(Exception e)
        {
            Debug.LogError("LogOutPutThread Init Exception:" + e.ToString());
        }
#endif
    }
   

    public void Log(LogInfo log)
    {
        if (log.m_logType == LogType.Error
                            || log.m_logType == LogType.Exception
                            || log.m_logType == LogType.Assert
                            )
        {
            this.mLogWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------");
            this.mLogWriter.WriteLine(System.DateTime.Now.ToString() + "\t" + log.m_logContent + "\n");
            this.mLogWriter.WriteLine(log.m_logTrack);
            this.mLogWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------");
        }
        else
        {
           // this.mLogWriter.WriteLine(System.DateTime.Now.ToString() + "\t" + log.m_logContent);
        }
    }

    public void Close()
    {
        ExitLogic();
        this.mLogWriter.Close();
    }

    public void Pause()
    {
        ExitLogic();
    }

    Dictionary<string, object> m_logData;

    //string ConfigName = "LogInfo";
    //string isCrashKey = "isCrash";
    //string logPathKey = "logPath";

    public void UpLoadLogic(string logPath)
    {
        //m_logData = ConfigManager.GetData(ConfigName);

        //if (m_logData.ContainsKey(ConfigName) && (bool)m_logData[isCrashKey] == true)
        //{
        //    Debug.Log("上传");
        //    //上传
        //    HTTPTool.Upload_Request(URLManager.GetURL("LogUpLoadURL"), (string)m_logData[logPathKey]);
        //}

        ////初始化数据
        //if (m_logData.ContainsKey(isCrashKey))
        //{
        //    m_logData[isCrashKey] = false;
        //    m_logData[logPathKey] = logPath;
        //}
        //else
        //{
        //    m_logData.Add(isCrashKey, false);
        //    m_logData.Add(logPathKey, logPath);
        //}
    }


    public void ExitLogic()
    {
        //if (m_logData.ContainsKey(isCrashKey))
        //{
        //    m_logData[isCrashKey] = false;
        //}
        //else
        //{
        //    m_logData.Add(isCrashKey, false);
        //}

        //ConfigManager.SaveData(ConfigName, m_logData);
    }

   

    static string GetPath(string logName)
    {
        return PathTool.GetAbsolutePath(ResLoadLocation.Persistent, PathTool.GetRelativelyPath(LogPath, logName, expandName));
    }


}
public class LogInfo
{
    public string m_logContent;
    public string m_logTrack;
    public LogType m_logType;
}
