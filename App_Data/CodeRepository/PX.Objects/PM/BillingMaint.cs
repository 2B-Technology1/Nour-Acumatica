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
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PM
{
	public class BillingMaint : PXGraph<BillingMaint, PMBilling>
	{
		// ReSharper disable InconsistentNaming
		[InjectDependency]
		private ICurrentUserInformationProvider _currentUserInformationProvider { get; set; }
		// ReSharper restore InconsistentNaming

		#region Metadata for PMFormulaEditor
		public PXSelect<PMTran> PMTranMetaData;
		public PXSelect<PMProject> PMProjectMetaData;
		public PXSelect<PMTask> PMTaskMetaData;
		public PXSelect<PMAccountGroup> PMAccountGroupMetaData;
		public PXSelect<PMBudget> PMBudget;
		public PXSelect<PX.Objects.EP.EPEmployee> EmployeesMetaData;
		public PXSelect<PX.Objects.AP.Vendor> VendorMetaData;
		public PXSelect<PX.Objects.AR.Customer> CustomerMetaData;
		public PXSelect<PX.Objects.IN.InventoryItem> InventoryItemMetaData;
		#endregion

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Billing Rule ID", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void _(Events.CacheAttached<PMBilling.billingID> e) { }

		public PXSelect<PMBilling> Billing;
		public PXSelect<PMBillingRule, Where<PMBillingRule.billingID, Equal<Current<PMBilling.billingID>>>> BillingRules;
		public PXSelect<PMBillingRule, Where<PMBillingRule.billingID, Equal<Current<PMBilling.billingID>>, And<PMBillingRule.stepID, Equal<Current<PMBillingRule.stepID>>>>> BillingRule;

		#region Event Handlers

		protected virtual void PMBillingRule_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as PMBillingRule;
			if (row != null)
			{
				PXUIFieldAttribute.SetVisible<PMBillingRule.subMask>(sender, e.Row, row.Type == PMBillingType.Transaction);
				PXUIFieldAttribute.SetVisible<PMBillingRule.subMaskBudget>(sender, e.Row, row.Type == PMBillingType.Budget);
				PXUIFieldAttribute.SetVisible<PMBillingRule.branchSourceBudget>(sender, e.Row, ShowBranchOptions() && row.Type == PMBillingType.Budget);
				PXUIFieldAttribute.SetVisible<PMBillingRule.accountID>(sender, e.Row, row.Type == PMBillingType.Transaction);
				PXUIFieldAttribute.SetVisible<PMBillingRule.accountGroupID>(sender, e.Row, row.Type == PMBillingType.Transaction);
				PXUIFieldAttribute.SetVisible<PMBillingRule.amountFormula>(sender, e.Row, row.Type == PMBillingType.Transaction);
				PXUIFieldAttribute.SetVisible<PMBillingRule.qtyFormula>(sender, e.Row, row.Type == PMBillingType.Transaction);
				PXUIFieldAttribute.SetVisible<PMBillingRule.rateTypeID>(sender, e.Row, row.Type == PMBillingType.Transaction);
				PXUIFieldAttribute.SetVisible<PMBillingRule.noRateOption>(sender, e.Row, row.Type == PMBillingType.Transaction);
				PXUIFieldAttribute.SetVisible<PMBillingRule.includeNonBillable>(sender, e.Row, row.Type == PMBillingType.Transaction);
				PXUIFieldAttribute.SetVisible<PMBillingRule.copyNotes>(sender, e.Row, row.Type == PMBillingType.Transaction);
				PXUIFieldAttribute.SetVisible<PMBillingRule.includeZeroAmountAndQty>(sender, e.Row, row.Type == PMBillingType.Transaction);
				PXUIFieldAttribute.SetVisible<PMBillingRule.includeZeroAmount>(sender, e.Row, row.Type == PMBillingType.Budget);
				PXUIFieldAttribute.SetVisible<PMBillingRule.groupByDate>(sender, e.Row, row.Type == PMBillingType.Transaction);
				PXUIFieldAttribute.SetVisible<PMBillingRule.groupByEmployee>(sender, e.Row, row.Type == PMBillingType.Transaction);
				PXUIFieldAttribute.SetVisible<PMBillingRule.groupByItem>(sender, e.Row, row.Type == PMBillingType.Transaction);
				PXUIFieldAttribute.SetVisible<PMBillingRule.groupByVendor>(sender, e.Row, row.Type == PMBillingType.Transaction);
				PXUIFieldAttribute.SetVisible<PMBillingRule.branchSource>(sender, e.Row, ShowBranchOptions() && row.Type == PMBillingType.Transaction);
			}
		}

		public static IDictionary<string, string> GetAccountSources(string billingType)
		{
			var valueCollection = new Dictionary<string, string>();
			bool isTransaction = billingType == PMBillingType.Transaction;

			if (isTransaction)
			{
				valueCollection.Add(PMAccountSource.None, Messages.GetLocal(Messages.AccountSource_SourceTransaction));
				valueCollection.Add(PMAccountSource.BillingRule, Messages.GetLocal(Messages.AccountSource_BillingRule));
			}
			else
			{
				valueCollection.Add(PMAccountSource.AccountGroup, Messages.GetLocal(Messages.AccountSource_AccountGroup));
			}

			valueCollection.Add(PMAccountSource.Project, Messages.GetLocal(Messages.AccountSource_Project));
			valueCollection.Add(PMAccountSource.Task, Messages.GetLocal(Messages.AccountSource_Task));
			valueCollection.Add(PMAccountSource.InventoryItem, Messages.GetLocal(Messages.AccountSource_InventoryItem));

			if (isTransaction)
			{
				valueCollection.Add(PMAccountSource.Customer, Messages.GetLocal(Messages.AccountSource_Customer));
				valueCollection.Add(PMAccountSource.Employee, Messages.GetLocal(Messages.AccountSource_Employee));
			}

			return valueCollection;
		}

		protected virtual void _(Events.RowUpdated<PMBillingRule> e)
		{
			var row = e.Row;

			if (row == null || row.IsActive != true || row.AccountGroupID == null)
			{
				return;
			}

			if (!DoesExistActiveBillingRuleWithTheSameAccountGroupId(row.AccountGroupID, row.StepID))
			{
				return;
			}

			PMAccountGroup selectedAccountGroup = GetAccountGroupByGroupId(row.AccountGroupID);

			if (selectedAccountGroup == null)
			{
				return;
			}

			BillingRules.Cache.RaiseExceptionHandling<PMBillingRule.accountGroupID>(
				e.Row,
				selectedAccountGroup.GroupCD,
				new PXSetPropertyException(
								Messages.PMBillingRuleDuplicateAccountGroup,
								PXErrorLevel.Error,
								selectedAccountGroup.GroupCD));
		}

		protected virtual void _(Events.RowPersisting<PMBilling> e)
		{
			var repeatingAccountGroup = GetRepeatingAccountGroup(out var row);

			if (repeatingAccountGroup != null)
			{
				BillingRules.Cache.RaiseExceptionHandling<PMBillingRule.accountGroupID>(
					row,
					repeatingAccountGroup.GroupCD,
					new PXSetPropertyException(
								Messages.PMBillingRuleDuplicateAccountGroup,
								repeatingAccountGroup.GroupCD));
			}
		}

		protected virtual void _(Events.FieldSelecting<PMBillingRule, PMBillingRule.accountSource> e)
		{
			if (e.Row != null)
			{
				IDictionary<string, string> accountSources = GetAccountSources(e.Row.Type);

				string[] allowedValues = accountSources.Keys.ToArray();
				string[] allowedLabels = accountSources.Values.ToArray();

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 1, false, typeof(PMBillingRule.accountSource).Name, true, 1, null, allowedValues, allowedLabels, true, allowedValues[0]);
			}
		}

		protected virtual void PMBillingRule_Type_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMBillingRule row = e.Row as PMBillingRule;
			if (row == null) return;

			if (row.Type == PMBillingType.Budget)
			{
				row.AccountGroupID = null;
				row.AccountSource = PMAccountSource.AccountGroup;
			}
			else
			{
				row.AccountSource = PMAccountSource.None;
			}

			row.AccountID = null;
			row.SubID = null;

		}

		protected virtual void PMBillingRule_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PMBillingRule row = e.Row as PMBillingRule;
			if (row == null) return;

			if (row.Type == PMBillingType.Transaction && row.AccountGroupID == null)
			{
				sender.RaiseExceptionHandling<PMBillingRule.accountGroupID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(PMBillingRule.accountGroupID)}]"));
			}

			if (row.SubMaskBudget != null && row.SubMaskBudget.Contains('B') && row.SubID == null)
			{
				sender.RaiseExceptionHandling<PMBillingRule.subID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(PMBillingRule.subID)}]"));
			}

			if (row.SubMask != null && row.SubMask.Contains('B') && row.SubID == null)
			{
				sender.RaiseExceptionHandling<PMBillingRule.subID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(PMBillingRule.subID)}]"));
			}

			if (row.SubMask == null && PXAccess.FeatureInstalled<FeaturesSet.subAccount>())
			{
				sender.RaiseExceptionHandling<PMBillingRule.subMask>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(PMBillingRule.subMask)}]"));
			}

			if (row.SubMaskBudget == null && PXAccess.FeatureInstalled<FeaturesSet.subAccount>())
			{
				sender.RaiseExceptionHandling<PMBillingRule.subMaskBudget>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(PMBillingRule.subMaskBudget)}]"));
			}

		}

		#endregion

		public virtual bool ShowBranchOptions()
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.branch>())
				return false;

			var ids = _currentUserInformationProvider.GetActiveBranches();

			return !(ids == null || ids.Count() <= 1);
		}

		#region Auxiliary methods
		protected virtual bool DoesExistActiveBillingRuleWithTheSameAccountGroupId(int? accountGroupId, int? exceptStepId)
		{
			if (!accountGroupId.HasValue || !exceptStepId.HasValue)
			{
				return false;
			}

			var billingRuleWithTheSameAccountGroup = BillingRules.Select()
							.Select(item => item.GetItem<PMBillingRule>())
							.FirstOrDefault(rule
							=> rule.IsActive == true
							&& rule.StepID != exceptStepId
							&& rule.AccountGroupID == accountGroupId);

			return billingRuleWithTheSameAccountGroup != null;
		}

		protected virtual PMAccountGroup GetRepeatingAccountGroup(out PMBillingRule ruleWithRepeatingAccountGroupId)
		{
			ruleWithRepeatingAccountGroupId = BillingRules.Select()
							.Select(item => item.GetItem<PMBillingRule>())
							.Where(rule => rule.IsActive == true && rule.AccountGroupID.HasValue)
							.GroupBy(rule => rule.AccountGroupID)
							.Where(group => group.Count() > 1)
							.Select(group => group.First())
							.FirstOrDefault();

			if (ruleWithRepeatingAccountGroupId == null)
			{
				return null;
			}

			return GetAccountGroupByGroupId(ruleWithRepeatingAccountGroupId.AccountGroupID);
		}

		protected virtual PMAccountGroup GetAccountGroupByGroupId(int? groupId)
		{
			if (!groupId.HasValue)
			{
				return null;
			}

			return SelectFrom<PMAccountGroup>
							.Where<PMAccountGroup.groupID.IsEqual<@P.AsInt>>
							.View.Select(this, groupId);
		}
		#endregion
	}
}
