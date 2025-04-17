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
using PX.Objects.IN.GraphExtensions;

namespace PX.Objects.PM.MaterialManagement.GraphExtensions.ItemAvailability
{
	public abstract class INRegisterItemAvailabilityProjectExtension<TRegisterGraph, TRegisterItemAvailExt> : ItemAvailabilityProjectExtension<TRegisterGraph, TRegisterItemAvailExt, INTran, INTranSplit>
		where TRegisterGraph : INRegisterEntryBase
		where TRegisterItemAvailExt : INRegisterItemAvailabilityExtension<TRegisterGraph>
	{
		protected override string GetStatusProject(INTran line)
		{
			string status = null;

			bool excludeCurrent = line?.Released != true;

			PMProject project;
			if (ProjectDefaultAttribute.IsProject(Base, line.ProjectID, out project)
				&& project.AccountingMode != ProjectAccountingModes.Linked)
			{
				if (FetchWithLineUOM(line, excludeCurrent, CostCenter.FreeStock) is IStatus availability &&
					FetchWithLineUOMProject(line, excludeCurrent, line.CostCenterID) is IStatus availabilityProject)
				{
					status = FormatStatusProject(availability, availabilityProject, line.UOM);
					Check(line, line.CostCenterID);
				}
			}
			
			return status;
		}

		private string FormatStatusProject(IStatus availability, IStatus availabilityProject, string uom)
		{
			return PXMessages.LocalizeFormatNoPrefixNLA(
				IN.Messages.Availability_ActualInfo_Project,
				uom,
				FormatQty(availabilityProject.QtyOnHand),
				FormatQty(availabilityProject.QtyAvail),
				FormatQty(availabilityProject.QtyHardAvail),
				FormatQty(availabilityProject.QtyActual),
				FormatQty(availabilityProject.QtyOnHand + availability.QtyOnHand),
				FormatQty(availabilityProject.QtyAvail + availability.QtyAvail),
				FormatQty(availabilityProject.QtyHardAvail + availability.QtyHardAvail),
				FormatQty(availabilityProject.QtyActual + availability.QtyActual));
		}
	}

	[PXProtectedAccess(typeof(INIssueEntry.ItemAvailabilityExtension))]
	public abstract class INIssueItemAvailabilityProjectExtension
		: INRegisterItemAvailabilityProjectExtension<INIssueEntry, INIssueEntry.ItemAvailabilityExtension>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.materialManagement>();
	}

	[PXProtectedAccess(typeof(INReceiptEntry.ItemAvailabilityExtension))]
	public abstract class INReceiptItemAvailabilityProjectExtension
		: INRegisterItemAvailabilityProjectExtension<INReceiptEntry, INReceiptEntry.ItemAvailabilityExtension>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.materialManagement>();
	}

	[PXProtectedAccess(typeof(INAdjustmentEntry.ItemAvailabilityExtension))]
	public abstract class INAdjustmentItemAvailabilityProjectExtension
		: INRegisterItemAvailabilityProjectExtension<INAdjustmentEntry, INAdjustmentEntry.ItemAvailabilityExtension>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.materialManagement>();
	}

	[PXProtectedAccess(typeof(INTransferEntry.ItemAvailabilityExtension))]
	public abstract class INTransferItemAvailabilityProjectExtension
		: INRegisterItemAvailabilityProjectExtension<INTransferEntry, INTransferEntry.ItemAvailabilityExtension>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.materialManagement>();
	}
}
