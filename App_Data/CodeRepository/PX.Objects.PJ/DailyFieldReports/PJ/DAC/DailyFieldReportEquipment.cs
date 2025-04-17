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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Equipment")]
    public class DailyFieldReportEquipment : IBqlTable
    {
        [PXDBIdentity]
        public virtual int? DailyFieldReportEquipmentId
        {
            get;
            set;
        }

        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId.FromCurrent>>))]
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDBString(10, IsKey = true, IsUnicode = true)]
        [PXParent(typeof(SelectFrom<EPEquipmentTimeCard>
            .Where<EPEquipmentTimeCard.timeCardCD.IsEqual<equipmentTimeCardCd.FromCurrent>>))]
        public virtual string EquipmentTimeCardCd
        {
            get;
            set;
        }

        [PXDBInt(IsKey = true)]
        [PXParent(typeof(SelectFrom<EPEquipmentDetail>
            .Where<EPEquipmentDetail.lineNbr.IsEqual<equipmentDetailLineNumber.FromCurrent>
                .And<EPEquipmentDetail.timeCardCD.IsEqual<equipmentTimeCardCd.FromCurrent>>>))]
        public virtual int? EquipmentDetailLineNumber
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
    }
}