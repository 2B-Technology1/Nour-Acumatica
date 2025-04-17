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

namespace PX.Objects.AM
{
    public static class DACExtensionMethods
    {
        public static bool OperationsEqual(this IProdOper oper1, IProdOper oper2)
        {
            return OperationsEqual(oper1, oper2?.OrderType, oper2?.ProdOrdID, oper2?.OperationID);
        }

        public static bool OperationsEqual(this IProdOper oper, string orderType, string prodOrdID, int? operationId)
        {
            return oper != null && 
                   oper.OrderType.EqualsWithTrim(orderType) &&
                   oper.ProdOrdID.EqualsWithTrim(prodOrdID) &&
                   oper.OperationID.GetValueOrDefault() == operationId.GetValueOrDefault();
        }

        public static decimal GetMachineUnitsPerHour(this IOperationMaster oper)
        {
            if (oper == null)
            {
                return 0m;
            }

            return oper.MachineUnitTime.GetValueOrDefault() == 0 ? 0m
                : UomHelper.Round(60m * oper.MachineUnits.GetValueOrDefault() / oper.MachineUnitTime.GetValueOrDefault(), 10);
        }

        public static decimal GetRunUnitsPerHour(this IOperationMaster oper)
        {
            if (oper == null)
            {
                return 0m;
            }

            return oper.RunUnitTime.GetValueOrDefault() == 0 ? 0m
                : UomHelper.Round(60m * oper.RunUnits.GetValueOrDefault() / oper.RunUnitTime.GetValueOrDefault(), 10);
        }

        /// <summary>
        /// Combine the key fields into a single string.
        /// </summary>
        public static string JoinKeys(this AMProdOper prodOper)
        {
            return prodOper == null
                ? string.Empty
                : string.Join("~", prodOper.OrderType.TrimIfNotNullEmpty(), prodOper.ProdOrdID.TrimIfNotNullEmpty(), prodOper.OperationID);
        }

        #region AMMTran

        /// <summary>
        /// Does the given <see cref="AMMTran"/> contain a reference to the given <see cref="AMProdMatl"/>
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="prodMatl"></param>
        /// <returns></returns>
        public static bool IsSameMatl(this AMMTran tran, AMProdMatl prodMatl)
        {
            return tran?.MatlLineId != null && prodMatl?.LineID != null && tran.OrderType == prodMatl.OrderType &&
                   tran.ProdOrdID == prodMatl.ProdOrdID && tran.OperationID == prodMatl.OperationID &&
                   tran.MatlLineId == prodMatl.LineID;
        }

        /// <summary>
        /// Is this transaction the original to the given transaction
        /// </summary>
        public static bool IsTransactionOriginal(this AMMTran tran1, AMMTran tran2)
        {
            return tran1 != null && tran2 != null &&
                   tran1.DocType == tran2.OrigDocType &&
                   tran1.BatNbr == tran2.OrigBatNbr &&
                   tran1.LineNbr == tran2.OrigLineNbr;
        }

#if DEBUG
        // do not use JoinKeys for AMMTran because that is an instance method name used on AMMTran that we are making obsolete 
#endif
        /// <summary>
        /// Combine the key fields into a single string.
        /// </summary>
        public static string JoinDacKeys(this AMMTran row)
        {
            return row == null
                ? string.Empty
                : string.Join("~", row.DocType.TrimIfNotNullEmpty(), row.BatNbr.TrimIfNotNullEmpty(), row.LineNbr);
        }

        #endregion

        #region AMProdMatl

        /// <summary>
        /// Gets the total required qty for a given order qty with a rounded value by the decimal places configured for the branch/company
        /// </summary>
        public static decimal GetTotalReqQtyCompanyRounded(this AMProdMatl prodMatl, decimal totalOrderQty)
        {
            return UomHelper.QuantityRound(GetTotalReqQty(prodMatl, totalOrderQty));
        }

        /// <summary>
        /// Gets the total required qty for a given order qty
        /// </summary>
        public static decimal GetTotalReqQty(this AMProdMatl prodMatl, decimal totalOrderQty)
        {
            return GetTotalReqQty(prodMatl, totalOrderQty, prodMatl?.QtyRoundUp == true);
        }

        /// <summary>
        /// Gets the total required qty for a given order qty
        /// </summary>
        public static decimal GetTotalReqQty(this AMProdMatl prodMatl, decimal totalOrderQty, bool roundUp)
        {
            return AMProdMatl.GetTotalRequiredQty(prodMatl, totalOrderQty, roundUp);
        }

        /// <summary>
        /// Gets the total base required qty for a given order qty
        /// </summary>
        public static decimal GetTotalBaseReqQty(this AMProdMatl prodMatl, decimal totalOrderQty)
        {
            return GetTotalBaseReqQty(prodMatl, totalOrderQty, prodMatl?.QtyRoundUp == true);
        }

        /// <summary>
        /// Gets the total base required qty for a given order qty
        /// </summary>
        public static decimal GetTotalBaseReqQty(this AMProdMatl prodMatl, decimal totalOrderQty, bool roundUp)
        {
            return AMProdMatl.GetTotalBaseRequiredQty(prodMatl, totalOrderQty, roundUp);
        }

        #endregion

        /// <summary>
        /// Map extension fields from InventoryItemExt to INItemSiteExt
        /// </summary>
        public static void MapAMExtensionFields(this CacheExtensions.INItemSiteExt toExtension, CacheExtensions.InventoryItemExt fromExtension)
        {
            if (fromExtension == null || toExtension == null)
            {
                return;
            }

            toExtension.AMMinOrdQty = fromExtension.AMMinOrdQty.GetValueOrDefault();
            toExtension.AMMaxOrdQty = fromExtension.AMMaxOrdQty.GetValueOrDefault();
            toExtension.AMLotSize = fromExtension.AMLotSize.GetValueOrDefault();
            toExtension.AMMFGLeadTime = fromExtension.AMMFGLeadTime.GetValueOrDefault();
            toExtension.AMGroupWindow = fromExtension.AMGroupWindow.GetValueOrDefault();
            toExtension.AMSafetyStock = fromExtension.AMSafetyStock.GetValueOrDefault();
            toExtension.AMMinQty = fromExtension.AMMinQty.GetValueOrDefault();
            toExtension.AMSourceSiteID = fromExtension.AMSourceSiteID.GetValueOrDefault();

            // Scrap Warehouse and Location
            toExtension.AMScrapSiteID = fromExtension.AMScrapSiteID;
            toExtension.AMScrapLocationID = fromExtension.AMScrapLocationID;
        }

		/// <summary>
		/// Matching OrderType and ProdOrdID
		/// </summary>
		public static bool IsSameOrder(this IProdOrder order1, IProdOrder order2)
		{
			return order1 != null && order2 != null && order1.OrderType == order2.OrderType && order1.ProdOrdID == order2.ProdOrdID;
		}
		
		public static bool SameBOM(this IBomRevision bom1, IBomRevision bom2)
		{
			return bom1 != null && bom2 != null && bom1.BOMID == bom2.BOMID && bom1.RevisionID == bom2.RevisionID;
		}

		public static bool SameBOMOper(this IBomOper bomOper1, IBomOper bomOper2)
		{
			return bomOper1 != null && bomOper2 != null && bomOper2.BOMID == bomOper1.BOMID && bomOper1.RevisionID == bomOper2.RevisionID && bomOper1.OperationID == bomOper2.OperationID;
		}
    }
}
