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
using PX.Objects.AM.Attributes;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    public class ProductionStatus
    {
        public static bool IsEditableStatus(AMProdItem amProdItem)
        {
            if (amProdItem == null)
            {
                return false;
            }

			return (amProdItem.Released == false || amProdItem.Hold == true)
						&& amProdItem.Canceled == false && amProdItem.Completed == false && amProdItem.Closed == false;
        }

        public static bool IsOpenOrder(AMProdItem prodItem)
        {
			return prodItem.Released == true && prodItem.Canceled == false && prodItem.Completed == false;
        }

        public static bool CanPlanAllocations(AMProdItem prodItem)
        {
            if (prodItem == null)
            {
                return false;
            }

			return prodItem.Released == true && prodItem.Completed == false && prodItem.Canceled == false;
        }

        /// <summary>
        /// Does the given production order allow for a transaction entry/process release
        /// </summary>
        public static bool IsValidTransactionStatus(PXGraph graph, string orderType, string productionOrderNbr)
        {
            if (string.IsNullOrWhiteSpace(orderType) || string.IsNullOrWhiteSpace(productionOrderNbr))
            {
                return false;
            }

            return IsValidTransactionStatus(PXSelect<AMProdItem,
                Where<AMProdItem.orderType, Equal<Required<AMProdItem.orderType>>,
                    And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>.Select(graph, orderType, productionOrderNbr));
        }

        /// <summary>
        /// Does the given production order allow for a transaction entry/process release
        /// </summary>
        public static bool IsValidTransactionStatus(AMProdItem prodItem)
        {
            if (prodItem == null || string.IsNullOrWhiteSpace(prodItem.ProdOrdID))
            {
                return false;
            }

            if (prodItem.Function == null
                || prodItem.Function != OrderTypeFunction.Regular
                || prodItem.Hold.GetValueOrDefault())
            {
                return false;
            }

			return prodItem.Closed == false && prodItem.Canceled == false && prodItem.Locked == false;
        }

        public static bool IsReleasedTransactionStatus(AMProdItem prodItem)
        {
			if (prodItem == null || prodItem.Function == null || string.IsNullOrWhiteSpace(prodItem.StatusID)
				|| (prodItem.Function != OrderTypeFunction.Regular && prodItem.Function != OrderTypeFunction.Disassemble)
				|| prodItem.Hold == true)
			{
				return false;
			}
			return prodItem.IsOpen == true;
        }

        public static bool IsClosedOrCanceled(AMProdItem amProdItem)
        {
            if (amProdItem == null
                || string.IsNullOrWhiteSpace(amProdItem.ProdOrdID))
            {
                return true;
            }

			return amProdItem.Closed == true || amProdItem.Canceled == true;
        }

        #region valid production status description (return status description if invalid)
        /// <summary>
        /// Public used within Manufacturing Shared for other verify production status methods
        /// </summary>
        /// <param name="aAMProdItem">AMProdItem DAC</param>
        /// <param name="PlanStatusValid">Is a plan status a valid status for transactions</param>
        /// <returns>If string not empty it will be the full status description that is an invalid status</returns>
        public static string ValidStatusDescription(AMProdItem aAMProdItem, bool PlanStatusValid)
        {
            var curProdStatusDescription = string.Empty;
            var validOrderTypes = "RIM";

            if (PlanStatusValid)
            {
                validOrderTypes += "P";
            }

			//ProdStatusInvalidForTransaction
			if (aAMProdItem != null)
            {
                if (aAMProdItem.Hold == true)
                {   //On Hold
                    curProdStatusDescription = Messages.Hold;
                }
                else
                {   //If NOT Released; In Process; Completed (M)
                    if (!(validOrderTypes.Contains(aAMProdItem?.StatusID.Trim())))
                    {
                        curProdStatusDescription = ProductionOrderStatus.GetStatusDescription(aAMProdItem?.StatusID.Trim());
                    }
                }
            }
            else
            {
                //Should never be null unless it was deleted
                curProdStatusDescription = Messages.Deleted;
            }

            return curProdStatusDescription;
        }
		#endregion

		public static void RaiseProdItemEvent(PXGraph graph, AMProdItem doc, PX.Data.WorkflowAPI.SelectedEntityEvent<AMProdItem> prodEvent)
		{
			prodEvent.FireOn(graph, doc);
			graph.Caches[typeof(AMProdItem)].Update(doc);
		}
	}
}
