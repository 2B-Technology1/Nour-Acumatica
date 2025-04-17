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
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.GL;

namespace PX.Objects.Common.GraphExtensions.Abstract
{
	public abstract class InvoiceGraphExtension<TGraph, TAdjust> : InvoiceBaseGraphExtension<TGraph, Invoice, InvoiceMapping>
        where TGraph : PXGraph
        where TAdjust : class, IBqlTable, IFinAdjust, new()
    {
        public abstract PXSelectBase<TAdjust> AppliedAdjustments { get; }

        protected override void _(Events.RowUpdated<Invoice> e)
        {
            base._(e);

            if (ShouldUpdateAdjustmentsOnDocumentUpdated(e))
            {
                foreach (TAdjust adjust in AppliedAdjustments.Select())
                {
                    if (!e.Cache.ObjectsEqual<Invoice.branchID>(e.Row, e.OldRow))
                    {
                        AppliedAdjustments.Cache.SetDefaultExt<Adjust.adjdBranchID>(adjust);
                    }

                    if (!e.Cache.ObjectsEqual<Invoice.headerTranPeriodID>(e.Row, e.OldRow))
                    {
                        FinPeriodIDAttribute.DefaultPeriods<Adjust.adjgFinPeriodID>(AppliedAdjustments.Cache, adjust);
                        FinPeriodIDAttribute.DefaultPeriods<Adjust.adjdFinPeriodID>(AppliedAdjustments.Cache, adjust);
                    }

                    (AppliedAdjustments.Cache as PXModelExtension<Adjust>)?.UpdateExtensionMapping(adjust);

                    AppliedAdjustments.Cache.MarkUpdated(adjust);
                }
            }
        }

        #region Adjustments

        protected virtual bool ShouldUpdateAdjustmentsOnDocumentUpdated(Events.RowUpdated<Invoice> e)
        {
            return ShouldUpdateDetailsOnDocumentUpdated(e);
        }

        #endregion
    }
}
