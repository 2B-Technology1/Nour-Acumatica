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
using PX.Data.BQL.Fluent;
using static PX.Objects.AM.AMWC;
using PX.Objects.CM;

namespace PX.Objects.AM
{
	/// <summary>
	/// Maintenance table for work centers on the Work Centers (AM205500) form (corresponding to the <see cref="WCMaint"/> graph).
	/// </summary>
	[Serializable]
    [PXCacheName(Messages.WorkCenter)]
    [PXPrimaryGraph(typeof(WCMaint))]
	public class AMWC : IBqlTable, INotable
    {
        #region Keys
        public class PK : PrimaryKeyOf<AMWC>.By<wcID>
        {
            public static AMWC Find(PXGraph graph, string wcID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, wcID, options);
        }

        public static class FK
        {
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMWC>.By<siteID> { }
        }
        #endregion

        #region WcID
        public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

        protected String _WcID;
        [WorkCenterIDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
        [PXSelector(typeof(Search<AMWC.wcID>),
            DescriptionField = typeof(AMWC.descr))]
        [PXReferentialIntegrityCheck]
        public virtual String WcID
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
		#region ActiveFlg
		public abstract class activeFlg : PX.Data.BQL.BqlBool.Field<activeFlg> { }

		protected Boolean? _ActiveFlg;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? ActiveFlg
		{
			get
			{
				return this._ActiveFlg;
			}
			set
			{
				this._ActiveFlg = value;
			}
		}
		#endregion
		#region BflushLbr
		public abstract class bflushLbr : PX.Data.BQL.BqlBool.Field<bflushLbr> { }

		protected Boolean? _BflushLbr;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Backflush Labor")]
		public virtual Boolean? BflushLbr
		{
			get
			{
				return this._BflushLbr;
			}
			set
			{
				this._BflushLbr = value;
			}
		}
		#endregion
		#region BflushMatl
		public abstract class bflushMatl : PX.Data.BQL.BqlBool.Field<bflushMatl> { }

		protected Boolean? _BflushMatl;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Backflush Materials")]
		public virtual Boolean? BflushMatl
		{
			get
			{
				return this._BflushMatl;
			}
			set
			{
				this._BflushMatl = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected String _Descr;
		[PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote]
        public virtual Guid? NoteID
        {
            get
            {
                return this._NoteID;
            }
            set
            {
                this._NoteID = value;
            }
        }
        #endregion
		#region OutsideFlg
		public abstract class outsideFlg : PX.Data.BQL.BqlBool.Field<outsideFlg> { }

		protected Boolean? _OutsideFlg;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Outside Process")]
		public virtual Boolean? OutsideFlg
		{
			get
			{
				return this._OutsideFlg;
			}
			set
			{
				this._OutsideFlg = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		protected Int32? _SiteID;
        [AMSite(Visibility = PXUIVisibility.SelectorVisible)]
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
		#region StdCost
		[Obsolete("This field is obsolete and will be removed in later Acumatica versions. Use AMWCCurySettings.stdCost")]
		public abstract class stdCost : PX.Data.BQL.BqlDecimal.Field<stdCost> { }

		[Obsolete("This field is obsolete and will be removed in later Acumatica versions. Use AMWCCurySettings.StdCost")]
		protected Decimal? _StdCost;
		[PXDBPriceCost(MinValue = 0.0)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Standard Cost")]
		public virtual Decimal? StdCost
		{
			get
			{
				return this._StdCost;
			}
			set
			{
				this._StdCost = value;
			}
		}
		#endregion
		#region WcBasis
		public abstract class wcBasis : PX.Data.BQL.BqlString.Field<wcBasis> { }

		protected String _WcBasis;
		[PXDBString(1, IsFixed=true)]
		[PXDefault(BasisForCapacity.CrewSize)]
		[PXUIField(DisplayName = "Basis for Capacity")]
        [BasisForCapacity.List]
		public virtual String WcBasis
		{
			get
			{
				return this._WcBasis;
			}
			set
			{
				this._WcBasis = value;
			}
		}
		#endregion
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        protected Int32? _LocationId;
        [Location(typeof(AMWC.siteID), Visible = false)]
        [PXForeignReference(typeof(CompositeKey<Field<siteID>.IsRelatedTo<INLocation.siteID>, Field<locationID>.IsRelatedTo<INLocation.locationID>>))]
        public virtual Int32? LocationID
        {
            get
            {
                return this._LocationId;
            }
            set
            {
                this._LocationId = value;
            }
        }
		#endregion
		#region Default Queue Time
		public abstract class defaultQueueTime: PX.Data.BQL.BqlInt.Field<defaultQueueTime> { }

		protected Int32? _DefaultQueueTime;
		[OperationDBTime]
		[PXUIField(DisplayName = "Default Queue Time")]
		[PXDefault(0,typeof(Search<AMBSetup.defaultQueueTime>))]
		public virtual Int32? DefaultQueueTime
		{
			get
			{
				return this._DefaultQueueTime;
			}
			set
			{
				this._DefaultQueueTime = value;
			}
		}
		#endregion
		#region Default Finish Time
		public abstract class defaultFinishTime : PX.Data.BQL.BqlInt.Field<defaultFinishTime> { }

		protected Int32? _DefaultFinishTime;
		[OperationDBTime]
		[PXUIField(DisplayName = "Default Finish Time")]
		[PXDefault(0,typeof(Search<AMBSetup.defaultFinishTime>))]
		public virtual Int32? DefaultFinishTime
		{
			get
			{
				return this._DefaultFinishTime;
			}
			set
			{
				this._DefaultFinishTime = value;
			}
		}
		#endregion
		#region Default Move Time
		public abstract class defaultMoveTime : PX.Data.BQL.BqlInt.Field<defaultMoveTime> { }

		protected Int32? _DefaultMoveTime;
		[OperationDBTime]
		[PXUIField(DisplayName = "Default Move Time")]
		[PXDefault(0,typeof(Search<AMBSetup.defaultMoveTime>))]
		public virtual Int32? DefaultMoveTime
		{
			get
			{
				return this._DefaultMoveTime;
			}
			set
			{
				this._DefaultMoveTime = value;
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
        #region ScrapAction
        public abstract class scrapAction : PX.Data.BQL.BqlInt.Field<scrapAction> { }

        protected int? _ScrapAction;
        [PXDBInt]
        [PXDefault(Attributes.ScrapAction.NoAction)]
        [PXUIField(DisplayName = "Scrap Action Default")]
        [ScrapAction.List]
        public virtual int? ScrapAction
        {
            get
            {
                return this._ScrapAction;
            }
            set
            {
                this._ScrapAction = value;
            }
        }
		#endregion
		#region AllowMultiClockEntry
		public abstract class allowMultiClockEntry : PX.Data.BQL.BqlBool.Field<allowMultiClockEntry> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Clock Entry for Multiple Production Orders")]
		public virtual bool? AllowMultiClockEntry { get; set; }
		#endregion

		#region ControlPoint
		public abstract class controlPoint : PX.Data.BQL.BqlInt.Field<controlPoint> { }

		protected bool? _ControlPoint;
		[ControlPoint(Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(false)]
		public virtual bool? ControlPoint
		{
			get
			{
				return this._ControlPoint;
			}
			set
			{
				this._ControlPoint = value;
			}
		}
		#endregion

		#region Methods/Attributes
		public static class BasisForCapacity
        {
            //Constants declaration 
            public const string CrewSize = "1";
            public const string Machines = "0";

            //List attribute 
            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                    : base(
                    new string[] { CrewSize, Machines },
                    new string[] { "Crew Size", "Machines" }) { ; }
            }

            //BQL constants declaration
            public class crewSize : PX.Data.BQL.BqlString.Constant<crewSize>
            {
                public crewSize() : base(CrewSize) { ;}
            }
            public class machines : PX.Data.BQL.BqlString.Constant<machines>
            {
                public machines() : base(Machines) { ;}
            }

        }
        #endregion



    }

	/// <summary>
	/// Projection of the AMWCCurySettings table that contains cost information for a work center.
	/// </summary>
	[Serializable]
	[PXCacheName("AMWCCurrency")]
	[PXProjection(typeof(SelectFrom<AMWCCurySettings>
	.Where<AMWCCurySettings.detailID.IsEqual<DetailType.wc>>), Persistent = true)]
	public class AMWCCury : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<AMWCCury>.By<wcID, curyID>
		{
			public static AMWCCury Find(PXGraph graph, string wcID, string curyID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, wcID, curyID, options);
		}
		public static class FK
		{
			public class WorkCenter : AMWC.PK.ForeignKeyOf<AMWCCury>.By<wcID> { }
		}
		#endregion


		#region WcID
		public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

		protected String _WcID;
		[WorkCenterIDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, Enabled = false, BqlField = typeof(AMWCCurySettings.wcID))]
		[PXDBDefault(typeof(AMWC.wcID))]
		[PXParent(typeof(FK.WorkCenter))]
		public virtual String WcID
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
		#region DetailID
		public abstract class detailID : PX.Data.BQL.BqlString.Field<detailID> { }

		protected String _DetailID;
		[PXDBString(30, IsKey = true, BqlField = typeof(AMWCCurySettings.detailID))]
		[PXUIField(DisplayName = "Detail", Enabled = false)]
		[PXDefault(DetailType.WC)]
		public virtual String DetailID
		{
			get
			{
				return this._DetailID;
			}
			set
			{
				this._DetailID = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		protected String _CuryID;
		[PXDBString(IsUnicode = true, IsKey = true, BqlField = typeof(AMWCCurySettings.curyID))]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		[PXSelector(typeof(Search<CurrencyList.curyID>))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion		
		#region StdCost
		public abstract class stdCost : PX.Data.BQL.BqlDecimal.Field<stdCost> { }

		protected Decimal? _StdCost;
		[PXDBPriceCost(MinValue = 0.0, BqlField = typeof(AMWCCurySettings.stdCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Standard Cost")]
		public virtual Decimal? StdCost
		{
			get
			{
				return this._StdCost;
			}
			set
			{
				this._StdCost = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected Byte[] _tstamp;
		[PXDBTimestamp(BqlField = typeof(AMWCCurySettings.Tstamp))]
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
		#region CreatedByID

		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		protected Guid? _CreatedByID;
		[PXDBCreatedByID(BqlField = typeof(AMWCCurySettings.createdByID))]
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
		[PXDBCreatedByScreenID(BqlField = typeof(AMWCCurySettings.createdByScreenID))]
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
		[PXDBCreatedDateTime(BqlField = typeof(AMWCCurySettings.createdDateTime))]
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
		[PXDBLastModifiedByID(BqlField = typeof(AMWCCurySettings.lastModifiedByID))]
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
		[PXDBLastModifiedByScreenID(BqlField = typeof(AMWCCurySettings.lastModifiedByScreenID))]
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
		[PXDBLastModifiedDateTime(BqlField = typeof(AMWCCurySettings.lastModifiedDateTime))]
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
	}

	#region Types/Attributes
	public static class DetailType
	{
		public const string WC = "-WC";

		public class wc : PX.Data.BQL.BqlString.Constant<wc>
		{
			public wc() : base(WC) { }
		}
	}
	#endregion
}
