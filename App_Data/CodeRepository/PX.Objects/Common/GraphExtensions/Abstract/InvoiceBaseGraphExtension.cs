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
using PX.Objects.CM.Extensions;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.CR;

namespace PX.Objects.Common.GraphExtensions.Abstract
{
	public abstract class InvoiceBaseGraphExtension<TGraph, TInvoice, TInvoiceMapping> : DocumentWithLinesGraphExtension<TGraph, TInvoice, TInvoiceMapping>
		where TGraph : PXGraph
		where TInvoice : InvoiceBase, new()
		where TInvoiceMapping : IBqlMapping
	{
		public PXSelectExtension<Contragent> Contragent;

		protected virtual ContragentMapping GetContragentMapping()
		{
			return new ContragentMapping(typeof(Stub));
		}

		public abstract void SuppressApproval();

		public virtual PXSelectBase<Location> Location { get; }

		public virtual PXSelectBase<CurrencyInfo> CurrencyInfo { get; }

		public PXSelectExtension<InvoiceTran> InvoiceTrans;

		protected virtual InvoiceTranMapping GetInvoiceTranMapping()
		{
			return new InvoiceTranMapping(typeof(Stub));
		}

		public PXSelectExtension<GenericTaxTran> TaxTrans;

		protected virtual GenericTaxTranMapping GetGenericTaxTranMapping()
		{
			return new GenericTaxTranMapping(typeof(Stub));
		}

		public PXSelectExtension<LineTax> LineTaxes;

		protected virtual LineTaxMapping GetLineTaxMapping()
		{
			return new LineTaxMapping(typeof(Stub));
		}

		#region Lines

		protected override bool ShouldUpdateDetailsOnDocumentUpdated(Events.RowUpdated<TInvoice> e)
		{
			return base.ShouldUpdateDetailsOnDocumentUpdated(e)
				   || !e.Cache.ObjectsEqual<Invoice.headerDocDate, Invoice.curyID>(e.Row, e.OldRow);
		}

		protected override void ProcessLineOnDocumentUpdated(Events.RowUpdated<TInvoice> e, DocumentLine line)
		{
			base.ProcessLineOnDocumentUpdated(e, line);

			if (!e.Cache.ObjectsEqual<Invoice.headerDocDate, Invoice.curyID>(e.Row, e.OldRow))
			{
				Lines.Cache.MarkUpdated(line);
			}
		}

		#endregion

		[PXHidden]
		protected class Stub : IBqlTable { }
	}
}