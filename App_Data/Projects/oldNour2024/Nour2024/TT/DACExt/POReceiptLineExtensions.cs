using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects.TX;
using PX.Objects;
using System.Collections.Generic;
using System;
using MyMaintaince;
namespace PX.Objects.PO
{
  public class POReceiptLineExt : PXCacheExtension<PX.Objects.PO.POReceiptLine>
  {
      #region UsrCommercialPrice
      [PXDBDecimal]
      [PXUIField(DisplayName="Commercial Price",Enabled=true)]
      public virtual Decimal? UsrCommercialPrice { get; set; }
      public abstract class usrCommercialPrice : IBqlField { }
      #endregion

      #region UsrVendorDiscount
      [PXDBDecimal]
      [PXUIField(DisplayName="Vendor Discount")]
      [PXFormula(typeof(Sub<POReceiptLineExt.usrCommercialPrice,POReceiptLine.curyUnitCost>),
           typeof(SumCalc<POReceiptLineExt.usrCommercialPrice>))]
        public virtual Decimal? UsrVendorDiscount { get; set; }
      public abstract class usrVendorDiscount : IBqlField { }
      #endregion

      #region UsrWarehouseDesc
      [PXDBString(300,IsUnicode = true)]
      [PXUIField(DisplayName="Warehouse Desc")]
      public virtual string UsrWarehouseDesc { get; set; }
      public abstract class usrWarehouseDesc : IBqlField { }
        #endregion


        #region UsrChassis
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Chassis")]
        public virtual string UsrChassis { get; set; }
        public abstract class usrChassis : IBqlField { }
        #endregion


        #region UsrBasePrice
        [PXDBDecimal]
        [PXUIField(DisplayName = "Base Price")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrBasePrice { get; set; }
        public abstract class usrBasePrice : PX.Data.BQL.BqlDecimal.Field<usrBasePrice> { }
        #endregion

        #region UsrInsurancePrice
        [PXDBDecimal]
        [PXUIField(DisplayName = "Insurance Price")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrInsurancePrice { get; set; }
        public abstract class usrInsurancePrice : PX.Data.BQL.BqlDecimal.Field<usrInsurancePrice> { }
        #endregion

        #region UsrTechSupportPrice
        [PXDBDecimal]
        [PXUIField(DisplayName = "TechSupport Price")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrTechSupportPrice { get; set; }
        public abstract class usrTechSupportPrice : PX.Data.BQL.BqlDecimal.Field<usrTechSupportPrice> { }
        #endregion

        #region UsrAccesoriesPrice
        [PXDBDecimal]
        [PXUIField(DisplayName = "Accesories Price")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrAccesoriesPrice { get; set; }
        public abstract class usrAccesoriesPrice : PX.Data.BQL.BqlDecimal.Field<usrAccesoriesPrice> { }
        #endregion

        #region UsrGovDevelopFees
        [PXDBDecimal]
        [PXUIField(DisplayName = "Gov. Fees Price")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrGovDevelopFees { get; set; }
        public abstract class usrGovDevelopFees : PX.Data.BQL.BqlDecimal.Field<usrGovDevelopFees> { }
        #endregion
    }
}