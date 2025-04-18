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
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.Common.Attributes;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO
{
	[PXCacheName(Messages.SOQuickPayment)]
	[PXVirtual]
	public class SOQuickPayment : IBqlTable
	{
		#region IsRefund
		public abstract class isRefund : Data.BQL.BqlBool.Field<isRefund> { }

		[PXBool]
		[PXDefault(typeof(Where<Current<SOOrderType.canHaveRefunds>, Equal<True>,
			And<Where<Current<SOOrder.behavior>, NotEqual<SOBehavior.mO>, Or<Current<SOOrder.curyOrderTotal>, Less<decimal0>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? IsRefund
		{
			get;
			set;
		}
		#endregion

		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
								Where<PaymentMethod.isActive, Equal<boolTrue>,
								And<PaymentMethod.useForAR, Equal<boolTrue>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXUIFieldAttribute(DisplayName = "Payment Method", Required = true)]
		public virtual String PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region PaymentMethodProcCenterID
		public abstract class paymentMethodProcCenterID : PX.Data.BQL.BqlString.Field<paymentMethodProcCenterID> { }

		[PXString(10, IsUnicode = true)]
		[PXDefault(typeof(Search2<CCProcessingCenterPmntMethod.processingCenterID,
				InnerJoin<CCProcessingCenter, On<CCProcessingCenter.processingCenterID, Equal<CCProcessingCenterPmntMethod.processingCenterID>>>,
				Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<paymentMethodID>>,
					And<CCProcessingCenterPmntMethod.isActive, Equal<True>,
					And<CCProcessingCenterPmntMethod.isDefault, Equal<True>,
					And<CCProcessingCenter.isActive, Equal<True>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CCProcessingCenter.processingCenterID>), ValidateValue = false)]
		public virtual string PaymentMethodProcCenterID
		{
			get;
			set;
		}
		#endregion

		#region AllowUnlinkedRefund
		public abstract class allowUnlinkedRefund : Data.BQL.BqlBool.Field<allowUnlinkedRefund> { }

		[PXBool]
		[PXFormula(typeof(IsNull<Selector<paymentMethodProcCenterID, CCProcessingCenter.allowUnlinkedRefund>, True>))]
		public virtual bool? AllowUnlinkedRefund
		{
			get;
			set;
		}
		#endregion
		#region RefTranExtNbr
		public abstract class refTranExtNbr : Data.BQL.BqlString.Field<refTranExtNbr> { }
		[PXString(50, IsUnicode = true)]
		[PXSelector(typeof(Search2<ExternalTransaction.tranNumber,
			InnerJoin<ARPayment, On<ExternalTransaction.docType, Equal<ARPayment.docType>, And<ExternalTransaction.refNbr, Equal<ARPayment.refNbr>>>>,
			Where<ExternalTransaction.procStatus, Equal<ExtTransactionProcStatusCode.captureSuccess>,
				And<ARPayment.customerID, Equal<Current2<SOOrder.customerID>>,
				And<ARPayment.paymentMethodID, Equal<Current2<paymentMethodID>>>>>,
			OrderBy<Desc<ExternalTransaction.tranNumber>>>),
			typeof(ExternalTransaction.refNbr),
			typeof(ARPayment.docDate),
			typeof(ExternalTransaction.amount),
			typeof(ExternalTransaction.tranNumber))]
		[PXUIField(DisplayName = "Orig. Transaction", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string RefTranExtNbr
		{
			get;
			set;
		}
		#endregion

		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }

		[PXInt()]
		[PXUIField(DisplayName = "Card/Account No")]
		[PXDefault(typeof(Coalesce<
						Search2<Customer.defPMInstanceID, InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
								And<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>>,
								Where<Customer.bAccountID, Equal<Current<SOOrder.customerID>>,
								  And<CustomerPaymentMethod.isActive, Equal<True>,
								  And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<SOQuickPayment.paymentMethodID>>>>>>,
						Search<CustomerPaymentMethod.pMInstanceID,
								Where<CustomerPaymentMethod.bAccountID, Equal<Current<SOOrder.customerID>>,
									And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<SOQuickPayment.paymentMethodID>>,
									And<CustomerPaymentMethod.isActive, Equal<True>>>>, OrderBy<Desc<CustomerPaymentMethod.expirationDate, Desc<CustomerPaymentMethod.pMInstanceID>>>>>)
						, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID, Where<CustomerPaymentMethod.bAccountID, Equal<Current2<SOOrder.customerID>>,
			And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<paymentMethodID>>,
			And<Where<CustomerPaymentMethod.isActive, Equal<boolTrue>, Or<CustomerPaymentMethod.pMInstanceID,
					Equal<Current<pMInstanceID>>>>>>>>), DescriptionField = typeof(CustomerPaymentMethod.descr))]
		[DeprecatedProcessing]
		[DisabledProcCenter]
		public virtual Int32? PMInstanceID
		{
			get;
			set;
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		[PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.cashAccountID,
									InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CustomerPaymentMethod.cashAccountID>,
										And<PaymentMethodAccount.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
										And<PaymentMethodAccount.useForAR, Equal<True>>>>>,
									Where<CustomerPaymentMethod.bAccountID, Equal<Current<SOOrder.customerID>>,
										And<CustomerPaymentMethod.pMInstanceID, Equal<Current2<pMInstanceID>>>>>,
								Search2<CashAccount.cashAccountID,
								InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
									And<PaymentMethodAccount.useForAR, Equal<True>,
									And<PaymentMethodAccount.aRIsDefault, Equal<True>,
									And<PaymentMethodAccount.paymentMethodID, Equal<Current2<paymentMethodID>>>>>>>,
									Where<CashAccount.branchID, Equal<Current<SOOrder.branchID>>,
										And<Match<Current<AccessInfo.userName>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[CashAccount(typeof(SOOrder.branchID), typeof(Search2<CashAccount.cashAccountID,
				InnerJoin<PaymentMethodAccount,
					On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
						And<PaymentMethodAccount.useForAR, Equal<True>,
						And<PaymentMethodAccount.paymentMethodID,
						Equal<Current2<paymentMethodID>>>>>>,
						Where<Match<Current<AccessInfo.userName>>>>), SuppressCurrencyValidation = false, Required = true)]
		public virtual Int32? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }

		[PXString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PaymentRef(
			typeof(cashAccountID),
			typeof(paymentMethodID),
			typeof(updateNextNumber),
			typeof(isMigratedRecordStub))]
		public virtual String ExtRefNbr
		{
			get;
			set;
		}
		#endregion
		#region UpdateNextNumber
		public abstract class updateNextNumber : PX.Data.BQL.BqlBool.Field<updateNextNumber> { }

		[PXBool()]
		[PXUIField(DisplayName = "Update Next Number", Visibility = PXUIVisibility.Invisible)]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? UpdateNextNumber
		{
			get;
			set;
		}
		#endregion
		#region DocDesc
		public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }

		[PXString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String DocDesc
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDefault(typeof(
			Coalesce<Search<CashAccount.curyID,
				Where<CashAccount.cashAccountID, Equal<Current<cashAccountID>>>>,
			Search<Branch.baseCuryID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

		[PXLong()]
		[CurrencyInfo(ModuleCode = BatchModule.AR)]
		public virtual Int64? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }

		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCurrency(typeof(curyInfoID), typeof(origDocAmt))]
		[PXUIField(DisplayName = "Payment Amount", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual Decimal? CuryOrigDocAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigDocAmt
		public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }

		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? OrigDocAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryRefundAmt
		public abstract class curyRefundAmt : PX.Data.BQL.BqlDecimal.Field<curyRefundAmt> { }

		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXCurrency(typeof(curyInfoID), typeof(refundAmt))]
		[PXUIField(DisplayName = "Refund Amount", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual decimal? CuryRefundAmt
		{
			get;
			set;
		}
		#endregion
		#region RefundAmt
		public abstract class refundAmt : PX.Data.BQL.BqlDecimal.Field<refundAmt> { }

		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? RefundAmt
		{
			get;
			set;
		}
		#endregion
		#region IsMigratedRecordStub
		public abstract class isMigratedRecordStub : PX.Data.BQL.BqlBool.Field<isMigratedRecordStub> { }
		// TODO: SOCreatePayment: Remove this field
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? IsMigratedRecordStub
		{
			get;
			set;
		}
		#endregion
		#region NewCard
		public abstract class newCard : PX.Data.BQL.BqlBool.Field<newCard> { }

		[PXBool]
		[PXUIField(DisplayName = "New Card")]
		public virtual bool? NewCard
		{
			get;
			set;
		}
		#endregion
		#region NewAccount
		public abstract class newAccount : PX.Data.BQL.BqlBool.Field<newAccount> { }

		[PXBool]
		[PXUIField(DisplayName = "New Account")]
		public virtual bool? NewAccount
		{
			get => this.NewCard;
			set => this.NewCard = value;
		}
		#endregion
		#region SaveCard
		public abstract class saveCard : PX.Data.BQL.BqlBool.Field<saveCard> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Save Card")]
		public virtual bool? SaveCard
		{
			get;
			set;
		}
		#endregion
		#region SaveAccount
		public abstract class saveAccount : PX.Data.BQL.BqlBool.Field<saveAccount> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Save Account")]
		public virtual bool? SaveAccount
		{
			get => this.SaveCard;
			set => this.SaveCard = value;
		}
		#endregion
		#region ProcessingCenterID
		public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }

		[PXString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Proc. Center ID")]
		[PXDefault(typeof(Coalesce<
			Search<CustomerPaymentMethod.cCProcessingCenterID,
				Where<CustomerPaymentMethod.pMInstanceID, Equal<Current<pMInstanceID>>>>,
			Search2<CCProcessingCenterPmntMethod.processingCenterID,
				InnerJoin<CCProcessingCenter, On<CCProcessingCenter.processingCenterID, Equal<CCProcessingCenterPmntMethod.processingCenterID>>>,
				Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<paymentMethodID>>,
					And<CCProcessingCenterPmntMethod.isActive, Equal<True>,
					And<CCProcessingCenterPmntMethod.isDefault, Equal<True>,
					And<CCProcessingCenter.isActive, Equal<True>>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search2<CCProcessingCenter.processingCenterID,
			InnerJoin<CCProcessingCenterPmntMethod, On<CCProcessingCenterPmntMethod.processingCenterID, Equal<CCProcessingCenter.processingCenterID>>>,
			Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<paymentMethodID>>,
				And<CCProcessingCenterPmntMethod.isActive, Equal<True>,
				And<CCProcessingCenter.isActive, Equal<True>>>>>), DescriptionField = typeof(CCProcessingCenter.name), ValidateValue = false)]
		[DisabledProcCenter(CheckFieldValue = DisabledProcCenterAttribute.CheckFieldVal.ProcessingCenterId)]
		public virtual string ProcessingCenterID
		{
			get;
			set;
		}
		#endregion

		#region Authorize
		public abstract class authorize : Data.BQL.BqlBool.Field<authorize> { }

		[PXBool]
		public virtual bool? Authorize
		{
			get;
			set;
		}
		#endregion
		#region Capture
		public abstract class capture : Data.BQL.BqlBool.Field<capture> { }

		[PXBool]
		public virtual bool? Capture
		{
			get;
			set;
		}
		#endregion
		#region Refund
		public abstract class refund : Data.BQL.BqlBool.Field<refund> { }

		[PXBool]
		public virtual bool? Refund
		{
			get;
			set;
		}
		#endregion

		#region AdjgDocType
		public new abstract class adjgDocType : Data.BQL.BqlString.Field<adjgDocType> { }
		[PXString(3, IsKey = true, IsFixed = true)]
		public virtual string AdjgDocType
		{
			get;
			set;
		}
		#endregion
		#region AdjgRefNbr
		public new abstract class adjgRefNbr : Data.BQL.BqlString.Field<adjgRefNbr> { }
		[PXString(15, IsUnicode = true, IsKey = true)]
		public virtual string AdjgRefNbr
		{
			get;
			set;
		}
		#endregion
	}
}
