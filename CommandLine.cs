using System;
using System.Collections.Generic;
using System.Linq;

namespace MedForm
{
  internal class CommandLine
  {
    private Dictionary<string, CommandLine.Option> Options;
    private Dictionary<string, string> OptionValues;

    public CommandLine(bool bIgnoreCase)
    {
      this.Options = new Dictionary<string, CommandLine.Option>(bIgnoreCase ? (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase : (IEqualityComparer<string>) StringComparer.Ordinal);
      this.OptionValues = new Dictionary<string, string>();
    }

    public void AddOption(CommandLine.Option opt)
    {
      if (this.Options.ContainsKey(opt.ShortName))
        throw new Exception("Duplicate option \"" + opt.ShortName + "\"");
      this.Options[opt.ShortName] = opt;
    }

    public void Parse()
    {
      string[] commandLineArgs = Environment.GetCommandLineArgs();
      for (int index = 1; index < ((IEnumerable<string>) commandLineArgs).Count<string>(); index += 2)
      {
        string key = commandLineArgs[index];
        string str = string.Empty;
        bool flag = false;
        int length = key.IndexOf('=');
        if (length > 0)
        {
          str = key.Substring(length + 1);
          key = key.Substring(0, length);
          flag = true;
        }
        CommandLine.Option option;
        if (!this.Options.TryGetValue(key, out option))
          throw new Exception("Invalid option \"" + key + "\"");
        string empty = string.Empty;
        if (this.OptionValues.ContainsKey(option.Name))
          throw new Exception("Duplicate option \"" + key + "\"");
        string s;
        if (option.Type != CommandLine.OptionType.Bool)
        {
          if (flag)
          {
            s = str;
            --index;
          }
          else
          {
            if (index + 1 >= ((IEnumerable<string>) commandLineArgs).Count<string>())
              throw new Exception("Option \"" + key + "\" must have value");
            s = commandLineArgs[index + 1];
          }
          if (option.Type == CommandLine.OptionType.Int)
          {
            int result = 0;
            if (!int.TryParse(s, out result))
              throw new Exception("Invalid option value \"" + s + "\"");
          }
        }
        else
        {
          s = bool.TrueString;
          --index;
        }
        this.OptionValues[option.Name] = s;
      }
      this.CheckForMandatoryOptions();
    }

    private void CheckForMandatoryOptions()
    {
      foreach (CommandLine.Option option in this.Options.Values)
      {
        if (option.Mandatory)
        {
          if (option.Type != CommandLine.OptionType.Bool && !this.OptionValues.ContainsKey(option.Name))
            throw new Exception("No mandatory option \"" + option.ShortName + "\"");
        }
        else if (!this.OptionValues.ContainsKey(option.Name))
          this.OptionValues[option.Name] = option.Type == CommandLine.OptionType.Bool ? bool.FalseString : option.DefVal;
      }
    }

    public string GetOpt(string sOptName)
    {
      string str;
      if (this.OptionValues.TryGetValue(sOptName, out str))
        return str;
      throw new Exception("Option \"" + sOptName + "\" not found");
    }

    public enum OptionType
    {
      Int = 1,
      String = 2,
      Bool = 3,
    }

    public struct Option
    {
      public string Name;
      public string ShortName;
      public CommandLine.OptionType Type;
      public bool Mandatory;
      public string DefVal;
    }
  }
}
