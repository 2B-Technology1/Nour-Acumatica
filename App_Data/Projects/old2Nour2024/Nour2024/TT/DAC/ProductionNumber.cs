namespace Maintenance
{
  using System;
  using PX.Data;
  using PX.Objects.SO;
  using PX.Objects.AR;
  using PX.Objects.IN;
  using PX.Objects.CS;
  using PX.Objects.CR;
    using Nour20220913V13NEW2023;

    [System.SerializableAttribute()]
  public class ProductionNumber : PX.Data.IBqlTable
  {
    #region RefNbr
    public abstract class refNbr : PX.Data.IBqlField
    {
    }
    protected string _RefNbr;
    [PXDBString(20, IsKey = true, IsUnicode = true)]
    [PXDefault()]
    [PXUIField(DisplayName = "Ref. No.")]
    [PXSelector(typeof(ProductionNumber.refNbr),
           new Type[]
            {
                typeof(ProductionNumber.refNbr),
                typeof(ProductionNumber.customerID),
                typeof(ProductionNumber.sORefNbr)
            },
           SubstituteKey = typeof(ProductionNumber.refNbr))]
    public virtual string RefNbr
    {
      get
      {
        return this._RefNbr;
      }
      set
      {
        this._RefNbr = value;
      }
    }
    #endregion
    #region CustomerID
    public abstract class customerID : PX.Data.IBqlField
    {
    }
    protected int? _CustomerID;
    [PXDefault()]
    //[PXSOCustomerCredit(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Filterable = true)]
        /*this is a test replacement*/[CustomerActive (Visibility =PXUIVisibility.SelectorVisible,DescriptionField =typeof(Customer.acctName),Filterable =true)]
    public virtual int? CustomerID
    {
      get
      {
        return this._CustomerID;
      }
      set
      {
        this._CustomerID = value;
      }
    }
    #endregion
    #region SORefNbr
    public abstract class sORefNbr : PX.Data.IBqlField
    {
    }
    protected string _SORefNbr;
    [PXDBString(20, IsUnicode = true)]
    [PXUIField(DisplayName = "SO No.")]
    //[SO.RefNbr(typeof(Search<SOOrder.orderNbr,
    //                    Where<SOOrder.customerID, Equal<Current<ProductionNumber.customerID>>,
    //                    And<SOOrderExt.usrApprove,Equal<False>>>>), Filterable = true)]
    public virtual string SORefNbr
    {
      get
      {
        return this._SORefNbr;
      }
      set
      {
        this._SORefNbr = value;
      }
    }
    #endregion
    #region InventoryID
    public abstract class inventoryID : PX.Data.IBqlField
    {
    }
    protected int? _InventoryID;
  
        [PXDefault()]
    [SOLineInventoryAtt(typeof(InnerJoin<SOLine,On<SOLine.inventoryID,Equal<InventoryItem.inventoryID>>>),typeof(Where<SOLine.orderNbr,Equal<Current<ProductionNumber.sORefNbr>>>),Filterable=true)]
  //  [SOLineInventoryItem(typeof(InnerJoin<SOLine,On<SOLine.inventoryID,Equal<InventoryItem.inventoryID>>>), Filterable = true)]
    public virtual int? InventoryID
    {
      get
      {
        return this._InventoryID;
      }
      set
      {
        this._InventoryID = value;
      }
    }
    #endregion
    #region Quantity
    public abstract class quantity : PX.Data.IBqlField
    {
    }
    protected decimal? _Quantity;
    [PXDBDecimal(6)]
    [PXUIField(DisplayName = "Quantity")]
    public virtual decimal? Quantity
    {
      get
      {
        return this._Quantity;
      }
      set
      {
        this._Quantity = value;
      }
    }
    #endregion
    #region Date
    public abstract class date : PX.Data.IBqlField
    {
    }
    protected DateTime? _Date;
    [PXDBDate()]
    [PXUIField(DisplayName = "Date")]
        [PXDefault(typeof(AccessInfo.businessDate))]
    public virtual DateTime? Date
    {
      get
      {
        return this._Date;
      }
      set
      {
        this._Date = value;
      }
    }
    #endregion
    #region Descr
    public abstract class descr : PX.Data.IBqlField
    {
    }
    protected string _Descr;
    [PXDBString(1000, IsUnicode = true)]
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
    #region RevisionID
    public abstract class revisionID : PX.Data.IBqlField
    {
    }
    protected string _RevisionID;
    [PXDBString(50, IsUnicode = true)]
    [PXUIField(DisplayName = "RevisionID")]
        [PXSelector(typeof(Search<INKitSpecHdr.revisionID,
                        Where<INKitSpecHdr.kitInventoryID, Equal<Current<ProductionNumber.inventoryID>>>>))]
    public virtual string RevisionID
    {
      get
      {
        return this._RevisionID;
      }
      set
      {
        this._RevisionID = value;
      }
    }
    #endregion
  }
}