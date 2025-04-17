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

namespace PX.Objects.AM
{
	/// <summary>
	/// Table for a single result of processing the APS Maintenance Process (AM512000) form and the <see cref="APSMaintenanceProcess"/> graph.
	/// </summary>
	[PXCacheName("APS Maintenance Setup")]
    [Serializable]
    public class AMAPSMaintenanceSetup : IBqlTable
    {
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        protected bool? _Selected = false;
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion

        #region WorkCenterCalendarProcessLastRunDateTime
        public abstract class workCenterCalendarProcessLastRunDateTime : PX.Data.BQL.BqlDateTime.Field<workCenterCalendarProcessLastRunDateTime> { }

        [PXDBDate(InputMask = "g", PreserveTime = true, UseTimeZone = true)]
        [PXUIField(DisplayName = "Work Center Calendar Process Last Run Date Time", Enabled = false)]
        public virtual DateTime? WorkCenterCalendarProcessLastRunDateTime { get; set; }
        #endregion

        #region WorkCenterCalendarProcessLastRunByID
        public abstract class workCenterCalendarProcessLastRunByID : PX.Data.BQL.BqlGuid.Field<workCenterCalendarProcessLastRunByID> { }

        [PXDBGuid]
        [PXUIField(DisplayName = "Work Center Calendar Process Last Run By", Enabled = false)]
        public virtual Guid? WorkCenterCalendarProcessLastRunByID { get; set; }
        #endregion

        #region BlockSizeSyncProcessLastRunDateTime
        public abstract class blockSizeSyncProcessLastRunDateTime : PX.Data.BQL.BqlDateTime.Field<blockSizeSyncProcessLastRunDateTime> { }

        [PXDBDate(InputMask = "g", PreserveTime = true, UseTimeZone = true)]
        [PXUIField(DisplayName = "Block Size Sync Process Last Run Date Time", Enabled = false)]
        public virtual DateTime? BlockSizeSyncProcessLastRunDateTime { get; set; }
        #endregion

        #region BlockSizeSyncProcessLastRunByID
        public abstract class blockSizeSyncProcessLastRunByID : PX.Data.BQL.BqlGuid.Field<blockSizeSyncProcessLastRunByID> { }

        [PXDBGuid]
        [PXUIField(DisplayName = "Block Size Sync Process Last Run By", Enabled = false)]
        public virtual Guid? BlockSizeSyncProcessLastRunByID { get; set; }
        #endregion

        #region LastBlockSize
        public abstract class lastBlockSize : PX.Data.BQL.BqlInt.Field<lastBlockSize> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Last Block Size", Enabled = false)]
        [SchdBlockSizeList]
        public virtual int? LastBlockSize { get; set; }
        #endregion

        #region CurrentBlockSize
        public abstract class currentBlockSize : PX.Data.BQL.BqlInt.Field<currentBlockSize> { }

        [PXInt]
        [PXUIField(DisplayName = "Current Block Size", Enabled = false)]
        [SchdBlockSizeList]
        [PXUnboundDefault(typeof(Search<AMPSetup.schdBlockSize>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? CurrentBlockSize { get; set; }
        #endregion

        #region HistoryCleanupProcessLastRunDateTime
        public abstract class historyCleanupProcessLastRunDateTime : PX.Data.BQL.BqlDateTime.Field<historyCleanupProcessLastRunDateTime> { }

        [PXDBDate(InputMask = "g", PreserveTime = true, UseTimeZone = true)]
        [PXUIField(DisplayName = "History Cleanup Process Last Run Date Time", Enabled = false)]
        public virtual DateTime? HistoryCleanupProcessLastRunDateTime { get; set; }
        #endregion

        #region HistoryCleanupProcessLastRunByID
        public abstract class historyCleanupProcessLastRunByID : PX.Data.BQL.BqlGuid.Field<historyCleanupProcessLastRunByID> { }

        [PXDBGuid]
        [PXUIField(DisplayName = "History Cleanup Process Last Run By", Enabled = false, Visible = false)]
        public virtual Guid? HistoryCleanupProcessLastRunByID { get; set; }
        #endregion

        #region WorkCalendarProcessLastRunDateTime
        public abstract class workCalendarProcessLastRunDateTime : PX.Data.BQL.BqlDateTime.Field<workCalendarProcessLastRunDateTime> { }

        [PXDBDate(InputMask = "g", PreserveTime = true, UseTimeZone = true)]
        [PXUIField(DisplayName = "Work Calendar Process Last Run Date Time", Enabled = false)]
        public virtual DateTime? WorkCalendarProcessLastRunDateTime { get; set; }
        #endregion

        #region WorkCalendarProcessLastRunByID
        public abstract class workCalendarProcessLastRunByID : PX.Data.BQL.BqlGuid.Field<workCalendarProcessLastRunByID> { }

        [PXDBGuid]
        [PXUIField(DisplayName = "Work Calendar Process Last Run By", Enabled = false, Visible = false)]
        public virtual Guid? WorkCalendarProcessLastRunByID { get; set; }
        #endregion

        #region LastModifiedByID

        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion

        #region LastModifiedByScreenID

        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        public virtual String LastModifiedByScreenID { get; set; }
        #endregion

        #region LastModifiedDateTime

        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
    }
}
