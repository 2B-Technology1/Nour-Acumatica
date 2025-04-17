namespace Maintenance
{
  using System;
  using PX.Data;
  using PX.Objects.SO;
  
  [System.SerializableAttribute()]
  public class LCDates : PX.Data.IBqlTable
  {
    #region LCNbr
    public abstract class lCNbr : PX.Data.IBqlField
    {
    }
    protected string _LCNbr;
    [PXDBString(15, IsUnicode = true,IsKey=true)]
    [PXDBDefault(typeof(LCOrder.lCNbr))]
    [PXParent(typeof(Select<LCOrder,
                      Where<LCOrder.lCNbr,
                          Equal<Current<LCDates.lCNbr>>>>))]
    public virtual string LCNbr
    {
      get
      {
        return this._LCNbr;
      }
      set
      {
        this._LCNbr = value;
      }
    }
    #endregion
    #region FromDate
    public abstract class fromDate : PX.Data.IBqlField
    {
    }
    protected DateTime? _FromDate;
        [PXDBDate(IsKey = true)]
    [PXUIField(DisplayName = "FromDate")]
    public virtual DateTime? FromDate
    {
      get
      {
        return this._FromDate;
      }
      set
      {
        this._FromDate = value;
      }
    }
    #endregion
    #region ToDate
    public abstract class toDate : PX.Data.IBqlField
    {
    }
    protected DateTime? _ToDate;
        [PXDBDate(IsKey = true)]
    [PXUIField(DisplayName = "ToDate")]
    public virtual DateTime? ToDate
    {
      get
      {
        return this._ToDate;
      }
      set
      {
        this._ToDate = value;
      }
    }
    #endregion
  }
}