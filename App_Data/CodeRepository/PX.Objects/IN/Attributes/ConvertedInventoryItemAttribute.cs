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
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.IN.Exceptions;

namespace PX.Objects.IN.Attributes
{
	public class ConvertedInventoryItemAttribute : PXEventSubscriberAttribute,
		IPXFieldUpdatedSubscriber//, IPXFieldVerifyingSubscriber
	{
		protected Type _isStockItemField;
		protected bool? _isStockItemValue;

		public ConvertedInventoryItemAttribute(Type isStockItemField)
		{
			_isStockItemField = isStockItemField;
		}

		public ConvertedInventoryItemAttribute(bool isStockItemValue)
		{
			_isStockItemValue = isStockItemValue;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			// We add the event handler by AddHandler because we need to execute the event handler even if we have "Cancel=true" in the graph event handler.
			sender.Graph.FieldVerifying.AddHandler(BqlTable, FieldName, FieldVerifying);
		}

		public virtual void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_isStockItemField == null)
				return;

			var inventoryID = (int?)sender.GetValue(e.Row, _FieldName);
			bool? isStockItem = null;

			if (inventoryID != null)
				isStockItem = InventoryItem.PK.Find(sender.Graph, inventoryID)?.StkItem;

			sender.SetValue(e.Row, _isStockItemField.Name, isStockItem);
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			bool? isStockItem = null;

			if (_isStockItemField != null)
			{
				isStockItem = sender.GetValue(e.Row, _isStockItemField.Name) as bool?;
			}
			else
			{
				isStockItem = _isStockItemValue;
			}

			if (isStockItem == null)
				return;

			var currentInventoryID = (int?)sender.GetValue(e.Row, _FieldName);
			if (currentInventoryID != null)
				return;

			var newInventoryID = e.NewValue as int?;
			if (newInventoryID == null)
				return;

			var newItem = InventoryItem.PK.Find(sender.Graph, newInventoryID);
			if (newItem?.IsConverted == true && newItem.StkItem != isStockItem)
				throw new ItemHasBeenConvertedException(newItem?.InventoryCD);
		}

		public virtual void Validate(PXCache cache, object row)
		{
			bool? isStockItem = null;

			if (_isStockItemField != null)
			{
				isStockItem = cache.GetValue(row, _isStockItemField.Name) as bool?;
			}
			else
			{
				isStockItem = _isStockItemValue;
			}

			if (isStockItem == null)
				return;

			var inventoryID = cache.GetValue(row, FieldName) as int?;
			if (inventoryID == null)
				return;

			var inventoryItem = InventoryItem.PK.Find(cache.Graph, inventoryID);
			if (inventoryItem?.IsConverted == true && inventoryItem.StkItem != isStockItem)
				throw new ItemHasBeenConvertedException(inventoryItem.InventoryCD);
		}

		public static void ValidateRow(PXCache cache, object row)
		{
			cache.GetAttributesReadonly(null)
				.OfType<ConvertedInventoryItemAttribute>()
				.ForEach(
					a => a.Validate(cache, row));
		}
	}
}
