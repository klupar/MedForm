// Decompiled with JetBrains decompiler
// Type: MedForm.Properties.Settings
// Assembly: MedForm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6D9E08C4-6498-4744-B3EB-7157B0D783E0
// Assembly location: C:\Users\Massaraksher\Desktop\Medform_\MedForm.exe

using System.CodeDom.Compiler;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace MedForm.Properties
{
  [CompilerGenerated]
  [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
  internal sealed class Settings : ApplicationSettingsBase
  {
    private static Settings defaultInstance = (Settings) SettingsBase.Synchronized((SettingsBase) new Settings());

    public static Settings Default
    {
      get
      {
        Settings defaultInstance = Settings.defaultInstance;
        Settings settings = defaultInstance;
        return settings;
      }
    }
  }
}
