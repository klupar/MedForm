using System;
using System.IO;
using System.Reflection;

namespace MedForm
{
  internal class SystemLog
  {
    private static StreamWriter LogFile;

    public static void Open()
    {
      SystemLog.LogFile = new StreamWriter(Assembly.GetEntryAssembly().Location + "." + DateTime.Now.ToString("yyyy-MM-dd") + ".log", true);
    }

    public static void Log(string sText)
    {
      if (SystemLog.LogFile == null)
        return;
      string str = DateTime.Now.ToString("yyyy.MM.dd hh.mm.ss");
      SystemLog.LogFile.WriteLine(str + " " + sText);
      SystemLog.LogFile.Flush();
    }

    public static void LogDBCommand(string sSQL)
    {
      SystemLog.Log("FbCommand CommandText=\"" + sSQL + "\"");
    }

    public static void LogException(Exception e)
    {
      SystemLog.Log("EXCEPTION: \"" + e.ToString() + "\" in " + e.StackTrace);
    }
  }
}
