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
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using System;

namespace PX.Objects.FS
{
    public class FSxEPExpenseClaimDetails : PXCacheExtension<EPExpenseClaimDetails>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region FSEntityTypeUI
        public abstract class fsEntityTypeUI : PX.Data.BQL.BqlString.Field<fsEntityTypeUI>
        {
            public abstract class Values : ListField_FSEntity_Type { }
        }

        [PXString]
        [PXUIField(DisplayName = "Related Svc. Doc. Type")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [fsEntityTypeUI.Values.List]
        public virtual string FSEntityTypeUI { get; set; }
        #endregion
        #region FSEntityType
        public string _FSEntityType;
        public abstract class fsEntityType : PX.Data.BQL.BqlString.Field<fsEntityType> { }
        [PXString]
        [PXDefault(ID.FSEntityType.ServiceOrder, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Switch<Case<Where<fsEntityTypeUI, IsNotNull>, fsEntityTypeUI>, fsEntityTypeUI.Values.serviceOrder>))]
        public virtual string FSEntityType { get; set; }
        #endregion
        #region FSEntityNoteID
        public abstract class fsEntityNoteID : PX.Data.BQL.BqlGuid.Field<fsEntityNoteID> { }
        [PXUIField(DisplayName = "Related Svc. Doc. Nbr.")]
        [FSEntityIDExpenseSelector(typeof(fsEntityTypeUI), typeof(EPExpenseClaimDetails.customerID), typeof(EPExpenseClaimDetails.contractID), typeof(EPExpenseClaimDetails.customerLocationID))]
        [PXDBGuid]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Guid? FSEntityNoteID { get; set; }
        #endregion
        #region IsDocBilledOrClosed
        public abstract class isDocBilledOrClosed : PX.Data.BQL.BqlBool.Field<isDocBilledOrClosed> { }
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? IsDocBilledOrClosed { get; set; }
        #endregion
        #region IsDocRelatedToProject
        public abstract class isDocRelatedToProject : PX.Data.BQL.BqlBool.Field<isDocRelatedToProject> { }
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? IsDocRelatedToProject { get; set; }
        #endregion
        #region FSBillable
        public abstract class fsBillable : PX.Data.BQL.BqlBool.Field<fsBillable> { }
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<fsEntityNoteID, fsEntityTypeUI>))]
        [PXUIField(DisplayName = "Billable in Svc. Doc.")]
        public virtual bool? FSBillable { get; set; }
        #endregion
    }
}
