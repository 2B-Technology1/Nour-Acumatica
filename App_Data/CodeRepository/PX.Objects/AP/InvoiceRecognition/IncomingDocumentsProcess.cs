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
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP.InvoiceRecognition.DAC;
using PX.SM;
using PX.TM;
using PX.Web.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PX.Concurrency;
using PX.Data.WorkflowAPI;
using System.Linq;

namespace PX.Objects.AP.InvoiceRecognition
{
	[PXInternalUseOnly]
	public class IncomingDocumentsProcess : PXGraph<IncomingDocumentsProcess>
	{
		private const string _processButtonName = "Process";
		private const string ViewPdfActionName = "ViewPdf";

		internal static readonly HashSet<string> JsonFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			nameof(RecognizedRecordForProcessing.recognitionResult),
			nameof(RecognizedRecordForProcessing.recognitionFeedback)
		};

		[PXFilterable]
		public PXProcessingJoin<RecognizedRecordForProcessing,
			   LeftJoin<RecognizedRecordDetail,
			   On<RecognizedRecordForProcessing.entityType, Equal<RecognizedRecordDetail.entityType>, And<
				  RecognizedRecordForProcessing.refNbr, Equal<RecognizedRecordDetail.refNbr>>>>,
			   Where<RecognizedRecordForProcessing.entityType, Equal<RecognizedRecordEntityTypeListAttribute.aPDocument>>,
			   OrderBy<Desc<RecognizedRecordForProcessing.createdDateTime>>> Records;

		public SelectFrom<RecognizedRecordErrorHistory>.
			   Where<RecognizedRecordErrorHistory.refNbr.IsEqual<RecognizedRecordForProcessing.refNbr.FromCurrent>.And<
					  RecognizedRecordErrorHistory.entityType.IsEqual<RecognizedRecordForProcessing.entityType.FromCurrent>>>.
			   OrderBy<RecognizedRecordErrorHistory.createdDateTime.Desc>.
			   View.ReadOnly ErrorHistory;

		public PXCancel<RecognizedRecordForProcessing> Cancel;
		public PXAction<RecognizedRecordForProcessing> Insert;
		public PXAction<RecognizedRecordForProcessing> Delete;
		public PXAction<RecognizedRecordForProcessing> EditRecord;
		public PXAction<RecognizedRecordForProcessing> ViewDocument;
		public PXAction<RecognizedRecordForProcessing> ViewErrorHistory;
		public PXAction<RecognizedRecordForProcessing> SearchVendor;
		public PXAction<RecognizedRecordForProcessing> UploadFiles;

		public IncomingDocumentsProcess()
		{
			Records.SetProcessCaption(Messages.Recognize);
			Records.SetProcessAllCaption(Messages.RecognizeAll);
			Records.SetAsyncProcessDelegate(RecognizeAsync);

			Actions.Move(_processButtonName, nameof(Insert));
			Actions.Move(_processButtonName, nameof(Delete));
			Actions.Move(nameof(Insert), nameof(Cancel));

			PXUIFieldAttribute.SetDisplayName<RecognizedRecordDetail.vendorID>(Caches[typeof(RecognizedRecordDetail)], Messages.RecognizedVendor);
		}

		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<IncomingDocumentsProcess, RecognizedRecordForProcessing>());
		protected static void Configure(WorkflowContext<IncomingDocumentsProcess, RecognizedRecordForProcessing> context)
		{
			context
				.UpdateScreenConfigurationFor(screen =>
					screen.WithActions(actions =>
						actions.AddNew(ViewPdfActionName, configAction =>
							configAction
								.DisplayName(Messages.ViewPdfActionDisplayName)
								.IsSidePanelScreen(sidePanelAction =>
									sidePanelAction
										.NavigateToScreen<PdfViewerManager>()
										.WithIcon("receipt")
										.WithAssignments(containerFiller =>
											containerFiller.Add<PdfFileInfo.recognizedRecordRefNbr>(c =>
												c.SetFromField<RecognizedRecordForProcessing.refNbr>()
										)
									)
							)
						)
					)
				);
		}

		public IEnumerable records()
		{
			var fieldsToSelect = Records.Cache.BqlFields
				.Where(f => !JsonFields.Contains(f.Name))
				.Concat(new[] { typeof(RecognizedRecordDetail) });
			var view = new PXView(this, Records.View.IsReadOnly, Records.View.BqlSelect);
			var startRow = PXView.StartRow;
			var totalRows = 0;

			using (new PXFieldScope(view, fieldsToSelect))
			{
				var records = view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings,
					PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);

				PXView.StartRow = 0;

				return records;
			}
		}

		[PXEntryScreenRights(typeof(APRecognizedInvoice), nameof(APInvoiceRecognitionEntry.Insert))]
		[PXInsertButton]
		[PXUIField]
		protected virtual void insert()
		{
			var graph = CreateInstance<APInvoiceRecognitionEntry>();

			throw new PXRedirectRequiredException(graph, null);
		}

		[PXEntryScreenRights(typeof(APRecognizedInvoice), nameof(APInvoiceRecognitionEntry.Delete))]
		[PXButton(ImageKey = Sprite.Main.Remove, ConfirmationMessage = ActionsMessages.ConfirmDeleteMultiple)]
		[PXUIField]
		protected virtual void delete()
		{
			Records.SetProcessDelegate(RecognizedRecordProcess.DeleteRecognizedRecord);
			Actions[_processButtonName].PressButton();
		}

		[PXButton]
		[PXUIField(Visible = false)]
		protected virtual void editRecord()
		{
			var refNbr = Records.Current?.RefNbr;
			if (refNbr == null)
			{
				return;
			}

			var select = new
				SelectFrom<APRecognizedInvoice>.
				Where<APRecognizedInvoice.recognizedRecordRefNbr.IsEqual<@P.AsGuid>>.
				View.ReadOnly(this);
			select.View.Clear();

			// Acuminator disable once PX1015 IncorrectNumberOfSelectParameters Diagnostic bug
			var record = select.SelectSingle(refNbr);
			if (record == null)
			{
				return;
			}

			var graph = CreateInstance<APInvoiceRecognitionEntry>();
			graph.Document.Current = record;

			throw new PXRedirectRequiredException(graph, null);
		}

		[PXButton]
		[PXUIField(Visible = false)]
		protected virtual void viewDocument()
		{
			var noteId = Records.Current?.DocumentLink;
			if (noteId == null)
			{
				return;
			}

			var document = (APInvoice)
				SelectFrom<APInvoice>.
				Where<APInvoice.noteID.IsEqual<@P.AsGuid>>.
				View.ReadOnly.SelectSingleBound(this, null, noteId);
			if (document == null)
			{
				return;
			}

			var graph = CreateInstance<APInvoiceEntry>();
			graph.Document.Current = document;

			throw new PXRedirectRequiredException(graph, null);
		}

		[PXEntryScreenRights(typeof(APRecognizedInvoice), nameof(APInvoiceRecognitionEntry.ViewErrorHistory))]
		[PXButton(Category = ActionsMessages.Actions)]
		[PXUIField(DisplayName = "View History", Enabled = false)]
		public virtual void viewErrorHistory()
		{
			ErrorHistory.AskExt();
		}

		[PXEntryScreenRights(typeof(APRecognizedInvoice), nameof(APInvoiceRecognitionEntry.SearchVendor))]
		[PXButton(Category = ActionsMessages.Actions)]
		[PXUIField(DisplayName = "Search for Vendor", Enabled = false)]
		protected virtual void searchVendor()
		{
			Records.SetAsyncProcessDelegate(SearchForVendorAsync);
			Actions[_processButtonName].PressButton();
		}

		[PXButton]
		[PXUIField(DisplayName = "Upload Files", Visible = false)]
		public virtual IEnumerable uploadFiles(PXAdapter adapter)
		{
			var recognitionGraph = CreateInstance<APInvoiceRecognitionEntry>();
			var uploadFileGraph = CreateInstance<UploadFileMaintenance>();

			foreach (var pair in adapter.Arguments)
			{
				if (!(pair.Value is byte[] fileData))
				{
					continue;
				}

				var fileName = pair.Key;
				var uid = Guid.NewGuid();
				var name = $"{uid}\\{fileName}";
				var fileInfo = new FileInfo(uid, name, null, fileData);

				if (!uploadFileGraph.SaveFile(fileInfo))
				{
					throw new PXException(Messages.FileCannotBeSaved, fileName);
				}

				var recognizedRecord = recognitionGraph.CreateRecognizedRecord(fileName, fileData, fileInfo.UID.Value);

				PXNoteAttribute.ForcePassThrow<RecognizedRecord.noteID>(recognitionGraph.RecognizedRecords.Cache);
				PXNoteAttribute.SetFileNotes(recognitionGraph.RecognizedRecords.Cache, recognizedRecord, fileInfo.UID.Value);

				recognitionGraph.RecognizedRecords.Cache.PersistUpdated(recognizedRecord);
			}

			// select top 1 to increase performance as the query result is not used anyway
			adapter.StartRow = 0;
			adapter.MaximumRows = 1;
			return adapter.Get();
		}


		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Created Date")]
		protected virtual void _(Events.CacheAttached<RecognizedRecordForProcessing.createdDateTime> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[Owner]
		protected virtual void _(Events.CacheAttached<RecognizedRecordForProcessing.owner> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Document Link", Visible = true)]
		protected virtual void _(Events.CacheAttached<RecognizedRecordForProcessing.documentLink> e)
		{
		}

		protected virtual void _(Events.RowSelected<RecognizedRecordForProcessing> e)
		{
			if (e.Row == null)
			{
				return;
			}

			var enableViewErrorHistory = e.Row.Status == RecognizedRecordStatusListAttribute.Error;
			ViewErrorHistory.SetEnabled(enableViewErrorHistory);
		}

		protected virtual void _(Events.FieldSelecting<RecognizedRecordForProcessing.documentLink> e)
		{
			if (!(e.Row is RecognizedRecordForProcessing row) || row.DocumentLink == null)
			{
				return;
			}

			var document = (APRegister)
				SelectFrom<APRegister>.
				Where<APRegister.noteID.IsEqual<@P.AsGuid>>.
				View.ReadOnly.SelectSingleBound(this, null, row.DocumentLink);

			e.ReturnValue = document != null ?
				$"{document.DocType} {document.RefNbr}" :
				string.Empty;
		}

		public static async Task SearchForVendorAsync(List<RecognizedRecordForProcessing> records, CancellationToken cancellationToken)
		{
			var refNbrs = records
				.Select(r => r.RefNbr)
				.ToArray();
			var graph = CreateInstance<APInvoiceRecognitionEntry>();
			var recordsWithJsonFields = new SelectFrom<RecognizedRecordForProcessing>
				.Where<RecognizedRecordForProcessing.refNbr.IsIn<@P.AsGuid>>
				.View.ReadOnly(graph)
				.Select(refNbrs)
				.FirstTableItems;

			var orderedRecordsWithJsonFields = recordsWithJsonFields
				.OrderBy(r => r.RefNbr)
				.ToArray();
			var orderedRecords = records
				.OrderBy(r => r.RefNbr)
				.ToArray();

			for (int i = 0; i < orderedRecords.Length; i++)
			{
				PXProcessing.SetCurrentItem(orderedRecords[i]);

				if (orderedRecords[i].Status != RecognizedRecordStatusListAttribute.Recognized)
				{
					PXProcessing.SetProcessed();

					continue;
				}

				var detail = (RecognizedRecordDetail)
					SelectFrom<RecognizedRecordDetail>.
					Where<RecognizedRecordDetail.entityType.IsEqual<@P.AsString>.And<
						  RecognizedRecordDetail.refNbr.IsEqual<@P.AsGuid>>>.
						  View.ReadOnly.Select(graph, orderedRecords[i].EntityType, orderedRecords[i].RefNbr);
				if (detail?.VendorID != null)
				{
					PXProcessing.SetProcessed();

					continue;
				}

				await graph.PopulateVendorId(orderedRecordsWithJsonFields[i], detail);

				PXProcessing.SetProcessed();
			}
		}

		public static async Task RecognizeAsync(List<RecognizedRecordForProcessing> records, CancellationToken cancellationToken)
		{
			var graph = CreateInstance<PXGraph>();
			var cache = graph.Caches[typeof(RecognizedRecordForProcessing)];
			var batch = new List<RecognizedRecordFileInfo>();

			foreach (var rec in records)
			{
				if (!APInvoiceRecognitionEntry.StatusValidForRecognitionSet.Contains(rec.Status))
				{
					PXProcessing.SetCurrentItem(rec);
					PXProcessing.SetProcessed();

					continue;
				}

				var files = APInvoiceRecognitionEntry.GetFilesToRecognize(cache, rec);
				if (files == null || files.Length == 0)
				{
					PXProcessing.SetCurrentItem(rec);
					PXProcessing.SetProcessed();

					continue;
				}

				var file = files[0];
				var fileInfo = new RecognizedRecordFileInfo(file.Name, file.Data, file.FileID.Value, rec);

				batch.Add(fileInfo);
			}

			if (batch.Count == 0)
			{
				return;
			}

			await APInvoiceRecognitionEntry.RecognizeRecordsBatch(batch, cancellationToken);
		}
	}
}
