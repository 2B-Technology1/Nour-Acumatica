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
using PX.Objects.AP;
using PX.Objects.Extensions;
using PX.Objects.PO;
using System.Collections;

namespace PX.Objects.FS
{
	public class AppointmentEntryRedirectExt : PXGraphExtension<RedirectExtension<AppointmentEntry>, AppointmentEntry>
	{
		private RedirectExtension<AppointmentEntry> BaseRedirect { get; set; }

		#region ViewPOVendor
		public PXAction<VendorR> viewPOVendor;
		[PXUIField(DisplayName = "View Vendor", MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select)]
		[PXButton(DisplayOnMainToolbar = false, VisibleOnProcessingResults = false, PopupVisible = false)]
		public virtual IEnumerable ViewPOVendor(PXAdapter adapter)
		{
			BaseRedirect = Base.GetExtension<RedirectExtension<AppointmentEntry>>();
			return BaseRedirect.ViewCustomerVendorEmployee<FSAppointmentDet.poVendorID>(adapter);
		}
		#endregion

		#region ViewPOVendorLocation
		public PXAction<VendorR> viewPOVendorLocation;
		[PXUIField(DisplayName = "View Vendor Location", MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select)]
		[PXButton(DisplayOnMainToolbar = false, VisibleOnProcessingResults = false, PopupVisible = false)]
		public virtual IEnumerable ViewPOVendorLocation(PXAdapter adapter)
		{
			BaseRedirect = Base.GetExtension<RedirectExtension<AppointmentEntry>>();
			return BaseRedirect.ViewVendorLocation<FSAppointmentDet.poVendorLocationID, FSAppointmentDet.poVendorID>(adapter);
		}
		#endregion

		#region ViewEmployee
		public PXAction<VendorR> viewEmployee;
		[PXUIField(DisplayName = "View Vendor", MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select)]
		[PXButton(DisplayOnMainToolbar = false, VisibleOnProcessingResults = false, PopupVisible = false)]
		public virtual IEnumerable ViewEmployee(PXAdapter adapter)
		{
			BaseRedirect = Base.GetExtension<RedirectExtension<AppointmentEntry>>();
			return BaseRedirect.ViewCustomerVendorEmployee<FSAppointmentEmployee.employeeID>(adapter);
		}
		#endregion
	}
}
