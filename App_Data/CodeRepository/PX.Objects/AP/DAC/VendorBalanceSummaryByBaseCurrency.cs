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

using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.AP
{
	[Serializable]
	[PXCacheName(Messages.VendorBalanceSummaryByBaseCurrency)]
	public partial class VendorBalanceSummaryByBaseCurrency : IBqlTable
	{
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		[PXDBInt()]
		[PXDefault()]
		public virtual Int32? VendorID { get; set; }
		#endregion
		#region BaseCuryID
		public abstract class baseCuryID : PX.Data.BQL.BqlString.Field<baseCuryID> { }
		[PXDBString(5, IsKey = true, IsUnicode = true, BqlTable = typeof(GL.Branch))]
		[PXUIField(DisplayName = "Currency")]
		public virtual string BaseCuryID { get; set; }
		#endregion
		#region Balance
		public abstract class balance : PX.Data.BQL.BqlDecimal.Field<balance> { }
		[CurySymbol(curyID: typeof(Vendor.baseCuryID))]
		[PXBaseCury(curyID: typeof(Vendor.baseCuryID))]
		[PXUIField(DisplayName = "Balance", Visible = true, Enabled = false)]
		public virtual Decimal? Balance { get; set; }
		#endregion
		#region DepositsBalance
		public abstract class depositsBalance : PX.Data.BQL.BqlDecimal.Field<depositsBalance> { }
		[CurySymbol(curyID: typeof(Vendor.baseCuryID))]
		[PXBaseCury(curyID: typeof(Vendor.baseCuryID))]
		[PXUIField(DisplayName = "Prepayment Balance", Enabled = false)]
		public virtual Decimal? DepositsBalance { get; set; }
		#endregion
		#region RetainageBalance
		public abstract class retainageBalance : PX.Data.BQL.BqlDecimal.Field<retainageBalance> { }
		[CurySymbol(curyID: typeof(Vendor.baseCuryID))]
		[PXBaseCury(curyID: typeof(Vendor.baseCuryID))]
		[PXUIField(DisplayName = "Retained Balance", Visibility = PXUIVisibility.Visible, Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainageBalance { get; set; }
		#endregion
	}
}
