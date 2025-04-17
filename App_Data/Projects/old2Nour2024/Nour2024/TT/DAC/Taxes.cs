namespace Maintenance
{
  using System;
  using PX.Data;
  
  [System.SerializableAttribute()]
  public class Taxes : PX.Data.IBqlTable
  {
    #region TaxesID
    public abstract class taxesID : PX.Data.IBqlField
    {
    }
    protected string _TaxesID;
    [PXDBString(20, IsUnicode = true, IsKey=true)]
    [PXDefault("")]
    [PXUIField(DisplayName = "TaxesID")]
    public virtual string TaxesID
    {
      get
      {
        return this._TaxesID;
      }
      set
      {
        this._TaxesID = value;
      }
    }
    #endregion
    #region Descr
    public abstract class descr : PX.Data.IBqlField
    {
    }
    protected string _Descr;
    [PXDBString(100, IsUnicode = true)]
    [PXUIField(DisplayName = "Descr")]
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