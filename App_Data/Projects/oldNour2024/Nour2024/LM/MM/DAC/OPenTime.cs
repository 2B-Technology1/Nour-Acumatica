using System;
using PX.Data;
using PX.Objects.IN;
using PX.Data.BQL;

namespace MyMaintaince
{
	
	
	[System.SerializableAttribute()]
	public class OPenTime : PX.Data.IBqlTable
	{
        public abstract class statusConatant : IBqlField
        {
            public const string Open = "Open";
            public const string Clos = "Close";

            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                    : base(
                        new string[] { Open, Clos },
                        new string[] { Open, Clos }) { ;}
            }
            public class open :BqlString.Constant<open>
            {
                public open() : base(Open) { ;}
            }
            public class clos : BqlString.Constant<clos>
            {
                public clos() : base(Clos) { ;}
            }
        }
        
        #region JobOrderID
        public abstract class jobOrderID : PX.Data.IBqlField
        {
        }
        protected string _JobOrderID;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Job Order ID")]
        //[PXDBDefault(typeof(JobOrder.jobOrdrID))]
        /*[PXParent(typeof(
        Select<JobOrder,
        Where<JobOrder.jobOrdrID,
        Equal<Current<OPenTïme.jobOrderID>>>>))]*/
        public virtual string JobOrderID
        {
            get
            {
                return this._JobOrderID;
            }
            set
            {
                this._JobOrderID = value;
            }
        }
        #endregion
        #region JobTimeID
        public abstract class jobTimeID : PX.Data.IBqlField
        {
        }
        protected string _JobTimeID;
        [PXDBString(50, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Open Time ID")]
        [PXSelector(typeof(Search2<JobTime.Code, InnerJoin<JobTimeAttachedNonStocks, On<JobTimeAttachedNonStocks.jobTimeID, Equal<JobTime.Code>>>, Where<JobTimeAttachedNonStocks.nonStockID, Equal<Current<OPenTime.inventoryCode>>>>)
                    , new Type[] { typeof(JobTime.Code), typeof(JobTime.name) }
                    , DescriptionField = typeof(JobTime.name)
                    , SubstituteKey = typeof(JobTime.Code))]
        /**
        [PXSelector(typeof(Search<JobTimeAttachedNonStocks.jobTimeID, Where<JobTimeAttachedNonStocks.nonStockID, Equal<Current<OPenTime.inventoryCode>>>>)
                    , new Type[] { typeof(JobTimeAttachedNonStocks.jobTimeID), typeof(JobTime.name) }
                    , DescriptionField = typeof(JobTime.name)
                    , SubstituteKey = typeof(JobTime.Code))]
        **/
        public virtual string JobTimeID
        {
            get
            {
                return this._JobTimeID;
            }
            set
            {
                this._JobTimeID = value;
            }
        }
        #endregion
        #region Code >> InventoryCode
        public abstract class inventoryCode : PX.Data.IBqlField
        {
        }
        protected int? _InventoryCode;
        [PXUIField(DisplayName = "Service/Non-Stock Item")]
        [PXDefault()]
        [PXDBInt]
        [PXSelector(typeof(Search2<InventoryItem.inventoryID, InnerJoin<JobOrderNonStockItems, On<JobOrderNonStockItems.inventoryCode,Equal<InventoryItem.inventoryID>>>, Where<JobOrderNonStockItems.jobOrdrID, Equal<Current<OPenTime.jobOrderID>>>>)
                   ,new Type[]{
                      typeof(InventoryItem.inventoryCD),
                      typeof(InventoryItem.descr),
                      typeof(InventoryItem.itemType),
                      typeof(InventoryItem.itemClassID),
                      typeof(InventoryItem.itemStatus),
                      typeof(InventoryItem.baseUnit),
                      typeof(InventoryItem.salesUnit),
                      typeof(InventoryItem.purchaseUnit),
                   }
                   , DescriptionField = typeof(InventoryItem.descr)
                   , SubstituteKey = typeof(InventoryItem.inventoryCD))]
		
        public virtual int? InventoryCode
        {
            get
            {
                return this._InventoryCode;
            }
            set
            {
                this._InventoryCode = value;
            }
        }
        #endregion
        #region OpenTimeID   just lineID
		public abstract class openTimeID : PX.Data.IBqlField
		{
		}
		protected int? _OpenTimeID;
		[PXDBIdentity(IsKey = true)]
        [PXUIField(DisplayName = "Open Time Id",IsReadOnly=true)]
        public virtual int? OpenTimeID
		{
			get
			{
				return this._OpenTimeID;
			}
			set
			{
				this._OpenTimeID = value;
			}
		}
		#endregion
        #region StartTime actually this is the start Date
		public abstract class startTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartTime;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName="Start Date")]
		public virtual DateTime? StartTime
		{
			get
			{
				return this._StartTime;
			}
			set
			{
				this._StartTime = value;
			}
		}
		#endregion
        #region StarTime
        public abstract class starTime : PX.Data.IBqlField
        {
        }
        protected string _StarTime;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Start Time")]

        public virtual string StarTime
        {
            get
            {
                return this._StarTime;
            }
            set
            {
                this._StarTime = value;
            }
        }
        #endregion
        #region Status
		public abstract class status : PX.Data.IBqlField
		{
		}
		protected string _Status;
		[PXDBString(20, IsUnicode = true)]
		[statusConatant.List()]
        [PXDefault(typeof(statusConatant.open))]
        [PXUIField(DisplayName = "Status")]
		public virtual string Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
        #region EndeTime actually this is the End Date
        public abstract class endeTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _EndeTime;
		[PXDBDate()]
		[PXUIField(DisplayName = "End Date")]
		public virtual DateTime? EndeTime
		{
			get
			{
				return this._EndeTime;
			}
			set
			{
				this._EndeTime = value;
			}
		}
		#endregion
        #region EndTime
        public abstract class endTime : PX.Data.IBqlField
        {
        }
        protected string _EndTime;
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "End Time")]
        public virtual string EndTime
        {
            get
            {
                return this._EndTime;
            }
            set
            {
                this._EndTime = value;
            }
        }
        #endregion
        #region Dauration
        public abstract class dauration : PX.Data.IBqlField
        {
        }
        protected string _Dauration;
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Duration",Enabled=false)]
        public virtual string Dauration
        {
            get
            {
                return this._Dauration;
            }
            set
            {
                this._Dauration = value;
            }
        }
        #endregion
        #region ManualDauration
        public abstract class manualDauration : PX.Data.IBqlField
        {
        }
        protected string _ManualDauration;
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "User Specific Duration")]
        public virtual string ManualDauration
        {
            get
            {
                return this._ManualDauration;
            }
            set
            {
                this._ManualDauration = value;
            }
        }
        #endregion
        #region Variation
        public abstract class variation : PX.Data.IBqlField
        {
        }
        protected string _Variation;
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Variation")]
        public virtual string Variation
        {
            get
            {
                return this._Variation;
            }
            set
            {
                this._Variation = value;
            }
        }
        #endregion
        #region Descrption
		public abstract class descrption : PX.Data.IBqlField
		{
		}
		protected string _Descrption;
		[PXDBString(2147483647, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
		public virtual string Descrption
		{
			get
			{
				return this._Descrption;
			}
			set
			{
				this._Descrption = value;
			}
		}
		#endregion
		#region Close
		public abstract class clse : PX.Data.IBqlField
		{
		}
		protected bool? _Clse;
		[PXDBBool()]
        [PXUIField(DisplayName="Close")]
		public virtual bool? Clse
		{
			get
			{
				return this._Clse;
			}
			set
			{
				this._Clse = value;
			}
		}
		#endregion
		
	}
}
