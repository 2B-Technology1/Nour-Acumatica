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
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
	/// <summary>
	/// The table that stores the schedule for an operation on a production order.
	/// Parent: <see cref="AMProdOper"/>
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.ScheduleOper)]
	[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMSchdOper : IBqlTable, IProdOper
    {
	    internal string DebuggerDisplay => $"[{OrderType}:{ProdOrdID}:{OperationID}:{SchdID}:{LineNbr}] Const = {ConstDate?.ToShortDateString()}, Start = {StartDate?.ToShortDateString()}, End = {EndDate?.ToShortDateString()}";

        #region Keys

        public class PK : PrimaryKeyOf<AMSchdOper>.By<orderType, prodOrdID, operationID, schdID, lineNbr>
        {
            public static AMSchdOper Find(PXGraph graph, string orderType, string prodOrdID, int? operationID, int? schdID, int? lineNbr, PKFindOptions options = PKFindOptions.None)
                => FindBy(graph, orderType, prodOrdID, operationID, schdID, lineNbr, options);
            public static AMSchdOper FindDirty(PXGraph graph, string orderType, string prodOrdID, int? operationID, int? schdID, int? lineNbr)
                => PXSelect<AMSchdOper,
                    Where<orderType, Equal<Required<orderType>>,
                        And<prodOrdID, Equal<Required<prodOrdID>>,
                        And<operationID, Equal<Required<operationID>>,
                        And<schdID, Equal<Required<schdID>>,
                        And<lineNbr, Equal<Required<lineNbr>>>>>>>>
                    .SelectWindowed(graph, 0, 1, orderType, prodOrdID, operationID, schdID, lineNbr);
        }

        public static class FK
        {
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMSchdOper>.By<orderType> { }
            public class ProductionOrder : AMProdItem.PK.ForeignKeyOf<AMSchdOper>.By<orderType, prodOrdID> { }
            public class Operation : AMProdOper.PK.ForeignKeyOf<AMSchdOper>.By<orderType, prodOrdID, operationID> { }
            public class SchdItem : AMSchdItem.PK.ForeignKeyOf<AMSchdOper>.By<orderType, prodOrdID, schdID> { }
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMSchdOper>.By<siteID> { }
            public class Workcenter : AMWC.PK.ForeignKeyOf<AMSchdOper>.By<wcID> { }
        }

        #endregion

        #region OrderType (IsKey)
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdOper.orderType))]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion
        #region ProdOrdID (IsKey)
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

		protected string _ProdOrdID;
        [ProductionNbr(IsKey = true, Enabled = false, Visible = false)]
        [PXDBDefault(typeof(AMProdOper.prodOrdID))]
        [PXParent(typeof(Select<AMProdOper,
            Where<AMProdOper.orderType, Equal<Current<AMSchdOper.orderType>>,
                And<AMProdOper.prodOrdID, Equal<Current<AMSchdOper.prodOrdID>>,
                And<AMProdOper.operationID, Equal<Current<AMSchdOper.operationID>>>>>>))]
        public virtual string ProdOrdID
		{
			get
			{
				return this._ProdOrdID;
			}
			set
			{
				this._ProdOrdID = value;
			}
		}
        #endregion
        #region OperationID (IsKey)
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

	    protected int? _OperationID;
	    [OperationIDField(IsKey = true, Visible = false, Enabled = false)]
	    [PXDefault]
	    public virtual int? OperationID
	    {
	        get
	        {
	            return this._OperationID;
	        }
	        set
	        {
	            this._OperationID = value;
	        }
	    }
	    #endregion
        #region SchdID (IsKey)
        public abstract class schdID : PX.Data.BQL.BqlInt.Field<schdID> { }

	    protected int? _SchdID;
	    [PXDBInt(IsKey = true)]
	    [PXDefault(0)]
	    [PXUIField(DisplayName = "Schd ID")]
	    public virtual int? SchdID
	    {
	        get
	        {
	            return this._SchdID;
	        }
	        set
	        {
	            this._SchdID = value;
	        }
	    }
	    #endregion
	    #region LineNbr (IsKey)
	    public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

	    protected int? _LineNbr;
	    [PXDBInt(IsKey = true)]
	    [PXDefault(0)]
	    [PXUIField(DisplayName = "Line Nbr", Enabled = false, Visible = false)]
	    public virtual int? LineNbr
	    {
	        get
	        {
	            return this._LineNbr;
	        }
	        set
	        {
	            this._LineNbr = value;
	        }
	    }
        #endregion
        #region SortOrder
        public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

        protected Int32? _SortOrder;
        [PXUIField(DisplayName = PX.Objects.AP.APTran.sortOrder.DispalyName, Visible = false, Enabled = false)]
        [PXDefault(0)]
        [PXDBInt]
        public virtual Int32? SortOrder
        {
            get
            {
                return this._SortOrder;
            }
            set
            {
                this._SortOrder = value;
            }
        }
        #endregion
        #region IsPlan
        /// <summary>
        /// Indicates if the record is plan order (most likely out of MRP planning)
        /// Plan orders should not have an Actual order in AMProdItem as of 6.0 release
        /// </summary>
        public abstract class isPlan : PX.Data.BQL.BqlBool.Field<isPlan> { }

        protected bool? _IsPlan;
        /// <summary>
        /// Indicates if the record is plan order (most likely out of MRP planning)
        /// Plan orders should not have an Actual order in AMProdItem as of 6.0 release
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Is Plan", Enabled = false)]
        public virtual bool? IsPlan
        {
            get
            {
                return this._IsPlan;
            }
            set
            {
                this._IsPlan = value;
            }
        }
        #endregion
        #region IsMRP
        /// <summary>
        /// When MRP Reruns it will reset the IsPlan records for this field back to false.
        /// Then rerun and if the same order is picked back up to reschedule it will contain a true value
        /// </summary>
        public abstract class isMRP : PX.Data.BQL.BqlBool.Field<isMRP> { }

        protected bool? _IsMRP;
        /// <summary>
        /// When MRP Reruns it will reset the IsPlan records for this field back to false.
        /// Then rerun and if the same order is picked back up to reschedule it will contain a true value
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Is MRP", Enabled = false)]
        public virtual bool? IsMRP
        {
            get
            {
                return this._IsMRP;
            }
            set
            {
                this._IsMRP = value;
            }
        }
        #endregion
        #region ConstDate
        public abstract class constDate : PX.Data.BQL.BqlDateTime.Field<constDate> { }

		protected DateTime? _ConstDate;
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Constraint", Enabled = false)]
		public virtual DateTime? ConstDate
		{
			get
			{
				return this._ConstDate;
			}
			set
			{
				this._ConstDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

		protected DateTime? _EndDate;
		[PXDBDateAndTime(DisplayNameDate = "End Date", DisplayNameTime = "End Time")]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "End Date")]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion
		#region MachID
		public abstract class machID : PX.Data.BQL.BqlString.Field<machID> { }

		protected string _MachID;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Machine ID")]
		public virtual string MachID
		{
			get
			{
				return this._MachID;
			}
			set
			{
				this._MachID = value;
			}
		}
		#endregion
		#region QueueTime
		public abstract class queueTime : PX.Data.BQL.BqlInt.Field<queueTime> { }

		[OperationDBTime]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Queue Time")]
		public virtual Int32? QueueTime { get; set; }
		#endregion
		#region SetupTime
		public abstract class setupTime : PX.Data.BQL.BqlInt.Field<setupTime> { }

		[OperationDBTime]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Setup Time")]
		public virtual Int32? SetupTime { get; set; }
		#endregion
		#region FinishTime
		public abstract class finishTime : PX.Data.BQL.BqlInt.Field<finishTime> { }

		[OperationDBTime]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Finish Time")]
		public virtual Int32? FinishTime { get; set; }
		#endregion
		#region MoveTime
		public abstract class moveTime : PX.Data.BQL.BqlInt.Field<moveTime> { }

	    [OperationDBTime]
	    [PXDefault(0)]
	    [PXUIField(DisplayName = "Move Time")]
	    public virtual Int32? MoveTime { get; set; }
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
		#region QtyComplete
		public abstract class qtyComplete : PX.Data.BQL.BqlDecimal.Field<qtyComplete> { }

		protected decimal? _QtyComplete;
		[PXDBDecimal(25)]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "QtyComplete")]
		public virtual decimal? QtyComplete
		{
			get
			{
				return this._QtyComplete;
			}
			set
			{
				this._QtyComplete = value;
			}
		}
		#endregion
		#region QtyScrapped
		public abstract class qtyScrapped : PX.Data.BQL.BqlDecimal.Field<qtyScrapped> { }

		protected decimal? _QtyScrapped;
		[PXDBDecimal(25)]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "QtyScrapped")]
		public virtual decimal? QtyScrapped
		{
			get
			{
				return this._QtyScrapped;
			}
			set
			{
				this._QtyScrapped = value;
			}
		}
		#endregion
		#region QtytoProd
		public abstract class qtytoProd : PX.Data.BQL.BqlDecimal.Field<qtytoProd> { }

		protected decimal? _QtytoProd;
		[PXDBDecimal(25)]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty to Produce")]
		public virtual decimal? QtytoProd
		{
			get
			{
				return this._QtytoProd;
			}
			set
			{
				this._QtytoProd = value;
			}
		}
        #endregion
	    #region QtyRemaining

	    public abstract class qtyRemaining : PX.Data.BQL.BqlDecimal.Field<qtyRemaining> { }

	    protected Decimal? _QtyRemaining;
	    [PXQuantity()]
	    [PXFormula(typeof(Switch<Case<Where<Current<AMPSetup.inclScrap>, Equal<True>>, SubNotLessThanZero<AMSchdOper.qtytoProd, Add<AMSchdOper.qtyComplete, AMSchdOper.qtyScrapped>>>,
	        SubNotLessThanZero<AMSchdOper.qtytoProd, AMSchdOper.qtyComplete>>))]
	    [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
	    [PXUIField(DisplayName = "Qty Remaining", Enabled = false)]
	    public virtual Decimal? QtyRemaining
	    {
	        get
	        {
	            return _QtyRemaining;
	        }
	        set
	        {
	            _QtyRemaining = value;
	        }
	    }
	    #endregion
        #region SchedulingMethod
        public abstract class schedulingMethod : PX.Data.BQL.BqlString.Field<schedulingMethod> { }

	    protected string _SchedulingMethod;
	    [PXDBString(1, IsFixed = true)]
	    [PXDefault(ScheduleMethod.StartOn)]
	    [PXUIField(DisplayName = "Scheduling Method")]
	    [ScheduleMethod.List]
	    public virtual string SchedulingMethod
        {
	        get
	        {
	            return this._SchedulingMethod;
	        }
	        set
	        {
	            this._SchedulingMethod = value;
	        }
	    }
        #endregion
        #region FirmSchedule
        public abstract class firmSchedule : PX.Data.BQL.BqlBool.Field<firmSchedule> { }

        protected bool? _FirmSchedule;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Firm Schedule", Visible = false, Enabled = false)]
        public virtual bool? FirmSchedule
        {
            get
            {
                return this._FirmSchedule;
            }
            set
            {
                this._FirmSchedule = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		protected int? _SiteID;
		[PXDBInt()]
		[PXDefault(0)]
		[PXUIField(DisplayName = "SiteID")]
		public virtual int? SiteID
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
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

		protected DateTime? _StartDate;
		[PXDBDateAndTime(DisplayNameDate = "Start Date", DisplayNameTime = "Start Time")]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
        #endregion
	    #region TotalPlanTime
	    public abstract class actualLaborTime : PX.Data.BQL.BqlInt.Field<actualLaborTime> { }

	    protected Int32? _TotalPlanTime;
	    [ProductionTotalTimeDB]
	    [PXDefault(0)]
	    [PXUIField(DisplayName = "Total Plan Time", Enabled = false)]
	    public virtual Int32? TotalPlanTime
        {
	        get
	        {
	            return this._TotalPlanTime;
	        }
	        set
	        {
	            this._TotalPlanTime = value;
	        }
	    }
        #endregion
		#region WcID
		public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

		protected string _WcID;
	    [WorkCenterIDField]
		public virtual string WcID
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
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual byte[] tstamp
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
		#region QueueStartDate
		public abstract class queueStartDate : PX.Data.BQL.BqlDateTime.Field<queueStartDate> { }

		protected DateTime? _QueueStartDate;
		[PXDBDateAndTime(DisplayNameDate = "Queue Start Date", DisplayNameTime = "Queue Start Time")]
		[PXUIField(DisplayName = "Queue Start Date", Enabled =false)]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		public virtual DateTime? QueueStartDate
		{
			get
			{
				return this._QueueStartDate;
			}
			set
			{
				this._QueueStartDate = value;
			}
		}
		#endregion
		#region MoveEndDate
		public abstract class moveEndDate : PX.Data.BQL.BqlDateTime.Field<moveEndDate> { }

		protected DateTime? _MoveEndDate;
		[PXDBDateAndTime(DisplayNameDate = "Move End Date", DisplayNameTime = "Move End Time")]
		[PXUIField(DisplayName = "Move End Date", Enabled =false)]
		[PXDefault(TypeCode.DateTime, "01/01/1900")]
		public virtual DateTime? MoveEndDate
		{
			get
			{
				return this._MoveEndDate;
			}
			set
			{
				this._MoveEndDate = value;
			}
		}
		#endregion
		#region StatusID
		public abstract class statusID : PX.Data.BQL.BqlString.Field<statusID> { }

		protected String _StatusID;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(ProductionOrderStatus.Planned)]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		[ProductionOrderStatus.List]
		public virtual String StatusID
		{
			get
			{
				return this._StatusID;
			}
			set
			{
				this._StatusID = value;
			}
		}
		#endregion
	}
}
