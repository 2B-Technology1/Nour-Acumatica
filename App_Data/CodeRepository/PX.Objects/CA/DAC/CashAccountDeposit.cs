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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.Common;
using System;


namespace PX.Objects.CA
{
	/// <summary>
	/// The main properties of clearing accounts.
	/// The records of this type define the settings for deposit to the cash account from the clearing accounts.
	/// The presence of this record for a particular pair of cash account and deposit account
	/// defines the possibility to post to the cash account from the specific clearing account.
	/// Clearing accounts are edited on the Cash Accounts (CA202000) form (which corresponds to the <see cref="CashAccountMaint"/> graph) on the tab Clearing Accounts.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.ClearingAccount)]
	public partial class CashAccountDeposit : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CashAccountDeposit>.By<cashAccountID, depositAcctID, paymentMethodID>
		{
			public static CashAccountDeposit Find(PXGraph graph, int? cashAccountID, int? depositAcctID, string paymentMethodID, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, cashAccountID, depositAcctID, paymentMethodID, options);
		}

		public static class FK
		{
			public class ParentCashAccount : CA.CashAccount.PK.ForeignKeyOf<CashAccountDeposit>.By<cashAccountID> { }
			public class DepositeCashAccount : CA.CashAccount.PK.ForeignKeyOf<CashAccountDeposit>.By<depositAcctID> { }
			public class ChargeEntryType : CA.CAEntryType.PK.ForeignKeyOf<CashAccountDeposit>.By<chargeEntryTypeID> { }
			public class PaymentMethod : CA.PaymentMethod.PK.ForeignKeyOf<CashAccountDeposit>.By<paymentMethodID> { }
		}

		#endregion

		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

        /// <summary>
        /// The unique identifier of the parent cash account.
        /// This field is the key field.
        /// </summary>
        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(CashAccount.cashAccountID))]
		[PXUIField(DisplayName = "Cash Account ID", Visible = false)]
        [PXParent(typeof(Select<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<CashAccountDeposit.cashAccountID>>>>))]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region AccountID
		[Obsolete(InternalMessages.FieldIsObsoleteAndWillBeRemoved2023R2)]
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		/// <summary>
		/// The unique identifier of the parent cash account.
		/// This field is the key field.
		/// </summary>
		[Obsolete(InternalMessages.PropertyIsObsoleteAndWillBeRemoved2023R2)]
		[PXInt]
		public virtual int? AccountID
		{
			get
			{
				return this.CashAccountID;
			}
			set
			{
				this.CashAccountID = value;
			}
		}
		#endregion
		#region DepositAcctID
		public abstract class depositAcctID : PX.Data.BQL.BqlInt.Field<depositAcctID> { }

        /// <summary>
        /// The cash account used to record customer payments that will later be deposited to the bank.
        /// Corresponds to the value of the <see cref="CashAccount.CashAccountID"/> field.
        /// </summary>
		[PXDefault]
        [CashAccount(typeof(CashAccount.branchID), typeof(Search<CashAccount.cashAccountID, Where<CashAccount.curyID, Equal<Current<CashAccount.curyID>>,
                And<CashAccount.cashAccountID, NotEqual<Current<CashAccount.cashAccountID>>,
                And<Where<CashAccount.clearingAccount, Equal<boolTrue>,
                Or<CashAccount.cashAccountID, Equal<Current<CashAccountDeposit.depositAcctID>>>>>>>>), IsKey = true, DisplayName = "Clearing Account")]
		public virtual int? DepositAcctID
		{
			get;
			set;
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }

        /// <summary>
        /// The payment method of the deposited payment to which this charge rate should be applied.
        /// If the field is filled by empty string (default), the charge rate is applied to deposited payments, regardless of their payment method.
        /// Corresponds to the value of the <see cref="PaymentMethod.PaymentMethodID"/> field.
        /// </summary>
		[PXDBString(10, IsKey = true, IsUnicode = true)]
		[PXDefault("", PersistingCheck = PXPersistingCheck.Null)]
		[PXUIField(DisplayName = "Payment Method", Required = false)]
		[PXSelector(typeof(PaymentMethod.paymentMethodID))]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region ChargeEntryTypeID
        public abstract class chargeEntryTypeID : PX.Data.BQL.BqlString.Field<chargeEntryTypeID> { }

        /// <summary>
        /// The entry type of the bank charges that apply to the deposit.
        /// Corresponds to the value of the <see cref="CAEntryType.EntryTypeId"/> field.
        /// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Charges Type")]
		[PXSelector(typeof(Search2<CAEntryType.entryTypeId,
			InnerJoin<CashAccountETDetail, On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>,
			Where<CashAccountETDetail.cashAccountID, Equal<Current<depositAcctID>>,
				And<CAEntryType.module, Equal<BatchModule.moduleCA>>>>),
			DescriptionField = typeof(CAEntryType.descr), DirtyRead = false)]
		[PXDefault(typeof(Search2<CAEntryType.entryTypeId, 
			InnerJoin<CashAccountETDetail, On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>,
			Where<CashAccountETDetail.cashAccountID, Equal<Current<depositAcctID>>,
				And<CAEntryType.module, Equal<BatchModule.moduleCA>,
				And<CAEntryType.useToReclassifyPayments, Equal<False>,
				And<CashAccountETDetail.isDefault, Equal<True>>>>>>), 
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<depositAcctID>))]
		public virtual string ChargeEntryTypeID
		{
			get;
			set;
		}
		#endregion
		#region ChargeRate
        public abstract class chargeRate : PX.Data.BQL.BqlDecimal.Field<chargeRate> { }

        /// <summary>
        /// The rate of the specified charges (expressed as a percent of the deposit total).
        /// </summary>
        [PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Charge Rate", Visibility = PXUIVisibility.Visible, Enabled = true)]
		public virtual decimal? ChargeRate
		{
			get;
			set;
		}
		#endregion
	}
}
