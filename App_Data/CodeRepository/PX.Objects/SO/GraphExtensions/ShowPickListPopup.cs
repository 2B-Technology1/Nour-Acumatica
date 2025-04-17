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
using System.Collections;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.SO.GraphExtensions
{
	// Should be used together with ShowPickListPanel.inc
	public static class ShowPickListPopup
	{
		public abstract class On<TGraph, TPrimary>
			where TGraph : PXGraph
			where TPrimary : class, IBqlTable, new()
		{
			public abstract class FilteredBy<TWhere> : PXGraphExtension<TGraph>
				where TWhere : IBqlWhere, new()
			{
				#region Views
				public
					SelectFrom<SOPickerListEntry>.
					InnerJoin<SOPicker>.On<SOPickerListEntry.FK.Picker>.
					InnerJoin<SOPickingWorksheet>.On<SOPicker.FK.Worksheet>.
					InnerJoin<SOPickingJob>.On<SOPickingJob.FK.Picker>.
					InnerJoin<IN.INLocation>.On<SOPickerListEntry.FK.Location>.
					InnerJoin<IN.InventoryItem>.On<SOPickerListEntry.FK.InventoryItem>.
					Where<TWhere>.
					OrderBy<
						IN.INLocation.pathPriority.Asc,
						IN.INLocation.locationCD.Asc,
						IN.InventoryItem.inventoryCD.Asc,
						SOPickerListEntry.lotSerialNbr.Asc>.
					View PickListEntries;
				#endregion

				#region Configuration
				protected virtual bool IsPickListExternalViewMode => true;

				protected virtual bool CanDeletePickList(TPrimary primaryRow) => IsPickListExternalViewMode == false;
				protected virtual void PerformPickListDeletion() => throw new NotImplementedException();
				#endregion

				#region Event Handlers
				protected virtual void _(Events.RowSelected<TPrimary> e)
				{
					DeletePickList.SetVisible(IsPickListExternalViewMode == false);
					DeletePickList.SetEnabled(CanDeletePickList(e.Row));

					ViewPickListSource.SetVisible(IsPickListExternalViewMode == true);
					ViewPickListSource.SetEnabled(IsPickListExternalViewMode == true);

					ShowPickList.SetEnabled(e.Row != null && PickListEntries.SelectSingle() != null);
				}
				#endregion

				#region Buttons
				[PXButton(CommitChanges = true), PXUIField(DisplayName = "Show Pick List", MapEnableRights = PXCacheRights.Select)]
				protected virtual void showPickList() => PickListEntries.AskExt();
				public PXAction<TPrimary> ShowPickList;

				[PXButton(CommitChanges = true, DisplayOnMainToolbar = false, ConfirmationMessage = Msg.DeleteConfirmation, ConfirmationType = PXConfirmationType.Always)]
				[PXUIField(DisplayName = "Delete Pick List")]
				protected virtual IEnumerable deletePickList(PXAdapter adapter)
				{
					PerformPickListDeletion();
					return adapter.Get();
				}
				public PXAction<TPrimary> DeletePickList;

				[PXButton(CommitChanges = true, DisplayOnMainToolbar = false), PXUIField(DisplayName = "View Source Document", MapEnableRights = PXCacheRights.Select)]
				protected virtual void viewPickListSource()
				{
					var sheet = SOPickerListEntry.FK.Worksheet.FindParent(Base, PickListEntries.Current);
					if (sheet != null)
					{
						if (sheet.WorksheetType == SOPickingWorksheet.worksheetType.Single)
						{
							var shipmentEntry = PXGraph.CreateInstance<SOShipmentEntry>();
							shipmentEntry.Document.Current = shipmentEntry.Document.Search<SOShipment.shipmentNbr>(sheet.SingleShipmentNbr);
							throw new PXRedirectRequiredException(shipmentEntry, newWindow: true, "");
						}
						else
						{
							var worksheetEntry = PXGraph.CreateInstance<SOPickingWorksheetReview>();
							worksheetEntry.worksheet.Current = worksheetEntry.worksheet.Search<SOPickingWorksheet.worksheetNbr>(sheet.WorksheetNbr);
							throw new PXRedirectRequiredException(worksheetEntry, newWindow: true, "");
						}
					}
				}
				public PXAction<TPrimary> ViewPickListSource;
				#endregion
			}
		}


		[PXLocalizable]
		public abstract class Msg
		{
			public const string DeleteConfirmation = "The current Pick List record will be deleted.";
		}
	}
}
