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
using PX.Data.BQL;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA.BankStatementHelpers;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Web.UI;
using PX.Objects.Common.Attributes;
using PX.Objects.TX;

namespace PX.Objects.CA
{
	/// <summary>
	/// The main properties of CA bank transactions and their classes.
	/// CA bank transactions are edited on the Process Bank Transactions (CA306000) form
	/// (which corresponds to the <see cref="CABankTransactionsMaint"/> graph).
	/// Also CA bank transactions are edited on the Process Incoming Payments (AR305000) form
	/// (which corresponds to the <see cref="CABankIncomingPaymentsMaint"/> graph).
	/// </summary>
	[System.SerializableAttribute]
	[PXCacheName(Messages.BankTransaction)]
	public partial class CABankTran : PX.Data.IBqlTable, ICADocWithTaxesSource
	{
		#region Keys
		public class PK : PrimaryKeyOf<CABankTran>.By<CABankTran.tranID>
		{
			public static CABankTran Find(PXGraph graph, int? tranID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, tranID, options);
		}

		public static class FK
		{
			public class CashAcccount : CA.CashAccount.PK.ForeignKeyOf<CABankTran>.By<cashAccountID> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<CABankTran>.By<curyID> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<CABankTran>.By<curyInfoID> { }
			public class MatchingCurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<CABankTran>.By<origCuryID> { }
			public class PayeeBusinessAccount : CR.BAccount.PK.ForeignKeyOf<CABankTran>.By<payeeBAccountID> { }
			public class PayeeLocation : CR.Location.PK.ForeignKeyOf<CABankTran>.By<payeeBAccountID, payeeLocationID> { }
			public class PaymentMethod : CA.PaymentMethod.PK.ForeignKeyOf<CABankTran>.By<paymentMethodID> { }
			public class CustomerPaymentMethod : AR.CustomerPaymentMethod.PK.ForeignKeyOf<CABankTran>.By<pMInstanceID> { }
			public class MatchingEntryType : CA.CAEntryType.PK.ForeignKeyOf<CABankTran>.By<entryTypeID> { }
			public class TransactionRule : CA.CABankTranRule.PK.ForeignKeyOf<CABankTran>.By<ruleID> { }
			public class BankStatement : CA.CABankTranHeader.PK.ForeignKeyOf<CABankTran>.By<cashAccountID, headerRefNbr, tranType> { }
		}

		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		/// <summary>
		/// The cash account specified on the bank statement for which you want to upload bank transactions.
		/// This field is a part of the compound key of the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CashAccount.CashAccountID"/> field.
		/// </value>
		[GL.CashAccount]
		[PXDefault(typeof(CABankTranHeader.cashAccountID))]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlInt.Field<tranID> { }

		/// <summary>
		/// The unique identifier of the CA bank transaction.
		/// This field is the key field.
		/// </summary>
		[PXUIField(DisplayName = "ID", Visible = false)]
		[PXDBIdentity(IsKey = true)]
		public virtual int? TranID
		{
			get;
			set;
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

		/// <summary>
		/// The type of the bank tansaction.
		///  The field is linked to the <see cref="CABankTranHeader.TranType"/> field.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"S"</c>: Bank Statement Import,
		/// <c>"I"</c>: Payments Import
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(CABankTranHeader.tranType))]
		[CABankTranType.List]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		public virtual string TranType
		{
			get;
			set;
		}
		#endregion
		#region HeaderRefNbr
		public abstract class headerRefNbr : PX.Data.BQL.BqlString.Field<headerRefNbr> { }

		/// <summary>
		/// The reference number of the imported bank statement (<see cref="CABankTranHeader">CABankTranHeader</see>),
		/// which the system generates automatically in accordance with the numbering sequence assigned to statements on the Cash Management Preferences (CA101000) form.
		/// </summary>
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDBDefault(typeof(CABankTranHeader.refNbr))]
		[PXUIField(DisplayName = "Statement Nbr.")]
		[PXParent(typeof(Select<CABankTranHeader, Where<CABankTranHeader.refNbr, Equal<Current<CABankTran.headerRefNbr>>,
								And<CABankTranHeader.tranType, Equal<Current<CABankTran.tranType>>>>>))]
		public virtual string HeaderRefNbr
		{
			get;
			set;
		}
		#endregion
		#region ExtTranID
		public abstract class extTranID : PX.Data.BQL.BqlString.Field<extTranID> { }

		/// <summary>
		/// The external identifier of the transaction.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Ext. Tran. ID", Visible = false)]
		public virtual string ExtTranID
		{
			get;
			set;
		}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }

		/// <summary>
		/// The balance type of the bank transaction.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"D"</c>: Receipt,
		/// <c>"C"</c>: Disbursement
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(CADrCr.CACredit)]
		[CADrCr.List]
		[PXUIField(DisplayName = "DrCr")]
		public virtual string DrCr
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		/// <summary>
		/// The identifier of currency of the bank transaction.
		/// </summary>
		[PXDBString(5, IsUnicode = true)]
		[PXDefault]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Currency")]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

		/// <summary>
		/// The identifier of the exchange rate record for the bank transaction amount.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CurrencyInfo.CuryInfoID"/> field.
		/// </value>
		[PXDBLong]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

		/// <summary>
		/// The transaction date.
		/// </summary>
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Tran. Date")]
		public virtual DateTime? TranDate
		{
			get;
			set;
		}

		#endregion
		#region MatchingPaymentDate
		public abstract class matchingPaymentDate : PX.Data.BQL.BqlDateTime.Field<matchingPaymentDate> { }

		[PXDBDate]
		[PXDefault(typeof(CABankTran.tranDate))]
		[PXUIField(DisplayName = "Payment Date")]
		public virtual DateTime? MatchingPaymentDate
		{
			get;
			set;
		}

		#endregion
		#region MatchingFinPeriodID
		public abstract class matchingfinPeriodID : PX.Data.BQL.BqlString.Field<matchingfinPeriodID> { }

		[CAAPAROpenPeriod(
			origModule: typeof(origModule),
			sourceType: typeof(matchingPaymentDate),
			branchSourceType: typeof(cashAccountID),
			branchSourceFormulaType: typeof(Selector<cashAccountID, CashAccount.branchID>),
			masterFinPeriodIDType: typeof(tranPeriodID),
			RedefaultOnDateChanged = true,
			IsHeader = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Fin. Period")]
		public virtual string MatchingFinPeriodID
		{
			get;
			set;
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : IBqlField { }

		[GL.PeriodID]
		public virtual string TranPeriodID
		{
			get;
			set;
		}
		#endregion
		#region TranEntryDate
		public abstract class tranEntryDate : PX.Data.BQL.BqlDateTime.Field<tranEntryDate> { }

		/// <summary>
		/// The bank transaction entry date.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Tran. Entry Date", Visible = false)]
		public virtual DateTime? TranEntryDate
		{
			get;
			set;
		}
		#endregion
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }

		/// <summary>
		/// The amount of the bank transaction in the selected currency.
		/// </summary>
	    [CM.PXDBCury(typeof(CABankTran.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "CuryTranAmt")]
		public virtual decimal? CuryTranAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigCuryID
		public abstract class origCuryID : PX.Data.BQL.BqlString.Field<origCuryID> { }

		/// <summary>
		/// The currency of the matching document.
		/// </summary>
		[PXDBString(5, IsUnicode = true)]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Orig. Currency", Visible = false)]
		public virtual string OrigCuryID
		{
			get;
			set;
		}
		#endregion

		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }

		/// <summary>
		/// The external reference number of the transaction.
		/// </summary>
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Ext. Ref. Nbr.")]
		public virtual string ExtRefNbr
		{
			get;
			set;
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

		/// <summary>
		/// The description of the bank transaction.
		/// </summary>
		[PXDBString(512, IsUnicode = true)]
		[PXUIField(DisplayName = "Tran. Desc")]
		public virtual string TranDesc
		{
			get;
			set;
		}
		#endregion
		#region UserDesc
		public abstract class userDesc : PX.Data.BQL.BqlString.Field<userDesc> { }

		/// <summary>
		/// The description of the bank transaction.
		/// You can use this field to specify a user description of the bank transaction while keeping the original bank description (<see cref="TranDesc"/>) untouched.
		/// </summary>
		[PXDBString(Common.Constants.TranDescLength512, IsUnicode = true)]
		[PXUIField(DisplayName = "Custom Tran. Desc.", Enabled = true, Visible = false)]
		public virtual string UserDesc
		{
			get;
			set;
		}
		#endregion
		#region PayeeName
		public abstract class payeeName : PX.Data.BQL.BqlString.Field<payeeName> { }

		/// <summary>
		/// The payee name, if any, specified for a transaction.
		/// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Payee/Payer", Visible = false)]
		public virtual string PayeeName
		{
			get;
			set;
		}
		#endregion
		#region PayeeAddress1
		public abstract class payeeAddress1 : PX.Data.BQL.BqlString.Field<payeeAddress1> { }

		/// <summary>
		/// The payee address, if any, specified for a transaction.
		/// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Payee Address1", Visible = false)]
		public virtual string PayeeAddress1
		{
			get;
			set;
		}
		#endregion
		#region PayeeCity
		public abstract class payeeCity : PX.Data.BQL.BqlString.Field<payeeCity> { }

		/// <summary>
		/// The payee city, if any, specified for a transaction.
		/// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Payee City", Visible = false)]
		public virtual string PayeeCity
		{
			get;
			set;
		}
		#endregion
		#region PayeeState
		public abstract class payeeState : PX.Data.BQL.BqlString.Field<payeeState> { }

		/// <summary>
		/// The payee state, if any, specified for a transaction.
		/// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Payee State", Visible = false)]
		public virtual string PayeeState
		{
			get;
			set;
		}
		#endregion
		#region PayeePostalCode
		public abstract class payeePostalCode : PX.Data.BQL.BqlString.Field<payeePostalCode> { }

		/// <summary>
		/// The payee postal code, if any, specified for a transaction.
		/// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Payee Postal Code", Visible = false)]
		public virtual string PayeePostalCode
		{
			get;
			set;
		}
		#endregion
		#region PayeePhone
		public abstract class payeePhone : PX.Data.BQL.BqlString.Field<payeePhone> { }

		/// <summary>
		/// The payee phone, if any, specified for a transaction.
		/// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Payee Phone", Visible = false)]
		public virtual string PayeePhone
		{
			get;
			set;
		}
		#endregion
		#region TranCode
		public abstract class tranCode : PX.Data.BQL.BqlString.Field<tranCode> { }

		/// <summary>
		/// The external code from the bank.
		/// </summary>
		[PXDBString(35, IsUnicode = true)]
		[PXUIField(DisplayName = "Tran. Code", Visible = false)]
		public virtual string TranCode
		{
			get;
			set;
		}
		#endregion
		#region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }

		/// <summary>
		/// The original module of the matching document.
		/// This field is displayed on the Create Payment tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"AP"</c>: Accounts Payable,
		/// <c>"AR"</c>: Accounts Receivable,
		/// <c>"CA"</c>: Cash Management.
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[PXStringList(new string[] { GL.BatchModule.AP, GL.BatchModule.AR, GL.BatchModule.CA },
			new string[] { GL.Messages.ModuleAP, GL.Messages.ModuleAR, GL.Messages.ModuleCA })]
		[PXUIField(DisplayName = "Module", Enabled = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string OrigModule
		{
			get;
			set;
		}
		#endregion
		#region PayeeBAccountID
		public abstract class payeeBAccountID : PX.Data.BQL.BqlInt.Field<payeeBAccountID> { }

		/// <summary>
		/// The vendor or customer associated with the document, by its business account ID.
		/// This field is displayed if the <c>"AP"</c> or <c>"AR"</c> option is selected in the <see cref="OrigModule"/> field.
		/// This field is displayed on the Create Payment tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXVendorCustomerSelector(typeof(CABankTran.origModule), true)]
		[PXUIField(DisplayName = "Business Account", Visible = false)]
		public virtual int? PayeeBAccountID
		{
			get;
			set;
		}
		#endregion
		#region PayeeLocationID
		public abstract class payeeLocationID : PX.Data.BQL.BqlInt.Field<payeeLocationID> { }

		/// <summary>
		/// The location of the vendor or customer. 
		/// This field is displayed if the <c>"AP"</c> or <c>"AR"</c> option is selected in the <see cref="OrigModule"/> field.
		/// This field is displayed on the Create Payment tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<CABankTran.payeeBAccountID>>>),
			DisplayName = "Location", DescriptionField = typeof(Location.descr), Visible = false)]
		[PXDefault(typeof(Search<BAccountR.defLocationID, Where<BAccountR.bAccountID, Equal<Current<CABankTran.payeeBAccountID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? PayeeLocationID
		{
			get;
			set;
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }

		/// <summary>
		/// The payment method used by a customer or vendor for the document. 
		/// This field is displayed if the <c>"AP"</c> or <c>"AR"</c> option is selected in the <see cref="OrigModule"/> field.
		/// This field is displayed on the Create Payment tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Coalesce<Coalesce<Search2<Customer.defPaymentMethodID, InnerJoin<PaymentMethod,
						On<PaymentMethod.paymentMethodID, Equal<Customer.defPaymentMethodID>, And<PaymentMethod.useForAR, Equal<True>>>,
						InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID, Equal<Customer.defPaymentMethodID>,
								And<PaymentMethodAccount.useForAR, Equal<True>, And<PaymentMethodAccount.cashAccountID, Equal<Current<CABankTran.cashAccountID>>>>>>>,
					Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAR>, And<Customer.bAccountID, Equal<Current<CABankTran.payeeBAccountID>>>>>,
				Search2<PaymentMethodAccount.paymentMethodID, InnerJoin<PaymentMethod, On<PaymentMethodAccount.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
						And<PaymentMethodAccount.cashAccountID, Equal<Current<CABankTran.cashAccountID>>,
							And<PaymentMethodAccount.useForAR, Equal<True>>>>>,
					Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAR>, And<PaymentMethod.useForAR, Equal<True>,
							And<PaymentMethod.isActive, Equal<boolTrue>>>>, OrderBy<Asc<PaymentMethodAccount.aRIsDefault, Desc<PaymentMethodAccount.paymentMethodID>>>>>,
			Coalesce<Search2<Location.vPaymentMethodID, InnerJoin<Vendor, On<Location.bAccountID, Equal<Vendor.bAccountID>,
									And<Location.locationID, Equal<Vendor.defLocationID>>>,
							InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID, Equal<Location.vPaymentMethodID>,
								And<PaymentMethodAccount.useForAP, Equal<True>, And<PaymentMethodAccount.cashAccountID, Equal<Current<CABankTran.cashAccountID>>>>>>>,
					Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAP>,
						And<Vendor.bAccountID, Equal<Current<CABankTran.payeeBAccountID>>>>>,
				Search2<PaymentMethodAccount.paymentMethodID, InnerJoin<PaymentMethod, On<PaymentMethodAccount.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
						And<PaymentMethodAccount.cashAccountID, Equal<Current<CABankTran.cashAccountID>>,
							And<PaymentMethodAccount.useForAP, Equal<True>>>>>,
					Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAP>,
						And<PaymentMethod.useForAP, Equal<True>, And<PaymentMethod.isActive, Equal<boolTrue>>>>,
					OrderBy<Asc<PaymentMethodAccount.aPIsDefault, Desc<PaymentMethodAccount.paymentMethodID>>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search2<PaymentMethod.paymentMethodID, InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID,
				Equal<PaymentMethod.paymentMethodID>, And<PaymentMethodAccount.cashAccountID, Equal<Current<CABankTran.cashAccountID>>,
					And<Where2<Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAP>,
					And<PaymentMethodAccount.useForAP, Equal<True>>>, Or<Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAR>,
					And<PaymentMethodAccount.useForAR, Equal<True>>>>>>>>>, Where<PaymentMethod.isActive, Equal<boolTrue>,
				And<Where2<Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAP>,
				And<PaymentMethod.useForAP, Equal<True>>>, Or<Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAR>,
				And<PaymentMethod.useForAR, Equal<True>>>>>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXUIField(DisplayName = "Payment Method", Visible = false)]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }

		/// <summary>
		/// The identifier of the credit card or account that is used by a customer or vendor for the document.
		/// This field is displayed on the Create Payment tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Card/Account Nbr.", Visible = false)]
		[PXDefault(typeof(Coalesce<Search2<Customer.defPMInstanceID, InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
							And<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>>, Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAR>,
							And<Customer.bAccountID, Equal<Current<CABankTran.payeeBAccountID>>, And<CustomerPaymentMethod.isActive, Equal<True>,
							And<CustomerPaymentMethod.paymentMethodID, Equal<Current<CABankTran.paymentMethodID>>>>>>>,
			Search<CustomerPaymentMethod.pMInstanceID, Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAR>,
					And<CustomerPaymentMethod.bAccountID, Equal<Current<CABankTran.payeeBAccountID>>,
						And<CustomerPaymentMethod.paymentMethodID, Equal<Current<CABankTran.paymentMethodID>>,
							And<CustomerPaymentMethod.isActive, Equal<True>>>>>,
				OrderBy<Desc<CustomerPaymentMethod.expirationDate,
					Desc<CustomerPaymentMethod.pMInstanceID>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID,
			Where<CustomerPaymentMethod.bAccountID, Equal<Current<CABankTran.payeeBAccountID>>,
				And<CustomerPaymentMethod.paymentMethodID, Equal<Current<CABankTran.paymentMethodID>>,
					And<CustomerPaymentMethod.isActive, Equal<boolTrue>>>>>),
			DescriptionField = typeof(CustomerPaymentMethod.descr))]
		[DeprecatedProcessing]
		[DisabledProcCenter]
		public virtual int? PMInstanceID
		{
			get;
			set;
		}
		#endregion
		#region InvoiceInfo
		public abstract class invoiceInfo : PX.Data.BQL.BqlString.Field<invoiceInfo> { }

		/// <summary>
		/// The reference number of the document (invoice or bill) generated to match a payment. 
		/// This field is displayed if the <c>"AP"</c> or <c>"AR"</c> option is selected in the <see cref="OrigModule"/> field.
		/// This field is displayed on the Create Payment tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Invoice Nbr.", Visible = false)]
		public virtual string InvoiceInfo
		{
			get;
			set;
		}
		#endregion
		#region DocumentMatched
		public abstract class documentMatched : PX.Data.BQL.BqlBool.Field<documentMatched> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that this bank transaction is matched to the payment and ready to be processed. 
		/// That is, the bank transaction has been matched to an existing transaction in the system, or details of a new document that matches this transaction have been specified.
		/// </summary>
		[PXDBBool]
		[PXHeaderImage(Sprite.AliasControl + "@" + Sprite.Control.CompleteHead)]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Matched", Visible = true, Enabled = false)]
		public virtual bool? DocumentMatched
		{
			get;
			set;
		}
		#endregion
		#region RuleApplied
		public abstract class ruleApplied : PX.Data.BQL.BqlBool.Field<ruleApplied> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the rule was applied to clear the transaction on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Rule Applied", Visible = false, Visibility = PXUIVisibility.Invisible, Enabled = false)]
		public virtual bool? RuleApplied
		{
			[PXDependsOnFields(typeof(ruleID), typeof(createDocument))]
			get
			{
				return this.CreateDocument == true && this.RuleID != null && this.OrigModule == GL.BatchModule.CA;
			}
		}
		#endregion
		#region ApplyRuleEnabled
		public abstract class applyRuleEnabled : PX.Data.BQL.BqlBool.Field<applyRuleEnabled> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the button <c>Create Rule</c> is enabled.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Create Rule Enabled", Visible = false, Visibility = PXUIVisibility.Invisible, Enabled = false)]
		public virtual bool? ApplyRuleEnabled
		{
			[PXDependsOnFields(typeof(ruleID), typeof(createDocument))]
			get
			{
				return this.CreateDocument == true && this.RuleID == null && this.OrigModule == GL.BatchModule.CA;
			}
		}
		#endregion
		#region MatchedToExisting
		public abstract class matchedToExisting : PX.Data.BQL.BqlBool.Field<matchedToExisting> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that this bank transaction is matched to the transaction in the system.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Matched", Visible = true, Enabled = false)]
		public virtual bool? MatchedToExisting
		{
			get;
			set;
		}
		#endregion
		#region MatchedToInvoice
		public abstract class matchedToInvoice : PX.Data.BQL.BqlBool.Field<matchedToInvoice> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that this bank transaction is matched to the invoice.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Matched to Invoice", Visible = false, Enabled = false)]
		public virtual bool? MatchedToInvoice
		{
			get;
			set;
		}
		#endregion
		#region MatchedToExpenseReceipt
		public abstract class matchedToExpenseReceipt : PX.Data.BQL.BqlBool.Field<matchedToExpenseReceipt> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that this bank transaction is matched to the Expense Receipt.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Matched To Expense Receipt", Visible = false, Enabled = false)]
		public virtual bool? MatchedToExpenseReceipt
		{
			get;
			set;
		}
		#endregion
		#region CreateDocument
		public abstract class createDocument : PX.Data.BQL.BqlBool.Field<createDocument> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that a new payment will be created for the selected bank transactions. 
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Create")]
		public virtual bool? CreateDocument
		{
			get;
			set;
		}
		#endregion
		#region MultipleMatching
		public abstract class multipleMatching : PX.Data.BQL.BqlBool.Field<multipleMatching>
		{
		}
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the selected bank transaction can be matched to multiple invoices.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Match to Multiple Documents")]
		public virtual bool? MultipleMatching
		{
			get;
			set;
		}
		#endregion
		#region MultipleMatchingToPayments
		public abstract class multipleMatchingToPayments : PX.Data.BQL.BqlBool.Field<multipleMatchingToPayments>
		{
		}
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the selected bank transaction can be matched to multiple payments.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Match to Multiple Payments")]
		public virtual bool? MultipleMatchingToPayments
		{
			get;
			set;
		}
		#endregion
		#region MatchReceiptsAndDisbursements
		public abstract class matchReceiptsAndDisbursements : PX.Data.BQL.BqlBool.Field<matchReceiptsAndDisbursements>
		{
		}
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the selected bank transaction can be matched to multiple documents with any amount and any direction.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Match to Receipts and Disbursements")]
		public virtual bool? MatchReceiptsAndDisbursements
		{
			get;
			set;
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		/// <summary>
		/// The status of the bank transaction.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"M"</c>: The bank transaction is matched to the payment and ready to be processed.
		/// <c>"I"</c>: The bank transaction is matched to the invoice.
		/// <c>"C"</c>: The bank transactions will be matched to a new payment.
		/// <c>"H"</c>: The bank transaction is hidden from the statement on the Process Bank Transactions (CA306000) form.
		/// <c>string.Empty</c>: The <see cref="DocumentMatched"/>, <see cref="MatchedToInvoice"/>, <see cref="CreateDocument"/>, and <see cref="Hidden"/> flags are set to <c>false</c>.
		/// </value>
		[PXString(1, IsFixed = true)]
		[CABankTranStatus.List]
		[PXUIField(DisplayName = "Match Type", Visibility = PXUIVisibility.SelectorVisible, Visible = false, Enabled = false)]
		public virtual string Status
		{
			[PXDependsOnFields(typeof(hidden), typeof(createDocument), typeof(matchedToInvoice), typeof(documentMatched), typeof(matchedToExpenseReceipt))]
			get
			{
				if (this.Hidden == true)
				{
					return CABankTranStatus.Hidden;
				}
				if (MatchedToExpenseReceipt == true)
				{
					return CABankTranStatus.ExpenseReceiptMatched;
				}
				if (this.CreateDocument == true)
				{
					return CABankTranStatus.Created;
				}
				if (this.MatchedToInvoice == true)
				{
					return CABankTranStatus.InvoiceMatched;
				}
				if (this.DocumentMatched == true)
				{
					return CABankTranStatus.Matched;
				}
				return string.Empty;
			}
		}
		#endregion
		#region Processed
		public abstract class processed : PX.Data.BQL.BqlBool.Field<processed> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that this bank transaction is processed.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Processed")]
		public virtual bool? Processed
		{
			get;
			set;
		}
		#endregion
		#region EntryTypeID
		public abstract class entryTypeID : PX.Data.BQL.BqlString.Field<entryTypeID> { }

		/// <summary>
		/// The identifier of an entry type that is used as a template for a new cash transaction to be created to match the selected bank transaction. 
		/// The field is displayed if the <c>CA</c> option is selected in the <see cref="OrigModule"/> field.
		/// This field is displayed on the Create Payment tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CAEntryType.EntryTypeId"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search2<CAEntryType.entryTypeId,
			InnerJoin<CashAccountETDetail, On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>,
			Where<CashAccountETDetail.cashAccountID, Equal<Current<CABankTran.cashAccountID>>,
				And<CAEntryType.module, Equal<GL.BatchModule.moduleCA>,
					And<Where<CAEntryType.drCr, Equal<Current<CABankTran.drCr>>>>>>>),
			DescriptionField = typeof(CAEntryType.descr))]
		[PXUIField(DisplayName = "Entry Type ID", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual string EntryTypeID
		{
			get;
			set;
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

		/// <summary>
		/// The tax zone that applies to the bank transaction.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="TaxZone.TaxZoneID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		[PXDefault(typeof(Search<CashAccountETDetail.taxZoneID,
						   Where<CashAccountETDetail.cashAccountID, Equal<Current<CABankTran.cashAccountID>>,
							 And<CashAccountETDetail.entryTypeID, Equal<Current<CABankTran.entryTypeID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string TaxZoneID
		{
			get;
			set;
		}
		#endregion
		#region TaxCalcMode
		public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }

		/// <summary>
		/// The tax calculation mode, which defines which amounts (tax-inclusive or tax-exclusive) 
		/// should be entered in the detail lines of a document. 
		/// This field is displayed only if the <see cref="FeaturesSet.NetGrossEntryMode"/> field is set to <c>true</c>.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"T"</c> (Tax Settings): The tax amount for the document is calculated according to the settings of the applicable tax or taxes.
		/// <c>"G"</c> (Gross): The amount in the document detail line includes a tax or taxes.
		/// <c>"N"</c> (Net): The amount in the document detail line does not include taxes.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<CashAccountETDetail.taxCalcMode,
			Where<CashAccountETDetail.cashAccountID, Equal<Current<CABankTran.cashAccountID>>,
				And<CashAccountETDetail.entryTypeID, Equal<Current<CABankTran.entryTypeID>>>>>))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode
		{
			get;
			set;
		}
		#endregion
		#region ChargeTypeID
		public abstract class chargeTypeID : PX.Data.BQL.BqlString.Field<chargeTypeID> { }

		/// <summary>
		/// The identifier of an entry type that is used as a template for a new cash transaction to be created to match the selected bank transaction. 
		/// The field is displayed if the <c>Multiple Documents</c> option is selected.
		/// This field is displayed on the Match to Invoices tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CAEntryType.EntryTypeId"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search2<CAEntryType.entryTypeId,
			InnerJoin<CashAccountETDetail, On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>,
			Where<CashAccountETDetail.cashAccountID, Equal<Current<CABankTran.cashAccountID>>,
				And<CAEntryType.module, Equal<GL.BatchModule.moduleCA>>>>),
			DescriptionField = typeof(CAEntryType.descr))]
		[PXUIField(DisplayName = "Charge Type", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual string ChargeTypeID
		{
			get;
			set;
		}
		#endregion
		#region ChargeTaxZoneID
		public abstract class chargeTaxZoneID : PX.Data.BQL.BqlString.Field<chargeTaxZoneID> { }

		/// <summary>
		/// The tax zone that applies to the bank transaction.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="TaxZone.TaxZoneID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Charge Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		[PXDefault(typeof(Search<CashAccountETDetail.taxZoneID,
						   Where<CashAccountETDetail.cashAccountID, Equal<Current<CABankTran.cashAccountID>>,
							 And<CashAccountETDetail.entryTypeID, Equal<Current<CABankTran.chargeTypeID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<chargeTypeID>))]
		public virtual string ChargeTaxZoneID
		{
			get;
			set;
		}
		#endregion
		#region ChargeTaxCalcMode
		public abstract class chargeTaxCalcMode : PX.Data.BQL.BqlString.Field<chargeTaxCalcMode> { }

		/// <summary>
		/// The tax calculation mode, which defines which amounts (tax-inclusive or tax-exclusive) 
		/// should be entered in the detail lines of a document. 
		/// This field is displayed only if the <see cref="FeaturesSet.NetGrossEntryMode"/> field is set to <c>true</c>.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"T"</c> (Tax Settings): The tax amount for the document is calculated according to the settings of the applicable tax or taxes.
		/// <c>"G"</c> (Gross): The amount in the document detail line includes a tax or taxes.
		/// <c>"N"</c> (Net): The amount in the document detail line does not include taxes.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TaxCalculationMode.TaxSetting, typeof(Search<CashAccountETDetail.taxCalcMode,
			Where<CashAccountETDetail.cashAccountID, Equal<Current<CABankTran.cashAccountID>>,
				And<CashAccountETDetail.entryTypeID, Equal<Current<CABankTran.chargeTypeID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Charge Tax Calculation Mode")]
		[PXFormula(typeof(Default<chargeTypeID>))]
		public virtual string ChargeTaxCalcMode
		{
			get;
			set;
		}
		#endregion
		#region ChargeDrCr
		public abstract class chargeDrCr : PX.Data.BQL.BqlString.Field<chargeDrCr> { }

		[PXDefault(typeof(Search<CAEntryType.drCr, Where<CAEntryType.entryTypeId, Equal<Current<CABankTran.chargeTypeID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(1, IsFixed = true)]
		[CADrCr.List]
		public virtual string ChargeDrCr
		{
			[PXDependsOnFields(typeof(chargeTypeID))]
			get;
			set;
		}
		#endregion
		#region CuryDebitAmt
		public abstract class curyDebitAmt : PX.Data.BQL.BqlDecimal.Field<curyDebitAmt> { }

		/// <summary>
		/// The amount of the receipt in the selected currency.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXCury(typeof(CABankTran.curyID))]
		[PXUIField(DisplayName = "Receipt")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<CABankTranHeader.curyDebitsTotal>))]
		public virtual decimal? CuryDebitAmt
		{
			[PXDependsOnFields(typeof(drCr), typeof(curyTranAmt))]
			get
			{
				return (this.DrCr == CADrCr.CADebit) ? this.CuryTranAmt : decimal.Zero;
			}

			set
			{
				if (value != 0m)
				{
					this.CuryTranAmt = value;
					this.DrCr = CADrCr.CADebit;
				}
				else if (this.DrCr == CADrCr.CADebit)
				{
					this.CuryTranAmt = 0m;
				}
			}
		}
		#endregion
		#region CuryCreditAmt
		public abstract class curyCreditAmt : PX.Data.BQL.BqlDecimal.Field<curyCreditAmt> { }

		/// <summary>
		/// The amount of the disbursement in the selected currency.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXCury(typeof(CABankTran.curyID))]
		[PXUIField(DisplayName = "Disbursement")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<CABankTranHeader.curyCreditsTotal>))]
		public virtual decimal? CuryCreditAmt
		{
			[PXDependsOnFields(typeof(drCr), typeof(curyTranAmt))]
			get
			{
				return (this.DrCr == CADrCr.CACredit) ? -this.CuryTranAmt : decimal.Zero;
			}

			set
			{
				if (value != 0m)
				{
					this.CuryTranAmt = -value;
					this.DrCr = CADrCr.CACredit;
				}
				else if (this.DrCr == CADrCr.CACredit)
				{
					this.CuryTranAmt = 0m;
				}
			}
		}
		#endregion
		#region CuryTotalAmt
		public abstract class curyTotalAmt : PX.Data.BQL.BqlDecimal.Field<curyTotalAmt> { }

		/// <summary>
		/// The total amount of the created document in the selected currency.
		/// This field is displayed if the <c>"AP"</c> or <c>"AR"</c> option is selected in the <see cref="OrigModule"/> field.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Amount")]
		[PXCury(typeof(CABankTran.curyID))]
		public virtual decimal? CuryTotalAmt
		{
			get
			{
				return (this.DrCr == CADrCr.CACredit) ? (-1 * this.CuryTranAmt) : this.CuryTranAmt;
			}

			set
			{
			}
		}
		#endregion
		#region CuryTotalAmtCopy
		public abstract class curyTotalAmtCopy : PX.Data.BQL.BqlDecimal.Field<curyTotalAmtCopy> { }

		/// <summary>
		/// The copy of the <see cref="CuryTotalAmt"/> field.
		/// The total amount of the created document in the selected currency.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Transaction Amount")]
		[PXCury(typeof(CABankTran.curyID))]
		public virtual decimal? CuryTotalAmtCopy
		{
			get
			{
				return this.CuryTotalAmt;
			}
			set
			{ }
		}
		#endregion
		#region CuryDetailsWithTaxesTotal
		public abstract class curyDetailsWithTaxesTotal : PX.Data.BQL.BqlDecimal.Field<curyDetailsWithTaxesTotal> { }

		/// <summary>
		/// The sum of all details and exclusive taxes in the selected currency.
		/// </summary>
		[PXDBCurrency(typeof(CABankTran.curyInfoID), typeof(CABankTran.detailsWithTaxesTotal))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryDetailsWithTaxesTotal
		{
			get;
			set;
		}
		#endregion
		#region DetailsWithTaxesTotal
		public abstract class detailsWithTaxesTotal : PX.Data.BQL.BqlDecimal.Field<detailsWithTaxesTotal> { }

		/// <summary>
		/// The sum of all details and exclusive taxes in the base currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? DetailsWithTaxesTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }

		/// <summary>
		/// The total amount of tax paid on the document in the selected currency.
		/// </summary>
		[PXDBCurrency(typeof(CABankTran.curyInfoID), typeof(CABankTran.taxTotal))]
		[PXUIField(DisplayName = "Tax Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryTaxTotal
		{
			get;
			set;
		}
		#endregion
		#region TaxTotal
		public abstract class taxTotal : PX.Data.BQL.BqlDecimal.Field<taxTotal> { }

		/// <summary>
		/// The total amount of tax paid on the document in the base currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryTotalAmtDisplay
		public abstract class curyTotalAmtDisplay : PX.Data.BQL.BqlDecimal.Field<curyTotalAmtDisplay> { }

		/// <summary>
		/// The copy of the <see cref="CuryTotalAmt"/> field.
		/// The total amount of the created document in the selected currency.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Transaction Amount")]
		[PXCury(typeof(CABankTran.curyID))]
		public virtual decimal? CuryTotalAmtDisplay
		{
			get
			{
				return this.CuryTotalAmt;
			}
		}
		#endregion
		#region CuryApplAmtCA
		public abstract class curyApplAmtCA : PX.Data.BQL.BqlDecimal.Field<curyApplAmtCA> { }

		/// <summary>
		/// The amount of the transaction for which the documents (to match the bank transaction) are added.
		/// Represented in the selected currency.
		/// This field is displayed if the <c>CA</c> option is selected in the <see cref="OrigModule"/> field.
		/// </summary>
	    [CM.PXDBCury(typeof(CABankTran.curyID))]
		[PXUIField(DisplayName = "Detail Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryApplAmtCA
		{
			get;
			set;
		}
		#endregion
		#region CuryUnappliedBalCA
		public abstract class curyUnappliedBalCA : PX.Data.BQL.BqlDecimal.Field<curyUnappliedBalCA> { }

		/// <summary>
		/// The balance of the transaction for which you can add the documents. 
		/// This field is displayed if the <c>CA</c> option is selected in the <see cref="OrigModule"/> field.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(unappliedBalCA))]
		[PXUIField(DisplayName = "Discrepancy", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryUnappliedBalCA
		{
			get;
			set;
		}
		#endregion
		#region UnappliedBalCA
		public abstract class unappliedBalCA : PX.Data.BQL.BqlDecimal.Field<unappliedBalCA> { }

		/// <summary>
		/// The amount of the transaction in the base currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? UnappliedBalCA
		{
			get;
			set;
		}
		#endregion
		#region CuryApplAmt
		public abstract class curyApplAmt : PX.Data.BQL.BqlDecimal.Field<curyApplAmt> { }

		/// <summary>
		/// The amount of the application for this payment. 
		/// This field is displayed if the <c>"AP"</c> or <c>"AR"</c> option is selected in the <see cref="OrigModule"/> field.
		/// </summary>
	    [CM.PXDBCury(typeof(CABankTran.curyID))]
		[PXUIField(DisplayName = "Application Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryApplAmt
		{
			get;
			set;
		}

		#endregion
		#region CuryUnappliedBal
		public abstract class curyUnappliedBal : PX.Data.BQL.BqlDecimal.Field<curyUnappliedBal> { }

		/// <summary>
		/// The unapplied balance of the document in the selected currency.
		/// This field is displayed if the <c>"AP"</c> or <c>"AR"</c> option is selected in the <see cref="OrigModule"/> field.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXCury(typeof(CABankTran.curyID))]
		[PXUIField(DisplayName = "Unapplied Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryUnappliedBal
		{
			[PXDependsOnFields(typeof(curyTotalAmt), typeof(curyApplAmt))]
			get
			{
				return (this.CuryTotalAmt ?? decimal.Zero) - (this.CuryApplAmt ?? decimal.Zero);
			}

			set
			{
			}
		}
		#endregion
		#region CuryApplAmtMatch
		public abstract class curyApplAmtMatch : PX.Data.BQL.BqlDecimal.Field<curyApplAmtMatch> { }

		/// <summary>
		/// The amount of the application for this payment. 
		/// This field is displayed if the <c>"AP"</c> or <c>"AR"</c> option is selected in the <see cref="OrigModule"/> field.
		/// </summary>
		[CM.PXDBCury(typeof(CABankTran.curyID))]
		[PXUIField(DisplayName = "Matched Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryApplAmtMatch
		{
			get;
			set;
		}
		#endregion
		#region CuryApplAmtMatchToInvoice
		public abstract class curyApplAmtMatchToInvoice : PX.Data.BQL.BqlDecimal.Field<curyApplAmtMatchToInvoice> { }

		/// <summary>
		/// The amount of the application for this payment. 
		/// This field is displayed if the <c>"AP"</c> or <c>"AR"</c> option is selected in the <see cref="OrigModule"/> field.
		/// </summary>
		[PXCury(typeof(CABankTran.curyID))]
		[PXUIField(DisplayName = "Matched Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryApplAmtMatchToInvoice
		{
			[PXDependsOnFields(typeof(matchedToInvoice), typeof(multipleMatching), typeof(matchedToExisting), typeof(drCr), typeof(chargeDrCr), typeof(curyApplAmtMatch), typeof(curyChargeAmt))]
			get
			{
				return (this.MatchedToInvoice == true || (this.MultipleMatching == true && this.MatchedToExisting == false))
					? this.CuryApplAmtMatch + ((this.ChargeDrCr == this.DrCr ? 1 : -1) * this.CuryChargeAmt ?? decimal.Zero)
					: decimal.Zero;
			}
		}
		#endregion
		#region CuryApplAmtMatchToPayment
		public abstract class curyApplAmtMatchToPayment : PX.Data.BQL.BqlDecimal.Field<curyApplAmtMatchToPayment> { }

		/// <summary>
		/// The amount of the application for this payment. 
		/// This field is displayed if the <c>"AP"</c> or <c>"AR"</c> option is selected in the <see cref="OrigModule"/> field.
		/// </summary>
		[PXCury(typeof(CABankTran.curyID))]
		[PXUIField(DisplayName = "Matched Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryApplAmtMatchToPayment
		{
			[PXDependsOnFields(typeof(matchedToInvoice), typeof(multipleMatchingToPayments), typeof(matchedToExisting), typeof(matchedToExpenseReceipt), typeof(curyApplAmtMatch))]
			get
			{
				return ((this.MatchedToExisting == true || this.MultipleMatchingToPayments == true) && this.MatchedToInvoice != true && this.MatchedToExpenseReceipt != true) ? this.CuryApplAmtMatch : decimal.Zero;
			}
		}
		#endregion
		#region CuryUnappliedBalMatch
		public abstract class curyUnappliedBalMatch : PX.Data.BQL.BqlDecimal.Field<curyUnappliedBalMatch> { }

		/// <summary>
		/// The unapplied balance of the document in the selected currency.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXCury(typeof(CABankTran.curyID))]
		[PXUIField(DisplayName = "Unmatched Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryUnappliedBalMatch
		{
			[PXDependsOnFields(typeof(curyTotalAmt), typeof(curyApplAmtMatch), typeof(chargeDrCr), typeof(curyChargeAmt))]
			get
			{
				return (this.CuryTotalAmt ?? decimal.Zero) - (this.CuryApplAmtMatch ?? decimal.Zero) - ((this.ChargeDrCr == this.DrCr ? 1 : -1) * this.CuryChargeAmt ?? decimal.Zero);
			}
			set { }
		}
		#endregion
		#region CuryVatExemptTotal
		public abstract class curyVatExemptTotal : PX.Data.BQL.BqlDecimal.Field<curyVatExemptTotal> { }

		/// <summary>
		/// The document total that is exempt from VAT in the selected currency. 
		/// This total is calculated as the taxable amount for the tax 
		/// with the <see cref="Tax.ExemptTax"/> field set to <c>true</c> (that is, the Include in VAT Exempt Total check box selected on the Taxes (TX205000) form).
		/// </summary>
		[PXDBCurrency(typeof(CABankTran.curyInfoID), typeof(CABankTran.vatExemptTotal))]
		[PXUIField(DisplayName = "VAT Exempt Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryVatExemptTotal
		{
			get;
			set;
		}
		#endregion
		#region VatExemptTaxTotal
		public abstract class vatExemptTotal : PX.Data.BQL.BqlDecimal.Field<vatExemptTotal> { }

		/// <summary>
		/// The document total that is exempt from VAT in the base currency. 
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? VatExemptTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryVatTaxableTotal
		public abstract class curyVatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyVatTaxableTotal> { }

		/// <summary>
		/// The document total that is subjected to VAT in the selected currency.
		/// The field is displayed only if 
		/// the <see cref="Tax.IncludeInTaxable"/> field is set to <c>true</c> (that is, the Include in VAT Exempt Total check box is selected on the Taxes (TX205000) form).
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(vatTaxableTotal))]
		[PXUIField(DisplayName = "VAT Taxable Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryVatTaxableTotal
		{
			get;
			set;
		}
		#endregion
		#region VatTaxableTotal
		public abstract class vatTaxableTotal : PX.Data.BQL.BqlDecimal.Field<vatTaxableTotal> { }

		/// <summary>
		/// The document total that is subjected to VAT in the base currency.
		/// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? VatTaxableTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxRoundDiff
		public abstract class curyTaxRoundDiff : PX.Data.BQL.BqlDecimal.Field<curyTaxRoundDiff> { }

		/// <summary>
		/// The difference between the original document amount and the rounded amount in the selected currency.
		/// </summary>
		[PXDBCurrency(typeof(CABankTran.curyInfoID), typeof(CABankTran.taxRoundDiff), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Rounding Diff.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public decimal? CuryTaxRoundDiff
		{
			get;
			set;
		}
		#endregion
		#region TaxRoundDiff
		public abstract class taxRoundDiff : PX.Data.BQL.BqlDecimal.Field<taxRoundDiff> { }

		/// <summary>
		/// The difference between the original document amount and the rounded amount in the base currency.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public decimal? TaxRoundDiff
		{
			get;
			set;
		}
		#endregion
		#region CuryChargeAmt
		public abstract class curyChargeAmt : PX.Data.BQL.BqlDecimal.Field<curyChargeAmt> { }

		/// <summary>
		/// The amount of the charge including tax (if applicable).
		/// </summary>
		[CM.PXDBCury(typeof(CABankTran.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Charge Amount")]
		public virtual decimal? CuryChargeAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryChargeTaxAmt
		public abstract class curyChargeTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyChargeTaxAmt> { }

		/// <summary>
		/// The amount of the charge including tax (if applicable).
		/// </summary>
		[CM.PXDBCury(typeof(CABankTran.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Charge Tax Amount")]
		public virtual decimal? CuryChargeTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryUnappliedBalMatchToInvoice
		public abstract class curyUnappliedBalMatchToInvoice : PX.Data.BQL.BqlDecimal.Field<curyUnappliedBalMatchToInvoice> { }

		/// <summary>
		/// The unapplied balance of the document in the selected currency.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXCury(typeof(CABankTran.curyID))]
		[PXUIField(DisplayName = "Unmatched Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryUnappliedBalMatchToInvoice
		{
			[PXDependsOnFields(typeof(matchedToInvoice), typeof(multipleMatching), typeof(matchedToExisting), typeof(curyUnappliedBalMatch))]
			get
			{
				return (this.MatchedToInvoice == true || (this.MultipleMatching == true && this.MatchedToExisting == false)) ? this.CuryUnappliedBalMatch : decimal.Zero;
			}
		}
		#endregion
		#region CuryUnappliedBalMatchToPayment
		public abstract class curyUnappliedBalMatchToPayment : PX.Data.BQL.BqlDecimal.Field<curyUnappliedBalMatchToPayment> { }

		/// <summary>
		/// The unapplied balance of the document in the selected currency.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXCury(typeof(CABankTran.curyID))]
		[PXUIField(DisplayName = "Unmatched Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryUnappliedBalMatchToPayment
		{
			[PXDependsOnFields(typeof(matchedToInvoice), typeof(multipleMatchingToPayments), typeof(matchedToExisting), typeof(matchedToExpenseReceipt), typeof(curyUnappliedBalMatch))]
			get
			{
				return ((this.MatchedToExisting == true || this.MultipleMatchingToPayments == true) && this.MatchedToInvoice != true && this.MatchedToExpenseReceipt != true) ? this.CuryUnappliedBalMatch : decimal.Zero;
			}
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		[PXString(3, IsFixed = true)]
		[PXDefault]
		[APPaymentType.List]
		[PXFieldDescription]
		public string DocType
		{
			get
			{
				if (this.OrigModule == GL.BatchModule.AP)
				{
					if (this.DrCr == CADrCr.CACredit)
					{
						return APDocType.Check;
					}
					return APDocType.Refund;
				}
				if (this.DrCr == CADrCr.CACredit)
				{
					return ARDocType.Refund;
				}
				return ARDocType.Payment;
			}

			set
			{
			}
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }

		/// <summary>
		/// The counter of related adjustments.
		/// The <c>PXParentAttribute</c> from the <see cref="CABankTranAdjustment.AdjNbr"/> field links on this field.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? LineCntr
		{
			get;
			set;
		}
		#endregion
		#region LineCntrCA
		public abstract class lineCntrCA : PX.Data.BQL.BqlInt.Field<lineCntrCA> { }

		/// <summary>
		/// The counter of related details.
		/// The <c>PXParentAttribute</c> from the <see cref="CABankTranDetail.LineNbr"/> field links on this field.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? LineCntrCA
		{
			get;
			set;
		}
		#endregion
		#region LineCntrMatch
		public abstract class lineCntrMatch : PX.Data.BQL.BqlInt.Field<lineCntrMatch> { }

		/// <summary>
		/// The counter of related details.
		/// The <c>PXParentAttribute</c> from the <see cref="CABankTranMatch.LineNbr"/> field links on this field.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? LineCntrMatch
		{
			get;
			set;
		}
		#endregion
		#region PayeeBAccountIDCopy
		public abstract class payeeBAccountIDCopy : PX.Data.BQL.BqlInt.Field<payeeBAccountIDCopy> { }

		/// <summary>
		/// The copy of the <see cref="PayeeBAccountID"/> field.
		/// This field is displayed on the Match to Invoices tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[PXInt]
		[PXSelector(typeof(Search<BAccountR.bAccountID>), SubstituteKey = typeof(BAccountR.acctCD), DescriptionField = typeof(BAccountR.acctName))]
		[PXUIField(DisplayName = "Business Account")]
		public virtual int? PayeeBAccountIDCopy
		{
			get
			{
				return this.PayeeBAccountID;
			}

			set
			{
				this.PayeeBAccountID = value;
			}
		}
		#endregion
		#region PayeeLocationIDCopy
		public abstract class payeeLocationIDCopy : PX.Data.BQL.BqlInt.Field<payeeLocationIDCopy> { }

		/// <summary>
		/// The copy of the <see cref="PayeeLocationID"/> field .
		/// This field is displayed on the Match to Invoices tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<CABankTran.payeeBAccountID>>>), DisplayName = "Location", DescriptionField = typeof(Location.descr), IsDBField = false)]
		public virtual int? PayeeLocationIDCopy
		{
			get
			{
				return this.PayeeLocationID;
			}

			set
			{
				this.PayeeLocationID = value;
			}
		}
		#endregion
		#region PaymentMethodIDCopy
		public abstract class paymentMethodIDCopy : PX.Data.BQL.BqlString.Field<paymentMethodIDCopy> { }

		/// <summary>
		/// The copy of the <see cref="PaymentMethodID"/> field.
		/// This field is displayed on the Match to Invoices tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[PXString(10, IsUnicode = true)]
		[PXSelector(typeof(Search2<PaymentMethod.paymentMethodID,
				InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID,
				Equal<PaymentMethod.paymentMethodID>,
				And<PaymentMethodAccount.cashAccountID, Equal<Current<CABankTran.cashAccountID>>,
					And<Where2<Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAP>, And<PaymentMethodAccount.useForAP, Equal<True>>>,
						Or<Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAR>, And<PaymentMethodAccount.useForAR, Equal<True>>>>>>>>>,
				Where<PaymentMethod.isActive, Equal<boolTrue>,
					And<Where2<Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAP>, And<PaymentMethod.useForAP, Equal<True>>>,
						Or<Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAR>, And<PaymentMethod.useForAR, Equal<True>>>>>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXUIField(DisplayName = "Payment Method", Visible = true)]
		public virtual string PaymentMethodIDCopy
		{
			get
			{
				return this.PaymentMethodID;
			}

			set
			{
				this.PaymentMethodID = value;
			}
		}
		#endregion
		#region PMInstanceIDCopy
		public abstract class pMInstanceIDCopy : PX.Data.BQL.BqlInt.Field<pMInstanceIDCopy> { }

		/// <summary>
		/// The copy of the <see cref="PMInstanceID"/> field.
		/// This field is displayed on the Match to Invoices tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[PXInt]
		[PXUIField(DisplayName = "Card/Account Nbr.")]
		[PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID,
									Where<CustomerPaymentMethod.bAccountID, Equal<Current<CABankTran.payeeBAccountID>>,
									  And<CustomerPaymentMethod.paymentMethodID, Equal<Current<CABankTran.paymentMethodID>>,
									  And<CustomerPaymentMethod.isActive, Equal<boolTrue>>>>>),
									  DescriptionField = typeof(CustomerPaymentMethod.descr))]
		public virtual int? PMInstanceIDCopy
		{
			get
			{
				return this.PMInstanceID;
			}

			set
			{
				this.PMInstanceID = value;
			}
		}
		#endregion
		#region RuleID
		public abstract class ruleID : PX.Data.BQL.BqlInt.Field<ruleID> { }

		/// <summary>
		/// The identifier of the rule that was applied to the bank transaction to create a document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CABankTranRule.RuleID"/> field.
		/// </value>
		[PXDBInt]
		[PXSelector(typeof(CABankTranRule.ruleID), DescriptionField = typeof(CABankTranRule.description))]
		[PXUIField(DisplayName = "Applied Rule", Enabled = false)]
		public int? RuleID
		{
			get;
			set;
		}
		#endregion
		#region Hidden
		public abstract class hidden : PX.Data.BQL.BqlBool.Field<hidden> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that this bank transaction has been hidden from the statement on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Hidden", Enabled = false)]
		public virtual bool? Hidden
		{
			get;
			set;
		}
		#endregion
		#region InvoiceNotFound
		public abstract class invoiceNotFound : PX.Data.BQL.BqlBool.Field<invoiceNotFound> { }

		/// <summary>
		/// Specifies (if set to <c>true</c>) that the invoice for matching to this bank transaction wasn't found.
		/// </summary>
		[PXDBBool]
		public bool? InvoiceNotFound
		{
			get;
			set;
		}
		#endregion
		#region CountMatches
		public abstract class countMatches : PX.Data.BQL.BqlInt.Field<countMatches> { }

		/// <summary>
		/// The count of matched payments.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXInt]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual int? CountMatches
		{
			get;
			set;
		}
		#endregion
		#region CountInvoiceMatches
		public abstract class countInvoiceMatches : PX.Data.BQL.BqlInt.Field<countInvoiceMatches> { }

		/// <summary>
		/// The count of matched invoices.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXInt]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual int? CountInvoiceMatches
		{
			get;
			set;
		}
		#endregion
		#region CountExpenseReceiptDetailMatches
		public abstract class countExpenseReceiptDetailMatches : PX.Data.BQL.BqlInt.Field<countExpenseReceiptDetailMatches> { }

		/// <summary>
		/// The count of matched expense receipts.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXInt]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual int? CountExpenseReceiptDetailMatches
		{
			get;
			set;
		}
		#endregion
		#region MatchStatsInfo
		public abstract class matchStatsInfo : PX.Data.BQL.BqlString.Field<matchStatsInfo> { }

		/// <summary>
		/// The user-friendly brief description of the status of the selected transaction.
		/// The field is displayed in the bottom of the table with bank transactions on the Process Bank Transactions (CA306000) form.
		/// This is a virtual field and it has no representation in the database.
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "MatchStatsInfo", Enabled = false, Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual string MatchStatsInfo
		{
			get;
			set;
		}
		#endregion
		#region AcctName
		public abstract class acctName : PX.Data.BQL.BqlInt.Field<acctName> { }

		/// <summary>
		/// The name of the vendor or customer associated with the document.
		/// </summary>
		[PXInt]
		[PXSelector(typeof(Search<BAccountR.bAccountID>), SubstituteKey = typeof(BAccountR.acctName))]
		[PXUIField(DisplayName = CR.Messages.BAccountName, Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual int? AcctName
		{
			get
			{
				return this.PayeeBAccountID;
			}

			set
			{
				this.PayeeBAccountID = value;
			}
		}
		#endregion
		#region PayeeBAccountID1
		public abstract class payeeBAccountID1 : PX.Data.BQL.BqlInt.Field<payeeBAccountID1> { }

		/// <summary>
		/// The copy of the <see cref="PayeeBAccountID"/> field.
		/// This field is displayed on the Match to Payments tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[PXInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<BAccountR.bAccountID>), SubstituteKey = typeof(BAccountR.acctCD), DescriptionField = typeof(BAccountR.acctName))]
		[PXUIField(DisplayName = "Business Account", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual int? PayeeBAccountID1
		{
			get
			{
				return this.PayeeBAccountID;
			}

			set
			{
				this.PayeeBAccountID = value;
			}
		}
		#endregion
		#region PayeeLocationID1
		public abstract class payeeLocationID1 : PX.Data.BQL.BqlInt.Field<payeeLocationID1> { }

		/// <summary>
		/// The copy of the <see cref="PayeeLocationID"/> field.
		/// This field is displayed on the Match to Payments tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[PXInt]
		[PXSelector(typeof(Search<Location.locationID, Where<Location.bAccountID, Equal<Current<CABankTran.payeeBAccountID>>>>), SubstituteKey = typeof(Location.locationCD), DescriptionField = typeof(Location.descr))]
		[PXUIField(DisplayName = "Location", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<BAccountR.defLocationID, Where<BAccountR.bAccountID, Equal<Current<CABankTran.payeeBAccountID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? PayeeLocationID1
		{
			get
			{
				return this.PayeeLocationID;
			}

			set
			{
				this.PayeeLocationID = value;
			}
		}
		#endregion
		#region PaymentMethodID1
		public abstract class paymentMethodID1 : PX.Data.BQL.BqlString.Field<paymentMethodID1> { }

		/// <summary>
		/// The copy of the <see cref="PaymentMethodID"/> field.
		/// This field is displayed on the Match to Payments tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(Coalesce<
							Coalesce<
								Search2<Customer.defPaymentMethodID,
									InnerJoin<PaymentMethod,
										On<PaymentMethod.paymentMethodID, Equal<Customer.defPaymentMethodID>,
										And<PaymentMethod.useForAR, Equal<True>>>,
									InnerJoin<PaymentMethodAccount,
										On<PaymentMethodAccount.paymentMethodID, Equal<Customer.defPaymentMethodID>,
										And<PaymentMethodAccount.useForAR, Equal<True>,
										And<PaymentMethodAccount.cashAccountID, Equal<Current<CABankTran.cashAccountID>>>>>>>,
									Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAR>,
										And<Customer.bAccountID, Equal<Current<CABankTran.payeeBAccountID>>>>>,
								Search2<PaymentMethodAccount.paymentMethodID,
									InnerJoin<PaymentMethod, On<PaymentMethodAccount.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
										And<PaymentMethodAccount.cashAccountID, Equal<Current<CABankTran.cashAccountID>>,
										And<PaymentMethodAccount.useForAR, Equal<True>>>>>,
									Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAR>,
										And<PaymentMethod.useForAR, Equal<True>,
										And<PaymentMethod.isActive, Equal<boolTrue>>>>, OrderBy<Asc<PaymentMethodAccount.aRIsDefault, Desc<PaymentMethodAccount.paymentMethodID>>>>>,
							Coalesce<
								Search2<Location.vPaymentMethodID,
									InnerJoin<Vendor, On<Location.bAccountID, Equal<Vendor.bAccountID>, And<Location.locationID, Equal<Vendor.defLocationID>>>,
									InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID, Equal<Location.vPaymentMethodID>,
										And<PaymentMethodAccount.useForAP, Equal<True>,
										And<PaymentMethodAccount.cashAccountID, Equal<Current<CABankTran.cashAccountID>>>>>>>,
									Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAP>,
										And<Vendor.bAccountID, Equal<Current<CABankTran.payeeBAccountID>>>>>,
								Search2<PaymentMethodAccount.paymentMethodID,
									InnerJoin<PaymentMethod, On<PaymentMethodAccount.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
										And<PaymentMethodAccount.cashAccountID, Equal<Current<CABankTran.cashAccountID>>,
										And<PaymentMethodAccount.useForAP, Equal<True>>>>>,
									Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAP>,
										And<PaymentMethod.useForAP, Equal<True>,
										And<PaymentMethod.isActive, Equal<boolTrue>>>>, OrderBy<Asc<PaymentMethodAccount.aPIsDefault, Desc<PaymentMethodAccount.paymentMethodID>>>>>>),
					PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search2<PaymentMethod.paymentMethodID,
				InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID,
				Equal<PaymentMethod.paymentMethodID>,
				And<PaymentMethodAccount.cashAccountID, Equal<Current<CABankTran.cashAccountID>>,
					And<Where2<Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAP>, And<PaymentMethodAccount.useForAP, Equal<True>>>,
						Or<Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAR>, And<PaymentMethodAccount.useForAR, Equal<True>>>>>>>>>,
				Where<PaymentMethod.isActive, Equal<boolTrue>,
					And<Where2<Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAP>, And<PaymentMethod.useForAP, Equal<True>>>,
						Or<Where<Current<CABankTran.origModule>, Equal<GL.BatchModule.moduleAR>, And<PaymentMethod.useForAR, Equal<True>>>>>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXUIField(DisplayName = "Payment Method", Visible = false)]
		public virtual string PaymentMethodID1
		{
			get
			{
				return this.PaymentMethodID;
			}

			set
			{
				this.PaymentMethodID = value;
			}
		}
		#endregion
		#region InvoiceInfo1
		public abstract class invoiceInfo1 : PX.Data.BQL.BqlString.Field<invoiceInfo1> { }

		/// <summary>
		/// The copy of the <see cref="InvoiceInfo"/> field.
		/// This field is displayed on the Match to Payments tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[PXString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Invoice Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual string InvoiceInfo1
		{
			get
			{
				return this.InvoiceInfo;
			}

			set
			{
				this.InvoiceInfo = value;
			}
		}
		#endregion
		#region EntryTypeID1
		public abstract class entryTypeID1 : PX.Data.BQL.BqlString.Field<entryTypeID1> { }

		/// <summary>
		/// The copy of the <see cref="EntryTypeID"/> field.
		/// This field is displayed on the Match to Payments tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[PXString(10, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search2<CAEntryType.entryTypeId,
							  InnerJoin<CashAccountETDetail, On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>,
							  Where<CashAccountETDetail.cashAccountID, Equal<Current<CABankTran.cashAccountID>>,
								And<CAEntryType.module, Equal<GL.BatchModule.moduleCA>,
								And<Where<CAEntryType.drCr, Equal<Current<CABankTran.drCr>>>>>>>),
					  DescriptionField = typeof(CAEntryType.descr))]
		[PXUIField(DisplayName = "Entry Type ID", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual string EntryTypeID1
		{
			get
			{
				return this.EntryTypeID;
			}

			set
			{
				this.EntryTypeID = value;
			}
		}
		#endregion
		#region OrigModule1
		public abstract class origModule1 : PX.Data.BQL.BqlString.Field<origModule1> { }

		/// <summary>
		/// The copy of the <see cref="OrigModule"/> field.
		/// This field is displayed on the Match to Payments tab of on the Process Bank Transactions (CA306000) form.
		/// </summary>
		[PXString(2, IsFixed = true)]
		[PXStringList(new string[] { GL.BatchModule.AP, GL.BatchModule.AR, GL.BatchModule.CA, }, new string[] { GL.Messages.ModuleAP, GL.Messages.ModuleAR, GL.Messages.ModuleCA })]
		[PXUIField(DisplayName = "Module", Enabled = false, Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string OrigModule1
		{
			get
			{
				return this.OrigModule;
			}

			set
			{
				this.OrigModule = value;
			}
		}
		#endregion
		#region CuryWOAmt
		public abstract class curyWOAmt : PX.Data.BQL.BqlDecimal.Field<curyWOAmt> { }

		/// <summary>
		/// The total amount of write-offs specified for documents to be applied in the selected currency. 
		/// This field is displayed if the <c>"AR"</c> option is selected in the <see cref="OrigModule"/> field.
		/// </summary>
		[PXCurrency(typeof(CABankTran.curyInfoID), typeof(CABankTran.wOAmt))]
		[PXUIField(DisplayName = "Write-Off Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryWOAmt
		{
			get;
			set;
		}
		#endregion
		#region WOAmt
		public abstract class wOAmt : PX.Data.BQL.BqlDecimal.Field<wOAmt> { }

		/// <summary>
		/// The total amount of write-offs specified for documents to be applied in the base currency. 
		/// </summary>
		[PXDecimal(4)]
		public virtual decimal? WOAmt
		{
			get;
			set;
		}
		#endregion
		#region CardNumber
		public abstract class cardNumber : BqlString.Field<cardNumber> { }

		[PXUIField(DisplayName = "Card Number")]
		[PXDBString(25)]
		public string CardNumber { get; set; }
		#endregion
		#region HasAdjustments
		[Obsolete("The field is obsoleted, use CountAdjustments instead")]
		public abstract class hasAdjustments : PX.Data.BQL.BqlBool.Field<hasAdjustments> { }

		[Obsolete("The field is obsoleted, use CountAdjustments instead")]
		[PXBool]
		[PXDBCalced(typeof(Switch<Case<Where<Exists<
			Select<CABankTranAdjustment,
					Where<CABankTranAdjustment.tranID, Equal<CABankTran.tranID>>>>>,
					True>, False>),
			typeof(bool))]
		public virtual bool? HasAdjustments
		{
			get;
			set;
		}
		#endregion

		#region CountAdjustments
		public abstract class countAdjustments : PX.Data.BQL.BqlInt.Field<countAdjustments> { }

		[PXDBInt]
		public virtual int? CountAdjustments
		{
			get;
			set;
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		[PXInt]
		public Int32? SortOrder
		{
			get;
			set;
		}
		#endregion
		#region Audit fields
		#region MatchReason
		public abstract class matchReason : BqlString.Field<matchReason>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
						new string[] { AutoMatch, Manual },
						new string[] { Messages.AutoMatch, Messages.Manual })
				{ }
			}

			public const string AutoMatch = "A";
			public const string Manual = "M";

			public class autoMatch : BqlString.Constant<autoMatch>
			{
				public autoMatch() : base(AutoMatch) { }
			}

			public class manual : BqlString.Constant<manual>
			{
				public manual() : base(Manual) { }
			}
		}

		[PXDBString(1)]
		[matchReason.List]
		[PXUIField(DisplayName = "Match Reason")]
		public virtual string MatchReason
		{
			get;
			set;
		}
		#endregion
		#region LastAutoMatchDate
		public abstract class lastAutoMatchDate : BqlDateTime.Field<lastAutoMatchDate> { }

		[PXDBDate(PreserveTime = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Last Auto-Match Date")]
		public virtual DateTime? LastAutoMatchDate
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXNote]
        public virtual Guid? NoteID
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
        public virtual DateTime? LastModifiedDateTime
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
        #endregion

        #region ICADocSource Members

        public int? BAccountID
        {
            get
            {
                return this.PayeeBAccountID;
            }

            set
            {
                this.PayeeBAccountID = value;
            }
        }

        public int? LocationID
        {
            get
            {
                return this.PayeeLocationID;
            }

            set
            {
                this.PayeeLocationID = value;
            }
        }

        #region ClearDate
        public abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared> { }
        [PXBool]
        public bool? Cleared
        {
            get
            {
                return true;
            }
        }
        #endregion
        #region ClearDate
        public abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate> { }
        [PXDate]
        public virtual DateTime? ClearDate
        {
            get
            {
                return TranDate;
            }
        }
        #endregion

        public int? CARefTranAccountID
        {
            get
            {
                return null;
            }
        }

        public long? CARefTranID
        {
            get
            {
                return null;
            }
        }

        public int? CARefSplitLineNbr
        {
            get
            {
                return null;
            }
        }

        public decimal? CuryOrigDocAmt
        {
            [PXDependsOnFields(typeof(curyTranAmt))]
            get
            {
                return this.CuryTranAmt.HasValue ? (this.CuryTranAmt.Value != decimal.Zero ? this.CuryTranAmt * Math.Sign(this.CuryTranAmt.Value) : decimal.Zero) : null; //Document sign is inverted compared to the CATran's
            }
        }

        long? ICADocSource.CuryInfoID
        {
            get
            {
                return null;
            }
        }

		string ICADocSource.FinPeriodID
		{
			get
			{
				return MatchingFinPeriodID;
			}
		}

		string ICADocSource.InvoiceNbr
        {
            get
            {
                return InvoiceInfo;
            }
        }

        string ICADocSource.TranDesc
        {
            get
            {
                return UserDesc;
            }
        }
        #endregion

		public virtual string GetFriendlyKeyImage(PXCache cache)
		{
			return String.Format("{0}: {1}, {2}: {3}",
				PXUIFieldAttribute.GetDisplayName<headerRefNbr>(cache), HeaderRefNbr,
				PXUIFieldAttribute.GetDisplayName<tranID>(cache), TranID);
		}
    }

    public class CABankTranType
	{
		public class ListAttribute : PXStringListAttribute
		{
		    public ListAttribute()
		        : base(
					new string[] { Statement, PaymentImport },
					new string[] { Messages.Statement, Messages.PaymentImport })
			{ }
		}

		public const string Statement = "S";
        public const string PaymentImport = "I";
		
		public class statement : PX.Data.BQL.BqlString.Constant<statement>
		{
			public statement() : base(Statement) { }
		}

		public class paymentImport : PX.Data.BQL.BqlString.Constant<paymentImport>
		{
			public paymentImport() : base(PaymentImport) { }
		}
	}

	public class CABankTranStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
			new string[] { Matched, InvoiceMatched, ExpenseReceiptMatched, Created, Hidden },
			new string[] { Messages.Matched, Messages.InvoiceMatched, Messages.MatchedToEReceipt, Messages.Created, Messages.Hidden})
			{ }
		}

		public class ImagesListAttribute : PXImagesListAttribute
		{
		    private static string getCustomSprite(string sprite)
		    {
		        return $"{Sprite.AliasMain}@{sprite}";
		    }

            public ImagesListAttribute() : base(new string[] { Matched, InvoiceMatched, Created, Hidden },
		        new string[] { Messages.Matched, Messages.InvoiceMatched, Messages.Created, Messages.Hidden },
				new string[] { getCustomSprite(Sprite.Main.Link), getCustomSprite(Sprite.Main.LinkWB), getCustomSprite(Sprite.Main.RecordAdd), getCustomSprite(Sprite.Main.Preview) })
			{ }
        }

		public const string Matched = "M";
		public const string InvoiceMatched = "I";
		public const string Created = "C";
		public const string Hidden = "H";
	    public const string ExpenseReceiptMatched = "R";

        public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Matched) { }
		}

		public class balanced : PX.Data.BQL.BqlString.Constant<balanced>
		{
			public balanced() : base(InvoiceMatched) { }
		}

		public class unposted : PX.Data.BQL.BqlString.Constant<unposted>
		{
			public unposted() : base(Created) { }
		}

		public class posted : PX.Data.BQL.BqlString.Constant<posted>
		{
			public posted() : base(Hidden) { }
		}
	}
}
