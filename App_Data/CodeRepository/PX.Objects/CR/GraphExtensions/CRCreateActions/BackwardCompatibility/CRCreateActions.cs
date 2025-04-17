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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;

namespace PX.Objects.CR.Extensions.CRCreateActions.BC
{
	public class CreateContactFromCustomerGraphExt : PXGraphExtension<CustomerMaint>
	{
		public PXAction<Customer> newContact;
		[PXUIField(DisplayName = Messages.AddContact, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert, Visible = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable NewContact(PXAdapter adapter)
		{
			var ext = Base.GetExtension<CustomerMaint.CreateContactFromCustomerGraphExt>();

			if (ext == null)
				return adapter.Get();

			return ext.CreateContact.Press(adapter);
		}
	}

	public class CreateContactFromVendorGraphExt : PXGraphExtension<VendorMaint>
	{
		public PXAction<Vendor> newContact;
		[PXUIField(DisplayName = Messages.AddContact, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert, Visible = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable NewContact(PXAdapter adapter)
		{
			var ext = Base.GetExtension<VendorMaint.CreateContactFromVendorGraphExt>();

			if (ext == null)
				return adapter.Get();

			return ext.CreateContact.Press(adapter);
		}
	}

	public class CreateContactFromOpportunityGraphExt : PXGraphExtension<OpportunityMaint>
	{
		public PXAction<CROpportunity> addNewContact;
		[PXUIField(DisplayName = Messages.AddContact, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert, Visible = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable AddNewContact(PXAdapter adapter)
		{
			var ext = Base.GetExtension<OpportunityMaint.CreateContactFromOpportunityGraphExt>();

			if (ext == null)
				return adapter.Get();

			return ext.CreateContact.Press(adapter);
		}
	}
}
