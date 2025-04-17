namespace Maintenance
{
  using System;
  using PX.Data;
  
  [System.SerializableAttribute()]
  public class POTerms : PX.Data.IBqlTable
  {
    #region GroupID
    public abstract class groupID : PX.Data.IBqlField
    {
    }
    protected int? _GroupID;
    [PXDBInt(IsKey = true)]
    [PXDefault()]
    [PXUIField(DisplayName = "GroupID",Enabled = true)]
    public virtual int? GroupID
    {
      get
      {
        return this._GroupID;
      }
      set
      {
        this._GroupID = value;
      }
    }
    #endregion
    #region Description
    public abstract class description : PX.Data.IBqlField
    {
    }
    protected string _Description;
    [PXDBString(IsUnicode = true)]
    [PXDefault("")]
    [PXUIField(DisplayName = "Description",Enabled = true)]
    public virtual string Description
    {
      get
      {
        return this._Description;
      }
      set
      {
        this._Description = value;
      }
    }
    #endregion
  }
}