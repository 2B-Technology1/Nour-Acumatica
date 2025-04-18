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

namespace PX.Objects.FS
{
    public class RelatedApptSummary
    {
        public RelatedApptSummary(int? soDetID)
        {
            this.SODetID = soDetID;

            this.ApptCntr = 0;
            this.ApptCntrIncludingRequestPO = 0;

            this.ApptEstimatedDuration = 0;
            this.ApptEstimatedQty = 0m;
            this.BaseApptEstimatedQty = 0m;
            this.CuryApptEstimatedTranAmt = 0m;
            this.ApptEstimatedTranAmt = 0m;

            this.ApptActualDuration = 0;
            this.ApptActualQty = 0m;
            this.BaseApptActualQty = 0m;
            this.CuryApptActualTranAmt = 0m;
            this.ApptActualTranAmt = 0m;

            this.ApptEffTranDuration = 0;
            this.ApptEffTranQty = 0m;
            this.BaseApptEffTranQty = 0m;
            this.CuryApptEffTranAmt = 0m;
            this.ApptEffTranAmt = 0m;
        }

        public virtual int? SODetID { get; set; }

        public virtual int ApptCntr { get; set; }
        public virtual int ApptCntrIncludingRequestPO { get; set; }

        #region Estimated fields

        public virtual int ApptEstimatedDuration { get; set; }

        public virtual decimal ApptEstimatedQty { get; set; }

        public virtual decimal BaseApptEstimatedQty { get; set; }

        public virtual decimal CuryApptEstimatedTranAmt { get; set; }

        public virtual decimal ApptEstimatedTranAmt { get; set; }
        #endregion
        #region Actual fields

        public virtual int ApptActualDuration { get; set; }

        public virtual decimal ApptActualQty { get; set; }

        public virtual decimal BaseApptActualQty { get; set; }

        public virtual decimal CuryApptActualTranAmt { get; set; }

        public virtual decimal ApptActualTranAmt { get; set; }
        #endregion
        #region Effective fields (based on appointment line status, not appointment header status)

        public virtual int ApptEffTranDuration { get; set; }

        public virtual decimal ApptEffTranQty { get; set; }

        public virtual decimal BaseApptEffTranQty { get; set; }

        public virtual decimal CuryApptEffTranAmt { get; set; }

        public virtual decimal ApptEffTranAmt { get; set; }
        #endregion

        #region Exceptions
        public virtual PXException ApptCntr_Exception { get; set; }

        public virtual PXException ApptEstimatedDuration_Exception { get; set; }

        public virtual PXException ApptActualDuration_Exception { get; set; }

        public virtual PXException ApptEffTranQty_Exception { get; set; }

        //public virtual PXException BaseApptEffTranQty_Exception { get; set; }

        public virtual PXException CuryApptEffTranAmt_Exception { get; set; }

        //public virtual PXException ApptEffTranAmt_Exception { get; set; }
        #endregion
    }
}
