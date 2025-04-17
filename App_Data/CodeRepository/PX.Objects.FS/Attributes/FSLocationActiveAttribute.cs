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
using PX.Objects.CS;
using PX.Objects.CR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS
{
	public class FSLocationActiveAttribute : LocationActiveAttribute
	{
		public FSLocationActiveAttribute(Type WhereType)
			: base(WhereType, typeof(
				LeftJoin<Address,
					On<Address.bAccountID, Equal<Location.bAccountID>,
						And<Address.addressID, Equal<Location.defAddressID>>>,
				LeftJoin<Country,
					On<Country.countryID, Equal<Address.countryID>>,
				LeftJoin<Contact,
					On<Contact.contactID, Equal<Location.defContactID>>>>
			>))
		{
		}

		protected override PXDimensionSelectorAttribute GetSelectorAttribute(Type searchType)
		{
			return new PXDimensionSelectorAttribute(
				DimensionName,
				searchType,
				typeof(Location.locationCD),
				typeof(Location.locationCD),
				typeof(Location.descr),
				typeof(Address.addressLine1),
				typeof(Address.addressLine2),
				typeof(Address.city),
				typeof(Address.state),
				typeof(Address.postalCode),
				typeof(Country.description),
				typeof(Contact.phone1)
			);
		}

	}
}
