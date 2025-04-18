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
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.AR;
using System;
using System.Collections;
using System.Collections.Generic;
using static PX.Objects.PM.ProformaEntry.ProformaTotalsCounter;
using PX.Objects.GL;
using System.Text;
using System.Diagnostics;
using PX.Data.WorkflowAPI;
using PX.Objects.IN;

namespace PX.Objects.CN.ProjectAccounting.PM.GraphExtensions
{
	public class ProformaEntryExt : PXGraphExtension<ProformaEntry>
    {
		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMProforma.projectID>>>> ProjectProperties;

		public const string AIAReport = "PM644000";
		public const string AIAWithQtyReport = "PM644500";
		

		public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
			ProjectAccountingMessages.TaskTypeIsNotAvailable, typeof(PMTask.type))]
		[PXFormula(typeof(Validate<PMProformaLine.costCodeID, PMProformaLine.description>))]
		protected virtual void _(Events.CacheAttached<PMProformaTransactLine.taskID> e)
		{
		}

		public PXAction<PMProforma> aia;
		[PXUIField(DisplayName = "Print AIA Report")]
		[PXButton(SpecialType = PXSpecialButtonType.Report)]
		public virtual IEnumerable Aia(PXAdapter adapter)
		{
			if (Base.Document.Current != null)
			{
				Base.RecalculateExternalTaxesSync = true;
				Base.Document.Cache.SetValue<PMProforma.isAIAOutdated>(Base.Document.Current, false);
				Base.Document.Cache.MarkUpdated(Base.Document.Current);
				Base.Save.Press();

				var resultset = BuildResultsetForAIA();
				
				throw new PXReportRequiredException(resultset, GetAIAReportID(), "AIA Report");
			}

			return adapter.Get();
		}

		public virtual string GetAIAReportID()
		{
			if (Base.Document.Current != null && Base.Project.Current != null)
			{
				if (Base.Project.Current.IncludeQtyInAIA == true)
					return AIAWithQtyReport;
			}

			return AIAReport;
		}

		
		
		public virtual PXReportResultset BuildResultsetForAIA()
		{
			var contractTotalSelect = new PXSelectGroupBy<PMRevenueBudget,
						Where<PMRevenueBudget.projectID, Equal<Current<PMProforma.projectID>>,
						And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>,
						Aggregate<Sum<PMRevenueBudget.curyAmount,
						Sum<PMRevenueBudget.qty,
						Sum<PMRevenueBudget.curyChangeOrderAmount,
						Sum<PMRevenueBudget.changeOrderQty,
						Sum<PMRevenueBudget.curyRevisedAmount,
						Sum<PMRevenueBudget.revisedQty>>>>>>>>(Base);

			PMRevenueBudget contractTotal = contractTotalSelect.Select();

			Dictionary<AmountBaseKey, AmountBaseTotals> proformaTotalsByTask = GetProformaTotalsToDate();
			Dictionary<AmountBaseKey, decimal> billedRetainage = Base.GetBilledRetainageToDate(Base.Document.Current.InvoiceDate);
			

			decimal retainageHeldTodateTotal = 0;
			decimal retainageEverHeldTodateTotal = 0;
			decimal completedTodateTotal = 0;
			foreach (AmountBaseTotals total in proformaTotalsByTask.Values)
			{
				retainageHeldTodateTotal += total.CuryRetainage;
				completedTodateTotal += total.CuryLineTotal;
			}
			retainageEverHeldTodateTotal = retainageHeldTodateTotal;
			foreach (decimal amount in billedRetainage.Values)
			{
				retainageHeldTodateTotal -= amount;
			}

			PMProforma previousProforma = PXSelectGroupBy<PMProforma,
				Where<PMProforma.projectID, Equal<Current<PMProforma.projectID>>,
				And<PMProforma.refNbr, Less<Current<PMProforma.refNbr>>,
				And<PMProforma.corrected, NotEqual<True>>>>,
				Aggregate<Max<PMProforma.refNbr>>>.Select(Base);

			DateTime? cutoffDate = null;
			decimal? line6FromPriorCertificate = 0;
			if (previousProforma != null)
			{
				cutoffDate = previousProforma.InvoiceDate;

				var completedToDateTotalSelect = new PXSelectGroupBy<PMProformaLine,
				Where<PMProformaLine.refNbr, LessEqual<Required<PMProforma.refNbr>>,
				And<PMProformaLine.projectID, Equal<Required<PMProforma.projectID>>,
				And<PMProformaLine.type, Equal<PMProformaLineType.progressive>,
				And<PMProformaLine.corrected, NotEqual<True>>>>>,
				Aggregate<Sum<PMProformaLine.curyLineTotal, Sum<PMProformaLine.curyRetainage>>>>(Base);

				PMProformaLine completedToDatePreviousTotal = completedToDateTotalSelect.Select(previousProforma.RefNbr, previousProforma.ProjectID);
				if (completedToDatePreviousTotal != null)
				{
					Dictionary<AmountBaseKey, decimal> billedRetainagePrevious = Base.GetBilledRetainageToDate(cutoffDate);
					decimal retainageHeldTodatePreviousTotal = completedToDatePreviousTotal.CuryRetainage.GetValueOrDefault();
					foreach (decimal amount in billedRetainagePrevious.Values)
					{
						retainageHeldTodatePreviousTotal -= amount;
					}
					line6FromPriorCertificate = completedToDatePreviousTotal.CuryLineTotal.GetValueOrDefault() - retainageHeldTodatePreviousTotal;
				}
			}

			var changeOrders = new PXSelectReadonly<PMChangeOrder,
				Where<PMChangeOrder.projectID, Equal<Current<PMProforma.projectID>>,
				And<PMChangeOrder.approved, Equal<True>,
				And<PMChangeOrder.completionDate, LessEqual<Current<PMProforma.invoiceDate>>>>>>(Base);

			decimal additions = 0;
			decimal deduction = 0;
			decimal additionsPrevious = 0;
			decimal deductionPrevious = 0;
			foreach (PMChangeOrder order in changeOrders.Select())
			{
				if (cutoffDate != null && order.CompletionDate.Value.Date <= cutoffDate.Value.Date)
				{
					if (order.RevenueTotal.GetValueOrDefault() < 0)
					{
						deductionPrevious += -1 * order.RevenueTotal.GetValueOrDefault();
					}
					else
					{
						additionsPrevious += order.RevenueTotal.GetValueOrDefault();
					}
				}
				else
				{
					if (order.RevenueTotal.GetValueOrDefault() < 0)
					{
						deduction += -1 * order.RevenueTotal.GetValueOrDefault();
					}
					else
					{
						additions += order.RevenueTotal.GetValueOrDefault();
					}
				}
			}

			Dictionary<AmountBaseKey, ChangeOrderTotals> coRevenue = new Dictionary<AmountBaseKey, ChangeOrderTotals>();

			var changeOrderRevenue = new PXSelectJoinGroupBy<PMChangeOrderRevenueBudget,
				InnerJoin<PMChangeOrder, On<PMChangeOrder.refNbr, Equal<PMChangeOrderRevenueBudget.refNbr>>>,
				Where<PMChangeOrder.projectID, Equal<Current<PMProforma.projectID>>,
				And<PMChangeOrder.approved, Equal<True>,
				And<PMChangeOrder.completionDate, LessEqual<Current<PMProforma.invoiceDate>>,
				And<PMChangeOrderRevenueBudget.type, Equal<GL.AccountType.income>>>>>,
				Aggregate<GroupBy<PMChangeOrderRevenueBudget.projectID,
				GroupBy<PMChangeOrderRevenueBudget.projectTaskID,
				GroupBy<PMChangeOrderRevenueBudget.inventoryID,
				GroupBy<PMChangeOrderRevenueBudget.costCodeID,
				GroupBy<PMChangeOrderRevenueBudget.accountGroupID,
				Sum<PMChangeOrderRevenueBudget.amount,
				Sum<PMChangeOrderRevenueBudget.qty>>>>>>>>>(Base);

			foreach (PMChangeOrderRevenueBudget revenueChange in changeOrderRevenue.Select())
			{
				int costCodeID = revenueChange.CostCodeID.Value;
				int inventoryID = revenueChange.InventoryID.Value;
				int accountGroupID = revenueChange.AccountGroupID.Value;

				if (Base.Project.Current.BudgetLevel == BudgetLevels.Task)
				{
					costCodeID = CostCodeAttribute.GetDefaultCostCode();
					inventoryID = PMInventorySelectorAttribute.EmptyInventoryID;
				}

				AmountBaseKey key = new AmountBaseKey(revenueChange.ProjectTaskID.Value, costCodeID, inventoryID, accountGroupID);

				ChangeOrderTotals coTotals;
				if (!coRevenue.TryGetValue(key, out coTotals))
				{
					coTotals = new ChangeOrderTotals() { Key = key, Amount = revenueChange.Amount.GetValueOrDefault(), Quantity = revenueChange.Qty.GetValueOrDefault() };
					coRevenue.Add(key, coTotals);
				}
				else
				{					
					coTotals.Amount += revenueChange.Amount.GetValueOrDefault();
					coTotals.Quantity += revenueChange.Qty.GetValueOrDefault();
					coRevenue[key] = coTotals;
				}
			}

			PMProformaInfo proformaInfo = new PMProformaInfo();
			proformaInfo.RefNbr = Base.Document.Current.RefNbr;
			proformaInfo.OriginalContractTotal = contractTotal.CuryAmount;
			proformaInfo.ChangeOrderTotal = additionsPrevious + additions - deductionPrevious - deduction;
			proformaInfo.RevisedContractTotal = proformaInfo.OriginalContractTotal + proformaInfo.ChangeOrderTotal;
			proformaInfo.PriorProformaLineTotal = line6FromPriorCertificate;
			proformaInfo.CompletedToDateLineTotal = completedTodateTotal;
			proformaInfo.RetainageHeldToDateTotal = retainageHeldTodateTotal;
			proformaInfo.ChangeOrderAdditions = additions;
			proformaInfo.ChangeOrderAdditionsPrevious = additionsPrevious;
			proformaInfo.ChangeOrderDeduction = deduction;
			proformaInfo.ChangeOrderDeductionPrevious = deductionPrevious;
			proformaInfo.ProgressBillingBase = ProgressBillingBase.Quantity;

			proformaInfo = CustomizeProformaInfo(proformaInfo);

			PXReportResultset resultset = new PXReportResultset(typeof(PMProformaProgressLine), typeof(PMProformaLineInfo), typeof(PMRevenueBudget), typeof(PMTask), typeof(PMProforma), typeof(PMProformaInfo), typeof(PMProject), typeof(Customer), typeof(PMAddress), typeof(PMContact), typeof(GL.Branch), typeof(CompanyBAccount), typeof(PMSiteAddress));

			PMAddress address = PXSelectReadonly<PMAddress, Where<PMAddress.addressID, Equal<Required<PMAddress.addressID>>>>.Select(Base, Base.Document.Current.BillAddressID);
			PMSiteAddress projectAddress = PXSelectReadonly<PMSiteAddress, Where<PMSiteAddress.addressID, Equal<Required<PMSiteAddress.addressID>>>>.Select(Base, Base.Project.Current.SiteAddressID);
			PMContact contact = PXSelectReadonly<PMContact, Where<PMContact.contactID, Equal<Required<PMContact.contactID>>>>.Select(Base, Base.Document.Current.BillContactID);
			PX.Objects.GL.Branch branch = PXSelectReadonly<PX.Objects.GL.Branch, Where<PX.Objects.GL.Branch.branchID, Equal<Required<PX.Objects.GL.Branch.branchID>>>>.Select(Base, Base.Document.Current.BranchID);
			CompanyBAccount company = PXSelectReadonly<CompanyBAccount, Where<CompanyBAccount.organizationID, Equal<Required<CompanyBAccount.organizationID>>>>.Select(Base, branch.OrganizationID);

			var selectTasks = new PXSelectReadonly<PMTask, Where<PMTask.projectID, Equal<Current<PMProforma.projectID>>>>(Base);
			Dictionary<int, PMTask> tasksLookup = new Dictionary<int, PMTask>();
			foreach (PMTask task in selectTasks.Select())
			{
				tasksLookup.Add(task.TaskID.Value, task);
			}

			decimal roundingOverflow = 0;
			PMProformaProgressLine lineWithLargestRetainage = null;

			foreach (PXResult<PMProformaProgressLine, PMRevenueBudget> res in Base.ProgressiveLines.Select())
			{
				PMProformaProgressLine line = PXCache<PMProformaProgressLine>.CreateCopy((PMProformaProgressLine)res);
				PMRevenueBudget budget = (PMRevenueBudget)res;
				PMProformaLineInfo lineInfo = new PMProformaLineInfo();

				// Calc Completed % as Amount if any line with Amount
				if(line.ProgressBillingBase == ProgressBillingBase.Amount)
				{
					proformaInfo.ProgressBillingBase = ProgressBillingBase.Amount;
				}

				lineInfo.RefNbr = line.RefNbr;
				lineInfo.LineNbr = line.LineNbr;
				lineInfo.ChangeOrderAmountToDate = 0m;
				lineInfo.ChangeOrderQtyToDate = 0m;

				decimal totalRetainedToDate = 0;
				decimal curyPreviouslyInvoiced = 0;
				decimal billedRetainageWithoutByLine = 0;
				decimal billedRetainageByLine = 0;
				decimal unitRate = 0;

				var totalQty = budget.Qty.GetValueOrDefault() + budget.ChangeOrderQty.GetValueOrDefault();
				if (totalQty != 0)
				{
					unitRate = (budget.CuryAmount.GetValueOrDefault() + budget.CuryChangeOrderAmount.GetValueOrDefault()) / totalQty;
				}

				AmountBaseKey key = new AmountBaseKey(line.TaskID.GetValueOrDefault(), line.CostCodeID.GetValueOrDefault(), line.InventoryID.GetValueOrDefault(), line.AccountGroupID.GetValueOrDefault());

				if (proformaTotalsByTask.TryGetValue(key, out AmountBaseTotals totals))
				{
					curyPreviouslyInvoiced = totals.CuryLineTotal - line.CuryLineTotal.GetValueOrDefault();
					totalRetainedToDate = totals.CuryRetainage;

					if (retainageEverHeldTodateTotal != 0 && billedRetainage.ContainsKey(Base.PayByLineOffKey))
					{
						decimal ratio = totals.CuryRetainage / retainageEverHeldTodateTotal;
						decimal billedRetainageWithoutByLineRaw = billedRetainage[Base.PayByLineOffKey] * ratio;
						billedRetainageWithoutByLine = Math.Round(billedRetainageWithoutByLineRaw, 2);
						decimal roundingDiff = billedRetainageWithoutByLineRaw - billedRetainageWithoutByLine;
						roundingOverflow += roundingDiff;
						Debug.Print("TaskID:{0} Ratio:{1} billedRetainageWithoutByLineRaw:{2}", key.TaskID, ratio, billedRetainageWithoutByLineRaw);
					}
				}

				key = new AmountBaseKey(key.TaskID, key.CostCodeID, key.InventoryID, key.AccountGroupID);
				billedRetainage.TryGetValue(key, out billedRetainageByLine);

				if (line.ProgressBillingBase == ProgressBillingBase.Amount)
				{
					line.CuryPreviouslyInvoiced = curyPreviouslyInvoiced;

					if (unitRate != 0)
					{
						line.Qty = line.CuryLineTotal.GetValueOrDefault() / unitRate;
						lineInfo.PreviousQty = curyPreviouslyInvoiced / unitRate;
					}
					else
						lineInfo.PreviousQty = 0;
				}
				else if(line.ProgressBillingBase == ProgressBillingBase.Quantity)
				{
					var proformaQbTotals = Base.TotalsCounter.GetQuantityBaseTotals(Base, Base.Document.Current.RefNbr, line);
					lineInfo.PreviousQty = proformaQbTotals.QuantityTotal;
					line.CuryPreviouslyInvoiced = proformaQbTotals.CuryLineTotal;

					decimal qtyToInvoice = 0.0m;
					decimal result = 0.0m;

					if (budget.RevisedQty.GetValueOrDefault() != 0 &&
						INUnitAttribute.TryConvertGlobalUnits(Base, line.UOM, budget.UOM, proformaQbTotals.QuantityTotal + line.Qty.GetValueOrDefault(), INPrecision.QUANTITY, out qtyToInvoice))
					{
						result = Math.Round(qtyToInvoice / budget.RevisedQty.Value, PMProformaProgressLine.completedPct.Precision);
					}

					lineInfo.QuantityBaseCompleterdPct = result;
				}


				int costCodeID = key.CostCodeID;
				int inventoryID = key.InventoryID;
				int accountGroupID = key.AccountGroupID;

				if (Base.Project.Current.BudgetLevel == BudgetLevels.Task)
				{
					costCodeID = CostCodeAttribute.GetDefaultCostCode();
					inventoryID = PMInventorySelectorAttribute.EmptyInventoryID;
				}
				AmountBaseKey coKey = new AmountBaseKey(key.TaskID, costCodeID, inventoryID, accountGroupID);
				ChangeOrderTotals coTotal;
				if (coRevenue.TryGetValue(coKey, out coTotal))
				{
					lineInfo.ChangeOrderAmountToDate = coTotal.Amount;
					lineInfo.ChangeOrderQtyToDate = coTotal.Quantity;
				}

				lineInfo = CustomizeProformaLineInfo(lineInfo);
							
				if (Base.Project.Current.RetainageMode == RetainageModes.Contract)
				{
					line.CuryRetainage = line.CuryAllocatedRetainedAmount;
				}
				else
				{
					//Calculate Held Retainage to date for a line and store in CuryRetainage field:
					line.CuryRetainage = totalRetainedToDate - (billedRetainageWithoutByLine + billedRetainageByLine);
				}

				if (lineWithLargestRetainage == null)
				{
					lineWithLargestRetainage = line;
				}
				else if (line.CuryRetainage > lineWithLargestRetainage.CuryRetainage)
				{
					lineWithLargestRetainage = line;
				}

				resultset.Add(line, lineInfo, budget, tasksLookup[line.TaskID.Value], Base.Document.Current, proformaInfo, Base.Project.Current, Base.Customer.Current, address, contact, branch, company, projectAddress);
			}
			Debug.Print("Rounding overflow total: {0}", roundingOverflow);
			if (lineWithLargestRetainage != null)
				lineWithLargestRetainage.CuryRetainage += roundingOverflow;

			if (GroupByTask())
			{
				resultset = GroupResultsetByTask(resultset);
			}
						
			return resultset;
		}

		public virtual PXReportResultset GroupResultsetByTask(PXReportResultset input)
		{
			PXReportResultset output = new PXReportResultset(typeof(PMProformaProgressLine), typeof(PMProformaLineInfo), typeof(PMRevenueBudget), typeof(PMTask), typeof(PMProforma), typeof(PMProformaInfo), typeof(PMProject), typeof(Customer), typeof(PMAddress), typeof(PMContact), typeof(GL.Branch), typeof(CompanyBAccount), typeof(PMSiteAddress));

			var byTask = new Dictionary<int, object[]>();
			foreach (object[] record in input)
			{
				PMProformaProgressLine line = (PMProformaProgressLine)record[0];
				PMProformaLineInfo info = (PMProformaLineInfo)record[1];
				PMRevenueBudget budget = (PMRevenueBudget)record[2];

				object[] existing;
				if (byTask.TryGetValue(line.TaskID.Value, out existing))
				{
					PMProformaProgressLine summaryLine = (PMProformaProgressLine)existing[0];
					PMProformaLineInfo summaryInfo = (PMProformaLineInfo)existing[1];
					PMRevenueBudget summaryBudget = (PMRevenueBudget)existing[2];

					BudgetAddToSummary(summaryBudget, budget);
					LineAddToSummary(summaryLine, line);
					InfoAddToSummary(summaryInfo, info);

					if(line.ProgressBillingBase == ProgressBillingBase.Quantity)
					{
						decimal revisedQty = summaryBudget.Qty.GetValueOrDefault() + summaryInfo.ChangeOrderQtyToDate.GetValueOrDefault();
						if(revisedQty != 0.0m && INUnitAttribute.TryConvertGlobalUnits(Base, summaryLine.UOM, summaryBudget.UOM, summaryInfo.PreviousQty.GetValueOrDefault() + summaryLine.Qty.GetValueOrDefault(), INPrecision.QUANTITY, out decimal qtyToInvoice))
						{
							summaryInfo.QuantityBaseCompleterdPct = Math.Round(qtyToInvoice / revisedQty, PMProformaProgressLine.completedPct.Precision);
						}
					}
				}
				else
				{
					output.Add(record);
					byTask.Add(line.TaskID.Value, record);
				}
			}

			return output;
		}

        public virtual bool GroupByTask()
		{
			return Base.Project.Current.AIALevel == CT.Contract.aIALevel.Summary;
		}

		public virtual void BudgetAddToSummary(PMRevenueBudget summary, PMRevenueBudget record)
		{
			summary.CuryAmount = summary.CuryAmount.GetValueOrDefault() + record.CuryAmount.GetValueOrDefault();
			summary.Qty = summary.Qty.GetValueOrDefault() + record.Qty.GetValueOrDefault();
		}

		public virtual void InfoAddToSummary(PMProformaLineInfo summary, PMProformaLineInfo record)
		{
			summary.ChangeOrderAmountToDate = summary.ChangeOrderAmountToDate.GetValueOrDefault() + record.ChangeOrderAmountToDate.GetValueOrDefault();
			summary.ChangeOrderQtyToDate = summary.ChangeOrderQtyToDate.GetValueOrDefault() + record.ChangeOrderQtyToDate.GetValueOrDefault();
			summary.PreviousQty = summary.PreviousQty.GetValueOrDefault() + record.PreviousQty.GetValueOrDefault();
		}

		public virtual void LineAddToSummary(PMProformaProgressLine summary, PMProformaProgressLine record)
		{
			summary.CuryAmount = summary.CuryAmount.GetValueOrDefault() + record.CuryAmount.GetValueOrDefault();
			summary.Amount = summary.Amount.GetValueOrDefault() + record.Amount.GetValueOrDefault();
			summary.CuryMaterialStoredAmount = summary.CuryMaterialStoredAmount.GetValueOrDefault() + record.CuryMaterialStoredAmount.GetValueOrDefault();
			summary.MaterialStoredAmount = summary.MaterialStoredAmount.GetValueOrDefault() + record.MaterialStoredAmount.GetValueOrDefault();
			summary.CuryRetainage = summary.CuryRetainage.GetValueOrDefault() + record.CuryRetainage.GetValueOrDefault();
			summary.Retainage = summary.Retainage.GetValueOrDefault() + record.Retainage.GetValueOrDefault();
			summary.CuryLineTotal = summary.CuryLineTotal.GetValueOrDefault() + record.CuryLineTotal.GetValueOrDefault();
			summary.LineTotal = summary.LineTotal.GetValueOrDefault() + record.LineTotal.GetValueOrDefault();
			summary.CuryPreviouslyInvoiced = summary.CuryPreviouslyInvoiced.GetValueOrDefault() + record.CuryPreviouslyInvoiced.GetValueOrDefault();
			summary.PreviouslyInvoiced = summary.PreviouslyInvoiced.GetValueOrDefault() + record.PreviouslyInvoiced.GetValueOrDefault();
			summary.Qty = summary.Qty.GetValueOrDefault() + record.Qty.GetValueOrDefault();
		}

		public virtual PMProformaInfo CustomizeProformaInfo(PMProformaInfo data)
		{
			return data;
		}

		public virtual PMProformaLineInfo CustomizeProformaLineInfo(PMProformaLineInfo data)
		{
			return data;
		}

		public virtual Dictionary<AmountBaseKey, AmountBaseTotals> GetProformaTotalsToDate()
		{
			Dictionary<AmountBaseKey, AmountBaseTotals> totalsToDate = new Dictionary<AmountBaseKey, AmountBaseTotals>();

			var totalsToDateSelect = new PXSelectJoinGroupBy<PMProformaLine,
			InnerJoin<PMProforma, On<PMProformaLine.refNbr, Equal<PMProforma.refNbr>,
				And<PMProformaLine.revisionID, Equal<PMProforma.revisionID>,
				And<PMProforma.curyID, Equal<Current<PMProforma.curyID>>>>>>,
			Where<PMProformaLine.refNbr, LessEqual<Current<PMProforma.refNbr>>,
			And<PMProformaLine.projectID, Equal<Current<PMProforma.projectID>>,
			And<PMProformaLine.type, Equal<PMProformaLineType.progressive>,
			And<PMProformaLine.corrected, NotEqual<True>>>>>,
			Aggregate<GroupBy<PMProformaLine.taskID,
			GroupBy<PMProformaLine.costCodeID,
			GroupBy<PMProformaLine.inventoryID,
			GroupBy<PMProformaLine.accountGroupID,
			Sum<PMProformaLine.curyRetainage,
			Sum<PMProformaLine.retainage,
			Sum<PMProformaLine.curyLineTotal,
			Sum<PMProformaLine.lineTotal,
			Sum<PMProformaLine.qty>>>>>>>>>>>(Base);

			foreach (PMProformaLine line in totalsToDateSelect.Select())
			{
				AmountBaseKey key = new AmountBaseKey(line.TaskID.GetValueOrDefault(), line.CostCodeID.GetValueOrDefault(), line.InventoryID.GetValueOrDefault(), line.AccountGroupID.GetValueOrDefault());

				AmountBaseTotals totals = new AmountBaseTotals();
				totals.Key = key;
				totals.CuryRetainage = line.CuryRetainage.GetValueOrDefault();
				totals.Retainage = line.Retainage.GetValueOrDefault();
				totals.CuryLineTotal = line.CuryLineTotal.GetValueOrDefault();
				totals.LineTotal = line.LineTotal.GetValueOrDefault();

				totalsToDate.Add(key, totals);
			}

			return totalsToDate;
		}

		public struct ChangeOrderTotals
		{
			public AmountBaseKey Key { get; set; }
			public decimal Amount { get; set; }
			public decimal Quantity { get; set; }
		}

		public PXAction<PMProforma> correct;
		[PXUIField(DisplayName = "Correct")]
		[PXProcessButton]
		public IEnumerable Correct(PXAdapter adapter)
		{
			if (Base.Document.Current != null)
			{
				ValidateAndRaiseExceptionCanCorrect(Base.Document.Current);

				if (Base.Project.Current.RetainageMode != RetainageModes.Normal || Base.Project.Current.SteppedRetainage == true)
				{
					string retainageMode = PX.Objects.PM.Messages.Retainage_Normal;
					switch (Base.Project.Current.RetainageMode)
					{
						case RetainageModes.Contract:
							retainageMode = PX.Objects.PM.Messages.Retainage_Contract;
							break;
						case RetainageModes.Line:
							retainageMode = PX.Objects.PM.Messages.Retainage_Line;
							break;
					}

					var subsequentProformas = new PXSelect<PMProforma, 
						Where<PMProforma.projectID, Equal<Current<PMProforma.projectID>>, 
						And<PMProforma.refNbr, Greater<Current<PMProforma.refNbr>>,
						And<PMProforma.corrected, Equal<False>>>>>(Base);

					string proformas = string.Empty;

					foreach(PMProforma proforma in subsequentProformas.Select())
                    {
						proformas += string.Format(" {0},", proforma.RefNbr);
                    }

					if (proformas != string.Empty)
                    {
						proformas = proformas.TrimEnd(',');

						string withStepsSuffix = Base.Project.Current.SteppedRetainage == true ? PX.Objects.PM.Messages.WithSteps : string.Empty;
						string msg = PXLocalizer.LocalizeFormat(PX.Objects.PM.Messages.CorrectRetainageWarning, retainageMode, withStepsSuffix, proformas);
						WebDialogResult res = Base.DocumentSettings.Ask(SharedMessages.CorrectProformaInvoice, msg, MessageButtons.OKCancel);
						if (res == WebDialogResult.Cancel)
						{
							return adapter.Get();
						}
					}					
				}


				List<Tuple<PMProformaProgressLine, string, Guid[]>> progressLines = new List<Tuple<PMProformaProgressLine, string, Guid[]>>();
				foreach (PMProformaProgressLine line in Base.ProgressiveLines.Select())
				{
					string note = PXNoteAttribute.GetNote(Base.ProgressiveLines.Cache, line);
					Guid[] files = PXNoteAttribute.GetFileNotes(Base.ProgressiveLines.Cache, line);

					progressLines.Add(new Tuple<PMProformaProgressLine, string, Guid[]>(CreateCorrectionProformaProgressiveLine(line), note, files));

					Base.SubtractFromTotalRetained(line);
					Base.SubtractPerpaymentRemainder(line, -1);
					Base.ProgressiveLines.Cache.SetValue<PMProformaProgressLine.corrected>(line, true);
					Base.ProgressiveLines.Cache.MarkUpdated(line);
				}

				List<Tuple<PMProformaTransactLine, string, Guid[]>> transactionLines = new List<Tuple<PMProformaTransactLine, string, Guid[]>>();
				foreach (PMProformaTransactLine line in Base.TransactionLines.Select())
				{
					string note = PXNoteAttribute.GetNote(Base.TransactionLines.Cache, line);
					Guid[] files = PXNoteAttribute.GetFileNotes(Base.TransactionLines.Cache, line);

					transactionLines.Add(new Tuple<PMProformaTransactLine, string, Guid[]>(CreateCorrectionProformaTransactLine(line), note, files));

					Base.SubtractFromTotalRetained(line);
					Base.SubtractPerpaymentRemainder(line, -1);
					Base.TransactionLines.Cache.SetValue<PMProformaTransactLine.corrected>(line, true);
					Base.TransactionLines.Cache.MarkUpdated(line);
				}

				ProformaEntry target = PXGraph.CreateInstance<ProformaEntry>();
				target.Clear();
				ProformaAutoNumberAttribute.DisableAutonumbiring(target.Document.Cache);
				PXFieldVerifying suppress = (_, e) => e.Cancel = true;
				target.FieldVerifying.AddHandler<PMProforma.finPeriodID>(suppress);
				OpenPeriodAttribute.SetValidatePeriod<PMProforma.finPeriodID>(target.Document.Cache, null, PeriodValidation.Nothing);

				CorrectProforma(target, Base.Document.Current);

				foreach (Tuple<PMProformaProgressLine, string, Guid[]> res in progressLines)
				{
					var line = target.ProgressiveLines.Insert(res.Item1);

					if (res.Item2 != null)
						PXNoteAttribute.SetNote(target.ProgressiveLines.Cache, line, res.Item2);
					if (res.Item3 != null && res.Item3.Length > 0)
						PXNoteAttribute.SetFileNotes(target.ProgressiveLines.Cache, line, res.Item3);
				}

				foreach (Tuple<PMProformaTransactLine, string, Guid[]> res in transactionLines)
				{
					var line = target.TransactionLines.Insert(res.Item1);

					if (res.Item2 != null)
						PXNoteAttribute.SetNote(target.TransactionLines.Cache, line, res.Item2);
					if (res.Item3 != null && res.Item3.Length > 0)
						PXNoteAttribute.SetFileNotes(target.TransactionLines.Cache, line, res.Item3);
				}

				using (var ts = new PXTransactionScope())
				{
					Base.Save.Press();
					target.SelectTimeStamp();
					target.Save.Press();
					ts.Complete();
				}

				PXRedirectHelper.TryRedirect(target, PXRedirectHelper.WindowMode.Same);
			}

			return adapter.Get();
		}

		public virtual PMProforma CorrectProforma(ProformaEntry target, PMProforma doc)
		{
			PMProforma correction = CreateCorrectionProforma(doc);
			string docNote = PXNoteAttribute.GetNote(Base.Document.Cache, doc);
			Guid[] docFiles = PXNoteAttribute.GetFileNotes(Base.Document.Cache, doc);
			var reversingDoc = Base.GetReversingDocument(doc.ARInvoiceDocType, doc.ARInvoiceRefNbr);

			Base.Document.Cache.SetValue<PMProforma.corrected>(doc, true);
			Base.Document.Cache.SetValue<PMProforma.reversedARInvoiceDocType>(doc, reversingDoc?.DocType);
			Base.Document.Cache.SetValue<PMProforma.reversedARInvoiceRefNbr>(doc, reversingDoc?.RefNbr);
			Base.Document.Cache.MarkUpdated(doc);

			correction = target.Document.Insert(correction);
			if (docNote != null)
				PXNoteAttribute.SetNote(target.Document.Cache, correction, docNote);
			if (docFiles != null && docFiles.Length > 0)
				PXNoteAttribute.SetFileNotes(target.Document.Cache, correction, docFiles);

			var selectFutureProformas = new PXSelect<PMProforma,
				Where<PMProforma.projectID, Equal<Required<PMProforma.projectID>>,
				And<PMProforma.refNbr, Greater<Required<PMProforma.refNbr>>>>>(Base);

			foreach (PMProforma proforma in selectFutureProformas.Select(doc.ProjectID, doc.RefNbr))
			{
				Base.Document.Cache.SetValue<PMProforma.isAIAOutdated>(proforma, true);
				Base.Document.Cache.MarkUpdated(proforma);
			}

			return correction;
		}

		protected virtual void ValidateAndRaiseExceptionCanCorrect(PMProforma proforma)
		{
			if (Base.TransactionLines.View.SelectMultiBound(new object[] { proforma }).Count > 0)
			{
				throw new PXException(PX.Objects.PM.Messages.CannotCorrectContainsTM);
			}

			ValidateThereIsNoUnreleasedRetainageInvoices(proforma);
			ValidateThatThereAreNoPayments(proforma);
			ValidateThatInvoiceCanBeReverted(proforma);
		}

		protected virtual void ValidateThereIsNoUnreleasedRetainageInvoices(PMProforma proforma)
		{
			if (!string.IsNullOrEmpty(proforma.ARInvoiceRefNbr))
			{
				var selectUnreleasedRetainageInvoices = new PXSelect<ARInvoice, Where<ARInvoice.isRetainageDocument, Equal<True>,
					And<ARInvoice.origDocType, Equal<Current<PMProforma.aRInvoiceDocType>>,
					And<ARInvoice.origRefNbr, Equal<Current<PMProforma.aRInvoiceRefNbr>>,
					And<ARInvoice.released, Equal<False>>>>>>(Base);

				StringBuilder sb = new StringBuilder();
				foreach (ARInvoice doc in selectUnreleasedRetainageInvoices.Select())
				{
					sb.AppendFormat(" {0}.{1},", doc.DocType, doc.RefNbr);
				}

				string docList = sb.ToString();
				if (!string.IsNullOrEmpty(docList))
				{
					throw new PXException(PX.Objects.PM.Messages.CannotCorrectContainsUnreleasedRetainage, docList.TrimEnd(','));
				}
			}
		}

		protected virtual void ValidateThatThereAreNoPayments(PMProforma proforma)
		{
			var reversingDocType = Base.GetReversingDocType(proforma.ARInvoiceDocType);

			var selectAdjustments = new PXSelectGroupBy<ARAdjust,
					Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
					And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
					And<ARAdjust.adjgDocType, NotEqual<Required<ARAdjust.adjgDocType>>,
					And<ARAdjust.voided, Equal<False>>>>>,
					Aggregate<GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr>>>>(Base);

			StringBuilder sb = new StringBuilder();
			foreach (ARAdjust doc in selectAdjustments.Select(
				proforma.ARInvoiceDocType,
				proforma.ARInvoiceRefNbr,
				reversingDocType))
			{
				sb.AppendFormat(" {0}.{1},", doc.AdjgDocType, doc.AdjgRefNbr);
			}

			string docList = sb.ToString();
			if (!string.IsNullOrEmpty(docList))
			{
				throw new PXException(PX.Objects.PM.Messages.CannotCorrectContainsApplications, string.Format("{0}.{1}", Base.Document.Current.ARInvoiceDocType, Base.Document.Current.ARInvoiceRefNbr), docList.TrimEnd(','));
			}
		}

		protected virtual void ValidateThatInvoiceCanBeReverted(PMProforma proforma)
		{
			ARInvoice origDoc = ARInvoice.PK.Find(Base, proforma.ARInvoiceDocType, proforma.ARInvoiceRefNbr);

			if (origDoc?.RetainageApply != true
				|| origDoc.CuryRetainageTotal == origDoc.CuryRetainageUnreleasedAmt)
				return;

			if (Base.GetReversingDocument(origDoc.DocType, origDoc.RefNbr) != null)
				return;

			string originalInvoice = string.Format("{0}.{1}", Base.Document.Current.ARInvoiceDocType, Base.Document.Current.ARInvoiceRefNbr);

			var selectReleasedRetainageInvoices = new PXSelect<ARInvoice, Where<ARInvoice.isRetainageDocument, Equal<True>,
				And<ARInvoice.origDocType, Equal<Required<ARInvoice.origDocType>>,
				And<ARInvoice.origRefNbr, Equal<Required<ARInvoice.origRefNbr>>,
				And<ARInvoice.released, Equal<True>>>>>>(Base);

			StringBuilder sb = new StringBuilder();
			foreach (ARInvoice doc in selectReleasedRetainageInvoices.Select(proforma.ARInvoiceDocType, proforma.ARInvoiceRefNbr))
			{
				sb.AppendFormat(" {0}.{1},", doc.DocType, doc.RefNbr);
			}

			string docList = sb.ToString();

			throw new PXException(PX.Objects.PM.Messages.CannotCorrectContainsRetainageBalance, docList, originalInvoice);
		}

		public virtual PMProforma CreateCorrectionProforma(PMProforma original)
		{
			PMProforma proforma = (PMProforma)Base.Document.Cache.CreateCopy(original);
			proforma.RevisionID++;
			proforma.Status = null;
			proforma.Hold = null;
			proforma.Approved = null;
			proforma.Rejected = null;
			proforma.Released = null;
			proforma.Corrected = null;
			proforma.ARInvoiceDocType = null;
			proforma.ARInvoiceRefNbr = null;
			proforma.TransactionalTotal = null;
			proforma.CuryTransactionalTotal = null;
			proforma.ProgressiveTotal = null;
			proforma.CuryProgressiveTotal = null;
			proforma.RetainageDetailTotal = null;
			proforma.CuryRetainageDetailTotal = null;
			proforma.RetainageTaxTotal = null;
			proforma.CuryRetainageTaxTotal = null;
			proforma.TaxTotal = null;
			proforma.CuryTaxTotal = null;
			proforma.DocTotal = null;
			proforma.CuryDocTotal = null;
			proforma.CuryAllocatedRetainedTotal = null;
			proforma.AllocatedRetainedTotal = null;
			proforma.IsTaxValid = null;
			proforma.tstamp = null;
			proforma.NoteID = null;
			proforma.IsAIAOutdated = true;
			proforma.ReversedARInvoiceDocType = null;
			proforma.ReversedARInvoiceRefNbr = null;

			return proforma;
		}

		public virtual PMProformaTransactLine CreateCorrectionProformaTransactLine(PMProformaTransactLine original)
		{
			PMProformaTransactLine line = (PMProformaTransactLine)Base.TransactionLines.Cache.CreateCopy(original);
			line.Released = null;
			line.Corrected = null;
			line.ARInvoiceDocType = null;
			line.ARInvoiceRefNbr = null;
			line.ARInvoiceLineNbr = null;
			line.RevisionID = null;
			line.NoteID = null;
			line.tstamp = null;

			return line;
		}

		public virtual PMProformaProgressLine CreateCorrectionProformaProgressiveLine(PMProformaProgressLine original)
		{
			PMProformaProgressLine line = (PMProformaProgressLine)Base.ProgressiveLines.Cache.CreateCopy(original);
			line.Released = null;
			line.Corrected = null;
			line.ARInvoiceDocType = null;
			line.ARInvoiceRefNbr = null;
			line.ARInvoiceLineNbr = null;
			line.RevisionID = null;
			line.NoteID = null;
			line.tstamp = null;

			return line;
		}

		public virtual bool CanBeCorrected(PMProforma row)
		{
			if (row.Released != true
				|| row.Corrected == true
				|| row.IsMigratedRecord == true)
				return false;

			ARInvoice arDoc = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Current<PMProforma.aRInvoiceDocType>>,
				And<ARInvoice.refNbr, Equal<Current<PMProforma.aRInvoiceRefNbr>>>>>.Select(Base);

			return arDoc == null || arDoc.Released == true;
		}

		protected virtual void _(Events.RowSelected<PMProforma> e)
		{
			if (Base.SuppressRowSeleted)
				return;

			if (e.Row != null)
			{
				correct.SetEnabled(CanBeCorrected(e.Row));
				PXUIFieldAttribute.SetEnabled<PMProforma.projectNbr>(e.Cache, e.Row, e.Row.Hold == true);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProforma, PMProforma.projectNbr> e)
		{
			string val = (string)e.NewValue;

			if (string.IsNullOrEmpty(val) || val.Equals(PX.Objects.PM.Messages.NotAvailable, StringComparison.InvariantCultureIgnoreCase))
				return;

			var selectDuplicate = new PXSelect<PMProforma, Where<PMProforma.projectID, Equal<Current<PMProforma.projectID>>,
				And<PMProforma.projectNbr, Equal<Required<PMProforma.projectNbr>>,
				And<PMProforma.corrected, NotEqual<True>>>>>(Base);

			if (e.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted)
			{
				selectDuplicate.WhereAnd<Where<PMProforma.refNbr, NotEqual<Current<PMProforma.refNbr>>>>();
			}

			PMProforma duplicate = selectDuplicate.Select(e.NewValue);

			if (duplicate != null)
			{
				throw new PXSetPropertyException(PX.Objects.PM.Messages.DuplicateProformaNumber, duplicate.RefNbr);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProforma, PMProforma.projectNbr> e)
		{
			PMProject project = ProjectProperties.Select();
			if (project != null && e.Row.ProjectNbr != PX.Objects.PM.Messages.NotAvailable)
			{
				if (string.IsNullOrEmpty(project.LastProformaNumber) == true ||
					(string.IsNullOrEmpty(e.Row.ProjectNbr) == false &&
						NumberHelper.GetTextPrefix(e.Row.ProjectNbr) == NumberHelper.GetTextPrefix(project.LastProformaNumber) &&
						NumberHelper.GetNumericValue(e.Row.ProjectNbr) >= NumberHelper.GetNumericValue(project.LastProformaNumber)))
				{
					project.LastProformaNumber = e.Row.ProjectNbr;
					ProjectProperties.Update(project);
				}
			}
		}

		protected virtual void _(Events.RowDeleted<PMProforma> e)
		{
			var select = new PXSelect<PMProformaRevision,
				Where<PMProformaRevision.refNbr, Equal<Required<PMProforma.refNbr>>,
				And<PMProformaRevision.revisionID, NotEqual<Required<PMProforma.revisionID>>>>,
				OrderBy<Desc<PMProformaRevision.revisionID>>>(Base);

			PMProformaRevision lastRevision = select.SelectWindowed(0, 1, e.Row.RefNbr, e.Row.RevisionID);

			PMProject project = ProjectProperties.Select();
			if (project != null && project.LastProformaNumber == e.Row.ProjectNbr && string.IsNullOrEmpty(project.LastProformaNumber) == false && lastRevision == null)
			{
				project.LastProformaNumber = NumberHelper.DecreaseNumber(project.LastProformaNumber, 1);
				ProjectProperties.Update(project);
			}
		}
	}

	public class ProformaEntryExt_Workflow : PXGraphExtension<ProformaEntry_Workflow, ProformaEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.construction>();
		}

		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ProformaEntry, PMProforma>());

		protected static void Configure(WorkflowContext<ProformaEntry, PMProforma> context)
		{
			context.UpdateScreenConfigurationFor(screen =>
			{
				return screen
					.WithActions(actions =>
					{
						actions.Add<ProformaEntryExt>(g => g.correct,
							c => c.InFolder(context.Categories.Get(ToolbarCategory.ActionCategoryNames.Corrections)));
					});
			});
		}
	}
}
