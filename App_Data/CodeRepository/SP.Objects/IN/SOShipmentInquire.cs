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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.SO;

namespace SP.Objects.IN
{
    public class SOShipmentInquire : PXGraph<SOShipmentInquire>
    {
        #region Constructor
        public SOShipmentInquire()
        {
            PXUIFieldAttribute.SetVisible<SOShipment.shipmentNbr>(shipmentlist.Cache, shipmentlist.Cache.Current);
            PXUIFieldAttribute.SetVisible<SOOrder.orderNbr>(SOOrderView.Cache, SOOrderView.Cache.Current, false);
        }
        #endregion

        #region Selector
        public PXSelect<SOOrder> SOOrderView;
        
        public PXSelectJoin<SOOrderShipment, 
            LeftJoin<SOShipment, 
            On<SOShipment.shipmentNbr, Equal<SOOrderShipment.shipmentNbr>, 
                And<SOShipment.shipmentType, Equal<SOOrderShipment.shipmentType>>>>, 
            Where<SOOrderShipment.orderType, Equal<Current<SOOrder.orderType>>,
                And<SOOrderShipment.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> shipmentlist;
        #endregion


        #region Actions
        public PXAction<SOOrder> ViewShipment;
        [PXUIField(DisplayName = "View Shipment")]
        [PXButton(Tooltip = "View Shipment")]
        public virtual IEnumerable viewShipment(PXAdapter adapter)
        {
            if (this.shipmentlist.Current != null)
            {
                SOOrderShipment rowcurrent = this.shipmentlist.Current;
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["SOShipment.ShipmentNbr"] = rowcurrent.ShipmentNbr;
                throw new PXReportRequiredException(parameters, "SO642000", null);
            }
            return adapter.Get();
        }
        #endregion

        #region Handler
        protected virtual void SOOrderShipment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            SOOrderShipment row = e.Row as SOOrderShipment;
            PXUIFieldAttribute.SetVisible<SOOrderShipment.shipmentNbr>(shipmentlist.Cache, row);
            PXUIFieldAttribute.SetVisible<SOOrder.orderNbr>(SOOrderView.Cache, SOOrderView.Cache.Current, false);
        }
        #endregion
    }
}
