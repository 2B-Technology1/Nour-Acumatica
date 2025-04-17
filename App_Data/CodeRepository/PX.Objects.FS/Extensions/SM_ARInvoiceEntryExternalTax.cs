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

using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.CR;
using PX.Objects.IN;
using System.Linq;
using PX.Objects.AR;

namespace PX.Objects.FS
{
    public class SM_ARInvoiceEntryExternalTax : PXGraphExtension<ARInvoiceEntryExternalTax, ARInvoiceEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public delegate IAddressLocation GetFromAddressLineDelegate(ARInvoice invoice, ARTran tran);
        public delegate IAddressLocation GetToAddressLineDelegate(ARInvoice invoice, ARTran tran);

        [PXOverride]
        public virtual IAddressLocation GetFromAddress(ARInvoice invoice, ARTran tran, GetFromAddressLineDelegate del)
        {
            string srvOrderType;
            string refNbr;
            GetServiceOrderKeys(tran, out srvOrderType, out refNbr);
            IAddressLocation returnAddress = null;

            if (string.IsNullOrEmpty(refNbr) == false && tran.SiteID == null)
            {
                returnAddress = PXSelectJoin<FSAddress,
                                InnerJoin<
                                    FSBranchLocation,
                                    On<FSBranchLocation.branchLocationAddressID, Equal<FSAddress.addressID>>,
                                InnerJoin<FSServiceOrder,
                                    On<FSServiceOrder.branchLocationID, Equal<FSBranchLocation.branchLocationID>>>>,
                                Where<
                                    FSServiceOrder.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>,
                                    And<FSServiceOrder.refNbr, Equal<Required<FSServiceOrder.refNbr>>>>>
                                    .Select(Base, srvOrderType, refNbr)
                                    .RowCast<FSAddress>()
                                    .FirstOrDefault();
            }
            else if (string.IsNullOrEmpty(refNbr) == false && tran.SiteID != null)
            {
                returnAddress = PXSelectJoin<Address,
                                InnerJoin<INSite, On<Address.addressID, Equal<INSite.addressID>>>,
                                Where<
                                    INSite.siteID, Equal<Required<INSite.siteID>>>>
                                .Select(Base, tran.SiteID)
                                .RowCast<Address>()
                                .FirstOrDefault();
            }

            if (returnAddress != null)
            {
                return returnAddress;
            }
            else
            {
                return del(invoice, tran);
            }
        }

        [PXOverride]
        public virtual IAddressLocation GetToAddress(ARInvoice invoice, ARTran tran, GetToAddressLineDelegate del)
        {
            string srvOrderType;
            string refNbr;
            GetServiceOrderKeys(tran, out srvOrderType, out refNbr);

            if (string.IsNullOrEmpty(refNbr) == false)
            {
                IAddressLocation returnAddress = null;

                returnAddress = PXSelectJoin<FSAddress,
                                InnerJoin<FSServiceOrder,
                                    On<FSServiceOrder.serviceOrderAddressID, Equal<FSAddress.addressID>>>,
                                Where<
                                    FSServiceOrder.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>,
                                    And<FSServiceOrder.refNbr, Equal<Required<FSServiceOrder.refNbr>>>>>
                                    .Select(Base, srvOrderType, refNbr)
                                    .RowCast<FSAddress>()
                                    .FirstOrDefault();

                return returnAddress;
            }
            
            return del(invoice, tran);
        }

        protected void GetServiceOrderKeys(ARTran tran, out string srvOrderType, out string refNbr)
        {
            FSARTran fsARTranRow = FSARTran.PK.Find(Base, tran.TranType, tran.RefNbr, tran.LineNbr);

            srvOrderType = fsARTranRow?.SrvOrdType;
            refNbr = fsARTranRow?.ServiceOrderRefNbr;
        }
    }
}
