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
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data.Wiki.Parser;
using PX.Objects.CR;
using PX.Data;
using System.Collections;
using System.Reflection;
using PX.Objects.Common.Extensions;
using PX.SM;
using PX.TM;
using PX.Objects.CS;

namespace PX.Objects.EP
{
	/// <summary>
	/// Implements the approval process logic.
	/// </summary>
	public class EPAssignmentHelper<Table> : PXGraph<EPAssignmentHelper<Table>>
		where Table : class, IBqlTable, new()
	{
		private readonly PXGraph _Graph;
		
		private readonly List<Guid> path = new List<Guid>();
		private readonly List<PXResult> results = new List<PXResult>();
		private PXGraph processGraph;
		private Type processMapType;
		
		public EPAssignmentHelper(PXGraph graph)
			: this()
		{
			_Graph = graph;
		}

		public EPAssignmentHelper()
		{
		}
		
		public virtual IEnumerable<ApproveInfo> Assign(Table item, EPAssignmentMap map, bool isApprove, int? currentStepSequence)
		{
			path.Clear();

			processMapType = GraphHelper.GetType(map.EntityType);
			Type itemType = item.GetType();

			PXSelectBase<EPRule> rs = new
				PXSelectReadonly<
					EPRule,
				Where<
					EPRule.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>>,
				OrderBy<
					Asc<EPAssignmentRoute.sequence>>>(this);

			PXResultset<EPRule> rules;
			if (isApprove)
			{
				PXSelectBase<EPRule> ss = new
					PXSelectReadonly<
						EPRule,
					Where<
						EPRule.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>,
						And<EPRule.isActive, Equal<boolTrue>,
						And<EPRule.sequence, Greater<Required<EPRule.sequence>>, 
						And<EPRule.stepID, IsNull>>>>,
					OrderBy<
						Asc<EPAssignmentRoute.sequence>>>(this);
				EPRule nextStep = ss.Select(map.AssignmentMapID, currentStepSequence ?? -1);

				if (nextStep == null) yield break;

				rs.WhereAnd<Where<EPRule.stepID, Equal<Required<EPRule.stepID>>>>();
				rules = rs.Select(map.AssignmentMapID, nextStep.RuleID, null);

				rules.ForEach(_ => ((EPRule)_).StepName = nextStep.Name);
			}
			else
			{
				rules = rs.Select(map.AssignmentMapID, null);
			}

			Type mapgraphtype = GraphHelper.GetType(map.GraphType);

			//impossible, but written to support purpose of legacy db states
			if (mapgraphtype == null)
			{
				this.Caches[itemType].Current = item;
				mapgraphtype = EntityHelper.GetPrimaryGraphType(this, processMapType);
			}

			if (_Graph != null && mapgraphtype.IsAssignableFrom(_Graph.GetType()))
			{
				processGraph = _Graph;
			}
			else
			{
				processGraph = CreateInstance(GraphHelper.GetType(map.GraphType) ?? mapgraphtype);
			}

			if (processGraph != null && processMapType != null)
			{
				if (processMapType.IsAssignableFrom(itemType))
				{
					processGraph.Caches[itemType].Current = item;
					processGraph.Caches[processMapType].Current = item;
				}
				else if (itemType.IsAssignableFrom(processMapType))
				{
					object placed = processGraph.Caches[processMapType].CreateInstance();
					PXCache cache = (PXCache)Activator.CreateInstance(typeof(PXCache<>).MakeGenericType(itemType), this);
					cache.RestoreCopy(placed, item);
					processGraph.Caches[processMapType].Current = placed;
				}
				else
					yield break;
			}

			foreach (var approveInfo in ProcessLevel(item, map.AssignmentMapID, rules))
			{
				yield return approveInfo;
			}
		}

		private IEnumerable<ApproveInfo> ProcessLevel(Table item, int? assignmentMap, PXResultset<EPRule> rules)
		{
			foreach (EPRule rule in rules)
			{
				if (rule.RuleID == null || rule.IsActive != true) continue;

				path.Add(rule.RuleID.Value);

				bool isSuccessful = true;
				var filteredItems = ExecuteRule(item, rule, ref isSuccessful);
				
				if (!isSuccessful || (filteredItems != null && filteredItems.Count == 0)) continue;

				int? OwnerID = null;
				int? WorkgroupID = rule.WorkgroupID;

				switch (rule.RuleType)
				{
					case EPRuleType.Direct:

						OwnerID = rule.OwnerID;
						
						FillOwnerWorkgroup(ref OwnerID, ref WorkgroupID);

						if (OwnerID == null && WorkgroupID == null) continue;

						yield return new ApproveInfo()
						{
							OwnerID = OwnerID,
							WorkgroupID = WorkgroupID,
							RuleID = rule.RuleID,
							StepID = rule.StepID,
							WaitTime = rule.WaitTime
						};

						break;

					case EPRuleType.Document:

						if (rule.OwnerSource == null) continue;
						
						PXView lineView = null;
						PXView primaryView = null;
						Type viewType = null;
						try
						{
							var s = rule.OwnerSource;
							var viewName = s.Substring(s.LastIndexOf("(") + 1, s.IndexOf(".") - (s.LastIndexOf("(") + 1));
							
							if(String.Equals(viewName, processGraph.PrimaryView, StringComparison.InvariantCultureIgnoreCase))
							{
								primaryView = processGraph.Views[viewName];
								viewType = primaryView.GetItemType();
							}
							else
							{
								lineView = processGraph.Views[viewName];
								viewType = lineView.GetItemType();
							}
						}
						catch (Exception)
						{
							// ignored
						}


						bool queryContainsDetails = filteredItems != null && filteredItems.Count > 0 && ((PXResult)filteredItems[0])[viewType] != null;

						var arrayToIterate = queryContainsDetails
							? filteredItems
							: lineView?.SelectMulti() ?? primaryView.Cache.Current.SingleToList() ?? filteredItems;

						foreach (object filteredItem in arrayToIterate)
						{
							if (filteredItem is PXResult)
							{
								var line = ((PXResult)filteredItem)[viewType];
								processGraph.Caches[viewType].Current = line;
							}
							else
							{
								processGraph.Caches[viewType].Current = filteredItem;
							}
							
							string code = PXTemplateContentParser.Instance.
								Process(rule.OwnerSource, processGraph, viewType, null,
									(PXCache cache, object value, string fieldName) => 
									{
										int? _ownerID = null;
										if (typeof(Contact).IsAssignableFrom(cache?.GetItemType()))
										{
											_ownerID = (int?)value;
										}
										else if (typeof(EPEmployee).IsAssignableFrom(cache?.GetItemType()) || typeof(CR.Standalone.EPEmployee).IsAssignableFrom(cache?.GetItemType()))
										{
											EPEmployee emp = null;
											if (fieldName.Equals(nameof(EPEmployee.acctCD), StringComparison.OrdinalIgnoreCase))
											{
												emp = PXSelect<
															EPEmployee,
														Where<EPEmployee.acctCD, Equal<Required<EPEmployee.acctCD>>>>
														.SelectWindowed(this, 0, 1, value);
											}
											else
											{
												emp = PXSelect<
															EPEmployee,
														Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>
													.SelectWindowed(this, 0, 1, value);
											}
											_ownerID = emp?.DefContactID;
										}
										else if (value != null)
							{
											EPEmployee emp2 = null;
											int valueInt = 0;

											if (int.TryParse(value.ToString(), out valueInt))
											{
												emp2 = PXSelect<
															EPEmployee,
														Where<EPEmployee.defContactID, Equal<Required<EPEmployee.defContactID>>>>
														.SelectWindowed(this, 0, 1, value);
											}

											if (emp2?.DefContactID == null)
											{
												emp2 = PXSelect<
															EPEmployee,
														Where<EPEmployee.acctCD, Equal<Required<EPEmployee.acctCD>>>>
														.SelectWindowed(this, 0, 1, value);
											}

											if (emp2?.DefContactID == null && valueInt > 0)
											{
												emp2 = PXSelect<
															EPEmployee,
														Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>
														.SelectWindowed(this, 0, 1, value);
											}

											if (emp2?.DefContactID == null)
											{
												emp2 = PXSelectJoin<
											EPEmployee,
										InnerJoin<Users,
											On<Users.pKID, Equal<EPEmployee.userID>>>,
										Where<
											Users.username, Equal<Required<Users.username>>>>
													.SelectWindowed(this, 0, 1, value);
											}
											_ownerID = emp2?.DefContactID;
										}
										return _ownerID.ToString();
									});

							if (int.TryParse(code, out int _result))
							{
								OwnerID = _result;
							}

							if (OwnerID == null && WorkgroupID == null) continue;

							yield return new ApproveInfo()
							{
								OwnerID = OwnerID,
								WorkgroupID = WorkgroupID,
								RuleID = rule.RuleID,
								StepID = rule.StepID,
								WaitTime = rule.WaitTime
							};
						}
						
						break;

					case EPRuleType.Filter:
						
						List<EPRuleBaseCondition> conditions =
							PXSelectReadonly<EPRuleEmployeeCondition,
								Where<EPRuleEmployeeCondition.ruleID, Equal<Required<EPRule.ruleID>>>>
							.Select(this, rule.RuleID)
							.Select(_ => (EPRuleBaseCondition)_)
							.ToList();

						if (conditions.Count == 0)
							break;
						
						foreach (var approveInfo in GetEmployeesByFilter(item, rule, conditions))
						{
							yield return approveInfo;
						}

						break;
				}

				if (rule.RuleID != null)
				{
					if (path.Contains(rule.RuleID.Value)) continue;

					foreach (var approveInfo in ProcessLevel(item, assignmentMap, PXSelectReadonly<EPRule>.Search<EPRule.ruleID>(this, rule.RuleID)))
					{
						yield return approveInfo;
					}
				}

				PXResultset<EPRule> result = PXSelectReadonly<
					EPRule,
					Where<
						EPRule.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>>,
					OrderBy<
						Asc<EPRule.sequence>>>.Select(this, assignmentMap);

				foreach (var approveInfo in ProcessLevel(item, assignmentMap, result))
				{
					yield return approveInfo;
				}
			}
		}

		private void FillOwnerWorkgroup(ref int? OwnerID, ref int? WorkgroupID)
		{
			if (WorkgroupID != null && OwnerID != null)
			{
				EPCompanyTreeMember member =
					PXSelectJoin<
						EPCompanyTreeMember,
					InnerJoin<EPCompanyTreeH,
						On<EPCompanyTreeH.workGroupID, Equal<EPCompanyTreeMember.workGroupID>>,
					InnerJoin<EPEmployee,
						On<EPEmployee.defContactID, Equal<EPCompanyTreeMember.contactID>>>>,
					Where<EPCompanyTreeH.parentWGID, Equal<Required<EPCompanyTreeH.parentWGID>>,
						And<EPCompanyTreeMember.contactID, Equal<Required<EPCompanyTreeMember.contactID>>>>>
					.SelectWindowed(this, 0, 1, WorkgroupID, OwnerID);

				if (member != null)
					WorkgroupID = member.WorkGroupID;
				else
					OwnerID = null;
			}

			if (WorkgroupID != null && OwnerID == null)
			{
				EPCompanyTreeMember owner =
					PXSelectJoin<
						EPCompanyTreeMember,
					InnerJoin<EPEmployee,
						On<EPEmployee.defContactID, Equal<EPCompanyTreeMember.contactID>>>,
					Where<EPCompanyTreeMember.workGroupID, Equal<Required<EPCompanyTreeMember.workGroupID>>,
						And<EPCompanyTreeMember.isOwner, Equal<boolTrue>>>>
					.Select(this, WorkgroupID);

				if (owner != null)
					OwnerID = owner.ContactID;
			}
		}
		
		private List<object> ExecuteRule(Table item, EPRule rule, ref bool isSuccessful)
		{
			try
			{
				List<EPRuleBaseCondition> conditions = 
					PXSelectReadonly<EPRuleCondition, 
						Where<EPRuleCondition.ruleID, Equal<Required<EPRule.ruleID>>, And<EPRuleCondition.isActive, Equal<boolTrue>>>>
					.Select(this, rule.RuleID)
					.Select(_ => (EPRuleBaseCondition)_)
					.ToList();

				if (conditions.Count == 0)
					return null;
				

				Type resultViewType = CreateResultView(processGraph, conditions);
				

				PXView itemView = new PXView(processGraph, false, BqlCommand.CreateInstance(resultViewType),
					(PXSelectDelegate)getItemRecord);

				Select(item, itemView, itemView.GetItemTypes().ToList(), null);


				PXFilterRow[] filters = GenerateFilters(item, conditions).ToArray<PXFilterRow>();

				int startRow = 0;
				int totalRows = 0;
				List<object> result = itemView.Select(null, null, null, null, null, filters, ref startRow, 0, ref totalRows);
				
				results.Clear();

				TraceResult(rule, conditions, result.Count);

				return result;
			}
			catch (Exception e)
			{
				isSuccessful = false;
				PXTrace.WriteInformation(e);
				return null;
			}
		}

		private IEnumerable<ApproveInfo> GetEmployeesByFilter(Table item, EPRule rule, List<EPRuleBaseCondition> conditions)
		{
			Type resultViewType = typeof(Select5<
					EPEmployee,
				LeftJoin<Address,
					On<Address.addressID, Equal<EPEmployee.defAddressID>>,
				LeftJoin<Contact,
					On<Contact.contactID, Equal<EPEmployee.defContactID>>,
				LeftJoin<CSAnswers,
					On<CSAnswers.refNoteID, Equal<EPEmployee.noteID>>>>>,
				Aggregate<GroupBy<EPEmployee.bAccountID>>>);

			PXView itemView = new PXView(this, false, BqlCommand.CreateInstance(resultViewType));

			PXFilterRow[] filters = GenerateFilters(item, conditions).ToArray<PXFilterRow>();

			int startRow = 0;
			int totalRows = 0;
			using (new PXFieldScope(itemView))
			{
				IEnumerable<PXResult<EPEmployee, Address, Contact, CSAnswers>> resultset = itemView
					.Select(null, null, null, null, null, filters, ref startRow, 0, ref totalRows)
					.Select(_ => (PXResult<EPEmployee, Address, Contact, CSAnswers>)_);

				foreach (PXResult<EPEmployee, Address, Contact, CSAnswers> res in resultset)
				{
					Contact contact = res;

					yield return new ApproveInfo()
					{
						OwnerID = contact.ContactID,
						WorkgroupID = rule.WorkgroupID,
						RuleID = rule.RuleID,
						StepID = rule.StepID,
						WaitTime = rule.WaitTime
					};
				}
			}
		}

		private IEnumerable GenerateFilters(Table item, List<EPRuleBaseCondition> conditions)
		{
			foreach (var condition in conditions)
			{
				Type entityType = GraphHelper.GetType(condition.Entity);
				PXFilterRow filter;
				bool attributeFilterUsed = false;

				if (condition.Entity.Equals(typeof(EPEmployee).FullName, StringComparison.InvariantCultureIgnoreCase)
					&& condition.FieldName.Equals(typeof(EPEmployee.workgroupID).Name, StringComparison.InvariantCultureIgnoreCase))
				{
					var employee = _Graph.Caches[entityType].Current;

					filter = CheckEmployeeInWorkgroup((EPEmployee)employee, condition);
				}
				else
				{
					var isAttributeField = !String.IsNullOrWhiteSpace(condition.FieldName) && condition.FieldName.Contains("_Attributes");

					if (isAttributeField)
					{
						string field = condition.FieldName.Substring(0, condition.FieldName.Length - "_Attribute".Length - 1);
						attributeFilterUsed = true;

						var attributeIDFilter = new PXFilterRow()
						{
							OpenBrackets = (condition.OpenBrackets ?? 0) + 1,
							DataField = nameof(CSAnswers) + "__" + nameof(CSAnswers.attributeID),
							Condition = PXCondition.EQ,
							Value = field,
							OrOperator = false
						};
						yield return attributeIDFilter;

						string AttrValue(string value)
						{
							switch (value)
							{
								case "True": return "1";
								case "False": return "0";
								default: return value;
							}
						}

						filter = new PXFilterRow()
						{
							DataField = nameof(CSAnswers) + "__" + nameof(CSAnswers.value),
							Condition = (PXCondition)(condition.Condition ?? 0),
							Value = condition.IsField == true
								? EvaluateField(item, condition.Value)
								: AttrValue(condition.Value),
							Value2 = AttrValue(condition.Value2)
						};
					}
					else
					{
						filter = new PXFilterRow()
						{
							DataField = entityType.Name + "__" + condition.FieldName,
							Condition = (PXCondition)(condition.Condition ?? 0),
							Value = condition.IsField == true
								? EvaluateField(item, condition.Value)
								: condition.Value,
							Value2 = condition.Value2
						};
					}
				}

				object val = filter.Value;
				var cache = _Graph.Caches[entityType];

				if (!attributeFilterUsed && condition.OpenBrackets != null)
					filter.OpenBrackets = condition.OpenBrackets.Value;

				filter.CloseBrackets = (condition.CloseBrackets ?? 0) + (attributeFilterUsed ? 1 : 0);

				filter.OrOperator = condition.Operator == 1;

				yield return filter;
			}
		}

		private void TraceResult(EPRule rule, List<EPRuleBaseCondition> conditions, int count)
		{
			PXTrace.WriteInformation(Messages.TraceRuleResult, 
				rule.StepName, 
				rule.Name, 
				count, 
				count > 0 
					? "Satisfied" 
					: "Unsatisfied");

			string result = null;

			foreach (var condition in conditions)
			{
				result += string.Format(Messages.TraceCondition,
					GraphHelper.GetType(condition.Entity).Name,
					condition.FieldName,
					PXEnumDescriptionAttribute.GetInfo(typeof(PXCondition), (PXCondition)(condition.Condition ?? 0)).Value,
					condition.Value, 
					condition.Value2);
			}

			PXTrace.WriteInformation(Messages.TraceConditions, result);
		}

		private static Type CreateResultView(PXGraph graph, List<EPRuleBaseCondition> conditions)
		{
			List<Type> entities = new List<Type>();
			List<Type> composable = new List<Type>();
			HashSet<Type> attributeEntities = new HashSet<Type>();

			foreach (var condition in conditions)
			{
				Type conditionEntityType = GraphHelper.GetType(condition.Entity);
				if (condition.FieldName.EndsWith("_Attributes"))
				{
					attributeEntities.Add(conditionEntityType);
				}

				if (!entities.Contains(conditionEntityType))
					entities.Add(conditionEntityType);
			}

			foreach (var attributeEntity in attributeEntities)
			{
				if (entities.Count(t => t == typeof(CSAnswers)) == 0)
			{
				entities.Add(typeof(CSAnswers));
			}
				Type noteidType = attributeEntity.GetNestedType("noteID");
				Type answerSelect = BqlCommand.Compose(
					typeof(Select<,>),
					typeof(CSAnswers),
					typeof(Where<,>),
					typeof(CSAnswers.refNoteID),
					typeof(Equal<>),
					typeof(Current<>),
					noteidType);
				PXView view = new PXView(graph, false, BqlCommand.CreateInstance(answerSelect));
				graph.Views.Add("tmp__" + attributeEntity.FullName + "_CSAnswers", view);
			}

			for (int i = 0; i < entities.Count; i++)
			{
				var entity = entities[i];
				PXDBAttributeAttribute.Activate(graph.Caches[entity]);				
				if (i == 0)
				{
					composable.Add(entity);
				}
				else if (i != entities.Count - 1)
				{
					composable.Add(typeof(LeftJoin<,,>));
					composable.Add(entity);
					composable.Add(typeof(On<True, Equal<True>>));
				}
				else
				{
					composable.Add(typeof(LeftJoin<,>));
					composable.Add(entity);
					composable.Add(typeof(On<True, Equal<True>>));
				}
			}

			composable.Insert(0, composable.Count == 1 ? typeof(Select<>) : typeof(Select2<,>));

			return BqlCommand.Compose(composable.ToArray());
		}

		private void Select(Table item, PXView itemView, List<Type> Tables, List<object> pars, int depth = 0)
		{
			if (pars == null)
				pars = new List<object>();

			Type view = BqlCommand.Compose(typeof(EPApprovalView<>), Tables[depth]);
			MethodInfo select = view.GetMethod("Select", BindingFlags.Public | BindingFlags.Static);

			IList source = GetListOfSelectResults(item, itemView, Tables, depth, select);

			depth++;

			foreach (PXResult result in source)
			{
				pars.Add(result[0]);

				if (depth == Tables.Count)
				{
					var entity = Tables.Count == 1 ? result : (PXResult)itemView.CreateResult(pars.ToArray());

					if (!results.Contains(entity))
						results.Add(entity);
				}
				else
				{
					Select(item, itemView, Tables, pars, depth);
				}

				pars.RemoveAt(depth - 1);
			}

			depth--;
		}

		private IList GetListOfSelectResults(Table item, PXView itemView, List<Type> Tables, int depth, MethodInfo select)
		{
			Type currentTable = Tables[depth];
			var source = typeof(Table).IsAssignableFrom(currentTable)
				? new PXResult<Table>(item).SingleToList()
				: (IList)select.Invoke(this, new object[] { processGraph, null });

			if (source.Count == 0)
			{
				// Create a dummy result to keep recursion going
				source = new List<PXResult>();
				source.Add((PXResult)itemView.CreateResult(new object[Tables.Count]));
			}

			return source;
		}

		private PXFilterRow CheckEmployeeInWorkgroup(EPEmployee employee, EPRuleBaseCondition rule)
		{
			var filter = new PXFilterRow()
			{
				DataField = typeof(EPCompanyTree.description).Name,
				Condition = (PXCondition)rule.Condition.Value,
				Value = rule.Value,
				Value2 = null
			};

			PXSelectBase<EPCompanyTree> select = new PXSelectJoin<EPCompanyTree,
					InnerJoin<EPCompanyTreeMember, On<EPCompanyTree.workGroupID, Equal<EPCompanyTreeMember.workGroupID>>>,
					Where<EPCompanyTreeMember.contactID, Equal<Required<EPEmployee.defContactID>>,
						And<EPCompanyTreeMember.active, Equal<True>>>>(this);

			int startRow = 0;
			int totalRows = 0;
			var result = select.View.Select(null, new object[] { employee.DefContactID }, null, null, null, new PXFilterRow[] { filter }, ref startRow, 1, ref totalRows);

			return new PXFilterRow()
			{
				DataField = typeof(EPEmployee).Name + "__" + typeof(EPEmployee.bAccountID).Name,
				Condition = result.Count > 0
					? PXCondition.ISNOTNULL
					: PXCondition.ISNULL,
				Value = null,
				Value2 = null
			};
		}

		private object EvaluateField(Table item, string field)
		{
			return _Graph.Views[_Graph.PrimaryView].Cache.GetValue(item, field) ?? field;
		}

		private IEnumerable getItemRecord()
		{
			return results;
		}
	}
}
