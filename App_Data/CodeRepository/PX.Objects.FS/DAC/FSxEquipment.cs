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
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.FS
{
    public class FSxEquipment : PXCacheExtension<EPEquipment>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Branch location ID", Enabled = false)]
        [PXSelector(typeof(Search<FSBranchLocation.branchLocationID,
                            Where<FSBranchLocation.branchID, Equal<Current<FSServiceOrder.branchID>>>>),
                            SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
                            DescriptionField = typeof(FSBranchLocation.descr))]
        [PXFormula(typeof(Default<FSxEquipment.branchID>))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? BranchLocationID { get; set; }
        #endregion
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Staff Member ID", Enabled = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[FSSelector_StaffMember_All]
        public virtual int? EmployeeID { get; set; }
        #endregion
        #region RoomID
        public abstract class roomID : PX.Data.BQL.BqlString.Field<roomID> { }
        [PXDBString(10)]
        [PXUIField(DisplayName = "Room", Enabled = false)]
        [PXSelector(typeof(Search<FSRoom.roomID, 
                                    Where<FSRoom.branchLocationID, 
                                    Equal<Current<FSxEquipment.branchLocationID>>>>), 
                            DescriptionField = typeof(FSRoom.descr))]
        [PXFormula(typeof(Default<FSxEquipment.branchLocationID>))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string RoomID { get; set; }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Warehouse ID", Enabled = false)]
        [PXSelector(typeof(INSite.siteID), 
                    SubstituteKey = typeof(INSite.siteCD))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? SiteID { get; set; }
        #endregion
        #region AllowSchedule
        public abstract class allowSchedule : PX.Data.BQL.BqlBool.Field<allowSchedule> { }
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Show in Calendars and Route Maps")]
        public virtual bool? AllowSchedule { get; set; }
        #endregion
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        [PXDBInt]
        [PXDefault(typeof(AccessInfo.branchID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Branch ID", Enabled = false)]
        [PXSelector(typeof(Search<Branch.branchID>), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public virtual int? BranchID { get; set; }
		#endregion
		#region VehicleDescr
		public abstract class vehicleDescr : PX.Data.IBqlField { }
		[PXString]
		[PXUIField(Visible = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string VehicleDescr { get; set; }
		#endregion
	}
}
