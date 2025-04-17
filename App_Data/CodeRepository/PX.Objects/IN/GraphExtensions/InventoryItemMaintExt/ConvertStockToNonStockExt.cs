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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN.GraphExtensions.NonStockItemMaintExt;
using PX.Objects.PO;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.IN.GraphExtensions.InventoryItemMaintExt
{
	public class ConvertStockToNonStockExt : ConvertStockToNonStockExtBase<InventoryItemMaint, NonStockItemMaint>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<FeaturesSet.inventory>() &&
			!PXAccess.FeatureInstalled<FeaturesSet.manufacturing>() &&
			!PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>() &&
			!PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>() &&
			!PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>() &&
			!PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();

		public override void Initialize()
		{
			base.Initialize();

			convert.SetCaption(Messages.ConvertToNonStockItem);
		}

		#region Verification

		protected override int Verify(InventoryItem item, List<string> errors)
		{
			int numberOfErrors = VerifySiteStatus(item, errors);

			numberOfErrors += base.Verify(item, errors);

			numberOfErrors += VerifyPIClass(item, errors);
			numberOfErrors += VerifyPI(item, errors);
			numberOfErrors += VerifyKitSpec(item, errors);
			numberOfErrors += VerifyINTransitLine(item, errors);
			numberOfErrors += VerifyReplenishment(item, errors);

			return numberOfErrors;
		}

		protected virtual int VerifySiteStatus(InventoryItem item, List<string> errors)
		{
			var siteStatuses = SelectFrom<INSiteStatusByCostCenter>
				.Where<INSiteStatusByCostCenter.qtyOnHand.IsNotEqual<decimal0>
					.And<INSiteStatusByCostCenter.inventoryID.IsEqual<@P.AsInt>>
					.And<INSiteStatusByCostCenter.siteID.IsNotEqual<@P.AsInt>>>
				.View.ReadOnly.Select(Base, item.InventoryID, Base.insetup.Current.TransitSiteID)
				.RowCast<INSiteStatusByCostCenter>().ToArray();

			if (siteStatuses.Any())
			{
				string error = PXLocalizer.LocalizeFormat(Messages.CannotConvertSiteStatuses, item.InventoryCD.Trim(),
					string.Join(", ", siteStatuses.Select(s => INSite.PK.Find(Base, s.SiteID)?.SiteCD?.Trim())));

				PXTrace.WriteError(error);
				errors.Add(error);
			}
			else
			{
				// We need to check the INLocation table for cases when
				// negative qty is allowed and sum of QtyOnHand of Locations is zero
				// but QtyOnHand of Location is not zero, for example:
				// QtyOnHand of Location1 is -1, QtyOnHand of Location2 is 1 on the same site.
				var locationStatuses = SelectFrom<INLocationStatusByCostCenter>
					.Where<INLocationStatusByCostCenter.qtyOnHand.IsNotEqual<decimal0>
						.And<INLocationStatusByCostCenter.inventoryID.IsEqual<@P.AsInt>>
						.And<INLocationStatusByCostCenter.siteID.IsNotEqual<@P.AsInt>>>
					.AggregateTo<GroupBy<INLocationStatusByCostCenter.siteID>>
					.View.ReadOnly.Select(Base, item.InventoryID, Base.insetup.Current.TransitSiteID)
					.RowCast<INLocationStatusByCostCenter>().ToArray();

				if (locationStatuses.Any())
				{
					string error = PXLocalizer.LocalizeFormat(Messages.CannotConvertSiteStatuses, item.InventoryCD.Trim(),
						string.Join(", ", locationStatuses.Select(s => INSite.PK.Find(Base, s.SiteID)?.SiteCD?.Trim())));

					PXTrace.WriteError(error);
					errors.Add(error);

					return locationStatuses.Length;
				}
			}

			return siteStatuses.Length;
		}

		protected virtual int VerifyPIClass(InventoryItem item, List<string> errors)
		{
			var classes = SelectFrom<INPIClass>
				.Where<Exists<Select<INPIClassItem, Where<INPIClassItem.inventoryID.IsEqual<@P.AsInt>.And<INPIClassItem.FK.PIClass>>>>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID)
				.RowCast<INPIClass>().ToArray();

			if (classes.Any())
			{
				string error = PXLocalizer.LocalizeFormat(Messages.CannotConvertPIClasses, item.InventoryCD.Trim(),
					string.Join(", ", classes.Select(c => c.PIClassID)));

				PXTrace.WriteError(error);
				errors.Add(error);
			}

			return classes.Length;
		}

		protected virtual int VerifyPI(InventoryItem item, List<string> errors)
		{
			var documents = SelectFrom<INPIHeader>
				.Where<INPIHeader.status.IsNotIn<INPIHdrStatus.completed, INPIHdrStatus.cancelled>
					.And<Exists<Select<INPIDetail, Where<INPIDetail.inventoryID.IsEqual<@P.AsInt>.And<INPIDetail.FK.PIHeader>>>>>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID)
				.RowCast<INPIHeader>().ToArray();

			if (documents.Any())
			{
				string error = PXLocalizer.LocalizeFormat(Messages.CannotConvertPI, item.InventoryCD.Trim(),
					string.Join(", ", documents.Select(c => c.PIID)));

				PXTrace.WriteError(error);
				errors.Add(error);
			}

			return documents.Length;
		}

		protected virtual int VerifyKitSpec(InventoryItem item, List<string> errors)
		{
			var documents = SelectFrom<INKitSpecHdr>
				.Where<Exists<Select<INKitSpecStkDet, Where<INKitSpecStkDet.compInventoryID.IsEqual<@P.AsInt>
					.And<INKitSpecStkDet.FK.KitSpecification>>>>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID)
				.RowCast<INKitSpecHdr>().ToArray();

			if (documents.Any())
			{
				var listOfSpec = documents.Select(s => PXLocalizer.LocalizeFormat(Messages.CannotConvertKitSpecListItem,
					InventoryItem.PK.Find(Base, s.KitInventoryID)?.InventoryCD.Trim(), s.RevisionID));

				string error = PXLocalizer.LocalizeFormat(Messages.CannotConvertKitSpecList, item.InventoryCD.Trim(), string.Join("; ", listOfSpec));

				PXTrace.WriteError(error);
				errors.Add(documents.Length == 1 ? error :
					PXLocalizer.LocalizeFormat(Messages.CannotConvertKitSpecSeeTrace, item.InventoryCD.Trim()));
			}

			return documents.Length;
		}

		protected virtual int VerifyINTransitLine(InventoryItem item, List<string> errors)
		{
			INTransitLine line = SelectFrom<INTransitLine>
				.InnerJoin<INLocationStatusByCostCenter>.On<INTransitLine.costSiteID.IsEqual<INLocationStatusByCostCenter.locationID>>
				.Where<INLocationStatusByCostCenter.inventoryID.IsEqual<@P.AsInt>
					.And<INLocationStatusByCostCenter.qtyOnHand.IsGreater<decimal0>>>
				.View.ReadOnly.Select(Base, item.InventoryID);

			if (line != null)
			{
				PXTrace.WriteError(Messages.CannotConvertINTransit);
				errors.Add(Messages.CannotConvertINTransit);

				return 1;
			}

			return 0;
		}

		protected virtual int VerifyReplenishment(InventoryItem item, List<string> errors)
		{
			var plans = SelectFrom<INItemPlan>
				.Where<INItemPlan.inventoryID.IsEqual<@P.AsInt>
					.And<INItemPlan.planType.IsEqual<INPlanConstants.plan90>>>
				.AggregateTo<GroupBy<INItemPlan.fixedSource>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID)
				.RowCast<INItemPlan>().ToArray();

			int numberOfErrors = 0;

			if (plans.Any())
			{
				foreach (var groupedPlan in plans.GroupBy(p => GetINItemPlanMessage(p)))
				{
					string error = PXLocalizer.LocalizeFormat(groupedPlan.Key, item.InventoryCD.Trim());

					PXTrace.WriteError(error);
					errors.Add(error);

					numberOfErrors++;
				}
			}

			return numberOfErrors;
		}

		#endregion Verification

		#region Convert

		protected override InventoryItem ConvertMainFields(NonStockItemMaint graph, InventoryItem source, InventoryItem newItem)
		{
			newItem = base.ConvertMainFields(graph, source, newItem);

			newItem.ItemType = INItemTypes.NonStockItem;
			newItem.NonStockReceipt = true;
			newItem.NonStockShip = true;
			newItem.CompletePOLine = CompletePOLineTypes.Quantity;

			return newItem;
		}

		protected override void DeleteRelatedRecords(NonStockItemMaint graph, int? inventoryID)
		{
			base.DeleteRelatedRecords(graph, inventoryID);

			var ext = graph.FindImplementation<ConvertNonStockToStockExt>();

			foreach (INItemSite row in ext.AllItemSiteRecords.Select(inventoryID))
				ext.AllItemSiteRecords.Delete(row);

			foreach (INItemRep row in ext.AllReplenishmentRecords.Select(inventoryID))
				ext.AllReplenishmentRecords.Delete(row);
			
			foreach (INItemBoxEx row in ext.Boxes.Select(inventoryID))
				ext.Boxes.Delete(row);
		}

		#endregion // Convert
	}
}
