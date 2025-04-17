/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PO
{
    [PXHidden]
    [PXProjection(typeof(
        SelectFrom<POAccrualSplit>
            .InnerJoin<POReceiptLine>
                .On<POReceiptLine.iNReleased.IsEqual<True>
                .And<POAccrualSplit.FK.ReceiptLine>>
        .Where<POAccrualSplit.finPeriodID.IsLessEqual<POAccrualInquiryFilter.finPeriodID.FromCurrent.Value>>
        .AggregateTo<
			 GroupBy<POAccrualSplit.pOReceiptType>,
			 GroupBy<POAccrualSplit.pOReceiptNbr>,
             GroupBy<POAccrualSplit.pOReceiptLineNbr>,
             Sum<POAccrualSplit.accruedCost>,
             Sum<POAccrualSplit.pPVAmt>,
             Sum<POAccrualSplit.taxAccruedCost>>
         ), Persistent = false)]
    public class POReceiptLineAccrual: IBqlTable
    {
		#region POReceiptType
		[PXDBString(2, IsFixed = true, IsKey = true, BqlField = typeof(POAccrualSplit.pOReceiptType))]
		public virtual string POReceiptType { get; set; }
		public abstract class pOReceiptType : BqlString.Field<pOReceiptType> { }
		#endregion
		#region POReceiptNbr
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(POAccrualSplit.pOReceiptNbr))]
        public virtual string POReceiptNbr { get; set; }
        public abstract class pOReceiptNbr : BqlString.Field<pOReceiptNbr> { }
        #endregion
        #region POReceiptLineNbr
        [PXDBInt(IsKey = true, BqlField = typeof(POAccrualSplit.pOReceiptLineNbr))]
        public virtual int? POReceiptLineNbr { get; set; }
        public abstract class pOReceiptLineNbr : BqlInt.Field<pOReceiptLineNbr> { }
        #endregion

        #region AccruedCost
        [PXDBDecimal(4, BqlField = typeof(POAccrualSplit.accruedCost))]
        public virtual decimal? AccruedCost { get; set; }
        public abstract class accruedCost : BqlDecimal.Field<accruedCost> { }
        #endregion
        #region PPVAmt
        [PXDBDecimal(4, BqlField = typeof(POAccrualSplit.pPVAmt))]
        public virtual decimal? PPVAmt { get; set; }
        public abstract class pPVAmt : BqlDecimal.Field<pPVAmt> { }
        #endregion
        #region TaxAccruedCost
        [PXDBDecimal(4, BqlField = typeof(POAccrualSplit.taxAccruedCost))]
        public virtual decimal? TaxAccruedCost { get; set; }
        public abstract class taxAccruedCost : BqlDecimal.Field<taxAccruedCost> { }
        #endregion
    }
}
