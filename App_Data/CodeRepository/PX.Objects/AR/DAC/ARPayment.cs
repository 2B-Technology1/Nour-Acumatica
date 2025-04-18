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
using PX.Data.BQL;
using PX.Data.WorkflowAPI;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Descriptor;
using PX.Objects.CM.Extensions;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.PM;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.Common;
using PX.Objects.Common.Attributes;
using PX.Objects.Common.Interfaces;
using PX.Objects.Common;

namespace PX.Objects.AR
{
	/// <summary>
	/// Represents an Accounts Receivable payment or a payment-like document, which can
	/// have one of the types defined by <see cref="ARPaymentType.ListAttribute"/>, such
	/// as credit memo, customer refund, or balance write-off. The entities of this type
	/// are edited on the Payments and Applications (AR302000) form, which corresponds
	/// to the <see cref="ARPaymentEntry"/> graph.
	/// </summary>
	/// <remarks>
	/// Credit memo, cash sale and cash return documents consist of both an 
	/// <see cref="ARInvoice">invoice</see> record and a corresponding 
	/// <see cref="ARPayment">payment</see> record.
	/// </remarks>
	[PXCacheName(Messages.ARPayment)]
	[PXTable]
	[PXSubstitute(GraphType = typeof(ARPaymentEntry))]
	[PXPrimaryGraph(
		new Type[] {
			typeof(ARCashSaleEntry ),
			typeof(ARPaymentEntry)},
		new Type[] {
	typeof(Select<AR.Standalone.ARCashSale,
			Where2<Where<AR.Standalone.ARCashSale.docType, Equal<ARDocType.cashSale>, Or<ARRegister.docType, Equal<ARDocType.cashReturn>>>,
			And<AR.Standalone.ARCashSale.docType, Equal<Current<ARPayment.docType>>,
			And<AR.Standalone.ARCashSale.refNbr, Equal<Current<ARPayment.refNbr>>>>>>),
	typeof(Select<ARPayment,
			Where<ARPayment.docType, Equal<Current<ARPayment.docType>>,
			And<ARPayment.refNbr, Equal<Current<ARPayment.refNbr>>>>>)})]
	[PXGroupMask(typeof(InnerJoinSingleTable<Customer, On<Customer.bAccountID, Equal<ARPayment.customerID>, And<Match<Customer, Current<AccessInfo.userName>>>>>))]
	public class ARPayment : ARRegister, CM.IInvoice, ICCPayment, IApprovable, IApprovalDescription, IReserved
	{
		#region Keys
		/// <exclude/>
		public new class PK : PrimaryKeyOf<ARPayment>.By<docType, refNbr>
		{
			public static ARPayment Find(PXGraph graph, string docType, string refNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, docType, refNbr, options);
		}
		public new static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<ARPayment>.By<branchID> { }
			public class Customer : AR.Customer.PK.ForeignKeyOf<ARPayment>.By<customerID> { }
			public class CustomerLocation : CR.Location.PK.ForeignKeyOf<ARPayment>.By<customerID, customerLocationID> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<ARPayment>.By<curyInfoID> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<ARPayment>.By<curyID> { }
			public class CashAccount : CA.CashAccount.PK.ForeignKeyOf<ARPayment>.By<cashAccountID> { }
			public class PaymentMethod : CA.PaymentMethod.PK.ForeignKeyOf<ARPayment>.By<paymentMethodID> { }
			public class CustomerPaymentMethod : AR.CustomerPaymentMethod.PK.ForeignKeyOf<ARPayment>.By<pMInstanceID> { }
		}
		#endregion
		#region Events
		public class Events : PXEntityEvent<ARPayment>.Container<Events>
		{
			public PXEntityEvent<ARPayment> ReleaseDocument;
			public PXEntityEvent<ARPayment> CloseDocument;
			public PXEntityEvent<ARPayment> OpenDocument;
			public PXEntityEvent<ARPayment> VoidDocument;
		}

		#endregion
		#region Selected
		public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		#endregion
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[ARPaymentType.ListEx()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		[PXFieldDescription]
		public override String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[ARPaymentType.RefNbr(typeof(Search2<Standalone.ARRegisterAlias.refNbr,
			InnerJoinSingleTable<ARPayment, On<ARPayment.docType, Equal<Standalone.ARRegisterAlias.docType>,
				And<ARPayment.refNbr, Equal<Standalone.ARRegisterAlias.refNbr>>>,
			InnerJoinSingleTable<Customer, On<Standalone.ARRegisterAlias.customerID, Equal<Customer.bAccountID>>>>,
			Where<Standalone.ARRegisterAlias.docType, Equal<Current<ARPayment.docType>>,
				And<Match<Customer, Current<AccessInfo.userName>>>>,
			OrderBy<Desc<Standalone.ARRegisterAlias.refNbr>>>), Filterable = true, IsPrimaryViewCompatible = true)]
		[ARPaymentType.Numbering()]
		[PXFieldDescription]
		public override String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region CustomerID
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[Customer(Visibility = PXUIVisibility.SelectorVisible, Filterable = true, TabOrder = 2)]
		[PXRestrictor(typeof(Where<Customer.status, Equal<CustomerStatus.active>,
					Or<Customer.status, Equal<CustomerStatus.oneTime>,
					Or<Customer.status, Equal<CustomerStatus.hold>,
					Or<Customer.status, Equal<CustomerStatus.creditHold>>>>>), Messages.CustomerIsInStatus, typeof(Customer.status))]
		[PXUIField(DisplayName = "Customer", TabOrder = 2)]
		[PXDefault()]
		public override Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region CustomerLocationID
		public new abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
		#endregion
		#region CuryOrigTaxDiscAmt
		public abstract class curyOrigTaxDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigTaxDiscAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal]
		public virtual decimal? CuryOrigTaxDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigTaxDiscAmt
		public abstract class origTaxDiscAmt : PX.Data.BQL.BqlDecimal.Field<origTaxDiscAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal]
		public virtual decimal? OrigTaxDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region Status
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		protected String _PaymentMethodID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Coalesce<
			Search2<CustomerPaymentMethod.paymentMethodID, 
				InnerJoin<Customer, On<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>,
				Where<Customer.bAccountID, Equal<Current<ARPayment.customerID>>,
					And<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>>>>,
			 Search<Customer.defPaymentMethodID,
				Where<Customer.bAccountID, Equal<Current<ARPayment.customerID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search5<PaymentMethod.paymentMethodID, 
			LeftJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
				And<CustomerPaymentMethod.bAccountID, Equal<Current<ARPayment.customerID>>>>,
			LeftJoin<CCProcessingCenterPmntMethod, On<CCProcessingCenterPmntMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>>,
			LeftJoin<CCProcessingCenter, On<CCProcessingCenter.processingCenterID, Equal<CCProcessingCenterPmntMethod.processingCenterID>>>>>,
			Where<PaymentMethod.isActive, Equal<True>, And<PaymentMethod.useForAR, Equal<True>>>,
			Aggregate<GroupBy<PaymentMethod.paymentMethodID, GroupBy<PaymentMethod.useForAR, GroupBy<PaymentMethod.useForAP>>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXUIField(DisplayName = "Payment Method", Enabled = false)]
		[PXForeignReference(typeof(Field<paymentMethodID>.IsRelatedTo<PaymentMethod.paymentMethodID>))]
		public virtual String PaymentMethodID
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
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[Branch(typeof(AccessInfo.branchID), IsDetail = false, TabOrder =0)]
		[PXFormula(typeof(Switch<Case<Where<PendingValue<ARPayment.branchID>, IsPending>, Null,
								Case<Where<ARPayment.customerLocationID, IsNotNull,
										And<Selector<ARPayment.customerLocationID, Location.cBranchID>, IsNotNull>>,
									Selector<ARPayment.customerLocationID, Location.cBranchID>,
								Case<Where<Current2<ARPayment.branchID>, IsNotNull>,
									Current2<ARPayment.branchID>>>>,
								Current<AccessInfo.branchID>>))]
		public override Int32? BranchID
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
		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		protected Int32? _PMInstanceID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Card/Account Nbr.")]
		[PXDefault(typeof(Coalesce<
						Search2<Customer.defPMInstanceID, InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<Customer.defPMInstanceID>,
								And<CustomerPaymentMethod.bAccountID, Equal<Customer.bAccountID>>>>,
								Where<Customer.bAccountID, Equal<Current<ARPayment.customerID>>,
								  And<CustomerPaymentMethod.isActive, Equal<True>,
								  And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<ARPayment.paymentMethodID>>>>>>,
						Search<CustomerPaymentMethod.pMInstanceID,
								Where<CustomerPaymentMethod.bAccountID, Equal<Current<ARPayment.customerID>>,
									And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<ARPayment.paymentMethodID>>,
									And<CustomerPaymentMethod.isActive, Equal<True>>>>, OrderBy<Desc<CustomerPaymentMethod.expirationDate, Desc<CustomerPaymentMethod.pMInstanceID>>>>>)
						, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID, Where<CustomerPaymentMethod.bAccountID, Equal<Current<ARPayment.customerID>>,
			And<CustomerPaymentMethod.paymentMethodID, Equal<Current2<ARPayment.paymentMethodID>>,
			And<Where<CustomerPaymentMethod.isActive, Equal<True>, Or<CustomerPaymentMethod.pMInstanceID,
				Equal<Current<ARPayment.pMInstanceID>>>>>>>>), DescriptionField = typeof(CustomerPaymentMethod.descr))]
		[DeprecatedProcessing]
		[DisabledProcCenter]
		[PXForeignReference(
			typeof(CompositeKey<
				Field<ARPayment.customerID>.IsRelatedTo<CustomerPaymentMethod.bAccountID>,
				Field<ARPayment.pMInstanceID>.IsRelatedTo<CustomerPaymentMethod.pMInstanceID>
			>))]
		[PXExcludeRowsFromReferentialIntegrityCheck(
			ForeignTableExcludingConditions = typeof(ExcludeWhen<PaymentMethod>
				.Joined<On<PaymentMethod.paymentMethodID, Equal<ARPayment.paymentMethodID>>>
				.Satisfies<Where<PaymentMethod.paymentType, In3<PaymentMethodType.creditCard, PaymentMethodType.eft>>>))]
		public virtual Int32? PMInstanceID
		{
			get
			{
				return this._PMInstanceID;
			}
			set
			{
				this._PMInstanceID = value;
			}
		}
		#endregion
		#region ProcessingCenterID
		public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Proc. Center ID")]
		[PXDefault(typeof(Coalesce<
			Search<CustomerPaymentMethod.cCProcessingCenterID,
				Where<CustomerPaymentMethod.pMInstanceID,Equal<Current2<ARPayment.pMInstanceID>>>>,
			Search2<CCProcessingCenterPmntMethod.processingCenterID,
				InnerJoin<CCProcessingCenter, On<CCProcessingCenter.processingCenterID, Equal<CCProcessingCenterPmntMethod.processingCenterID>>>,
				Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<ARPayment.paymentMethodID>>,
					And<CCProcessingCenterPmntMethod.isActive, Equal<True>,
					And<CCProcessingCenterPmntMethod.isDefault, Equal<True>,
					And<CCProcessingCenter.isActive, Equal<True>>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search2<CCProcessingCenter.processingCenterID, 
			InnerJoin<CCProcessingCenterPmntMethod, On<CCProcessingCenterPmntMethod.processingCenterID, Equal<CCProcessingCenter.processingCenterID>>>,
			Where<CCProcessingCenterPmntMethod.paymentMethodID, Equal<Current<ARPayment.paymentMethodID>>,
				And<CCProcessingCenterPmntMethod.isActive, Equal<True>,
				And<CCProcessingCenter.isActive, Equal<True>>>>>), DescriptionField = typeof(CCProcessingCenter.name), ValidateValue = false)]
		[DeprecatedProcessing(ChckVal = DeprecatedProcessingAttribute.CheckVal.ProcessingCenterId)]
		[DisabledProcCenter(CheckFieldValue = DisabledProcCenterAttribute.CheckFieldVal.ProcessingCenterId)]
		public virtual string ProcessingCenterID
		{
			get;
			set;
		}
		#endregion
		#region SyncLock
		public abstract class syncLock : IBqlField
		{
		}
		[PXDBBool]
		public virtual bool? SyncLock
		{
			get;
			set;
		}
		#endregion
		#region SyncLockReason
		public abstract class syncLockReason : PX.Data.BQL.BqlString.Field<syncLockReason>
		{
			public const string NewCard = "N";
			public const string NeedValidation = "V";
			public class newCard : BqlString.Constant<newCard> { public newCard() : base(NewCard) { } }
			public class needValidation : BqlString.Constant<newCard> { public needValidation() : base(NeedValidation) { } }
		}
		[PXDBString(1, IsFixed = true, IsUnicode = false)]
		public virtual string SyncLockReason
		{
			get;
			set;
		}
		#endregion
		#region NewCard
		public abstract class newCard : PX.Data.IBqlField
		{
		}
		[PXBool]
		[PXUIField(DisplayName = "New Card")]
		public virtual bool? NewCard
		{
			get;
			set;
		}
		#endregion
		#region NewAccount
		public abstract class newAccount : PX.Data.IBqlField { }
		[PXBool]
		[PXUIField(DisplayName = "New Account")]
		public virtual bool? NewAccount
		{
			get => this.NewCard;
			set => this.NewCard = value;
		}
		#endregion
		#region PMInstanceID_CustomerPaymentMethod_descr
		public abstract class pMInstanceID_CustomerPaymentMethod_descr : PX.Data.BQL.BqlString.Field<pMInstanceID_CustomerPaymentMethod_descr> { }
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		protected Int32? _CashAccountID;
		[PXDefault(typeof(Coalesce<Search2<CustomerPaymentMethod.cashAccountID,
									InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID,Equal<CustomerPaymentMethod.cashAccountID>,
										And<PaymentMethodAccount.paymentMethodID,Equal<CustomerPaymentMethod.paymentMethodID>,
										And<PaymentMethodAccount.useForAR, Equal<True>>>>>, 
								Where<CustomerPaymentMethod.bAccountID, Equal<Current<ARPayment.customerID>>,
								And<Current<ARPayment.docType>, NotEqual<ARDocType.refund>,
								And<CustomerPaymentMethod.pMInstanceID, Equal<Current2<ARPayment.pMInstanceID>>>>>>, 
							Search2<CashAccount.cashAccountID, 
								InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>, 
									And<PaymentMethodAccount.useForAR,Equal<True>,
									And2<Where2<Where<Current<ARPayment.docType>,NotEqual<ARDocType.refund>,
										And<PaymentMethodAccount.aRIsDefault,Equal<True>>>,
										Or<Where<Current<ARPayment.docType>,Equal<ARDocType.refund>,
										And<PaymentMethodAccount.aRIsDefaultForRefund,Equal<True>>>>>,
									And<PaymentMethodAccount.paymentMethodID, Equal<Current2<ARPayment.paymentMethodID>>>>>>>,
									Where<CashAccount.branchID,Equal<Current<ARPayment.branchID>>,
										And<Match<Current<AccessInfo.userName>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[CashAccount(typeof(ARPayment.branchID), typeof(Search<CashAccount.cashAccountID,
					Where2<Match<Current<AccessInfo.userName>>,
						And<CashAccount.cashAccountID, In2<Search<PaymentMethodAccount.cashAccountID,
							Where<PaymentMethodAccount.paymentMethodID, Equal<Current2<ARPayment.paymentMethodID>>,
							And<PaymentMethodAccount.useForAR, Equal<True>>>>>>>>), Visibility = PXUIVisibility.Visible)]
		public virtual Int32? CashAccountID
		{
			get
			{
				return this._CashAccountID;
			}
			set
			{
				this._CashAccountID = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[ProjectDefault(BatchModule.AR,
			typeof(Search<Location.cDefProjectID,
				Where<Location.bAccountID, Equal<Current<ARPayment.customerID>>,
					And<Location.locationID, Equal<Current<ARPayment.customerLocationID>>,
					And<ARDocType.creditMemo, NotEqual<Current<ARPayment.docType>>>>>>),
			typeof(ARPayment.cashAccountID))]
		[PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.visibleInAR, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
		[ProjectBaseAttribute()]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[ActiveProjectTask(typeof(ARPayment.projectID), BatchModule.AR, NeedTaskValidationField = typeof(needTaskValidation), DisplayName = "Project Task")]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region UpdateNextNumber
		public abstract class updateNextNumber : PX.Data.BQL.BqlBool.Field<updateNextNumber> { }
		protected Boolean? _UpdateNextNumber;

		[PXBool()]
		[PXUIField(DisplayName = "Update Next Number", Visibility = PXUIVisibility.Invisible)]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? UpdateNextNumber
		{
			get
			{
				return this._UpdateNextNumber;
			}
			set
			{
				this._UpdateNextNumber = value;
			}
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		protected String _ExtRefNbr;
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PaymentRef(
			typeof(ARPayment.cashAccountID), 
			typeof(ARPayment.paymentMethodID), 
			typeof(ARPayment.updateNextNumber),
			typeof(ARPayment.isMigratedRecord))]
		public virtual String ExtRefNbr
		{
			get
			{
				return this._ExtRefNbr;
			}
			set
			{
				this._ExtRefNbr = value;
			}
		}
		#endregion
		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		#endregion
		#region AdjDate
		public abstract class adjDate : PX.Data.BQL.BqlDateTime.Field<adjDate> { }
		protected DateTime? _AdjDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Application Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? AdjDate
		{
			get
			{
				return this._AdjDate;
			}
			set
			{
				this._AdjDate = value;
			}
		}
		#endregion
		#region AdjFinPeriodID
		public abstract class adjFinPeriodID : PX.Data.BQL.BqlString.Field<adjFinPeriodID> { }
		protected String _AdjFinPeriodID;
		[AROpenPeriod(
			typeof(ARPayment.adjDate),
			branchSourceType: typeof(ARPayment.branchID),
            masterFinPeriodIDType: typeof(ARPayment.adjTranPeriodID),
			selectionModeWithRestrictions: FinPeriodSelectorAttribute.SelectionModesWithRestrictions.All,
		    IsHeader = true)]
		[PXUIField(DisplayName = "Application Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String AdjFinPeriodID
		{
			get
			{
				return this._AdjFinPeriodID;
			}
			set
			{
				this._AdjFinPeriodID = value;
			}
		}
		#endregion
		#region AdjTranPeriodID
		public abstract class adjTranPeriodID : PX.Data.BQL.BqlString.Field<adjTranPeriodID> { }
		protected String _AdjTranPeriodID;
		[PeriodID]
		public virtual String AdjTranPeriodID
		{
			get
			{
				return this._AdjTranPeriodID;
			}
			set
			{
				this._AdjTranPeriodID = value;
			}
		}
		#endregion
		#region DocDate
		public new abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Payment Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public override DateTime? DocDate
		{
			get
			{
				return this._DocDate;
			}
			set
			{
				this._DocDate = value;
			}
		}
		#endregion
		#region TranPeriodID
		public new abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		[PeriodID]
		public override String TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[AROpenPeriod(
			typeof(ARPayment.docDate),
            masterFinPeriodIDType: typeof(ARPayment.tranPeriodID),
			selectionModeWithRestrictions: FinPeriodSelectorAttribute.SelectionModesWithRestrictions.All,
			sourceSpecificationTypes:
			new[]
			{
				typeof(CalendarOrganizationIDProvider.SourceSpecification<ARPayment.branchID, True>),
				typeof(CalendarOrganizationIDProvider.SourceSpecification<
					ARPayment.cashAccountID,
					Selector<ARPayment.cashAccountID, CashAccount.branchID>,
					False>),
			})]
		[PXDefault()]
		[PXUIField(DisplayName = "Payment Period", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public override String FinPeriodID
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
		#region CuryDocBal
		public new abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
		#endregion
		#region CuryOrigDocAmt
		public new abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARPayment.curyInfoID), typeof(ARPayment.origDocAmt))]
		[PXUIField(DisplayName = "Payment Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public override Decimal? CuryOrigDocAmt
		{
			get
			{
				return this._CuryOrigDocAmt;
			}
			set
			{
				this._CuryOrigDocAmt = value;
			}
		}
		#endregion
		#region OrigDocAmt
		public new abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		#endregion
		#region CuryConsolidateChargeTotal
		public abstract class curyConsolidateChargeTotal : PX.Data.BQL.BqlDecimal.Field<curyConsolidateChargeTotal> { }
		protected Decimal? _CuryConsolidateChargeTotal;
		[PXDBCurrency(typeof(ARPayment.curyInfoID), typeof(ARPayment.consolidateChargeTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Deducted Charges", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? CuryConsolidateChargeTotal
		{
			get
			{
				return this._CuryConsolidateChargeTotal;
			}
			set
			{
				this._CuryConsolidateChargeTotal = value;
			}
		}
		#endregion
		#region ConsolidateChargeTotal
		public abstract class consolidateChargeTotal : PX.Data.BQL.BqlDecimal.Field<consolidateChargeTotal> { }
		protected Decimal? _ConsolidateChargeTotal;
		[PXDBDecimal(4)]
		public virtual Decimal? ConsolidateChargeTotal
		{
			get
			{
				return this._ConsolidateChargeTotal;
			}
			set
			{
				this._ConsolidateChargeTotal = value;
			}
		}
		#endregion
		#region AdjCntr
		public new abstract class adjCntr : PX.Data.BQL.BqlInt.Field<adjCntr> { }
		#endregion
		#region ChargeCntr
		public abstract class chargeCntr : PX.Data.BQL.BqlInt.Field<chargeCntr> { }
		protected Int32? _ChargeCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? ChargeCntr
		{
			get
			{
				return this._ChargeCntr;
			}
			set
			{
				this._ChargeCntr = value;
			}
		}
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		#endregion
		#region CuryUnappliedBal
		public abstract class curyUnappliedBal : PX.Data.BQL.BqlDecimal.Field<curyUnappliedBal> { }
		protected Decimal? _CuryUnappliedBal;
		[PXCurrency(typeof(ARPayment.curyInfoID), typeof(ARPayment.unappliedBal))]
		[PXUIField(DisplayName = "Available Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXFormula(typeof(Sub<ARPayment.curyDocBal, Add<ARPayment.curyApplAmt, ARPayment.curySOApplAmt>>))]
		public virtual Decimal? CuryUnappliedBal
		{
			get
			{
				return this._CuryUnappliedBal;
			}
			set
			{
				this._CuryUnappliedBal = value;
			}
		}
		#endregion
		#region UnappliedBal
		public abstract class unappliedBal : PX.Data.BQL.BqlDecimal.Field<unappliedBal> { }
		protected Decimal? _UnappliedBal;
		[PXDecimal(4)]
		public virtual Decimal? UnappliedBal
		{
			get
			{
				return this._UnappliedBal;
			}
			set
			{
				this._UnappliedBal = value;
			}
		}
		#endregion
		#region CuryInitDocBal
		public new abstract class curyInitDocBal : PX.Data.BQL.BqlDecimal.Field<curyInitDocBal> { }

		/// <summary>
		/// The entered in migration mode balance of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARRegister.curyInfoID), typeof(ARRegister.initDocBal))]
		[PXUIField(DisplayName = "Available Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXUIVerify(typeof(
			Where<ARPayment.hold, Equal<True>,
				Or<ARPayment.openDoc, NotEqual<True>,
				Or<ARPayment.isMigratedRecord, NotEqual<True>,
				Or2<Where<ARPayment.voidAppl, Equal<True>,
					And<ARPayment.curyInitDocBal, LessEqual<decimal0>,
					And<ARPayment.curyInitDocBal, GreaterEqual<ARPayment.curyOrigDocAmt>>>>,
				Or<Where<ARPayment.voidAppl, NotEqual<True>,
					And<ARPayment.curyInitDocBal, GreaterEqual<decimal0>,
					And<ARPayment.curyInitDocBal, LessEqual<ARPayment.curyOrigDocAmt>>>>>>>>>),
			PXErrorLevel.Error, Common.Messages.IncorrectMigratedBalance,
			CheckOnInserted = false, 
			CheckOnRowSelected = false, 
			CheckOnVerify = false, 
			CheckOnRowPersisting = true)]
		public override decimal? CuryInitDocBal
		{
			get;
			set;
		}
		#endregion
		#region CuryApplAmt
		public abstract class curyApplAmt : PX.Data.BQL.BqlDecimal.Field<curyApplAmt> { }
		protected Decimal? _CuryApplAmt;
		[PXCurrency(typeof(ARPayment.curyInfoID), typeof(ARPayment.applAmt))]
		[PXUIField(DisplayName = "Applied to Documents", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		// Formula on ARAdjust that recalculates this field relies on PXParentAttribute 
		// that relies on these two fields. So we should keep it here to make optimized CB API mode working correctly.
		[PXDependsOnFields(typeof(ARPayment.adjCntr), typeof(ARPayment.released))] 
		public virtual Decimal? CuryApplAmt
		{
			get
			{
				return this._CuryApplAmt;
			}
			set
			{
				this._CuryApplAmt = value;
			}
		}
		#endregion
		#region CurySOApplAmt
		public abstract class curySOApplAmt : PX.Data.BQL.BqlDecimal.Field<curySOApplAmt> { }
		protected Decimal? _CurySOApplAmt;
		[PXCurrency(typeof(ARPayment.curyInfoID), typeof(ARPayment.sOApplAmt))]
		[PXUIField(DisplayName = "Applied to Orders", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? CurySOApplAmt
		{
			get
			{
				return this._CurySOApplAmt;
			}
			set
			{
				this._CurySOApplAmt = value;
			}
		}
		#endregion
		#region SOApplAmt
		public abstract class sOApplAmt : PX.Data.BQL.BqlDecimal.Field<sOApplAmt> { }
		protected Decimal? _SOApplAmt;
		[PXDecimal(4)]
		public virtual Decimal? SOApplAmt
		{
			get
			{
				return this._SOApplAmt;
			}
			set
			{
				this._SOApplAmt = value;
			}
		}
		#endregion
		#region ApplAmt
		public abstract class applAmt : PX.Data.BQL.BqlDecimal.Field<applAmt> { }
		protected Decimal? _ApplAmt;
		[PXDecimal(4)]
		public virtual Decimal? ApplAmt
		{
			get
			{
				return this._ApplAmt;
			}
			set
			{
				this._ApplAmt = value;
			}
		}
		#endregion
		#region CuryWOAmt
		public abstract class curyWOAmt : PX.Data.BQL.BqlDecimal.Field<curyWOAmt> { }
		protected Decimal? _CuryWOAmt;
		[PXCurrency(typeof(ARPayment.curyInfoID), typeof(ARPayment.wOAmt))]
		[PXUIField(DisplayName = "Write-Off Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? CuryWOAmt
		{
			get
			{
				return this._CuryWOAmt;
			}
			set
			{
				this._CuryWOAmt = value;
			}
		}
		#endregion
		#region WOAmt
		public abstract class wOAmt : PX.Data.BQL.BqlDecimal.Field<wOAmt> { }
		protected Decimal? _WOAmt;
		[PXDecimal(4)]
		public virtual Decimal? WOAmt
		{
			get
			{
				return this._WOAmt;
			}
			set
			{
				this._WOAmt = value;
			}
		}
		#endregion
		#region Cleared
		public abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared> { }
		protected Boolean? _Cleared;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cleared")]
		public virtual Boolean? Cleared
		{
			get
			{
				return this._Cleared;
			}
			set
			{
				this._Cleared = value;
			}
		}
		#endregion
		#region ClearDate
		public abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate> { }
		protected DateTime? _ClearDate;
		[PXDBDate]
		[PXUIField(DisplayName = "Clear Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? ClearDate
		{
			get
			{
				return this._ClearDate;
			}
			set
			{
				this._ClearDate = value;
			}
		}
		#endregion
		#region BatchNbr
		public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		#endregion
		#region Voided
		public new abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		[PXDBBool()]
		[PXUIField(DisplayName = "Voided", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public override Boolean? Voided
		{
			get
			{
				return this._Voided;
			}
			set
			{
				this._Voided = value;
			}
		}
		#endregion
		#region VoidAppl
		public abstract class voidAppl : PX.Data.BQL.BqlBool.Field<voidAppl> { }
		[PXBool()]
		[PXUIField(DisplayName = "Void Application", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? VoidAppl
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARPaymentType.VoidAppl(this._DocType);
			}
			set
			{
				if ((bool)value && !ARPaymentType.VoidAppl(DocType))
				{
					DocType = ARPaymentType.GetVoidingARDocType(DocType);
				}
			}
		}
		#endregion
		#region CustomerID_Customer_acctName
		public new abstract class customerID_Customer_acctName : PX.Data.BQL.BqlString.Field<customerID_Customer_acctName> { }
		#endregion
		#region CanHaveBalance
		public abstract class canHaveBalance : PX.Data.BQL.BqlBool.Field<canHaveBalance> { }
		[PXBool()]
		[PXUIField(DisplayName = "Can Have Balance", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? CanHaveBalance
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARPaymentType.CanHaveBalance(this._DocType);
			}
			set
			{
			}
		}
		#endregion
		#region Released
		public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		#endregion
		#region Hold
		public new abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		#endregion
		#region OpenDoc
		public new abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }
		protected string _DrCr;
		[PXString(1, IsFixed = true)]
		public virtual string DrCr
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return ARPaymentType.DrCr(this._DocType);
			}
			set
			{
			}
		}
		#endregion
		#region CATranID
		public abstract class cATranID : PX.Data.BQL.BqlLong.Field<cATranID> { }
		protected Int64? _CATranID;
		[PXDBLong()]
		[ARCashTranID()]
		public virtual Int64? CATranID
		{
			get
			{
				return this._CATranID;
			}
			set
			{
				this._CATranID = value;
			}
		}
		#endregion
		#region DiscDate
		public virtual DateTime? DiscDate
		{
			get
			{
				return new DateTime();
			}
			set
			{
				;
			}
		}
		#endregion
		#region CuryWhTaxBal
		public virtual Decimal? CuryWhTaxBal
		{
			get
			{
				return 0m;
			}
			set
			{
				;
			}
		}
		#endregion
		#region WhTaxBal
		public virtual Decimal? WhTaxBal
		{
			get
			{
				return 0m;
			}
			set
			{
				;
			}
		}
		#endregion
		#region CustomerPaymentMethod_descr
		public abstract class CustomerPaymentMethod_descr : PX.Data.BQL.BqlString.Field<CustomerPaymentMethod_descr> { }
		#endregion
		#region IsCCPayment
		public abstract class isCCPayment : PX.Data.BQL.BqlBool.Field<isCCPayment> { }

		protected bool? _IsCCPayment;
		[PXDBBool]
		[PXDefault(false)]
		[PXFormula(typeof(Switch<Case<Where<isMigratedRecord, Equal<False>,
			And<Selector<paymentMethodID, PaymentMethod.paymentType>, In3<PaymentMethodType.creditCard, PaymentMethodType.eft>,
			And<Selector<paymentMethodID, PaymentMethod.aRIsProcessingRequired>, Equal<True>>>>, True>,
			False>))]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual bool? IsCCPayment
		{
			get
			{
				return this._IsCCPayment;
			}
			set
			{
				this._IsCCPayment = value;
			}
		}
		#endregion
		#region IsCCAuthorized
		public abstract class isCCAuthorized : PX.Data.BQL.BqlBool.Field<isCCAuthorized> { }
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsCCAuthorized
		{
			get;
			set;
		}
		#endregion
		#region IsCCCaptured
		public abstract class isCCCaptured : PX.Data.BQL.BqlBool.Field<isCCCaptured> { }
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsCCCaptured
		{
			get;
			set;
		}
		#endregion
		#region IsCCCaptureFailed
		public abstract class isCCCaptureFailed : PX.Data.BQL.BqlBool.Field<isCCCaptureFailed> { }
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsCCCaptureFailed
		{
			get;
			set;
		}
		#endregion
		#region IsCCRefunded
		public abstract class isCCRefunded : PX.Data.BQL.BqlBool.Field<isCCRefunded> { }
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsCCRefunded
		{
			get;
			set;
		}
		#endregion
		#region IsCCUserAttention
		public abstract class isCCUserAttention : PX.Data.BQL.BqlBool.Field<isCCUserAttention> { }
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsCCUserAttention
		{
			get;
			set;
		}
		#endregion
		#region PendingProcessing
		public new abstract class pendingProcessing : Data.BQL.BqlBool.Field<pendingProcessing> { }
		#endregion
		#region SaveCard
		public abstract class saveCard : PX.Data.BQL.BqlBool.Field<saveCard> { }
		[PXDBBool]
		[PXDefault(false)]
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
		[PXUIField(DisplayName = "Save Account")]
		public virtual bool? SaveAccount
		{
			get => this.SaveCard;
			set => this.SaveCard = value;
		}
		#endregion
		#region CCPaymentStateDescr
		public abstract class cCPaymentStateDescr : PX.Data.BQL.BqlString.Field<cCPaymentStateDescr> { }
		protected String _CCPaymentStateDescr;
		[PXString(255)]
		[PXUIField(DisplayName = "Processing Status", Enabled = false)]
		public virtual String CCPaymentStateDescr
		{
			get
			{
				return this._CCPaymentStateDescr;
			}
			set
			{
				this._CCPaymentStateDescr = value;
			}
		}
		#endregion
		#region ARDepositAsBatch
		public abstract class depositAsBatch : PX.Data.BQL.BqlBool.Field<depositAsBatch> { }
		protected Boolean? _DepositAsBatch;
		[PXDBBool()]
		[PXDefault(false, typeof(Search<CashAccount.clearingAccount, Where<CashAccount.cashAccountID, Equal<Current<ARPayment.cashAccountID>>>>))]
		[PXUIField(DisplayName = "Batch Deposit", Enabled = false)]
		public virtual Boolean? DepositAsBatch
		{
			get
			{
				return this._DepositAsBatch;
			}
			set
			{
				this._DepositAsBatch = value;
			}
		}
		#endregion
		#region DepositAfter
		public abstract class depositAfter : PX.Data.BQL.BqlDateTime.Field<depositAfter> { }
		protected DateTime? _DepositAfter;
		[PXDBDate()]
		[PXDefault(PersistingCheck =PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Deposit After", Enabled=false,Visible=false)]
		public virtual DateTime? DepositAfter
		{
			get
			{
				return this._DepositAfter;
			}
			set
			{
				this._DepositAfter = value;
			}
		}
		#endregion
		#region DepositDate
		public abstract class depositDate : PX.Data.BQL.BqlDateTime.Field<depositDate> { }
		protected DateTime? _DepositDate;
		[PXDBDate()]		
		[PXUIField(DisplayName = "Batch Deposit Date", Enabled=false)]
		public virtual DateTime? DepositDate
		{
			get
			{
				return this._DepositDate;
			}
			set
			{
				this._DepositDate = value;
			}
		}
		#endregion
		#region Deposited
		public abstract class deposited : PX.Data.BQL.BqlBool.Field<deposited> { }
		protected Boolean? _Deposited;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Deposited",Enabled=false)]
		public virtual Boolean? Deposited
		{
			get
			{
				return this._Deposited;
			}
			set
			{
				this._Deposited = value;
			}
		}
		#endregion
		#region DepositType
		public abstract class depositType : PX.Data.BQL.BqlString.Field<depositType> { }
		protected String _DepositType;
		[PXDBString(3, IsFixed=true)]
		
		public virtual String DepositType
		{
			get
			{
				return this._DepositType;
			}
			set
			{
				this._DepositType = value;
			}
		}
		#endregion
		#region DepositNbr
		public abstract class depositNbr : PX.Data.BQL.BqlString.Field<depositNbr> { }
		protected String _DepositNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Deposit Nbr.", Enabled=false)]
		[PXSelector(typeof(Search<CADeposit.refNbr,Where<CADeposit.tranType,Equal<Current<ARPayment.depositType>>>>))]
		public virtual String DepositNbr
		{
			get
			{
				return this._DepositNbr;
			}
			set
			{
				this._DepositNbr = value;
			}
		}
		#endregion
		#region CARefTranAccountID
		protected Int32? _CARefTranAccountID;
		public virtual Int32? CARefTranAccountID
		{
			get
			{
				return this._CARefTranAccountID;
			}
			set
			{
				this._CARefTranAccountID = value;
			}
		}
		#endregion
		#region CARefTranID
		protected Int64? _CARefTranID;
		public virtual Int64? CARefTranID
		{
			get
			{
				return this._CARefTranID;
			}
			set
			{
				this._CARefTranID = value;
			}
		}
		#endregion
		#region CARefSplitLineNbr
		protected Int32? _CARefSplitLineNbr;
		public virtual Int32? CARefSplitLineNbr
		{
			get
			{
				return this._CARefSplitLineNbr;
			}
			set
			{
				this._CARefSplitLineNbr = value;
			}
		}
		#endregion
		#region CCTransactionRefund
		public abstract class cCTransactionRefund : PX.Data.BQL.BqlBool.Field<cCTransactionRefund> { }
		[PXDBBool]
		[PXDefault(typeof(IIf<Where<Current<docType>, Equal<ARDocType.refund>, 
			And<Selector<paymentMethodID, PaymentMethod.paymentType>, In3<PaymentMethodType.creditCard, PaymentMethodType.eft>,
			And<Selector<paymentMethodID, PaymentMethod.aRIsProcessingRequired>, Equal<True>>>>, True, False>))]
		[PXUIField(DisplayName = "Use Orig. Transaction for Refund", Visible = false)]
		public virtual bool? CCTransactionRefund
		{
			get;
			set;
		}
		#endregion
		#region RefTranExtNbr
		public abstract class refTranExtNbr : PX.Data.BQL.BqlString.Field<refTranExtNbr> { }
		protected String _RefTranExtNbr;
		[PXDBString(50, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[RefTranExtNbr]
		[PXUIField(DisplayName = "Orig. Transaction", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual String RefTranExtNbr
		{
			get
			{
				return this._RefTranExtNbr;
			}
			set
			{
				this._RefTranExtNbr = value;
			}
		}
		#endregion
		#region CCReauthDate

		public abstract class cCReauthDate : Data.BQL.BqlDateTime.Field<cCReauthDate> { }

		[PXDBDateAndTime]
		public virtual DateTime? CCReauthDate
		{
			get;
			set;
		}
		#endregion
		#region CCReauthTriesLeft

		public abstract class cCReauthTriesLeft : Data.BQL.BqlDateTime.Field<cCReauthTriesLeft> { }

		[PXDBInt]
		public virtual int? CCReauthTriesLeft
		{
			get;
			set;
		}
		#endregion
		#region CCActualExternalTransactionID

		public abstract class cCActualExternalTransactionID : Data.BQL.BqlInt.Field<cCActualExternalTransactionID> { }

		[PXDBInt]
		[PXDBChildIdentity(typeof(ExternalTransaction.transactionID))]
		public virtual int? CCActualExternalTransactionID
		{
			get;
			set;
		}
		#endregion
		#region IsMigratedRecord
		public new abstract class isMigratedRecord : PX.Data.BQL.BqlBool.Field<isMigratedRecord> { }
		#endregion
		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXSearchable(SM.SearchCategory.AR, Messages.SearchableTitleDocument, new Type[] { typeof(ARPayment.docType), typeof(ARPayment.refNbr), typeof(ARPayment.customerID), typeof(Customer.acctName) },
			new Type[] { typeof(ARPayment.extRefNbr), typeof(ARPayment.docDesc) },
			NumberFields = new Type[] { typeof(ARPayment.refNbr) },
			Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(ARPayment.docDate), typeof(ARPayment.status), typeof(ARPayment.extRefNbr) },
			Line2Format = "{0}", Line2Fields = new Type[] { typeof(ARPayment.docDesc) },
			MatchWithJoin = typeof(InnerJoin<Customer, On<Customer.bAccountID, Equal<ARPayment.customerID>>>),
			SelectForFastIndexing = typeof(Select2<ARPayment, InnerJoin<Customer, On<ARPayment.customerID, Equal<Customer.bAccountID>>>>)
		)]
		[PXNote()]
		public override Guid? NoteID
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

		#region NeedTaskValidation
		public abstract class needTaskValidation : PX.Data.BQL.BqlBool.Field<needTaskValidation> { }

		/// <summary>
		/// Indicates whether validation for the presence of the correct <see cref="TaskID"/> must be performed for the line before it is persisted to the database.
		/// </summary>
		[PXBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? NeedTaskValidation
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				if (this.DocType == ARPaymentType.CreditMemo )
					return false;
				return true;
			}
			set
			{

			}
		}
		#endregion

		#region PostponeReleasedFlag
		public abstract class postponeReleasedFlag : Data.BQL.BqlBool.Field<postponeReleasedFlag> { }
		[PXBool]
		public virtual bool? PostponeReleasedFlag
		{
			get;
			set;
		}
		#endregion
		#region PostponeVoidedFlag
		public abstract class postponeVoidedFlag : Data.BQL.BqlBool.Field<postponeVoidedFlag> { }
		[PXBool]
		public virtual bool? PostponeVoidedFlag
		{
			get;
			set;
		}
		#endregion
		#region Settled
		public abstract class settled : PX.Data.BQL.BqlBool.Field<settled> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Settled")]
		public virtual bool? Settled { get; set; }
		#endregion
		#region ICCPayment Members

		
		
		
		string ICCPayment.OrigDocType
		{
			get
			{
				return null;
			}			
		}

		string ICCPayment.OrigRefNbr
		{
			get
			{
				return null;
			}
			
		}

		decimal? ICCPayment.CuryDocBal
		{
			get
			{
				return CuryOrigDocAmt;
			}
			set { }
		}

		#endregion

		#region OrigReleased // original value of ARPayment.Released before Release
		public abstract class origReleased : Data.BQL.BqlBool.Field<origReleased> { }
		[PXBool]
		public virtual bool? OrigReleased { get; set; }
		#endregion
	}
}
namespace PX.Objects.AR.Standalone
{
	[Serializable()]
	[PXHidden(ServiceVisible = false)]
	[PXCacheName(Messages.ARPayment)]
	public partial class ARPayment : PX.Data.IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected string _DocType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		public virtual String DocType
		{
			get
			{
				return this._DocType;
			}
			set
			{
				this._DocType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected string _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		protected Int32? _PMInstanceID;
		[PXDBInt()]
		public virtual Int32? PMInstanceID
		{
			get
			{
				return this._PMInstanceID;
			}
			set
			{
				this._PMInstanceID = value;
			}
		}
		#endregion
		#region ProcessingCenterID
		public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID>{ }

		[PXDBString(10, IsUnicode = true)]
		[PXDBDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CCProcessingCenter.processingCenterID>))]
		public virtual string ProcessingCenterID
		{
			get;
			set;
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		protected String _PaymentMethodID;
		[PXDBString(10, IsUnicode = true)]
		public virtual String PaymentMethodID
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
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		protected Int32? _CashAccountID;
		[PXDBInt()]
		public virtual Int32? CashAccountID
		{
			get
			{
				return this._CashAccountID;
			}
			set
			{
				this._CashAccountID = value;
			}
		}
		#endregion
		#region CuryOrigTaxDiscAmt
		public abstract class curyOrigTaxDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigTaxDiscAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal]
		public virtual decimal? CuryOrigTaxDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigTaxDiscAmt
		public abstract class origTaxDiscAmt : PX.Data.BQL.BqlDecimal.Field<origTaxDiscAmt> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal]
		public virtual decimal? OrigTaxDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		protected String _ExtRefNbr;
		[PXDBString(40, IsUnicode = true)]
		public virtual String ExtRefNbr
		{
			get
			{
				return this._ExtRefNbr;
			}
			set
			{
				this._ExtRefNbr = value;
			}
		}
		#endregion
		#region AdjDate
		public abstract class adjDate : PX.Data.BQL.BqlDateTime.Field<adjDate> { }
		protected DateTime? _AdjDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? AdjDate
		{
			get
			{
				return this._AdjDate;
			}
			set
			{
				this._AdjDate = value;
			}
		}
		#endregion
		#region AdjFinPeriodID
		public abstract class adjFinPeriodID : PX.Data.BQL.BqlString.Field<adjFinPeriodID> { }
		protected String _AdjFinPeriodID;
		[PXDBString()]
		public virtual String AdjFinPeriodID
		{
			get
			{
				return this._AdjFinPeriodID;
			}
			set
			{
				this._AdjFinPeriodID = value;
			}
		}
		#endregion
		#region AdjTranPeriodID
		public abstract class adjTranPeriodID : PX.Data.BQL.BqlString.Field<adjTranPeriodID> { }
		protected String _AdjTranPeriodID;
		[PXDBString()]
		public virtual String AdjTranPeriodID
		{
			get
			{
				return this._AdjTranPeriodID;
			}
			set
			{
				this._AdjTranPeriodID = value;
			}
		}
		#endregion
		#region Cleared
		public abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared> { }
		protected Boolean? _Cleared;
		[PXDBBool()]
		public virtual Boolean? Cleared
		{
			get
			{
				return this._Cleared;
			}
			set
			{
				this._Cleared = value;
			}
		}
		#endregion
		#region ClearDate
		public abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate> { }
		protected DateTime? _ClearDate;
		[PXDBDate()]
		public virtual DateTime? ClearDate
		{
			get
			{
				return this._ClearDate;
			}
			set
			{
				this._ClearDate = value;
			}
		}
		#endregion
		#region Settled
		public abstract class settled : PX.Data.BQL.BqlBool.Field<settled> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Settled")]
		public virtual bool? Settled
		{
			get;
			set;
		}
		#endregion
		#region CATranID
		public abstract class cATranID : PX.Data.BQL.BqlLong.Field<cATranID> { }
		protected Int64? _CATranID;
		[PXDBLong()]
		public virtual Int64? CATranID
		{
			get
			{
				return this._CATranID;
			}
			set
			{
				this._CATranID = value;
			}
		}
		#endregion
		#region ARDepositAsBatch
		public abstract class depositAsBatch : PX.Data.BQL.BqlBool.Field<depositAsBatch> { }
		protected Boolean? _DepositAsBatch;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? DepositAsBatch
		{
			get
			{
				return this._DepositAsBatch;
			}
			set
			{
				this._DepositAsBatch = value;
			}
		}
		#endregion
		#region DepositAfter
		public abstract class depositAfter : PX.Data.BQL.BqlDateTime.Field<depositAfter> { }
		protected DateTime? _DepositAfter;
		[PXDBDate()]
		[PXDefault(PersistingCheck =PXPersistingCheck.Nothing)]		
		public virtual DateTime? DepositAfter
		{
			get
			{
				return this._DepositAfter;
			}
			set
			{
				this._DepositAfter = value;
			}
		}
		#endregion
		#region DepositDate
		public abstract class depositDate : PX.Data.BQL.BqlDateTime.Field<depositDate> { }
		protected DateTime? _DepositDate;
		[PXDBDate()]				
		public virtual DateTime? DepositDate
		{
			get
			{
				return this._DepositDate;
			}
			set
			{
				this._DepositDate = value;
			}
		}
		#endregion
		#region Deposited
		public abstract class deposited : PX.Data.BQL.BqlBool.Field<deposited> { }
		protected Boolean? _Deposited;
		[PXDBBool()]
		[PXDefault(false)]		
		public virtual Boolean? Deposited
		{
			get
			{
				return this._Deposited;
			}
			set
			{
				this._Deposited = value;
			}
		}
		#endregion
		#region DepositType
		public abstract class depositType : PX.Data.BQL.BqlString.Field<depositType> { }
		protected String _DepositType;
		[PXDBString(3, IsFixed=true)]		
		public virtual String DepositType
		{
			get
			{
				return this._DepositType;
			}
			set
			{
				this._DepositType = value;
			}
		}
		#endregion
		#region DepositNbr
		public abstract class depositNbr : PX.Data.BQL.BqlString.Field<depositNbr> { }
		protected String _DepositNbr;
		[PXDBString(15, IsUnicode = true)]
		public virtual String DepositNbr
		{
			get
			{
				return this._DepositNbr;
			}
			set
			{
				this._DepositNbr = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[ProjectDefault(BatchModule.AR)]
		[PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.visibleInAR, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
		[ProjectBaseAttribute()]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[ActiveProjectTask(typeof(ARPayment.projectID), BatchModule.AR, DisplayName = "Project Task")]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region ChargeCntr
		public abstract class chargeCntr : PX.Data.BQL.BqlInt.Field<chargeCntr> { }
		protected Int32? _ChargeCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? ChargeCntr
		{
			get
			{
				return this._ChargeCntr;
			}
			set
			{
				this._ChargeCntr = value;
			}
		}
		#endregion
		#region CuryConsolidateChargeTotal
		public abstract class curyConsolidateChargeTotal : PX.Data.BQL.BqlDecimal.Field<curyConsolidateChargeTotal> { }
		protected Decimal? _CuryConsolidateChargeTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryConsolidateChargeTotal
		{
			get
			{
				return this._CuryConsolidateChargeTotal;
			}
			set
			{
				this._CuryConsolidateChargeTotal = value;
			}
		}
		#endregion
		#region ConsolidateChargeTotal
		public abstract class consolidateChargeTotal : PX.Data.BQL.BqlDecimal.Field<consolidateChargeTotal> { }
		protected Decimal? _ConsolidateChargeTotal;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]        
		public virtual Decimal? ConsolidateChargeTotal
		{
			get
			{
				return this._ConsolidateChargeTotal;
			}
			set
			{
				this._ConsolidateChargeTotal = value;
			}
		}
		#endregion
		#region RefTranExtNbr
		public abstract class refTranExtNbr : PX.Data.BQL.BqlString.Field<refTranExtNbr> { }
		protected String _RefTranExtNbr;
		[PXDBString(50, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String RefTranExtNbr
		{
			get
			{
				return this._RefTranExtNbr;
			}
			set
			{
				this._RefTranExtNbr = value;
			}
		}
		#endregion
		#region IsCCPayment
		public abstract class isCCPayment : PX.Data.BQL.BqlBool.Field<isCCPayment> { }
		[PXDBBool(BqlField = typeof(AR.ARPayment.isCCPayment))]
		[PXDefault(false)]
		public virtual bool? IsCCPayment
		{
			get;
			set;
		}
		#endregion
		#region IsCCAuthorized
		public abstract class isCCAuthorized : PX.Data.BQL.BqlBool.Field<isCCAuthorized> { }
		[PXDBBool(BqlField = typeof(AR.ARPayment.isCCAuthorized))]
		[PXDefault(false)]
		public virtual bool? IsCCAuthorized
		{
			get;
			set;
		}
		#endregion
		#region IsCCCaptured
		public abstract class isCCCaptured : PX.Data.BQL.BqlBool.Field<isCCCaptured> { }
		[PXDBBool(BqlField = typeof(AR.ARPayment.isCCCaptured))]
		[PXDefault(false)]
		public virtual bool? IsCCCaptured
		{
			get;
			set;
		}
		#endregion
		#region IsCCCaptureFailed
		public abstract class isCCCaptureFailed : PX.Data.BQL.BqlBool.Field<isCCCaptureFailed> { }
		[PXDBBool(BqlField = typeof(AR.ARPayment.isCCCaptureFailed))]
		[PXDefault(false)]
		public virtual bool? IsCCCaptureFailed
		{
			get;
			set;
		}
		#endregion
		#region IsCCRefunded
		public abstract class isCCRefunded : PX.Data.BQL.BqlBool.Field<isCCRefunded> { }
		[PXDBBool(BqlField = typeof(AR.ARPayment.isCCRefunded))]
		[PXDefault(false)]
		public virtual bool? IsCCRefunded
		{
			get;
			set;
		}
		#endregion
		#region IsCCUserAttention
		public abstract class isCCUserAttention : PX.Data.BQL.BqlBool.Field<isCCUserAttention> { }
		[PXDBBool(BqlField = typeof(AR.ARPayment.isCCUserAttention))]
		[PXDefault(false)]
		public virtual bool? IsCCUserAttention
		{
			get;
			set;
		}
		#endregion
		#region CCActualExternalTransactionID

		public abstract class cCActualExternalTransactionID : Data.BQL.BqlInt.Field<cCActualExternalTransactionID> { }

		[PXDBInt(BqlField = typeof(AR.ARPayment.cCActualExternalTransactionID))]
		public virtual int? CCActualExternalTransactionID
		{
			get;
			set;
		}
		#endregion
	}
}
