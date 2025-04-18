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

using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

using PX.Data;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.AR.Repositories;
using PX.Objects.AR.GraphExtensions;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.Extensions.PaymentTransaction;
using PX.Objects.AR.MigrationMode;
using PX.Objects.Common.Attributes;
using PX.Objects.SO;

namespace PX.Objects.AR
{
	[TableAndChartDashboardType]
    [Serializable]
	public class ARChargeInvoices : PXGraph<ARChargeInvoices>
	{
		#region Internal  types
        [Serializable]
		public partial class PayBillsFilter : PX.Data.IBqlTable
		{
			#region PayDate
			public abstract class payDate : PX.Data.BQL.BqlDateTime.Field<payDate> { }
			protected DateTime? _PayDate;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Payment Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? PayDate
			{
				get
				{
					return this._PayDate;
				}
				set
				{
					this._PayDate = value;
				}
			}
			#endregion
			#region PayFinPeriodID
			public abstract class payFinPeriodID : PX.Data.BQL.BqlString.Field<payFinPeriodID> { }
			protected string _PayFinPeriodID;
			[AROpenPeriod(typeof(PayBillsFilter.payDate))]
			[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.Visible)]
			public virtual String PayFinPeriodID
			{
				get
				{
					return this._PayFinPeriodID;
				}
				set
				{
					this._PayFinPeriodID = value;
				}
			}
			#endregion
			#region OverDueFor
			public abstract class overDueFor : PX.Data.BQL.BqlShort.Field<overDueFor> { }
			protected Int16? _OverDueFor;
			[PXDBShort()]
			[PXUIField(Visibility = PXUIVisibility.Visible)]
			[PXDefault((short)0)]
			public virtual Int16? OverDueFor
			{
				get
				{
					return this._OverDueFor;
				}
				set
				{
					this._OverDueFor = value;
				}
			}
			#endregion
			#region ShowOverDueFor
			public abstract class showOverDueFor : PX.Data.BQL.BqlBool.Field<showOverDueFor> { }
			protected Boolean? _ShowOverDueFor;
			[PXDBBool()]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Overdue For", Visibility = PXUIVisibility.Visible)]
			public virtual Boolean? ShowOverDueFor
			{
				get
				{
					return this._ShowOverDueFor;
				}
				set
				{
					this._ShowOverDueFor = value;
				}
			}
			#endregion
			#region DueInLessThan
			public abstract class dueInLessThan : PX.Data.BQL.BqlShort.Field<dueInLessThan> { }
			protected Int16? _DueInLessThan;
			[PXDBShort()]
			[PXUIField(Visibility = PXUIVisibility.Visible)]
			[PXDefault((short)7)]
			public virtual Int16? DueInLessThan
			{
				get
				{
					return this._DueInLessThan;
				}
				set
				{
					this._DueInLessThan = value;
				}
			}
			#endregion
			#region ShowDueInLessThan
			public abstract class showDueInLessThan : PX.Data.BQL.BqlBool.Field<showDueInLessThan> { }
			protected Boolean? _ShowDueInLessThan;
			[PXDBBool()]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Due in Less Than", Visibility = PXUIVisibility.Visible)]
			public virtual Boolean? ShowDueInLessThan
			{
				get
				{
					return this._ShowDueInLessThan;
				}
				set
				{
					this._ShowDueInLessThan = value;
				}
			}
			#endregion
			#region DiscountExparedWithinLast
			public abstract class discountExparedWithinLast : PX.Data.BQL.BqlShort.Field<discountExparedWithinLast> { }
			protected Int16? _DiscountExparedWithinLast;
			[PXDBShort()]
			[PXUIField(Visibility = PXUIVisibility.Visible)]
			[PXDefault((short)0)]
			public virtual Int16? DiscountExparedWithinLast
			{
				get
				{
					return this._DiscountExparedWithinLast;
				}
				set
				{
					this._DiscountExparedWithinLast = value;
				}
			}
			#endregion
			#region ShowDiscountExparedWithinLast
			public abstract class showDiscountExparedWithinLast : PX.Data.BQL.BqlBool.Field<showDiscountExparedWithinLast> { }
			protected Boolean? _ShowDiscountExparedWithinLast;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Cash Discount Expired Within Past", Visibility = PXUIVisibility.Visible)]
			public virtual Boolean? ShowDiscountExparedWithinLast
			{
				get
				{
					return this._ShowDiscountExparedWithinLast;
				}
				set
				{
					this._ShowDiscountExparedWithinLast = value;
				}
			}
			#endregion
			#region DiscountExpiredInLessThan
			public abstract class discountExpiresInLessThan : PX.Data.BQL.BqlShort.Field<discountExpiresInLessThan> { }
			protected Int16? _DiscountExpiresInLessThan;
			[PXDBShort()]
			[PXUIField(Visibility = PXUIVisibility.Visible)]
			[PXDefault((short)7)]
			public virtual Int16? DiscountExpiresInLessThan
			{
				get
				{
					return this._DiscountExpiresInLessThan;
				}
				set
				{
					this._DiscountExpiresInLessThan = value;
				}
			}
			#endregion
			#region ShowDiscountExpiresInLessThan
			public abstract class showDiscountExpiresInLessThan : PX.Data.BQL.BqlBool.Field<showDiscountExpiresInLessThan> { }
			protected Boolean? _ShowDiscountExpiresInLessThan;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Cash Discount Expires in Less Than", Visibility = PXUIVisibility.Visible)]
			public virtual Boolean? ShowDiscountExpiresInLessThan
			{
				get
				{
					return this._ShowDiscountExpiresInLessThan;
				}
				set
				{
					this._ShowDiscountExpiresInLessThan = value;
				}
			}
			#endregion
    		
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			protected String _CuryID;
			[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
			[PXDefault(typeof(Current<AccessInfo.baseCuryID>))]
			[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, Visible = true)]
			[PXSelector(typeof(Currency.curyID))]
			public virtual String CuryID
			{
				get
				{
					return this._CuryID;
				}
				set
				{
					this._CuryID = value;
				}
			}
			#endregion
			#region CuryInfoID
			public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
			protected Int64? _CuryInfoID;
			[PXDBLong()]
			[CurrencyInfo(ModuleCode = BatchModule.AR)]
			public virtual Int64? CuryInfoID
			{
				get
				{
					return this._CuryInfoID;
				}
				set
				{
					this._CuryInfoID = value;
				}
			}
			#endregion
		}

		#endregion

		#region Type Override events

		#region BranchID

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Branch", Visible = false)]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.branch>.Or<FeatureInstalled<FeaturesSet.multiCompany>>))]
		protected virtual void _(Events.CacheAttached<ARInvoice.branchID> e) { }
		#endregion

		#endregion

		#region Standard Buttons + Ctor
		public ARChargeInvoices()
		{
			ARSetup setup = ARSetup.Current;
			ARDocumentList.SetSelected<ARInvoice.selected>();
			ARDocumentList.SetProcessCaption(Messages.Process);
			ARDocumentList.SetProcessAllCaption(Messages.ProcessAll);
			ARDocumentList.Cache.AllowInsert = false;
			PXUIFieldAttribute.SetEnabled<ARInvoice.docType>(ARDocumentList.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<ARInvoice.refNbr>(ARDocumentList.Cache, null, true);
			PeriodValidation validationValue = this.IsContractBasedAPI ||
				this.IsImport || this.IsExport || this.UnattendedMode ? PeriodValidation.DefaultUpdate : PeriodValidation.DefaultSelectUpdate;
			OpenPeriodAttribute.SetValidatePeriod<PayBillsFilter.payFinPeriodID>(Filter.Cache, null, validationValue);
		}
		public PXFilter<PayBillsFilter> Filter;
		public PXCancel<PayBillsFilter> Cancel;


		#region Custom Buttons
		public PXAction<PayBillsFilter> viewDocument;
		[PXUIField(DisplayName = Messages.ViewDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			ARInvoice doc = this.ARDocumentList.Current;
			if (doc != null)
			{

				ARInvoiceEntry pe = PXGraph.CreateInstance<ARInvoiceEntry>();
				pe.Document.Current = pe.Document.Search<ARInvoice.refNbr>(doc.RefNbr, doc.DocType);
				throw new PXRedirectRequiredException(pe, true, "Invoice") { Mode = PXRedirectRequiredException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		#endregion
		
		#endregion

		#region Selects + Overrides
		[PXFilterable]
		public PXFilteredProcessingJoin<ARInvoice, PayBillsFilter,
            InnerJoin<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>,
            InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<ARInvoice.pMInstanceID>>,
            LeftJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CustomerPaymentMethod.cashAccountID>>>>>> ARDocumentList;

		public PXSelectJoin<ARInvoice,
								InnerJoin<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>,
								InnerJoinSingleTable<CustomerPaymentMethod, On<CustomerPaymentMethod.bAccountID, Equal<ARInvoice.customerID>,
									And<CustomerPaymentMethod.pMInstanceID, Equal<ARInvoice.pMInstanceID>,
									And<CustomerPaymentMethod.isActive, Equal<True>>>>,
								LeftJoin<PaymentMethodAccount, On<ARInvoice.cashAccountID, IsNull,
									And<
										Where2<Where<PaymentMethodAccount.cashAccountID, Equal<CustomerPaymentMethod.cashAccountID>,
											And<PaymentMethodAccount.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
											And<PaymentMethodAccount.useForAR, Equal<True>,
											And<PaymentMethodAccount.aRIsDefault, Equal<True>,
											And<ARInvoice.docType, NotEqual<ARDocType.creditMemo>>>>>>,
										Or<Where<CustomerPaymentMethod.cashAccountID, IsNull,
											And<PaymentMethodAccount.paymentMethodID, Equal<ARInvoice.paymentMethodID>,
											And<Where2<Where<ARInvoice.docType, NotEqual<ARDocType.creditMemo>,
												And<PaymentMethodAccount.aRIsDefault, Equal<True>>>,
												Or<Where<ARInvoice.docType, Equal<ARDocType.creditMemo>,
												And<PaymentMethodAccount.aRIsDefaultForRefund, Equal<True>>>>>>>>>>>>,
								InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
									And<PaymentMethod.paymentType, In3<PaymentMethodType.creditCard, PaymentMethodType.eft>,
									And<PaymentMethod.aRIsProcessingRequired, Equal<True>>>>,
								LeftJoin<CashAccount, On<CashAccount.cashAccountID, Equal<ARInvoice.cashAccountID>,
									Or<Where<ARInvoice.cashAccountID, IsNull,
										And<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>>>,
								LeftJoin<ARAdjust, On<ARAdjust.adjdDocType, Equal<ARInvoice.docType>,
									And<ARAdjust.adjdRefNbr, Equal<ARInvoice.refNbr>,
									And<ARAdjust.released, Equal<False>,
									And<ARAdjust.voided, Equal<False>>>>>,
								LeftJoin<ARPayment, On<ARPayment.docType, Equal<ARAdjust.adjgDocType>,
									And<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>>>>>>>>>>,
								Where<ARInvoice.released, Equal<True>,
									And<ARInvoice.openDoc, Equal<True>,
									And<ARInvoice.pendingPPD, NotEqual<True>,
									And2<Where2<Where2<Where<Current<PayBillsFilter.showOverDueFor>, Equal<True>,
												And<ARInvoice.dueDate, LessEqual<Required<ARInvoice.dueDate>>
												>>,
										Or2<Where<Current<PayBillsFilter.showDueInLessThan>, Equal<True>,
												And<ARInvoice.dueDate, GreaterEqual<Current<PayBillsFilter.payDate>>,
												And<ARInvoice.dueDate, LessEqual<Required<ARInvoice.dueDate>>
												>>>,
										Or2<Where<Current<PayBillsFilter.showDiscountExparedWithinLast>, Equal<True>,
												And<ARInvoice.discDate, GreaterEqual<Required<ARInvoice.discDate>>,
												And<ARInvoice.discDate, LessEqual<Current<PayBillsFilter.payDate>>
												>>>,
										Or<Where<Current<PayBillsFilter.showDiscountExpiresInLessThan>, Equal<True>,
												And<ARInvoice.discDate, GreaterEqual<Current<PayBillsFilter.payDate>>,
												And<ARInvoice.discDate, LessEqual<Required<ARInvoice.discDate>>
												>>>>>>>,
										Or<Where<Current<PayBillsFilter.showOverDueFor>, Equal<False>,
									   And<Current<PayBillsFilter.showDueInLessThan>, Equal<False>,
									   And<Current<PayBillsFilter.showDiscountExparedWithinLast>, Equal<False>,
									   And<Current<PayBillsFilter.showDiscountExpiresInLessThan>, Equal<False>>>>>>>,

									And<Where2<Where<ARAdjust.adjgRefNbr, IsNull, Or<ARPayment.voided, Equal<True>>>,
									And<Match<Customer, Current<AccessInfo.userName>>>>>>>>>,
									OrderBy<Asc<ARInvoice.customerID>>> ARDocumentListView;
		public ToggleCurrency<PayBillsFilter> CurrencyView;
		public PXSelect<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>> arPayment;

		public PXSelect<CurrencyInfo> currencyinfo;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;

		protected virtual IEnumerable ardocumentlist()
		{
			PXDelegateResult delegateResult = new PXDelegateResult() { IsResultSorted = true };

			PayBillsFilter filter = this.Filter.Current;
			if (filter == null || filter.PayDate == null) return delegateResult;

			DateTime OverDueForDate = ((DateTime)filter.PayDate).AddDays((short)-1 * (short)filter.OverDueFor);
			DateTime DueInLessThan = ((DateTime)filter.PayDate).AddDays((short)+1 * (short)filter.DueInLessThan);

			DateTime DiscountExparedWithinLast = ((DateTime)filter.PayDate).AddDays((short)-1 * (short)filter.DiscountExparedWithinLast);
			DateTime DiscountExpiresInLessThan = ((DateTime)filter.PayDate).AddDays((short)+1 * (short)filter.DiscountExpiresInLessThan);

			foreach (PXResult<ARInvoice, Customer, CustomerPaymentMethod, PaymentMethodAccount, PaymentMethod, CashAccount, ARAdjust, ARPayment> it
					in ARDocumentListView.SelectWithViewContextLazy(OverDueForDate,
												DueInLessThan,
												DiscountExparedWithinLast,
												DiscountExpiresInLessThan))
			{
				ARInvoice doc = it;
				CashAccount acct = it;
				if (acct == null || acct.AccountID == null)
				{
					acct = this.findDefaultCashAccount(doc);
				}
				if (acct == null) continue;
				if (String.IsNullOrEmpty(filter.CuryID) == false && (filter.CuryID != acct.CuryID)) continue;
				delegateResult.Add(new PXResult<ARInvoice, Customer, CustomerPaymentMethod, CashAccount>(it, it, it, acct));
			}
			PXView.StartRow = 0;

			ARDocumentList.Cache.IsDirty = false;

			return delegateResult;
		}
		#endregion

		#region Setups
		
		public ARSetupNoMigrationMode ARSetup;

		#endregion

		#region Processing functions
		public static void CreatePayments(List<ARInvoice> list, PayBillsFilter filter, CurrencyInfo info)
		{
			if (RunningFlagScope<ARChargeInvoices>.IsRunning)
				throw new PXSetPropertyException(Messages.AnotherChargeInvoiceRunning, PXErrorLevel.Warning);

			using (new RunningFlagScope<ARChargeInvoices>())
			{
				_createPayments(list, filter, info);
			}
		}

		private static void _createPayments(List<ARInvoice> list, PayBillsFilter filter, CurrencyInfo info)
		{
			bool failed = false;
			ARPaymentEntry pe = PXGraph.CreateInstance<ARPaymentEntry>();
			list.Sort((in1, in2) =>
				{
					if (in1.CustomerID != in2.CustomerID) return ((IComparable)in1.CustomerID).CompareTo(in2.CustomerID);
					return ((IComparable)in1.PMInstanceID).CompareTo(in2.PMInstanceID);
				}
			);
			for (int i = 0; i < list.Count; i++)
			{
				ARInvoice doc = list[i];
				ARPayment pmt = null;
				bool docFailed = false;
				try
				{
					string paymentType = doc.DocType == ARDocType.CreditMemo ? ARDocType.Refund : ARDocType.Payment;
					pe.CreatePayment(doc, CM.Extensions.CurrencyInfo.GetEX(info), filter.PayDate, filter.PayFinPeriodID, false, paymentType);			
					pmt = pe.Document.Current;
					if ( pmt!= null)
					{
						pmt.Hold = false;
						pe.Document.Update(pmt);
						
						FinPeriodIDAttribute.SetPeriodsByMaster<ARPayment.finPeriodID>(pe.Document.Cache, pmt, filter.PayFinPeriodID);
					}				
					pe.Save.Press();					
				}
				catch (Exception e)
				{
					PXFilteredProcessing<ARInvoice, PayBillsFilter>.SetError(i, e.Message);
					docFailed = failed = true;
				}

				if (!docFailed)
				{
					PXFilteredProcessing<ARInvoice, PayBillsFilter>.SetInfo(i, PXMessages.LocalizeFormatNoPrefixNLA(Messages.ARPaymentIsCreatedProcessingINProgress, pmt.RefNbr));
				}
			}	
			if (failed)
			{
				 throw new PXException(Messages.CreationOfARPaymentFailedForSomeInvoices);
			}
		} 
		#endregion

		#region Filter Events
 
		protected virtual void PayBillsFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PayBillsFilter filter = e.Row as PayBillsFilter;
			if (filter == null) return;
			PXUIFieldAttribute.SetVisible<PayBillsFilter.curyID>(sender, filter, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());

			CurrencyInfo info = CurrencyInfo_CuryInfoID.Select(filter.CuryInfoID);
			ARDocumentList.SetProcessDelegate(
				delegate(List<ARInvoice> list)
				{
					CreatePayments(list, filter, info);
				}
			);
		} 

		protected virtual void PayBillsFilter_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARDocumentList.Cache.Clear();		
		}
		
		protected virtual void PayBillsFilter_PayDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			foreach (CurrencyInfo info in PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<PayBillsFilter.curyInfoID>>>>.Select(this, null))
			{
				currencyinfo.Cache.SetDefaultExt<CurrencyInfo.curyEffDate>(info);
			}
			ARDocumentList.Cache.Clear();
		}

		#endregion

		#region Currency Info Events
		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			foreach (PayBillsFilter filter in Filter.Cache.Inserted)
			{
				e.NewValue = filter.PayDate;
			}
		}

		#endregion

		#region ARInvoice Events

		protected virtual void ARInvoice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            PXUIFieldAttribute.SetEnabled<ARInvoice.docType>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<ARInvoice.refNbr>(sender, e.Row, false);
		}

		protected CashAccount findDefaultCashAccount(ARInvoice aDoc)
		{
			CashAccount acct = null;			
			PXCache cache = this.arPayment.Cache;
			ARPayment payment = new ARPayment();
			payment.DocType = ARDocType.Payment;
			payment.CustomerID = aDoc.CustomerID;
			payment.CustomerLocationID = aDoc.CustomerLocationID;
			payment.BranchID = aDoc.BranchID;
			payment.PaymentMethodID = aDoc.PaymentMethodID;
			payment.PMInstanceID = aDoc.PMInstanceID;		
			{
				object newValue;
				cache.RaiseFieldDefaulting<ARPayment.cashAccountID>(payment, out newValue);
				Int32? acctID = newValue as Int32?;
				if (acctID.HasValue)
				{
					acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, acctID);
				}
			}
			return acct;
		}
		#endregion
	}

	[Serializable]
	public partial class ARPaymentInfo : ARPayment
	{
		#region PMInstanceDescr
		public abstract class pMInstanceDescr : PX.Data.BQL.BqlString.Field<pMInstanceDescr> { }
		protected String _PMInstanceDescr;
		[PXString(255)]
		[PXDefault("", PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Card/Account Nbr.", Enabled = false)]
		public virtual String PMInstanceDescr
		{
			get
			{
				return this._PMInstanceDescr;
			}
			set
			{
				this._PMInstanceDescr = value;
			}
		}
		#endregion
		#region CCTranDescr
		public abstract class cCTranDescr : PX.Data.BQL.BqlString.Field<cCTranDescr> { }
		protected String _CCTranDescr;
		[PXString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXDefault("", PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Error Descr.", Enabled = false)]
		public virtual String CCTranDescr
		{
			get
			{
				return this._CCTranDescr;
			}
			set
			{
				this._CCTranDescr = value;
			}
		}
		#endregion
		#region IsExpired
		public abstract class isCCExpired : PX.Data.BQL.BqlBool.Field<isCCExpired> { }
		protected Boolean? _IsCCExpired;
		[PXBool()]
		[PXDefault("", PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Expired", Enabled = false)]
		public virtual Boolean? IsCCExpired
		{
			get
			{
				return this._IsCCExpired;
			}
			set
			{
				this._IsCCExpired = value;
			}
		}
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion
	}

	[TableAndChartDashboardType]
    [Serializable]
	public class ARPaymentsAutoProcessing : PXGraph<ARPaymentsAutoProcessing>
	{
		#region Internal  types
        [Serializable]
		public partial class PaymentFilter : PX.Data.IBqlTable
		{
			#region PayDate
			public abstract class payDate : PX.Data.BQL.BqlDateTime.Field<payDate> { }
			protected DateTime? _PayDate;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Payment Date Before", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? PayDate
			{
				get
				{
					return this._PayDate;
				}
				set
				{
					this._PayDate = value;
				}
			}
			#endregion			
			#region StatementCycleId
			public abstract class statementCycleId : PX.Data.BQL.BqlString.Field<statementCycleId> { }
			protected String _StatementCycleId;
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Statement Cycle ID")]
			[PXSelector(typeof(ARStatementCycle.statementCycleId))]
		//	[PXDefault(typeof(Search<CustomerClass.statementCycleId, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>),PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual String StatementCycleId
			{
				get
				{
					return this._StatementCycleId;
				}
				set
				{
					this._StatementCycleId = value;
				}
			}
			#endregion			
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[Customer(Visibility = PXUIVisibility.SelectorVisible, Required = false, DescriptionField = typeof(Customer.acctName))]
			[PXDefault()]
			public virtual Int32? CustomerID
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
			#region Balance
			public abstract class balance : PX.Data.BQL.BqlDecimal.Field<balance> { }
			protected Decimal? _Balance;
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXDBDecimal(4)]
			[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual Decimal? Balance
			{
				get
				{
					return this._Balance;
				}
				set
				{
					this._Balance = value;
				}
			}
			#endregion
			#region CurySelTotal
			public abstract class curySelTotal : PX.Data.BQL.BqlDecimal.Field<curySelTotal> { }
			protected Decimal? _CurySelTotal;
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXDBCurrency(typeof(PaymentFilter.curyInfoID), typeof(PaymentFilter.selTotal))]
			[PXUIField(DisplayName = "Selection Total", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual Decimal? CurySelTotal
			{
				get
				{
					return this._CurySelTotal;
				}
				set
				{
					this._CurySelTotal = value;
				}
			}
			#endregion
			#region SelTotal
			public abstract class selTotal : PX.Data.BQL.BqlDecimal.Field<selTotal> { }
			protected Decimal? _SelTotal;
			[PXDBDecimal(4)]
			public virtual Decimal? SelTotal
			{
				get
				{
					return this._SelTotal;
				}
				set
				{
					this._SelTotal = value;
				}
			}
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			protected String _CuryID;
			[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
			[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, Visible = false)]
			[PXSelector(typeof(Currency.curyID))]
			public virtual String CuryID
			{
				get
				{
					return this._CuryID;
				}
				set
				{
					this._CuryID = value;
				}
			}
			#endregion
			#region CuryInfoID
			public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
			protected Int64? _CuryInfoID;
			[PXDBLong()]
			[CurrencyInfo(ModuleCode = BatchModule.AP)]
			public virtual Int64? CuryInfoID
			{
				get
				{
					return this._CuryInfoID;
				}
				set
				{
					this._CuryInfoID = value;
				}
			}
			#endregion

			#region PaymentMethodID
			public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
			protected String _PaymentMethodID;
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Search<PaymentMethod.paymentMethodID, 
						Where<PaymentMethod.isActive, Equal<True>,
						And<PaymentMethod.aRIsProcessingRequired,Equal<True>,
						And<PaymentMethod.paymentType, In3<PaymentMethodType.creditCard, PaymentMethodType.eft>>>>>),DescriptionField = typeof(PaymentMethod.descr))]
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
			#region ProcessingCenterID
			public abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }
			protected String _ProcessingCenterID;
			[PXDBString(10, IsUnicode = true)]
			[PXSelector(typeof(Search<CCProcessingCenter.processingCenterID>),DescriptionField=typeof(CCProcessingCenter.name))]
			[PXUIField(DisplayName = "Processing Center ID")]
			[DeprecatedProcessing(ChckVal = DeprecatedProcessingAttribute.CheckVal.ProcessingCenterId)]
			[DisabledProcCenter(CheckFieldValue = DisabledProcCenterAttribute.CheckFieldVal.ProcessingCenterId)]
			public virtual String ProcessingCenterID
			{
				get
				{
					return this._ProcessingCenterID;
				}
				set
				{
					this._ProcessingCenterID = value;
				}
			}
			#endregion
			
		}





		#endregion

		#region Type Override events

		#region BranchID

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Branch", Visible = false)]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.branch>.Or<FeatureInstalled<FeaturesSet.multiCompany>>))]
		protected virtual void _(Events.CacheAttached<ARPaymentInfo.branchID> e) { }
		#endregion

		#endregion

		#region Standard Buttons + Ctor
		CCProcTranRepository tranRepo;
		public ARPaymentsAutoProcessing()
		{
			ARSetup setup = ARSetup.Current;
			PXCurrencyAttribute.SetBaseCalc<PaymentFilter.curySelTotal>(Filter.Cache, null, false);
			ARDocumentList.SetSelected<ARPaymentInfo.selected>();
			ARDocumentList.SetProcessDelegate<ARPaymentCCProcessing>(delegate(ARPaymentCCProcessing aGraph,ARPaymentInfo doc)
				{
					ProcessPayment(aGraph, doc);
				}
			);
			ARDocumentList.Cache.AllowInsert = false;
			ARDocumentList.ParallelProcessingOptions =
				settings =>
				{
					settings.IsEnabled = true;
				};
			tranRepo = new CCProcTranRepository(this);
		}
		public PXFilter<PaymentFilter> Filter;
		public PXCancel<PaymentFilter> Cancel;

		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2019R1)]
		public PXAction<PaymentFilter> EditDetail;

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXEditDetailButton]
		public virtual IEnumerable editDetail(PXAdapter adapter)
		{
			ARPayment doc = this.ARDocumentList.Current;
			if (doc != null)
			{

				ARPaymentEntry pe = PXGraph.CreateInstance<ARPaymentEntry>();
				pe.Document.Current = pe.Document.Search<ARPayment.refNbr>(doc.RefNbr, doc.DocType);
				throw new PXRedirectRequiredException(pe, true, "Payment"){Mode = PXRedirectRequiredException.WindowMode.NewWindow};
			}
			return adapter.Get();
		}
		#endregion

		#region Selects + Overrides
		[PXFilterable]
		public PXFilteredProcessing<ARPaymentInfo, PaymentFilter> ARDocumentList;
		public ToggleCurrency<PaymentFilter> CurrencyView;

		public PXSelect<CurrencyInfo> currencyinfo;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;

		protected virtual IEnumerable ardocumentlist()
		{
			DateTime now = DateTime.Now.Date;
			var query = new PXSelectJoinGroupBy<ARPaymentInfo,
						InnerJoin<Customer, On<Customer.bAccountID, Equal<ARPaymentInfo.customerID>>,
						InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<ARPaymentInfo.paymentMethodID>,
							And<PaymentMethod.paymentType, In3<PaymentMethodType.creditCard, PaymentMethodType.eft>,
							And<PaymentMethod.aRIsProcessingRequired, Equal<True>>>>,
						LeftJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<ARPaymentInfo.pMInstanceID>>,
						LeftJoin<ExternalTransaction, On<ExternalTransaction.transactionID, Equal<ARPaymentInfo.cCActualExternalTransactionID>>,
						LeftJoin<ARAdjust, On<ARAdjust.adjgDocType, Equal<ARPaymentInfo.docType>,
								And<ARAdjust.adjgRefNbr, Equal<ARPaymentInfo.refNbr>>>, 
						LeftJoin<Standalone.ARRegister, On<Standalone.ARRegister.docType, Equal<ARAdjust.adjdDocType>,
								And<Standalone.ARRegister.refNbr, Equal<ARAdjust.adjdRefNbr>,
								And<Standalone.ARRegister.released, Equal<False>>>>,
						LeftJoin<SOAdjust, On<SOAdjust.adjgDocType, Equal<ARPaymentInfo.docType>,
								And<SOAdjust.adjgRefNbr, Equal<ARPaymentInfo.refNbr>>>>>>>>>>,
						Where<ARPaymentInfo.released, Equal<False>,
							And<ARPaymentInfo.hold, Equal<False>,
							And<ARPaymentInfo.voided, Equal<False>,
							And<ARPaymentInfo.isCCCaptured, Equal<False>,
							And<ARPaymentInfo.docDate, LessEqual<Current<PaymentFilter.payDate>>,
							And<ARPaymentInfo.isMigratedRecord, NotEqual<True>,
							And<Standalone.ARRegister.refNbr, IsNull,
							And<SOAdjust.adjdOrderNbr, IsNull, 
							And2<Where<ARPaymentInfo.docType, Equal<ARPaymentType.payment>, 
								Or<ARPaymentInfo.docType, Equal<ARPaymentType.prepayment>>>,
							And2<Where<Customer.statementCycleId, Equal<Current<PaymentFilter.statementCycleId>>,
								Or<Current<PaymentFilter.statementCycleId>, IsNull>>,
							And2<Where<Customer.bAccountID, Equal<Current<PaymentFilter.customerID>>,
								Or<Current<PaymentFilter.customerID>, IsNull>>,
							And2<Where<PaymentMethod.paymentMethodID, Equal<Current<PaymentFilter.paymentMethodID>>,
								Or<Current<PaymentFilter.paymentMethodID>, IsNull>>,
							And2<Where<ARPaymentInfo.processingCenterID, Equal<Current<PaymentFilter.processingCenterID>>,
								Or<CustomerPaymentMethod.cCProcessingCenterID, Equal<Current<PaymentFilter.processingCenterID>>,
								Or<Current<PaymentFilter.processingCenterID>, IsNull>>>,
							And<Match<Customer, Current<AccessInfo.userName>>>>>>>>>>>>>>>>,
						Aggregate<GroupBy<ARPaymentInfo.docType, GroupBy<ARPaymentInfo.refNbr>>>,
						OrderBy<Asc<ARPaymentInfo.refNbr>>>(this);

			foreach (PXResult<ARPaymentInfo, Customer, PaymentMethod, CustomerPaymentMethod, ExternalTransaction> it in query.Select())
			{
				ARPaymentInfo doc = (ARPaymentInfo)it;
				CustomerPaymentMethod cpm = (CustomerPaymentMethod)it;
				ExternalTransaction extTran = (ExternalTransaction)it;
				ARDocKey key = new ARDocKey(doc);
				if (cpm?.PMInstanceID != null)
				{
					doc.PMInstanceDescr = cpm.Descr;
					doc.IsCCExpired = (cpm.ExpirationDate < now);
				}
				doc.ProcessingCenterID = (cpm != null) ? cpm.CCProcessingCenterID : doc.ProcessingCenterID;
				ExternalTransactionState paymentState = new ExternalTransactionState();
				if (extTran != null && extTran.Active == true)
				{
					paymentState = ExternalTranHelper.GetTransactionState(this, extTran);
				}
				ExternalTransaction lastTran = (ExternalTransaction)paymentState.ExternalTransaction;
				doc.CCPaymentStateDescr = paymentState.Description;
				int? tranId = paymentState?.ExternalTransaction?.TransactionID;
				if (paymentState.HasErrors && tranId != null)
				{
					string errText = tranRepo.GetCCProcTranByTranID(tranId).OrderByDescending(i=>i.TranNbr).Select(i=>i.ErrorText).FirstOrDefault();
					doc.CCTranDescr = errText;
				}
				if (doc.IsCCExpired == true && string.IsNullOrEmpty(doc.CCTranDescr))
				{
					doc.CCTranDescr = Messages.CreditCardIsExpired;
				}

				if (doc.PMInstanceID != PaymentTranExtConstants.NewPaymentProfile
					|| (paymentState.IsCaptured && paymentState.IsOpenForReview) || paymentState.IsPreAuthorized)
				{
					yield return doc;
				}
			}
		}
		#endregion
		#region Setups
		public CMSetupSelect CMSetup;

		public PXSetup<ARSetup> ARSetup;

		#endregion

		#region Processing functions
		public static void ProcessPayment(ARPaymentCCProcessing aProcessingGraph, ARPaymentInfo aDoc)
		{
			ARPaymentEntry graph = CreateInstance<ARPaymentEntry>();
			graph.Document.Current = graph.Document.Search<ARPayment.refNbr>(aDoc.RefNbr, aDoc.DocType);
			if (graph.Document.Current.IsCCUserAttention == true)
			{
				throw new Exception(Messages.PendingReviewCCDocCannotBeProcessed);
			}
			var ext = graph.GetExtension<ARPaymentEntryPaymentTransaction>();
			var adapter = new PXAdapter(new PXView.Dummy(graph, graph.Document.View.BqlSelect,
				new List<object>() { graph.Document.Current }));
			ext.CaptureCCPayment(adapter);
		}

		#endregion

		#region Filter Events

		protected virtual void PaymentFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PaymentFilter filter = e.Row as PaymentFilter;
			if (filter != null)
			{
				CurrencyInfo info = CurrencyInfo_CuryInfoID.Select(filter.CuryInfoID);
				var test = ARDocumentList.Select().Count;
			}
		}

		protected virtual void PaymentFilter_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARDocumentList.Cache.Clear();			
		}

		protected virtual void PaymentFilter_PayDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			foreach (CurrencyInfo info in PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<PaymentFilter.curyInfoID>>>>.Select(this, null))
			{
				currencyinfo.Cache.SetDefaultExt<CurrencyInfo.curyEffDate>(info);
			}
			ARDocumentList.Cache.Clear();
		}

		protected virtual void PaymentFilter_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARDocumentList.Cache.Clear();
		}
		protected virtual void PaymentFilter_ProcessingCenterID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARDocumentList.Cache.Clear();
		}
		protected virtual void PaymentFilter_StatementCycleID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARDocumentList.Cache.Clear();
		}

		
		#endregion	

		#region ARPaymentInfo Events
		protected virtual void ARPaymentInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARPaymentInfo row = e.Row as ARPaymentInfo;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<ARPaymentInfo.docType>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<ARPaymentInfo.refNbr>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<ARPaymentInfo.selected>(sender, e.Row, row.IsCCUserAttention == false);

			PXSetPropertyException ex = null;
			if (row?.IsCCUserAttention == true)
			{
				ex =new PXSetPropertyException(Messages.PendingReviewCCDocCannotBeProcessed, PXErrorLevel.RowWarning);
			}

			sender.RaiseExceptionHandling<ARPaymentInfo.refNbr>(row, row.CCPaymentStateDescr, ex);
		}

		#endregion
	
	}

	public class ARDocKey : AP.Pair<string, string>
	{
		public ARDocKey(string aType, string aRefNbr) : base(aType, aRefNbr) { }
		public ARDocKey(ARRegister aDoc) : base(aDoc.DocType, aDoc.RefNbr) { }
	}

	[PXHidden]
	public class ARPaymentCCProcessing : PXGraph<ARPaymentCCProcessing> 
	{
		public PXSelect<ARPayment> Document;
		public PXSelectReadonly<ExternalTransaction, 
			Where<ExternalTransaction.refNbr, Equal<Current<ARPayment.refNbr>>, 
				And<Where<ExternalTransaction.docType, Equal<Current<ARPayment.docType>>,
				Or<ARDocType.voidPayment, Equal<Current<ARPayment.docType>>>>>>,
			OrderBy<Desc<ExternalTransaction.transactionID>>> ExtTran;
	}


}
