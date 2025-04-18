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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.Common.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.PJ.ProjectAccounting.PM.GraphExtensions
{
    public class ForecastMaintExt : PXGraphExtension<ForecastMaint>
    {
        public PXAction<PMForecast> Report;
        public PXAction<PMForecast> BudgetForecastReport;

        public override void Initialize()
        {
            base.Initialize();
            Report.AddMenuAction(BudgetForecastReport);
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXButton(SpecialType = PXSpecialButtonType.Report, MenuAutoOpen = true)]
        [PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
        protected void report()
        {
        }

        [PXUIField(DisplayName = "Print Project Budget Forecast", MapEnableRights = PXCacheRights.Select)]
        [PXButton(Category = PX.Objects.PM.Messages.PrintingAndEmailing)]
        protected virtual IEnumerable budgetForecastReport(PXAdapter adapter)
        {
            Base.Save.Press();
            var filter = Base.Filter.Current;
            var parameters = new Dictionary<string, string>();
            SetProject(filter, parameters);
            SetAccountGroup(filter, parameters);
            SetTask(filter, parameters);
            SetInventoryItem(filter, parameters);
            SetCostCode(filter, parameters);
            parameters["RevisionId"] = Base.Revisions.Current.RevisionID;
            parameters["AccountGroupType"] = filter.AccountGroupType;
            throw new PXReportRequiredException(parameters, Constants.ReportIds.BudgetForecast);
        }

        private void SetProject(ForecastMaint.PMForecastFilter filter, IDictionary<string, string> parameters)
        {
            var project = Base.Select<PMProject>().SingleOrDefault(p => p.ContractID == filter.ProjectID);
            parameters["ProjectId"] = filter.ProjectID != null && project != null
                ? project.ContractCD
                : string.Empty;
        }

        private void SetAccountGroup(ForecastMaint.PMForecastFilter filter, IDictionary<string, string> parameters)
        {
            var group = Base.Select<PMAccountGroup>().SingleOrDefault(g => g.GroupID == filter.AccountGroupID);
            parameters["AccountGroupId"] = filter.AccountGroupID != null && group != null
                ? group.GroupCD
                : string.Empty;
        }

        private void SetTask(ForecastMaint.PMForecastFilter filter, IDictionary<string, string> parameters)
        {
            var task = Base.Select<PMTask>().SingleOrDefault(t => t.TaskID == filter.ProjectTaskID);
            parameters["ProjectTaskId"] = filter.ProjectTaskID != null && task != null
                ? task.TaskCD
                : string.Empty;
        }

        private void SetInventoryItem(ForecastMaint.PMForecastFilter filter, IDictionary<string, string> parameters)
        {
            InventoryItem item =
                SelectFrom<InventoryItem>.Where<InventoryItem.inventoryID.IsEqual<P.AsInt>>.View.Select(Base,
                    filter.InventoryID);
            parameters["InventoryId"] = filter.InventoryID != null && item != null
                ? item.InventoryCD
                : string.Empty;
        }

        private void SetCostCode(ForecastMaint.PMForecastFilter filter, IDictionary<string, string> parameters)
        {
            var code = Base.Select<PMCostCode>().SingleOrDefault(c => c.CostCodeID == filter.CostCodeID);
            parameters["CostCodeId"] = filter.ProjectID != null && code != null
                ? code.CostCodeCD
                : string.Empty;
        }
    }
}