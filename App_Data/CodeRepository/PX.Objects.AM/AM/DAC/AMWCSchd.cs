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
using System;
using PX.Objects.AM.Attributes;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Objects.AM
{
	/// <summary>
	/// The table that stores the schedule for a shift on a work center.
	/// Parent: <see cref="AMShift"/>
	/// </summary>
	[Serializable]
    [PXCacheName("Work Center Schedule")]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMWCSchd : IBqlTable, ISchd
    {
        internal string DebuggerDisplay => $"WcID = {WcID}, ShiftCD = {ShiftCD}, SchdDate = {SchdDate?.ToShortDateString()}";

        #region Keys
        public class PK : PrimaryKeyOf<AMWCSchd>.By<wcID, shiftCD, schdDate>
        {
            public static AMWCSchd Find(PXGraph graph, string wcID, string shiftCD, DateTime schdDate, PKFindOptions options = PKFindOptions.None) => FindBy(graph, wcID, shiftCD, schdDate, options);
        }

        public static class FK
        {
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMWCSchd>.By<siteID> { }
            public class WorkCenter : AMWC.PK.ForeignKeyOf<AMWCSchd>.By<wcID> { }
            public class Shift : AMShift.PK.ForeignKeyOf<AMWCSchd>.By<wcID, shiftCD> { }
        }
        #endregion

        #region ResourceID (Unbound)
        public abstract class resourceID : PX.Data.BQL.BqlString.Field<resourceID> { }

        [PXString(20, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Resource ID", Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual string ResourceID
        {
            get
            {
                return this._WcID;
            }
            set
            {
                this._WcID = value;
            }
        }
        #endregion
        #region WcID (KEY)

        public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

        protected String _WcID;
        [WorkCenterIDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXDefault]
        [PXSelector(typeof(Search<AMWC.wcID>))]
        public virtual string WcID
        {
            get { return this._WcID; }
            set { this._WcID = value; }
        }

        #endregion
        #region ShiftCD (KEY)
        public abstract class shiftCD : PX.Data.BQL.BqlString.Field<shiftCD> { }

        protected String _ShiftCD;
        [ShiftCDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [PXDefault]
        [ShiftCodeSelector]
        public virtual String ShiftCD
        {
            get
            {
                return this._ShiftCD;
            }
            set
            {
                this._ShiftCD = value;
            }
        }
        #endregion
        #region SchdDate (KEY)

        public abstract class schdDate : PX.Data.BQL.BqlDateTime.Field<schdDate> { }

        protected DateTime? _SchdDate;
        [PXDBDate(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Schedule Date", Enabled = false)]
        public virtual DateTime? SchdDate
        {
            get { return this._SchdDate; }
            set { this._SchdDate = value; }
        }

        #endregion
        #region StartTime

        public abstract class startTime : PX.Data.BQL.BqlDateTime.Field<startTime> { }

        protected DateTime? _StartTime;

        [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
        [PXUIField(DisplayName = "Start Time", Enabled = false, Visible = false)]
        public virtual DateTime? StartTime
        {
            get { return this._StartTime; }
            set { this._StartTime = value; }
        }

        #endregion
        #region EndTime

        public abstract class endTime : PX.Data.BQL.BqlDateTime.Field<endTime> { }

        protected DateTime? _EndTime;

        [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
        [PXUIField(DisplayName = "End Time", Enabled = false, Visible = false)]
        public virtual DateTime? EndTime
        {
            get { return this._EndTime; }
            set { this._EndTime = value; }
        }

        #endregion
        #region WorkTime

        public abstract class workTime : PX.Data.BQL.BqlInt.Field<workTime> { }

        protected int? _WorkTime;
        [PXDBInt]
        [PXUIField(DisplayName = "Work Time", Visible = false, Enabled = false)]
        [PXDefault(0)]
        public virtual int? WorkTime
        {
            get
            {
                return this._WorkTime;
            }
            set
            {
                this._WorkTime = value;
            }
        }

        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [PX.Objects.IN.Site(Enabled = false)]
        [PXDefault]
        [PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
        public virtual Int32? SiteID
        {
            get
            {
                return this._SiteID;
            }
            set
            {
                this._SiteID = value;
            }
        }
        #endregion
        #region TotalBlocks
        public abstract class totalBlocks : PX.Data.BQL.BqlInt.Field<totalBlocks> { }

        protected int? _TotalBlocks;
        [PXDBInt(MinValue = 0)]
        [PXUIField(DisplayName = "Total Blocks", Enabled = false)]
        [PXDefault(0)]
        public virtual int? TotalBlocks
        {
            get
            {
                return this._TotalBlocks;
            }
            set
            {
                this._TotalBlocks = value;
            }
        }
        #endregion
        #region SchdTime

        public abstract class schdTime : PX.Data.BQL.BqlInt.Field<schdTime> { }

		protected int? _SchdTime;
        /// <summary>
		/// Indicates total schedule time
        /// </summary>
        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Schedule Time", Enabled = false)]
        [PXDefault(0)]
        public virtual int? SchdTime
		{
			get
			{
				return _SchdTime;
			}
			set
			{
				_SchdTime = value;
			}
		}

        #endregion
        #region PlanBlocks
        public abstract class planBlocks : PX.Data.BQL.BqlInt.Field<planBlocks> { }

        protected int? _PlanBlocks;
        [PXDBInt(MinValue = 0)]
        [PXUIField(DisplayName = "Plan Blocks", Enabled = false)]
        [PXDefault(0)]
        public virtual int? PlanBlocks
        {
            get { return this._PlanBlocks; }
            set { this._PlanBlocks = value; }
        }
        #endregion
        #region SchdBlocks
        public abstract class schdBlocks : PX.Data.BQL.BqlInt.Field<schdBlocks> { }

        protected int? _SchdBlocks;
        [PXDBInt(MinValue = 0)]
        [PXUIField(DisplayName = "Scheduled Blocks", Enabled = false)]
        [PXDefault(0)]
        public virtual int? SchdBlocks
        {
            get
            {
                return this._SchdBlocks;
            }
            set
            {
                this._SchdBlocks = value;
            }
        }
        #endregion
        #region AvailableBlocks
        public abstract class availableBlocks : PX.Data.BQL.BqlInt.Field<availableBlocks> { }

        protected int? _AvailableBlocks;
        [PXDBInt(MinValue = 0)]
        [PXUIField(DisplayName = "Available Blocks", Enabled = false)]
        [PXDefault(0)]
        [PXFormula(typeof(Switch<Case<Where<Sub<AMWCSchd.totalBlocks, AMWCSchd.schdBlocks>, LessEqual<Zero>>, Zero>, Sub<AMWCSchd.totalBlocks, AMWCSchd.schdBlocks>>))]
        public virtual int? AvailableBlocks
        {
            get
            {
                return this._AvailableBlocks;
            }
            set
            {
                this._AvailableBlocks = value;
            }
        }
        #endregion
        #region ExceptionDate
        public abstract class exceptionDate : PX.Data.BQL.BqlBool.Field<exceptionDate> { }

        protected bool? _ExceptionDate;

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Exception Date", Enabled = false)]
        public virtual bool? ExceptionDate
        {
            get { return this._ExceptionDate; }
            set { this._ExceptionDate = value; }
        }
        #endregion
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected Byte[] _tstamp;
        [PXDBTimestamp()]
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
