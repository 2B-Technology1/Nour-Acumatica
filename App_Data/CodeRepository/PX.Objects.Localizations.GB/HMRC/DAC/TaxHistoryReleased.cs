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
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.TX;

namespace PX.Objects.Localizations.GB.HMRC.DAC
{
	[Serializable()]
	[PXProjection(typeof(Select2<TaxHistory,
		 InnerJoin<Branch,
			  On<Branch.branchID, Equal<TaxHistory.branchID>>,
		 InnerJoin<TaxPeriod, On<
					TaxPeriod.vendorID, Equal<TaxHistory.vendorID>,
					And<TaxPeriod.taxPeriodID, Equal<TaxHistory.taxPeriodID>,
					And<TaxPeriod.status, Equal<TaxPeriodStatus.closed>,
					And<TaxPeriod.organizationID, Equal<Branch.organizationID>
			  >>>>>>>))]
	[PXHidden]
	public partial class TaxHistoryReleased : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[PXDBInt(IsKey = true, BqlTable = typeof(TaxHistory))]
		[PXDefault()]
		public virtual Int32? BranchID { get; set; }
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		[PXDBInt(IsKey = true, BqlTable = typeof(TaxHistory))]
		[PXDefault()]
		public virtual Int32? VendorID { get; set; }
		#endregion
		#region TaxPeriodID
		public abstract class taxPeriodID : PX.Data.BQL.BqlString.Field<taxPeriodID> { }

		[PX.Objects.GL.FinPeriodID(IsKey = true, BqlTable = typeof(TaxHistory))]
		[PXDefault()]
		public virtual String TaxPeriodID { get; set; }
		#endregion
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }

		[PXDBInt(IsKey = true, BqlTable = typeof(TaxHistory))]
		[PXDefault()]
		[PXUIField(DisplayName = "Revision ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Int32? RevisionID { get; set; }
		#endregion
		#region TaxReportRevisionID
		public abstract class taxReportRevisionID : PX.Data.BQL.BqlInt.Field<taxReportRevisionID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(1)]
		public virtual int? TaxReportRevisionID { get; set; }
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		[PXDBInt(IsKey = true, BqlTable = typeof(TaxHistory))]
		[PXDefault()]
		public virtual Int32? LineNbr { get; set; }
		#endregion
		#region FiledAmt
		public abstract class filedAmt : PX.Data.BQL.BqlDecimal.Field<filedAmt> { }
		[PXDBBaseCury(typeof(TaxHistory.vendorID), BqlTable = typeof(TaxHistory))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? FiledAmt { get; set; }
		#endregion
		#region UnfiledAmt
		public abstract class unfiledAmt : PX.Data.BQL.BqlDecimal.Field<unfiledAmt> { }
		[PXDBBaseCury(typeof(TaxHistory.vendorID), BqlTable = typeof(TaxHistory))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? UnfiledAmt { get; set; }
		#endregion
		#region ReportFiledAmt
		public abstract class reportFiledAmt : PX.Data.BQL.BqlDecimal.Field<reportFiledAmt> { }

		[PXDBBaseCury(typeof(TaxHistory.vendorID), BqlTable = typeof(TaxHistory))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? ReportFiledAmt { get; set; }
		#endregion
		#region ReportUnfiledAmt
		public abstract class reportUnfiledAmt : PX.Data.BQL.BqlDecimal.Field<reportUnfiledAmt> { }

		[PXDBBaseCury(typeof(TaxHistory.vendorID), BqlTable = typeof(TaxHistory))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? ReportUnfiledAmt { get; set; }
		#endregion
	}
}
