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
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
	/// <summary>
	/// Is a projection DAC over <see cref="EPEquipmentDetail"/>, <see cref="EPEquipmentTimeCard"/>,
	/// <see cref="EPEquipment"/>, and <see cref="DailyFieldReportEquipment"/> classes.
	/// The records of this type are created and edited through the <b>Equipment</b> tab of the Daily Field Report (PJ304000) form
	/// (which corresponds to the <see cref="DailyFieldReportEntry"/> graph).
	/// </summary>
	[EquipmentProjection]
    [PXBreakInheritance]
    [PXCacheName("Daily Field Report Equipment")]
    public class EquipmentProjection : EPEquipmentDetail
    {
        [PXDBIdentity(BqlField = typeof(DailyFieldReportEquipment.dailyFieldReportEquipmentId), IsKey = true)]
        [PXExtraKey]
        public virtual int? DailyFieldReportEquipmentId
        {
            get;
            set;
        }

        [PXDBInt(BqlField = typeof(DailyFieldReportEquipment.dailyFieldReportId))]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId.FromCurrent>>))]
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDBString(BqlField = typeof(DailyFieldReportEquipment.equipmentTimeCardCd))]
        [PXUIField(DisplayName = "Time Card Ref.", IsReadOnly = true)]
        public virtual string EquipmentTimeCardCd
        {
            get;
            set;
        }

        [PXDBInt(BqlField = typeof(DailyFieldReportEquipment.equipmentDetailLineNumber))]
        public virtual int? EquipmentDetailLineNumber
        {
            get;
            set;
        }

        [PXDefault]
        [PXDBInt(BqlField = typeof(EPEquipmentTimeCard.equipmentID))]
        [PXUIField(DisplayName = "Equipment ID")]
        [PXSelector(typeof(SearchFor<EPEquipment.equipmentID>.
                Where<EPEquipment.status.IsEqual<EPEquipmentStatus.EquipmentStatusActive>>),
            SubstituteKey = typeof(EPEquipment.equipmentCD))]
        public virtual int? EquipmentId
        {
            get;
            set;
        }

        [PXDBString(10, InputMask = ">CCCCCCCCCC", IsUnicode = true)]
        public override string TimeCardCD
        {
            get;
            set;
        }

        [PXDBString(BqlField = typeof(EPEquipment.description))]
        [PXDefault(typeof(SearchFor<EPEquipment.description>
            .Where<EPEquipment.equipmentID.IsEqual<equipmentId.FromCurrent>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<equipmentId>))]
        [PXUIField(DisplayName = "Equipment Description", Visibility = PXUIVisibility.SelectorVisible,
            Enabled = false)]
        public virtual string EquipmentDescription
        {
            get;
            set;
        }

        [PXDBString(1, BqlField = typeof(EPEquipmentTimeCard.status))]
        [PXDefault(EPEquipmentTimeCardStatusAttribute.OnHold)]
        [EPEquipmentTimeCardStatus]
        public virtual string TimeCardStatus
        {
            get;
            set;
        }

        [PXDBDate(BqlField = typeof(date))]
        [PXDefault(typeof(DailyFieldReport.date.FromCurrent))]
        [PXUIField(DisplayName = "Date")]
        public override DateTime? Date
        {
            get;
            set;
        }

        [PXDefault(typeof(DailyFieldReport.projectId.FromCurrent))]
        [EPEquipmentActiveProject]
        public override int? ProjectID
        {
            get;
            set;
        }

        [PXDefault(typeof(SearchFor<PMTask.taskID>
            .Where<PMTask.projectID.IsEqual<DailyFieldReport.projectId.FromCurrent>
                .And<PMTask.type.IsNotEqual<ProjectTaskType.revenue>>
                .And<PMTask.isDefault.IsEqual<True>>>))]
        [EPTimecardProjectTask(typeof(DailyFieldReport.projectId), BatchModule.TA, DisplayName = "Project Task",
            BqlField = typeof(projectTaskID), Required = true)]
        [PXRestrictor(typeof(Where<PMTask.type.IsNotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable)]
        public override int? ProjectTaskID
        {
            get;
            set;
        }

        [CostCode(null, typeof(projectTaskID), AccountType.Expense, Required = true, BqlField = typeof(costCodeID))]
        public override int? CostCodeID
        {
            get;
            set;
        }

        [PXUIField(DisplayName = "Last Modification Date", IsReadOnly = true)]
        [PXDBLastModifiedDateTime]
        public override DateTime? LastModifiedDateTime
        {
            get;
            set;
        }

        public abstract class dailyFieldReportEquipmentId : BqlInt.Field<dailyFieldReportEquipmentId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class equipmentTimeCardCd : BqlString.Field<equipmentTimeCardCd>
        {
        }

        public abstract class equipmentDetailLineNumber : BqlInt.Field<equipmentDetailLineNumber>
        {
        }

        public abstract class equipmentId : BqlInt.Field<equipmentId>
        {
        }

        public abstract class equipmentDescription : BqlString.Field<equipmentDescription>
        {
        }

        public abstract class timeCardStatus : BqlString.Field<timeCardStatus>
        {
        }
    }
}
