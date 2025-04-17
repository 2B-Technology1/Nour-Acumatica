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
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using System;

namespace PX.Objects.PM.BudgetControl
{
	public class ChangeOrderEntryExt : BudgetControlGraph<ChangeOrderEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectModule>();
		}

		protected override DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(PMChangeOrder))
			{
				Hold = typeof(PMChangeOrder.hold),
				WarningAmount = typeof(PMChangeOrder.commitmentTotal)
			};
		}

		protected override DetailMapping GetDetailMapping()
		{
			return new DetailMapping(typeof(PMChangeOrderLine))
			{
				ProjectID = typeof(PMChangeOrderLine.projectID),
				TaskID = typeof(PMChangeOrderLine.taskID),
				InventoryID = typeof(POLine.inventoryID),
				CostCodeID = typeof(PMChangeOrderLine.costCodeID),
				WarningAmount = typeof(PMChangeOrderLine.amount),
				LineNbr = typeof(PMChangeOrderLine.lineNbr),
				TranDescription = typeof(PMChangeOrderLine.description)
			};
		}

		protected override int? GetAccountGroup(Detail row)
		{
			PMChangeOrderLine line = (PMChangeOrderLine)Details.Cache.GetMain(row);

			InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<PMChangeOrderLine.inventoryID>(Base.Details.Cache, line);
			if (item != null && item.StkItem == true && item.COGSAcctID != null)
			{
				Account account = (Account)PXSelectorAttribute.Select<InventoryItem.cOGSAcctID>(Base.Caches[typeof(InventoryItem)], item);
				if (account != null && account.AccountGroupID != null)
					return account.AccountGroupID;
			}
			else
			{
				Account account = (Account)PXSelectorAttribute.Select<PMChangeOrderLine.accountID>(Base.Details.Cache, line);
				if (account != null && account.AccountGroupID != null)
					return account.AccountGroupID;
			}
			return null;
		}

		protected override bool DetailIsChanged(Detail oldRow, Detail row)
		{
			PMChangeOrderLine oldLine = (PMChangeOrderLine)Details.Cache.GetMain(oldRow);
			PMChangeOrderLine line = (PMChangeOrderLine)Details.Cache.GetMain(row);
			var result =
				oldLine.ProjectID != line.ProjectID ||
				oldLine.TaskID != line.TaskID ||
				oldLine.InventoryID != line.InventoryID ||
				oldLine.CostCodeID != line.CostCodeID ||
				oldLine.AccountID != line.AccountID ||
				oldLine.Amount != line.Amount;
			return result;
		}

		protected override bool IsBudgetControlRequiredForDocument()
		{
			if (Base.Document?.Current == null) return false;
			var result = Base.Document.Current.Released == false && Base.Document.Current.Rejected == false;
			return result;
		}

		protected override decimal? GetDetailAmount(Detail row)
		{
			PMChangeOrderLine line = (PMChangeOrderLine)Details.Cache.GetMain(row);
			var result = line.AmountInProjectCury;
			return result;
		}

		protected override decimal? GetDetailBaseAmount(Detail row)
		{
			throw new NotImplementedException();
		}

		protected override decimal? GetDetailCuryAmount(Detail row)
		{
			throw new NotImplementedException();
		}
	}
}
