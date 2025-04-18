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

using PX.Objects.AM.Attributes;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using System;
using PX.Data;

namespace PX.Objects.AM
{
	/// <summary>
	/// The table that stores the schedule details for a tool on a production order.
	/// Parent: <see cref="AMToolMst"/>
	/// </summary>
	[Serializable]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [PXCacheName(Messages.ToolSchdDetail)]
    public class AMToolSchdDetail : IBqlTable, ISchdDetail<AMToolSchdDetail>, ISchdReference
    {
        internal string DebuggerDisplay => $"RecordID={RecordID}; ToolID={ToolID}; SchdQty={SchdQty.GetValueOrDefault()}; {SchdDate.GetValueOrDefault().ToShortDateString()}; {StartTime.GetValueOrDefault().ToShortTimeString()} - {EndTime.GetValueOrDefault().ToShortTimeString()}{(IsBreak == true ? "; IsBreak=true" : string.Empty)}";

        #region Keys

        public class PK : PrimaryKeyOf<AMToolSchdDetail>.By<recordID>
        {
            public static AMToolSchdDetail Find(PXGraph graph, int? recordID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, recordID, options);
        }

        public static class FK
        {
            public class Tool : AMToolMst.PK.ForeignKeyOf<AMToolSchdDetail>.By<toolID> { }
            public class Site : IN.INSite.PK.ForeignKeyOf<AMToolSchdDetail>.By<siteID> { }
        }

        #endregion

        #region ResourceID (Unbound)
        public abstract class resourceID : PX.Data.BQL.BqlString.Field<resourceID> { }

        [PXString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Resource ID", Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual string ResourceID
        {
            get
            {
                return this._ToolID;
            }
            set
            {
                this._ToolID = value;
            }
        }
        #endregion
        #region RecordID (KEY)
        public abstract class recordID : PX.Data.BQL.BqlLong.Field<recordID> { }

        protected Int64? _RecordID;
        [PXDBLongIdentity(IsKey = true)]
        [PXUIField(DisplayName = "Record ID", Enabled = false, Visible = false)]
        public virtual Int64? RecordID
        {
            get { return this._RecordID; }
            set { this._RecordID = value; }
        }
        #endregion
        #region ToolID
        public abstract class toolID : PX.Data.BQL.BqlString.Field<toolID> { }

        protected string _ToolID;
        [ToolIDField]
        [PXDefault]
        [PXSelector(typeof(Search<AMToolMst.toolID>))]
        [PXForeignReference(typeof(Field<AMToolSchdDetail.toolID>.IsRelatedTo<AMToolMst.toolID>))]
        public virtual string ToolID
        {
            get
            {
                return this._ToolID;
            }
            set
            {
                this._ToolID = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [Site(Enabled = false)]
        [PXDefault]
        [PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
        public virtual Int32? SiteID
        {
            get { return this._SiteID; }
            set { this._SiteID = value; }
        }
        #endregion
        #region SchdDate

        public abstract class schdDate : PX.Data.BQL.BqlDateTime.Field<schdDate> { }

        protected DateTime? _SchdDate;
        [PXDBDate]
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
        [PXUIField(DisplayName = "Start Time", Enabled = false)]
        [PXDefault]
        public virtual DateTime? StartTime
        {
            get { return this._StartTime; }
            set
            {
                this._StartTime = value;
                this._StartTimeString = this._StartTime == null ? string.Empty : this._StartTime.GetValueOrDefault().ToString("hh:mm tt");
            }
        }

        #endregion
        #region EndTime

        public abstract class endTime : PX.Data.BQL.BqlDateTime.Field<endTime> { }

        protected DateTime? _EndTime;

        [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
        [PXUIField(DisplayName = "End Time", Enabled = false)]
        [PXDefault]
        public virtual DateTime? EndTime
        {
            get { return this._EndTime; }
            set
            {
                this._EndTime = value;
                this._EndTimeString = this._EndTime == null ? string.Empty : this._EndTime.GetValueOrDefault().ToString("hh:mm tt");
            }
        }

        #endregion
        #region WORK AROUNDS FOR GI
        /// <summary>
        /// Work around for GI not being able to display time
        /// </summary>
        public abstract class startTimeString : PX.Data.BQL.BqlDateTime.Field<startTimeString> { }

        protected String _StartTimeString;
        /// <summary>
        /// Work around for GI not being able to display time
        /// </summary>
        [PXString]
        [PXUIField(DisplayName = "Start Time", Enabled = false)]
        [PXDependsOnFields(typeof(AMToolSchdDetail.startTime))]
        public virtual String StartTimeString
        {
            get { return this._StartTimeString; }
        }

        /// <summary>
        /// Work around for GI not being able to display time
        /// </summary>
        public abstract class endTimeString : PX.Data.BQL.BqlDateTime.Field<endTimeString> { }

        protected String _EndTimeString;
        /// <summary>
        /// Work around for GI not being able to display time
        /// </summary>
        [PXString]
        [PXUIField(DisplayName = "End Time", Enabled = false)]
        [PXDependsOnFields(typeof(AMToolSchdDetail.endTime))]
        public virtual String EndTimeString
        {
            get { return this._EndTimeString; }
        }

        #endregion
        #region OrderByDate

        public abstract class orderByDate : PX.Data.BQL.BqlDateTime.Field<orderByDate> { }

        protected DateTime? _OrderByDate;

        [PXDBDateAndTime(UseTimeZone = false)]
        [PXUIField(DisplayName = "Order By Date Time", Enabled = false, Visible = false)]
        [PXDefault]
        public virtual DateTime? OrderByDate
        {
            get { return this._OrderByDate; }
            set { this._OrderByDate = value; }
        }

        #endregion
        #region Description

        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        protected String _Description;
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Enabled = false)]
        public virtual String Description
        {
            get { return this._Description; }
            set { this._Description = value; }
        }
        #endregion
        #region PlanQty
        public abstract class planQty : PX.Data.BQL.BqlInt.Field<planQty> { }

        protected int? _PlanQty;
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Plan Qty", Enabled = false)]
        public virtual int? PlanQty
		{
			get
			{
				return this._PlanQty;
			}
			set
			{
				this._PlanQty = value;
			}
		}
		#endregion
		#region SchdQty
		public abstract class schdQty : PX.Data.BQL.BqlInt.Field<schdQty> { }

        protected int? _SchdQty;
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Schedule Qty", Enabled = false)]
        public virtual int? SchdQty
		{
			get
			{
				return this._SchdQty;
			}
			set
			{
				this._SchdQty = value;
			}
		}
        #endregion
        #region ProdToolNoteID
        /// <summary>
        /// Link to <see cref="AMProdTool"/> via to <see cref="AMProdTool.noteID"/>
        /// </summary>
        public abstract class prodToolNoteID : PX.Data.BQL.BqlGuid.Field<prodToolNoteID> { }

        protected Guid? _ProdToolNoteID;

        /// <summary>
        /// Link to <see cref="AMProdTool"/> via to <see cref="AMProdTool.noteID"/>
        /// </summary>
        [PXNote]
        public virtual Guid? ProdToolNoteID
        {
            get { return this._ProdToolNoteID; }
            set { this._ProdToolNoteID = value; }
        }

        #endregion
        #region IsBreak
        public abstract class isBreak : PX.Data.BQL.BqlBool.Field<isBreak> { }

        protected bool? _IsBreak;

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Down Time", Enabled = false)]
        public virtual bool? IsBreak
        {
            get { return this._IsBreak; }
            set { this._IsBreak = value; }
        }
        #endregion
        #region RunTimeBase

        public abstract class runTimeBase : PX.Data.BQL.BqlInt.Field<runTimeBase> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Run Time Without Efficiency", Enabled = false, Visible = false)]
        [PXDefault(0)]
        public virtual int? RunTimeBase { get; set; }

        #endregion
        #region RunTime

        public abstract class runTime : PX.Data.BQL.BqlInt.Field<runTime> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Run Time", Enabled = false)]
        [PXDefault(0)]
        public virtual int? RunTime { get; set; }

        #endregion
        #region SchdBlocks
        public abstract class schdBlocks : PX.Data.BQL.BqlInt.Field<schdBlocks> { }

        protected int? _SchdBlocks;
        [PXDBInt(MinValue = 0)]
        [PXUIField(DisplayName = "Scheduled Blocks", Enabled = false)]
        [PXDefault(0)]
        public virtual int? SchdBlocks
        {
            get { return this._SchdBlocks; }
            set { this._SchdBlocks = value; }
        }
        #endregion
        #region SchdKey

        public abstract class schdKey : PX.Data.BQL.BqlGuid.Field<schdKey> { }

        protected Guid? _SchdKey;
        [PXDBGuid]
        [PXUIField(DisplayName = "Schedule Key", Enabled = false, Visible = false)]
        public virtual Guid? SchdKey
        {
            get { return this._SchdKey; }
            set { this._SchdKey = value; }
        }
        #endregion
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID]
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
        [PXDBCreatedByScreenID]
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
        [PXDBCreatedDateTime]
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
        [PXDBLastModifiedByID]
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
        [PXDBLastModifiedByScreenID]
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
        [PXDBLastModifiedDateTime]
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
        [PXDBTimestamp]
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

        /// <summary>
        /// Makes a copy of the object excluding the recordID, created by, last mod by, and timestamps fields
        /// </summary>
        /// <param name="schdDetail">Schd Detail record to copy</param>
        /// <returns>new object with copied values</returns>
        public static AMToolSchdDetail Copy(AMToolSchdDetail schdDetail)
        {
            return schdDetail.Copy();
        }

#pragma warning disable PX1031 // DACs cannot contain instance methods
        /// <summary>
        /// Makes a copy of the object excluding the recordID, created by, last mod by, and timestamps fields
        /// </summary>
        /// <returns>new object with copied values</returns>
        public virtual AMToolSchdDetail Copy()
#pragma warning restore PX1031 // DACs cannot contain instance methods
        {
            return new AMToolSchdDetail
            {
                SchdKey = this.SchdKey,
                ToolID = this.ToolID,
                SiteID = this.SiteID,
                SchdDate = this.SchdDate,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                OrderByDate = this.OrderByDate,
                Description = this.Description,
                PlanQty = this.PlanQty,
                SchdQty = this.SchdQty,
                IsBreak = this.IsBreak,
                SchdBlocks = this.SchdBlocks,
                RunTimeBase = this.RunTimeBase,
                RunTime = this.RunTime
            };
        }
    }
}
