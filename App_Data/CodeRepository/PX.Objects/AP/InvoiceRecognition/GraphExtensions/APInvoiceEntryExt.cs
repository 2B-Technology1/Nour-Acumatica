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

using PX.CloudServices.DAC;
using PX.Common;
using PX.Concurrency;
using PX.Data;
using PX.Objects.CS;
using PX.Data.Search;
using PX.Objects.AP.InvoiceRecognition.DAC;
using PX.Objects.AP.InvoiceRecognition.Feedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using PX.Data.BQL;
using PX.SM;

namespace PX.Objects.AP.InvoiceRecognition
{
	[PXInternalUseOnly]
	public sealed class APInvoiceExt : PXCacheExtension<APInvoice>
	{
		[PXBool]
		[PXUIField(Visible = false)]
		public bool? RenameFileScreenId { get; set; }
		public abstract class renameFileScreenId : BqlBool.Field<renameFileScreenId> { }

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.apDocumentRecognition>();
		}
	}

	[PXInternalUseOnly]
	public class APInvoiceEntryExt : PXGraphExtension<APInvoiceEntry>
	{
        internal const string FEEDBACK_FIELD_BOUND_KEY = "feedback:field-bound";
        internal const string FEEDBACK_RECORD_SAVED_KEY = "feedback:record-saved";

		[InjectDependency]
		internal IEntitySearchService EntitySearchService { get; set; }

		[InjectDependency]
		public IInvoiceRecognitionService InvoiceRecognitionClient { get; set; }

		[InjectDependency]
		public IConfiguration Configuration { get; set; }
		
		public PXFilter<FeedbackParameters> FeedbackParameters;

		public SelectFrom<APRecognizedInvoice>
			   .Where<APRecognizedInvoice.documentLink.IsEqual<APInvoice.noteID.AsOptional>>
			   .View.ReadOnly SourceDocument;

		public PXAction<APInvoice> viewSourceDocument;
		[PXUIField(DisplayName = "View Source Document")]
		[PXLookupButton]
		public virtual void ViewSourceDocument()
		{
			var recognizedSourceDocument = SourceDocument.SelectSingle();
			if (recognizedSourceDocument == null)
			{
				return;
			}

			var graph = PXGraph.CreateInstance<APInvoiceRecognitionEntry>();
			graph.Document.Current = recognizedSourceDocument;

			throw new PXRedirectRequiredException(graph, null);
		}

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.apDocumentRecognition>();
		}

		public override void Initialize()
		{
			var isRecognitionEnabled = InvoiceRecognitionClient.IsConfigured();
			if (!isRecognitionEnabled)
			{
				return;
			}

			var recognizedRecordCache = Base.Caches[typeof(RecognizedRecord)];
			Base.Views.Caches.Add(recognizedRecordCache.GetItemType());

			Base.RowPersisted.AddHandler<APInvoice>(ReplacePrimaryScreenId);
			Base.OnAfterPersist += SendFeedback;
			Base.OnAfterPersist += CorrectRecognizedVendor;
		}

		private void ReplacePrimaryScreenId(Events.RowPersisted<APInvoice> e)
		{
			if (e.Operation != PXDBOperation.Insert || e.TranStatus != PXTranStatus.Open)
			{
				return;
			}

			var invoiceExt = e.Cache.GetExtension<APInvoiceExt>(e.Row);
			if (invoiceExt?.RenameFileScreenId != true)
			{
				return;
			}
			e.Cache.SetValue<APInvoiceExt.renameFileScreenId>(e.Row, false);

			var screenId = PXContext.GetScreenID().Replace(".", "") ??
				PXSiteMap.Provider.FindSiteMapNodeByGraphType(typeof(APInvoiceEntry).FullName)?.ScreenID;
			if (string.IsNullOrEmpty(screenId))
			{
				return;
			}

			var fileIds = PXNoteAttribute.GetFileNotes(e.Cache, e.Row);
			if (fileIds == null)
			{
				return;
			}

			foreach (var fileId in fileIds)
			{
				var uploadFile = Base.Select<UploadFile>()
					.Where(f => f.FileID == fileId && f.PrimaryScreenID != screenId)
					.FirstOrDefault();
				if (uploadFile == null)
				{
					continue;
				}

				var uploadFileCache = Base.Caches[typeof(UploadFile)];
				uploadFile.PrimaryScreenID = screenId;
				uploadFile = uploadFileCache.Update(uploadFile) as UploadFile;

				using (new PXTimeStampScope(null))
				{
					PXTimeStampScope.SetRecordComesFirst(typeof(UploadFile), true);
				// Acuminator disable once PX1043 SavingChangesInEventHandlers Valid Persist method is used
					uploadFileCache.PersistUpdated(uploadFile);
				}
			}
		}

		public sealed override void Configure(PXScreenConfiguration configuration) =>
			Configure(configuration.GetScreenConfigurationContext<APInvoiceEntry, APInvoice>());
		protected static void Configure(WorkflowContext<APInvoiceEntry, APInvoice> context)
		{
			var isRecognitionEnabled = PXAccess.FeatureInstalled<FeaturesSet.apDocumentRecognition>();
			if (!isRecognitionEnabled)
			{
				return;
			}

			context
				.UpdateScreenConfigurationFor(screen =>
					screen.WithActions(actions =>
						actions.Add<APInvoiceEntryExt>(g => g.viewSourceDocument, c => c.InFolder(FolderType.InquiriesFolder))));
		}

		protected void _(Events.RowSelected<APInvoice> e, PXRowSelected baseEvent)
		{
			baseEvent(e.Cache, e.Args);

			if (e.Row == null)
			{
				return;
			}

			RecognizedRecord recognizedRecord =
				PXSelect<RecognizedRecord, Where<RecognizedRecord.documentLink,
					Equal<Required<RecognizedRecord.documentLink>>>>.SelectWindowed(Base, 0, 1, e.Row.NoteID);

			viewSourceDocument.SetEnabled(recognizedRecord != null);
		}

		protected void _(Events.RowDeleted<APInvoice> e, PXRowDeleted baseEvent)
		{
			baseEvent?.Invoke(e.Cache, e.Args);

			if (e.Row?.NoteID == null)
			{
				return;
			}

			// Acuminator disable once PX1015 IncorrectNumberOfSelectParameters
			var document = SourceDocument.Select(e.Row.NoteID);
			if (document == null)
			{
				return;
			}

			// Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers
			var graph = PXGraph.CreateInstance<APInvoiceRecognitionEntry>();
			graph.Document.Current = document;
			// Acuminator disable once PX1071 PXActionExecutionInEventHandlers
			graph.Delete.Press();
		}

		private void SendFeedback(PXGraph graph)
		{
			if (!(graph is APInvoiceEntry invoiceEntry))
			{
				return;
			}

			var primaryRow = invoiceEntry.Document.Current;
			if (primaryRow == null || !APDocType.Invoice.Equals(primaryRow.DocType, StringComparison.Ordinal))
			{
				return;
			}

            var feedbackBuilder = FeedbackParameters.Current.FeedbackBuilder;
            if (feedbackBuilder == null)
            {
                return;
            }

			var primaryView = invoiceEntry.Document.View;
			var detailView = invoiceEntry.Transactions.View;
			var detailRows = invoiceEntry.Transactions.Select().Select(t => (APTran)t);
			var feedback = feedbackBuilder.ToRecordSavedFeedback(primaryView, primaryRow, detailView, detailRows,
				EntitySearchService);
			
            var docNoteId = primaryRow.NoteID;

			var links = FeedbackParameters.Current.Links;
			if (links == null)
			{
				return;
			}

			FeedbackParameters.Reset();
			
			//we doing it this way because it allows us to always have consistent recognition client and feedback
			//so if somebody overrides the client, we immediately lose the feedback, which is GOOD
			if (InvoiceRecognitionClient is IInvoiceRecognitionFeedback feedbackService)
			{
				var sendBoundFeedback = Configuration.GetValue("SendDocumentInboxFeedback", false);
				graph.LongOperationManager.StartAsyncOperation(Guid.NewGuid(),
					_ => SendFeedbackAsync(links, feedbackService, docNoteId, feedback, sendBoundFeedback));
			}
		}

        private static async Task SendFeedbackAsync(Dictionary<string, Uri> links, IInvoiceRecognitionFeedback feedbackService,
            Guid? documentLink, DocumentFeedback recordSavedFeedback, bool sendBoundFeedback)
        {
            if (links == null)
            {
                PXTrace.WriteError("IDocumentRecognitionClient: Unable to send feedback - links are not found");
                return;
            }

			if (sendBoundFeedback)
			{
				await SendBoundFeedbackAsync(links, feedbackService, documentLink);
			}

            await SendRecordSavedFeedbackAsync(links, feedbackService, recordSavedFeedback);
        }

        private static async Task SendRecordSavedFeedbackAsync(Dictionary<string, Uri> links, IInvoiceRecognitionFeedback feedbackService,
            DocumentFeedback recordSavedFeedback)
        {
            if (recordSavedFeedback == null)
                return;
            if (!links.TryGetValue(FEEDBACK_RECORD_SAVED_KEY, out var recordSavedLink))
            {
                PXTrace.WriteError("IDocumentRecognitionClient: Unable to send feedback - link is not found:{LinkKey}",
                    FEEDBACK_RECORD_SAVED_KEY);
                return;
            }

            var formatter = new JsonMediaTypeFormatter {SerializerSettings = DocumentFeedback._settings};
            await feedbackService.Send(recordSavedLink,
                new ObjectContent(recordSavedFeedback.GetType(), recordSavedFeedback, formatter));
        }

        private static async Task SendBoundFeedbackAsync(Dictionary<string, Uri> links, IInvoiceRecognitionFeedback feedbackService,
            Guid? documentLink)
        {
            if ( !links.TryGetValue(FEEDBACK_FIELD_BOUND_KEY, out var link))
            {
                PXTrace.WriteError("IDocumentRecognitionClient: Unable to send feedback - link is not found:{LinkKey}",
                    FEEDBACK_FIELD_BOUND_KEY);
                return;
            }

            var graph = PXGraph.CreateInstance<PXGraph>();
            var recognizedRecord =
                PXSelect<RecognizedRecord, Where<RecognizedRecord.documentLink,
                        Equal<Required<RecognizedRecord.documentLink>>>>.Select(graph, documentLink).FirstOrDefault()
                    ?.GetItem<RecognizedRecord>();
            if (recognizedRecord?.RecognitionFeedback == null)
                return;
            var reader = new System.IO.StringReader(recognizedRecord.RecognitionFeedback);

            while (true)
            {
                var item = await reader.ReadLineAsync();
                if (item == null) break;
                if(string.IsNullOrWhiteSpace(item)) continue;
                await feedbackService.Send(link, new StringContent(item, Encoding.UTF8, "application/json"));
            }

            return;
        }

		private void CorrectRecognizedVendor(PXGraph graph)
		{
			if (!(graph is APInvoiceEntry invoiceEntry))
			{
				return;
			}

			var vendorId = invoiceEntry.Document.Current?.VendorID;
			if (vendorId == null)
			{
				return;
			}

			if (!invoiceEntry.Caches.TryGetValue(typeof(RecognizedVendorMapping), out var recognizedVendorCache) ||
				!(recognizedVendorCache.Current is RecognizedVendorMapping newRecognizedVendor) ||
				newRecognizedVendor.VendorNamePrefix == null || newRecognizedVendor.VendorName == null)
			{
				return;
			}

			var existingRecognizedVendor = SelectFrom<RecognizedVendorMapping>.
				Where<RecognizedVendorMapping.vendorNamePrefix.IsEqual<@P.AsString>.
					And<RecognizedVendorMapping.vendorName.IsEqual<@P.AsString>>>.
				View.ReadOnly.Select(graph, newRecognizedVendor.VendorNamePrefix, newRecognizedVendor.VendorName)?.TopFirst;
			if (existingRecognizedVendor?.VendorID == vendorId)
			{
				return;
			}

			if (existingRecognizedVendor != null)
			{
				existingRecognizedVendor.VendorID = vendorId;
				recognizedVendorCache.PersistUpdated(existingRecognizedVendor);
			}
			else
			{
				newRecognizedVendor.VendorID = vendorId;
				recognizedVendorCache.PersistInserted(newRecognizedVendor);
			}

			recognizedVendorCache.Clear();
		}
	}
}
