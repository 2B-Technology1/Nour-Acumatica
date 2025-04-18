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
using PX.Data.EP;
using PX.Objects.GL.Constants;
using PX.Objects.GL.DAC;
using PX.Objects.CS;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.GL
{
	namespace Constants {
		public static class AllocationMethod 
		{
			public const string ByPercent="C";
			public const string ByWeight = "W";
			public const string ByAcctPTD = "P";
			public const string ByAcctYTD = "Y";
			public const string ByExternalRule = "E";

			public  class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
						new string[] { AllocationMethod.ByPercent, AllocationMethod.ByWeight, AllocationMethod.ByAcctPTD, AllocationMethod.ByAcctYTD },
						new string[] { Messages.ByPercent, Messages.ByWeight,Messages.ByAccountPTD, Messages.ByAccountYTD }){ }
			}
		}

		public static class AllocationCollectMethod
		{
			public const string AcctPTD = "P";
			public const string FromPrevAllocation = "V";
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
						new string[] { AllocationCollectMethod.AcctPTD, AllocationCollectMethod.FromPrevAllocation},
						new string[] { Messages.CollectByAccountPTD, Messages.CollectFromPreviousAllocation}) { }
			}
		}

		public static class PercentLimitType 
		{
			public const string ByAllocation = "A";
			public const string ByPeriod = "P";
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
						new string[] { PercentLimitType.ByPeriod, PercentLimitType.ByAllocation},
						new string[] { Messages.PercentLimitTypeByPeriod, Messages.PercentLimitTypeByAllocation}) { }
			}
			
		}
	}
	
	[PXCacheName(Messages.Allocation)]
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(AllocationMaint))]
	public partial class GLAllocation : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<GLAllocation>.By<gLAllocationID>
		{
			public static GLAllocation Find(PXGraph graph, String gLAllocationID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, gLAllocationID, options);
		}
		public static class FK
		{
			public class AllocationLedger : GL.Ledger.PK.ForeignKeyOf<GLAllocation>.By<allocLedgerID> { }
			public class SourceLedger : GL.Ledger.PK.ForeignKeyOf<GLAllocation>.By<sourceLedgerID> { }
			public class BaseLedger : GL.Ledger.PK.ForeignKeyOf<GLAllocation>.By<basisLederID> { }
			public class Branch : GL.Branch.PK.ForeignKeyOf<GLAllocation>.By<branchID> { }
		}
		#endregion

		#region GLAllocationID
		public abstract class gLAllocationID : PX.Data.BQL.BqlString.Field<gLAllocationID> { }
		protected String _GLAllocationID;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[AutoNumber(typeof(GLSetup.allocationNumberingID), typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Allocation ID", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = true)]
		[PXSelector(typeof(Search<GLAllocation.gLAllocationID>))]
		[PXFieldDescription]
		public virtual String GLAllocationID
		{
			get
			{
				return this._GLAllocationID;
			}
			set
			{
				this._GLAllocationID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		protected bool? _Active;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion
		#region StartFinPeriodID
		public abstract class startFinPeriodID : PX.Data.BQL.BqlString.Field<startFinPeriodID> { }
		protected String _StartFinPeriodID;
		[FinPeriodSelector(null, null, branchSourceType: typeof(GLAllocation.branchID))]
		[PXUIField(DisplayName = "Start Period")]
		public virtual String StartFinPeriodID
		{
			get
			{
				return this._StartFinPeriodID;
			}
			set
			{
				this._StartFinPeriodID = value;
			}
		}
		#endregion
		#region EndFinPeriodID
		public abstract class endFinPeriodID : PX.Data.BQL.BqlString.Field<endFinPeriodID> { }
		protected String _EndFinPeriodID;
	    [FinPeriodSelector(null, null, branchSourceType: typeof(GLAllocation.branchID))]
        [PXUIField(DisplayName = "End Period")]
		public virtual String EndFinPeriodID
		{
			get
			{
				return this._EndFinPeriodID;
			}
			set
			{
				this._EndFinPeriodID = value;
			}
		}
		#endregion
		#region Recurring
		public abstract class recurring : PX.Data.BQL.BqlBool.Field<recurring> { }
		protected bool? _Recurring;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Recurring")]
		public virtual bool? Recurring
		{
			get
			{
				return this._Recurring;
			}
			set
			{
				this._Recurring = value;
			}
		}
		#endregion
		#region AllocMethod
		public abstract class allocMethod : PX.Data.BQL.BqlString.Field<allocMethod> { }
		protected String _AllocMethod;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(AllocationMethod.ByPercent)]
		[PXUIField(DisplayName = "Distribution Method")]
		[AllocationMethod.List()]
		public virtual String AllocMethod
		{
			get
			{
				return this._AllocMethod;
			}
			set
			{
				this._AllocMethod = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch()]
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
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
		[PXInt]
		[PXFormula(typeof(Selector<branchID, Branch.organizationID>))]
		public virtual Int32? OrganizationID
		{
			get;
			set;
		}
		#endregion
		#region AllocLedgerID
		public abstract class allocLedgerID : PX.Data.BQL.BqlInt.Field<allocLedgerID> { }
		protected Int32? _AllocLedgerID;
		[PXDBInt()]
		[PXDefault(typeof(Search<Branch.ledgerID, Where<Branch.branchID, Equal<Current<branchID>>>>))]
		[PXUIField(DisplayName = "Allocation Ledger")]
		[PXSelector(typeof(Search2<Ledger.ledgerID, InnerJoin<OrganizationLedgerLink,
						On<Ledger.ledgerID, Equal<OrganizationLedgerLink.ledgerID>, And<OrganizationLedgerLink.organizationID, Equal<Current<organizationID>>>>>,
						Where<Ledger.balanceType, NotEqual<LedgerBalanceType.budget>>>),
			SubstituteKey = typeof(Ledger.ledgerCD),
			DescriptionField = typeof(Ledger.descr))]
		public virtual Int32? AllocLedgerID
		{
			get
			{
				return this._AllocLedgerID;
			}
			set
			{
				this._AllocLedgerID = value;
			}
		}
		#endregion
		#region AllocLedgerBalanceType
		public abstract class allocLedgerBalanceType : PX.Data.BQL.BqlString.Field<allocLedgerBalanceType> { }
		/// <summary>
		/// The allocation ledger balance type that is updated whenever the allocation ledger changes. <br/>
		/// It affects the range of <see cref="Branch">branches</see> available for selection as the <see cref="GLAllocationDestination">allocation destination</see>
		/// <see cref="GLAllocationDestination.BranchID"> branch</see>.
		/// </summary>
		/// <value>
		/// The <see cref="Ledger.BalanceType"/> of the ledger referenced by the <see cref="GLAllocation.AllocLedgerID"/> field.
		/// </value>
		[PXFormula(typeof(Selector<GLAllocation.allocLedgerID, Ledger.balanceType>))]
		[PXUIField(Visibility = PXUIVisibility.Invisible)]
		public virtual String AllocLedgerBalanceType { get; set; }
		#endregion
		#region AllocLedgerBaseCuryID
		public abstract class allocLedgerBaseCuryID : PX.Data.BQL.BqlString.Field<allocLedgerBaseCuryID> { }
		/// <summary>
		/// The allocation ledger base currency that is updated whenever the allocation ledger changes. <br/>
		/// It affects the range of <see cref="Branch">branches</see> available for selection as the <see cref="GLAllocationDestination">allocation destination</see>
		/// <see cref="GLAllocationDestination.BranchID"> branch</see>.
		/// </summary>
		/// <value>
		/// The <see cref="Ledger.BaseCuryID"/> of the ledger referenced by the <see cref="GLAllocation.AllocLedgerID"/> field.
		/// </value>
		[PXFormula(typeof(Selector<GLAllocation.allocLedgerID, Ledger.baseCuryID>))]
		[PXUIField(Visibility = PXUIVisibility.Invisible)]
		public virtual String AllocLedgerBaseCuryID { get; set; }
		#endregion
		#region SourceLedgerID
		public abstract class sourceLedgerID : PX.Data.BQL.BqlInt.Field<sourceLedgerID> { }
		protected Int32? _SourceLedgerID;
		[PXDBInt()]
		[PXDefault(typeof(Search<Branch.ledgerID, Where<Branch.branchID, Equal<Current<branchID>>>>))]
		[PXUIField(DisplayName = "Source Ledger")]
		[PXSelector(typeof(Search2<Ledger.ledgerID, InnerJoin<LedgerA, On<Ledger.baseCuryID, Equal<LedgerA.baseCuryID>>>, Where<LedgerA.ledgerID, Equal<Current<GLAllocation.allocLedgerID>>>>), SubstituteKey = typeof(Ledger.ledgerCD), DescriptionField = typeof(Ledger.descr))]
		//[PXSelector(typeof(Ledger.ledgerID), SubstituteKey = typeof(Ledger.ledgerCD), DescriptionField = typeof(Ledger.descr))]
		public virtual Int32? SourceLedgerID
		{
			get
			{
				return this._SourceLedgerID;
			}
			set
			{
				this._SourceLedgerID = value;
			}
		}
		#endregion
		#region BasisLederID
		public abstract class basisLederID : PX.Data.BQL.BqlInt.Field<basisLederID> { }
		protected Int32? _BasisLederID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Base Ledger")]
		[PXSelector(typeof(Ledger.ledgerID), SubstituteKey = typeof(Ledger.ledgerCD), DescriptionField = typeof(Ledger.descr), CacheGlobal = true)]
		public virtual Int32? BasisLederID
		{
			get
			{
				return this._BasisLederID;
			}
			set
			{
				this._BasisLederID = value;
			}
		}
			#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlShort.Field<sortOrder> { }
		protected Int16? _SortOrder;
		[PXDBShort()]
		[PXDefault((short)1)]
		[PXUIField(DisplayName = "Sort Order")]
		public virtual Int16? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(GLAllocation.gLAllocationID))]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region LastRevisionOn
		public abstract class lastRevisionOn : PX.Data.BQL.BqlDateTime.Field<lastRevisionOn> { }
		protected DateTime? _LastRevisionOn;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Last Revision Date")]
		public virtual DateTime? LastRevisionOn
		{
			get
			{
				return this._LastRevisionOn;
			}
			set
			{
				this._LastRevisionOn = value;
			}
		}
		#endregion
		#region AllocCollectMethod
		public abstract class allocCollectMethod : PX.Data.BQL.BqlString.Field<allocCollectMethod> { }
		protected String _AllocCollectMethod;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(AllocationCollectMethod.AcctPTD)]
		[PXUIField(DisplayName = "Allocation Method")]
		[AllocationCollectMethod.List()]
		public virtual String AllocCollectMethod
		{
			get
			{
				return this._AllocCollectMethod;
			}
			set
			{
				this._AllocCollectMethod = value;
			}
		}
		#endregion
		#region AllocateSeparately
		public abstract class allocateSeparately : PX.Data.BQL.BqlBool.Field<allocateSeparately> { }
		protected bool? _AllocateSeparately;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allocate Source Accounts Separately")]
		public virtual bool? AllocateSeparately
		{
			get
			{
				return this._AllocateSeparately;
			}
			set
			{
				this._AllocateSeparately = value;
			}
		}
		#endregion AllocateSeparately
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
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
	}
    //Alias
	[PXHidden]
	[Serializable]
	public partial class LedgerA : Ledger 
	{
		public new abstract class baseCuryID : PX.Data.BQL.BqlString.Field<baseCuryID> { }
		public new abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
	}
}
