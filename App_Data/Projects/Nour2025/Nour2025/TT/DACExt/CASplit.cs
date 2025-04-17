using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects;
using System.Collections.Generic;
using System;


namespace PX.Objects.CA
{
  public class CASplitEx : PXCacheExtension<PX.Objects.CA.CASplit>
  {
      #region UsrSubDescr
      [PXDBString(300, IsKey=false, BqlTable=typeof(PX.Objects.CA.CASplit), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Sub Account Description")]
      public virtual string UsrSubDescr { get; set; }
      public abstract class usrSubDescr : IBqlField { }
      #endregion

      #region UsrVendorID
      public abstract class usrVendorID : PX.Data.IBqlField
      {
      }
      protected string _UsrVendorID;
      //[PXDBString(50)]
      [PXDBString(50, IsKey=false, BqlTable=typeof(PX.Objects.CA.CASplit), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Vendor ID")]
      //[PXDimensionSelector("BIZACCT", typeof(AP.Vendor.bAccountID), typeof(AP.Vendor.acctCD), DescriptionField = typeof(AP.Vendor.acctName))]
      //[PXSelector((typeof(AP.Vendor.acctCD)),
      //      new Type[]
      //      {
      //          typeof(AP.Vendor.acctCD),
      //          typeof(AP.Vendor.acctName)
      //      })]

      [PXSelector(typeof(PX.Objects.CR.BAccount.acctCD),
            new Type[]
            {
                typeof(PX.Objects.CR.BAccount.acctCD),
                typeof(PX.Objects.CR.BAccount.acctName)
            })]
           // DescriptionField = typeof(PX.Objects.CR.BAccount.acctName), SubstituteKey = typeof(PX.Objects.CR.BAccount.acctCD))]
      //[PXSelector(typeof(AP.Vendor.bAccountID),
      //          typeof(AP.Vendor.acctCD),
      //          typeof(AP.Vendor.acctName),
      // DescriptionField = typeof(AP.Vendor.acctName), SubstituteKey = typeof(AP.Vendor.acctCD))]

      public virtual string UsrVendorID
      {
          get
          {
              return this._UsrVendorID;
          }
          set
          {
              this._UsrVendorID = value;
          }
      }
      #endregion

      
      
     
     
     

      #region UsrCommercialRecord
      [PXDBString(IsKey=false, BqlTable=typeof(PX.Objects.CA.CASplit), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Commercial Record")]
      public virtual string UsrCommercialRecord { get; set; }
      public abstract class usrCommercialRecord : IBqlField { }
      #endregion

      #region UsrTaxNo
      [PXDBString(IsKey=false, BqlTable=typeof(PX.Objects.CA.CASplit), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Invoice Nbr")]
      public virtual string UsrTaxNo { get; set; }
      public abstract class usrTaxNo : IBqlField { }
      #endregion

      #region UsrTaxFile
      [PXDBString(IsKey=false, BqlTable=typeof(PX.Objects.CA.CASplit), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Tax File")]
      public virtual string UsrTaxFile { get; set; }
      public abstract class usrTaxFile : IBqlField { }
      #endregion

      #region UsrRegistrationNo
      [PXDBString(IsKey=false, BqlTable=typeof(PX.Objects.CA.CASplit), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Registration No")]
      public virtual string UsrRegistrationNo { get; set; }
      public abstract class usrRegistrationNo : IBqlField { }
      #endregion

      #region UsrTaxesID
      [PXDBString(IsKey=false, BqlTable=typeof(PX.Objects.CA.CASplit), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Address")]
      public virtual string UsrTaxesID { get; set; }
      public abstract class usrTaxesID : IBqlField { }
      #endregion

      #region UsrVendorName
      [PXDBString(50, IsKey=false, BqlTable=typeof(PX.Objects.CA.CASplit), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Vendor Name")]
      public virtual string UsrVendorName { get; set; }
      public abstract class usrVendorName : IBqlField { }
      #endregion

      #region UsrNetValue
      [PXDBString(IsKey=false, BqlTable=typeof(PX.Objects.CA.CASplit), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Net Value")]
      public virtual string UsrNetValue { get; set; }
      public abstract class usrNetValue : IBqlField { }
      #endregion

      #region UsrTaxType
      [PXDBString(60, IsKey=false, BqlTable=typeof(PX.Objects.CA.CASplit), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Tax Type")]
      [PXStringList(new string[] { ".5", "1", "2", "3", "5", "13", "14" }, new string[] { ".5%", "1%", "2%", "3%", "5%", "13%", "14%" })] //updated to include 1%,3%
      public virtual string UsrTaxType { get; set; }
      public abstract class usrTaxType : IBqlField { }
      #endregion

      #region UsrType
      [PXDBString(60, IsKey=false, BqlTable=typeof(PX.Objects.CA.CASplit), IsFixed=false, IsUnicode=true)]
      [PXUIField(DisplayName="Type")]
      [PXStringList(new string[] {"Supplies","Services","Free professions" , "VatTax"},new string[] {"Supplies","Services","Free professions", "VatTax"})]
      public virtual string UsrType { get; set; }
      public abstract class usrType : IBqlField { }
      #endregion
  }
}