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

using CommonServiceLocator;
using PX.Data;
using PX.Objects.CA.Descriptor;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
    public class POCommitmentAttribute : PMCommitmentAttribute
	{
		public POCommitmentAttribute() : base(typeof(POOrder))
		{
		}

		public override void DocumentRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			POOrder row = e.Row as POOrder;
			POOrder oldRow = e.OldRow as POOrder;

			if (IsCommitmentSyncRequired(sender, row, oldRow))
			{
				foreach (ICommitmentSource line in PXParentAttribute.SelectChildren(sender.Graph.Caches[detailEntity], row, primaryEntity))
				{
					this.SyncCommitment(sender, line);
				}
			}
		}

		protected override bool EraseCommitment(PXCache sender, object row)
		{
			ICommitmentSource poline = (ICommitmentSource)row;
			POOrder order = (POOrder)PXParentAttribute.SelectParent(sender.Graph.Caches[detailEntity], row, typeof(POOrder));

			//If commitment is not applicable
			if (poline.TaskID == null || order.OrderType == POOrderType.Blanket || order.OrderType == POOrderType.StandardBlanket)
			{
				return true;
			}

			//If line or order is cancelled and no receipts or bills are associated with this line - commitment is erazed.
			if ((poline.Cancelled == true || order.Cancelled == true) && poline.ReceivedQty == 0 && poline.BilledQty == 0 && poline.BilledAmt == 0)
			{
				return true;
			}

			//When OnHold or Unapproved - delete commitment unless it is locked.
			if (order.Hold == true || order.Approved != true)
			{
				//LockCommitment is used to modify PO by puting it on Hold without loosing the PMCommitment.
				if (order.LockCommitment == true)
				{
					return false;
				}

				return true;
			}

			return GetAccountGroup(sender, row) == null;
		}
		
		protected override PMCommitment FromRecord(PXCache sender, object row)
		{
			ICommitmentSource poline = (ICommitmentSource) row;
			POOrder order = (POOrder)PXParentAttribute.SelectParent(sender.Graph.Caches[detailEntity], row, typeof(POOrder));

			PMCommitment commitment = new PMCommitment();
			commitment.Type = PMCommitmentType.Internal;
			commitment.Status = CommitmentStatusFromSource(poline);
			commitment.CommitmentID = poline.CommitmentID ?? Guid.NewGuid();
			commitment.AccountGroupID = GetAccountGroup(sender, row);
			commitment.ProjectID = poline.ProjectID;
			commitment.ProjectTaskID = poline.TaskID;
			commitment.UOM = poline.UOM;
			if (poline.OrigExtCost == null)
			{
				commitment.OrigQty = poline.OrderQty.GetValueOrDefault();
			}
			else
			{
				commitment.OrigQty = poline.OrigOrderQty.GetValueOrDefault();
			}
			commitment.Qty = poline.OrderQty;

			IProjectMultiCurrency mc = ServiceLocator.Current.GetInstance<IProjectMultiCurrency>();
			PMProject project = PMProject.PK.Find(sender.Graph, commitment.ProjectID);

			decimal? origAmount;
			if (poline.OrigExtCost == null)
			{
				origAmount = poline.CuryExtCost + poline.CuryRetainageAmt.GetValueOrDefault();
			}
			else
			{
				origAmount = poline.OrigExtCost;
			}
			decimal? amount = poline.CuryExtCost + poline.CuryRetainageAmt.GetValueOrDefault();
			decimal? billedAmount = poline.CuryBilledAmt;

			commitment.OrigAmount = mc.GetValueInProjectCurrency(sender.Graph, project, order.CuryID, order.OrderDate, origAmount);
			commitment.Amount = mc.GetValueInProjectCurrency(sender.Graph, project, order.CuryID, order.OrderDate, amount);
			commitment.InvoicedAmount = mc.GetValueInProjectCurrency(sender.Graph, project, order.CuryID, order.OrderDate, billedAmount);

			commitment.ReceivedQty = poline.CompletedQty;
			commitment.InvoicedQty = poline.BilledQty;
			
			commitment.OpenQty = commitment.Status == PMCommitmentStatus.Open ? CalculateOpenQty(commitment, poline.CompletePOLine) : 0;
			commitment.OpenAmount = commitment.Status == PMCommitmentStatus.Open ? CalculateOpenAmount(commitment, poline.CompletePOLine) : 0;

			commitment.RefNoteID = order.NoteID;
			commitment.InventoryID = poline.InventoryID ?? PMInventorySelectorAttribute.EmptyInventoryID;
			commitment.CostCodeID = poline.CostCodeID ?? CostCodeAttribute.GetDefaultCostCode();
			commitment.BranchID = poline.BranchID;

			return commitment;
		}

		private string CommitmentStatusFromSource(ICommitmentSource poline)
        {
			if (poline.Closed == true)
				return PMCommitmentStatus.Closed;
			else if (poline.Cancelled == true)
				return PMCommitmentStatus.Canceled;
			else
				return PMCommitmentStatus.Open;
		}

        protected virtual decimal CalculateReceivedAmount(PMCommitment commitment)
        {
			if (commitment.Qty.GetValueOrDefault() == 0)
			{
				return 0;
			}
			else
			{
				return (commitment.Amount.GetValueOrDefault() / commitment.Qty.GetValueOrDefault()) * commitment.ReceivedQty.GetValueOrDefault();
			}
        }

		protected virtual decimal CalculateUnbilledAmount(PMCommitment commitment)
        {
			return commitment.Amount.GetValueOrDefault() - commitment.InvoicedAmount.GetValueOrDefault();
        }

		protected virtual Decimal CalculateOpenQty(PMCommitment commitment, string completeMethod)
		{
			if (completeMethod == CompletePOLineTypes.Amount)
				return CalculateOpenQtyByAmount(commitment);
			else
				return CalculateOpenQtyByQuantity(commitment);

		}

		protected virtual decimal CalculateOpenQtyByAmount(PMCommitment commitment)
        {
			return Math.Min(commitment.Qty.GetValueOrDefault() - commitment.ReceivedQty.GetValueOrDefault(), 
				commitment.Qty.GetValueOrDefault() - commitment.InvoicedQty.GetValueOrDefault() );
		}

		protected virtual decimal CalculateOpenQtyByQuantity(PMCommitment commitment)
		{
			return CalculateOpenQtyByAmount(commitment);
		}
	

		protected virtual Decimal CalculateOpenAmount(PMCommitment commitment, string completeMethod)
		{
			if (completeMethod == CompletePOLineTypes.Amount)
				return CalculateOpenAmountByAmount(commitment);
			else
				return CalculateOpenAmountByQuantity(commitment);

		}

		protected decimal CalculateOpenAmountByAmount(PMCommitment commitment)
		{
			decimal receivedAmount = CalculateReceivedAmount(commitment);
			decimal unbilledAmount = CalculateUnbilledAmount(commitment);
			decimal balancedAmount = commitment.Amount.GetValueOrDefault() - receivedAmount;

			if (balancedAmount < 0 && unbilledAmount <= 0)
				return Math.Max(balancedAmount, unbilledAmount);

			return Math.Min(balancedAmount, unbilledAmount);
		}

		protected decimal CalculateOpenAmountByQuantity(PMCommitment commitment)
		{
			if (commitment.Qty.GetValueOrDefault() == 0)
			{
				decimal unbilledAmount = CalculateUnbilledAmount(commitment);
				return Math.Min(0, unbilledAmount);
			}
            else
            {
				return (commitment.Amount.GetValueOrDefault() / commitment.Qty.GetValueOrDefault())
					* Math.Min(
						commitment.Qty.GetValueOrDefault() - commitment.ReceivedQty.GetValueOrDefault(),
						commitment.Qty.GetValueOrDefault() - commitment.InvoicedQty.GetValueOrDefault());
            }
		}

		protected override int? GetAccountGroup(PXCache sender, object row)
		{
			ICommitmentSource poline = (ICommitmentSource)row;
			InventoryItem item = InventoryItem.PK.Find(sender.Graph, poline.InventoryID);
			if (item != null && item.StkItem == true && item.COGSAcctID != null)
			{
				return GetAccountGroupFromAccountID(sender, item.COGSAcctID);
			}
			else
			{
				return GetAccountGroupFromAccountID(sender, poline.ExpenseAcctID);
			}
		}

		private int? GetAccountGroupFromAccountID(PXCache sender, int? accountID)
        {
			Account account = Account.PK.Find(sender.Graph, accountID);
			if (account != null && account.AccountGroupID != null)
			{
				return account.AccountGroupID;
			}

			return null;
		}

		protected override bool IsCommitmentSyncRequired(PXCache sender, object row, object oldRow)
		{
			return IsCommitmentSyncRequired((ICommitmentSource) row, (ICommitmentSource)oldRow);
		}

		private bool IsCommitmentSyncRequired(ICommitmentSource row, ICommitmentSource oldRow)
		{
			return row.OrderQty != oldRow.OrderQty
				|| row.ExtCost != oldRow.ExtCost
				|| row.BilledQty != oldRow.BilledQty
				|| row.ReceivedQty != oldRow.ReceivedQty
				|| row.CompletedQty != oldRow.CompletedQty
				|| row.BilledQty != oldRow.BilledQty
				|| row.BilledAmt != oldRow.BilledAmt
				|| row.ProjectID != oldRow.ProjectID
				|| row.TaskID != oldRow.TaskID
				|| row.ExpenseAcctID != oldRow.ExpenseAcctID
				|| row.InventoryID != oldRow.InventoryID
				|| row.CostCodeID != oldRow.CostCodeID
				|| row.UOM != oldRow.UOM
				|| row.Completed != oldRow.Completed
				|| row.Cancelled != oldRow.Cancelled
				|| row.Closed != oldRow.Closed;
		}

		protected virtual bool IsCommitmentSyncRequired(PXCache sender, POOrder row, POOrder oldRow)
		{
			bool? originalApproved = (bool?)sender.GetValueOriginal<POOrder.approved>(row); //This hack is required cause when the document is pre-approved by EPApprovalAutomation the RowUpdated event is not fired. 

			return row.Hold != oldRow.Hold || row.Cancelled != oldRow.Cancelled || row.Approved != oldRow.Approved || (row.Approved == true && originalApproved != true);
		}


		protected override bool IsCommitmentTrackingEnabled(PXCache sender)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
				return false;

			PMSetup setup = PXSelect<PMSetup>.Select(sender.Graph);

			if (setup == null)
				return false;

			return setup.CostCommitmentTracking == true;
		}

	}
}
