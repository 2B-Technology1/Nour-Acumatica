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
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Objects.PM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
	public class DailyFieldReportEntryProgressWorksheetExtension : DailyFieldReportEntryExtension<DailyFieldReportEntry>, PXImportAttribute.IPXPrepareItems
	{
		[PXViewName(PX.Objects.PM.Messages.Project)]
		public PXSetup<PMProject>.Where<PMProject.contractID.IsEqual<DailyFieldReport.projectId.FromCurrent>> Project;
		public PXSetup<PMSetup> Setup;

		[PXViewName(ViewNames.ProgressWorksheetLines)]
		[PXCopyPasteHiddenView]
		[PXImport(typeof(PMProgressWorksheet))]
		public SelectFrom<PMProgressWorksheetLine>
			.LeftJoin<DailyFieldReportProgressWorksheet>
				.On<PMProgressWorksheetLine.refNbr.IsEqual<DailyFieldReportProgressWorksheet.progressWorksheetId>
					.Or<Brackets<PMProgressWorksheetLine.refNbr.IsNull.And<DailyFieldReportProgressWorksheet.progressWorksheetId.IsNull>>>>
			.LeftJoin<PMProgressWorksheet>
				.On<PMProgressWorksheetLine.refNbr.IsEqual<PMProgressWorksheet.refNbr>>
			.Where<DailyFieldReportProgressWorksheet.dailyFieldReportId.IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View ProgressWorksheetLines;

		[PXViewName(ViewNames.ProgressWorksheets)]
		[PXCopyPasteHiddenView]
		public SelectFrom<PMProgressWorksheet>
			.LeftJoin<DailyFieldReportProgressWorksheet>
				.On<PMProgressWorksheet.refNbr.IsEqual<DailyFieldReportProgressWorksheet.progressWorksheetId>>
			.Where<DailyFieldReportProgressWorksheet.dailyFieldReportId.IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View ProgressWorksheets;

		[PXViewName(ViewNames.DailyFieldReportProgressWorksheets)]
		[PXCopyPasteHiddenView]
		public SelectFrom<DailyFieldReportProgressWorksheet>
			.Where<DailyFieldReportProgressWorksheet.dailyFieldReportId
				.IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View DailyFieldReportProgressWorksheets;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
		}

		protected override (string Entity, string View) Name =>
			(DailyFieldReportEntityNames.ProgressWorksheet, ViewNames.ProgressWorksheetLines);

		protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
		{
			return new DailyFieldReportRelationMapping(typeof(PMProgressWorksheetLine))
			{
				RelationNumber = typeof(PMProgressWorksheetLine.refNbr)
			};
		}

		protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
		{
			return new PXSelectExtension<DailyFieldReportRelation>(ProgressWorksheetLines);
		}

		public PXAction<DailyFieldReport> ViewProgressWorksheet;

		[PXButton]
		[PXUIField]
		public virtual void viewProgressWorksheet()
		{
			if (ProgressWorksheetLines.Current != null && ProgressWorksheetLines.Current.RefNbr != null)
			{
				var progressWorksheetEntry = PXGraph.CreateInstance<ProgressWorksheetEntry>();
				progressWorksheetEntry.Document.Current =
					progressWorksheetEntry.Document.Search<PMProgressWorksheet.refNbr>(ProgressWorksheetLines.Current.RefNbr);
				if (progressWorksheetEntry.Document.Current != null)
				{
					PXRedirectHelper.TryRedirect(progressWorksheetEntry, PXRedirectHelper.WindowMode.NewWindow);
				}
			}
		}

		[System.SerializableAttribute()]
		[PXCacheName(PX.Objects.PM.Messages.CostBudgetLineFilter)]
		public partial class CostBudgetLineFilter : IBqlTable
		{
			#region ProjectID
			public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

			/// <summary>
			/// The identifier of the <see cref="PMProject">project</see> associated with the Cost Budget Line Filter.
			/// </summary>
			/// <value>
			/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID"/> field.
			/// </value>
			[PXInt()]
			public virtual Int32? ProjectID
			{
				get;
				set;
			}
			#endregion
			#region TaskID
			public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

			/// <summary>
			/// The identifier of the <see cref="PMTask">task</see> associated with the cost budget line filter.
			/// </summary>
			/// <value>
			/// The value of this field corresponds to the value of the <see cref="PMTask.TaskID"/> field.
			/// </value>
			[ActiveOrInPlanningProjectTask(typeof(CostBudgetLineFilter.projectID), CheckMandatoryCondition = typeof(Where<True, Equal<False>>), DisplayName = "Project Task")]
			public virtual Int32? TaskID
			{
				get;
				set;
			}
			#endregion
			#region AccountGroupID
			public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }

			/// <summary>The identifier of the <see cref="PMAccountGroup">account group</see> associated with the Cost Budget Line Filter.</summary>
			/// <value>
			/// The value of this field corresponds to the value of the <see cref="PMAccountGroup.GroupID" /> field.
			/// </value>
			[PXUIField(DisplayName = "Account Group")]
			[PXInt()]
			[PXSelector(typeof(SelectFrom<PMAccountGroup>
				.InnerJoin<PMCostBudget>.On<PMAccountGroup.groupID.IsEqual<PMCostBudget.accountGroupID>>
				.Where<PMCostBudget.projectID.IsEqual<CostBudgetLineFilter.projectID.FromCurrent>.And<PMCostBudget.type.IsEqual<GL.AccountType.expense>>.And<PMCostBudget.accountGroupID.IsNotNull>
					.And<PMAccountGroup.isExpense.IsEqual<True>>>
				.AggregateTo<GroupBy<PMCostBudget.accountGroupID>>
				.SearchFor<PMAccountGroup.groupID>),
				SubstituteKey = typeof(PMAccountGroup.groupCD))]
			public virtual Int32? AccountGroupID
			{
				get;
				set;
			}
			#endregion
			#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

			/// <summary>
			/// The identifier of the <see cref="InventoryItem">inventory item</see> associated with the Cost Budget Line Filter.
			/// </summary>
			/// <value>
			/// The value of this field corresponds to the value of the <see cref="InventoryItem.InventoryID"/> field.
			/// </value>
			[PXUIField(DisplayName = "Inventory ID")]
			[PXInt()]
			[PXSelector(typeof(SelectFrom<InventoryItem>
				.InnerJoin<PMCostBudget>.On<InventoryItem.inventoryID.IsEqual<PMCostBudget.inventoryID>>
				.Where<PMCostBudget.projectID.IsEqual<CostBudgetLineFilter.projectID.FromCurrent>.And<PMCostBudget.type.IsEqual<GL.AccountType.expense>>.And<PMCostBudget.inventoryID.IsNotNull>>
				.AggregateTo<GroupBy<PMCostBudget.inventoryID>>
				.SearchFor<InventoryItem.inventoryID>),
				SubstituteKey = typeof(InventoryItem.inventoryCD))]
			public virtual Int32? InventoryID
			{
				get;
				set;
			}
			#endregion
			#region CostCodeFrom
			public abstract class costCodeFrom : PX.Data.BQL.BqlInt.Field<costCodeFrom> { }

			/// <summary>
			/// The identifier of the <see cref="PMCostCode">cost code</see> associated with the Cost Budget Line Filter.
			/// </summary>
			[PXUIField(DisplayName = "Cost Code From", FieldClass = CostCodeAttribute.COSTCODE)]
			[PXInt()]
			[PXDimensionSelector(CostCodeAttribute.COSTCODE, typeof(SelectFrom<PMCostCode>
				.InnerJoin<PMCostBudget>.On<PMCostCode.costCodeID.IsEqual<PMCostBudget.costCodeID>>
				.Where<PMCostBudget.projectID.IsEqual<CostBudgetLineFilter.projectID.FromCurrent>.And<PMCostBudget.type.IsEqual<GL.AccountType.expense>>.And<PMCostBudget.costCodeID.IsNotNull>>
				.AggregateTo<GroupBy<PMCostBudget.costCodeID>>
				.SearchFor<PMCostCode.costCodeID>), typeof(PMCostCode.costCodeCD))]
			public virtual Int32? CostCodeFrom
			{
				get;
				set;
			}
			#endregion
			#region CostCodeTo
			public abstract class costCodeTo : PX.Data.BQL.BqlInt.Field<costCodeTo> { }

			/// <summary>
			/// The identifier of the <see cref="PMCostCode">cost code</see> associated with the Cost Budget Line Filter.
			/// </summary>
			[PXUIField(DisplayName = "Cost Code To", FieldClass = CostCodeAttribute.COSTCODE)]
			[PXInt()]
			[PXDimensionSelector(CostCodeAttribute.COSTCODE, typeof(SelectFrom<PMCostCode>
				.InnerJoin<PMCostBudget>.On<PMCostCode.costCodeID.IsEqual<PMCostBudget.costCodeID>>
				.Where<PMCostBudget.projectID.IsEqual<CostBudgetLineFilter.projectID.FromCurrent>.And<PMCostBudget.type.IsEqual<GL.AccountType.expense>>.And<PMCostBudget.costCodeID.IsNotNull>>
				.AggregateTo<GroupBy<PMCostBudget.costCodeID>>
				.SearchFor<PMCostCode.costCodeID>), typeof(PMCostCode.costCodeCD))]
			public virtual Int32? CostCodeTo
			{
				get;
				set;
			}
			#endregion
		}

		public PXFilter<CostBudgetLineFilter> costBudgetfilter;

		[PXFilterable]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<PMCostBudget,
				LeftJoin<PMCostCode, On<PMCostBudget.costCodeID, Equal<PMCostCode.costCodeID>>,
				InnerJoin<PMTask, On<PMCostBudget.projectID, Equal<PMTask.projectID>, And<PMCostBudget.projectTaskID, Equal<PMTask.taskID>>>,
				InnerJoin<PMAccountGroup, On<PMCostBudget.accountGroupID, Equal<PMAccountGroup.groupID>>>>>,
				Where<PMCostBudget.projectID, Equal<Current<CostBudgetLineFilter.projectID>>,
					And<PMCostBudget.type, Equal<GL.AccountType.expense>,
					And<PMCostBudget.productivityTracking, IsNotNull,
					And<PMCostBudget.productivityTracking, NotEqual<PMProductivityTrackingType.notAllowed>,
					And<PMTask.status, NotEqual<ProjectTaskStatus.canceled>,
					And<PMTask.status, NotEqual<ProjectTaskStatus.completed>,
					And<PMCostCode.isActive, NotEqual<False>,
					And<PMAccountGroup.isActive, Equal<True>>>>>>>>>> CostBudgets;

		public IEnumerable costBudgets()
		{
			List<object> parameters = new List<object>();
			var costBudgetSelect = new PXSelectJoin<PMCostBudget,
				LeftJoin<PMCostCode, On<PMCostBudget.costCodeID, Equal<PMCostCode.costCodeID>>,
				InnerJoin<PMTask, On<PMCostBudget.projectID, Equal<PMTask.projectID>, And<PMCostBudget.projectTaskID, Equal<PMTask.taskID>>>,
				InnerJoin<PMAccountGroup, On<PMCostBudget.accountGroupID, Equal<PMAccountGroup.groupID>>>>>,
				Where<PMCostBudget.projectID, Equal<Current<CostBudgetLineFilter.projectID>>,
					And<PMCostBudget.type, Equal<GL.AccountType.expense>,
					And<PMCostBudget.productivityTracking, IsNotNull,
					And<PMCostBudget.productivityTracking, NotEqual<PMProductivityTrackingType.notAllowed>,
					And<PMTask.status, NotEqual<ProjectTaskStatus.canceled>,
					And<PMTask.status, NotEqual<ProjectTaskStatus.completed>,
					And<PMCostCode.isActive, NotEqual<False>,
					And<PMAccountGroup.isActive, Equal<True>>>>>>>>>>(Base);

			if (costBudgetfilter.Current.TaskID != null)
			{
				costBudgetSelect.WhereAnd(typeof(Where<PMCostBudget.projectTaskID, Equal<Required<PMCostBudget.projectTaskID>>>));
				parameters.Add(costBudgetfilter.Current.TaskID);
			}

			if (costBudgetfilter.Current.AccountGroupID != null)
			{
				costBudgetSelect.WhereAnd(typeof(Where<PMCostBudget.accountGroupID, Equal<Required<PMCostBudget.accountGroupID>>>));
				parameters.Add(costBudgetfilter.Current.AccountGroupID);
			}

			if (costBudgetfilter.Current.InventoryID != null)
			{
				costBudgetSelect.WhereAnd(typeof(Where<PMCostBudget.inventoryID, Equal<Required<PMCostBudget.inventoryID>>>));
				parameters.Add(costBudgetfilter.Current.InventoryID);
			}

			if (costBudgetfilter.Current.CostCodeFrom != null)
			{
				PMCostCode costCodeFrom = (new PXSelect<PMCostCode, Where<PMCostCode.costCodeID, Equal<Current<CostBudgetLineFilter.costCodeFrom>>>>(Base)).SelectSingle();
				costBudgetSelect.WhereAnd(typeof(Where<PMCostCode.costCodeCD, GreaterEqual<Required<PMCostCode.costCodeCD>>>));
				parameters.Add(costCodeFrom.CostCodeCD);
			}

			if (costBudgetfilter.Current.CostCodeTo != null)
			{
				PMCostCode costCodeTo = (new PXSelect<PMCostCode, Where<PMCostCode.costCodeID, Equal<Current<CostBudgetLineFilter.costCodeTo>>>>(Base)).SelectSingle();
				costBudgetSelect.WhereAnd(typeof(Where<PMCostCode.costCodeCD, LessEqual<Required<PMCostCode.costCodeCD>>>));
				parameters.Add(costCodeTo.CostCodeCD);
			}
			parameters.AddRange(PXView.Parameters);

			PXDelegateResult delResult = new PXDelegateResult();
			delResult.Capacity = 202;
			delResult.IsResultFiltered = false;
			delResult.IsResultSorted = true;
			delResult.IsResultTruncated = false;

			var view = new PXView(Base, false, costBudgetSelect.View.BqlSelect);
			var startRow = PXView.StartRow;
			int totalRows = 0;
			var resultset = view.Select(PXView.Currents, parameters.ToArray(), PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;

			Dictionary<BudgetKeyTuple, List<PMProgressWorksheet>> existing = GetExistingCostBudgetLines();

			foreach (PXResult<PMCostBudget, PMCostCode> costBudgetResult in resultset)
			{
				PMCostBudget costBudget = costBudgetResult;
				List<PMProgressWorksheet> list;
				if (!(existing.TryGetValue(BudgetKeyTuple.Create(costBudget), out list) && list.Any(item => item == null || item.Status == ProgressWorksheetStatus.OnHold)))
				{
					delResult.Add(costBudget);
				}
			}

			return delResult;
		}

		protected virtual Dictionary<BudgetKeyTuple, List<PMProgressWorksheet>> GetExistingCostBudgetLines()
		{
			Dictionary<BudgetKeyTuple, List<PMProgressWorksheet>> existing = new Dictionary<BudgetKeyTuple, List<PMProgressWorksheet>>();
			foreach (PXResult<PMProgressWorksheetLine, DailyFieldReportProgressWorksheet, PMProgressWorksheet> line in ProgressWorksheetLines.Select())
			{
				PMProgressWorksheet progressWorksheet = line;
				PMProgressWorksheetLine progressWorksheetLine = line;
				BudgetKeyTuple lineKey = BudgetKeyTuple.Create(progressWorksheetLine);
				List<PMProgressWorksheet> list;
				if (!existing.TryGetValue(lineKey, out list))
				{
					list = new List<PMProgressWorksheet>();
					existing.Add(lineKey, list);
				}
				if (progressWorksheet == null || progressWorksheet.RefNbr == null)
				{
					list.Add(null);
				}
				else if (!list.Any(item => item != null && item.RefNbr == progressWorksheet.RefNbr))
				{
					list.Add(progressWorksheet);
				}
			}

			return existing;
		}

		public void _(Events.RowInserting<PMProgressWorksheetLine> e)
		{
			if (e.Row != null)
			{
				if (e.Row.RefNbr == null)
				{
					PMProgressWorksheet pw = GetOnHoldProgressWorksheet();

					string refNbr = null;
					if (pw != null)
					{
						refNbr = pw.RefNbr;
					}
					else
					{
						Numbering numbering = PXSelect<Numbering, Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>>.SelectSingleBound(Base, null, Setup.Current.ProgressWorksheetNumbering);
						if (numbering != null)
						{
							refNbr = numbering.NewSymbol;
						}
					}

					if (refNbr != null)
					{
						e.Row.RefNbr = refNbr;
						e.Row.LineNbr = ProgressWorksheetLines.Select().RowCast<PMProgressWorksheetLine>().Max(line => line.LineNbr).GetValueOrDefault(0) + 1;
						e.Row.ProjectID = Project.Current.ContractID;
					}
				}
			}
		}

		public void _(Events.RowInserted<PMProgressWorksheetLine> e)
		{
			if (e.Row != null)
			{
				PMProgressWorksheet pw = GetOnHoldProgressWorksheet();
				if (pw == null)
				{
					pw = new PMProgressWorksheet();
					pw.Hidden = true;
					pw.Status = ProgressWorksheetStatus.OnHold;
					pw.ProjectID = Project.Current.ContractID;
					pw = ProgressWorksheets.Insert(pw);
				}
			}
		}

		private ProgressWorksheetEntry graph;

		[PXOverride]
		public void Persist(Action baseMethod)
		{
			if (Base.DailyFieldReport.Current != null)
			{
				foreach (PMProgressWorksheetLine line in ProgressWorksheetLines.Select())
				{
					ProgressWorksheetEntry.CheckCostBudgetLine(Base, ProgressWorksheetLines.Cache, Project.Current, line);
				}

				HashSet<PMProgressWorksheetLine> savedLines = new HashSet<PMProgressWorksheetLine>();
				PMProgressWorksheet newProgressWorksheet = ProgressWorksheets.Cache.Inserted.RowCast<PMProgressWorksheet>().SingleOrDefault();
				if (newProgressWorksheet != null)
				{
					if (ProgressWorksheetLines.Cache.Inserted.Count() == 0)
					{
						ProgressWorksheets.Cache.Clear();
					}
					else
					{
						graph = PXGraph.CreateInstance<ProgressWorksheetEntry>();
						graph.Document.Current = new PMProgressWorksheet();
						graph.Document.Current.Hidden = newProgressWorksheet.Hidden;
						graph.Document.Current.Date = Base.DailyFieldReport.Current.Date;
						graph.Document.Current.ProjectID = Base.DailyFieldReport.Current.ProjectId;
						graph.Document.Insert(graph.Document.Current);

						foreach (PMProgressWorksheetLine insertedLine in ProgressWorksheetLines.Cache.Inserted)
						{
							insertedLine.RefNbr = graph.Document.Current.RefNbr;
							insertedLine.LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<PMProgressWorksheetLine.lineNbr>(graph.Details.Cache, graph.Document.Current);
							graph.Details.Cache.Insert(insertedLine);
							savedLines.Add(insertedLine);
						}

						graph.Persist();
						ProgressWorksheets.Cache.Clear();

						DailyFieldReportProgressWorksheet dailyFieldReportProgressWorksheet = new DailyFieldReportProgressWorksheet();
						dailyFieldReportProgressWorksheet.ProgressWorksheetId = graph.Document.Current.RefNbr;
						DailyFieldReportProgressWorksheets.Insert(dailyFieldReportProgressWorksheet);
					}
				}

				Dictionary<string, List<PMProgressWorksheetLine>> linesByRefNbr = new Dictionary<string, List<PMProgressWorksheetLine>>();
				foreach (PMProgressWorksheet pw in ProgressWorksheets.Select())
				{
					if (pw != newProgressWorksheet && pw.Status == ProgressWorksheetStatus.OnHold && pw.Date != Base.DailyFieldReport.Current.Date)
					{
						linesByRefNbr.Add(pw.RefNbr, new List<PMProgressWorksheetLine>());
					}
				}
				foreach (PMProgressWorksheetLine line in ProgressWorksheetLines.Cache.Deleted)
				{
					if (!savedLines.Contains(line))
					{
						AddLineByRefNbr(linesByRefNbr, line);
					}
				}
				foreach (PMProgressWorksheetLine line in ProgressWorksheetLines.Cache.Updated)
				{
					if (!savedLines.Contains(line))
					{
						AddLineByRefNbr(linesByRefNbr, line);
					}
				}
				foreach (PMProgressWorksheetLine line in ProgressWorksheetLines.Cache.Inserted)
				{
					if (!savedLines.Contains(line))
					{
						AddLineByRefNbr(linesByRefNbr, line);
					}
				}

				foreach (KeyValuePair<string, List<PMProgressWorksheetLine>> lines in linesByRefNbr)
				{
					graph = PXGraph.CreateInstance<ProgressWorksheetEntry>();
					graph.Document.Current = PXSelect<PMProgressWorksheet, Where<PMProgressWorksheet.refNbr, Equal<Required<PMProgressWorksheet.refNbr>>>>.SelectSingleBound(graph, null, lines.Key);
					graph.Document.Current.Date = Base.DailyFieldReport.Current.Date;
					graph.Document.Cache.SetStatus(graph.Document.Current, PXEntryStatus.Updated);

					foreach (PMProgressWorksheetLine line in lines.Value)
					{
						if (ProgressWorksheetLines.Cache.GetStatus(line) == PXEntryStatus.Deleted)
						{
							graph.Details.Cache.Delete(line);
							savedLines.Add(line);
						}
					}

					foreach (PMProgressWorksheetLine line in lines.Value)
					{
						if (ProgressWorksheetLines.Cache.GetStatus(line) == PXEntryStatus.Updated)
						{
							graph.Details.Cache.Update(line);
							savedLines.Add(line);
						}
						else if (ProgressWorksheetLines.Cache.GetStatus(line) == PXEntryStatus.Inserted)
						{
							line.LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<PMProgressWorksheetLine.lineNbr>(graph.Details.Cache, graph.Document.Current);
							graph.Details.Cache.Insert(line);
							savedLines.Add(line);
						}
					}

					graph.Persist();

					if (graph.Document.Current.Status == ProgressWorksheetStatus.OnHold && graph.Details.Select().Count() == 0)
					{
						graph.Document.Cache.Delete(graph.Document.Current);
						graph.Persist();

						Base.SelectTimeStamp();
					}
				}

				ProgressWorksheetLines.Cache.Clear();
			}
			else
			{
				DailyFieldReport dfr = Base.DailyFieldReport.Cache.Deleted.RowCast<DailyFieldReport>().SingleOrDefault();
				if (dfr!= null)
				{
					HashSet<string> pwForDelete = new HashSet<string>();
					foreach (PMProgressWorksheetLine line in ProgressWorksheetLines.Cache.Deleted)
					{
						pwForDelete.Add(line.RefNbr);
					}

					DailyFieldReportProgressWorksheets.Cache.Persist(PXDBOperation.Delete);

					foreach (string refNbr in pwForDelete)
					{
						graph = PXGraph.CreateInstance<ProgressWorksheetEntry>();
						graph.Document.Current = PXSelect<PMProgressWorksheet, Where<PMProgressWorksheet.refNbr, Equal<Required<PMProgressWorksheet.refNbr>>>>.SelectSingleBound(graph, null, refNbr);
						graph.Document.Cache.Delete(graph.Document.Current);
						graph.Persist();
					}

					if (pwForDelete.Count > 0)
					{
						ProgressWorksheetLines.Cache.Clear();

						Base.SelectTimeStamp();
					}
				}
			}

			baseMethod();
		}

		protected virtual void AddLineByRefNbr(Dictionary<string, List<PMProgressWorksheetLine>> linesByRefNbr, PMProgressWorksheetLine line)
		{
			List<PMProgressWorksheetLine> lines;
			if (!linesByRefNbr.TryGetValue(line.RefNbr, out lines))
			{
				lines = new List<PMProgressWorksheetLine>();
				linesByRefNbr.Add(line.RefNbr, lines);
			}
			lines.Add(line);
		}

		[PXOverride]
		public IEnumerable Complete(PXAdapter adapter, DailyFieldReportEntry.CompleteDelegate baseMethod)
		{
			var result = baseMethod(adapter);

			graph = PXGraph.CreateInstance<ProgressWorksheetEntry>();
			foreach (PMProgressWorksheet pw in ProgressWorksheets.Select())
			{
				if (pw.Hidden == true)
				{
					graph.Document.Current = PXSelect<PMProgressWorksheet, Where<PMProgressWorksheet.refNbr, Equal<Required<PMProgressWorksheet.refNbr>>>>.SelectSingleBound(graph, null, pw.RefNbr);
					graph.Document.Current.Hidden = false;
					graph.Document.Update(graph.Document.Current);
					graph.Persist();
				}
			}

			return result;
		}

		private PMProgressWorksheet GetOnHoldProgressWorksheet()
		{
			foreach (PMProgressWorksheet pw in ProgressWorksheets.Select())
			{
				if (pw.Status == ProgressWorksheetStatus.OnHold)
				{
					return pw;
				}
			}

			return null;
		}

		public PXAction<DailyFieldReport> loadTemplate;
		[PXUIField(DisplayName = "Load Template", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable LoadTemplate(PXAdapter adapter)
		{
			if (Base.DailyFieldReport.Current.Hold == true && Base.DailyFieldReport.Current.ProjectId != null)
			{
				ProgressWorksheetLines.Cache.ForceExceptionHandling = true;

				Dictionary<BudgetKeyTuple, List<PMProgressWorksheet>> existingLines = GetExistingCostBudgetLines();

				var costBudgetSelect = new PXSelectJoin<PMCostBudget,
					InnerJoin<PMTask, On<PMCostBudget.projectID, Equal<PMTask.projectID>, And<PMCostBudget.projectTaskID, Equal<PMTask.taskID>>>,
					InnerJoin<PMAccountGroup, On<PMCostBudget.accountGroupID, Equal<PMAccountGroup.groupID>>>>,
					Where<PMCostBudget.projectID, Equal<Required<PMProject.contractID>>,
						And<PMCostBudget.type, Equal<GL.AccountType.expense>,
						And<PMCostBudget.productivityTracking, Equal<PMProductivityTrackingType.template>,
						And<PMTask.status, NotEqual<ProjectTaskStatus.canceled>,
						And<PMTask.status, NotEqual<ProjectTaskStatus.completed>,
						And<PMAccountGroup.isActive, Equal<True>>>>>>>>(Base);

				foreach (PMCostBudget line in costBudgetSelect.Select(Base.DailyFieldReport.Current.ProjectId))
				{
					List<PMProgressWorksheet> list;
					if (!existingLines.TryGetValue(BudgetKeyTuple.Create(line), out list) || list.All(item => item != null && item.Status != ProgressWorksheetStatus.OnHold))
					{
						PMProgressWorksheetLine newline = new PMProgressWorksheetLine();
						newline.ProjectID = line.ProjectID;
						newline.TaskID = line.ProjectTaskID;
						newline.InventoryID = line.InventoryID;
						newline.AccountGroupID = line.AccountGroupID;
						newline.CostCodeID = line.CostCodeID;
						ProgressWorksheetLines.Insert(newline);
					}
				}
			}

			return adapter.Get();
		}

		public PXAction<DailyFieldReport> selectBudgetLines;
		[PXUIField(DisplayName = "Select Budget Lines", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable SelectBudgetLines(PXAdapter adapter)
		{
			IEnumerable result = null;

			if (Base.DailyFieldReport.Current.Hold == true && Base.DailyFieldReport.Current.ProjectId != null)
			{
				costBudgetfilter.Current.ProjectID = Base.DailyFieldReport.Current.ProjectId;
				if (CostBudgets.AskExt() == WebDialogResult.OK)
				{
					result = AddSelectedBudgetLines(adapter);
				}

				costBudgetfilter.Cache.Clear();
				CostBudgets.Cache.Clear();
				CostBudgets.ClearDialog();
				CostBudgets.View.Clear();
				CostBudgets.View.ClearDialog();
			}

			if (result != null)
			{
				return result;
			}

			return adapter.Get();
		}

		public PXAction<PMProgressWorksheet> addSelectedBudgetLines;
		[PXUIField(DisplayName = "Add", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable AddSelectedBudgetLines(PXAdapter adapter)
		{
			if (Base.DailyFieldReport.Current.Hold == true)
			{
				ProgressWorksheetLines.Cache.ForceExceptionHandling = true;

				Dictionary<BudgetKeyTuple, List<PMProgressWorksheet>> existing = GetExistingCostBudgetLines();

				foreach (PMCostBudget line in CostBudgets.Cache.Cached)
				{
					List<PMProgressWorksheet> list = null;
					if (line.Selected == true && (!existing.TryGetValue(BudgetKeyTuple.Create(line), out list) || !list.Any(item => item ==null || item.Status == ProgressWorksheetStatus.OnHold)))
					{
						PMProgressWorksheetLine newline = new PMProgressWorksheetLine();
						newline.ProjectID = line.ProjectID;
						newline.TaskID = line.ProjectTaskID;
						newline.InventoryID = line.InventoryID;
						newline.AccountGroupID = line.AccountGroupID;
						newline.CostCodeID = line.CostCodeID;
						ProgressWorksheetLines.Insert(newline);
					}
				}
			}

			return adapter.Get();
		}

		protected virtual void _(Events.RowSelecting<PMProgressWorksheetLine> e)
		{
			if (e.Row != null)
			{
				if (blockLineRowSelectingEvent == false)
				{
					CalculateQuantities(e.Cache, (PMProgressWorksheetLine)e.Row);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProgressWorksheetLine.taskID> e)
		{
			if (e.Row != null)
			{
				CalculateQuantities(e.Cache, (PMProgressWorksheetLine)e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProgressWorksheetLine.accountGroupID> e)
		{
			if (e.Row != null)
			{
				CalculateQuantities(e.Cache, (PMProgressWorksheetLine)e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProgressWorksheetLine.inventoryID> e)
		{
			if (e.Row != null)
			{
				CalculateQuantities(e.Cache, (PMProgressWorksheetLine)e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProgressWorksheetLine.costCodeID> e)
		{
			if (e.Row != null)
			{
				CalculateQuantities(e.Cache, (PMProgressWorksheetLine)e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProgressWorksheetLine.qty> e)
		{
			if (e.Row != null)
			{
				CalculateQuantities(e.Cache, (PMProgressWorksheetLine)e.Row);
			}
		}

		private bool blockLineRowSelectingEvent;

		protected virtual void CalculateQuantities(PXCache cache, PMProgressWorksheetLine line)
		{
			try
			{
				blockLineRowSelectingEvent = true;

				PMProgressWorksheet pw;
				using (new PXConnectionScope())
				{
					pw = ProgressWorksheets.Select().RowCast<PMProgressWorksheet>().SingleOrDefault(doc => doc.RefNbr == line.RefNbr);
				}

				bool newRefNbr;
				string status;
				if (pw == null)
				{
					newRefNbr = true;
					status = ProgressWorksheetStatus.OnHold;
				}
				else
				{
					newRefNbr = ProgressWorksheets.Cache.GetStatus(pw) == PXEntryStatus.Inserted;
					status = pw.Status;
				}

				ProgressWorksheetEntry.CalculateQuantities(Base, cache, Base.DailyFieldReport.Current.Date.Value, status, line, newRefNbr, Setup.Current, Project.Current);
			}
			finally
			{
				blockLineRowSelectingEvent = false;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProgressWorksheetLine, PMProgressWorksheetLine.projectID> e)
		{
			if (e.Row != null && e.Row.ProjectID == null && Project.Current != null)
			{
				e.NewValue = Project.Current.ContractID;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProgressWorksheetLine, PMProgressWorksheetLine.inventoryID> e)
		{
			if (e.Row != null && e.Row.InventoryID == null && IsInventoryVisible(Project.Current) == false)
			{
				e.NewValue = PMInventorySelectorAttribute.EmptyInventoryID;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProgressWorksheetLine, PMProgressWorksheetLine.costCodeID> e)
		{
			if (e.Row != null && e.Row.CostCodeID == null && IsCostCodeVisible(Project.Current) == false)
			{
				e.NewValue = CostCodeAttribute.GetDefaultCostCode();
			}
		}

		protected virtual void _(Events.FieldVerifying<DailyFieldReport.date> e)
		{
			if (e.Row != null)
			{
				foreach (PMProgressWorksheet pw in SelectFrom<PMProgressWorksheet>
					.LeftJoin<DailyFieldReportProgressWorksheet>
						.On<PMProgressWorksheet.refNbr.IsEqual<DailyFieldReportProgressWorksheet.progressWorksheetId>>
					.Where<DailyFieldReportProgressWorksheet.dailyFieldReportId.IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View.Select(Base))
				{
					if (pw.Status != ProgressWorksheetStatus.OnHold)
					{
						throw new PXSetPropertyException<DailyFieldReport.date>(DailyFieldReportMessages.NoChangeDateByProgressWorksheet);
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProgressWorksheetLine.taskID> e)
		{
			if (e.Row != null)
			{
				PMTask data = (PMTask)PXSelectorAttribute.Select<PMProgressWorksheetLine.taskID>(e.Cache, e.Row, e.NewValue);
				PMProgressWorksheetLine row = (PMProgressWorksheetLine)e.Row;

				string refNbr = null;
				if (row.RefNbr == null)
				{
					PMProgressWorksheet pw = GetOnHoldProgressWorksheet();
					if (pw != null)
					{
						refNbr = pw.RefNbr;
					}
				}
				else
				{
					refNbr = row.RefNbr.Trim();
				}
				var lines = ProgressWorksheetLines.Select().RowCast<PMProgressWorksheetLine>().Where(line => line.RefNbr == refNbr).ToList();
				ProgressWorksheetEntry.CheckDublicateLine(ProgressWorksheetLines.Cache,lines, row, (int?)e.NewValue, row.AccountGroupID, row.InventoryID, row.CostCodeID, data?.TaskCD);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProgressWorksheetLine.accountGroupID> e)
		{
			if (e.Row != null)
			{
				PMAccountGroup data = (PMAccountGroup)PXSelectorAttribute.Select<PMProgressWorksheetLine.accountGroupID>(e.Cache, e.Row, e.NewValue);
				PMProgressWorksheetLine row = (PMProgressWorksheetLine)e.Row;

				string refNbr = null;
				if (row.RefNbr == null)
				{
					PMProgressWorksheet pw = GetOnHoldProgressWorksheet();
					if (pw != null)
					{
						refNbr = pw.RefNbr;
					}
				}
				else
				{
					refNbr = row.RefNbr.Trim();
				}
				var lines = ProgressWorksheetLines.Select().RowCast<PMProgressWorksheetLine>().Where(line => line.RefNbr == refNbr).ToList();
				ProgressWorksheetEntry.CheckDublicateLine(ProgressWorksheetLines.Cache, lines, row, row.TaskID, (int?)e.NewValue, row.InventoryID, row.CostCodeID, data?.GroupCD);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProgressWorksheetLine.inventoryID> e)
		{
			if (e.Row != null)
			{
				InventoryItem data = (InventoryItem)PXSelectorAttribute.Select<PMProgressWorksheetLine.inventoryID>(e.Cache, e.Row, e.NewValue);
				PMProgressWorksheetLine row = (PMProgressWorksheetLine)e.Row;

				string refNbr = null;
				if (row.RefNbr == null)
				{
					PMProgressWorksheet pw = GetOnHoldProgressWorksheet();
					if (pw != null)
					{
						refNbr = pw.RefNbr;
					}
				}
				else
				{
					refNbr = row.RefNbr.Trim();
				}
				var lines = ProgressWorksheetLines.Select().RowCast<PMProgressWorksheetLine>().Where(line => line.RefNbr == refNbr).ToList();
				ProgressWorksheetEntry.CheckDublicateLine(ProgressWorksheetLines.Cache, lines, row, row.TaskID, row.AccountGroupID, (int?)e.NewValue, row.CostCodeID, data?.InventoryCD);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProgressWorksheetLine.costCodeID> e)
		{
			if (e.Row != null)
			{
				PMCostCode data = (PMCostCode)PXSelectorAttribute.Select<PMProgressWorksheetLine.costCodeID>(e.Cache, e.Row, e.NewValue);
				PMProgressWorksheetLine row = (PMProgressWorksheetLine)e.Row;

				string refNbr = null;
				if (row.RefNbr == null)
				{
					PMProgressWorksheet pw = GetOnHoldProgressWorksheet();
					if (pw != null)
					{
						refNbr = pw.RefNbr;
					}
				}
				else
				{
					refNbr = row.RefNbr.Trim();
				}
				var lines = ProgressWorksheetLines.Select().RowCast<PMProgressWorksheetLine>().Where(line => line.RefNbr == refNbr).ToList();
				ProgressWorksheetEntry.CheckDublicateLine(ProgressWorksheetLines.Cache, lines, row, row.TaskID, row.AccountGroupID, row.InventoryID, (int?)e.NewValue, data?.CostCodeCD);
			}
		}

		public void _(Events.RowDeleting<PMProgressWorksheetLine> e)
		{
			if (e.Row != null)
			{
				if (e.Row.RefNbr != null && e.Row.RefNbr != Base.DailyFieldReport.Current.DailyFieldReportCd)
				{
					PMProgressWorksheet pw = PXSelect<PMProgressWorksheet, Where<PMProgressWorksheet.refNbr, Equal<Required<PMProgressWorksheet.refNbr>>>>.SelectSingleBound(Base, null, e.Row.RefNbr);
					if (pw != null && pw.Status != ProgressWorksheetStatus.OnHold)
					{
						throw new PXException(DailyFieldReportMessages.NoDeleteProgressWorksheetLine);
					}
				}
			}
		}

		
		public void _(Events.RowSelected<DailyFieldReport> e)
		{
			if (e.Row != null)
			{
				bool isTabEditable = ShouldTabsBeEditable(Base.DailyFieldReport.Current);

				loadTemplate.SetEnabled(isTabEditable);
				selectBudgetLines.SetEnabled(isTabEditable);

				PXUIFieldAttribute.SetVisible<PMProgressWorksheetLine.inventoryID>(ProgressWorksheetLines.Cache, null, IsInventoryVisible(Project.Current));
				PXUIFieldAttribute.SetVisible<PMProgressWorksheetLine.costCodeID>(ProgressWorksheetLines.Cache, null, IsCostCodeVisible(Project.Current));
			}
		}

		public void _(Events.RowSelected<PMProgressWorksheetLine> e)
		{
			if (e.Row != null)
			{
				bool isTabEditable = ShouldTabsBeEditable(Base.DailyFieldReport.Current);

				bool isLineEditable = true;
				if (e.Row.RefNbr != null && e.Row.RefNbr != Base.DailyFieldReport.Current.DailyFieldReportCd)
				{
					PMProgressWorksheet pw = PXSelect<PMProgressWorksheet, Where<PMProgressWorksheet.refNbr, Equal<Required<PMProgressWorksheet.refNbr>>>>.SelectSingleBound(Base, null, e.Row.RefNbr);
					if (pw != null && pw.Status != ProgressWorksheetStatus.OnHold)
					{
						isLineEditable = false;
					}
				}

				PXUIFieldAttribute.SetEnabled(ProgressWorksheetLines.Cache, e.Row, isTabEditable && isLineEditable);

				PXUIFieldAttribute.SetEnabled<PMProgressWorksheetLine.description>(ProgressWorksheetLines.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PMProgressWorksheetLine.uOM>(ProgressWorksheetLines.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PMProgressWorksheetLine.completedPercentTotalQuantity>(ProgressWorksheetLines.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PMProgressWorksheetLine.currentPeriodQuantity>(ProgressWorksheetLines.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PMProgressWorksheetLine.previouslyCompletedQuantity>(ProgressWorksheetLines.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PMProgressWorksheetLine.priorPeriodQuantity>(ProgressWorksheetLines.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PMProgressWorksheetLine.totalBudgetedQuantity>(ProgressWorksheetLines.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PMProgressWorksheetLine.totalCompletedQuantity>(ProgressWorksheetLines.Cache, e.Row, false);
			}
		}

		protected virtual bool IsInventoryVisible(PMProject project)
		{
			return project != null && (project.CostBudgetLevel == BudgetLevels.Item || project.CostBudgetLevel == BudgetLevels.Detail);
		}

		protected virtual bool IsCostCodeVisible(PMProject project)
		{
			return project != null && (project.CostBudgetLevel == BudgetLevels.CostCode || project.CostBudgetLevel == BudgetLevels.Detail) &&
					PXAccess.FeatureInstalled<FeaturesSet.costCodes>();
		}

		bool PXImportAttribute.IPXPrepareItems.PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (viewName.Equals(ProgressWorksheetLines.View.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				int? taskID = null;
				if (values.Contains(typeof(PMProgressWorksheetLine.taskID).Name))
				{
					string taskCD = (string)values[typeof(PMProgressWorksheetLine.taskID).Name];
					PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskCD, Equal<Required<PMTask.taskCD>>>>>
						.SelectSingleBound(Base, null, Project.Current.ContractID, taskCD);
					if (task != null)
					{
						taskID = task.TaskID;
					}
				}

				int? accountGroupID = null;
				if (values.Contains(typeof(PMProgressWorksheetLine.accountGroupID).Name))
				{
					string accountGroupCD = (string)values[typeof(PMProgressWorksheetLine.accountGroupID).Name];
					PMAccountGroup accountGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupCD, Equal<Required<PMAccountGroup.groupCD>>>>
						.SelectSingleBound(Base, null, accountGroupCD);
					if (accountGroup != null)
					{
						accountGroupID = accountGroup.GroupID;
					}
				}

				int? inventoryID = null;
				if (values.Contains(typeof(PMProgressWorksheetLine.inventoryID).Name))
				{
					string inventoryCD = (string)values[typeof(PMProgressWorksheetLine.inventoryID).Name];
					InventoryItem inventory = PXSelect<InventoryItem, Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>
						.SelectSingleBound(Base, null, inventoryCD);
					if (inventory != null)
					{
						inventoryID = inventory.InventoryID;
					}
				}

				int? costCodeID = null;
				if (values.Contains(typeof(PMProgressWorksheetLine.costCodeID).Name))
				{
					string costCodeCD = (string)values[typeof(PMProgressWorksheetLine.costCodeID).Name];
					PMCostCode costCode = PXSelect<PMCostCode, Where<PMCostCode.costCodeCD, Equal<Required<PMCostCode.costCodeCD>>>>
						.SelectSingleBound(Base, null, costCodeCD);
					if (costCode != null)
					{
						costCodeID = costCode.CostCodeID;
					}
				}

				foreach (PXResult<PMProgressWorksheetLine, DailyFieldReportProgressWorksheet, PMProgressWorksheet> line in ProgressWorksheetLines.Select())
				{
					PMProgressWorksheet progressWorksheet = line;
					PMProgressWorksheetLine progressWorksheetLine = line;
					if ((progressWorksheet == null || progressWorksheet.RefNbr == null || progressWorksheet.Hidden == true || progressWorksheet.Status == ProgressWorksheetStatus.OnHold) &&
						progressWorksheetLine.TaskID == taskID && progressWorksheetLine.AccountGroupID == accountGroupID &&
						(progressWorksheetLine.InventoryID == inventoryID || inventoryID == null && progressWorksheetLine.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID) &&
						(progressWorksheetLine.CostCodeID == costCodeID || costCodeID == null && progressWorksheetLine.CostCodeID == CostCodeAttribute.GetDefaultCostCode()))
					{
						return false;
					}
				}
			}

			return true;
		}

		bool PXImportAttribute.IPXPrepareItems.RowImporting(string viewName, object row)
		{
			return row == null;
		}

		bool PXImportAttribute.IPXPrepareItems.RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		void PXImportAttribute.IPXPrepareItems.PrepareItems(string viewName, IEnumerable items)
		{
		}
	}
}
