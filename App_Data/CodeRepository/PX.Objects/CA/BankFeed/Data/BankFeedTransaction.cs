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
using System;

namespace PX.Objects.CA
{
	[PXCacheName("BankFeedTransaction")]
	public class BankFeedTransaction : IBqlTable
	{
		#region TransactionID
		public abstract class transactionID : PX.Data.BQL.BqlString.Field<transactionID> { }
		[PXString(IsKey = true)]
		[PXUIField(DisplayName = "Transaction ID", Visible = false)]
		public string TransactionID { get; set; }
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		[PXString]
		[PXUIField(DisplayName = "Type", Visible = false)]
		public string Type { get; set; }
		#endregion

		#region AccountOwner
		public abstract class accountOwner : PX.Data.BQL.BqlString.Field<accountOwner> { }
		[PXString]
		[PXUIField(DisplayName = "Account Owner", Visible = false)]
		public string AccountOwner { get; set; }
		#endregion

		#region PendingTransactionID
		public abstract class pendingTransactionID : PX.Data.BQL.BqlString.Field<pendingTransactionID> { }
		[PXString]
		[PXUIField(DisplayName = "Pending Transaction ID", Visible = false)]
		public string PendingTransactionID { get; set; }
		#endregion

		#region Pending
		public abstract class pending : PX.Data.BQL.BqlBool.Field<pending> { }
		[PXBool]
		[PXUIField(DisplayName = "Pending", Visible = false)]
		public bool? Pending { get; set; }
		#endregion

		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		[PXDate]
		[PXUIField(DisplayName = "Date", Visible = false)]
		public DateTime? Date { get; set; }
		#endregion

		#region IsoCurrencyCode
		public abstract class isoCurrencyCode : PX.Data.BQL.BqlString.Field<isoCurrencyCode> { }
		[PXString]
		[PXUIField(DisplayName = "Currency Code", Visible = false)]
		public string IsoCurrencyCode { get; set; }
		#endregion

		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		[PXDecimal]
		[PXUIField(DisplayName = "Amount", Visible = false)]
		public decimal? Amount { get; set; }
		#endregion

		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlString.Field<accountID> { }
		[PXString]
		[PXUIField(DisplayName = "Account ID", Visible = false)]
		public string AccountID { get; set; }
		#endregion

		#region Name
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
		[PXString]
		[PXUIField(DisplayName = "Name", Visible = false)]
		public string Name { get; set; }
		#endregion

		#region Category
		public abstract class category : PX.Data.BQL.BqlString.Field<category> { }
		[PXString]
		[PXUIField(DisplayName = "Category", Visible = false)]
		public string Category { get; set; }
		#endregion

		#region CheckNumber
		public abstract class checkNumber : PX.Data.BQL.BqlString.Field<checkNumber> { }
		[PXString]
		[PXUIField(DisplayName = "Check Number", Visible = false)]
		public string CheckNumber { get; set; }
		#endregion

		#region Memo
		public abstract class memo : PX.Data.BQL.BqlString.Field<memo> { }
		[PXString]
		[PXUIField(DisplayName = "Memo", Visible = false)]
		public string Memo { get; set; }
		#endregion

		#region CreatedAt
		public abstract class createdAt : PX.Data.BQL.BqlDateTime.Field<createdAt> { }
		/// <summary>
		/// Contains data from service's field created_at
		/// </summary>
		[PXDateAndTime]
		[PXUIField(DisplayName = "Created At", Visible = false)]
		public DateTime? CreatedAt { get; set; }

		#endregion

		#region PostedAt
		public abstract class postedAt : PX.Data.BQL.BqlDateTime.Field<postedAt> { }
		/// <summary>
		/// Contains data from service's field posted_at
		/// </summary>
		[PXDateAndTime]
		[PXUIField(DisplayName = "Posted At", Visible = false)]
		public DateTime? PostedAt { get; set; }
		#endregion

		#region TransactedAt
		public abstract class transactedAt : PX.Data.BQL.BqlDateTime.Field<transactedAt> { }
		/// <summary>
		/// Contains data from service's field transacted_at
		/// </summary>
		[PXDateAndTime]
		[PXUIField(DisplayName = "Transacted At", Visible = false)]
		public DateTime? TransactedAt { get; set; }
		#endregion

		#region UpdatedAt
		public abstract class updatedAt : PX.Data.BQL.BqlDateTime.Field<updatedAt> { }
		/// <summary>
		/// Contains data from service's field updated_at
		/// </summary>
		[PXDateAndTime]
		[PXUIField(DisplayName = "Updated At", Visible = false)]
		public DateTime? UpdatedAt { get; set; }
		#endregion

		#region AccountStringId
		public abstract class accountStringId : PX.Data.BQL.BqlString.Field<accountStringId> { }
		/// <summary>
		/// Contains data from service's field account_id
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Account String ID", Visible = false)]
		public string AccountStringId { get; set; }
		#endregion

		#region CategoryGuid
		public abstract class categoryGuid : PX.Data.BQL.BqlString.Field<categoryGuid> { }
		/// <summary>
		/// Contains data from service's field category_guid
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Category GUID", Visible = false)]
		public string CategoryGuid { get; set; }
		#endregion

		#region ExtendedTransactionType
		public abstract class extendedTransactionType : PX.Data.BQL.BqlString.Field<extendedTransactionType> { }
		/// <summary>
		/// Contains data from service's field extended_transaction_type
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Extended Transaction Type", Visible = false)]
		public string ExtendedTransactionType { get; set; }
		#endregion

		#region Id
		public abstract class id : PX.Data.BQL.BqlString.Field<id> { }
		/// <summary>
		/// Contains data from service's field id
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "ID", Visible = false)]
		public string Id { get; set; }
		#endregion

		#region IsBillPay
		public abstract class isBillPay : PX.Data.BQL.BqlBool.Field<isBillPay> { }
		/// <summary>
		/// Contains data from service's field is_bill_pay
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Is Bill Pay", Visible = false)]
		public bool? IsBillPay { get; set; }
		#endregion

		#region IsDirectDeposit
		public abstract class isDirectDeposit : PX.Data.BQL.BqlBool.Field<isDirectDeposit> { }
		/// <summary>
		/// Contains data from service's field is_direct_deposit
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Is Direct Deposit", Visible = false)]
		public bool? IsDirectDeposit { get; set; }

		#endregion

		#region IsExpense
		public abstract class isExpense : PX.Data.BQL.BqlBool.Field<isExpense> { }
		/// <summary>
		/// Contains data from service's field is_expense
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Is Expense", Visible = false)]
		public bool? IsExpense { get; set; }
		#endregion

		#region IsFee
		public abstract class isFee : PX.Data.BQL.BqlBool.Field<isFee> { }
		/// <summary>
		/// Contains data from service's field is_fee
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Is Fee", Visible = false)]
		public bool? IsFee { get; set; }

		#endregion

		#region IsIncome
		public abstract class isIncome : PX.Data.BQL.BqlBool.Field<isIncome> { }
		/// <summary>
		/// Contains data from service's field is_income
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Is Income", Visible = false)]
		public bool? IsIncome { get; set; }
		#endregion

		#region IsInternational
		public abstract class isInternational : PX.Data.BQL.BqlBool.Field<isInternational> { }
		/// <summary>
		/// Contains data from service's field is_international
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Is International", Visible = false)]
		public bool? IsInternational { get; set; }
		#endregion

		#region IsOverdraftFee
		public abstract class isOverdraftFee : PX.Data.BQL.BqlBool.Field<isOverdraftFee> { }
		/// <summary>
		/// Contains data from service's field is_overdraft_fee
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Is Overdraft Fee", Visible = false)]
		public bool? IsOverdraftFee { get; set; }
		#endregion

		#region IsPayrollAdvance
		public abstract class isPayrollAdvance : PX.Data.BQL.BqlBool.Field<isPayrollAdvance> { }
		/// <summary>
		/// Contains data from service's field is_payroll_advance
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Is Payroll Advance", Visible = false)]
		public bool? IsPayrollAdvance { get; set; }
		#endregion

		#region IsRecurring
		public abstract class isRecurring : PX.Data.BQL.BqlBool.Field<isRecurring> { }
		/// <summary>
		/// Contains data from service's field is_recurring
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Is Recurring", Visible = false)]
		public bool? IsRecurring { get; set; }
		#endregion

		#region IsSubscription
		public abstract class isSubscription : PX.Data.BQL.BqlBool.Field<isSubscription> { }
		/// <summary>
		/// Contains data from service's field is_subcription
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Is Subscription", Visible = false)]
		public bool? IsSubscription { get; set; }
		#endregion

		#region Latitude
		public abstract class latitude : PX.Data.BQL.BqlString.Field<latitude> { }
		/// <summary>
		/// Contains data from service's field latitude
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Latitude", Visible = false)]
		public string Latitude { get; set; }
		#endregion

		#region LocalizedDescription
		public abstract class localizedDescription : PX.Data.BQL.BqlString.Field<localizedDescription> { }
		/// <summary>
		/// Contains data from service's field localized_description
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Localized Description", Visible = false)]
		public string LocalizedDescription { get; set; }
		#endregion

		#region LocalizedMemo
		public abstract class localizedMemo : PX.Data.BQL.BqlString.Field<localizedMemo> { }
		/// <summary>
		/// Contains data from service's field localized_memo
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Localized Memo", Visible = false)]
		public string LocalizedMemo { get; set; }
		#endregion

		#region Longitude
		public abstract class longitude : PX.Data.BQL.BqlString.Field<longitude> { }
		/// <summary>
		/// Contains data from service's field longitude
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Longitude", Visible = false)]
		public string Longitude { get; set; }
		#endregion

		#region MemberIsManagedByUser
		public abstract class memberIsManagedByUser : PX.Data.BQL.BqlBool.Field<memberIsManagedByUser> { }
		/// <summary>
		/// Contains data from service's field member_is_managed_by_user
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Member Is Managed By User", Visible = false)]
		public bool? MemberIsManagedByUser { get; set; }
		#endregion

		#region MerchantCategoryCode
		public abstract class merchantCategoryCode : PX.Data.BQL.BqlString.Field<merchantCategoryCode> { }
		/// <summary>
		/// Contains data from service's field merchant_category_code
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Merchant Category Code", Visible = false)]
		public string MerchantCategoryCode { get; set; }
		#endregion

		#region MerchantGuid
		public abstract class merchantGuid : PX.Data.BQL.BqlString.Field<merchantGuid> { }
		/// <summary>
		/// Contains data from service's field merchant_guid
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Merchant GUID", Visible = false)]
		public string MerchantGuid { get; set; }
		#endregion

		#region MerchantLocationGuid
		public abstract class merchantLocationGuid : PX.Data.BQL.BqlString.Field<merchantLocationGuid> { }
		/// <summary>
		/// Contains data from service's field merchant_location_guid
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Merchant Location GUID", Visible = false)]
		public string MerchantLocationGuid { get; set; }
		#endregion

		#region Metadata
		public abstract class metadata : PX.Data.BQL.BqlString.Field<metadata> { }
		/// <summary>
		/// Contains data from service's field metadata
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Metadata", Visible = false)]
		public string Metadata { get; set; }
		#endregion

		#region OriginalDescription
		public abstract class originalDescription : PX.Data.BQL.BqlString.Field<originalDescription> { }
		/// <summary>
		/// Contains data from service's field original_description
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Original Description", Visible = false)]
		public string OriginalDescription { get; set; }
		#endregion

		#region UserId
		public abstract class userId : PX.Data.BQL.BqlString.Field<userId> { }
		/// <summary>
		/// Contains data from service's field user_id
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "User ID", Visible = false)]
		public string UserId { get; set; }
		#endregion

		#region AuthorizedDate
		public abstract class authorizedDate : PX.Data.BQL.BqlDateTime.Field<authorizedDate> { }
		/// <summary>
		/// Contains data from service's field authorized_date
		/// </summary>
		[PXDate]
		[PXUIField(DisplayName = "Authorized Date", Visible = false)]
		public DateTime? AuthorizedDate { get; set; }
		#endregion

		#region AuthorizedDatetime
		public abstract class authorizedDatetime : PX.Data.BQL.BqlDateTime.Field<authorizedDatetime> { }
		/// <summary>
		/// Contains data from service's field authorized_datetime
		/// </summary>
		[PXDate]
		[PXUIField(DisplayName = "Authorized Datetime", Visible = false)]
		public DateTime? AuthorizedDatetime { get; set; }

		#endregion

		#region DatetimeValue
		public abstract class datetimeValue : PX.Data.BQL.BqlDateTime.Field<datetimeValue> { }
		/// <summary>
		/// Contains data from service's field datetime
		/// </summary>
		[PXDateAndTime]
		[PXUIField(DisplayName = "Datetime Value", Visible = false)]
		public DateTime? DatetimeValue { get; set; }
		#endregion

		#region Address
		public abstract class address : PX.Data.BQL.BqlString.Field<address> { }
		/// <summary>
		/// Contains data from service's field address
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Address", Visible = false)]
		public string Address { get; set; }
		#endregion

		#region City
		public abstract class city : PX.Data.BQL.BqlString.Field<city> { }
		/// <summary>
		/// Contains data from service's field city
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "City", Visible = false)]
		public string City { get; set; }
		#endregion

		#region Country
		public abstract class country : PX.Data.BQL.BqlString.Field<country> { }
		/// <summary>
		/// Contains data from service's field country
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Country", Visible = false)]
		public string Country { get; set; }
		#endregion

		#region PostalCode
		public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }
		/// <summary>
		/// Contains data from service's field postal_code
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Postal Code", Visible = false)]
		public string PostalCode { get; set; }
		#endregion

		#region Region
		public abstract class region : PX.Data.BQL.BqlString.Field<region> { }
		/// <summary>
		/// Contains data from service's field region
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Region", Visible = false)]
		public string Region { get; set; }
		#endregion

		#region StoreNumber
		public abstract class storeNumber : PX.Data.BQL.BqlString.Field<storeNumber> { }
		/// <summary>
		/// Contains data from service's field store_number
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Store Number", Visible = false)]
		public string StoreNumber { get; set; }
		#endregion

		#region MerchantName
		public abstract class merchantName : PX.Data.BQL.BqlString.Field<merchantName> { }
		/// <summary>
		/// Contains data from service's field merchant_name
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Merchant Name", Visible = false)]
		public string MerchantName { get; set; }
		#endregion

		#region PaymentChannel
		public abstract class paymentChannel : PX.Data.BQL.BqlString.Field<paymentChannel> { }
		/// <summary>
		/// Contains data from service's field payment_channel
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Payment Channel", Visible = false)]
		public string PaymentChannel { get; set; }
		#endregion

		#region ByOrderOf
		public abstract class byOrderOf : PX.Data.BQL.BqlString.Field<byOrderOf> { }
		/// <summary>
		/// Contains data from service's field by_order_of
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "By Order Of", Visible = false)]
		public string ByOrderOf { get; set; }
		#endregion

		#region Payee
		public abstract class payee : PX.Data.BQL.BqlString.Field<payee> { }
		/// <summary>
		/// Contains data from service's field payee
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Payee", Visible = false)]
		public string Payee { get; set; }
		#endregion

		#region Payer
		public abstract class payer : PX.Data.BQL.BqlString.Field<payer> { }
		/// <summary>
		/// Contains data from service's field payer
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Payer", Visible = false)]
		public string Payer { get; set; }
		#endregion

		#region PaymentMethod
		public abstract class paymentMethod : PX.Data.BQL.BqlString.Field<paymentMethod> { }
		/// <summary>
		/// Contains data from service's field payment_method
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Payment Method", Visible = false)]
		public string PaymentMethod { get; set; }
		#endregion

		#region PaymentProcessor
		public abstract class paymentProcessor : PX.Data.BQL.BqlString.Field<paymentProcessor> { }
		/// <summary>
		/// Contains data from service's field payment_processor
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Payment Processor", Visible = false)]
		public string PaymentProcessor { get; set; }
		#endregion

		#region PpdId
		public abstract class ppdId : PX.Data.BQL.BqlString.Field<ppdId> { }
		/// <summary>
		/// Contains data from service's field ppd_id
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "PPD ID", Visible = false)]
		public string PpdId { get; set; }
		#endregion

		#region Reason
		public abstract class reason : PX.Data.BQL.BqlString.Field<reason> { }
		/// <summary>
		/// Contains data from service's field reason
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Reason", Visible = false)]
		public string Reason { get; set; }
		#endregion

		#region ReferenceNumber
		public abstract class referenceNumber : PX.Data.BQL.BqlString.Field<referenceNumber> { }
		/// <summary>
		/// Contains data from service's field reference_number
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Reference Number", Visible = false)]
		public string ReferenceNumber { get; set; }
		#endregion

		#region PersonalFinanceCategory
		public abstract class personalFinanceCategory : PX.Data.BQL.BqlString.Field<personalFinanceCategory> { }
		/// <summary>
		/// Contains data from service's field personal_finance_category
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Personal Finance Category", Visible = false)]
		public string PersonalFinanceCategory { get; set; }
		#endregion

		#region TransactionCode
		public abstract class transactionCode : PX.Data.BQL.BqlString.Field<transactionCode> { }
		/// <summary>
		/// Contains data from service's field transaction_code
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Transaction Code", Visible = false)]
		public string TransactionCode { get; set; }
		#endregion

		#region UnofficialCurrencyCode
		public abstract class unofficialCurrencyCode : PX.Data.BQL.BqlString.Field<unofficialCurrencyCode> { }
		/// <summary>
		/// Contains data from service's field unofficial_currency_code
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Unofficial Currency Code", Visible = false)]
		public string UnofficialCurrencyCode { get; set; }
		#endregion

		#region PartnerAccountID
		public abstract class partnerAccountID : PX.Data.BQL.BqlString.Field<partnerAccountID> { }
		[PXString]
		[PXUIField(DisplayName = "Partner Account ID", Visible = false)]
		public string PartnerAccountId { get; set; }
		#endregion
	}
}
