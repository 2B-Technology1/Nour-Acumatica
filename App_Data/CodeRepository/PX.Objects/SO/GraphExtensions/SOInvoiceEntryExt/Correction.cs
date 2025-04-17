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
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.Common.Exceptions;
using PX.Objects.CS;

using ARInvoiceAdjusted = PX.Objects.AR.Standalone.ARInvoiceAdjusted;

namespace PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt
{
	public class Correction : PXGraphExtension<SOInvoiceEntry>
	{
		private bool CancellationInvoiceCreationOnRelease = false;

		public PXAction<ARInvoice> cancelInvoice;
		[PXUIField(DisplayName = Messages.CancelInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable CancelInvoice(PXAdapter adapter)
		{
			return CancelSOInvoice(adapter, false);
		}

		public PXAction<ARInvoice> reverseDirectInvoice;
		[PXUIField(DisplayName = Messages.ReverseInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable ReverseDirectInvoice(PXAdapter adapter)
		{
			return CancelSOInvoice(adapter, true);
		}

		public PXAction<ARInvoice> correctInvoice;
		[PXUIField(DisplayName = Messages.CorrectInvoice, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable CorrectInvoice(PXAdapter adapter)
		{
			if (Base.Document.Current == null)
			{
				return adapter.Get();
			}
			Base.Document.Cache.MarkUpdated(Base.Document.Current);
			Base.Save.Press();

			EnsureCanCancel(Base.Document.Current, true, false);

			return Base.ReverseDocumentAndApplyToReversalIfNeeded(adapter,
				new ReverseInvoiceArgs
				{
					ApplyToOriginalDocument = false,
					PreserveOriginalDocumentSign = true,
					DateOption = ReverseInvoiceArgs.CopyOption.SetDefault,
					CurrencyRateOption = ReverseInvoiceArgs.CopyOption.SetDefault
				});
		}

		protected virtual void _(Events.RowSelected<ARInvoice> e)
		{
			bool canCancelCorrect =
				e.Row?.DocType == ARDocType.Invoice
				&& e.Row.Released == true
				&& e.Row.IsUnderCorrection == false;

			cancelInvoice.SetEnabled(canCancelCorrect);
			correctInvoice.SetEnabled(canCancelCorrect);
			reverseDirectInvoice.SetEnabled(canCancelCorrect);

			Base.Actions[SOInvoiceEntry_Workflow.ActionCategories.CorrectionsCategoryID]?.SetVisible(nameof(CancelInvoice), e.Row?.DocType == ARDocType.Invoice);
			Base.Actions[SOInvoiceEntry_Workflow.ActionCategories.CorrectionsCategoryID]?.SetVisible(nameof(CorrectInvoice), e.Row?.DocType == ARDocType.Invoice);
			Base.Actions[SOInvoiceEntry_Workflow.ActionCategories.CorrectionsCategoryID]?.SetVisible(nameof(ReverseDirectInvoice), false); //set the correct condition
		}

		[Obsolete("Event handler is kept to avoid breaking changes.")]
		protected virtual void _(Events.RowPersisted<ARInvoice> e)
		{
		}

		[PXOverride]
		public void Persist(Action basePersist)
		{
			foreach (ARInvoice row in Base.Document.Cache.Inserted.Concat_(Base.Document.Cache.Deleted))
			{
				PXEntryStatus status = Base.Document.Cache.GetStatus(row);
				bool insert = status == PXEntryStatus.Inserted;
				bool delete = status == PXEntryStatus.Deleted;
				if ((insert || delete)
					&& (row.IsCorrection == true || row.IsCancellation == true) && row.OrigDocType != null && row.OrigRefNbr != null
				&& !this.CancellationInvoiceCreationOnRelease)
			{
					var cache = Base.Caches<ARInvoiceAdjusted>();
					ARInvoiceAdjusted arInvoiceAdjusted = SelectFrom<ARInvoiceAdjusted>.
						Where<ARInvoiceAdjusted.docType.IsEqual<ARInvoice.origDocType.FromCurrent>.
							And<ARInvoiceAdjusted.refNbr.IsEqual<ARInvoice.origRefNbr.FromCurrent>>>.
						View.SelectSingleBound(Base, new object[] { row });

					bool? origIsUnderCorrection = (bool?) cache.GetValueOriginal<ARInvoiceAdjusted.isUnderCorrection>(arInvoiceAdjusted);
					if (origIsUnderCorrection == insert)
				{
						throw new PXLockViolationException(typeof(ARInvoice), PXDBOperation.Update, new[] { row.OrigDocType, row.OrigRefNbr });
					}

					arInvoiceAdjusted.IsUnderCorrection = insert;
					cache.Update(arInvoiceAdjusted);
				}
			}

			basePersist();
		}

		protected virtual IEnumerable CancelSOInvoice(PXAdapter adapter, bool allowDirectSales)
		{
			if (Base.Document.Current == null)
			{
				return adapter.Get();
			}
			Base.Document.Cache.MarkUpdated(Base.Document.Current);
			Base.Save.Press();

			EnsureCanCancel(Base.Document.Current, false, allowDirectSales);

			ReverseInvoiceArgs reverseArgs = PrepareReverseInvoiceArgs(allowDirectSales);

			return Base.ReverseDocumentAndApplyToReversalIfNeeded(adapter, reverseArgs);
		}

		protected virtual ReverseInvoiceArgs PrepareReverseInvoiceArgs(bool reverseINTransaction)
		{
			var reverseArgs = new ReverseInvoiceArgs
			{
				ApplyToOriginalDocument = true,
				ReverseINTransaction = reverseINTransaction
			};

			if (this.CancellationInvoiceCreationOnRelease)
			{
				var existingCorrectionInvoiceSet = (PXResult<ARInvoice, CurrencyInfo>)
					PXSelectReadonly2<ARInvoice,
					InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARInvoice.curyInfoID>>>,
					Where<ARInvoice.origDocType, Equal<Current<ARInvoice.docType>>,
						And<ARInvoice.origRefNbr, Equal<Current<ARInvoice.refNbr>>,
						And<ARInvoice.isCorrection, Equal<True>>>>>
					.Select(Base);
				ARInvoice existingCorrectionInvoice = existingCorrectionInvoiceSet;
				CurrencyInfo currencyInfo = existingCorrectionInvoiceSet;

				if (existingCorrectionInvoice == null)
					throw new RowNotFoundException(Base.Document.Cache, Base.Document.Current.DocType, Base.Document.Current.RefNbr);
				reverseArgs.DateOption = ReverseInvoiceArgs.CopyOption.Override;
				reverseArgs.DocumentDate = existingCorrectionInvoice.DocDate;
				reverseArgs.DocumentFinPeriodID = existingCorrectionInvoice.FinPeriodID;
				reverseArgs.CurrencyRateOption = ReverseInvoiceArgs.CopyOption.Override;
				reverseArgs.CurrencyRate = currencyInfo;
				reverseArgs.OverrideDocumentHold = false;
				
				using (new PXLocaleScope(Base.customer.Current.LocaleName))
				{
					reverseArgs.OverrideDocumentDescr = PXMessages.LocalizeFormatNoPrefixNLA(Messages.CorrectionOfInvoice, Base.Document.Current.RefNbr);
				}
			}

			return reverseArgs;
		}

		public virtual void EnsureCanCancel(ARInvoice doc, bool isCorrection, bool allowDirectSales)
		{
			if (doc.DocType != ARDocType.Invoice)
			{
				throw new PXException(Messages.CantCancelDocType, doc.DocType);
			}

			if (doc.InstallmentCntr > 0)
			{
				throw new PXException(Messages.CantCancelMultipleInstallmentsInvoice);
			}

			var arAdjustGroups = PXSelectGroupBy<ARAdjust,
				Where<ARAdjust.adjdDocType, Equal<Current<ARInvoice.docType>>, And<ARAdjust.adjdRefNbr, Equal<Current<ARInvoice.refNbr>>,
					And<ARAdjust.voided, NotEqual<True>>>>,
				Aggregate<
					GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr, GroupBy<ARAdjust.released,
					Sum<ARAdjust.curyAdjdAmt>>>>>>
				.SelectMultiBound(Base, new[] { doc })
				.RowCast<ARAdjust>().ToList();

			if (arAdjustGroups.Any(a => a.Released == false))
			{
				throw new PXException(Messages.CantCancelInvoiceWithUnreleasedApplications);
			}

			var nonReversedCreditMemo = arAdjustGroups.FirstOrDefault(a => a.CuryAdjdAmt != 0m && a.AdjgDocType == ARDocType.CreditMemo);
			if (nonReversedCreditMemo != null)
			{
				throw new PXException(Messages.CantCancelInvoiceWithCM, nonReversedCreditMemo.AdjdRefNbr, nonReversedCreditMemo.AdjgRefNbr);
			}

			var nonReversedApplication = arAdjustGroups.FirstOrDefault(a => a.CuryAdjdAmt != 0m);
			if (nonReversedApplication != null)
			{
				throw new PXException(Messages.CantCancelInvoiceWithPayment, nonReversedApplication.AdjdRefNbr, nonReversedApplication.AdjgRefNbr);
			}

			if (!allowDirectSales)
			{
				ARTran directSale = PXSelectReadonly<ARTran,
					Where<ARTran.tranType, Equal<Current<ARInvoice.docType>>, And<ARTran.refNbr, Equal<Current<ARInvoice.refNbr>>,
						And<ARTran.invtMult, NotEqual<short0>, And<ARTran.lineType, Equal<SOLineType.inventory>>>>>>
					.SelectSingleBound(Base, new[] { doc });
				if (directSale != null)
				{
					throw new PXException(Messages.CantCancelInvoiceWithDirectStockSales, doc.RefNbr);
				}
			}

			SOOrderShipment notRequireShipment = PXSelectReadonly<SOOrderShipment,
				Where<SOOrderShipment.invoiceType, Equal<Current<ARInvoice.docType>>, And<SOOrderShipment.invoiceNbr, Equal<Current<ARInvoice.refNbr>>,
					And<SOOrderShipment.shipmentNbr, Equal<Constants.noShipmentNbr>>>>>
				.SelectSingleBound(Base, new[] { doc });
			if (notRequireShipment != null)
			{
				throw new PXException(Messages.CantCancelInvoiceWithOrdersNotRequiringShipments, doc.RefNbr);
			}

			SOOrder notClosedRmaOrder =
				SelectFrom<SOOrder>.
				InnerJoin<SOLine>.On<SOLine.FK.Order>.
				Where<
					SOLine.FK.Invoice.SameAsCurrent.
					And<SOOrder.behavior.IsEqual<SOBehavior.rM>>>.
				View.SelectSingleBound(Base, new[] { doc });
			if (notClosedRmaOrder != null)
			{
				throw new PXException(Messages.CantCancelInvoiceWithNotCompletedCertainOrderType, doc.RefNbr, notClosedRmaOrder.OrderNbr, notClosedRmaOrder.OrderType);
			}
		}

		[PXOverride]
		public virtual bool AskUserApprovalIfReversingDocumentAlreadyExists(ARInvoice origDoc, Func<ARInvoice, bool> baseImpl)
		{
			if (CancellationInvoiceCreationOnRelease)
				return true;
			return baseImpl(origDoc);
		}

		[PXOverride]
		public virtual ARInvoice CreateReversalARInvoice(ARInvoice doc, ReverseInvoiceArgs reverseArgs, Func<ARInvoice, ReverseInvoiceArgs, ARInvoice> baseMethod)
		{
			var result = baseMethod(doc, reverseArgs);

			if (reverseArgs.PreserveOriginalDocumentSign)
			{
				result.IsCorrection = true;
			}
			else
			{
				result.IsCancellation = true;

				if (this.CancellationInvoiceCreationOnRelease == true)
				{
				result.DontPrint = true;
				result.DontEmail = true;
			}
				else
				{
					result.DontPrint = null;
					result.DontEmail = null;
				}
			}

			return result;
		}

		[PXOverride]
		public virtual ARTran CreateReversalARTran(ARTran srcTran, ReverseInvoiceArgs reverseArgs, Func<ARTran, ReverseInvoiceArgs, ARTran> baseMethod)
		{
			if (srcTran.LineType == SOLineType.Freight) return null;

			var ret = baseMethod(srcTran, reverseArgs);
			ret.OrigInvoiceType = srcTran.TranType;
			ret.OrigInvoiceNbr = srcTran.RefNbr;
			ret.OrigInvoiceLineNbr = srcTran.LineNbr;

			if (!reverseArgs.PreserveOriginalDocumentSign)
			{
				ret.TranCost = srcTran.TranCost;
				ret.TranCostOrig = srcTran.TranCostOrig;
				ret.IsTranCostFinal = srcTran.IsTranCostFinal;
			}

			return ret;
		}

		[PXOverride]
		public virtual void InsertReversedTransactionDetails(ARRegister doc, ReverseInvoiceArgs reverseArgs, Dictionary<int?, int?> origLineNbrsDict, Action<ARRegister, ReverseInvoiceArgs, Dictionary<int?, int?>> baseMethod)
		{
			baseMethod(doc, reverseArgs, origLineNbrsDict);

			Action<ARTran, string, string, int?> SetLinkToOrigFreightTran = delegate (ARTran row, string origInvoiceType, string origInvoiceNbr, int? origInvoiceLineNbr)
			{
				row.OrigInvoiceType = origInvoiceType;
				row.OrigInvoiceNbr = origInvoiceNbr;
				row.OrigInvoiceLineNbr = origInvoiceLineNbr;
			};

			foreach (SOFreightDetail freight in Base.FreightDetails.View.SelectMultiBound(new[] { (ARInvoice)doc }))
			{
				SOFreightDetail newfreight = PXCache<SOFreightDetail>.CreateCopy(freight);

				ARTran origFreightTran = Base.GetFreightTran(doc.DocType, doc.RefNbr, freight);
				string origInvoiceType = origFreightTran?.TranType;
				string origInvoiceNbr = origFreightTran?.RefNbr;
				int? origInvoiceLineNbr = origFreightTran?.LineNbr;

				newfreight.DocType = null;
				newfreight.RefNbr = null;
				newfreight.CuryInfoID = null;
				newfreight.NoteID = null;

				try
				{
					Base.RowInserted.AddHandler<ARTran>((cache, args) => SetLinkToOrigFreightTran(args.Row as ARTran, origInvoiceType, origInvoiceNbr, origInvoiceLineNbr));
					newfreight = Base.FreightDetails.Insert(newfreight);
				}
				finally
				{
					Base.RowInserted.RemoveHandler<ARTran>((cache, args) => SetLinkToOrigFreightTran(args.Row as ARTran, origInvoiceType, origInvoiceNbr, origInvoiceLineNbr));
				}
			}
		}

		[PXOverride]
		public virtual void ReverseInvoiceProc(ARRegister doc, ReverseInvoiceArgs reverseArgs, Action<ARRegister, ReverseInvoiceArgs> baseMethod)
		{
			baseMethod(doc, reverseArgs);
		}

		[PXOverride]
		public virtual ARInvoiceState GetDocumentState(PXCache cache, ARInvoice doc, Func<PXCache, ARInvoice, ARInvoiceState> baseMethod)
		{
			ARInvoiceState state = baseMethod(cache, doc);

			state.IsCancellationDocument = doc.IsCancellation == true;
			state.IsCorrectionDocument = doc.IsCorrection == true;
			state.ShouldDisableHeader |= state.IsCancellationDocument;
			state.IsTaxZoneIDEnabled &= !state.IsCancellationDocument;
			state.IsAvalaraCustomerUsageTypeEnabled &= !state.IsCancellationDocument;
			state.IsAssignmentEnabled &= !state.IsCancellationDocument;
			state.AllowDeleteDocument |= state.IsCancellationDocument && !state.IsDocumentReleased;
			state.DocumentHoldEnabled |= state.IsCancellationDocument && !state.IsDocumentReleased;
			state.DocumentDateEnabled |= state.IsCancellationDocument && !state.IsDocumentReleased;
			state.DocumentDescrEnabled |= state.IsCancellationDocument && !state.IsDocumentReleased;

			state.BalanceBaseCalc |= state.IsCancellationDocument && !state.IsDocumentReleased;
			state.AllowDeleteTransactions &= !state.IsCancellationDocument && !state.IsCorrectionDocument;
			state.AllowUpdateTransactions &= !state.IsCancellationDocument;
			state.AllowInsertTransactions &= !state.IsCancellationDocument && !state.IsCorrectionDocument;
			state.AllowDeleteTaxes &= !state.IsCancellationDocument;
			state.AllowUpdateTaxes &= !state.IsCancellationDocument;
			state.AllowInsertTaxes &= !state.IsCancellationDocument;
			state.AllowDeleteDiscounts &= !state.IsCancellationDocument;
			state.AllowUpdateDiscounts &= !state.IsCancellationDocument;
			state.AllowInsertDiscounts &= !state.IsCancellationDocument;
			state.AllowUpdateCMAdjustments &= !state.IsCancellationDocument;

			return state;
		}

		/// <summary>
		/// Overrides <see cref="SOInvoiceEntry.ReleaseInvoiceProc"/>
		/// </summary>
		[PXOverride]
		public virtual void ReleaseInvoiceProc(List<ARRegister> list, bool isMassProcess, Action<List<ARRegister>, bool> baseMethod)
		{
			list = list
				.Select((doc, index) => CreateAndReleaseCancellationInvoice(doc, index, isMassProcess) ? doc : null)
				.ToList();

			list = list
				.Select((doc, index) => ReleaseOnSeparateTransaction(doc, index, isMassProcess) ? null : doc)
				.ToList();

			if(list.Any(x => x != null))
			baseMethod(list, isMassProcess);
		}

		protected virtual bool CreateAndReleaseCancellationInvoice(ARRegister doc, int index, bool isMassProcess)
		{
			if (doc?.IsCorrection != true) return true;

			try
			{
				var invoiceGraph = PXGraph.CreateInstance<SOInvoiceEntry>();

				ARInvoice existingCancellationInvoice = PXSelect<ARInvoice,
					Where<ARInvoice.origDocType, Equal<Current<ARInvoice.origDocType>>,
						And<ARInvoice.origRefNbr, Equal<Current<ARInvoice.origRefNbr>>,
						And<ARInvoice.isCancellation, Equal<True>>>>>
					.SelectSingleBound(invoiceGraph, new[] { doc });
				if (existingCancellationInvoice != null)
				{
					if (existingCancellationInvoice.Released != true)
					{
						throw new PXException(Messages.CancellationInvoiceExists, existingCancellationInvoice.RefNbr, doc.RefNbr);
					}
					else
					{
						return true;
					}
				}

				invoiceGraph.GetExtension<Correction>().CancellationInvoiceCreationOnRelease = true;
				invoiceGraph.Document.Current = invoiceGraph.Document.Search<ARInvoice.refNbr>(doc.OrigRefNbr, doc.OrigDocType);
				invoiceGraph.Actions[nameof(cancelInvoice)].Press();

				using (var scope = new PXTransactionScope())
				{
					invoiceGraph.Save.Press();

					invoiceGraph.ReleaseInvoiceProcImpl(
						new List<ARRegister> { invoiceGraph.Document.Current },
						isMassProcess: false);

					scope.Complete();
				}

				return true;
			}
			catch (PXException e) when (isMassProcess)
			{
				PXProcessing<ARRegister>.SetError(index, e);
				return false;
			}
		}

		protected virtual bool ReleaseOnSeparateTransaction(ARRegister doc, int index, bool isMassProcess)
        {
			if (doc?.IsCancellation != true)
				return false;

			try
			{
				using (var scope = new PXTransactionScope())
				{
					var docs = new ARRegister[index + 1].ToList();
					docs[index] = doc;

					Base.ReleaseInvoiceProcImpl(docs, isMassProcess);
					scope.Complete();
				}
			}
			catch (PXOperationCompletedWithErrorException) when (isMassProcess)
			{
			}
			return true;
		}
	}
}
