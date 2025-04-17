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
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing - Regenerate MRP process page
    /// </summary>
    public class Fullregen : PXGraph<Fullregen>
    {
        public PXProcessing<MrpProcessingSetup> MrpProcessing;
        public PXSelect<AMRPAuditTable> AuditDetailRecs;
        public PXSetup<AMRPSetup> setup;

        // Turn off the new UI processing window (19R1+)
        public override bool IsProcessing
        {
            get { return false; }
            set { }
        }

		public Fullregen()
        {
            //check for setup record entered
            var setupCache = setup.Cache;
            if (setupCache == null || setupCache.Current == null)
            {
                throw new PXSetupNotEnteredException(AM.Messages.GetLocal(AM.Messages.SetupNotEntered), typeof(AMRPSetup), AM.Messages.GetLocal(AM.Messages.MrpSetup));
            }

            MrpProcessing.SetProcessDelegate(ProcessRegen);
            MrpProcessing.SetProcessEnabled(false);
            MrpProcessing.SetProcessVisible(false);
            MrpProcessing.SetProcessAllVisible(true);
            MrpProcessing.SetProcessAllCaption(PX.Objects.IN.Messages.Process);
        }

        public static void ProcessRegen(List<MrpProcessingSetup> list)
        {
            var fullRegen = CreateInstance<Fullregen>();
            var uid = GetProcessID(list);
            if (uid != null)
            {
                // Required to correctly check to see if MRP is running now that we are not 
                //  passing the original graph which started the process.
                fullRegen.UID = uid;
            }

            CreateInstance<MRPEngine>().Run(fullRegen);
        }

        protected static Guid? GetProcessID(List<MrpProcessingSetup> list)
        {
            return list == null || list.Count == 0 ? null : list[0]?.ProcessID;
        }

		public PXAction<MrpProcessingSetup> MrpHistory;
		[PXUIField(DisplayName = "History", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual System.Collections.IEnumerable mrpHistory(PXAdapter adapter)
		{
			var productionSummaryGi = new GIMRPHistory();
			productionSummaryGi.CallGenericInquiry();

			return adapter.Get();
		}

	}

    public class DefaultProcessIDAttribute : PXUnboundDefaultAttribute
    {
        public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            try
            {
                e.NewValue = sender.Graph.UID;
            }
            catch
            {
                // ignored
            }
        }
    }

    /// <summary>
    /// Regenerate MRP processing setup record.
    /// (AMRPSetup Projection)
    /// </summary>
    [Serializable]
    [PXProjection(typeof(Select<AMRPSetup>))]
    [PXCacheName(PX.Objects.AM.Messages.MrpSetup)]
    public class MrpProcessingSetup : IBqlTable, IPXSelectable
    {
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected")]
        public bool? Selected { get; set; }
        #endregion
        #region ProcessID
        public abstract class processID : PX.Data.BQL.BqlGuid.Field<processID> { }
        protected Guid? _ProcessID;
        [PXGuid]
        [PXUIField(Visible = false)]
        [DefaultProcessID]
        public virtual Guid? ProcessID
        {
            get
            {
                return this._ProcessID;
            }
            set
            {
                this._ProcessID = value;
            }
        }
        #endregion
        #region LastMrpRegenCompletedByID

        public abstract class lastMrpRegenCompletedByID : PX.Data.BQL.BqlGuid.Field<lastMrpRegenCompletedByID> { }
        protected Guid? _LastMrpRegenCompletedByID;
        [PXDBCreatedByID(DisplayName = "Last Completed By", DontOverrideValue = true, BqlField = typeof(AMRPSetup.lastMrpRegenCompletedByID))]
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
        [PXDBDateAndTime(BqlField = typeof(AMRPSetup.lastMrpRegenCompletedDateTime))]
        [PXUIField(DisplayName = "Last Completed At", Enabled = false, IsReadOnly = true, Visible = true)]
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
    }
}
