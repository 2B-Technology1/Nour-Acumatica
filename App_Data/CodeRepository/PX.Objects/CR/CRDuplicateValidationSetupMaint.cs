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
using System.Reflection;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.CR.DAC;
using PX.Objects.CR.Extensions.Cache;
using PX.Objects.EP;
using PX.Web.UI;
using System.Web.Compilation;


namespace PX.Objects.CR
{
	public class CRDuplicateValidationSetupMaint : PXGraph<CRDuplicateValidationSetupMaint>
	{
		#region Views

		public SelectFrom<CRSetup>.View Setup;

		public SelectFrom<CRValidationTree>.View Nodes;

		[PXHidden]
		public SelectFrom<CRValidationTree>.View NodesSelect;

		protected virtual IEnumerable nodes([PXDBInt] int? nodeID)
		{
			if (nodeID == null)
			{
				return NodesSelect.Select();
			}
			else return new CRValidation[0];
		}

		public SelectFrom<
				CRValidation>
			.Where<
				CRValidation.iD.IsEqual<CRValidationTree.iD.FromCurrent>>
			.View
			CurrentNode;

		public SelectFrom<
				CRValidationRules>
			.Where<
				CRValidationRules.validationType.IsEqual<CRValidation.type.FromCurrent>>
			.View
			ValidationRules;

		public PXFilter<CRValidationRulesBuffer> Buffer;

		#endregion

		#region Actions

		public PXSave<CRSetup> Save;

		public PXCancel<CRSetup> Cancel;

		public PXAction<CRValidationRules> Copy;
		[PXButton(ImageKey = Sprite.Main.Copy, Tooltip = ActionsMessages.CopyDocument)]
		[PXUIField(DisplayName = ActionsMessages.CopyRec, Enabled = false)]
		public IEnumerable copy(PXAdapter adapter)
		{
			Buffer.Cache.Clear();
			foreach (CRValidationRules pxResult in ValidationRules.Select()
																	.RowCast<CRValidationRules>()
																	.Where(r => !String.IsNullOrEmpty(r.MatchingEntity) && !String.IsNullOrEmpty(r.MatchingField)))
			{
				CRValidationRulesBuffer insertnode = Buffer.Cache.CreateInstance() as CRValidationRulesBuffer;
				insertnode.MatchingEntity = pxResult.MatchingEntity;
				insertnode.MatchingField = pxResult.MatchingField;
				insertnode.ScoreWeight = pxResult.ScoreWeight;
				insertnode.TransformationRule = pxResult.TransformationRule;
				insertnode.CreateOnEntry = pxResult.CreateOnEntry;
				Buffer.Cache.Insert(insertnode);
			}
			return adapter.Get();
		}

		public PXAction<CRValidationRules> Paste;
		[PXButton(ImageKey = Sprite.Main.Paste, Tooltip = ActionsMessages.PasteDocument)]
		[PXUIField(DisplayName = ActionsMessages.Paste, Enabled = false)]
		internal IEnumerable paste(PXAdapter adapter)
		{
			ValidationRules.Cache.Clear();

			List<string> matchingFields = Buffer.Cache.Cached.RowCast<CRValidationRulesBuffer>().Select(r => String.Concat(r.MatchingEntity, DOUBLE_UNDERSCORE, r.MatchingField)).ToList();
			ValidationRules.Select()
				.RowCast<CRValidationRules>()
				.Where(r => !matchingFields.Contains(String.Concat(r.MatchingEntity, DOUBLE_UNDERSCORE, r.MatchingField)))
				.ForEach(rule => ValidationRules.Delete(rule));

			foreach (CRValidationRulesBuffer ruleBuffer in Buffer.Cache.Cached)
			{
				if (String.IsNullOrEmpty(ruleBuffer.MatchingField)) continue;

				CRValidationRules rule = null;

				rule = ValidationRules
					.Select()
					.FirstTableItems
					.FirstOrDefault(_ =>
						_.ValidationType == CurrentNode.Current.Type
						&& _.MatchingEntity == ruleBuffer.MatchingEntity
						&& _.MatchingField == ruleBuffer.MatchingField
					);

				rule = (rule == null)
						? ValidationRules.Cache.CreateInstance() as CRValidationRules
						: ValidationRules.Cache.CreateCopy(rule) as CRValidationRules;
				
				rule.ValidationType = CurrentNode.Current.Type;
				rule.MatchingEntity = ruleBuffer.MatchingEntity;
				rule.MatchingField = ruleBuffer.MatchingField;
				rule.ScoreWeight = ruleBuffer.ScoreWeight;
				rule.TransformationRule = ruleBuffer.TransformationRule;
				rule.CreateOnEntry = ruleBuffer.CreateOnEntry;
				rule = (CRValidationRules)ValidationRules.Cache.Update(rule);
			}

			return adapter.Get();
		}

		#endregion

		#region Events

		#region CRValidation

		public virtual void _(Events.RowSelected<CRValidation> e)
		{
			PXUIFieldAttribute.SetEnabled<CRValidation.validateOnEntry>(e.Cache, e.Row,
				!ValidationRules
					.Select()
					.FirstTableItems
					.Any(_ => _.CreateOnEntry != CreateOnEntryAttribute.Allow));
		}

		public virtual void _(Events.FieldUpdated<CRValidation.validationThreshold> e)
		{
			ValidationRules
				.Select()
				.FirstTableItems
				.Where(_ => _.CreateOnEntry != CreateOnEntryAttribute.Allow)
				.ForEach(rule =>
				{
					rule.ScoreWeight = e.NewValue as decimal?;

					ValidationRules.Update(rule);
				});
		}

		public virtual void _(Events.RowPersisting<CRValidation> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.GramValidationDateTime == null)
			{
				e.Row.GramValidationDateTime = PXTimeZoneInfo.Now;
			}
		}

		public virtual void _(Events.FieldSelecting<CRValidationTree, CRValidationTree.description> e)
		{
			if (e.Row?.Type is string type
			 && e.Cache.GetStateExt<CRValidationTree.type>(e.Row) is PXStringState state)
			{
				var dict = state.ValueLabelDic;
				if (dict != null && dict.TryGetValue(type, out var label))
				{
					e.ReturnValue = label;
				}
			}
		}

		#endregion

		#region CRValidationRules

		public virtual void _(Events.FieldSelecting<CRValidationRules, CRValidationRules.matchingFieldUI> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.ValidationType == ValidationTypesAttribute.AccountToAccount)
			{
				CreateFieldStateForFieldName(e.Row.ValidationType, e.Args, typeof(Contact), typeof(Address), typeof(Standalone.Location), typeof(BAccount));
			}
			else
			{
				CreateFieldStateForFieldName(e.Row.ValidationType, e.Args, typeof(Contact), typeof(Address));
			}
		}

		public virtual void _(Events.RowUpdated<CRValidationRules> e)
		{
			if (e.Row == null || e.OldRow == null)
				return;

			if (e.Row.CreateOnEntry != e.OldRow.CreateOnEntry)
			{
				ProcessBlockTypeChange(e);
			}

			if (IsSignificantlyChanged(e.Cache, e.Row, e.OldRow))
			{
				UpdateGramValidationDate(e.Row);
			}
		}

		public virtual void _(Events.RowInserted<CRValidationRules> e)
		{
			if (e.Row == null)
				return;
			
			UpdateGramValidationDate(e.Row);
		}

		public virtual void _(Events.RowDeleted<CRValidationRules> e)
		{
			if (e.Row == null)
				return;

			UpdateGramValidationDate(e.Row);
		}

		public virtual void _(Events.RowSelected<CRValidationRules> e)
		{
			if (e.Row == null)
				return;

			PXUIFieldAttribute.SetEnabled<CRValidationRules.validationType>(e.Cache, e.Row, false);
			PXUIFieldAttribute.SetEnabled<CRValidationRules.scoreWeight>(e.Cache, e.Row, e.Row.CreateOnEntry == CreateOnEntryAttribute.Allow);

			TransformationRulesSetList(e.Cache, e.Row);
		}

		public virtual void TransformationRulesSetList(PXCache cache, CRValidationRules row)
		{
			if (IsEmailField(row))
			{
				if (row.TransformationRule != TransformationRulesAttribute.SplitWords)
				{
					PXStringListAttribute.SetList<CRValidationRules.transformationRule>(cache, row,
						allowedValues: new[] { TransformationRulesAttribute.DomainName, TransformationRulesAttribute.None, TransformationRulesAttribute.SplitEmailAddresses },
						allowedLabels: new[] { Messages.DomainName, Messages.None, Messages.SplitEmailAddresses });
				}
			}
			else
			{
				PXStringListAttribute.SetList<CRValidationRules.transformationRule>(cache, row,
					allowedValues: new[] { TransformationRulesAttribute.DomainName, TransformationRulesAttribute.None, TransformationRulesAttribute.SplitWords },
					allowedLabels: new[] { Messages.DomainName, Messages.None, Messages.SplitWords });
			}
		}

		protected virtual void _(Events.FieldVerifying<CRValidationRules, CRValidationRules.matchingFieldUI> e)
		{
			if (e.Row == null)
				return;

			var matchingFieldUIValue = e.NewValue?.ToString();

			if (matchingFieldUIValue == null)
				return;

			var matchingEntity = GetMatchingEntity(matchingFieldUIValue);
			var matchingField = GetMatchingField(matchingFieldUIValue);

			foreach (CRValidationRules rule in
				PXSelect<CRValidationRules,
					Where<CRValidationRules.validationType, Equal<Current<CRValidationRules.validationType>>,
						And<CRValidationRules.matchingEntity, Equal<Required<CRValidationRules.matchingEntity>>,
						And<CRValidationRules.matchingField, Equal<Required<CRValidationRules.matchingField>>>>>>
				.Select(this, matchingEntity, matchingField)
			)
			{
				if (rule != null && rule.MatchingFieldUI == matchingFieldUIValue)
					throw new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded);
			}
		}

		public virtual void _(Events.FieldUpdated<CRValidationRules, CRValidationRules.transformationRule> e)
		{
			if (e.Row == null)
				return;

			var transformationRule = e.NewValue?.ToString();

			if (transformationRule == null)
				return;

			CRValidationRules row = e.Row as CRValidationRules;

			bool isEmailField = IsEmailField(row);

			CheckIsTransformationRuleValid(e.Cache, row, transformationRule, isEmailField);
		}

		public virtual void _(Events.FieldUpdated<CRValidationRules, CRValidationRules.matchingFieldUI> e)
		{
			if (e.Row == null)
				return;

			var matchingFieldUI = e.NewValue?.ToString();

			if (matchingFieldUI == null)
				return;

			CRValidationRules row = e.Row as CRValidationRules;

			var matchingFieldUIState = e.Cache.GetStateExt<CRValidationRules.matchingFieldUI>(row) as PXStringState;

			if (matchingFieldUIState == null)
				return;

			int matchingFieldUIIndex = Array.IndexOf(matchingFieldUIState.AllowedValues, row.MatchingFieldUI);

			if (matchingFieldUIIndex >= 0)
			{
				var matchingFieldUIValue = matchingFieldUIState.AllowedValues[matchingFieldUIIndex];

				row.MatchingEntity = GetMatchingEntity(matchingFieldUIValue);
				row.MatchingField = GetMatchingField(matchingFieldUIValue);
			}

			var transformationRule = row.TransformationRule;
			bool isEmailField = IsEmailField(row);

			if (
				transformationRule.Equals(TransformationRulesAttribute.SplitEmailAddresses) && !isEmailField
				|| transformationRule.Equals(TransformationRulesAttribute.SplitWords) && isEmailField
			)
			{
				e.Cache.SetValueExt<CRValidationRules.transformationRule>(row, TransformationRulesAttribute.None);
			}

			CheckIsTransformationRuleValid(e.Cache, row, transformationRule, isEmailField);
		}

		protected void CheckIsTransformationRuleValid(PXCache rowCache, CRValidationRules row, string transformationRule, bool isEmailField)
		{
			if (transformationRule == TransformationRulesAttribute.DomainName && isEmailField)
			{
				rowCache.RaiseExceptionHandling<CRValidationRules.transformationRule>(row, row.TransformationRule, new PXSetPropertyException(ErrorMessages.EmailFieldWrongTranformationRule, PXErrorLevel.Warning));
			}
		}

		#endregion

		#endregion

		#region Methods

		protected const string DOUBLE_UNDERSCORE = "__";

		public virtual void ProcessBlockTypeChange(Events.RowUpdated<CRValidationRules> e, bool dummyToSuppressEmbeddingIntoEventsList = false)
		{
			if (this.CurrentNode.Current != null && e.Row.CreateOnEntry != CreateOnEntryAttribute.Allow)
			{
				e.Row.ScoreWeight = this.CurrentNode.Current.ValidationThreshold ?? e.Row.ScoreWeight;

				var validation = this.CurrentNode.Current;

				validation.ValidateOnEntry = true;

				this.CurrentNode.Update(validation);
			}
			else
			{
				e.Cache.SetDefaultExt<CRValidationRules.scoreWeight>(e.Row);
			}
		}

		public class FieldDTO
		{
			public string EntityName;
			public string EntityDisplayName;
			public string FieldName;
			public string FieldDisplayName;
			public string FullyQualifiedFieldName => $"{EntityName}{DOUBLE_UNDERSCORE}{FieldName}";
		}

		public virtual void CreateFieldStateForFieldName(string validationType, PXFieldSelectingEventArgs e, params Type[] types)
		{
			List<string> allowedValues = new List<string>();
			List<string> allowedLabels = new List<string>();

			List<FieldDTO> fields = new List<FieldDTO>();

			foreach (var type in types)
			{
				var entityName = PXCacheNameAttribute.GetName(type);

				foreach (var fieldName in this.Caches[type].GetFields_DeduplicationSearch(validationType))
				{
					PXFieldState fs = this.Caches[type].GetStateExt(null, fieldName) as PXFieldState;
					var fieldDisplayName = fs != null ? fs.DisplayName : fieldName;

					fields.RemoveAll(_ => _.FieldName == fieldName || _.FieldDisplayName == fieldDisplayName);

					fields.Add(new FieldDTO
					{
						EntityName = type.FullName,
						EntityDisplayName = entityName,
						FieldName = fieldName,
						FieldDisplayName = fieldDisplayName
					});
				}
			}

			foreach (var item in fields.OrderBy(i => i.FieldDisplayName))
			{
				allowedValues.Add(item.FullyQualifiedFieldName);
				allowedLabels.Add(item.FieldDisplayName);
			}

			e.ReturnState = PXStringState.CreateInstance(e.ReturnValue, 60, null, nameof(CRValidationRules.MatchingFieldUI), false, 1, null, allowedValues.ToArray(), allowedLabels.ToArray(), true, null);
		}

		public virtual bool IsSignificantlyChanged(PXCache sender, object row, object oldRow)
		{
			if (row == null || oldRow == null)
				return true;

			return !sender.ObjectsEqual<CRValidationRules.matchingEntity>(row, oldRow)
				|| !sender.ObjectsEqual<CRValidationRules.matchingField>(row, oldRow)
				|| !sender.ObjectsEqual<CRValidationRules.matchingFieldUI>(row, oldRow)
				|| !sender.ObjectsEqual<CRValidationRules.scoreWeight>(row, oldRow)
				|| !sender.ObjectsEqual<CRValidationRules.transformationRule>(row, oldRow);
		}

		public virtual void UpdateGramValidationDate(CRValidationRules rules)
		{
			System.Diagnostics.Debug.Assert(CurrentNode.Current?.Type == rules.ValidationType, "wrong current node");
				
			var node = PXCache<CRValidation>.CreateCopy(CurrentNode.Current);
			node.GramValidationDateTime = null;
			CurrentNode.Update(node);
		}

		public virtual bool IsEmailField(CRValidationRules rule)
		{
			if (rule.MatchingEntity == null)
				return false;

			var type = PXBuildManager.GetType(rule.MatchingEntity, false);

			if (type == null)
				return false;

			return this.Caches[type]
				.GetFields_WithAttribute<PXDBEmailAttribute>()
				.Any(field => string.Equals(field, rule.MatchingField, StringComparison.OrdinalIgnoreCase));
		}

		public virtual string GetMatchingEntity(string matchingFieldUI) => matchingFieldUI.Substring(0, matchingFieldUI.IndexOf(DOUBLE_UNDERSCORE, StringComparison.InvariantCultureIgnoreCase));

		public virtual string GetMatchingField(string matchingFieldUI) => matchingFieldUI.Substring(matchingFieldUI.IndexOf(DOUBLE_UNDERSCORE, StringComparison.InvariantCultureIgnoreCase) + DOUBLE_UNDERSCORE.Length);

		#endregion

		#region Extensions

		public class GramRecalculationExt : Extensions.CRDuplicateEntities.CRGramRecalculationExt<CRDuplicateValidationSetupMaint>
		{
			public static bool IsActive() => IsFeatureActive();
		}

		#endregion
	}
}
