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
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using Messages = PX.Objects.CN.Subcontracts.AP.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.AP.CacheExtensions
{
    public sealed class PoOrderRsExt : PXCacheExtension<POOrderRS>
    {
	    public static bool IsActive()
	    {
		    return PXAccess.FeatureInstalled<FeaturesSet.construction>();
	    }

		public abstract class subcontractNbr : BqlString.Field<subcontractNbr>
	    {
	    }

		[PXDBString(BqlField = typeof(POOrder.orderNbr))]
        [PXUIField(DisplayName = Messages.Subcontract.SubcontractNumber)]
        public string SubcontractNbr
        {
            get;
            set;
        }

		public abstract class subcontractTotal : BqlDecimal.Field<subcontractTotal>
		{
		}

		[PXDBCurrency(typeof(POOrder.curyInfoID), typeof(POOrder.orderTotal), BqlField = typeof(POOrder.curyOrderTotal))]
        [PXUIField(DisplayName = Messages.Subcontract.SubcontractTotal, Enabled = false)]
        public decimal? SubcontractTotal
        {
            get;
            set;
        }

		[Obsolete]
        public abstract class projectCD : BqlString.Field<subcontractNbr>
        {
        }

		[Obsolete]
		[PXString]
        [PXUIField(DisplayName = Messages.Subcontract.Project)]
        public string ProjectCD
        {
            get;
            set;
        }

        #region SubcontractBilledQty
        public abstract class subcontractBilledQty : BqlDecimal.Field<subcontractBilledQty> { }
        [PXQuantity]
        [PXFormula(typeof(Sub<POOrder.orderQty, POOrder.unbilledOrderQty>))]
        [PXUIField(DisplayName = Messages.Subcontract.SubcontractBilledQty, Enabled = false)]
        public decimal? SubcontractBilledQty
        {
            get;
            set;
        }
        #endregion

        #region CurySubcontractBilledTotal
        public abstract class curySubcontractBilledTotal : BqlDecimal.Field<curySubcontractBilledTotal> { }
        [PXCurrency(typeof(POOrder.curyInfoID), typeof(subcontractBilledTotal))]
        [PXUIField(DisplayName = Messages.Subcontract.SubcontractBilledTotal, Enabled = false)]
        [PXFormula(typeof(Sub<POOrder.curyOrderTotal, POOrder.curyUnbilledOrderTotal>))]
        public decimal? CurySubcontractBilledTotal
        {
            get;
            set;
        }
        #endregion

        #region SubcontractBilledTotal
        public abstract class subcontractBilledTotal : BqlDecimal.Field<subcontractBilledTotal> { }
        [PXBaseCury]
        public decimal? SubcontractBilledTotal
        {
            get;
            set;
        }
        #endregion 
    }
}
