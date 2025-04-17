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

using PX.Commerce.Core;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CA;
using PX.Objects.CM;
using System;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// Represents a mapping between a payment method in an external store and a payment method in ERP.
	/// </summary>
	[Serializable]
	[PXCacheName("BCPaymentMethods")]
	public class BCPaymentMethods : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<BCPaymentMethods>.By<BCPaymentMethods.paymentMappingID>
		{
			public static BCPaymentMethods Find(PXGraph graph, int? paymentMappingID) => FindBy(graph, paymentMappingID);
		}
		public static class FK
		{
			public class Binding : BCBinding.BindingIndex.ForeignKeyOf<BCPaymentMethods>.By<bindingID> { }
			public class CashAcc : CashAccount.PK.ForeignKeyOf<BCPaymentMethods>.By<cashAccountID> { }
		}
		#endregion

		#region BindingID
		/// <summary>
		/// Represents a store to which the entity belongs.
		/// The property is a key field.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Store")]
		[PXSelector(typeof(BCBinding.bindingID),
					typeof(BCBinding.bindingName),
					SubstituteKey = typeof(BCBinding.bindingName))]
		[PXParent(typeof(Select<BCBinding,
			Where<BCBinding.bindingID, Equal<Current<BCPaymentMethods.bindingID>>>>))]
		[PXDBDefault(typeof(BCBinding.bindingID),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? BindingID { get; set; }
		/// <inheritdoc cref="BindingID"/>
		public abstract class bindingID : PX.Data.BQL.BqlInt.Field<bindingID> { }
		#endregion

		#region PaymentMappingID
		/// <summary>
		/// The identity of this record.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
		public int? PaymentMappingID { get; set; }
		/// <inheritdoc cref="PaymentMappingID"/>
		public abstract class paymentMappingID : PX.Data.BQL.BqlInt.Field<paymentMappingID> { }
		#endregion

		#region StorePaymentMethod
		/// <summary>
		/// The payment method from the external store that is being mapped.
		/// </summary>
		[PXDBString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Store Payment Method")]
		[PXDefault]
		[BCCapitalLettersAttribute]
		public virtual string StorePaymentMethod { get; set; }
		/// <inheritdoc cref="StorePaymentMethod"/>
		public abstract class storePaymentMethod : PX.Data.BQL.BqlString.Field<storePaymentMethod> { }
		#endregion

		#region StoreCurrency
		/// <summary>
		/// The currency of the payment method from the external store that is being mapped.
		/// </summary>
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Store Currency")]
		[PXSelector(typeof(Currency.curyID))]
		[PXDefault]
		public virtual string StoreCurrency { get; set; }
		/// <inheritdoc cref="StoreCurrency"/>
		public abstract class storeCurrency : PX.Data.BQL.BqlString.Field<storeCurrency> { }
		#endregion

		#region StoreOrderPaymentMethod
		/// <summary>
		/// The order payment method from the external store that is being mapped.
		/// </summary>
		[PXDBString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Store Order Payment Method", Visible = false)]
		[BCCapitalLettersAttribute]
		public virtual string StoreOrderPaymentMethod { get; set; }
		/// <inheritdoc cref="StoreOrderPaymentMethod"/>
		public abstract class storeOrderPaymentMethod : PX.Data.BQL.BqlString.Field<storeOrderPaymentMethod> { }
		#endregion

		#region PaymentMethodID
		/// <summary>
		/// The ID of the payment method in ERP that is being mapped.
		/// </summary>
		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "ERP Payment Method")]
		[PXSelector(typeof(Search4<PaymentMethod.paymentMethodID,
			Where<PaymentMethod.isActive, Equal<True>,
				And<PaymentMethod.useForAR, Equal<True>>>,
			Aggregate< GroupBy<PaymentMethod.paymentMethodID, GroupBy<PaymentMethod.useForAR, GroupBy<PaymentMethod.useForAP>>>>>),
			DescriptionField = typeof(PaymentMethod.descr))]
		public virtual string PaymentMethodID { get; set; }
		/// <inheritdoc cref="PaymentMethodID"/>
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		#endregion

		#region CashAcccount
		/// <summary>
		/// The Cash Account to use when mapping payments with this mapping.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Cash Account")]
		[PXSelector(typeof(Search2<CashAccount.cashAccountID,
						InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
							And<PaymentMethodAccount.paymentMethodID, Equal<Current<BCPaymentMethods.paymentMethodID>>,
							And<PaymentMethodAccount.useForAR, Equal<True>>>>>,
						Where<CashAccount.active, Equal<True>,
							And<CashAccount.curyID, Equal<Current<BCPaymentMethods.storeCurrency>>,
							And<Where<CashAccount.branchID, Equal<Current<BCBinding.branchID>>,
								Or<CashAccount.restrictVisibilityWithBranch, Equal<False>>>>>>>),
				 DescriptionField = typeof(CashAccount.descr),
					SubstituteKey = typeof(CashAccount.cashAccountCD)
			)]
		public virtual int? CashAccountID { get; set; }
		/// <inheritdoc cref="CashAccountID"/>
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		#endregion

		#region ProcessingCenterID
		/// <summary>
		/// The ID of the processing center to use when mapping payments with this mapping
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Proc. Center ID")]
		[PXSelector(typeof(Search2<CCProcessingCenter.processingCenterID,
			InnerJoin<CCProcessingCenterPmntMethod,
				On<CCProcessingCenterPmntMethod.processingCenterID, Equal<CCProcessingCenter.processingCenterID>>>,
			Where<CCProcessingCenter.isActive, Equal<True>,
				And<CCProcessingCenterPmntMethod.isActive, Equal<True>, And<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<BCPaymentMethods.paymentMethodID>>,
				And<CCProcessingCenter.cashAccountID, Equal<Current<BCPaymentMethods.cashAccountID>>>>>>>))]
		public virtual string ProcessingCenterID { get; set; }
		/// <inheritdoc cref="ProcessingCenterID"/>
		public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }
		#endregion

		#region Release Payments and Refunds
		/// <summary>
		/// Indicates whether payments should be automatically released when importing payments that use this mapping.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Release Payments and Refunds")]
		[PXDefault(false)]
		public virtual bool? ReleasePayments { get; set; }
		/// <inheritdoc cref="ReleasePayments"/>
		public abstract class releasePayments : PX.Data.BQL.BqlBool.Field<releasePayments> { }
		#endregion

		#region Active
		/// <summary>
		/// Indicates whether this mapping is currently active.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(false)]
		public virtual bool? Active { get; set; }
		/// <inheritdoc cref="Active"/>
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		#endregion

		#region ProcessRefunds
		/// <summary>
		/// Indicates whether payments of this type should allow refunds to be processed.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Process Refunds")]
		[PXDefault(false)]
		public virtual bool? ProcessRefunds { get; set; }
		/// <inheritdoc cref="ProcessRefunds"/>
		public abstract class processRefunds : PX.Data.BQL.BqlBool.Field<processRefunds> { }
		#endregion

		#region CreatePaymentFromOrder
		/// <summary>
		/// Indicates whether payments should be automatically created for orders with this payment method.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Create Payment from Order", Visible = false)]
		[PXDefault(false)]
		public virtual bool? CreatePaymentFromOrder { get; set; }
		/// <inheritdoc cref="CreatePaymentFromOrder"/>
		public abstract class createPaymentFromOrder : PX.Data.BQL.BqlBool.Field<createPaymentFromOrder> { }
		#endregion
	}

	/// <summary>
	/// Represents a payment method from BigCommerce.
	/// </summary>
	[Serializable]
	[PXHidden]
	public class BCBigCommercePayment : IBqlTable
	{
		#region Name
		/// <summary>
		/// The name of the payment method.
		/// </summary>
		[PXDBString(IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Name")]
		public virtual string Name { get; set; }
		/// <inheritdoc cref="Name"/>
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
		#endregion

		#region Currency
		/// <summary>
		/// The currency of the payment method
		/// </summary>
		[PXDBString(IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Currency")]
		public virtual string Currency { get; set; }
		/// <inheritdoc cref="Currency"/>
		public abstract class currency : PX.Data.BQL.BqlString.Field<currency> { }
		#endregion

		#region Create Payment from Order
		/// <summary>
		/// Indicates whether payments should be automatically created for orders with this payment method.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Create Payment from Order")]
		[PXDefault(false)]
		public virtual bool? CreatePaymentfromOrder { get; set; }
		/// <inheritdoc cref="CreatePaymentfromOrder"/>
		public abstract class createPaymentfromOrder : PX.Data.BQL.BqlBool.Field<createPaymentfromOrder> { }
		#endregion
	}
}
