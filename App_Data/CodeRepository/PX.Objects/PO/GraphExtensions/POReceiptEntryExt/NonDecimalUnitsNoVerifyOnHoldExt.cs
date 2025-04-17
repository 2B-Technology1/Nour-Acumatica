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
using PX.Objects.Extensions;
using System.Collections.Generic;

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	public class NonDecimalUnitsNoVerifyOnHoldExt : NonDecimalUnitsNoVerifyOnHoldExt<POReceiptEntry, POReceipt, POReceiptLine, POReceiptLine.receiptQty, POReceiptLineSplit, POReceiptLineSplit.qty>
	{
		private NonDecimalUnitsNoVerifyOnDropShipExt _nonDecimalUnitsNoVerifyOnDropShipExt;

		protected NonDecimalUnitsNoVerifyOnDropShipExt NonDecimalUnitsNoVerifyOnDropShipExt
			=> _nonDecimalUnitsNoVerifyOnDropShipExt = _nonDecimalUnitsNoVerifyOnDropShipExt ?? Base.FindImplementation<NonDecimalUnitsNoVerifyOnDropShipExt>();

		public override bool HaveHoldStatus(POReceipt doc) => doc.Hold == true;
		public override int? GetLineNbr(POReceiptLine line) => line.LineNbr;
		public override int? GetLineNbr(POReceiptLineSplit split) => split.LineNbr;
		public override IEnumerable<POReceiptLine> GetLines() => Base.transactions.Select().RowCast<POReceiptLine>();
		protected override POReceiptLine LocateLine(POReceiptLineSplit split) =>
			(POReceiptLine)Base.transactions.Cache.Locate(new POReceiptLine
			{
				ReceiptType = split.ReceiptType,
				ReceiptNbr = split.ReceiptNbr,
				LineNbr = split.LineNbr
			});

		public override IEnumerable<POReceiptLineSplit> GetSplits()
			=> PXSelect<POReceiptLineSplit, Where<POReceiptLineSplit.FK.Receipt.SameAsCurrent>>.Select(Base).RowCast<POReceiptLineSplit>();

		protected override void VerifyLine(PXCache lineCache, POReceiptLine line)
		{
			NonDecimalUnitsNoVerifyOnDropShipExt.SetDecimalVerifyMode(lineCache, line);
			base.VerifyLine(lineCache, line);
		}
	}
}
