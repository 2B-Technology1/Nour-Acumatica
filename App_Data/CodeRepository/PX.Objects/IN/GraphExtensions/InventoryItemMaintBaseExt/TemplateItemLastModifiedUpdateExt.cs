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
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.DAC.Accumulators;

namespace PX.Objects.IN.GraphExtensions.InventoryItemMaintBaseExt
{
	public abstract class TemplateItemLastModifiedUpdateExt<TGraph> : PXGraphExtension<TGraph>
		where TGraph : InventoryItemMaintBase
	{
		public PXSelect<TemplateItemLastModifiedUpdate> TemplateItemLastModifiedUpdate;

		protected virtual void _(Events.RowInserted<InventoryItem> eventArgs)
		{
			InsertAccumulatorRecord(eventArgs.Row);
		}

		protected virtual void _(Events.RowUpdated<InventoryItem> eventArgs)
		{
			InsertAccumulatorRecord(eventArgs.OldRow);
		}

		protected virtual void _(Events.RowDeleted<InventoryItem> eventArgs)
		{
			InsertAccumulatorRecord(eventArgs.Row);
		}

		protected virtual void InsertAccumulatorRecord(InventoryItem row)
		{
			if (row?.TemplateItemID != null)
			{
				TemplateItemLastModifiedUpdate.Insert(new TemplateItemLastModifiedUpdate()
				{
					InventoryID = row.TemplateItemID
				});
			}
		}
	}

	public class StockTemplateItemLastModifiedUpdateExt : TemplateItemLastModifiedUpdateExt<InventoryItemMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.matrixItem>();
		}
	}

	public class NonStockTemplateItemLastModifiedUpdateExt : TemplateItemLastModifiedUpdateExt<NonStockItemMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.matrixItem>();
		}
	}
}
