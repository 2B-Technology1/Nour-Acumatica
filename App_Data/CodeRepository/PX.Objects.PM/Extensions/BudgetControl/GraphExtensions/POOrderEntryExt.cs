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

namespace PX.Objects.PM.BudgetControl
{
	public class POOrderEntryExt : BudgetControlGraph<POOrderEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectModule>();
		}

		protected override DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(POOrder))
			{
				CuryID = typeof(POOrder.curyID),
				Date = typeof(POOrder.orderDate),
				Hold = typeof(POOrder.hold),
				WarningAmount = typeof(POOrder.curyOrderTotal)
			};
		}

		protected override DetailMapping GetDetailMapping()
		{
			return new DetailMapping(typeof(POLine))
			{
				ProjectID = typeof(POLine.projectID),
				TaskID = typeof(POLine.taskID),
				InventoryID = typeof(POLine.inventoryID),
				CostCodeID = typeof(POLine.costCodeID),
				WarningAmount = typeof(POLine.curyLineAmt),
				LineNbr = typeof(POLine.lineNbr),
				TranDescription = typeof(POLine.tranDesc)
			};
		}

		protected override int? GetAccountGroup(Detail row)
		{
			POLine line = (POLine)Details.Cache.GetMain(row);

			InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<POLine.inventoryID>(Base.Transactions.Cache, line);
			if (item != null && item.StkItem == true && item.COGSAcctID != null)
			{
				Account account = (Account)PXSelectorAttribute.Select<InventoryItem.cOGSAcctID>(Base.Caches[typeof(InventoryItem)], item);
				if (account != null && account.AccountGroupID != null)
					return account.AccountGroupID;
			}
			else
			{
				Account account = (Account)PXSelectorAttribute.Select<POLine.expenseAcctID>(Base.Transactions.Cache, line);
				if (account != null && account.AccountGroupID != null)
					return account.AccountGroupID;
			}
			return null;
		}

		protected override bool DetailIsChanged(Detail oldRow, Detail row)
		{
			POLine oldLine = (POLine)Details.Cache.GetMain(oldRow);
			POLine line = (POLine)Details.Cache.GetMain(row);
			var result =
				oldLine.ProjectID != line.ProjectID ||
				oldLine.TaskID != line.TaskID ||
				oldLine.InventoryID != line.InventoryID ||
				oldLine.CostCodeID != line.CostCodeID ||
				oldLine.ExpenseAcctID != line.ExpenseAcctID ||
				oldLine.CuryExtCost != line.CuryExtCost ||
				oldLine.CuryRetainageAmt != line.CuryRetainageAmt ||
				oldLine.Completed != line.Completed ||
				oldLine.Cancelled != line.Cancelled ||
				oldLine.Closed != line.Closed;
			return result;
		}

		protected override decimal? GetDetailBaseAmount(Detail row)
		{
			POLine line = (POLine)Details.Cache.GetMain(row);
			var result = line.ExtCost + line.RetainageAmt.GetValueOrDefault();
			return result;
		}

		protected override decimal? GetDetailCuryAmount(Detail row)
		{
			POLine line = (POLine)Details.Cache.GetMain(row);
			var result = line.CuryExtCost + line.CuryRetainageAmt.GetValueOrDefault();
			return result;
		}

		protected override bool IsBudgetControlRequiredForDocument()
		{
			if (Base.Document?.Current == null) return false;
			if (Base.Document.Current.Hold == false && Base.Document.Current.Approved == true) return false;
			var result = 
				Base.Document.Current.OrderType == POOrderType.RegularOrder ||
				Base.Document.Current.OrderType == POOrderType.RegularSubcontract ||
				Base.Document.Current.OrderType == POOrderType.DropShip;
			return result;
		}

		protected override bool IsBudgetControlRequiredForDetail(Detail row)
		{
			POLine line = (POLine)Details.Cache.GetMain(row);
			var result = !(line.Completed == true || line.Cancelled == true || line.Closed == true);
			return result;
		}
	}
}
