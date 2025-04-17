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

using PX.Commerce.Core;
using PX.Commerce.Objects;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CR;
using System.Linq;

namespace PX.Commerce.Shopify
{
	/// <summary>
	/// Graph Extension of the Location Maint graph to add support for company roles.
	/// </summary>
	public class SPLocationRoleAssignmentExt : PXGraphExtension<CustomerLocationMaint>
	{
		/// <summary>
		/// Returns <see langword="true"/> if the Shopify connector feature is enabled.
		/// </summary>
		/// <returns>returns <see langword="true"/> if the Shopify connector feature is enabled</returns>
		public static bool IsActive()
		{
			return CommerceFeaturesHelper.ShopifyConnector && CommerceFeaturesHelper.CommerceB2B;
		}

		public SelectFrom<BCRoleAssignment>
			.Where<BCRoleAssignment.bAccountID.IsEqual<Location.bAccountID.FromCurrent>.And<BCRoleAssignment.locationID.IsEqual<Location.locationID.FromCurrent>>>
			.View RoleAssignments;

		public virtual void _(Events.RowPersisting<Location> e)
		{
			var list = RoleAssignments.Select().Select(x => x.GetItem<BCRoleAssignment>())?.ToList();
			bool? hasMultipleItems = list?.GroupBy(x => x.ContactID).Where(x => x.Count() > 1).Any();
			if (hasMultipleItems == true)
			{
				throw new PXException(BCObjectsMessages.DuplicateRoleAssignmentForContact);
			}
		}
	}
}
