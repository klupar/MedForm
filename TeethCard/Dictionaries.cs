using MedForm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace TeethCard
{
  internal class Dictionaries
  {
    public static Dictionary<int, Dictionaries.DictToothRecord> DictTooth = new Dictionary<int, Dictionaries.DictToothRecord>();
    public static Dictionary<int, Dictionaries.DictToothZoneRecord> DictToothZone = new Dictionary<int, Dictionaries.DictToothZoneRecord>();
    public static Dictionary<int, Dictionaries.DictToothRootRecord> DictToothRoot = new Dictionary<int, Dictionaries.DictToothRootRecord>();
    public static Dictionary<int, Dictionaries.DictToothStatusRecord> DictToothStatus = new Dictionary<int, Dictionaries.DictToothStatusRecord>();
    public static Dictionary<int, Dictionaries.DictToothZoneStatusRecord> DictToothZoneStatus = new Dictionary<int, Dictionaries.DictToothZoneStatusRecord>();
    public static Dictionary<int, Dictionaries.DictToothRootStatusRecord> DictToothRootStatus = new Dictionary<int, Dictionaries.DictToothRootStatusRecord>();
    public static Dictionary<int, Color> DictColors = new Dictionary<int, Color>();
    public static Dictionary<KeyValuePair<Dictionaries.ImageTypes, int>, Image> DictImages = new Dictionary<KeyValuePair<Dictionaries.ImageTypes, int>, Image>();
    public static Dictionary<int, Dictionaries.DictToothDiagnosisRecord> DictToothDiagnosis;
    public static Dictionary<int, Dictionaries.DictToothStatusRecordV2> DictToothStatusV2;
    public static Dictionary<int, Dictionaries.DictToothRootStatusRecordV2> DictToothRootStatusV2;

    static Dictionaries()
    {
      if (Program.Version != Program.VersionType.NewProVersion)
        return;
      Dictionaries.DictToothDiagnosis = new Dictionary<int, Dictionaries.DictToothDiagnosisRecord>();
      Dictionaries.DictToothStatusV2 = new Dictionary<int, Dictionaries.DictToothStatusRecordV2>();
      Dictionaries.DictToothRootStatusV2 = new Dictionary<int, Dictionaries.DictToothRootStatusRecordV2>();
    }

    public static void Load()
    {
      Dictionaries.LoadDictTooth();
      Dictionaries.LoadDictToothZone();
      Dictionaries.LoadDictToothRoot();
      Dictionaries.LoadDictToothStatus();
      Dictionaries.LoadDictToothZoneStatus();
      Dictionaries.LoadDictToothRootStatus();
      Dictionaries.LoadDictColors();
      if (Program.Version != Program.VersionType.NewProVersion)
        return;
      Dictionaries.LoadDictToothDiagnosis();
      Dictionaries.LoadDictToothStatusV2();
      Dictionaries.LoadDictToothRootStatusV2();
    }

    public static void LoadDictTooth()
    {
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.SelectProc("TEETH_GET_DICT_TOOTH").Tables[0].Rows)
      {
        Dictionaries.DictToothRecord dictToothRecord = new Dictionaries.DictToothRecord();
        int key = (int) row["id"];
        dictToothRecord.OrderNumber = (int) row["ordernumber"];
        dictToothRecord.Name = (string) row["name"];
        Dictionaries.DictTooth.Add(key, dictToothRecord);
      }
    }

    public static void LoadDictToothDiagnosis()
    {
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.SelectProc("TEETH_GET_TOOTH_DIAGNOSIS").Tables[0].Rows)
      {
        Dictionaries.DictToothDiagnosisRecord toothDiagnosisRecord = new Dictionaries.DictToothDiagnosisRecord();
        int key = (int) row["id"];
        toothDiagnosisRecord.ShortName = (string) row["shortname"];
        toothDiagnosisRecord.Name = (string) row["name"];
        Dictionaries.DictToothDiagnosis.Add(key, toothDiagnosisRecord);
      }
    }

    public static void LoadDictToothZone()
    {
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.SelectProc("TEETH_GET_DICT_TOOTHZONE").Tables[0].Rows)
      {
        Dictionaries.DictToothZoneRecord dictToothZoneRecord = new Dictionaries.DictToothZoneRecord();
        int key = (int) row["id"];
        dictToothZoneRecord.ShortName = (string) row["shortname"];
        dictToothZoneRecord.Name = (string) row["name"];
        Dictionaries.DictToothZone.Add(key, dictToothZoneRecord);
      }
    }

    public static void LoadDictToothRoot()
    {
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.SelectProc("TEETH_GET_DICT_TOOTHROOT").Tables[0].Rows)
      {
        Dictionaries.DictToothRootRecord dictToothRootRecord = new Dictionaries.DictToothRootRecord();
        int key = (int) row["id"];
        dictToothRootRecord.ShortName = (string) row["shortname"];
        dictToothRootRecord.Name = (string) row["name"];
        Dictionaries.DictToothRoot.Add(key, dictToothRootRecord);
      }
    }

    public static void LoadDictToothStatus()
    {
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.SelectProc("TEETH_GET_TOOTH_STATUSES").Tables[0].Rows)
      {
        Dictionaries.DictToothStatusRecord toothStatusRecord = new Dictionaries.DictToothStatusRecord();
        int num = (int) row["id"];
        toothStatusRecord.ShortName = (string) row["shortname"];
        toothStatusRecord.Name = (string) row["name"];
        toothStatusRecord.ColorId = (int) row["colorid"];
        toothStatusRecord.ColorName = (string) row["colorname"];
        toothStatusRecord.ImageFileName = DBUtil.GetStringValue(row, "imagefilename");
        Dictionaries.LoadImage(Dictionaries.ImageTypes.ToothStatus, num, toothStatusRecord.ImageFileName);
        Dictionaries.DictToothStatus.Add(num, toothStatusRecord);
      }
    }

    public static void LoadDictToothStatusV2()
    {
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.SelectProc("TEETH_GET_TOOTH_STATUSES2").Tables[0].Rows)
      {
        Dictionaries.DictToothStatusRecordV2 toothStatusRecordV2 = new Dictionaries.DictToothStatusRecordV2();
        int num = (int) row["id"];
        toothStatusRecordV2.ShortName = (string) row["shortname"];
        toothStatusRecordV2.Name = (string) row["name"];
        toothStatusRecordV2.ColorId = (int) row["colorid"];
        toothStatusRecordV2.ColorName = (string) row["colorname"];
        toothStatusRecordV2.ImageFileName = DBUtil.GetStringValue(row, "imagefilename");
        toothStatusRecordV2.IsInvert = (int) row["isinvert"] == 1;
        toothStatusRecordV2.OrderNumber = (int) row["ordernumber"];
        Dictionaries.LoadImage(Dictionaries.ImageTypes.ToothStatusV2, num, toothStatusRecordV2.ImageFileName);
        Dictionaries.DictToothStatusV2.Add(num, toothStatusRecordV2);
      }
    }

    public static void LoadDictToothZoneStatus()
    {
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.SelectProc("TEETH_GET_TOOTHZONE_STATUSES").Tables[0].Rows)
      {
        Dictionaries.DictToothZoneStatusRecord zoneStatusRecord = new Dictionaries.DictToothZoneStatusRecord();
        int key = (int) row["id"];
        zoneStatusRecord.ShortName = (string) row["shortname"];
        zoneStatusRecord.Name = (string) row["name"];
        zoneStatusRecord.ColorId = (int) row["colorid"];
        zoneStatusRecord.ColorName = (string) row["colorname"];
        Dictionaries.DictToothZoneStatus.Add(key, zoneStatusRecord);
      }
    }

    public static void LoadDictToothRootStatus()
    {
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.SelectProc("TEETH_GET_TOOTHROOT_STATUSES").Tables[0].Rows)
      {
        Dictionaries.DictToothRootStatusRecord rootStatusRecord = new Dictionaries.DictToothRootStatusRecord();
        int num = (int) row["id"];
        rootStatusRecord.ShortName = (string) row["shortname"];
        rootStatusRecord.Name = (string) row["name"];
        rootStatusRecord.ImageFileName = DBUtil.GetStringValue(row, "imagefilename");
        Dictionaries.LoadImage(Dictionaries.ImageTypes.RootStatus, num, rootStatusRecord.ImageFileName);
        Dictionaries.DictToothRootStatus.Add(num, rootStatusRecord);
      }
    }

    public static void LoadDictToothRootStatusV2()
    {
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.SelectProc("TEETH_GET_TOOTHROOT_STATUSES2").Tables[0].Rows)
      {
        Dictionaries.DictToothRootStatusRecordV2 rootStatusRecordV2 = new Dictionaries.DictToothRootStatusRecordV2();
        int num = (int) row["id"];
        rootStatusRecordV2.ShortName = (string) row["shortname"];
        rootStatusRecordV2.Name = (string) row["name"];
        rootStatusRecordV2.OrderNumber = (int) row["ordernumber"];
        rootStatusRecordV2.ImageFileName = DBUtil.GetStringValue(row, "imagefilename");
        Dictionaries.LoadImage(Dictionaries.ImageTypes.RootStatusV2, num, rootStatusRecordV2.ImageFileName);
        Dictionaries.DictToothRootStatusV2.Add(num, rootStatusRecordV2);
      }
    }

    public static void LoadDictColors()
    {
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.Select("SELECT ID, RGB FROM COLOR").Tables[0].Rows)
      {
        int key = (int) row["ID"];
        Dictionaries.DictColors.Add(key, Utils.StringToColor((string) row["RGB"]));
      }
    }

    public static void LoadImage(Dictionaries.ImageTypes imageType, int id, string FileName)
    {
      if (FileName == null || FileName == "")
        return;
      Bitmap bitmap = (Bitmap) null;
      try
      {
        bitmap = (Bitmap) Image.FromFile(Config.ImagePath + FileName, true);
      }
      catch (Exception)
      {
      }
      Dictionaries.DictImages.Add(new KeyValuePair<Dictionaries.ImageTypes, int>(imageType, id), (Image) bitmap);
    }

    public static Image GetImage(Dictionaries.ImageTypes type, int id)
    {
      Image image = (Image) null;
      Dictionaries.DictImages.TryGetValue(new KeyValuePair<Dictionaries.ImageTypes, int>(type, id), out image);
      return image;
    }

    public struct DictToothRecord
    {
      public int OrderNumber;
      public string Name;
    }

    public struct DictToothZoneRecord
    {
      public string ShortName;
      public string Name;
    }

    public struct DictToothRootRecord
    {
      public string ShortName;
      public string Name;
    }

    public struct DictToothStatusRecord
    {
      public string ShortName;
      public string Name;
      public int ColorId;
      public string ColorName;
      public string ImageFileName;
    }

    public struct DictToothStatusRecordV2
    {
      public string ShortName;
      public string Name;
      public int ColorId;
      public string ColorName;
      public string ImageFileName;
      public bool IsInvert;
      public int OrderNumber;
    }

    public struct DictToothZoneStatusRecord
    {
      public string ShortName;
      public string Name;
      public int ColorId;
      public string ColorName;
    }

    public struct DictToothRootStatusRecord
    {
      public string ShortName;
      public string Name;
      public string ImageFileName;
    }

    public struct DictToothRootStatusRecordV2
    {
      public string ShortName;
      public string Name;
      public string ImageFileName;
      public int OrderNumber;
    }

    public struct DictToothDiagnosisRecord
    {
      public string ShortName;
      public string Name;
    }

    public enum ImageTypes
    {
      ToothStatus,
      RootStatus,
      ToothStatusV2,
      RootStatusV2,
    }
  }
}
