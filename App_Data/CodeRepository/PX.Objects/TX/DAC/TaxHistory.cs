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

namespace PX.Objects.TX
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.AP;
	using PX.Objects.CM;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.TaxHistory)]
	public partial class TaxHistory : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<TaxHistory>.By<branchID, vendorID, taxReportRevisionID, accountID, subID, taxID, taxPeriodID, lineNbr, revisionID>
		{
			public static TaxHistory Find(PXGraph graph, Int32? branchID, Int32? vendorID, Int32? taxReportRevisionID, Int32? accountID, Int32? subID, String taxID, string taxPeriodID, Int32? lineNbr, Int32? revisionID, PKFindOptions options = PKFindOptions.None) 
				=> FindBy(graph, branchID, vendorID, taxReportRevisionID, accountID, subID, taxID, taxPeriodID, lineNbr, revisionID, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<TaxHistory>.By<branchID> { }
			public class Account : GL.Account.PK.ForeignKeyOf<TaxHistory>.By<accountID> { }
			public class Subaccount : GL.Sub.PK.ForeignKeyOf<TaxHistory>.By<subID> { }
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<TaxHistory>.By<vendorID> { }
			public class Currency : PX.Objects.CM.Currency.PK.ForeignKeyOf<TaxHistory>.By<curyID> { }
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region TaxReportRevisionID
		public abstract class taxReportRevisionID : PX.Data.BQL.BqlInt.Field<taxReportRevisionID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(1)]
		public virtual int? TaxReportRevisionID { get; set; }
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		protected String _TaxID;
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		public virtual String TaxID
		{
			get
			{
				return this._TaxID;
			}
			set
			{
				this._TaxID = value;
			}
		}
		#endregion
		#region TaxPeriodID
		public abstract class taxPeriodID : PX.Data.BQL.BqlString.Field<taxPeriodID> { }
		protected String _TaxPeriodID;
		[GL.FinPeriodID(IsKey = true)]
		[PXDefault()]
		public virtual String TaxPeriodID
		{
			get
			{
				return this._TaxPeriodID;
			}
			set
			{
				this._TaxPeriodID = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		protected Int32? _RevisionID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Revision ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Int32? RevisionID
		{
			get
			{
				return this._RevisionID;
			}
			set
			{
				this._RevisionID = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency")]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region FiledAmt
		public abstract class filedAmt : PX.Data.BQL.BqlDecimal.Field<filedAmt> { }
		protected Decimal? _FiledAmt;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? FiledAmt
		{
			get
			{
				return this._FiledAmt;
			}
			set
			{
				this._FiledAmt = value;
			}
		}
		#endregion
		#region UnfiledAmt
		public abstract class unfiledAmt : PX.Data.BQL.BqlDecimal.Field<unfiledAmt> { }
		protected Decimal? _UnfiledAmt;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? UnfiledAmt
		{
			get
			{
				return this._UnfiledAmt;
			}
			set
			{
				this._UnfiledAmt = value;
			}
		}
		#endregion
		#region ReportFiledAmt
		public abstract class reportFiledAmt : PX.Data.BQL.BqlDecimal.Field<reportFiledAmt> { }
		protected Decimal? _ReportFiledAmt;

		[PXDBVendorCury(typeof(TaxHistory.vendorID), typeof(TaxHistory.branchID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? ReportFiledAmt
		{
			get
			{
				return this._ReportFiledAmt;
			}
			set
			{
				this._ReportFiledAmt = value;
			}
		}
		#endregion
		#region ReportUnfiledAmt
		public abstract class reportUnfiledAmt : PX.Data.BQL.BqlDecimal.Field<reportUnfiledAmt> { }
		protected Decimal? _ReportUnfiledAmt;
		[PXDBVendorCury(typeof(TaxHistory.vendorID), typeof(TaxHistory.branchID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? ReportUnfiledAmt
		{
			get
			{
				return this._ReportUnfiledAmt;
			}
			set
			{
				this._ReportUnfiledAmt = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}

	[System.SerializableAttribute()]
	[PXProjection(typeof(Select4<TaxHistory,
		Aggregate<
		GroupBy<TaxHistory.branchID,
		GroupBy<TaxHistory.vendorID,
		GroupBy<TaxHistory.taxReportRevisionID,
		GroupBy<TaxHistory.taxPeriodID,
		GroupBy<TaxHistory.lineNbr,
		GroupBy<TaxHistory.revisionID,
		Sum<TaxHistory.filedAmt,
		Sum<TaxHistory.unfiledAmt,
		Sum<TaxHistory.reportFiledAmt,
		Sum<TaxHistory.reportUnfiledAmt>>>>>>>>>>>>))]
	[PXCacheName(Messages.TaxHistorySum)]
	public partial class TaxHistorySum : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<TaxHistorySum>.By<branchID, vendorID, taxReportRevisionID, taxPeriodID, lineNbr, revisionID>
		{
			public static TaxHistorySum Find(PXGraph graph, Int32? branchID, Int32? vendorID, Int32? taxReportRevisionID, string taxPeriodID, Int32? lineNbr, Int32? revisionID, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, branchID, vendorID, taxReportRevisionID, taxPeriodID, lineNbr, revisionID, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<TaxHistorySum>.By<branchID> { }
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<TaxHistorySum>.By<vendorID> { }
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[PXDBInt(IsKey = true, BqlTable = typeof(TaxHistory))]
		[PXDefault()]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[PXDBInt(IsKey = true, BqlTable = typeof(TaxHistory))]
		[PXDefault()]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region TaxReportRevisionID
		public abstract class taxReportRevisionID : PX.Data.BQL.BqlInt.Field<taxReportRevisionID> { }
		[PXDBInt(IsKey = true, BqlTable = typeof(TaxHistory))]
		[PXDefault()]
		public virtual int? TaxReportRevisionID { get; set; }
		#endregion
		#region TaxPeriodID
		public abstract class taxPeriodID : PX.Data.BQL.BqlString.Field<taxPeriodID> { }
		protected String _TaxPeriodID;
		[GL.FinPeriodID(IsKey = true, BqlTable = typeof(TaxHistory))]
		[PXDefault()]
		public virtual String TaxPeriodID
		{
			get
			{
				return this._TaxPeriodID;
			}
			set
			{
				this._TaxPeriodID = value;
			}
		}
		#endregion
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		protected Int32? _RevisionID;
		[PXDBInt(IsKey = true, BqlTable = typeof(TaxHistory))]
		[PXDefault()]
		[PXUIField(DisplayName = "Revision ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Int32? RevisionID
		{
			get
			{
				return this._RevisionID;
			}
			set
			{
				this._RevisionID = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlTable = typeof(TaxHistory))]
		[PXDefault()]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region FiledAmt
		public abstract class filedAmt : PX.Data.BQL.BqlDecimal.Field<filedAmt> { }
		protected Decimal? _FiledAmt;

		[PXDBVendorCury(typeof(TaxHistory.vendorID), typeof(TaxHistory.branchID), BqlTable = typeof(TaxHistory))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? FiledAmt
		{
			get
			{
				return this._FiledAmt;
			}
			set
			{
				this._FiledAmt = value;
			}
		}
		#endregion
		#region UnfiledAmt
		public abstract class unfiledAmt : PX.Data.BQL.BqlDecimal.Field<unfiledAmt> { }
		protected Decimal? _UnfiledAmt;
		[PXDBVendorCury(typeof(TaxHistory.vendorID), typeof(TaxHistory.branchID), BqlTable = typeof(TaxHistory))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? UnfiledAmt
		{
			get
			{
				return this._UnfiledAmt;
			}
			set
			{
				this._UnfiledAmt = value;
			}
		}
		#endregion		
		#region ReportFiledAmt
		public abstract class reportFiledAmt : PX.Data.BQL.BqlDecimal.Field<reportFiledAmt> { }
		protected Decimal? _ReportFiledAmt;

		[PXDBVendorCury(typeof(TaxHistory.vendorID), typeof(TaxHistory.branchID), BqlTable = typeof(TaxHistory))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? ReportFiledAmt
		{
			get
			{
				return this._ReportFiledAmt;
			}
			set
			{
				this._ReportFiledAmt = value;
			}
		}
		#endregion
		#region ReportUnfiledAmt
		public abstract class reportUnfiledAmt : PX.Data.BQL.BqlDecimal.Field<reportUnfiledAmt> { }
		protected Decimal? _ReportUnfiledAmt;
		[PXDBVendorCury(typeof(TaxHistory.vendorID), typeof(TaxHistory.branchID), BqlTable = typeof(TaxHistory))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? ReportUnfiledAmt
		{
			get
			{
				return this._ReportUnfiledAmt;
			}
			set
			{
				this._ReportUnfiledAmt = value;
			}
		}
		#endregion		
	}
}

