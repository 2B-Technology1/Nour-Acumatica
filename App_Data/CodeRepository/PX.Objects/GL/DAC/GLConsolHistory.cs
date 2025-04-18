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

namespace PX.Objects.GL
{
	[Serializable]
    [PXHidden]
	public partial class GLConsolHistory : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<GLConsolHistory>.By<setupID, ledgerID, branchID, accountID, subID, finPeriodID>
		{
			public static GLConsolHistory Find(PXGraph graph, Int32? setupID, Int32? ledgerID, Int32? branchID, Int32? accountID, Int32? subID, String finPeriodID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, setupID, ledgerID, branchID, accountID, subID, finPeriodID, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<GLConsolHistory>.By<branchID> { }
			public class Ledger : GL.Ledger.PK.ForeignKeyOf<GLConsolHistory>.By<ledgerID> { }
			public class Account : GL.Account.PK.ForeignKeyOf<GLConsolHistory>.By<accountID> { }
			public class Subaccount : GL.Sub.PK.ForeignKeyOf<GLConsolHistory>.By<subID> { }
		}
		#endregion

		#region SetupID
		public abstract class setupID : PX.Data.BQL.BqlInt.Field<setupID> { }
		protected Int32? _SetupID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? SetupID
		{
			get
			{
				return this._SetupID;
			}
			set
			{
				this._SetupID = value;
			}
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
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		protected Int32? _LedgerID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? LedgerID
		{
			get
			{
				return this._LedgerID;
			}
			set
			{
				this._LedgerID = value;
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
		#region FinPeriod
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[PXDBString(6, IsKey = true)]
		[PXDefault()]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region PtdCredit
		public abstract class ptdCredit : PX.Data.BQL.BqlDecimal.Field<ptdCredit> { }
		protected Decimal? _PtdCredit;
		[CM.PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? PtdCredit
		{
			get
			{
				return this._PtdCredit;
			}
			set
			{
				this._PtdCredit = value;
			}
		}
		#endregion
		#region PtdDebit
		public abstract class ptdDebit : PX.Data.BQL.BqlDecimal.Field<ptdDebit> { }
		protected Decimal? _PtdDebit;
		[CM.PXDBBaseCury(typeof(GLHistory.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? PtdDebit
		{
			get
			{
				return this._PtdDebit;
			}
			set
			{
				this._PtdDebit = value;
			}
		}
		#endregion
		#region tstamp
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
}
