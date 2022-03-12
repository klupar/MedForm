using MedForm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TeethCard
{
  internal class Tooth : StatusObject
  {
    private string Diagnosis = string.Empty;
    private string StatusV2 = string.Empty;
    public int Id;
    public int DBId;
    public Dictionary<int, ToothZone> Zones;
    public Dictionary<int, ToothRoot> Roots;
    public string Name;
    private Rectangle PaintRect;
    private const int TEETH_PER_ROW = 16;
    public bool IsActive;
    public bool InitialActive;
    public const int RootSpacing = 5;

    public Tooth(int id, Dictionaries.DictToothRecord toothRec)
    {
      this.Id = id;
      this.Zones = new Dictionary<int, ToothZone>();
      this.Roots = new Dictionary<int, ToothRoot>();
      foreach (KeyValuePair<int, Dictionaries.DictToothZoneRecord> keyValuePair in Dictionaries.DictToothZone)
      {
        ToothZone toothZone = new ToothZone(keyValuePair.Value);
        this.Zones.Add(keyValuePair.Key, toothZone);
      }
      foreach (KeyValuePair<int, Dictionaries.DictToothRootRecord> keyValuePair in Dictionaries.DictToothRoot)
      {
        ToothRoot toothRoot = new ToothRoot(keyValuePair.Value);
        this.Roots.Add(keyValuePair.Key, toothRoot);
      }
      this.Name = toothRec.Name;
      int num1 = (toothRec.OrderNumber - 1) / 16;
      int num2 = (toothRec.OrderNumber - 1) % 16;
      this.PaintRect.Width = Config.PaintConfig.ToothWidth;
      this.PaintRect.Height = Config.PaintConfig.ToothHeight;
      this.PaintRect.X = num2 * (Config.PaintConfig.ToothSpacing + this.PaintRect.Width) + Config.PaintConfig.ToothSpacing;
      this.PaintRect.Y = num1 * (Config.PaintConfig.ToothSpacing + this.PaintRect.Height) + Config.PaintConfig.ToothSpacing;
      this.IsActive = false;
    }

    public void DrawSmall(Graphics g)
    {
      if (!this.DrawTooth())
        return;
      bool flag = this.IsLower();
      Brush brush1 = (Brush) new SolidBrush(Program.FormMain.BackColor);
      g.FillRectangle(brush1, this.PaintRect);
      brush1.Dispose();
      Rectangle paintRect1 = this.PaintRect;
      paintRect1.Height = Config.PaintConfig.CardTextHeight;
      paintRect1.Y += flag ? 0 : this.PaintRect.Bottom - Config.PaintConfig.CardTextHeight - 4;
      g.DrawString(this.Name, SystemFonts.DefaultFont, Brushes.Black, (RectangleF) paintRect1, Config.PaintConfig.CenterFormat);
      Color fillColor;
      string statusText;
      this.GetDrawParameters(out fillColor, out statusText);
      Rectangle paintRect2 = this.PaintRect;
      paintRect2.Height = paintRect2.Width;
      paintRect2.X += 2;
      paintRect2.Width -= 4;
      paintRect2.Y += flag ? paintRect1.Height * 2 + 4 : paintRect1.Height + 8;
      paintRect2.Height -= 4;
      if (!this.DrawStatusImages(g, paintRect2))
      {
        Brush brush2 = (Brush) new SolidBrush(fillColor);
        g.FillEllipse(brush2, paintRect2);
        brush2.Dispose();
      }
      int num = 2;
      Rectangle r1 = new Rectangle(paintRect2.X + num, paintRect2.Y + num, paintRect2.Width - 2 * num, paintRect2.Height - 2 * num);
      foreach (ToothZone toothZone in this.Zones.Values)
      {
        if ((uint) toothZone.Status > 0U)
          toothZone.Draw(g, r1);
      }
      Rectangle r2 = new Rectangle(paintRect2.X, flag ? paintRect2.Bottom + 2 : paintRect2.Top - paintRect1.Height - 2, paintRect2.Width, paintRect1.Height);
      this.DrawRoots(g, r2, 0);
      if ((uint) this.Status > 0U)
      {
        Rectangle rectangle = new Rectangle(paintRect2.X, flag ? paintRect2.Top - paintRect1.Height - 2 : paintRect2.Bottom + 2, paintRect2.Width, paintRect1.Height + 4);
        g.DrawString(statusText, SystemFonts.DefaultFont, Brushes.Black, (RectangleF) rectangle, Config.PaintConfig.CenterFormat);
      }
      if (!Utils.IsMilkTooth(this.Id))
        return;
      Pen pen = new Pen(Color.Pink, 4f);
      g.DrawEllipse(pen, paintRect2);
      pen.Dispose();
    }

    private bool DrawStatusImages(Graphics g, Rectangle drawRect)
    {
      if (Program.Version == Program.VersionType.GoodOldVersion)
        return Utils.DrawImageIfAny(Dictionaries.ImageTypes.ToothStatus, this.Status, g, drawRect, false);
      g.FillEllipse(Brushes.White, drawRect);
      if (this.StatusV2 == string.Empty)
        return true;
      List<KeyValuePair<int, Dictionaries.DictToothStatusRecordV2>> list = ((IEnumerable<string>) this.StatusV2.Split(',')).Select<string, KeyValuePair<int, Dictionaries.DictToothStatusRecordV2>>((Func<string, KeyValuePair<int, Dictionaries.DictToothStatusRecordV2>>) (x => new KeyValuePair<int, Dictionaries.DictToothStatusRecordV2>(int.Parse(x), Dictionaries.DictToothStatusV2[int.Parse(x)]))).ToList<KeyValuePair<int, Dictionaries.DictToothStatusRecordV2>>();
      list.Sort((Comparison<KeyValuePair<int, Dictionaries.DictToothStatusRecordV2>>) ((x, y) => x.Value.OrderNumber - y.Value.OrderNumber));
      foreach (KeyValuePair<int, Dictionaries.DictToothStatusRecordV2> keyValuePair in list)
      {
        bool flipVert = keyValuePair.Value.IsInvert && this.IsLower();
        Utils.DrawImageIfAny(Dictionaries.ImageTypes.ToothStatusV2, keyValuePair.Key, g, drawRect, flipVert);
      }
      return true;
    }

    private static Rectangle GetToothBodyRect(Rectangle r)
    {
      Rectangle rectangle = r;
      int num = (r.Width - Config.PaintConfig.ToothBodyWidth) / 2;
      rectangle.X += num;
      rectangle.Y += num;
      rectangle.Width = Config.PaintConfig.ToothBodyWidth;
      rectangle.Height = rectangle.Width;
      return rectangle;
    }

    public void DrawBody(Graphics g, Rectangle r)
    {
      Rectangle toothBodyRect = Tooth.GetToothBodyRect(r);
      Color blue = Color.Blue;
      Color fillColor;
      string statusText;
      this.GetDrawParameters(out fillColor, out statusText);
      SolidBrush solidBrush = new SolidBrush(fillColor);
      if (!this.DrawStatusImages(g, toothBodyRect))
      {
        Pen pen = new Pen(blue, (float) Config.PaintConfig.ToothBorderWidth);
        g.FillEllipse((Brush) solidBrush, toothBodyRect);
        g.DrawEllipse(pen, toothBodyRect);
        pen.Dispose();
      }
      Rectangle rectangle = r;
      rectangle.Height = Config.PaintConfig.ToothBodyTextHeight + 8;
      rectangle.Y = r.Bottom - rectangle.Height - 4;
      g.DrawString(statusText, Config.PaintConfig.ToothBodyFont, Program.Version == Program.VersionType.GoodOldVersion ? (Brush) solidBrush : Brushes.Black, (RectangleF) rectangle, Config.PaintConfig.CenterFormat);
      solidBrush.Dispose();
    }

    public void DrawZones(Graphics g, Rectangle r)
    {
      Rectangle toothBodyRect = Tooth.GetToothBodyRect(r);
      foreach (ToothZone toothZone in this.Zones.Values)
        toothZone.Draw(g, toothBodyRect);
      Pen pen = new Pen(Color.Blue, (float) Config.PaintConfig.ToothBorderWidth);
      g.DrawEllipse(pen, toothBodyRect);
      pen.Dispose();
    }

    public void DrawRoots(Graphics g, Rectangle r, int spacing)
    {
      Rectangle firstRootRect = this.GetFirstRootRect(r, spacing);
      foreach (ToothRoot toothRoot in this.Roots.Values)
      {
        if (toothRoot.Active)
        {
          toothRoot.Draw(g, firstRootRect);
          firstRootRect.X += firstRootRect.Width + spacing;
        }
      }
    }

    public void Highlight(Graphics g, bool drawBody, Color color)
    {
      Pen pen = new Pen(color, 2f);
      g.DrawRectangle(pen, this.PaintRect);
      if (drawBody)
        this.DrawSmall(g);
      pen.Dispose();
    }

    public bool Contains(Point p, Rectangle r)
    {
      Point rect = Utils.NormalizePointCoordsToRect(Tooth.GetToothBodyRect(r), p);
      return Utils.CircleContains(Config.PaintConfig.ToothBodyWidth / 2, rect);
    }

    public bool Contains(Point p)
    {
      return this.DrawTooth() && this.PaintRect.Contains(p);
    }

    public ToothZone GetZoneByPoint(Point p, Rectangle r)
    {
      string Type = "";
      Point rect = Utils.NormalizePointCoordsToRect(Tooth.GetToothBodyRect(r), p);
      if (Utils.CircleContains(Config.PaintConfig.ToothCentralZoneWidth / 2, rect))
        Type = rect.X < 0 ? "CL" : (rect.X > 0 ? "CR" : "");
      else if (Utils.CircleContains(Config.PaintConfig.ToothBodyWidth / 2, rect))
        Type = rect.X + rect.Y <= 0 ? (-rect.X + rect.Y > 0 ? "L" : "B") : (-rect.X + rect.Y > 0 ? "F" : "R");
      return Type != "" ? this.FindZoneByType(Type) : (ToothZone) null;
    }

    private int GetRootCount()
    {
      int num = 0;
      foreach (ToothRoot toothRoot in this.Roots.Values)
        num += toothRoot.Active ? 1 : 0;
      return num;
    }

    private Rectangle GetFirstRootRect(Rectangle bodyRect, int spacing)
    {
      Rectangle rectangle = bodyRect;
      int rootCount = this.GetRootCount();
      if (rootCount == 0)
        return bodyRect;
      rectangle.Width = rectangle.Width / 4 - spacing;
      int num = rectangle.Width * rootCount + spacing * (rootCount - 1);
      rectangle.X += (bodyRect.Width - num) / 2;
      return rectangle;
    }

    public ToothRoot GetRootByPoint(Point p, Rectangle r)
    {
      Rectangle firstRootRect = this.GetFirstRootRect(r, 5);
      foreach (ToothRoot toothRoot in this.Roots.Values)
      {
        if (toothRoot.Active)
        {
          if (firstRootRect.Contains(p))
            return toothRoot;
          firstRootRect.X += firstRootRect.Width + 5;
        }
      }
      return (ToothRoot) null;
    }

    private ToothZone FindZoneByType(string Type)
    {
      foreach (KeyValuePair<int, Dictionaries.DictToothZoneRecord> keyValuePair in Dictionaries.DictToothZone)
      {
        if (keyValuePair.Value.ShortName == Type)
          return this.Zones[keyValuePair.Key];
      }
      return (ToothZone) null;
    }

    public void GetDrawParameters(out Color fillColor, out string statusText)
    {
      fillColor = Color.White;
      statusText = "";
      if (Program.Version == Program.VersionType.GoodOldVersion)
      {
        Dictionaries.DictToothStatusRecord toothStatusRecord = new Dictionaries.DictToothStatusRecord();
        if (!Dictionaries.DictToothStatus.TryGetValue(this.Status, out toothStatusRecord))
          return;
        fillColor = Utils.GetColorById(toothStatusRecord.ColorId);
        statusText = toothStatusRecord.ShortName;
      }
      else
      {
        foreach (int key in Utils.ParseIntCSV(this.Diagnosis))
        {
          Dictionaries.DictToothDiagnosisRecord toothDiagnosisRecord = new Dictionaries.DictToothDiagnosisRecord();
          if (Dictionaries.DictToothDiagnosis.TryGetValue(key, out toothDiagnosisRecord))
            statusText += statusText != string.Empty ? "," + toothDiagnosisRecord.ShortName : toothDiagnosisRecord.ShortName;
        }
      }
    }

    public new bool IsModified()
    {
      if (base.IsModified())
        return true;
      foreach (StatusObject statusObject in this.Zones.Values)
      {
        if (statusObject.IsModified())
          return true;
      }
      foreach (StatusObject statusObject in this.Roots.Values)
      {
        if (statusObject.IsModified())
          return true;
      }
      return false;
    }

    public void SaveModified(int id)
    {
      if (!this.IsModified())
        return;
      StatusObject.SaveAction saveAction1 = this.GetSaveAction();
      if (Utils.IsMilkTooth(id))
      {
        if (!this.InitialActive && this.IsActive)
          saveAction1 = StatusObject.SaveAction.Add;
        if (this.InitialActive && !this.IsActive)
          saveAction1 = StatusObject.SaveAction.Delete;
        if (!this.InitialActive && !this.IsActive)
          saveAction1 = StatusObject.SaveAction.None;
      }
      if ((uint) saveAction1 > 0U)
      {
        if (saveAction1 == StatusObject.SaveAction.Delete)
          DBUtil.ExecProc("TEETHMAP_DELETE", (object) DBUtil.InitialRecordID, (object) id, (object) Program.CommandLine.GetOpt("TableID"));
        else if (Program.Version == Program.VersionType.GoodOldVersion)
        {
          switch (saveAction1)
          {
            case StatusObject.SaveAction.Add:
              DBUtil.ExecProc("TEETHMAP_ADD", (object) DBUtil.InitialRecordID, (object) id, (object) this.Status, (object) Program.CommandLine.GetOpt("TableID"));
              break;
            case StatusObject.SaveAction.Update:
              DBUtil.ExecProc("TEETHMAP_UPDATE", (object) DBUtil.InitialRecordID, (object) id, (object) this.Status, (object) Program.CommandLine.GetOpt("TableID"));
              break;
          }
        }
        else
        {
          switch (saveAction1)
          {
            case StatusObject.SaveAction.Add:
              DBUtil.ExecProc("TEETHMAP_ADD", (object) DBUtil.InitialRecordID, (object) id, (object) 0, (object) Program.CommandLine.GetOpt("TableID"));
              DBUtil.ExecProc("TEETHMAPSTATUS_ADD", (object) DBUtil.InitialRecordID, (object) id, (object) this.StatusV2, (object) Program.CommandLine.GetOpt("TableID"));
              DBUtil.ExecProc("TEETHMAPDIAGNOSIS_ADD", (object) DBUtil.InitialRecordID, (object) id, (object) this.Diagnosis, (object) Program.CommandLine.GetOpt("TableID"));
              break;
            case StatusObject.SaveAction.Update:
              DBUtil.ExecProc("TEETHMAPSTATUS_UPDATE", (object) DBUtil.InitialRecordID, (object) id, (object) this.StatusV2, (object) Program.CommandLine.GetOpt("TableID"));
              DBUtil.ExecProc("TEETHMAPDIAGNOSIS_UPDATE", (object) DBUtil.InitialRecordID, (object) id, (object) this.Diagnosis, (object) Program.CommandLine.GetOpt("TableID"));
              break;
          }
        }
      }
      foreach (KeyValuePair<int, ToothZone> zone in this.Zones)
      {
        StatusObject.SaveAction saveAction2 = zone.Value.GetSaveAction();
        if ((uint) saveAction2 > 0U)
        {
          switch (saveAction2)
          {
            case StatusObject.SaveAction.Add:
              DBUtil.ExecProc("TEETHZONEMAP_ADD", (object) DBUtil.InitialRecordID, (object) id, (object) zone.Key, (object) zone.Value.Status, (object) Program.CommandLine.GetOpt("TableID"));
              break;
            case StatusObject.SaveAction.Update:
              DBUtil.ExecProc("TEETHZONEMAP_UPDATE", (object) DBUtil.InitialRecordID, (object) id, (object) zone.Key, (object) zone.Value.Status, (object) Program.CommandLine.GetOpt("TableID"));
              break;
            case StatusObject.SaveAction.Delete:
              DBUtil.ExecProc("TEETHZONEMAP_DELETE", (object) DBUtil.InitialRecordID, (object) id, (object) zone.Key, (object) Program.CommandLine.GetOpt("TableID"));
              break;
          }
        }
        zone.Value.SetUnmodified();
      }
      foreach (KeyValuePair<int, ToothRoot> root in this.Roots)
      {
        StatusObject.SaveAction saveAction2 = root.Value.GetSaveAction();
        if (root.Value.Modified && root.Value.Active && saveAction2 == StatusObject.SaveAction.None)
          saveAction2 = StatusObject.SaveAction.Add;
        if ((uint) saveAction2 > 0U)
        {
          if (saveAction2 == StatusObject.SaveAction.Delete)
            DBUtil.ExecProc("TEETHROOTMAP_DELETE", (object) DBUtil.InitialRecordID, (object) id, (object) root.Key, (object) Program.CommandLine.GetOpt("TableID"));
          else if (Program.Version == Program.VersionType.GoodOldVersion)
          {
            switch (saveAction2)
            {
              case StatusObject.SaveAction.Add:
                DBUtil.ExecProc("TEETHROOTMAP_ADD", (object) DBUtil.InitialRecordID, (object) id, (object) root.Key, (object) root.Value.Status, (object) Program.CommandLine.GetOpt("TableID"));
                break;
              case StatusObject.SaveAction.Update:
                DBUtil.ExecProc("TEETHROOTMAP_UPDATE", (object) DBUtil.InitialRecordID, (object) id, (object) root.Key, (object) root.Value.Status, (object) Program.CommandLine.GetOpt("TableID"));
                break;
            }
          }
          else
          {
            switch (saveAction2)
            {
              case StatusObject.SaveAction.Add:
                DBUtil.ExecProc("TEETHROOTMAP_ADD", (object) DBUtil.InitialRecordID, (object) id, (object) root.Key, (object) 0, (object) Program.CommandLine.GetOpt("TableID"));
                DBUtil.ExecProc("TEETHROOTMAPSTATUS_ADD", (object) DBUtil.InitialRecordID, (object) id, (object) root.Key, (object) root.Value.GetStatusV2(), (object) Program.CommandLine.GetOpt("TableID"));
                break;
              case StatusObject.SaveAction.Update:
                DBUtil.ExecProc("TEETHROOTMAPSTATUS_UPDATE", (object) DBUtil.InitialRecordID, (object) id, (object) root.Key, (object) root.Value.GetStatusV2(), (object) Program.CommandLine.GetOpt("TableID"));
                break;
            }
          }
        }
        root.Value.SetUnmodified();
      }
      this.SetUnmodified();
      this.InitialActive = this.IsActive;
    }

    private bool DrawTooth()
    {
      if (Utils.IsMilkTooth(this.Id))
      {
        if (!this.IsActive)
          return false;
      }
      else if (Program.MedCard.HasMilkTooth(Utils.GetMilkToothId(this.Id)))
        return false;
      return true;
    }

    public bool ToggleMilk()
    {
      if (Utils.IsMilkTooth(this.Id))
      {
        this.IsActive = !this.IsActive;
        this.Modified = this.IsActive != this.InitialActive;
      }
      return this.IsActive;
    }

    public bool SetMilk(bool milk)
    {
      if (Utils.IsMilkTooth(this.Id))
      {
        this.IsActive = milk;
        this.Modified = this.IsActive != this.InitialActive;
      }
      return this.IsActive;
    }

    public DiagnosisMenu.Selection GetDiagnosisAndStatus()
    {
      return new DiagnosisMenu.Selection()
      {
        DiagnosisList = this.Diagnosis,
        StatusList = this.StatusV2
      };
    }

    public void SetDiagnosisAndStatus(DiagnosisMenu.Selection data)
    {
      if (this.Diagnosis != data.DiagnosisList || this.StatusV2 != data.StatusList)
        this.Modified = true;
      this.Diagnosis = data.DiagnosisList;
      this.StatusV2 = data.StatusList;
      this.SetStatus(this.Diagnosis != string.Empty || this.StatusV2 != string.Empty ? -1 : 0);
    }

    public void AddDiagnosis(int id)
    {
      this.SetStatus(-1);
      this.SetUnmodified();
      this.Diagnosis += this.Diagnosis != string.Empty ? "," + id.ToString() : id.ToString();
    }

    public void AddStatusV2(int id)
    {
      this.SetStatus(-1);
      this.SetUnmodified();
      this.StatusV2 += this.StatusV2 != string.Empty ? "," + id.ToString() : id.ToString();
    }

    public bool IsLower()
    {
      return (Utils.IsMilkTooth(this.Id) ? (this.Id < 70 ? 1 : 0) : (this.Id < 30 ? 1 : 0)) == 0;
    }
  }
}
