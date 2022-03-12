using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MedForm
{
  internal static class DBUtil
  {
    private static FbConnection DBConn;
    private static string ConnectionString;
    public static string TableName;
    public static int RecordID;
    public static string InitialTableName;
    public static int InitialRecordID;

    public static void Connect(string sConnString)
    {
      DBUtil.Disconnect();
      DBUtil.DBConn = new FbConnection(sConnString);
      try
      {
        DBUtil.DBConn.Open();
      }
      catch (Exception ex)
      {
        throw new Exception("Невозможно подключиться к БД:\n" + ex.Message);
      }
      DBUtil.ConnectionString = sConnString;
    }

    public static void Disconnect()
    {
      if (DBUtil.DBConn == null)
        return;
      DBUtil.DBConn.Close();
    }

    public static void ExecProc(string sName, params object[] list)
    {
      string str = "execute procedure " + sName + "(" + DBUtil.ParamsToString(list) + ")";
      SystemLog.LogDBCommand(str);
      new FbCommand(str, DBUtil.DBConn).ExecuteNonQuery();
    }

    public static DataSet Select(string sSQL)
    {
      SystemLog.LogDBCommand(sSQL);
      DataSet dataSet = new DataSet();
      new FbDataAdapter(sSQL, DBUtil.DBConn).Fill(dataSet);
      return dataSet;
    }

    public static DataSet SelectProc(string sName, params object[] list)
    {
      string sSQL;
      if ((uint) ((IEnumerable<object>) list).Count<object>() > 0U)
        sSQL = "select * from " + sName + "(" + DBUtil.ParamsToString(list) + ")";
      else
        sSQL = "select * from " + sName;
      return DBUtil.Select(sSQL);
    }

        public static DataSet Select2Fields(string sName, params object[] list)
        {
            string sSQL = "SELECT fieldname, fieldvalue FROM " + sName + "(" + DBUtil.ParamsToString(list) + ")";
            return DBUtil.Select(sSQL);
        }

        public static string GetStringValue(DataRow row, string sFieldName)
    {
      return Convert.IsDBNull(row[sFieldName]) ? "" : Convert.ToString(row[sFieldName]);
    }

    public static string GetProcResult(DataSet ds, string sFieldName)
    {
      return ds.Tables[0].Rows.Count > 0 ? DBUtil.GetStringValue(ds.Tables[0].Rows[0], sFieldName) : "";
    }

    private static string ParamsToString(params object[] list)
    {
      string empty = string.Empty;
      foreach (object obj in list)
      {
        if (empty != string.Empty)
          empty += ",";
        bool flag = !(obj is int) && !(obj is float);
        if (flag)
          empty += "'";
        empty += obj.ToString();
        if (flag)
          empty += "'";
      }
      return empty;
    }

    public static void ImpersonateUser()
    {
            /*
            DataSet ds = DBUtil.SelectProc("MF_GET_USER_DETAILS", (object) Program.CommandLine.GetOpt("UserID"));
            string procResult1 = DBUtil.GetProcResult(ds, "Login");
            string procResult2 = DBUtil.GetProcResult(ds, "Password");
            DBUtil.Connect(DBUtil.ConnectionString.Replace("SYSDBA", procResult1).Replace("masterkey", procResult2));
            */
        DBUtil.Connect(DBUtil.ConnectionString);
    }

    public static void GetRecordID()
    {
      string opt = Program.CommandLine.GetOpt("UserID");
      string s = Program.CommandLine.GetOpt("RecordID");
      if (s == "0")
        s = DBUtil.GetProcResult(DBUtil.SelectProc("MF_GET_RECORDID", (object) DBUtil.TableName, (object) opt), "RecordID");
      DBUtil.RecordID = int.Parse(s);
    }

    public static void GetTableName()
    {
      DBUtil.TableName = DBUtil.GetProcResult(DBUtil.SelectProc("MF_GET_TABLENAME", (object) Program.CommandLine.GetOpt("TableID")), "tablename");
    }

    public static string GetFieldValueStr(string FieldName)
    {
      return DBUtil.GetProcResult(DBUtil.SelectProc("MF_GET_FIELD_VALUE_STR", (object) DBUtil.TableName, (object) FieldName, (object) DBUtil.RecordID), "StrValue");
    }

    public static int GetFieldValueInt(string FieldName, out Decimal min, out Decimal max)
    {
      DataSet ds = DBUtil.SelectProc("MF_GET_FIELD_VALUE_INT", (object) DBUtil.TableName, (object) FieldName, (object) DBUtil.RecordID);
      min = new Decimal(int.MinValue);
      max = new Decimal(int.MaxValue);
      string procResult = DBUtil.GetProcResult(ds, "IntValue");
      return int.Parse(procResult == "" ? 0.ToString() : procResult);
    }

    public static Decimal GetFieldValueDecimal(
      string FieldName,
      out Decimal min,
      out Decimal max)
    {
      DataSet ds = DBUtil.SelectProc("MF_GET_FIELD_VALUE_DECIMAL", (object) DBUtil.TableName, (object) FieldName, (object) DBUtil.RecordID);
      min = new Decimal(-10000);
      max = new Decimal(10000);
      string procResult = DBUtil.GetProcResult(ds, "DecimalValue");
      return Decimal.Parse(procResult == "" ? Decimal.Zero.ToString() : procResult);
    }

    public static DateTime GetFieldValueDate(string FieldName)
    {
      return DateTime.Parse(DBUtil.GetProcResult(DBUtil.SelectProc("MF_GET_FIELD_VALUE_DATE", (object) DBUtil.TableName, (object) FieldName, (object) DBUtil.RecordID), "DateValue"));
    }

    public static DateTime GetFieldValueDateTime(string FieldName)
    {
      string procResult = DBUtil.GetProcResult(DBUtil.SelectProc("MF_GET_FIELD_VALUE_DATETIME", (object) DBUtil.TableName, (object) FieldName, (object) DBUtil.RecordID), "DateTimeValue");
      return procResult == "" ? DateTime.Parse("01.01.2001") : DateTime.Parse(procResult);
    }

    public static void SetFieldValueStr(string FieldName, string Value)
    {
      DBUtil.ExecProc("MF_SET_FIELD_VALUE_STR", (object) DBUtil.TableName, (object) FieldName, (object) DBUtil.RecordID, (object) Value);
    }

    public static void SetFieldValueInt(string FieldName, int Value)
    {
      DBUtil.ExecProc("MF_SET_FIELD_VALUE_INT", (object) DBUtil.TableName, (object) FieldName, (object) DBUtil.RecordID, (object) Value);
    }

    public static void SetFieldValueDecimal(string FieldName, Decimal Value)
    {
      string str = Value.ToString().Replace(',', '.');
      DBUtil.ExecProc("MF_SET_FIELD_VALUE_DECIMAL", (object) DBUtil.TableName, (object) FieldName, (object) DBUtil.RecordID, (object) str);
    }

    public static void SetFieldValueDate(string FieldName, DateTime Value)
    {
      string str = Value.ToString("dd.MM.yyyy");
      DBUtil.ExecProc("MF_SET_FIELD_VALUE_DATE", (object) DBUtil.TableName, (object) FieldName, (object) DBUtil.RecordID, (object) str);
    }

    public static void SetFieldValueDateTime(string FieldName, DateTime Value)
    {
      DBUtil.ExecProc("MF_SET_FIELD_VALUE_DATETIME", (object) DBUtil.TableName, (object) FieldName, (object) DBUtil.RecordID, (object) Value);
    }

    public static Dictionary<int, string> GetFieldValueFkeyDict(string FieldName)
    {
      Dictionary<int, string> dictionary = new Dictionary<int, string>();
      DataSet dataSet = DBUtil.SelectProc("MF_GET_FIELD_VALUE_FKEY_DICT", (object) DBUtil.TableName, (object) FieldName);
      for (int index1 = 0; index1 < dataSet.Tables[0].Rows.Count; ++index1)
      {
        DataRow row = dataSet.Tables[0].Rows[index1];
        int index2 = (int) row["fkid"];
        string str = (string) row["fkvalue"];
        dictionary[index2] = str;
      }
      return dictionary;
    }

    public static int GetFieldValueFkey(string FieldName)
    {
      DataSet ds = DBUtil.SelectProc("MF_GET_FIELD_VALUE_FKEY", (object) DBUtil.TableName, (object) FieldName, (object) DBUtil.RecordID);
      return ds.Tables[0].Rows.Count == 0 ? -1 : int.Parse(DBUtil.GetProcResult(ds, "fkid"));
    }

    public static void SetFieldValueFkey(string FieldName, int fkid)
    {
      DBUtil.ExecProc("MF_SET_FIELD_VALUE_FKEY", (object) DBUtil.TableName, (object) FieldName, (object) DBUtil.RecordID, (object) fkid);
    }

    public static void Initialize()
    {
      DBUtil.Connect(Program.Settings.Get("DBConnectionString"));
      DBUtil.ImpersonateUser();
      DBUtil.GetTableName();
      DBUtil.GetRecordID();
      DBUtil.InitialTableName = DBUtil.TableName;
      DBUtil.InitialRecordID = DBUtil.RecordID;
    }

    public static bool SetTableAndId(string _TableName, int _RecordID)
    {
      bool flag = _TableName != DBUtil.TableName || _RecordID != DBUtil.RecordID;
      DBUtil.TableName = _TableName;
      DBUtil.RecordID = _RecordID;
      return flag;
    }

    public static int CreateNewToothRecord(int dictToothId)
    {
      return int.Parse(DBUtil.GetProcResult(DBUtil.SelectProc("TEETHMAP_ADD", (object) DBUtil.InitialRecordID, (object) dictToothId, (object) 0), "id"));
    }
  }
}
