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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.DependencyInjection;
using PX.Data.WorkflowAPI;
using PX.LicensePolicy;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.CA;
using PX.Objects.CM.Extensions;
using PX.Objects.Common;
using PX.Objects.Common.Bql;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.Extensions.MultiCurrency.AR;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.Reclassification.UI;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;
using PX.Objects.TX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ARCashSale = PX.Objects.AR.Standalone.ARCashSale;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.AR
{
	public class ARCashSaleEntry : ARDataEntryGraph<ARCashSaleEntry, ARCashSale>, IGraphWithInitialization
	{

		#region Entity Event Handlers
		public PXWorkflowEventHandler<ARCashSale> OnUpdateStatus;
		#endregion


	    #region Extensions

	    public class ARCashSaleEntryDocumentExtension : PaidInvoiceGraphExtension<ARCashSaleEntry>
	    {
	        public override void Initialize()
	        {
	            base.Initialize();

	            Documents = new PXSelectExtension<PaidInvoice>(Base.Document);
	            Lines = new PXSelectExtension<DocumentLine>(Base.Transactions);
	        }

			public override void SuppressApproval()
			{
				Base.Approval.SuppressApproval = true;
			}

			protected override PaidInvoiceMapping GetDocumentMapping()
	        {
	            return new PaidInvoiceMapping(typeof(ARCashSale))
	            {
                    HeaderTranPeriodID = typeof(ARCashSale.adjTranPeriodID),
	                HeaderDocDate = typeof(ARCashSale.adjDate)
                };
	        }

	        protected override DocumentLineMapping GetDocumentLineMapping()
	        {
	            return new DocumentLineMapping(typeof(ARTran));
	        }
	    }

		public class MultiCurrency : ARMultiCurrencyGraph<ARCashSaleEntry, ARCashSale>
		{
			protected override string DocumentStatus => Base.Document.Current?.Status;

			protected override CurySourceMapping GetCurySourceMapping()
			{
				return new CurySourceMapping(typeof(CashAccount))
				{
					CuryID = typeof(CashAccount.curyID),
					CuryRateTypeID = typeof(CashAccount.curyRateTypeID)
				};
			}

			protected override CurySource CurrentSourceSelect()
			{
				CurySource CurySource = base.CurrentSourceSelect();
				if (CurySource == null) return null;

				if (Base.Document?.Current?.DocType == ARDocType.CashReturn)
				{
					CurySource.AllowOverrideRate = false;
					CurySource.AllowOverrideCury = false;
				}
				else CurySource.AllowOverrideRate = Base.customer?.Current?.AllowOverrideRate;

				return CurySource;
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(ARCashSale))
				{
					DocumentDate = typeof(ARCashSale.adjDate),
					BAccountID = typeof(ARCashSale.customerID)
				};
			}

			protected override PXSelectBase[] GetChildren()
			{
				return new PXSelectBase[]
				{
					Base.Document,
					Base.Transactions,
					Base.Tax_Rows,
					Base.Taxes,
					Base.salesPerTrans,
					Base.PaymentCharges,
					Base.dummy_CATran
				};
			}

			protected override void _(Events.FieldUpdated<Extensions.MultiCurrency.Document, Extensions.MultiCurrency.Document.documentDate> e)
			{
				//this behavior is copied from previous versions. I belive it should be allowed to change effective date when Cash Return is NOT created by reversing process.
				//TODO: there could be more accurate condition
				if (Base.Document?.Current?.DocType == ARDocType.CashReturn) return;
				else base._(e);
			}

			protected virtual void _(Events.FieldUpdated<ARCashSale, ARCashSale.cashAccountID> e)
			{
				if (Base._IsVoidCheckInProgress || !PXAccess.FeatureInstalled<FeaturesSet.multicurrency>()) return;
				else
				{
					SourceFieldUpdated<ARCashSale.curyInfoID, ARCashSale.curyID, ARCashSale.adjDate>(e.Cache, e.Row);
					SetDetailCuryInfoID(Base.Transactions, e.Row.CuryInfoID);
				}
			}
		}

		#endregion

		#region Selects
		public PXSelect<InventoryItem> dummy_nonstockitem_for_redirect_newitem;
		public PXSelect<AP.Vendor> dummy_vendor_taxAgency_for_avalara;

        [PXCopyPasteHiddenFields(typeof(ARCashSale.extRefNbr), typeof(ARCashSale.clearDate),typeof(ARCashSale.cleared))]
		[PXViewName(Messages.ARCashSale)]
		public PXSelectJoin<ARCashSale,
			LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<ARCashSale.customerID>>>,
			Where<ARCashSale.docType, Equal<Optional<ARCashSale.docType>>,
			And2<Where<ARCashSale.origModule, NotEqual<BatchModule.moduleSO>, Or<ARCashSale.released, Equal<boolTrue>>>,
			And<Where<Customer.bAccountID, IsNull,
			Or<Match<Customer, Current<AccessInfo.userName>>>>>>>> Document;
		public PXSelect<ARCashSale, Where<ARCashSale.docType, Equal<Current<ARCashSale.docType>>, And<ARCashSale.refNbr, Equal<Current<ARCashSale.refNbr>>>>> CurrentDocument;

		public PXSelectJoin<CCProcessingCenter, LeftJoin<CustomerPaymentMethod,
				On<CCProcessingCenter.processingCenterID, Equal<CustomerPaymentMethod.cCProcessingCenterID>>>,
				Where<CustomerPaymentMethod.pMInstanceID, Equal<Current<ARCashSale.pMInstanceID>>>> ProcessingCenter;

		[PXViewName(Messages.ARTran)]
		public PXSelect<ARTran, Where<ARTran.tranType, Equal<Current<ARCashSale.docType>>, And<ARTran.refNbr, Equal<Current<ARCashSale.refNbr>>>>, OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>> Transactions;
		public PXSelect<ARTax, Where<ARTax.tranType, Equal<Current<ARCashSale.docType>>, And<ARTax.refNbr, Equal<Current<ARCashSale.refNbr>>>>, OrderBy<Asc<ARTax.tranType, Asc<ARTax.refNbr, Asc<ARTax.taxID>>>>> Tax_Rows;
        [PXCopyPasteHiddenView]
        public PXSelectJoin<ARTaxTran, LeftJoin<Tax, On<Tax.taxID, Equal<ARTaxTran.taxID>>>, Where<ARTaxTran.module, Equal<BatchModule.moduleAR>, And<ARTaxTran.tranType, Equal<Current<ARCashSale.docType>>, And<ARTaxTran.refNbr, Equal<Current<ARCashSale.refNbr>>>>>> Taxes;
		[PXViewName(Messages.ARBillingAddress)]
		public PXSelect<ARAddress, Where<ARAddress.addressID, Equal<Current<ARCashSale.billAddressID>>>> Billing_Address;
		[PXViewName(Messages.ARBillingContact)]
		public PXSelect<ARContact, Where<ARContact.contactID, Equal<Current<ARCashSale.billContactID>>>> Billing_Contact;

		[PXViewName(Messages.ARShippingAddress)]
		public PXSelect<ARShippingAddress, Where<ARShippingAddress.addressID, Equal<Current<ARCashSale.shipAddressID>>>> Shipping_Address;
		[PXViewName(Messages.ARShippingContact)]
		public PXSelect<ARShippingContact, Where<ARShippingContact.contactID, Equal<Current<ARCashSale.shipContactID>>>> Shipping_Contact;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARCashSale.curyInfoID>>>> currencyinfo;
		[PXReadOnlyView]
		public PXSelect<CATran, Where<CATran.tranID, Equal<Current<ARCashSale.cATranID>>>> dummy_CATran;


		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;

		[PXViewName(Messages.Customer)]
		public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<ARCashSale.customerID>>>> customer;
		public PXSetup<CustomerClass, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>> customerclass;
		public PXSetup<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<ARCashSale.cashAccountID>>>> cashaccount;
		public PXSetup<OrganizationFinPeriod, Where<OrganizationFinPeriod.finPeriodID, Equal<Current<ARCashSale.adjFinPeriodID>>,
													And<EqualToOrganizationOfBranch<OrganizationFinPeriod.organizationID, Current<ARCashSale.branchID>>>>> finperiod;
		public PXSetup<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<ARCashSale.paymentMethodID>>>> paymentmethod;

		public PXSetup<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<ARCashSale.taxZoneID>>>> taxzone;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Current<ARCashSale.customerID>>, And<Location.locationID, Equal<Optional<ARCashSale.customerLocationID>>>>> location;

		public PXSetup<GLSetup> glsetup;
		public PXSetup<ARSetup> arsetup;

		public PXSelect<ARBalances> arbalances;

		public PXSelect<CustSalesPeople, Where<CustSalesPeople.bAccountID, Equal<Current<ARCashSale.customerID>>,
												And<CustSalesPeople.locationID, Equal<Current<ARCashSale.customerLocationID>>>>> salesPerSettings;
		public PXSelect<ARSalesPerTran, Where<ARSalesPerTran.docType, Equal<Current<ARCashSale.docType>>,
												And<ARSalesPerTran.refNbr, Equal<Current<ARCashSale.refNbr>>,
												And<ARSalesPerTran.adjdDocType, Equal<ARDocType.undefined>,
												And2<Where<Current<ARSetup.sPCommnCalcType>, Equal<SPCommnCalcTypes.byInvoice>, Or<Current<ARCashSale.released>, Equal<boolFalse>>>,
												Or<ARSalesPerTran.adjdDocType, Equal<Current<ARCashSale.docType>>,
												And<ARSalesPerTran.adjdRefNbr, Equal<Current<ARCashSale.refNbr>>,
												And<Current<ARSetup.sPCommnCalcType>, Equal<SPCommnCalcTypes.byPayment>>>>>>>>> salesPerTrans;

		public ARPaymentChargeSelect<ARCashSale, ARCashSale.paymentMethodID, ARCashSale.cashAccountID, ARCashSale.docDate, ARCashSale.tranPeriodID, ARCashSale.pMInstanceID,
			Where<ARPaymentChargeTran.docType, Equal<Current<ARCashSale.docType>>,
				And<ARPaymentChargeTran.refNbr, Equal<Current<ARCashSale.refNbr>>>>> PaymentCharges;

		public PXSelect<GLVoucher, Where<True, Equal<False>>> Voucher;
		public PXSelect<ARSetupApproval,
			Where<Current<ARCashSale.docType>, Equal<ARDocType.cashReturn>,
				And<ARSetupApproval.docType, Equal<ARDocType.cashReturn>>>> SetupApproval;

		public PXSelect<ExternalTransaction,
			Where<ExternalTransaction.refNbr, Equal<Current<ARCashSale.refNbr>>,
				And<ExternalTransaction.docType, Equal<Current<ARCashSale.docType>>,
			Or<Where<ExternalTransaction.refNbr, Equal<Current<ARCashSale.origRefNbr>>,
				And<ExternalTransaction.docType, Equal<Current<ARCashSale.origDocType>>>>>>>,
			OrderBy<Desc<ExternalTransaction.transactionID>>> ExternalTran;

		public PXSelectOrderBy<CCProcTran, OrderBy<Desc<CCProcTran.tranNbr>>> ccProcTran;
		public IEnumerable CcProcTran()
		{
			var externalTrans = ExternalTran.Select();
			var query = new PXSelect<CCProcTran,
				Where<CCProcTran.transactionID, Equal<Required<CCProcTran.transactionID>>>>(this);
			foreach (ExternalTransaction extTran in externalTrans)
			{
				foreach (CCProcTran procTran in query.Select(extTran.TransactionID))
				{
					yield return procTran;
				}
			}
		}

		[PXCopyPasteHiddenView()]
		[PXViewName(CR.Messages.CustomerPaymentMethodDetails)]
		public SelectFrom<CustomerPaymentMethod>
			.Where<CustomerPaymentMethod.bAccountID.IsEqual<ARCashSale.customerID.FromCurrent>
				.And<CustomerPaymentMethod.paymentMethodID.IsEqual<ARCashSale.paymentMethodID.FromCurrent>>>.View CustomerPaymentMethodDetails;

		#endregion
		[PXViewName(EP.Messages.Approval)]
		public EPApprovalAutomationWithoutHoldDefaulting<ARCashSale, ARCashSale.approved, ARCashSale.rejected, ARCashSale.hold, ARSetupApproval> Approval;

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		public ARCashSaleEntry()
			: base()
		{
			{
				ARSetup record = arsetup.Select();
			}

			{
				GLSetup record = glsetup.Select();
			}
			RowUpdated.AddHandler<ARCashSale>(ParentRowUpdated);
			OpenPeriodAttribute.SetValidatePeriod<ARCashSale.adjFinPeriodID>(Document.Cache, null, PeriodValidation.DefaultSelectUpdate);
			TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(Transactions.Cache, null, TaxCalc.ManualLineCalc);

			FieldDefaulting.AddHandler<InventoryItem.stkItem>((sender, e) =>
			{
				if (e.Row != null) e.NewValue = false;
			});

			var arAddressCache = Caches[typeof(ARAddress)];
			var arContactCache = Caches[typeof(ARContact)];
			var arShippingAddressCache = Caches[typeof(ARShippingAddress)];
			var arShippingContactCache = Caches[typeof(ARShippingContact)];
		}

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += _licenseLimits.GetCheckerDelegate<ARCashSale>(
					new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(ARTran), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<ARTran.tranType>(PXDbType.Char, 3, ((ARCashSaleEntry)graph).Document.Current?.DocType),
							new PXDataFieldValue<ARTran.refNbr>(((ARCashSaleEntry)graph).Document.Current?.RefNbr)
						};
					}));
			}
		}

		public override Dictionary<string, string> PrepareReportParams(string reportID, ARCashSale doc)
		{
			if (reportID == "AR641000")
			{
				var parameters = new Dictionary<string, string>();
				parameters["ARInvoice.DocType"] = doc.DocType;
				parameters["ARInvoice.RefNbr"] = doc.RefNbr;
				return parameters;
			}

			return base.PrepareReportParams(reportID, doc);
		}

		#region Cache Attached
        #region InventoryItem
        #region COGSSubID
        [PXDefault(typeof(Search<INPostClass.cOGSSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>))]
        [SubAccount(typeof(InventoryItem.cOGSAcctID), DisplayName = "Expense Sub.", DescriptionField = typeof(Sub.description))]
        public virtual void InventoryItem_COGSSubID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

		#region ARSalesPerTran
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(ARRegister.docType))]
		protected virtual void ARSalesPerTran_DocType_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(ARCashSale.refNbr))]
		[PXParent(typeof(Select<ARCashSale, Where<ARCashSale.docType, Equal<Current<ARSalesPerTran.docType>>,
						 And<ARCashSale.refNbr, Equal<Current<ARSalesPerTran.refNbr>>>>>))]
		protected virtual void ARSalesPerTran_RefNbr_CacheAttached(PXCache sender)
		{
		}

        [PXDBInt()]
        [PXDBDefault(typeof(ARCashSale.branchID))]
        protected virtual void ARSalesPerTran_BranchID_CacheAttached(PXCache sender)
        {
        }
		[SalesPerson(DirtyRead = true, Enabled = false, IsKey = true, DescriptionField = typeof(Contact.displayName))]
		protected virtual void ARSalesPerTran_SalespersonID_CacheAttached(PXCache sender)
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Coalesce<Search<CustSalesPeople.commisionPct, Where<CustSalesPeople.bAccountID, Equal<Current<ARCashSale.customerID>>,
				And<CustSalesPeople.locationID, Equal<Current<ARCashSale.customerLocationID>>,
				And<CustSalesPeople.salesPersonID, Equal<Current<ARSalesPerTran.salespersonID>>>>>>,
			Search<SalesPerson.commnPct, Where<SalesPerson.salesPersonID, Equal<Current<ARSalesPerTran.salespersonID>>>>>))]
		[PXUIField(DisplayName = "Commission %")]
		protected virtual void ARSalesPerTran_CommnPct_CacheAttached(PXCache sender)
		{
		}
		[PXDBCurrency(typeof(ARSalesPerTran.curyInfoID), typeof(ARSalesPerTran.commnblAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Commissionable Amount", Enabled = false)]
		[PXFormula(null, typeof(SumCalc<ARCashSale.curyCommnblAmt>))]
		protected virtual void ARSalesPerTran_CuryCommnblAmt_CacheAttached(PXCache sender)
		{
		}
		[PXDBCurrency(typeof(ARSalesPerTran.curyInfoID), typeof(ARSalesPerTran.commnAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Mult<ARSalesPerTran.curyCommnblAmt, Div<ARSalesPerTran.commnPct, decimal100>>), typeof(SumCalc<ARCashSale.curyCommnAmt>))]
		[PXUIField(DisplayName = "Commission Amt.", Enabled = false)]
		protected virtual void ARSalesPerTran_CuryCommnAmt_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region ARPaymentChargeTran
		#region LineNbr
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXLineNbr(typeof(ARCashSale.chargeCntr), DecrementOnDelete = false)]
        public virtual void ARPaymentChargeTran_LineNbr_CacheAttached(PXCache sender)
        {
        }
		#endregion
		#region CashAccountID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDBDefault(typeof(ARCashSale.cashAccountID))]
        public virtual void ARPaymentChargeTran_CashAccountID_CacheAttached(PXCache sender)
        {
        }
		#endregion
		#region EntryTypeID

		/// <summary>
		/// <see cref="ARPaymentChargeTran.EntryTypeID"/> cache attached event.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXSelector(typeof(Search2<CAEntryType.entryTypeId,
                            InnerJoin<CashAccountETDetail, On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>,
                            Where<CashAccountETDetail.cashAccountID, Equal<Current<ARCashSale.cashAccountID>>,
                            And<CAEntryType.drCr, Equal<CADrCr.cACredit>>>>))]
		public virtual void ARPaymentChargeTran_EntryTypeID_CacheAttached(PXCache sender)
        {
        }
		#endregion

		#region TranDate
		[PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDBDefault(typeof(ARCashSale.adjDate))]
        public virtual void ARPaymentChargeTran_TranDate_CacheAttached(PXCache sender)
        {
        }
		#endregion
		#region ProjectID
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt]
		[PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible, Enabled = false, FieldClass = ProjectAttribute.DimensionName)]
		[PXSelector(typeof(Search<PMProject.contractID, Where<PMProject.customerID, Equal<Current<ARPayment.customerID>>, Or<PMProject.contractID, Equal<Zero>>>>),
			SubstituteKey = typeof(PMProject.contractCD),
			ValidateValue = false)]
		[ProjectDefault]
		public virtual void _(Events.CacheAttached<ARPaymentChargeTran.projectID> e)
		{
		}
		#endregion

		#region FinPeriodID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDefault()]
		[FinPeriodID(
			branchSourceType: typeof(ARPaymentChargeTran.cashAccountID),
			branchSourceFormulaType: typeof(Selector<ARPaymentChargeTran.cashAccountID, CashAccount.branchID>),
			masterFinPeriodIDType: typeof(ARPaymentChargeTran.tranPeriodID),
			headerMasterFinPeriodIDType: typeof(ARCashSale.adjTranPeriodID))]
        public virtual void ARPaymentChargeTran_FinPeriodID_CacheAttached(PXCache sender)
        {
        }
		#endregion

		#region CuryTranAmt
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUnboundFormula(typeof(Switch<Case<Where<ARPaymentChargeTran.consolidate, Equal<True>>, ARPaymentChargeTran.curyTranAmt>, decimal0>), typeof(SumCalc<ARCashSale.curyConsolidateChargeTotal>))]
		public virtual void ARPaymentChargeTran_CuryTranAmt_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

		#region ARShippingAddress

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
		protected virtual void _(Events.CacheAttached<ARShippingAddress.latitude> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Visible), true)]
		protected virtual void _(Events.CacheAttached<ARShippingAddress.longitude> e) { }

		#endregion

		[PXDBInt()]
		[PXDefault(typeof(ARCashSale.projectID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void ARTran_ProjectID_CacheAttached(PXCache sender)
		{ }

        [FinPeriodID(
            branchSourceType: typeof(ARTran.branchID),
            masterFinPeriodIDType: typeof(ARTran.tranPeriodID),
            headerMasterFinPeriodIDType: typeof(ARCashSale.adjTranPeriodID))]
        protected virtual void ARTran_FinPeriodID_CacheAttached(PXCache sender)
        {
        }


		[PXDefault(typeof(Coalesce<Search<PMAccountTask.taskID, Where<PMAccountTask.projectID, Equal<Current<ARTran.projectID>>, And<PMAccountTask.accountID, Equal<Current<ARTran.accountID>>>>>,
					Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<ARTran.projectID>>, And<PMTask.isDefault, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[ActiveProjectTask(typeof(ARTran.projectID), BatchModule.CA, DisplayName = "Project Task")]
		protected virtual void ARTran_TaskID_CacheAttached(PXCache sender)
		{ }

		[PXDBDefault(typeof(ARRegister.branchID))]
		[Branch(Enabled = false)]
		protected virtual void ARTaxTran_BranchID_CacheAttached(PXCache sender)
		{
		}

	    [FinPeriodID(branchSourceType: typeof(ARTaxTran.branchID),
	        headerMasterFinPeriodIDType: typeof(ARCashSale.adjTranPeriodID))]
	    [PXDefault]
	    protected virtual void ARTaxTran_FinPeriodID_CacheAttached(PXCache sender)
	    {
	    }

        [PXDefault]
		[PXDBInt]
		[PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID, Where<EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeARCashSale>>>),
		DescriptionField = typeof(EPAssignmentMap.name))]
		[PXUIField(DisplayName = "Approval Map")]
		protected virtual void EPApproval_AssignmentMapID_CacheAttached(PXCache sender)
		{
		}
		[PXDBDate]
		[PXDefault(typeof(ARCashSale.docDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt]
		[PXDefault(typeof(ARCashSale.customerID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(ARCashSale.docDesc), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong]
		[CurrencyInfo(typeof(ARCashSale.curyInfoID))]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(ARCashSale.curyOrigDocAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(ARCashSale.origDocAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}
		protected virtual void EPApproval_SourceItemType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = new ARDocType.ListAttribute()
					.ValueLabelDic[Document.Current.DocType];

				e.Cancel = true;
			}
		}
		protected virtual void EPApproval_Details_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = EPApprovalHelper.BuildEPApprovalDetailsString(sender, Document.Current);
			}
		}
		#endregion

		#region Other Buttons
		[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public override IEnumerable Release(PXAdapter adapter)
		{
			PXCache cache = Document.Cache;
			List<ARRegister> list = new List<ARRegister>();
			foreach (ARCashSale ardoc in adapter.Get<ARCashSale>())
			{
				if (ardoc.Hold == false)
				{
					cache.MarkUpdated(ardoc);
					list.Add(ardoc);
				}
			}
			if (list.Count == 0)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}
			Save.Press();

			PXLongOperation.StartOperation(this, delegate() { ARDocumentRelease.ReleaseDoc(list, false); });
			return list;
		}

		/// <summary>
		/// Ask user for approval for creation of another reversal if reversing document already exists.
		/// </summary>
		/// <param name="origDoc">The original document.</param>
		/// <returns>
		/// True if user approves, false if not.
		/// </returns>
		protected virtual bool AskUserApprovalIfReversingDocumentAlreadyExists(ARCashSale origDoc)
		{
			ARRegister reversingDoc = PXSelect<ARRegister,
				Where<ARRegister.docType, Equal<ARDocType.cashReturn>,
					And<ARRegister.origDocType, Equal<Required<ARRegister.origDocType>>,
					And<ARRegister.origRefNbr, Equal<Required<ARRegister.origRefNbr>>>>>,
				OrderBy<Desc<ARRegister.createdDateTime>>>
				.SelectSingleBound(this, null, origDoc.DocType, origDoc.RefNbr);

			if (reversingDoc != null)
			{
				string localizedMsg = PXMessages.LocalizeFormatNoPrefix(
					Messages.ReversingDocumentExists,
					ARDocType.GetDisplayName(ARDocType.CashReturn),
					reversingDoc.RefNbr);
				return Document.View.Ask(localizedMsg, MessageButtons.YesNo) == WebDialogResult.Yes;
			}

			return true;
		}

		[PXUIField(DisplayName = "Reverse", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public override IEnumerable VoidCheck(PXAdapter adapter)
		{
			if (Document.Current != null && Document.Current.Released == true && Document.Current.Voided == false && Document.Current.DocType == ARDocType.CashSale)
			{
				ARCashSale doc = PXCache<ARCashSale>.CreateCopy(Document.Current);
				FinPeriodUtils.VerifyAndSetFirstOpenedFinPeriod<ARCashSale.finPeriodID, ARCashSale.branchID>(Document.Cache, doc, finperiod, typeof(OrganizationFinPeriod.aRClosed));

				if (!AskUserApprovalIfReversingDocumentAlreadyExists(doc))
				{
					return adapter.Get();
				}

                try
				{
					_IsVoidCheckInProgress = true;
					this.VoidCheckProc(doc);
				}
				finally
				{
					_IsVoidCheckInProgress = false;
				}

				Document.Cache.RaiseExceptionHandling<ARCashSale.finPeriodID>(Document.Current, Document.Current.FinPeriodID, null);

				List<ARCashSale> rs = new List<ARCashSale>();
				if (IsContractBasedAPI || IsImport)
				{
					return new[] { this.Document.Current };
				}
				else
				{
					// Redirect to itself for UI
				throw new PXRedirectRequiredException(this, Messages.Voided);
			}
			}
			else if (Document.Current != null && Document.Current.Released == false && Document.Current.Voided == false && Document.Current.DocType == ARDocType.CashSale)
			{
				if (ExternalTranHelper.HasTransactions(ExternalTran))
				{
					ARCashSale doc = Document.Current;
					doc.Voided = true;
					doc.OpenDoc = false;
					doc.PendingProcessing = false;
					doc = this.Document.Update(doc);
					this.Save.Press();
				}
			}
			return adapter.Get();
		}


		public PXAction<ARCashSale> ViewOriginalDocument;

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		protected virtual IEnumerable viewOriginalDocument(PXAdapter adapter)
		{
			RedirectionToOrigDoc.TryRedirect(Document.Current.OrigDocType, Document.Current.OrigRefNbr, Document.Current.OrigModule);
			return adapter.Get();
		}

		public PXAction<ARCashSale> reclassifyBatch;
		[PXUIField(DisplayName = AP.Messages.ReclassifyGLBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ReclassifyBatch(PXAdapter adapter)
		{
			var document = Document.Current;

			if (document != null)
			{
				ReclassifyTransactionsProcess.TryOpenForReclassificationOfDocument(Document.View, BatchModule.AR, document.BatchNbr, document.DocType,
					document.RefNbr);
			}

			return adapter.Get();
		}

		public PXAction<ARCashSale> customerDocuments;
		[PXUIField(DisplayName = "Customer Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable CustomerDocuments(PXAdapter adapter)
		{
			if (customer.Current != null)
			{
				ARDocumentEnq graph = PXGraph.CreateInstance<ARDocumentEnq>();
				graph.Filter.Current.CustomerID = customer.Current.BAccountID;
				graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Customer Details");
			}
			return adapter.Get();
		}

		public PXAction<ARCashSale> sendARInvoiceMemo;
		[PXUIField(DisplayName = "Send AR Invoice/Memo", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable SendARInvoiceMemo(PXAdapter adapter)
		{
			ARCashSale invoice = Document.Current;
			if (Document.Current != null)
			{
				using (new LocalizationFeatureScope(this))
				{
					ReportNotificationGenerator reportNotificationGenerator =
						ReportNotificationGeneratorFactory("AR641000");
					Dictionary<string, string> mailParams = new Dictionary<string, string>
					{
						["DocType"] = invoice.DocType,
						["RefNbr"] = invoice.RefNbr
					};
					reportNotificationGenerator.Parameters = mailParams;

					if (!reportNotificationGenerator.Send().Any())
					{
						throw new PXException(ErrorMessages.MailSendFailed);
					}

					Clear();
					Document.Current = Document.Search<ARInvoice.refNbr>(invoice.RefNbr, invoice.DocType);
				}
			}
			return adapter.Get();
		}


		public PXAction<ARCashSale> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (ARCashSale current in adapter.Get<ARCashSale>())
			{
				if (current != null)
				{
					FindAllImplementations<IAddressValidationHelper>().ValidateAddresses();
				}
				yield return current;
			}
		}

		public PXAction<ARCashSale> viewSchedule;
		[PXUIField(DisplayName = "View Deferrals", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ViewSchedule(PXAdapter adapter)
		{
			ARTran currentLine = Transactions.Current;

			if (currentLine != null &&
				Transactions.Cache.GetStatus(currentLine) == PXEntryStatus.Notchanged)
			{
				Save.Press();
				ARInvoiceEntry.ViewScheduleForLine(this, Document.Current, Transactions.Current);
			}

			return adapter.Get();
		}
		public PXAction<ARCashSale> emailInvoice;
		[PXButton, PXUIField(DisplayName = "Email", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable EmailInvoice(PXAdapter adapter) => SendARInvoiceMemo(adapter);

		public PXAction<ARCashSale> printInvoice;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Print", MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable PrintInvoice(PXAdapter adapter, string reportID = null) => Report(adapter, reportID ?? "AR641000");

		public PXAction<ARCashSale> sendEmail;
		[PXButton, PXUIField(DisplayName = "Send Email", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable SendEmail(PXAdapter adapter) => this.GetExtension<ARCashSaleEntry_ActivityDetailsExt>().NewMailActivity.Press(adapter);
		#endregion

		#region ARCashSale Events

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Original Document", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual void ARCashSale_OrigRefNbr_CacheAttached(PXCache sender)
		{
		}

		protected virtual void ARCashSale_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = ARDocType.CashSale;
			e.Cancel = true;
		}

		protected virtual void ARCashSale_Hold_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARCashSale doc = e.Row as ARCashSale;
			if (IsApprovalRequired(doc))
			{
				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Legacy - requested for approval process working]
				sender.SetValue<ARCashSale.hold>(doc, true);
			}
		}

        [PopupMessage]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void ARCashSale_CustomerID_CacheAttached(PXCache sender)
        {
        }


		protected virtual void ARCashSale_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			customer.RaiseFieldUpdated(sender, e.Row);

			{
				sender.SetDefaultExt<ARCashSale.customerLocationID>(e.Row);
				sender.SetDefaultExt<ARCashSale.printInvoice>(e.Row);

                sender.SetDefaultExt<ARPayment.paymentMethodID>(e.Row);
				if (((ARCashSale)e.Row).DocType != ARDocType.CreditMemo)
				{
					sender.SetDefaultExt<ARCashSale.termsID>(e.Row);
				}
				else
				{
					sender.SetValueExt<ARCashSale.termsID>(e.Row, null);
				}
			}

			ARAddressAttribute.DefaultRecord<ARCashSale.billAddressID>(sender, e.Row);
			ARContactAttribute.DefaultRecord<ARCashSale.billContactID>(sender, e.Row);
		}

		protected virtual void ARCashSale_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARCashSale row = e.Row as ARCashSale;
			if (row != null)
			{
				e.NewValue = GetDefaultTaxZone(row);
			}
		}

		public virtual string GetDefaultTaxZone(ARCashSale row)
		{
			string result = null;
			if (row != null)
			{
				Location customerLocation = location.SelectSingle(row.CustomerLocationID);
				if (customerLocation != null)
				{
					if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
					{
						result = customerLocation.CTaxZoneID;
					}
				}

				if (result == null)
				{
					ARShippingAddress address = Shipping_Address.Select();
					if (address != null)
					{
						result = TaxBuilderEngine.GetTaxZoneByAddress(this, address);
					}
				}

				if (result== null)
				{
					BAccount companyAccount = PXSelectJoin<BAccountR, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(this, row.BranchID);
					if (companyAccount != null)
					{
						Location companyLocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(this, companyAccount.BAccountID, companyAccount.DefLocationID);
						if (companyLocation != null)
							result = companyLocation.VTaxZoneID;
					}
				}
			}

			return result;
		}

		protected virtual void ARCashSale_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			foreach (ARTaxTran taxTran in Taxes.Select())
			{
				Taxes.Cache.MarkUpdated(taxTran);
			}
		}

		protected virtual void ARShippingAddress_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARShippingAddress row = e.Row as ARShippingAddress;
			ARShippingAddress oldRow = e.OldRow as ARShippingAddress;
			if (row != null)
			{
				if (!IsTaxZoneDerivedFromCustomer() && Document.Current.Released != true &&
					((!string.IsNullOrEmpty(row.PostalCode) && oldRow.PostalCode != row.PostalCode) ||
					(!string.IsNullOrEmpty(row.CountryID) && oldRow.CountryID != row.CountryID) ||
					(!string.IsNullOrEmpty(row.State) && oldRow.State != row.State)))
				{
					string taxZone = TaxBuilderEngine.GetTaxZoneByAddress(this, row);

					if (taxZone == null)
					{
						return;
					}

					if (Document.Current != null && Document.Current.TaxZoneID != taxZone)
					{
						ARCashSale old_row = PXCache<ARCashSale>.CreateCopy(Document.Current);
						Document.Cache.SetValueExt<ARCashSale.taxZoneID>(Document.Current, taxZone);
						Document.Cache.RaiseRowUpdated(Document.Current, old_row);
					}
				}
			}
		}

		private bool IsTaxZoneDerivedFromCustomer()
		{
			Location customerLocation = location.Select();
			if (customerLocation != null)
			{
				if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
				{
					return true;
				}
			}

			return false;
		}

		protected virtual void ARCashSale_CustomerLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			location.RaiseFieldUpdated(sender, e.Row);

				sender.SetDefaultExt<ARCashSale.aRAccountID>(e.Row);
				sender.SetDefaultExt<ARCashSale.aRSubID>(e.Row);
			sender.SetDefaultExt<ARCashSale.taxCalcMode>(e.Row);
			sender.SetDefaultExt<ARCashSale.salesPersonID>(e.Row);
			sender.SetDefaultExt<ARCashSale.workgroupID>(e.Row);
			sender.SetDefaultExt<ARCashSale.ownerID>(e.Row);
			sender.SetDefaultExt<ARCashSale.externalTaxExemptionNumber>(e.Row);
			sender.SetDefaultExt<ARCashSale.avalaraCustomerUsageType>(e.Row);
			if (PM.ProjectAttribute.IsPMVisible( BatchModule.AR))
			{
				sender.SetDefaultExt<ARCashSale.projectID>(e.Row);
			}

			ARShippingAddressAttribute.DefaultRecord<ARCashSale.shipAddressID>(sender, e.Row);
			ARShippingContactAttribute.DefaultRecord<ARCashSale.shipContactID>(sender, e.Row);
		}

		protected virtual void ARCashSale_ExtRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
            ARCashSale row = (ARCashSale)e.Row;
			if (e.Row != null && ((ARCashSale)e.Row).DocType == ARDocType.VoidPayment)
			{
				//avoid webdialog in PaymentRef attribute
				e.Cancel = true;
			}
			else
			{
				if (row!= null && string.IsNullOrEmpty((string)e.NewValue) == false && String.IsNullOrEmpty(row.PaymentMethodID))
				{
                    PaymentMethod pm = this.paymentmethod.Current;
                    ARCashSale dup = null;
                    if (pm != null && pm.IsAccountNumberRequired == true)
                    {
                        dup = PXSelectReadonly<ARCashSale, Where<ARCashSale.customerID, Equal<Current<ARCashSale.customerID>>, And<ARCashSale.pMInstanceID, Equal<Current<ARCashSale.pMInstanceID>>, And<ARCashSale.extRefNbr, Equal<Required<ARCashSale.extRefNbr>>, And<ARCashSale.voided, Equal<False>, And<Where<ARCashSale.docType, NotEqual<Current<ARCashSale.docType>>, Or<ARCashSale.refNbr, NotEqual<Current<ARCashSale.refNbr>>>>>>>>>>.Select(this, e.NewValue);
                    }
                    else
                    {
                        dup = PXSelectReadonly<ARCashSale, Where<ARCashSale.customerID, Equal<Current<ARCashSale.customerID>>, And<ARCashSale.paymentMethodID, Equal<Current<ARCashSale.paymentMethodID>>, And<ARCashSale.extRefNbr, Equal<Required<ARCashSale.extRefNbr>>, And<ARCashSale.voided, Equal<False>, And<Where<ARCashSale.docType, NotEqual<Current<ARCashSale.docType>>, Or<ARCashSale.refNbr, NotEqual<Current<ARCashSale.refNbr>>>>>>>>>>.Select(this, e.NewValue);
                    }
					if (dup != null)
					{
                        sender.RaiseExceptionHandling<ARCashSale.extRefNbr>(e.Row, e.NewValue, new PXSetPropertyException(Messages.DuplicateCustomerPayment, PXErrorLevel.Warning, dup.ExtRefNbr, dup.DocDate, dup.DocType, dup.RefNbr));
					}
				}
			}
		}



		private object GetAcctSub<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			object NewValue = cache.GetValueExt<Field>(data);
			if (NewValue is PXFieldState)
			{
				return ((PXFieldState)NewValue).Value;
			}
			else
			{
				return NewValue;
			}
		}

		protected virtual void ARCashSale_ARAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && e.Row != null)
			{
				e.NewValue = GetAcctSub<Location.aRAccountID>(location.Cache, location.Current);
			}
		}

		protected virtual void ARCashSale_ARSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (location.Current != null && e.Row != null)
			{
				e.NewValue = GetAcctSub<Location.aRSubID>(location.Cache, location.Current);
			}
		}

		protected virtual void ARCashSale_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<ARCashSale.pMInstanceID>(e.Row);
            sender.SetDefaultExt<ARCashSale.cashAccountID>(e.Row);
        }

		protected virtual void ARCashSale_PMInstanceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetValueExt<ARPayment.refTranExtNbr>(e.Row, null);
			sender.SetDefaultExt<ARCashSale.cashAccountID>(e.Row);
			sender.SetDefaultExt<ARCashSale.processingCenterID>(e.Row);
        }

		protected virtual void ARCashSale_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARCashSale payment = (ARCashSale)e.Row;
			if (cashaccount.Current == null || cashaccount.Current.CashAccountID != payment.CashAccountID)
			{
				cashaccount.Current = (CashAccount)PXSelectorAttribute.Select<ARCashSale.cashAccountID>(sender, e.Row);
			}

			sender.SetDefaultExt<ARCashSale.depositAsBatch>(e.Row);
			sender.SetDefaultExt<ARCashSale.depositAfter>(e.Row);

			payment.Cleared = false;
			payment.ClearDate = null;

			PaymentMethod pm = paymentmethod.Select();
			if (pm?.PaymentType != PaymentMethodType.CreditCard && cashaccount.Current?.Reconcile == false)
			{
				payment.Cleared = true;
				payment.ClearDate = payment.DocDate;
			}
		}

		protected virtual void ARCashSale_Cleared_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARCashSale payment = (ARCashSale)e.Row;

			if (payment.Cleared == true)
			{
				if (payment.ClearDate == null)
				{
					payment.ClearDate = payment.DocDate;
				}
			}
			else
			{
				payment.ClearDate = null;
			}
		}

		protected void _(Events.FieldUpdated<ARCashSale, ARCashSale.adjDate> e)
		{
			if (e.Row.Released == false && e.Row.DocType != ARDocType.CashReturn)
			{
				e.Cache.SetDefaultExt<ARCashSale.depositAfter>(e.Row);
			}
		}

		protected virtual void ARCashSale_DepositAfter_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARCashSale row = (ARCashSale)e.Row;
			if ((row.DocType == ARDocType.Payment || row.DocType == ARDocType.CashSale || row.DocType == ARDocType.Refund || row.DocType == ARDocType.CashReturn)
				&& row.DepositAsBatch == true)
			{
				e.NewValue = row.AdjDate;
				e.Cancel = true;
			}
		}

		protected virtual void ARCashSale_DepositAsBatch_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARCashSale row = (ARCashSale)e.Row;

			if ((row.DocType == ARDocType.Payment || row.DocType == ARDocType.CashSale || row.DocType == ARDocType.Refund) || row.DocType == ARDocType.CashReturn)
			{
				sender.SetDefaultExt<ARPayment.depositAfter>(e.Row);
			}
		}

		protected virtual void ARCashSale_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARCashSale doc = (ARCashSale)e.Row;

			if (doc.CashAccountID == null)
			{
				if (sender.RaiseExceptionHandling<ARCashSale.cashAccountID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(ARCashSale.cashAccountID)}]")))
				{
					throw new PXRowPersistingException(typeof(ARCashSale.cashAccountID).Name, null, ErrorMessages.FieldIsEmpty, nameof(ARCashSale.cashAccountID));
				}
			}

            if (String.IsNullOrEmpty(doc.PaymentMethodID))
            {
                if (sender.RaiseExceptionHandling<ARCashSale.paymentMethodID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(ARCashSale.paymentMethodID)}]")))
                {
                    throw new PXRowPersistingException(typeof(ARCashSale.paymentMethodID).Name, null, ErrorMessages.FieldIsEmpty, nameof(ARCashSale.paymentMethodID));
                }
            }

			ValidateTaxConfiguration(sender, doc);

            PaymentMethod currentPaymentMethod = this.paymentmethod.Current;

			PXDefaultAttribute.SetPersistingCheck<ARPayment.pMInstanceID>(sender, doc, (currentPaymentMethod != null
				&& currentPaymentMethod.IsAccountNumberRequired == true) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			Terms terms = (Terms)PXSelectorAttribute.Select<ARCashSale.termsID>(Document.Cache, doc);

			if (terms == null)
			{
				sender.SetValue<ARCashSale.termsID>(doc, null);
				return;
			}

			if (terms.InstallmentType == CS.TermsInstallmentType.Multiple)
			{
				sender.RaiseExceptionHandling<ARCashSale.termsID>(doc, doc.TermsID, new PXSetPropertyException(Messages.Cash_Sale_Cannot_Have_Multiply_Installments, $"[{nameof(ARCashSale.termsID)}]"));
			}

			PXDefaultAttribute.SetPersistingCheck<ARCashSale.extRefNbr>(sender, doc, ((doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn) || arsetup.Current.RequireExtRef == false) ?
				PXPersistingCheck.Nothing : PXPersistingCheck.Null);

			PaymentRefAttribute.SetUpdateCashManager<ARCashSale.extRefNbr>(sender, e.Row, ((ARCashSale)e.Row).DocType != ARDocType.VoidPayment);
		}

		private void ValidateTaxConfiguration(PXCache cache, ARCashSale cashSale)
		{
			bool reduceOnEarlyPayments = false;
			bool reduceTaxableAmount = false;
			foreach (PXResult<ARTax, Tax> result in PXSelectJoin<ARTax,
				InnerJoin<Tax, On<Tax.taxID, Equal<ARTax.taxID>>>,
				Where<ARTax.tranType, Equal<Current<ARCashSale.docType>>,
				And<ARTax.refNbr, Equal<Current<ARCashSale.refNbr>>>>>.Select(this))
			{
				Tax tax = (Tax)result;
				if (tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToPromtPayment)
				{
					reduceOnEarlyPayments = true;
				}
				if (tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxableAmount)
				{
					reduceTaxableAmount = true;
				}
				if (reduceOnEarlyPayments && reduceTaxableAmount)
				{
					cache.RaiseExceptionHandling<ARCashSale.taxZoneID>(cashSale, cashSale.TaxZoneID, new PXSetPropertyException(TX.Messages.InvalidTaxConfiguration));
				}
			}
		}

		protected bool InternalCall = false;
		/// <summary>
		/// Determines whether the approval is required for the document.
		/// </summary>
		/// <param name="doc">The document for which the check should be performed.</param>
		/// <param name="cache">The cache.</param>
		/// <returns>Returns <c>true</c> if approval is required; otherwise, returns <c>false</c>.</returns>
	    private bool IsApprovalRequired(ARCashSale doc)
		{
			return EPApprovalSettings<ARSetupApproval>.ApprovedDocTypes.Contains(doc.DocType);
		}

		protected virtual void ARCashSale_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ARCashSale doc = e.Row as ARCashSale;
			release.SetEnabled(true);
			if (doc == null || InternalCall)
			{
				return;
			}
			this.release.SetEnabled(true);
			this.reclassifyBatch.SetEnabled(true);

			bool dontApprove = !IsApprovalRequired(doc);
			if (doc.DontApprove != dontApprove)
			{
				cache.SetValueExt<ARCashSale.dontApprove>(doc, dontApprove);
			}

			// We need this for correct tabs repainting
			// in migration mode.
			//
			PaymentCharges.Cache.AllowSelect = true;

			bool isDepositAfterEditable = doc.DocType == ARDocType.Payment ||
										  doc.DocType == ARDocType.CashSale ||
										  doc.DocType == ARDocType.CashReturn;

			PXUIFieldAttribute.SetVisible<ARCashSale.depositAfter>(cache, doc, isDepositAfterEditable && doc.DepositAsBatch == true);

			bool clearEnabled = doc.Hold == true && cashaccount.Current?.Reconcile == true;


			bool isDocumentReleasedOrVoided = doc.Released == true || doc.Voided == true;
			Shipping_Address.Cache.AllowUpdate =
			Shipping_Contact.Cache.AllowUpdate = !isDocumentReleasedOrVoided;

			if (isDocumentReleasedOrVoided)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				cache.AllowDelete = false;
				Transactions.Cache.AllowDelete = false;
				Transactions.Cache.AllowUpdate = false;
				Transactions.Cache.AllowInsert = false;
				release.SetEnabled(false);
				voidCheck.SetEnabled(doc.Voided == false);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<ARCashSale.status>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<ARCashSale.curyID>(cache, doc, false);

				cache.AllowUpdate = true;
				Transactions.Cache.AllowDelete = true;
				Transactions.Cache.AllowUpdate = true;
				Transactions.Cache.AllowInsert = doc.CustomerID != null && doc.CustomerLocationID != null;
				release.SetEnabled(doc.Hold == false);
				voidCheck.SetEnabled(false);
			}

			PXUIFieldAttribute.SetEnabled<ARCashSale.docType>(cache, doc);
			PXUIFieldAttribute.SetEnabled<ARCashSale.refNbr>(cache, doc);
			PXUIFieldAttribute.SetEnabled<ARCashSale.batchNbr>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.curyLineTotal>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.curyTaxTotal>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.curyDocBal>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.curyCommnblAmt>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.curyCommnAmt>(cache, doc, false);
            PXUIFieldAttribute.SetEnabled<ARCashSale.curyVatExemptTotal>(cache, doc, false);
            PXUIFieldAttribute.SetEnabled<ARCashSale.curyVatTaxableTotal>(cache, doc, false);

			PXUIFieldAttribute.SetEnabled<ARCashSale.cleared>(cache, doc, clearEnabled);
			PXUIFieldAttribute.SetEnabled<ARCashSale.clearDate>(cache, doc, clearEnabled && doc.Cleared == true);

			PXUIFieldAttribute.SetEnabled<ARCashSale.depositAsBatch>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.deposited>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.depositType>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.depositNbr>(cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARCashSale.depositDate>(cache, null, false);

			PXUIFieldAttribute.SetEnabled<ARCashSale.depositAfter>(cache, doc, isDepositAfterEditable && doc.DepositAsBatch == true);
			PXUIFieldAttribute.SetRequired<ARCashSale.depositAfter>(cache, isDepositAfterEditable && doc.DepositAsBatch == true);

			PXPersistingCheck depositAfterPersistCheck = (isDepositAfterEditable && doc.DepositAsBatch == true) ? PXPersistingCheck.NullOrBlank
																											    : PXPersistingCheck.Nothing;
			PXDefaultAttribute.SetPersistingCheck<ARCashSale.depositAfter>(cache, doc, depositAfterPersistCheck);

			if (doc.CustomerID != null && Transactions.Any())
				{
					PXUIFieldAttribute.SetEnabled<ARCashSale.customerID>(cache, doc, false);
				}

			PXUIFieldAttribute.SetEnabled<ARCashSale.cCPaymentStateDescr>(cache, null, false);

			bool isDeposited = string.IsNullOrEmpty(doc.DepositNbr) == false && string.IsNullOrEmpty(doc.DepositType) == false;
			CashAccount cashAccount = this.cashaccount.Current;
			bool isClearingAccount = cashAccount != null && cashAccount.CashAccountID == doc.CashAccountID && cashAccount.ClearingAccount == true;
			bool enableDepositEdit = !isDeposited && cashAccount != null && (isClearingAccount || doc.DepositAsBatch != isClearingAccount);

			if (enableDepositEdit)
			{
				cache.AllowUpdate = true;
				PXSetPropertyException exception = doc.DepositAsBatch != isClearingAccount ? new PXSetPropertyException(Messages.DocsDepositAsBatchSettingDoesNotMatchClearingAccountFlag, PXErrorLevel.Warning)
																						   : null;
				cache.RaiseExceptionHandling<ARCashSale.depositAsBatch>(doc, doc.DepositAsBatch, exception);
			}

			PXUIFieldAttribute.SetEnabled<ARCashSale.depositAsBatch>(cache, doc, enableDepositEdit);
			PXUIFieldAttribute.SetEnabled<ARCashSale.depositAfter>(cache, doc, !isDeposited && isClearingAccount && doc.DepositAsBatch == true);

			CheckCashAccount(cache, doc);

			this.validateAddresses.SetEnabled(doc.Released == false && FindAllImplementations<IAddressValidationHelper>().RequiresValidation());

			bool allowPaymentChargesEdit = doc.Released != true && (doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn);
			this.PaymentCharges.Cache.AllowInsert = allowPaymentChargesEdit;
			this.PaymentCharges.Cache.AllowUpdate = allowPaymentChargesEdit;
			this.PaymentCharges.Cache.AllowDelete = allowPaymentChargesEdit;

			Taxes.Cache.AllowInsert = Transactions.Cache.AllowInsert;
			Taxes.Cache.AllowUpdate = Transactions.Cache.AllowUpdate;
			Taxes.Cache.AllowDelete = Transactions.Cache.AllowDelete;

			#region Migration Mode Settings

			bool isMigratedDocument = doc.IsMigratedRecord == true;
			bool isUnreleasedMigratedDocument = isMigratedDocument && doc.Released != true;

			if (isUnreleasedMigratedDocument)
			{
				PaymentCharges.Cache.AllowSelect = false;
			}

			bool disableCaches = arsetup.Current?.MigrationMode == true
				? !isMigratedDocument
				: isUnreleasedMigratedDocument;
			if (disableCaches)
			{
				bool primaryCacheAllowInsert = Document.Cache.AllowInsert;
				bool primaryCacheAllowDelete = Document.Cache.AllowDelete;
				this.DisableCaches();
				Document.Cache.AllowInsert = primaryCacheAllowInsert;
				Document.Cache.AllowDelete = primaryCacheAllowDelete;
			}

			#endregion
			if (IsApprovalRequired(doc))
			{
				if (doc.Status == ARDocStatus.PendingApproval || doc.Status == ARDocStatus.Rejected)
				{
					release.SetEnabled(false);
				}

				if (doc.DocType == ARDocType.CashReturn)
				{
					if ((doc.Status == ARDocStatus.PendingApproval || doc.Status == ARDocStatus.Rejected ||
						doc.Status == ARDocStatus.Closed || doc.Status == ARDocStatus.Balanced) && doc.DontApprove == false)
					{
						PXUIFieldAttribute.SetEnabled(cache, doc, false);
					}
					if (doc.Status == ARDocStatus.PendingApproval || doc.Status == ARDocStatus.Rejected ||
						doc.Status == ARDocStatus.Balanced)
					{
						PXUIFieldAttribute.SetEnabled<ARPayment.hold>(cache, doc, true);
					}
				}

				if (doc.Status == ARDocStatus.PendingApproval
					|| doc.Status == ARDocStatus.Rejected
					|| (doc.Status == ARDocStatus.Balanced && doc.DontApprove == false)
					|| doc.Status == ARDocStatus.Closed)
				{
					Transactions.Cache.AllowInsert = false;
					Taxes.Cache.AllowInsert = false;
					Approval.Cache.AllowInsert = false;
					salesPerTrans.Cache.AllowInsert = false;
					PaymentCharges.Cache.AllowInsert = false;

					Transactions.Cache.AllowUpdate = false;
					Taxes.Cache.AllowUpdate = false;
					Approval.Cache.AllowUpdate = false;
					salesPerTrans.Cache.AllowUpdate = false;
					PaymentCharges.Cache.AllowUpdate = false;

					Transactions.Cache.AllowDelete = false;
					Taxes.Cache.AllowDelete = false;
					Approval.Cache.AllowDelete = false;
					salesPerTrans.Cache.AllowDelete = false;
					PaymentCharges.Cache.AllowDelete = false;
				}
			}

		    PXUIFieldAttribute.SetEnabled<ARCashSale.docType>(cache, doc, true);
		    PXUIFieldAttribute.SetEnabled<ARCashSale.refNbr>(cache, doc, true);

			#region CC Settings
			ExternalTransactionState tranState = ExternalTranHelper.GetActiveTransactionState(this, ExternalTran);

			bool isCreditCardProcInfoTabVisible = doc.IsCCPayment == true &&
				(PXAccess.FeatureInstalled<FeaturesSet.integratedCardProcessing>() == true ||
				 PXAccess.FeatureInstalled<FeaturesSet.integratedCardProcessing>() == false
					&& tranState?.IsActive == true);
			this.ccProcTran.Cache.AllowSelect = isCreditCardProcInfoTabVisible;
			this.ccProcTran.AllowUpdate = false;
			this.ccProcTran.AllowDelete = false;
			this.ccProcTran.AllowInsert = false;

			bool CCActionsNoAvailable = doc.Status == ARDocStatus.CCHold && !PXAccess.FeatureInstalled<FeaturesSet.integratedCardProcessing>();
			UIState.RaiseOrHideErrorByErrorLevelPriority<ARPayment.status>(cache, e.Row, CCActionsNoAvailable,
				Messages.CardProcessingActionsNotAvailable, PXErrorLevel.Warning);

			bool isPMInstanceRequired = false;

			if (!string.IsNullOrEmpty(doc.PaymentMethodID))
			{
				isPMInstanceRequired = paymentmethod.Current?.IsAccountNumberRequired == true;
			}

			PXUIFieldAttribute.SetRequired<ARCashSale.pMInstanceID>(cache, isPMInstanceRequired);
			PXUIFieldAttribute.SetEnabled<ARCashSale.pMInstanceID>(cache, e.Row, isPMInstanceRequired && !isDocumentReleasedOrVoided);

			bool enableVoidCheck = doc.Released == true && doc.DocType == ARDocType.CashSale && doc.Voided == false;
			bool isCCStateClear = !(tranState.IsCaptured || tranState.IsPreAuthorized);

			if (doc.Released == false && !enableVoidCheck && doc.DocType == ARDocType.CashSale && doc.Voided == false)
			{
				bool isVoidableIfFeatureIsOff = !PXAccess.FeatureInstalled<FeaturesSet.integratedCardProcessing>() && doc.Status == ARDocStatus.CCHold;
				if (ExternalTranHelper.HasTransactions(this.ExternalTran) && (isCCStateClear || isVoidableIfFeatureIsOff))
				{
					enableVoidCheck = true;
				}
				cache.AllowDelete = !ExternalTranHelper.HasTransactions(this.ExternalTran);
			}

			this.voidCheck.SetEnabled(enableVoidCheck);
			#endregion
		}

		private void CheckCashAccount(PXCache cache, ARCashSale doc)
		{
			CCProcessingCenter procCenter = ProcessingCenter.SelectSingle();
			if (procCenter?.ImportSettlementBatches != true)
				return;

			PXSelectBase<CashAccountDeposit> cashAccountDepositSelect = new PXSelect<
				CashAccountDeposit, Where<CashAccountDeposit.cashAccountID, Equal<Required<CCProcessingCenter.depositAccountID>>,
						And<CashAccountDeposit.depositAcctID, Equal<Required<ARCashSale.cashAccountID>>,
							And<Where<CashAccountDeposit.paymentMethodID, Equal<Required<ARCashSale.paymentMethodID>>,
									Or<CashAccountDeposit.paymentMethodID, Equal<BQLConstants.EmptyString>>>>>>>(this);

			bool cashAccountIsClearingForDeposit = cashAccountDepositSelect.Select(procCenter.DepositAccountID, doc.CashAccountID, doc.PaymentMethodID).Any();
			if (!cashAccountIsClearingForDeposit)
			{
				CashAccount procCenterDepositAccount = CashAccount.PK.Find(this, procCenter.DepositAccountID);
				cache.RaiseExceptionHandling<ARCashSale.cashAccountID>(
					doc,
					doc.CashAccountID,
					new PXSetPropertyException(Messages.CashAccountIsNotClearingPaymentWontBeIncludedInDeposit,
					PXErrorLevel.Warning,
					procCenterDepositAccount.CashAccountCD));
			}
			else
			{
				cache.RaiseExceptionHandling<ARCashSale.cashAccountID>(doc, doc.CashAccountID, null);
			}
		}

        protected virtual void ARCashSale_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
			ARCashSale doc = e.Row as ARCashSale;
			if (doc != null)
			{
				using (new PXConnectionScope())
				{
					PXFormulaAttribute.CalcAggregate<ARPaymentChargeTran.curyTranAmt>(PaymentCharges.Cache, e.Row, true);
				}
			}
        }

		protected virtual void ARCashSale_DocDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && ((ARCashSale)e.Row).Released == false )
			{
				e.NewValue = ((ARCashSale)e.Row).AdjDate;
				e.Cancel = true;
			}
		}

		protected virtual void ARCashSale_DocDesc_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARCashSale cashSale = (ARCashSale)e.Row;
			if (cashSale?.Released != false) return;

			foreach (ARTaxTran aRTaxTran in Taxes.Select())
			{
				aRTaxTran.Description = cashSale.DocDesc;
				Taxes.Cache.Update(aRTaxTran);
			}
		}

		protected virtual void ARCashSale_FinPeriodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && ((ARCashSale)e.Row).Released == false)
			{
				e.NewValue = ((ARCashSale)e.Row).AdjFinPeriodID;
				e.Cancel = true;
			}
		}

		protected virtual void ARCashSale_TranPeriodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && ((ARCashSale)e.Row).Released == false)
			{
				e.NewValue = ((ARCashSale)e.Row).AdjTranPeriodID;
				e.Cancel = true;
			}
		}

		protected virtual void ARCashSale_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARCashSale row = e.Row as ARCashSale;
			if (row != null)
			{
				foreach (ARTran tran in Transactions.Select())
				{
					Transactions.Cache.SetDefaultExt<ARTran.projectID>(tran);
					Transactions.Update(tran);
				}
			}
		}

		protected virtual void ARCashSale_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(this, ExternalTran);
			PaymentMethod pm = this.paymentmethod.Current;
			ARCashSale doc = e.Row as ARCashSale;
			bool cashReturnWithoutRefund = doc.DocType == ARDocType.CashReturn && !state.IsRefunded && !state.IsVoided;
			if (pm?.PaymentType == CA.PaymentMethodType.CreditCard && pm?.ARIsProcessingRequired == true && state?.IsActive == true && !cashReturnWithoutRefund)
			{
				throw new PXException(AR.Messages.CannotDeletedBecauseOfTransactions);
			}
		}

		protected virtual void ARCashSale_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			ARReleaseProcess.UpdateARBalances(this, (ARRegister)e.Row, -(((ARRegister)e.Row).OrigDocAmt));
		}

		protected virtual void ARCashSale_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			ARCashSale payment = (ARCashSale)e.Row;
			if (payment.Released == false)
			{
					payment.DocDate = payment.AdjDate;

				payment.FinPeriodID = payment.AdjFinPeriodID;
				payment.TranPeriodID = payment.AdjTranPeriodID;

				sender.RaiseExceptionHandling<ARCashSale.finPeriodID>(e.Row, payment.FinPeriodID, null);
			}

			ARReleaseProcess.UpdateARBalances(this, (ARRegister)e.Row, (((ARRegister)e.Row).OrigDocAmt));
		}

		protected virtual void ARCashSale_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARCashSale doc = e.Row as ARCashSale;
			if (doc.Released != true)
			{
					doc.DocDate = doc.AdjDate;

				doc.FinPeriodID  = doc.AdjFinPeriodID;
				doc.TranPeriodID = doc.AdjTranPeriodID;

				if (!this.IsCopyPasteContext)
				{
				sender.RaiseExceptionHandling<ARCashSale.finPeriodID>(doc, doc.FinPeriodID, null);

				if (sender.ObjectsEqual<ARCashSale.curyDocBal, ARCashSale.curyOrigDiscAmt>(e.Row, e.OldRow) == false && doc.CuryDocBal - doc.CuryOrigDiscAmt != doc.CuryOrigDocAmt)
				{
					if (doc.CuryDocBal != null && doc.CuryOrigDiscAmt != null && doc.CuryDocBal != 0)
						sender.SetValueExt<ARCashSale.curyOrigDocAmt>(doc, doc.CuryDocBal - doc.CuryOrigDiscAmt);
					else
						sender.SetValueExt<ARCashSale.curyOrigDocAmt>(doc, 0m);
				}
				else if (sender.ObjectsEqual<ARCashSale.curyOrigDocAmt>(e.Row, e.OldRow) == false)
				{
					if (doc.CuryDocBal != null && doc.CuryOrigDocAmt != null && doc.CuryDocBal != 0)
						sender.SetValueExt<ARCashSale.curyOrigDiscAmt>(doc, doc.CuryDocBal - doc.CuryOrigDocAmt);
					else
						sender.SetValueExt<ARCashSale.curyOrigDiscAmt>(doc, 0m);
				}
				if (doc.Hold != true)
				{
					if (doc.CuryDocBal < doc.CuryOrigDocAmt)
					{
						sender.RaiseExceptionHandling<ARCashSale.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else if (doc.CuryOrigDocAmt < 0)
					{
						sender.RaiseExceptionHandling<ARCashSale.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
					}
					else
					{
						sender.RaiseExceptionHandling<ARCashSale.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, null);
					}
				}

				PaymentCharges.UpdateChangesFromPayment(sender, e);
			}
			}

			if (e.OldRow != null)
			{
				ARReleaseProcess.UpdateARBalances(this, (ARRegister)e.OldRow, -(((ARRegister)e.OldRow).OrigDocAmt));
			}
			ARReleaseProcess.UpdateARBalances(this, (ARRegister)e.Row, (((ARRegister)e.Row).OrigDocAmt));
		}

        protected virtual void ParentRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
			if (!sender.ObjectsEqual<ARCashSale.branchID>(e.Row, e.OldRow))
        {
            foreach (ARSalesPerTran tran in salesPerTrans.Select())
            {
                this.salesPerTrans.Cache.MarkUpdated(tran);
            }
        }
        }
		#endregion

		#region CurrencyInfo Events

		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (cashaccount.Current != null && !string.IsNullOrEmpty(cashaccount.Current.CuryID))
				{
					e.NewValue = cashaccount.Current.CuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (cashaccount.Current != null && !string.IsNullOrEmpty(cashaccount.Current.CuryRateTypeID))
				{
					e.NewValue = cashaccount.Current.CuryRateTypeID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Cache.Current != null)
			{
				e.NewValue = ((ARCashSale)Document.Cache.Current).DocDate;
				e.Cancel = true;
			}
		}
		#endregion

		#region CATran Events
		protected virtual void CATran_CashAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_FinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_TranPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_ReferenceID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_CuryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		#endregion

		#region ARTran events
		protected virtual void ARTran_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARTran tran = (ARTran)e.Row;
			if (tran == null || Document.Current == null) return;

			Customer c = customer.Current
				?? SelectFrom<Customer>
					.Where<Customer.bAccountID.IsEqual<@P.AsInt>>
				.View.Select(this, Document.Current.CustomerID);

			if ((tran.InventoryID == null
					|| (c.IsBranch == true
						&& arsetup.Current.IntercompanySalesAccountDefault == ARAcctSubDefault.MaskLocation))
				&& location.Current != null)
			{
				e.NewValue = location.Current.CSalesAcctID;
				e.Cancel = true;
			}
		}

		protected virtual void ARTran_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARTran tran = (ARTran)e.Row;
			if (tran != null && tran.AccountID != null && location.Current != null)
			{
				InventoryItem item = (InventoryItem)PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, tran.InventoryID);
				EPEmployee employee = (EPEmployee)PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(this, PXAccess.GetUserID());
				CRLocation companyloc =
					PXSelectJoin<CRLocation, InnerJoin<BAccountR, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<GL.Branch, On<BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>, Where<GL.Branch.branchID, Equal<Required<ARTran.branchID>>>>.Select(this, tran.BranchID);
				SalesPerson salesperson = (SalesPerson)PXSelect<SalesPerson, Where<SalesPerson.salesPersonID, Equal<Current<ARTran.salesPersonID>>>>.SelectSingleBound(this, new object[] { e.Row });

				int? customer_SubID = (int?)Caches[typeof(Location)].GetValue<Location.cSalesSubID>(location.Current);
				int? item_SubID = (int?)Caches[typeof(InventoryItem)].GetValue<InventoryItem.salesSubID>(item);
				int? employee_SubID = (int?)Caches[typeof(EPEmployee)].GetValue<EPEmployee.salesSubID>(employee);
				int? company_SubID = (int?)Caches[typeof(CRLocation)].GetValue<CRLocation.cMPSalesSubID>(companyloc);
				int? salesperson_SubID = (int?)Caches[typeof(SalesPerson)].GetValue<SalesPerson.salesSubID>(salesperson);

				object value = SubAccountMaskAttribute.MakeSub<ARSetup.salesSubMask>(this, arsetup.Current.SalesSubMask,
					new object[] { customer_SubID, item_SubID, employee_SubID, company_SubID, salesperson_SubID },
					new Type[] { typeof(Location.cSalesSubID), typeof(InventoryItem.salesSubID), typeof(EPEmployee.salesSubID), typeof(Location.cMPSalesSubID), typeof(SalesPerson.salesSubID) });

				sender.RaiseFieldUpdating<ARTran.subID>(e.Row, ref value);

				e.NewValue = (int?)value;
				e.Cancel = true;
			}
		}

        [PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Category")]
        [ARCashSaleTax(typeof(ARCashSale), typeof(ARTax), typeof(ARTaxTran), parentBranchIDField: typeof(ARCashSale.branchID),
			   //Per Unit Tax settings
			   Inventory = typeof(ARTran.inventoryID), UOM = typeof(ARTran.uOM), LineQty = typeof(ARTran.qty))]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
        [PXDefault(typeof(Search<InventoryItem.taxCategoryID,
            Where<InventoryItem.inventoryID, Equal<Current<ARTran.inventoryID>>>>),
            PersistingCheck = PXPersistingCheck.Nothing, SearchOnDefault = false)]
        protected virtual void ARTran_TaxCategoryID_CacheAttached(PXCache sender)
        {
        }

		[PXBool]
		[DR.DRTerms.Dates(typeof(ARTran.dRTermStartDate), typeof(ARTran.dRTermEndDate), typeof(ARTran.inventoryID), typeof(ARTran.deferredCode), typeof(ARCashSale.hold))]
		protected virtual void ARTran_RequiresTerms_CacheAttached(PXCache sender) { }

		protected virtual void ARTran_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (TaxAttribute.GetTaxCalc<ARTran.taxCategoryID>(sender, e.Row) == TaxCalc.Calc && taxzone.Current != null && !string.IsNullOrEmpty(taxzone.Current.DfltTaxCategoryID) && ((ARTran)e.Row).InventoryID == null)
			{
				e.NewValue = taxzone.Current.DfltTaxCategoryID;
			}
		}

		protected virtual void ARTran_UnitPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (((ARTran)e.Row).InventoryID == null)
			{
				e.NewValue = 0m;
			}
		}


		protected virtual void ARTran_CuryUnitPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row == null)
                return;

            ARCashSale doc = this.Document.Current;
			if (doc != null && row.InventoryID != null && row.UOM != null && doc.CustomerID != null && row.Qty != null && row.ManualPrice != true)
			{
				string customerPriceClass = ARPriceClass.EmptyPriceClass;
				Location c = location.Select();
				if (c != null && !string.IsNullOrEmpty(c.CPriceClassID))
					customerPriceClass = c.CPriceClassID;

				CurrencyInfo currencyInfo = currencyinfo.Select();
				e.NewValue = ARSalesPriceMaint.CalculateSalesPrice(
					sender,
					customerPriceClass,
					doc.CustomerID,
					row.InventoryID,
					row.SiteID,
					currencyInfo.GetCM(),
					row.UOM,
					row.Qty,
					doc.DocDate.Value,
					row.CuryUnitPrice,
					doc.TaxCalcMode
					) ?? 0m;

				ARSalesPriceMaint.CheckNewUnitPrice<ARTran, ARTran.curyUnitPrice>(sender, row, e.NewValue);
			}
			else
			{
				e.NewValue = sender.GetValue<ARTran.curyUnitPrice>(e.Row);
				e.Cancel = e.NewValue != null;
				return;
			}
		}

		protected virtual void ARTran_ManualPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row != null)
			{
				if (row.ManualPrice != true && row.IsFree != true && !sender.Graph.IsCopyPasteContext)
				{
					sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);
				}
			}
		}

		protected virtual void ARTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARTran.unitPrice>(e.Row);
			sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);
			sender.SetValue<ARTran.unitPrice>(e.Row, null);
		}

		protected virtual void ARTran_Qty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARTran row = e.Row as ARTran;
			if (row != null)
			{
                sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);
			}
		}


		protected virtual void ARTran_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall)
			{
				e.Cancel = true;
			}
		}

		protected virtual void ARTran_SOShipmentNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void ARTran_SalesPersonID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARTran.subID>(e.Row);
		}

        [PopupMessage]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noSales>>), PX.Objects.IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
		protected virtual void ARTran_InventoryID_CacheAttached(PXCache sender)
        {
        }


		protected virtual void ARTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<ARTran.accountID>(e.Row);
			sender.SetDefaultExt<ARTran.subID>(e.Row);
			sender.SetDefaultExt<ARTran.taxCategoryID>(e.Row);
			sender.SetDefaultExt<ARTran.deferredCode>(e.Row);
			sender.SetDefaultExt<ARTran.uOM>(e.Row);

			sender.SetDefaultExt<ARTran.unitPrice>(e.Row);
			sender.SetDefaultExt<ARTran.curyUnitPrice>(e.Row);

			ARTran tran = e.Row as ARTran;
			IN.InventoryItem item = PXSelectorAttribute.Select<IN.InventoryItem.inventoryID>(sender, tran) as IN.InventoryItem;
			if (item != null && tran != null)
			{
				tran.TranDesc = PXDBLocalizableStringAttribute.GetTranslation(Caches[typeof(InventoryItem)], item, "Descr", customer.Current?.LocaleName);
			}
		}

		protected virtual void ARTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARTran row = (ARTran)e.Row;
			ARTran oldRow = (ARTran)e.OldRow;
			if (row != null)
			{
				TaxAttribute.Calculate<ARTran.taxCategoryID, ARCashSaleTaxAttribute>(sender, e);
			}

			if (row.ManualDisc != true)
			{
				var discountCode = (ARDiscount)PXSelectorAttribute.Select<SOLine.discountID>(sender, row);
				row.DiscPctDR = (discountCode != null && discountCode.IsAppliedToDR == true) ? row.DiscPct : 0.0m;
			}

			if ((e.ExternalCall || sender.Graph.IsImport)
					&& sender.ObjectsEqual<ARTran.inventoryID>(e.Row, e.OldRow) && sender.ObjectsEqual<ARTran.uOM>(e.Row, e.OldRow)
					&& sender.ObjectsEqual<ARTran.qty>(e.Row, e.OldRow) && sender.ObjectsEqual<ARTran.branchID>(e.Row, e.OldRow)
					&& sender.ObjectsEqual<ARTran.siteID>(e.Row, e.OldRow) && sender.ObjectsEqual<ARTran.manualPrice>(e.Row, e.OldRow)
					&& (!sender.ObjectsEqual<ARTran.curyUnitPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<ARTran.curyExtPrice>(e.Row, e.OldRow))
					&& row.ManualPrice == oldRow.ManualPrice)
				row.ManualPrice = true;

			if (row.ManualPrice != true)
			{
				row.CuryUnitPriceDR = row.CuryUnitPrice;
			}
		}

		protected virtual void ARTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			TaxAttribute.Calculate<ARTran.taxCategoryID, ARCashSaleTaxAttribute>(sender, e);
		}

		protected virtual void ARTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
		}

		protected virtual void ARTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARTran documentLine = e.Row as ARTran;
			if (documentLine == null) return;

			viewSchedule.SetEnabled(sender.GetStatus(e.Row) != PXEntryStatus.Inserted);

			#region Migration Mode Settings

			ARCashSale doc = Document.Current;

			if (doc != null &&
				doc.IsMigratedRecord == true &&
				doc.Released != true)
			{
				PXUIFieldAttribute.SetEnabled<ARTran.defScheduleID>(sender, null, false);
				PXUIFieldAttribute.SetEnabled<ARTran.deferredCode>(sender, null, false);
				PXUIFieldAttribute.SetEnabled<ARTran.dRTermStartDate>(sender, null, false);
				PXUIFieldAttribute.SetEnabled<ARTran.dRTermEndDate>(sender, null, false);
			}

			#endregion
		}

		protected virtual void ARTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row == null) return;

			DR.ScheduleHelper.DeleteAssociatedScheduleIfDeferralCodeChanged(this, e.Row as ARTran);
		}

		protected virtual void ARTran_DrCr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = ARInvoiceType.DrCr(Document.Current.DocType);
				e.Cancel = true;
			}
		}

		protected virtual void ARTran_DRTermStartDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var line = e.Row as ARTran;

			if (line != null && line.RequiresTerms == true)
			{
				e.NewValue = Document.Current.DocDate;
			}
		}
		#endregion

		#region ARTaxTran Events
		protected virtual void ARTaxTran_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = Document.Current.TaxZoneID;
				e.Cancel = true;
			}
		}

		protected virtual void ARTaxTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (!(e.Row is ARTaxTran arTaxTran))
				return;

			PXUIFieldAttribute.SetEnabled<ARTaxTran.taxID>(sender, e.Row, sender.GetStatus(e.Row) == PXEntryStatus.Inserted);
		}

		protected virtual void ARTaxTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (Document.Current != null && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update))
			{
				((ARTaxTran)e.Row).TaxZoneID = Document.Current.TaxZoneID;
			}
		}

		protected virtual void _(Events.FieldUpdated<ARTaxTran, ARTaxTran.taxID> e)
		{
			if (!(e.Row is ARTaxTran arTaxTran))
				return;

			if (e.OldValue != null && e.OldValue != e.NewValue)
			{
				Taxes.Cache.SetDefaultExt<ARTaxTran.accountID>(arTaxTran);
				Taxes.Cache.SetDefaultExt<ARTaxTran.taxType>(arTaxTran);
				Taxes.Cache.SetDefaultExt<ARTaxTran.taxBucketID>(arTaxTran);
			}
		}
		#endregion

		#region ARSalesPerTran events

		protected virtual void ARSalesPerTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			ARSalesPerTran row = (ARSalesPerTran)e.Row;
			foreach (ARSalesPerTran iSpt in this.salesPerTrans.Select())
			{
				if (iSpt.SalespersonID == row.SalespersonID)
				{
					PXEntryStatus status = this.salesPerTrans.Cache.GetStatus(iSpt);
					if (!(status == PXEntryStatus.InsertedDeleted || status == PXEntryStatus.Deleted))
					{
						sender.RaiseExceptionHandling<ARSalesPerTran.salespersonID>(e.Row, null, new PXException(Messages.ERR_DuplicatedSalesPersonAdded));
						e.Cancel = true;
						break;
					}
				}
			}
		}
		#endregion

        #region Voiding
        private bool _IsVoidCheckInProgress = false;

		protected virtual void ARCashSale_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		protected virtual void ARCashSale_FinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		protected virtual void ARCashSale_AdjFinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		public virtual void VoidCheckProc(ARCashSale doc)
		{
			this.Clear(PXClearOption.PreserveTimeStamp);

            TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.NoCalc);

			foreach (PXResult<ARCashSale, CurrencyInfo> res in PXSelectJoin<ARCashSale, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARCashSale.curyInfoID>>>, Where<ARCashSale.docType, Equal<Required<ARCashSale.docType>>, And<ARCashSale.refNbr, Equal<Required<ARCashSale.refNbr>>>>>.Select(this, (object)doc.DocType, doc.RefNbr))
			{
				CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)res);
				info.CuryInfoID = null;
				info.IsReadOnly = false;
				info = PXCache<CurrencyInfo>.CreateCopy(this.currencyinfo.Insert(info));

				ARCashSale newdocument = Document.Insert(new ARCashSale
				{
					DocType = ARDocType.CashReturn,
					RefNbr = null,
					CuryInfoID = info.CuryInfoID,
					OrigDocType = ((ARCashSale)res).DocType,
					OrigRefNbr = ((ARCashSale)res).RefNbr,
					OrigModule = GL.BatchModule.AR
				});

				if (newdocument.RefNbr == null)
				{
					//manual numbering, check for occasional duplicate
					ARCashSale duplicate = PXSelect<ARCashSale>.Search<ARCashSale.docType, ARCashSale.refNbr>(this, newdocument.DocType, newdocument.OrigRefNbr);
					if (duplicate != null)
					{
						throw new PXException(ErrorMessages.RecordExists);
					}

					newdocument.RefNbr = newdocument.OrigRefNbr;
					this.Document.Cache.Normalize();
					newdocument = this.Document.Update(newdocument);
				}

				newdocument = PXCache<ARCashSale>.CreateCopy((ARCashSale)res);
                newdocument.OrigModule = GL.BatchModule.AR;

				ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(this, ExternalTran);
				if (state.IsCaptured)
				{
					newdocument.RefTranExtNbr = state.ExternalTransaction.TranNumber;
				}
				newdocument.CuryInfoID = info.CuryInfoID;
				newdocument.DocType = Document.Current.DocType;
				newdocument.RefNbr = Document.Current.RefNbr;
				newdocument.OrigDocType = Document.Current.OrigDocType;
				newdocument.OrigRefNbr = Document.Current.OrigRefNbr;
				newdocument.CATranID = null;
				newdocument.NoteID = null;
				newdocument.RefNoteID = null;
				newdocument.IsTaxPosted = false;
				newdocument.IsTaxValid = false;

				//must set for _RowSelected
				newdocument.OpenDoc = true;
				newdocument.Released = false;
				Document.Cache.SetDefaultExt<ARPayment.hold>(newdocument);
				Document.Cache.SetDefaultExt<ARPayment.isMigratedRecord>(newdocument);
				Document.Cache.SetDefaultExt<ARPayment.status>(newdocument);
				newdocument.Printed = false;
				newdocument.Emailed = false;
				newdocument.LineCntr = 0;
				newdocument.AdjCntr = 0;
				newdocument.BatchNbr = null;
				newdocument.AdjDate = doc.DocDate;
			    FinPeriodIDAttribute.SetPeriodsByMaster<ARCashSale.adjFinPeriodID>(Document.Cache, newdocument, doc.AdjTranPeriodID);
				newdocument.CuryDocBal = newdocument.CuryOrigDocAmt + newdocument.CuryOrigDiscAmt;
				newdocument.CuryChargeAmt = 0;
				newdocument.CuryConsolidateChargeTotal = 0;
				newdocument.ClosedDate = null;
				newdocument.ClosedFinPeriodID = null;
				newdocument.ClosedTranPeriodID = null;

				newdocument.Cleared = false;
				newdocument.ClearDate = null;

				newdocument.Deposited = false;
				newdocument.DepositDate = null;
				newdocument.DepositType = null;
				newdocument.DepositNbr = null;

                newdocument.CuryVatTaxableTotal = 0m;
                newdocument.CuryVatExemptTotal = 0m;
				this.Document.Update(newdocument);

				using (new PX.SM.SuppressWorkflowAutoPersistScope(this))
				{
				this.initializeState.Press();
				}


				if (info != null)
				{
					CurrencyInfo b_info = (CurrencyInfo)PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARCashSale.curyInfoID>>>>.Select(this, null);
					b_info.CuryID = info.CuryID;
					b_info.CuryEffDate = info.CuryEffDate;
					b_info.CuryRateTypeID = info.CuryRateTypeID;
					b_info.CuryRate = info.CuryRate;
					b_info.RecipRate = info.RecipRate;
					b_info.CuryMultDiv = info.CuryMultDiv;
					this.currencyinfo.Update(b_info);
				}
			}

			this.FieldDefaulting.AddHandler<ARTran.salesPersonID>((sender, e) => { e.NewValue = null; e.Cancel = true; });

			foreach (ARTran srcTran in PXSelect<ARTran,
				Where<ARTran.tranType, Equal<Required<ARTran.tranType>>, And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
					And<Where<ARTran.lineType, IsNull, Or<ARTran.lineType, NotEqual<SO.SOLineType.discount>>>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				ARTran tran = PXCache<ARTran>.CreateCopy(srcTran);
				tran.TranType = null;
				tran.RefNbr = null;
				tran.DrCr = null;
				tran.Released = null;
                tran.CuryInfoID = null;
				tran.NoteID = null;
				tran.IsStockItem = null;

				SalesPerson sp = (SalesPerson)PXSelectorAttribute.Select<ARTran.salesPersonID>(this.Transactions.Cache, tran);
				if (sp == null || sp.IsActive == false)
					tran.SalesPersonID = null;

				tran = Transactions.Insert(tran);
				PXNoteAttribute.CopyNoteAndFiles(Transactions.Cache, srcTran, Transactions.Cache, tran);
			}

			this.RowInserting.AddHandler<ARSalesPerTran>((sender, e) => { e.Cancel = true; });

			foreach (ARSalesPerTran salespertran in PXSelect<ARSalesPerTran, Where<ARSalesPerTran.docType, Equal<Required<ARSalesPerTran.docType>>, And<ARSalesPerTran.refNbr, Equal<Required<ARSalesPerTran.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				ARSalesPerTran newtran = PXCache<ARSalesPerTran>.CreateCopy(salespertran);

				newtran.DocType = Document.Current.DocType;
				newtran.RefNbr = Document.Current.RefNbr;
				newtran.Released = false;
				newtran.CuryInfoID = Document.Current.CuryInfoID;
				newtran.CuryCommnblAmt *= -1m;
				newtran.CuryCommnAmt *= -1m;

				SalesPerson sp = (SalesPerson)PXSelectorAttribute.Select<ARSalesPerTran.salespersonID>(this.salesPerTrans.Cache, newtran);
				if (!(sp == null || sp.IsActive == false))
				{
					this.salesPerTrans.Update(newtran);
				}
			}

			TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualCalc);

			if (!IsExternalTax(doc.TaxZoneID))
			{
				foreach (ARTaxTran tax in PXSelect<ARTaxTran, Where<ARTaxTran.tranType, Equal<Required<ARTaxTran.tranType>>, And<ARTaxTran.refNbr, Equal<Required<ARTaxTran.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
				{
					ARTaxTran new_artax = new ARTaxTran();
					new_artax.TaxID = tax.TaxID;

					new_artax = this.Taxes.Insert(new_artax);

					if (new_artax != null)
					{
						new_artax = PXCache<ARTaxTran>.CreateCopy(new_artax);
						new_artax.TaxRate = tax.TaxRate;
						new_artax.CuryTaxableAmt = tax.CuryTaxableAmt;
						new_artax.CuryTaxAmt = tax.CuryTaxAmt;
						new_artax.CuryTaxDiscountAmt = tax.CuryTaxDiscountAmt;
						new_artax.CuryTaxableDiscountAmt = tax.CuryTaxableDiscountAmt;
						new_artax = this.Taxes.Update(new_artax);
					}
				}
			}
			ARCashSale document = Document.Current;
			document.CuryOrigDiscAmt = doc.CuryOrigDiscAmt;
			Document.Update(document);

			PaymentCharges.ReverseCharges(doc, Document.Current);
        }
		#endregion

		#region External Tax Provider

		public virtual bool IsExternalTax(string taxZoneID)
			{
					return false;
					}

		public virtual ARCashSale CalculateExternalTax(ARCashSale invoice)
					{
			return invoice;
		}

		#endregion

		#region Address Lookup Extension
		/// <exclude/>
		public class ARCashSaleEntryAddressLookupExtension : CR.Extensions.AddressLookupExtension<ARCashSaleEntry, ARCashSale, ARAddress>
		{
			protected override string AddressView => nameof(Base.Billing_Address);
		}

		/// <exclude/>
		public class ARCashSaleEntryShippingAddressLookupExtension : CR.Extensions.AddressLookupExtension<ARCashSaleEntry, ARCashSale, ARShippingAddress>
		{
			protected override string AddressView => nameof(Base.Shipping_Address);
		}

		public class ARCashSaleEntryAddressCachingHelper : AddressValidationExtension<ARCashSaleEntry, ARAddress>
		{
			protected override IEnumerable<PXSelectBase<ARAddress>> AddressSelects()
			{
				yield return Base.Billing_Address;
			}
		}

		public class ARCashSaleEntryShippingAddressCachingHelper : AddressValidationExtension<ARCashSaleEntry, ARShippingAddress>
		{
			protected override IEnumerable<PXSelectBase<ARShippingAddress>> AddressSelects()
			{
				yield return Base.Shipping_Address;
			}
		}
		#endregion
	}
}
