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
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing MRP Preferences table
    /// </summary>
    [PXPrimaryGraph(typeof(MRPSetupMaint))]
    [PXCacheName(PX.Objects.AM.Messages.MrpSetup)]
    [Serializable]
    public class AMRPSetup : IBqlTable
    {
        #region IncludeOnHoldSalesOrder
        public abstract class includeOnHoldSalesOrder : PX.Data.BQL.BqlBool.Field<includeOnHoldSalesOrder> { }

        protected bool? _IncludeOnHoldSalesOrder;
        /// <summary>
        /// Defines if a sales order on hold should be included as Demand in MRP
        /// Previously AMRPDefaults.Admin
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Include On Hold Sales Orders")]
        public virtual bool? IncludeOnHoldSalesOrder
        {
            get
            {
                return this._IncludeOnHoldSalesOrder;
            }
            set
            {
                this._IncludeOnHoldSalesOrder = value;
            }
        }
        #endregion
        #region IncludeOnHoldPurchaseOrder
        public abstract class includeOnHoldPurchaseOrder : PX.Data.BQL.BqlBool.Field<includeOnHoldPurchaseOrder> { }

        protected bool? _IncludeOnHoldPurchaseOrder;
        /// <summary>
        /// Defines if a purchase order on hold should be included as Supply in MRP
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Include On Hold Purchase Orders")]
        public virtual bool? IncludeOnHoldPurchaseOrder
        {
            get
            {
                return this._IncludeOnHoldPurchaseOrder;
            }
            set
            {
                this._IncludeOnHoldPurchaseOrder = value;
            }
        }
        #endregion
        #region IncludeOnHoldProductionOrder
        public abstract class includeOnHoldProductionOrder : PX.Data.BQL.BqlBool.Field<includeOnHoldProductionOrder> { }

        protected bool? _IncludeOnHoldProductionOrder;
        /// <summary>
        /// Defines if a production order on hold should be included as supply (Item) and demand (material) in MRP
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Include On Hold Production Orders", FieldClass = nameof(FeaturesSet.ManufacturingMRP))]
        public virtual bool? IncludeOnHoldProductionOrder
        {
            get
            {
                return this._IncludeOnHoldProductionOrder;
            }
            set
            {
                this._IncludeOnHoldProductionOrder = value;
            }
        }
        #endregion
        #region ExceptionDaysBefore
        public abstract class exceptionDaysBefore : PX.Data.BQL.BqlInt.Field<exceptionDaysBefore> { }

        protected int? _ExceptionDaysBefore;
        /// <summary>
        /// Exception days before
        /// Previously AMRPDefaults.EXWin
        /// </summary>
        [PXDBInt]
        [PXDefault(10)]
        [PXUIField(DisplayName = "Days Before")]
        public virtual int? ExceptionDaysBefore
        {
            get
            {
                return this._ExceptionDaysBefore;
            }
            set
            {
                this._ExceptionDaysBefore = value;
            }
        }
        #endregion
        #region ExceptionDaysAfter
        public abstract class exceptionDaysAfter : PX.Data.BQL.BqlInt.Field<exceptionDaysAfter> { }

        protected int? _ExceptionDaysAfter;
        /// <summary>
        /// Exception days after
        /// Previously AMRPDefaults.EXWin1
        /// </summary>
        [PXDBInt]
        [PXDefault(10)]
        [PXUIField(DisplayName = "Days After")]
        public virtual int? ExceptionDaysAfter
        {
            get
            {
                return this._ExceptionDaysAfter;
            }
            set
            {
                this._ExceptionDaysAfter = value;
            }
        }
        #endregion
        #region ForecastPlanHorizon
        /// <summary>
        /// Demand Time Fence (days)
        /// </summary>
        public abstract class forecastPlanHorizon : PX.Data.BQL.BqlInt.Field<forecastPlanHorizon> { }

        protected int? _ForecastPlanHorizon;
        /// <summary>
        /// Demand Time Fence (days)
        /// </summary>
        [PXDBInt]
        [PXDefault(30)]
        [PXUIField(DisplayName = "Demand Time Fence")]
        public virtual int? ForecastPlanHorizon
        {
            get
            {
                return this._ForecastPlanHorizon;
            }
            set
            {
                this._ForecastPlanHorizon = value;
            }
        }
        #endregion
        #region ForecastNumberingID

        public abstract class forecastNumberingID : PX.Data.BQL.BqlString.Field<forecastNumberingID> { }

        protected String _ForecastNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Numbering Sequence", Visibility = PXUIVisibility.Visible)]
        public virtual String ForecastNumberingID
        {
            get
            {
                return this._ForecastNumberingID;
            }
            set
            {
                this._ForecastNumberingID = value;
            }
        }
        #endregion
        #region PurchaseCalendarID
        public abstract class purchaseCalendarID : PX.Data.BQL.BqlString.Field<purchaseCalendarID> { }

        protected String _PurchaseCalendarID;
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Purchase Calendar ID")]
        [PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
        [PXForeignReference(typeof(Field<AMRPSetup.purchaseCalendarID>.IsRelatedTo<CSCalendar.calendarID>))]
        public virtual String PurchaseCalendarID
        {
            get
            {
                return this._PurchaseCalendarID;
            }
            set
            {
                this._PurchaseCalendarID = value;
            }
        }
        #endregion
        #region UseFixMfgLeadTime
        public abstract class useFixMfgLeadTime : PX.Data.BQL.BqlBool.Field<useFixMfgLeadTime> { }

        protected bool? _UseFixMfgLeadTime;
        /// <summary>
        /// Flag indicating if the MRP regen process should use fixed manufacturing lead times if checked
        /// Previously AMRPDefaults.MFGLead
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Use Fixed Manufacturing Times")]
        public virtual bool? UseFixMfgLeadTime
        {
            get
            {
                return this._UseFixMfgLeadTime;
            }
            set
            {
                this._UseFixMfgLeadTime = value;
            }
        }
        #endregion
        #region MPSFence
        public abstract class mPSFence : PX.Data.BQL.BqlInt.Field<mPSFence> { }

        protected int? _MPSFence;
        /// <summary>
        /// MPS Time Fence (days)
        /// </summary>
        [PXDBInt()]
        [PXDefault(30)]
        [PXUIField(DisplayName = "Time Fence")]
        public virtual int? MPSFence
        {
            get
            {
                return this._MPSFence;
            }
            set
            {
                this._MPSFence = value;
            }
        }
        #endregion
        #region DefaultMPSTypeID
        public abstract class defaultMPSTypeID : PX.Data.BQL.BqlString.Field<defaultMPSTypeID> { }

        protected string _DefaultMPSTypeID;
        /// <summary>
        /// Default MPS Type ID when creating new MPS entires
        /// </summary>
        [PXDBString(20, IsUnicode = true)]
        [PXUIField(DisplayName = "Default Type")]
        [PXSelector(typeof(Search<AMMPSType.mPSTypeID>))]
        public virtual string DefaultMPSTypeID
        {
            get
            {
                return this._DefaultMPSTypeID;
            }
            set
            {
                this._DefaultMPSTypeID = value;
            }
        }
        #endregion
        #region GracePeriod
        public abstract class gracePeriod : PX.Data.BQL.BqlInt.Field<gracePeriod> { }

        protected int? _GracePeriod;
        /// <summary>
        /// MRP Grace Period (days)
        /// Previously AMRPDefaults.PlnH
        /// </summary>
        [PXDBInt]
        [PXDefault(30)]
        [PXUIField(DisplayName = "Grace Period")]
        public virtual int? GracePeriod
        {
            get
            {
                return this._GracePeriod;
            }
            set
            {
                this._GracePeriod = value;
            }
        }
        #endregion
        #region StockingMethod
        public abstract class stockingMethod : PX.Data.BQL.BqlInt.Field<stockingMethod> { }

        protected int? _StockingMethod;
        [PXDBInt]
        [PXDefault(MRPStockingMethod.SafetyStock)]
        [PXUIField(DisplayName = "Stocking Method")]
        [MRPStockingMethod.List]
        public virtual int? StockingMethod
        {
            get
            {
                return this._StockingMethod;
            }
            set
            {
                this._StockingMethod = value;
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
        #region PlanOrderType
        public abstract class planOrderType : PX.Data.BQL.BqlString.Field<planOrderType> { }

        protected String _PlanOrderType;

		/// <summary>
		/// Plan order type
		/// </summary>
		[AMOrderTypeField(DisplayName = "Plan Order Type", FieldClass = nameof(FeaturesSet.ManufacturingMRP))]
        [PXRestrictor(typeof(Where<AMOrderType.function, Equal<OrderTypeFunction.planning>>), AM.Messages.IncorrectOrderTypeFunction)]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
        [PXDefault]
        public virtual String PlanOrderType
        {
            get
            {
                return this._PlanOrderType;
            }
            set
            {
                this._PlanOrderType = value;
            }
        }
        #endregion
        #region LastMrpRegenCompletedByID

        public abstract class lastMrpRegenCompletedByID : PX.Data.BQL.BqlGuid.Field<lastMrpRegenCompletedByID> { }

        protected Guid? _LastMrpRegenCompletedByID;
        [PXDBGuid]
        [PXUIField(DisplayName = "Last Regen Completed By", Enabled = false, IsReadOnly = true, Visible = false)]
        public virtual Guid? LastMrpRegenCompletedByID
        {
            get
            {
                return this._LastMrpRegenCompletedByID;
            }
            set
            {
                this._LastMrpRegenCompletedByID = value;
            }
        }
        #endregion
        #region LastMrpRegenCompletedDateTime

        public abstract class lastMrpRegenCompletedDateTime : PX.Data.BQL.BqlDateTime.Field<lastMrpRegenCompletedDateTime> { }

        protected DateTime? _LastMrpRegenCompletedDateTime;
        [PXDBDateAndTime]
        [PXUIField(DisplayName = "Last Regen Completed At", Enabled = false, IsReadOnly = true, Visible = false)]
        public virtual DateTime? LastMrpRegenCompletedDateTime
        {
            get
            {
                return this._LastMrpRegenCompletedDateTime;
            }
            set
            {
                this._LastMrpRegenCompletedDateTime = value;
            }
        }
        #endregion
		#region UseDaysSupplytoConsolidateOrders
		public abstract class useDaysSupplytoConsolidateOrders: PX.Data.BQL.BqlBool.Field<useDaysSupplytoConsolidateOrders> { }
		[PXDBBool]
		[PXUIField(DisplayName="Use Days of Supply to Consolidate Orders")]
		[PXDefault(false)]
		public virtual bool? UseDaysSupplytoConsolidateOrders{ get; set; }
		#endregion
		#region UseLongTermConsolidationBucket
		public abstract class useLongTermConsolidationBucket : PX.Data.BQL.BqlBool.Field<useLongTermConsolidationBucket> { }
		[PXDBBool]
		[PXUIField(DisplayName="Use Long-Term Consolidation Bucket")]
		[PXDefault(false)]
		public virtual bool? UseLongTermConsolidationBucket { get; set; }
		#endregion
		#region ConsolidateAfterDays
		public abstract class consolidateAfterDays : PX.Data.BQL.BqlInt.Field<consolidateAfterDays> { }
		[PXDBInt]
		[PXUIField(DisplayName="Consolidate After (Days)", Visibility = PXUIVisibility.Invisible, Visible = false)]
		[PXDefault(TypeCode.Int32, "0")]
		public virtual int? ConsolidateAfterDays { get; set; }
		#endregion
		#region BucketDays
		public abstract class bucketDays : PX.Data.BQL.BqlInt.Field<bucketDays> { }
		[PXDBInt]
		[PXUIField(DisplayName="Bucket (Days)", Visibility = PXUIVisibility.Invisible, Visible = false)]
		[PXDefault(TypeCode.Int32, "0")]
		public virtual  int? BucketDays { get; set; }
		#endregion
		#region IncludeExpiredBlanketSalesOrders
		public abstract class includeExpiredBlanketSalesOrders : PX.Data.BQL.BqlBool.Field<includeExpiredBlanketSalesOrders> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Include Expired Blanket Sales Orders")]
		[PXDefault(false)]
		public virtual bool? IncludeExpiredBlanketSalesOrders { get; set; }
		#endregion

		#region Setup Constants

		public static class MRPStockingMethod
        {
            public const int SafetyStock = 1;
            public const int ReorderPoint = 2;

            public class ListAttribute : PXIntListAttribute
            {
                public ListAttribute()
                    : base(
                        new int[] {
                            SafetyStock,
                            ReorderPoint },
                        new string[] {
                            Messages.StockingMethodSafetyStock,
                            Messages.StockingMethodReorderPoint })
                {
                }
            }

            public class safetyStock : PX.Data.BQL.BqlInt.Constant<safetyStock>
            {
                public safetyStock()
                    : base(SafetyStock) { }
            }

            public class reorderPoint : PX.Data.BQL.BqlInt.Constant<reorderPoint>
            {
                public reorderPoint()
                    : base(ReorderPoint) { }
            }
        }

        #endregion
    }
}
