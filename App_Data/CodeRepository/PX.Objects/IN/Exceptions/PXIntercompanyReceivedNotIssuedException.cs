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
using System.Runtime.Serialization;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;

namespace PX.Objects.IN
{
	public class PXIntercompanyReceivedNotIssuedException : PXException
	{
		public PXIntercompanyReceivedNotIssuedException(PXCache cache, IBqlTable row)
			: base(Messages.IntercompanyReceivedNotIssued,
				PXForeignSelectorAttribute.GetValueExt<ItemLotSerial.lotSerialNbr>(cache, row),
				PXForeignSelectorAttribute.GetValueExt<ItemLotSerial.inventoryID>(cache, row))
		{
		}

		public PXIntercompanyReceivedNotIssuedException(Exception exc, string message, string receiptNbr)
			: base(exc, message, receiptNbr)
		{
		}

		public PXIntercompanyReceivedNotIssuedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
