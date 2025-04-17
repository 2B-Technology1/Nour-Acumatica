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

using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.IN.GraphExtensions;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;
using IQtyAllocated = PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.Abstraction.IQtyAllocated;

namespace PX.Objects.PM.MaterialManagement.GraphExtensions.ItemAvailability
{
	public abstract class ItemAvailabilityProjectExtension<TGraph, TItemAvailExt, TLine, TSplit> : PXGraphExtension<TItemAvailExt, TGraph>
		where TGraph : PXGraph
		where TItemAvailExt : ItemAvailabilityExtension<TGraph, TLine, TSplit>
		where TLine : class, IBqlTable, ILSPrimary, new()
		where TSplit : class, IBqlTable, ILSDetail, new()
	{
		protected TItemAvailExt ItemAvailBase => Base1;
				
		protected virtual bool IsLinkedProject(int? projectID)
		{
			return
				projectID.IsNotIn(null, ProjectDefaultAttribute.NonProject()) &&
				PMProject.PK.Find(Base, projectID) is PMProject project &&
				project.AccountingMode == ProjectAccountingModes.Linked;
		}


		/// Overrides <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.GetStatus(TLine)"/>
		[PXOverride]
		public virtual string GetStatus(TLine line,
			Func<TLine, string> base_GetStatus)
		{
			if (PXAccess.FeatureInstalled<CS.FeaturesSet.materialManagement>())
				return GetStatusProject(line) ?? base_GetStatus(line);
			else
				return base_GetStatus(line);
		}

		protected abstract string GetStatusProject(TLine line);

		#region Check
		/// Overrides <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.Check(ILSMaster, int?)"/>
		[PXOverride]
		public virtual void Check(ILSMaster row, int? costCenterID,
			Action<ILSMaster, int?> base_Check)
		{
			using (PXAccess.FeatureInstalled<CS.FeaturesSet.materialManagement>() ? ProjectAvailabilityScope() : null)
				base_Check(row, costCenterID);
		}

		/// Overrides <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.GetStatusLevel(IStatus)"/>
		[PXOverride]
		public virtual ItemAvailabilityExtension<TGraph, TLine, TSplit>.StatusLevel GetStatusLevel(IStatus availability,
			Func<IStatus, ItemAvailabilityExtension<TGraph, TLine, TSplit>.StatusLevel> base_GetWarningLevel)
		{
			switch (availability)
			{
				case  LotSerialStatusByCostCenter _:
					return ItemAvailabilityExtension<TGraph, TLine, TSplit>.StatusLevel.LotSerial;
				case LocationStatusByCostCenter _:
					return ItemAvailabilityExtension<TGraph, TLine, TSplit>.StatusLevel.Location;
				case SiteStatusByCostCenter _:
					return ItemAvailabilityExtension<TGraph, TLine, TSplit>.StatusLevel.Site;
				default:
					return base_GetWarningLevel(availability);
			}
		}
		#endregion

		#region Fetch
		public IStatus FetchWithLineUOMProject(TLine line, bool excludeCurrent, int? costCenterID)
		{
			using (ProjectAvailabilityScope())
				return FetchWithLineUOM(line, excludeCurrent, costCenterID);
		}

		public virtual IStatus FetchWithBaseUOMProject(ILSMaster row, bool excludeCurrent, int? costCenterID)
		{
			using (ProjectAvailabilityScope())
				return FetchWithBaseUOM(row, excludeCurrent, costCenterID);
		}

		#region Overrides
		
		/// Overrides <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.FetchSite(ILSDetail, bool, int?)"/>
		[PXOverride]
		public virtual IStatus FetchSite(ILSDetail split, bool excludeCurrent, int? costCenterID,
			Func<ILSDetail, bool, int?, IStatus> base_FetchSite)
		{
			if (!_projectAvailability)
				return base_FetchSite(split, excludeCurrent, costCenterID);

			if (split.ProjectID == null)
			{
				TLine line = PXParentAttribute.SelectParent<TLine>(SplitCache, split);
				if (line != null)
				{
					split.ProjectID = line.ProjectID;
					split.TaskID = line.TaskID;
				}
			}

			if (split.TaskID == null)
			{
				if (split.ProjectID == ProjectDefaultAttribute.NonProject())
					return base_FetchSite(split, excludeCurrent, CostCenter.FreeStock);

				PMProject project = PMProject.PK.Find(Base, split.ProjectID);
				if (project != null &&
					project.BaseType == CT.CTPRType.Project &&
					project.AccountingMode != ProjectAccountingModes.Linked)
				{
					var select = new PXSelect<INCostCenter,
						Where<INCostCenter.siteID, Equal<Required<INCostCenter.siteID>>,
						And<INCostCenter.projectID, Equal<Required<INCostCenter.projectID>>>>>(Base);

					IStatus summary = null;
					foreach (INCostCenter costCenter in select.Select(split.SiteID, split.ProjectID))
					{
						if (summary == null)
						{
							summary = base_FetchSite(split, excludeCurrent, costCenter.CostCenterID);
						}
						else
						{
							summary.Add(base_FetchSite(split, excludeCurrent, costCenter.CostCenterID));
						}
					}

					return summary;
				}
				else
				{
					return base_FetchSite(split, excludeCurrent, costCenterID);
				}
			}
			else
				return base_FetchSite(split, excludeCurrent, costCenterID);
		}
	
		#endregion
		#endregion

		#region ProjectAvailabilityScope
		protected IDisposable ProjectAvailabilityScope() => new Common.SimpleScope(
			onOpen: () => _projectAvailability = true,
			onClose: () => _projectAvailability = false);
		private bool _projectAvailability;
		#endregion

		#region Protected Access
		/// Uses <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.LineCache"/>
		[PXProtectedAccess] protected abstract PXCache<TLine> LineCache { get; }

		/// Uses <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.SplitCache"/>
		[PXProtectedAccess] protected abstract PXCache<TSplit> SplitCache { get; }

		/// Uses <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.Fetch{TQtyAllocated}(ILSDetail, IStatus, IStatus, bool)"/>
		[PXProtectedAccess] protected abstract IStatus Fetch<TQtyAllocated>(ILSDetail split, IStatus allocated, IStatus existing, bool excludeCurrent) where TQtyAllocated : class, IQtyAllocated, IBqlTable, new();

		/// Uses <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.InitializeRecord{T}(T)"/>
		[PXProtectedAccess] protected abstract T InitializeRecord<T>(T row) where T : class, IBqlTable, new();

		/// Uses <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.FetchWithLineUOM(TLine, bool, int?)"/>
		[PXProtectedAccess] protected abstract IStatus FetchWithLineUOM(TLine line, bool excludeCurrent, int? costCenterID);

		/// Uses <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.FetchWithBaseUOM(ILSMaster, bool, int?)"/>
		[PXProtectedAccess] protected abstract IStatus FetchWithBaseUOM(ILSMaster row, bool excludeCurrent, int? costCenterID);

		/// Uses <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.Check(ILSMaster, int?)"/>
		[PXProtectedAccess] protected abstract void Check(ILSMaster row, int? costCenterID);

		/// Uses <see cref="ItemAvailabilityExtension{TGraph, TLine, TSplit}.FormatQty(decimal?)"/>
		[PXProtectedAccess] protected abstract string FormatQty(decimal? value);
		#endregion
	}
}
