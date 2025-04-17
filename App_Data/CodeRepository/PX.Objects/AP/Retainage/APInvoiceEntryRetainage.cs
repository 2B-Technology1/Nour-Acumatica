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
using PX.Data.WorkflowAPI;
using PX.Objects.CM.Extensions;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;
using PX.Objects.PM;
using PX.Objects.TX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static PX.Objects.AP.APInvoiceEntry;

namespace PX.Objects.AP
{
	using static BoundedTo<APInvoiceEntry, APInvoice>;
	[Serializable]
	public class APInvoiceEntryRetainage : PXGraphExtension<APInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.retainage>();
		}

		public override void Initialize()
		{
			base.Initialize();
		}

		#region Cache Attached Events

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[APRetainedTax(typeof(APRegister), typeof(APTax), typeof(APTaxTran), typeof(APInvoice.taxCalcMode), parentBranchIDField: typeof(APRegister.branchID))]
		protected virtual void APTran_TaxCategoryID_CacheAttached(PXCache sender) { }

		[DBRetainagePercent(
			typeof(APInvoice.retainageApply),
			typeof(APInvoice.defRetainagePct),
			typeof(Sub<Current<APTran.curyLineAmt>, Current<APTran.curyDiscAmt>>),
			typeof(APTran.curyRetainageAmt),
			typeof(APTran.retainagePct))]
		protected virtual void APTran_RetainagePct_CacheAttached(PXCache sender) { }

		[DBRetainageAmount(
			typeof(APTran.curyInfoID),
			typeof(
				Switch<Case<Where<APTran.tranType, Equal<APDocType.prepayment>>, APTran.curyPrepaymentAmt>,
				Sub<APTran.curyLineAmt, APTran.curyDiscAmt>>),
			typeof(APTran.curyRetainageAmt),
			typeof(APTran.retainageAmt),
			typeof(APTran.retainagePct))]
		protected virtual void APTran_CuryRetainageAmt_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<APRegister.vendorID, Vendor.retainagePct>))]
		protected virtual void APInvoice_DefRetainagePct_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(APRegister.curyRetainageTotal))]
		protected virtual void APInvoice_CuryRetainageUnreleasedAmt_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Switch<Case<Where2<
			FeatureInstalled<FeaturesSet.retainage>,
				And<APRegister.retainageApply, Equal<True>,
				And<APRegister.released, NotEqual<True>>>>,
			APRegister.curyRetainageTotal>,
			APRegister.curyRetainageUnpaidTotal>))]
		protected virtual void APInvoice_CuryRetainageUnpaidTotal_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(
			IIf<Where2<FeatureInstalled<FeaturesSet.retainage>,
				And2<Where<APRegister.docType, Equal<APInvoiceType.invoice>,
					Or<APRegister.docType, Equal<APInvoiceType.debitAdj>>>,
					And<APRegister.origModule, Equal<BatchModule.moduleAP>,
					And<Current<APSetup.migrationMode>, Equal<False>>>>>,
				IsNull<Selector<APRegister.vendorID, Vendor.retainageApply>, False>,
				False>))]
		[PXUIVerify(
			typeof(Where<
			APRegister.retainageApply, NotEqual<True>,
			And<APRegister.isRetainageDocument, NotEqual<True>,
				Or<Selector<APInvoice.termsID, Terms.installmentType>, NotEqual<TermsInstallmentType.multiple>>>>),
			PXErrorLevel.Error,
			Messages.RetainageWithMultipleCreditTerms)]
		[PXUIVerify(
			typeof(Where<APRegister.retainageApply, NotEqual<True>,
				Or<APRegister.curyID, Equal<Current<CurrencyInfo.baseCuryID>>>>),
			PXErrorLevel.Error,
			Messages.RetainageDocumentNotInBaseCurrency)]
		protected virtual void APInvoice_RetainageApply_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIVerify(
			typeof(Where<APRegister.curyRetainageTotal, GreaterEqual<decimal0>, And<APRegister.hold, NotEqual<True>,
				Or<APRegister.hold, Equal<True>>>>),
			PXErrorLevel.Error,
			Messages.IncorrectRetainageTotalAmount)]
		protected virtual void APInvoice_CuryRetainageTotal_CacheAttached(PXCache sender) { }

		#endregion

		#region APInvoice Events

		public delegate bool IsInvoiceNbrRequiredDelegate(APInvoice doc);

		[PXOverride]
		public bool IsInvoiceNbrRequired(APInvoice doc, IsInvoiceNbrRequiredDelegate baseMethod)
		{
			return doc.IsChildRetainageDocument()
				? false
				: baseMethod(doc);
		}

		protected virtual void APInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			APInvoice document = e.Row as APInvoice;
			if (document == null) return;

			bool isDocumentReleasedOrPrebooked = document.Released == true || document.Prebooked == true;
			bool isDocumentVoided = document.Voided == true;

			releaseRetainage.SetEnabled(false);

			if (isDocumentReleasedOrPrebooked || isDocumentVoided)
			{
				releaseRetainage.SetEnabled(
					document.Released == true &&
					document.RetainageApply == true &&
					!document.HasZeroBalance<APRegister.curyRetainageUnreleasedAmt, APTran.curyRetainageBal>(cache.Graph));
			}
		}

		protected virtual void APInvoice_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			APInvoice document = e.Row as APInvoice;
			if (document == null) return;

			if (document.RetainageApply == true &&
				document.Released == true)
			{
				using (new PXConnectionScope())
				{
					APRetainageInvoice dummyInvoice = new APRetainageInvoice();
					dummyInvoice.CuryRetainageUnpaidTotal = 0m;
					dummyInvoice.CuryRetainagePaidTotal = 0m;

					foreach (APRetainageInvoice childRetainageBill in RetainageDocuments
						.Select(document.DocType, document.RefNbr)
						.RowCast<APRetainageInvoice>()
						.Where(res => res.Released == true))
					{
						dummyInvoice.DocType = childRetainageBill.DocType;
						dummyInvoice.RefNbr = childRetainageBill.RefNbr;
						dummyInvoice.CuryOrigDocAmt = childRetainageBill.CuryOrigDocAmt;
						dummyInvoice.CuryDocBal = childRetainageBill.CuryOrigDocAmt;

						foreach (APAdjust application in PXSelect<APAdjust,
							Where<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>,
								And<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>,
								And<APAdjust.released, Equal<True>,
								And<APAdjust.voided, NotEqual<True>,
								And<APAdjust.adjgDocType, NotEqual<APDocType.debitAdj>>>>>>>
							.Select(Base, childRetainageBill.DocType, childRetainageBill.RefNbr))
						{
							dummyInvoice.AdjustBalance(application);
						}

						// We should consider amount sign both for Original and Child Retainage document
						// to cover all possible combinations between , ACR and ADR documents.
						decimal sign = document.SignAmount * childRetainageBill.SignAmount ?? 0m;

						// Acuminator disable once PX1048 RowChangesInEventHandlersAllowedForArgsOnly [dummyInvoice is local object is used only for intermediate storage of data]
						dummyInvoice.CuryRetainageUnpaidTotal += childRetainageBill.DocBal * sign;
						// Acuminator disable once PX1048 RowChangesInEventHandlersAllowedForArgsOnly [Justification]
						dummyInvoice.CuryRetainagePaidTotal += (dummyInvoice.CuryOrigDocAmt - dummyInvoice.CuryDocBal) * sign;
					}

					document.CuryRetainageUnpaidTotal = document.CuryRetainageUnreleasedAmt + dummyInvoice.CuryRetainageUnpaidTotal;
					document.CuryRetainagePaidTotal = dummyInvoice.CuryRetainagePaidTotal;
				}
			}
		}

		protected virtual void APInvoice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APInvoice doc = (APInvoice)e.Row;

			Terms terms = (Terms)PXSelectorAttribute.Select<APInvoice.termsID>(Base.Document.Cache, doc);

			if (terms != null && doc.RetainageApply == true && terms.InstallmentType == CS.TermsInstallmentType.Multiple)
			{
				sender.RaiseExceptionHandling<APInvoice.termsID>(doc, doc.TermsID, new PXSetPropertyException(Messages.RetainageWithMultipleCreditTerms));
			}

			bool disablePersistingCheckForRetainageAccountAndSub = doc.RetainageApply != true;
			PXDefaultAttribute.SetPersistingCheck<APRegister.retainageAcctID>(sender, doc, disablePersistingCheckForRetainageAccountAndSub
				? PXPersistingCheck.Nothing
				: PXPersistingCheck.NullOrBlank);
			PXDefaultAttribute.SetPersistingCheck<APInvoice.retainageSubID>(sender, doc, disablePersistingCheckForRetainageAccountAndSub
				? PXPersistingCheck.Nothing
				: PXPersistingCheck.NullOrBlank);
		}

		protected virtual void APInvoice_RetainageAcctID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Location vendorLocation = Base.location.View.SelectSingleBound(new[] { e.Row }) as Location;
			if (vendorLocation != null)
			{
				e.NewValue = Base.GetAcctSub<Location.aPRetainageAcctID>(Base.location.Cache, vendorLocation);
			}
		}

		protected virtual void APInvoice_RetainageSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null)
			{
				Location vendorLocation = Base.location.View.SelectSingleBound(new[] { e.Row }) as Location;
				if (vendorLocation != null && e.Row != null)
				{
					e.NewValue = Base.GetAcctSub<Location.aPRetainageSubID>(Base.location.Cache, vendorLocation);
				}
			}
		}

		protected virtual void APInvoice_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<APInvoice.retainageAcctID>(e.Row);
			sender.SetDefaultExt<APInvoice.retainageSubID>(e.Row);
		}

		protected virtual void APInvoice_RetainageApply_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APInvoice document = (APInvoice)e.Row;
			bool? newValue = (bool?)e.NewValue;

			if (document == null) return;

			if (document.RetainageApply == true && newValue == false)
			{
				IEnumerable<APTran> trans = Base.Transactions.Select().AsEnumerable().Where(tran => ((APTran)tran).CuryRetainageAmt != 0 || ((APTran)tran).RetainagePct != 0).RowCast<APTran>();

				if (!trans.Any()) return;

				WebDialogResult wdr =
					Base.Document.Ask(
						Messages.Warning,
						Messages.UncheckApplyRetainage,
						MessageButtons.YesNo,
						MessageIcon.Warning);

				if (wdr == WebDialogResult.Yes)
				{
					foreach (APTran tran in trans)
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

		protected virtual void APTran_SubID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void ClearCurrentDocumentDiscountDetails()
		{
			Base.DiscountDetails
					.Select()
					.RowCast<APInvoiceDiscountDetail>()
					.ForEach(discountDetail => Base.DiscountDetails.Cache.Delete(discountDetail));

			Base.Discount_Row
				.Select()
				.RowCast<APTran>()
				.ForEach(tran => Base.Discount_Row.Cache.Delete(tran));
		}

		#endregion

		#region APInvoiceDiscountDetail events

		[PXOverride]
		public virtual void APInvoiceDiscountDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			APInvoice invoice = Base.Document.Current;

			if (invoice?.RetainageApply == true ||
				invoice?.IsRetainageDocument == true)
			{
				e.Cancel = true;
			}
		}

		public delegate void AddDiscountDelegate(PXCache sender, APInvoice row);

		[PXOverride]
		public void AddDiscount(
			PXCache sender,
			APInvoice row,
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
		// APRetainageInvoice class is a APRegister class alias
		// because only APRegister part is affecting by the release process
		// and only this way we can get a proper behavior for the QueryCache mechanism.
		//
		public PXSelectJoin<APRetainageInvoice,
			InnerJoinSingleTable<APInvoice, On<APInvoice.docType, Equal<APRetainageInvoice.docType>,
				And<APInvoice.refNbr, Equal<APRetainageInvoice.refNbr>>>>,
			Where<APRetainageInvoice.isRetainageDocument, Equal<True>,
				And<APRetainageInvoice.origDocType, Equal<Optional<APInvoice.docType>>,
				And<APRetainageInvoice.origRefNbr, Equal<Optional<APInvoice.refNbr>>>>>> RetainageDocuments;

		[PXCopyPasteHiddenView]
		public PXFilter<RetainageOptions> ReleaseRetainageOptions;

		public PXAction<APInvoice> releaseRetainage;

		[PXUIField(
			DisplayName = "Release Retainage",
			MapEnableRights = PXCacheRights.Update,
			MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[APMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ReleaseRetainage(PXAdapter adapter)
		{
			APInvoice doc = Base.Document.Current;
			
			APRegister reversingDoc;
			if (Base.CheckReversingRetainageDocumentAlreadyExists(Base.Document.Current, out reversingDoc))
			{
				throw new PXException(
					Messages.ReleaseRetainageReversingDocumentExists,
					PXMessages.LocalizeNoPrefix(APDocTypeDict[doc.DocType]),
					PXMessages.LocalizeNoPrefix(APDocTypeDict[reversingDoc.DocType]),
					reversingDoc.RefNbr);
			}

			Base.Save.Press();

			APRetainageRelease retainageGraph = PXGraph.CreateInstance<APRetainageRelease>();

			retainageGraph.Filter.Current.DocDate = Base.Accessinfo.BusinessDate > doc.DocDate
						? Base.Accessinfo.BusinessDate
						: doc.DocDate;

			retainageGraph.Filter.Current.FinPeriodID = Base.FinPeriodRepository.GetPeriodIDFromDate(retainageGraph.Filter.Current.DocDate, FinPeriod.organizationID.MasterValue);
			retainageGraph.Filter.Current.BranchID = doc.BranchID;
			retainageGraph.Filter.Current.OrgBAccountID = PXAccess.GetBranch(doc.BranchID).BAccountID;
			retainageGraph.Filter.Current.VendorID = doc.VendorID;
			retainageGraph.Filter.Current.RefNbr = doc.RefNbr;
			retainageGraph.Filter.Current.ShowBillsWithOpenBalance = doc.OpenDoc == true;

			APInvoiceExt retainageDocToRelease = retainageGraph.DocumentList.SelectSingle();
			if (retainageDocToRelease == null)
			{
				APRetainageInvoice retainageDoc = RetainageDocuments
					.Select()
					.RowCast<APRetainageInvoice>()
					.FirstOrDefault(row => row.Released != true);

				throw new PXException(
					Messages.ReleaseRetainageNotReleasedDocument,
					PXMessages.LocalizeNoPrefix(APDocTypeDict[retainageDoc.DocType]),
					retainageDoc.RefNbr,
					PXMessages.LocalizeNoPrefix(APDocTypeDict[doc.DocType]));
			}

			throw new PXRedirectRequiredException(retainageGraph, nameof(ReleaseRetainage));
		}

		public virtual void ReleaseRetainageProc(List<APInvoiceExt> list, RetainageOptions retainageOpts, bool isAutoRelease = false)
		{
			Base.FieldDefaulting.AddHandler<APTran.subID>((sender, e) => {e.Cancel = true; });
			bool failed = false;
			List<APInvoice> result = new List<APInvoice>();

			foreach (var group in list.GroupBy(row => new { row.DocType, row.RefNbr }))
			{
				APInvoiceExt doc = group.First();
				PXProcessing<APInvoiceExt>.SetCurrentItem(doc);
				decimal curyRetainageSum = group.Sum(row => row.CuryRetainageReleasedAmt ?? 0m);

				try
				{
					Base.Clear(PXClearOption.ClearAll);
					PXUIFieldAttribute.SetError(Base.Document.Cache, null, null, null);

					APTran tranMax = null;
					TaxCalc oldTaxCalc = TaxBaseAttribute.GetTaxCalc<APTran.taxCategoryID>(Base.Transactions.Cache, null);

					Base.Clear(PXClearOption.PreserveTimeStamp);


					// Magic. We need to prevent rewriting of CurrencyInfo.IsReadOnly 
					// by true in CurrencyInfoView
					// 
					Base.CurrentDocument.Cache.AllowUpdate = true;

					PXResult<APInvoice, CurrencyInfo, Terms, Vendor> resultDoc =
						APInvoice_CurrencyInfo_Terms_Vendor
							.SelectSingleBound(Base, null, doc.DocType, doc.RefNbr, doc.VendorID).AsEnumerable()
							.Cast<PXResult<APInvoice, CurrencyInfo, Terms, Vendor>>()
							.First();

					CurrencyInfo info = resultDoc;
					APInvoice origInvoice = resultDoc;
					Vendor vendor = resultDoc;

					CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(info);
					new_info.CuryInfoID = null;
					new_info.IsReadOnly = false;
					new_info = PXCache<CurrencyInfo>.CreateCopy(Base.currencyinfo.Insert(new_info));

					APInvoice invoice = PXCache<APInvoice>.CreateCopy(origInvoice);
					object ownerID = Base.CurrentDocument.Cache.GetValue<APInvoice.employeeID>(origInvoice);
					try
					{
						Base.CurrentDocument.Cache.RaiseFieldVerifying<APInvoice.employeeID>(origInvoice, ref ownerID);
					}
					catch(Exception ex)
					{
						invoice.EmployeeID = null;
					}

					invoice.CuryInfoID = new_info.CuryInfoID;
					if (curyRetainageSum != 0m)
					{
						invoice.DocType = curyRetainageSum < 0m ? APDocType.DebitAdj : APDocType.Invoice;
					}
					invoice.RefNbr = null;
					invoice.LineCntr = null;
					invoice.InvoiceNbr = origInvoice.InvoiceNbr;

					// Must be set for _RowSelected event handler
					// 
					invoice.OpenDoc = true;
					invoice.Released = false;

					Base.Document.Cache.SetDefaultExt<APInvoice.isMigratedRecord>(invoice);
					invoice.BatchNbr = null;
					invoice.PrebookBatchNbr = null;
					invoice.Prebooked = false;
					invoice.ScheduleID = null;
					invoice.Scheduled = false;
					invoice.NoteID = null;

					if (invoice.DocType == APDocType.DebitAdj)
					{
						invoice.TermsID = null;
					}
					invoice.DueDate = null;
					invoice.DiscDate = null;
					invoice.CuryOrigDiscAmt = 0m;
					invoice.OrigDocType = origInvoice.DocType;
					invoice.OrigRefNbr = origInvoice.RefNbr;
					invoice.OrigDocDate = origInvoice.DocDate;

					invoice.PaySel = false;
					invoice.CuryLineTotal = 0m;
					invoice.IsTaxPosted = false;
					invoice.IsTaxValid = false;
					invoice.CuryVatTaxableTotal = 0m;
					invoice.CuryVatExemptTotal = 0m;

					invoice.CuryDetailExtPriceTotal = 0m;
					invoice.DetailExtPriceTotal = 0m;
					invoice.CuryLineDiscTotal = 0m;
					invoice.LineDiscTotal = 0m;

					invoice.CuryDocBal = 0m;
					invoice.CuryOrigDocAmt = Math.Abs(curyRetainageSum);
					if (invoice.IsMigratedRecord == true)
					{
						invoice.CuryInitDocBal = invoice.CuryOrigDocAmt;
					}
					invoice.Hold = !isAutoRelease && Base.apsetup.Current.HoldEntry == true || Base.IsApprovalRequired(invoice);

					invoice.DocDate = retainageOpts.DocDate;
					FinPeriodIDAttribute.SetPeriodsByMaster<APInvoice.finPeriodID>(Base.Document.Cache, invoice, retainageOpts.MasterFinPeriodID);

					Base.ClearRetainageSummary(invoice);
					invoice.RetainageApply = false;
					invoice.IsRetainageDocument = true;

					invoice = Base.Document.Insert(invoice);
					decimal origSign = doc.SignAmount * invoice.SignAmount ?? 0m;

					if (new_info != null)
					{
						CurrencyInfo b_info = (CurrencyInfo)PXSelect<CurrencyInfo,
							Where<CurrencyInfo.curyInfoID, Equal<Current<APInvoice.curyInfoID>>>>.Select(Base);

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
					var retainageDetails = new Dictionary<(string TranType, string RefNbr, int? LineNbr), APTranRetainageData>();

					foreach (APInvoiceExt docLine in group)
					{
						PXProcessing<APInvoiceExt>.SetCurrentItem(docLine);

						PXResultset<APTran> details = isRetainageByLines
							? PXSelect<APTran,
								Where<APTran.tranType, Equal<Required<APTran.tranType>>,
									And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
									And<APTran.lineNbr, Equal<Required<APTran.lineNbr>>>>>>
								.SelectSingleBound(Base, null, docLine.DocType, docLine.RefNbr, docLine.LineNbr)
							: PXSelectGroupBy<APTran,
								Where<APTran.tranType, Equal<Required<APTran.tranType>>,
									And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
									And<APTran.curyRetainageAmt, NotEqual<decimal0>>>>,
								Aggregate<
									GroupBy<APTran.taxCategoryID,
									Sum<APTran.curyRetainageAmt,
									Max<APTran.box1099>>>>,
								OrderBy <Asc<APTran.taxCategoryID>>>
								.Select(Base, docLine.DocType, docLine.RefNbr);

						TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualCalc);

						foreach (APTran detail in details)
						{
							// Create APTran record for chosen retainage amount, 
							// clear all required fields to prevent tax calculation,
							// discount calculation and retainage calculation.
							// CuryUnitCost = 0m and CuryLineAmt = 0m here to prevent their 
							// FieldDefaulting events, because in our case default value 
							// should be equal to zero.
							//
							APTran tranNew = Base.Transactions.Insert(new APTran
							{
								CuryUnitCost = 0m,
								CuryLineAmt = 0m
							});

							tranNew.BranchID = origInvoice.BranchID;
							tranNew.TaxCategoryID = detail.TaxCategoryID;
							tranNew.AccountID = origInvoice.RetainageAcctID;
							tranNew.SubID = origInvoice.RetainageSubID;
							tranNew.ProjectID = PXAccess.FeatureInstalled<FeaturesSet.projectModule>()
								? origInvoice.ProjectID
								: ProjectDefaultAttribute.NonProject();
							tranNew.Box1099 = detail.Box1099;

							tranNew.Qty = 0m;
							tranNew.CuryUnitCost = 0m;
							tranNew.ManualDisc = true;
							tranNew.DiscPct = 0m;
							tranNew.CuryDiscAmt = 0m;
							tranNew.RetainagePct = 0m;
							tranNew.CuryRetainageAmt = 0m;
							tranNew.CuryTaxableAmt = 0m;
							tranNew.CuryTaxAmt = 0;
							tranNew.CuryExpenseAmt = 0m;
							tranNew.GroupDiscountRate = 1m;
							tranNew.DocumentDiscountRate = 1m;

							tranNew.OrigLineNbr = docLine.LineNbr;

							using (new PXLocaleScope(vendor.LocaleName))
							{
								tranNew.TranDesc = PXMessages.LocalizeFormatNoPrefix(
									Messages.RetainageForTransactionDescription,
									APDocTypeDict[origInvoice.DocType],
									origInvoice.RefNbr);
							}

							decimal curyLineAmt = 0m;
							bool isFinalRetainageDetail = docLine.CuryRetainageUnreleasedCalcAmt == 0m;

							if (isFinalRetainageDetail)
							{
								PXResultset<APTran> detailsRetainage = isRetainageByLines
									? PXSelectJoin<APTran,
										InnerJoin<APRegister, On<APRegister.docType, Equal<APTran.tranType>,
											And<APRegister.refNbr, Equal<APTran.refNbr>>>>,
										Where<APRegister.isRetainageDocument, Equal<True>,
											And<APRegister.released, Equal<True>,
											And<APRegister.origDocType, Equal<Required<APRegister.origDocType>>,
											And<APRegister.origRefNbr, Equal<Required<APRegister.origRefNbr>>,
											And<APTran.origLineNbr, Equal<Required<APTran.origLineNbr>>>>>>>>
										.Select(Base, docLine.DocType, docLine.RefNbr, docLine.LineNbr)
									: PXSelectJoin<APTran,
										InnerJoin<APRegister, On<APRegister.docType, Equal<APTran.tranType>,
											And<APRegister.refNbr, Equal<APTran.refNbr>>>>,
										Where<APRegister.isRetainageDocument, Equal<True>,
											And<APRegister.released, Equal<True>,
											And<APRegister.origDocType, Equal<Required<APRegister.origDocType>>,
											And<APRegister.origRefNbr, Equal<Required<APRegister.origRefNbr>>,
											And<Where<APTran.taxCategoryID, Equal<Required<APTran.taxCategoryID>>,
												Or<Required<APTran.taxCategoryID>, IsNull>>>>>>>>
										.Select(Base, docLine.DocType, docLine.RefNbr, detail.TaxCategoryID, detail.TaxCategoryID);

								decimal detailsRetainageSum = 0m;
								foreach (PXResult<APTran, APRegister> res in detailsRetainage)
								{
									APTran detailRetainage = res;
									APRegister docRetainage = res;

									// We should consider amount sign both for Original and Child Retainage document
									// to cover all possible combinations between INV, ADR and ACR documents.
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

							tranNew.CuryLineAmt = curyLineAmt;
							tranNew = Base.Transactions.Update(tranNew);

							tranNew.TaskID = null;

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
									new APTranRetainageData()
									{
										Detail = tranNew,
										RemainAmt = (docLine.CuryRetainageReleasedAmt * invoice.SignAmount) - tranNew.CuryLineAmt,
										IsFinal = isFinalRetainageDetail
									});
							}
							else if (tranMax == null || Math.Abs(tranMax.CuryLineAmt ?? 0m) < Math.Abs(tranNew.CuryLineAmt ?? 0m))
							{
								tranMax = tranNew;
							}
						}

						PXProcessing<APInvoiceExt>.SetProcessed();
					}

					ClearCurrentDocumentDiscountDetails();

					// We should copy all taxes from the original document
					// because it is possible to add or delete them.
					// 
					var taxes = PXSelectJoin<APTaxTran,
						LeftJoin<Tax, On<Tax.taxID, Equal<APTaxTran.taxID>>>,
						Where<APTaxTran.module, Equal<BatchModule.moduleAP>,
							And<APTaxTran.tranType, Equal<Required<APTaxTran.tranType>>,
							And<APTaxTran.refNbr, Equal<Required<APTaxTran.refNbr>>,
							And<APTaxTran.curyRetainedTaxAmt, NotEqual<decimal0>>>>>>
						.Select(Base, group.Key.DocType, group.Key.RefNbr);

					// Insert taxes first and only after that copy 
					// all needed values to prevent tax recalculation
					// during the next tax insertion.
					// 
					Dictionary<string, APTaxTran> insertedTaxes = null;
					insertedTaxes = new Dictionary<string, APTaxTran>();
					taxes.RowCast<APTaxTran>().ForEach(tax => insertedTaxes.Add(tax.TaxID, Base.Taxes.Insert(new APTaxTran() { TaxID = tax.TaxID })));

					foreach (PXResult<APTaxTran, Tax> res in taxes)
					{
						APTaxTran origAPTaxTran = res;
						Tax tax = res;

						APTaxTran new_aptaxtran = insertedTaxes[origAPTaxTran.TaxID];
						if (new_aptaxtran == null || 
							new_aptaxtran.CuryTaxableAmt == 0m && 
							new_aptaxtran.CuryTaxAmt == 0m && 
							new_aptaxtran.CuryExpenseAmt == 0m) continue;

						APReleaseProcess.AdjustTaxCalculationLevelForNetGrossEntryMode(invoice, null, ref tax);
						decimal curyTaxAmt = 0m;

						if (isRetainageByLines)
						{
							foreach (APTax aptax in Base.Tax_Rows.Select()
								.RowCast<APTax>()
								.Where(row => row.TaxID == origAPTaxTran.TaxID))
							{
								APTranRetainageData retainageDetail = retainageDetails[(aptax.TranType, aptax.RefNbr, aptax.LineNbr)];
								decimal detailCuryTaxAmt = 0m;

								APTax origAPTax = PXSelect<APTax,
									Where<APTax.tranType, Equal<Required<APTax.tranType>>,
										And<APTax.refNbr, Equal<Required<APTax.refNbr>>,
										And<APTax.lineNbr, Equal<Required<APTax.lineNbr>>,
										And<APTax.taxID, Equal<Required<APTax.taxID>>>>>>>
									.SelectSingleBound(Base, null, group.Key.DocType, group.Key.RefNbr, retainageDetail.Detail.OrigLineNbr, aptax.TaxID);

								if (retainageDetail.IsFinal)
								{
									PXResultset<APTax> taxDetailsRetainage = PXSelectJoin<APTax,
										InnerJoin<APRegister, On<APRegister.docType, Equal<APTax.tranType>,
											And<APRegister.refNbr, Equal<APTax.refNbr>>>,
										InnerJoin<APTran, On<APTran.tranType, Equal<APTax.tranType>,
											And<APTran.refNbr, Equal<APTax.refNbr>,
											And<APTran.lineNbr, Equal<APTax.lineNbr>>>>>>,
										Where<APRegister.isRetainageDocument, Equal<True>,
											And<APRegister.released, Equal<True>,
											And<APRegister.origDocType, Equal<Required<APRegister.origDocType>>,
											And<APRegister.origRefNbr, Equal<Required<APRegister.origRefNbr>>,
											And<APTran.origLineNbr, Equal<Required<APTran.origLineNbr>>,
											And<APTax.taxID, Equal<Required<APTax.taxID>>>>>>>>>
										.Select(Base, invoice.OrigDocType, invoice.OrigRefNbr, retainageDetail.Detail.OrigLineNbr, aptax.TaxID);

									decimal taxDetailsRetainageSum = 0m;
									foreach (PXResult<APTax, APRegister> resTaxDetailsRetainage in taxDetailsRetainage)
									{
										APTax taxDetailRetainage = resTaxDetailsRetainage;
										APRegister docRetainage = resTaxDetailsRetainage;

										// We should consider amount sign both for Original and Child Retainage document
										// to cover all possible combinations between INV, ADR and ACR documents.
										decimal sign = doc.SignAmount * docRetainage.SignAmount ?? 0m;
										taxDetailsRetainageSum += ((taxDetailRetainage.CuryTaxAmt ?? 0m) + (taxDetailRetainage.CuryExpenseAmt ?? 0m)) * sign;
									}

									detailCuryTaxAmt = ((origAPTax.CuryRetainedTaxAmt ?? 0m) - taxDetailsRetainageSum) * origSign;
								}
								else
								{
									decimal retainedPercent = Math.Abs((decimal)origAPTax.CuryRetainedTaxAmt / (decimal)origAPTax.CuryRetainedTaxableAmt);
									detailCuryTaxAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury((decimal)aptax.CuryTaxableAmt * retainedPercent);
								}

								curyTaxAmt += detailCuryTaxAmt;

								APTax new_aptax = PXCache<APTax>.CreateCopy(aptax);
								decimal detailDeductiblePercent = 100m - (new_aptax.NonDeductibleTaxRate ?? 100m);
								new_aptax.CuryExpenseAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(detailCuryTaxAmt * detailDeductiblePercent / 100m);
								new_aptax.CuryTaxAmt = detailCuryTaxAmt - new_aptax.CuryExpenseAmt;
								new_aptax = Base.Tax_Rows.Update(new_aptax);

								if (APReleaseProcess.IncludeTaxInLineBalance(tax))
								{
									retainageDetail.RemainAmt -= detailCuryTaxAmt;
								}
							}
						}
						else
						{
							if (isFinalRetainageDoc)
							{
								PXResultset<APTaxTran> taxDetailsRetainage = PXSelectJoin<APTaxTran,
									InnerJoin<APRegister, On<APRegister.docType, Equal<APTaxTran.tranType>,
										And<APRegister.refNbr, Equal<APTaxTran.refNbr>>>>,
									Where<APRegister.isRetainageDocument, Equal<True>,
										And<APRegister.released, Equal<True>,
										And<APRegister.origDocType, Equal<Required<APRegister.origDocType>>,
										And<APRegister.origRefNbr, Equal<Required<APRegister.origRefNbr>>,
										And<APTaxTran.taxID, Equal<Required<APTaxTran.taxID>>>>>>>>
									.Select(Base, origAPTaxTran.TranType, origAPTaxTran.RefNbr, origAPTaxTran.TaxID);

								decimal taxDetailsRetainageSum = 0m;
								foreach (PXResult<APTaxTran, APRegister> resTaxDetailsRetainage in taxDetailsRetainage)
								{
									APTaxTran taxDetailRetainage = resTaxDetailsRetainage;
									APRegister docRetainage = resTaxDetailsRetainage;

									// We should consider amount sign both for Original and Child Retainage document
									// to cover all possible combinations between INV, ADR and ACR documents.
									decimal sign = doc.SignAmount * docRetainage.SignAmount ?? 0m;
									taxDetailsRetainageSum += ((taxDetailRetainage.CuryTaxAmt ?? 0m) + (taxDetailRetainage.CuryExpenseAmt ?? 0m)) * sign;
								}

								curyTaxAmt = ((origAPTaxTran.CuryRetainedTaxAmt ?? 0m) - taxDetailsRetainageSum) * origSign;
							}
							else
							{
								APTax retainedTaxableSum = PXSelectGroupBy<APTax,
									Where<APTax.tranType, Equal<Required<APTax.tranType>>,
										And<APTax.refNbr, Equal<Required<APTax.refNbr>>,
										And<APTax.taxID, Equal<Required<APTax.taxID>>>>>,
									Aggregate<
										GroupBy<APTax.tranType,
										GroupBy<APTax.refNbr,
										GroupBy<APTax.taxID,
										Sum<APTax.curyRetainedTaxableAmt>>>>>>
									.SelectSingleBound(Base, null, origAPTaxTran.TranType, origAPTaxTran.RefNbr, origAPTaxTran.TaxID);

								decimal retainedPercent = Math.Abs((decimal)origAPTaxTran.CuryRetainedTaxAmt / (decimal)retainedTaxableSum.CuryRetainedTaxableAmt);
								curyTaxAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury( (decimal)new_aptaxtran.CuryTaxableAmt * retainedPercent);
							}
						}

						new_aptaxtran = PXCache<APTaxTran>.CreateCopy(new_aptaxtran);

						// We should adjust APTax taxable amount for inclusive tax, 
						// because it used during the release process to post correct 
						// amount on Expense account for each APTran record. 
						// See APReleaseProcess.GetExpensePostingAmount method for details.
						// 
						decimal taxDiff = (new_aptaxtran.CuryTaxAmt ?? 0m) + (new_aptaxtran.CuryExpenseAmt ?? 0m) - curyTaxAmt;
						if (tax?.IsRegularInclusiveTax() == true && taxDiff != 0m)
						{
							new_aptaxtran.CuryTaxableAmt += taxDiff;

							foreach (APTax roundAPTax in Base.Tax_Rows.Select()
								.AsEnumerable().RowCast<APTax>()
								.Where(row => row.TaxID == new_aptaxtran.TaxID))
							{
								APTax roundTaxDetail = PXCache<APTax>.CreateCopy(roundAPTax);
								roundTaxDetail.CuryTaxableAmt += taxDiff;
								roundTaxDetail = Base.Tax_Rows.Update(roundTaxDetail);

								foreach (APTax lineAPTax in Base.Tax_Rows.Select()
									.AsEnumerable().RowCast<APTax>()
									.Where(row => row.TaxID != roundAPTax.TaxID && row.LineNbr == roundAPTax.LineNbr))
								{
									APTaxTran lineAPTaxTran = insertedTaxes[lineAPTax.TaxID];
									lineAPTaxTran.CuryTaxableAmt += taxDiff;
									lineAPTaxTran = Base.Taxes.Update(lineAPTaxTran);

									APTax lineTaxDetail = PXCache<APTax>.CreateCopy(lineAPTax);
									lineTaxDetail.CuryTaxableAmt += taxDiff;
									lineTaxDetail = Base.Tax_Rows.Update(lineTaxDetail);
								}
							}
						}

						new_aptaxtran.TaxRate = origAPTaxTran.TaxRate;
						decimal deductiblePercent = 100m - (new_aptaxtran.NonDeductibleTaxRate ?? 100m);
						new_aptaxtran.CuryExpenseAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury( curyTaxAmt * deductiblePercent / 100m);
						new_aptaxtran.CuryTaxAmt = curyTaxAmt - new_aptaxtran.CuryExpenseAmt;
						// !!! after implementing 'Inclusive input VAT on document level - AP' feature, the CuryTaxAmtSumm field should be calculated in a separate way
						new_aptaxtran.CuryTaxAmtSumm = new_aptaxtran.CuryTaxAmt;
						new_aptaxtran = Base.Taxes.Update(new_aptaxtran);
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

					if (invoice.CuryTaxAmt != invoice.CuryTaxTotal)
					{
						invoice.CuryTaxAmt = invoice.CuryTaxTotal;
						invoice = Base.Document.Update(invoice);
					}

					TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID>(Base.Transactions.Cache, null, oldTaxCalc);

					if (!isAutoRelease && Base.apsetup.Current.HoldEntry == false && Base.IsApprovalRequired(invoice))
					{
						Base.releaseFromHold.Press();
					}

					Base.Save.Press();

					if (isAutoRelease && invoice.Hold != true)
					{
						using (new PXTimeStampScope(null))
						{
							APDocumentRelease.ReleaseDoc(new List<APRegister> { invoice }, false);
						}
					}
				}
				catch (PXException exc)
				{
					PXProcessing<APInvoiceExt>.SetError(exc);
					failed = true;
				}
			}

			if (failed)
			{
				throw new PXOperationCompletedWithErrorException(GL.Messages.DocumentsNotReleased);
			}
		}

		public class APTranRetainageData
		{
			public APTran Detail;
			public decimal? RemainAmt;
			public bool IsFinal;
		}

		private void ProcessRoundingDiff(decimal diff, APTran tran)
		{
			tran.CuryLineAmt += diff;
			tran = Base.Transactions.Update(tran);

			foreach (var group in Base.Tax_Rows.Select()
				.AsEnumerable().RowCast<APTax>()
				.Where(row => row.LineNbr == tran.LineNbr)
				.GroupBy(row => new { row.TranType, row.RefNbr, row.TaxID }))
			{
				foreach (APTax taxDetail in group)
				{
					APTax newTaxDetail = PXCache<APTax>.CreateCopy(taxDetail);
					newTaxDetail.CuryTaxableAmt += diff;
					newTaxDetail = Base.Tax_Rows.Update(newTaxDetail);
				}

				APTaxTran taxSum = PXSelect<APTaxTran,
					Where<APTaxTran.tranType, Equal<Required<APTaxTran.tranType>>,
						And<APTaxTran.refNbr, Equal<Required<APTaxTran.refNbr>>,
						And<APTaxTran.taxID, Equal<Required<APTaxTran.taxID>>>>>>
					.SelectSingleBound(Base, null, group.Key.TranType, group.Key.RefNbr, group.Key.TaxID);
				if (taxSum != null)
				{
					APTaxTran newTaxSum = PXCache<APTaxTran>.CreateCopy(taxSum);
					newTaxSum.CuryTaxableAmt += diff;
					newTaxSum = Base.Taxes.Update(newTaxSum);
				}
			}
		}

		public PXAction<APInvoice> ViewRetainageDocument;

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
	}

	public class APInvoiceEntryRetainage_Workflow : PXGraphExtension<APInvoiceEntry_Workflow, APInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.retainage>();
		}

		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<APInvoiceEntry, APInvoice>());

		protected static void Configure(WorkflowContext<APInvoiceEntry, APInvoice> context)
		{
			var apconditions = context.Conditions.GetPack<APInvoiceEntry_Workflow.Conditions>();
			
			var processingCategory = context.Categories.Get(categoryName: APInvoiceEntry_Workflow.ActionCategory.Processing);

			Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();

			var conditions = new
			{
				IsNotRetenageApplied
					= Bql<Not<APRegister.docType.IsIn<APDocType.invoice, APDocType.debitAdj>
						.And<APRegister.released.IsEqual<True>>
						.And<APRegister.retainageApply.IsEqual<True>>>>(),
				IsNotRetenageDoc = Bql<APRegister.docType.IsNotIn<APDocType.invoice, APDocType.debitAdj>>(),

			}.AutoNameConditions();

			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.WithActions(actions =>
					{
						actions.Add<APInvoiceEntryRetainage>(g => g.releaseRetainage,
							c => c.InFolder(processingCategory, g => g.payInvoice)
								.PlaceAfter(g => g.payInvoice)
								.IsHiddenWhen(conditions.IsNotRetenageDoc || apconditions.IsMigrationMode)
								.IsDisabledWhen(conditions.IsNotRetenageApplied));
					});
			});
		}
	}
}
