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

using PX.Common;

namespace PX.Objects.IN.Turnover
{
	[PXLocalizable]
	public static class Messages
	{
		public const string INTurnoverCalcFilter = "Manage Turnover History Filter";
		public const string INTurnoverEnqFilter = "Inventory Turnover Filter";
		public const string INTurnoverCalc = "Turnover Calculation";
		public const string INTurnoverCalcItem = "Turnover Calculation Item";

		public const string InventoryLinkFilter = "Inventory List Filter";

		#region Actions

		public const string NonePlaceholder = "<SELECT>";
		public const string ListPlaceholder = "<LIST>";

		public const string CalculateTurnover = "Calculate Turnover";
		public const string DeleteTurnover = "Delete Records";

		#endregion

		#region CalculatedBy

		public const string CalculateByPeriod = "Period";
		public const string CalculateByYear = "Year";
		public const string CalculateByRange = "Selected Range";

		#endregion

		#region INTurnoverCalcItem fields

		public const string BeginningInventory = "Beginning Inventory";
		public const string BeginningInventoryCurrency = "Beginning Inventory ({0})";
		public const string EndingInventory = "Ending Inventory";
		public const string EndingInventoryCurrency = "Ending Inventory ({0})";
		public const string AverageInventory = "Average Inventory";
		public const string AverageInventoryCurrency = "Average Inventory ({0})";
		public const string CostOfGoodsSold = "Cost of Goods Sold";
		public const string CostOfGoodsSoldCurrency = "Cost of Goods Sold ({0})";
		public const string TurnoverRatio = "Turnover Ratio";
		public const string TurnoverRatioCurrency = "Turnover Ratio ({0})";

		#endregion

		#region Error and Warning messages

		public const string CannotReplaceFullTurnoverOfBranch =	 "The turnover cannot be recalculated for the current selection because it has been calculated for all warehouses and inventory items in this period range. To recalculate the turnover, clear the values in the Warehouse, Item Class, and Inventory ID boxes, and click Calculate Turnover.";
		public const string CannotReplaceFullTurnoverOfCompany = "The turnover cannot be recalculated for the current selection because it has been calculated for all warehouses and inventory items of at least one branch in this period range. To recalculate the turnover, clear the values in the Warehouse, Item Class, and Inventory ID boxes, and click Calculate Turnover.";

		public const string BranchTurnoverHasNotBeenCalculated =   "The turnover for the {0} - {1} period range has not been calculated yet. To review the turnover, click Calculate Turnover.";
		public const string CompanyTurnoversHasNotBeenCalculated = "The turnover for the {0} - {1} period range has not been calculated for all branches of the company. To review the turnover for the company, click Calculate Turnover.";
		public const string GroupTurnoversHasNotBeenCalculated =   "The turnover for the {0} - {1} period range has not been calculated for all companies of the group. To review the turnover for the group of companies, click Calculate Turnover.";

		public const string CompanyTurnoversHaveBeenFiltered =  "The turnover calculations for some branches of the {0} company have been filtered by one or more of the following parameters: warehouse, item class, or inventory ID. The data cannot be shown. To review the turnover for the selected company, click Calculate Turnover.";
		public const string GroupTurnoversHaveBeenFiltered =	"The turnover calculations for some branches of the {0} group of companies have been filtered by one or more of the following parameters: warehouse, item class, or inventory ID. The data cannot be shown. To review the turnover for the selected group of companies, click Calculate Turnover.";

		public const string IsSiteCalc =			"The turnover for the selected period range has been calculated for the following warehouse: {0}. The calculated data is not complete for the current value of the Warehouse box. To calculate the turnover for the current value, click Calculate Turnover.";
		public const string IsItemClassCalc =		"The turnover for the selected period range has been calculated for the following item class: {0}.The calculated data is not complete for the current value of the Item Class box. To calculate the turnover for the current value, click Calculate Turnover.";
		public const string IsInventoryCalc =		"The turnover for the selected period range has been calculated for the following inventory ID: {0}. The calculated data is not complete for the current value of the Inventory ID box. To calculate the turnover for the current value, click Calculate Turnover.";
		public const string IsInventoryListCalc =	"The turnover for the selected period range has been calculated for the list of inventory IDs. The calculated data is not complete for the current list of the inventory IDs. To calculate the turnover for the current list, click Calculate Turnover.";

		#endregion
	}
}
