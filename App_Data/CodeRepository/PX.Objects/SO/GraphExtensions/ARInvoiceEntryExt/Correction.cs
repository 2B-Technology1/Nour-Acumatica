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
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.Common;
using System;
using System.Collections;
using ARRegisterAlias = PX.Objects.AR.Standalone.ARRegisterAlias;

namespace PX.Objects.SO.GraphExtensions.ARInvoiceEntryExt
{
	public class Correction : ARAdjustCorrectionExtension<ARInvoiceEntry, ARInvoice.isCancellation>
	{
		public PXAction<ARInvoice> ViewCorrectionDocument;

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		protected virtual IEnumerable viewCorrectionDocument(PXAdapter adapter)
		{
			RedirectionToOrigDoc.TryRedirect(Base.Document.Current.CorrectionDocType, Base.Document.Current.CorrectionRefNbr, GL.BatchModule.SO);
			return adapter.Get();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Correction Document", Enabled = false)]
		[PXUIVisible(typeof(Where<ARInvoice.isUnderCorrection, Equal<True>, And<ARInvoice.correctionRefNbr, IsNotNull>>))]
		protected virtual void _(Events.CacheAttached<ARInvoice.correctionRefNbr> e)
		{
		}

		protected virtual void _(Events.RowSelecting<ARInvoice> e)
		{
			if (e.Row?.IsUnderCorrection != true)
				return;
			PXView view = new SelectFrom<ARRegisterAlias>
				.Where<ARRegisterAlias.origDocType.IsEqual<ARInvoice.docType.FromCurrent>
					.And<ARRegisterAlias.origRefNbr.IsEqual<ARInvoice.refNbr.FromCurrent>
					.And<Where2<ARRegisterAlias.isCorrection.IsEqual<True>,
						Or<ARRegisterAlias.isCancellation.IsEqual<True>
							.And<ARRegisterAlias.released.IsNotEqual<True>>>>>>>.View.ReadOnly(Base).View;

			using (new PXFieldScope(view,
				typeof(ARRegisterAlias.docType),
				typeof(ARRegisterAlias.refNbr),
				typeof(ARRegisterAlias.isCancellation)))
			{
				// Acuminator disable once PX1042 DatabaseQueriesInRowSelecting [false positive]
				var correction = (ARRegisterAlias)view.SelectSingleBound(new[] { e.Row });
				e.Row.CorrectionDocType = correction?.DocType;
				e.Row.CorrectionRefNbr = correction?.RefNbr;
				e.Row.IsUnderCancellation = correction?.IsCancellation == true;
			}
		}

		protected virtual void _(Events.RowSelected<ARInvoice> e)
		{
			if (e.Row == null)
				return;

			e.Cache.RaiseExceptionHandling<ARInvoice.origRefNbr>(e.Row, e.Row.OrigRefNbr, IsCorrectionFromOriginal(e.Cache, e.Row)
				? new PXSetPropertyException<ARInvoice.origRefNbr>(AR.Messages.CurrentDocumentIsCorrection, PXErrorLevel.Warning, e.Row.OrigRefNbr)
				: null);

			e.Cache.RaiseExceptionHandling<ARInvoice.correctionRefNbr>(e.Row, e.Row.CorrectionRefNbr, IsOriginalOfCorrection(e.Cache, e.Row) 
				? new PXSetPropertyException<ARInvoice.correctionRefNbr>(AR.Messages.CurrentDocumentHasBeenCorrected, PXErrorLevel.Warning, e.Row.CorrectionRefNbr) 
				: null);
		}

		protected virtual bool IsCorrectionFromOriginal(PXCache cache, ARInvoice invoice)
		{
			return invoice?.OrigRefNbr != null && invoice.IsCorrection == true;
		}

		protected virtual bool IsOriginalOfCorrection(PXCache cache, ARInvoice invoice) => invoice?.CorrectionRefNbr != null && invoice.IsUnderCancellation != true;

		[PXOverride]
		public virtual ARRegister OnBeforeRelease(ARRegister doc, Func<ARRegister, ARRegister> baseMethod)
		{
			if ((doc.IsCancellation == true || doc.IsCorrection == true) && doc.OrigRefNbr == null)
			{
				throw new PXException(Messages.CorrectionDocumentMissingLink);
			}

			return baseMethod(doc);
		}

		[PXOverride]
		public virtual ARTran CreateReversalARTran(ARTran srcTran, ReverseInvoiceArgs reverseArgs, Func<ARTran, ReverseInvoiceArgs, ARTran> baseMethod)
		{
			var ret = baseMethod(srcTran, reverseArgs);
			if (ret == null)
				return null;

			if (Base.Document.Current?.OrigModule != GL.BatchModule.SO)
			{
				ret.SOOrderType = null;
				ret.SOOrderNbr = null;
				ret.SOOrderLineNbr = null;
				ret.SOOrderLineOperation = null;
				ret.SOOrderSortOrder = null;
				ret.SOOrderLineSign = null;
				ret.SOShipmentType = null;
				ret.SOShipmentNbr = null;
				ret.SOShipmentLineNbr = null;
			}

			return ret;
		}
	}
}
