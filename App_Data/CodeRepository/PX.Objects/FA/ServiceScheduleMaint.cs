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

namespace PX.Objects.FA
{
    public class ServiceScheduleMaint : PXGraph<ServiceScheduleMaint>
    {
        public PXCancel<FAServiceSchedule> Cancel;
		public PXSavePerRow<FAServiceSchedule, FAServiceSchedule.scheduleID> Save;
		#region Selects Declaration
        public PXSelect<FAServiceSchedule> ServiceSchedule;
		public PXSetup<FASetup> FASetup;
		#endregion

		#region Constructor
		public ServiceScheduleMaint() 
		{
			FASetup setup = FASetup.Current;
		}

        #endregion

		#region
		protected virtual void FAServiceSchedule_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			FAServiceSchedule sch = (FAServiceSchedule)e.Row;
			if (e.Operation == PXDBOperation.Delete)
			{
				if (PXSelect<FixedAsset, Where<FixedAsset.serviceScheduleID, Equal<Current<FAServiceSchedule.scheduleID>>>>.SelectSingleBound(this, new object[] { e.Row }).Count > 0)
				{
					throw new PXRowPersistingException("ScheduleCD", sch.ScheduleCD, Messages.ScheduleExistsHistory);
				}
			}
		}

		protected virtual void FAServiceSchedule_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			FAServiceSchedule sch = (FAServiceSchedule)e.Row;
			
			if (PXSelect<FixedAsset, Where<FixedAsset.serviceScheduleID, Equal<Current<FAServiceSchedule.scheduleID>>>>.SelectSingleBound(this, new object[] { e.Row }).Count > 0)
			{
				throw new PXSetPropertyException(Messages.ScheduleExistsHistory);    	
			}
		}
		#endregion
    }		
}