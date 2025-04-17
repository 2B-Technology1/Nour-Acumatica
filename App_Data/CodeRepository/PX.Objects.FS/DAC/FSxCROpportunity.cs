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
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public class FSxCROpportunity : PXCacheExtension<CROpportunity>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>()
                && PXAccess.FeatureInstalled<FeaturesSet.customerModule>();
        }

        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Branch Location")]
        [PXSelector(typeof(
            Search<FSBranchLocation.branchLocationID,
            Where<
                FSBranchLocation.branchID, Equal<Current<CROpportunity.branchID>>>>),
            SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
            DescriptionField = typeof(FSBranchLocation.descr))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
        [PXDBString(4, IsFixed = true, InputMask = ">AAAA")]
        [PXUIField(DisplayName = "Service Order Type")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorActiveSrvOrdType]
        [PXFormula(typeof(Default<FSxCROpportunity.sDEnabled>))]
        public virtual string SrvOrdType { get; set; }
		#endregion
		#region ServiceOrderRefNbr
		public abstract class serviceOrderRefNbr : PX.Data.BQL.BqlString.Field<serviceOrderRefNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Service Order Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<FSServiceOrder.refNbr,
			Where<
			FSServiceOrder.srvOrdType, Equal<Current<FSxCROpportunity.srvOrdType>>>>))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string ServiceOrderRefNbr { get; set; }
		#endregion
		#region SDEnabled
		public abstract class sDEnabled : PX.Data.BQL.BqlBool.Field<sDEnabled> { }
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Create Service Order")]
        public virtual bool? SDEnabled { get; set; }
		#endregion
        #region ChkServiceManagement
        public abstract class ChkServiceManagement : PX.Data.BQL.BqlBool.Field<ChkServiceManagement> { }
        [PXBool]
        [PXUIField(Visible = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? chkServiceManagement
        {
            get
            {
                return true;
            }
        }
        #endregion
    }
}

namespace PX.Objects.FS.Opportunity
{
    ///This cache extension is needed because screen OP000373 is working with PX.Objects.CR.CROpportunity what is a PXProjection
    ///and not with PX.Objects.CR.Standalone wich is the table in the Database. Without this, fields are not saved in the DB.
    public class FSxCROpportunity : PXCacheExtension<CR.Standalone.CROpportunity>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>()
                && PXAccess.FeatureInstalled<FeaturesSet.customerModule>();
        }

        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Branch Location")]
        [PXSelector(typeof(
            Search<FSBranchLocation.branchLocationID,
            Where<
                FSBranchLocation.branchID, Equal<Current<CROpportunity.branchID>>>>),
            SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
            DescriptionField = typeof(FSBranchLocation.descr))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
        [PXDBString(4, InputMask = ">AAAA")]
        [PXUIField(DisplayName = "Service Order Type")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorActiveSrvOrdType]
        [PXFormula(typeof(Default<FSxCROpportunity.sDEnabled>))]
        public virtual string SrvOrdType { get; set; }
		#endregion
		#region ServiceOrderRefNbr
		public abstract class serviceOrderRefNbr : PX.Data.BQL.BqlString.Field<serviceOrderRefNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Service Order Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<FSServiceOrder.refNbr,
			Where<
			FSServiceOrder.srvOrdType, Equal<Current<FSxCROpportunity.srvOrdType>>>>))]
		public virtual string ServiceOrderRefNbr { get; set; }
		#endregion
		#region SDEnabled
		public abstract class sDEnabled : PX.Data.BQL.BqlBool.Field<sDEnabled> { }
        [PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Create Service Order")]
        public virtual bool? SDEnabled { get; set; }
		#endregion
		#region ChkServiceManagement
		public abstract class ChkServiceManagement : PX.Data.BQL.BqlBool.Field<ChkServiceManagement> { }
        [PXBool]
        [PXUIField(Visible = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? chkServiceManagement
        {
            get
            {
                return true;
            }
        }
        #endregion
    }
}
