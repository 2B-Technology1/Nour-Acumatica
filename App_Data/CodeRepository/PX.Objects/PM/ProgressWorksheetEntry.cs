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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CommonServiceLocator;

using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.IN;

namespace PX.Objects.PM
{
	public class ProgressWorksheetEntry : PXGraph<ProgressWorksheetEntry, PMProgressWorksheet>
	{
		[PXViewName(Messages.ProgressWorksheet)]
		public SelectFrom<PMProgressWorksheet>
			.LeftJoin<PMProject>
				.On<PMProject.contractID.IsEqual<PMProgressWorksheet.projectID>>
			.Where<
				Brackets<PMProject.contractID.IsNull.Or<MatchUserFor<PMProject>>>
				.And<PMProgressWorksheet.hidden.IsNotEqual<True>>>
			.View Document;

		public PXSelect<PMProgressWorksheet, Where<PMProgressWorksheet.refNbr, Equal<Current<PMProgressWorksheet.refNbr>>>> DocumentSettings;
		[PXViewName(PM.Messages.Project)]
		public PXSetup<PMProject>.Where<PMProject.contractID.IsEqual<PMProgressWorksheet.projectID.FromCurrent>> Project;
		public PXSetup<Company> Company;
		public PXSetup<PMSetup> Setup;

		[PXImport(typeof(PMProgressWorksheet))]
		[PXFilterable]
		public PXSelect<PMProgressWorksheetLine,
			Where<PMProgressWorksheetLine.refNbr, Equal<Current<PMProgressWorksheet.refNbr>>>> Details;

		#region Entity Event Handlers

		public PXWorkflowEventHandler<PMProgressWorksheet> OnRelease;

		#endregion

		[PXCopyPasteHiddenView]
		[PXViewName(PM.Messages.Approval)]
		public EPApprovalAutomation<PMProgressWorksheet, PMProgressWorksheet.approved, PMProgressWorksheet.rejected, PMProgressWorksheet.hold, PMSetupProgressWorksheetApproval> Approval;

		[System.SerializableAttribute()]
		[PXCacheName(Messages.CostBudgetLineFilter)]
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
					And<PMAccountGroup.isActive, Equal<True>>>>>>>>>>(this);

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
				PMCostCode costCodeFrom = (new PXSelect<PMCostCode, Where<PMCostCode.costCodeID, Equal<Current<CostBudgetLineFilter.costCodeFrom>>>>(this)).SelectSingle();
				costBudgetSelect.WhereAnd(typeof(Where<PMCostCode.costCodeCD, GreaterEqual<Required<PMCostCode.costCodeCD>>>));
				parameters.Add(costCodeFrom.CostCodeCD);
			}

			if (costBudgetfilter.Current.CostCodeTo != null)
			{
				PMCostCode costCodeTo = (new PXSelect<PMCostCode, Where<PMCostCode.costCodeID, Equal<Current<CostBudgetLineFilter.costCodeTo>>>>(this)).SelectSingle();
				costBudgetSelect.WhereAnd(typeof(Where<PMCostCode.costCodeCD, LessEqual<Required<PMCostCode.costCodeCD>>>));
				parameters.Add(costCodeTo.CostCodeCD);
			}
			parameters.AddRange(PXView.Parameters);

			PXDelegateResult delResult = new PXDelegateResult();
			delResult.Capacity = 202;
			delResult.IsResultFiltered = false;
			delResult.IsResultSorted = true;
			delResult.IsResultTruncated = false;

			var view = new PXView(this, false, costBudgetSelect.View.BqlSelect);
			var startRow = PXView.StartRow;
			int totalRows = 0;
			var resultset = view.Select(PXView.Currents, parameters.ToArray(), PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;

			HashSet<BudgetKeyTuple> existing = GetExistingCostBudgetLines();

			foreach (PXResult<PMCostBudget, PMCostCode> costBudgetResult in resultset)
			{
				PMCostBudget costBudget = costBudgetResult;
				if (!existing.Contains(BudgetKeyTuple.Create(costBudget)))
				{
				delResult.Add(costBudget);
			}
			}

			return delResult;
		}

		protected virtual HashSet<BudgetKeyTuple> GetExistingCostBudgetLines()
		{
			HashSet<BudgetKeyTuple> existing = new HashSet<BudgetKeyTuple>();
			foreach (PMProgressWorksheetLine line in Details.Select())
			{
				existing.Add(BudgetKeyTuple.Create(line));
			}

			return existing;
		}

		public PXAction<PMProgressWorksheet> removeHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Remove Hold")]
		protected virtual IEnumerable RemoveHold(PXAdapter adapter) => adapter.Get();

		public PXAction<PMProgressWorksheet> hold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold")]
		protected virtual IEnumerable Hold(PXAdapter adapter) => adapter.Get();

		public PXAction<PMProgressWorksheet> release;
		[PXUIField(DisplayName = GL.Messages.Release)]
		[PXProcessButton]
		public IEnumerable Release(PXAdapter adapter)
		{
			this.Save.Press();

			PXLongOperation.StartOperation(this, delegate () {

				ProgressWorksheetEntry pe = PXGraph.CreateInstance<ProgressWorksheetEntry>();
				pe.Document.Current = Document.Current;
				pe.ReleaseDocument(Document.Current);

			});
			return adapter.Get();
		}

		public virtual void ReleaseDocument(PMProgressWorksheet doc)
		{
			if (doc == null)
				throw new ArgumentNullException();

			if (doc.Released == true)
				throw new PXException(EP.Messages.AlreadyReleased);

			doc.Released = true;
			Document.Update(doc);
			Save.Press();
		}

		public PXAction<PMProgressWorksheet> loadTemplate;
		[PXUIField(DisplayName = "Load Template", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable LoadTemplate(PXAdapter adapter)
		{
			if (Document.Current.Hold == true && Document.Current.ProjectID != null)
			{
				Details.Cache.ForceExceptionHandling = true;

				HashSet<BudgetKeyTuple> existing = GetExistingCostBudgetLines();

				var costBudgetSelect = new PXSelectJoin<PMCostBudget,
					InnerJoin<PMTask, On<PMCostBudget.projectID, Equal<PMTask.projectID>, And<PMCostBudget.projectTaskID, Equal<PMTask.taskID>>>,
					InnerJoin<PMAccountGroup, On<PMCostBudget.accountGroupID, Equal<PMAccountGroup.groupID>>>>,
					Where<PMCostBudget.projectID, Equal<Required<PMProject.contractID>>,
						And<PMCostBudget.type, Equal<GL.AccountType.expense>,
						And<PMCostBudget.productivityTracking, Equal<PMProductivityTrackingType.template>,
						And<PMTask.status, NotEqual<ProjectTaskStatus.canceled>,
						And<PMTask.status, NotEqual<ProjectTaskStatus.completed>,
						And<PMAccountGroup.isActive, Equal<True>>>>>>>>(this);

				foreach (PMCostBudget line in costBudgetSelect.Select(Document.Current.ProjectID))
				{
					if (!existing.Contains(BudgetKeyTuple.Create(line)))
					{
						PMProgressWorksheetLine newline = new PMProgressWorksheetLine();
						newline.ProjectID = line.ProjectID;
						newline.TaskID = line.ProjectTaskID;
						newline.InventoryID = line.InventoryID;
						newline.AccountGroupID = line.AccountGroupID;
						newline.CostCodeID = line.CostCodeID;
						Details.Insert(newline);
					}
				}
			}

			return adapter.Get();
		}

		public PXAction<PMProgressWorksheet> selectBudgetLines;
		[PXUIField(DisplayName = "Select Budget Lines", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable SelectBudgetLines(PXAdapter adapter)
		{
			IEnumerable result = null;

			if (this.Document.Current.Hold == true && Document.Current.ProjectID != null)
			{
				costBudgetfilter.Current.ProjectID = Document.Current.ProjectID;
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
			if (this.Document.Current.Hold == true)
			{
				Details.Cache.ForceExceptionHandling = true;

				HashSet<BudgetKeyTuple> existing = GetExistingCostBudgetLines();

				foreach (PMCostBudget line in CostBudgets.Cache.Cached)
				{
					if (line.Selected == true && !existing.Contains(BudgetKeyTuple.Create(line)))
					{
						PMProgressWorksheetLine newline = new PMProgressWorksheetLine();
						newline.ProjectID = line.ProjectID;
						newline.TaskID = line.ProjectTaskID;
						newline.InventoryID = line.InventoryID;
						newline.AccountGroupID = line.AccountGroupID;
						newline.CostCodeID = line.CostCodeID;
						Details.Insert(newline);
					}
				}
			}

			return adapter.Get();
		}

		public const string ProgressWorksheetReport = "PM657000";

		public PXAction<PMProgressWorksheet> print;
		[PXUIField(DisplayName = "Print Project Progress Report", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.Report)]
		protected virtual IEnumerable Print(PXAdapter adapter)
		{
			OpenReport(ProgressWorksheetReport, Document.Current);

			return adapter.Get();
		}

		public virtual void OpenReport(string reportID, PMProgressWorksheet doc)
		{
			if (doc != null && doc.ProjectID != null)
			{
				string specificReportID = new NotificationUtility(this).SearchProjectReport(reportID, Project.Current.ContractID, Project.Current.DefaultBranchID);

				throw new PXReportRequiredException(new Dictionary<string, string>
				{
					["ProjectID"] = Project.Current.ContractCD,
					["EndDate"] = doc.Date.ToString()
				}, specificReportID, specificReportID);
			}
		}

		public PXAction<PMProgressWorksheet> correct;
		[PXUIField(DisplayName = "Correct")]
		[PXProcessButton]
		public IEnumerable Correct(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				PXLongOperation.StartOperation(this, delegate ()
				{
					ProgressWorksheetEntry pe = PXGraph.CreateInstance<ProgressWorksheetEntry>();
					pe.Document.Current = Document.Current;
					pe.CorrectDocument(Document.Current);
				});
			}

			return adapter.Get();
		}

		public virtual void CorrectDocument(PMProgressWorksheet doc)
		{
			doc.Hold = true;
			doc.Approved = false;
			doc.Rejected = false;
			doc.Released = false;
			doc.Status = ProgressWorksheetStatus.OnHold;
			Document.Update(doc);
			Save.Press();
		}

		public PXAction<PMProgressWorksheet> reverse;
		[PXUIField(DisplayName = "Reverse")]
		[PXProcessButton]
		public IEnumerable Reverse(PXAdapter adapter)
		{
			ReverseDocument();

			return new PMProgressWorksheet[] { Document.Current };
		}

		public virtual void ReverseDocument()
		{
			if (Document.Current == null)
				return;

			ProgressWorksheetEntry target = PXGraph.CreateInstance<ProgressWorksheetEntry>();
			target.SelectTimeStamp();

			PMProgressWorksheet source = (PMProgressWorksheet)Document.Cache.CreateCopy(Document.Current);

			List<PMProgressWorksheetLine> lines = new List<PMProgressWorksheetLine>();
			foreach (PMProgressWorksheetLine line in Details.Select())
			{
				PMProgressWorksheetLine sourceLine = (PMProgressWorksheetLine)Details.Cache.CreateCopy(line);
				lines.Add(sourceLine);
			}

			source.RefNbr = null;
			source.Released = false;
			source.Hold = true;
			source.Approved = false;
			source.Rejected = false;
			source.Status = ProgressWorksheetStatus.OnHold;
			source.LineCntr = 0;
			source.NoteID = Guid.NewGuid();

			source = target.Document.Insert(source);

			foreach (PMProgressWorksheetLine line in lines)
			{
				line.RefNbr = source.RefNbr;
				line.Qty = -line.Qty.GetValueOrDefault();
				line.NoteID = null;
				line.LineNbr = null;
				target.Details.Insert(line);
			}

			PXRedirectHelper.TryRedirect(target, PXRedirectHelper.WindowMode.Same);
		}

		#region EPApproval Cache Attached - Approvals Fields
		[PXDBDate()]
		[PXDefault(typeof(PMProgressWorksheet.date), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.docDate> e)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(PMProgressWorksheet.description), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.descr> e)
		{
		}

		#endregion

		public virtual bool CanDeleteDocument(PMProgressWorksheet doc)
		{
			if (doc == null)
				return true;

			if (doc.Released == true)
				return false;

			if (doc.Hold == true)
			{
				return true;
			}
			else
			{
				if (doc.Rejected == true)
					return false;

				if (doc.Approved == true)
				{
					if (PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>() && Setup.Current.ProgressWorksheetApprovalMapID != null)
					{
						return false;
					}
					else
					{
						return true;
					}
				}
				else
				{
					return false;
				}
			}
		}

		public virtual bool IsProjectEnabled()
		{
			if (Details.Cache.IsInsertedUpdatedDeleted)
				return false;

			if (Details.Select().Count > 0)
				return false;

			return true;
		}

		protected virtual void _(Events.FieldVerifying<PMProgressWorksheetLine.taskID> e)
		{
			if (e.Row != null)
			{
				PMTask data = (PMTask)PXSelectorAttribute.Select<PMProgressWorksheetLine.taskID>(e.Cache, e.Row, e.NewValue);
				PMProgressWorksheetLine row = (PMProgressWorksheetLine)e.Row;
				CheckDublicateLine(Details.Cache, Details.Select().RowCast<PMProgressWorksheetLine>(), row, (int?)e.NewValue, row.AccountGroupID, row.InventoryID, row.CostCodeID, data?.TaskCD);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProgressWorksheetLine.accountGroupID> e)
		{
			if (e.Row != null)
			{
				PMAccountGroup data = (PMAccountGroup)PXSelectorAttribute.Select<PMProgressWorksheetLine.accountGroupID>(e.Cache, e.Row, e.NewValue);
				PMProgressWorksheetLine row = (PMProgressWorksheetLine)e.Row;
				CheckDublicateLine(Details.Cache, Details.Select().RowCast<PMProgressWorksheetLine>(), row, row.TaskID, (int?)e.NewValue, row.InventoryID, row.CostCodeID, data?.GroupCD);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProgressWorksheetLine.inventoryID> e)
		{
			if (e.Row != null)
			{
				InventoryItem data = (InventoryItem)PXSelectorAttribute.Select<PMProgressWorksheetLine.inventoryID>(e.Cache, e.Row, e.NewValue);
				PMProgressWorksheetLine row = (PMProgressWorksheetLine)e.Row;
				CheckDublicateLine(Details.Cache, Details.Select().RowCast<PMProgressWorksheetLine>(), row, row.TaskID, row.AccountGroupID, (int?)e.NewValue, row.CostCodeID, data?.InventoryCD);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProgressWorksheetLine.costCodeID> e)
		{
			if (e.Row != null)
			{
				PMCostCode data = (PMCostCode)PXSelectorAttribute.Select<PMProgressWorksheetLine.costCodeID>(e.Cache, e.Row, e.NewValue);
				PMProgressWorksheetLine row = (PMProgressWorksheetLine)e.Row;
				CheckDublicateLine(Details.Cache, Details.Select().RowCast<PMProgressWorksheetLine>(), row, row.TaskID, row.AccountGroupID, row.InventoryID, (int?)e.NewValue, data?.CostCodeCD);

			}
		}

		public static void CheckDublicateLine(
			PXCache cache,
			IEnumerable<PMProgressWorksheetLine> lines,
			PMProgressWorksheetLine row,
			int? taskID,
			int? accountGroupID,
			int? inventoryID,
			int? costCodeID,
			string newValueCD)
		{
			if (taskID != null && accountGroupID != null)
			{
				foreach (PMProgressWorksheetLine line in lines)
				{
					if (line != row &&  line.TaskID == taskID && line.AccountGroupID == accountGroupID && line.InventoryID == inventoryID && line.CostCodeID == costCodeID)
					{
						var ex = new PXSetPropertyException(Messages.PWDublicateLine, PXErrorLevel.Error);
						ex.ErrorValue = newValueCD;
						throw ex;
					}
				}
			}

			PXUIFieldAttribute.SetError<PMProgressWorksheetLine.accountGroupID>(cache, row, null);
		}

		public static void CheckCostBudgetLine(PXGraph graph, PXCache cache, PMProject project, PMProgressWorksheetLine line)
		{
			var costBudgetSelect = new PXSelect<PMCostBudget,
				Where<PMCostBudget.projectID, Equal<Required<PMCostBudget.projectID>>,
					And<PMCostBudget.projectTaskID, Equal<Required<PMCostBudget.projectTaskID>>,
					And<PMCostBudget.accountGroupID, Equal<Required<PMCostBudget.accountGroupID>>,
					And<PMCostBudget.inventoryID, Equal<Required<PMCostBudget.inventoryID>>,
					And<PMCostBudget.costCodeID, Equal<Required<PMCostBudget.costCodeID>>,
					And<PMCostBudget.type, Equal<GL.AccountType.expense>,
					And<PMCostBudget.productivityTracking, IsNotNull,
					And<PMCostBudget.productivityTracking, NotEqual<PMProductivityTrackingType.notAllowed>>>>>>>>>>(graph);
			PMCostBudget costBudget = costBudgetSelect.SelectSingle(line.ProjectID, line.TaskID, line.AccountGroupID, line.InventoryID, line.CostCodeID);

			if (costBudget == null)
			{
				PXUIFieldAttribute.SetError<PMProgressWorksheetLine.accountGroupID>(cache, line, string.Format(Messages.PWCostBudgetNotExist, project.ContractCD));
				throw new PXException(Messages.PWCostBudgetNotExist, project.ContractCD);
			}
		}
		public override void Persist()
		{
			if (Document.Current != null)
			{
				if (Project.Current != null && Project.Current.Status != ProjectStatus.Active)
				{
					PXUIFieldAttribute.SetError<PMProgressWorksheet.projectID>(Document.Cache, Document.Current, string.Format(Messages.PWProjectIsNotActive, Project.Current.ContractCD), Project.Current.ContractCD);
					throw new PXException(Messages.PWProjectIsNotActive, Project.Current.ContractCD);
				}

				List<PMProgressWorksheetLine> lines;
				try
				{
					blockLineSelectingEvent = true;

					lines = Details.Select().RowCast<PMProgressWorksheetLine>().ToList();
				}
				finally
				{
					blockLineSelectingEvent = false;
				}

				foreach (PMProgressWorksheetLine line in lines)
				{
					if (line.AccountGroupID != null)
					{
						PMAccountGroup group = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, line.AccountGroupID);
						if (group.IsActive == false)
						{
							PXUIFieldAttribute.SetError<PMProgressWorksheetLine.accountGroupID>(Details.Cache, line, string.Format(PM.Messages.InactiveAccountGroup, group.GroupCD), group.GroupCD);
							throw new PXException(PM.Messages.InactiveAccountGroup, group.GroupCD);
						}
					}

					if (line.TaskID != null)
					{
						PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(this, line.ProjectID, line.TaskID);
						if (task.Status == ProjectTaskStatus.Canceled || task.Status == ProjectTaskStatus.Completed)
						{
							string status;
							(new ProjectTaskStatus.ListAttribute()).ValueLabelDic.TryGetValue(task.Status, out status);
							PXUIFieldAttribute.SetError<PMProgressWorksheetLine.taskID>(Details.Cache, line, string.Format(PM.Messages.PWProjectTaskNotActive, task.TaskCD, status), task.TaskCD);
							throw new PXException(PM.Messages.PWProjectTaskNotActive, task.TaskCD, status);
						}
					}

					CheckCostBudgetLine(this, Details.Cache, Project.Current, line);
				}
			}

			base.Persist();
		}

		protected virtual void _(Events.RowSelected<PMProgressWorksheet> e)
		{
			if (e.Row != null)
			{
				bool isDeletable = CanDeleteDocument(e.Row);
				bool isEditable = e.Row.Hold == true;
				bool hasActiveProject = Project.Current != null && Project.Current.Status == ProjectStatus.Active;

				Document.Cache.AllowDelete = isDeletable;
				Details.Cache.AllowUpdate = isEditable && hasActiveProject;
				Details.Cache.AllowDelete = isEditable;
				Details.Cache.AllowInsert = isEditable && hasActiveProject;

				correct.SetEnabled(CanBeCorrected(e.Row));

				loadTemplate.SetEnabled(isEditable && hasActiveProject);
				selectBudgetLines.SetEnabled(isEditable && hasActiveProject);

				PXUIFieldAttribute.SetEnabled<PMProgressWorksheet.date>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProgressWorksheet.description>(e.Cache, e.Row, isEditable);

				PXUIFieldAttribute.SetEnabled<PMProgressWorksheet.projectID>(e.Cache, e.Row, isEditable && IsProjectEnabled());

				PXUIFieldAttribute.SetVisible<PMProgressWorksheetLine.inventoryID>(Details.Cache, null, IsInventoryVisible());
				PXUIFieldAttribute.SetVisible<PMProgressWorksheetLine.costCodeID>(Details.Cache, null, IsCostCodeVisible());

				if (PXUIFieldAttribute.GetError< PMProgressWorksheet.projectID >(Document.Cache, e.Row) == null &&
					Document.Current.Status != ProgressWorksheetStatus.Closed && Document.Current.Status != ProgressWorksheetStatus.Rejected &&
					Project.Current != null && Project.Current.Status != ProjectStatus.Active)
				{
					PXUIFieldAttribute.SetError<PMProgressWorksheet.projectID>(Document.Cache, e.Row, string.Format(Messages.PWProjectIsNotActive, Project.Current.ContractCD), Project.Current.ContractCD);
				}

				CheckDate(Document.Current.Date);
			}
		}

		public virtual bool CanBeCorrected(PMProgressWorksheet row)
		{
			if (row.Released != true)
				return false;

			return true;
		}

		protected virtual void _(Events.RowSelecting<PMProgressWorksheetLine> e)
		{
			if (blockLineSelectingEvent == false)
			{
				CalculateQuantities(e.Cache, (PMProgressWorksheetLine)e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProgressWorksheetLine.taskID> e)
		{
			CalculateQuantities(e.Cache, (PMProgressWorksheetLine)e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMProgressWorksheetLine.accountGroupID> e)
		{
			CalculateQuantities(e.Cache, (PMProgressWorksheetLine)e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMProgressWorksheetLine.inventoryID> e)
		{
			CalculateQuantities(e.Cache, (PMProgressWorksheetLine)e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMProgressWorksheetLine.costCodeID> e)
		{
			CalculateQuantities(e.Cache, (PMProgressWorksheetLine)e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMProgressWorksheetLine.qty> e)
		{
			CalculateQuantities(e.Cache, (PMProgressWorksheetLine)e.Row);
		}

		private bool blockLineSelectingEvent;

		protected virtual void CalculateQuantities(PXCache cache, PMProgressWorksheetLine line)
		{
			try
			{
				blockLineSelectingEvent = true;

				if (Document.Current != null)
				{
					CalculateQuantities(this, cache, Document.Current.Date.Value, Document.Current.Status, line, Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Inserted, Setup.Current, Project.Current);
				}
			}
			finally
			{
				blockLineSelectingEvent = false;
			}
		}

		public static void CalculateQuantities(PXGraph graph, PXCache cache, DateTime docDate, string docStatus, PMProgressWorksheetLine calculatedLine, bool newRefNbr, PMSetup setup, PMProject project)
		{
			if (calculatedLine != null && calculatedLine.TaskID != null && calculatedLine.AccountGroupID != null && calculatedLine.InventoryID != null && calculatedLine.CostCodeID != null)
			{
				Func<PXGraph, IFinPeriodRepository> factoryFunc = (Func<PXGraph, IFinPeriodRepository>)ServiceLocator.Current.GetService(typeof(Func<PXGraph, IFinPeriodRepository>));
				IFinPeriodRepository service = factoryFunc(graph);

				var parentOrganizationID = PXAccess.GetParentOrganizationID(graph.Accessinfo.BranchID);
				var currentDate = docDate;
				var currentFinperiod = service.GetFinPeriodByDate(currentDate, parentOrganizationID);

				FinPeriod previousFinperiod = null;
				if (currentFinperiod != null)
				{
					previousFinperiod = service.FindPrevPeriod(parentOrganizationID, currentFinperiod.FinPeriodID);
				}

				using (new PXConnectionScope())
				{
					var costBudgetSelect = new PXSelect<PMCostBudget,
						Where<PMCostBudget.projectID, Equal<Required<PMCostBudget.projectID>>,
							And<PMCostBudget.projectTaskID, Equal<Required<PMCostBudget.projectTaskID>>,
							And<PMCostBudget.accountGroupID, Equal<Required<PMCostBudget.accountGroupID>>,
							And<PMCostBudget.inventoryID, Equal<Required<PMCostBudget.inventoryID>>,
							And<PMCostBudget.costCodeID, Equal<Required<PMCostBudget.costCodeID>>,
							And<PMCostBudget.type, Equal<GL.AccountType.expense>>>>>>>>(graph);
					PMCostBudget costBudget = costBudgetSelect.SelectSingle(calculatedLine.ProjectID, calculatedLine.TaskID, calculatedLine.AccountGroupID, calculatedLine.InventoryID, calculatedLine.CostCodeID);

					List<object> parameters = new List<object>() { calculatedLine.ProjectID, calculatedLine.TaskID, calculatedLine.AccountGroupID, calculatedLine.InventoryID, calculatedLine.CostCodeID };
					var progressWorksheetSelect = new PXSelectJoin<PMProgressWorksheetLine,
						InnerJoin<PMProgressWorksheet, On<PMProgressWorksheetLine.refNbr, Equal<PMProgressWorksheet.refNbr>>>,
						Where<PMProgressWorksheetLine.projectID, Equal<Required<PMProgressWorksheetLine.projectID>>,
							And<PMProgressWorksheetLine.taskID, Equal<Required<PMProgressWorksheetLine.taskID>>,
							And<PMProgressWorksheetLine.accountGroupID, Equal<Required<PMProgressWorksheetLine.accountGroupID>>,
							And<PMProgressWorksheetLine.inventoryID, Equal<Required<PMProgressWorksheetLine.inventoryID>>,
							And<PMProgressWorksheetLine.costCodeID, Equal<Required<PMProgressWorksheetLine.costCodeID>>,
							And<PMProgressWorksheet.status, Equal<ProgressWorksheetStatus.closed>>>>>>>>(graph);
					if (newRefNbr == false)
					{
						progressWorksheetSelect.WhereAnd(typeof(Where<PMProgressWorksheet.refNbr, NotEqual<Required<PMProgressWorksheet.refNbr>>>));
						parameters.Add(calculatedLine.RefNbr);
					}

					decimal previouslyCompletedQuantity = 0;
					decimal priorPeriodQuantity = 0;
					decimal currentPeriodQuantity = 0;
					decimal totalCompletedQuantity = 0;
					decimal completedPercentTotalQuantity = 0;
					foreach (PXResult<PMProgressWorksheetLine, PMProgressWorksheet> selectResult in progressWorksheetSelect.Select(parameters.ToArray()))
					{
						PMProgressWorksheet pw = selectResult;
						PMProgressWorksheetLine pwl = selectResult;

						if (pwl.RefNbr != calculatedLine.RefNbr)
						{
							var finperiod = service.GetFinPeriodByDate(pw.Date, parentOrganizationID);

							if (pw.Date <= currentDate)
							{
								previouslyCompletedQuantity += pwl.Qty.Value;
							}

							if (finperiod != null &&
								previousFinperiod != null &&
								finperiod.FinPeriodID == previousFinperiod.FinPeriodID)
							{
								priorPeriodQuantity += pwl.Qty.Value;
							}

							if (finperiod != null &&
								currentFinperiod != null &&
								finperiod.FinPeriodID == currentFinperiod.FinPeriodID &&
								pw.Date <= currentDate)
							{
								currentPeriodQuantity += pwl.Qty.Value;
							}
						}
					}
					progressWorksheetSelect.View.Clear();

					if (docStatus == ProgressWorksheetStatus.Closed)
					{
						currentPeriodQuantity += calculatedLine.Qty.GetValueOrDefault(0);
					}

					totalCompletedQuantity = previouslyCompletedQuantity + calculatedLine.Qty.GetValueOrDefault(0);
					if (costBudget != null && costBudget.RevisedQty != 0)
					{
						if (costBudget.ProductivityTracking != PMProductivityTrackingType.NotAllowed || docStatus == ProgressWorksheetStatus.Closed)
						{
							completedPercentTotalQuantity = Math.Round(100.0M * totalCompletedQuantity / costBudget.RevisedQty.Value, 2);
						}
					}

					cache.SetValue<PMProgressWorksheetLine.previouslyCompletedQuantity>(calculatedLine, previouslyCompletedQuantity);
					cache.SetValue<PMProgressWorksheetLine.priorPeriodQuantity>(calculatedLine, priorPeriodQuantity);
					cache.SetValue<PMProgressWorksheetLine.currentPeriodQuantity>(calculatedLine, currentPeriodQuantity);
					cache.SetValue<PMProgressWorksheetLine.totalCompletedQuantity>(calculatedLine, totalCompletedQuantity);
					cache.SetValue<PMProgressWorksheetLine.completedPercentTotalQuantity>(calculatedLine, completedPercentTotalQuantity);
					if (costBudget == null)
					{
						if (docStatus == ProgressWorksheetStatus.Closed)
						{
							string description = null;
							if (project.CostBudgetLevel == BudgetLevels.CostCode || project.CostBudgetLevel == BudgetLevels.Detail)
							{
								description = PMCostCode.PK.Find(graph, calculatedLine.CostCodeID).Description;
							}
							else if (project.CostBudgetLevel == BudgetLevels.Item)
							{
								description = InventoryItem.PK.Find(graph, calculatedLine.InventoryID).Descr;
							}
							cache.SetValue<PMProgressWorksheetLine.description>(calculatedLine, description);

							cache.SetValue<PMProgressWorksheetLine.uOM>(calculatedLine, setup.EmptyItemUOM);
							cache.SetValue<PMProgressWorksheetLine.totalBudgetedQuantity>(calculatedLine, 0.0M);
						}
						else
						{
							cache.SetValue<PMProgressWorksheetLine.description>(calculatedLine, null);
							cache.SetValue<PMProgressWorksheetLine.uOM>(calculatedLine, null);
							cache.SetValue<PMProgressWorksheetLine.totalBudgetedQuantity>(calculatedLine, 0.0M);
						}
					}
					else if (costBudget.ProductivityTracking == PMProductivityTrackingType.NotAllowed)
					{
						if (docStatus == ProgressWorksheetStatus.Closed)
						{
							cache.SetValue<PMProgressWorksheetLine.description>(calculatedLine, costBudget.Description);
							cache.SetValue<PMProgressWorksheetLine.uOM>(calculatedLine, costBudget.UOM);
							cache.SetValue<PMProgressWorksheetLine.totalBudgetedQuantity>(calculatedLine, costBudget.RevisedQty);
						}
						else
						{
							cache.SetValue<PMProgressWorksheetLine.description>(calculatedLine, null);
							cache.SetValue<PMProgressWorksheetLine.uOM>(calculatedLine, null);
							cache.SetValue<PMProgressWorksheetLine.totalBudgetedQuantity>(calculatedLine, 0.0M);
						}
					}
					else
					{
						cache.SetValue<PMProgressWorksheetLine.description>(calculatedLine, costBudget.Description);
						cache.SetValue<PMProgressWorksheetLine.uOM>(calculatedLine, costBudget.UOM);
						cache.SetValue<PMProgressWorksheetLine.totalBudgetedQuantity>(calculatedLine, costBudget.RevisedQty);
					}
				}
			}
			else if (calculatedLine != null)
			{
				cache.SetValue<PMProgressWorksheetLine.previouslyCompletedQuantity>(calculatedLine, 0.0M);
				cache.SetValue<PMProgressWorksheetLine.priorPeriodQuantity>(calculatedLine, 0.0M);
				cache.SetValue<PMProgressWorksheetLine.currentPeriodQuantity>(calculatedLine, 0.0M);
				cache.SetValue<PMProgressWorksheetLine.totalCompletedQuantity>(calculatedLine, 0.0M);
				cache.SetValue<PMProgressWorksheetLine.completedPercentTotalQuantity>(calculatedLine, 0.0M);
				cache.SetValue<PMProgressWorksheetLine.description>(calculatedLine, null);
				cache.SetValue<PMProgressWorksheetLine.uOM>(calculatedLine, null);
				cache.SetValue<PMProgressWorksheetLine.totalBudgetedQuantity>(calculatedLine, 0.0M);
			}
		}

		protected virtual bool IsInventoryVisible()
		{
			return Project.Current != null && (Project.Current.CostBudgetLevel == BudgetLevels.Item || Project.Current.CostBudgetLevel == BudgetLevels.Detail);
		}

		protected virtual bool IsCostCodeVisible()
		{
			return Project.Current != null && (Project.Current.CostBudgetLevel == BudgetLevels.CostCode || Project.Current.CostBudgetLevel == BudgetLevels.Detail) &&
					PXAccess.FeatureInstalled<FeaturesSet.costCodes>();
		}

		protected virtual void _(Events.FieldDefaulting<PMProgressWorksheetLine, PMProgressWorksheetLine.inventoryID> e)
		{
			if (e.Row != null && e.Row.InventoryID == null && IsInventoryVisible() == false)
			{
				e.NewValue = PMInventorySelectorAttribute.EmptyInventoryID;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProgressWorksheetLine, PMProgressWorksheetLine.costCodeID> e)
		{
			if (e.Row != null && e.Row.CostCodeID == null && IsCostCodeVisible() == false)
			{
				e.NewValue = CostCodeAttribute.GetDefaultCostCode();
			}
		}

		protected virtual void CheckDate(DateTime? date)
		{
			if (date != null)
			{
				Func<PXGraph, IFinPeriodRepository> factoryFunc = (Func<PXGraph, IFinPeriodRepository>)ServiceLocator.Current.GetService(typeof(Func<PXGraph, IFinPeriodRepository>));
				IFinPeriodRepository service = factoryFunc(this);

				string strDate = date.Value.ToShortDateString();
				if (service != null)
				{
					try
					{
						var finperiod = service.GetFinPeriodByDate(date, PXAccess.GetParentOrganizationID(Accessinfo.BranchID));

						if (finperiod == null)
						{
							PXUIFieldAttribute.SetWarning<PMProgressWorksheet.date>(Document.Cache, Document.Current, string.Format(Messages.PWDateInvalidFinperiod, strDate));
						}
						else if (finperiod.Status == FinPeriod.status.Inactive)
						{
							PXUIFieldAttribute.SetWarning<PMProgressWorksheet.date>(Document.Cache, Document.Current,
								string.Format(Messages.PWDateFinperiodInactive, strDate, PXAccess.GetParentOrganization(this.Accessinfo.BranchID).OrganizationCD));
						}
						else if (finperiod.Status == FinPeriod.status.Closed)
						{
							PXUIFieldAttribute.SetWarning<PMProgressWorksheet.date>(Document.Cache, Document.Current,
								string.Format(Messages.PWDateFinperiodClosed, strDate, PXAccess.GetParentOrganization(this.Accessinfo.BranchID).OrganizationCD));
						}
					}
					catch (PXException ex)
					{
						PXUIFieldAttribute.SetWarning<PMProgressWorksheet.date>(Document.Cache, Document.Current, string.Format(Messages.PWDateInvalidFinperiod, strDate));
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProgressWorksheet, PMProgressWorksheet.date> e)
		{
			if (e.NewValue != null)
			{
				CheckDate((DateTime?)e.NewValue);
			}
		}
	}
}
