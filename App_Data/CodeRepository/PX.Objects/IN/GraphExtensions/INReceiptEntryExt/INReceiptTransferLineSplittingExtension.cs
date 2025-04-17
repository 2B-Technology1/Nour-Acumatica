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

namespace PX.Objects.IN.GraphExtensions.INReceiptEntryExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PXProtectedAccess(typeof(INReceiptEntry.LineSplittingExtension))]
	public abstract class INReceiptTransferLineSplittingExtension
		: TransferLineSplittingExtension<INReceiptEntry, INReceiptEntry.LineSplittingExtension, INRegister, INTran, INTranSplit>
	{
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(INLotSerialNbrAttribute))]
		[TransferLotSerialNbr(typeof(INTran.inventoryID), typeof(INTran.subItemID), typeof(INTran.locationID), typeof(INTran.costCenterID), typeof(INTran.tranType),
			typeof(INTran.origRefNbr), typeof(INTran.origLineNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<INTran.lotSerialNbr> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(INLotSerialNbrAttribute))]
		[TransferLotSerialNbr(typeof(INTranSplit.inventoryID), typeof(INTranSplit.subItemID), typeof(INTranSplit.locationID), typeof(INTran.costCenterID),
			typeof(INTranSplit.tranType), typeof(INTran.origRefNbr), typeof(INTran.origLineNbr), typeof(INTran.lotSerialNbr))]
		protected virtual void _(Events.CacheAttached<INTranSplit.lotSerialNbr> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXDBQuantityAttribute), nameof(PXDBQuantityAttribute.MinValue), .0)]
		protected virtual void _(Events.CacheAttached<INTranSplit.qty> e)
		{
		}

		protected virtual void _(Events.RowSelected<INRegister> e)
		{
			if (e.Row == null) return;

			bool isTransferReceipt = e.Row.TransferNbr != null;

			LineCache.Adjust<INExpireDateAttribute>()
				.For<INTran.expireDate>(a => a.ForceDisable = isTransferReceipt);
			SplitCache.Adjust<INExpireDateAttribute>()
				.For<INTranSplit.expireDate>(a => a.ForceDisable = isTransferReceipt);
		}
	}
}
