namespace PX.Objects.PO
{
  using System;
  using PX.Data;
    using PX.Objects.IN;
    using PX.Objects.CS;

  
  [System.SerializableAttribute()]
     public class POReceiptSequence : PX.Data.IBqlTable
  {
    #region ReceiptType
    public abstract class receiptType : PX.Data.IBqlField
    {
    }
    protected string _ReceiptType;
    [PXDBString(2, IsFixed = true,IsKey=true)]
    [PXUIField(DisplayName = "Receipt Type")]
        [POReceiptType.List()]
    public virtual string ReceiptType
    {
      get
      {
        return this._ReceiptType;
      }
      set
      {
        this._ReceiptType = value;
      }
    }
    #endregion
    #region SiteID
    public abstract class siteID : PX.Data.IBqlField
    {
    }
    protected Int32? _SiteID;
 
    [PXUIField(DisplayName = "SiteID")]
        [IN.Site(DisplayName = "Warehouse ID", DescriptionField = typeof(INSite.descr), Visibility = PXUIVisibility.SelectorVisible,IsKey=true)]
    public virtual Int32? SiteID
    {
      get
      {
        return this._SiteID;
      }
      set
      {
        this._SiteID = value;
      }
    }
    #endregion
    #region Sequence
    public abstract class sequence : PX.Data.IBqlField
    {
    }
    protected string _Sequence;
    [PXDBString(50, IsUnicode = true)]
    [PXUIField(DisplayName = "Sequence")]
        [PXSelector(typeof(Numbering.numberingID))]
    public virtual string Sequence
    {
      get
      {
        return this._Sequence;
      }
      set
      {
        this._Sequence = value;
      }
    }
    #endregion
  }
}