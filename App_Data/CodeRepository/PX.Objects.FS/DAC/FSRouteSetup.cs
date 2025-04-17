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
using System;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.ROUTES_SETUP)]
    [PXPrimaryGraph(typeof(RouteSetupMaint))]
    public class FSRouteSetup : PX.Data.IBqlTable
	{
        #region Keys
        public static class FK
        {
            public class RouteNumbering : Numbering.PK.ForeignKeyOf<FSRouteSetup>.By<routeNumberingID> { }
            public class DfltServiceOrderType : FSSrvOrdType.PK.ForeignKeyOf<FSRouteSetup>.By<dfltSrvOrdType> { }
        }
        #endregion

        public const string RouteManagementFieldClass = "ROUTEMANAGEMENT";

        #region RouteNumberingID
        public abstract class routeNumberingID : PX.Data.BQL.BqlString.Field<routeNumberingID> { }

		[PXDBString(10)]
		[PXDefault]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Route Numbering Sequence")]
        public virtual string RouteNumberingID { get; set; }
		#endregion
        #region AutoCalculateRouteStats
        public abstract class autoCalculateRouteStats : PX.Data.BQL.BqlBool.Field<autoCalculateRouteStats> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Calculate Route Statistics Automatically")]
        public virtual bool? AutoCalculateRouteStats { get; set; }
        #endregion 
        #region DfltSrvOrdType
        public abstract class dfltSrvOrdType : PX.Data.BQL.BqlString.Field<dfltSrvOrdType> { }

        [PXDBString(4, IsUnicode = false)]
        [PXUIField(DisplayName = "Default Service Order Type")]
		[PXRestrictor(typeof(Where<FSSrvOrdType.active, Equal<True>>), null)]
		[FSSelectorSrvOrdTypeRoute]
        public virtual string DfltSrvOrdType { get; set; }
        #endregion        
        #region GroupINDocumentsByPostingProcess
        public abstract class groupINDocumentsByPostingProcess : PX.Data.BQL.BqlBool.Field<groupINDocumentsByPostingProcess> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Group Inventory Documents by Posting Process")]
        public virtual bool? GroupINDocumentsByPostingProcess { get; set; }
        #endregion
        #region TrackRouteLocation
        public abstract class trackRouteLocation : PX.Data.BQL.BqlBool.Field<trackRouteLocation> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Track Start and Complete Location of Route")]
        public virtual bool? TrackRouteLocation { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        [PXDBCreatedByID]
        [PXUIField(DisplayName = "CreatedByID")]
        public virtual Guid? CreatedByID { get; set; }

        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "CreatedByScreenID")]
        public virtual string CreatedByScreenID { get; set; }

        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "CreatedDateTime")]
        public virtual DateTime? CreatedDateTime { get; set; }

        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "LastModifiedByID")]
        public virtual Guid? LastModifiedByID { get; set; }

        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "LastModifiedByScreenID")]
        public virtual string LastModifiedByScreenID { get; set; }

        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "LastModifiedDateTime")]
        public virtual DateTime? LastModifiedDateTime { get; set; }

        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        [PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
        [PXUIField(DisplayName = "tstamp")]
        public virtual byte[] tstamp { get; set; }

        #endregion

        #region SetFirstManualAppointment
        public abstract class setFirstManualAppointment : PX.Data.BQL.BqlBool.Field<setFirstManualAppointment> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Set Appointments Created Manually as First in Route")]
        public virtual bool? SetFirstManualAppointment { get; set; }
        #endregion 
        #region EnableSeasonScheduleContract
        public abstract class enableSeasonScheduleContract : PX.Data.BQL.BqlBool.Field<enableSeasonScheduleContract> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Enable Seasons in Schedule Contracts")]
        public virtual bool? EnableSeasonScheduleContract { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion
	}
}
