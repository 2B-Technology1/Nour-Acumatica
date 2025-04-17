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
using PX.Objects.CN.Compliance.CL.Descriptor;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver;

namespace PX.Objects.CN.Compliance.CL.DAC
{
    /// <summary>
    /// Auxiliary DAC used in reports.
    /// </summary>
    [PXHidden]
    public class CLReportParameters : IBqlTable
    {
        [PXInt]
        [PXUIField]
        [LienWaiverReportSelector(Constants.LienWaiverDocumentTypeValues.UnconditionalPartial)]
        public virtual int? UnconditionalPartial
        {
            get;
            set;
        }

        [PXInt]
        [PXUIField]
        [LienWaiverReportSelector(Constants.LienWaiverDocumentTypeValues.ConditionalPartial)]
        public virtual int? ConditionalPartial
        {
            get;
            set;
        }

        [PXInt]
        [PXUIField]
        [LienWaiverReportSelector(Constants.LienWaiverDocumentTypeValues.UnconditionalFinal)]
        public virtual int? UnconditionalFinal
        {
            get;
            set;
        }

        [PXInt]
        [PXUIField]
        [LienWaiverReportSelector(Constants.LienWaiverDocumentTypeValues.ConditionalFinal)]
        public virtual int? ConditionalFinal
        {
            get;
            set;
        }
    }
}
