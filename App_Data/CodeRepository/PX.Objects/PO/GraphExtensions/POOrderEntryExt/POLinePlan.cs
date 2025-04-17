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
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.PO.GraphExtensions.POOrderEntryExt
{
	public class POLinePlan : POLinePlan<POOrderEntry>
	{
	}

	public abstract class POLinePlan<TGraph> : POLinePlanBase<TGraph, POLine>
		where TGraph : PXGraph
	{
		public override void _(Events.RowUpdated<POOrder> e)
		{
			base._(e);

			if (!e.Cache.ObjectsEqual<POOrder.status, POOrder.cancelled>(e.Row, e.OldRow))
			{
				foreach (INItemPlan plan in PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<POOrder.noteID>>>>.Select(Base))
				{
					if (e.Row.Cancelled == true)
					{
						PlanCache.Delete(plan);
					}
					else
					{
						INItemPlan copy = PXCache<INItemPlan>.CreateCopy(plan);

						bool isOnHold = IsOrderOnHold(e.Row);
						if (TryCalcPlanType(plan, isOnHold, out string newPlanType))
						{
							plan.PlanType = newPlanType;
						}

						plan.Hold = isOnHold;

						if (!string.Equals(copy.PlanType, plan.PlanType))
							PlanCache.RaiseRowUpdated(plan, copy);

						PlanCache.MarkUpdated(plan, assertError: true);
					}
				}
			}
		}

		protected virtual bool TryCalcPlanType(INItemPlan plan, bool isOnHold, out string newPlanType)
		{
			newPlanType = null;
			switch (plan.PlanType)
			{
				case INPlanConstants.PlanM3:
				case INPlanConstants.PlanM4:
					newPlanType = isOnHold ? INPlanConstants.PlanM3 : INPlanConstants.PlanM4;
					break;
				case INPlanConstants.Plan70:
				case INPlanConstants.Plan73:
					newPlanType = isOnHold ? INPlanConstants.Plan73 : INPlanConstants.Plan70;
					break;
				case INPlanConstants.Plan76:
				case INPlanConstants.Plan78:
					newPlanType = isOnHold ? INPlanConstants.Plan78 : INPlanConstants.Plan76;
					break;
				case INPlanConstants.Plan74:
				case INPlanConstants.Plan79:
					newPlanType = isOnHold ? INPlanConstants.Plan79 : INPlanConstants.Plan74;
					break;
				case INPlanConstants.PlanF7:
				case INPlanConstants.PlanF8:
					newPlanType = isOnHold ? INPlanConstants.PlanF8 : INPlanConstants.PlanF7;
					break;
			}
			return newPlanType != null;
		}
	}
}
