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

using PX.Commerce.Core;
using PX.Data;
using PX.Objects.IN;
using System;

namespace PX.Commerce.Objects
{
	public class BCInventoryItemMaintExt : PXGraphExtension<InventoryItemMaint>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		public PXSelect<BCInventoryFileUrls, Where<BCInventoryFileUrls.inventoryID, Equal<Current<InventoryItem.inventoryID>>>> InventoryFileUrls;

		#region Cache Attached
		//Sync Time
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PX.Commerce.Core.BCSyncExactTime()]
		public void InventoryItem_LastModifiedDateTime_CacheAttached(PXCache sender) { }
		#endregion

		#region Event handlers
		protected virtual void BCInventoryFileUrls_FileURL_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = (BCInventoryFileUrls)e.Row;
			if (row == null) return;

			string val = e.NewValue?.ToString();
			foreach (BCInventoryFileUrls item in InventoryFileUrls.Select())
			{
				if (item.FileURL == val)
				{
					throw new PXSetPropertyException(PX.Commerce.Core.BCMessages.URLAlreadyExists);
				}
			}

			if (val != null)
			{
				if (!IsValidUrl(val, row.FileType))
				{
					throw new PXSetPropertyException(PX.Commerce.Core.BCMessages.InvalidURL);
				}
			}
		}

		protected void InventoryItem_CustomURL_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = (InventoryItem)e.Row;
			if (row == null || e.NewValue == null) return;
			String url = e.NewValue.ToString();

			if (url.StartsWith("/")) return;
			e.NewValue = url = "/" + url;
		}

		protected virtual void BCInventoryFileUrls_FileType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = (BCInventoryFileUrls)e.Row;
			if (row == null) return;

			string val = e.NewValue?.ToString();
			if (val != null)
			{
				if (!IsValidUrl(row.FileURL, val))
				{
					sender.RaiseExceptionHandling<BCInventoryFileUrls.fileURL>(row, row.FileURL, new PXSetPropertyException(PX.Commerce.Core.BCMessages.InvalidURL, PXErrorLevel.Error));
				}
			}
		}

		protected virtual void _(Events.RowSelected<InventoryItem> e)
		{
			var row = e.Row;
			if (row == null) return;
			//Show a warning if description is empty and export to external is true.
			PXUIFieldAttribute.SetError<InventoryItem.descr>(e.Cache, row, null);
			if (e.Cache.GetStatus(row) != PXEntryStatus.Inserted &&
			    row.ExportToExternal == true &&
			    string.IsNullOrEmpty(row.Descr))
			{
				e.Cache.RaiseExceptionHandling<InventoryItem.descr>(
					row,
					null,
					new PXSetPropertyException(
						BCObjectsMessages.InventoryItemDescriptionRequired,
						PXErrorLevel.Warning
					)
				);
			}

			var ext = row.GetExtension<BCInventoryItem>();
			if (ext == null) return;
			string val = row.ItemStatus;
			if (val == InventoryItemStatus.Inactive || val == InventoryItemStatus.MarkedForDeletion || val == InventoryItemStatus.NoSales)
			{
				PXUIFieldAttribute.SetEnabled<BCInventoryItem.visibility>(e.Cache, row, false);
			}
			else
				PXUIFieldAttribute.SetEnabled<BCInventoryItem.visibility>(e.Cache, row, true);
		}

		protected virtual void _(Events.RowPersisting<InventoryItem> e)
		{
			var row = e.Row;
			if (row == null) return;
			//Show a warning if description is empty and export to external is true.
			if (row.ExportToExternal == true &&
			    string.IsNullOrEmpty(row.Descr))
			{
				e.Cache.RaiseExceptionHandling<InventoryItem.descr>(
					row,
					null,
					new PXSetPropertyException(
						BCObjectsMessages.InventoryItemDescriptionRequired,
						PXErrorLevel.Warning
					)
				);
			}
		}
		protected virtual void _(Events.FieldVerifying<InventoryItem, InventoryItem.availabilityAdjustment> e)
		{
			InventoryItem row = e.Row;
			if (row == null) return;
			e.NewValue ??= 0;
		}
			#endregion

			private static bool IsValidUrl(string val, string type)
		{
			if (val == null) return false;
			return Uri.TryCreate(val, UriKind.Absolute, out var _);
		}
	}
}
