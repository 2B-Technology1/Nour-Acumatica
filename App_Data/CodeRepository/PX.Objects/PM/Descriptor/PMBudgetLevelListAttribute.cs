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
using System.Collections.Generic;

namespace PX.Objects.PM
{
	public class PMBudgetLevelListAttribute : PXStringListAttribute
	{
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			List<string> allowedValues = new List<string>();
			List<string> allowedLabels = new List<string>();

			allowedValues.Add(BudgetLevels.Task);
			allowedLabels.Add(PXMessages.LocalizeNoPrefix(Messages.Task));

			bool useCostCode = CostCodeAttribute.UseCostCode();
			if (useCostCode)
			{
				allowedValues.Add(BudgetLevels.CostCode);
				allowedLabels.Add(PXMessages.LocalizeNoPrefix(Messages.BudgetLevel_CostCode));
			}

			allowedValues.Add(BudgetLevels.Item);
			allowedLabels.Add(PXMessages.LocalizeNoPrefix(Messages.BudgetLevel_Item));

			if (useCostCode)
			{
				allowedValues.Add(BudgetLevels.Detail);
				allowedLabels.Add(PXMessages.LocalizeNoPrefix(Messages.BudgetLevel_Detail));
			}

			_AllowedValues = allowedValues.ToArray();
			_AllowedLabels = allowedLabels.ToArray();

			base.FieldSelecting(sender, e);
		}
	}
}