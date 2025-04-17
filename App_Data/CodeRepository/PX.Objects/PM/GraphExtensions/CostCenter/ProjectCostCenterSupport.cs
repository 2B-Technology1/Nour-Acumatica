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
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.PM
{
	public class ProjectCostCenterSupport<T> : ProjectCostCenterBase<T>, IINTranCostCenterSupport
		where T : INRegisterEntryBase
	{
		public bool IsSupported(string layerType)
		{
			return layerType == CostLayerType.Project || layerType == CostLayerType.Normal;
		}

		public virtual int SortOrder => 200;

		public bool IsShipmentPosting { get; set; }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<INTran.costLayerType.FromCurrent.IsNotEqual<CostLayerType.project>
			.Or<PMProject.nonProject.IsNotEqual<True>.And<PMProject.accountingMode.IsEqual<ProjectAccountingModes.projectSpecific>>>>),
			Messages.NotProjectCostLayerAndProjectTrackedByCost, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<INTran.costLayerType.FromCurrent.IsNotEqual<CostLayerType.normal>
			.Or<PMProject.nonProject.IsEqual<True>>.Or<PMProject.accountingMode.IsNotEqual<ProjectAccountingModes.projectSpecific>>>),
			Messages.ProjectCostLayerAndProjectNotTrackedByCost, typeof(PMProject.contractCD))]
		protected virtual void _(Events.CacheAttached<INTran.projectID> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<INTran.toCostLayerType.FromCurrent.IsNotEqual<CostLayerType.project>
			.Or<PMProject.nonProject.IsNotEqual<True>.And<PMProject.accountingMode.IsEqual<ProjectAccountingModes.projectSpecific>>>>),
			Messages.NotProjectCostLayerAndProjectTrackedByCost, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<INTran.toCostLayerType.FromCurrent.IsNotEqual<CostLayerType.normal>
			.Or<PMProject.nonProject.IsEqual<True>>.Or<PMProject.accountingMode.IsNotEqual<ProjectAccountingModes.projectSpecific>>>),
			Messages.ProjectCostLayerAndProjectNotTrackedByCost, typeof(PMProject.contractCD))]
		protected virtual void _(Events.CacheAttached<INTran.toProjectID> e)
		{
		}

		protected virtual void _(Events.FieldDefaulting<INTran, INTran.projectID> e)
		{
			if (e.Row?.CostLayerType == CostLayerType.Project)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldVerifying<INTranSplit, INTranSplit.locationID> e)
		{
			if (e.Row != null)
			{
				INTran tran = (INTran)PXParentAttribute.SelectParent(Base.INTranSplitDataMember.Cache, e.Row, typeof(INTran));
				if (tran != null &&
					e.NewValue != null &&
					tran.LocationID != (int?)e.NewValue &&
					GetAccountingMode(tran.ProjectID) == ProjectAccountingModes.ProjectSpecific &&
					!IsShipmentPosting)
				{
					PMProject project = PMProject.PK.Find(Base, tran.ProjectID);
					INLocation location = INLocation.PK.Find(Base, (int?)e.NewValue);

					PXSetPropertyException ex = new PXSetPropertyException(Messages.MixedLocationsAreNotAllowed, project.ContractCD.Trim(), e.Row.LineNbr?.ToString());
					if (location != null)
						// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [False positive - The instance passed e.Row is not modified]
						ex.ErrorValue = location.LocationCD;

					throw ex;
				}
			}
		}
		
		public virtual IEnumerable<Type> GetFieldsDependOn()
		{
			yield return typeof(INTran.isSpecialOrder);
			yield return typeof(INTran.siteID);
			yield return typeof(INTran.locationID);
			yield return typeof(INTran.projectID);
			yield return typeof(INTran.taskID);
		}

		public virtual bool IsSpecificCostCenter(INTran tran)
			=> tran.IsSpecialOrder != true && IsSpecificCostCenter(tran.SiteID, tran.ProjectID, tran.TaskID);

		public virtual IEnumerable<Type> GetDestinationFieldsDependOn()
		{
			yield return typeof(INTran.isSpecialOrder);
			yield return typeof(INTran.toSiteID);
			yield return typeof(INTran.toLocationID);
			yield return typeof(INTran.toProjectID);
			yield return typeof(INTran.toTaskID);
		}

		public virtual bool IsDestinationSpecificCostCenter(INTran tran)
			=> IsSpecificCostCenter(tran.ToSiteID, tran.ToProjectID, tran.ToTaskID);
				
		public virtual int GetCostCenterID(INTran tran)
		{
			return (int)FindOrCreateCostCenter(tran.SiteID, tran.ProjectID, tran.TaskID);
		}

		public virtual int GetDestinationCostCenterID(INTran tran)
		{
			return (int)FindOrCreateCostCenter(tran.ToSiteID, tran.ToProjectID, tran.ToTaskID);
		}

		public virtual void OnCostLayerTypeChanged(INTran tran, string newCostLayerType)
		{
			Base.Caches<INTran>().SetValueExt<INTran.taskID>(tran, null);
			Base.Caches<INTran>().SetValueExt<INTran.projectID>(tran, ProjectDefaultAttribute.NonProject());
			Base.Caches<INTran>().SetValueExt<INTran.costCenterID>(tran, CostCenter.FreeStock);
			Base.Caches<INTran>().SetValueExt<INTran.costCodeID>(tran, null);
		}

		public virtual void OnDestinationCostLayerTypeChanged(INTran tran, string newCostLayerType)
		{
			Base.Caches<INTran>().SetValueExt<INTran.toTaskID>(tran, null);
			Base.Caches<INTran>().SetValueExt<INTran.toProjectID>(tran, ProjectDefaultAttribute.NonProject());
			Base.Caches<INTran>().SetValueExt<INTran.toCostCenterID>(tran, CostCenter.FreeStock);
			Base.Caches<INTran>().SetValueExt<INTran.toCostCodeID>(tran, null);
		}

		public virtual void ValidateForPersisting(INTran tran)
		{
			if (tran.CostLayerType == CostLayerType.Project &&
				(tran.TaskID == null ||
				tran.SiteID == null))
			{
				var project = PMProject.PK.Find(Base, tran.ProjectID);
				Base.Caches<INTran>().RaiseExceptionHandling<INTran.projectID>(tran, project?.ContractCD,
					new PXSetPropertyException(Messages.WarehouseLocationProjectTaskShouldBeFilled, PXErrorLevel.Error));

				throw new PXRowPersistingException(nameof(INTran.projectID),
					tran.ProjectID, Messages.WarehouseLocationProjectTaskShouldBeFilled);
			}
		}

		public virtual void ValidateDestinationForPersisting(INTran tran)
		{
			if (tran.ToCostLayerType == CostLayerType.Project &&
				(tran.ToTaskID == null ||
				tran.ToSiteID == null))
			{
				var project = PMProject.PK.Find(Base, tran.ToProjectID);
				Base.Caches<INTran>().RaiseExceptionHandling<INTran.toProjectID>(tran, project?.ContractCD,
					new PXSetPropertyException(Messages.DestWarehouseLocationProjectTaskShouldBeFilled, PXErrorLevel.Error));

				throw new PXRowPersistingException(nameof(INTran.toProjectID),
					tran.ToProjectID, Messages.DestWarehouseLocationProjectTaskShouldBeFilled);
			}
		}

		
	}
}
