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
using System.Linq;
using PX.Common;

namespace PX.Objects.CR
{
	[PXUnboundFormula(typeof(Switch<Case<Where<CRActivity.incoming, Equal<True>>, True>, False>), typeof(LastActivity<CRActivityStatistics.lastIncomingActivityNoteID, CRSMEmail.noteID>))]
	[PXUnboundFormula(typeof(Switch<Case<Where<CRActivity.incoming, Equal<True>>, True>, False>), typeof(LastActivity<CRActivityStatistics.lastIncomingActivityDate, CRSMEmail.createdDateTime>))]
	[PXUnboundFormula(typeof(Switch<Case<Where<CRActivity.outgoing, Equal<True>>, True>, False>), typeof(LastActivity<CRActivityStatistics.lastOutgoingActivityNoteID, CRSMEmail.noteID>))]
	[PXUnboundFormula(typeof(Switch<Case<Where<CRActivity.outgoing, Equal<True>>, True>, False>), typeof(LastActivity<CRActivityStatistics.lastOutgoingActivityDate, CRSMEmail.createdDateTime>))]
	[PXUnboundFormula(typeof(Switch<Case<
		Where<CRActivity.outgoing, Equal<True>, 
			And<CRActivity.completedDate, IsNotNull,
			And<CRActivity.uistatus,Equal<ActivityStatusAttribute.completed>>>>, True>, False>),typeof(LastActivity<CRActivityStatistics.initialOutgoingActivityCompletedAtDate, CRSMEmail.completedDate>))]
	[PXUnboundFormula(typeof(Switch<Case<
		Where<CRActivity.outgoing, Equal<True>,
			And<CRActivity.completedDate, IsNotNull,
			And<CRActivity.uistatus, Equal<ActivityStatusAttribute.completed>>>>, True>, False>),typeof(LastActivity<CRActivityStatistics.initialOutgoingActivityCompletedAtNoteID, CRSMEmail.noteID>))]
	public sealed class CRSMEmailStatisticFormulas : PXAggregateAttribute { }

	[PXUnboundFormula(typeof(Switch<Case<Where<CRActivity.incoming, Equal<True>>, True>, False>), typeof(LastActivity<CRActivityStatistics.lastIncomingActivityNoteID, CRActivity.noteID>))]
	[PXUnboundFormula(typeof(Switch<Case<Where<CRActivity.incoming, Equal<True>>, True>, False>), typeof(LastActivity<CRActivityStatistics.lastIncomingActivityDate, CRActivity.createdDateTime>))]
	[PXUnboundFormula(typeof(Switch<Case<Where<CRActivity.outgoing, Equal<True>>, True>, False>), typeof(LastActivity<CRActivityStatistics.lastOutgoingActivityNoteID, CRActivity.noteID>))]
	[PXUnboundFormula(typeof(Switch<Case<Where<CRActivity.outgoing, Equal<True>>, True>, False>), typeof(LastActivity<CRActivityStatistics.lastOutgoingActivityDate, CRActivity.createdDateTime>))]
	[PXUnboundFormula(typeof(Switch<Case<
		Where<CRActivity.outgoing, Equal<True>,
			And<CRActivity.completedDate, IsNotNull,
			And<CRActivity.uistatus, Equal<ActivityStatusAttribute.completed>>>>, True>, False>), typeof(LastActivity<CRActivityStatistics.initialOutgoingActivityCompletedAtDate, CRActivity.completedDate>))]
	[PXUnboundFormula(typeof(Switch<Case<
		Where<CRActivity.outgoing, Equal<True>,
			And<CRActivity.completedDate, IsNotNull,
			And<CRActivity.uistatus, Equal<ActivityStatusAttribute.completed>>>>, True>, False>), typeof(LastActivity<CRActivityStatistics.initialOutgoingActivityCompletedAtNoteID, CRActivity.noteID>))]
	public sealed class CRActivityStatisticFormulas : PXAggregateAttribute { }

	public sealed class LastActivity<TargetField, ReturnField> : IBqlUnboundAggregateCalculator
		where TargetField : IBqlField	
		where ReturnField : IBqlField
	{
		private static object CalcFormula(IBqlCreator formula, PXCache cache, object item)
		{
			object value = null;
			bool? result = null;

			BqlFormula.Verify(cache, item, formula, ref result, ref value);

			return value;
		}

		public object Calculate(PXCache cache, object row, IBqlCreator formula, object[] records, int digit)
		{
		    if (cache.Cached.Count() < 1) return null;

			bool isInitialCalc = 
				typeof(TargetField) == typeof(CRActivityStatistics.initialOutgoingActivityCompletedAtDate) ||
				typeof(TargetField) == typeof(CRActivityStatistics.initialOutgoingActivityCompletedAtNoteID);

			PXCache crActivityCache = cache.Graph.Caches[typeof(CRActivity)];
			
			if (row is CRActivity && (bool?)CalcFormula(formula, cache, row) == true && isInitialCalc == false)
			{
				if (crActivityCache == null) return cache.GetValue<ReturnField>(row);
				var val = getLastValue(crActivityCache, formula, records) ?? cache.GetValue<ReturnField>(row);
				return val;
			}

		    if (records.Length < 1 || !(records[0] is CRActivity)) return null;

		    if (crActivityCache == null) return null;

			if (isInitialCalc == true)
			{
				var value = crActivityCache.GetValue<ReturnField>(
					records.Where(a => ((bool?)CalcFormula(formula, crActivityCache, a) == true))
						.OrderBy(a => ((CRActivity)a).CompletedDate)
						.FirstOrDefault());
				return value;
			}

		    return getLastValue(crActivityCache, formula, records);
		}

		private object getLastValue(PXCache crActivityCache, IBqlCreator formula, object[] records)
		{
			return crActivityCache.GetValue<ReturnField>(
				records.Where(a => ((bool?)CalcFormula(formula, crActivityCache, a) == true))
					.OrderBy(a => ((CRActivity)a).CreatedDateTime)
						.LastOrDefault());
		}

		public object Calculate(PXCache cache, object row, object oldrow, IBqlCreator formula, int digit)
		{
			return null;
		}
	}
}
