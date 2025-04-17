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
    public class FSxPMTimeActivity : PXCacheExtension<PMTimeActivity>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>()
                && PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>();
        }

        #region ServiceID
        public abstract class serviceID : PX.Data.BQL.BqlInt.Field<serviceID> { }
        [Service]
        [PXUIField(DisplayName = "Service")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? ServiceID { get; set; }
        #endregion
        #region AppointmentCustomerID
        public abstract class appointmentCustomerID : PX.Data.BQL.BqlInt.Field<appointmentCustomerID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Customer ID")]
        [FSSelectorCustomer]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? AppointmentCustomerID { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Appointment Nbr.")]
        [PXSelector(typeof(Search<FSAppointment.appointmentID>),
                           SubstituteKey = typeof(FSAppointment.refNbr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? AppointmentID { get; set; }
        #endregion
        #region LogLineNbr
        public abstract class logLineNbr : PX.Data.BQL.BqlInt.Field<logLineNbr> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Log Ref. Nbr.", Enabled = false)]
        [PXSelector(
                    typeof(Search<FSAppointmentLog.lineNbr,
                                Where<FSAppointmentLog.docID, Equal<Current<FSxPMTimeActivity.appointmentID>>>>),
                    SubstituteKey = typeof(FSAppointmentLog.lineRef))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? LogLineNbr { get; set; }
        #endregion

		#region LastBillable
		public abstract class lastBillable : PX.Data.BQL.BqlBool.Field<lastBillable> { }
		[PXBool]
        [PXUIField(Visible = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? LastBillable { get; set; }
        #endregion
 
    }
}
