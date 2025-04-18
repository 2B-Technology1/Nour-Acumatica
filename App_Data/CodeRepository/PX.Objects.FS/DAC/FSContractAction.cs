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

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class FSContractAction : PX.Data.IBqlTable
    {
        #region ServiceContractID
        public abstract class serviceContractID : PX.Data.BQL.BqlInt.Field<serviceContractID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Service Contract ID", Enabled = false)]
        [PXParent(typeof(Select<FSServiceContract, Where<FSServiceContract.serviceContractID, Equal<Current<FSContractAction.serviceContractID>>>>))]
        [PXDBDefault(typeof(FSServiceContract.serviceContractID))]
        public virtual int? ServiceContractID { get; set; }
        #endregion
        #region RecordID
        public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }

        [PXDBIdentity(IsKey = true)]
        public virtual int? RecordID { get; set; }
        #endregion
        #region Type
        public abstract class type : ListField_RecordType_ContractAction
        {
        }

        [type.ListAtrribute]
        [PXDBString(1, IsUnicode = false)]
        [PXUIField(DisplayName = "Type", Enabled = false)]
        public virtual string Type { get; set; }
        #endregion
        #region Action
        public abstract class action : ListField_Action_ContractAction
        {
        }

        [action.ListAtrribute]
        [PXDBString(1, IsUnicode = false)]
        [PXUIField(DisplayName = "Action", Enabled = false)]
        public virtual string Action { get; set; }
        #endregion
        #region ActionBusinessDate
        public abstract class actionBusinessDate : PX.Data.BQL.BqlDateTime.Field<actionBusinessDate> { }

        [PXDBDate]
        [PXUIField(DisplayName = "Date", Enabled = false)]
        public virtual DateTime? ActionBusinessDate { get; set; }
        #endregion
        #region EffectiveDate
        public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }

        [PXDBDate]
        [PXUIField(DisplayName = "Effective Date", Enabled = false)]
        public virtual DateTime? EffectiveDate { get; set; }
        #endregion
        #region ScheduleChangeRecurrence
        public abstract class scheduleChangeRecurrence : PX.Data.BQL.BqlBool.Field<scheduleChangeRecurrence> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Change Recurrence", Enabled = false)]
        public virtual bool? ScheduleChangeRecurrence { get; set; }
        #endregion
        #region ScheduleNextExecutionDate
        public abstract class scheduleNextExecutionDate : PX.Data.BQL.BqlDateTime.Field<scheduleNextExecutionDate> { }

        [PXDBDate]
        [PXUIField(DisplayName = "Effective Recurrence Start Date", Enabled = false)]
        public virtual DateTime? ScheduleNextExecutionDate { get; set; }
        #endregion
        #region ScheduleRecurrenceDescr
        public abstract class scheduleRecurrenceDescr : PX.Data.BQL.BqlString.Field<scheduleRecurrenceDescr> { }
        [PXDBString(int.MaxValue, IsUnicode = false)]
        [PXUIField(DisplayName = "Recurrence Description", Enabled = false)]
        public virtual string ScheduleRecurrenceDescr { get; set; }
        #endregion
        #region ScheduleRefNbr
        public abstract class scheduleRefNbr : PX.Data.BQL.BqlString.Field<scheduleRefNbr> { }
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Schedule ID", Enabled = false)]
        public virtual string ScheduleRefNbr { get; set; }
		#endregion
		#region OrigServiceContractRefNbr
		public abstract class origServiceContractRefNbr : PX.Data.BQL.BqlString.Field<origServiceContractRefNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXSelector(typeof(Search<FSServiceContract.refNbr>), ValidateValue = false)]
		[PXUIField(DisplayName = "Orig. Service Contract ID", Enabled = false)]
		public virtual string OrigServiceContractRefNbr { get; set; }
		#endregion
		#region OrigScheduleRefNbr
		public abstract class origScheduleRefNbr : PX.Data.BQL.BqlString.Field<origScheduleRefNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXSelector(typeof(Search<FSSchedule.refNbr>), ValidateValue = false)]
		[PXUIField(DisplayName = "Orig. Schedule ID", Enabled = false)]
		public virtual string OrigScheduleRefNbr { get; set; }
		#endregion
		#region Applied
		public abstract class applied : PX.Data.BQL.BqlBool.Field<applied> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Applied", Enabled = false)]
        public virtual bool? Applied { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "Created By ID")]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "Created By Screen ID")]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Created DateTime")]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "Last Modified By ID")]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "Last Modified By Screen ID")]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Modified Date Time")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected Byte[] _tstamp;
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}
}
