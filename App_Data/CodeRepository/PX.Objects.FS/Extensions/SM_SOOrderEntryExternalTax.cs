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

namespace PX.Objects.FS
{
    public class SM_SOOrderEntryExternalTax : PXGraphExtension<SOOrderEntryExternalTax, SOOrderEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public delegate IAddressLocation GetFromAddressLineDelegate(SOOrder order, SOLine line);
        public delegate IAddressLocation GetToAddressLineDelegate(SOOrder order, SOLine line);

        [PXOverride]
        public virtual IAddressLocation GetFromAddress(SOOrder order, SOLine line, GetFromAddressLineDelegate del)
        {
            string srvOrderType;
            string refNbr;
            GetServiceOrderKeys(line, out srvOrderType, out refNbr);

            if (string.IsNullOrEmpty(refNbr) == false && line.SiteID == null)
            {
                IAddressLocation returnAddress = null;

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
                
                return returnAddress;
            }

            return del(order, line);
        }

        [PXOverride]
        public virtual IAddressLocation GetToAddress(SOOrder order, SOLine line, GetToAddressLineDelegate del)
        {
            string srvOrderType;
            string refNbr;
            GetServiceOrderKeys(line, out srvOrderType, out refNbr);

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
            
            return del(order, line);
        }

        protected void GetServiceOrderKeys(SOLine line, out string srvOrderType, out string refNbr)
        {
            FSxSOLine row = PXCache<SOLine>.GetExtension<FSxSOLine>(line);
            srvOrderType = row?.SrvOrdType;
            refNbr = row?.ServiceOrderRefNbr;
        }
    }
}
