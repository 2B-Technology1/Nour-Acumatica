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
using PX.Objects.Common;

namespace PX.Objects.CA
{
	/// <summary>
	/// Cash account-specific values for the <see cref="PaymentMethodDetail">payment method settings</see> related to cash accounts.
	/// The records of this type are edited on the Remittance Settings tab of the Cash Accounts (CA20200) form.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.CashAccountPaymentMethodDetail)]
	public partial class CashAccountPaymentMethodDetail : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CashAccountPaymentMethodDetail>.By<cashAccountID, paymentMethodID, detailID>
		{
			public static CashAccountPaymentMethodDetail Find(PXGraph graph, int? cashAccountID, string paymentMethodID, string detailID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, cashAccountID, paymentMethodID, detailID, options);
		}

		public static class FK
		{
			public class CashAccount : CA.CashAccount.PK.ForeignKeyOf<CashAccountPaymentMethodDetail>.By<cashAccountID> { }
			public class PaymentMethod : CA.PaymentMethod.PK.ForeignKeyOf<CashAccountPaymentMethodDetail>.By<paymentMethodID> { }
			public class PaymentMethodDetail : CA.PaymentMethodDetail.PK.ForeignKeyOf<CashAccountPaymentMethodDetail>.By<paymentMethodID, detailID> { }
			public class PaymentMethodForCashAccount : CA.PaymentMethodAccount.PK.ForeignKeyOf<CashAccountPaymentMethodDetail>.By<paymentMethodID, cashAccountID> { }
		}

		#endregion

		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(CashAccount.cashAccountID))]
		[PXUIField(DisplayName = "Cash Account", Visible = false, Enabled = false)]
		[PXParent(typeof(Select<PaymentMethodAccount,
			Where<PaymentMethodAccount.cashAccountID, Equal<Current<CashAccountPaymentMethodDetail.cashAccountID>>,
			And<PaymentMethodAccount.paymentMethodID, Equal<Current<CashAccountPaymentMethodDetail.paymentMethodID>>,
			And<PaymentMethodAccount.useForAP, Equal<True>>>>>))]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region AccountID
		[Obsolete(InternalMessages.FieldIsObsoleteAndWillBeRemoved2023R2)]
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

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
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Payment Method", Visible = false)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID>))]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region DetailID
		public abstract class detailID : PX.Data.BQL.BqlString.Field<detailID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "ID", Visible = false, Enabled = false)]
		[PXSelector(typeof(Search<PaymentMethodDetail.detailID, Where<PaymentMethodDetail.paymentMethodID,
					Equal<Current<CashAccountPaymentMethodDetail.paymentMethodID>>,
						And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
						Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>))]
		public virtual string DetailID
		{
			get;
			set;
		}
		#endregion
		#region DetailValue
		public abstract class detailValue : PX.Data.BQL.BqlString.Field<detailValue> { }

		[PXDBStringWithMask(255, typeof(Search<PaymentMethodDetail.entryMask, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<CashAccountPaymentMethodDetail.paymentMethodID>>,
									   And<PaymentMethodDetail.detailID, Equal<Current<CashAccountPaymentMethodDetail.detailID>>,
									   And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
										   Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>), IsUnicode = true)]
		[PXUIField(DisplayName = "Value")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[DynamicValueValidation(typeof(Search<PaymentMethodDetail.validRegexp, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<CashAccountPaymentMethodDetail.paymentMethodID>>,
										And<PaymentMethodDetail.detailID, Equal<Current<CashAccountPaymentMethodDetail.detailID>>,
										And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
											Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>))]
		public virtual string DetailValue
		{
			get;
			set;
		}
		#endregion
	}
}
