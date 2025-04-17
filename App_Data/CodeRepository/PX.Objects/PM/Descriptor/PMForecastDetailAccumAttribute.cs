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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	public class PMForecastHistoryAccumAttribute : PXAccumulatorAttribute
	{
		public PMForecastHistoryAccumAttribute()
		{
			base._SingleRecord = true;
		}
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			PMForecastHistory item = (PMForecastHistory)row;

			columns.Update<PMForecastHistory.actualQty>(item.ActualQty, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMForecastHistory.curyActualAmount>(item.CuryActualAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMForecastHistory.actualAmount>(item.ActualAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMForecastHistory.changeOrderQty>(item.ChangeOrderQty, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMForecastHistory.curyChangeOrderAmount>(item.CuryChangeOrderAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMForecastHistory.draftChangeOrderQty>(item.DraftChangeOrderQty, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMForecastHistory.curyDraftChangeOrderAmount>(item.CuryDraftChangeOrderAmount, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMForecastHistory.curyArAmount>(item.CuryArAmount, PXDataFieldAssign.AssignBehavior.Summarize);

			return true;
		}


	}
}
