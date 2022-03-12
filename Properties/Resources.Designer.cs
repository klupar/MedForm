// Decompiled with JetBrains decompiler
// Type: MedForm.Properties.Resources
// Assembly: MedForm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6D9E08C4-6498-4744-B3EB-7157B0D783E0
// Assembly location: C:\Users\Massaraksher\Desktop\Medform_\MedForm.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace MedForm.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (MedForm.Properties.Resources.resourceMan == null)
          MedForm.Properties.Resources.resourceMan = new ResourceManager("MedForm.Properties.Resources", typeof (MedForm.Properties.Resources).Assembly);
        return MedForm.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get
      {
        return MedForm.Properties.Resources.resourceCulture;
      }
      set
      {
        MedForm.Properties.Resources.resourceCulture = value;
      }
    }
  }
}
