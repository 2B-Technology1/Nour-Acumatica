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
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CS
{
	public class ShippingZoneMaint : PXGraph<ShippingZoneMaint>
	{
        public PXSavePerRow<ShippingZone> Save;
		public PXCancel<ShippingZone> Cancel;
		[PXImport(typeof(ShippingZone))]
		public PXSelect<ShippingZone> ShippingZones;

		protected virtual void ShippingZone_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			ShippingZone zone = PXSelect<ShippingZone, Where<ShippingZone.zoneID, Equal<Required<ShippingZone.zoneID>>>>.SelectWindowed(this, 0, 1, ((ShippingZone)e.Row).ZoneID);
			if (zone != null)
			{
				cache.RaiseExceptionHandling<ShippingZone.zoneID>(e.Row, ((ShippingZone)e.Row).ZoneID, new PXException(ErrorMessages.RecordExists));
				e.Cancel = true;
			}
		}
	}
}
