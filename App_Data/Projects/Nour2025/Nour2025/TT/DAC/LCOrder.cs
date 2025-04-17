namespace Maintenance
{
  using System;
  using PX.Data;
    
    using PX.Objects.GL;
    using PX.Objects.CA;
    using PX.Objects.AR;
    using PX.Objects.CS;
  
  [System.SerializableAttribute()]
  public class LCOrder : PX.Data.IBqlTable
  {
    #region LCNbr
    public abstract class lCNbr : PX.Data.IBqlField
    {
    }
    protected string _LCNbr;
    [PXDBString(10, IsUnicode = true,IsKey=true)]
    [PXUIField(DisplayName = "LCNbr")]
        [PXSelector(typeof(lCNbr),
            new Type[]
            {
                typeof(lCNbr), 
                typeof(type),
                typeof(startDate),
                typeof(endDate),
            })]
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
    #region Type
    public abstract class type : PX.Data.IBqlField
    {
    }
    protected string _Type;
    [PXDBString(50, IsUnicode = true)]
    [PXUIField(DisplayName = "Type")]
        [PXStringList(new string[] { "LC", "LG" }, new string[] { "LC", "LG" })]
    public virtual string Type
    {
      get
      {
        return this._Type;
      }
      set
      {
        this._Type = value;
      }
    }
    #endregion
    #region Status
    public abstract class status : PX.Data.IBqlField
    {
    }
    protected string _Status;
    [PXDBString(50, IsUnicode = true)]
    [PXUIField(DisplayName = "Status")]
        [PXStringList(new string[] { "Open", "Closed" }, new string[] { "Open", "Closed" })]
    public virtual string Status
    {
      get
      {
        return this._Status;
      }
      set
      {
        this._Status = value;
      }
    }
    #endregion
    #region CashAccountID
    public abstract class cashAccountID : PX.Data.IBqlField
    {
    }
    protected int? _CashAccountID;
    [PXDBInt()]
    [PXUIField(DisplayName = "Cash Account")]
        [PXSelector(typeof(CashAccount.cashAccountID),
            new Type[]
            {
                typeof(CashAccount.cashAccountCD),
                typeof(CashAccount.descr),
                typeof(CashAccount.curyID)
            },
            SubstituteKey = typeof(CashAccount.cashAccountCD),
            DescriptionField = typeof(CashAccount.descr))]
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
    #region GuranteeType
    public abstract class guranteeType : PX.Data.IBqlField
    {
    }
    protected string _GuranteeType;
    [PXDBString(50, IsUnicode = true)]
    [PXUIField(DisplayName = "Gurantee Type",Enabled=false)]
        [PXStringList(new string[] { "Primary", "Final" }, new string[] { "Primary", "Final" })]
    public virtual string GuranteeType
    {
      get
      {
        return this._GuranteeType;
      }
      set
      {
        this._GuranteeType = value;
      }
    }
    #endregion
    #region LCAmount
    public abstract class lCAmount : PX.Data.IBqlField
    {
    }
    protected decimal? _LCAmount;
    [PXDBDecimal(4)]
    [PXUIField(DisplayName = "LC Amount")]
    public virtual decimal? LCAmount
    {
      get
      {
        return this._LCAmount;
      }
      set
      {
        this._LCAmount = value;
      }
    }
    #endregion
    #region Currancy
    public abstract class currancy : PX.Data.IBqlField
    {
    }
    protected string _Currancy;
    [PXDBString(5, IsUnicode = true)]
    [PXUIField(DisplayName = "Currency")]
    public virtual string Currancy
    {
      get
      {
        return this._Currancy;
      }
      set
      {
        this._Currancy = value;
      }
    }
    #endregion
    #region StartDate
    public abstract class startDate : PX.Data.IBqlField
    {
    }
    protected DateTime? _StartDate;
    [PXDBDate()]
    [PXUIField(DisplayName = "Start Date")]
    public virtual DateTime? StartDate
    {
      get
      {
        return this._StartDate;
      }
      set
      {
        this._StartDate = value;
      }
    }
    #endregion
    #region EndDate
    public abstract class endDate : PX.Data.IBqlField
    {
    }
    protected DateTime? _EndDate;
    [PXDBDate()]
    [PXUIField(DisplayName = "End Date")]
    public virtual DateTime? EndDate
    {
      get
      {
        return this._EndDate;
      }
      set
      {
        this._EndDate = value;
      }
    }
    #endregion
    #region CompanyMargin
    public abstract class companyMargin : PX.Data.IBqlField
    {
    }
    protected decimal? _CompanyMargin;
    [PXDBDecimal(4)]
    [PXUIField(DisplayName = "Company Margin")]
    public virtual decimal? CompanyMargin
    {
      get
      {
        return this._CompanyMargin;
      }
      set
      {
        this._CompanyMargin = value;
      }
    }
    #endregion
  }
}