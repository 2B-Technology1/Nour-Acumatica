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
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PM.GraphExtensions
{
	public class ProgressWorksheetEntryDailyFieldReportExtension : PXGraphExtension<ProgressWorksheetEntry>
	{
		public PXSelect<DailyFieldReportProgressWorksheet, Where<DailyFieldReportProgressWorksheet.progressWorksheetId, Equal<Current<PMProgressWorksheet.refNbr>>>> DFRProgressWorksheet;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
		}

		public PXAction<PMProgressWorksheet> ViewDailyFieldReport;

		[PXButton]
		[PXUIField]
		public virtual void viewDailyFieldReport()
		{
			var dfr = DFRProgressWorksheet.SelectSingle();
			if (dfr != null && dfr.DailyFieldReportId != null)
			{
				var dailyFieldReportEntry = PXGraph.CreateInstance<DailyFieldReportEntry>();
				dailyFieldReportEntry.DailyFieldReport.Current =
					PXSelect<DailyFieldReport, Where<DailyFieldReport.dailyFieldReportId, Equal<Required<DailyFieldReport.dailyFieldReportId>>>>
					.SelectSingleBound(Base, null, dfr.DailyFieldReportId);
				PXRedirectHelper.TryRedirect(dailyFieldReportEntry, PXRedirectHelper.WindowMode.NewWindow);
			}
			}

		protected virtual void _(Events.FieldSelecting<PMProgressWorksheet, PMProgressWorksheet.dailyFieldReportCD> e)
		{
			if (e.Row != null)
			{
				var dfrpw = DFRProgressWorksheet.SelectSingle();
				if (dfrpw != null && dfrpw.DailyFieldReportId != null)
				{
					DailyFieldReport dfr = PXSelect<DailyFieldReport, Where<DailyFieldReport.dailyFieldReportId, Equal<Required<DailyFieldReport.dailyFieldReportId>>>>
						.SelectSingleBound(Base, null, dfrpw.DailyFieldReportId);
					e.ReturnValue = dfr.DailyFieldReportCd;
				}
			}
		}

		protected virtual void _(Events.RowSelected<PMProgressWorksheet> e)
		{
			if (e.Row != null)
			{
				var dfrpw = DFRProgressWorksheet.SelectSingle();
				if (dfrpw != null && dfrpw.DailyFieldReportId != null)
				{
					DailyFieldReport dfr = PXSelect<DailyFieldReport, Where<DailyFieldReport.dailyFieldReportId, Equal<Required<DailyFieldReport.dailyFieldReportId>>>>
						.SelectSingleBound(Base, null, dfrpw.DailyFieldReportId);
					if (dfr.Status != DailyFieldReportStatus.Hold)
					{
						Base.Details.AllowDelete = false;
						Base.Details.AllowInsert = false;
						Base.Details.AllowUpdate = false;
						Base.loadTemplate.SetEnabled(false);
						Base.selectBudgetLines.SetEnabled(false);
					}

					PXUIFieldAttribute.SetEnabled<PMProgressWorksheet.date>(e.Cache, e.Row, false);
					PXUIFieldAttribute.SetEnabled<PMProgressWorksheet.projectID>(e.Cache, e.Row, false);
				}
			}
		}
	}
}
