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

namespace PX.Objects.SO.GraphExtensions.SOShipmentEntryExt
{
	public class NonDecimalUnitsNoVerifyOnHoldExt : NonDecimalUnitsNoVerifyOnHoldExt<SOShipmentEntry, SOShipment, SOShipLine, SOShipLine.shippedQty, SOShipLineSplit, SOShipLineSplit.qty>
	{
		public override bool HaveHoldStatus(SOShipment doc) => doc.Hold == true;
		public override int? GetLineNbr(SOShipLine line) => line.LineNbr;
		public override int? GetLineNbr(SOShipLineSplit split) => split.LineNbr;

		public override IEnumerable<SOShipLine> GetLines() => Base.Transactions.Select().RowCast<SOShipLine>();

		public override IEnumerable<SOShipLineSplit> GetSplits()
			=> PXSelect<SOShipLineSplit, Where<SOShipLineSplit.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>>.Select(Base).RowCast<SOShipLineSplit>();

		protected override SOShipLine LocateLine(SOShipLineSplit split) =>
			(SOShipLine)Base.Transactions.Cache.Locate(new SOShipLine
			{
				ShipmentNbr = split.ShipmentNbr,
				ShipmentType = Base.CurrentDocument.Current.ShipmentType,
				LineNbr = split.LineNbr
			});
	}
}
