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

using System;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.AR
{
	public sealed class ARTranMultipleBaseCurrencies : PXCacheExtension<ARTran>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		#region BranchBaseCuryID
		public new abstract class branchBaseCuryID : PX.Data.BQL.BqlString.Field<branchBaseCuryID> { }

		[PXString]
		[PXFormula(typeof(Selector<ARTran.branchID, Branch.baseCuryID>))]
		public string BranchBaseCuryID { get; set; }
		#endregion

		#region CuryInventoryID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(Search<InventoryItemCurySettings.inventoryID,
			Where<InventoryItemCurySettings.inventoryID, Equal<ARTran.inventoryID.FromCurrent>,
				And<InventoryItemCurySettings.curyID, Equal<branchBaseCuryID.FromCurrent>>>>),
			ValidateValue = false)]
		public int? CuryInventoryID { get; set; }
		#endregion
	}
}
