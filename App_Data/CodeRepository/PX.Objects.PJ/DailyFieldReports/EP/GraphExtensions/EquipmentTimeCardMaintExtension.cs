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

using System.Linq;
using PX.Objects.PJ.Common.Services;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.EP.GraphExtensions
{
    public class EquipmentTimeCardMaintExtension : PXGraphExtension<EquipmentTimeCardMaint>
    {
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportEquipment>.View DailyFieldReportEquipment;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public virtual void _(Events.RowDeleting<EPEquipmentTimeCard> args)
        {
            if (args.Row is EPEquipmentTimeCard equipmentTimeCard &&
                !SiteMapExtension.IsDailyFieldReportScreen() &&
                DoesTimeCardHasRelatedDailyFieldReport(equipmentTimeCard.TimeCardCD))
            {
                var message = CreateWarningMessage();
                throw new PXException(message);
            }
        }

        public virtual void _(Events.RowDeleting<EPEquipmentDetail> args)
        {
            if (args.Row is EPEquipmentDetail equipmentDetail && !SiteMapExtension.IsDailyFieldReportScreen() &&
                DoesDetailLineHaveRelatedDailyFieldReport(equipmentDetail))
            {
                Base.Details.View.Ask(CreateWarningMessage(), MessageButtons.OK);
                args.Cancel = true;
            }
        }

        public virtual void _(Events.RowDeleting<EPEquipmentSummary> args)
        {
            if (args.Row is EPEquipmentSummary equipmentSummary)
            {
                var equipmentDetail = GetEquipmentDetailOfEquipmentSummary(equipmentSummary);
                if (!SiteMapExtension.IsDailyFieldReportScreen() &&
                    DoesDetailLineHaveRelatedDailyFieldReport(equipmentDetail))
                {
                    Base.Details.View.Ask(CreateWarningMessage(), MessageButtons.OK);
                    args.Cancel = true;
                }
            }
        }

        private static string CreateWarningMessage()
        {
            return string.Format(DailyFieldReportMessages.EntityCannotBeDeletedBecauseItIsLinked,
                DailyFieldReportEntityNames.Equipment.Capitalize());
        }

        private bool DoesDetailLineHaveRelatedDailyFieldReport(EPEquipmentDetail equipmentDetail)
        {
            return SelectFrom<DailyFieldReportEquipment>
                .Where<DailyFieldReportEquipment.equipmentTimeCardCd.IsEqual<P.AsString>
                    .And<DailyFieldReportEquipment.equipmentDetailLineNumber.IsEqual<P.AsInt>>>.View
                .Select(Base, equipmentDetail?.TimeCardCD, equipmentDetail?.LineNbr).Any();
        }

        private bool DoesTimeCardHasRelatedDailyFieldReport(string timeCardCd)
        {
            return SelectFrom<DailyFieldReportEquipment>
                .Where<DailyFieldReportEquipment.equipmentTimeCardCd.IsEqual<P.AsString>>.View
                .Select(Base, timeCardCd).Any();
        }

        private EPEquipmentDetail GetEquipmentDetailOfEquipmentSummary(EPEquipmentSummary equipmentSummary)
        {
            var equipmentSummaryLineNbr = equipmentSummary.LineNbr;
            return SelectFrom<EPEquipmentDetail>
                .Where<EPEquipmentDetail.timeCardCD.IsEqual<P.AsString>
                    .And<EPEquipmentDetail.setupSummarylineNbr.IsEqual<P.AsInt>
                        .Or<EPEquipmentDetail.runSummarylineNbr.IsEqual<P.AsInt>>
                        .Or<EPEquipmentDetail.suspendSummarylineNbr.IsEqual<P.AsInt>>>>.View
                .Select(Base, equipmentSummary.TimeCardCD, equipmentSummaryLineNbr,
                    equipmentSummaryLineNbr, equipmentSummaryLineNbr);
        }
    }
}