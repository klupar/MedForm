using System.Drawing;

namespace TeethCard
{
  internal class PaintConfig
  {
    private static string DEFAULT_TOOTH_HIGHLIGHTCOLOR = "#0000FF";
    private static int DEFAULT_TOOTHCENTRALZONE_PERCENTRADIUS = 40;
    private const int DEFAULT_TOOTH_WIDTH = 40;
    private const int DEFAULT_TOOTH_HEIGHT = 100;
    private const int DEFAULT_TOOTH_SPACING = 5;
    private const int DEFAULT_ACTIVEPANEL_WIDTH = 160;
    private const int DEFAULT_ACTIVEPANEL_HEIGHT = 200;
    private const int DEFAULT_ACTIVEPANEL_SPACING = 5;
    private const int DEFAULT_CARDTEXT_HEIGHT = 16;
    private const int DEFAULT_TOOTHBODYTEXT_HEIGHT = 16;
    private const int DEFAULT_TOOTH_BORDERWIDTH = 4;
    public int ToothWidth;
    public int ToothHeight;
    public int ToothSpacing;
    public int ActivePanelWidth;
    public int ActivePanelHeight;
    public int ActivePanelSpacing;
    public int CardTextHeight;
    public int ToothBodyTextHeight;
    public int ToothBorderWidth;
    public string ToothHighlightColorStr;
    public Color ToothHighlightColor;
    public int ToothCentralZonePercentRadius;
    public StringFormat CenterFormat;
    public Font ToothBodyFont;
    public int ToothBodyWidth;
    public int ToothCentralZoneWidth;

    public void InitStockObjects()
    {
      this.CenterFormat = new StringFormat();
      this.CenterFormat.LineAlignment = StringAlignment.Center;
      this.CenterFormat.Alignment = StringAlignment.Center;
      this.ToothBodyFont = new Font(FontFamily.GenericSansSerif, (float) this.ToothBodyTextHeight);
      this.ToothBodyWidth = this.ActivePanelWidth - 4 * this.ToothSpacing;
      this.ToothCentralZoneWidth = this.ToothBodyWidth * Config.PaintConfig.ToothCentralZonePercentRadius / 100;
      this.ToothHighlightColor = Utils.StringToColor(this.ToothHighlightColorStr);
    }

    public void Load()
    {
      this.ToothWidth = Config.ReadInt("ToothWidth", 40);
      this.ToothHeight = Config.ReadInt("ToothHeight", 100);
      this.ToothSpacing = Config.ReadInt("ToothSpacing", 5);
      this.ActivePanelWidth = Config.ReadInt("ActivePanelWidth", 160);
      this.ActivePanelHeight = Config.ReadInt("ActivePanelHeight", 200);
      this.ActivePanelSpacing = Config.ReadInt("ActivePanelSpacing", 5);
      this.CardTextHeight = Config.ReadInt("CardTextHeight", 16);
      this.ToothBorderWidth = Config.ReadInt("ToothBorderWidth", 4);
      this.ToothHighlightColorStr = Config.ReadString("ToothHighlightColor", PaintConfig.DEFAULT_TOOTH_HIGHLIGHTCOLOR);
      this.ToothBodyTextHeight = Config.ReadInt("ToothBodyTextHeight", 16);
      this.ToothCentralZonePercentRadius = Config.ReadInt("ToothCentralZonePercentRadius", PaintConfig.DEFAULT_TOOTHCENTRALZONE_PERCENTRADIUS);
      this.InitStockObjects();
    }

    public void Save()
    {
      Config.WriteInt("ToothWidth", this.ToothWidth);
      Config.WriteInt("ToothHeight", this.ToothHeight);
      Config.WriteInt("ToothSpacing", this.ToothSpacing);
      Config.WriteInt("ActivePanelWidth", this.ActivePanelWidth);
      Config.WriteInt("ActivePanelHeight", this.ActivePanelHeight);
      Config.WriteInt("ActivePanelSpacing", this.ActivePanelSpacing);
      Config.WriteInt("CardTextHeight", this.CardTextHeight);
      Config.WriteInt("ToothBorderWidth", this.ToothBorderWidth);
      Config.WriteString("ToothHighlightColor", this.ToothHighlightColorStr);
      Config.WriteInt("ToothBodyTextHeight", this.ToothBodyTextHeight);
      Config.WriteInt("ToothCentralZonePercentRadius", this.ToothCentralZonePercentRadius);
    }
  }
}
