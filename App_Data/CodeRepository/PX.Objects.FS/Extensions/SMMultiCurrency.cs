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
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.Extensions.MultiCurrency;

namespace PX.Objects.FS
{
	public abstract class SMMultiCurrencyGraph<TGraph, TPrimary> : MultiCurrencyGraph<TGraph, TPrimary>
        where TGraph : PXGraph
        where TPrimary : class, IBqlTable, new()
    {
		protected override string Module => GL.BatchModule.AR;

		protected override CurySourceMapping GetCurySourceMapping()
        {
            return new CurySourceMapping(typeof(Customer));
        }

        protected override void _(Events.RowSelected<Document> e)
        {
            base._(e);

            if (e.Row == null) return;

            PXUIFieldAttribute.SetVisible<Document.curyID>(e.Cache, e.Row, IsMultyCurrencyEnabled);
            switch (Documents.Cache.GetMain(e.Row))
            {
                case FSServiceOrder _:
					{
						ServiceOrderEntry graphServiceOrder = (ServiceOrderEntry)Documents.Cache.Graph;
						FSServiceOrder fsServiceOrderRow = graphServiceOrder.ServiceOrderRecords?.Current;

						bool isAllowedForInvoiceOrInvoiced = fsServiceOrderRow?.AllowInvoice == true || fsServiceOrderRow?.Billed == true;
						PXUIFieldAttribute.SetEnabled<Document.curyID>(e.Cache, e.Row, graphServiceOrder.ServiceOrderAppointments.Select().Count == 0
																						&& !isAllowedForInvoiceOrInvoiced);
                        return;
                    }
                case FSAppointment fsAppointmentRow:
                    {
                        PXUIFieldAttribute.SetEnabled<Document.curyID>(e.Cache, e.Row, fsAppointmentRow.SOID < 0);
                        return;
                    }
            }
        }

        public virtual bool IsMultyCurrencyEnabled
        {
            get { return PXAccess.FeatureInstalled<FeaturesSet.multicurrency>(); }
        }
    }
}
