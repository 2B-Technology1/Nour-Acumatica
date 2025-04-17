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
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.PO;
using Messages = PX.Objects.CN.Subcontracts.AP.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.AP.CacheExtensions
{
    public sealed class ApTranExt : PXCacheExtension<APTran>
    {
        [PXString(15, IsUnicode = true)]
        [PXUIField(DisplayName = Messages.Subcontract.SubcontractNumber, Enabled = false, IsReadOnly = true)]
        public string SubcontractNbr =>
            Base.POOrderType == POOrderType.RegularSubcontract
                ? Base.PONbr
                : null;

        [PXInt]
        [PXUIField(DisplayName = Messages.Subcontract.SubcontractLine, Visible = false)]
        [PXSelector(typeof(Search<POLine.lineNbr, Where<POLine.orderType, Equal<Current<APTran.pOOrderType>>,
            And<POLine.orderNbr, Equal<Current<APTran.pONbr>>>>>),
            typeof(POLine.lineNbr), typeof(POLine.projectID), typeof(POLine.taskID), typeof(POLine.costCodeID),
            typeof(POLine.inventoryID), typeof(POLine.lineType), typeof(POLine.tranDesc), typeof(POLine.uOM),
            typeof(POLine.orderQty), typeof(POLine.curyUnitCost), typeof(POLine.curyExtCost))]
        public int? SubcontractLineNbr
        {
            get
            {
                return Base.POOrderType == POOrderType.RegularSubcontract
               ? Base.POLineNbr
               : null;
            }
            set
            {
                Base.POLineNbr = value;
            }
        }
           

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class subcontractNbr : IBqlField
        {
        }

        public abstract class subcontractLineNbr : IBqlField
        {
        }
    }
}