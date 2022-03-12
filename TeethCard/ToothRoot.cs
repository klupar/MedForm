using MedForm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TeethCard
{
  internal class ToothRoot : StatusObject
  {
    private string StatusV2 = string.Empty;
    public bool Active;

    public ToothRoot(Dictionaries.DictToothRootRecord rec)
    {
      this.Active = false;
    }

    public void Draw(Graphics g, Rectangle r)
    {
      if (!this.Active || this.DrawStatusImages(g, r))
        return;
      g.FillRectangle(Brushes.White, r);
      g.DrawRectangle(Pens.Black, r);
    }

    private bool DrawStatusImages(Graphics g, Rectangle drawRect)
    {
      if (Program.Version == Program.VersionType.GoodOldVersion)
        return Utils.DrawImageIfAny(Dictionaries.ImageTypes.ToothStatus, this.Status, g, drawRect, false);
      if (this.StatusV2 != string.Empty)
      {
        if (this.StatusV2 == string.Empty)
          return false;
        List<KeyValuePair<int, Dictionaries.DictToothStatusRecordV2>> list = ((IEnumerable<string>) this.StatusV2.Split(',')).Select<string, KeyValuePair<int, Dictionaries.DictToothStatusRecordV2>>((Func<string, KeyValuePair<int, Dictionaries.DictToothStatusRecordV2>>) (x => new KeyValuePair<int, Dictionaries.DictToothStatusRecordV2>(int.Parse(x), Dictionaries.DictToothStatusV2[int.Parse(x)]))).ToList<KeyValuePair<int, Dictionaries.DictToothStatusRecordV2>>();
        list.Sort((Comparison<KeyValuePair<int, Dictionaries.DictToothStatusRecordV2>>) ((x, y) => x.Value.OrderNumber - y.Value.OrderNumber));
        foreach (KeyValuePair<int, Dictionaries.DictToothStatusRecordV2> keyValuePair in list)
          Utils.DrawImageIfAny(Dictionaries.ImageTypes.RootStatusV2, keyValuePair.Key, g, drawRect, false);
      }
      return true;
    }

    public void DrawSmall(Graphics g, Rectangle r)
    {
      Pen pen = new Pen(this.Active ? Color.Black : Program.FormMain.BackColor);
      g.DrawEllipse(pen, r);
      pen.Dispose();
    }

    public override StatusObject.SaveAction GetSaveAction()
    {
      return !this.Modified || this.Active ? base.GetSaveAction() : StatusObject.SaveAction.Delete;
    }

    public void AddStatusV2(int id)
    {
      this.SetStatus(-1);
      this.SetUnmodified();
      this.StatusV2 += this.StatusV2 != string.Empty ? "," + id.ToString() : id.ToString();
    }

    public string GetStatusV2()
    {
      return this.StatusV2;
    }

    public void SetStatusV2(string statusList)
    {
      if (this.StatusV2 != statusList)
        this.Modified = true;
      this.StatusV2 = statusList;
      this.SetStatus(this.StatusV2 != string.Empty ? -1 : 0);
    }
  }
}
