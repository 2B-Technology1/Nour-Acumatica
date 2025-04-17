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

using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using PX.Objects.CM.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.TX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Util;
using static PX.Objects.AR.ARInvoiceEntry;

namespace PX.Objects.AR
{
	using static BoundedTo<ARInvoiceEntry, ARInvoice>;

	[Serializable]
	public class ARInvoiceEntryRetainage : PXGraphExtension<ARInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.retainage>();
		}

		public override void Initialize()
		{
			base.Initialize();

			RetainageOptions releaseRetainageOptions = ReleaseRetainageOptions.Current;

			PXAction action = Base.Actions["action"];
			if (action != null)
			{
				action.AddMenuAction(releaseRetainage);
			}
		}

		#region Cache Attached Events

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[ARRetainedTax(typeof(ARInvoice), typeof(ARTax), typeof(ARTaxTran))]
		protected virtual void ARTran_TaxCategoryID_CacheAttached(PXCache sender) { }

		[DBRetainagePercent(
			typeof(ARInvoice.retainageApply),
			typeof(ARInvoice.defRetainagePct),
			typeof(Sub<Current<ARTran.curyExtPrice>, Current<ARTran.curyDiscAmt>>),
			typeof(ARTran.curyRetainageAmt),
			typeof(ARTran.retainagePct))]
		protected virtual void ARTran_RetainagePct_CacheAttached(PXCache sender) { }

		[DBRetainageAmount(
			typeof(ARTran.curyInfoID),
			typeof(Sub<ARTran.curyExtPrice, ARTran.curyDiscAmt>),
			typeof(ARTran.curyRetainageAmt),
			typeof(ARTran.retainageAmt),
			typeof(ARTran.retainagePct))]
		protected virtual void ARTran_CuryRetainageAmt_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(
			IIf<Where<ARInvoice.projectID, NotEqual<NonProject>,
					And<Selector<ARInvoice.projectID, PMProject.baseType>, Equal<PMProject.ProjectBaseType>>>,
				Selector<ARInvoice.projectID, PMProject.retainagePct>,
				Selector<ARRegister.customerID, Customer.retainagePct>>
			))]
		protected virtual void ARInvoice_DefRetainagePct_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(ARRegister.curyRetainageTotal))]
		protected virtual void ARInvoice_CuryRetainageUnreleasedAmt_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Switch<Case<Where2<
			FeatureInstalled<FeaturesSet.retainage>,
				And<ARRegister.retainageApply, Equal<True>,
				And<ARRegister.released, NotEqual<True>>>>,
			ARRegister.curyRetainageTotal>,
			ARRegister.curyRetainageUnpaidTotal>))]
		protected virtual void ARInvoice_CuryRetainageUnpaidTotal_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(
			IIf<Where2<FeatureInstalled<FeaturesSet.retainage>,
					And2<Where<ARRegister.docType, Equal<ARInvoiceType.invoice>,
						Or<ARRegister.docType, Equal<ARInvoiceType.creditMemo>>>,
					And<ARRegister.origModule, Equal<BatchModule.moduleAR>,
					And<Current<ARSetup.migrationMode>, Equal<False>>>>>,
				IsNull<Selector<ARRegister.customerID, Customer.retainageApply>, False>,
				False>))]
		[PXUIVerify(
			typeof(Where<
			ARRegister.retainageApply, NotEqual<True>,
			And<ARRegister.isRetainageDocument, NotEqual<True>,
				Or<Selector<ARInvoice.termsID, Terms.installmentType>, NotEqual<TermsInstallmentType.multiple>>>>),
			PXErrorLevel.Error,
			AP.Messages.RetainageWithMultipleCreditTerms)]
		[PXUIVerify(
			typeof(Where<ARRegister.retainageApply, NotEqual<True>,
				Or<ARRegister.curyID, Equal<Current<CurrencyInfo.baseCuryID>>>>),
			PXErrorLevel.Error,
			AP.Messages.RetainageDocumentNotInBaseCurrency)]
		protected virtual void ARInvoice_RetainageApply_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIVerify(
			typeof(Where<ARRegister.curyRetainageTotal, GreaterEqual<decimal0>, And<ARRegister.hold, NotEqual<True>,
				Or<ARRegister.hold, Equal<True>>>>),
			PXErrorLevel.Error,
			AP.Messages.IncorrectRetainageTotalAmount)]
		protected virtual void ARInvoice_CuryRetainageTotal_CacheAttached(PXCache sender) { }

		#endregion

		#region APInvoice Events

		protected virtual void ARInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ARInvoice doc = e.Row as ARInvoice;
			if (doc == null) return;

			releaseRetainage.SetEnabled(
				doc.Released == true &&
				doc.RetainageApply == true &&
				!doc.HasZeroBalance<ARRegister.curyRetainageUnreleasedAmt, ARTran.curyRetainageBal>(cache.Graph));

			RetainageDocuments.Cache.AllowUpdate = false;
			RetainageDocuments.Cache.AllowInsert = false;
			RetainageDocuments.Cache.AllowDelete = false;
		}

		protected virtual void ARInvoice_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
		}

		protected virtual void ARInvoice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARInvoice doc = (ARInvoice)e.Row;

			Terms terms = (Terms)PXSelectorAttribute.Select<ARInvoice.termsID>(Base.Document.Cache, doc);

			if (terms != null && doc.RetainageApply == true && terms.InstallmentType == CS.TermsInstallmentType.Multiple)
			{
				sender.RaiseExceptionHandling<ARInvoice.termsID>(doc, doc.TermsID, new PXSetPropertyException(AP.Messages.RetainageWithMultipleCreditTerms));
			}

			bool disablePersistingCheckForRetainageAccountAndSub = doc.RetainageApply != true;
			PXDefaultAttribute.SetPersistingCheck<ARRegister.retainageAcctID>(sender, doc, disablePersistingCheckForRetainageAccountAndSub
				? PXPersistingCheck.Nothing
				: PXPersistingCheck.NullOrBlank);
			PXDefaultAttribute.SetPersistingCheck<ARInvoice.retainageSubID>(sender, doc, disablePersistingCheckForRetainageAccountAndSub
				? PXPersistingCheck.Nothing
				: PXPersistingCheck.NullOrBlank);
		}

		protected virtual void ARInvoice_RetainageAcctID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Base.location.Current != null && e.Row != null)
			{
				e.NewValue = Base.GetAcctSub<CR.Location.aRRetainageAcctID>(Base.location.Cache, Base.location.Current);
			}
		}

		protected virtual void ARInvoice_RetainageSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Base.location.Current != null && e.Row != null)
			{
				e.NewValue = Base.GetAcctSub<CR.Location.aRRetainageSubID>(Base.location.Cache, Base.location.Current);
			}
		}

		protected virtual void ARInvoice_CustomerLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARInvoice document = (ARInvoice)e.Row;
			if (document == null) return;

			if (document.RetainageApply == true)
			{
				sender.SetDefaultExt<ARInvoice.retainageAcctID>(document);
				sender.SetDefaultExt<ARInvoice.retainageSubID>(document);
			}
		}

		protected virtual void ARInvoice_RetainageApply_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARInvoice document = (ARInvoice)e.Row;
			if (document == null) return;

			if (document.RetainageApply == true)
			{
				sender.SetDefaultExt<ARInvoice.retainageAcctID>(document);
				sender.SetDefaultExt<ARInvoice.retainageSubID>(document);
			}
			else if (document.IsRetainageDocument == false)
			{
				sender.SetValueExt<ARInvoice.retainageAcctID>(document, null);
				sender.SetValueExt<ARInvoice.retainageSubID>(document, null);
			}
		}

		protected virtual void ARInvoice_RetainageApply_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARInvoice document = (ARInvoice)e.Row;
			bool? newValue = (bool?)e.NewValue;

			if (document == null) return;

			if (document.RetainageApply == true && newValue == false)
			{
				IEnumerable<ARTran> trans = Base.Transactions.Select().AsEnumerable().Where(tran => ((ARTran)tran).CuryRetainageAmt != 0 || ((ARTran)tran).RetainagePct != 0).RowCast<ARTran>();

				if (!trans.Any()) return;

				WebDialogResult wdr =
					Base.Document.Ask(
						Messages.Warning,
						AP.Messages.UncheckApplyRetainage,
						MessageButtons.YesNo,
						MessageIcon.Warning);

				if (wdr == WebDialogResult.Yes)
				{
					foreach (ARTran tran in trans)
					{
						tran.CuryRetainageAmt = 0m;
						tran.RetainagePct = 0m;
						Base.Transactions.Update(tran);
					}
				}
				else
				{
					e.Cancel = true;
					e.NewValue = true;
				}
			}
			else if (document.RetainageApply != true && newValue == true)
			{
				ClearCurrentDocumentDiscountDetails();
			}
		}

		protected virtual void ARTran_SubID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void ClearCurrentDocumentDiscountDetails()
		{
			Base.ARDiscountDetails
					.Select()
					.RowCast<ARInvoiceDiscountDetail>()
					.ForEach(discountDetail => Base.ARDiscountDetails.Cache.Delete(discountDetail));

			Base.Discount_Row
				.Select()
				.RowCast<ARTran>()
				.ForEach(tran => Base.Discount_Row.Cache.Delete(tran));
		}

		#endregion

		#region ARInvoiceDiscountDetail events

		[PXOverride]
		public virtual void ARInvoiceDiscountDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			ARInvoice invoice = Base.Document.Current;

			if (invoice?.RetainageApply == true ||
				invoice?.IsRetainageDocument == true)
			{
				e.Cancel = true;
			}
		}

		public delegate void AddDiscountDelegate(PXCache sender, ARInvoice row);

		[PXOverride]
		public void AddDiscount(
			PXCache sender,
			ARInvoice row,
			AddDiscountDelegate baseMethod)
		{
			bool isRetainage =
				row.RetainageApply == true ||
				row.IsRetainageDocument == true;

			if (!isRetainage)
			{
				baseMethod(sender, row);
			}
		}

		#endregion

		[PXReadOnlyView]
		[PXCopyPasteHiddenView]
		// ARRetainageInvoice class is a ARRegister class alias
		// because only ARRegister part is affecting by the release process
		// and only this way we can get a proper behavior for the QueryCache mechanism.
		//
		public PXSelect<ARRetainageWithApplications,
			Where<ARRetainageWithApplications.origRefNbr, Equal<Current<ARInvoice.refNbr>>,
			And<ARRetainageWithApplications.origDocType, Equal<Current<ARInvoice.docType>>>>> RetainageDocuments;

		[PXCopyPasteHiddenView]
		public PXFilter<RetainageOptions> ReleaseRetainageOptions;

		public PXAction<ARInvoice> releaseRetainage;

		[PXUIField(
			DisplayName = "Release Retainage",
			MapEnableRights = PXCacheRights.Update,
			MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ReleaseRetainage(PXAdapter adapter)
		{
			ARInvoice doc = Base.Document.Current;

			ARRegister reversingDoc;
			if (Base.CheckReversingRetainageDocumentAlreadyExists(Base.Document.Current, out reversingDoc))
			{
				throw new PXException(
					AP.Messages.ReleaseRetainageReversingDocumentExists,
					PXMessages.LocalizeNoPrefix(ARDocTypeDict[doc.DocType]),
					PXMessages.LocalizeNoPrefix(ARDocTypeDict[reversingDoc.DocType]),
					reversingDoc.RefNbr);
			}

			if (doc.ProformaExists == true)
			{
				PMProforma proforma = PXSelect<PMProforma, Where<PMProforma.aRInvoiceDocType, Equal<Current<ARInvoice.docType>>,
					And<PMProforma.aRInvoiceRefNbr, Equal<Current<ARInvoice.refNbr>>>>>.Select(Base);

				if (proforma != null && proforma.Corrected == true)
				{
					throw new PXException(PX.Objects.PM.Messages.CannotReleaseRetainage, doc.RefNbr, proforma.RefNbr);
				}
			}

			Base.Save.Press();

			ARRetainageRelease retainageGraph = PXGraph.CreateInstance<ARRetainageRelease>();

			retainageGraph.Filter.Current.BranchID = doc.BranchID;
			retainageGraph.Filter.Current.OrgBAccountID = PXAccess.GetBranch(doc.BranchID).BAccountID;
			retainageGraph.Filter.Current.CustomerID = doc.CustomerID;
			retainageGraph.Filter.Current.RefNbr = doc.RefNbr;
			retainageGraph.Filter.Current.ShowBillsWithOpenBalance = doc.OpenDoc == true;

			ARInvoiceExt retainageDocToRelease = retainageGraph.DocumentList.SelectSingle();
			if (retainageDocToRelease == null)
			{
				ARRetainageWithApplications retainageDoc = RetainageDocuments
				.Select()
				.FirstOrDefault(row => ((ARRetainageWithApplications)row).Released != true);

				if (retainageDoc != null)
				{
					throw new PXException(
						AP.Messages.ReleaseRetainageNotReleasedDocument,
						PXMessages.LocalizeNoPrefix(ARDocTypeDict[retainageDoc.DocType]),
						retainageDoc.RefNbr,
						PXMessages.LocalizeNoPrefix(ARDocTypeDict[doc.DocType]));
				}
			}

			throw new PXRedirectRequiredException(retainageGraph, nameof(ReleaseRetainage));
		}

		public virtual void ReleaseRetainageProc(List<ARInvoiceExt> list, RetainageOptions retainageOpts, bool isAutoRelease = false)
		{
			bool failed = false;
			List<ARInvoice> result = new List<ARInvoice>();
			IProjectDataProvider projectDataProvider = Base.GetService<IProjectDataProvider>();
			var groupedList = (PXAccess.FeatureInstalled<FeaturesSet.paymentsByLines>()) ?
				list.GroupBy(row =>
					new
					{
						row.CustomerID,
						row.ProjectID,
						row.CustomerLocationID,
						row.TaxZoneID,
						row.BranchID,
						row.ARAccountID,
						row.ARSubID,
						row.TaxCalcMode,
						row.ExternalTaxExemptionNumber,
						row.AvalaraCustomerUsageType,
						row.CuryID
					}).Select(x => x.ToList()).ToList()
				:
				list.GroupBy(row => new { row.DocType, row.RefNbr }).Select(x => x.ToList()).ToList();

			foreach (var grouper in groupedList)
			{
				try
				{
					//inside the group (future consolidated retainage)
					bool isConsolidated = grouper.GroupBy(x => new { x.DocType, x.RefNbr }).Count() > 1;
					decimal curyRetainageSumAll = grouper.Sum(row => row.CuryRetainageReleasedAmt ?? 0m);
					Dictionary<int?, ARTran> arTranList = new Dictionary<int?, ARTran>();
					Dictionary<int?, decimal> signList = new Dictionary<int?, decimal>();
					List<ARTaxTran> arTaxTranList = new List<ARTaxTran>();
					List<ARTax> arTaxList = new List<ARTax>();
					ARInvoice invoice = new ARInvoice();
					TaxCalc oldTaxCalc = TaxBaseAttribute.GetTaxCalc<ARTran.taxCategoryID>(Base.Transactions.Cache, null);

					foreach (var origdoc in grouper.GroupBy(row => new { row.DocType, row.RefNbr }))
					{
						ARInvoiceExt doc = origdoc.First();
						PXProcessing<ARInvoiceExt>.SetCurrentItem(doc);

						decimal curyRetainageSum = origdoc.Sum(row => row.CuryRetainageReleasedAmt ?? 0m);

						Base.Clear(PXClearOption.ClearAll);
						PXUIFieldAttribute.SetError(Base.Document.Cache, null, null, null);

						ARTran tranMax = null;
						oldTaxCalc = TaxBaseAttribute.GetTaxCalc<ARTran.taxCategoryID>(Base.Transactions.Cache, null);

						Base.Clear(PXClearOption.PreserveTimeStamp);

						// Magic. We need to prevent rewriting of CurrencyInfo.IsReadOnly 
						// by true in CurrencyInfoView
						// 
						Base.CurrentDocument.Cache.AllowUpdate = true;

						PXResult<ARInvoice, CurrencyInfo, Terms, Customer> resultDoc =
							ARInvoice_CurrencyInfo_Terms_Customer
								.SelectSingleBound(Base, null, doc.DocType, doc.RefNbr).AsEnumerable()
								.Cast<PXResult<ARInvoice, CurrencyInfo, Terms, Customer>>()
								.First();

						ARInvoice origInvoice = resultDoc;
						Customer customer = resultDoc;

						CurrencyInfo new_info = Base.GetExtension<MultiCurrency>().CloneCurrencyInfo(resultDoc);
						new_info.IsReadOnly = false;

						invoice = PXCache<ARInvoice>.CreateCopy(origInvoice);
						object ownerID = Base.CurrentDocument.Cache.GetValue<ARInvoice.ownerID>(origInvoice);
						try
						{
							Base.CurrentDocument.Cache.RaiseFieldVerifying<ARInvoice.ownerID>(origInvoice, ref ownerID);
						}
						catch (Exception ex)
						{
							invoice.OwnerID = null;
						}

						invoice.CuryInfoID = new_info.CuryInfoID;
						if (curyRetainageSum != 0m)
						{
							invoice.DocType = curyRetainageSum < 0m ? ARDocType.CreditMemo : ARDocType.Invoice;
						}
						invoice.RefNbr = null;
						invoice.LineCntr = null;
						PMProject projectInfo = projectDataProvider.GetProject(Base, origInvoice.ProjectID);
						invoice.DocDesc = isConsolidated ?
							string.Format(Messages.ConsolidatedRetainageDescription, projectInfo.ContractCD.TrimEnd(), projectInfo.Description.TrimEnd())
							: origInvoice.DocDesc;
						invoice.InvoiceNbr = origInvoice.InvoiceNbr;

						// Must be set for _RowSelected event handler
						// 
						invoice.OpenDoc = true;
						invoice.Released = false;

						Base.Document.Cache.SetDefaultExt<ARInvoice.isMigratedRecord>(invoice);
						Base.Document.Cache.SetDefaultExt<ARInvoice.hold>(invoice);
						invoice.BatchNbr = null;
						invoice.ScheduleID = null;
						invoice.Scheduled = false;
						invoice.NoteID = null;

						invoice.TermsID = (invoice.DocType == ARDocType.CreditMemo) ? null : customer.TermsID;
						invoice.DueDate = null;
						invoice.DiscDate = null;
						invoice.CuryOrigDiscAmt = 0m;
						invoice.OrigDocType = origInvoice.DocType;
						invoice.OrigRefNbr = origInvoice.RefNbr;
						invoice.OrigDocDate = origInvoice.DocDate;
						invoice.OrigRefNbr = isConsolidated ? null : origInvoice.RefNbr;
						invoice.OrigDocType = isConsolidated ? null : origInvoice.DocType;

						invoice.CuryLineTotal = 0m;
						invoice.IsTaxPosted = false;
						invoice.IsTaxValid = false;
						invoice.CuryVatTaxableTotal = 0m;
						invoice.CuryVatExemptTotal = 0m;

						invoice.CuryDetailExtPriceTotal = 0m;
						invoice.DetailExtPriceTotal = 0m;
						invoice.CuryLineDiscTotal = 0m;
						invoice.LineDiscTotal = 0m;
						invoice.CuryMiscExtPriceTotal = 0m;
						invoice.MiscExtPriceTotal = 0m;
						invoice.CuryGoodsExtPriceTotal = 0m;
						invoice.GoodsExtPriceTotal = 0m;

						invoice.CuryDocBal = 0m;
						Base.Document.SetValueExt<ARInvoice.curyOrigDocAmt>(invoice, Math.Abs(curyRetainageSum));
						if (invoice.IsMigratedRecord == true)
						{
							invoice.CuryInitDocBal = invoice.CuryOrigDocAmt;
						}
						invoice.Hold = !isAutoRelease && Base.ARSetup.Current.HoldEntry == true || Base.IsApprovalRequired(invoice);

						invoice.DocDate = retainageOpts.DocDate;
						FinPeriodIDAttribute.SetPeriodsByMaster<ARInvoice.finPeriodID>(Base.Document.Cache, invoice, retainageOpts.MasterFinPeriodID);

						Base.ClearRetainageSummary(invoice);
						invoice.RetainageApply = false;
						invoice.IsRetainageDocument = true;

						invoice = Base.Document.Insert(invoice);
						decimal origSign = doc.SignAmount * invoice.SignAmount ?? 0m;

						if (new_info != null)
						{
							CurrencyInfo b_info = (CurrencyInfo)PXSelect<CurrencyInfo,
								Where<CurrencyInfo.curyInfoID, Equal<Current<ARInvoice.curyInfoID>>>>.Select(Base);

							b_info.CuryID = new_info.CuryID;
							b_info.CuryEffDate = new_info.CuryEffDate;
							b_info.CuryRateTypeID = new_info.CuryRateTypeID;
							b_info.CuryRate = new_info.CuryRate;
							b_info.RecipRate = new_info.RecipRate;
							b_info.CuryMultDiv = new_info.CuryMultDiv;
							Base.currencyinfo.Update(b_info);
						}

						bool isRetainageByLines = doc.LineNbr != 0;
						bool isFinalRetainageDoc = !isRetainageByLines && doc.CuryRetainageUnreleasedCalcAmt == 0m;
						var retainageDetails = new Dictionary<(string TranType, string RefNbr, int? LineNbr), ARTranRetainageData>();

						foreach (ARInvoiceExt docLine in origdoc)
						{
							PXProcessing<ARInvoiceExt>.SetCurrentItem(docLine);

							PXResultset<ARTran> details = isRetainageByLines
								? PXSelect<ARTran,
									Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
										And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
										And<ARTran.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>
									.SelectSingleBound(Base, null, docLine.DocType, docLine.RefNbr, docLine.LineNbr)
								: PXSelectGroupBy<ARTran,
									Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
										And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
										And<ARTran.curyRetainageAmt, NotEqual<decimal0>>>>,
									Aggregate<
										GroupBy<ARTran.taxCategoryID,
										Sum<ARTran.curyRetainageAmt>>>,
									OrderBy<Asc<ARTran.taxCategoryID>>>
									.Select(Base, docLine.DocType, docLine.RefNbr);

							TaxBaseAttribute.SetTaxCalc<ARTran.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualCalc);

							foreach (ARTran detail in details)
							{
								// Create ARTran record for chosen retainage amount, 
								// clear all required fields to prevent tax calculation,
								// discount calculation and retainage calculation.
								// CuryUnitPrice = 0m and CuryExtPrice = 0m here to prevent their 
								// FieldDefaulting events, because in our case default value 
								// should be equal to zero.
								//
								ARTran tranNew = Base.Transactions.Insert(new ARTran
								{
									CuryUnitPrice = 0m,
									CuryExtPrice = 0m
								});

								tranNew.BranchID = origInvoice.BranchID;
								tranNew.TaxCategoryID = detail.TaxCategoryID;
								tranNew.AccountID = origInvoice.RetainageAcctID;
								tranNew.SubID = origInvoice.RetainageSubID;
								tranNew.ProjectID = ProjectDefaultAttribute.NonProject();
								tranNew.TaskID = null;
								tranNew.CostCodeID = null;

								tranNew.Qty = 0m;
								tranNew.ManualDisc = true;
								tranNew.DiscPct = 0m;
								tranNew.CuryDiscAmt = 0m;
								tranNew.RetainagePct = 0m;
								tranNew.CuryRetainageAmt = 0m;
								tranNew.CuryTaxableAmt = 0m;
								tranNew.CuryTaxAmt = 0;
								tranNew.GroupDiscountRate = 1m;
								tranNew.DocumentDiscountRate = 1m;

								tranNew.OrigLineNbr = docLine.LineNbr;
								tranNew.OrigDocType = detail.TranType;
								tranNew.OrigRefNbr = detail.RefNbr;

								using (new PXLocaleScope(customer.LocaleName))
								{
									tranNew.TranDesc = PXMessages.LocalizeFormatNoPrefix(
										AP.Messages.RetainageForTransactionDescription,
										ARDocTypeDict[detail.TranType],
										detail.RefNbr);
								}

								decimal curyLineAmt = 0m;
								bool isFinalRetainageDetail = docLine.CuryRetainageUnreleasedCalcAmt == 0m;

								if (isFinalRetainageDetail)
								{
									PXResultset<ARTran> detailsRetainage = isRetainageByLines
										? PXSelectJoin<ARTran,
											InnerJoin<ARRegister, On<ARRegister.docType, Equal<ARTran.tranType>,
												And<ARRegister.refNbr, Equal<ARTran.refNbr>>>>,
											Where<ARRegister.isRetainageDocument, Equal<True>,
												And<ARRegister.released, Equal<True>,
												And<ARRegister.origDocType, Equal<Required<ARRegister.origDocType>>,
												And<ARRegister.origRefNbr, Equal<Required<ARRegister.origRefNbr>>,
												And<ARTran.origLineNbr, Equal<Required<ARTran.origLineNbr>>>>>>>>
											.Select(Base, docLine.DocType, docLine.RefNbr, docLine.LineNbr)
										: PXSelectJoin<ARTran,
											InnerJoin<ARRegister, On<ARRegister.docType, Equal<ARTran.tranType>,
												And<ARRegister.refNbr, Equal<ARTran.refNbr>>>>,
											Where<ARRegister.isRetainageDocument, Equal<True>,
												And<ARRegister.released, Equal<True>,
												And<ARRegister.origDocType, Equal<Required<ARRegister.origDocType>>,
												And<ARRegister.origRefNbr, Equal<Required<ARRegister.origRefNbr>>,
												And<Where<ARTran.taxCategoryID, Equal<Required<ARTran.taxCategoryID>>,
													Or<Required<ARTran.taxCategoryID>, IsNull>>>>>>>>
											.Select(Base, docLine.DocType, docLine.RefNbr, detail.TaxCategoryID, detail.TaxCategoryID);

									decimal detailsRetainageSum = 0m;
									foreach (PXResult<ARTran, ARRegister> res in detailsRetainage)
									{
										ARTran detailRetainage = res;
										ARRegister docRetainage = res;

										// We should consider amount sign both for Original and Child Retainage document
										// to cover all possible combinations between INV, CRM and DRM documents.
										decimal sign = doc.SignAmount * docRetainage.SignAmount ?? 0m;
										detailsRetainageSum += (detailRetainage.CuryTranAmt ?? 0m) * sign;
									}

									curyLineAmt = ((detail.CuryRetainageAmt ?? 0m) - detailsRetainageSum) * origSign;
								}
								else
								{
									decimal retainagePercent = Math.Abs((decimal)(docLine.CuryRetainageReleasedAmt /
										(isRetainageByLines ? detail.CuryOrigRetainageAmt : doc.CuryRetainageTotal)));
									curyLineAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury((detail.CuryRetainageAmt ?? 0m) * retainagePercent * origSign);
								}

								tranNew.CuryExtPrice = curyLineAmt;
								tranNew = Base.Transactions.Update(tranNew);

								if (invoice.PaymentsByLinesAllowed == true)
								{
									tranNew.IsStockItem = detail.IsStockItem;
									tranNew.InventoryID = detail.InventoryID;
									tranNew.ProjectID = detail.ProjectID;
									tranNew.TaskID = detail.TaskID;
									tranNew.CostCodeID = detail.CostCodeID;
								}

								if (isRetainageByLines)
								{
									retainageDetails.Add(
										(tranNew.TranType, tranNew.RefNbr, tranNew.LineNbr),
										new ARTranRetainageData()
										{
											Detail = tranNew,
											RemainAmt = (docLine.CuryRetainageReleasedAmt * invoice.SignAmount) - tranNew.CuryExtPrice,
											IsFinal = isFinalRetainageDetail
										});
								}
								else if (tranMax == null || Math.Abs(tranMax.CuryExtPrice ?? 0m) < Math.Abs(tranNew.CuryExtPrice ?? 0m))
								{
									tranMax = tranNew;
								}
							}

							PXProcessing<ARInvoiceExt>.SetProcessed();
						}

						ClearCurrentDocumentDiscountDetails();

						// We should copy all taxes from the original document
						// because it is possible to add or delete them.
						// 
						var taxes = PXSelectJoin<ARTaxTran,
							LeftJoin<Tax, On<Tax.taxID, Equal<ARTaxTran.taxID>>>,
							Where<ARTaxTran.module, Equal<BatchModule.moduleAR>,
								And<ARTaxTran.tranType, Equal<Required<ARTaxTran.tranType>>,
								And<ARTaxTran.refNbr, Equal<Required<ARTaxTran.refNbr>>,
								And<ARTaxTran.curyRetainedTaxAmt, NotEqual<decimal0>>>>>>
							.Select(Base, origdoc.Key.DocType, origdoc.Key.RefNbr);

						// Insert taxes first and only after that copy 
						// all needed values to prevent tax recalculation
						// during the next tax insertion.
						// 
						Dictionary<string, ARTaxTran> insertedTaxes = null;
						insertedTaxes = new Dictionary<string, ARTaxTran>();
						taxes.RowCast<ARTaxTran>().ForEach(tax => insertedTaxes.Add(tax.TaxID, Base.Taxes.Insert(new ARTaxTran() { TaxID = tax.TaxID })));

						foreach (PXResult<ARTaxTran, Tax> res in taxes)
						{
							ARTaxTran origARTaxTran = res;
							Tax tax = res;

							ARTaxTran new_artaxtran = insertedTaxes[origARTaxTran.TaxID];
							if (new_artaxtran == null ||
								new_artaxtran.CuryTaxableAmt == 0m &&
								new_artaxtran.CuryTaxAmt == 0m &&
								new_artaxtran.CuryExpenseAmt == 0m) continue;

							decimal curyTaxAmt = 0m;

							if (isRetainageByLines)
							{
								foreach (ARTax artax in Base.Tax_Rows.Select()
									.RowCast<ARTax>()
									.Where(row => row.TaxID == origARTaxTran.TaxID))
								{
									ARTranRetainageData retainageDetail = retainageDetails[(artax.TranType, artax.RefNbr, artax.LineNbr)];
									decimal detailCuryTaxAmt = 0m;

									ARTax origARTax = PXSelect<ARTax,
										Where<ARTax.tranType, Equal<Required<ARTax.tranType>>,
											And<ARTax.refNbr, Equal<Required<ARTax.refNbr>>,
											And<ARTax.lineNbr, Equal<Required<ARTax.lineNbr>>,
											And<ARTax.taxID, Equal<Required<ARTax.taxID>>>>>>>
										.SelectSingleBound(Base, null, origdoc.Key.DocType, origdoc.Key.RefNbr, retainageDetail.Detail.OrigLineNbr, artax.TaxID);

									if (retainageDetail.IsFinal)
									{
										PXResultset<ARTax> taxDetailsRetainage = PXSelectJoin<ARTax,
											InnerJoin<ARTranPost, On<ARTranPost.sourceDocType, Equal<ARTax.tranType>,
												And<ARTranPost.sourceRefNbr, Equal<ARTax.refNbr>,
												And<ARTranPost.type, Equal<ARTranPost.type.retainage>>>>,
											InnerJoin<ARRegister, On<ARRegister.docType, Equal<ARTranPost.sourceDocType>,
												And<ARRegister.refNbr, Equal<ARTranPost.sourceRefNbr>>>,
											InnerJoin<ARTran, On<ARTran.tranType, Equal<ARTax.tranType>,
												And<ARTran.refNbr, Equal<ARTax.refNbr>,
												And<ARTran.lineNbr, Equal<ARTax.lineNbr>>>>>>>,
											Where<ARRegister.isRetainageDocument, Equal<True>,
												And<ARRegister.released, Equal<True>,
												And<ARTranPost.docType, Equal<Required<ARTranPost.docType>>,
												And<ARTranPost.refNbr, Equal<Required<ARTranPost.refNbr>>,
												And<ARTran.origLineNbr, Equal<Required<ARTran.origLineNbr>>,
												And<ARTax.taxID, Equal<Required<ARTax.taxID>>>>>>>>>
											.Select(Base, retainageDetail.Detail.OrigDocType, retainageDetail.Detail.OrigRefNbr, retainageDetail.Detail.OrigLineNbr, artax.TaxID);

										decimal taxDetailsRetainageSum = 0m;
										foreach (PXResult<ARTax, ARTranPost, ARRegister> resTaxDetailsRetainage in taxDetailsRetainage)
										{
											ARTax taxDetailRetainage = resTaxDetailsRetainage;
											ARRegister docRetainage = resTaxDetailsRetainage;

											// We should consider amount sign both for Original and Child Retainage document
											// to cover all possible combinations between INV, CRM and DRM documents.
											decimal sign = doc.SignAmount * docRetainage.SignAmount ?? 0m;
											taxDetailsRetainageSum += ((taxDetailRetainage.CuryTaxAmt ?? 0m) + (taxDetailRetainage.CuryExpenseAmt ?? 0m)) * sign;
										}

										detailCuryTaxAmt = ((origARTax.CuryRetainedTaxAmt ?? 0m) - taxDetailsRetainageSum) * origSign;
									}
									else
									{
										decimal retainedPercent = Math.Abs((decimal)origARTax.CuryRetainedTaxAmt / (decimal)origARTax.CuryRetainedTaxableAmt);
										detailCuryTaxAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury((decimal)artax.CuryTaxableAmt * retainedPercent);
									}

									curyTaxAmt += detailCuryTaxAmt;

									ARTax new_artax = PXCache<ARTax>.CreateCopy(artax);
									decimal detailDeductiblePercent = 100m - (new_artax.NonDeductibleTaxRate ?? 100m);
									new_artax.CuryExpenseAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(detailCuryTaxAmt * detailDeductiblePercent / 100m);
									new_artax.CuryTaxAmt = detailCuryTaxAmt - new_artax.CuryExpenseAmt;
									new_artax = Base.Tax_Rows.Update(new_artax);

									if (tax != null &&
										tax.TaxType != CSTaxType.Use &&
										tax.TaxType != CSTaxType.Withholding &&
										!(invoice.TaxCalcMode == TaxCalculationMode.Gross
										|| tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive
										&& invoice.TaxCalcMode != TaxCalculationMode.Net))
									{
										retainageDetail.RemainAmt -= detailCuryTaxAmt;
									}
								}
							}
							else
							{
								if (isFinalRetainageDoc)
								{
									PXResultset<ARTaxTran> taxDetailsRetainage = PXSelectJoin<ARTaxTran,
										InnerJoin<ARTranPost, On<ARTaxTran.tranType, Equal<ARTranPost.sourceDocType>,
											And<ARTranPost.sourceRefNbr, Equal<ARTaxTran.refNbr>,
											And<ARTranPost.type, Equal<ARTranPost.type.retainage>>>>,
										LeftJoin<ARRegister, On<ARRegister.docType, Equal<ARTranPost.sourceDocType>,
											And<ARRegister.refNbr, Equal<ARTranPost.sourceRefNbr>>>>>,
										Where<ARRegister.isRetainageDocument, Equal<True>,
											And<ARRegister.released, Equal<True>,
											And<ARTranPost.docType, Equal<Required<ARTranPost.docType>>,
											And<ARTranPost.refNbr, Equal<Required<ARTranPost.refNbr>>,
											And<ARTaxTran.taxID, Equal<Required<ARTaxTran.taxID>>>>>>>>
										.Select(Base, origARTaxTran.TranType, origARTaxTran.RefNbr, origARTaxTran.TaxID);

									decimal taxDetailsRetainageSum = 0m;
									foreach (PXResult<ARTaxTran, ARTranPost, ARRegister> resTaxDetailsRetainage in taxDetailsRetainage)
									{
										ARTaxTran taxDetailRetainage = resTaxDetailsRetainage;
										ARRegister docRetainage = resTaxDetailsRetainage;

										// We should consider amount sign both for Original and Child Retainage document
										// to cover all possible combinations between INV, CRM and DRM documents.
										decimal sign = doc.SignAmount * docRetainage.SignAmount ?? 0m;
										taxDetailsRetainageSum += ((taxDetailRetainage.CuryTaxAmt ?? 0m) + (taxDetailRetainage.CuryExpenseAmt ?? 0m)) * sign;
									}

									curyTaxAmt = ((origARTaxTran.CuryRetainedTaxAmt ?? 0m) - taxDetailsRetainageSum) * origSign;
								}
								else
								{
									ARTax retainedTaxableSum = PXSelectGroupBy<ARTax,
										Where<ARTax.tranType, Equal<Required<ARTax.tranType>>,
											And<ARTax.refNbr, Equal<Required<ARTax.refNbr>>,
											And<ARTax.taxID, Equal<Required<ARTax.taxID>>>>>,
										Aggregate<
											GroupBy<ARTax.tranType,
											GroupBy<ARTax.refNbr,
											GroupBy<ARTax.taxID,
											Sum<ARTax.curyRetainedTaxableAmt>>>>>>
										.SelectSingleBound(Base, null, origARTaxTran.TranType, origARTaxTran.RefNbr, origARTaxTran.TaxID);

									decimal retainedPercent = Math.Abs((decimal)origARTaxTran.CuryRetainedTaxAmt / (decimal)retainedTaxableSum.CuryRetainedTaxableAmt);
									curyTaxAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury((decimal)new_artaxtran.CuryTaxableAmt * retainedPercent);
								}
							}
							new_artaxtran = PXCache<ARTaxTran>.CreateCopy(new_artaxtran);

							// We should adjust ARTax taxable amount for inclusive tax, 
							// because it used during the release process to post correct 
							// amount on Expense account for each ARTran record. 
							// See ARReleaseProcess.GetSalesPostingAmount method for details.
							// 
							decimal taxDiff = (new_artaxtran.CuryTaxAmt ?? 0m) + (new_artaxtran.CuryExpenseAmt ?? 0m) - curyTaxAmt;
							if ((tax?.TaxCalcLevel == CSTaxCalcLevel.Inclusive && invoice.TaxCalcMode != TaxCalculationMode.Net
							|| invoice.TaxCalcMode == TaxCalculationMode.Gross) && taxDiff != 0m)
							{
								new_artaxtran.CuryTaxableAmt += taxDiff;

								foreach (ARTax roundARTax in Base.Tax_Rows.Select()
									.AsEnumerable().RowCast<ARTax>()
									.Where(row => row.TaxID == new_artaxtran.TaxID))
								{
									ARTax roundTaxDetail = PXCache<ARTax>.CreateCopy(roundARTax);
									roundTaxDetail.CuryTaxableAmt += taxDiff;
									roundTaxDetail = Base.Tax_Rows.Update(roundTaxDetail);

									foreach (ARTax lineARTax in Base.Tax_Rows.Select()
										.AsEnumerable().RowCast<ARTax>()
										.Where(row => row.TaxID != roundARTax.TaxID && row.LineNbr == roundARTax.LineNbr))
									{
										ARTaxTran lineARTaxTran = insertedTaxes[lineARTax.TaxID];
										lineARTaxTran.CuryTaxableAmt += taxDiff;
										lineARTaxTran = Base.Taxes.Update(lineARTaxTran);

										ARTax lineTaxDetail = PXCache<ARTax>.CreateCopy(lineARTax);
										lineTaxDetail.CuryTaxableAmt += taxDiff;
										lineTaxDetail = Base.Tax_Rows.Update(lineTaxDetail);
									}
								}
							}

							new_artaxtran.TaxRate = origARTaxTran.TaxRate;
							decimal deductiblePercent = 100m - (new_artaxtran.NonDeductibleTaxRate ?? 100m);
							new_artaxtran.CuryExpenseAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(curyTaxAmt * deductiblePercent / 100m);
							new_artaxtran.CuryTaxAmt = curyTaxAmt - new_artaxtran.CuryExpenseAmt;
							new_artaxtran.CuryTaxAmtSumm = new_artaxtran.CuryTaxAmt;
							new_artaxtran = Base.Taxes.Update(new_artaxtran);
						}

						if (isRetainageByLines)
						{
							retainageDetails.Values
								.Where(value => value.RemainAmt != 0m)
								.ForEach(value => ProcessRoundingDiff(value.RemainAmt ?? 0m, value.Detail));
						}
						else if (tranMax != null)
						{
							decimal diff = Math.Abs(curyRetainageSum) - (invoice.CuryDocBal ?? 0m);
							if (diff != 0m)
							{
								ProcessRoundingDiff(diff, tranMax);
							}
						}

						#region consolidate data
						if (isConsolidated)
						{
							// accumulate calculated data
							List<ARTax> tempArTax = new List<ARTax>();
							List<ARTaxTran> tempArTaxTran = new List<ARTaxTran>();
							Base.Taxes.Select().ForEach(a => tempArTaxTran.Add(a));
							tempArTaxTran.ForEach(a => arTaxTranList.Add(a));
							Base.Tax_Rows.Select().RowCast<ARTax>().ForEach(a => tempArTax.Add(a));

							if (invoice.PaymentsByLinesAllowed != true)
							{
								// for consolidated document we should copy CuryTaxAmt value from ArtaxTran to ARTax
								// (ArtaxTran.CuryTaxAmt -> ARTax.CuryTaxAmt)
								// because PBL = true for consolidated, but PBL = false for orig document (where ARTAx.CuryTaxAmt=0 and
								// calculated tax amount lives in ARTaxTran table)
								foreach (ARTax tax in tempArTax)
								{
									tax.CuryTaxAmt = tempArTaxTran.Where(a => a.TaxID == tax.TaxID).First().CuryTaxAmt ?? 0m;
								}
							}

							foreach (ARTran tran in Base.Transactions.Select().RowCast<ARTran>())
							{
								if (arTranList.Keys.Contains(tran.LineNbr))
								{
									int ind = arTranList.Keys.Count() + 1;
									// change ARTax.LineNbr according to its change in ARTran (tran.LineNbr <= ind)
									tempArTax.Where(a => a.LineNbr == tran.LineNbr).ForEach(a =>
									{
										ARTax newTax = PXCache<ARTax>.CreateCopy(a);
										newTax.LineNbr = ind;
										arTaxList.Add(newTax);
									});

									tran.LineNbr = ind;
									arTranList.Add(ind, tran);
									signList.Add(tran.LineNbr, invoice.SignAmount ?? 1);
								}
								else
								{
									arTranList.Add(tran.LineNbr, tran);
									tempArTax.Where(a => a.LineNbr == tran.LineNbr).ForEach(a => arTaxList.Add(a));
									signList.Add(tran.LineNbr, invoice.SignAmount ?? 1);
								}
							}
						}
						#endregion
					}

					#region consolidate retainage
					if (isConsolidated)
					{
						// create consolidated document according to accumulated data
						ARInvoiceExt doc = grouper.First();

						Base.Clear(PXClearOption.ClearAll);
						PXUIFieldAttribute.SetError(Base.Document.Cache, null, null, null);
						oldTaxCalc = TaxBaseAttribute.GetTaxCalc<ARTran.taxCategoryID>(Base.Transactions.Cache, null);
						Base.Clear(PXClearOption.PreserveTimeStamp);
						Base.CurrentDocument.Cache.AllowUpdate = true;

						PXResult<ARInvoice, CurrencyInfo, Terms, Customer> resultDoc =
							ARInvoice_CurrencyInfo_Terms_Customer
								.SelectSingleBound(Base, null, doc.DocType, doc.RefNbr).AsEnumerable()
								.Cast<PXResult<ARInvoice, CurrencyInfo, Terms, Customer>>()
								.First();

						ARInvoice origInvoice = resultDoc;
						Customer customer = resultDoc;
						CurrencyInfo new_info = Base.GetExtension<MultiCurrency>().CloneCurrencyInfo(resultDoc);
						new_info.IsReadOnly = false;
						invoice = PXCache<ARInvoice>.CreateCopy(origInvoice);
						object ownerID = Base.CurrentDocument.Cache.GetValue<ARInvoice.ownerID>(origInvoice);
						try
						{
							Base.CurrentDocument.Cache.RaiseFieldVerifying<ARInvoice.ownerID>(origInvoice, ref ownerID);
						}
						catch (Exception ex)
						{
							invoice.OwnerID = null;
						}

						invoice.CuryInfoID = new_info.CuryInfoID;
						if (curyRetainageSumAll != 0m)
						{
							invoice.DocType = curyRetainageSumAll < 0m ? ARDocType.CreditMemo : ARDocType.Invoice;
						}
						invoice.RefNbr = null;
						invoice.LineCntr = null;
						PMProject projectInfo = projectDataProvider.GetProject(Base, origInvoice.ProjectID);
						invoice.DocDesc = string.Format(Messages.ConsolidatedRetainageDescription, projectInfo.ContractCD.TrimEnd(), projectInfo.Description.TrimEnd());

						invoice.InvoiceNbr = origInvoice.InvoiceNbr;
						invoice.OpenDoc = true;
						invoice.Released = false;

						Base.Document.Cache.SetDefaultExt<ARInvoice.isMigratedRecord>(invoice);
						Base.Document.Cache.SetDefaultExt<ARInvoice.hold>(invoice);
						invoice.BatchNbr = null;
						invoice.ScheduleID = null;
						invoice.Scheduled = false;
						invoice.NoteID = null;

						invoice.TermsID = (invoice.DocType == ARDocType.CreditMemo) ? null : customer.TermsID;
						invoice.DueDate = null;
						invoice.DiscDate = null;
						invoice.CuryOrigDiscAmt = 0m;
						invoice.OrigDocType = origInvoice.DocType;
						invoice.OrigRefNbr = origInvoice.RefNbr;
						invoice.OrigDocDate = origInvoice.DocDate;
						invoice.OrigRefNbr = null;
						invoice.OrigDocType = null;

						invoice.CuryLineTotal = 0m;
						invoice.IsTaxPosted = false;
						invoice.IsTaxValid = false;
						invoice.CuryVatTaxableTotal = 0m;
						invoice.CuryVatExemptTotal = 0m;

						invoice.CuryDetailExtPriceTotal = 0m;
						invoice.DetailExtPriceTotal = 0m;
						invoice.CuryLineDiscTotal = 0m;
						invoice.LineDiscTotal = 0m;
						invoice.CuryMiscExtPriceTotal = 0m;
						invoice.MiscExtPriceTotal = 0m;
						invoice.CuryGoodsExtPriceTotal = 0m;
						invoice.GoodsExtPriceTotal = 0m;

						invoice.CuryDocBal = 0m;
						Base.Document.SetValueExt<ARInvoice.curyOrigDocAmt>(invoice, Math.Abs(curyRetainageSumAll));
						invoice.Hold = !isAutoRelease && Base.ARSetup.Current.HoldEntry == true || Base.IsApprovalRequired(invoice);

						invoice.DocDate = retainageOpts.DocDate;
						FinPeriodIDAttribute.SetPeriodsByMaster<ARInvoice.finPeriodID>(Base.Document.Cache, invoice, retainageOpts.MasterFinPeriodID);

						Base.ClearRetainageSummary(invoice);
						invoice.RetainageApply = false;
						invoice.IsRetainageDocument = true;

						invoice = Base.Document.Insert(invoice);
						decimal origSign = invoice.SignAmount ?? 0m;

						if (new_info != null)
						{
							CurrencyInfo b_info = (CurrencyInfo)PXSelect<CurrencyInfo,
								Where<CurrencyInfo.curyInfoID, Equal<Current<ARInvoice.curyInfoID>>>>.Select(Base);

							b_info.CuryID = new_info.CuryID;
							b_info.CuryEffDate = new_info.CuryEffDate;
							b_info.CuryRateTypeID = new_info.CuryRateTypeID;
							b_info.CuryRate = new_info.CuryRate;
							b_info.RecipRate = new_info.RecipRate;
							b_info.CuryMultDiv = new_info.CuryMultDiv;
							Base.currencyinfo.Update(b_info);
						}
						invoice.PaymentsByLinesAllowed = true;

						foreach (ARTran t in arTranList.Values)
						{
							ARTran tranNew = Base.Transactions.Insert(new ARTran
							{
								CuryUnitPrice = 0m,
								CuryExtPrice = 0m
							});

							tranNew.BranchID = t.BranchID;
							tranNew.TaxCategoryID = t.TaxCategoryID;
							tranNew.AccountID = t.AccountID;
							tranNew.SubID = t.SubID;
							tranNew.ProjectID = ProjectDefaultAttribute.NonProject();
							tranNew.TaskID = null;
							tranNew.CostCodeID = null;
							tranNew.Qty = 0m;
							tranNew.ManualDisc = true;
							tranNew.DiscPct = 0m;
							tranNew.CuryDiscAmt = 0m;
							tranNew.RetainagePct = 0m;
							tranNew.CuryRetainageAmt = 0m;
							tranNew.CuryTaxableAmt = 0m;
							tranNew.CuryTaxAmt = 0;
							tranNew.GroupDiscountRate = 1m;
							tranNew.DocumentDiscountRate = 1m;
							tranNew.OrigLineNbr = t.OrigLineNbr;
							tranNew.OrigDocType = t.OrigDocType;
							tranNew.OrigRefNbr = t.OrigRefNbr;
							tranNew.TranDesc = t.TranDesc;

							tranNew.IsStockItem = t.IsStockItem;
							tranNew.InventoryID = t.InventoryID;
							tranNew.ProjectID = t.ProjectID;
							tranNew.TaskID = t.TaskID;
							tranNew.CostCodeID = t.CostCodeID;

							tranNew.CuryExtPrice = t.CuryExtPrice * origSign * signList[t.LineNbr];
							Base.Transactions.Update(tranNew);
						}

						var taxIDs = arTaxTranList.GroupBy(x => new { x.TaxID }).Select(x => x.ToList<ARTaxTran>());
						foreach (var group in taxIDs)
						{
							Base.Taxes.Insert(new ARTaxTran() { TaxID = group.First().TaxID });
						}

						//insert ARTaxes
						Base.Tax_Rows.Cache.Clear();
						arTaxList.ForEach(a =>
						{
							a.CuryTaxableAmt = a.CuryTaxableAmt * origSign * signList[a.LineNbr];
							a.CuryTaxAmt = a.CuryTaxAmt * origSign * signList[a.LineNbr];
							a.TranType = invoice.DocType;
							Base.Tax_Rows.Insert(a);
						});

						// update curyTaxableAmt and CuryTaxAmt for already inserted ARTaxTrans
						foreach (ARTaxTran taxtran in Base.Taxes.Select())
						{
							decimal curyTaxableAmt = arTaxList.Where(a => a.TaxID == taxtran.TaxID).Sum(row => row.CuryTaxableAmt ?? 0m);
							decimal curyTaxAmt = arTaxList.Where(a => a.TaxID == taxtran.TaxID).Sum(row => row.CuryTaxAmt ?? 0m);
							taxtran.CuryTaxableAmt = curyTaxableAmt;
							taxtran.CuryTaxAmt = curyTaxAmt;
							taxtran.CuryTaxAmtSumm = curyTaxAmt;
							Base.Taxes.Update(taxtran);
						}
					}
					#endregion

					TaxBaseAttribute.SetTaxCalc<ARTran.taxCategoryID>(Base.Transactions.Cache, null, oldTaxCalc);

					if (!isAutoRelease && Base.ARSetup.Current.HoldEntry == false && Base.IsApprovalRequired(invoice))
					{
						Base.releaseFromHold.Press();
					}

					Base.Save.Press();

					if (isAutoRelease && invoice.Hold != true)
					{
						using (new PXTimeStampScope(null))
						{
							ARDocumentRelease.ReleaseDoc(new List<ARRegister> { invoice }, false);
						}
					}
				}
				catch (PXException exc)
				{
					PXProcessing<ARInvoiceExt>.SetError(exc);
					failed = true;
				}
			}
			if (failed)
			{
				throw new PXOperationCompletedWithErrorException(GL.Messages.DocumentsNotReleased);
			}
		}

		public class ARTranRetainageData
		{
			public ARTran Detail;
			public decimal? RemainAmt;
			public bool IsFinal;
		}

		private void ProcessRoundingDiff(decimal diff, ARTran tran)
		{
			tran.CuryExtPrice += diff;
			tran = Base.Transactions.Update(tran);

			foreach (var group in Base.Tax_Rows.Select()
				.AsEnumerable().RowCast<ARTax>()
				.Where(row => row.LineNbr == tran.LineNbr)
				.GroupBy(row => new { row.TranType, row.RefNbr, row.TaxID }))
			{
				foreach (ARTax taxDetail in group)
				{
					ARTax newTaxDetail = PXCache<ARTax>.CreateCopy(taxDetail);
					newTaxDetail.CuryTaxableAmt += diff;
					newTaxDetail = Base.Tax_Rows.Update(newTaxDetail);
				}

				ARTaxTran taxSum = PXSelect<ARTaxTran,
					Where<ARTaxTran.tranType, Equal<Required<ARTaxTran.tranType>>,
						And<ARTaxTran.refNbr, Equal<Required<ARTaxTran.refNbr>>,
						And<ARTaxTran.taxID, Equal<Required<ARTaxTran.taxID>>>>>>
					.SelectSingleBound(Base, null, group.Key.TranType, group.Key.RefNbr, group.Key.TaxID);
				if (taxSum != null)
				{
					ARTaxTran newTaxSum = PXCache<ARTaxTran>.CreateCopy(taxSum);
					newTaxSum.CuryTaxableAmt += diff;
					newTaxSum = Base.Taxes.Update(newTaxSum);
				}
			}
		}

		public PXAction<ARInvoice> ViewRetainageDocument;

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		protected virtual IEnumerable viewRetainageDocument(PXAdapter adapter)
		{
			RedirectionToOrigDoc.TryRedirect(
				RetainageDocuments.Current.DocType,
				RetainageDocuments.Current.RefNbr,
				RetainageDocuments.Current.OrigModule,
				true);
			return adapter.Get();
		}

		public PXAction<ARInvoice> viewOrigRetainageDocument;
		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewOrigRetainageDocument(PXAdapter adapter)
		{
			if (Base.Transactions.Current != null)
			{
				RedirectionToOrigDoc.TryRedirect(
					Base.Transactions.Current.OrigDocType,
					Base.Transactions.Current.OrigRefNbr,
					Base.Document.Current.OrigModule,
					true);
			}
			return adapter.Get();
		}
	}

	public class ARInvoiceEntryRetainage_Workflow : PXGraphExtension<ARInvoiceEntry_Workflow, ARInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.retainage>();
		}

		[PXWorkflowDependsOnType(typeof(ARSetupApproval))]
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ARInvoiceEntry, ARInvoice>());

		protected static void Configure(WorkflowContext<ARInvoiceEntry, ARInvoice> context)
		{
			var processingCategory = context.Categories.Get(ARInvoiceEntry_Workflow.CategoryID.Processing);
			
			Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
			
			var conditions = new
			{
				IsNotRetenageApplied
					= Bql<Not<ARRegister.docType.IsIn<ARDocType.invoice, ARDocType.creditMemo>
						.And<ARRegister.released.IsEqual<True>>
						.And<ARRegister.retainageApply.IsEqual<True>>>>(),
				IsNotRetenageInvoice = Bql<ARRegister.docType.IsNotIn<ARDocType.invoice, ARDocType.creditMemo>>(),
				IsRetenageMigrationMode =
					ARSetupDefinition.GetSlot().MigrationMode == true
						? Bql<True.IsEqual<True>>()
						: Bql<True.IsEqual<False>>(),

			}.AutoNameConditions();

			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.WithActions(actions =>
					{
						actions.Add<ARInvoiceEntryRetainage>(g => g.releaseRetainage,
							c => c.InFolder(processingCategory, g => g.payInvoice)
								.PlaceAfter(g => g.payInvoice)
								.IsHiddenWhen(conditions.IsNotRetenageInvoice || conditions.IsRetenageMigrationMode)
								.IsDisabledWhen(conditions.IsNotRetenageApplied));
					});
			});
		}
	}

	/// <summary>
	/// All retainage documents (released and not released) related to the specified origin document
	/// with its released retainage and paid amount (accumulated from ARTranPost DAC)
	/// </summary>
	[Serializable]
	[PXCacheName("AR Retainage documents with released/paid amount")]
	[PXProjection(typeof(Select5<ARRetainageInvoice,
			LeftJoin<ARTran, On<ARTran.refNbr, Equal<ARRetainageInvoice.refNbr>,
				And<ARTran.tranType, Equal<ARRetainageInvoice.docType>,
				And<ARTran.lineNbr, Equal<IIf<Where<ARRetainageInvoice.paymentsByLinesAllowed, Equal<True>>, ARTran.lineNbr, int1>>>>>,
			InnerJoinSingleTable<ARInvoice, On<ARInvoice.docType, Equal<ARRetainageInvoice.docType>,
				And<ARInvoice.refNbr, Equal<ARRetainageInvoice.refNbr>>>,
			LeftJoin<ARTranPost, On<ARTranPost.sourceRefNbr, Equal<ARRetainageInvoice.refNbr>,
				And<ARTranPost.sourceDocType, Equal<ARRetainageInvoice.docType>,
				And<ARTranPost.type, Equal<ARTranPost.type.retainage>,
				And<ARTranPost.refNbr, Equal<ARTran.origRefNbr>,
				And<ARTranPost.docType, Equal<ARTran.origDocType>>>>>>,
			LeftJoin<ARTranPostAlias,
				On<ARTranPostAlias.refNbr, Equal<ARTran.refNbr>,
				And<ARTranPostAlias.docType, Equal<ARTran.tranType>,
				And<ARTranPostAlias.lineNbr, Equal<
					IIf<Where<ARRetainageInvoice.paymentsByLinesAllowed, Equal<True>>,
						ARTran.lineNbr,
						int0>>,
				And<Where<ARTranPostAlias.type, Equal<ARTranPost.type.application>,
					Or<ARTranPostAlias.type, Equal<ARTranPost.type.adjustment>>>>>>>>>>>,
			Where<ARRetainageInvoice.isRetainageDocument, Equal<True>>,
			Aggregate<GroupBy<ARRetainageInvoice.docType,
				GroupBy<ARRetainageInvoice.refNbr,
				GroupBy<ARTran.origDocType,
				GroupBy<ARTran.origRefNbr,
				Sum<ARTranPostAlias.curyAmt, Sum<ARTranPostAlias.amt,
				Sum<ARTranPostAlias.curyDiscAmt, Sum<ARTranPostAlias.discAmt,
				Sum<ARTranPostAlias.curyWOAmt, Sum<ARTranPostAlias.wOAmt>>>>>>>>>>>>))]
	public partial class ARRetainageWithApplications : ARRetainageInvoice
	{
		#region OrigDocType
		public new abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		/// <summary>
		/// The type of the original retainage document.
		/// Depends on the <see cref="ARTran.OrigDocType"/> field.
		/// </summary>
		[PXDBString(IsKey = true, BqlField = typeof(ARTran.origDocType))]
		[PXUIField(DisplayName = "Doc. Type")]
		public override string OrigDocType { get; set; }
		#endregion
		#region OrigRefNbr
		public new abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		/// <summary>
		/// The reference number of the original retainage document.
		/// Depends on the <see cref="ARTran.OrigRefNbr"/> field.
		/// </summary>
		[PXDBString(IsKey = true, BqlField = typeof(ARTran.origRefNbr))]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public override string OrigRefNbr { get; set; }
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID>{}
		/// <summary>
		/// The identifier of the <see cref="CurrencyInfo">CurrencyInfo</see> object associated with the document.
		/// </summary>
		[PXDBLong(BqlField = typeof(ARTranPost.curyInfoID))]
		[CurrencyInfo(typeof(ARRegister.curyInfoID))]
		public override long? CuryInfoID { get; set; }
		#endregion
		#region glSign
		public abstract class glSign : PX.Data.BQL.BqlShort.Field<glSign> { }
		/// <summary>
		/// Indicates the sign of the document's impact on AR balance.
		/// Depends on the <see cref="ARTranPost.GLSign"/> field.
		/// </summary>
		[PXDBShort(BqlField = typeof(ARTranPost.glSign))]
		public virtual short? GlSign { get; set; }
		#endregion
		#region BalanceSign
		public abstract class balanceSign : PX.Data.BQL.BqlShort.Field<balanceSign> { }
		/// <summary>
		/// Indicates the sign of the document's impact on AR balance.
		/// Depends on the <see cref="ARTranPost.BalanceSign"/> field.
		/// </summary>
		[PXDBShort(BqlField = typeof(ARTranPost.balanceSign))]
		public virtual short? BalanceSign { get; set; }
		#endregion
		#region RetainageAmt
		public abstract class retainageAmt : PX.Data.BQL.BqlDecimal.Field<retainageAmt> { }
		/// <summary>
		/// Retainage amount. Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// Depends on the <see cref="ARTranPost.RetainageAmt"/> field.
		/// </summary>
		[PXDBBaseCury(BqlField = typeof(ARTranPost.retainageAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? RetainageAmt { get; set; }
		#endregion
		#region CuryRetainageAmt
		public abstract class curyRetainageAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageAmt> { }
		/// <summary>
		/// Retainage amount.
		/// Depends on the <see cref="ARTranPost.CuryRetainageAmt"/> field.
		/// </summary>
		[PXDBCurrency(typeof(curyInfoID), typeof(retainageAmt), BaseCalc = false, BqlField = typeof(ARTranPost.curyRetainageAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryRetainageAmt { get; set; }
		#endregion
		#region Amt
		public abstract class amt : PX.Data.BQL.BqlDecimal.Field<amt> { }
		/// <summary>
		/// Paid retainage. Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// Depends on the <see cref="ARTranPost.Amt"/> field.
		/// </summary>
		[PXDBBaseCury(BqlField = typeof(ARTranPostAlias.amt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? Amt { get; set; }
		#endregion
		#region CuryAmt
		public abstract class curyAmt : PX.Data.BQL.BqlDecimal.Field<curyAmt> { }
		/// <summary>
		/// Paid retainage.
		/// Depends on the <see cref="ARTranPost.CuryAmt"/> field.
		/// </summary>
		[PXDBCurrency(typeof(ARTranPostAlias.curyInfoID), typeof(ARTranPostAlias.amt), BaseCalc = false, BqlField = typeof(ARTranPostAlias.curyAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Paid Retainage")]
		public virtual decimal? CuryAmt { get; set; }
		#endregion

		#region DiscAmt
		public abstract class discAmt : PX.Data.BQL.BqlDecimal.Field<discAmt> { }
		/// <summary>
		/// Paid retainage discount. Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// Depends on the <see cref="ARTranPost.Amt"/> field.
		/// </summary>
		[PXDBBaseCury(BqlField = typeof(ARTranPostAlias.discAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? DiscAmt { get; set; }
		#endregion
		#region CuryDiscAmt
		public abstract class curyDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAmt> { }
		/// <summary>
		/// Paid retainage discount.
		/// Depends on the <see cref="ARTranPost.CuryDiscAmt"/> field.
		/// </summary>
		[PXDBCurrency(typeof(ARTranPostAlias.curyInfoID), typeof(ARTranPostAlias.discAmt), BaseCalc = false, BqlField = typeof(ARTranPostAlias.curyDiscAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryDiscAmt { get; set; }
		#endregion
		#region WOAmt
		public abstract class wOAmt : PX.Data.BQL.BqlDecimal.Field<wOAmt> { }
		/// <summary>
		/// Paid retainage write off. Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// Depends on the <see cref="ARTranPost.Amt"/> field.
		/// </summary>
		[PXDBBaseCury(BqlField = typeof(ARTranPostAlias.wOAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? WOAmt { get; set; }
		#endregion
		#region CuryWOAmt
		public abstract class curyWOAmt : PX.Data.BQL.BqlDecimal.Field<curyWOAmt> { }
		/// <summary>
		/// Paid retainage write off.
		/// Depends on the <see cref="ARTranPost.CuryWOAmt"/> field.
		/// </summary>
		[PXDBCurrency(typeof(ARTranPostAlias.curyInfoID), typeof(ARTranPostAlias.wOAmt), BaseCalc = false, BqlField = typeof(ARTranPostAlias.curyWOAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryWOAmt { get; set; }
		#endregion

		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		protected string _PaymentMethodID;
		/// <summary>
		/// The identifier of the payment method that is used for the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="ARInvoice.PaymentMethodID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true, BqlField = typeof(ARInvoice.paymentMethodID))]
		[PXUIFieldAttribute(DisplayName = "Payment Method")]
		public virtual String PaymentMethodID { get; set; }
		#endregion
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		protected String _InvoiceNbr;
		/// <summary>
		/// The original reference number or ID assigned by the customer to the customer document.
		/// Depends on the <see cref="ARInvoice.InvoiceNbr"/> field.
		/// </summary>
		[PXDBString(40, IsUnicode = true, BqlField = typeof(ARInvoice.invoiceNbr))]
		[PXUIField(DisplayName = "Customer Order Nbr.", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
		public virtual String InvoiceNbr { get; set; }
		#endregion
		#region CuryRetainageReleasedAmt
		public abstract class curyRetainageReleasedAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageReleasedAmt> { }
		/// <summary>
		/// Released Retainage.
		/// Given in the currency of the document.
		/// </summary>
		[PXFormula(typeof(IIf<Where<ARRetainageWithApplications.curyRetainageAmt, IsNull>, decimal0,
			Data.Mult<ARRetainageWithApplications.curyRetainageAmt, Mult<ARRetainageWithApplications.glSign, Minus<ARRetainageWithApplications.balanceSign>>>>))]
		[PXDecimal()]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Released Retainage")]
		public virtual decimal? CuryRetainageReleasedAmt { get; set; }
		#endregion
		#region CuryRetainagePaidAmt
		public abstract class curyRetainagePaidAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainagePaidAmt> { }
		/// <summary>
		/// Paid Retainage with the document sign.
		/// Given in the currency of the document.
		/// </summary>
		[PXFormula(typeof(IIf<Where<curyAmt.IsNull.Or<curyDiscAmt.IsNull.Or<curyWOAmt.IsNull>>>, decimal0,
			Data.Mult<curyAmt.Add<curyDiscAmt.Add<curyWOAmt>>, Mult<ARRetainageWithApplications.glSign, ARRetainageWithApplications.balanceSign>>>))]
		[PXDecimal()]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Paid Retainage")]
		public virtual decimal? CuryRetainagePaidAmt { get; set; }
		#endregion
		#region OrigDocAmt
		public new abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
		/// <summary>
		/// Total amount of the original retainage document. Given in the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBBaseCury(BqlField = typeof(ARRetainageInvoice.origDocAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public override Decimal? OrigDocAmt { get; set; }
		#endregion
		#region CuryOrigDocAmt
		public new abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
		/// <summary>
		/// Total amount of the original retainage document.
		/// Given in the currency of the document.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(curyInfoID), typeof(origDocAmt), BqlField = typeof(ARRetainageInvoice.curyOrigDocAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public override Decimal? CuryOrigDocAmt { get; set; }
		#endregion
	}
}
