using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Objects;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace PX.Objects.SO
{
    public class SOOrderExt : PXCacheExtension<PX.Objects.SO.SOOrder>
    {
        #region UsrTotalPayments
        [PXDBDecimal]
        [PXUIField(DisplayName = "Total Payments")]

        public virtual Decimal? UsrTotalPayments { get; set; }
        public abstract class usrTotalPayments : IBqlField { }
        #endregion

        #region UsrSOPaymentType
        [PXDBString(5)]
        [PXUIField(DisplayName = "SO Payment Type")]
        [PXStringList(new string[] { "CA", "IN", "II" }, new string[] { "Cash", "Installment", "Internal Installment" })]
        public virtual string UsrSOPaymentType { get; set; }
        public abstract class usrSOPaymentType : IBqlField { }
        #endregion

        #region Sales Order Reference
        [PXDBString(50)]
        [PXUIField(DisplayName = "Sales Order Reference")]
        public virtual string UsrSalesOrderReference { get; set; }
        public abstract class usrSalesOrderReference : IBqlField { }
        #endregion

        #region Purchase Order Reference
        [PXDBString(50)]
        [PXUIField(DisplayName = "Purchase Order Reference")]
        public virtual string UsrPurchaseOrderReference { get; set; }
        public abstract class usrPurchaseOrderReference : IBqlField { }
        #endregion

        #region Purchase Order Description
        [PXDBString(200)]
        [PXUIField(DisplayName = "Purchase Order Description")]
        public virtual string UsrPurchaseOrderDescription { get; set; }
        public abstract class usrPurchaseOrderDescription : IBqlField { }
        #endregion



    }
}