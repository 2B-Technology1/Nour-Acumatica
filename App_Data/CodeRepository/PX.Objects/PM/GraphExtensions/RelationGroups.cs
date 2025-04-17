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
using PX.Objects.CT;
using PX.Objects.IN;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	public class RelationGroupsExt : PXGraphExtension<RelationGroups>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.projectModule>();
		}

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt]
		protected void _(Events.CacheAttached<Contract.templateID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt]
		protected void _(Events.CacheAttached<Contract.duration> e) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBString(1)]
		protected void _(Events.CacheAttached<Contract.durationType> e) { }

		[PXOverride]
		public bool CanBeRestricted(Type entityType, object instance)
		{
			if (entityType == typeof(InventoryItem))
			{
				InventoryItem item = instance as InventoryItem;
				if (item != null)
				{
					return item.ItemStatus != InventoryItemStatus.Unknown;
				}
			}

			if (entityType == typeof(Contract) || entityType == typeof(PMProject))
			{
				Contract item = instance as Contract;
				if (item != null)
				{
					return item.NonProject != true && item.BaseType == CTPRType.Project;
				}
			}

			return true;
		}
	}
}
