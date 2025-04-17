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

namespace PX.Objects.PR
{

	public class PROvertimeRuleMaint : PXGraph<PROvertimeRuleMaint>
	{
		#region Views

		[PXFilterable]
		public SelectFrom<PROvertimeRule>
			.Where<MatchPRCountry<PROvertimeRule.countryID>>.View OvertimeRules;

		#endregion

		#region Actions

		public PXSave<PROvertimeRule> Save;
		public PXCancel<PROvertimeRule> Cancel;

		#endregion

		#region Event Handlers

		protected virtual void _(Events.FieldVerifying<PROvertimeRule, PROvertimeRule.overtimeThreshold> e)
		{
			if (string.IsNullOrWhiteSpace(e.Row?.RuleType) || e.NewValue == null)
			{
				return;
			}

			decimal overtimeThreshold = e.Row.RuleType == PROvertimeRuleType.Weekly ? DateConstants.HoursPerWeek : DateConstants.HoursPerDay;

			if (e.NewValue as decimal? > overtimeThreshold)
			{
				e.NewValue = overtimeThreshold;
			}
		}

		protected virtual void _(Events.FieldUpdated<PROvertimeRule, PROvertimeRule.ruleType> e)
		{
			if (e.Row == null)
			{
				return;
			}

			string newRuleType = e.NewValue as string;

			if ((newRuleType == PROvertimeRuleType.Daily || newRuleType == PROvertimeRuleType.Consecutive) && e.Row.OvertimeThreshold > DateConstants.HoursPerDay)
			{
				e.Row.OvertimeThreshold = DateConstants.HoursPerDay;
			}
		}

		protected virtual void _(Events.RowPersisting<PROvertimeRule> e)
		{
			if (e.Row == null || (e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
			{
				return;
			}

			PXPersistingCheck numberOfConsecutiveDaysPersistingCheck = e.Row.RuleType == PROvertimeRuleType.Consecutive ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;
			PXDefaultAttribute.SetPersistingCheck<PROvertimeRule.numberOfConsecutiveDays>(e.Cache, e.Row, numberOfConsecutiveDaysPersistingCheck);
		}

		#endregion
	}
}
