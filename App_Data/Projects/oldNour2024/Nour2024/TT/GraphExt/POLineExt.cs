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

namespace PX.Objects.PO
{
    public  class POLineExt : PXCacheExtension<PX.Objects.PO.POLine>
    {
        public static bool IsActive()
        {
            return true;
        }

        #region UsrCommercialPrice
        [PXDBDecimal]
        [PXUIField(DisplayName = "Commercial Price")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UsrCommercialPrice { get; set; }
        public abstract class usrCommercialPrice : IBqlField { }
        #endregion

        #region UsrVendorDiscount
        [PXDBDecimal]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Vendor Discount")]
        [PXFormula(typeof(Sub<POLineExt.usrCommercialPrice, POLine.curyUnitCost>),
       typeof(SumCalc<POLineExt.usrCommercialPrice>))]
        public virtual decimal? UsrVendorDiscount { get; set; }
        public abstract class usrVendorDiscount : IBqlField { }
        #endregion

        #region UsrWarehouseDesc
        [PXDBString(300, IsUnicode = true)]
        [PXUIField(DisplayName = "Warehouse Desc.")]
        public virtual string UsrWarehouseDesc { get; set; }
        public abstract class usrWarehouseDesc : IBqlField { }
        #endregion
    }
}