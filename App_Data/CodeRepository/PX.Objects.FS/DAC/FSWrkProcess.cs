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
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.FS
{
	[System.SerializableAttribute]
    [PXCacheName(TX.TableName.FSWrkProcess)]
    public class FSWrkProcess : PX.Data.IBqlTable
	{
        #region Keys
        public class PK : PrimaryKeyOf<FSWrkProcess>.By<processID>
        {
            public static FSWrkProcess Find(PXGraph graph, int? processID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, processID, options);
        }

        public static class FK
        {
            public class ServiceOrderType : FSSrvOrdType.PK.ForeignKeyOf<FSWrkProcess>.By<srvOrdType> { }
            public class Appointment : FSAppointment.UK.ForeignKeyOf<FSWrkProcess>.By<appointmentID> { }
            public class ServiceOrder : FSServiceOrder.UK.ForeignKeyOf<FSWrkProcess>.By<sOID> { }
            public class Branch : GL.Branch.PK.ForeignKeyOf<FSWrkProcess>.By<branchID> { }
            public class Customer : AR.Customer.PK.ForeignKeyOf<FSWrkProcess>.By<customerID> { }
            public class BranchLocation : FSBranchLocation.PK.ForeignKeyOf<FSWrkProcess>.By<branchLocationID> { }
            public class Room : FSRoom.PK.ForeignKeyOf<FSWrkProcess>.By<branchLocationID, roomID> { }
            public class Equipment : FSEquipment.PK.ForeignKeyOf<FSWrkProcess>.By<smEquipmentID> { }
            public class TargetScreen : PX.SM.SiteMap.UK.ForeignKeyOf<FSWrkProcess>.By<targetScreenID> { }
        }
        #endregion

        #region ProcessID
        public abstract class processID : PX.Data.BQL.BqlInt.Field<processID> { }
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false, Visible=false)]
        public virtual int? ProcessID { get; set; }
		#endregion
		#region SrvOrdType
		public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
		[PXDBString(4, IsFixed = true)]
		[PXUIField(DisplayName = "Service Order Type")]
        public virtual string SrvOrdType { get; set; }
		#endregion
		#region SOID
		public abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Service Order ID")]
        public virtual int? SOID { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Appointment ID")]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Branch ID")]
        public virtual int? BranchID { get; set; }
        #endregion
		#region BranchLocationID
		public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Branch Location ID")]
        public virtual int? BranchLocationID { get; set; }
		#endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Customer ID")]
        [PXForeignReference(typeof(FK.Customer))]
        public virtual int? CustomerID { get; set; }
        #endregion  
        #region RoomID
        public abstract class roomID : PX.Data.BQL.BqlString.Field<roomID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Room ID")]
        public virtual string RoomID { get; set; }
		#endregion
		#region ScheduledDateTimeBegin
		public abstract class scheduledDateTimeBegin : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeBegin> { }
        protected DateTime? _ScheduledDateTimeBegin;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true)]
		[PXUIField(DisplayName = "Scheduled Date Time Begin")]
        public virtual DateTime? ScheduledDateTimeBegin
        {
            get
            {
                return this._ScheduledDateTimeBegin;
            }

            set
            {
                this.ScheduledDateTimeBeginUTC = value;
                this._ScheduledDateTimeBegin = value;
            }
        }
        #endregion
        #region ScheduledDateTimeEnd
        public abstract class scheduledDateTimeEnd : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeEnd> { }
        protected DateTime? _ScheduledDateTimeEnd;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true)]
		[PXUIField(DisplayName = "Scheduled Date Time End")]
        public virtual DateTime? ScheduledDateTimeEnd
        {
            get
            {
                return this._ScheduledDateTimeEnd;
            }

            set
            {
                this.ScheduledDateTimeEndUTC = value;
                this._ScheduledDateTimeEnd = value;
            }
        }
        #endregion
        #region LineRefList
        public abstract class lineRefList : PX.Data.BQL.BqlString.Field<lineRefList> { }
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Ref. Nbr. List")]
        public virtual string LineRefList { get; set; }
		#endregion
		#region EmployeeIDList
		public abstract class employeeIDList : PX.Data.BQL.BqlString.Field<employeeIDList> { }
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Employee ID List")]
        public virtual string EmployeeIDList { get; set; }
		#endregion
		#region EquipmentIDList
		public abstract class equipmentIDList : PX.Data.BQL.BqlString.Field<equipmentIDList> { }
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Equipment ID List")]
        public virtual string EquipmentIDList { get; set; }
		#endregion
		#region TargetScreenID
		public abstract class targetScreenID : PX.Data.BQL.BqlString.Field<targetScreenID> { }
		[PXDBString(8, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Target Screen ID")]
        public virtual string TargetScreenID { get; set; }
		#endregion
		#region ExtraParms
		public abstract class extraParms : PX.Data.BQL.BqlString.Field<extraParms> { }
		[PXDBString(int.MaxValue, IsUnicode = true)]
		[PXUIField(DisplayName = "Extra Parameters")]
        public virtual string ExtraParms { get; set; }
        #endregion
        #region SMEquipmentID
        public abstract class smEquipmentID : PX.Data.BQL.BqlInt.Field<smEquipmentID> { }
        [PXDBInt]
        public virtual int? SMEquipmentID { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
        public virtual byte[] tstamp { get; set; }
		#endregion

        #region UTC Fields
        #region ScheduledDateTimeBeginUTC
        public abstract class scheduledDateTimeBeginUTC : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeBeginUTC> { }
        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true)]
        [PXUIField(DisplayName = "Scheduled Date Time Begin")]
        public virtual DateTime? ScheduledDateTimeBeginUTC { get; set; }
        #endregion
        #region ScheduledDateTimeEndUTC
        public abstract class scheduledDateTimeEndUTC : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeEndUTC> { }
        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true)]
        [PXUIField(DisplayName = "Scheduled Date Time End")]
        public virtual DateTime? ScheduledDateTimeEndUTC { get; set; }
        #endregion
        #endregion
    }
}
