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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.IN.GraphExtensions.InventoryItemMaintExt;
using PX.Objects.IN.GraphExtensions.NonStockItemMaintExt;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.Objects
{
	public class BCStockToNonStockConverter : PXGraphExtension<InventoryItemMaintBase>
	{
		public static bool IsActive() { return PXAccess.FeatureInstalled<FeaturesSet.inventory>() && CommerceFeaturesHelper.CommerceEdition; }

		public PXSelect<BCSyncStatus> SyncStatuses;
		public PXSelect<BCSyncDetail> SyncDetails;
		public virtual void Convert(PXGraph graph, InventoryItem item, String typeFrom, String typeTo)
		{
			PXCache cache = graph.Caches[typeof(BCSyncStatus)];

			PXDatabase.SelectDate(out DateTime dtLocal, out DateTime dtUtc);
			dtLocal = PX.Common.PXTimeZoneInfo.ConvertTimeFromUtc(dtUtc, PX.Common.LocaleInfo.GetTimeZone());

			//Changin the entity type for status record from the source type to the destination type.
			foreach (BCSyncStatus status in PXSelect<BCSyncStatus,
						Where<BCSyncStatus.entityType, Equal<Required<BCSyncStatus.entityType>>,
							And<BCSyncStatus.localID, Equal<Required<BCSyncStatus.localID>>>>>.Select(graph, typeFrom, item.NoteID))
			{
				status.EntityType = typeTo;
				status.LastOperation = BCSyncOperationAttribute.ForcedToResync;
				status.LastErrorMessage = null;
				status.PendingSync = true;
				status.LastOperationTS = dtLocal;
				status.SyncInProcess = false;
				status.LastErrorMessage = null;
				status.Status = BCSyncStatusAttribute.Pending;
				status.AttemptCount = 0;
				status.Deleted = false;

				cache.Update(status);
			}
		}
	}

	public class BCConvertStockToNonStockExt : PXGraphExtension<ConvertStockToNonStockExt, InventoryItemMaint>
	{
		public static bool IsActive() { return (PXGraph.GeneratorIsActive || ConvertStockToNonStockExt.IsActive()) && PXAccess.FeatureInstalled<FeaturesSet.inventory>() && CommerceFeaturesHelper.CommerceEdition; }

		[PXOverride]
		public virtual InventoryItem ConvertCommerceData(NonStockItemMaint graph, InventoryItem source, InventoryItem newItem,
			Func<NonStockItemMaint, InventoryItem, InventoryItem, InventoryItem> handler)
		{
			if (handler != null) newItem = handler(graph, source, newItem);

			//Copy the main fields
			BCInventoryItem newItemExt = newItem?.GetExtension<BCInventoryItem>();
			BCInventoryItem sourceExt = source?.GetExtension<BCInventoryItem>();

			newItemExt.CustomURL = sourceExt.CustomURL;
			newItemExt.PageTitle = sourceExt.PageTitle;
			newItemExt.SearchKeywords = sourceExt.SearchKeywords;
			newItemExt.MetaKeywords = sourceExt.MetaKeywords;
			newItemExt.MetaDescription = sourceExt.MetaDescription;

			if (newItem.Availability == BCItemAvailabilities.AvailableTrack)
				newItem.Availability = BCItemAvailabilities.AvailableSkip;
			newItem.NotAvailMode = BCItemNotAvailModes.StoreDefault;

			//Update the Sync Status to reflect changes
			BCStockToNonStockConverter ext = graph.FindImplementation<BCStockToNonStockConverter>();
			if (ext != null) ext.Convert(graph, newItem, BCEntitiesAttribute.StockItem, BCEntitiesAttribute.NonStockItem);

			return newItem;
		}

		[PXOverride]
		public virtual void DeleteRelatedRecords(NonStockItemMaint graph, int? inventoryID, Action<NonStockItemMaint, int?> handler)
		{
			if (handler != null) handler(graph, inventoryID);

			PXCache cache = graph.Caches[typeof(BCSyncStatus)];

			InventoryItem row = InventoryItem.PK.Find(graph, inventoryID);
			if (row != null)
			{
				//Delete availability
				foreach (BCSyncStatus status in PXSelect<BCSyncStatus,
										Where<BCSyncStatus.entityType, Equal<BCEntitiesAttribute.productAvailability>,
											And<BCSyncStatus.localID, Equal<Required<BCSyncStatus.localID>>>>>.Select(Base, row.NoteID))
				{
					cache.Delete(status);
				}
			}
		}
	}
	public class ConvertNonStockToStockExtExt : PXGraphExtension<ConvertNonStockToStockExt, NonStockItemMaint>
	{
		public static bool IsActive() { return (PXGraph.GeneratorIsActive || ConvertNonStockToStockExt.IsActive()) && PXAccess.FeatureInstalled<FeaturesSet.inventory>() && CommerceFeaturesHelper.CommerceEdition; }


		[PXOverride]
		public virtual int VerifyInventoryItem(InventoryItem item, List<string> errors, Func<InventoryItem, List<string>, int> handler)
		{
			int result = 0;
			if (handler != null) result = handler(item, errors);

			var giftCertificates = PXSelectJoin<BCBinding,
				InnerJoin<BCBindingExt, On<BCBinding.bindingID, Equal<BCBindingExt.bindingID>>>,
				Where<BCBindingExt.giftCertificateItemID, Equal<Required<BCBindingExt.giftCertificateItemID>>>>
				.SelectWindowed(Base, 0, -1, item.InventoryID).RowCast<BCBinding>().ToArray();
			if (giftCertificates.Any())
			{
				result += giftCertificates.Length;

				string error = PXLocalizer.LocalizeFormat(BCObjectsMessages.CannotConvertInventoryGiftCertificate,
					item.InventoryCD.Trim(), string.Join(", ", giftCertificates.Select(d => d.BindingName)));

				PXTrace.WriteError(error);
				errors.Add(error);
			}

			var giftWrappings = PXSelectJoin<BCBinding,
				InnerJoin<BCBindingExt, On<BCBinding.bindingID, Equal<BCBindingExt.bindingID>>>,
				Where<BCBindingExt.giftWrappingItemID, Equal<Required<BCBindingExt.giftWrappingItemID>>>>
				.SelectWindowed(Base, 0, -1, item.InventoryID).RowCast<BCBinding>().ToArray();
			if (giftWrappings.Any())
			{
				result += giftWrappings.Length;

				string error = PXLocalizer.LocalizeFormat(BCObjectsMessages.CannotConvertInventoryGiftWrapping,
					item.InventoryCD.Trim(), string.Join(", ", giftWrappings.Select(d => d.BindingName)));

				PXTrace.WriteError(error);
				errors.Add(error);
			}

			var refundItems = PXSelectJoin<BCBinding,
				InnerJoin<BCBindingExt, On<BCBinding.bindingID, Equal<BCBindingExt.bindingID>>>,
				Where<BCBindingExt.refundAmountItemID, Equal<Required<BCBindingExt.refundAmountItemID>>>>
				.SelectWindowed(Base, 0, -1, item.InventoryID).RowCast<BCBinding>().ToArray();
			if (refundItems.Any())
			{
				result += refundItems.Length;

				string error = PXLocalizer.LocalizeFormat(BCObjectsMessages.CannotConvertInventoryRefundItem,
					item.InventoryCD.Trim(), string.Join(", ", refundItems.Select(d => d.BindingName)));

				PXTrace.WriteError(error);
				errors.Add(error);
			}

			return result;
		}

		[PXOverride]
		public virtual InventoryItem ConvertCommerceData(InventoryItemMaint graph, InventoryItem source, InventoryItem newItem,
			Func<InventoryItemMaint, InventoryItem, InventoryItem, InventoryItem> handler)
		{
			if (handler != null) newItem = handler(graph, source, newItem);

			//Copy the main fields
			BCInventoryItem newItemExt = newItem?.GetExtension<BCInventoryItem>();
			BCInventoryItem sourceExt = source?.GetExtension<BCInventoryItem>();

			newItemExt.CustomURL = sourceExt.CustomURL;
			newItemExt.PageTitle = sourceExt.PageTitle;
			newItemExt.SearchKeywords = sourceExt.SearchKeywords;
			newItemExt.MetaKeywords = sourceExt.MetaKeywords;
			newItemExt.MetaDescription = sourceExt.MetaDescription;

			//Update the Sync Status to reflect changes
			BCStockToNonStockConverter ext = graph.FindImplementation<BCStockToNonStockConverter>();
			if(ext != null) ext.Convert(graph, newItem, BCEntitiesAttribute.NonStockItem, BCEntitiesAttribute.StockItem);

			return newItem;
		}
	}
}
