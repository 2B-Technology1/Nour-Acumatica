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
using PX.Objects.PO;
using ScMessages = PX.Objects.CN.Subcontracts.SC.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts
{
    [PXCacheName("Subcontract Report Information")]
    public class SubcontractReportInformation : IBqlTable
    {
        [PXSelector(typeof(Search<POOrder.orderNbr,
                Where<POOrder.orderType, Equal<POOrderType.regularSubcontract>>,
                OrderBy<Desc<POOrder.orderNbr>>>),
            typeof(POOrder.orderNbr),
            typeof(POOrder.vendorID),
            typeof(POOrder.vendorLocationID),
            typeof(POOrder.orderDate),
            typeof(POOrder.status),
            typeof(POOrder.curyID),
            typeof(POOrder.vendorRefNbr),
            typeof(POOrder.curyOrderTotal),
            typeof(POOrder.lineTotal),
            typeof(POOrder.sOOrderType),
            typeof(POOrder.sOOrderNbr),
            typeof(POOrder.orderDesc),
            typeof(POOrder.ownerID),
            Headers = new[]
            {
                ScMessages.Subcontract.SubcontractNumber,
                ScMessages.Subcontract.Vendor,
                ScMessages.Subcontract.Location,
                ScMessages.Subcontract.Date,
                ScMessages.Subcontract.Status,
                ScMessages.Subcontract.Currency,
                ScMessages.Subcontract.VendorReference,
                ScMessages.Subcontract.SubcontractTotal,
                ScMessages.Subcontract.LineTotal,
                ScMessages.Subcontract.SalesOrderType,
                ScMessages.Subcontract.SalesOrderNumber,
                ScMessages.Subcontract.Description,
                ScMessages.Subcontract.Owner,
            })]
        public virtual int? SubcontractNumber
        {
            get;
            set;
        }
    }
}