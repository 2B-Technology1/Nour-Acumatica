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
using PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt;

namespace PX.Objects.PM.MaterialManagement.GraphExtensions.ItemAvailability
{
	[PXProtectedAccess(typeof(SOInvoiceItemAvailabilityExtension))]
	public abstract class SOInvoiceItemAvailabilityProjectExtension
		: ItemAvailabilityProjectExtension<SOInvoiceEntry, SOInvoiceItemAvailabilityExtension, AR.ARTran, ARTranAsSplit>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.materialManagement>();
		protected override string GetStatusProject(AR.ARTran line)
		{
			string status = null;

			bool excludeCurrent = true;

			SOLine soLine = SOLine.PK.Find(Base, line.SOOrderType, line.SOOrderNbr, line.SOOrderLineNbr);

			PMProject project;
			if (soLine != null && ProjectDefaultAttribute.IsProject(Base, line.ProjectID, out project)
				&& project.AccountingMode != ProjectAccountingModes.Linked)
			{
				if (FetchWithLineUOM(line, excludeCurrent, CostCenter.FreeStock) is IStatus availability &&
					FetchWithLineUOMProject(line, excludeCurrent, soLine.CostCenterID) is IStatus availabilityProject)
				{
					status = FormatStatusProject(availability, availabilityProject, line.UOM);
					Check(line, soLine.CostCenterID);
				}
			}

			return status;
		}

		private string FormatStatusProject(IStatus availability, IStatus availabilityProject, string uom)
		{
			return PXMessages.LocalizeFormatNoPrefixNLA(
				IN.Messages.Availability_Info_Project,
				uom,
				FormatQty(availabilityProject.QtyOnHand),
				FormatQty(availabilityProject.QtyAvail),
				FormatQty(availabilityProject.QtyHardAvail),
				FormatQty(availabilityProject.QtyOnHand + availability.QtyOnHand),
				FormatQty(availabilityProject.QtyAvail + availability.QtyAvail),
				FormatQty(availabilityProject.QtyHardAvail + availability.QtyHardAvail));
		}
	}
}
