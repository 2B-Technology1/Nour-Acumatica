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
using PX.Objects.CS;
using PX.Objects.EP;
using System;

namespace PX.Objects.PM.TaxZoneExtension
{
	public class ExpenseClaimDetailEntryExt : PXGraphExtension<ExpenseClaimDetailEntry>
	{
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Default<EPExpenseClaimDetails.contractID>))]
		protected virtual void EPExpenseClaimDetails_TaxZoneID_CacheAttached(PXCache cache)
		{
		}

		public static bool IsActive()
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>())
				return false;

			ProjectSettingsManager settings = new ProjectSettingsManager();
			return settings.CalculateProjectSpecificTaxes;
		}

		[PXOverride]
		public virtual string GetDefaultTaxZone(EPExpenseClaimDetails row, Func<EPExpenseClaimDetails, string> baseMethod)
		{
			PMProject project = PMProject.PK.Find(Base, row?.ContractID);
			if (project != null && !string.IsNullOrEmpty(project.CostTaxZoneID))
			{
				return project.CostTaxZoneID;
			}
			else
			{
				return baseMethod(row);
			}
		}
	}
}
