namespace Maintenance
{
  using System;
  using PX.Data;
  
  [System.SerializableAttribute()]
  public class CustomerActivity : PX.Data.IBqlTable
  {
    #region CustomerActivityID
    public abstract class customerActivityID : PX.Data.IBqlField
    {
    }
    protected string _CustomerActivityID;
        [PXDBString(20, IsUnicode = true, IsKey = true)]
    [PXDefault()]
    [PXUIField(DisplayName = "CustomerActivityID")]
    public virtual string CustomerActivityID
    {
      get
      {
        return this._CustomerActivityID;
      }
      set
      {
        this._CustomerActivityID = value;
      }
    }
    #endregion
    #region Descr
    public abstract class descr : PX.Data.IBqlField
    {
    }
    protected string _Descr;
    [PXDBString(100, IsUnicode = true)]
    [PXUIField(DisplayName = "Description")]
    public virtual string Descr
    {
      get
      {
        return this._Descr;
      }
      set
      {
        this._Descr = value;
      }
    }
    #endregion
  }
}