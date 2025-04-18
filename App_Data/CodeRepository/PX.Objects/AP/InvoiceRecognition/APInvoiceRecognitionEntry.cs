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

using Newtonsoft.Json;
using PX.CloudServices.DAC;
using PX.CloudServices.DocumentRecognition;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.Wiki.Parser;
using PX.Metadata;
using PX.Objects.AP.InvoiceRecognition.DAC;
using PX.Objects.AP.InvoiceRecognition.Feedback;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.SM;
using Serilog.Events;
using SerilogTimings.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ILogger = Serilog.ILogger;
using PX.CloudServices.Tenants;
using PX.Data.DependencyInjection;
using PX.Objects.SO;
using PX.Objects.Extensions.MultiCurrency.AP;
using System.Net.Http.Formatting;
using System.Net.Http;
using PX.Concurrency;
using PdfSharp.Pdf.IO;
using PX.Web.UI;

namespace PX.Objects.AP.InvoiceRecognition
{
	[PXInternalUseOnly]
	public class APInvoiceRecognitionEntry : PXGraph<APInvoiceRecognitionEntry, APRecognizedInvoice>, IGraphWithInitialization
	{
		public class MultiCurrency : APMultiCurrencyGraph<APInvoiceRecognitionEntry, APRecognizedInvoice>
		{
			protected override string DocumentStatus => Base.Document.Current?.Status;

			protected override CurySourceMapping GetCurySourceMapping()
			{
				return new CurySourceMapping(typeof(VendorR));
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(APRecognizedInvoice))
				{
					DocumentDate = typeof(APRecognizedInvoice.docDate),
					BAccountID = typeof(APRecognizedInvoice.vendorID)
				};
			}

			protected override PXSelectBase[] GetChildren()
			{
				return new PXSelectBase[]
				{
					Base.Document,
					Base.Transactions,
				};
			}
		}

		public class PXDeleteWithRecognizedRecord<TNode> : PXDelete<TNode>
			where TNode : class, IBqlTable, new()
		{
			public PXDeleteWithRecognizedRecord(PXGraph graph, string name)
			: base(graph, name)
			{ }

			public PXDeleteWithRecognizedRecord(PXGraph graph, Delegate handler)
			: base(graph, handler)
			{ }

			[PXUIField(DisplayName = ActionsMessages.Delete, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete)]
			[PXDeleteButton(ConfirmationMessage = ActionsMessages.ConfirmDeleteExplicit)]
			protected override IEnumerable Handler(PXAdapter adapter)
			{
				if (Graph is APInvoiceRecognitionEntry graph)
				{
					APRecognizedInvoice invoice = graph.Document.Current;
					if (invoice?.ReleasedOrPrebooked == true)
					{
						RecognizedRecord record = graph.RecognizedRecords.Current;
						RecognizedRecordForProcessing rp = SelectFrom<RecognizedRecordForProcessing>.
							Where<RecognizedRecordForProcessing.refNbr.IsEqual<@P.AsGuid>>.
							View.Select(graph, record.RefNbr);
						RecognizedRecordProcess.DeleteRecognizedRecord(rp);
					}
				}
				return base.Handler(adapter);
			}
		}

		private enum ReloadData
		{
			All,
			VendorId
		}

		internal const string FEEDBACK_VENDOR_SEARCH = "feedback:entity-resolution";
		private const int _recognitionTimeoutMinutes = 20;
		private const int MaxFilePageCount = 50;
		private static readonly TimeSpan RecognitionPollingInterval = TimeSpan.FromSeconds(1);
		private static readonly string _screenInfoLoad = $"{typeof(APInvoiceRecognitionEntry).FullName}_screenInfoLoad";

		internal const string PdfExtension = ".pdf";

		public const string RefNbrNavigationParam = nameof(APRecognizedInvoice.RecognizedRecordRefNbr);
		public const string StatusNavigationParam = nameof(APRecognizedInvoice.RecognitionStatus);
		public const string NoteIdNavigationParam = nameof(APRecognizedInvoice.NoteID);

		private JsonSerializer _jsonSerializer = JsonSerializer.CreateDefault(DocumentFeedback._settings);

		private readonly HashSet<string> _alwaysDefaultPrimaryFields = new HashSet<string>
		{
			nameof(APRecognizedInvoice.VendorLocationID),
			nameof(APRecognizedInvoice.RecognizedRecordRefNbr),
			nameof(APRecognizedInvoice.RecognizedRecordStatus),
			nameof(APRecognizedInvoice.RecognitionStatus),
			nameof(APRecognizedInvoice.AllowFiles),
			nameof(APRecognizedInvoice.AllowFilesMsg),
			nameof(APRecognizedInvoice.AllowUploadFile)
		};

		internal static HashSet<string> StatusValidForRecognitionSet { get; } = new HashSet<string>
		{
			RecognizedRecordStatusListAttribute.PendingRecognition,
			RecognizedRecordStatusListAttribute.Error
		};

		[InjectDependency]
		internal IScreenInfoProvider ScreenInfoProvider { get; set; }

		[InjectDependency]
		internal Serilog.ILogger _logger { get; set; }

		[InjectDependency]
		public IInvoiceRecognitionService InvoiceRecognitionClient { get; set; }

		[InjectDependency]
		internal ICloudTenantService _cloudTenantService { get; set; }

		[InjectDependency]
		internal RecognizedRecordDetailsManager DetailsPopulator { get; set; }

		public void Initialize()
		{
			SwitchDefaultsOffForUIFields();
		}

		private void SwitchDefaultsOffForUIFields()
		{
			var (primaryFields, detailFields) = GetUIFields();
			if (primaryFields == null || detailFields == null)
			{
				return;
			}

			PXFieldDefaulting defaultingSwitchOff = (sender, args) => args.Cancel = true;
			var primaryFieldsWithoutDefaulting = primaryFields.Where(fieldName => !_alwaysDefaultPrimaryFields.Contains(fieldName));
			foreach (var field in primaryFieldsWithoutDefaulting)
			{
				FieldDefaulting.AddHandler(Document.View.Name, field, defaultingSwitchOff);
			}

			foreach (var field in detailFields)
			{
				FieldDefaulting.AddHandler(Transactions.View.Name, field, defaultingSwitchOff);
			}
		}

		public PXSetup<APSetup> APSetup;

		public SelectFrom<APInvoice>.Where<APInvoice.docType.IsEqual<APRecognizedInvoice.docType.FromCurrent>.And<
			APInvoice.refNbr.IsEqual<APRecognizedInvoice.refNbr.FromCurrent>>> Invoices;

		public PXSelect<APRecognizedInvoice> Document;
		public PXSelect<APInvoice,
			Where<APInvoice.docType, Equal<Current<APRecognizedInvoice.docType>>,
				And<Where<APInvoice.refNbr, Equal<Current<APRecognizedInvoice.refNbr>>>>>> DocumentBase;

		public PXSelect<Location,
			Where<Location.bAccountID, Equal<Current<APRecognizedInvoice.vendorID>>,
				And<Location.locationID, Equal<Optional<APRecognizedInvoice.vendorLocationID>>>>>
		VendorLocation;

		protected virtual IEnumerable document()
		{
			if (Document.Current != null && Caches[typeof(CurrencyInfo)].Current != null)
			{
				if (Caches[typeof(APRegister)].Current != Document.Current)
					Caches[typeof(APRegister)].Current = Document.Current;
				yield return Document.Current;
			}
			else
			{
				var records = this.QuickSelect(Document.View.BqlSelect);
				foreach (APRecognizedInvoice record in records)
				{
					if (record.RefNbr == null && record.DocType == null)
					{
						DefaultInvoiceValues(record);
						Document.Cache.SetStatus(record, PXEntryStatus.Held);
						Caches[typeof(APRegister)].Current = record;
					}

					if (Document.Current == null)
					{
						record.IsRedirect = true;
					}
					else
					{
						var (primaryFields, _) = GetUIFields();
						if (primaryFields != null)
						{
							var fieldsToCopy = primaryFields.Union(new[] { nameof(APRecognizedInvoice.IsDataLoaded) });
							foreach (var field in fieldsToCopy)
							{
								var currentValue = Document.Cache.GetValue(Document.Current, field);
								Document.Cache.SetValue(record, field, currentValue);
							}
						}
					}

					yield return record;
				}
			}
		}

		private void DefaultInvoiceValues(APRecognizedInvoice record)
		{
			record.DocType = record.EntityType;
			var inserted = DocumentBase.Insert(record);
			DocumentBase.Cache.Remove(inserted);
			DocumentBase.Cache.RestoreCopy(record, inserted);
			DocumentBase.Cache.IsDirty = false;
		}

		public PXSelect<APRecognizedTran,
			Where<APRecognizedTran.tranType, Equal<Current<APRecognizedInvoice.docType>>,
				And<Where<APRecognizedTran.refNbr, Equal<Current<APRecognizedInvoice.refNbr>>>>>>
			Transactions;

		public PXSelect<VendorR> Vendors;
		
		public SelectFrom<RecognizedRecord>
			  .Where<RecognizedRecord.refNbr.IsEqual<APRecognizedInvoice.recognizedRecordRefNbr.FromCurrent>>
			  .View RecognizedRecords;

		public SelectFrom<RecognizedRecordErrorHistory>
			   .Where<RecognizedRecordErrorHistory.refNbr.IsEqual<APRecognizedInvoice.recognizedRecordRefNbr.FromCurrent>.And<
				      RecognizedRecordErrorHistory.entityType.IsEqual<APRecognizedInvoice.entityType.FromCurrent>>>
			   .OrderBy<RecognizedRecordErrorHistory.createdDateTime.Desc>
			   .View.ReadOnly ErrorHistory;

		public SelectFrom<RecognizedRecordDetail>
			   .Where<RecognizedRecordDetail.refNbr.IsEqual<@P.AsGuid>.And<
					  RecognizedRecordDetail.entityType.IsEqual<@P.AsString>>>
			   .View RecognizedRecordDetails;

		public SelectFrom<APRecognizedInvoice>
			   .OrderBy<APRecognizedInvoice.recognizedRecordCreatedDateTime.Desc>
			   .View.ReadOnly NavigationSelect;

		public PXFilter<BoundFeedback> BoundFeedback;
		public PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Current<APRecognizedInvoice.vendorID>>>> CurrentVendor;
		public PXSelect<GL.Branch, Where<GL.Branch.branchID, Equal<Current<APRecognizedInvoice.branchID>>>> CurrentBranch;

		new public PXDeleteWithRecognizedRecord<APRecognizedInvoice> Delete;

		public new PXAction<APRecognizedInvoice> First;
		public new PXAction<APRecognizedInvoice> Previous;
		public new PXAction<APRecognizedInvoice> Next;
		public new PXAction<APRecognizedInvoice> Last;
		public PXAction<APRecognizedInvoice> ContinueSave;
		public PXAction<APRecognizedInvoice> ProcessRecognition;
		public PXAction<APRecognizedInvoice> RefreshStatus;
		public PXAction<APRecognizedInvoice> OpenDocument;
		public PXAction<APRecognizedInvoice> OpenDuplicate;
		public PXAction<APRecognizedInvoice> DeleteAllTransactions;
		public PXAction<APRecognizedInvoice> ViewErrorHistory;
		public PXAction<APRecognizedInvoice> SearchVendor;

		public PXAction<APRecognizedInvoice> DumpTableFeedback;

		public PXAction<APRecognizedInvoice> AttachFromMobile;
		[PXButton]
		[PXUIField(Visible = false, Enabled = false)]
		public virtual void attachFromMobile()
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBString(IsKey = false)]
		protected virtual void APRecognizedInvoice_RefNbr_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBString(IsKey = false)]
		protected virtual void APRecognizedInvoice_DocType_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PopupMessage]
		[VendorActiveOrHoldPayments(
			Visibility = PXUIVisibility.SelectorVisible,
			DescriptionField = typeof(Vendor.acctName),
			CacheGlobal = true,
			Filterable = true)]
		[PXDefault]
		protected virtual void APRecognizedInvoice_VendorID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDefault(typeof(APRecognizedInvoice.refNbr))]
		[PXParent(typeof(
			Select<APRecognizedInvoice,
			Where<APRecognizedInvoice.docType, Equal<Current<APRecognizedTran.tranType>>, And<APRecognizedInvoice.refNbr, Equal<Current<APRecognizedTran.refNbr>>>>>
		))]
		protected virtual void _(Events.CacheAttached<APRecognizedTran.refNbr> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(
			null,
			typeof(SumCalc<APRecognizedInvoice.curyLineTotal>)
		)]
		[PXUIField(Visible = false)]
		protected virtual void _(Events.CacheAttached<APRecognizedTran.curyTranAmt> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDefault(typeof(APRecognizedInvoice.docType))]
		protected virtual void _(Events.CacheAttached<APRecognizedTran.tranType> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault(CloudServices.Models.APInvoiceDocumentType)]
		protected virtual void RecognizedRecord_EntityType_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault(CloudServices.Models.APInvoiceDocumentType)]
		protected virtual void APRecognizedInvoice_EntityType_CacheAttached(PXCache sender)
		{
		}

		[PXUIField]
		[PXFirstButton]
		public IEnumerable first(PXAdapter adapter)
		{
			if (Document.Current?.CreatedDateTime == null)
			{
				yield break;
			}

			var firstRecord = NavigationSelect.SelectSingle();

			if (firstRecord == null)
			{
				yield return Document.Current;
			}
			else
			{
				Clear();
				SelectTimeStamp();

				yield return firstRecord;
			}
		}

		[PXUIField]
		[PXPreviousButton]
		public IEnumerable previous(PXAdapter adapter)
		{
			if (Document.Current?.CreatedDateTime == null)
			{
				yield break;
			}

			var command = NavigationSelect.View.BqlSelect.
				WhereNew<Where<
				APRecognizedInvoice.recognizedRecordCreatedDateTime.IsGreater
				<APRecognizedInvoice.recognizedRecordCreatedDateTime.FromCurrent>>>().
				OrderByNew<OrderBy<Asc<APRecognizedInvoice.recognizedRecordCreatedDateTime>>>();
			var view = new PXView(graph: this, isReadOnly: true, command);
			var prevRecord = view.SelectSingle();

			if (prevRecord == null)
			{
				yield return Document.Current;
			}
			else
			{
				Clear();
				SelectTimeStamp();

				yield return prevRecord;
			}
		}

		[PXUIField]
		[PXNextButton]
		public IEnumerable next(PXAdapter adapter)
		{
			if (Document.Current?.CreatedDateTime == null)
			{
				yield break;
			}

			var command = NavigationSelect.View.BqlSelect.WhereNew<Where<
				APRecognizedInvoice.recognizedRecordCreatedDateTime.IsLess
				<APRecognizedInvoice.recognizedRecordCreatedDateTime.FromCurrent>>>();
			var view = new PXView(graph: this, isReadOnly: true, command);
			var nextRecord = view.SelectSingle();

			if (nextRecord == null)
			{
				yield return Document.Current;
			}
			else
			{
				Clear();
				SelectTimeStamp();

				yield return nextRecord;
			}
		}

		[PXUIField]
		[PXLastButton]
		public IEnumerable last(PXAdapter adapter)
		{
			if (Document.Current?.CreatedDateTime == null)
			{
				yield break;
			}

			var lastRecord = NavigationSelect.SelectWindowed(startRow: -1, totalRows: 1).TopFirst;
			if (lastRecord == null)
			{
				yield return Document.Current;
			}
			else
			{
				Clear();
				SelectTimeStamp();

				yield return lastRecord;
			}
		}

		public IEnumerable transactions()
		{
			if (Document.Current?.RecognitionStatus == RecognizedRecordStatusListAttribute.Processed)
			{
				return SelectFrom<APRecognizedTran>.
					Where<
						APRecognizedTran.tranType.IsEqual<APRecognizedInvoice.docType.FromCurrent>.
						And<APRecognizedTran.refNbr.IsEqual<APRecognizedInvoice.refNbr.FromCurrent>>.
						And<APRecognizedTran.lineType.IsNotEqual<SOLineType.discount>>>.
					OrderBy<APRecognizedTran.tranType.Asc, APRecognizedTran.refNbr.Asc, APRecognizedTran.lineNbr.Asc>.
					View.ReadOnly.Select(this);
			}

			var cachedTransactions = new List<APRecognizedTran>();

			foreach (APRecognizedTran tran in Transactions.Cache.Cached)
			{
				var status = Transactions.Cache.GetStatus(tran);
				if (status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
				{
					cachedTransactions.Add(tran);
				}
			}

			return cachedTransactions;
		}

		private void RemoveAttachedFile()
		{
			if (Document.Current?.FileID == null)
			{
				return;
			}

			var fileMaint = CreateInstance<UploadFileMaintenance>();

			var fileLink = (NoteDoc)fileMaint.FileNoteDoc.Select(Document.Current.FileID, Document.Current.NoteID);
			if (fileLink == null)
			{
				return;
			}

			fileMaint.FileNoteDoc.Delete(fileLink);
			fileMaint.Persist();
			PXNoteAttribute.ResetFileListCache(Document.Cache);

			Document.Current.FileID = null;
		}

		[PXUIField(DisplayName = "Save and Continue")]
		[PXButton]
		public void continueSave()
		{
			var invoiceEntryGraph = CreateInstance<APInvoiceEntry>();
			using (var tran = new PXTransactionScope())
			{
				SaveFeedback();

				EnsureTransactions();

				Document.Cache.IsDirty = false;
				Transactions.Cache.IsDirty = false;

				invoiceEntryGraph.SelectTimeStamp();
				using (new APInvoiceFillFromRecognizedScope())
				{
				InsertInvoiceData(invoiceEntryGraph);
				}
				InsertCrossReferences(invoiceEntryGraph);
				InsertRecognizedRecordVendor(invoiceEntryGraph);
				tran.Complete();
			}
            
			throw new PXRedirectRequiredException(invoiceEntryGraph, false, null) { Mode = PXBaseRedirectException.WindowMode.InlineWindow };
		}

		private void SaveFeedback()
		{
			var recognizedRecord = RecognizedRecords.Current ?? RecognizedRecords.SelectSingle();
			var sb = new StringBuilder(recognizedRecord.RecognitionFeedback);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
			if (Document.Current.FeedbackBuilder == null)
			{
				return;
			}
            var feedbackList = Document.Current.FeedbackBuilder.ToTableFeedbackList(Transactions.View.Name);
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw){Formatting = Formatting.None})
            {
                foreach (var feedbackItem in feedbackList)
                {
                    sb.AppendLine();
                    _jsonSerializer.Serialize(jsonWriter,feedbackItem);
                }
            }
            RecognizedRecords.Cache.SetValue<RecognizedRecord.recognitionFeedback>(recognizedRecord, sw.ToString());
            RecognizedRecords.Cache.PersistUpdated(recognizedRecord);
        }

		[PXButton]
		[PXUIField(DisplayName = "Recognize")]
		public virtual IEnumerable processRecognition(PXAdapter adapter)
		{
			var refNbr = Document.Current.RecognizedRecordRefNbr.Value;
			var fileId = Document.Current.FileID.Value;

			var logger = _logger; //to avoid closing over graph
			PXLongOperation.StartOperation(this, method: () =>
			{
				RecognizeInvoiceData(refNbr, fileId, logger);
				PXLongOperation.SetCustomInfoPersistent(ReloadData.All);
			});

			return adapter.Get();
		}

		[PXButton]
		[PXUIField(DisplayName = "Refresh Status")]
		public virtual IEnumerable refreshStatus(PXAdapter adapter)
		{
			var refNbr = Document.Current.RecognizedRecordRefNbr.Value;
			var fileId = Document.Current.FileID.Value;
			var logger = _logger;

			PXLongOperation.StartOperation(this, method: () =>
			{
				RefreshInvoiceStatus(refNbr, logger);
				PXLongOperation.SetCustomInfoPersistent(ReloadData.All);
			});

			return adapter.Get();
		}

		[PXButton]
		[PXUIField(DisplayName = "Open Document")]
		public virtual void openDocument()
		{
			Document.Cache.IsDirty = false;
			Transactions.Cache.IsDirty = false;

			var recognizedInvoice = (APInvoice)
				SelectFrom<APInvoice>
				.Where<APInvoice.noteID.IsEqual<@P.AsGuid>>
				.View.ReadOnly
				.SelectSingleBound(this, null, Document.Current.DocumentLink);

			var graph = CreateInstance<APInvoiceEntry>();
			graph.Document.Current = recognizedInvoice;

			throw new PXRedirectRequiredException(graph, null);
		}

		[PXButton]
		[PXUIField(DisplayName = "Open Duplicate Document")]
		public virtual void openDuplicate()
		{
			var duplicatedRecognizedInvoice = (APRecognizedInvoice)
				SelectFrom<APRecognizedInvoice>
				.Where<APRecognizedInvoice.recognizedRecordRefNbr.IsEqual<APRecognizedInvoice.duplicateLink.FromCurrent>>
				.View.ReadOnly
				.SelectSingleBound(this, null);

			var graph = CreateInstance<APInvoiceRecognitionEntry>();
			graph.Document.Current = duplicatedRecognizedInvoice;

			throw new PXRedirectRequiredException(graph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		[PXButton(Tooltip = "Clear Table")]
		[PXUIField(DisplayName = "Clear Table")]
		public virtual void deleteAllTransactions()
		{
			foreach (APRecognizedTran tran in Transactions.Select())
			{
				Transactions.Delete(tran);
			}
		}

		[PXButton]
		[PXUIField(DisplayName = "View History")]
		public virtual void viewErrorHistory()
		{
			ErrorHistory.AskExt();
		}

		[PXButton]
		[PXUIField(DisplayName = "Search for Vendor")]
		public virtual IEnumerable searchVendor(PXAdapter adapter)
		{
			var record = RecognizedRecords.SelectSingle();
			var refNbr = record.RefNbr;
			LongOperationManager.StartAsyncOperation(async _ =>
			{
				await PopulateVendorId(refNbr);
				PXLongOperation.SetCustomInfoPersistent(ReloadData.VendorId);
			});

			return adapter.Get();
		}

		public static async Task PopulateVendorId(Guid? refNbr)
		{
			var graph = CreateInstance<APInvoiceRecognitionEntry>();
			PXResult row = SelectFrom<RecognizedRecord>.
				LeftJoin<RecognizedRecordDetail>.
				On<RecognizedRecord.entityType.IsEqual<RecognizedRecordDetail.entityType>.And<
				   RecognizedRecord.refNbr.IsEqual<RecognizedRecordDetail.refNbr>>>.
				Where<RecognizedRecord.entityType.IsEqual<RecognizedRecordEntityTypeListAttribute.aPDocument>.And<
					  RecognizedRecord.refNbr.IsEqual<@P.AsGuid>>>.
				View.ReadOnly.SelectSingleBound(graph, null, refNbr);
			var record = row[typeof(RecognizedRecord)] as RecognizedRecord;
			var detail = row[typeof(RecognizedRecordDetail)] as RecognizedRecordDetail;

			await graph.PopulateVendorId(record, detail);
		}

		[PXButton]
		[PXUIField]
		public virtual void dumpTableFeedback()
		{
			Document.Current.FeedbackBuilder?.DumpTableFeedback();
		}

		protected virtual void _(Events.RowDeleting<APRecognizedInvoice> e)
		{
			if (e.Row == null)
			{
				return;
			}

			var recognizedRecord = RecognizedRecords.SelectSingle();
			if (recognizedRecord == null)
			{
				return;
			}

			// Keep attachments for invoice
			if (recognizedRecord.Status == RecognizedRecordStatusListAttribute.Processed)
			{
				var isSharedNote = Document.Current.DocumentLink == Document.Current.NoteID;
				if (isSharedNote)
				{
					PXNoteAttribute.ForceRetain<RecognizedRecord.noteID>(RecognizedRecords.Cache);
					PXNoteAttribute.ForceRetain<APRecognizedInvoice.noteID>(e.Cache);
				}
			}

			recognizedRecord.RecognitionResult = null;

			// Update timestamp to delete row in case of invocation from PL
			SelectTimeStamp();
			RecognizedRecords.Cache.PersistUpdated(recognizedRecord);
			RecognizedRecords.Cache.ResetPersisted(recognizedRecord);

			// Update timestamp to get newer value then timestamp from record
			SelectTimeStamp();
			RecognizedRecords.Delete(recognizedRecord);
			UpdateDuplicates(recognizedRecord.RefNbr);

			Transactions.Cache.Clear();
		}

		protected virtual void _(Events.RowPersisting<APRecognizedInvoice> e)
		{
			e.Cancel = true;
		}

		protected virtual void _(Events.FieldDefaulting<APRecognizedInvoice.hold> e)
		{
			e.NewValue = true;
		}

		protected virtual void _(Events.FieldDefaulting<APRecognizedInvoice.allowFilesMsg> e)
		{
			e.NewValue = PXMessages.LocalizeFormatNoPrefixNLA(Web.UI.Msg.ErrFileTypesAllowed, PdfExtension);
		}

		protected virtual void _(Events.FieldDefaulting<APInvoice.docDate> e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void _(Events.FieldDefaulting<BAccountR.type> e)
		{
			e.NewValue = BAccountType.VendorType;
		}

		//This handler is kept to avoid breaking changes
		protected virtual void _(Events.FieldUpdated<APRecognizedInvoice.vendorID> e)
		{
		}

		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				Vendor vendor = CurrentVendor.Select();
				GL.Branch branch = CurrentBranch.Select();
				if (vendor != null && vendor.CuryID != null)
				{
					e.NewValue = vendor.CuryID;
					e.Cancel = true;
				}
				else if (branch != null && !string.IsNullOrEmpty(branch.BaseCuryID))
				{
					e.NewValue = branch.BaseCuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<APRecognizedInvoice.vendorLocationID> e)
		{
			var transactions = new List<APRecognizedTran>();

			foreach (APRecognizedTran tran in Transactions.Select())
			{
				if (tran.InventoryIDManualInput != true)
				{
					SetTranInventoryID(Transactions.Cache, tran);
				}

				transactions.Add(tran);
			}

			if (transactions.Count > 0)
			{
				SetTranRecognizedPONumbers(Transactions.Cache, transactions, false);
			}
		}

		private void SetTranRecognizedPONumbers(int? vendorId, HashSet<int?> inventoryIds)
		{
			SetTranRecognizedPONumbers(vendorId, inventoryIds, null);
		}

		private void SetTranRecognizedPONumbers(int? vendorId, HashSet<int?> inventoryIds, APRecognizedTran apTran)
		{
			var recognizedPONumbers = GetRecognizedPONumbers(vendorId, inventoryIds);

			if (apTran == null)
			{
				foreach (APRecognizedTran tran in Transactions.Select())
				{
					if (!inventoryIds.Contains(tran.InventoryID))
					{
						continue;
					}

					SetPOLink(tran, recognizedPONumbers);
				}
			}
			else
			{
				if (!inventoryIds.Contains(apTran.InventoryID))
				{
					return;
				}

				SetPOLink(apTran, recognizedPONumbers);
			}
		}

		public virtual void SetPOLink(APRecognizedTran apTran, IList<(int? InventoryId, string PONumber, PageWord PageWord)> recognizedPONumbers)
		{
			(int? _, string poNumber, PageWord pageWord) = recognizedPONumbers.FirstOrDefault(r => apTran.InventoryID == r.InventoryId);

			var poNumberJson = JsonConvert.SerializeObject(pageWord);
			var poNumberEncodedJson = HttpUtility.UrlEncode(poNumberJson);

			Transactions.Cache.SetValueExt<APRecognizedTran.pONumberJson>(apTran, poNumberEncodedJson);
			AutoLinkAPAndPO(apTran, poNumber);
		}

		//see LinkRecognizedLineExtension for implementation
		public virtual void AutoLinkAPAndPO(APRecognizedTran tran, string poNumber)
		{
		}

		private IList<(int? InventoryId, string PONumber, PageWord PageWord)> GetRecognizedPONumbers(int? vendorId, HashSet<int?> inventoryIds)
		{
			var poNumbers = new List<(int? inventoryId, string pONumber, PageWord pageWord)>();

			var recognizedRecord = RecognizedRecords.SelectSingle();
			if (recognizedRecord == null || string.IsNullOrEmpty(recognizedRecord.RecognitionResult))
			{
				return poNumbers;
			}

			var recognitionResultTyped = JsonConvert.DeserializeObject<DocumentRecognitionResult>(recognizedRecord.RecognitionResult);
			var pageCount = recognitionResultTyped?.Pages?.Count;

			if (pageCount == null || pageCount == 0)
			{
				return poNumbers;
			}

			var poInfos = GetPONumbers(vendorId, inventoryIds);
			if (poInfos.Count == 0)
			{
				return poNumbers;
			}

			foreach (var (inventoryId, pONumber) in poInfos)
			{
				PageWord pageWord = null;

				for (var pageIndex = 0; pageIndex < recognitionResultTyped.Pages?.Count && pageWord == null; pageIndex++)
				{
					var page = recognitionResultTyped.Pages[pageIndex];

					for (var wordIndex = 0; wordIndex < page?.Words?.Count && pageWord == null; wordIndex++)
					{
						var word = page.Words[wordIndex];

						if (!string.Equals(word?.Text, pONumber, StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}

						pageWord = new PageWord
						{
							Page = pageIndex,
							Word = wordIndex
						};
					}
				}

				if (pageWord == null)
				{
					continue;
				}

				poNumbers.Add((inventoryId, pONumber, pageWord));
			}

			return poNumbers;
		}

		protected virtual void _(Events.RowInserted<APRecognizedInvoice> e)
		{
            if (e.Row != null)
                Caches[typeof(APRegister)].Current = e.Row;
        }

		protected virtual void _(Events.RowSelected<APRecognizedInvoice> e)
		{
			Document.View.SetAnswer(null, WebDialogResult.OK);

            if (!(e.Row is APRecognizedInvoice document)) return;
            if(e.Row.DocType==null)
                DefaultInvoiceValues(document);
            var recognizedRecord = RecognizedRecords.Current??RecognizedRecords.SelectSingle();
            if (recognizedRecord != null)
                e.Row.RecognitionStatus = recognizedRecord.Status;
            if (e.Row.IsRedirect == true)
            {
				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs to load data on redirect
				e.Row.IsDataLoaded = false;
				e.Row.IsRedirect = false;
            }

			var reloadData = PXLongOperation.GetCustomInfoPersistent(UID) as ReloadData?;
			if (e.Row.RecognizedRecordRefNbr != null && (e.Row.IsDataLoaded != true || reloadData != null))
            {
				PXLongOperation.RemoveCustomInfoPersistent(UID);
                RecognizedRecords.Cache.SetValue<RecognizedRecord.recognitionFeedback>(recognizedRecord, null);
				var reloadVendorId = reloadData == ReloadData.VendorId;
				// Acuminator disable once PX1046 LongOperationInEventHandlers to send vendor feedback right away
				LoadRecognizedData(reloadVendorId);
			}

			if (e.Row.NoteID != null)
            {
                ProcessFile(e.Cache, e.Row);
            }

            e.Row.AllowUploadFile = e.Row.RecognitionStatus ==
                                    APRecognizedInvoiceRecognitionStatusListAttribute.PendingFile ||
                                    e.Row.RecognitionStatus ==
                                    RecognizedRecordStatusListAttribute.PendingRecognition ||
                                    e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Error;

			var showDelete = e.Row.RecognitionStatus != APRecognizedInvoiceRecognitionStatusListAttribute.PendingFile;
            Delete.SetVisible(showDelete);

            var showSaveContinue = e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Recognized ||
                                   e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Error;
            ContinueSave.SetVisible(showSaveContinue);

			var showProcessRecognition = e.Row.FileID != null &&
										 (e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.PendingRecognition ||
										 e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Error);

            ProcessRecognition.SetVisible(showProcessRecognition);

            var showOpenDocument = e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Processed;
            OpenDocument.SetVisible(showOpenDocument);

            var showOpenDuplicate = e.Row.DuplicateLink != null;
            OpenDuplicate.SetVisible(showOpenDuplicate);
			SetWarningOnStatus(e.Cache, e.Row, showOpenDuplicate, recognizedRecord);

			var enableDeleteAllTransactions = e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Recognized;
			DeleteAllTransactions.SetEnabled(enableDeleteAllTransactions);

			if (StatusValidForRecognitionSet.Contains(e.Row.RecognitionStatus) && e.Row.FileID == null)
			{
				PXUIFieldAttribute.SetWarning<APRecognizedInvoice.recognitionStatus>(e.Cache, e.Row, Messages.CannotRecognizeNoFile);
			}
			else if (e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Error && !string.IsNullOrEmpty(e.Row.ErrorMessage))
			{
				PXUIFieldAttribute.SetError<APRecognizedInvoice.recognitionStatus>(e.Cache, e.Row, e.Row.ErrorMessage, e.Row.RecognitionStatus);
			}

            var allowEdit = e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Recognized ||
                            e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Error;
            Document.AllowInsert = allowEdit;
            Document.AllowUpdate = allowEdit;
            Document.AllowDelete = allowEdit;
            Transactions.AllowInsert = allowEdit;
            Transactions.AllowUpdate = allowEdit;
            Transactions.AllowDelete = allowEdit;

			var showViewErrorHistory = e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Error && ErrorHistory.Select().Count > 0;
			ViewErrorHistory.SetVisible(showViewErrorHistory);

			var showSearchVendor = e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Recognized;
			SearchVendor.SetVisible(showSearchVendor);

			var showRefreshStatus = e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.InProgress &&
				PXTimeZoneInfo.Now >= recognizedRecord?.LastModifiedDateTime.Value.AddMinutes(_recognitionTimeoutMinutes * 2);
			RefreshStatus.SetVisible(showRefreshStatus);

			HideNotSupportedActions();
		}

		private void SetWarningOnStatus(PXCache invoiceCache, APRecognizedInvoice recognizedInvoice, bool showOpenDuplicate,
			RecognizedRecord recognizedRecord)
		{
			var duplicateWarning = GetDuplicateFileWarning(showOpenDuplicate, recognizedRecord);
			var manyPagesWarning = GetFileHasManyPagesWarning(recognizedRecord);
			var warning = new StringBuilder();

			if (!string.IsNullOrEmpty(duplicateWarning))
			{
				warning.Append(duplicateWarning);
			}

			if (!string.IsNullOrEmpty(manyPagesWarning))
			{
				if (warning.Length > 0)
				{
					warning.AppendLine();
				}

				warning.Append(manyPagesWarning);
			}

			if (warning.Length == 0)
			{
				return;
			}

			PXUIFieldAttribute.SetWarning<APRecognizedInvoice.recognitionStatus>(invoiceCache, recognizedInvoice, warning.ToString());
		}

		private string GetDuplicateFileWarning(bool showOpenDuplicate, RecognizedRecord recognizedRecord)
		{
			if (!showOpenDuplicate || recognizedRecord == null)
			{
				return null;
			}

			var (_, subject) = CheckForDuplicates(recognizedRecord.RefNbr, recognizedRecord.FileHash);
			if (subject == null)
			{
				return null;
			}

			return PXMessages.LocalizeFormatNoPrefixNLA(Messages.DuplicateFileForRecognitionTooltip, subject);
		}

		private static string GetFileHasManyPagesWarning(RecognizedRecord recognizedRecord)
		{
			if (recognizedRecord == null)
			{
				return null;
			}

			return recognizedRecord.PageCount > MaxFilePageCount ?
				PXMessages.LocalizeNoPrefix(Messages.DocumentHasManyPages) :
				null;
		}

		private void HideNotSupportedActions()
		{
			var attachFromScanner = Actions[ActionsMessages.AttachFromScanner];
			if (attachFromScanner != null)
			{
				attachFromScanner.SetVisible(false);
			}

			// Replace the action as we cannot control its visibility
			// because it is based not on a primary view
			var attachFromMobile = Actions[nameof(AttachFromMobile)];
			if (attachFromMobile != null)
			{
				Actions[nameof(AttachFromMobile)] = AttachFromMobile;
			}
        }

		protected virtual void _(Events.FieldUpdated<APRecognizedInvoice.docType> e)
		{
			if (!(e.Args.Row is APRecognizedInvoice row))
			{
				return;
			}

			var docType = row.DocType;
			var drCr = row.DrCr;

			foreach (APRecognizedTran tran in Transactions.Select())
			{
				Transactions.Cache.SetValue<APRecognizedTran.tranType>(tran, docType);
				Transactions.Cache.SetValue<APRecognizedTran.drCr>(tran, drCr);

				if (tran.InventoryID != null)
				{
					object inventoryId = tran.InventoryID;

					try
					{
						Transactions.Cache.RaiseFieldVerifying<APRecognizedTran.inventoryID>(tran, ref inventoryId);
					}
					catch (PXSetPropertyException exception)
					{
						Transactions.Cache.RaiseExceptionHandling<APRecognizedTran.inventoryID>(tran, inventoryId, exception);
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdating<BoundFeedback.tableRelated> e)
		{
			var document = Document.Current;
			if (document == null)
			{
				return;
			}

			var unsupportedDocType = !APDocType.Invoice.Equals(document.DocType, StringComparison.Ordinal);
			if (unsupportedDocType)
			{
				return;
			}

			var feedbackBuilder = document.FeedbackBuilder;
			if (feedbackBuilder == null)
			{
				return;
			}

			var cellBoundJsonEncoded = e.NewValue as string;
			if (string.IsNullOrWhiteSpace(cellBoundJsonEncoded))
			{
				return;
			}

			var cellBoundJson = HttpUtility.UrlDecode(cellBoundJsonEncoded);
			feedbackBuilder.ProcessCellBound(cellBoundJson);

			e.NewValue = null;
		}

		protected virtual void _(Events.FieldUpdating<BoundFeedback.fieldBound> e)
		{
			var document = Document.Current;
            var recognizedRecord = RecognizedRecords.Current;
			if (document == null || recognizedRecord == null)
			{
				return;
			}

            var unsupportedDocType = !APDocType.Invoice.Equals(document.DocType, StringComparison.Ordinal);
			if (unsupportedDocType)
			{
				return;
			}

			var feedbackBuilder = document.FeedbackBuilder;
			if (feedbackBuilder == null)
			{
				return;
			}

			var documentJsonEncoded = e.NewValue as string;
			if (string.IsNullOrWhiteSpace(documentJsonEncoded))
			{
				return;
			}

			var documentJson = HttpUtility.UrlDecode(documentJsonEncoded);
			var fieldBoundFeedback = feedbackBuilder.ToFieldBoundFeedback(documentJson);
			if (fieldBoundFeedback == null)
			{
				return;
			}

            var sb = new StringBuilder(recognizedRecord.RecognitionFeedback);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw){Formatting = Formatting.None})
            {
                sb.AppendLine();
                _jsonSerializer.Serialize(jsonWriter,fieldBoundFeedback);
            }
            RecognizedRecords.Cache.SetValue<RecognizedRecord.recognitionFeedback>(recognizedRecord, sw.ToString());
            e.NewValue = null;
		}

		protected virtual void _(Events.RowPersisting<APRecognizedTran> e)
		{
			e.Cancel = true;
		}

		protected virtual void _(Events.RowSelected<APRecognizedTran> e)
		{
			if (e.Row == null)
			{
				return;
			}

			var alternateIdWarning = e.Row.AlternateID != null && e.Row.NumOfFoundIDByAlternate > 1 ?
				PXMessages.LocalizeNoPrefix(CrossItemAttribute.CrossItemMessages.ManyItemsForCurrentAlternateID):
				null;

			PXUIFieldAttribute.SetWarning<APRecognizedTran.alternateID>(e.Cache, e.Row, alternateIdWarning);
		}

		protected virtual void _(Events.FieldUpdated<APRecognizedTran.alternateID> e)
		{
			if (!(e.Row is APRecognizedTran row))
			{
				return;
			}

			SetTranInventoryID(e.Cache, row);
			SetTranRecognizedPONumbers(e.Cache, new[] { row }, false);
		}

		private void SetTranInventoryID(PXCache cache, APRecognizedTran tran)
		{
			cache.SetValueExt<APRecognizedTran.internalAlternateID>(tran, tran.AlternateID);

			if (tran.InternalAlternateID == null && tran.InventoryIDManualInput == true)
			{
				return;
			}

			cache.SetValueExt<APRecognizedTran.inventoryID>(tran, tran.InternalAlternateID);
			Transactions.View.RequestRefresh();
		}

		protected virtual void _(Events.FieldUpdated<APRecognizedTran.inventoryID> e)
		{
			if (!(e.Row is APRecognizedTran row))
			{
				return;
			}

			var inventoryId = e.NewValue as int?;
			if (inventoryId != null)
			{
				e.Cache.SetDefaultExt<APRecognizedTran.uOM>(row);
			}

			var isManualInput = e.ExternalCall && e.NewValue != null;
			SetTranRecognizedPONumbers(e.Cache, new[] { row }, isManualInput);
		}

		private void SetTranRecognizedPONumbers(PXCache cache, IEnumerable<APRecognizedTran> transactions, bool isManualInput)
		{
			var vendorId = Document.Current?.VendorID;
			var inventoryIds = new HashSet<int?>();

			foreach (var tran in transactions)
			{
				if (tran.InventoryID != null)
				{
					if (tran.InventoryIDManualInput != true)
					{
						tran.InventoryIDManualInput = isManualInput;
					}

					if (vendorId != null)
					{
						inventoryIds.Add(tran.InventoryID);
					}
					else
					{
						cache.SetValueExt<APRecognizedTran.pONumberJson>(tran, null);
						AutoLinkAPAndPO(tran, null);
					}
				}
				else
				{
					cache.SetValueExt<APRecognizedTran.pONumberJson>(tran, null);
					AutoLinkAPAndPO(tran, null);
				}
			}

			if (inventoryIds.Count > 0)
			{
				if (transactions.Count() == 1)
				{
					SetTranRecognizedPONumbers(vendorId, inventoryIds, transactions.First());
				}
				else
				{
					SetTranRecognizedPONumbers(vendorId, inventoryIds);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<APRecognizedTran.uOM> e)
		{
			if (!(e.Row is APRecognizedTran row))
			{
				return;
			}
			if (e.ExternalCall)
			{
				var uOM = e.NewValue as string;
				if (uOM == null)
				{
					AutoLinkAPAndPO(row, null);
				}
			}
		}

		protected virtual void _(Events.RowPersisting<CurrencyInfo> e)
		{
			e.Cancel = true;
		}

		internal static bool IsAllowedFile(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return false;
			}

			var fileExtension = Path.GetExtension(name);

			return string.Equals(fileExtension, PdfExtension, StringComparison.OrdinalIgnoreCase);
		}

		private void LoadRecognizedData(bool reloadVendorId)
		{
			var recognizedRecord = RecognizedRecords.SelectSingle();
			if(recognizedRecord==null)
			{
				return;
			}

			if (string.IsNullOrEmpty(recognizedRecord.RecognitionResult))
			{
				Document.Current.RecognitionStatus = recognizedRecord.Status;
				Document.Current.DuplicateLink = recognizedRecord.DuplicateLink;

				return;
			}

			if (Caches[typeof(APRegister)].Current != Document.Current)
			{
				Caches[typeof(APRegister)].Current = Document.Current;
			}

			var detail = RecognizedRecordDetails.SelectSingle(recognizedRecord.RefNbr, recognizedRecord.EntityType);
			var recognitionResult = JsonConvert.DeserializeObject<DocumentRecognitionResult>(recognizedRecord.RecognitionResult);

			LoadRecognizedDataToGraph(this, recognizedRecord, detail, recognitionResult, reloadVendorId);
		}

		private void ProcessFile(PXCache cache, APRecognizedInvoice invoice)
        {
			// File notes has random order as NoteDoc doesn't contain CreatedDateTime column
            var fileNotes = PXNoteAttribute.GetFileNotes(cache, invoice);

			if (fileNotes == null || fileNotes.Length == 0)
			{
				if (invoice.FileID != null)
				{
					RemoveAttachedFile();
					UpdateFileInfo(null);

					invoice.FileID = null;
				}

				return;
			}

			var fileId = fileNotes[0];
			var file = GetFile(this, fileId);

			if (invoice.RecognitionStatus == APRecognizedInvoiceRecognitionStatusListAttribute.PendingFile)
			{
				invoice.RecognitionStatus = RecognizedRecordStatusListAttribute.PendingRecognition;

				var recognizedRecord = CreateRecognizedRecord(file.Name, file.Data, invoice, fileId);

                invoice.EntityType = recognizedRecord.EntityType;
                invoice.FileHash = recognizedRecord.FileHash;
                invoice.RecognitionStatus = recognizedRecord.Status;
				invoice.DuplicateLink = recognizedRecord.DuplicateLink;
			}
			else if (invoice.FileID != null)
			{
				// File notes ordered by created time descending
				if (invoice.FileID != fileId)
				{
					RemoveAttachedFile();
					UpdateFileInfo(file);
				}
				// File notes ordered by created time ascending
				else if (fileNotes.Length == 2)
			{
					fileId = fileNotes[1];
					file = GetFile(this, fileId);

					RemoveAttachedFile();
				UpdateFileInfo(file);
			}
			}

			if (file == null)
			{
				return;
			}

			invoice.FileID = fileId;

			// To load restricted file by page via GetFile.ashx
			var fileInfoInMemory = new PX.SM.FileInfo(fileId, file.Name, null, file.Data);
			PXContext.SessionTyped<PXSessionStatePXData>().FileInfo[fileInfoInMemory.UID.ToString()] = fileInfoInMemory;
		}

        

        private void UpdateFileInfo(UploadFile file)
		{
			var recognizedRecord = RecognizedRecords.SelectSingle();
			if (recognizedRecord == null)
			{
				return;
			}

			if (file == null)
			{
				recognizedRecord.Subject = null;
				recognizedRecord.FileHash = null;
				recognizedRecord.DuplicateLink = null;
			}
			else
			{
				var originalFileName = PX.SM.FileInfo.GetShortName(file.Name);
				recognizedRecord.Subject = GetRecognizedSubject(null, originalFileName);
				recognizedRecord.FileHash = ComputeFileHash(file.Data);

				SetDuplicateLink(recognizedRecord);
			}

			recognizedRecord.Owner = PXAccess.GetContactID();

			Caches[typeof(Note)].Clear();
			SelectTimeStamp();
			RecognizedRecords.Cache.PersistUpdated(recognizedRecord);
			RecognizedRecords.Cache.Clear();
			RecognizedRecords.Cache.IsDirty = false;
			SelectTimeStamp();
		}

		public RecognizedRecord CreateRecognizedRecord(string fileName, byte[] fileData, Guid fileId, string description = null, string mailFrom = null, string messageId = null,
			int? owner = null, Guid? noteId = null)
		{
			var recognizedRecord = RecognizedRecords.Insert();
			var originalFileName = PX.SM.FileInfo.GetShortName(fileName);

			recognizedRecord.Subject = description ?? GetRecognizedSubject(null, originalFileName);
			recognizedRecord.MailFrom = mailFrom;
			recognizedRecord.MessageID = string.IsNullOrWhiteSpace(messageId) ?
				messageId :
				NormalizeMessageId(messageId);
			recognizedRecord.FileHash = ComputeFileHash(fileData ?? new byte[0]);
			recognizedRecord.Owner = owner ?? PXAccess.GetContactID();
            recognizedRecord.CloudTenantId = _cloudTenantService.TenantId;
            recognizedRecord.ModelName = InvoiceRecognitionClient.ModelName;
            recognizedRecord.CloudFileId = fileId;
			if (noteId != null)
			{
				recognizedRecord.NoteID = noteId;
			}

			SetDuplicateLink(recognizedRecord);
			SetFilePageCount(recognizedRecord, fileData);

			RecognizedRecords.Cache.PersistInserted(recognizedRecord);
			RecognizedRecords.Cache.Persisted(false);
			SelectTimeStamp();

			return recognizedRecord;
		}

        private RecognizedRecord CreateRecognizedRecord(string fileName, byte[] fileData, APRecognizedInvoice recognizedInvoice, Guid fileId)
        {
            fileName.ThrowOnNullOrWhiteSpace(nameof(fileName));
            fileData.ThrowOnNull(nameof(fileData));
            var recognizedRecord = (RecognizedRecord) RecognizedRecords.Cache.CreateInstance();
            recognizedRecord.NoteID = recognizedInvoice.NoteID;
            recognizedRecord.CustomInfo = recognizedInvoice.CustomInfo;
            recognizedRecord.DocumentLink = recognizedInvoice.DocumentLink;
            recognizedRecord.DuplicateLink = recognizedInvoice.DuplicateLink;
            recognizedRecord.EntityType = recognizedInvoice.EntityType;
            recognizedRecord.FileHash = ComputeFileHash(fileData);
            recognizedRecord.MailFrom = recognizedInvoice.MailFrom;
            recognizedRecord.MessageID = recognizedInvoice.MessageID;
            recognizedRecord.Owner = recognizedInvoice.Owner ?? PXAccess.GetContactID();
            recognizedRecord.RecognitionResult = recognizedInvoice.RecognitionResult;
            recognizedRecord.RecognitionStarted = recognizedInvoice.RecognitionStarted;
            recognizedRecord.RefNbr = recognizedInvoice.RecognizedRecordRefNbr;
            recognizedRecord.Status = recognizedInvoice.RecognitionStatus;
            var originalFileName = PX.SM.FileInfo.GetShortName(fileName);
            recognizedRecord.Subject = GetRecognizedSubject(null, originalFileName);
			SetDuplicateLink(recognizedRecord);
            recognizedRecord.CloudTenantId = _cloudTenantService.TenantId;
            recognizedRecord.ModelName = InvoiceRecognitionClient.ModelName;
            recognizedRecord.CloudFileId = fileId;
			SetFilePageCount(recognizedRecord, fileData);
            recognizedRecord = (RecognizedRecord)RecognizedRecords.Cache.Insert(recognizedRecord);
			RecognizedRecords.Cache.SetStatus(recognizedRecord, PXEntryStatus.Notchanged);
            RecognizedRecords.Cache.PersistInserted(recognizedRecord);
			RecognizedRecords.Cache.IsDirty = false;
			SelectTimeStamp();
            return recognizedRecord;
        }

		internal static byte[] ComputeFileHash(byte[] data)
		{
			using (var provider = new MD5CryptoServiceProvider())
			{
				return provider.ComputeHash(data);
			}
		}

		private static void SetFilePageCount(RecognizedRecord record, byte[] data)
		{
			if (PdfReader.TestPdfFile(data) == 0)
			{
				return;
			}

			int pageCount;
			var stream = new MemoryStream(data);

			try
			{
				using var document = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly);
				pageCount = document.PageCount;
			}
			catch (Exception e)
			{
				PXTrace.WriteError(e);
				return;
			}

			record.PageCount = pageCount;
		}

		private void SetDuplicateLink(RecognizedRecord recognizedRecord)
		{
			var (refNbr, _) = CheckForDuplicates(recognizedRecord.RefNbr, recognizedRecord.FileHash);

			recognizedRecord.DuplicateLink = refNbr;
		}

		private void EnsureTransactions()
		{
			var detailsNotEmpty = Transactions.Cache.Cached
				.Cast<object>()
				.Any();
			if (detailsNotEmpty)
			{
                foreach (APRecognizedTran tran in Transactions.Cache.Cached)
                {
                    var documentCuryInfo = Document.Current?.CuryInfoID;
                    if(tran.CuryInfoID!=documentCuryInfo)
                        Transactions.Cache.SetValue<APRecognizedTran.curyInfoID>(tran, documentCuryInfo);
                }
				return;
			}

			var document = Document.Current;
			if (document == null)
			{
				return;
			}

			var summaryDetail = Transactions.Insert();
			if (summaryDetail == null)
			{
				return;
			}

			summaryDetail.TranDesc = document.DocDesc;
			summaryDetail.CuryLineAmt = document.CuryOrigDocAmt;

			Transactions.Update(summaryDetail);
		}

		private void InsertRowWithFieldValues(PXCache sourceCache, PXCache destCache, IEnumerable<string> fieldsToCopy,
			HashSet<string> forcedFields, object sourceRow, Action<object> onAfterInsert = null)
		{
			var newRow = destCache.Insert();

			onAfterInsert?.Invoke(newRow);

			var cacheFields = fieldsToCopy.Where(field => destCache.Fields.Contains(field));

			foreach(var field in cacheFields)
			{
				var valueExt = sourceCache.GetValueExt(sourceRow, field);
				var value = PXFieldState.UnwrapValue(valueExt);
				if (value == null)
				{
					continue;
				}

				// To obtain correct state
				destCache.RaiseRowSelected(newRow);

				var state = destCache.GetStateExt(newRow, field) as PXFieldState;
				if (state?.Enabled == false && forcedFields?.Contains(field) != true)
				{
					continue;
				}

				destCache.SetValueExt(newRow, field, new PXCache.ExternalCallMarker(value));
				destCache.Update(newRow);
			}

			destCache.SetStatus(newRow, PXEntryStatus.Inserted);
		}

		private void CopyFiles(APInvoiceEntry graph)
		{
			graph.Document.Cache.SetValueExt<APInvoice.noteID>(graph.Document.Current, null);
			graph.Caches<APVendorRefNbr>().Clear(); // It contains old row with old noteID
			PXNoteAttribute.CopyNoteAndFiles(Document.Cache, Document.Current, graph.Document.Cache, graph.Document.Current, false, true);
			graph.Document.Cache.SetValue<APInvoiceExt.renameFileScreenId>(graph.Document.Current, true);
		}

		private void InsertInvoiceData(APInvoiceEntry graph)
		{
			var (primaryFields, detailFields) = GetUIFields();
			var holdField = new[] { nameof(APInvoice.Hold) };
			primaryFields = holdField.Union(primaryFields, StringComparer.OrdinalIgnoreCase);

			InsertRowWithFieldValues(Document.Cache, graph.Document.Cache, primaryFields, holdField.ToHashSet(), Document.Current);
			CopyFiles(graph);

			var manualFields = new[] { nameof(APTran.ManualPrice), nameof(APTran.ManualDisc) };
			detailFields = manualFields.Union(detailFields, StringComparer.OrdinalIgnoreCase);
			var detailFieldsWithPO = detailFields.Union(LinkRecognizedLineExtension.APTranPOFields, StringComparer.OrdinalIgnoreCase);
			var detailPOFields = LinkRecognizedLineExtension.APTranPOFields;
			foreach (APRecognizedTran tran in Transactions.Select())
			{
				IEnumerable<string> fieldsToCopy;
				HashSet<string> forcedFields;

				tran.ManualPrice = true;
				tran.ManualDisc = true;

				if (tran.POLinkStatus == APPOLinkStatus.Linked)
				{
					fieldsToCopy = detailFieldsWithPO;
					forcedFields = detailPOFields
						.Union(manualFields, StringComparer.OrdinalIgnoreCase)
						.ToHashSet();
				}
				else
			{
					fieldsToCopy = detailFields;
					forcedFields = manualFields.ToHashSet();
				}

				InsertRowWithFieldValues(Transactions.Cache, graph.Transactions.Cache, fieldsToCopy, forcedFields, tran);
			}

			var invoiceEntryExt = graph.GetExtension<APInvoiceEntryExt>();
			invoiceEntryExt.FeedbackParameters.Current.FeedbackBuilder = Document.Current.FeedbackBuilder;
			invoiceEntryExt.FeedbackParameters.Current.Links = Document.Current.Links;

			var recognizedRecord = PXSelect<RecognizedRecord, Where<RecognizedRecord.refNbr, Equal<PX.Data.Required<RecognizedRecord.refNbr>>>>.Select(graph, Document.Current.RecognizedRecordRefNbr).FirstTableItems.FirstOrDefault();
			if(recognizedRecord==null)
				return;
            recognizedRecord.DocumentLink = graph.Document.Current.NoteID;
			recognizedRecord.Status = RecognizedRecordStatusListAttribute.Processed;

			var recognizedRecordCache = graph.Caches[typeof(RecognizedRecord)];
			recognizedRecordCache.Update(recognizedRecord);

			RecognizedRecords.View.Clear();
		}

		private void InsertCrossReferences(APInvoiceEntry graph)
		{
			if (Document.Current?.VendorID == null)
			{
				return;
			}

			var transactionsWitXref = Transactions
				.Select()
				.Select(r => r.GetItem<APRecognizedTran>())
				.Where(t => !string.IsNullOrEmpty(t.AlternateID) &&
							t.InventoryID != null)
				.ToList();
			if (transactionsWitXref.Count == 0)
			{
				return;
			}

			var xRefCache = graph.Caches[typeof(INItemXRef)];
			xRefCache.RaiseFieldDefaulting<INItemXRef.subItemID>(null, out var defSubItemId);

			var xRefView = new
				SelectFrom<INItemXRef>.
				Where<Match<AccessInfo.userName.FromCurrent>.And<
					  INItemXRef.inventoryID.IsEqual<@P.AsInt>.And<
					  INItemXRef.alternateType.IsEqual<INAlternateType.vPN>>.And<
					  INItemXRef.bAccountID.IsEqual<@P.AsInt>>>.And<
					  INItemXRef.alternateID.IsEqual<@P.AsString>>.And<
					  INItemXRef.subItemID.IsEqual<@P.AsInt>>>.
				View.ReadOnly(graph);

			foreach (var tran in transactionsWitXref)
			{
				var isRefExists = xRefView.SelectSingle(tran.InventoryID, Document.Current.VendorID, tran.AlternateID, defSubItemId) != null;
				if (isRefExists)
				{
					continue;
				}

				var newXref = new INItemXRef
				{
					InventoryID = tran.InventoryID,
					AlternateType = INAlternateType.VPN,
					BAccountID = Document.Current.VendorID,
					AlternateID = tran.AlternateID,
					SubItemID = defSubItemId as int?
				};

				xRefCache.Insert(newXref);
			}
		}

		private void InsertRecognizedRecordVendor(APInvoiceEntry graph)
		{
			if (string.IsNullOrEmpty(Document.Current?.VendorName))
			{
				return;
			}

			var recognizedRecordVendor = new RecognizedVendorMapping
			{
				VendorNamePrefix = RecognizedVendorMapping.GetVendorPrefixFromName(Document.Current.VendorName),
				VendorName = Document.Current.VendorName
			};

			var cache = graph.Caches[typeof(RecognizedVendorMapping)];
			cache.Insert(recognizedRecordVendor);
			cache.SetStatus(recognizedRecordVendor, PXEntryStatus.Held);
		}

		internal static string NormalizeMessageId(string rawMessageId)
		{
			rawMessageId.ThrowOnNullOrWhiteSpace(nameof(rawMessageId));

			var braceIndex = rawMessageId.IndexOf('>');
			if (braceIndex == -1 || braceIndex == rawMessageId.Length - 1)
			{
				return rawMessageId;
			}

			return rawMessageId.Substring(0, braceIndex + 1);
		}

		internal static string GetRecognizedSubject(string emailSubject, string fileName)
		{
			if (string.IsNullOrWhiteSpace(emailSubject))
			{
				return fileName;
			}

			return $"{emailSubject}: {fileName}";
		}

		public (Guid? RefNbr, string Subject) CheckForDuplicates(Guid? recognizedRefNbr, byte[] fileHash)
		{
			var duplicateRecord = (RecognizedRecord)
				SelectFrom<RecognizedRecord>
				.Where<RecognizedRecord.refNbr.IsNotEqual<@P.AsGuid>.And<
					   RecognizedRecord.fileHash.IsEqual<@P.AsByteArray>>>
				.OrderBy<RecognizedRecord.createdDateTime.Asc>
				.View.ReadOnly.Select(this, recognizedRefNbr, fileHash);

			if (duplicateRecord == null)
			{
				return (null, null);
			}

			return (duplicateRecord.RefNbr, duplicateRecord.Subject);
		}

		public void UpdateDuplicates(Guid? refNbr)
		{
			var duplicatesView = new SelectFrom<RecognizedRecord>
				.Where<RecognizedRecord.duplicateLink.IsEqual<@P.AsGuid>>
				.OrderBy<RecognizedRecord.createdDateTime.Asc>
				.View(this);
			var newDuplicateLink = default(Guid?);

			foreach (RecognizedRecord record in duplicatesView.Select(refNbr))
			{
				if (newDuplicateLink == null)
				{
					newDuplicateLink = record.RefNbr;
					record.DuplicateLink = null;
				}
				else
				{
					record.DuplicateLink = newDuplicateLink;
				}

				RecognizedRecords.Update(record);
			}
		}

		public static void RefreshInvoiceStatus(Guid recognizedRecordRefNbr, ILogger logger)
		{
			var graph = CreateInstance<APInvoiceRecognitionEntry>();

			graph.RefreshInvoiceStatusInternal(recognizedRecordRefNbr, logger);
		}

		public static void RecognizeInvoiceData(Guid recognizedRecordRefNbr, Guid fileId, ILogger logger)
		{
			var graph = CreateInstance<APInvoiceRecognitionEntry>();
           
            graph.RecognizeInvoiceDataInternal(recognizedRecordRefNbr, fileId, logger);
        }

		private void RefreshInvoiceStatusInternal(Guid recognizedRecordRefNbr, ILogger logger)
		{
			var startRow = 0;
			var maximumRows = 1;
			var totalRows = 1;
			var searches = new object[] { recognizedRecordRefNbr };
			var sortColumns = new[] { nameof(APRecognizedInvoice.recognizedRecordRefNbr) };
			Document.Current = Document.View.Select(currents: null, parameters: null, searches, sortColumns, descendings: null,
				filters: null, ref startRow, maximumRows, ref totalRows).First() as APRecognizedInvoice;

			var recognizedRecord = RecognizedRecords.SelectSingle();
			string errorMessage = null;
			DocumentRecognitionResult recognitionResult = null;

			using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(_recognitionTimeoutMinutes));
			try
			{
				var recognitionResponse = new DocumentRecognitionResponse(recognizedRecord.ResultUrl);
				recognitionResult = PollForResults(this, recognizedRecord, InvoiceRecognitionClient, recognitionResponse, logger,
					cancellationTokenSource.Token).Result;
			}
			catch (AggregateException e)
			{
				errorMessage = GetRecognitionErrorMessage(e, recognizedRecord.PageCount);
				throw;
			}
			finally
			{
				UpdateRecognizedRecord(this, recognizedRecord, recognitionResult, errorMessage);
				PopulateRecognizedRecordDetail(recognizedRecord, recognitionResult).Wait();
			}
		}

		private static string GetRecognitionErrorMessage(AggregateException exception, int? recognizedPageCount)
		{
			var fileHasManyPages = recognizedPageCount > MaxFilePageCount;
			var exceptionMessages = exception.Flatten().InnerExceptions
				.Select(i => fileHasManyPages &&
					i is RecognitionServiceUnexpectedResponseException || i is TaskCanceledException ?
					PXMessages.LocalizeNoPrefix(Messages.DocumentHasManyPagesFailedRecognition) :
					i.Message);

			return string.Join(Environment.NewLine, exceptionMessages);
		}

		private void RecognizeInvoiceDataInternal(Guid recognizedRecordRefNbr, Guid fileId, ILogger logger)
		{
			var document = (APRecognizedInvoice)Document.Cache.CreateInstance();
			document.RecognizedRecordRefNbr = recognizedRecordRefNbr;
			int maxrows = 1;
			int startrow = 0;
			var result = Document.View.Select(null, null, new object[] {recognizedRecordRefNbr},
				new string[] {nameof(APRecognizedInvoice.recognizedRecordRefNbr)}, new[] {false}, null, ref startrow, 1, ref maxrows).FirstOrDefault();
			if (result!=null)
				Document.Current = result as APRecognizedInvoice;
			var recognizedRecord = RecognizedRecords.SelectSingle();

			document.RecognitionStatus = recognizedRecord.Status;

			try
			{
				var file = GetFile(this, fileId);

				if (file == null)
				{
					throw new PXException(Messages.NoFileAttached);
				}

				if (file.Data == null || file.Data.Length == 0)
				{
					throw new PXException(Messages.FileIsCorrupted);
				}

				if (!IsAllowedFile(file.Name))
				{
					var message = PXMessages.LocalizeFormatNoPrefixNLA(Messages.InvalidFileForRecognition, PdfExtension);

					throw new PXArgumentException(nameof(file), message);
				}

				if (recognizedRecord.RecognitionStarted != true)
				{
					MarkRecognitionStarted(this, recognizedRecord, null);
				}

				if (recognizedRecord.Status == RecognizedRecordStatusListAttribute.Error)
				{
					SetNewCloudFileId(this, recognizedRecord);
				}

				DocumentRecognitionResult recognitionResult = null;
				string errorMessage = null;
				try
				{
					PXTrace.WriteInformation("Starting Recognition of the \"{0}\" file", file.Name);
					recognitionResult = GetRecognitionInfo(this, recognizedRecord, InvoiceRecognitionClient, file, logger,
						recognizedRecord.CloudFileId.Value).Result;
				}
				catch (AggregateException e)
				{
					errorMessage = GetRecognitionErrorMessage(e, recognizedRecord.PageCount);
					throw;
				}
				finally
				{
					UpdateRecognizedRecord(this, recognizedRecord, recognitionResult, errorMessage);
					PopulateRecognizedRecordDetail(recognizedRecord, recognitionResult).Wait();
				}
            }
			catch (PXException e)
			{
				UpdateRecognizedRecord(this, recognizedRecord, null, e.Message);
				document.RecognitionStatus = RecognizedRecordStatusListAttribute.Error;

				throw;
			}
			catch
			{
				document.RecognitionStatus = RecognizedRecordStatusListAttribute.Error;

				throw;
			}
		}

		private async Task PopulateRecognizedRecordDetail(RecognizedRecord record, DocumentRecognitionResult recognitionResult)
		{
			if (record.Status != RecognizedRecordStatusListAttribute.Recognized)
			{
				return;
			}

			await DetailsPopulator.FillRecognizedFields(record, recognitionResult);
		}

		internal async Task PopulateVendorId(RecognizedRecord record, RecognizedRecordDetail detail)
		{
			if (record.Status != RecognizedRecordStatusListAttribute.Recognized)
			{
				return;
			}

			await DetailsPopulator.FillVendorId(record, detail);
		}

		internal static UploadFile GetFile(PXGraph graph, Guid fileId)
		{
			var result = (PXResult<UploadFile, UploadFileRevision>)
				PXSelectJoin<UploadFile,
				InnerJoin<UploadFileRevision,
				On<UploadFile.fileID, Equal<UploadFileRevision.fileID>, And<
				   UploadFile.lastRevisionID, Equal<UploadFileRevision.fileRevisionID>>>>,
				Where<UploadFile.fileID, Equal<Required<UploadFile.fileID>>>>.Select(graph, fileId);
			if (result == null)
			{
				return null;
			}

			var file = (UploadFile)result;
			var fileRevision = (UploadFileRevision)result;

			file.Data = fileRevision.Data;

			return file;
		}

		private static void SetNewCloudFileId(APInvoiceRecognitionEntry graph, RecognizedRecord record)
		{
			record.CloudFileId = Guid.NewGuid();

			graph.RecognizedRecords.Update(record);
			graph.Persist();
		}

		private static void MarkRecognitionStarted(APInvoiceRecognitionEntry graph, RecognizedRecord record, string url)
		{
			record.RecognitionStarted = true;
			record.Status = RecognizedRecordStatusListAttribute.InProgress;
            record.ResultUrl = url;

			graph.RecognizedRecords.Update(record);
			graph.Persist();
		}

        private static void UpdateRecognizedRecordUrl(APInvoiceRecognitionEntry graph, RecognizedRecord record, string url)
        {
            record.ResultUrl = url;
            record = graph.RecognizedRecords.Update(record);
            graph.RecognizedRecords.Cache.PersistUpdated(record);
            graph.RecognizedRecords.Cache.Persisted(false);
            graph.SelectTimeStamp();
        }

		private static void UpdateRecognizedRecord(APInvoiceRecognitionEntry graph, RecognizedRecord record,
			DocumentRecognitionResult recognitionResult, string errorMessage)
		{
			var isError = recognitionResult == null;

			record.RecognitionResult = JsonConvert.SerializeObject(recognitionResult);

			var recognizedPageCount = recognitionResult?.Pages?.Count;
			if (record.PageCount == null || (recognizedPageCount != null && recognizedPageCount != record.PageCount))
			{
				record.PageCount = recognizedPageCount ?? 0;
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				record.ErrorMessage = errorMessage;
				isError = true;
			}
			else if (recognizedPageCount == null || recognizedPageCount == 0)
			{
				record.ErrorMessage = Messages.RecognitionServiceEmptyResult;
				isError = true;
			}

			record.Status = isError ?
				RecognizedRecordStatusListAttribute.Error :
				RecognizedRecordStatusListAttribute.Recognized;

			if (record.Status == RecognizedRecordStatusListAttribute.Recognized)
			{
				record.ErrorMessage = null;
			}
			else
			{
				var errorHistoryCache = graph.Caches[typeof(RecognizedRecordErrorHistory)];
				var errorHistoryRow = errorHistoryCache.CreateInstance() as RecognizedRecordErrorHistory;

				errorHistoryRow.RefNbr = record.RefNbr;
				errorHistoryRow.EntityType = record.EntityType;
				errorHistoryRow.CloudFileId = record.CloudFileId;
				errorHistoryRow.ErrorMessage = record.ErrorMessage;

				errorHistoryRow = errorHistoryCache.Insert(errorHistoryRow) as RecognizedRecordErrorHistory;
				errorHistoryCache.PersistInserted(errorHistoryRow);
			}

			record = graph.RecognizedRecords.Update(record);
			graph.RecognizedRecords.Cache.PersistUpdated(record);
			graph.RecognizedRecords.Cache.Persisted(false);
			graph.SelectTimeStamp();
		}

		private static async Task<DocumentRecognitionResult> GetRecognitionInfo(APInvoiceRecognitionEntry graph,
			RecognizedRecord record, IInvoiceRecognitionService client,
			UploadFile file, ILogger logger, Guid cloudFileId)
		{
			var extension = Path.GetExtension(file.Name);
			var mimeType = MimeTypes.GetMimeType(extension);

			using (var op = logger.OperationAt(LogEventLevel.Verbose, LogEventLevel.Error)
				.Begin("Recognizing document"))
			{
				using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(_recognitionTimeoutMinutes)))
				{
					try
					{
						var response = await client.SendFile(cloudFileId, file.Data, mimeType, cancellationTokenSource.Token);
						UpdateRecognizedRecordUrl(graph, record, response.State);
						var result = await PollForResults(graph, record, client, response, logger, cancellationTokenSource.Token);
						op.Complete();
						return result;
					}
					catch (Exception e)
					{
						op.SetException(e);
						throw;
					}
				}
			}
		}

		private static async Task<DocumentRecognitionResult> PollForResults(APInvoiceRecognitionEntry graph,
			RecognizedRecord record, IInvoiceRecognitionService imageRecognitionWebClient, DocumentRecognitionResponse response,
			ILogger logger,	CancellationToken cancellationToken)
		{
			var (result, state) = response;
			if (result != null)
			{
				LogSyncResponse(logger);
				return result;
			}

			using (var op = logger.BeginOperationVerbose("Polling for recognition results"))
			{
				var attempts = 0;
				while (!cancellationToken.IsCancellationRequested)
				{
					if (state == null)
						throw new InvalidOperationException("Unexpected empty state in document recognition response");

					attempts++;
					(result, state) = await imageRecognitionWebClient.GetResult(state, cancellationToken);
					if (result != null)
					{
						op.Complete("Attempts", attempts);
						return result;
					}
					else if (record.ResultUrl != state)
					{
						UpdateRecognizedRecordUrl(graph, record, state);
					}

					await Task.Delay(RecognitionPollingInterval, cancellationToken);
				}

				op.EnrichWith("Attempts", attempts);

				//TODO: change to cancellationToken.ThrowIfCancellationRequested
				throw new PXException(Messages.WaitingTimeExceeded);
			}
		}

		private static void LogSyncResponse(ILogger logger) => logger.Verbose("Recognition returned result synchronously");

		private static void LoadRecognizedDataToGraph(APInvoiceRecognitionEntry graph, RecognizedRecord record,
			RecognizedRecordDetail detail, DocumentRecognitionResult recognitionResult, bool reloadVendorId)
		{
			var document = graph.Document.Current;
            var cache = graph.Document.Cache;
            cache.SetValue<APRecognizedInvoice.recognitionStatus>(document, record.Status);
            cache.SetValue<APRecognizedInvoice.duplicateLink>(document, record.DuplicateLink);
            cache.SetValue<APRecognizedInvoice.recognizedDataJson>(document, HttpUtility.UrlEncode(record.RecognitionResult));
            cache.SetValue<APRecognizedInvoice.noteID>(document, record.NoteID);
			cache.SetValue<APRecognizedInvoice.isDataLoaded>(document, true);

			if (recognitionResult == null)
			{
				return;
			}

			if (document.RecognitionStatus != RecognizedRecordStatusListAttribute.Processed)
			{
			// To avoid double calculation
			cache.SetValue<APRecognizedInvoice.curyLineTotal>(document, decimal.Zero);
			}

			var siteMapNode = PXSiteMap.Provider.FindSiteMapNodesByGraphType(typeof(APInvoiceRecognitionEntry).FullName).FirstOrDefault();
			if (siteMapNode == null)
			{
				return;
			}

			var (_, detailFields) = graph.GetUIFields();
			if (detailFields == null)
			{
				return;
			}

			if (document.RecognitionStatus != RecognizedRecordStatusListAttribute.Processed)
			{
				var vendorId = detail?.VendorID;
				var invoiceDataLoader = new InvoiceDataLoader(recognitionResult, graph, detailFields.ToArray(), vendorId, reloadVendorId);
				invoiceDataLoader.Load(document);

				cache.SetValue<APRecognizedInvoice.vendorTermIndex>(document, detail?.VendorTermIndex);
				cache.SetValue<APRecognizedInvoice.vendorName>(document, detail?.VendorName);
			}

			document.FeedbackBuilder = graph.GetFeedbackBuilder();
			document.Links = recognitionResult.Links;

			graph.Document.Cache.IsDirty = false;
			graph.Transactions.Cache.IsDirty = false;
		}

		private static async Task SendVendorSearchFeedbackAsync(Dictionary<string, Uri> links, IInvoiceRecognitionFeedback feedbackService,
			VendorSearchFeedback feedback)
		{
			if (!links.TryGetValue(FEEDBACK_VENDOR_SEARCH, out var link))
			{
				PXTrace.WriteError("IDocumentRecognitionClient: Unable to send feedback - link is not found:{LinkKey}", FEEDBACK_VENDOR_SEARCH);
				return;
			}

			var formatter = new JsonMediaTypeFormatter { SerializerSettings = VendorSearchFeedback.Settings };
			var content = new ObjectContent(feedback.GetType(), feedback, formatter);

			await feedbackService.Send(link, content);
		}

		private (IEnumerable<string> PrimaryFields, IEnumerable<string> DetailFields) GetUIFields()
		{
			var siteMapNode = PXSiteMap.Provider
				.FindSiteMapNodesByGraphType(typeof(APInvoiceRecognitionEntry).FullName)
				.FirstOrDefault();
			if (siteMapNode == null)
			{
				return (null, null);
			}

			if (PXContext.GetSlot<bool?>(_screenInfoLoad) == true)
			{
				return (null, null);
			}

			PXContext.SetSlot(_screenInfoLoad, true);
			PXSiteMap.ScreenInfo screenInfo;
			try
			{
				screenInfo = ScreenInfoProvider.TryGet(siteMapNode.ScreenID);
			}
			finally
			{
				PXContext.ClearSlot(_screenInfoLoad);
			}

			if (screenInfo == null)
			{
				return (null, null);
			}

			if (!screenInfo.Containers.TryGetValue(nameof(Document), out var primaryContainer) ||
				!screenInfo.Containers.TryGetValue(nameof(Transactions), out var detailContainer))
			{
				return (null, null);
			}

			var primaryFields = primaryContainer.Fields.Select(f => f.FieldName);
			var detailFields = detailContainer.Fields.Select(f => f.FieldName);

			return (primaryFields, detailFields);
		}

		private DocumentFeedbackBuilder GetFeedbackBuilder()
		{
			var (primaryFields, detailFields) = GetUIFields();
			if (primaryFields == null || detailFields == null)
			{
				return null;
			}

			return new DocumentFeedbackBuilder(Document.Cache, primaryFields.ToHashSet(), detailFields.ToHashSet());
		}

		internal static Task RecognizeRecordsBatch(IEnumerable<RecognizedRecordFileInfo> batch, CancellationToken cancellationToken = default)
		{
			return RecognizeRecordsBatch(batch, subject: null, mailFrom: null, messageId: null, ownerId: null, newFiles: false, cancellationToken);
		}

		internal static async Task RecognizeRecordsBatch(IEnumerable<RecognizedRecordFileInfo> batch, string subject = null,
			string mailFrom = null, string messageId = null, int? ownerId = null, bool newFiles = false,
			CancellationToken externalCancellationToken = default)
		{
            var recognitionGraph = CreateInstance<APInvoiceRecognitionEntry>();
			var listToProcess = await StartFilesRecogntion(recognitionGraph, batch, subject, mailFrom, messageId, ownerId, newFiles);
			await ProcessStartedFiles(recognitionGraph, listToProcess, externalCancellationToken);

			var anyFailedFile = false;
			var failedToProcess = listToProcess.Where(r => r.Status == RecognizedRecordStatusListAttribute.Error);
			foreach (var failed in failedToProcess)
			{
				anyFailedFile = true;

				PXProcessing.SetCurrentItem(failed);
				PXProcessing.SetError(failed.ErrorMessage);
			}

			if (anyFailedFile)
			{
				throw new PXException(Messages.FailedFilesToProcess);
			}
        }

		private static async Task<List<RecognizedRecord>> StartFilesRecogntion(APInvoiceRecognitionEntry recognitionGraph,
			IEnumerable<RecognizedRecordFileInfo> batch, string subject, string mailFrom, string messageId,
			int? ownerId, bool newFiles)
		{
			var listToProcess = new List<RecognizedRecord>();

			foreach (var item in batch)
			{
				string fileErrorMessage = null;
				string fileName = null;

				if (item.FileId == Guid.Empty)
				{
					fileErrorMessage = Messages.NoFileAttached;
				}
				else if (string.IsNullOrWhiteSpace(item.FileName) || item.FileData == null || item.FileData.Length == 0)
				{
					fileErrorMessage = Messages.FileIsCorrupted;
				}
				else if (!IsAllowedFile(fileName = PX.SM.FileInfo.GetShortName(item.FileName)))
				{
					fileErrorMessage = PXMessages.LocalizeNoPrefix(Messages.OnlyPdfFilesAreSupported);
				}
				else if (newFiles)
				{
					var fileInfoDb = new PX.SM.FileInfo(item.FileId, item.FileName, null, item.FileData);

					var uploadFileGraph = CreateInstance<UploadFileMaintenance>();
					if (!uploadFileGraph.SaveFile(fileInfoDb))
					{
						fileErrorMessage = PXMessages.LocalizeFormatNoPrefixNLA(Messages.FileCannotBeSaved, item.FileName);
					}
				}

				var recognizedRecord = item.RecognizedRecord;

				if (recognizedRecord == null)
				{
					var recognizedSubject = GetRecognizedSubject(subject, fileName);
					
					if (!string.IsNullOrWhiteSpace(messageId))
					{
						messageId = NormalizeMessageId(messageId);
					}

					recognizedRecord = recognitionGraph.CreateRecognizedRecord(fileName, item.FileData, item.FileId, recognizedSubject, mailFrom,
						messageId, ownerId);

					if (!string.IsNullOrEmpty(fileErrorMessage))
					{
						UpdateRecognizedRecord(recognitionGraph, recognizedRecord, null, fileErrorMessage);

						PXProcessing.SetCurrentItem(recognizedRecord);
						PXProcessing.SetError(recognizedRecord.ErrorMessage);

						continue;
					}

					PXNoteAttribute.ForcePassThrow<RecognizedRecord.noteID>(recognitionGraph.RecognizedRecords.Cache);
					PXNoteAttribute.SetFileNotes(recognitionGraph.RecognizedRecords.Cache, recognizedRecord, item.FileId);
				}
				else if (recognizedRecord.Status == RecognizedRecordStatusListAttribute.Error)
				{
					SetNewCloudFileId(recognitionGraph, recognizedRecord);
				}

				if (!IsAllowedFile(fileName))
				{
					var errorMessage = PXMessages.LocalizeNoPrefix(Messages.OnlyPdfFilesAreSupported);
					UpdateRecognizedRecord(recognitionGraph, recognizedRecord, null, errorMessage);

					PXProcessing.SetCurrentItem(recognizedRecord);
					PXProcessing.SetError(recognizedRecord.ErrorMessage);

					continue;
				}

				var extension = Path.GetExtension(fileName);
				var mimeType = MimeTypes.GetMimeType(extension);

				DocumentRecognitionResult result = null;
				string state = null;
				try
				{
					//TODO: add more reasonable cancellation
					(result, state) = await recognitionGraph.InvoiceRecognitionClient.SendFile(item.FileId, item.FileData, mimeType, CancellationToken.None);
				}
				catch (Exception e)
				{
					UpdateRecognizedRecord(recognitionGraph, recognizedRecord, null, e.Message);

					PXProcessing.SetCurrentItem(recognizedRecord);
					PXProcessing.SetError(recognizedRecord.ErrorMessage);

					continue;
				}

				MarkRecognitionStarted(recognitionGraph, recognizedRecord, state);
				if (result != null)
				{
					UpdateRecognizedRecord(recognitionGraph, recognizedRecord, result, null);
				
					PXProcessing.SetCurrentItem(recognizedRecord);
					PXProcessing.SetProcessed();
				}
				else
					listToProcess.Add(recognizedRecord);
			}

			return listToProcess;
		}

		private static async Task ProcessStartedFiles(APInvoiceRecognitionEntry recognitionGraph, List<RecognizedRecord> listToProcess,
			CancellationToken externalCancellationToken)
		{
			var filesInProgress = listToProcess.Where(r => r.Status == RecognizedRecordStatusListAttribute.InProgress);
			var filesToProcessCount = filesInProgress.Count();

			using (var timedCts = new CancellationTokenSource(TimeSpan.FromMinutes(_recognitionTimeoutMinutes)))
			using (var joinedCts = CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken, timedCts.Token))
			{
				var cancellationToken = joinedCts.Token;
				using (var op = recognitionGraph._logger.BeginOperationVerbose("Polling for recognition results"))
				{
					var attempts = 0;

					try
					{
						var processedFilesCount = 0;

						while (!cancellationToken.IsCancellationRequested && processedFilesCount < filesToProcessCount)
						{
							attempts++;

							foreach (var recognizedRecord in filesInProgress)
							{
								DocumentRecognitionResult result = null;
								string state = null;

								try
								{
									(result, state) = await recognitionGraph.InvoiceRecognitionClient.GetResult(recognizedRecord.ResultUrl, cancellationToken);
								}
								catch (Exception e)
								{
									var fileHasManyPages = recognizedRecord.PageCount > MaxFilePageCount;
									var message = fileHasManyPages && e is RecognitionServiceUnexpectedResponseException ?
										PXMessages.LocalizeNoPrefix(Messages.DocumentHasManyPagesFailedRecognition) :
										e.Message;

									UpdateRecognizedRecord(recognitionGraph, recognizedRecord, null, message);
									PXProcessing.SetCurrentItem(recognizedRecord);
									PXProcessing.SetError(e);
								}

								if (recognizedRecord.Status == RecognizedRecordStatusListAttribute.Error)
								{
									processedFilesCount++;
								}
								else
								{
									if (result != null)
									{
										op.Complete("Attempts", attempts);
										UpdateRecognizedRecord(recognitionGraph, recognizedRecord, result, null);
										await recognitionGraph.PopulateRecognizedRecordDetail(recognizedRecord, result);
										processedFilesCount++;

										PXProcessing.SetCurrentItem(recognizedRecord);
										PXProcessing.SetProcessed();
									}
									else if (state != recognizedRecord.ResultUrl)
									{
										UpdateRecognizedRecordUrl(recognitionGraph, recognizedRecord, state);
									}
								}
							}

							if (processedFilesCount < filesToProcessCount)
							{
								await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
							}
						}

						if (processedFilesCount < filesToProcessCount)
						{
							op.EnrichWith("Attempts", attempts);

							//TODO: change to cancellationToken.ThrowIfCancellationRequested
							throw new PXException(Messages.WaitingTimeExceeded);
						}
					}
					catch (Exception e)
					{
						foreach (var recognizedRecord in filesInProgress)
						{
							UpdateRecognizedRecord(recognitionGraph, recognizedRecord, null, e.Message);
						}
					}
				}
			}
		}

		internal static bool IsRecognitionInProgress(string messageId)
		{
			messageId.ThrowOnNullOrWhiteSpace(nameof(messageId));
			messageId = NormalizeMessageId(messageId);

			using (var record = PXDatabase.SelectSingle<RecognizedRecord>(
				new PXDataField<RecognizedRecord.refNbr>(),
				new PXDataFieldValue<RecognizedRecord.messageID>(messageId),
				new PXDataFieldValue<RecognizedRecord.status>(RecognizedRecordStatusListAttribute.InProgress)))
			{
				return record != null;
			}
		}

		internal static UploadFile[] GetFilesToRecognize(PXCache cache, object row)
		{
			var fileNotes = PXNoteAttribute.GetFileNotes(cache, row);
			if (fileNotes == null)
			{
				return null;
			}

			return fileNotes
				.Select(n => GetFile(cache.Graph, n))
				.Where(file => file?.Name != null && IsAllowedFile(file.Name))
				.ToArray();
		}

		public virtual IList<(int? InventoryId, string PONumber)> GetPONumbers(int? vendorId, HashSet<int?> inventoryIds)
		{
			var poNumbers = new List<(int? InventoryId, string PONumber)>();

			var openPOLines = SelectFrom<POLine>.Where<POLine.vendorID.IsEqual<@P.AsInt>.
				And<POLine.cancelled.IsEqual<False>>.
				And<POLine.closed.IsEqual<False>>.
				And<Brackets<POLine.curyUnbilledAmt.IsNotEqual<decimal0>>.Or<POLine.unbilledQty.IsNotEqual<decimal0>>>>.View.Select(this, vendorId);

			foreach (POLine line in openPOLines)
			{
				if (inventoryIds.Contains(line.InventoryID))
				{
					poNumbers.Add((line.InventoryID, line.OrderNbr));
				}
			}

			return poNumbers;
		}
	}
}
