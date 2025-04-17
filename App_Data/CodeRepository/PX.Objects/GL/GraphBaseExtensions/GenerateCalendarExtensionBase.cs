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
using PX.Objects.GL.FinPeriods;
using System.Collections;
using PX.Objects.Common.Extensions;
using PX.Objects.FA;
using PX.Data.BQL.Fluent;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System.Threading;

namespace PX.Objects.GL.GraphBaseExtensions
{
	public class GenerateCalendarExtensionBase<FinPeriodMaintenanceGraph, PrimaryFinYear> : PXGraphExtension<FinPeriodMaintenanceGraph>
		where FinPeriodMaintenanceGraph : PXGraph, IFinPeriodMaintenanceGraph, new()
		where PrimaryFinYear : class, IBqlTable, IFinYear, new()
	{
		public PXFilter<FinPeriodGenerateParameters> GenerateParams;

		public PXAction<PrimaryFinYear> GenerateYears;
		[PXButton(Category = CS.ActionCategories.PeriodManagement, DisplayOnMainToolbar = true)]
		[PXUIField(DisplayName = "Generate Calendar", MapEnableRights = PXCacheRights.Update)]
		public virtual IEnumerable generateYears(PXAdapter adapter)
		{
			IFinPeriodRepository finPeriodRepository = Base.GetService<IFinPeriodRepository>();
			IFinPeriodUtils finPeriodUtils = Base.GetService<IFinPeriodUtils>();
			PrimaryFinYear primaryYear = (PrimaryFinYear)Base.Caches<PrimaryFinYear>().Current;

			if(primaryYear == null)
			{
				throw new PXException(Messages.NeedToCreateFirstCalendarYear);
			}

			int? firstExistingYear = int.TryParse(finPeriodRepository.FindFirstYear(primaryYear.OrganizationID ?? 0, clearQueryCache: true)?.Year, out int parsedFirstExistingYear)
				? parsedFirstExistingYear
				: (int?)null;
			int? lastExistingYear = int.TryParse(finPeriodRepository.FindLastYear(primaryYear.OrganizationID ?? 0, clearQueryCache: true)?.Year, out int parsedLastExistingYear)
				? parsedLastExistingYear
				: (int?)null;

			bool generateCalendar = true;
			if (!Base.IsContractBasedAPI)
			{
				generateCalendar=GenerateParams.AskExtFullyValid(
					(graph, viewName) =>
					{
						FinPeriodGenerateParameters parameters = GenerateParams.Current;
						parameters.OrganizationID = primaryYear.OrganizationID;
						parameters.FromYear =
						parameters.ToYear = lastExistingYear == null ? primaryYear.Year : (lastExistingYear + 1).ToString();
						parameters.FirstFinYear = firstExistingYear?.ToString();
						parameters.LastFinYear = lastExistingYear?.ToString();
					},
					DialogAnswerType.Positive);
			}

			if (generateCalendar)
			{
				int fromYear = int.Parse(GenerateParams.Current.FromYear);
				int toYear = int.Parse(GenerateParams.Current.ToYear);

				IFinPeriodMaintenanceGraph processingGraph = Base.Clone();
				PXLongOperation.StartOperation(
					Base,
					delegate ()
					{
						finPeriodUtils.CheckParametersOfCalendarGeneration(primaryYear.OrganizationID, fromYear, toYear);
						processingGraph.GenerateCalendar(primaryYear.OrganizationID, fromYear, toYear);
					});
				PXLongOperation.WaitCompletion(Base.UID);
				if (!Base.IsContractBasedAPI)
				{
					FABookPeriod unsynchronizedPeriod = SelectFrom<FABookPeriod>
						.InnerJoin<FABook>
							.On<FABookPeriod.bookID.IsEqual<FABook.bookID>
								.And<FABook.updateGL.IsEqual<True>>>
						.InnerJoin<FinPeriod>
							.On<FABookPeriod.organizationID.IsEqual<FinPeriod.organizationID>
								.And<FABookPeriod.finPeriodID.IsEqual<FinPeriod.finPeriodID>>>
						.Where<FABookPeriod.startDate.IsNotEqual<FinPeriod.startDate>
							.Or<FABookPeriod.endDate.IsNotEqual<FinPeriod.endDate>>>
						.View
						.ReadOnly
						.Select(Base);

					if (unsynchronizedPeriod != null)
					{
						throw new PXException(Messages.FinPeriodsChange);
					}
				}
			}
			return adapter.Get();
		}
	}
}
