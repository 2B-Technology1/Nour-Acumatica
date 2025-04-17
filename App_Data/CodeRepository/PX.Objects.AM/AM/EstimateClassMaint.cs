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

namespace PX.Objects.AM
{
    public class EstimateClassMaint : PXGraph<EstimateClassMaint, AMEstimateClass>
    {
        public PXSelect<AMEstimateClass> EstimateClassRecords;

        protected virtual void AMEstimateClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var estimateClass = (AMEstimateClass)e.Row;
            if (estimateClass == null)
            {
                return;
            }

            AMEstimateItem amEstimateItem = PXSelect<AMEstimateItem, 
                Where<AMEstimateItem.estimateClassID, Equal<Required<AMEstimateClass.estimateClassID>>>>.Select(this, estimateClass.EstimateClassID);
            if (amEstimateItem == null)
            {
                return;
            }
            sender.RaiseExceptionHandling<AMEstimateClass.estimateClassID>(
                estimateClass,
                estimateClass.EstimateClassID,
                new PXSetPropertyException(Messages.GetLocal(Messages.EstimateClass_NotDeleted,
                    amEstimateItem.EstimateID, amEstimateItem.RevisionID), PXErrorLevel.Error)
            );
            e.Cancel = true;
        }
    }
}