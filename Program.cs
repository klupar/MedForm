using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TeethCard;

namespace MedForm
{
  internal static class Program
  {
    public static CommandLine CommandLine;
    public static Settings Settings;
    public static FormMain FormMain;
    public static MedCard MedCard;
    public const string MainWindowTitle = "Медкарта";
    public static Program.VersionType Version;

    [DllImport("User32.dll")]
    private static extern IntPtr SetForegroundWindow(int hWnd);

    [STAThread]
    private static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      try
      {
        foreach (Process process in Process.GetProcesses())
        {
          if (process.MainWindowTitle == "Медкарта")
          {
            Program.SetForegroundWindow((int) process.MainWindowHandle);
            return;
          }
        }
        SystemLog.Open();
        SystemLog.Log("MedForm started. Arguments: " + string.Join(" ", Environment.GetCommandLineArgs()));
        Program.Initialize();
        bool bHasTeethCard = bool.Parse(Program.CommandLine.GetOpt("TeethCard"));
        if (bHasTeethCard)
        {
          try
          {
            Dictionaries.Load();
            Program.MedCard = new MedCard();
            Program.MedCard.Load(DBUtil.RecordID);
          }
          catch (Exception ex)
          {
            throw new Exception("Ошибка при загрузке данных из БД:\n" + ex.ToString());
          }
        }
        Program.FormMain = new FormMain(bHasTeethCard);
        Application.Run((Form) Program.FormMain);
        DBUtil.Disconnect();
        Program.Settings.Save();
      }
      catch (Exception ex)
      {
        SystemLog.LogException(ex);
        int num = (int) MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
      SystemLog.Log("MedForm closed");
    }

    private static void Initialize()
    {
      Program.CommandLine = new CommandLine(false);
      Program.CommandLine.AddOption(new CommandLine.Option()
      {
        Name = "TableID",
        ShortName = "-t",
        Type = CommandLine.OptionType.Int,
        Mandatory = true
      });
      Program.CommandLine.AddOption(new CommandLine.Option()
      {
        Name = "RecordID",
        ShortName = "-r",
        Type = CommandLine.OptionType.Int,
        Mandatory = true
      });
      Program.CommandLine.AddOption(new CommandLine.Option()
      {
        Name = "UserID",
        ShortName = "-employeeid",
        Type = CommandLine.OptionType.Int,
        Mandatory = false,
        DefVal = "1"
      });
      Program.CommandLine.AddOption(new CommandLine.Option()
      {
        Name = "TeethCard",
        ShortName = "-c",
        Type = CommandLine.OptionType.Bool,
        Mandatory = false
      });
      Program.Settings = new Settings();
      Program.Settings.Set("WindowWidth", "640");
      Program.Settings.Set("WindowHeight", "480");
      Program.Settings.Set("WindowState", "0");
      Program.Settings.Set("FieldNameWidth", "100");
      Program.Settings.Set("FieldWidth", "0");
      Program.Settings.Set("ControlXSpacing", "10");
      Program.Settings.Set("ControlYSpacing", "10");
      Program.Settings.Set("ControlHeight", "20");
      Program.Settings.Set("MultilineControlHeight", "100");
      Program.Settings.Set("FileLinkPath", "C:\\Temp");
      Program.Settings.Set("ImagePath", "");
      Program.Settings.Set("DBConnectionString", "character set=None;initial catalog=C:\\Pack\\bin\\beauty.fdb;data source=localhost;port number=3050;dialect=3;password=masterkey;user id=SYSDBA");
            Program.CommandLine.Parse();
      Program.Settings.Load();
      DBUtil.Initialize();
      Program.Versioning();
    }

    private static void Versioning()
    {
      try
      {
        Program.Version = (Program.VersionType) Enum.Parse(typeof (Program.VersionType), DBUtil.GetProcResult(DBUtil.SelectProc("DENT_GET_MEDFORM_VERSION"), "formversion"));
      }
      catch (Exception)
      {
        Program.Version = Program.VersionType.GoodOldVersion;
      }
    }

    public enum VersionType
    {
      GoodOldVersion = 1,
      NewProVersion = 2,
    }
  }
}
