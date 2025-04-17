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
using PX.Data;

namespace PX.Objects.PO
{
    //This class contains POLineS projection definition only. POLineS DAC is currently located in POReceiptEntry 
    //TODO: Refactor POLineS and similar classes
    public class POLineForReceivingProjection : PXProjectionAttribute
    {
        public POLineForReceivingProjection()
            : base(typeof(Select<POLine>))
        {
        }

        protected override Type GetSelect(PXCache sender)
        {
            POSetup posetup = PXSetup<POSetup>.Select(sender.Graph);
				
            if (posetup.AddServicesFromNormalPOtoPR == true && posetup.AddServicesFromDSPOtoPR != true)
            {
                return typeof(Select<POLine, Where<POLine.lineType, NotEqual<POLineType.service>,
                    Or<POLine.orderType, Equal<POOrderType.regularOrder>>>>);
            }
            else if (posetup.AddServicesFromNormalPOtoPR != true && posetup.AddServicesFromDSPOtoPR == true)
            {
                return typeof(Select<POLine, Where<POLine.lineType, NotEqual<POLineType.service>,
                    Or<POLine.orderType, Equal<POOrderType.dropShip>>>>);
            }
            else if (posetup.AddServicesFromNormalPOtoPR == true && posetup.AddServicesFromDSPOtoPR == true)
            {
                return typeof(Select<POLine, Where<POLine.lineType, NotEqual<POLineType.service>,
                    Or<POLine.orderType, Equal<POOrderType.regularOrder>,
                        Or<POLine.orderType, Equal<POOrderType.dropShip>>>>>);
            }
            else
            {
                return typeof(Select<POLine, Where<POLine.lineType, NotEqual<POLineType.service>, Or<POLine.processNonStockAsServiceViaPR, Equal<True>>>>);
            }
        }
    }
}