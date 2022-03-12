namespace TeethCard
{
  internal class StatusObject
  {
    public int Status;
    private int InitialStatus;
    public bool Modified;

    public StatusObject()
    {
      this.Status = 0;
      this.InitialStatus = 0;
      this.Modified = false;
    }

    public void SetStatus(int newStatus)
    {
      if (newStatus == this.Status)
        return;
      if (!this.Modified)
      {
        this.InitialStatus = this.Status;
        this.Modified = true;
      }
      else if (newStatus == this.InitialStatus)
        this.Modified = false;
      this.Status = newStatus;
    }

    public bool IsModified()
    {
      return this.Modified;
    }

    public virtual StatusObject.SaveAction GetSaveAction()
    {
      if (!this.Modified)
        return StatusObject.SaveAction.None;
      return this.InitialStatus == 0 ? StatusObject.SaveAction.Add : (this.Status == 0 ? StatusObject.SaveAction.Delete : StatusObject.SaveAction.Update);
    }

    public void SetUnmodified()
    {
      this.InitialStatus = this.Status;
      this.Modified = false;
    }

    public enum SaveAction
    {
      None,
      Add,
      Update,
      Delete,
    }
  }
}
