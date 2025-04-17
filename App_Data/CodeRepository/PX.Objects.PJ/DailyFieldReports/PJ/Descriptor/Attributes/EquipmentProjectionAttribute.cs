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

using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class EquipmentProjectionAttribute : PXProjectionAttribute
    {
        public EquipmentProjectionAttribute()
            : base(typeof(SelectFrom<EPEquipmentDetail>
            .LeftJoin<EPEquipmentTimeCard>.On<EPEquipmentDetail.timeCardCD.IsEqual<EPEquipmentTimeCard.timeCardCD>>
            .LeftJoin<EPEquipment>.On<EPEquipment.equipmentID.IsEqual<EPEquipmentTimeCard.equipmentID>>
            .LeftJoin<DailyFieldReportEquipment>
                .On<DailyFieldReportEquipment.equipmentTimeCardCd.IsEqual<EPEquipmentDetail.timeCardCD>
                    .And<DailyFieldReportEquipment.equipmentDetailLineNumber.IsEqual<EPEquipmentDetail.lineNbr>>>),
            new[]
            {
                typeof(DailyFieldReportEquipment)
            })
        {
        }

        public override bool PersistDeleted(PXCache cache, object row)
        {
            return true;
        }
    }
}