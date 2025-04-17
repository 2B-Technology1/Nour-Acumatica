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

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Due to PX.Data.Parent not triggering a formula to update, add this to AMEstimateOper fields where the change of value should re-trigger estimate overhead row formulas
    /// </summary>
    public class TriggerEstimateOvhdFormulaAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        public void FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var amEstimateOper = (AMEstimateOper)e.Row;
            if (amEstimateOper == null)
            {
                return;
            }

            foreach (AMEstimateOvhd estimateOvhd in PXSelect<AMEstimateOvhd, Where<AMEstimateOvhd.estimateID,
                Equal<Required<AMEstimateOper.estimateID>>, And<AMEstimateOvhd.revisionID, Equal<Required<AMEstimateOper.revisionID>>,
                    And<AMEstimateOvhd.operationID, Equal<Required<AMEstimateOper.operationID>>
                    >>>>.Select(cache.Graph, amEstimateOper.EstimateID, amEstimateOper.RevisionID, amEstimateOper.OperationID))
            {
                cache.Graph.Caches<AMEstimateOvhd>().RaiseFieldUpdated<AMEstimateOvhd.overheadCostRate>(estimateOvhd, estimateOvhd.OverheadCostRate);
            }
            bool IsReadOnly = cache.GetStatus(e.Row) == PXEntryStatus.Notchanged;
            PXFormulaAttribute.CalcAggregate<AMEstimateOvhd.variableOvhdOperCost>(cache.Graph.Caches<AMEstimateOvhd>(), e.Row, IsReadOnly);
            cache.RaiseFieldUpdated<AMEstimateOper.variableOverheadCalcCost>(e.Row, null);
        }
    }
}
