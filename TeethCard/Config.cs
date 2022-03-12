using MedForm;
using System.IO;
using System.Reflection;

namespace TeethCard
{
  internal class Config
  {
    public static PaintConfig PaintConfig = new PaintConfig();
    public const int DEFAULT_MAX_DIAGNOSIS = 2;
    public const int DEFAULT_MAX_STATUS = 2;
    public static string ImagePathRel;
    public static string ImagePath;
    public static int MaxDiagnosis;

    public static string ReadString(string ParamName, string DefValue)
    {
      string str1 = DefValue;
      try
      {
        string str2 = Program.Settings.Get(ParamName);
        if (str2 != null && str2 != "")
          str1 = str2;
      }
      catch
      {
      }
      return str1;
    }

    public static int ReadInt(string ParamName, int DefValue)
    {
      int num = DefValue;
      try
      {
        num = int.Parse(Program.Settings.Get(ParamName));
      }
      catch
      {
      }
      return num;
    }

    public static void WriteString(string ParamName, string ParamValue)
    {
      Program.Settings.Set(ParamName, ParamValue);
    }

    public static void WriteInt(string ParamName, int ParamValue)
    {
      Config.WriteString(ParamName, ParamValue.ToString());
    }

    public static void Load()
    {
      Config.ImagePathRel = Program.Settings.Get("ImagePath");
      Config.ImagePath = Config.ImagePathRel;
      if (Config.ImagePath == null)
        Config.ImagePath = "";
      if (!Path.IsPathRooted(Config.ImagePath))
      {
        Config.ImagePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + Config.ImagePath;
        if (!Config.ImagePath.EndsWith("\\"))
          Config.ImagePath += "\\";
      }
      try
      {
        Config.PaintConfig.Load();
      }
      catch
      {
      }
      Config.MaxDiagnosis = Config.ReadInt("MaxDiagnosis", 2);
    }

    public static void Save()
    {
      Config.WriteString("ImagePath", Config.ImagePathRel);
      Config.PaintConfig.Save();
      Config.WriteInt("MaxDiagnosis", Config.MaxDiagnosis);
    }
  }
}
