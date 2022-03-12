using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MedForm
{
  public class FormMain : Form
  {
    private IContainer components = (IContainer) null;
    private FieldOps FieldOps;
    private ListBox QuickPhrases;
    private TextBox LastActiveTextBox;
    private bool HasTeethCard;
    private TeethCard.FormMain TeethForm;
    private int PrevHeight;
    private Panel FieldsPanel;
    private OpenFileDialog OpenFileDialog;
    private Panel ButtonsPanel;
    private Button BtnClose;
    private Button BtnSave;
    private Panel HeaderPanel;
    private Panel TeethPanel;
    private Button BtnMouth;
    private Panel QPPanel;

    public FormMain(bool bHasTeethCard)
    {
      this.HasTeethCard = bHasTeethCard;
      this.FieldOps = new FieldOps();
      this.QuickPhrases = new ListBox();
      this.QuickPhrases.DoubleClick += new EventHandler(this.QuickPhrase_DoubleClick);
      this.LastActiveTextBox = (TextBox) null;
      this.InitializeComponent();
      this.SetSize();
    }

    private void SetSize()
    {
      Size size = new Size(int.Parse(Program.Settings.Get("WindowWidth")), this.PrevHeight = int.Parse(Program.Settings.Get("WindowHeight")));
      Rectangle bounds = Screen.FromControl((Control) this).Bounds;
      this.Size = size;
      this.Location = new Point((bounds.Width - size.Width) / 2, (bounds.Height - size.Height) / 2);
      this.WindowState = (FormWindowState) int.Parse(Program.Settings.Get("WindowState"));
    }

    private void FormMain_Load(object sender, EventArgs e)
    {
      if (Program.Settings.Drawing.FieldWidth == 0)
        Program.Settings.Drawing.FieldWidth = this.ClientRectangle.Width / 4;
      if (!this.LoadData())
        this.Close();
      this.CreateControls();
      this.BtnMouth.Visible = this.HasTeethCard;
      if (this.HasTeethCard)
      {
        this.TeethForm = new TeethCard.FormMain();
        this.TeethForm.TopLevel = false;
        this.TeethPanel.Controls.Add((Control) this.TeethForm);
        this.TeethPanel.Visible = true;
        this.TeethForm.Show();
        this.UpdateFieldsPanel(true, false, DBUtil.InitialRecordID);
      }
      else
      {
        this.TeethPanel.Visible = false;
        this.TeethPanel.Height = 0;
      }
      this.MouseWheel += new MouseEventHandler(this.DoNothing_MouseWheel);
      this.FieldsPanel.MouseWheel += new MouseEventHandler(this.DoNothing_MouseWheel);
      this.UpdateLayout();
    }

    private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
    {
      e.Cancel = !this.SaveModified(false);
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
      this.SaveData();
    }

    private void BtnClose_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private bool LoadData()
    {
      try
      {
        this.FieldOps.LoadFields();
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show("Ошибка при загрузке данных из БД:\n" + ex.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        return false;
      }
      return true;
    }

    private bool SaveData()
    {
      try
      {
        if (this.HasTeethCard)
        {
          Program.MedCard.Save();
          if (DBUtil.RecordID == 0)
          {
            DBUtil.RecordID = DBUtil.CreateNewToothRecord(this.TeethForm.GetSelectedToothId());
            this.TeethForm.SetSelectedToothDBId(DBUtil.RecordID);
          }
        }
        this.FieldOps.SaveFields();
        this.BtnSave.Enabled = false;
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show("Ошибка при сохранении данных в БД:\n" + ex.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        return false;
      }
      return true;
    }

    private bool SaveModified(bool forceSave)
    {
      if (this.HasTeethCard && this.TeethForm != null)
      {
        this.TeethForm.SaveModifiedTooth();
        Program.MedCard.Save();
      }
      if (this.FieldOps.IsModified())
      {
        switch (forceSave ? DialogResult.Yes : MessageBox.Show("Данные были изменены.\nСохранить изменения? (Да)\nЗакрыть без сохранения? (Нет)\nПродолжить редактирование. (Отмена)", "Закрыть карту", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
        {
          case DialogResult.Cancel:
            return false;
          case DialogResult.Yes:
            if (!this.SaveData())
              return false;
            break;
        }
      }
      return true;
    }

    private void CreateControls()
    {
      this.HeaderPanel.Controls.Clear();
      Point loc = new Point(Program.Settings.Drawing.ControlXSpacing, Program.Settings.Drawing.ControlYSpacing);
      this.HeaderPanel.Top = this.ButtonsPanel.Bottom + Program.Settings.Drawing.ControlYSpacing;
      List<Control> controlList = new List<Control>();
      foreach (FieldOps.FieldInfo field in this.FieldOps.Fields)
      {
        if (field.IsHeader)
        {
          controlList.Add(this.CreateControl(this.HeaderPanel, loc, field));
          loc.Y += this.GetControlHeight(field.Type) + Program.Settings.Drawing.ControlYSpacing;
        }
      }
      this.HeaderPanel.Height = loc.Y;
      this.FieldsPanel.Top = this.HeaderPanel.Bottom + Program.Settings.Drawing.ControlYSpacing;
      this.CreateFieldControls();
      foreach (Control control in controlList)
        control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    }

    public void CreateFieldControls()
    {
      this.FieldsPanel.Controls.Clear();
      this.QPPanel.Controls.Clear();
      Point loc = new Point(Program.Settings.Drawing.ControlXSpacing, Program.Settings.Drawing.ControlYSpacing);
      int y = loc.Y;
      int num = 0;
      foreach (FieldOps.FieldInfo field in this.FieldOps.Fields)
      {
        if (!field.IsHeader)
        {
          this.CreateControl(this.FieldsPanel, loc, field);
          loc.Y += this.GetControlHeight(field.Type) + Program.Settings.Drawing.ControlYSpacing;
          ++num;
        }
      }
      if (num > 0)
      {
        int height = loc.Y - y - Program.Settings.Drawing.ControlYSpacing;
        this.QuickPhrases.Location = new Point(Program.Settings.Drawing.ControlXSpacing, y);
        this.QuickPhrases.Size = new Size(this.QPPanel.ClientRectangle.Width - Program.Settings.Drawing.ControlXSpacing * 2, height);
        this.QuickPhrases.HorizontalScrollbar = true;
        this.QuickPhrases.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        this.QPPanel.Controls.Add((Control) this.QuickPhrases);
        this.QuickPhrases.MouseWheel += new MouseEventHandler(this.DoNothing_MouseWheel);
      }
      this.BtnSave.Enabled = false;
    }

    private Control CreateControl(Panel panel, Point loc, FieldOps.FieldInfo fi)
    {
      Label label = new Label();
      label.Location = loc;
      label.Size = new Size(Program.Settings.Drawing.FieldNameWidth, this.GetControlHeight(fi.Type));
      label.Text = fi.Caption;
      label.TextAlign = ContentAlignment.MiddleLeft;
      panel.Controls.Add((Control) label);
      Control control = (Control) null;
      switch (fi.Type)
      {
        case FieldOps.FieldType.MultiText:
        case FieldOps.FieldType.SingleText:
        case FieldOps.FieldType.FileLink:
          TextBox textBox1 = new TextBox();
          textBox1.ReadOnly = fi.IsReadOnly;
          textBox1.Text = fi.Value.ToString();
          textBox1.TextChanged += new EventHandler(this.Control_ValueChanged);
          control = (Control) textBox1;
          break;
        case FieldOps.FieldType.Number:
        case FieldOps.FieldType.Decimal:
          NumericUpDown numericUpDown = new NumericUpDown();
          numericUpDown.Minimum = fi.MinValue;
          numericUpDown.Maximum = fi.MaxValue;
          numericUpDown.ReadOnly = fi.IsReadOnly;
          numericUpDown.Value = Decimal.Parse(fi.Value.ToString());
          numericUpDown.ValueChanged += new EventHandler(this.Control_ValueChanged);
          control = (Control) numericUpDown;
          break;
        case FieldOps.FieldType.Date:
        case FieldOps.FieldType.DateTime:
          DateTimePicker dateTimePicker = new DateTimePicker();
          dateTimePicker.Enabled = fi.IsReadOnly;
          dateTimePicker.Value = (DateTime) fi.Value;
          dateTimePicker.ValueChanged += new EventHandler(this.Control_ValueChanged);
          control = (Control) dateTimePicker;
          break;
        case FieldOps.FieldType.Checkbox:
          CheckBox checkBox = new CheckBox();
          checkBox.Enabled = !fi.IsReadOnly;
          checkBox.Checked = (int) fi.Value == 1;
          checkBox.CheckedChanged += new EventHandler(this.Control_ValueChanged);
          control = (Control) checkBox;
          break;
        case FieldOps.FieldType.ForeignKey:
          TextBox textBox2 = new TextBox();
          textBox2.ReadOnly = true;
          string empty = string.Empty;
          fi.FkeyValues.TryGetValue((int) fi.Value, out empty);
          textBox2.Text = empty;
          control = (Control) textBox2;
          break;
      }
      int x1 = label.Location.X + label.Width + Program.Settings.Drawing.ControlXSpacing;
      control.Location = new Point(x1, loc.Y);
      control.Size = new Size(this.GetControlWidth(fi, control.Location.X, panel.ClientRectangle.Width), this.GetControlHeight(fi.Type));
      control.Tag = (object) fi;
      control.Enter += new EventHandler(this.Control_Enter);
      control.Leave += new EventHandler(this.Control_Leave);
      control.MouseWheel += new MouseEventHandler(this.DoNothing_MouseWheel);
      switch (fi.Type)
      {
        case FieldOps.FieldType.MultiText:
          ((TextBoxBase) control).Multiline = true;
          control.KeyDown += new KeyEventHandler(this.Control_KeyDown);
          break;
        case FieldOps.FieldType.Number:
          ((NumericUpDown) control).Minimum = fi.MinValue;
          ((NumericUpDown) control).Maximum = fi.MaxValue;
          break;
        case FieldOps.FieldType.Decimal:
          ((NumericUpDown) control).DecimalPlaces = 2;
          ((NumericUpDown) control).Increment = new Decimal(1, 0, 0, false, (byte) 2);
          break;
        case FieldOps.FieldType.Date:
          ((DateTimePicker) control).Format = DateTimePickerFormat.Long;
          break;
        case FieldOps.FieldType.DateTime:
          ((DateTimePicker) control).Format = DateTimePickerFormat.Custom;
          ((DateTimePicker) control).CustomFormat = "dd.MM.yyyy hh:mm:ss";
          break;
        case FieldOps.FieldType.ForeignKey:
          control.Enabled = !fi.IsReadOnly;
          control.BackColor = SystemColors.Window;
          control.ForeColor = SystemColors.ControlText;
          break;
        case FieldOps.FieldType.FileLink:
          TextBox textBox3 = control as TextBox;
          textBox3.ReadOnly = true;
          textBox3.BackColor = SystemColors.Window;
          textBox3.ForeColor = SystemColors.HotTrack;
          textBox3.Font = new Font(textBox3.Font, FontStyle.Underline);
          textBox3.Cursor = Cursors.Hand;
          textBox3.Click += new EventHandler(this.FileLink_Click);
          Button button1 = new Button();
          Button button2 = button1;
          int x2 = textBox3.Location.X + textBox3.Width + 1;
          Point location = textBox3.Location;
          int y1 = location.Y;
          Point point1 = new Point(x2, y1);
          button2.Location = point1;
          button1.Size = new Size(Program.Settings.Drawing.ControlHeight, Program.Settings.Drawing.ControlHeight);
          button1.Text = "…";
          button1.Tag = (object) textBox3;
          button1.Enabled = !fi.IsReadOnly;
          button1.Click += new EventHandler(this.FileLinkOpen_Click);
          panel.Controls.Add((Control) button1);
          Button button3 = new Button();
          Button button4 = button3;
          location = button1.Location;
          int x3 = location.X + button1.Width + 1;
          location = button1.Location;
          int y2 = location.Y;
          Point point2 = new Point(x3, y2);
          button4.Location = point2;
          button3.Size = new Size(Program.Settings.Drawing.ControlHeight, Program.Settings.Drawing.ControlHeight);
          button3.Text = "X";
          button3.Tag = (object) textBox3;
          button3.Enabled = !fi.IsReadOnly;
          button3.Click += new EventHandler(this.FileLinkClear_Click);
          panel.Controls.Add((Control) button3);
          break;
      }
      panel.Controls.Add(control);
      return control;
    }

    private int GetControlWidth(FieldOps.FieldInfo fi, int X, int MaxWidth)
    {
      int num = fi == null || fi.IsHeader ? MaxWidth - X - Program.Settings.Drawing.ControlXSpacing : Program.Settings.Drawing.FieldWidth;
      if (fi != null && fi.Type == FieldOps.FieldType.FileLink)
        num -= Program.Settings.Drawing.ControlHeight * 2 + 2;
      return num;
    }

    private int GetControlHeight(FieldOps.FieldType type)
    {
      return type == FieldOps.FieldType.MultiText ? Program.Settings.Drawing.MultilineControlHeight : Program.Settings.Drawing.ControlHeight;
    }

    private void Control_ValueChanged(object sender, EventArgs e)
    {
      FieldOps.FieldInfo tag = (FieldOps.FieldInfo) ((Control) sender).Tag;
      bool isModified = tag.IsModified;
      tag.IsModified = true;
      switch (sender)
      {
        case TextBox _:
          if (tag.Type == FieldOps.FieldType.ForeignKey)
          {
            if (this.HasTeethCard && tag.Name == "DICTTOOTHSTATUSID")
            {
              this.TeethForm.UpdateSelectedToothStatus((int) tag.Value);
              tag.IsModified = isModified;
              break;
            }
            break;
          }
          tag.Value = (object) (sender as TextBox).Text;
          break;
        case NumericUpDown _:
          tag.Value = (object) (sender as NumericUpDown).Value;
          break;
        case CheckBox _:
          tag.Value = (object) ((sender as CheckBox).Checked ? 1 : 0);
          break;
        case DateTimePicker _:
          tag.Value = (object) (sender as DateTimePicker).Value;
          break;
      }
      this.BtnSave.Enabled = tag.IsModified;
    }

    private void Control_Enter(object sender, EventArgs e)
    {
      FieldOps.FieldInfo tag = (FieldOps.FieldInfo) ((Control) sender).Tag;
      if (sender is TextBox && tag.PhraseList != null && tag.PhraseList.Count > 0)
      {
        this.QuickPhrases.Items.Clear();
        foreach (KeyValuePair<string, List<string>> phrase in tag.PhraseList)
        {
          this.QuickPhrases.Items.Add((object) phrase.Key);
          foreach (string str in phrase.Value)
            this.QuickPhrases.Items.Add((object) ("    " + str));
        }
        this.LastActiveTextBox = sender as TextBox;
      }
      else if (sender is TextBox && tag.FkeyValues != null && tag.FkeyValues.Count > 0 && !tag.IsReadOnly)
      {
        this.QuickPhrases.Items.Clear();
        foreach (KeyValuePair<int, string> fkeyValue in tag.FkeyValues)
          this.QuickPhrases.Items.Add((object) new FormMain.ListBoxItem(fkeyValue.Value, fkeyValue.Key));
        this.LastActiveTextBox = sender as TextBox;
      }
      else
      {
        if (sender is ListBox)
          return;
        this.QuickPhrases.Items.Clear();
      }
    }

    private void Control_Leave(object sender, EventArgs e)
    {
    }

    private void FileLink_Click(object sender, EventArgs e)
    {
      string text = (sender as TextBox).Text;
      if (!(text != ""))
        return;
      try
      {
        Process.Start(Program.Settings.Get("FileLinkPath") + "\\" + text);
      }
      catch
      {
      }
    }

    private void FileLinkOpen_Click(object sender, EventArgs e)
    {
      TextBox tag = (sender as Control).Tag as TextBox;
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.DefaultExt = "*.*";
      openFileDialog.FileName = "";
      openFileDialog.ShowReadOnly = true;
      openFileDialog.RestoreDirectory = true;
      if (DialogResult.OK != openFileDialog.ShowDialog())
        return;
      int recordId = DBUtil.RecordID;
      string str1 = Program.Settings.Get("FileLinkPath");
      int num1 = 0;
      string fileName = Path.GetFileName(openFileDialog.FileName);
      string str2 = fileName;
      string str3 = "";
      int num2 = fileName.LastIndexOf(".");
      if (fileName.Length > num2)
      {
        str2 = fileName.Substring(0, num2);
        str3 = fileName.Substring(num2, fileName.Length - num2);
      }
      string str4 = str2 + "_" + recordId.ToString() + str3;
      string str5;
      object[] objArray;
      for (str5 = str1 + "\\" + str4; File.Exists(str5); str5 = string.Concat(objArray))
      {
        ++num1;
        objArray = new object[8]
        {
          (object) str1,
          (object) "\\",
          (object) str2,
          (object) "_",
          (object) recordId,
          (object) "_",
          (object) num1.ToString(),
          (object) str3
        };
      }
      string str6 = str4;
      if (num1 > 0)
        str6 = str2 + "_" + recordId.ToString() + "_" + num1.ToString() + str3;
      File.Copy(openFileDialog.FileName, str5);
      tag.Text = str6;
      this.Control_ValueChanged((object) tag, new EventArgs());
    }

    private void FileLinkClear_Click(object sender, EventArgs e)
    {
      TextBox tag = (sender as Control).Tag as TextBox;
      tag.Text = "";
      this.Control_ValueChanged((object) tag, new EventArgs());
    }

    private void QuickPhrase_DoubleClick(object sender, EventArgs e)
    {
      object selectedItem = this.QuickPhrases.SelectedItem;
      if (selectedItem == null)
        return;
      if (selectedItem is FormMain.ListBoxItem)
      {
        string str = selectedItem.ToString();
        int tag1 = (selectedItem as FormMain.ListBoxItem).GetTag();
        if (this.LastActiveTextBox == null)
          return;
        this.LastActiveTextBox.Text = str;
        FieldOps.FieldInfo tag2 = this.LastActiveTextBox.Tag as FieldOps.FieldInfo;
        if ((int) tag2.Value != tag1)
        {
          tag2.Value = (object) tag1;
          tag2.IsModified = !(tag2.Name == "DICTTOOTHSTATUSID") || tag2.IsModified;
          this.LastActiveTextBox.Tag = (object) tag2;
          this.Control_ValueChanged((object) this.LastActiveTextBox, e);
        }
      }
      else
      {
        string str1 = selectedItem.ToString();
        bool flag = str1.StartsWith(" ");
        string str2 = str1.Trim();
        if (flag && this.LastActiveTextBox != null)
        {
          if (this.LastActiveTextBox.Multiline)
          {
            int selectionStart = this.LastActiveTextBox.SelectionStart;
            if (this.LastActiveTextBox.SelectionLength == 0)
            {
              this.LastActiveTextBox.Text = this.LastActiveTextBox.Text.Insert(selectionStart, str2);
              this.LastActiveTextBox.SelectionStart = selectionStart + str2.Length;
            }
            else
              this.LastActiveTextBox.SelectedText = str2;
          }
          else
            this.LastActiveTextBox.Text = str2;
          this.Control_ValueChanged((object) this.LastActiveTextBox, e);
        }
      }
    }

    public bool UpdateFieldsPanel(bool bShow, bool bTooth, int id)
    {
      this.FieldsPanel.Visible = bShow;
      if (!this.SaveModified(true))
        return false;
      if (bShow && DBUtil.SetTableAndId(bTooth ? "TEETHMAP" : DBUtil.InitialTableName, id))
      {
        this.FieldOps.LoadFields();
        this.CreateFieldControls();
      }
      this.UpdateLayout();
      return true;
    }

    public void UpdateLayout()
    {
      Rectangle clientRectangle;
      if (this.TeethForm == null)
      {
        this.FieldsPanel.Top = this.HeaderPanel.Bottom + Program.Settings.Drawing.ControlYSpacing;
        this.FieldsPanel.Height = this.ClientRectangle.Bottom - this.FieldsPanel.Top - Program.Settings.Drawing.ControlYSpacing;
      }
      else
      {
        this.TeethPanel.Top = this.HeaderPanel.Bottom + Program.Settings.Drawing.ControlYSpacing;
        this.TeethPanel.Height = this.TeethForm.Height + 2;
        this.FieldsPanel.Top = this.TeethPanel.Bottom + Program.Settings.Drawing.ControlYSpacing;
        Panel fieldsPanel = this.FieldsPanel;
        clientRectangle = this.ClientRectangle;
        int num = clientRectangle.Bottom - this.FieldsPanel.Top - Program.Settings.Drawing.ControlYSpacing;
        fieldsPanel.Height = num;
      }
      this.FieldsPanel.Width = Program.Settings.Drawing.FieldNameWidth + Program.Settings.Drawing.FieldWidth + Program.Settings.Drawing.ControlXSpacing * 3 + 20;
      //if (this.FieldsPanel.VerticalScroll.Visible)
//        ;
      this.QPPanel.Left = this.FieldsPanel.Right + Program.Settings.Drawing.ControlXSpacing;
      Panel qpPanel = this.QPPanel;
      clientRectangle = this.ClientRectangle;
      int num1 = clientRectangle.Right - this.QPPanel.Left - Program.Settings.Drawing.ControlXSpacing - 2;
      qpPanel.Width = num1;
      this.QPPanel.Top = this.FieldsPanel.Top;
      this.QPPanel.Height = this.FieldsPanel.Height;
      this.QuickPhrases.Height = this.QPPanel.Height - 2 * Program.Settings.Drawing.ControlYSpacing;
    }

    private void BtnMouth_Click(object sender, EventArgs e)
    {
      this.TeethForm.DeselectTooth();
      this.UpdateFieldsPanel(!this.FieldsPanel.Visible, false, DBUtil.InitialRecordID);
    }

    private void FormMain_Resize(object sender, EventArgs e)
    {
      this.UpdateLayout();
    }

    private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
    {
      int windowState = (int) this.WindowState;
      int num1 = windowState != 0 ? this.RestoreBounds.Width : this.Width;
      int num2 = windowState != 0 ? this.RestoreBounds.Height : this.Height;
      Program.Settings.Set("WindowWidth", num1.ToString());
      Program.Settings.Set("WindowHeight", num2.ToString());
      int num3 = windowState == 1 ? 0 : windowState;
      Program.Settings.Set("WindowState", num3.ToString());
    }

    private void Control_KeyDown(object sender, KeyEventArgs e)
    {
      if ((Control.ModifierKeys & Keys.Control) == Keys.Control && e.KeyCode == Keys.A)
      {
        if (this.ActiveControl == null || !(this.ActiveControl is TextBox))
          return;
        (this.ActiveControl as TextBox).SelectAll();
      }
      else
      {
        if (this.TeethForm == null)
          return;
        this.TeethForm.FormMain_KeyDown(sender, e);
      }
    }

    private void FormMain_KeyUp(object sender, KeyEventArgs e)
    {
      if (this.TeethForm == null)
        return;
      this.TeethForm.FormMain_KeyUp(sender, e);
    }

    public void UpdateSelectedToothStatus(int Status, int DBId)
    {
      foreach (FieldOps.FieldInfo field in this.FieldOps.Fields)
      {
        if (field.Name == "DICTTOOTHSTATUSID")
        {
          DBUtil.RecordID = DBId;
          field.Value = (object) Status;
          if ((uint) DBId > 0U)
            field.SetValue();
          IEnumerator enumerator1 = this.FieldsPanel.Controls.GetEnumerator();
          try
          {
            while (enumerator1.MoveNext())
            {
              Control current1 = (Control) enumerator1.Current;
              if (current1.Tag == field)
              {
                using (Dictionary<int, string>.Enumerator enumerator2 = field.FkeyValues.GetEnumerator())
                {
                  while (enumerator2.MoveNext())
                  {
                    KeyValuePair<int, string> current2 = enumerator2.Current;
                    if (current2.Key == Status && current1 is TextBox)
                    {
                      (current1 as TextBox).Text = current2.Value;
                      break;
                    }
                  }
                  break;
                }
              }
            }
            break;
          }
          finally
          {
            if (enumerator1 is IDisposable disposable)
              disposable.Dispose();
          }
        }
      }
    }

    private void DoNothing_MouseWheel(object sender, MouseEventArgs e)
    {
      ((HandledMouseEventArgs) e).Handled = true;
      this.FieldsPanel.AutoScrollPosition = new Point(0, Math.Min(this.FieldsPanel.VerticalScroll.Maximum, Math.Max(this.FieldsPanel.VerticalScroll.Minimum, this.FieldsPanel.VerticalScroll.Value - e.Delta / 4)));
    }

    private void FormMain_Layout(object sender, LayoutEventArgs e)
    {
      this.UpdateLayout();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.FieldsPanel = new Panel();
      this.OpenFileDialog = new OpenFileDialog();
      this.ButtonsPanel = new Panel();
      this.BtnMouth = new Button();
      this.BtnClose = new Button();
      this.BtnSave = new Button();
      this.HeaderPanel = new Panel();
      this.TeethPanel = new Panel();
      this.QPPanel = new Panel();
      this.ButtonsPanel.SuspendLayout();
      this.SuspendLayout();
      this.FieldsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
      this.FieldsPanel.AutoScroll = true;
      this.FieldsPanel.BackColor = SystemColors.Control;
      this.FieldsPanel.BorderStyle = BorderStyle.FixedSingle;
      this.FieldsPanel.Location = new Point(12, 309);
      this.FieldsPanel.Name = "FieldsPanel";
      this.FieldsPanel.Size = new Size(220, 90);
      this.FieldsPanel.TabIndex = 2;
      this.OpenFileDialog.Filter = "Все файлы|*.*";
      this.ButtonsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.ButtonsPanel.BorderStyle = BorderStyle.FixedSingle;
      this.ButtonsPanel.Controls.Add((Control) this.BtnMouth);
      this.ButtonsPanel.Controls.Add((Control) this.BtnClose);
      this.ButtonsPanel.Controls.Add((Control) this.BtnSave);
      this.ButtonsPanel.Location = new Point(12, 12);
      this.ButtonsPanel.Name = "ButtonsPanel";
      this.ButtonsPanel.Size = new Size(444, 45);
      this.ButtonsPanel.TabIndex = 3;
      this.BtnMouth.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.BtnMouth.Location = new Point(324, 10);
      this.BtnMouth.Name = "BtnMouth";
      this.BtnMouth.Size = new Size(109, 23);
      this.BtnMouth.TabIndex = 4;
      this.BtnMouth.Text = "Полость рта";
      this.BtnMouth.UseVisualStyleBackColor = true;
      this.BtnMouth.Click += new EventHandler(this.BtnMouth_Click);
      this.BtnClose.Location = new Point(91, 10);
      this.BtnClose.Name = "BtnClose";
      this.BtnClose.Size = new Size(75, 23);
      this.BtnClose.TabIndex = 3;
      this.BtnClose.Text = "Закрыть";
      this.BtnClose.UseVisualStyleBackColor = true;
      this.BtnClose.Click += new EventHandler(this.BtnClose_Click);
      this.BtnSave.Location = new Point(10, 10);
      this.BtnSave.Name = "BtnSave";
      this.BtnSave.Size = new Size(75, 23);
      this.BtnSave.TabIndex = 2;
      this.BtnSave.Text = "Сохранить";
      this.BtnSave.UseVisualStyleBackColor = true;
      this.BtnSave.Click += new EventHandler(this.BtnSave_Click);
      this.HeaderPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.HeaderPanel.BackColor = SystemColors.Control;
      this.HeaderPanel.BorderStyle = BorderStyle.FixedSingle;
      this.HeaderPanel.Location = new Point(12, 63);
      this.HeaderPanel.Name = "HeaderPanel";
      this.HeaderPanel.Size = new Size(444, 58);
      this.HeaderPanel.TabIndex = 4;
      this.TeethPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
      this.TeethPanel.BorderStyle = BorderStyle.FixedSingle;
      this.TeethPanel.Location = new Point(12, (int) sbyte.MaxValue);
      this.TeethPanel.Name = "TeethPanel";
      this.TeethPanel.Size = new Size(444, 176);
      this.TeethPanel.TabIndex = 5;
      this.QPPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      this.QPPanel.AutoScroll = true;
      this.QPPanel.BackColor = SystemColors.Control;
      this.QPPanel.BorderStyle = BorderStyle.FixedSingle;
      this.QPPanel.Location = new Point(238, 309);
      this.QPPanel.Name = "QPPanel";
      this.QPPanel.Size = new Size(218, 90);
      this.QPPanel.TabIndex = 6;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(468, 411);
      this.Controls.Add((Control) this.QPPanel);
      this.Controls.Add((Control) this.TeethPanel);
      this.Controls.Add((Control) this.HeaderPanel);
      this.Controls.Add((Control) this.ButtonsPanel);
      this.Controls.Add((Control) this.FieldsPanel);
      this.KeyPreview = true;
      this.Name = nameof (FormMain);
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = "Медкарта";
      this.FormClosing += new FormClosingEventHandler(this.FormMain_FormClosing);
      this.FormClosed += new FormClosedEventHandler(this.FormMain_FormClosed);
      this.Load += new EventHandler(this.FormMain_Load);
      this.KeyDown += new KeyEventHandler(this.Control_KeyDown);
      this.KeyUp += new KeyEventHandler(this.FormMain_KeyUp);
      this.Layout += new LayoutEventHandler(this.FormMain_Layout);
      this.Resize += new EventHandler(this.FormMain_Resize);
      this.ButtonsPanel.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    private class ListBoxItem
    {
      private string Text;
      private int Tag;

      public ListBoxItem(string _Text, int _Tag)
      {
        this.Text = _Text;
        this.Tag = _Tag;
      }

      public override string ToString()
      {
        return this.Text;
      }

      public int GetTag()
      {
        return this.Tag;
      }
    }
  }
}
