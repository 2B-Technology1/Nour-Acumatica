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
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.Extensions.CostAccrual
{
	/// <summary>This extension defines the accrual account/subaccount selection mechanism.</summary>
	/// <typeparam name="TGraph">A <see cref="PX.Data.PXGraph" /> type.</typeparam>
	/// <typeparam name="TPrimary">A DAC (a <see cref="PX.Data.IBqlTable" /> type).</typeparam>
	public abstract class NonStockAccrualGraph<TGraph, TPrimary> : PXGraphExtension<TGraph>
			where TGraph : PXGraph
			where TPrimary : class, IBqlTable, new()
	{
		public delegate object GetAccountSubUsingPostingClassDelegate(InventoryItem item, INSite site, INPostClass postClass);
		public delegate object GetAccountSubFromItemDelegate(InventoryItem item);

		public virtual object SetExpenseAccountSub(PXCache sender, PXFieldDefaultingEventArgs e, int? inventoryID, int? siteID,
			GetAccountSubUsingPostingClassDelegate GetAccountSubUsingPostingClass,
			GetAccountSubFromItemDelegate GetAccountSubFromItem)
		{
			if (inventoryID != null)
			{
				InventoryItem item = PX.Objects.IN.InventoryItem.PK.Find(sender.Graph, inventoryID);
				if (item != null && item.StkItem != true)
				{
					return SetExpenseAccountSub(sender, e, item, siteID, GetAccountSubUsingPostingClass, GetAccountSubFromItem);
				}
			}
			return null;
		}

		public virtual object SetExpenseAccountSub(PXCache sender, PXFieldDefaultingEventArgs e, InventoryItem item, int? siteID,
		GetAccountSubUsingPostingClassDelegate GetAccountSubUsingPostingClass,
		GetAccountSubFromItemDelegate GetAccountSubFromItem)
		{
			object expenseAccountSubID = null;
			expenseAccountSubID = GetExpenseAccountSub(sender, e, item, siteID, GetAccountSubUsingPostingClass, GetAccountSubFromItem);

			e.NewValue = expenseAccountSubID;
			e.Cancel = true;

			return expenseAccountSubID;
		}

		public virtual object GetExpenseAccountSub(PXCache sender, PXFieldDefaultingEventArgs e, int? inventoryID, int? siteID,
			GetAccountSubUsingPostingClassDelegate GetAccountSubUsingPostingClass,
			GetAccountSubFromItemDelegate GetAccountSubFromItem)
		{
			if (inventoryID != null)
			{
				InventoryItem item = PX.Objects.IN.InventoryItem.PK.Find(sender.Graph, inventoryID);
				if (item != null && item.StkItem != true)
				{
					return GetExpenseAccountSub(sender, e, item, siteID, GetAccountSubUsingPostingClass, GetAccountSubFromItem);
				}
			}
			return null;
		}

		public virtual object GetExpenseAccountSub(PXCache sender, PXFieldDefaultingEventArgs e, InventoryItem item, int? siteID,
		GetAccountSubUsingPostingClassDelegate GetAccountSubUsingPostingClass,
		GetAccountSubFromItemDelegate GetAccountSubFromItem)
		{
			object expenseAccountSubID = null;

			if (item != null && item.StkItem != true)
			{
				var insetup = (INSetup)PXSetup<INSetup>.SelectWindowed(sender.Graph, 0, 1);
				if (item.NonStockReceipt == true && PXAccess.FeatureInstalled<FeaturesSet.inventory>() && insetup != null && insetup.UpdateGL == true)
				{
					INPostClass postClass = INPostClass.PK.Find(sender.Graph, item.PostClassID);
					if (postClass != null)
					{
						var site = INSite.PK.Find(sender.Graph, siteID);
						try
						{
							expenseAccountSubID = GetAccountSubUsingPostingClass(item, site, postClass);
						}
						catch (PXMaskArgumentException)
						{
						}
					}
					else
					{
						expenseAccountSubID = null;
					}
				}
				else
				{
					expenseAccountSubID = GetAccountSubFromItem(item);
				}
			}

			return expenseAccountSubID;
		}
	}

}