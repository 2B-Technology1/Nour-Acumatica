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
    [System.SerializableAttribute]
    public class FSReasonFilter : PX.Data.IBqlTable
    {
        #region ReasonType
        public abstract class reasonType : ListField_ReasonType
        {
        }

        [PXString(4, IsFixed = true)]
        [reasonType.ListAtrribute]
        [PXDefault(ID.ReasonType.CANCEL_APPOINTMENT)]
        [PXUIField(DisplayName = "Reason Type")]
        public virtual string ReasonType { get; set; }
        #endregion    
        #region WFID
        public abstract class wFID : PX.Data.BQL.BqlInt.Field<wFID> { }

        [PXInt]
        [PXUIField(DisplayName = "Service Order Type")]
        [FSSelectorWorkflow]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? WFID { get; set; }
        #endregion
        #region WFStageID
        public abstract class wFStageID : PX.Data.BQL.BqlInt.Field<wFStageID> { }

        [PXInt]
        [PXUIField(DisplayName = "Workflow Stage")]
        [FSSelectorWorkflowStageInReason]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? WFStageID { get; set; }
        #endregion
    }
}
