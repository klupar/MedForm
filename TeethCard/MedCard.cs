using MedForm;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace TeethCard
{
  internal class MedCard
  {
    private int MedCardRecordID;
    //private int Depth;
    //private int Width;
    //private int Penetration;
    private Dictionary<int, Tooth> Teeth;
    public const int NUM_TEETH = 32;
    public const int NUM_ROWS = 2;
    public const int TEETH_PER_ROW = 16;

    public MedCard()
    {
      this.Teeth = new Dictionary<int, Tooth>();
      foreach (KeyValuePair<int, Dictionaries.DictToothRecord> keyValuePair in Dictionaries.DictTooth)
      {
        Tooth tooth = new Tooth(keyValuePair.Key, keyValuePair.Value);
        this.Teeth.Add(keyValuePair.Key, tooth);
      }
    }

    public void Load(int _MedCardRecordID)
    {
      this.MedCardRecordID = _MedCardRecordID;
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.SelectProc("TEETH_GET_TEETHMAP", (object) DBUtil.RecordID, (object) Program.CommandLine.GetOpt("TableID")).Tables[0].Rows)
      {
        int num1 = (int) row["id"];
        int id = (int) row["DICTTOOTHID"];
        int num2 = (int) row["DICTTOOTHSTATUSID"];
        this.Teeth[id].Status = num2;
        this.Teeth[id].DBId = num1;
        if (Utils.IsMilkTooth(id))
        {
          this.Teeth[id].IsActive = true;
          this.Teeth[id].InitialActive = true;
        }
      }
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.SelectProc("TEETH_GET_TEETHZONEMAP", (object) DBUtil.RecordID, (object) Program.CommandLine.GetOpt("TableID")).Tables[0].Rows)
      {
        int num = (int) row["id"];
        this.Teeth[(int) row["DICTTOOTHID"]].Zones[(int) row["DICTTOOTHZONEID"]].Status = (int) row["DICTTOOTHZONESTATUSID"];
      }
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.SelectProc("TEETH_GET_TEETHROOTMAP", (object) DBUtil.RecordID, (object) Program.CommandLine.GetOpt("TableID")).Tables[0].Rows)
      {
        int num1 = (int) row["id"];
        int index1 = (int) row["DICTTOOTHID"];
        int index2 = (int) row["DICTTOOTHROOTID"];
        int num2 = (int) row["DICTTOOTHROOTSTATUSID"];
        this.Teeth[index1].Roots[index2].Status = num2;
        this.Teeth[index1].Roots[index2].Active = true;
      }
      if (Program.Version != Program.VersionType.NewProVersion)
        return;
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.SelectProc("TEETH_GET_TEETHMAPDIAGNOSIS", (object) DBUtil.RecordID, (object) Program.CommandLine.GetOpt("TableID")).Tables[0].Rows)
      {
        int num = (int) row["id"];
        this.Teeth[(int) row["DICTTOOTHID"]].AddDiagnosis((int) row["DICTTOOTHDIAGNOSISID"]);
      }
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.SelectProc("TEETH_GET_TEETHMAPSTATUS", (object) DBUtil.RecordID, (object) Program.CommandLine.GetOpt("TableID")).Tables[0].Rows)
      {
        int num = (int) row["id"];
        this.Teeth[(int) row["DICTTOOTHID"]].AddStatusV2((int) row["DICTTOOTHSTATUS2ID"]);
      }
      foreach (DataRow row in (InternalDataCollectionBase) DBUtil.SelectProc("TEETH_GET_TEETHROOTMAPSTATUS", (object) DBUtil.RecordID, (object) Program.CommandLine.GetOpt("TableID")).Tables[0].Rows)
      {
        int num = (int) row["id"];
        int index1 = (int) row["DICTTOOTHID"];
        int index2 = (int) row["DICTTOOTHROOTID"];
        int id = (int) row["DICTTOOTHROOTSTATUS2ID"];
        this.Teeth[index1].Roots[index2].AddStatusV2(id);
        this.Teeth[index1].Roots[index2].Active = true;
      }
    }

    public void Save()
    {
      foreach (KeyValuePair<int, Tooth> tooth in this.Teeth)
        tooth.Value.SaveModified(tooth.Value.Id);
    }

    public bool IsModified()
    {
      foreach (Tooth tooth in this.Teeth.Values)
      {
        if (tooth.IsModified())
          return true;
      }
      return false;
    }

    public void Draw(Graphics g)
    {
      foreach (Tooth tooth in this.Teeth.Values)
        tooth.DrawSmall(g);
    }

    public Tooth FindTooth(Point p)
    {
      foreach (Tooth tooth in this.Teeth.Values)
      {
        if (tooth.Contains(p))
          return tooth;
      }
      return (Tooth) null;
    }

    public bool HasTooth(int id)
    {
      return this.Teeth.ContainsKey(id);
    }

    public bool HasMilkTooth(int id)
    {
      Tooth tooth;
      return this.Teeth.TryGetValue(id, out tooth) && tooth.IsActive;
    }

    public Tooth GetTooth(int id)
    {
      return this.Teeth[id];
    }

    public Tooth ToggleMilk(int id)
    {
      this.Teeth[id].ToggleMilk();
      Tooth tooth = this.GetTooth(Utils.IsMilkTooth(id) ? Utils.GetNonMilkToothId(id) : Utils.GetMilkToothId(id));
      tooth.ToggleMilk();
      return tooth;
    }

    public Tooth SetMilk(int id, bool milk)
    {
      this.Teeth[id].SetMilk(milk);
      Tooth tooth = this.GetTooth(Utils.IsMilkTooth(id) ? Utils.GetNonMilkToothId(id) : Utils.GetMilkToothId(id));
      tooth.SetMilk(milk);
      return tooth;
    }
  }
}
