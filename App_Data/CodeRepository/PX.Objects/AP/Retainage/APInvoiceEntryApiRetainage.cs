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
using System.Linq;

using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.GL;

using CommonMessages = PX.Objects.Common.Messages;

namespace PX.Objects.AP
{
	public sealed class APInvoiceEntryApiRetainage : PXGraphExtension<APInvoiceEntry>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.retainage>();

		public PXFilter<RetainageOptionsFilter> ReleaseRetainageFilter;

		public PXAction<APInvoice> releaseRetainageAmount;

		[PXButton]
		[PXUIField(DisplayName = "Release Retainage Amount", Visible = false)]
		public IEnumerable ReleaseRetainageAmount(PXAdapter adapter)
		{
			var retainageOptions = ReleaseRetainageFilter.Current;
			var invoice = Base.Document.Current;
			ValidateOptionsAndInvoice(retainageOptions, invoice);

			var retainageGraph = InitRetainageGraph(invoice);
			bool isAutoRelease = retainageGraph.APSetup.Current?.RetainageBillsAutoRelease == true;

			var graphInvoices = retainageGraph.DocumentList;
			var invoicesToRelease = graphInvoices
				.Select()
				.RowCast<APInvoiceExt>()
				.ToList();

			if (retainageOptions.AmountToRelease != null)
			{
				foreach (var invoiceToRelease in invoicesToRelease)
				{
					graphInvoices.Cache.SetValueExt<APInvoiceExt.curyRetainageReleasedAmt>(
						invoiceToRelease, retainageOptions.AmountToRelease);
				}
			}

			var graphFilter = retainageGraph.Filter;
			try
			{
				graphFilter.Cache.SetValueExt<APRetainageFilter.docDate>(
					graphFilter.Current, retainageOptions.Date);
			}
			catch (PXSetPropertyException error)
			{
				throw new PXException(CommonMessages.ProcessingEntityError,
					nameof(RetainageOptionsFilter.Date), error.Message);
			}

			graphFilter.Cache.SetValueExt<APRetainageFilter.finPeriodID>(
				graphFilter.Current, retainageOptions.PostPeriod);
			PXFieldState postPeriodState = (PXFieldState) graphFilter.Cache
				.GetStateExt<APRetainageFilter.finPeriodID>(graphFilter.Current);
			if (!string.IsNullOrEmpty(postPeriodState?.Error))
			{
				if (postPeriodState.ErrorLevel == PXErrorLevel.Error)
					// Acuminator disable once PX1050 HardcodedStringInLocalizationMethod
					// [postPeriodState.Error is already localized]
					throw new PXException("{0}", postPeriodState.Error);
				else
					throw new PXException(CommonMessages.ProcessingEntityError,
						nameof(RetainageOptionsFilter.PostPeriod), postPeriodState.Error);
			}

			var updatedOptions = new RetainageOptions
			{
				DocDate = graphFilter.Current.DocDate,
				MasterFinPeriodID = FinPeriodIDAttribute.CalcMasterPeriodID<APRetainageFilter.finPeriodID>(
					retainageGraph.Caches[typeof(APRetainageFilter)], graphFilter.Current)
			};

			PXLongOperation.StartOperation(this, () =>
			{
				var invoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();
				var retainageExtension = invoiceEntry.GetExtension<APInvoiceEntryRetainage>();
				retainageExtension?.ReleaseRetainageProc(invoicesToRelease, updatedOptions, isAutoRelease);
			});

			return adapter.Get();
		}

		protected void ValidateOptionsAndInvoice(RetainageOptionsFilter options, APInvoice invoice)
		{
			if (options?.Date == null)
				throw new PXException(Messages.RetainageParameterMandatoryForRelease,
					nameof(RetainageOptionsFilter.Date));

			if (string.IsNullOrEmpty(options.PostPeriod))
				throw new PXException(Messages.RetainageParameterMandatoryForRelease,
					nameof(RetainageOptionsFilter.PostPeriod));

			if (string.IsNullOrEmpty(invoice?.RefNbr))
				throw new PXException(Messages.RetainageParameterMandatoryForRelease,
					"ReferenceNbr");

			if (invoice.RetainageApply != true)
				throw new PXException(Messages.RetainageReleaseActionIsNotAvailable);

			var retainageInvoiceEntry = Base.GetExtension<APInvoiceEntryRetainage>();
			if (retainageInvoiceEntry?.releaseRetainage?.GetEnabled() != true)
				throw new PXException(Messages.RetainageReleaseActionIsNotAvailable);

			if (options.AmountToRelease <= 0)
				throw new PXException(Messages.RetainagePositiveAmountRequired);

			if (options.AmountToRelease > 0 && invoice.PaymentsByLinesAllowed == true)
				throw new PXException(Messages.RetainageCannotReleaseAmountWithPayByLine);

			if (options.AmountToRelease > invoice.CuryRetainageUnreleasedAmt)
				throw new PXException(Messages.RetainageReleaseAmountGreaterThanUnreleased);

			APRegister reversingDoc;
			if (Base.CheckReversingRetainageDocumentAlreadyExists(invoice, out reversingDoc))
			{
				throw new PXException(
					Messages.ReleaseRetainageReversingDocumentExists,
					GetDocumentType(invoice.DocType),
					GetDocumentType(reversingDoc.DocType),
					reversingDoc.RefNbr);
			}
		}

		protected APRetainageRelease InitRetainageGraph(APInvoice invoice)
		{
			var retainageGraph = PXGraph.CreateInstance<APRetainageRelease>();

			var currentFilter = retainageGraph.Filter.Current;
			currentFilter.BranchID = invoice.BranchID;
			currentFilter.OrgBAccountID = PXAccess.GetBranch(invoice.BranchID).BAccountID;
			currentFilter.VendorID = invoice.VendorID;
			currentFilter.RefNbr = invoice.RefNbr;
			currentFilter.ShowBillsWithOpenBalance = invoice.OpenDoc == true;

			var retainageDocToRelease = retainageGraph.DocumentList.SelectSingle();
			if (retainageDocToRelease != null)
				return retainageGraph;

			var retainageDoc =
				PXSelectJoin<APRetainageInvoice,
					InnerJoinSingleTable<APInvoice,
						On<
							APInvoice.docType, Equal<APRetainageInvoice.docType>,
							And<APInvoice.refNbr, Equal<APRetainageInvoice.refNbr>>>>,
					Where<
						APRetainageInvoice.isRetainageDocument, Equal<True>,
						And<APRetainageInvoice.origDocType, Equal<Required<APInvoice.docType>>,
						And<APRetainageInvoice.origRefNbr, Equal<Required<APInvoice.refNbr>>>>>>
				.Select(Base, invoice.DocType, invoice.RefNbr)
				.RowCast<APRetainageInvoice>()
				.FirstOrDefault(i => i.Released != true);

			throw new PXException(
				Messages.ReleaseRetainageNotReleasedDocument,
				GetDocumentType(retainageDoc.DocType),
				retainageDoc.RefNbr,
				GetDocumentType(invoice.DocType));
		}

		// Acuminator disable once PX1050 HardcodedStringInLocalizationMethod
		// [APDocTypeDict contains strings from localizable class]
		// Acuminator disable once PX1051 NonLocalizableString
		// [APDocTypeDict contains strings from localizable class]
		private static string GetDocumentType(string documentTypeCode)
			=> string.IsNullOrEmpty(documentTypeCode)
			? null
			: PXMessages.LocalizeNoPrefix(APInvoiceEntry.APDocTypeDict[documentTypeCode]);

		/// <summary>
		/// DAC for filter which is used to release retainage via API
		/// </summary>
		[PXHidden]
		public class RetainageOptionsFilter : IBqlTable
		{
			public abstract class date : BqlDateTime.Field<date> {}

			/// <summary>
			/// Date of generated retainage documents
			/// </summary>
			[PXDate]
			[PXUIField(DisplayName = "Date")]
			public virtual DateTime? Date { get; set; }

			public abstract class postPeriod : BqlString.Field<postPeriod> {}

			/// <summary>
			/// Financial period for generated retainage documents
			/// </summary>
			[PXString]
			[PXUIField(DisplayName = "Post Period")]
			public virtual string PostPeriod { get; set; }

			public abstract class amountToRelease : BqlDecimal.Field<amountToRelease> {}

			/// <summary>
			/// Which retainage amount should be released
			/// </summary>
			[PXDecimal]
			[PXUIField(DisplayName = "Amount to Release")]
			public virtual decimal? AmountToRelease { get; set; }
		}
	}
}
