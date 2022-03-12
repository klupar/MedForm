using System;
using System.Collections.Generic;
using System.Drawing;

namespace TeethCard
{
  internal class Utils
  {
    public static T[] InitializeArray<T>(int length) where T : new()
    {
      T[] objArray = new T[length];
      for (int index = 0; index < length; ++index)
        objArray[index] = new T();
      return objArray;
    }

    public static bool CircleContains(Rectangle rect, Point p)
    {
      Point rect1 = Utils.NormalizePointCoordsToRect(rect, p);
      return Utils.CircleContains(rect.Width / 2, rect1);
    }

    public static bool CircleContains(int radius, Point p)
    {
      return p.X * p.X + p.Y * p.Y <= radius * radius;
    }

    public static Point NormalizePointCoordsToRect(Rectangle rect, Point p)
    {
      p.X -= (rect.Right + rect.Left) / 2;
      p.Y -= (rect.Top + rect.Bottom) / 2;
      return p;
    }

    public static Color GetColorById(int id)
    {
      Color black = Color.Black;
      Dictionaries.DictColors.TryGetValue(id, out black);
      return black;
    }

    public static Color StringToColor(string sColor)
    {
      return Color.FromArgb(Convert.ToInt32(sColor.Substring(1, 2), 16), Convert.ToInt32(sColor.Substring(3, 2), 16), Convert.ToInt32(sColor.Substring(5, 2), 16));
    }

    public static bool IsMilkTooth(int id)
    {
      return id > 48;
    }

    public static int GetMilkToothId(int id)
    {
      return id + 40;
    }

    public static int GetNonMilkToothId(int id)
    {
      return id - 40;
    }

    public static List<int> ParseIntCSV(string str)
    {
      List<int> intList = new List<int>();
      if (str != string.Empty)
      {
        string str1 = str;
        char[] chArray = new char[1]{ ',' };
        foreach (string s in str1.Split(chArray))
          intList.Add(int.Parse(s));
      }
      return intList;
    }

    public static HashSet<int> ParseIntCSVAsSet(string str)
    {
      HashSet<int> intSet = new HashSet<int>();
      if (str != string.Empty)
      {
        string str1 = str;
        char[] chArray = new char[1]{ ',' };
        foreach (string s in str1.Split(chArray))
          intSet.Add(int.Parse(s));
      }
      return intSet;
    }

    public static string HashSetToString(HashSet<int> set)
    {
      string empty = string.Empty;
      foreach (int num in set)
        empty += empty != string.Empty ? "," + num.ToString() : num.ToString();
      return empty;
    }

    public static bool DrawImageIfAny(
      Dictionaries.ImageTypes type,
      int id,
      Graphics g,
      Rectangle drawRect,
      bool flipVert = false)
    {
      Image image;
      if ((image = Dictionaries.GetImage(type, id)) == null)
        return false;
      g.DrawImage(image, drawRect.X, drawRect.Y + (flipVert ? drawRect.Height : 0), drawRect.Width, drawRect.Height * (flipVert ? -1 : 1));
      return true;
    }
  }
}
