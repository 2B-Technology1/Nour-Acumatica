namespace Maintenance
{
  using System;
  using PX.Data;
  
  [System.SerializableAttribute()]
  public class CheckNo : PX.Data.IBqlTable
  {
    #region CashAccountID
    public abstract class cashAccountID : PX.Data.IBqlField
    {
    }
    protected int? _CashAccountID;
    [PXDBInt(IsKey=true)]
    [PXUIField(DisplayName = "CashAccountID")]
    public virtual int? CashAccountID
    {
      get
      {
        return this._CashAccountID;
      }
      set
      {
        this._CashAccountID = value;
      }
    }
        #endregion

        #region CheckNumber
    public abstract class checkNumber : PX.Data.IBqlField { }
    protected string _CheckNumber;
    [PXDBString(50, IsUnicode = true,IsKey=true)]
    [PXUIField(DisplayName = "CheckNumber")]
    public virtual string CheckNumber
    {
      get
      {
        return this._CheckNumber;
      }
      set
      {
        this._CheckNumber = value;
      }
    }
        #endregion

        #region Used
    public abstract class used : PX.Data.IBqlField { }
    protected bool? _Used;
    [PXDBBool()]
    [PXDefault()]
    [PXUIField(DisplayName = "Used")]
    public virtual bool? Used
    {
      get
      {
        return this._Used;
      }
      set
      {
        this._Used = value;
      }
    }
    #endregion
  }
}