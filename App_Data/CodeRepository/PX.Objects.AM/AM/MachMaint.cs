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
using PX.Data.BQL.Fluent;
using PX.Objects.Common.GraphExtensions;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Machine Maintenance
    /// </summary>
    public class MachMaint : PXGraph<MachMaint, AMMach>
	{
        public PXSelect<AMMach> Machines;
        public PXSelect<AMMach, Where<AMMach.machID, Equal<Current<AMMach.machID>>>> MachSelected;
		public SelectFrom<AMMachCurySettings>.Where<AMMachCurySettings.machID.IsEqual<AMMach.machID.FromCurrent>
			.And<AMMachCurySettings.curyID.IsEqual<AccessInfo.baseCuryID.AsOptional>>>.View MachCurySelected;
		[PXHidden]
        public PXSelect<AMWCMach> MachineRecords;
		public PXSelect<AMWCMachCury> MachineCuryRecords;

		public class CurySettings : CurySettingsExtension<MachMaint, AMMach, AMMachCurySettings>
		{
			public static bool IsActive() => true;
		}

		protected virtual void AMMach_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
			AMMach machinerecord = (AMMach)e.Row;
            if (machinerecord != null && machinerecord.ActiveFlg == true)
            {
                // Check for Machine in Work Centers where Machine Override is false
                AMWCMach amMachRecord = PXSelectJoin<AMWCMach, InnerJoin<AMWC, On<AMWCMach.wcID, Equal<AMWC.wcID>>>,
                    Where<AMWCMach.machID, Equal<Required<AMWCMach.machID>>,
                        And<AMWC.activeFlg, Equal<True>>>>.Select(this, machinerecord.MachID);

                // Check for Machine in Work Centers where Machine Override is false
                if (amMachRecord != null)
                {
                    e.Cancel = true;
                    throw new PXException(Messages.GetLocal(Messages.MachineUsedinActiveWorkCenter), amMachRecord.WcID);
                }
            }
        }

        protected virtual void AMMach_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            var row = (AMMach)e.Row;
            if (row == null || e.TranStatus != PXTranStatus.Open )
            {
                return;
            }

            if (e.Operation != PXDBOperation.Update)
            {
                return;
            }

            foreach (PXResult<AMWCMach, AMWCMachCury> result in PXSelectJoin<AMWCMach,
				LeftJoin<AMWCMachCury, On<AMWCMachCury.wcID, Equal<AMWCMach.wcID>,
					And<AMWCMachCury.detailID, Equal<AMWCMach.machID>,
					And<AMWCMachCury.curyID, Equal<Current<AccessInfo.baseCuryID>>>>>,
                InnerJoin<AMWC, On<AMWCMach.wcID, Equal<AMWC.wcID>>>>,
                    Where<AMWCMach.machID, Equal<Required<AMWCMach.machID>>,
                        And<AMWCMach.machineOverride, Equal<False>, 
                        And<AMWC.activeFlg, Equal<True>>>>>.Select(this, row.MachID))
                {
				var amMachRecord = (AMWCMach)result;
				var amMachCuryRecord = (AMWCMachCury)result;
                amMachRecord.MachAcctID = row.MachAcctID;
                amMachRecord.MachSubID = row.MachSubID;
                MachineRecords.Cache.Update(amMachRecord);

				if(amMachCuryRecord == null)
				{
					amMachCuryRecord = (AMWCMachCury)MachineCuryRecords.Cache.Insert(new AMWCMachCury {
						WcID = amMachRecord.WcID,
						DetailID = amMachRecord.MachID,
						CuryID = Accessinfo.BaseCuryID
					});
				}
				amMachCuryRecord.StdCost = row.StdCost;
				MachineCuryRecords.Cache.Update(amMachRecord);
            }
        }

        protected virtual void AMMach_MachID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            var newValueString = Convert.ToString(e.NewValue);
            if (string.IsNullOrWhiteSpace(newValueString))
            {
                return;
            }
            // Prevent silly users from entering leading spaces...
            e.NewValue = newValueString.TrimStart();
        }
    }
}
