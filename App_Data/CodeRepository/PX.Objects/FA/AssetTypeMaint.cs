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

namespace PX.Objects.FA
{
	public class AssetTypeMaint : PXGraph<AssetTypeMaint>
	{
		public PXSavePerRow<FAType, FAType.assetTypeID> Save;
		public PXCancel<FAType> Cancel;
		
		#region Selects Declaration

		public PXSelect<FAType> AssetTypes;

		#endregion

		#region Ctor
		
		public AssetTypeMaint()
		{
			AssetTypes.Cache.AllowInsert = true;
			AssetTypes.Cache.AllowUpdate = true;
			AssetTypes.Cache.AllowDelete = true;
		}

		#endregion

		#region Events

		protected virtual void FAType_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			FAType row = (FAType)e.Row;
			if (row != null && IsUsed(row.AssetTypeID))
			{
				throw new PXSetPropertyException(Messages.FATypeDeleteUsed, PXErrorLevel.RowError);
			}
		}

		protected virtual void FAType_IsTangible_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FAType row = (FAType)e.Row;
			if (row != null && IsUsed(row.AssetTypeID))
			{
				sender.RaiseExceptionHandling<FAType.isTangible>(
					row,
					e.NewValue, 
					new PXSetPropertyException(Messages.FATypeChangeUsed, PXErrorLevel.RowWarning));
			}
		}

		protected virtual void FAType_Depreciable_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FAType row = (FAType)e.Row;
			if (row != null && IsUsed(row.AssetTypeID))
			{
				sender.RaiseExceptionHandling<FAType.depreciable>(
					row,
					e.NewValue, 
					new PXSetPropertyException(Messages.FATypeChangeUsed, PXErrorLevel.RowWarning));
			}
		}

		protected virtual void FAType_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FAType row = (FAType)e.Row;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<FAType.assetTypeID>(sender, row, sender.GetStatus(row) == PXEntryStatus.Inserted);
		}

		#endregion

		private bool IsUsed(string assetTypeID)
		{
			return 
				assetTypeID != null &&
				PXSelect<FixedAsset, Where<FixedAsset.assetTypeID, IsNotNull, 
					And<FixedAsset.assetTypeID, Equal<Required<FixedAsset.assetTypeID>>>>>.SelectSingleBound(this, null, assetTypeID).Count > 0;
		}
	}
}
