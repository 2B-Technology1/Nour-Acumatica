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

using PX.Objects.IN;
using PX.Objects.IN.GraphExtensions;

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PXProtectedAccess(typeof(POReceiptLineSplittingExtension))]
	public abstract class POReceiptTransferLineSplittingExtension
		: TransferLineSplittingExtension<POReceiptEntry, POReceiptLineSplittingExtension, POReceipt, POReceiptLine, POReceiptLineSplit>
	{
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(POLotSerialNbrAttribute))]
		[POTransferLotSerialNbr(typeof(POReceiptLine.inventoryID), typeof(POReceiptLine.subItemID), typeof(POReceiptLine.locationID), typeof(POReceiptLine.costCenterID),
			typeof(POReceiptLine.tranType), typeof(POReceiptLine.origRefNbr), typeof(POReceiptLine.origLineNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<POReceiptLine.lotSerialNbr> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(POLotSerialNbrAttribute))]
		[POTransferLotSerialNbr(typeof(POReceiptLineSplit.inventoryID), typeof(POReceiptLineSplit.subItemID), typeof(POReceiptLineSplit.locationID), typeof(POReceiptLine.costCenterID),
			typeof(POReceiptLineSplit.tranType), typeof(POReceiptLine.origRefNbr), typeof(POReceiptLine.origLineNbr), typeof(POReceiptLine.lotSerialNbr))]
		protected virtual void _(Events.CacheAttached<POReceiptLineSplit.lotSerialNbr> e)
		{
		}

		protected virtual void _(Events.RowSelected<POReceipt> e)
		{
			if (e.Row == null) return;

			bool isTransferReceipt = e.Row.ReceiptType == POReceiptType.TransferReceipt;

			LineCache.Adjust<INExpireDateAttribute>()
				.For<POReceiptLine.expireDate>(a => a.ForceDisable = isTransferReceipt);
			SplitCache.Adjust<INExpireDateAttribute>()
				.For<POReceiptLineSplit.expireDate>(a => a.ForceDisable = isTransferReceipt);
		}

		protected override void VerifyUnassignedQty(POReceiptLine line) { }
	}
}
