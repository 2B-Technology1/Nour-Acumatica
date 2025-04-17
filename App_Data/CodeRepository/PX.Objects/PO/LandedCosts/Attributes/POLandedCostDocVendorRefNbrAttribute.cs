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
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;

namespace PX.Objects.PO.LandedCosts.Attributes
{
	public class POLandedCostDocVendorRefNbrAttribute : BaseVendorRefNbrAttribute
	{
		#region Ctor
		public POLandedCostDocVendorRefNbrAttribute() : base(typeof(POLandedCostDoc.vendorID))
		{
		}
		#endregion

		#region Implementation
		protected override bool IsIgnored(PXCache sender, object row)
		{
			POLandedCostDoc r = (POLandedCostDoc)row;
			return r.Released == true || r.CreateBill != true || base.IsIgnored(sender, row) || (String.IsNullOrEmpty(r.VendorRefNbr) && r.Hold == true);
		}

		protected override EntityKey GetEntityKey(PXCache sender, object row)
		{
			var ek = new EntityKey();
			ek._DetailID = DETAIL_DUMMY;
			ek._MasterID = GetMasterNoteId(typeof(POLandedCostDoc), typeof(POLandedCostDoc.noteID), row);

			return ek;
		}

		public override Guid? GetSiblingID(PXCache sender, object row)
		{
			return (Guid?)sender.GetValue<POLandedCostDoc.noteID>(row);
		}
		#endregion
	}
}
