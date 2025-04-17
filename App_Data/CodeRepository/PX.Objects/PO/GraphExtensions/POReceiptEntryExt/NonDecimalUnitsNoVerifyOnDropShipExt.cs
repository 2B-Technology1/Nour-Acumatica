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
using PX.Objects.IN;
using System.Linq;

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	/// <summary>
	/// Disabling of validation for decimal values for drop ship lines in PO Receipt
	/// </summary>
	public class NonDecimalUnitsNoVerifyOnDropShipExt: NonDecimalUnitsNoVerifyOnDropShipExt<POReceiptEntry, POReceiptLine>
	{
		protected override bool IsDropShipLine(POReceiptLine line) => POLineType.IsDropShip(line.LineType);

		protected override DecimalVerifyMode GetLineVerifyMode(PXCache cache, POReceiptLine line)
		{
			if (!IsDropShipLine(line))
			{
				var cacheAttribute = cache.GetAttributesOfType<PXDBQuantityAttribute>(null, nameof(POReceiptLine.receiptQty)).FirstOrDefault();
				if (cacheAttribute != null)
					return cacheAttribute.DecimalVerifyMode;
			}
			return DecimalVerifyMode.Off;
		}
	}
}
