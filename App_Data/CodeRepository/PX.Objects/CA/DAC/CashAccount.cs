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
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CA.BankStatementHelpers;

namespace PX.Objects.CA
{
	/// <summary>
	/// The details and settings of cash accounts.
	/// </summary>
	[PXCacheName(Messages.CashAccount)]
	[Serializable]
	[PXPrimaryGraph(
		new Type[] { typeof(CashAccountMaint) },
		new Type[] { typeof(Select<CashAccount,
			Where<CashAccount.cashAccountID, Equal<Current<CashAccount.cashAccountID>>>>)
		})]
	[PXGroupMask(typeof(InnerJoin<Account, On<Account.accountID, Equal<CashAccount.accountID>, And<Match<Account, Current<AccessInfo.userName>>>>,
		InnerJoin<Sub, On<Sub.subID, Equal<CashAccount.subID>, And<Match<Sub, Current<AccessInfo.userName>>>>>>))]
	public partial class CashAccount : IBqlTable, IMatchSettings
	{
	    #region Keys
	    public class PK : PrimaryKeyOf<CashAccount>.By<cashAccountID>
	    {
	        public static CashAccount Find(PXGraph graph, int? cashAccountID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, cashAccountID, options);
	    }

		public static class FK
		{
			public class Account : GL.Account.PK.ForeignKeyOf<CashAccount>.By<accountID> { }
			public class Branch : GL.Branch.PK.ForeignKeyOf<CashAccount>.By<branchID> { }
			public class Subaccount : GL.Sub.PK.ForeignKeyOf<CashAccount>.By<subID> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<CashAccount>.By<curyID> { }
			public class Bank : AP.Vendor.PK.ForeignKeyOf<CashAccount>.By<referenceID> { }
			public class ReconciliationNumberingSequence : CS.Numbering.PK.ForeignKeyOf<CashAccount>.By<reconNumberingID> { }
		}

	    #endregion

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.Visible)]
		public virtual bool? Active
		{
			get;
			set;
		}
		#endregion

		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		[PXDBIdentity]
		[PXUIField(Enabled = false)]
		[PXReferentialIntegrityCheck]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region CashAccountCD
		public abstract class cashAccountCD : PX.Data.BQL.BqlString.Field<cashAccountCD> { }

		[CashAccountRaw(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		public virtual string CashAccountCD
		{
			get;
			set;
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[Account(Required = true, Visibility = PXUIVisibility.SelectorVisible, AvoidControlAccounts = true)]
		[PXRestrictor(typeof(Where<Account.controlAccountModule, IsNull>), Messages.OnlyNonControlAccountCanBeCashAccount)]
		public virtual int? AccountID
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[Branch(Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[SubAccount(typeof(CashAccount.accountID), DisplayName = "Subaccount", DescriptionField = typeof(Sub.description),
			Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? SubID
		{
			get;
			set;
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual string Descr
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, Required = true)]
		[PXSelector(typeof(CM.Currency.curyID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryRateTypeID
		public abstract class curyRateTypeID : PX.Data.BQL.BqlString.Field<curyRateTypeID> { }

		[PXDBString(6, IsUnicode = true)]
		[PXSelector(typeof(CM.CurrencyRateType.curyRateTypeID))]
		[PXForeignReference(typeof(Field<curyRateTypeID>.IsRelatedTo<CM.CurrencyRateType.curyRateTypeID>))]
		[PXUIField(DisplayName = "Curr. Rate Type")]
		public virtual string CuryRateTypeID
		{
			get;
			set;
		}
		#endregion
		#region AllowOverrideCury
		public abstract class allowOverrideCury : PX.Data.BQL.BqlBool.Field<allowOverrideCury>
		{
			public class Disabled : Case<Where<True, Equal<True>>, False> { }
		}
		/// <summary>
		/// If set to <c>true</c>, indicates that the currency 
		/// of customer documents (which is specified by <see cref="CashAccount.CuryID"/>)
		/// can be overridden by a user during document entry.
		/// /// </summary>
		[PXBool]
		[PXFormula(typeof(allowOverrideCury.Disabled))]
		[PXUIField(DisplayName = "Enable Currency Override")]
		public virtual bool? AllowOverrideCury { get; set; }
		#endregion
		#region AllowOverrideRate
		public abstract class allowOverrideRate : PX.Data.BQL.BqlBool.Field<allowOverrideRate>
		{
			public class Enabled : Case<Where<True, Equal<True>>, True> { }
		}
		/// <summary>
		/// If set to <c>true</c>, indicates that the currency rate
		/// for customer documents (which is calculated by the system 
		/// from the currency rate history) can be overridden by a user 
		/// during document entry.
		/// </summary>
		[PXBool]
		[PXFormula(typeof(allowOverrideRate.Enabled))]
		[PXUIField(DisplayName = "Enable Rate Override")]
		public virtual bool? AllowOverrideRate { get; set; }
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }

		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "External Ref. Number", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string ExtRefNbr
		{
			get;
			set;
		}
		#endregion
		#region Reconcile
		public abstract class reconcile : PX.Data.BQL.BqlBool.Field<reconcile> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Requires Reconciliation")]
		public virtual bool? Reconcile
		{
			get;
			set;
		}
		#endregion
		#region ReferenceID
		public abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID> { }

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[Vendor(DescriptionField = typeof(Vendor.acctName), DisplayName = "Bank ID")]
		[PXUIField(DisplayName = "Bank ID")]
		public virtual int? ReferenceID
		{
			get;
			set;
		}
		#endregion
		#region ReconNumberingID
		public abstract class reconNumberingID : PX.Data.BQL.BqlString.Field<reconNumberingID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Numbering.numberingID),
					 DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Reconciliation Numbering Sequence", Required = false)]
		public virtual string ReconNumberingID
		{
			get;
			set;
		}
		#endregion
		#region ClearingAccount
		public abstract class clearingAccount : PX.Data.BQL.BqlBool.Field<clearingAccount> { }
		
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Clearing Account")]
		public virtual bool? ClearingAccount
		{
			get;
			set;
		}
		#endregion
		#region Signature
		public abstract class signature : PX.Data.BQL.BqlString.Field<signature> { }
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Signature")]
		public virtual string Signature
		{
			get;
			set;
		}
		#endregion
		#region SignatureDescr
		public abstract class signatureDescr : PX.Data.BQL.BqlString.Field<signatureDescr> { }
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Name")]
		public virtual string SignatureDescr
		{
			get;
			set;
		}
		#endregion
		#region StatementImportTypeName
		public abstract class statementImportTypeName : PX.Data.BQL.BqlString.Field<statementImportTypeName> { }

		[PXDBString(255)]
		[PXUIField(DisplayName = "Statement Import Service")]
		[PXProviderTypeSelector(typeof(IStatementReader))]
		public virtual string StatementImportTypeName
		{
			get;
			set;
		}
		#endregion
		#region RestrictVisibilityWithBranch
		public abstract class restrictVisibilityWithBranch : PX.Data.BQL.BqlBool.Field<restrictVisibilityWithBranch> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Restrict Visibility with Branch")]
		public virtual bool? RestrictVisibilityWithBranch
		{
			get;
			set;
		}
		#endregion
		#region PTInstanceAllowed
		public abstract class pTInstancesAllowed : PX.Data.BQL.BqlBool.Field<pTInstancesAllowed> { }

		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Cards Allowed", Visible = false, Enabled = false)]
		public virtual bool? PTInstancesAllowed
		{
			get;
			set;
		}
		#endregion
		#region AcctSettingsAllowed
		public abstract class acctSettingsAllowed : PX.Data.BQL.BqlBool.Field<acctSettingsAllowed> { }

		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Account Settings Allowed", Visible = false, Enabled = false)]
		public virtual bool? AcctSettingsAllowed
		{
			get;
			set;
		}
		#endregion
		#region MatchToBatch
		public abstract class matchToBatch : PX.Data.BQL.BqlBool.Field<matchToBatch> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Match Bank Transactions to Batch Payments")]
		public virtual bool? MatchToBatch
		{
			get;
			set;
		}
		#endregion
		#region UseForCorpCard
		public abstract class useForCorpCard : Data.BQL.BqlBool.Field<useForCorpCard> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use for Corporate Cards")]
		public virtual bool? UseForCorpCard { get; set; }
		#endregion
		#region BaseCuryID
		public abstract class baseCuryID : PX.Data.BQL.BqlString.Field<baseCuryID> { }
		[PXDBString(5, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual String BaseCuryID { get; set; }
		#endregion

		#region ReceiptTranDaysBefore
		/// <summary>
		/// Gets sets ReceiptTranDaysBefore
		/// </summary>
		public abstract class receiptTranDaysBefore : PX.Data.BQL.BqlInt.Field<receiptTranDaysBefore> { }
		/// <summary>
		/// Gets sets ReceiptTranDaysBefore
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(5, typeof(CASetup.receiptTranDaysBefore), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Days Before Bank Transaction Date")]
		public virtual Int32? ReceiptTranDaysBefore { get; set; }
        #endregion
        #region ReceiptTranDaysAfter
        /// <summary>
        /// Gets sets ReceiptTranDaysAfter
        /// </summary>
        public abstract class receiptTranDaysAfter : PX.Data.BQL.BqlInt.Field<receiptTranDaysAfter> { }
		/// <summary>
		/// Gets sets ReceiptTranDaysAfter
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(2, typeof(CASetup.receiptTranDaysAfter), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Days After Bank Transaction Date")]
		public virtual Int32? ReceiptTranDaysAfter { get; set; }
        #endregion
        #region DisbursementTranDaysBefore
        /// <summary>
        /// Gets sets DisbursementTranDaysBefore
        /// </summary>
        public abstract class disbursementTranDaysBefore : PX.Data.BQL.BqlInt.Field<disbursementTranDaysBefore> { }
		/// <summary>
		/// Gets sets DisbursementTranDaysBefore
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(5, typeof(CASetup.disbursementTranDaysBefore), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Days Before Bank Transaction Date")]
		public virtual Int32? DisbursementTranDaysBefore { get; set; }
        #endregion
        #region DisbursementTranDaysAfter
        /// <summary>
        /// Gets sets DisbursementTranDaysAfter
        /// </summary>
        public abstract class disbursementTranDaysAfter : PX.Data.BQL.BqlInt.Field<disbursementTranDaysAfter> { }
		/// <summary>
		/// Gets sets DisbursementTranDaysAfter
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(2, typeof(CASetup.disbursementTranDaysAfter), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Days After Bank Transaction Date")]
		public virtual Int32? DisbursementTranDaysAfter { get; set; }

        #endregion
        #region AllowMatchingCreditMemo
        /// <summary>
        /// Gets sets AllowMatchingCreditMemo
        /// </summary>
        public abstract class allowMatchingCreditMemo : PX.Data.BQL.BqlBool.Field<allowMatchingCreditMemo> { }
		/// <summary>
		/// Gets sets AllowMatchingCreditMemo
		/// </summary>
		[PXDBBool]
		[PXDefault(false, typeof(CASetup.allowMatchingCreditMemo), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Allow Matching to Credit Memo")]
		public virtual bool? AllowMatchingCreditMemo
		{
			get;
			set;
		}
        #endregion

        #region RefNbrCompareWeight
        /// <summary>
        /// Gets sets RefNbrCompareWeight
        /// </summary>
        public abstract class refNbrCompareWeight : PX.Data.BQL.BqlDecimal.Field<refNbrCompareWeight> { }
		/// <summary>
		/// Gets sets RefNbrCompareWeight
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 100.0)]
		[PXDefault(TypeCode.Decimal, "70.0", typeof(CASetup.refNbrCompareWeight),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Ref. Nbr. Weight")]
		public virtual Decimal? RefNbrCompareWeight { get; set; }

        #endregion
        #region DateCompareWeight
        /// <summary>
        /// Gets sets DateCompareWeight
        /// </summary>
        public abstract class dateCompareWeight : PX.Data.BQL.BqlDecimal.Field<dateCompareWeight> { }
		/// <summary>
		/// Gets sets DateCompareWeight
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "20.0",
			typeof(CASetup.dateCompareWeight),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Doc. Date Weight")]
		public virtual Decimal? DateCompareWeight { get; set; }

        #endregion
        #region PayeeCompareWeight
        /// <summary>
        /// Gets sets PayeeCompareWeight
        /// </summary>
        public abstract class payeeCompareWeight : PX.Data.BQL.BqlDecimal.Field<payeeCompareWeight> { }
		/// <summary>
		/// Gets sets PayeeCompareWeight
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "10.0",
			typeof(CASetup.payeeCompareWeight),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Doc. Payee Weight")]
		public virtual Decimal? PayeeCompareWeight { get; set; }

		#endregion
		protected Decimal TotalWeight
		{
			get
			{
				decimal total = (this.DateCompareWeight ?? Decimal.Zero)
								+ (this.RefNbrCompareWeight ?? Decimal.Zero)
								+ (this.PayeeCompareWeight ?? Decimal.Zero);
				return total;
			}

		}
        #region RefNbrComparePercent
        /// <summary>
        /// Gets sets RefNbrComparePercent
        /// </summary>
        public abstract class refNbrComparePercent : PX.Data.BQL.BqlDecimal.Field<refNbrComparePercent> { }
		/// <summary>
		/// Gets sets RefNbrComparePercent
		/// </summary>
		[PXDecimal()]
		[PXUIField(DisplayName = "%", Enabled = false)]
		public virtual Decimal? RefNbrComparePercent
		{
			get
			{
				Decimal total = this.TotalWeight;
				return ((total != Decimal.Zero ? (this.RefNbrCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
        #endregion
        #region EmptyRefNbrMatching
        /// <summary>
        /// Gets sets EmptyRefNbrMatching
        /// </summary>
        public abstract class emptyRefNbrMatching : PX.Data.BQL.BqlBool.Field<emptyRefNbrMatching> { }
		/// <summary>
		/// Gets sets EmptyRefNbrMatching
		/// </summary>
		[PXDBBool]
		[PXDefault(false, typeof(CASetup.emptyRefNbrMatching), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Consider Empty Ref. Nbr. as Matching", Visibility = PXUIVisibility.Visible)]
		public virtual bool? EmptyRefNbrMatching
		{
			get;
			set;
		}
        #endregion EmptyRefNbrMatching
        #region DateComparePercent
        /// <summary>
        /// Gets sets DateComparePercent
        /// </summary>
        public abstract class dateComparePercent : PX.Data.BQL.BqlDecimal.Field<dateComparePercent> { }
		/// <summary>
		/// Gets sets DateComparePercent
		/// </summary>
		[PXDecimal()]
		[PXUIField(DisplayName = "%", Enabled = false)]
		public virtual Decimal? DateComparePercent
		{
			get
			{
				Decimal total = this.TotalWeight;
				return ((total != Decimal.Zero ? (this.DateCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
        #endregion
        #region PayeeComparePercent
        /// <summary>
        /// Gets sets PayeeComparePercent
        /// </summary>
        public abstract class payeeComparePercent : PX.Data.BQL.BqlDecimal.Field<payeeComparePercent> { }
		/// <summary>
		/// Gets sets PayeeComparePercent
		/// </summary>
		[PXDecimal()]
		[PXUIField(DisplayName = "%", Enabled = false)]
		public virtual Decimal? PayeeComparePercent
		{
			get
			{
				Decimal total = this.TotalWeight;
				return ((total != Decimal.Zero ? (this.PayeeCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
        #endregion
        #region DateMeanOffset
        /// <summary>
        /// Gets sets DateMeanOffset
        /// </summary>
        public abstract class dateMeanOffset : PX.Data.BQL.BqlDecimal.Field<dateMeanOffset> { }
		/// <summary>
		/// Gets sets DateMeanOffset
		/// </summary>
		[PXDBDecimal(MinValue = -365, MaxValue = 365)]
		[PXDefault(TypeCode.Decimal, "10.0",
			typeof(CASetup.dateMeanOffset),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Payment Clearing Average Delay")]
		public virtual Decimal? DateMeanOffset { get; set; }

        #endregion
        #region DateSigma
        /// <summary>
        /// Gets sets DateSigma
        /// </summary>
        public abstract class dateSigma : PX.Data.BQL.BqlDecimal.Field<dateSigma> { }
		/// <summary>
		/// Gets sets DateSigma
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 365)]
		[PXDefault(TypeCode.Decimal, "5.0",
			typeof(CASetup.dateSigma),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Estimated Deviation (Days)")]
		public virtual Decimal? DateSigma { get; set; }
        #endregion

        #region SkipVoided
        /// <summary>
        /// Gets sets SkipVoided
        /// </summary>
        public abstract class skipVoided : PX.Data.BQL.BqlBool.Field<skipVoided> { }
		protected Boolean? _SkipVoided;
		/// <summary>
		/// Gets sets SkipVoided
		/// </summary>
		[PXDBBool()]
		[PXDefault(false, typeof(CASetup.skipVoided),
				PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Skip Voided Transactions During Matching")]
		public virtual Boolean? SkipVoided
		{
			get
			{
				return this._SkipVoided;
			}
			set
			{
				this._SkipVoided = value;
			}
		}
        #endregion
        #region CuryDiffThreshold
        /// <summary>
        /// Gets sets CuryDiffThreshold
        /// </summary>
        public abstract class curyDiffThreshold : PX.Data.BQL.BqlDecimal.Field<curyDiffThreshold> { }
		/// <summary>
		/// Gets sets CuryDiffThreshold
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "5.0",
			typeof(CASetup.curyDiffThreshold),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Amount Difference Threshold (%)")]
		public virtual Decimal? CuryDiffThreshold { get; set; }
        #endregion
        #region AmountWeight
        /// <summary>
        /// Gets sets AmountWeight
        /// </summary>
        public abstract class amountWeight : PX.Data.BQL.BqlDecimal.Field<amountWeight> { }
		/// <summary>
		/// Gets sets AmountWeight
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "10.0",
			typeof(CASetup.amountWeight),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Amount Weight")]
		public virtual Decimal? AmountWeight { get; set; }

		#endregion
		protected Decimal ExpenseReceiptTotalWeight
		{
			get
			{
				decimal total = (this.DateCompareWeight ?? Decimal.Zero)
								+ (this.RefNbrCompareWeight ?? Decimal.Zero)
								+ (this.AmountWeight ?? Decimal.Zero);
				return total;
			}

		}
        #region ExpenseReceiptRefNbrComparePercent
        /// <summary>
        /// Gets sets ExpenseReceiptRefNbrComparePercent
        /// </summary>
        public abstract class expenseReceiptRefNbrComparePercent : PX.Data.BQL.BqlDecimal.Field<expenseReceiptRefNbrComparePercent> { }
		/// <summary>
		/// Gets sets ExpenseReceiptRefNbrComparePercent
		/// </summary>
		[PXDecimal()]
		public virtual Decimal? ExpenseReceiptRefNbrComparePercent
		{
			get
			{
				Decimal total = this.ExpenseReceiptTotalWeight;
				return ((total != Decimal.Zero ? (this.RefNbrCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
        #endregion
        #region ExpenseReceiptDateComparePercent
        /// <summary>
        /// Gets sets ExpenseReceiptDateComparePercent
        /// </summary>
        public abstract class expenseReceiptDateComparePercent : PX.Data.BQL.BqlDecimal.Field<expenseReceiptDateComparePercent> { }
		/// <summary>
		/// Gets sets ExpenseReceiptDateComparePercent
		/// </summary>
		[PXDecimal()]
		public virtual Decimal? ExpenseReceiptDateComparePercent
		{
			get
			{
				Decimal total = this.ExpenseReceiptTotalWeight;
				return ((total != Decimal.Zero ? (this.DateCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
        #endregion
        #region ExpenseReceiptAmountComparePercent
        /// <summary>
        /// Gets sets ExpenseReceiptAmountComparePercent
        /// </summary>
        public abstract class expenseReceiptAmountComparePercent : PX.Data.BQL.BqlDecimal.Field<expenseReceiptAmountComparePercent> { }
		/// <summary>
		/// Gets sets ExpenseReceiptAmountComparePercent
		/// </summary>
		[PXDecimal()]
		public virtual Decimal? ExpenseReceiptAmountComparePercent
		{
			get
			{
				Decimal total = this.ExpenseReceiptTotalWeight;
				return ((total != Decimal.Zero ? (this.AmountWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
        #endregion
        #region RatioInRelevanceCalculationLabel
        /// <summary>
        /// Gets sets RatioInRelevanceCalculationLabel
        /// </summary>
        public abstract class ratioInRelevanceCalculationLabel : PX.Data.BQL.BqlDecimal.Field<ratioInRelevanceCalculationLabel> { }
		/// <summary>
		/// Gets sets RatioInRelevanceCalculationLabel
		/// </summary>
		[PXString]
		[PXUIField]
		public virtual string RatioInRelevanceCalculationLabel
		{
			get
			{
				return PXMessages.LocalizeFormatNoPrefix(CA.Messages.RatioInRelevanceCalculation,
					ExpenseReceiptRefNbrComparePercent,
					ExpenseReceiptDateComparePercent,
					ExpenseReceiptAmountComparePercent);
			}
			set
			{

			}
		}
        #endregion
        #region MatchSettingsPerAccount
        /// <summary>
        /// Gets sets MatchSettingsPerAccount
        /// </summary>
        public abstract class matchSettingsPerAccount : PX.Data.BQL.BqlBool.Field<matchSettingsPerAccount> { }
		/// <summary>
		/// Gets sets MatchSettingsPerAccount
		/// </summary>
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? MatchSettingsPerAccount { get; set; }

		#endregion

		#region MatchThreshold
		public abstract class matchThreshold : PX.Data.BQL.BqlDecimal.Field<matchThreshold> { }

		/// <summary>
		/// Absolute Relevance Threshold used in auto-matching of transactions. 
		/// Document will be matched automatically to a bank transaction if:
		/// 1. If it's the only match and Match Relevance > Relative Relevance Threshold
		///	2. There is any number of matches and the best match has Match Relevance > Absolute Relevance Threshold
		/// 3. There is any number of matches and Match Relevance of the best match -  Match Relevance of the second best match>= Relative Relevance Threshold
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "75.0",
			typeof(CASetup.matchThreshold),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Absolute Relevance Threshold")]
		public virtual Decimal? MatchThreshold
		{
			get;
			set;
		}
		#endregion

		#region RelativeMatchThreshold
		public abstract class relativeMatchThreshold : PX.Data.BQL.BqlDecimal.Field<relativeMatchThreshold> { }

		/// <summary>
		/// Relative Relevance Threshold used in auto-matching of transactions. 
		/// Document will be matched automatically to a bank transaction if:
		/// 1. If it's the only match and Match Relevance > Relative Relevance Threshold
		///	2. There is any number of matches and the best match has Match Relevance > Absolute Relevance Threshold
		/// 3. There is any number of matches and Match Relevance of the best match -  Match Relevance of the second best match>= Relative Relevance Threshold
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "20.0",
			typeof(CASetup.relativeMatchThreshold),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Relative Relevance Threshold")]
		public virtual Decimal? RelativeMatchThreshold
		{
			get;
			set;
		}
		#endregion

		#region InvoiceFilterByDate
		public abstract class invoiceFilterByDate : PX.Data.BQL.BqlBool.Field<invoiceFilterByDate> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that invoices will be filtered by dates for matching to bank transactions on the Process Bank Transactions (CA306000) form. 
		/// </summary>
		[PXDBBool]
		[PXDefault(false, typeof(CASetup.invoiceFilterByDate),
				PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Match by Discount and Due Date")]
		public virtual bool? InvoiceFilterByDate
		{
			get;
			set;
		}
		#endregion

		#region DaysBeforeInvoiceDiscountDate
		public abstract class daysBeforeInvoiceDiscountDate : PX.Data.BQL.BqlInt.Field<daysBeforeInvoiceDiscountDate> { }

		/// <summary>
		/// The maximum number of days between the invoice discount date and the date of the selected bank transaction, 
		/// to classify invoice as a match
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(typeof(CASetup.daysBeforeInvoiceDiscountDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Days Before Discount Date")]
		public virtual int? DaysBeforeInvoiceDiscountDate
		{
			get;
			set;
		}
		#endregion

		#region DaysBeforeInvoiceDueDate
		public abstract class daysBeforeInvoiceDueDate : PX.Data.BQL.BqlInt.Field<daysBeforeInvoiceDueDate> { }

		/// <summary>
		/// The maximum number of days between the date of the selected bank transaction and the invoice due date, 
		/// to classify invoice as a match, if bank transaction date earlier than invoice due date
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(typeof(CASetup.daysBeforeInvoiceDueDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Days Before Due Date")]
		public virtual int? DaysBeforeInvoiceDueDate
		{
			get;
			set;
		}
		#endregion

		#region DaysAfterInvoiceDueDate
		public abstract class daysAfterInvoiceDueDate : PX.Data.BQL.BqlInt.Field<daysAfterInvoiceDueDate> { }

		/// <summary>
		/// The maximum number of days between the invoice due date and the date of the selected bank transaction, 
		/// to classify invoice as a match, if bank transaction date later than invoice due date
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(typeof(CASetup.daysAfterInvoiceDueDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Days After Due Date")]
		public virtual int? DaysAfterInvoiceDueDate
		{
			get;
			set;
		}
		#endregion

		#region InvoiceFilterByCashAccount
		public abstract class invoiceFilterByCashAccount : PX.Data.BQL.BqlBool.Field<invoiceFilterByCashAccount> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that only Invoices with the same cash account or empty cash account should be selected for matching with bank transactions on the Process Bank Transactions (CA306000) form. 
		/// </summary>
		[PXDBBool]
		[PXDefault(false, typeof(CASetup.invoiceFilterByCashAccount),
				PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Match by Cash Account")]
		public virtual bool? InvoiceFilterByCashAccount
		{
			get;
			set;
		}
		#endregion

		#region InvoiceRefNbrCompareWeight
		public abstract class invoiceRefNbrCompareWeight : PX.Data.BQL.BqlDecimal.Field<invoiceRefNbrCompareWeight> { }

		/// <summary>
		/// The relative weight of the evaluated difference between the reference numbers of the bank transaction and the invoice.
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 100.0)]
		[PXDefault(TypeCode.Decimal, "87.5", typeof(CASetup.invoiceRefNbrCompareWeight), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Ref. Nbr. Weight")]
		public virtual decimal? InvoiceRefNbrCompareWeight
		{
			get;
			set;
		}
		#endregion

		#region InvoiceDateCompareWeight
		public abstract class invoiceDateCompareWeight : PX.Data.BQL.BqlDecimal.Field<invoiceDateCompareWeight> { }

		/// <summary>
		/// The relative weight of the evaluated difference between the dates of the bank transaction and the invoice.
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(CASetup.invoiceDateCompareWeight), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Doc. Date Weight")]
		public virtual decimal? InvoiceDateCompareWeight
		{
			get;
			set;
		}
		#endregion
		
		#region InvoicePayeeCompareWeight
		public abstract class invoicePayeeCompareWeight : PX.Data.BQL.BqlDecimal.Field<invoicePayeeCompareWeight> { }

		/// <summary>
		/// The relative weight of the evaluated difference between the names of the customer on the bank transaction and the invoice.
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "12.5", typeof(CASetup.invoicePayeeCompareWeight), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Doc. Payee Weight")]
		public virtual decimal? InvoicePayeeCompareWeight
		{
			get;
			set;
		}
		#endregion

		protected Decimal InvoiceTotalWeight
		{
			get
			{
				decimal total = (this.InvoiceDateCompareWeight ?? Decimal.Zero)
								+ (this.InvoiceRefNbrCompareWeight ?? Decimal.Zero)
								+ (this.InvoicePayeeCompareWeight ?? Decimal.Zero);
				return total;
			}

		}

		#region InvoiceRefNbrComparePercent
		/// <summary>
		/// Gets sets InvoiceRefNbrComparePercent
		/// </summary>
		public abstract class invoiceRefNbrComparePercent : PX.Data.BQL.BqlDecimal.Field<invoiceRefNbrComparePercent> { }
		/// <summary>
		/// Gets sets RefNbrComparePercent
		/// </summary>
		[PXDecimal()]
		[PXUIField(DisplayName = "%", Enabled = false)]
		public virtual Decimal? InvoiceRefNbrComparePercent
		{
			get
			{
				Decimal total = this.InvoiceTotalWeight;
				return ((total != Decimal.Zero ? (this.InvoiceRefNbrCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
		#endregion

		#region InvoiceDateComparePercent
		/// <summary>
		/// Gets sets InvoiceDateComparePercent
		/// </summary>
		public abstract class invoiceDateComparePercent : PX.Data.BQL.BqlDecimal.Field<invoiceDateComparePercent> { }
		/// <summary>
		/// Gets sets InvoiceDateComparePercent
		/// </summary>
		[PXDecimal()]
		[PXUIField(DisplayName = "%", Enabled = false)]
		public virtual Decimal? InvoiceDateComparePercent
		{
			get
			{
				Decimal total = this.InvoiceTotalWeight;
				return ((total != Decimal.Zero ? (this.InvoiceDateCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
		#endregion
		
		#region InvoicePayeeComparePercent
		/// <summary>
		/// Gets sets InvoicePayeeComparePercent
		/// </summary>
		public abstract class invoicePayeeComparePercent : PX.Data.BQL.BqlDecimal.Field<invoicePayeeComparePercent> { }
		/// <summary>
		/// Gets sets InvoicePayeeComparePercent
		/// </summary>
		[PXDecimal()]
		[PXUIField(DisplayName = "%", Enabled = false)]
		public virtual Decimal? InvoicePayeeComparePercent
		{
			get
			{
				Decimal total = this.InvoiceTotalWeight;
				return ((total != Decimal.Zero ? (this.InvoicePayeeCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
		#endregion

		#region AveragePaymentDelay
		public abstract class averagePaymentDelay : PX.Data.BQL.BqlDecimal.Field<averagePaymentDelay> { }

		/// <summary>
		/// 
		/// </summary>
		[PXDBDecimal(MinValue = -365, MaxValue = 365)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(CASetup.averagePaymentDelay), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Average Payment Delay")]
		public virtual decimal? AveragePaymentDelay
		{
			get;
			set;
		}
		#endregion
		
		#region InvoiceDateSigma
		public abstract class invoiceDateSigma : PX.Data.BQL.BqlDecimal.Field<invoiceDateSigma> { }

		/// <summary>
		/// 
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 365)]
		[PXDefault(TypeCode.Decimal, "2.0", typeof(CASetup.invoiceDateSigma), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Estimated Deviation (Days)")]
		public virtual decimal? InvoiceDateSigma
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote(DescriptionField = typeof(CashAccount.cashAccountCD))]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}
}
