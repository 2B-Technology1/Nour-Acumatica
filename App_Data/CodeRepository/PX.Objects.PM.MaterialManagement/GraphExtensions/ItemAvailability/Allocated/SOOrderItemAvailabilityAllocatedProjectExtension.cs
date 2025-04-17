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
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.SO.GraphExtensions.SOOrderEntryExt;

namespace PX.Objects.PM.MaterialManagement.GraphExtensions.ItemAvailability.Allocated
{
	[PXProtectedAccess]
	public abstract class SOOrderItemAvailabilityAllocatedProjectExtension : ItemAvailabilityAllocatedProjectExtension<
		SOOrderEntry,
		SOOrderItemAvailabilityExtension,
		SOOrderItemAvailabilityAllocatedExtension,
		SOOrderItemAvailabilityProjectExtension,
		SOLine, SOLineSplit>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.materialManagement>();

		protected override string GetStatusWithAllocatedProject(SOLine line)
		{
			string status = null;

			bool excludeCurrent = line?.Completed != true;

			PMProject project;
			if (ProjectDefaultAttribute.IsProject(Base, line.ProjectID, out project)
				&& project.AccountingMode != ProjectAccountingModes.Linked
				&& ItemAvailBase.FetchWithLineUOM(line, excludeCurrent, line.CostCenterID) is IStatus availability &&
				ItemAvailProjExt.FetchWithLineUOMProject(line, excludeCurrent, line.CostCenterID) is IStatus availabilityProject)
			{
				decimal? allocated = GetAllocatedQty(line);

				status = FormatStatusAllocatedProject(availability, availabilityProject, allocated, line.UOM);
				Check(line, availabilityProject);
			}

			return status;
		}

		private string FormatStatusAllocatedProject(IStatus availability, IStatus availabilityProject, decimal? allocated, string uom)
		{
			return PXMessages.LocalizeFormatNoPrefixNLA(
				SO.Messages.Availability_AllocatedInfo_Project,
				uom,
				FormatQty(availabilityProject.QtyOnHand),
				FormatQty(availabilityProject.QtyAvail),
				FormatQty(availabilityProject.QtyHardAvail),
				FormatQty(allocated),
				FormatQty(availabilityProject.QtyOnHand + availability.QtyOnHand),
				FormatQty(availabilityProject.QtyAvail + availability.QtyAvail),
				FormatQty(availabilityProject.QtyHardAvail + availability.QtyHardAvail));
		}

		#region PXProtectedAccess
		/// Uses <see cref="SOOrderItemAvailabilityAllocatedExtension.GetAllocatedQty(SOLine)"/>
		[PXProtectedAccess(typeof(SOOrderItemAvailabilityAllocatedExtension))] protected abstract decimal GetAllocatedQty(SOLine line);

		/// Uses <see cref="IN.GraphExtensions.ItemAvailabilityExtension{TGraph, TLine, TSplit}.Check(ILSMaster, IStatus)"/>
		[PXProtectedAccess(typeof(SOOrderItemAvailabilityExtension))] protected abstract void Check(ILSMaster row, IStatus availability);

		/// Uses <see cref="IN.GraphExtensions.ItemAvailabilityExtension{TGraph, TLine, TSplit}.FormatQty(decimal?)"/>
		[PXProtectedAccess(typeof(SOOrderItemAvailabilityExtension))] protected abstract string FormatQty(decimal? value);
		#endregion
	}
}
