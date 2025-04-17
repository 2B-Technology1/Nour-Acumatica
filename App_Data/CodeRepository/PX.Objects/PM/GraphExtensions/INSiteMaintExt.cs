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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
    public class INSiteMaintExt : PXGraphExtension<INSiteMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.projectAccounting>();
        }

		[PXOverride]
		public virtual void Persist(Action baseHandler)
		{
			var currentLocations = Base.location.Select().RowCast<INLocation>()
				.Where(a => a.ProjectID != null && a.Active == true && !ProjectDefaultAttribute.IsNonProject(a.ProjectID));

			foreach (INLocation location in currentLocations)
				{
				if (Base.location.Cache.GetStatus(location) == PXEntryStatus.Notchanged)
					continue;

				var duplicatedInDifferentWarehouse = SelectFrom<INLocation>
					.Where<INLocation.siteID.IsNotEqual<P.AsInt>
					.And<INLocation.projectID.IsEqual<P.AsInt>>
					.And<INLocation.active.IsEqual<True>>>
					.View.Select(Base, location.SiteID, location.ProjectID);

				var duplicatedInSameWarehoue = Base.location.Select().RowCast<INLocation>()
				.Where(a => a.ProjectID == location.ProjectID && a.Active == true);

				var invalidRecordsInSameWarehouse = SelectFrom<INLocation>
					.Where<INLocation.siteID.IsEqual<P.AsInt>
					.And<INLocation.projectID.IsEqual<P.AsInt>>
					.And<INLocation.active.IsEqual<True>>>
					.View.Select(Base, location.SiteID, location.ProjectID);

				if(invalidRecordsInSameWarehouse.Any())
				{
					duplicatedInSameWarehoue = duplicatedInSameWarehoue.OrderBy(a => a.CreatedDateTime);
				}

				if (duplicatedInDifferentWarehouse.Any())
				{
					ShowErrorWhenDuplicatedInDifferentWarehouse(location, duplicatedInDifferentWarehouse, duplicatedInSameWarehoue);
				}
				else if(duplicatedInSameWarehoue.Any())
				{
					INLocation originalLocation = duplicatedInSameWarehoue.First();
					if (location == originalLocation)
						continue;

					ShowErrorWhenDuplicatedInSameWarehouse(location, duplicatedInSameWarehoue, originalLocation);
				}
			}

			baseHandler();
		}

		private void ShowErrorWhenDuplicatedInSameWarehouse(INLocation location, IEnumerable<INLocation> duplicatedInSameWarehoue, INLocation originalLocation)
		{
			INSite currentWarehouse = INSite.PK.Find(Base, originalLocation.SiteID);
			INLocation originalTaskLocation = duplicatedInSameWarehoue.RowCast<INLocation>()
				.Where(a => a.TaskID == location.TaskID).First();

			bool isTaskDuplicatedInSameWarehouse = location != originalTaskLocation
				&& location.TaskID == originalTaskLocation.TaskID && originalTaskLocation.TaskID != null;

			bool setErrorInProject = false;
			var locationsWithNoTasks = duplicatedInSameWarehoue.Where(a => a.TaskID == null);
			if (locationsWithNoTasks.Any())
			{
				setErrorInProject = locationsWithNoTasks.First() != location;
			}
			
			if (originalLocation.TaskID == null && location.TaskID == null  )
					{
				PMProject project = PMProject.PK.Find(Base, originalLocation.ProjectID);
				Base.location.Cache.RaiseExceptionHandling<INLocation.projectID>(location, project.ContractCD,
					new PXSetPropertyException(Messages.ProjectAlreadyAssigned, project.ContractCD, originalLocation.LocationCD, currentWarehouse.SiteCD));
			}
			else if (location.TaskID == null && setErrorInProject)
			{
				PMProject project = PMProject.PK.Find(Base, originalLocation.ProjectID);
				Base.location.Cache.RaiseExceptionHandling<INLocation.projectID>(location, project.ContractCD,
					new PXSetPropertyException(Messages.SameProjectTaskCombination, project.ContractCD, originalLocation.LocationCD, currentWarehouse.SiteCD));
			}
			else if (isTaskDuplicatedInSameWarehouse)
					{
				PMTask task = PMTask.PK.Find(Base, originalTaskLocation.ProjectID, originalTaskLocation.TaskID);
				Base.location.Cache.RaiseExceptionHandling<INLocation.taskID>(location, task.TaskCD,
					new PXSetPropertyException(Messages.ProjectTaskAlreadyAssigned, task.TaskCD, originalTaskLocation.LocationCD, Base.site.Current.SiteCD));
					}
				}

		private void ShowErrorWhenDuplicatedInDifferentWarehouse(INLocation location, PXResultset<INLocation> duplicatedInDifferentWarehouse, IEnumerable<INLocation> duplicatedInSameWarehoue)
		{
			INLocation originalLocationFromDifferntWarehouse = duplicatedInDifferentWarehouse.First();
			INLocation originalTaskLocation = duplicatedInSameWarehoue.RowCast<INLocation>()
				.Where(a => a.TaskID == location.TaskID).First();
			PMProject project = PMProject.PK.Find(Base, location.ProjectID);
			INSite warehouse = INSite.PK.Find(Base, originalLocationFromDifferntWarehouse.SiteID);

			bool isTaskDuplicatedInDifferendWarehouse = location.TaskID != null && originalLocationFromDifferntWarehouse.TaskID == location.TaskID;
			bool isTaskDuplicatedInSameWarehouse = location != originalTaskLocation
				&& location.TaskID == originalTaskLocation.TaskID && originalTaskLocation.TaskID != null && originalLocationFromDifferntWarehouse.TaskID != null;

			bool setErrorInProject = false;
			if(originalLocationFromDifferntWarehouse.TaskID != null)
			{
				var locationsWithNoTasks = duplicatedInSameWarehoue.Where(a => a.TaskID == null);
				if (locationsWithNoTasks.Any())
				{
					setErrorInProject = locationsWithNoTasks.First() != location;
				}
			}
			else
			{
				if(location != originalLocationFromDifferntWarehouse && location.TaskID == null)
				{
					Base.location.Cache.RaiseExceptionHandling<INLocation.projectID>(location, project.ContractCD,
						new PXSetPropertyException(Messages.ProjectAlreadyAssigned, project.ContractCD, originalLocationFromDifferntWarehouse.LocationCD, warehouse.SiteCD));
				}
			}

			if (isTaskDuplicatedInDifferendWarehouse)
			{
				PMTask task = PMTask.PK.Find(Base, originalLocationFromDifferntWarehouse.ProjectID, originalLocationFromDifferntWarehouse.TaskID);
				Base.location.Cache.RaiseExceptionHandling<INLocation.taskID>(location, task.TaskCD,
					new PXSetPropertyException(Messages.ProjectTaskAlreadyAssigned, task.TaskCD, originalLocationFromDifferntWarehouse.LocationCD, warehouse.SiteCD));
			}
			else if (isTaskDuplicatedInSameWarehouse)
			{
				PMTask task = PMTask.PK.Find(Base, originalTaskLocation.ProjectID, originalTaskLocation.TaskID);
				Base.location.Cache.RaiseExceptionHandling<INLocation.taskID>(location, task.TaskCD,
					new PXSetPropertyException(Messages.ProjectTaskAlreadyAssigned, task.TaskCD, originalTaskLocation.LocationCD, Base.site.Current.SiteCD));
			}
			else if (location.TaskID == null && setErrorInProject)
			{
				Base.location.Cache.RaiseExceptionHandling<INLocation.projectID>(location, project.ContractCD,
					new PXSetPropertyException(Messages.SameProjectTaskCombination, project.ContractCD, originalLocationFromDifferntWarehouse.LocationCD, warehouse.SiteCD));
			}
		}

		protected virtual void _(Events.FieldUpdated<INLocation, INLocation.projectID> e)
		{
			if (e.Row?.ProjectID != null)
				e.Cache.SetValueExt<INLocation.isCosted>(e.Row, true);
		}

		protected virtual void _(Events.FieldVerifying<INLocation, INLocation.projectID> e)
		{
			//TODO: Redo this using Plans and Status tables once we have them in version 7.0

			if (e.Row == null) return;

			PO.POReceiptLine unreleasedPO =
				SelectFrom<PO.POReceiptLine>.
				Where<
					PO.POReceiptLine.projectID.IsEqual<P.AsInt>.
					And<PO.POReceiptLine.released.IsEqual<False>>.
					And<PO.POReceiptLine.locationID.IsEqual<P.AsInt>>>.
				View.SelectWindowed(Base, 0, 1, e.Row.ProjectID, e.Row.LocationID);

			if (unreleasedPO != null)
			{
				PMProject project = SelectFrom<PMProject>.Where<PMProject.contractID.IsEqual<P.AsInt>>.View.Select(Base, e.Row.ProjectID ?? e.NewValue);
				if (project != null)
					e.NewValue = project.ContractCD;

				throw new PXSetPropertyException(IN.Messages.ProjectUsedInPO);
			}

			SO.SOShipLine unreleasedSO =
				SelectFrom<SO.SOShipLine>.
				Where<
					SO.SOShipLine.projectID.IsEqual<P.AsInt>.
					And<SO.SOShipLine.released.IsEqual<False>>.
					And<SO.SOShipLine.locationID.IsEqual<P.AsInt>>>.
				View.SelectWindowed(Base, 0, 1, e.Row.ProjectID, e.Row.LocationID);

			if (unreleasedSO != null)
			{
				PMProject project = SelectFrom<PMProject>.Where<PMProject.contractID.IsEqual<P.AsInt>>.View.Select(Base, e.Row.ProjectID ?? e.NewValue);
				if (project != null)
					e.NewValue = project.ContractCD;

				throw new PXSetPropertyException(IN.Messages.ProjectUsedInSO);
			}

			INLocationStatusByCostCenter locationStatus =
				SelectFrom<INLocationStatusByCostCenter>.
				Where<
					INLocationStatusByCostCenter.siteID.IsEqual<P.AsInt>.
					And<INLocationStatusByCostCenter.locationID.IsEqual<P.AsInt>>.
					And<INLocationStatusByCostCenter.qtyOnHand.IsNotEqual<decimal0>>>.
				View.SelectWindowed(Base, 0, 1, e.Row.SiteID, e.Row.LocationID);

			if (locationStatus != null)
			{
				PMProject project = SelectFrom<PMProject>.Where<PMProject.contractID.IsEqual<P.AsInt>>.View.Select(Base, e.Row.ProjectID ?? e.NewValue);
				if (project != null)
					e.NewValue = project.ContractCD;

				throw new PXSetPropertyException(IN.Messages.ProjectUsedInIN);
			}
		}

		protected virtual void _(Events.FieldVerifying<INLocation, INLocation.taskID> e)
		{
			//TODO: Redo this using Plans and Status tables once we have them in version 7.0

			if (e.Row == null) return;

			PO.POReceiptLine unreleasedPO =
				SelectFrom<PO.POReceiptLine>.
				Where<
					PO.POReceiptLine.taskID.IsEqual<P.AsInt>.
					And<PO.POReceiptLine.released.IsEqual<False>>.
					And<PO.POReceiptLine.locationID.IsEqual<P.AsInt>>>.
				View.SelectWindowed(Base, 0, 1, e.Row.TaskID, e.Row.LocationID);

			if (unreleasedPO != null)
			{
				PMTask task = PMTask.PK.FindDirty(Base, e.Row.ProjectID, e.Row.TaskID ?? (int?)e.NewValue);
				if (task != null)
					e.NewValue = task.TaskCD;

				throw new PXSetPropertyException(IN.Messages.TaskUsedInPO);
			}

			SO.SOShipLine unreleasedSO =
				SelectFrom<SO.SOShipLine>.
				Where<
					SO.SOShipLine.taskID.IsEqual<P.AsInt>.
					And<SO.SOShipLine.released.IsEqual<False>>.
					And<SO.SOShipLine.locationID.IsEqual<P.AsInt>>>.
				View.SelectWindowed(Base, 0, 1, e.Row.TaskID, e.Row.LocationID);

			if (unreleasedSO != null)
			{
				PMTask task = PMTask.PK.FindDirty(Base, e.Row.ProjectID, e.Row.TaskID ?? (int?)e.NewValue);
				if (task != null)
					e.NewValue = task.TaskCD;

				throw new PXSetPropertyException(IN.Messages.TaskUsedInSO);
			}

			INLocationStatusByCostCenter locationStatus =
				SelectFrom<INLocationStatusByCostCenter>.
				Where<
					INLocationStatusByCostCenter.siteID.IsEqual<P.AsInt>.
					And<INLocationStatusByCostCenter.locationID.IsEqual<P.AsInt>>.
					And<INLocationStatusByCostCenter.qtyOnHand.IsNotEqual<decimal0>>>.
				View.SelectWindowed(Base, 0, 1, e.Row.SiteID, e.Row.LocationID);

			if (locationStatus != null)
			{
				PMTask task = PMTask.PK.FindDirty(Base, e.Row.ProjectID, e.Row.TaskID ?? (int?)e.NewValue);
				if (task != null)
					e.NewValue = task.TaskCD;

				throw new PXSetPropertyException(IN.Messages.TaskUsedInIN);
			}
		}
	}
}
