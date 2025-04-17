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

namespace PX.Objects.IN.InventoryRelease.Accumulators.Statistics.Item
{
	using static PXDataFieldAssign.AssignBehavior;

	public class ItemStatsAccumulatorAttribute : PXAccumulatorAttribute
	{
		public Type LastCostDateField { get; set; }
		public Type LastCostField { get; set; }
		public Type MinCostField { get; set; }
		public Type MaxCostField { get; set; }
		public Type QtyOnHandField { get; set; }
		public Type TotalCostField { get; set; }
		public Type LastPurchasedDateField { get; set; }

		public ItemStatsAccumulatorAttribute()
		{
			SingleRecord = true;
		}

		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
				return false;

			if (!sender.IsKeysFilled(row))
				return false;

			var lastCostDate = (DateTime?)sender.GetValue(row, LastCostDateField.Name);
			if (lastCostDate == INItemStats.MinDate.get())
			{
				sender.SetValue(row, LastCostField.Name, null);
				columns.Update(LastCostField, 0m, Initialize);
				// It is needed for correct work of DBLastChangedDateTime.
				columns.Update(LastCostDateField, Initialize);
			}
			else
			{
				columns.Update(LastCostField, Replace);
				columns.Update(LastCostDateField, Replace);
			}

			var minCost = (decimal?)sender.GetValue(row, MinCostField.Name);
			columns.Update(MinCostField, minCost, minCost == 0m ? Initialize : Minimize);

			columns.Update(MaxCostField, Maximize);
			columns.Update(QtyOnHandField, Summarize);
			columns.Update(TotalCostField, Summarize);

			if (LastPurchasedDateField != null)
			{
				var lastPurchasedDate = (DateTime?)sender.GetValue(row, LastPurchasedDateField.Name);

				if (lastPurchasedDate != null)
					columns.Update(LastPurchasedDateField, lastPurchasedDate, Maximize);
			}

			return true;
		}
	}
}
