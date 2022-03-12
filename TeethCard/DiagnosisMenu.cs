using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TeethCard
{
  public class DiagnosisMenu : Form
  {
    private int MAX_DIAGNOSIS = 2;
    private bool SaveChanges_ = false;
    private DiagnosisMenu.Selection Result_ = new DiagnosisMenu.Selection();
    private bool WPCHooked_ = false;
    private IContainer components = (IContainer) null;
    private Dictionary<KeyValuePair<DiagnosisMenu.MenuType, int>, CheckBox> CheckBoxMap;
    private DiagnosisMenu.Selection UnmodifiedSel_;
    private const int WM_WINDOWPOSCHANGING = 70;
    private const int SWP_NOSIZE = 1;
    private const int SWP_NOMOVE = 2;
    private const int SWP_NOACTIVATE = 16;
    private SplitContainer splitContainer;
    private Label label1;
    private Label label2;

    public DiagnosisMenu()
    {
      this.MAX_DIAGNOSIS = Config.MaxDiagnosis;
      this.CheckBoxMap = new Dictionary<KeyValuePair<DiagnosisMenu.MenuType, int>, CheckBox>();
      this.InitializeComponent();
    }

    public bool Show(Point screenLocation, DiagnosisMenu.Selection sel)
    {
      this.SetCheckList(DiagnosisMenu.MenuType.Diagnosis, sel.DiagnosisList);
      this.SetCheckList(DiagnosisMenu.MenuType.Status, sel.StatusList);
      this.UnmodifiedSel_ = sel;
      this.SaveChanges_ = false;
      this.WPCHooked_ = true;
      this.Location = screenLocation;
      int num = (int) this.ShowDialog();
      return this.SaveChanges_;
    }

    public DiagnosisMenu.Selection GetResult()
    {
      return this.Result_;
    }

    public void MakeMenu()
    {
      int num1 = 0;
      foreach (KeyValuePair<int, Dictionaries.DictToothDiagnosisRecord> dictToothDiagnosi in Dictionaries.DictToothDiagnosis)
      {
        int num2 = this.MakeMenuItem(DiagnosisMenu.MenuType.Diagnosis, new KeyValuePair<int, string>(dictToothDiagnosi.Key, dictToothDiagnosi.Value.Name));
        if (num2 > num1)
          num1 = num2;
      }
      int num3 = 0;
      foreach (KeyValuePair<int, Dictionaries.DictToothStatusRecordV2> keyValuePair in Dictionaries.DictToothStatusV2)
      {
        int num2 = this.MakeMenuItem(DiagnosisMenu.MenuType.Status, new KeyValuePair<int, string>(keyValuePair.Key, keyValuePair.Value.Name));
        if (num2 > num3)
          num3 = num2;
      }
      this.ClientSize = new Size(num1 + this.splitContainer.SplitterWidth + num3, Math.Max(DiagnosisMenu.GetPanelControlsHeight(this.splitContainer.Panel1), DiagnosisMenu.GetPanelControlsHeight(this.splitContainer.Panel2)));
      this.splitContainer.SplitterDistance = num1;
    }

    private int MakeMenuItem(DiagnosisMenu.MenuType itemType, KeyValuePair<int, string> data)
    {
      CheckBox checkBox = new CheckBox();
      SplitterPanel panel = itemType == DiagnosisMenu.MenuType.Diagnosis ? this.splitContainer.Panel1 : this.splitContainer.Panel2;
      checkBox.Tag = (object) data.Key;
      checkBox.Text = data.Value;
      checkBox.AutoSize = true;
      checkBox.Left = 0;
      checkBox.Top = DiagnosisMenu.GetPanelControlsHeight(panel);
      checkBox.Click += new EventHandler(this.ItemClicked);
      this.CheckBoxMap.Add(new KeyValuePair<DiagnosisMenu.MenuType, int>(itemType, data.Key), checkBox);
      panel.Controls.Add((Control) checkBox);
      return checkBox.Width;
    }

    private static int GetPanelControlsHeight(SplitterPanel panel)
    {
      return panel.Controls.Count > 0 ? panel.Controls[panel.Controls.Count - 1].Bottom + 1 : 0;
    }

    private int GetCheckCount(DiagnosisMenu.MenuType menuType)
    {
      int num = 0;
      foreach (KeyValuePair<KeyValuePair<DiagnosisMenu.MenuType, int>, CheckBox> checkBox in this.CheckBoxMap)
        num += checkBox.Key.Key != menuType || !checkBox.Value.Checked ? 0 : 1;
      return num;
    }

    private string GetCheckList(DiagnosisMenu.MenuType menuType)
    {
      string empty = string.Empty;
      foreach (KeyValuePair<KeyValuePair<DiagnosisMenu.MenuType, int>, CheckBox> checkBox in this.CheckBoxMap)
      {
        if (checkBox.Key.Key == menuType && checkBox.Value.Checked)
        {
          string str = checkBox.Value.Tag.ToString();
          empty += empty != string.Empty ? "," + str : str;
        }
      }
      return empty;
    }

    private void SetCheckList(DiagnosisMenu.MenuType menuType, string idList)
    {
      HashSet<int> intCsvAsSet = Utils.ParseIntCSVAsSet(idList);
      foreach (KeyValuePair<KeyValuePair<DiagnosisMenu.MenuType, int>, CheckBox> checkBox in this.CheckBoxMap)
        checkBox.Value.Checked = checkBox.Key.Key == menuType ? intCsvAsSet.Contains((int) checkBox.Value.Tag) : checkBox.Value.Checked;
    }

    private void ItemClicked(object sender, EventArgs e)
    {
      if (this.GetCheckCount(DiagnosisMenu.MenuType.Diagnosis) <= this.MAX_DIAGNOSIS)
        return;
      (sender as CheckBox).Checked = false;
      int num = (int) MessageBox.Show("Можно выбирать не более " + this.MAX_DIAGNOSIS.ToString() + " диагнозов", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }

    private void DiagnosisMenu_FormClosing(object sender, FormClosingEventArgs e)
    {
      this.WPCHooked_ = false;
      this.Result_.DiagnosisList = this.GetCheckList(DiagnosisMenu.MenuType.Diagnosis);
      this.Result_.StatusList = this.GetCheckList(DiagnosisMenu.MenuType.Status);
      if (this.Result_.EqualTo(this.UnmodifiedSel_))
        return;
      this.SaveChanges_ = true;
    }

    protected override void WndProc(ref Message m)
    {
      base.WndProc(ref m);
      if (m.Msg != 70 || !this.WPCHooked_ || ((DiagnosisMenu.WINDOWPOS) Marshal.PtrToStructure(m.LParam, typeof (DiagnosisMenu.WINDOWPOS))).flags != 19U)
        return;
      this.Close();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.splitContainer = new SplitContainer();
      this.label1 = new Label();
      this.label2 = new Label();
      this.splitContainer.BeginInit();
      this.splitContainer.Panel1.SuspendLayout();
      this.splitContainer.Panel2.SuspendLayout();
      this.splitContainer.SuspendLayout();
      this.SuspendLayout();
      this.splitContainer.Dock = DockStyle.Fill;
      this.splitContainer.IsSplitterFixed = true;
      this.splitContainer.Location = new Point(0, 0);
      this.splitContainer.Name = "splitContainer";
      this.splitContainer.Panel1.BackColor = SystemColors.ControlLight;
      this.splitContainer.Panel1.Controls.Add((Control) this.label1);
      this.splitContainer.Panel2.BackColor = SystemColors.ControlLight;
      this.splitContainer.Panel2.Controls.Add((Control) this.label2);
      this.splitContainer.Size = new Size(284, 262);
      this.splitContainer.SplitterDistance = 135;
      this.splitContainer.TabIndex = 1;
      this.label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.label1.BackColor = Color.Blue;
      this.label1.ForeColor = Color.White;
      this.label1.Location = new Point(0, 0);
      this.label1.Name = "label1";
      this.label1.Size = new Size(134, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "Диагнозы";
      this.label1.TextAlign = ContentAlignment.MiddleCenter;
      this.label2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.label2.BackColor = Color.Blue;
      this.label2.ForeColor = Color.White;
      this.label2.Location = new Point(-1, 0);
      this.label2.Name = "label2";
      this.label2.Size = new Size(145, 20);
      this.label2.TabIndex = 1;
      this.label2.Text = "Статусы";
      this.label2.TextAlign = ContentAlignment.MiddleCenter;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(284, 262);
      this.Controls.Add((Control) this.splitContainer);
      this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
      this.Name = nameof (DiagnosisMenu);
      this.StartPosition = FormStartPosition.Manual;
      this.Text = "Диагнозы и статусы";
      this.FormClosing += new FormClosingEventHandler(this.DiagnosisMenu_FormClosing);
      this.splitContainer.Panel1.ResumeLayout(false);
      this.splitContainer.Panel2.ResumeLayout(false);
      this.splitContainer.EndInit();
      this.splitContainer.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    private enum MenuType
    {
      Diagnosis,
      Status,
    }

    public class Selection
    {
      public string DiagnosisList;
      public string StatusList;

      public bool EqualTo(DiagnosisMenu.Selection other)
      {
        return this.DiagnosisList.CompareTo(other.DiagnosisList) == 0 && this.StatusList.CompareTo(other.StatusList) == 0;
      }
    }

    public struct WINDOWPOS
    {
      public IntPtr hwnd;
      public IntPtr hwndInsertAfter;
      public int x;
      public int y;
      public int cx;
      public int cy;
      public uint flags;
    }
  }
}
