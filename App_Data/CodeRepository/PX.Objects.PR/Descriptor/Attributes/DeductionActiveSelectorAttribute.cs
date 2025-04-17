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
using System;

namespace PX.Objects.PR
{
	public class DeductionActiveSelectorAttribute : PXSelectorAttribute
	{
		public DeductionActiveSelectorAttribute(Type where, Type matchCountryIDField) : 
			base(BqlTemplate.OfCommand<SearchFor<PRDeductCode.codeID>
				.Where<PRDeductCode.isActive.IsEqual<True>
					.And<PRDeductCode.countryID.IsEqual<BqlPlaceholder.A.AsField.FromCurrent>>
					.And<BqlPlaceholder.B>>>
				.Replace<BqlPlaceholder.A>(matchCountryIDField)
				.Replace<BqlPlaceholder.B>(where == null ? typeof(Where<True.IsEqual<True>>) : where)
				.ToType())
		{
			SubstituteKey = typeof(PRDeductCode.codeCD);
			DescriptionField = typeof(PRDeductCode.description);
		}

		public DeductionActiveSelectorAttribute(Type where) :
			base(BqlTemplate.OfCommand<SearchFor<PRDeductCode.codeID>
				.Where<PRDeductCode.isActive.IsEqual<True>
					.And<MatchPRCountry<PRDeductCode.countryID>>
					.And<BqlPlaceholder.A>>>
				.Replace<BqlPlaceholder.A>(where == null ? typeof(Where<True.IsEqual<True>>) : where)
				.ToType())
		{
			SubstituteKey = typeof(PRDeductCode.codeCD);
			DescriptionField = typeof(PRDeductCode.description);
		}
	}
}
