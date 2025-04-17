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
using PX.Objects.FS;
using PX.Objects.IN;

namespace PX.Objects.PM.MaterialManagement.GraphExtensions.ItemAvailability
{
	// TODO: ensure this class is even needed - could project availability be used in ServiceOrderEntry?
	// if yes, then the GetStatusProject's meaningful implementation is missing, otherwise this class should be removed
	[PXProtectedAccess(typeof(FSServiceOrderItemAvailabilityExtension))]
	public abstract class FSServiceOrderItemAvailabilityProjectExtension
		: ItemAvailabilityProjectExtension<ServiceOrderEntry, FSServiceOrderItemAvailabilityExtension, FSSODet, FSSODetSplit>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.materialManagement>();
		protected override string GetStatusProject(FSSODet line)
		{
			bool excludeCurrent = line?.Completed != true;

			if (FetchWithLineUOM(line, excludeCurrent, CostCenter.FreeStock) is IStatus availability &&
				FetchWithLineUOMProject(line, excludeCurrent, CostCenter.FreeStock) is IStatus availabilityProject)
			{
				Check(line, CostCenter.FreeStock);
				return FormatStatusProject(availability, availabilityProject, line.UOM);
			}
			return null;
		}

		private string FormatStatusProject(IStatus availability, IStatus availabilityProject, string uom)
		{
			return PXMessages.LocalizeFormatNoPrefixNLA(
				IN.Messages.Availability_Info_Project,
				uom,
				FormatQty(availabilityProject.QtyOnHand),
				FormatQty(availabilityProject.QtyAvail),
				FormatQty(availabilityProject.QtyHardAvail),
				FormatQty(availability.QtyOnHand),
				FormatQty(availability.QtyAvail),
				FormatQty(availability.QtyHardAvail));
		}
	}
}
