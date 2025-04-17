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
using PX.Data;
using PX.Objects.SO;
using PX.Objects.Common.Discount.Mappers;
using PX.Objects.AP;

namespace PX.Objects.Common.Discount.Attributes
{
	public class PrepaymentDiscount : ManualDiscountMode
	{
		public PrepaymentDiscount(
			Type curyDiscAmt,
			Type curyTranAmt,
			Type discPct,
			Type freezeManualDisc,
			DiscountFeatureType discountType)
			:base(curyDiscAmt, curyTranAmt, discPct, freezeManualDisc, discountType)
		{
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if ((e.Row as APTran) == null || sender.GetValue<APTran.tranType>(e.Row) as string != APDocType.Prepayment)
			{
				base.RowUpdated(sender, e);
				return;
			}

			AmountLineFields lineAmountsFields = GetDiscountDocumentLine(sender, e.Row);
			if (lineAmountsFields.FreezeManualDisc == true)
			{
				lineAmountsFields.FreezeManualDisc = false;
				return;
			}

			DiscountLineFields lineDiscountFields = GetDiscountedLine(sender, e.Row);
			DiscountLineFields oldLineDiscountFields = GetDiscountedLine(sender, e.OldRow);

			if (lineDiscountFields.LineType == SOLineType.Discount)
				return;

			// Force auto mode.
			if (lineDiscountFields.ManualDisc == false && oldLineDiscountFields.ManualDisc == true)
			{
				sender.SetValueExt(e.Row, sender.GetField(typeof(DiscountLineFields.discPct)), 0m);
				sender.SetValueExt(e.Row, sender.GetField(typeof(DiscountLineFields.curyDiscAmt)), 0m);
				return;
			}

			if ((lineAmountsFields.CuryExtPrice ?? 0m) == 0)
			{
				sender.SetValueExt(e.Row, sender.GetField(typeof(DiscountLineFields.curyDiscAmt)), 0m);
				return;
			}

			LineEntitiesFields lineEntities = LineEntitiesFields.GetMapFor(e.Row, sender);
			AmountLineFields oldLineAmountsFields = GetDiscountDocumentLine(sender, e.OldRow);

			bool discountIsUpdated = false;
			if (lineDiscountFields.CuryDiscAmt != oldLineDiscountFields.CuryDiscAmt)
			{
				if (Math.Abs(lineDiscountFields.CuryDiscAmt ?? 0m) > Math.Abs(lineAmountsFields.CuryExtPrice.Value))
				{
					sender.SetValueExt(e.Row, sender.GetField(typeof(DiscountLineFields.curyDiscAmt)), lineAmountsFields.CuryExtPrice);
					PXUIFieldAttribute.SetWarning<DiscountLineFields.curyDiscAmt>(sender, e.Row,
						PXMessages.LocalizeFormatNoPrefix(AR.Messages.LineDiscountAmtMayNotBeGreaterExtPrice, lineAmountsFields.ExtPriceDisplayName));
				}

				decimal? discPct = CalcDiscountPercent(lineAmountsFields, lineDiscountFields);

				sender.SetValueExt(e.Row, sender.GetField(typeof(DiscountLineFields.discPct)), discPct);
				discountIsUpdated = true;
			}
			else if (lineDiscountFields.DiscPct != oldLineDiscountFields.DiscPct
				|| oldLineAmountsFields.CuryExtPrice != lineAmountsFields.CuryExtPrice)
			{
				decimal discAmt = CalcDiscountAmount(sender, GetLineDiscountTarget(sender, lineEntities),
					lineAmountsFields, lineDiscountFields);

				sender.SetValueExt(e.Row, sender.GetField(typeof(DiscountLineFields.curyDiscAmt)), discAmt);
				discountIsUpdated = true;
			}

			if (discountIsUpdated || sender.Graph.IsCopyPasteContext)
			{
				sender.SetValue(e.Row, this.FieldName, true); // Switch to manual mode.
			}
		}
	}
}
