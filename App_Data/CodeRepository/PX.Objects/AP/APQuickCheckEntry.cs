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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.Data.WorkflowAPI;
using PX.LicensePolicy;

using PX.Objects.Common;
using PX.Objects.Common.Bql;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.CM.Extensions;
using PX.Objects.CA;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.TX;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.GL.Reclassification.UI;
using PX.Objects.PO;
using APQuickCheck = PX.Objects.AP.Standalone.APQuickCheck;
using AP1099Hist = PX.Objects.AP.Overrides.APDocumentRelease.AP1099Hist;
using AP1099Yr = PX.Objects.AP.Overrides.APDocumentRelease.AP1099Yr;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.Extensions.MultiCurrency.AP;

namespace PX.Objects.AP
{
	public partial class APQuickCheckEntry : APDataEntryGraph<APQuickCheckEntry, APQuickCheck>, IGraphWithInitialization
	{			
		#region Extensions

		public class APQuickCheckEntryDocumentExtension : PaidInvoiceGraphExtension<APQuickCheckEntry>
	    {
	        public override PXSelectBase<Location> Location => Base.location;

	        public override PXSelectBase<CurrencyInfo> CurrencyInfo => Base.currencyinfo;

            public override void Initialize()
	        {
	            base.Initialize();

	            Documents = new PXSelectExtension<PaidInvoice>(Base.Document);
	            Lines = new PXSelectExtension<DocumentLine>(Base.Transactions);
	            InvoiceTrans = new PXSelectExtension<InvoiceTran>(Base.Transactions);
	            TaxTrans = new PXSelectExtension<GenericTaxTran>(Base.Taxes);
	            LineTaxes = new PXSelectExtension<LineTax>(Base.Tax_Rows);
            }

			public override void SuppressApproval()
			{
				Base.Approval.SuppressApproval = true;
			}

			protected override PaidInvoiceMapping GetDocumentMapping()
	        {
	            return new PaidInvoiceMapping(typeof(APQuickCheck))
	            {
                    HeaderFinPeriodID = typeof(APQuickCheck.adjFinPeriodID),
                    HeaderTranPeriodID = typeof(APQuickCheck.adjTranPeriodID),
                    HeaderDocDate = typeof(APQuickCheck.adjDate),
	                ContragentID = typeof(APQuickCheck.vendorID),
	                ContragentLocationID = typeof(APQuickCheck.vendorLocationID),
                };
	        }

	        protected override DocumentLineMapping GetDocumentLineMapping()
	        {
	            return new DocumentLineMapping(typeof(APTran));
            }

	        protected override ContragentMapping GetContragentMapping()
	        {
	            return new ContragentMapping(typeof(Vendor));
	        }

	        protected override InvoiceTranMapping GetInvoiceTranMapping()
	        {
	            return new InvoiceTranMapping(typeof(APTran));
	        }

	        protected override GenericTaxTranMapping GetGenericTaxTranMapping()
	        {
	            return new GenericTaxTranMapping(typeof(APTaxTran));
	        }

	        protected override LineTaxMapping GetLineTaxMapping()
	        {
	            return new LineTaxMapping(typeof(APTax));
	        }
        }

		public class MultiCurrency : APMultiCurrencyGraph<APQuickCheckEntry, APQuickCheck>
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

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(APQuickCheck))
				{
					DocumentDate = typeof(APQuickCheck.adjDate),
					BAccountID = typeof(APQuickCheck.vendorID)
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
					Base.PaymentCharges
				};
			}

			protected override bool ShouldBeDisabledDueToDocStatus()
			{
				return Base.Document.Current?.DocType == APDocType.VoidQuickCheck 
					|| base.ShouldBeDisabledDueToDocStatus();
			}

			protected virtual void _(Events.FieldUpdated<APQuickCheck, APQuickCheck.cashAccountID> e)
			{
				if (Base._IsVoidCheckInProgress || !PXAccess.FeatureInstalled<FeaturesSet.multicurrency>()) return;
				else
				{
					SourceFieldUpdated<APQuickCheck.curyInfoID, APQuickCheck.curyID, APQuickCheck.adjDate>(e.Cache, e.Row);
					SetDetailCuryInfoID(Base.Transactions, e.Row.CuryInfoID);
				}
			}
		}

		#endregion

		public PXSelect<InventoryItem, Where<InventoryItem.stkItem, Equal<False>, And<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>> nonStockItem;

		[PXCopyPasteHiddenFields(typeof(APQuickCheck.extRefNbr), typeof(APQuickCheck.cleared), typeof(APQuickCheck.clearDate))]
		[PXViewName(Messages.QuickCheck)]
		public PXSelectJoin<APQuickCheck,
			LeftJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<APQuickCheck.vendorID>>>,
			Where<APQuickCheck.docType, Equal<Optional<APQuickCheck.docType>>,
			And<Where<Vendor.bAccountID, IsNull,
			Or<Match<Vendor, Current<AccessInfo.userName>>>>>>> Document;
		[PXCopyPasteHiddenFields(typeof(APQuickCheck.printCheck), typeof(APQuickCheck.cleared), typeof(APQuickCheck.clearDate))]
		public PXSelect<APQuickCheck, Where<APQuickCheck.docType, Equal<Current<APQuickCheck.docType>>, And<APQuickCheck.refNbr, Equal<Current<APQuickCheck.refNbr>>>>> CurrentDocument;

		[PXViewName(Messages.QuickCheckLine)]
		public PXSelect<APTran, Where<APTran.tranType, Equal<Current<APQuickCheck.docType>>, And<APTran.refNbr, Equal<Current<APQuickCheck.refNbr>>>>> Transactions;
		public PXSelect<APTax> ItemTaxes;

	    [PXCopyPasteHiddenView]
	    public PXSelect<APTax,
	        Where<
	                APTax.tranType, Equal<Current<APInvoice.docType>>,
	            And<APTax.refNbr, Equal<Current<APInvoice.refNbr>>>>,
	        OrderBy<
	            Asc<APTax.tranType,
	            Asc<APTax.refNbr, 
	            Asc<APTax.taxID>>>>>
	        Tax_Rows;

        public PXSelectJoin<APTaxTran, LeftJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, Where<APTaxTran.module, Equal<BatchModule.moduleAP>, And<APTaxTran.tranType, Equal<Current<APQuickCheck.docType>>, And<APTaxTran.refNbr, Equal<Current<APQuickCheck.refNbr>>>>>> Taxes;

		// We should use read only view here
		// to prevent cache merge because it
		// used only as a shared BQL query.
		// 
		public PXSelectReadonly2<APTaxTran, 
			LeftJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>, 
			Where<APTaxTran.module, Equal<BatchModule.moduleAP>, 
				And<APTaxTran.tranType, Equal<Current<APQuickCheck.docType>>, 
				And<APTaxTran.refNbr, Equal<Current<APQuickCheck.refNbr>>,
			And<Tax.taxType, Equal<CSTaxType.use>>>>>> UseTaxes;

		[PXViewName(Messages.APAddress)]
		public PXSelect<APAddress, Where<APAddress.addressID, Equal<Current<APQuickCheck.remitAddressID>>>> Remittance_Address;
		[PXViewName(Messages.APContact)]
		public PXSelect<APContact, Where<APContact.contactID, Equal<Current<APQuickCheck.remitContactID>>>> Remittance_Contact;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APQuickCheck.curyInfoID>>>> currencyinfo;
		public PXSelect<PaymentMethodAccount, Where<PaymentMethodAccount.cashAccountID, Equal<Current<APQuickCheck.cashAccountID>>,
					And<PaymentMethodAccount.useForAP,Equal<True>>>, OrderBy<Asc<PaymentMethodAccount.aPIsDefault>>> CashAcctDetail_AccountID;

		public APPaymentChargeSelect<APQuickCheck, APQuickCheck.paymentMethodID, APQuickCheck.cashAccountID, APQuickCheck.docDate, APQuickCheck.tranPeriodID,
            Where<APPaymentChargeTran.docType, Equal<Current<APQuickCheck.docType>>,
				And<APPaymentChargeTran.refNbr, Equal<Current<APQuickCheck.refNbr>>>>> PaymentCharges;

		public PXSetup<Vendor, Where<Vendor.bAccountID, Equal<Current<APQuickCheck.vendorID>>>> vendor;
		public PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<APQuickCheck.vendorID>>>> EmployeeByVendor;

		[PXViewName(EP.Messages.Employee)]
		public PXSelect<EPEmployee, Where<EPEmployee.defContactID, Equal<Current<APQuickCheck.employeeID>>>> employee;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Current<APQuickCheck.vendorID>>, And<Location.locationID, Equal<Optional<APQuickCheck.vendorLocationID>>>>> location;
		public PXSetup<CashAccount, Where<CashAccount.cashAccountID, Equal<Optional<APQuickCheck.cashAccountID>>>> cashaccount;
		public PXSetup<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Optional<APQuickCheck.paymentMethodID>>>> paymenttype;
		public PXSetup<PaymentMethodAccount, Where<PaymentMethodAccount.cashAccountID, Equal<Optional<APQuickCheck.cashAccountID>>, And<PaymentMethodAccount.paymentMethodID, Equal<Current<APQuickCheck.paymentMethodID>>>>> cashaccountdetail;

		public PXSetup<
			OrganizationFinPeriod, 
			Where<OrganizationFinPeriod.finPeriodID, Equal<Current<APQuickCheck.adjFinPeriodID>>,
				And<EqualToOrganizationOfBranch<OrganizationFinPeriod.organizationID, Current<APQuickCheck.branchID>>>>> 
			finperiod;

		public PXSetup<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<APQuickCheck.taxZoneID>>>> taxzone;

		public PXSelect<AP1099Hist> ap1099hist;
		public PXSelect<AP1099Yr> ap1099year;

		public PXSetup<GLSetup> glsetup;

		public PXSelect<GLVoucher, Where<True, Equal<False>>> Voucher;

		public PXSelect<APSetupApproval,
			Where<Current<APQuickCheck.docType>, Equal<APDocType.quickCheck>,
				And<APSetupApproval.docType, Equal<APDocType.quickCheck>>>> SetupApproval;
		[PXViewName(EP.Messages.Approval)]
		public EPApprovalAutomationWithoutHoldDefaulting<APQuickCheck, APQuickCheck.approved, APQuickCheck.rejected, APQuickCheck.hold, APSetupApproval> Approval;

		[PXCopyPasteHiddenView]
		public PXSelect<APAdjust> dummy_APAdjust;
		public PXSelect<CashAccountCheck> dummy_CashAccountCheck;

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		#region Other Buttons
		public PXAction<APQuickCheck> printAPEdit;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "AP Edit Detailed", MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable PrintAPEdit(PXAdapter adapter, string reportID = null) => Report(adapter, reportID ?? "AP610500");

		public PXAction<APQuickCheck> printAPRegister;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "AP Register Detailed", MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable PrintAPRegister(PXAdapter adapter, string reportID = null) => Report(adapter, reportID ??"AP622000");

		public PXAction<APQuickCheck> printAPPayment;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "AP Payment Register", MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable PrintAPPayment(PXAdapter adapter, string reportID = null) => Report(adapter, reportID ?? "AP622500");
		
		public PXAction<APQuickCheck> printCheck;

		[PXUIField(DisplayName = "Print/Process", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable PrintCheck(PXAdapter adapter)
		{
			if (this.IsDirty)
				this.Save.Press();
			APPayment doc = PXSelect<APPayment, Where<APPayment.docType, Equal<Current<APQuickCheck.docType>>, And<APPayment.refNbr, Equal<Current<APQuickCheck.refNbr>>>>>.SelectSingleBound(this, new object[] {Document.Current});
			APPrintChecks pp = PXGraph.CreateInstance<APPrintChecks>();
			PrintChecksFilter filter_copy = PXCache<PrintChecksFilter>.CreateCopy(pp.Filter.Current);
			filter_copy.BranchID = CurrentDocument.Current.BranchID;
			filter_copy.PayAccountID = doc.CashAccountID;
			filter_copy.PayTypeID = doc.PaymentMethodID;
			pp.Filter.Cache.Update(filter_copy);
			doc.Selected = true;
			doc.Passed = true;
			pp.APPaymentList.Cache.Update(doc);
			pp.APPaymentList.Cache.SetStatus(doc, PXEntryStatus.Updated);
			pp.APPaymentList.Cache.IsDirty = false;
			throw new PXRedirectRequiredException(pp, "Preview");
		}

		[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public override IEnumerable Release(PXAdapter adapter)
		{

			PXCache cache = Document.Cache;
			List<APRegister> list = new List<APRegister>();
			foreach (APQuickCheck apdoc in adapter.Get<APQuickCheck>())
			{
				if (apdoc.Status != APDocStatus.Balanced && apdoc.Status != APDocStatus.Printed && apdoc.Status != APDocStatus.Prebooked)
				{
					throw new PXException(Messages.Document_Status_Invalid);
				}
				if (this.PaymentRefMustBeUnique && string.IsNullOrEmpty(apdoc.ExtRefNbr))
				{
					cache.RaiseExceptionHandling<APQuickCheck.extRefNbr>(apdoc, apdoc.ExtRefNbr,
						new PXRowPersistingException(typeof(APQuickCheck.extRefNbr).Name, null, ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APQuickCheck.extRefNbr>(cache)));
				}
				cache.Update(apdoc);
				list.Add(apdoc);
			}
			Save.Press();
			PXLongOperation.StartOperation(this, delegate () { APDocumentRelease.ReleaseDoc(list, false); });
			return list;
		}

		public PXAction<APQuickCheck> prebook;

		[PXUIField(DisplayName = "Pre-release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable Prebook(PXAdapter adapter)
		{
			PXCache cache = Document.Cache;
			List<APRegister> list = new List<APRegister>();

			foreach (APQuickCheck apdoc in adapter.Get<APQuickCheck>())
			{
				if (apdoc.Status != APDocStatus.Balanced && apdoc.Status != APDocStatus.Printed)
				{
					throw new PXException(Messages.Document_Status_Invalid);
				}
				if (apdoc.PrebookAcctID == null)
				{
					cache.RaiseExceptionHandling<APQuickCheck.prebookAcctID>(apdoc, apdoc.PrebookAcctID,
						new PXSetPropertyException(Messages.PrebookingAccountIsRequiredForPrebooking));
					continue;
				}
				if (apdoc.PrebookSubID == null)
				{
					cache.RaiseExceptionHandling<APQuickCheck.prebookSubID>(apdoc, apdoc.PrebookSubID,
						new PXSetPropertyException(Messages.PrebookingAccountIsRequiredForPrebooking));
					continue;
				}
				if (this.PaymentRefMustBeUnique && string.IsNullOrEmpty(apdoc.ExtRefNbr))
				{
					cache.RaiseExceptionHandling<APQuickCheck.extRefNbr>(apdoc, apdoc.ExtRefNbr,
						new PXRowPersistingException(typeof(APQuickCheck.extRefNbr).Name, null, ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APQuickCheck.extRefNbr>(cache)));
				}
				cache.Update(apdoc);
				list.Add(apdoc);
			}

			Save.Press();
			PXLongOperation.StartOperation(this, delegate { APDocumentRelease.ReleaseDoc(list, isMassProcess: false, isPrebooking: true); });
			return list;
		}

		[PXUIField(DisplayName = "Void", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public override IEnumerable VoidCheck(PXAdapter adapter)
		{
			if (Document.Current != null && (Document.Current.Released == true || Document.Current.Prebooked == true) && Document.Current.Voided == false && Document.Current.DocType == APDocType.QuickCheck)
			{
				APQuickCheck doc = PXCache<APQuickCheck>.CreateCopy(Document.Current);

				FinPeriodUtils.VerifyAndSetFirstOpenedFinPeriod<APQuickCheck.finPeriodID, APQuickCheck.branchID>(Document.Cache, doc, finperiod, typeof(OrganizationFinPeriod.aPClosed));

				try
				{
					_IsVoidCheckInProgress = true;
					this.VoidCheckProc(doc);
				}
				catch (PXSetPropertyException)
				{
					this.Clear();
					Document.Current = doc;
					throw;
				}
				finally
				{
					_IsVoidCheckInProgress = false;
				}

				Document.Cache.RaiseExceptionHandling<APQuickCheck.finPeriodID>(Document.Current, Document.Current.FinPeriodID, null);

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
			return adapter.Get();
		}

		public PXAction<APQuickCheck> reclassifyBatch;
		[PXUIField(DisplayName = Messages.ReclassifyGLBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ReclassifyBatch(PXAdapter adapter)
		{
			var document = Document.Current;

			if (document != null)
			{
				ReclassifyTransactionsProcess.TryOpenForReclassificationOfDocument(Document.View, BatchModule.AP, document.BatchNbr, document.DocType,
					document.RefNbr);
			}

			return adapter.Get();
		}

		public PXAction<APQuickCheck> vendorDocuments;
		[PXUIField(DisplayName = "Vendor Details", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable VendorDocuments(PXAdapter adapter)
		{
			if (vendor.Current != null)
			{
				APDocumentEnq graph = PXGraph.CreateInstance<APDocumentEnq>();
				graph.Filter.Current.VendorID = vendor.Current.BAccountID;
				graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Vendor Details");
			}
			return adapter.Get();
		}

		public PXAction<APQuickCheck> viewSchedule;
		[PXUIField(DisplayName = "View Deferrals")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Settings)]
		public virtual IEnumerable ViewSchedule(PXAdapter adapter)
		{
			var currentTransaction = Transactions.Current;

			var invoicePart = PXSelect<
				APInvoice,
				Where<
					APInvoice.docType, Equal<Current<APQuickCheck.docType>>,
					And<APInvoice.refNbr, Equal<Current<APQuickCheck.refNbr>>>>>
				.Select(this);

			if (currentTransaction != null &&
				invoicePart != null &&
				Transactions.Cache.GetStatus(currentTransaction) == PXEntryStatus.Notchanged)
			{
				Save.Press();
				APInvoiceEntry.ViewScheduleForLine(this, invoicePart, Transactions.Current);
			}

			return adapter.Get();
		}

		public PXAction<APQuickCheck> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddress, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: false,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (APQuickCheck current in adapter.Get<APQuickCheck>())
			{
				if (current != null)
				{
					FindAllImplementations<IAddressValidationHelper>().ValidateAddresses();
				}
				yield return current;
			}
		}

		public PXAction<APQuickCheck> ViewOriginalDocument;

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		protected virtual IEnumerable viewOriginalDocument(PXAdapter adapter)
		{
			RedirectionToOrigDoc.TryRedirect(Document.Current.OrigDocType, Document.Current.OrigRefNbr, Document.Current.OrigModule);
			return adapter.Get();
		}
		#endregion

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }

		public APQuickCheckEntry()
			:base()
		{
			{
				APSetup record = apsetup.Select();
			}

			{
				GLSetup record = glsetup.Select();
			}

			OpenPeriodAttribute.SetValidatePeriod<APQuickCheck.adjFinPeriodID>(Document.Cache, null, PeriodValidation.DefaultSelectUpdate);
			PXUIFieldAttribute.SetVisible<APTran.projectID>(Transactions.Cache, null, PM.ProjectAttribute.IsPMVisible( BatchModule.AP));
			PXUIFieldAttribute.SetVisible<APTran.taskID>(Transactions.Cache, null, PM.ProjectAttribute.IsPMVisible( BatchModule.AP));
			PXUIFieldAttribute.SetVisible<APTran.nonBillable>(Transactions.Cache, null, PM.ProjectAttribute.IsPMVisible( BatchModule.AP));

			FieldDefaulting.AddHandler<InventoryItem.stkItem>((sender, e) => { if (e.Row != null) e.NewValue = false; });
			APQuickCheckTaxAttribute.IncludeDirectTaxLine<APTran.taxCategoryID>(Transactions.Cache, null, true);
		}

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += _licenseLimits.GetCheckerDelegate<APQuickCheck>(
					new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(APTran), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<APTran.tranType>(PXDbType.Char, 3, ((APQuickCheckEntry)graph).Document.Current?.DocType),
							new PXDataFieldValue<APTran.refNbr>(((APQuickCheckEntry)graph).Document.Current?.RefNbr)
						};
					}));
			}
		}

		#region InventoryItem
		#region COGSSubID
		[PXDefault(typeof(Search<INPostClass.cOGSSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>))]
		[SubAccount(typeof(InventoryItem.cOGSAcctID), DisplayName = "Expense Sub.", DescriptionField = typeof(Sub.description))]
		public virtual void InventoryItem_COGSSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion

		#region APQuickCheck Events

		protected virtual void APQuickCheck_Cleared_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APQuickCheck payment = (APQuickCheck)e.Row;
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

		protected virtual void APQuickCheck_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = APDocType.QuickCheck;
			e.Cancel = true;
		}

		protected virtual void APQuickCheck_DocDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && ((APQuickCheck)e.Row).Released == false)
			{
				e.NewValue = ((APQuickCheck)e.Row).AdjDate;
				e.Cancel = true;
			}
		}

		protected virtual void APQuickCheck_DocDesc_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APQuickCheck quickCheck = (APQuickCheck)e.Row;
			if (quickCheck?.Released != false) return;

			foreach (APTaxTran aPTaxTran in Taxes.Select())
			{
				aPTaxTran.Description = quickCheck.DocDesc;
				Taxes.Cache.Update(aPTaxTran);
			}
		}

		protected virtual void APQuickCheck_FinPeriodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null)
			{
				e.NewValue = ((APQuickCheck)e.Row).AdjFinPeriodID;
				e.Cancel = true;
			}
		}

		protected virtual void APQuickCheck_TranPeriodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null)
			{
				e.NewValue = ((APQuickCheck)e.Row).AdjTranPeriodID;
				e.Cancel = true;
			}
		}

		protected virtual void APQuickCheck_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			vendor.RaiseFieldUpdated(sender, e.Row);

			sender.SetDefaultExt<APQuickCheck.vendorLocationID>(e.Row);
			sender.SetDefaultExt<APQuickCheck.termsID>(e.Row);
			if (vendor.Current?.Type == BAccountType.EmployeeType)
			{
				sender.SetDefaultExt<APQuickCheck.employeeID>(e.Row);
			}
		}

		protected virtual void APQuickCheck_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			location.RaiseFieldUpdated(sender, e.Row);

			sender.SetDefaultExt<APQuickCheck.paymentMethodID>(e.Row);
			sender.SetDefaultExt<APQuickCheck.aPAccountID>(e.Row);
			sender.SetDefaultExt<APQuickCheck.aPSubID>(e.Row);

			APAddressAttribute.DefaultRecord<APPayment.remitAddressID>(sender, e.Row);
			APContactAttribute.DefaultRecord<APPayment.remitContactID>(sender, e.Row);

			sender.SetDefaultExt<APQuickCheck.taxCalcMode>(e.Row);
			sender.SetDefaultExt<APQuickCheck.taxZoneID>(e.Row);
			sender.SetDefaultExt<APQuickCheck.prebookAcctID>(e.Row);
			sender.SetDefaultExt<APQuickCheck.prebookSubID>(e.Row);

		}

		protected virtual void APQuickCheck_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APQuickCheck payment = (APQuickCheck)e.Row;
			cashaccount.RaiseFieldUpdated(sender, e.Row);

			payment.Cleared = false;
			payment.ClearDate = null;

			if ((cashaccount.Current != null) && (cashaccount.Current.Reconcile == false))
			{
				payment.Cleared = true;
				payment.ClearDate = payment.DocDate;
			}
		}

		protected virtual void APQuickCheck_Hold_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APQuickCheck doc = (APQuickCheck)e.Row;
			if (IsApprovalRequired(doc))
				{
				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Legacy - requested for approval process working]
					sender.SetValue<APQuickCheck.hold>(doc, true);
				}
			}
		protected virtual void APQuickCheck_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			paymenttype.RaiseFieldUpdated(sender, e.Row);

			sender.SetDefaultExt<APQuickCheck.cashAccountID>(e.Row);
			sender.SetDefaultExt<APQuickCheck.printCheck>(e.Row);
		}

		protected virtual void APQuickCheck_PrintCheck_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			switch (((APQuickCheck)e.Row).DocType)
			{
				case APDocType.Refund:
				case APDocType.Prepayment:
				case APDocType.VoidQuickCheck:
					e.NewValue = false;
					e.Cancel = true;
					break;
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="payment"></param>
		/// <returns></returns>
		protected virtual bool MustPrintCheck(APQuickCheck quickCheck)
			{
			return APPaymentEntry.MustPrintCheck(quickCheck, paymenttype.Current);
		}

		protected virtual void APQuickCheck_PrintCheck_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (MustPrintCheck(e.Row as APQuickCheck))
			{
				sender.SetValueExt<APQuickCheck.extRefNbr>(e.Row, null);
			}
		}


		protected virtual void APQuickCheck_AdjDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APQuickCheck quickCheck = e.Row as APQuickCheck;

			if (quickCheck == null)
				return;

			if (quickCheck.VoidAppl != true)
			{
				if (vendor.Current != null && (bool)vendor.Current.Vendor1099)
				{
					string year1099 = ((DateTime)e.NewValue).Year.ToString();
					AP1099Year year = PXSelect<AP1099Year,
												Where<AP1099Year.finYear, Equal<Required<AP1099Year.finYear>>,
														And<AP1099Year.organizationID, Equal<Required<AP1099Year.organizationID>>>>>
												.Select(this, year1099, PXAccess.GetParentOrganizationID(quickCheck.BranchID));

					if (year != null && year.Status != "N")
					{
						throw new PXSetPropertyException(Messages.AP1099_PaymentDate_NotIn_OpenYear, PXUIFieldAttribute.GetDisplayName<APQuickCheck.adjDate>(sender));
					}
				}
			}
		}

		protected virtual void APQuickCheck_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APQuickCheck doc = (APQuickCheck)e.Row;

			if (doc.CashAccountID == null)
			{
				if (sender.RaiseExceptionHandling<APQuickCheck.cashAccountID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(APQuickCheck.cashAccountID)}]")))
				{
					throw new PXRowPersistingException(typeof(APQuickCheck.cashAccountID).Name, null, ErrorMessages.FieldIsEmpty, nameof(APQuickCheck.cashAccountID));
				}
			}

			if (string.IsNullOrEmpty(doc.PaymentMethodID))
			{
				if (sender.RaiseExceptionHandling<APQuickCheck.paymentMethodID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(APQuickCheck.paymentMethodID)}]")))
				{
					throw new PXRowPersistingException(typeof(APQuickCheck.paymentMethodID).Name, null, ErrorMessages.FieldIsEmpty, nameof(APQuickCheck.paymentMethodID));
				}
			}

			ValidateTaxConfiguration(sender, doc);

			Terms terms = (Terms)PXSelectorAttribute.Select<APQuickCheck.termsID>(Document.Cache, doc);

			if (terms == null)
			{
				sender.SetValue<APQuickCheck.termsID>(doc, null);
				return;
			}

			if (CM.PXCurrencyAttribute.IsNullOrEmpty(terms.DiscPercent) == false &&
				(EPEmployee)PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<APQuickCheck.vendorID>>>>.Select(this) != null)
			{
				sender.RaiseExceptionHandling<APQuickCheck.termsID>(doc, doc.TermsID, new PXSetPropertyException(Messages.Employee_Cannot_Have_Discounts, $"[{nameof(APQuickCheck.termsID)}]"));
			}

			if (terms.InstallmentType == TermsInstallmentType.Multiple)
			{
				sender.RaiseExceptionHandling<APQuickCheck.termsID>(doc, doc.TermsID, new PXSetPropertyException(Messages.Quick_Check_Cannot_Have_Multiply_Installments, $"[{nameof(APQuickCheck.termsID)}]"));
			}

			if (string.IsNullOrEmpty(doc.ExtRefNbr) && this.PaymentRefMustBeUnique && doc.Status == APDocStatus.Prebooked)
			{
				sender.RaiseExceptionHandling<APQuickCheck.extRefNbr>(doc, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty));
			}

			PaymentRefAttribute.SetUpdateCashManager<APQuickCheck.extRefNbr>(sender, e.Row, ((APQuickCheck)e.Row).DocType != APDocType.VoidQuickCheck && ((APQuickCheck)e.Row).DocType != APDocType.Refund);
		}

		private void ValidateTaxConfiguration(PXCache cache, APQuickCheck cashSale)
		{
			bool reduceOnEarlyPayments = false;
			bool reduceTaxableAmount = false;
			foreach (PXResult<APTax, Tax> result in PXSelectJoin<APTax,
				InnerJoin<Tax, On<Tax.taxID, Equal<APTax.taxID>>>,
				Where<APTax.tranType, Equal<Current<APQuickCheck.docType>>,
				And<APTax.refNbr, Equal<Current<APQuickCheck.refNbr>>>>>.Select(this))
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
					cache.RaiseExceptionHandling<APQuickCheck.taxZoneID>(cashSale, cashSale.TaxZoneID, new PXSetPropertyException(TX.Messages.InvalidTaxConfiguration));
				}
			}
		}

		protected bool InternalCall = false;

		protected virtual bool PaymentRefMustBeUnique => PaymentRefAttribute.PaymentRefMustBeUnique(paymenttype.Current);

	    private bool IsApprovalRequired(APQuickCheck doc)
	    {
	        return EPApprovalSettings<APSetupApproval>.ApprovedDocTypes.Contains(doc.DocType);
	    }

		protected virtual void APQuickCheck_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			APQuickCheck doc = e.Row as APQuickCheck;

			if (doc == null)
			{
				return;
			}
			this.release.SetEnabled(true);
			this.prebook.SetEnabled(true);
			this.reclassifyBatch.SetEnabled(true);

			bool dontApprove = !IsApprovalRequired(doc);
			if (doc.DontApprove != dontApprove)
			{
				cache.SetValueExt<APQuickCheck.dontApprove>(doc, dontApprove);
			}

			if (InternalCall) return;
			// We need this for correct tabs repainting
			// in migration mode.
			// 
			PaymentCharges.Cache.AllowSelect = true;

			bool docTypeNotDebitAdj = (doc.DocType != APDocType.DebitAdj);
			PXUIFieldAttribute.SetVisible<APQuickCheck.curyID>(cache, doc, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
			PXUIFieldAttribute.SetVisible<APQuickCheck.cashAccountID>(cache, doc, docTypeNotDebitAdj);
			PXUIFieldAttribute.SetVisible<APQuickCheck.cleared>(cache, doc, docTypeNotDebitAdj);
			PXUIFieldAttribute.SetVisible<APQuickCheck.clearDate>(cache, doc, docTypeNotDebitAdj);
			PXUIFieldAttribute.SetVisible<APQuickCheck.paymentMethodID>(cache, doc, docTypeNotDebitAdj);
			PXUIFieldAttribute.SetVisible<APQuickCheck.extRefNbr>(cache, doc, docTypeNotDebitAdj);


			PXUIFieldAttribute.SetEnabled(Transactions.Cache, null, true);

			//true for DebitAdj and Prepayment Requests
			bool docReleased = (doc.Released == true) || (doc.Prebooked == true);
			bool docOnHold = doc.Hold == true;
			const bool curyEnabled = false;
			bool clearEnabled = docOnHold && (cashaccount.Current != null) && (cashaccount.Current.Reconcile == true);

			PXUIFieldAttribute.SetRequired<APQuickCheck.cashAccountID>(cache, !docReleased);
			PXUIFieldAttribute.SetRequired<APQuickCheck.paymentMethodID>(cache, !docReleased);
			PXUIFieldAttribute.SetRequired<APQuickCheck.extRefNbr>(cache, !docReleased && PaymentRefMustBeUnique);
			
			PaymentRefAttribute.SetUpdateCashManager<APQuickCheck.extRefNbr>(cache, e.Row, doc.DocType != APDocType.VoidQuickCheck && doc.DocType != APDocType.Refund);

			bool isPrebookedNotCompleted = doc.Prebooked == true && doc.Released == false;
			bool docReallyPrinted = APPaymentEntry.IsCheckReallyPrinted(doc);

			if (doc.DocType == APDocType.VoidQuickCheck && !docReleased)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APQuickCheck.adjDate>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APQuickCheck.adjFinPeriodID>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APQuickCheck.docDesc>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APQuickCheck.hold>(cache, doc, true);

				cache.AllowUpdate = true;
				cache.AllowDelete = true;
				Transactions.Cache.AllowDelete = false;
				Transactions.Cache.AllowUpdate = false;
				Transactions.Cache.AllowInsert = false;
				
				Taxes.Cache.AllowUpdate = false;
			}
			else if (doc.Released == true || doc.Voided == true || doc.Prebooked== true)
			{
				bool Enable1099 = (vendor.Current != null && vendor.Current.Vendor1099 == true && doc.Voided == false);

				foreach (APAdjust adj in PXSelect<APAdjust,
							Where<APAdjust.adjgDocType, Equal<Required<APAdjust.adjgDocType>>,
								And<APAdjust.adjgRefNbr, Equal<Required<APAdjust.adjgRefNbr>>,
								And<APAdjust.released, Equal<True>>>>>
					.Select(this, doc.DocType, doc.RefNbr))
				{
					string year1099 = ((DateTime)adj.AdjgDocDate).Year.ToString();

					AP1099Year year = PXSelect<AP1099Year,
											Where<AP1099Year.finYear, Equal<Required<AP1099Year.finYear>>,
													And<AP1099Year.organizationID, Equal<Required<AP1099Year.organizationID>>>>>
											.Select(this, year1099, PXAccess.GetParentOrganizationID(adj.AdjgBranchID));

					if (year != null && year.Status != AP1099Year.status.Open)
					{
						Enable1099 = false;
					}
				}

				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				cache.AllowDelete = false;
				cache.AllowUpdate = Enable1099 || isPrebookedNotCompleted;
				Transactions.Cache.AllowDelete = false;
				Transactions.Cache.AllowUpdate = Enable1099 || isPrebookedNotCompleted;
				Transactions.Cache.AllowInsert = false;

				Remittance_Address.Cache.AllowUpdate = false;
				Remittance_Contact.Cache.AllowUpdate = false;

				if (Enable1099)
				{
					PXUIFieldAttribute.SetEnabled(Transactions.Cache, null, false);
					PXUIFieldAttribute.SetEnabled<APTran.box1099>(Transactions.Cache, null, true);
				}

				if (isPrebookedNotCompleted)
				{
					PXUIFieldAttribute.SetEnabled(Transactions.Cache, null, false);
					PXUIFieldAttribute.SetEnabled<APTran.accountID>(Transactions.Cache, null, true);
					PXUIFieldAttribute.SetEnabled<APTran.subID>(Transactions.Cache, null, true);
					PXUIFieldAttribute.SetEnabled<APTran.branchID>(Transactions.Cache, null, true);
				}
				Taxes.Cache.AllowUpdate = false;
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APQuickCheck.status>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<APQuickCheck.curyID>(cache, doc, curyEnabled);
				PXUIFieldAttribute.SetEnabled<APQuickCheck.printCheck>(
					cache,
					doc,
					!docReallyPrinted
						&& doc.DocType != APDocType.VoidQuickCheck
						&& doc.DocType != APDocType.Prepayment
						&& doc.DocType != APDocType.Refund
						&& doc.DocType != APDocType.DebitAdj);

				cache.AllowDelete = !docReallyPrinted;
				cache.AllowUpdate = true;
				Transactions.Cache.AllowDelete = true;
				Transactions.Cache.AllowUpdate = true;
				Transactions.Cache.AllowInsert = (doc.VendorID != null) && (doc.VendorLocationID != null);

				Remittance_Address.Cache.AllowUpdate = true;
				Remittance_Contact.Cache.AllowUpdate = true;
				
				Taxes.Cache.AllowUpdate = true;
			}
			bool accountEnable = !docReleased && (doc.DocType == APDocType.Prepayment || doc.DocType == APDocType.QuickCheck);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.cashAccountID>(cache, doc, !docReleased && !docReallyPrinted && doc.DocType != APDocType.VoidQuickCheck);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.paymentMethodID>(cache, doc, !docReleased && !docReallyPrinted && doc.DocType != APDocType.VoidQuickCheck);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.cleared>(cache, doc, clearEnabled);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.clearDate>(cache, doc, clearEnabled && doc.Cleared == true);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.docType>(cache, doc);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.refNbr>(cache, doc);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.batchNbr>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.aPAccountID>(cache, doc, accountEnable);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.aPSubID>(cache, doc, accountEnable);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.curyDocBal>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.curyLineTotal>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.curyTaxTotal>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.curyOrigWhTaxAmt>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.printed>(cache, doc, doc.DocType != APDocType.VoidQuickCheck);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.curyVatExemptTotal>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.curyVatTaxableTotal>(cache, doc, false);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.adjDate>(cache, doc, !docReleased && !docReallyPrinted);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.adjFinPeriodID>(cache, doc, !docReleased && !docReallyPrinted);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.vendorLocationID>(cache, doc, !docReleased && !docReallyPrinted && doc.DocType != APDocType.VoidQuickCheck);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.curyOrigDocAmt>(cache, doc, !docReleased && !docReallyPrinted && doc.DocType != APDocType.VoidQuickCheck);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.curyOrigDiscAmt>(cache, doc, !docReleased && !docReallyPrinted && doc.DocType != APDocType.VoidQuickCheck);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.branchID>(cache, doc, !docReleased && !docReallyPrinted && doc.DocType != APDocType.VoidQuickCheck);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.docDate>(cache, doc, !docReleased && !docReallyPrinted);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.finPeriodID>(cache, doc, !docReleased && !docReallyPrinted);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.taxZoneID>(cache, doc, !docReleased && !docReallyPrinted && doc.DocType != APDocType.VoidQuickCheck);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.termsID>(cache, doc, !docReleased && !docReallyPrinted && doc.DocType != APDocType.VoidQuickCheck);

			voidCheck.SetEnabled((doc.Released == true || doc.Prebooked == true) && doc.Voided == false && doc.DocType == APDocType.QuickCheck);

			if (doc.VendorID != null)
			{
				if (Transactions.Any())
				{
					PXUIFieldAttribute.SetEnabled<APQuickCheck.vendorID>(cache, doc, false);
				}
			}

			PXUIFieldAttribute.SetVisible<APTran.box1099>(Transactions.Cache, null, vendor.Current?.Vendor1099 == true);
			if (vendor.Current?.Vendor1099 != true)
			{
				PXUIFieldAttribute.SetEnabled<APTran.box1099>(Transactions.Cache, null, false);
			}

			this.validateAddresses.SetEnabled(!docReleased && FindAllImplementations<IAddressValidationHelper>().RequiresValidation());
			PXUIFieldAttribute.SetVisible<APQuickCheck.prebookBatchNbr>(cache, doc, doc.Prebooked == true);
			PXUIFieldAttribute.SetVisible<APQuickCheck.voidBatchNbr>(cache, doc, false); //Now void is implemented through VoidQuickCheck

			this.PaymentCharges.Cache.AllowInsert = ((doc.DocType == APDocType.QuickCheck || doc.DocType == APDocType.VoidQuickCheck) && doc.Released != true && doc.Prebooked != true);
			this.PaymentCharges.Cache.AllowUpdate = ((doc.DocType == APDocType.QuickCheck || doc.DocType == APDocType.VoidQuickCheck) && doc.Released != true && doc.Prebooked != true);
			this.PaymentCharges.Cache.AllowDelete = ((doc.DocType == APDocType.QuickCheck || doc.DocType == APDocType.VoidQuickCheck) && doc.Released != true && doc.Prebooked != true);

			Taxes.Cache.AllowDelete = Transactions.Cache.AllowDelete;
			Taxes.Cache.AllowInsert = Transactions.Cache.AllowInsert;

			PXUIFieldAttribute.SetVisible<APQuickCheck.curyTaxAmt>(cache, doc, PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>() &&
				(apsetup.Current.RequireControlTaxTotal == true || docReleased));

			bool showRoundingDiff = doc.CuryRoundDiff != 0 || PXAccess.FeatureInstalled<FeaturesSet.invoiceRounding>();
			PXUIFieldAttribute.SetVisible<APQuickCheck.curyRoundDiff>(cache, doc, showRoundingDiff);

			viewSchedule.SetEnabled(true);

			if (UseTaxes.Select().Count != 0)
			{
				cache.RaiseExceptionHandling<APQuickCheck.curyTaxTotal>(doc, doc.CuryTaxTotal, new PXSetPropertyException(TX.Messages.UseTaxExcludedFromTotals, PXErrorLevel.Warning));
			}

			PXUIFieldAttribute.SetVisible<APQuickCheck.printCheck>(
				cache,
				doc,
				paymenttype.Current?.PrintOrExport == true
					&& (doc.DocType != APDocType.VoidQuickCheck
						&& doc.DocType != APDocType.DebitAdj
						&& doc.DocType != APDocType.Prepayment
						&& doc.DocType != APDocType.Refund
						|| paymenttype.Current == null));
			PXUIFieldAttribute.SetEnabled<APQuickCheck.extRefNbr>(cache, doc, !(doc.PrintCheck ?? false));

			if (!_IsVoidCheckInProgress)
			{
				TaxAttribute.SetTaxCalc<APTran.taxCategoryID>(Transactions.Cache, null,
					Document.Current.Released == true ? TaxCalc.NoCalc : TaxCalc.Calc);
			}

			#region Migration Mode Settings

			bool isMigratedDocument = doc.IsMigratedRecord == true;
			bool isUnreleasedMigratedDocument = isMigratedDocument && doc.Released != true;

			if (isMigratedDocument)
			{
				cache.SetValue<APQuickCheck.printCheck>(doc, false);
				PXUIFieldAttribute.SetEnabled<APQuickCheck.printCheck>(cache, doc, false);
			}

			if (isUnreleasedMigratedDocument)
			{
				PaymentCharges.Cache.AllowSelect = false;
			}

			bool disableCaches = apsetup.Current?.MigrationMode == true
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
				if (doc.DocType == APDocType.QuickCheck)
				{
					if (doc.Status == APDocStatus.PendingApproval
						|| doc.Status == APDocStatus.Rejected
						|| doc.Status == APDocStatus.Closed
						|| doc.Status == APDocStatus.Printed
						|| doc.Status == APDocStatus.Voided
						|| doc.Status == APDocStatus.PendingPrint && doc.DontApprove != true && doc.Approved == true)
					{
						PXUIFieldAttribute.SetEnabled(cache, doc, false);

						Transactions.Cache.AllowInsert = false;
						Taxes.Cache.AllowInsert = false;
						Approval.Cache.AllowInsert = false;
						PaymentCharges.Cache.AllowInsert = false;

						Transactions.Cache.AllowUpdate = false;
						Taxes.Cache.AllowUpdate = false;
						Approval.Cache.AllowUpdate = false;
						PaymentCharges.Cache.AllowUpdate = false;
					}

		
				}
				if (doc.Released != true)
				{
					cache.AllowDelete = true;
				}
	
			}

			
			if (doc.Status == APDocStatus.PendingApproval
			    || doc.Status == APDocStatus.Rejected
			    || doc.Status == APDocStatus.Balanced
			    || doc.Status == APDocStatus.PendingPrint
			    || doc.Status == APDocStatus.Hold)
			{
				PXUIFieldAttribute.SetEnabled<APQuickCheck.hold>(cache, doc, true);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled<APQuickCheck.hold>(cache, doc, false);
			}
			PXUIFieldAttribute.SetEnabled<APQuickCheck.docType>(cache, doc, true);
			PXUIFieldAttribute.SetEnabled<APQuickCheck.refNbr>(cache, doc, true);

			if (doc.DocType == APDocType.VoidQuickCheck)
			{
				PXUIFieldAttribute.SetEnabled<APQuickCheck.extRefNbr>(cache, doc, false);
			}
			if (doc.Status == APDocStatus.Printed)
			{
				PXUIFieldAttribute.SetEnabled<APQuickCheck.extRefNbr>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<APQuickCheck.docDesc>(cache, doc, true);
			}

		}

		protected virtual void APQuickCheck_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			APQuickCheck doc = e.Row as APQuickCheck;
			if (doc.Released != true && doc.Prebooked !=true)
			{
				doc.DocDate = doc.AdjDate;
				doc.FinPeriodID = doc.AdjFinPeriodID;
				doc.TranPeriodID = doc.AdjTranPeriodID;

				sender.RaiseExceptionHandling<APQuickCheck.finPeriodID>(doc, doc.FinPeriodID, null);

				PaymentCharges.UpdateChangesFromPayment(sender, e);

				if (!sender.ObjectsEqual<APQuickCheck.curyDocBal, APQuickCheck.curyOrigDiscAmt, APQuickCheck.curyOrigWhTaxAmt>(e.Row, e.OldRow)
					&& doc.CuryDocBal - doc.CuryOrigDiscAmt - doc.CuryOrigWhTaxAmt != doc.CuryOrigDocAmt
					&& !APPaymentEntry.IsCheckReallyPrinted(doc))
				{
					if (doc.CuryDocBal != null && doc.CuryOrigDiscAmt != null && doc.CuryOrigWhTaxAmt != null && doc.CuryDocBal != 0)
						sender.SetValueExt<APQuickCheck.curyOrigDocAmt>(doc, doc.CuryDocBal - doc.CuryOrigDiscAmt - doc.CuryOrigWhTaxAmt);
					else
						sender.SetValueExt<APQuickCheck.curyOrigDocAmt>(doc, 0m);
				}
				else if (sender.ObjectsEqual<APQuickCheck.curyOrigDocAmt>(e.Row, e.OldRow) == false)
				{
					if (doc.CuryDocBal != null && doc.CuryOrigDocAmt != null && doc.CuryOrigWhTaxAmt != null && doc.CuryDocBal != 0)
						sender.SetValueExt<APQuickCheck.curyOrigDiscAmt>(doc, doc.CuryDocBal - doc.CuryOrigDocAmt - doc.CuryOrigWhTaxAmt);
					else
						sender.SetValueExt<APQuickCheck.curyOrigDiscAmt>(doc, 0m);
				}

				if (doc.Hold != true && doc.Released != true && doc.Prebooked != true)
				{
					decimal curyOrigDocTotal = (doc.CuryOrigDocAmt ?? 0m) + (doc.CuryOrigDiscAmt ?? 0m) + (doc.CuryOrigWhTaxAmt ?? 0m);

					if (APPaymentEntry.IsCheckReallyPrinted(doc) && doc.CuryDocBal != curyOrigDocTotal)
					{
						sender.RaiseExceptionHandling<APQuickCheck.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXSetPropertyException(Messages.PrintedQuickCheckOutOfBalance));
					}
					else if (doc.CuryDocBal < curyOrigDocTotal)
					{
						sender.RaiseExceptionHandling<APQuickCheck.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else if (curyOrigDocTotal < 0)
					{
						sender.RaiseExceptionHandling<APQuickCheck.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXSetPropertyException(Messages.DocumentBalanceNegative));
					}
					else if ((doc.CuryOrigDiscAmt ?? 0m) < 0)
					{
						sender.RaiseExceptionHandling<APQuickCheck.curyOrigDiscAmt>(doc, doc.CuryOrigDiscAmt, new PXSetPropertyException(Messages.DocumentOutOfBalance));						
					}
					else
					{
						sender.RaiseExceptionHandling<APQuickCheck.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, null);
					}
				}

				bool checkControlTaxTotal = apsetup.Current.RequireControlTaxTotal == true && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>();

				if ((doc.Hold != true || doc.Printed != true) && doc.Released != true && doc.Prebooked != true)
				{
					if (doc.CuryTaxTotal != doc.CuryTaxAmt && checkControlTaxTotal)
					{
						sender.RaiseExceptionHandling<APQuickCheck.curyTaxAmt>(doc, doc.CuryTaxAmt, new PXSetPropertyException(Messages.TaxTotalAmountDoesntMatch));
					}
					else
					{
						if (checkControlTaxTotal)
						{
							sender.RaiseExceptionHandling<APQuickCheck.curyTaxAmt>(doc, null, null);
						}
						else
						{
							sender.SetValueExt<APQuickCheck.curyTaxAmt>(doc, doc.CuryTaxTotal != null && doc.CuryTaxTotal != 0 ? doc.CuryTaxTotal : 0m);
						}
					}
				}

				sender.RaiseExceptionHandling<APQuickCheck.curyRoundDiff>(doc, null, null);

				if (doc.Hold != true && doc.RoundDiff != 0)
				{
					if (checkControlTaxTotal || PXAccess.FeatureInstalled<FeaturesSet.invoiceRounding>() && doc.TaxRoundDiff == 0)
					{
						if (Math.Abs(doc.RoundDiff.Value) > Math.Abs(CM.CurrencyCollection.GetCurrency(currencyinfo.Current.BaseCuryID).RoundingLimit.Value))
						{
							sender.RaiseExceptionHandling<APQuickCheck.curyRoundDiff>(doc, doc.CuryRoundDiff,
								new PXSetPropertyException(Messages.RoundingAmountTooBig, currencyinfo.Current.BaseCuryID, PXDBQuantityAttribute.Round(doc.RoundDiff),
									PXDBQuantityAttribute.Round(CM.CurrencyCollection.GetCurrency(currencyinfo.Current.BaseCuryID).RoundingLimit)));
						}
					}
					else
					{
						if (!PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>())
						{
							sender.RaiseExceptionHandling<APQuickCheck.curyRoundDiff>(doc, doc.CuryRoundDiff,
								new PXSetPropertyException(Messages.CannotEditTaxAmtWOFeature));
						}
						else
						{
							sender.RaiseExceptionHandling<APQuickCheck.curyRoundDiff>(doc, doc.CuryRoundDiff,
								new PXSetPropertyException(Messages.CannotEditTaxAmtWOAPSetup));
						}
					}
				}
			}
		}

		protected virtual void _(Events.RowDeleting<APQuickCheck> e)
		{
			if (e.Row.OrigModule == BatchModule.EP
			    && e.Row.OrigDocType == EPExpenseClaimDetails.DocType
				&& !(e.Row.DocType == APDocType.VoidQuickCheck && e.Row.Released == false))
			{
				throw new PXException(Messages.DocumentCannotBeDeleted);
			}
		}
		#endregion

		
		#region APPaymentChargeTran
		#region LineNbr
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXLineNbr(typeof(APQuickCheck.chargeCntr), DecrementOnDelete = false)]
		public virtual void APPaymentChargeTran_LineNbr_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region CashAccountID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDefault(typeof(APQuickCheck.cashAccountID))]
		public virtual void APPaymentChargeTran_CashAccountID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region EntryTypeID
		/// <summary>
		/// <see cref="APPaymentChargeTran.EntryTypeID"/> cache attached event.
		/// </summary>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(Search2<CAEntryType.entryTypeId,
							InnerJoin<CashAccountETDetail, On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>,
							Where<CashAccountETDetail.cashAccountID, Equal<Current<APQuickCheck.cashAccountID>>,
							And<CAEntryType.drCr, Equal<CADrCr.cACredit>>>>))]
		public virtual void APPaymentChargeTran_EntryTypeID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region TranDate
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDefault(typeof(APQuickCheck.adjDate))]
		public virtual void APPaymentChargeTran_TranDate_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region FinPeriodID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[FinPeriodID(
		    branchSourceType: typeof(APPaymentChargeTran.cashAccountID),
		    branchSourceFormulaType: typeof(Selector<APPaymentChargeTran.cashAccountID, CashAccount.branchID>),
		    masterFinPeriodIDType: typeof(APPaymentChargeTran.tranPeriodID),
		    headerMasterFinPeriodIDType: typeof(APQuickCheck.adjTranPeriodID))]
        public virtual void APPaymentChargeTran_FinPeriodID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region EP Approval Defaulting
		[PXDBDate]
		[PXDefault(typeof(APQuickCheck.docDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt]
		[PXDefault(typeof(APQuickCheck.vendorID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(APQuickCheck.docDesc), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong]
		[CurrencyInfo(typeof(APQuickCheck.curyInfoID))]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(APQuickCheck.curyOrigDocAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(APQuickCheck.origDocAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}

		protected virtual void EPApproval_SourceItemType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = new APDocType.ListAttribute()
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

		#endregion

		#region APTran Events
		protected virtual void APTran_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APTran apTran = (APTran)e.Row;
			if (vendor.Current == null || apTran == null ) return;

			// We should allow entering an AccountID for stock inventory
			// item when migration mode is activated in AP module.
			// 
			if (apsetup.Current?.MigrationMode != true &&
				apTran.InventoryID != null)
			{
				InventoryItem item = nonStockItem.Select(apTran.InventoryID);
				if (item?.StkItem == true)
				{
					e.NewValue = null;
					e.Cancel = true;
					return;
				}
			}

			if ((apTran.InventoryID == null
					&& (vendor.Current.Type == BAccountType.VendorType
						|| vendor.Current.Type == BAccountType.CombinedType)
					&& location.Current?.VExpenseAcctID != null)
				|| (apTran.InventoryID != null
					&& vendor.Current.IsBranch == true
					&& apTran.AccrueCost != true
					&& apsetup.Current?.IntercompanyExpenseAccountDefault == APAcctSubDefault.MaskLocation))
			{
				e.NewValue = location.Current.VExpenseAcctID;
				e.Cancel = true;
			}
			else if (apTran.InventoryID == null && vendor.Current.Type == BAccountType.EmployeeType)
			{
				EPEmployee employeeVendor = EmployeeByVendor.Select();
				e.NewValue = employeeVendor.ExpenseAcctID ?? e.NewValue;
				e.Cancel = true;
			}
		}

		protected virtual void APTran_AccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (vendor.Current != null && (bool)vendor.Current.Vendor1099)
			{
				sender.SetDefaultExt<APTran.box1099>(e.Row);
			}

			sender.SetDefaultExt<APTran.projectID>(e.Row);
		}

		protected virtual void APTran_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APTran row = (APTran)e.Row;
			if (row == null || vendor.Current == null || vendor.Current.Type == null) return;
			if (!String.IsNullOrEmpty(row.PONbr) || !String.IsNullOrEmpty(row.ReceiptNbr)) return;
			InventoryItem item = nonStockItem.Select(row.InventoryID);
			EPEmployee employeeByUser = (EPEmployee)PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(this, PXAccess.GetUserID());
			CRLocation companyloc =
				PXSelectJoin<CRLocation, InnerJoin<BAccountR, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<Branch, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>>>, Where<Branch.branchID, Equal<Required<APTran.branchID>>>>.Select(this, row.BranchID);
			CT.Contract project = PXSelect<CT.Contract, Where<CT.Contract.contractID, Equal<Required<CT.Contract.contractID>>>>.Select(this, row.ProjectID);
			string expenseSubMask = apsetup.Current.ExpenseSubMask;
			int? projectTask_SubID = null;
			if (project == null || project.BaseType == CT.CTPRType.Contract)
			{
				project = PXSelect<CT.Contract, Where<CT.Contract.nonProject, Equal<True>>>.Select(this);
				expenseSubMask = expenseSubMask.Replace(APAcctSubDefault.MaskTask, APAcctSubDefault.MaskProject);
			}
			else
			{
				PM.PMTask task = PXSelect<PM.PMTask, Where<PM.PMTask.projectID, Equal<Required< PM.PMTask.projectID>>, And<PM.PMTask.taskID, Equal<Required<PM.PMTask.taskID>>>>>.Select(this, row.ProjectID, row.TaskID);
				if (task != null)
					projectTask_SubID = task.DefaultExpenseSubID;
			}
			int? vendor_SubID = null;
			switch (vendor.Current.Type)
			{
				case BAccountType.VendorType:
				case BAccountType.CombinedType:
					if (location.Current?.VExpenseSubID != null)
					{
						vendor_SubID = (int?)Caches[typeof(Location)].GetValue<Location.vExpenseSubID>(location.Current);
					}
					break;
				case BAccountType.EmployeeType:
					vendor_SubID = EmployeeByVendor.SelectSingle()?.ExpenseSubID ?? vendor_SubID;
					break;
			}


			int? item_SubID = (int?)Caches[typeof(InventoryItem)].GetValue<InventoryItem.cOGSSubID>(item);
			int? employeeByUser_SubID = (int?)Caches[typeof(EPEmployee)].GetValue<EPEmployee.expenseSubID>(employeeByUser);
			int? company_SubID = (int?)Caches[typeof(CRLocation)].GetValue<CRLocation.cMPExpenseSubID>(companyloc);
			int? project_SubID = project.DefaultExpenseSubID;

			object value = SubAccountMaskAttribute.MakeSub<APSetup.expenseSubMask>(this, apsetup.Current.ExpenseSubMask,
				new object[] { vendor_SubID, item_SubID, employeeByUser_SubID, company_SubID, project_SubID, projectTask_SubID },
								new Type[] { typeof(Location.vExpenseSubID), typeof(InventoryItem.cOGSSubID), typeof(EPEmployee.expenseSubID), typeof(Location.cMPExpenseSubID), typeof(PM.PMProject.defaultExpenseSubID), typeof(PM.PMTask.defaultExpenseSubID) });

			if (value != null)
			{
				sender.RaiseFieldUpdating<APTran.subID>(row, ref value);
			}
			else
			{
				value = row.SubID;
			}

			e.NewValue = (int?)value;
			e.Cancel = true;

		}

		[PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[APQuickCheckTax(typeof(APQuickCheck), typeof(APTax), typeof(APTaxTran), typeof(APQuickCheck.taxCalcMode), typeof(APQuickCheck.branchID),
			   //Per Unit Tax settings
			   Inventory = typeof(APTran.inventoryID), UOM = typeof(APTran.uOM), LineQty = typeof(APTran.qty))]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		[PXDefault(typeof(Search<InventoryItem.taxCategoryID,
			Where<InventoryItem.inventoryID, Equal<Current<APTran.inventoryID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void APTran_TaxCategoryID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void APTran_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APTran row = (APTran)e.Row;
			if (row == null || row.InventoryID != null || vendor == null || vendor.Current == null || vendor.Current.TaxAgency == true) return;

			if (TaxAttribute.GetTaxCalc<APTran.taxCategoryID>(sender, row) == TaxCalc.Calc &&
			 taxzone.Current != null &&
			 !string.IsNullOrEmpty(taxzone.Current.DfltTaxCategoryID))
			{
				e.NewValue = taxzone.Current.DfltTaxCategoryID;
				e.Cancel = true;
			}
		}

		protected virtual void APTran_UnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APTran row = (APTran)e.Row;
			if (row == null || row.InventoryID != null) return;
			e.NewValue = 0m;
			e.Cancel = true;
		}

		protected virtual void APTran_CuryUnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			APTran tran = (APTran)e.Row;

			if (tran == null) return;

			if (!CM.PXCurrencyAttribute.IsNullOrEmpty(tran.UnitCost))
			{
				decimal CuryUnitCost = GetExtension<MultiCurrency>().GetDefaultCurrencyInfo().CuryConvCury(tran.UnitCost.Value);
				e.NewValue = INUnitAttribute.ConvertToBase<APTran.inventoryID>(sender, tran, tran.UOM, CuryUnitCost, INPrecision.UNITCOST);
				e.Cancel = true;
			}

			APQuickCheck doc = this.Document.Current;

			if (doc != null && doc.VendorID != null && tran != null && tran.InventoryID != null && tran.UOM != null)
			{
				if (tran.ManualPrice != true || tran.CuryUnitCost == null)
				{
					decimal? vendorUnitCost = null;

					if (tran.InventoryID != null && tran.UOM != null)
					{
						DateTime date = Document.Current.DocDate.Value;

						vendorUnitCost = APVendorPriceMaint.CalculateUnitCost(
							sender,
							tran.VendorID,
							doc.VendorLocationID, 
							tran.InventoryID,
							tran.SiteID, 
							currencyinfo.SelectSingle().GetCM(),
							tran.UOM,
							tran.Qty, 
							date, 
							tran.CuryUnitCost
							);

						e.NewValue = vendorUnitCost;
					}

					if (vendorUnitCost == null)
					{
						e.NewValue = POItemCostManager.Fetch<APTran.inventoryID, APTran.curyInfoID>(sender.Graph, tran,
							doc.VendorID, doc.VendorLocationID, doc.DocDate, doc.CuryID, tran.InventoryID, null, null, tran.UOM);
					}

					APVendorPriceMaint.CheckNewUnitCost<APTran, APTran.curyUnitCost>(sender, tran, e.NewValue);
				}
				else
				{
					e.NewValue = tran.CuryUnitCost ?? 0m;
				}

					e.Cancel = true;
				}
			}

		protected virtual void APTran_ManualPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row != null)
				sender.SetDefaultExt<APTran.curyUnitCost>(e.Row);
		}

		protected virtual void APTran_Qty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row != null && row.Qty != 0)
			{
				sender.SetDefaultExt<APTran.curyUnitCost>(e.Row);
			}
		}

		protected virtual void APTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran tran = (APTran)e.Row;
			sender.SetDefaultExt<APTran.unitCost>(tran);
			sender.SetDefaultExt<APTran.curyUnitCost>(tran);
			sender.SetValue<APTran.unitCost>(tran, null);
		}

		protected virtual void APTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran tran = e.Row as APTran;
			if (tran != null)
			{
				if (String.IsNullOrEmpty(tran.ReceiptNbr) && string.IsNullOrEmpty(tran.PONbr))
				{
					sender.SetDefaultExt<APTran.accountID>(tran);
					sender.SetDefaultExt<APTran.subID>(tran);
					sender.SetDefaultExt<APTran.taxCategoryID>(tran);
					sender.SetDefaultExt<APTran.deferredCode>(tran);
					sender.SetDefaultExt<APTran.uOM>(tran);

					sender.SetDefaultExt<APTran.unitCost>(tran);
					sender.SetDefaultExt<APTran.curyUnitCost>(tran);
					sender.SetValue<APTran.unitCost>(tran, null);

					InventoryItem item = nonStockItem.Select(tran.InventoryID);
					if (item != null)
					{
						tran.TranDesc = PXDBLocalizableStringAttribute.GetTranslation(Caches[typeof(InventoryItem)], item, "Descr", vendor.Current?.LocaleName);
					}
				}
			}
		}

		[ProjectDefault(BatchModule.AP, typeof(Search<Location.vDefProjectID,
															Where<Location.bAccountID, Equal<Current<APQuickCheck.vendorID>>,
																	And<Location.locationID, Equal<Current<APQuickCheck.vendorLocationID>>>>>),
			typeof(APTran.accountID))]
		[APActiveProject]
		protected virtual void APTran_ProjectID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void APTran_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row == null) return;

			sender.SetDefaultExt<APTran.subID>(row);
		}

	    [FinPeriodID(
	        branchSourceType: typeof(APTran.branchID),
	        masterFinPeriodIDType: typeof(APTran.tranPeriodID),
	        headerMasterFinPeriodIDType: typeof(APQuickCheck.adjTranPeriodID))]
        protected virtual void APTran_FinPeriodID_CacheAttached(PXCache sender)
	    {
	    }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noPurchases>>), PX.Objects.IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
		protected virtual void APTran_InventoryID_CacheAttached(PXCache sender)
		{
		}

		[PXBool]
		[DR.DRTerms.Dates(typeof(APTran.dRTermStartDate), typeof(APTran.dRTermEndDate), typeof(APTran.inventoryID), typeof(APTran.deferredCode), typeof(APQuickCheck.hold))]
		protected virtual void APTran_RequiresTerms_CacheAttached(PXCache sender) { }

		protected virtual void APTran_TaskID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row == null) return;

			sender.SetDefaultExt<APTran.subID>(row);
		}


		protected virtual void APTran_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			APTran row = e.Row as APTran;
			if (row == null) return;

			APQuickCheck doc = Document.Current;

			bool is1099Enabled = vendor.Current?.Vendor1099 == true && doc != null && doc.Voided != true;
			bool isDocumentReleased = doc?.Released == true;

			// rest value to enable state  
			PXUIFieldAttribute.SetEnabled<APTran.deferredCode>(cache, row, true);

			if (!(String.IsNullOrEmpty(row.PONbr) && String.IsNullOrEmpty(row.ReceiptNbr)))
			{
				PXUIFieldAttribute.SetEnabled<APTran.inventoryID>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<APTran.uOM>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<APTran.accountID>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<APTran.subID>(cache, row, false);
			}

			bool isProjectEditable = string.IsNullOrEmpty(row.PONbr);

			InventoryItem ns = (InventoryItem)PXSelectorAttribute.Select(cache, e.Row, cache.GetField(typeof(APTran.inventoryID)));
			if (ns != null && ns.StkItem != true && ns.NonStockReceipt != true)
			{
				isProjectEditable = true;
			}

			isProjectEditable = isProjectEditable && (!isDocumentReleased || !is1099Enabled);

			PXUIFieldAttribute.SetEnabled<APTran.projectID>(cache, row, isProjectEditable);
			PXUIFieldAttribute.SetEnabled<APTran.taskID>(cache, row, isProjectEditable);

			#region Direct Tax Configs

			if (row.IsDirectTaxLine == true)
			{
				PXUIFieldAttribute.SetEnabled<APTran.deferredCode>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<APTran.dRTermStartDate>(cache, row, false);
				PXUIFieldAttribute.SetEnabled<APTran.dRTermEndDate>(cache, row, false);
			}
			#endregion
			#region Migration Mode Settings

			if (doc != null &&
				doc.IsMigratedRecord == true &&
				doc.Released != true)
			{
				PXUIFieldAttribute.SetEnabled<APTran.defScheduleID>(cache, null, false);
				PXUIFieldAttribute.SetEnabled<APTran.deferredCode>(cache, null, false);
				PXUIFieldAttribute.SetEnabled<APTran.dRTermStartDate>(cache, null, false);
				PXUIFieldAttribute.SetEnabled<APTran.dRTermEndDate>(cache, null, false);
			}
			#endregion
		}

		protected virtual void APTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<APTran.box1099>(e.Row, e.OldRow))
			{
				foreach (APAdjust adj in PXSelect<APAdjust, Where<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>, And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>, And<APAdjust.released, Equal<True>>>>>.Select(this, ((APTran)e.Row).TranType, ((APTran)e.Row).RefNbr))
				{
					APReleaseProcess.Update1099Hist(this, -1m, adj, (APTran)e.OldRow, Document.Current);
					APReleaseProcess.Update1099Hist(this, 1m, adj, (APTran)e.Row, Document.Current);
				}

				if(Document.Current.Released == true && Document.Current.OrigDocAmt == 0)
				{
					APReleaseProcess.Update1099Hist(this, -1m, (APTran)e.OldRow, Document.Current, Document.Current.DocDate, Document.Current.BranchID, 0);
					APReleaseProcess.Update1099Hist(this, 1m, (APTran)e.Row, Document.Current, Document.Current.DocDate, Document.Current.BranchID, 0);
				}
			}

			APTran row = e.Row as APTran;
			if (row != null)
			{
				if ((e.ExternalCall || sender.Graph.IsImport)
				&& sender.ObjectsEqual<APTran.inventoryID>(e.Row, e.OldRow) && sender.ObjectsEqual<APTran.uOM>(e.Row, e.OldRow)
				&& sender.ObjectsEqual<APTran.qty>(e.Row, e.OldRow) && sender.ObjectsEqual<APTran.branchID>(e.Row, e.OldRow)
				&& sender.ObjectsEqual<APTran.siteID>(e.Row, e.OldRow) && sender.ObjectsEqual<APTran.manualPrice>(e.Row, e.OldRow)
				&& (!sender.ObjectsEqual<APTran.curyUnitCost>(e.Row, e.OldRow) || !sender.ObjectsEqual<APTran.curyLineAmt>(e.Row, e.OldRow)))
					row.ManualPrice = true;
			}
		}

		protected virtual void APTran_Box1099_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (vendor.Current == null || vendor.Current.Vendor1099 == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void APTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row == null) return;

			DR.ScheduleHelper.DeleteAssociatedScheduleIfDeferralCodeChanged(this, e.Row as APTran);
		}

		protected virtual void AP1099Hist_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (((AP1099Hist)e.Row).BoxNbr == null)
			{
				e.Cancel = true;
			}
		}

		protected virtual void APTran_DrCr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = APInvoiceType.DrCr(Document.Current.DocType);
				e.Cancel = true;
			}
		}
		#endregion

		#region APTaxTran Events
		protected virtual void APTaxTran_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null)
			{
				e.NewValue = Document.Current.TaxZoneID;
				e.Cancel = true;
			}
		}

		protected virtual void APTaxTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (!(e.Row is APTaxTran apTaxTran))
				return;

			PXUIFieldAttribute.SetEnabled<APTaxTran.taxID>(sender, e.Row, sender.GetStatus(e.Row) == PXEntryStatus.Inserted);
		}

		protected virtual void APTaxTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (Document.Current != null && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update))
			{
				((APTaxTran)e.Row).TaxZoneID = Document.Current.TaxZoneID;
			}
		}

		[Branch(typeof(APRegister.branchID), Enabled = false)]
		protected virtual void APTaxTran_BranchID_CacheAttached(PXCache sender)
		{
		}

	    [FinPeriodID(branchSourceType: typeof(APTaxTran.branchID),
	        headerMasterFinPeriodIDType: typeof(APQuickCheck.adjTranPeriodID))]
	    [PXDefault]
        protected virtual void APTaxTran_FinPeriodID_CacheAttached(PXCache sender)
	    {
	    }

		protected virtual void _(Events.FieldUpdated<APTaxTran, APTaxTran.taxID> e)
		{
			if (!(e.Row is APTaxTran apTaxTran))
				return;

			if (e.OldValue != null && e.OldValue != e.NewValue)
			{
				Taxes.Cache.SetDefaultExt<APTaxTran.accountID>(apTaxTran);
				Taxes.Cache.SetDefaultExt<APTaxTran.taxType>(apTaxTran);
				Taxes.Cache.SetDefaultExt<APTaxTran.taxBucketID>(apTaxTran);
			}
		}
        #endregion

        #region Voiding
        private bool _IsVoidCheckInProgress = false;

		protected virtual void APQuickCheck_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		protected virtual void APQuickCheck_FinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		protected virtual void APQuickCheck_AdjFinPeriodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		protected virtual void APQuickCheck_EmployeeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_IsVoidCheckInProgress)
			{
				e.Cancel = true;
			}
		}

		public virtual void VoidCheckProc(APQuickCheck doc)
		{
			Clear(PXClearOption.PreserveTimeStamp);
			Document.View.Answer = WebDialogResult.No;

			TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID>(Transactions.Cache, null, TaxCalc.NoCalc);

			foreach (PXResult<APQuickCheck, CurrencyInfo> res in PXSelectJoin<APQuickCheck,
				InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APQuickCheck.curyInfoID>>>,
				Where<APQuickCheck.docType, Equal<Required<APQuickCheck.docType>>,
					And<APQuickCheck.refNbr, Equal<Required<APQuickCheck.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				doc = res;

				CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy(res);
				info.CuryInfoID = null;
				info.IsReadOnly = false;
				info = PXCache<CurrencyInfo>.CreateCopy(currencyinfo.Insert(info));

				APQuickCheck payment = new APQuickCheck
				{
					DocType = APDocType.VoidQuickCheck,
					RefNbr = doc.RefNbr,
					CuryInfoID = info.CuryInfoID
				};
				Document.Insert(payment);

				payment = PXCache<APQuickCheck>.CreateCopy(doc);

				payment.DocType = APDocType.VoidQuickCheck;
				payment.CuryInfoID = info.CuryInfoID;
				payment.CATranID = null;
				payment.NoteID = null;

				//must set for _RowSelected
				payment.OpenDoc = true;
				payment.Released = false;
				if(doc.Released == true)
				{
					payment.PrebookBatchNbr = null;
				}
				payment.Prebooked = false; //Temporary, Check Later.  Rigth now only the Prebooked & Released Quick Checks may be voided
				Document.Cache.SetDefaultExt<APQuickCheck.hold>(payment);
				Document.Cache.SetDefaultExt<APQuickCheck.isMigratedRecord>(payment);
				Document.Cache.SetDefaultExt<APQuickCheck.status>(payment);
				payment.LineCntr = 0;
				payment.AdjCntr = 0;
				payment.BatchNbr = null;
				payment.AdjDate = doc.DocDate;
			    payment.AdjFinPeriodID = doc.AdjFinPeriodID;
			    payment.AdjTranPeriodID = doc.AdjTranPeriodID;
                payment.CuryDocBal = payment.CuryOrigDocAmt + payment.CuryOrigDiscAmt;
				payment.CuryLineTotal = 0m;
				payment.CuryTaxTotal = 0m;
				payment.CuryOrigWhTaxAmt = 0m;
				payment.CuryChargeAmt = 0;
				payment.CuryVatExemptTotal = 0;
				payment.CuryVatTaxableTotal = 0;
				payment.ClosedDate = null;
				payment.ClosedFinPeriodID = null;
				payment.ClosedTranPeriodID = null;
				payment.Printed	= true;
				payment.PrintCheck = false; //We don't need print voided quick checks
				payment.CashAccountID = null;

				Document.Cache.SetDefaultExt<APQuickCheck.employeeID>(payment);
				Document.Cache.SetDefaultExt<APQuickCheck.employeeWorkgroupID>(payment);

				if (payment.Cleared == true)
				{
					payment.ClearDate = payment.DocDate;
				}
				else
				{
					payment.ClearDate = null;
				}

				Document.Update(payment);

				payment.CashAccountID = doc.CashAccountID;
				Document.Update(payment);

				using (new PX.SM.SuppressWorkflowAutoPersistScope(this))
				{
				initializeState.Press();
				}

				if (info != null)
				{
					CurrencyInfo b_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<APQuickCheck.curyInfoID>>>>.Select(this, null);
					b_info.CuryID = info.CuryID;
					b_info.CuryEffDate = info.CuryEffDate;
					b_info.CuryRateTypeID = info.CuryRateTypeID;
					b_info.CuryRate = info.CuryRate;
					b_info.RecipRate = info.RecipRate;
					b_info.CuryMultDiv = info.CuryMultDiv;
					currencyinfo.Update(b_info);
				}
			}

			TaxAttribute.SetTaxCalc<APTran.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualCalc);

			foreach (APTran srcTran in PXSelect<APTran, Where<APTran.tranType, Equal<Required<APTran.tranType>>, And<APTran.refNbr, Equal<Required<APTran.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				//TODO Create new APTran and explicitly fill the required fields
				APTran tran = PXCache<APTran>.CreateCopy(srcTran);
				tran.TranType = null;
				tran.RefNbr = null;
			    tran.TranID = null;
				tran.DrCr = null;
				tran.Released = null;
				tran.CuryInfoID = null;
				tran.NoteID = null;

				InventoryItem item = InventoryItem.PK.Find(this, tran.InventoryID);
				if (item?.IsConverted == true && tran.IsStockItem != null && tran.IsStockItem != item.StkItem)
				{
					if (item.StkItem == true)
						tran.InventoryID = null;

					tran.IsStockItem = null;
				}

				tran = Transactions.Insert(tran);
				PXNoteAttribute.CopyNoteAndFiles(Transactions.Cache, srcTran, Transactions.Cache, tran);

				if (srcTran.Box1099 == null)
				{
					tran.Box1099 = null;
				}
			}

			List<APTaxTran> taxUpdates = new List<APTaxTran>();
			foreach (APTaxTran tax in PXSelect<APTaxTran, Where<APTaxTran.tranType, Equal<Required<APTaxTran.tranType>>, And<APTaxTran.refNbr, Equal<Required<APTaxTran.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				APTaxTran new_tax = this.Taxes.Insert(new APTaxTran() { TaxID = tax.TaxID });

				if (new_tax != null)
				{
					new_tax = PXCache<APTaxTran>.CreateCopy(new_tax);
					new_tax.TaxRate = tax.TaxRate;
					new_tax.CuryTaxableAmt = tax.CuryTaxableAmt;
					new_tax.CuryTaxAmt = tax.CuryTaxAmt;
					new_tax.CuryTaxAmtSumm = tax.CuryTaxAmtSumm;
					new_tax.CuryTaxDiscountAmt = tax.CuryTaxDiscountAmt;
					new_tax.CuryTaxableDiscountAmt = tax.CuryTaxableDiscountAmt;
					taxUpdates.Add(new_tax);
				}
				}
			foreach (var tax in taxUpdates)
			{
				this.Taxes.Update(tax);
			}

			// reassgn values from original aptax values to avoid if there are calculation decrepencies when voiding
			foreach (APTax srcAPTax in PXSelect<APTax, Where<APTax.tranType, Equal<Required<APTax.tranType>>, And<APTax.refNbr, Equal<Required<APTax.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr))
			{
				APTax voidedAPTax = this.Tax_Rows.Cache.Locate(new APTax
				{
					TranType = APDocType.VoidQuickCheck,
					RefNbr = doc.RefNbr,
					LineNbr = srcAPTax.LineNbr,
					TaxID = srcAPTax.TaxID
				}) as APTax;

				if (voidedAPTax != null)
				{
					this.Tax_Rows.Cache.SetValueExt<APTax.taxRate>(voidedAPTax, srcAPTax.TaxRate);
					this.Tax_Rows.Cache.SetValueExt<APTax.curyTaxableAmt>(voidedAPTax, srcAPTax.CuryTaxableAmt);
					this.Tax_Rows.Cache.SetValueExt<APTax.curyTaxAmt>(voidedAPTax, srcAPTax.CuryTaxAmt);
					this.Tax_Rows.Cache.SetValueExt<APTax.curyExpenseAmt>(voidedAPTax, srcAPTax.CuryExpenseAmt);
				}				
			}

			APQuickCheck newdocument = Document.Current;
			newdocument.CuryOrigDiscAmt = doc.CuryOrigDiscAmt;
			Document.Update(newdocument);
			PaymentCharges.ReverseCharges(doc, Document.Current);
		}
		#endregion

		#region Address Lookup Extension
		/// <exclude/>
		public class APQuickCheckEntryAddressLookupExtension : CR.Extensions.AddressLookupExtension<APQuickCheckEntry, APQuickCheck, APAddress>
		{
			protected override string AddressView => nameof(Base.Remittance_Address);
		}

		public class APQuickCheckEntryAddressCachingHelper : AddressValidationExtension<APQuickCheckEntry, APAddress>
		{
			protected override IEnumerable<PXSelectBase<APAddress>> AddressSelects()
			{
				yield return Base.Remittance_Address;
			}
		}
		#endregion
	}
}
