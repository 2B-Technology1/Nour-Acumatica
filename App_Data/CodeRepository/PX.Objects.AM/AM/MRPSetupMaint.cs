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

namespace PX.Objects.AM
{
    /// <summary>
    /// Graph for Manufacturing MRP Preferences
    /// </summary>
    
    public class MRPSetupMaint : PXGraph<MRPSetupMaint>
    {
        public PXSelect<AMRPSetup> setup;
        public PXSave<AMRPSetup> Save;
        public PXCancel<AMRPSetup> Cancel;

		public MRPSetupMaint()
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.manufacturingMRP>())
			{
				PXDefaultAttribute.SetPersistingCheck<AMRPSetup.planOrderType>(setup.Cache, null, PXPersistingCheck.Nothing);
			}
		}

		protected virtual void AMRPSetup_ExceptionDaysAfter_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            var amrpSetup = (AMRPSetup)e.Row;
            if ( (int)e.NewValue > amrpSetup.GracePeriod)
            {
                throw new PXSetPropertyException(Messages.GetLocal(AM.Messages.MrpSetupExceptionWindow,
                    PXUIFieldAttribute.GetDisplayName<AMRPSetup.exceptionDaysAfter>(setup.Cache),
                    PXUIFieldAttribute.GetDisplayName<AMRPSetup.gracePeriod>(setup.Cache)));
            }
        }

        protected virtual void AMRPSetup_GracePeriod_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            var amrpSetup = (AMRPSetup)e.Row;
            if ((int)e.NewValue < amrpSetup.ExceptionDaysAfter)
            {
                throw new PXSetPropertyException(Messages.GetLocal(AM.Messages.MrpSetupExceptionWindow,
                    PXUIFieldAttribute.GetDisplayName<AMRPSetup.exceptionDaysAfter>(setup.Cache),
                    PXUIFieldAttribute.GetDisplayName<AMRPSetup.gracePeriod>(setup.Cache)));
            }
        }

        protected virtual void AMRPSetup_UseFixMfgLeadTime_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            var amrpSetup = (AMRPSetup)e.Row;
            if (amrpSetup == null || (bool?) e.NewValue != true)
            {
                return;
            }

            var ampSetup = (AMPSetup)PXSelect<AMPSetup>.Select(this);
            if (ampSetup?.FixMfgCalendarID != null)
            {
                return;
            }

            throw new PXSetPropertyException(Messages.GetLocal(Messages.MrpFixedLeadTimeRequiresProdPreferencesCalendar));
        }

		protected virtual void _(Events.FieldUpdated<AMRPSetup, AMRPSetup.useDaysSupplytoConsolidateOrders> e)
		{
			if ((bool?)e.NewValue == false && e.Row.UseLongTermConsolidationBucket == true)
			{
				e.Cache.SetValueExt<AMRPSetup.useLongTermConsolidationBucket>(e.Row, false);
			}
		}

		protected virtual void _(Events.RowSelected<AMRPSetup> e)
		{
			bool enableFields = e.Row != null && e.Row.UseDaysSupplytoConsolidateOrders == true;
			bool hideFields = e.Row != null && e.Row.UseLongTermConsolidationBucket == true;

			PXUIFieldAttribute.SetEnabled<AMRPSetup.useLongTermConsolidationBucket>(e.Cache, e.Row, enableFields);
			PXUIFieldAttribute.SetVisible<AMRPSetup.consolidateAfterDays>(e.Cache, e.Row, hideFields);
			PXUIFieldAttribute.SetVisible<AMRPSetup.bucketDays>(e.Cache, e.Row, hideFields);
		}
	}

}
