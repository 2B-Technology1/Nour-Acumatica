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
using PX.Objects.AP;
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
            GroupBy<POAccrualSplit.aPDocType>,
            GroupBy<POAccrualSplit.aPRefNbr>,
            GroupBy<POAccrualSplit.aPLineNbr>,
            Sum<POAccrualSplit.accruedCost>,
            Sum<POAccrualSplit.pPVAmt>,
            Sum<POAccrualSplit.taxAccruedCost>>
        ), Persistent = false)]
    public class APTranAccrual : IBqlTable
    {
		#region APDocType
		[APDocType.List]
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(POAccrualSplit.aPDocType))]
		public virtual string APDocType { get; set; }
		public abstract class aPDocType : BqlString.Field<aPDocType> { }
		#endregion

		#region APRefNbr
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(POAccrualSplit.aPRefNbr))]
		public virtual string APRefNbr { get; set; }
        public abstract class aPRefNbr : BqlString.Field<aPRefNbr> { }
        #endregion

        #region APLineNbr
		[PXDBInt(IsKey = true, BqlField = typeof(POAccrualSplit.aPLineNbr))]
		public virtual int? APLineNbr { get; set; }
        public abstract class aPLineNbr : BqlInt.Field<aPLineNbr> { }
		#endregion

		#region AccruedCost
		public abstract class accruedCost : BqlDecimal.Field<accruedCost> { }
		[PXDBDecimal(4, BqlField = typeof(POAccrualSplit.accruedCost))]
		public virtual decimal? AccruedCost { get; set; }
		#endregion

		#region PPVAmt
		public abstract class pPVAmt : BqlDecimal.Field<pPVAmt> { }
		[PXDBDecimal(4, BqlField = typeof(POAccrualSplit.pPVAmt))]
		public virtual decimal? PPVAmt { get; set; }
        #endregion

        #region TaxAccruedCost
        [PXDBDecimal(4, BqlField = typeof(POAccrualSplit.taxAccruedCost))]
        public virtual decimal? TaxAccruedCost { get; set; }
        public abstract class taxAccruedCost : BqlDecimal.Field<taxAccruedCost> { }
        #endregion
    }
}
