namespace Maintenance
{
  using System;
  using PX.Data;
  
  [System.SerializableAttribute()]
  public class VendorClass2 : PX.Data.IBqlTable
  {
    #region VendorClassID
    public abstract class vendorClassID : PX.Data.IBqlField
    {
    }
    protected string _VendorClassID;
    [PXDBString(10, IsUnicode = true,IsKey=true)]
    [PXUIField(DisplayName = "VendorClassID")]
    public virtual string VendorClassID
    {
      get
      {
        return this._VendorClassID;
      }
      set
      {
        this._VendorClassID = value;
      }
    }
    #endregion
    #region Descr
    public abstract class descr : PX.Data.IBqlField
    {
    }
    protected string _Descr;
    [PXDBString(60, IsUnicode = true)]
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