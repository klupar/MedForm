using System;
using System.Collections.Generic;
using System.Data;

namespace MedForm
{
  internal class FieldOps
  {
    public List<FieldOps.FieldInfo> Fields;

    public void LoadFields()
    {
      this.Fields = new List<FieldOps.FieldInfo>();
      DataSet dataSet = DBUtil.SelectProc("MF_GET_FIELD_LIST", (object) DBUtil.TableName);
      DataSet dataSet_Temp = DBUtil.Select2Fields("MF_GET_FIELD_TEMPLATE", (object) DBUtil.TableName, (object)DBUtil.RecordID);
            for (int index = 0; index < dataSet.Tables[0].Rows.Count; ++index)
      {
        DataRow row = dataSet.Tables[0].Rows[index];
        FieldOps.FieldInfo fieldInfo = new FieldOps.FieldInfo();
                fieldInfo.FromRow(row);

                foreach (DataRow temp_row in dataSet_Temp.Tables[0].Rows)
                {
                    if (row.ItemArray[0].ToString() == temp_row.ItemArray[0].ToString())
                    {
                        string temp_val = temp_row.ItemArray[1].ToString();
                        

                        fieldInfo.Value = temp_val.TrimEnd(' ');
                        break;
                        /*
                        if (fieldInfo.Type == FieldType.ForeignKey)
                        {
                            fieldInfo.Value = fieldInfo.FkeyValues[int.Parse(temp_row.ItemArray[1].ToString())];
                            break;
                        }
                        else
                        {
                            fieldInfo.Value = temp_row.ItemArray[1];
                            break;
                        }*/
                    }
                }
                
                this.Fields.Add(fieldInfo);
            }
    }

    public void SaveFields()
    {
      foreach (FieldOps.FieldInfo field in this.Fields)
      {
        if (field.IsModified)
        {
          field.SetValue();
          field.IsModified = false;
        }
      }
    }

    public bool IsModified()
    {
      foreach (FieldOps.FieldInfo field in this.Fields)
      {
        if (field.IsModified)
          return true;
      }
      return false;
    }

    public enum FieldType
    {
      MultiText = 1,
      SingleText = 2,
      Number = 3,
      Decimal = 4,
      Date = 5,
      DateTime = 6,
      Checkbox = 7,
      ForeignKey = 8,
      FileLink = 9,
    }

    public class FieldInfo
    {
      public FieldOps.FieldType Type;
      public string Name;
      public object Value;
      public Decimal MinValue;
      public Decimal MaxValue;
      public string Caption;
      public bool IsReadOnly;
      public bool IsHeader;
      public bool IsModified;
      public Dictionary<string, List<string>> PhraseList;
      public Dictionary<int, string> FkeyValues;

      public void FromRow(DataRow row)
      {
        this.Name = row["FieldName"].ToString();
        this.Type = (FieldOps.FieldType) Enum.Parse(typeof (FieldOps.FieldType), row["MFFIELDTYPEID"].ToString());
        this.Caption = row["Caption"].ToString();
        this.IsReadOnly = int.Parse(row["IsReadOnly"].ToString()) == 1;
        this.IsHeader = int.Parse(row["IsHeader"].ToString()) == 1;
        this.PhraseList = (Dictionary<string, List<string>>) null;
        this.FkeyValues = (Dictionary<int, string>) null;
                
        this.GetValue(row);
        
        if (this.Type == FieldOps.FieldType.MultiText)
          this.GetPhraseList();
        this.IsModified = false;
      }

      private void GetValue(DataRow row)
      {
        switch (this.Type)
        {
          case FieldOps.FieldType.MultiText:
          case FieldOps.FieldType.SingleText:
          case FieldOps.FieldType.FileLink:
            this.Value = (object) DBUtil.GetFieldValueStr(this.Name);
            break;
          case FieldOps.FieldType.Number:
          case FieldOps.FieldType.Checkbox:
            this.Value = (object) DBUtil.GetFieldValueInt(this.Name, out this.MinValue, out this.MaxValue);
            break;
          case FieldOps.FieldType.Decimal:
            this.Value = (object) DBUtil.GetFieldValueDecimal(this.Name, out this.MinValue, out this.MaxValue);
            break;
          case FieldOps.FieldType.Date:
            this.Value = (object) DBUtil.GetFieldValueDate(this.Name);
            break;
          case FieldOps.FieldType.DateTime:
            this.Value = (object) DBUtil.GetFieldValueDateTime(this.Name);
            break;
          case FieldOps.FieldType.ForeignKey:
            this.Value = (object) DBUtil.GetFieldValueFkey(this.Name);
            this.FkeyValues = DBUtil.GetFieldValueFkeyDict(this.Name);
            break;
        }
      }

      private void GetPhraseList()
      {
        this.PhraseList = new Dictionary<string, List<string>>();
        DataSet dataSet = DBUtil.SelectProc("MF_GET_PHRASE_LIST", (object) DBUtil.TableName, (object) this.Name, (object) DBUtil.RecordID);
        for (int index = 0; index < dataSet.Tables[0].Rows.Count; ++index)
        {
          DataRow row = dataSet.Tables[0].Rows[index];
          string key = row["PhraseGroup"].ToString();
          string str = row["PhraseText"].ToString();
          if (!this.PhraseList.ContainsKey(key))
            this.PhraseList.Add(key, new List<string>());
          this.PhraseList[key].Add(str);
        }
      }

      public void SetValue()
      {
        switch (this.Type)
        {
          case FieldOps.FieldType.MultiText:
          case FieldOps.FieldType.SingleText:
          case FieldOps.FieldType.FileLink:
            DBUtil.SetFieldValueStr(this.Name, this.Value.ToString());
            break;
          case FieldOps.FieldType.Number:
          case FieldOps.FieldType.Checkbox:
            DBUtil.SetFieldValueInt(this.Name, int.Parse(this.Value.ToString()));
            break;
          case FieldOps.FieldType.Decimal:
            DBUtil.SetFieldValueDecimal(this.Name, (Decimal) this.Value);
            break;
          case FieldOps.FieldType.Date:
            DBUtil.SetFieldValueDate(this.Name, (DateTime) this.Value);
            break;
          case FieldOps.FieldType.DateTime:
            DBUtil.SetFieldValueDateTime(this.Name, (DateTime) this.Value);
            break;
          case FieldOps.FieldType.ForeignKey:
            DBUtil.SetFieldValueFkey(this.Name, (int) this.Value);
            break;
        }
      }
    }
  }
}
