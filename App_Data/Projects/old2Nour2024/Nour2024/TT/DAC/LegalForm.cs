namespace Maintenance
{
  using System;
  using PX.Data;
  
  [System.SerializableAttribute()]
  public class LegalForm : PX.Data.IBqlTable
  {
    #region LegalFormID
    public abstract class legalFormID : PX.Data.IBqlField
    {
    }
    protected string _LegalFormID;
    [PXDBString(10, IsUnicode = true,IsKey=true)]
    [PXUIField(DisplayName = "LegalFormID")]
    public virtual string LegalFormID
    {
      get
      {
        return this._LegalFormID;
      }
      set
      {
        this._LegalFormID = value;
      }
    }
    #endregion
    #region Descr
    public abstract class descr : PX.Data.IBqlField
    {
    }
    protected string _Descr;
    [PXDBString(60, IsUnicode = true)]
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