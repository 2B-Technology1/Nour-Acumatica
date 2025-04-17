namespace Maintenance
{
  using System;
  using PX.Data;
  
  [System.SerializableAttribute()]
  public class BRBank : PX.Data.IBqlTable
  {
    #region BRID
    public abstract class bRID : PX.Data.IBqlField
    {
    }
    protected string _BRID;
    [PXDBString(15, IsKey = true, IsUnicode = true)]
    [PXDefault()]
    [PXUIField(DisplayName = "BRID")]
    public virtual string BRID
    {
      get
      {
        return this._BRID;
      }
      set
      {
        this._BRID = value;
      }
    }
    #endregion
   
    #region BRName
    public abstract class bRName : PX.Data.IBqlField
    {
    }
    protected string _BRName;
    [PXDBString(100, IsUnicode = true)]
    [PXUIField(DisplayName = "BRName")]
    public virtual string BRName
    {
      get
      {
        return this._BRName;
      }
      set
      {
        this._BRName = value;
      }
    }
    #endregion
  }
}