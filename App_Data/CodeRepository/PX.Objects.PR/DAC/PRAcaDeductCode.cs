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
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRAcaDeductCode)]
	[Serializable]
	[PXTable(typeof(PRDeductCode.codeID), IsOptional = true)]
	public sealed class PRAcaDeductCode : PXCacheExtension<PRDeductCode>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollUS>();
		}

		#region AcaApplicable
		public abstract class acaApplicable : PX.Data.BQL.BqlBool.Field<acaApplicable> { }
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "ACA Applicable", Visible = false)]
		[PXUIEnabled(typeof(Where<PRDeductCode.isWorkersCompensation.IsEqual<False>
			.And<PRDeductCode.isPayableBenefit.IsEqual<False>>
			.And<PRDeductCode.isGarnishment.IsEqual<False>>>))]
		public bool? AcaApplicable { get; set; }
		#endregion
		#region MinimumIndividualContribution
		public abstract class minimumIndividualContribution : PX.Data.BQL.BqlDecimal.Field<minimumIndividualContribution> { }
		[PRCurrency(MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Minimum Individual Contribution")]
		[PXUIRequired(typeof(Where<PRAcaDeductCode.acaApplicable.IsEqual<True>>))]
		public decimal? MinimumIndividualContribution { get; set; }
		#endregion
	}
}
