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
using PX.Objects.CS;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Production Preferences table
    /// </summary>
    [PXPrimaryGraph(typeof(ProdSetup))]
    [PXCacheName(Messages.ProductionSetup)]
    [Serializable]
	public class AMPSetup : IBqlTable
	{
        #region MoveNumberingID

        public abstract class moveNumberingID : PX.Data.BQL.BqlString.Field<moveNumberingID> { }

        protected String _MoveNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Move Numbering Sequence", Visibility = PXUIVisibility.Visible)]
        public virtual String MoveNumberingID
        {
            get
            {
                return this._MoveNumberingID;
            }
            set
            {
                this._MoveNumberingID = value;
            }
        }
        #endregion
        #region LaborNumberingID

        public abstract class laborNumberingID : PX.Data.BQL.BqlString.Field<laborNumberingID> { }

        protected String _LaborNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Labor Numbering Sequence", Visibility = PXUIVisibility.Visible)]
        public virtual String LaborNumberingID
        {
            get
            {
                return this._LaborNumberingID;
            }
            set
            {
                this._LaborNumberingID = value;
            }
        }
        #endregion
        #region MaterialNumberingID

        public abstract class materialNumberingID : PX.Data.BQL.BqlString.Field<materialNumberingID> { }

        protected String _MaterialNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Material Numbering Sequence", Visibility = PXUIVisibility.Visible)]
        public virtual String MaterialNumberingID
        {
            get
            {
                return this._MaterialNumberingID;
            }
            set
            {
                this._MaterialNumberingID = value;
            }
        }
        #endregion
        #region WipAdjustNumberingID

        public abstract class wipAdjustNumberingID : PX.Data.BQL.BqlString.Field<wipAdjustNumberingID> { }

        protected String _WipAdjustNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Wip Adjust Numbering Sequence", Visibility = PXUIVisibility.Visible)]
        public virtual String WipAdjustNumberingID
        {
            get
            {
                return this._WipAdjustNumberingID;
            }
            set
            {
                this._WipAdjustNumberingID = value;
            }
        }
        #endregion
        #region ProdGLNumberingID

        public abstract class prodCostNumberingID : PX.Data.BQL.BqlString.Field<prodCostNumberingID> { }

        protected String _ProdCostNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Cost Numbering Sequence", Visibility = PXUIVisibility.Visible)]
        public virtual String ProdCostNumberingID
        {
            get
            {
                return this._ProdCostNumberingID;
            }
            set
            {
                this._ProdCostNumberingID = value;
            }
        }
        #endregion
        #region DfltLbrRate
        public abstract class dfltLbrRate : PX.Data.BQL.BqlString.Field<dfltLbrRate> { }

		protected String _DfltLbrRate;
		[PXDBString(1, IsFixed = true)]
        [PXDefault(LaborRateType.Standard)]
		[PXUIField(DisplayName = "Use Labor Rate")]
        [LaborRateType.List]
		public virtual String DfltLbrRate
		{
			get
			{
				return this._DfltLbrRate;
			}
			set
			{
				this._DfltLbrRate = value;
			}
		}
		#endregion
		#region FMLTime
		public abstract class fMLTime : PX.Data.BQL.BqlBool.Field<fMLTime> { }

		protected Boolean? _FMLTime;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Fixed Manufacturing Times")]
		public virtual Boolean? FMLTime
		{
			get
			{
				return this._FMLTime;
			}
			set
			{
				this._FMLTime = value;
			}
		}
		#endregion
		#region FMLTMRPOrdorOP
		public abstract class fMLTMRPOrdorOP : PX.Data.BQL.BqlBool.Field<fMLTMRPOrdorOP> { }

		protected Boolean? _FMLTMRPOrdorOP;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Order Start Date for MRP")]
		public virtual Boolean? FMLTMRPOrdorOP
		{
			get
			{
				return this._FMLTMRPOrdorOP;
			}
			set
			{
				this._FMLTMRPOrdorOP = value;
			}
		}
		#endregion
		#region InclScrap
		public abstract class inclScrap : PX.Data.BQL.BqlBool.Field<inclScrap> { }

		protected Boolean? _InclScrap;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include Scrap in Completions")]
		public virtual Boolean? InclScrap
		{
			get
			{
				return this._InclScrap;
			}
			set
			{
				this._InclScrap = value;
			}
		}
		#endregion
        #region MachineScheduling
        public abstract class machineScheduling : PX.Data.BQL.BqlBool.Field<machineScheduling> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Machine Scheduling")]
        public virtual Boolean? MachineScheduling { get; set; }
        #endregion
        #region ToolScheduling
        public abstract class toolScheduling : PX.Data.BQL.BqlBool.Field<toolScheduling> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Tool Scheduling", FieldClass = Features.ADVANCEDPLANNINGFIELDCLASS)]
        public virtual Boolean? ToolScheduling { get; set; }
        #endregion
        #region UseShiftCrewSize
        /// <summary>
        /// When scheduling a production order should the process use crew size and efficiency
        /// Default is false to match logic before upgrade. Requires APS
        /// </summary>
        public abstract class useShiftCrewSize : PX.Data.BQL.BqlBool.Field<useShiftCrewSize> { }
        /// <summary>
        /// When scheduling a production order should the process use crew size and efficiency
        /// Default is false to match logic before upgrade. Requires APS
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Use Shift Crew Size")]
        public virtual Boolean? UseShiftCrewSize { get; set; }
        #endregion
        #region IncludeFinishTimeInCapacity
        /// <summary>
        /// When scheduling a production order will operation finish time consume capacity at the work center
        /// Requires APS
        /// </summary>
        public abstract class includeFinishTimeInCapacity : PX.Data.BQL.BqlBool.Field<includeFinishTimeInCapacity> { }
        /// <summary>
        /// When scheduling a production order will operation finish time consume capacity at the work center
        /// Requires APS
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Consume Capacity for Finish Time")]
        public virtual Boolean? IncludeFinishTimeInCapacity { get; set; }
        #endregion
        #region IncludeQueueTimeInCapacity
        /// <summary>
        /// When scheduling a production order will operation queue time consume capacity at the work center
        /// Requires APS
        /// </summary>
        public abstract class includeQueueTimeInCapacity : PX.Data.BQL.BqlBool.Field<includeQueueTimeInCapacity> { }
        /// <summary>
        /// When scheduling a production order will operation queue time consume capacity at the work center
        /// Requires APS
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Consume Capacity for Queue Time")]
        public virtual Boolean? IncludeQueueTimeInCapacity { get; set; }
        #endregion
        #region FixMfgCalendarID
        public abstract class fixMfgCalendarID : PX.Data.BQL.BqlString.Field<fixMfgCalendarID> { }

        protected String _FixMfgCalendarID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fixed Mfg Calendar ID")]
        [PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
        [PXForeignReference(typeof(Field<AMPSetup.fixMfgCalendarID>.IsRelatedTo<CSCalendar.calendarID>))]
        public virtual String FixMfgCalendarID
        {
            get
            {
                return this._FixMfgCalendarID;
            }
            set
            {
                this._FixMfgCalendarID = value;
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
        #region SummPost

        public abstract class summPost : PX.Data.BQL.BqlBool.Field<summPost> { }

        protected Boolean? _SummPost;
        [PXDBBool]
        [PXUIField(DisplayName = "Post Summary on Updating GL")]
        [PXDefault(false)]
        public virtual Boolean? SummPost
        {
            get
            {
                return this._SummPost;
            }
            set
            {
                this._SummPost = value;
            }
        }
        #endregion
        #region HoldEntry
        public abstract class holdEntry : PX.Data.BQL.BqlBool.Field<holdEntry> { }

        protected Boolean? _HoldEntry;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Hold Documents on Entry")]
        public virtual Boolean? HoldEntry
        {
            get
            {
                return this._HoldEntry;
            }
            set
            {
                this._HoldEntry = value;
            }
        }
        #endregion
        #region RequireControlTotal

        public abstract class requireControlTotal : PX.Data.BQL.BqlBool.Field<requireControlTotal> { }

        protected Boolean? _RequireControlTotal;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Validate Document Totals on Entry")]
        public virtual Boolean? RequireControlTotal
        {
            get
            {
                return this._RequireControlTotal;
            }
            set
            {
                this._RequireControlTotal = value;
            }
        }
        #endregion
        #region FMLTimeUnits
        public abstract class fMLTimeUnits : PX.Data.BQL.BqlInt.Field<fMLTimeUnits> { }

        protected int? _FMLTimeUnits;
        [PXDBInt]
        [PXDefault(TimeUnits.Days, PersistingCheck = PXPersistingCheck.Nothing)]
        [TimeUnits.LeadTimeList]
        [PXUIField(DisplayName = "Fixed Mfg Units")]
        public virtual int? FMLTimeUnits
        {
            get
            {
                return this._FMLTimeUnits;
            }
            set
            {
                this._FMLTimeUnits = value;
            }
        }
        #endregion
        #region SchdBlockSize
        /// <summary>
        /// Scheduling block size (in minutes) for APS
        /// </summary>
        public abstract class schdBlockSize : PX.Data.BQL.BqlInt.Field<schdBlockSize> { }

        protected int? _SchdBlockSize;
        /// <summary>
        /// Scheduling block size (in minutes) for APS
        /// </summary>
        [PXDBInt]
        [PXUIField(DisplayName = "Block Size", FieldClass = Features.ADVANCEDPLANNINGFIELDCLASS)]
        [PXDefault(30)]
        [SchdBlockSizeList]
        public virtual int? SchdBlockSize
        {
            get
            {
                return this._SchdBlockSize;
            }
            set
            {
                this._SchdBlockSize = value;
            }
        }
        #endregion
        #region DefaultEmployee
        public abstract class defaultEmployee : PX.Data.BQL.BqlBool.Field<defaultEmployee> { }

        protected Boolean? _DefaultEmployee;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Default User Employee ID")]
        public virtual Boolean? DefaultEmployee
        {
            get
            {
                return this._DefaultEmployee;
            }
            set
            {
                this._DefaultEmployee = value;
            }
        }
        #endregion
        #region DefaultOrderType
        public abstract class defaultOrderType : PX.Data.BQL.BqlString.Field<defaultOrderType> { }

        protected String _DefaultOrderType;
        [AMOrderTypeField(DisplayName = "Default Order Type")]
        [PXRestrictor(typeof(Where<AMOrderType.function, Equal<OrderTypeFunction.regular>>), Messages.IncorrectOrderTypeFunction)]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
        public virtual String DefaultOrderType
        {
            get
            {
                return this._DefaultOrderType;
            }
            set
            {
                this._DefaultOrderType = value;
            }
        }
        #endregion
        #region CTPOrderType
        public abstract class cTPOrderType : PX.Data.BQL.BqlString.Field<cTPOrderType> { }

        protected String _CTPOrderType;
        [AMOrderTypeField(DisplayName = "Capable to Promise Order Type")]
        [PXRestrictor(typeof(Where<AMOrderType.function, Equal<OrderTypeFunction.planning>>), Messages.IncorrectOrderTypeFunction)]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
        public virtual String CTPOrderType
        {
            get
            {
                return this._CTPOrderType;
            }
            set
            {
                this._CTPOrderType = value;
            }
        }
        #endregion
        #region DisassemblyNumberingID

        public abstract class disassemblyNumberingID : PX.Data.BQL.BqlString.Field<disassemblyNumberingID> { }

	    protected String _DisassemblyNumberingID;
	    [PXDBString(10, IsUnicode = true)]
	    [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
	    [PXUIField(DisplayName = "Disassembly Numbering Sequence", Visibility = PXUIVisibility.Visible)]
	    public virtual String DisassemblyNumberingID
        {
	        get
	        {
	            return this._DisassemblyNumberingID;
	        }
	        set
	        {
	            this._DisassemblyNumberingID = value;
	        }
	    }
        #endregion
	    #region DefaultDisassembleOrderType
	    public abstract class defaultDisassembleOrderType : PX.Data.BQL.BqlString.Field<defaultDisassembleOrderType> { }

	    protected String _DefaultDisassembleOrderType;
	    [AMOrderTypeField(DisplayName = "Default Disassemble Order Type")]
	    [PXRestrictor(typeof(Where<AMOrderType.function, Equal<OrderTypeFunction.disassemble>>), Messages.IncorrectOrderTypeFunction)]
	    [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
	    [AMOrderTypeSelector]
	    public virtual String DefaultDisassembleOrderType
        {
	        get
	        {
	            return this._DefaultDisassembleOrderType;
	        }
	        set
	        {
	            this._DefaultDisassembleOrderType = value;
	        }
	    }
        #endregion
        #region VendorShipmentNumberingID

        public abstract class vendorShipmentNumberingID : PX.Data.BQL.BqlString.Field<vendorShipmentNumberingID> { }

        protected String _VendorShipmentNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Vendor Shipment Numbering Sequence", Visibility = PXUIVisibility.Visible)]
        public virtual String VendorShipmentNumberingID
        {
            get
            {
                return this._VendorShipmentNumberingID;
            }
            set
            {
                this._VendorShipmentNumberingID = value;
            }
        }
        #endregion
        #region HoldShipmentsOnEntry
        public abstract class holdShipmentsOnEntry : PX.Data.BQL.BqlBool.Field<holdShipmentsOnEntry> { }

        protected Boolean? _HoldShipmentsOnEntry;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Hold Shipments on Entry")]
        public virtual Boolean? HoldShipmentsOnEntry
        {
            get
            {
                return this._HoldShipmentsOnEntry;
            }
            set
            {
                this._HoldShipmentsOnEntry = value;
            }
        }
        #endregion
        #region ValidateShipmentTotalOnConfirm
        public abstract class validateShipmentTotalOnConfirm : PX.Data.BQL.BqlBool.Field<validateShipmentTotalOnConfirm> { }

        protected Boolean? _ValidateShipmentTotalOnConfirm;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Validate Shipment Total on Confirmation")]
        public virtual Boolean? ValidateShipmentTotalOnConfirm
        {
            get
            {
                return this._ValidateShipmentTotalOnConfirm;
            }
            set
            {
                this._ValidateShipmentTotalOnConfirm = value;
            }
        }
        #endregion
        #region RestrictClockCurrentUser
        public abstract class restrictClockCurrentUser : PX.Data.BQL.BqlBool.Field<restrictClockCurrentUser> { }

        protected Boolean? _RestrictClockCurrentUser;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Restrict Clock Entry to Current User")]
        public virtual Boolean? RestrictClockCurrentUser
        {
            get
            {
                return this._RestrictClockCurrentUser;
            }
            set
            {
                this._RestrictClockCurrentUser = value;
            }
        }
		#endregion

		#region LockWorkflowEnabled
		public abstract class lockWorkflowEnabled : PX.Data.BQL.BqlBool.Field<lockWorkflowEnabled> { }

		protected Boolean? _LockWorkflowEnabled;
		/// <summary>
		/// Lock status on production order preferrence
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Lock Production Orders before Closing")]
		public virtual Boolean? LockWorkflowEnabled
		{
			get
			{
				return this._LockWorkflowEnabled;
			}
			set
			{
				this._LockWorkflowEnabled = value;
			}
		}
		#endregion

		#region Methods

		public static AMPSetup CheckSetup(PXGraph graph)
        {
            AMPSetup ampSetup = PXSelect<AMPSetup>.Select(graph);
            
            CheckSetup(ampSetup);

            return ampSetup;
        }

        public static void CheckSetup(AMPSetup ampSetup)
        {
            if (ampSetup == null)
            {
                throw new ProductionSetupNotEnteredException();
            }
        }

        public static bool CheckSetup(AMPSetup ampSetup, out Exception exception)
        {
            exception = null;
            if (ampSetup == null)
            {
                exception = new PXException(Messages.GetLocal(Messages.SetupNotEntered, Messages.GetLocal(Messages.ProductionSetup)));
                return true;
            }

            return false;
        }

        #endregion
    }
}
