using MedForm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace TeethCard
{
  public class FormMain : Form
  {
    private bool Multiselect_ = false;
    private HashSet<int> SelectedTeeth_ = new HashSet<int>();
    private IContainer components = (IContainer) null;
    private Tooth HighlightedTooth;
    private Tooth SelectedTooth;
    private ToothZone SelectedZone;
    private ToothRoot SelectedRoot;
    private DiagnosisMenu DiagnosisMenu;
    private Rectangle RootsRect;
    private Panel PanelTeeth;
    private Panel PanelToothBody;
    private Panel PanelToothZones;
    private Panel PanelToothRoots;
    private ContextMenuStrip ContextMenuTooth;
    private ContextMenuStrip ContextMenuToothZone;
    private ContextMenuStrip ContextMenuToothRoot;
    private Button ButtonAddDelRoot;
    private ContextMenuStrip ContextMenuAddDelRoot;
    private CheckBox CheckBoxMilk;
    private Label LabelTooth;

    public FormMain()
    {
      this.InitializeComponent();
      this.LoadMenus();
    }

    private void LoadMenus()
    {
      foreach (KeyValuePair<int, Dictionaries.DictToothStatusRecord> dictToothStatu in Dictionaries.DictToothStatus)
        this.ContextMenuTooth.Items.Add(dictToothStatu.Value.Name).Tag = (object) dictToothStatu.Key;
      foreach (KeyValuePair<int, Dictionaries.DictToothZoneStatusRecord> dictToothZoneStatu in Dictionaries.DictToothZoneStatus)
        this.ContextMenuToothZone.Items.Add(dictToothZoneStatu.Value.Name).Tag = (object) dictToothZoneStatu.Key;
      foreach (KeyValuePair<int, Dictionaries.DictToothRootRecord> keyValuePair in Dictionaries.DictToothRoot)
        this.ContextMenuAddDelRoot.Items.Add(keyValuePair.Value.Name).Tag = (object) keyValuePair.Key;
      if (Program.Version == Program.VersionType.GoodOldVersion)
      {
        foreach (KeyValuePair<int, Dictionaries.DictToothRootStatusRecord> dictToothRootStatu in Dictionaries.DictToothRootStatus)
          this.ContextMenuToothRoot.Items.Add(dictToothRootStatu.Value.Name).Tag = (object) dictToothRootStatu.Key;
      }
      else
      {
        foreach (KeyValuePair<int, Dictionaries.DictToothRootStatusRecordV2> keyValuePair in Dictionaries.DictToothRootStatusV2)
          this.ContextMenuToothRoot.Items.Add(keyValuePair.Value.Name).Tag = (object) keyValuePair.Key;
        this.DiagnosisMenu = new DiagnosisMenu();
        this.DiagnosisMenu.MakeMenu();
      }
    }

    private void HighlightTooth(Tooth t)
    {
      if (t == this.HighlightedTooth)
        return;
      Graphics graphics = this.PanelTeeth.CreateGraphics();
      if (this.HighlightedTooth != null && this.SelectedTooth != this.HighlightedTooth)
        this.HighlightedTooth.Highlight(graphics, true, this.PanelTeeth.BackColor);
      t?.Highlight(graphics, false, Config.PaintConfig.ToothHighlightColor);
      graphics.Dispose();
      this.HighlightedTooth = t;
    }

    private void SelectTooth(Tooth t)
    {
      bool bShow = t != null;
      if (this.SelectedTooth != t && this.SelectedTeeth_.Count < 2)
      {
        if (!Program.FormMain.UpdateFieldsPanel(bShow, true, bShow ? t.DBId : -1))
          return;
        if (this.SelectedTooth != null)
          this.SelectedTooth.SaveModified(this.SelectedTooth.Id);
      }
      if (!this.Multiselect_)
        this.ClearTeethSelection();
      if (t != null)
      {
        Graphics graphics = this.PanelTeeth.CreateGraphics();
        if (!this.AddToothSelection(t))
        {
          if (this.SelectedTeeth_.Count > 1)
          {
            t.Highlight(graphics, true, this.PanelTeeth.BackColor);
            this.RemoveToothSelection(t);
          }
        }
        else
          t.Highlight(graphics, false, Config.PaintConfig.ToothHighlightColor);
        graphics.Dispose();
        t = this.SelectedTeeth_.Count > 1 ? (Tooth) null : this.GetFirstSelectedTooth();
      }
      else if (!this.Multiselect_ && this.SelectedTooth != null && this.SelectedTooth != t)
      {
        Graphics graphics = this.PanelTeeth.CreateGraphics();
        this.SelectedTooth.Highlight(graphics, true, this.PanelTeeth.BackColor);
        graphics.Dispose();
      }
      bool flag = t != null;
      this.SelectedTooth = t;
      this.LabelTooth.Text = t != null ? t.Name : "";
      this.PanelToothBody.Refresh();
      this.PanelToothZones.Refresh();
      this.PanelToothRoots.Refresh();
      this.CheckBoxMilk.Checked = t != null && t.IsActive;
      this.CheckBoxMilk.Enabled = t != null && (t.IsActive || Program.MedCard.HasTooth(Utils.GetMilkToothId(t.Id)));
      Size clientSize = this.ClientSize;
      clientSize.Width = this.PanelToothRoots.Right + this.PanelTeeth.Left;
      clientSize.Height = this.PanelTeeth.Bottom + this.PanelTeeth.Left;
      this.ClientSize = clientSize;
      this.PanelToothBody.Visible = flag;
      this.PanelToothZones.Visible = flag;
      this.PanelToothRoots.Visible = flag;
      this.LabelTooth.Visible = flag;
      this.CheckBoxMilk.Visible = flag;
      if (this.SelectedTeeth_.Count > 1)
      {
        this.CheckBoxMilk.Left = this.PanelTeeth.Right + 5;
        this.CheckBoxMilk.Top = this.PanelTeeth.Top;
        this.CheckBoxMilk.Enabled = true;
        this.CheckBoxMilk.Checked = false;
        this.CheckBoxMilk.Visible = true;
        foreach (int selectedTooth in this.SelectedTeeth_)
        {
          t = Program.MedCard.GetTooth(selectedTooth);
          if (t.IsActive)
          {
            this.CheckBoxMilk.Checked = true;
            break;
          }
        }
      }
      else
      {
        this.CheckBoxMilk.Left = this.PanelToothRoots.Left;
        this.CheckBoxMilk.Top = this.PanelToothRoots.Bottom + 1;
      }
    }

    public void DeselectTooth()
    {
      this.SelectTooth((Tooth) null);
    }

    private void RedrawSelectedTooth()
    {
      if (this.SelectedTooth == null)
        return;
      Graphics graphics = this.PanelTeeth.CreateGraphics();
      this.SelectedTooth.DrawSmall(graphics);
      this.SelectedTooth.Highlight(graphics, false, Config.PaintConfig.ToothHighlightColor);
      graphics.Dispose();
    }

    private void MarkMenuItem(ref ContextMenuStrip menu, int id)
    {
      foreach (ToolStripMenuItem toolStripMenuItem in (ArrangedElementCollection) menu.Items)
      {
        int tag = (int) toolStripMenuItem.Tag;
        toolStripMenuItem.Checked = tag == id;
      }
    }

    private void FormMain_Load(object sender, EventArgs e)
    {
      int left = this.PanelTeeth.Left;
      this.PanelTeeth.Width = (Config.PaintConfig.ToothWidth + Config.PaintConfig.ToothSpacing) * 16 + Config.PaintConfig.ToothSpacing + 3;
      this.PanelTeeth.Height = (Config.PaintConfig.ToothHeight + Config.PaintConfig.ToothSpacing) * 2 + Config.PaintConfig.ToothSpacing + 3;
      this.PanelToothBody.Location = new Point(this.PanelTeeth.Right + Config.PaintConfig.ToothSpacing, this.PanelTeeth.Top);
      this.PanelToothBody.Width = Config.PaintConfig.ActivePanelWidth;
      this.PanelToothBody.Height = Config.PaintConfig.ActivePanelHeight;
      this.PanelToothZones.Location = new Point(this.PanelToothBody.Left + Config.PaintConfig.ActivePanelWidth + Config.PaintConfig.ActivePanelSpacing, this.PanelToothBody.Location.Y);
      this.PanelToothZones.Size = this.PanelToothBody.Size;
      this.LabelTooth.Left = this.PanelToothBody.Left;
      this.LabelTooth.Top = this.PanelToothBody.Bottom + 1;
      this.LabelTooth.Width = Config.PaintConfig.ActivePanelWidth;
      this.PanelToothRoots.Location = new Point(this.PanelToothBody.Left + 2 * (Config.PaintConfig.ActivePanelWidth + Config.PaintConfig.ActivePanelSpacing), this.PanelToothBody.Location.Y);
      this.PanelToothRoots.Size = this.PanelToothBody.Size;
      this.CheckBoxMilk.Left = this.PanelToothRoots.Left;
      this.CheckBoxMilk.Top = this.PanelToothRoots.Bottom + 1;
      this.CheckBoxMilk.Width = Config.PaintConfig.ActivePanelWidth;
      this.Width = this.PanelToothRoots.Right + left;
      this.Height = this.PanelTeeth.Height + left;
      this.DeselectTooth();
      this.MakeRootsRect();
    }

    private void MakeRootsRect()
    {
      this.RootsRect = this.PanelToothRoots.ClientRectangle;
      int num1 = 3;
      int num2 = 3;
      this.RootsRect.X += num1;
      this.RootsRect.Width -= 2 * num1;
      this.RootsRect.Y += num2;
      this.RootsRect.Height = this.ButtonAddDelRoot.Top - 2 * num2;
    }

    private void PanelTeeth_Paint(object sender, PaintEventArgs e)
    {
      Program.MedCard.Draw(e.Graphics);
      if (this.SelectedTooth == null)
        return;
      Graphics graphics = this.PanelTeeth.CreateGraphics();
      this.SelectedTooth.Highlight(graphics, false, Config.PaintConfig.ToothHighlightColor);
      graphics.Dispose();
    }

    private void PanelTeeth_MouseMove(object sender, MouseEventArgs e)
    {
      if (this.Multiselect_ || this.SelectedTeeth_.Count >= 2)
        return;
      this.HighlightTooth(Program.MedCard.FindTooth(e.Location));
    }

    private void PanelTeeth_MouseLeave(object sender, EventArgs e)
    {
      if (this.Multiselect_ || this.SelectedTeeth_.Count >= 2)
        return;
      this.HighlightTooth((Tooth) null);
    }

    private void PanelTeeth_MouseDown(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left)
      {
        this.SelectTooth(Program.MedCard.FindTooth(e.Location));
      }
      else
      {
        if (e.Button != MouseButtons.Right)
          return;
        this.DiagnosisMenuHandler(sender, e);
      }
    }

    private void PanelToothBody_Paint(object sender, PaintEventArgs e)
    {
      if (this.SelectedTooth == null)
        return;
      this.SelectedTooth.DrawBody(e.Graphics, this.PanelToothBody.ClientRectangle);
    }

    private void PanelToothZones_Paint(object sender, PaintEventArgs e)
    {
      if (this.SelectedTooth == null)
        return;
      this.SelectedTooth.DrawZones(e.Graphics, this.PanelToothZones.ClientRectangle);
    }

    private void PanelToothRoots_Paint(object sender, PaintEventArgs e)
    {
      if (this.SelectedTooth == null)
        return;
      this.SelectedTooth.DrawRoots(e.Graphics, this.RootsRect, 5);
    }

    private void PanelToothBody_MouseDown(object sender, MouseEventArgs e)
    {
      if (this.SelectedTooth == null || !this.SelectedTooth.Contains(e.Location, this.PanelToothBody.ClientRectangle))
        return;
      if (Program.Version == Program.VersionType.GoodOldVersion)
      {
        this.MarkMenuItem(ref this.ContextMenuTooth, this.SelectedTooth.Status);
        this.ContextMenuTooth.Show(this.PanelToothBody.PointToScreen(e.Location));
      }
      else
        this.DiagnosisMenuHandler(sender, e);
    }

    private void DiagnosisMenuHandler(object sender, MouseEventArgs e)
    {
      HashSet<int> set1 = new HashSet<int>();
      HashSet<int> set2 = new HashSet<int>();
      DiagnosisMenu.Selection sel = new DiagnosisMenu.Selection();
      if (this.SelectedTeeth_.Count <= 1)
      {
        foreach (int selectedTooth in this.SelectedTeeth_)
        {
          DiagnosisMenu.Selection diagnosisAndStatus = Program.MedCard.GetTooth(selectedTooth).GetDiagnosisAndStatus();
          HashSet<int> intCsvAsSet1 = Utils.ParseIntCSVAsSet(diagnosisAndStatus.DiagnosisList);
          HashSet<int> intCsvAsSet2 = Utils.ParseIntCSVAsSet(diagnosisAndStatus.StatusList);
          set1.UnionWith((IEnumerable<int>) intCsvAsSet1);
          set2.UnionWith((IEnumerable<int>) intCsvAsSet2);
        }
      }
      sel.DiagnosisList = Utils.HashSetToString(set1);
      sel.StatusList = Utils.HashSetToString(set2);
      if (!this.DiagnosisMenu.Show((sender as Control).PointToScreen(e.Location), sel))
        return;
      DiagnosisMenu.Selection result = this.DiagnosisMenu.GetResult();
      foreach (int selectedTooth in this.SelectedTeeth_)
      {
        Tooth tooth = Program.MedCard.GetTooth(selectedTooth);
        tooth.SetDiagnosisAndStatus(result);
        if (tooth.DBId == 0)
          tooth.DBId = DBUtil.CreateNewToothRecord(tooth.Id);
        Graphics graphics = this.PanelTeeth.CreateGraphics();
        tooth.DrawSmall(graphics);
        tooth.Highlight(graphics, false, Config.PaintConfig.ToothHighlightColor);
        graphics.Dispose();
      }
      this.PanelToothBody.Refresh();
    }

    private void PanelToothZones_MouseDown(object sender, MouseEventArgs e)
    {
      if (this.SelectedTooth == null)
        return;
      this.SelectedZone = this.SelectedTooth.GetZoneByPoint(e.Location, this.PanelToothZones.ClientRectangle);
      if (this.SelectedZone == null)
        return;
      this.MarkMenuItem(ref this.ContextMenuToothZone, this.SelectedZone.Status);
      this.ContextMenuToothZone.Show(this.PanelToothZones.PointToScreen(e.Location));
    }

    private void PanelToothRoots_MouseDown(object sender, MouseEventArgs e)
    {
      if (this.SelectedTooth == null)
        return;
      this.SelectedRoot = this.SelectedTooth.GetRootByPoint(e.Location, this.RootsRect);
      if (this.SelectedRoot == null)
        return;
      if (Program.Version == Program.VersionType.GoodOldVersion)
      {
        this.MarkMenuItem(ref this.ContextMenuToothRoot, this.SelectedRoot.Status);
      }
      else
      {
        HashSet<int> intCsvAsSet = Utils.ParseIntCSVAsSet(this.SelectedRoot.GetStatusV2());
        foreach (ToolStripMenuItem toolStripMenuItem in (ArrangedElementCollection) this.ContextMenuToothRoot.Items)
        {
          int tag = (int) toolStripMenuItem.Tag;
          toolStripMenuItem.Checked = intCsvAsSet.Contains(tag);
        }
      }
      this.ContextMenuToothRoot.Show(this.PanelToothRoots.PointToScreen(e.Location));
    }

    private void ButtonAddDelRoot_MouseClick(object sender, MouseEventArgs e)
    {
      if (this.SelectedTooth == null)
        return;
      foreach (ToolStripMenuItem toolStripMenuItem in (ArrangedElementCollection) this.ContextMenuAddDelRoot.Items)
      {
        int tag = (int) toolStripMenuItem.Tag;
        toolStripMenuItem.Checked = this.SelectedTooth.Roots[tag].Active;
      }
      this.ContextMenuAddDelRoot.Show(this.ButtonAddDelRoot.PointToScreen(e.Location));
    }

    private void ContextMenuTooth_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
      int tag = (int) e.ClickedItem.Tag;
      Tooth selectedTooth1 = this.SelectedTooth;
      foreach (int selectedTooth2 in this.SelectedTeeth_)
      {
        this.SelectedTooth = Program.MedCard.GetTooth(selectedTooth2);
        this.FullUpdateSelectedToothStatus(tag);
      }
      this.SelectedTooth = selectedTooth1;
    }

    private void FullUpdateSelectedToothStatus(int Status)
    {
      if (this.SelectedTooth == null)
        return;
      this.SelectedTooth.SetStatus(Status);
      this.PanelToothBody.Refresh();
      this.RedrawSelectedTooth();
      if (this.SelectedTooth.DBId == 0)
      {
        this.SelectedTooth.DBId = DBUtil.CreateNewToothRecord(this.SelectedTooth.Id);
        this.SelectedTooth.SaveModified(this.SelectedTooth.Id);
      }
      if (Status == 0)
        this.SelectedTooth.DBId = 0;
      Program.FormMain.UpdateSelectedToothStatus(Status, this.SelectedTooth.DBId);
    }

    private void ContextMenuToothZone_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
      int tag = (int) e.ClickedItem.Tag;
      if (this.SelectedZone == null)
        return;
      this.SelectedZone.SetStatus(tag);
      this.PanelToothZones.Refresh();
      this.RedrawSelectedTooth();
    }

    private void ContextMenuToothRoot_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
      int tag = (int) e.ClickedItem.Tag;
      if (this.SelectedRoot == null)
        return;
      ToolStripMenuItem clickedItem = (ToolStripMenuItem) e.ClickedItem;
      clickedItem.Checked = !clickedItem.Checked;
      if (Program.Version == Program.VersionType.GoodOldVersion)
      {
        this.SelectedRoot.SetStatus(tag);
      }
      else
      {
        bool flag = ((ToolStripMenuItem) e.ClickedItem).Checked;
        if (tag == 0 & flag)
        {
          this.SelectedRoot.SetStatusV2("0");
          foreach (ToolStripMenuItem toolStripMenuItem in (ArrangedElementCollection) this.ContextMenuToothRoot.Items)
            toolStripMenuItem.Checked = tag == (int) toolStripMenuItem.Tag;
        }
        else
        {
          HashSet<int> intCsvAsSet = Utils.ParseIntCSVAsSet(this.SelectedRoot.GetStatusV2());
          intCsvAsSet.Remove(0);
          foreach (ToolStripMenuItem toolStripMenuItem in (ArrangedElementCollection) this.ContextMenuToothRoot.Items)
          {
            if ((int) toolStripMenuItem.Tag == 0)
              toolStripMenuItem.Checked = false;
          }
          if (flag)
            intCsvAsSet.Add(tag);
          else
            intCsvAsSet.Remove(tag);
          this.SelectedRoot.SetStatusV2(Utils.HashSetToString(intCsvAsSet));
        }
      }
      this.PanelToothRoots.Refresh();
      this.RedrawSelectedTooth();
    }

    private void ContextMenuAddDelRoot_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
      int tag = (int) e.ClickedItem.Tag;
      if (this.SelectedTooth == null)
        return;
      bool flag = !((ToolStripMenuItem) e.ClickedItem).Checked;
      this.SelectedTooth.Roots[tag].Active = flag;
      this.SelectedTooth.Roots[tag].SetStatus(0);
      this.SelectedTooth.Roots[tag].SetStatusV2("0");
      this.SelectedTooth.Roots[tag].Modified = true;
      this.SelectedTooth.Modified = true;
      this.PanelToothRoots.Refresh();
      this.RedrawSelectedTooth();
      ToolStripMenuItem clickedItem = (ToolStripMenuItem) e.ClickedItem;
      clickedItem.Checked = !clickedItem.Checked;
    }

    private void CheckBoxMilk_CheckedChanged(object sender, EventArgs e)
    {
    }

    private void CheckBoxMilk_Click(object sender, EventArgs e)
    {
      if (this.SelectedTeeth_.Count > 1)
      {
        bool milk = (sender as CheckBox).Checked;
        Graphics graphics = this.PanelTeeth.CreateGraphics();
        foreach (int selectedTooth in this.SelectedTeeth_)
        {
          if (Program.MedCard.HasTooth(Utils.GetMilkToothId(selectedTooth)) || Utils.IsMilkTooth(selectedTooth))
          {
            Tooth tooth = Program.MedCard.SetMilk(selectedTooth, milk);
            tooth.DrawSmall(graphics);
            tooth.Highlight(graphics, false, Config.PaintConfig.ToothHighlightColor);
          }
        }
        graphics.Dispose();
      }
      else
      {
        if (this.SelectedTooth == null)
          return;
        this.SelectTooth(Program.MedCard.ToggleMilk(this.SelectedTooth.Id));
        this.RedrawSelectedTooth();
        this.HighlightTooth(this.SelectedTooth);
      }
    }

    public int GetSelectedToothId()
    {
      return this.SelectedTooth != null ? this.SelectedTooth.Id : -1;
    }

    public void SetSelectedToothDBId(int id)
    {
      if (this.SelectedTooth == null)
        return;
      this.SelectedTooth.DBId = id;
    }

    public void UpdateSelectedToothStatus(int Status)
    {
      if (this.SelectedTooth == null)
        return;
      this.SelectedTooth.SetStatus(Status);
      this.SelectTooth(this.SelectedTooth);
    }

    public void SaveModifiedTooth()
    {
      if (this.SelectedTooth == null)
        return;
      this.SelectedTooth.SaveModified(this.SelectedTooth.Id);
    }

    public void FormMain_KeyDown(object sender, KeyEventArgs e)
    {
      if (!e.Control || e.KeyData != (Keys.ControlKey | Keys.Control))
        return;
      this.StartMultiselect();
    }

    public void FormMain_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Control || e.KeyData != Keys.ControlKey)
        return;
      this.StopMultiselect();
    }

    private void StartMultiselect()
    {
      this.Multiselect_ = true;
      this.HighlightTooth((Tooth) null);
    }

    private void StopMultiselect()
    {
      this.Multiselect_ = false;
    }

    private bool AddToothSelection(Tooth t)
    {
      return this.SelectedTeeth_.Add(t.Id);
    }

    private void RemoveToothSelection(Tooth t)
    {
      this.SelectedTeeth_.Remove(t.Id);
    }

    private void ClearTeethSelection()
    {
      Graphics graphics = this.PanelTeeth.CreateGraphics();
      foreach (int selectedTooth in this.SelectedTeeth_)
        Program.MedCard.GetTooth(selectedTooth).Highlight(graphics, true, this.PanelTeeth.BackColor);
      graphics.Dispose();
      this.SelectedTeeth_.Clear();
    }

    private Tooth GetFirstSelectedTooth()
    {
      using (HashSet<int>.Enumerator enumerator = this.SelectedTeeth_.GetEnumerator())
      {
        if (enumerator.MoveNext())
        {
          int current = enumerator.Current;
          return Program.MedCard.GetTooth(current);
        }
      }
      return (Tooth) null;
    }

    private void ContextMenuAddDelRoot_MouseLeave(object sender, EventArgs e)
    {
      this.ContextMenuAddDelRoot.Close();
    }

    private void ContextMenuToothRoot_MouseLeave(object sender, EventArgs e)
    {
      this.ContextMenuToothRoot.Close();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = (IContainer) new Container();
      this.PanelTeeth = new Panel();
      this.PanelToothBody = new Panel();
      this.PanelToothZones = new Panel();
      this.PanelToothRoots = new Panel();
      this.ButtonAddDelRoot = new Button();
      this.ContextMenuTooth = new ContextMenuStrip(this.components);
      this.ContextMenuToothZone = new ContextMenuStrip(this.components);
      this.ContextMenuToothRoot = new ContextMenuStrip(this.components);
      this.ContextMenuAddDelRoot = new ContextMenuStrip(this.components);
      this.CheckBoxMilk = new CheckBox();
      this.LabelTooth = new Label();
      this.PanelToothRoots.SuspendLayout();
      this.SuspendLayout();
      this.PanelTeeth.BorderStyle = BorderStyle.FixedSingle;
      this.PanelTeeth.Location = new Point(12, 12);
      this.PanelTeeth.Name = "PanelTeeth";
      this.PanelTeeth.Size = new Size(521, 148);
      this.PanelTeeth.TabIndex = 2;
      this.PanelTeeth.Paint += new PaintEventHandler(this.PanelTeeth_Paint);
      this.PanelTeeth.MouseDown += new MouseEventHandler(this.PanelTeeth_MouseDown);
      this.PanelTeeth.MouseLeave += new EventHandler(this.PanelTeeth_MouseLeave);
      this.PanelTeeth.MouseMove += new MouseEventHandler(this.PanelTeeth_MouseMove);
      this.PanelToothBody.BorderStyle = BorderStyle.FixedSingle;
      this.PanelToothBody.Location = new Point(539, 12);
      this.PanelToothBody.Name = "PanelToothBody";
      this.PanelToothBody.Size = new Size(129, 160);
      this.PanelToothBody.TabIndex = 0;
      this.PanelToothBody.Paint += new PaintEventHandler(this.PanelToothBody_Paint);
      this.PanelToothBody.MouseDown += new MouseEventHandler(this.PanelToothBody_MouseDown);
      this.PanelToothZones.BorderStyle = BorderStyle.FixedSingle;
      this.PanelToothZones.Location = new Point(674, 12);
      this.PanelToothZones.Name = "PanelToothZones";
      this.PanelToothZones.Size = new Size(129, 160);
      this.PanelToothZones.TabIndex = 3;
      this.PanelToothZones.Paint += new PaintEventHandler(this.PanelToothZones_Paint);
      this.PanelToothZones.MouseDown += new MouseEventHandler(this.PanelToothZones_MouseDown);
      this.PanelToothRoots.BorderStyle = BorderStyle.FixedSingle;
      this.PanelToothRoots.Controls.Add((Control) this.ButtonAddDelRoot);
      this.PanelToothRoots.Location = new Point(809, 12);
      this.PanelToothRoots.Name = "PanelToothRoots";
      this.PanelToothRoots.Size = new Size(129, 160);
      this.PanelToothRoots.TabIndex = 4;
      this.PanelToothRoots.Paint += new PaintEventHandler(this.PanelToothRoots_Paint);
      this.PanelToothRoots.MouseDown += new MouseEventHandler(this.PanelToothRoots_MouseDown);
      this.ButtonAddDelRoot.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      this.ButtonAddDelRoot.Location = new Point(3, 125);
      this.ButtonAddDelRoot.Name = "ButtonAddDelRoot";
      this.ButtonAddDelRoot.Size = new Size(121, 30);
      this.ButtonAddDelRoot.TabIndex = 6;
      this.ButtonAddDelRoot.Text = "Добавить/удалить";
      this.ButtonAddDelRoot.UseVisualStyleBackColor = true;
      this.ButtonAddDelRoot.MouseClick += new MouseEventHandler(this.ButtonAddDelRoot_MouseClick);
      this.ContextMenuTooth.Name = "ContextMenuTooth";
      this.ContextMenuTooth.Size = new Size(61, 4);
      this.ContextMenuTooth.ItemClicked += new ToolStripItemClickedEventHandler(this.ContextMenuTooth_ItemClicked);
      this.ContextMenuToothZone.Name = "ContextMenuToothZone";
      this.ContextMenuToothZone.Size = new Size(61, 4);
      this.ContextMenuToothZone.ItemClicked += new ToolStripItemClickedEventHandler(this.ContextMenuToothZone_ItemClicked);
      this.ContextMenuToothRoot.AutoClose = false;
      this.ContextMenuToothRoot.Name = "ContextMenuToothRoot";
      this.ContextMenuToothRoot.Size = new Size(61, 4);
      this.ContextMenuToothRoot.ItemClicked += new ToolStripItemClickedEventHandler(this.ContextMenuToothRoot_ItemClicked);
      this.ContextMenuToothRoot.MouseLeave += new EventHandler(this.ContextMenuToothRoot_MouseLeave);
      this.ContextMenuAddDelRoot.AutoClose = false;
      this.ContextMenuAddDelRoot.Name = "ContextMenuAddRoot";
      this.ContextMenuAddDelRoot.Size = new Size(61, 4);
      this.ContextMenuAddDelRoot.ItemClicked += new ToolStripItemClickedEventHandler(this.ContextMenuAddDelRoot_ItemClicked);
      this.ContextMenuAddDelRoot.MouseLeave += new EventHandler(this.ContextMenuAddDelRoot_MouseLeave);
      this.CheckBoxMilk.AutoSize = true;
      this.CheckBoxMilk.Location = new Point(674, -8);
      this.CheckBoxMilk.Name = "CheckBoxMilk";
      this.CheckBoxMilk.Size = new Size(78, 17);
      this.CheckBoxMilk.TabIndex = 6;
      this.CheckBoxMilk.Text = "Молочный";
      this.CheckBoxMilk.UseVisualStyleBackColor = true;
      this.CheckBoxMilk.CheckedChanged += new EventHandler(this.CheckBoxMilk_CheckedChanged);
      this.CheckBoxMilk.Click += new EventHandler(this.CheckBoxMilk_Click);
      this.LabelTooth.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold, GraphicsUnit.Point, (byte) 204);
      this.LabelTooth.Location = new Point(538, -14);
      this.LabelTooth.Name = "LabelTooth";
      this.LabelTooth.Size = new Size(130, 23);
      this.LabelTooth.TabIndex = 5;
      this.LabelTooth.Text = "Номер зуба";
      this.LabelTooth.TextAlign = ContentAlignment.MiddleCenter;
      this.ClientSize = new Size(954, 185);
      this.Controls.Add((Control) this.CheckBoxMilk);
      this.Controls.Add((Control) this.LabelTooth);
      this.Controls.Add((Control) this.PanelToothRoots);
      this.Controls.Add((Control) this.PanelToothZones);
      this.Controls.Add((Control) this.PanelToothBody);
      this.Controls.Add((Control) this.PanelTeeth);
      this.FormBorderStyle = FormBorderStyle.None;
      this.KeyPreview = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (FormMain);
      this.StartPosition = FormStartPosition.Manual;
      this.Text = "Карта зубов";
      this.TopMost = true;
      this.Load += new EventHandler(this.FormMain_Load);
      this.KeyDown += new KeyEventHandler(this.FormMain_KeyDown);
      this.KeyUp += new KeyEventHandler(this.FormMain_KeyUp);
      this.PanelToothRoots.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
