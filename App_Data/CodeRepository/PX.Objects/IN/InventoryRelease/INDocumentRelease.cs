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
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.AP;
using PX.Objects.IN.InventoryRelease;

namespace PX.Objects.IN
{
	[TableAndChartDashboardType]
	public class INDocumentRelease : PXGraph<INDocumentRelease>
	{
		public PXCancel<INRegister> Cancel;
		public PXAction<INRegister> viewDocument;

		[PXFilterable]
		public PXProcessing<INRegister, Where<INRegister.released, Equal<boolFalse>, And<INRegister.hold, Equal<boolFalse>>>> INDocumentList;
		public PXSetup<INSetup> insetup;

		public INDocumentRelease()
		{
			INSetup record = insetup.Current;
			INDocumentList.SetProcessDelegate((List<INRegister> list) => ReleaseDoc(list, true));
			INDocumentList.SetProcessCaption(Messages.Release);
			INDocumentList.SetProcessAllCaption(Messages.ReleaseAll);
		}

		public static void ReleaseDoc(List<INRegister> list, bool isMassProcess,
			bool releaseFromHold = false,
			PXQuickProcess.ActionFlow processFlow  = PXQuickProcess.ActionFlow.NoFlow)
		{
			bool failed = false;

			INReleaseProcess rg = PXGraph.CreateInstance<INReleaseProcess>();
			JournalEntry je = rg.CreateJournalEntry();
			var pg = new Lazy<PostGraph>(() => rg.CreatePostGraph());
			Dictionary<int, int> batchbind = new Dictionary<int, int>();
			var releasedDocs = new List<INRegister>();

			for (int i = 0; i < list.Count; i++)
			{
				INRegister doc = list[i];
				try
				{
					rg.Clear();

					rg.ReleaseDocProcR(je, doc, releaseFromHold);
					int k;
					if ((k = je.created.IndexOf(je.BatchModule.Current)) >= 0 && batchbind.ContainsKey(k) == false)
					{
						batchbind.Add(k, i);
					}

					if (isMassProcess)
					{
						PXProcessing<INRegister>.SetInfo(i, ActionsMessages.RecordProcessed);
					}

					releasedDocs.Add(doc);
				}
				catch (Exception e)
				{
					je.Clear();
					if (isMassProcess)
					{
						PXProcessing<INRegister>.SetError(i, e);
						failed = true;
					}
					else if (list.Count == 1)
					{
						throw new PXOperationCompletedSingleErrorException(e);
					}
					else
					{
						PXTrace.WriteError(e);
						failed = true;
					}
				}
			}

			for (int i = 0; i < je.created.Count; i++)
			{
				Batch batch = je.created[i];
				try
				{
					if (rg.AutoPost)
					{
						pg.Value.Clear();
						pg.Value.PostBatchProc(batch);
					}
				}
				catch (Exception e)
				{
					if (isMassProcess)
					{
						failed = true;
						PXProcessing<INRegister>.SetError(batchbind[i], e);
					}
					else if (list.Count == 1)
					{
						throw new PXMassProcessException(batchbind[i], e);
					}
					else
					{
						PXTrace.WriteError(e);
						failed = true;
					}
				}
			}

			if (failed)
			{
				throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
			}
			else if (releasedDocs.Count == 1 && processFlow != PXQuickProcess.ActionFlow.NoFlow)
			{
				RedirectTo(releasedDocs[0]);
			}
		}

		// TODO: MBC: Remove this method, DisplayName in INRegister is the same.
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Currency", Enabled = false, FieldClass = nameof(FeaturesSet.MultipleBaseCurrencies))]
		protected virtual void _(Events.CacheAttached<INRegister.branchBaseCuryID> eventArgs) { }

		[PXUIField(DisplayName = "")]
		[PXEditDetailButton]
		protected virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (this.INDocumentList.Current != null)
			{
				INRegister r = PXCache<INRegister>.CreateCopy(this.INDocumentList.Current);
				RedirectTo(r, PXBaseRedirectException.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public static void RedirectTo(INRegister doc, PXBaseRedirectException.WindowMode windowMode = PXBaseRedirectException.WindowMode.Same)
		{
			switch (doc.DocType)
			{
				case INDocType.Issue:
				{
					INIssueEntry graph = PXGraph.CreateInstance<INIssueEntry>();
					graph.issue.Current = graph.issue.Search<INRegister.refNbr>(doc.RefNbr, doc.DocType);
					throw new PXRedirectRequiredException(graph, "IN Issue") {Mode = windowMode};
				}
				case INDocType.Receipt:
				{
					INReceiptEntry graph = PXGraph.CreateInstance<INReceiptEntry>();
					graph.receipt.Current = graph.receipt.Search<INRegister.refNbr>(doc.RefNbr, doc.DocType);
					throw new PXRedirectRequiredException(graph, "IN Receipt") {Mode = windowMode};
				}
				case INDocType.Transfer:
				{
					INTransferEntry graph = PXGraph.CreateInstance<INTransferEntry>();
					graph.transfer.Current = graph.transfer.Search<INRegister.refNbr>(doc.RefNbr, doc.DocType);
					throw new PXRedirectRequiredException(graph, "IN Transfer") {Mode = windowMode};
				}
				case INDocType.Adjustment:
				{
					INAdjustmentEntry graph = PXGraph.CreateInstance<INAdjustmentEntry>();
					graph.adjustment.Current = graph.adjustment.Search<INRegister.refNbr>(doc.RefNbr, doc.DocType);
					throw new PXRedirectRequiredException(graph, "IN Adjustment") {Mode = windowMode};
				}
				case INDocType.Production:
				case INDocType.Disassembly:
				{
					KitAssemblyEntry graph = PXGraph.CreateInstance<KitAssemblyEntry>();
					graph.Document.Current = graph.Document.Search<INKitRegister.refNbr>(doc.RefNbr, doc.DocType);
					throw new PXRedirectRequiredException(graph, "IN Kit Assembly") {Mode = windowMode};
				}
				default:
					throw new PXException(Messages.UnknownDocumentType);
			}
		}
	}
}
