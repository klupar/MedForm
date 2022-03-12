using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using TeethCard;

namespace MedForm
{
  internal class Settings
  {
    public Settings.DrawingSettings Drawing;
    private Dictionary<string, string> SettingsValues;

    public Settings()
    {
      this.SettingsValues = new Dictionary<string, string>();
    }

    public void Load()
    {
      foreach (string allKey in ConfigurationManager.AppSettings.AllKeys)
        this.SettingsValues[allKey] = ConfigurationManager.AppSettings[allKey];
      this.Drawing.Load();
      Config.Load();
    }

    public void Save()
    {
      System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
      configuration.AppSettings.Settings.Clear();
      foreach (KeyValuePair<string, string> settingsValue in this.SettingsValues)
        configuration.AppSettings.Settings.Add(settingsValue.Key, settingsValue.Value);
      Config.Save();
      configuration.Save(ConfigurationSaveMode.Full);
    }

    public string Get(string sName)
    {
      string empty = string.Empty;
      if (!this.SettingsValues.TryGetValue(sName, out empty))
        throw new Exception("Setting \"" + sName + "\" not found");
      return empty;
    }

    public void Set(string sName, string sValue)
    {
      this.SettingsValues[sName] = sValue;
    }

    public struct DrawingSettings
    {
      public int FieldNameWidth;
      public int FieldWidth;
      public int ControlXSpacing;
      public int ControlYSpacing;
      public int ControlHeight;
      public int MultilineControlHeight;

      public void Load()
      {
        this.FieldNameWidth = int.Parse(Program.Settings.Get("FieldNameWidth"));
        this.FieldWidth = int.Parse(Program.Settings.Get("FieldWidth"));
        this.ControlXSpacing = int.Parse(Program.Settings.Get("ControlXSpacing"));
        this.ControlYSpacing = int.Parse(Program.Settings.Get("ControlYSpacing"));
        this.ControlHeight = int.Parse(Program.Settings.Get("ControlHeight"));
        this.MultilineControlHeight = int.Parse(Program.Settings.Get("MultilineControlHeight"));
      }
    }
  }
}
