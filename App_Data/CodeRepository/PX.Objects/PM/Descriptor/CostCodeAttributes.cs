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

using CommonServiceLocator;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	[PXDBInt]
	[PXInt]
	[PXUIField(DisplayName = "Cost Code", FieldClass = COSTCODE)]	
	public class CostCodeAttribute : PXEntityAttribute, IPXRowPersistingSubscriber, IPXFieldVerifyingSubscriber, IPXRowSelectedSubscriber
	{
		private CostCodeDimensionSelectorAttribute CostCodeSelector => (CostCodeDimensionSelectorAttribute)_Attributes[_SelAttrIndex];

		public const string COSTCODE = "COSTCODE";
		protected Type task;
		public bool AllowNullValue { get; set; }
		public bool SkipVerification { get; set; }
		public bool SkipVerificationForDefault { get; set; }
		public bool AllowNullValueIfReleased { get; set; }
		public Type ReleasedField { get; set; }

		public Type ProjectField { get; set; }

		public CostCodeAttribute() : this(null, null, null)
		{
		}

		public CostCodeAttribute(Type account, Type task) : this(account, task, null)
		{

		}

		public CostCodeAttribute(Type account, Type task, string budgetType) : this(account, task, budgetType, null) { }

		public CostCodeAttribute(Type account, Type task, string budgetType, Type accountGroup): this(account, task, budgetType, accountGroup, false) { }

		public CostCodeAttribute(Type account, Type task, string budgetType, Type accountGroup, bool disableProjectSpecific)
		{
			this.task = task;
			CostCodeDimensionSelectorAttribute select = new CostCodeDimensionSelectorAttribute(account, task, budgetType, accountGroup, disableProjectSpecific);

			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static bool UseCostCode()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.costCodes>();
		}

		public static int GetDefaultCostCode()
		{
			return DefaultCostCode.GetValueOrDefault();
		}

		public static int? DefaultCostCode
		{
			get
			{
				ICostCodeManager ccm = ServiceLocator.Current.GetInstance<ICostCodeManager>();
				return ccm.DefaultCostCodeID;
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (ProjectField != null)
			{
				sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), ProjectField.Name, OnProjectUpdated);
			}
		}

		protected virtual void OnProjectUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (ProjectField != null)
			{
				object projectID = sender.GetValue(e.Row, ProjectField.Name);
				if (projectID == null || ProjectDefaultAttribute.IsNonProject((int?)projectID))
				{
					sender.SetValueExt(e.Row, _FieldName, null);
				}
			}
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (task == null)
				return;

			if (AllowNullValue)
				return;

			if (!UseCostCode())
				return;

			int? taskID = (int?)sender.GetValue(e.Row, task.Name);
			if (taskID == null)
				return;

			if(AllowNullValueIfReleased)
			{
				if (ReleasedField != null)
				{
					bool? released = (bool?)sender.GetValue(e.Row, ReleasedField.Name);
					if (released == true)
					{
						return;
					}
				}
			}

			int? costCodeID = (int?)sender.GetValue(e.Row, FieldOrdinal);

			if (costCodeID == null)
			{
				if (sender.RaiseExceptionHandling(FieldName, e.Row, null, new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, $"[{FieldName}]")))
				{
					throw new PXRowPersistingException(FieldName, null, Data.ErrorMessages.FieldIsEmpty, FieldName);
				}

			}
		}

		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (task != null && !SkipVerification && e.NewValue != null && UseCostCode())
			{
				int costCodeID = (int)e.NewValue;
				if (SkipVerificationForDefault == true && costCodeID == GetDefaultCostCode())
					return;

				if (!IsValid(costCodeID))
				{
					string warning = CostCodeSelector.GetValidationWarning(sender, e.Row, costCodeID);
					sender.RaiseExceptionHandling(FieldName, e.Row, e.NewValue, new PXSetPropertyException(warning, PXErrorLevel.Warning));
				}
			}
		}

		protected virtual bool IsValid(int costCode)
		{
			if (!CostCodeSelector.IsCostCodeActive(costCode))
			{
				return false;
			}
			return CostCodeSelector.IsProjectSpecific(costCode);
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (SkipVerification)
				return;

			if (e.Row == null)
				return;

			if (!UseCostCode())
				return;

			int? costCode = (int?) sender.GetValue(e.Row, _FieldName);
			if (costCode == null)
				return;

			if (ReleasedField != null)
			{
				bool? released = (bool?)sender.GetValue(e.Row, ReleasedField.Name);
				if (released == true)
				{
					return;
				}
			}

			if (SkipVerificationForDefault == true && costCode == GetDefaultCostCode())
				return;

			if (!IsValid(costCode.Value))
			{
				string warning = CostCodeSelector.GetValidationWarning(sender, e.Row, costCode.Value);
				PXUIFieldAttribute.SetWarning(sender, e.Row, _FieldName, warning);
			}
			else if (PXUIFieldAttribute.GetErrorOnly(sender, e.Row, _FieldName) == null)
			{
				PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null);
			}
		}
	}

	[PXRestrictor(typeof(Where<PMCostCode.isActive, Equal<True>>), Messages.CostCodeInactiveWithFormat, typeof(PMCostCode.costCodeCD))]
	public class CostCodeDimensionSelectorAttribute : PXDimensionSelectorAttribute
	{
		private bool _disableProjectSpecific;
		private Type _taskID;
		private Type _accountID;
		private Type _accountGroupID;
		private PXGraph _graph;
		private Type _cacheType;

		public CostCodeDimensionSelectorAttribute(Type searchType) :
			base(CostCodeAttribute.COSTCODE, typeof(Search<PMCostCode.costCodeID>), typeof(PMCostCode.costCodeCD))
		{
			this.DescriptionField = typeof(PMCostCode.description);

			_disableProjectSpecific = true;
			var costCodeSelector = new PXSelectorAttribute(searchType);
			costCodeSelector.SubstituteKey = typeof(PMCostCode.costCodeCD);
			costCodeSelector.DescriptionField = typeof(PMCostCode.description);
			_Attributes[_Attributes.Count - 1] = costCodeSelector;
		}

		public CostCodeDimensionSelectorAttribute(Type account, Type task, string budgetType, Type accountGroup, bool disableProjectSpecific) :
			base(CostCodeAttribute.COSTCODE, typeof(Search<PMCostCode.costCodeID>), typeof(PMCostCode.costCodeCD))
		{
			_disableProjectSpecific = disableProjectSpecific;
			_taskID = task;
			_accountID = account;
			_accountGroupID = accountGroup;

			this.DescriptionField = typeof(PMCostCode.description);

			if (disableProjectSpecific == true)
			{
				var searchType = typeof(PMCostCode.costCodeID);
				var costCodeSelector = new PXSelectorAttribute(searchType);
				costCodeSelector.SubstituteKey = typeof(PMCostCode.costCodeCD);
				costCodeSelector.DescriptionField = typeof(PMCostCode.description);
				_Attributes[_Attributes.Count - 1] = costCodeSelector;
			}
			else
			{
				_Attributes[_Attributes.Count - 1] = new CostCodeSelectorAttribute()
				{
					TaskID = task,
					AccountID = account,
					BudgetType = budgetType,
					AccountGroup = accountGroup,
					DisableProjectSpecific = disableProjectSpecific
				};
			}
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			foreach (var attr in _Attributes.OfType<PXRestrictorAttribute>())
			{
				attr.FieldVerifying(sender, e);
			}

			base.FieldVerifying(sender, e);
		}

		public override void CacheAttached(PXCache sender)
		{
			_cacheType = sender.GetItemType();
			_graph = sender.Graph;
			
			base.CacheAttached(sender);
		}

		public bool IsProjectSpecific(int costCode)
		{
			if (_disableProjectSpecific == true)
			{
				return false;
			}
			else
			{
				var selector = (CostCodeSelectorAttribute)_Attributes[_Attributes.Count - 1];
				return selector.IsProjectSpecific(costCode);
			}
		}

		public bool IsCostCodeActive(int costCodeID)
		{
			PMCostCode costCode = PMCostCode.PK.Find(_graph, costCodeID);

			return costCode.IsActive == true;
		}

		public string GetValidationWarning(PXCache cache, object row, int costCode)
		{
			if (_disableProjectSpecific)
			{
				return GetCostCodeValidationWarning(cache, row, costCode);
			}
			else
			{
				CostCodeSelectorAttribute selector = (CostCodeSelectorAttribute)_Attributes[_Attributes.Count - 1];
				return selector.GetValidationWarning(cache, row, costCode);
			}
		}

		public string GetCostCodeValidationWarning(PXCache cache, object row, int costCodeID)
		{
			PMCostCode costCode = PMCostCode.PK.Find(_graph, costCodeID);

			string costCodeCD = FormatValueByMask(cache, row, costCode.CostCodeCD);
			if (costCode.IsActive != true)
			{
				return string.Format(PXLocalizer.Localize(Messages.CostCodeInactiveWithFormat), costCodeCD);
			}

			PMTask task = SelectFrom<PMTask>
				.Where<PMTask.taskID.IsEqual<@P.AsInt>>
				.View
				.Select(_graph, GetTaskID());

			int? accountGroupID = GetAccountGroupID();
			if (accountGroupID == null)
			{
				Account account = SelectFrom<Account>
				.Where<Account.accountID.IsEqual<@P.AsInt>>
				.View
				.Select(_graph, GetAccountID());

				if (account != null)
				{
					accountGroupID = account.AccountGroupID;
				}
			}

			PMAccountGroup accountGroup = SelectFrom<PMAccountGroup>
				.Where<PMAccountGroup.groupID.IsEqual<@P.AsInt>>
				.View
				.Select(_graph, accountGroupID);

			return string.Format(Messages.CostCodeNotInBudget, costCodeCD, task.TaskCD.Trim(), accountGroup?.GroupCD.Trim());
		}

		private string FormatValueByMask(PXCache cache, object row, string value)
		{
			var fieldState = (PXSegmentedState)cache.GetStateExt(row, _FieldName);
			var fieldValue = PX.Common.Mask.Format(fieldState.InputMask, value);
			return fieldValue;
		}

		public static string FormatValueByMask<T>(PXCache cache, object row, string value)
			where T : IBqlField
		{
			var state = cache.GetStateExt<T>(row) as PXSegmentedState;

			if(state != null)
			{
				return PX.Common.Mask.Format(state.InputMask, value);
			}

			return value;
		}

		protected virtual int? GetTaskID()
		{
			if (_taskID == null)
				return null;

			return GetIDByFieldName(_taskID.Name);
		}

		protected virtual int? GetAccountID()
		{
			if (_accountID == null)
				return null;

			return GetIDByFieldName(_accountID.Name);
		}

		protected virtual int? GetAccountGroupID()
		{
			if (_accountGroupID == null)
				return null;

			return GetIDByFieldName(_accountGroupID.Name);
		}

		protected virtual int? GetIDByFieldName(string fieldName)
		{
			object current = null;
			if (PXView.Currents != null && PXView.Currents.Length > 0)
			{
				current = PXView.Currents[0];
			}
			else
			{
				current = _graph.Caches[_cacheType].Current;
			}

			return (int?)_graph.Caches[_cacheType].GetValue(current, fieldName);
		}
	}

	public class CostCodeSelectorAttribute : PXCustomSelectorAttribute
	{
        public string BudgetType { get; set; }
		public Type TaskID { get; set; }
		public Type AccountID { get; set; }
		public Type AccountGroup { get; set; }
		public bool DisableProjectSpecific { get; set; }

		public CostCodeSelectorAttribute() : base(typeof(PMCostCode.costCodeID))
		{
			this.SubstituteKey = typeof(PMCostCode.costCodeCD);
			this.DescriptionField = typeof(PMCostCode.description);
			this.Filterable = true;
			this.FilterEntity = typeof(PMCostCode);
		}

		protected virtual IEnumerable GetRecords()
		{
			var query = new PXSelect<PMCostCode>(_Graph);
			
			PXResultset<PMCostCode> resultset = query.Select();
			Dictionary<int, PMCostCode> list = new Dictionary<int, PMCostCode>(resultset.Count);
			foreach (PMCostCode record in resultset)
			{
				record.IsProjectOverride = false;
				if (!list.ContainsKey(record.CostCodeID.Value) && record.IsActive == true)
				{
					list.Add(record.CostCodeID.Value, record);
				}
			}

			if (!DisableProjectSpecific)
			{
				foreach (PMBudgetedCostCode budget in GetProjectSpecificRecords().Values)
				{
					PMCostCode found = null;
					if (list.TryGetValue(budget.CostCodeID.Value, out found))
					{
						PMCostCode record = new PMCostCode();
						record.CostCodeID = found.CostCodeID;
						record.CostCodeCD = found.CostCodeCD;
						record.NoteID = found.NoteID;
						record.IsDefault = false;
						record.Description = budget.Description;
						record.IsProjectOverride = true;

						list[budget.CostCodeID.Value] = record;

					}
				}
			}

			return list.Values;
		}

		public override void SubstituteKeyFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			PMCostCode costCode = null;
			if (e.NewValue != null)
			{
				if (e.NewValue.GetType() == typeof(string))
                {
					costCode = PMCostCode.UK.Find(sender.Graph, (string) e.NewValue);
					if (costCode != null)
					{
						e.NewValue = costCode.CostCodeID;
						e.Cancel = true;
					}
				}
				else if (e.NewValue.GetType() == typeof(int))
				{
					costCode = PMCostCode.PK.Find(sender.Graph, (int?)e.NewValue);
				}
				if (costCode == null)
				{
					var fieldName = PXUIFieldAttribute.GetDisplayName(sender, _FieldName);
					string fieldValue = FormatValueByMask(sender, e.Row, (string)e.NewValue);

					throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ValueDoesntExist, fieldName, fieldValue));
				}
			}
		}

		private string FormatValueByMask(PXCache cache, object row, string value)
		{
			var fieldState = (PXSegmentedState)cache.GetStateExt(row, _FieldName);
			var fieldValue = PX.Common.Mask.Format(fieldState.InputMask, value);
			return fieldValue;
		}

		public override void SubstituteKeyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			object value = e.ReturnValue;
			e.ReturnValue = null;
			base.FieldSelecting(sender, e);

			PMCostCode costCode = PMCostCode.PK.Find(sender.Graph, (int?)value);
			if (costCode != null)
			{
				e.ReturnValue = costCode.CostCodeCD;
			}
			else
			{
				if (e.Row != null)
					e.ReturnValue = null;
			}
		}

		protected virtual int? GetTaskID()
		{
			if (TaskID == null)
				return null;

			return GetIDByFieldName(TaskID.Name);
		}

		protected virtual int? GetAccountID()
		{
			if (AccountID == null)
				return null;

			return GetIDByFieldName(AccountID.Name);
		}

		protected virtual int? GetAccountGroupID()
		{
			if (AccountGroup == null)
				return null;

			return GetIDByFieldName(AccountGroup.Name);
		}

		protected virtual int? GetIDByFieldName(string fieldName)
		{
			object current = null;
			if (PXView.Currents != null && PXView.Currents.Length > 0)
			{
				current = PXView.Currents[0];
			}
			else
			{
				current = _Graph.Caches[_CacheType].Current;
			}

			return (int?)_Graph.Caches[_CacheType].GetValue(current, fieldName);
		}
				
		protected virtual Dictionary<int?, PMBudgetedCostCode> GetProjectSpecificRecords()
		{
			int? taskID = GetTaskID();

			if (taskID == null)
				return new Dictionary<int?, PMBudgetedCostCode>();

			PXResultset<PMBudgetedCostCode> resultset = null;
			if (AccountID != null)
			{
				int? accountID = GetAccountID();
								
				if (accountID != null)
				{
					var select = new PXSelectJoin<PMBudgetedCostCode,
						InnerJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<PMBudgetedCostCode.accountGroupID>>,
						InnerJoin<Account, On<Account.accountGroupID, Equal<PMAccountGroup.groupID>>>>,
						Where<PMBudgetedCostCode.projectTaskID, Equal<Required<PMBudgetedCostCode.projectTaskID>>,
						And<Account.accountID, Equal<Required<Account.accountID>>>>>(_Graph);

					resultset = select.Select(taskID, accountID);
				}
			}
			else if (AccountGroup != null)
			{
				int? accountGroupID = GetAccountGroupID();

				if (accountGroupID != null)
				{
					var select = new PXSelect<PMBudgetedCostCode,
						Where<PMBudgetedCostCode.projectTaskID, Equal<Required<PMBudgetedCostCode.projectTaskID>>,
						And<PMBudgetedCostCode.accountGroupID, Equal<Required<PMBudgetedCostCode.accountGroupID>>>>>(_Graph);

					resultset = select.Select(taskID, accountGroupID);
				}
			}
			

			if (resultset == null)
			{
				if (!string.IsNullOrEmpty(BudgetType))
				{
					var select = new PXSelect<PMBudgetedCostCode, Where<PMBudgetedCostCode.projectTaskID, Equal<Required<PMBudgetedCostCode.projectTaskID>>,
						And<PMBudgetedCostCode.type, Equal<Required<PMBudgetedCostCode.type>>>>>(_Graph);
					resultset = select.Select(taskID, BudgetType);
				}
				else
				{
					var select = new PXSelect<PMBudgetedCostCode, Where<PMBudgetedCostCode.projectTaskID, Equal<Required<PMBudgetedCostCode.projectTaskID>>>>(_Graph);
					resultset = select.Select(taskID);
				}
			}

			Dictionary<int?, PMBudgetedCostCode> records = new Dictionary<int?, PMBudgetedCostCode>(resultset.Count);

			foreach (PMBudgetedCostCode budget in resultset)
			{
				if (!records.ContainsKey(budget.CostCodeID.Value))
				{
					records.Add(budget.CostCodeID.Value, budget);
				}
			}

			return records;
		}

		public bool IsProjectSpecific(int costCode)
		{
			if (GetTaskID().HasValue && (GetAccountGroupID().HasValue || GetAccountID().HasValue))
			{
				var records = GetProjectSpecificRecords();
				return records.ContainsKey(costCode);
			}
			else
			{
				return true;
			}
		}

		public string GetValidationWarning(PXCache cache, object row, int costCodeID)
		{
			PMCostCode costCode = PMCostCode.PK.Find(_Graph, costCodeID);

			string costCodeCD = FormatValueByMask(cache, row, costCode.CostCodeCD);
			if (costCode.IsActive != true)
			{
				return string.Format(PXLocalizer.Localize(Messages.CostCodeInactiveWithFormat), costCodeCD);
			}

			PMTask task = SelectFrom<PMTask>
				.Where<PMTask.taskID.IsEqual<@P.AsInt>>
				.View
				.Select(_Graph, GetTaskID());

			int? accountGroupID = GetAccountGroupID();
			if (accountGroupID == null)
			{
				Account account = SelectFrom<Account>
				.Where<Account.accountID.IsEqual<@P.AsInt>>
				.View
				.Select(_Graph, GetAccountID());

				if (account != null)
				{
					accountGroupID = account.AccountGroupID;
				}
			}

			PMAccountGroup accountGroup = SelectFrom<PMAccountGroup>
				.Where<PMAccountGroup.groupID.IsEqual<@P.AsInt>>
				.View
				.Select(_Graph, accountGroupID);

			return string.Format(Messages.CostCodeNotInBudget, costCodeCD, task?.TaskCD.Trim(), accountGroup?.GroupCD.Trim());
		}
	}
}


