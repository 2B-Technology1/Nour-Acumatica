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
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.AP;

namespace PX.Objects.EP
{
	[System.SerializableAttribute()]
    [PXHidden]
	public partial class EPVendorClass : VendorClass
	{
		public new abstract class vendorClassID : PX.Data.BQL.BqlString.Field<vendorClassID> { }
		public new abstract class discTakenAcctID : PX.Data.BQL.BqlInt.Field<discTakenAcctID> { }
		public new abstract class discTakenSubID : PX.Data.BQL.BqlInt.Field<discTakenSubID> { }
		public new abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID> { }
		public new abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
	}

	[PXPrimaryGraph(typeof(EmployeeClassMaint))]
	[System.SerializableAttribute()]
	[PXTable]
	[PXCacheName(Messages.EmployeeClass)]
	public partial class EPEmployeeClass : VendorClass
	{
		#region Keys
		public new class PK : PrimaryKeyOf<EPEmployeeClass>.By<vendorClassID>
		{
			public static EPEmployeeClass Find(PXGraph graph, string vendorClassID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, vendorClassID, options);
		}
		public new static class FK
		{
			public class Terms : CS.Terms.PK.ForeignKeyOf<EPEmployeeClass>.By<termsID> { }
			public class PaymentMethod : CA.PaymentMethod.PK.ForeignKeyOf<EPEmployeeClass>.By<paymentMethodID> { }
			public class CashAccount : CA.CashAccount.PK.ForeignKeyOf<EPEmployeeClass>.By<cashAcctID> { }

			public class TaxZone : TX.TaxZone.PK.ForeignKeyOf<EPEmployeeClass>.By<taxZoneID> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<EPEmployeeClass>.By<curyID> { }
			public class CurrencyRateType : CM.CurrencyRateType.PK.ForeignKeyOf<EPEmployeeClass>.By<curyRateTypeID> { }

			public class APAccount : GL.Account.PK.ForeignKeyOf<EPEmployeeClass>.By<aPAcctID> { }
			public class APSubaccount : GL.Sub.PK.ForeignKeyOf<EPEmployeeClass>.By<aPSubID> { }

			public class CashDiscountAccount : GL.Account.PK.ForeignKeyOf<EPEmployeeClass>.By<discTakenAcctID> { }
			public class CashDiscountSubaccount : GL.Sub.PK.ForeignKeyOf<EPEmployeeClass>.By<discTakenSubID> { }

			public class ExpenseAccount : GL.Account.PK.ForeignKeyOf<EPEmployeeClass>.By<expenseAcctID> { }
			public class ExpenseSubaccount : GL.Sub.PK.ForeignKeyOf<EPEmployeeClass>.By<expenseSubID> { }

			public class PrepaymentAccount : GL.Account.PK.ForeignKeyOf<EPEmployeeClass>.By<prepaymentAcctID> { }
			public class PrepaymentSubaccount : GL.Sub.PK.ForeignKeyOf<EPEmployeeClass>.By<prepaymentSubID> { }

			public class SalesAccount : GL.Account.PK.ForeignKeyOf<EPEmployeeClass>.By<salesAcctID> { }
			public class SalesSubaccount : GL.Sub.PK.ForeignKeyOf<EPEmployeeClass>.By<salesSubID> { }
		}
		#endregion

		#region VendorClassID
		public new abstract class vendorClassID : PX.Data.BQL.BqlString.Field<vendorClassID> { }
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 0)]
		[PXSelector(typeof(EPEmployeeClass.vendorClassID), CacheGlobal = true)]
		public override String VendorClassID
		{
			get
			{
				return this._VendorClassID;
			}
			set
			{
				this._VendorClassID = value;
			}
		}

		#endregion
		#region DiscTakenAcctID
		public new abstract class discTakenAcctID : PX.Data.BQL.BqlInt.Field<discTakenAcctID> { }

		[PXDefault(typeof(
			Coalesce<
				Search2<EPEmployeeClass.discTakenAcctID, InnerJoin<APSetup, On<EPEmployeeClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>,
				Search2<EPVendorClass.discTakenAcctID, InnerJoin<APSetup, On<EPVendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>>))]
		[Account(DisplayName = "Cash Discount Account", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<EPEmployeeClass.discTakenAcctID>.IsRelatedTo<Account.accountID>))]
		public override Int32? DiscTakenAcctID
		{
			get
			{
				return this._DiscTakenAcctID;
			}
			set
			{
				this._DiscTakenAcctID = value;
			}
		}
		#endregion
		#region DiscTakenSubID
		public new abstract class discTakenSubID : PX.Data.BQL.BqlInt.Field<discTakenSubID> { }

		[PXDefault(typeof(
			Coalesce<
				Search2<EPEmployeeClass.discTakenSubID, InnerJoin<APSetup, On<EPEmployeeClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>,
				Search2<EPVendorClass.discTakenSubID, InnerJoin<APSetup, On<EPVendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>>))]
		[SubAccount(typeof(EPEmployeeClass.discTakenAcctID), DisplayName = "Cash Discount Sub.", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<EPEmployeeClass.discTakenSubID>.IsRelatedTo<Sub.subID>))]
		public override Int32? DiscTakenSubID
		{
			get
			{
				return this._DiscTakenSubID;
			}
			set
			{
				this._DiscTakenSubID = value;
			}
		}
		#endregion
		#region PaymentMethodID
		public new abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method")]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
							Where<PaymentMethod.useForAP, Equal<True>,
								And<PaymentMethod.isActive, Equal<True>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		public override String PaymentMethodID
		{
			get
			{
				return this._PaymentMethodID;
			}
			set
			{
				this._PaymentMethodID = value;
			}
		}
		#endregion
		#region CashAcctID
		public new abstract class cashAcctID : PX.Data.BQL.BqlInt.Field<cashAcctID> { }
		
		[CashAccount(typeof(Search2<CashAccount.cashAccountID,
						InnerJoin<PaymentMethodAccount,
							On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
						Where2<Match<Current<AccessInfo.userName>>,
							And<CashAccount.clearingAccount, Equal<False>,
							And<PaymentMethodAccount.paymentMethodID, Equal<Current<EPEmployeeClass.paymentMethodID>>,
							And<PaymentMethodAccount.useForAP, Equal<True>>>>>>))]
		public override Int32? CashAcctID
		{
			get
			{
				return this._CashAcctID;
			}
			set
			{
				this._CashAcctID = value;
			}
		}
		#endregion
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.BQL.BqlInt.Field<salesAcctID> { }
		protected Int32? _SalesAcctID;
		[Account(DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<EPEmployeeClass.salesAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? SalesAcctID
		{
			get
			{
				return this._SalesAcctID;
			}
			set
			{
				this._SalesAcctID = value;
			}
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.BQL.BqlInt.Field<salesSubID> { }
		protected Int32? _SalesSubID;
		[SubAccount(typeof(EPEmployeeClass.salesAcctID), DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<EPEmployeeClass.salesSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? SalesSubID
		{
			get
			{
				return this._SalesSubID;
			}
			set
			{
				this._SalesSubID = value;
			}
		}
		#endregion
		#region CalendarID
		public abstract class calendarID : PX.Data.BQL.BqlString.Field<calendarID> { }
		protected String _CalendarID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Calendar", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
		public virtual String CalendarID
		{
			get
			{
				return this._CalendarID;
			}
			set
			{
				this._CalendarID = value;
			}
		}
		#endregion
        #region HoursValidation
        public abstract class hoursValidation : PX.Data.BQL.BqlString.Field<hoursValidation> { }
        protected String _HoursValidation;
        [PXDBString(1)]
        [PXUIField(DisplayName = "Regular Hours Validation")]
        [HoursValidationOption.List]
        [PXDefault(HoursValidationOption.Validate)]
        public virtual String HoursValidation
        {
            get
            {
                return this._HoursValidation;
            }
            set
            {
                this._HoursValidation = value;
            }
        }
        #endregion
		#region VPaymentByType
		public new abstract class paymentByType : PX.Data.BQL.BqlInt.Field<paymentByType> { }
		[PXDBInt()]
		[PXDefault(APPaymentBy.DueDate, PersistingCheck = PXPersistingCheck.Nothing)]
		[APPaymentBy.List]
		[PXUIField(DisplayName = "Payment By")]
		public override int? PaymentByType
		{
			get
			{
				return this._PaymentByType;
			}
			set
			{
				this._PaymentByType = value;
			}
		}
		#endregion
		#region DefaultDateInActivity
		public abstract class defaultDateInActivity : PX.Data.BQL.BqlString.Field<defaultDateInActivity>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { NextWorkDay, LastDay },
					new string[] { Messages.NextWorkDay, Messages.LastDay }) { ; }
			}

			public const string LastDay = "LD";
			public const string NextWorkDay = "NW";
		}
		protected String _DefaultDateInActivity;
		[PXDBString(2)]
		[PXUIField(DisplayName = "Default Date in Time Cards")]
		[defaultDateInActivity.List]
		[PXDefault(defaultDateInActivity.NextWorkDay)]
		public virtual String DefaultDateInActivity
		{
			get
			{
				return this._DefaultDateInActivity;
			}
			set
			{
				this._DefaultDateInActivity = value;
			}
		}
		#endregion

		#region ProbationPeriodMonths
		/// <summary>
		/// The probation period (in months).
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 12)]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Probation Period (Months)")]
		public virtual int? ProbationPeriodMonths { get; set; }
		public abstract class probationPeriodMonths : PX.Data.BQL.BqlInt.Field<probationPeriodMonths> { }
		#endregion

		// stubs with no Defaults for fields that are not visible on Employee Class form
		#region DiscountAcctID
		public new abstract class discountAcctID : PX.Data.BQL.BqlInt.Field<discountAcctID> { }

		[Account(DisplayName = "Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<EPEmployeeClass.discountAcctID>.IsRelatedTo<Account.accountID>))]
		public override Int32? DiscountAcctID { get; set; }
		#endregion
		#region DiscountSubID
		public new abstract class discountSubID : PX.Data.BQL.BqlInt.Field<discountSubID> { }

		[SubAccount(typeof(EPEmployeeClass.discountAcctID), DisplayName = "Discount Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<EPEmployeeClass.discountSubID>.IsRelatedTo<Sub.subID>))]
		public override Int32? DiscountSubID { get; set; }
		#endregion

		#region FreightAcctID
		public new abstract class freightAcctID : PX.Data.BQL.BqlInt.Field<freightAcctID> { }

		[Account(DisplayName = "Freight Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<EPEmployeeClass.freightAcctID>.IsRelatedTo<Account.accountID>))]
		public override Int32? FreightAcctID { get; set; }
		#endregion
		#region FreightSubID
		public new abstract class freightSubID : PX.Data.BQL.BqlInt.Field<freightSubID> { }

		[SubAccount(typeof(EPEmployeeClass.freightAcctID), DisplayName = "Freight Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<EPEmployeeClass.freightSubID>.IsRelatedTo<Sub.subID>))]
		public override Int32? FreightSubID { get; set; }
		#endregion

		#region POAccrualAcctID
		public new abstract class pOAccrualAcctID : PX.Data.BQL.BqlInt.Field<pOAccrualAcctID> { }

		[Account(DisplayName = "PO Accrual Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.PO)]
		[PXForeignReference(typeof(Field<EPEmployeeClass.pOAccrualAcctID>.IsRelatedTo<Account.accountID>))]
		public override Int32? POAccrualAcctID { get; set; }
		#endregion
		#region POAccrualSubID
		public new abstract class pOAccrualSubID : PX.Data.BQL.BqlInt.Field<pOAccrualSubID> { }

		[SubAccount(typeof(EPEmployeeClass.pOAccrualAcctID), DisplayName = "PO Accrual Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<EPEmployeeClass.pOAccrualSubID>.IsRelatedTo<Sub.subID>))]
		public override Int32? POAccrualSubID { get; set; }
		#endregion

		#region PrebookAcctID
		public new abstract class prebookAcctID : PX.Data.BQL.BqlInt.Field<prebookAcctID> { }

		[Account(DisplayName = "Reclassification Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<EPEmployeeClass.prebookAcctID>.IsRelatedTo<Account.accountID>))]
		public override Int32? PrebookAcctID { get; set; }
		#endregion
		#region PrebookSubID
		public new abstract class prebookSubID : PX.Data.BQL.BqlInt.Field<prebookSubID> { }

		[SubAccount(typeof(EPEmployeeClass.prebookAcctID), DisplayName = "Reclassification Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<EPEmployeeClass.prebookSubID>.IsRelatedTo<Sub.subID>))]
		public override Int32? PrebookSubID { get; set; }
		#endregion
	}

    public static class HoursValidationOption
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { Validate, WarningOnly, None },
                new string[] { Messages.Validate, Messages.WarningOnly, Messages.None }) { ; }
        }
        
        public const string Validate = "V";
        public const string WarningOnly = "W";
        public const string None = "N";
    }
}

namespace PX.Objects.EP.Standalone
{
	using System;
	using PX.Data;

    [Serializable]
	public partial class EPEmployeeClass : PX.Data.IBqlTable
	{
		#region VendorClassID
		public abstract class vendorClassID : PX.Data.BQL.BqlString.Field<vendorClassID> { }
		protected string _VendorClassID;
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		public virtual String VendorClassID
		{
			get
			{
				return this._VendorClassID;
			}
			set
			{
				this._VendorClassID = value;
			}
		}
		#endregion
	}
}
