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
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.GL
{

	public class TrialBalanceImportOrganizationTreeSelect : PXSelectOrganizationTree
	{
		public TrialBalanceImportOrganizationTreeSelect(PXGraph graph) : base(graph) { }

		public TrialBalanceImportOrganizationTreeSelect(PXGraph graph, Delegate handler) : base(graph, handler) { }

		public override IEnumerable tree([PXString] string AcctCD)
		{
			List<BranchItem> result = new List<BranchItem>();
			foreach (PXResult<BAccount, Branch, Organization> row in
				SelectFrom<BAccount>
					.LeftJoin<Branch>
						.On<BAccount.bAccountID.IsEqual<Branch.bAccountID>>
					.InnerJoin<Organization>
						.On<Branch.organizationID.IsEqual<Organization.organizationID>
							.Or<BAccount.bAccountID.IsEqual<Organization.bAccountID>>>
					.Where<Brackets<Branch.branchID.IsNull
							.Or<Branch.bAccountID.IsEqual<Organization.bAccountID>>
							.Or<Branch.branchID.IsNotNull
								.And<Organization.organizationType.IsEqual<@P.AsString>>>>
						.And<MatchWithBranch<Branch.branchID>>
						.And<MatchWithOrganization<Organization.organizationID>>
						.And<Branch.branchID.IsNull.Or<Branch.active.IsEqual<True>>>
						.And<Organization.organizationID.IsNull.Or<Organization.active.IsEqual<True>>>>
					.View
					.Select(_Graph, OrganizationTypes.WithBranchesBalancing))
			{
				BAccount bAccount = row;
				Branch branch = row;
				Organization organization = row;

				BranchItem item = new BranchItem
				{
					BAccountID = bAccount.BAccountID,
					AcctCD = bAccount.AcctCD,
					AcctName = bAccount.AcctName,
					CanSelect = true
				};

				if (branch?.BAccountID != null && organization.BAccountID != branch.BAccountID)
				{
					item.ParentBAccountID = PXAccess.GetParentOrganization(branch.BranchID).BAccountID;
				}

				item.CanSelect = !(organization.OrganizationType == OrganizationTypes.WithBranchesBalancing && item.BAccountID == organization.BAccountID);

				result.Add(item);
			}
			return result;
		}
	}

	[Serializable]
	[PXPrimaryGraph(typeof(JournalEntryImport))]
	[PXCacheName(Messages.GLTrialBalanceImportMap)]
	public partial class GLTrialBalanceImportMap : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<GLTrialBalanceImportMap>.By<number>
		{
			public static GLTrialBalanceImportMap Find(PXGraph graph, String number, PKFindOptions options = PKFindOptions.None) => FindBy(graph, number, options);
		}
		public static class FK
		{
			public class Ledger : GL.Ledger.PK.ForeignKeyOf<GLTrialBalanceImportMap>.By<ledgerID> { }
		}
		#endregion

		#region Number
		public abstract class number : PX.Data.BQL.BqlString.Field<number> { }

		protected String _Number;

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Import Number", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(GLTrialBalanceImportMap.number),
			typeof(GLTrialBalanceImportMap.number), typeof(GLTrialBalanceImportMap.ledgerID), typeof(GLTrialBalanceImportMap.status), typeof(GLTrialBalanceImportMap.orgBAccountID), typeof(GLTrialBalanceImportMap.finPeriodID), typeof(GLTrialBalanceImportMap.importDate), typeof(GLTrialBalanceImportMap.totalBalance), typeof(GLTrialBalanceImportMap.batchNbr))]
		[AutoNumber(typeof(GLSetup.tBImportNumberingID), typeof(GLTrialBalanceImportMap.importDate))]
		[PXFieldDescription]
		public virtual String Number
		{
			get { return _Number; }
			set { _Number = value; }
		}

		#endregion

		#region OrgBAccountID
		public abstract class orgBAccountID : PX.Data.BQL.BqlInt.Field<orgBAccountID> { }
		/// <summary>
		/// A reference to the <see cref="BAccount"/>
		/// An integer identifier of the organizational entity.
		/// BAccountID of the Organization if OrganizationType != OrganizationTypes.WithBranchesBalancing
		/// BAccountID of the Branch if OrganizationType = OrganizationTypes.WithBranchesBalancing
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="BAccount.BAccountID"/> field.
		/// </value>
		[OrganizationTree(
			sourceOrganizationID: null,
			sourceBranchID: null,
			onlyActive: true,
			treeDataMember: typeof(TrialBalanceImportOrganizationTreeSelect))]
		[PXUIVisible(typeof(Where2<FeatureInstalled<FeaturesSet.branch>, Or<FeatureInstalled<FeaturesSet.multiCompany>>>))]
		public int? OrgBAccountID { get; set; }
		#endregion

		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleGL>>>))]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual String BatchNbr
		{
			get { return _BatchNbr; }
			set { _BatchNbr = value; }
		}
		#endregion

		#region ImportDate
		public abstract class importDate : PX.Data.BQL.BqlDateTime.Field<importDate> { }

		protected DateTime? _ImportDate;
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Import Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? ImportDate
		{
			get { return _ImportDate; }
			set { _ImportDate = value; }
		}
		#endregion

		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		protected String _FinPeriodID;
		[OpenPeriod(searchType: null,
			sourceType: typeof(GLTrialBalanceImportMap.importDate),
			branchSourceType: null,
			branchSourceFormulaType : typeof(Selector<Branch.branchID, GLTrialBalanceImportMap.orgBAccountID>))]
		[PXDefault()]
		[PXUIField(DisplayName = "Period", Visibility = PXUIVisibility.Visible)]
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

		#region BegFinPeriod
		public abstract class begFinPeriod : PX.Data.BQL.BqlString.Field<begFinPeriod> { }

		public virtual String BegFinPeriod
		{
			[PXDependsOnFields(typeof(finPeriodID))]
			get
			{
				return this._FinPeriodID == null ? null : 
					string.Concat(FinPeriodUtils.FiscalYear(this._FinPeriodID), "01");
			}
		}
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		protected String _Description;
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
		public virtual String Description
		{
			get { return _Description; }
			set { _Description = value; }
		}
		#endregion

		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		protected Int32? _LedgerID;
		[PXDBInt]
		[PXDefault(typeof(SearchFor<Ledger.ledgerID>
			.In<
				SelectFrom<Ledger>
					.InnerJoin<OrganizationLedgerLink>
						.On<OrganizationLedgerLink.ledgerID.IsEqual<Ledger.ledgerID>>
							.LeftJoin<Organization>
								.On<Organization.organizationID.IsEqual<OrganizationLedgerLink.organizationID>>
									.LeftJoin<Branch>
										.On<Branch.organizationID.IsEqual<OrganizationLedgerLink.organizationID>>
				.Where<Ledger.balanceType.IsEqual<LedgerBalanceType.actual>
					.And<Brackets<Organization.organizationType.IsEqual<OrganizationTypes.withBranchesBalancing>
							.And<Branch.bAccountID.IsEqual<GLTrialBalanceImportMap.orgBAccountID.FromCurrent>>>
						.Or<Organization.organizationType.IsNotEqual<OrganizationTypes.withBranchesBalancing>
							.And<Organization.bAccountID.IsEqual<GLTrialBalanceImportMap.orgBAccountID.FromCurrent>>>>>
				.Aggregate<To<GroupBy<Ledger.ledgerID>>>>))]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(SearchFor<Ledger.ledgerID>
			.In<
				SelectFrom<Ledger>
					.InnerJoin<OrganizationLedgerLink>
						.On<OrganizationLedgerLink.ledgerID.IsEqual<Ledger.ledgerID>>
							.LeftJoin<Organization>
								.On<Organization.organizationID.IsEqual<OrganizationLedgerLink.organizationID>>
									.LeftJoin<Branch>
										.On<Branch.organizationID.IsEqual<OrganizationLedgerLink.organizationID>>
				.Where<Ledger.balanceType.IsNotEqual<LedgerBalanceType.budget>
					.And<Brackets<Organization.organizationType.IsEqual<OrganizationTypes.withBranchesBalancing>
							.And<Branch.bAccountID.IsEqual<GLTrialBalanceImportMap.orgBAccountID.FromCurrent>>>
						.Or<Organization.organizationType.IsNotEqual<OrganizationTypes.withBranchesBalancing>
							.And<Organization.bAccountID.IsEqual<GLTrialBalanceImportMap.orgBAccountID.FromCurrent>>>>>
				.Aggregate<To<GroupBy<Ledger.ledgerID>>>>),
			SubstituteKey = typeof(Ledger.ledgerCD),
			CacheGlobal = true,
			DescriptionField = typeof(Ledger.descr))]
		public virtual Int32? LedgerID
		{
			get { return _LedgerID; }
			set { _LedgerID = value; }
		}
		#endregion

		#region IsHold
		public abstract class isHold : PX.Data.BQL.BqlBool.Field<isHold> { }

		protected Boolean? _IsHold;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold")]
		public virtual Boolean? IsHold
		{
			get { return _IsHold; }
			set { _IsHold = value; }
		}
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		protected string _Status;
		[PXDBString]
		[TrialBalanceImportMapStatus]
		[PXDefault(TrialBalanceImportMapStatusAttribute.Hold)]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		public virtual string Status
		{
			get { return _Status; }
			set { _Status = value; }
		}
		#endregion
		
		#region IsEditable
		public abstract class isEditable : PX.Data.BQL.BqlBool.Field<isEditable> { }

		// Acuminator disable once PX1030 PXDefaultIncorrectUse [Legacy]
		[PXBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "", Visible = false)]
		[PXDependsOnFields(typeof(status))]
		public virtual Boolean? IsEditable
		{
			get { return _Status != TrialBalanceImportMapStatusAttribute.Released; }
		}
		#endregion

		#region CreditTotalBalance
		public abstract class creditTotalBalance : PX.Data.BQL.BqlDecimal.Field<creditTotalBalance> { }

		protected Decimal? _CreditTotalBalance;

		[CM.PXDBBaseCury(typeof(GLTrialBalanceImportMap.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Total", Enabled = false)]
		[PXFormula(typeof(
			Mult<
				Add<GLTrialBalanceImportMap.liabilityTotal, GLTrialBalanceImportMap.incomeTotal>,
				Switch<Case<Where<Current<GLSetup.trialBalanceSign>, Equal<GL.GLSetup.trialBalanceSign.normal>>, decimal1>, decimal_1>>))]
		public virtual Decimal? CreditTotalBalance
		{
			get { return _CreditTotalBalance; }
			set { _CreditTotalBalance = value; }
		}
		#endregion

		#region DebitTotalBalance
		public abstract class debitTotalBalance : PX.Data.BQL.BqlDecimal.Field<debitTotalBalance> { }

		protected Decimal? _DebitTotalBalance;
		
		[CM.PXDBBaseCury(typeof(GLTrialBalanceImportMap.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Debit Total", Enabled = false)]
		[PXFormula(typeof(
			Add<GLTrialBalanceImportMap.assetTotal, GLTrialBalanceImportMap.expenseTotal>))]
		public virtual Decimal? DebitTotalBalance
		{
			get { return _DebitTotalBalance; }
			set { _DebitTotalBalance = value; }
		}
		#endregion

		#region LiabilityTotal
		public abstract class liabilityTotal : PX.Data.BQL.BqlDecimal.Field<liabilityTotal> { }


		/// <summary>
		/// Total amount of details that have Account type = Liability
		/// </summary>
		[CM.PXDBBaseCury(typeof(GLTrialBalanceImportMap.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Liability Total", Enabled = false)]
		public virtual Decimal? LiabilityTotal
		{
			get;
			set;
		}
		#endregion

		#region IncomeTotal
		public abstract class incomeTotal : PX.Data.BQL.BqlDecimal.Field<incomeTotal> { }


		/// <summary>
		/// Total amount of details that have Account type = Income
		/// </summary>
		[CM.PXDBBaseCury(typeof(GLTrialBalanceImportMap.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Income Total", Enabled = false)]
		public virtual Decimal? IncomeTotal
		{
			get;
			set;
		}
		#endregion

		#region AssetTotal
		public abstract class assetTotal : PX.Data.BQL.BqlDecimal.Field<assetTotal> { }


		/// <summary>
		/// Total amount of details that have Account type = Asset
		/// </summary>
		[CM.PXDBBaseCury(typeof(GLTrialBalanceImportMap.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Asset Total", Enabled = false)]
		public virtual Decimal? AssetTotal
		{
			get;
			set;
		}
		#endregion

		#region ExpenseTotal
		public abstract class expenseTotal : PX.Data.BQL.BqlDecimal.Field<expenseTotal> { }


		/// <summary>
		/// Total amount of details that have Account type = Expense
		/// </summary>
		[CM.PXDBBaseCury(typeof(GLTrialBalanceImportMap.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Expense Total", Enabled = false)]
		public virtual Decimal? ExpenseTotal
		{
			get;
			set;
		}
		#endregion

		#region TotalBalance
		public abstract class totalBalance : PX.Data.BQL.BqlDecimal.Field<totalBalance> { }

		protected Decimal? _TotalBalance;

		[CM.PXDBBaseCury(typeof(GLTrialBalanceImportMap.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Control Total", Enabled = false)]
		public virtual Decimal? TotalBalance
		{
			get
			{
				return _TotalBalance;
			}
			set
			{
				_TotalBalance = value;
			}
		}
		#endregion

        #region LineCntr
        public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
        protected Int32? _LineCntr;
        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? LineCntr
        {
            get
            {
                return this._LineCntr;
            }
            set
            {
                this._LineCntr = value;
            }
        }
        #endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote(DescriptionField = typeof(GLTrialBalanceImportMap.number))]
		public virtual Guid? NoteID { get; set; }
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
		[PXDBLastModifiedByID]
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

}
