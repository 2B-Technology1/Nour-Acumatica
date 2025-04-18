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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	/// <summary>
	/// A balance record of an accounts receivable customer.
	/// Customer balances are accumulated into records across the 
	/// following dimensions: branch, customer, and customer location. 
	/// The balance records are created and updated by the <see cref="ARDocumentRelease"/> 
	/// graph during the document release process.
	/// </summary>
	[Serializable]
    [Overrides.ARDocumentRelease.ARBalAccum]
	[PXCacheName(Messages.ARBalances)]
    public partial class ARBalances : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<ARBalances>.By<branchID, customerID, customerLocationID>
		{
			public static ARBalances Find(PXGraph graph, Int32? branchID, Int32? customerID, Int32? customerLocationID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, branchID, customerID, customerLocationID, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<ARBalances>.By<branchID> { }
			public class Customer : AR.Customer.PK.ForeignKeyOf<ARBalances>.By<customerID> { }
			public class CustomerLocation : CR.Location.PK.ForeignKeyOf<ARBalances>.By<customerID, customerLocationID> { }
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[PXDBInt(IsKey=true)]
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

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<ARBalances.customerID>>>>))]
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? CustomerID { get; set; }
		#endregion

		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
		protected Int32? _CustomerLocationID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? CustomerLocationID
		{
			get
			{
				return this._CustomerLocationID;
			}
			set
			{
				this._CustomerLocationID = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
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
		#region CurrentBal
		public abstract class currentBal : PX.Data.BQL.BqlDecimal.Field<currentBal> { }
		protected Decimal? _CurrentBal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CurrentBal
		{
			get
			{
				return this._CurrentBal;
			}
			set
			{
				this._CurrentBal = value;
			}
		}
		#endregion
		#region UnreleasedBal
		public abstract class unreleasedBal : PX.Data.BQL.BqlDecimal.Field<unreleasedBal> { }
		protected Decimal? _UnreleasedBal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnreleasedBal
		{
			get
			{
				return this._UnreleasedBal;
			}
			set
			{
				this._UnreleasedBal = value;
			}
		}
		#endregion
		#region TotalPrepayments
		public abstract class totalPrepayments : PX.Data.BQL.BqlDecimal.Field<totalPrepayments> { }
		protected Decimal? _TotalPrepayments;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TotalPrepayments
		{
			get
			{
				return this._TotalPrepayments;
			}
			set
			{
				this._TotalPrepayments = value;
			}
		}
		#endregion
		#region TotalQuotations
		public abstract class totalQuotations : PX.Data.BQL.BqlDecimal.Field<totalQuotations> { }
		protected Decimal? _TotalQuotations;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TotalQuotations
		{
			get
			{
				return this._TotalQuotations;
			}
			set
			{
				this._TotalQuotations = value;
			}
		}
		#endregion
		#region TotalOpenOrders
		public abstract class totalOpenOrders : PX.Data.BQL.BqlDecimal.Field<totalOpenOrders> { }
		protected Decimal? _TotalOpenOrders;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TotalOpenOrders
		{
			get
			{
				return this._TotalOpenOrders;
			}
			set
			{
				this._TotalOpenOrders = value;
			}
		}
		#endregion
		#region TotalShipped
		public abstract class totalShipped : PX.Data.BQL.BqlDecimal.Field<totalShipped> { }
		protected Decimal? _TotalShipped;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TotalShipped
		{
			get
			{
				return this._TotalShipped;
			}
			set
			{
				this._TotalShipped = value;
			}
		}
		#endregion
		#region LastInvoiceDate
		public abstract class lastInvoiceDate : PX.Data.BQL.BqlDateTime.Field<lastInvoiceDate> { }
		protected DateTime? _LastInvoiceDate;
		[PXDBDate()]
		public virtual DateTime? LastInvoiceDate
		{
			get
			{
				return this._LastInvoiceDate;
			}
			set
			{
				this._LastInvoiceDate = value;
			}
		}
		#endregion
		#region OldInvoiceDate
		public abstract class oldInvoiceDate : PX.Data.BQL.BqlDateTime.Field<oldInvoiceDate> { }
		protected DateTime? _OldInvoiceDate;
		[PXDBDate()]
		public virtual DateTime? OldInvoiceDate
		{
			get
			{
				return this._OldInvoiceDate;
			}
			set
			{
				this._OldInvoiceDate = value;
			}
		}
		#endregion
		#region NumberInvoicePaid
		public abstract class numberInvoicePaid : PX.Data.BQL.BqlInt.Field<numberInvoicePaid> { }
		protected Int32? _NumberInvoicePaid;
		[PXDBInt()]
		public virtual Int32? NumberInvoicePaid
		{
			get
			{
				return this._NumberInvoicePaid;
			}
			set
			{
				this._NumberInvoicePaid = value;
			}
		}
		#endregion
		#region PaidInvoiceDays
		public abstract class paidInvoiceDays : PX.Data.BQL.BqlInt.Field<paidInvoiceDays> { }
		protected Int32? _PaidInvoiceDays;
		[PXDBInt()]
		public virtual Int32? PaidInvoiceDays
		{
			get
			{
				return this._PaidInvoiceDays;
			}
			set
			{
				this._PaidInvoiceDays = value;
			}
		}
		#endregion
		#region AverageDaysToPay
		public abstract class averageDaysToPay : PX.Data.BQL.BqlInt.Field<averageDaysToPay> { }
		protected Int32? _AverageDaysToPay;
		[PXDBInt()]
		public virtual Int32? AverageDaysToPay
		{
			get
			{
				return this._AverageDaysToPay;
			}
			set
			{
				this._AverageDaysToPay = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
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
        #region DatesUpdated
        public abstract class datesUpdated : PX.Data.BQL.BqlBool.Field<datesUpdated> { }
        protected Boolean? _DatesUpdated;
        [PXBool]
        [PXDefault(false)]
        public virtual Boolean? DatesUpdated
        {
            get
            {
                return this._DatesUpdated;
            }
            set
            {
                this._DatesUpdated = value;
            }
        }
		#endregion
		#region LastDocDate
		public abstract class lastDocDate : PX.Data.BQL.BqlDateTime.Field<lastDocDate> { }
		[PXDBDate()]
		public virtual DateTime? LastDocDate { get; set; }
		#endregion
		#region StatementRequired
		public abstract class statementRequired : PX.Data.BQL.BqlBool.Field<statementRequired> { }
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? StatementRequired { get; set; }
		#endregion
	}

	[Serializable]
	[PXCacheName(Messages.ARBalancesByBaseCurrency)]
	[PXProjection(typeof(Select5<ARBalances,
						InnerJoin<GL.Branch, On<GL.Branch.branchID, Equal<ARBalances.branchID>>>,
					Aggregate<
					GroupBy<ARBalances.customerID,
					GroupBy<GL.Branch.baseCuryID,
					Sum<ARBalances.currentBal,
					Sum<ARBalances.totalPrepayments,
					Sum<ARBalances.unreleasedBal>>>>>>>))]
	public class ARBalancesByBaseCuryID : IBqlTable
	{
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[PXDBInt(IsKey = true, BqlTable = typeof(ARBalances))]
		public virtual int? CustomerID { get; set; }
		#endregion
		#region BaseCuryID
		public abstract class baseCuryID : PX.Data.BQL.BqlString.Field<baseCuryID> { }
		[PXDBString(5, IsKey = true, IsUnicode = true, BqlTable = typeof(GL.Branch))]
		[PXUIField(DisplayName = "Currency")]
		public virtual string BaseCuryID { get; set; }
		#endregion
		#region CurrentBal
		public abstract class currentBal : PX.Data.BQL.BqlDecimal.Field<currentBal> { }
		[PXDBCury(typeof(baseCuryID), BqlTable = typeof(ARBalances))]
		[PXUIField(DisplayName = "Balance")]
		public virtual decimal? CurrentBal { get; set; }
		#endregion
		#region TotalPrepayments
		public abstract class totalPrepayments : PX.Data.BQL.BqlDecimal.Field<totalPrepayments> { }
		[PXDBCury(typeof(baseCuryID), BqlTable = typeof(ARBalances))]
		[PXUIField(DisplayName = "Prepayment Balance")]
		public virtual decimal? TotalPrepayments { get; set; }
		#endregion
		#region UnreleasedBal
		public abstract class unreleasedBal : PX.Data.BQL.BqlDecimal.Field<unreleasedBal> { }
		[PXDBCury(typeof(baseCuryID), BqlTable = typeof(ARBalances))]
		[PXUIField(DisplayName = "Unreleased Balance")]
		public virtual decimal? UnreleasedBal { get; set; }
		#endregion
		#region ConsolidatedBalance
		public abstract class consolidatedBalance : PX.Data.BQL.BqlDecimal.Field<consolidatedBalance> { }
		[PXDBCury(typeof(baseCuryID), BqlTable = typeof(ARBalances))]
		[PXUIField(DisplayName = "Consolidated Balance")]
		public virtual decimal? ConsolidatedBalance { get; set; }
		#endregion
		#region RetainageBalance
		public abstract class retainageBalance : PX.Data.BQL.BqlDecimal.Field<retainageBalance> { }
		[PXDBCury(typeof(baseCuryID), BqlTable = typeof(ARBalances))]
		[PXUIField(DisplayName = "Retained Balance", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainageBalance { get; set; }
		#endregion
	}

}
