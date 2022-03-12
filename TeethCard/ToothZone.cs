using System.Drawing;

namespace TeethCard
{
  internal class ToothZone : StatusObject
  {
    public string Type;

    public ToothZone(Dictionaries.DictToothZoneRecord rec)
    {
      this.Type = rec.ShortName;
    }

    public void Draw(Graphics g, Rectangle r)
    {
      Color blue = Color.Blue;
      Color color = Color.White;
      Dictionaries.DictToothZoneStatusRecord zoneStatusRecord;
      if (Dictionaries.DictToothZoneStatus.TryGetValue(this.Status, out zoneStatusRecord))
        color = Utils.GetColorById(zoneStatusRecord.ColorId);
      Pen pen = new Pen(blue, 1f);
      SolidBrush solidBrush = new SolidBrush(color);
      float startAngle = 0.0f;
      float sweepAngle = 90f;
      string type1 = this.Type;
      if (!(type1 == "L"))
      {
        if (!(type1 == "R"))
        {
          if (!(type1 == "B"))
          {
            if (type1 == "F")
            {
              startAngle = 45f;
            }
            else
            {
              sweepAngle = 180f;
              string type2 = this.Type;
              if (!(type2 == "CL"))
              {
                if (type2 == "CR")
                  startAngle = 270f;
              }
              else
                startAngle = 90f;
              int num1 = r.Width * Config.PaintConfig.ToothCentralZonePercentRadius / 100;
              int num2 = (r.Width - num1) / 2;
              r.X += num2;
              r.Y += num2;
              r.Width = num1;
              r.Height = r.Width;
            }
          }
          else
            startAngle = 225f;
        }
        else
          startAngle = 315f;
      }
      else
        startAngle = 135f;
      g.FillPie((Brush) solidBrush, r, startAngle, sweepAngle);
      g.DrawPie(pen, r, startAngle, sweepAngle);
      solidBrush.Dispose();
      pen.Dispose();
    }
  }
}
