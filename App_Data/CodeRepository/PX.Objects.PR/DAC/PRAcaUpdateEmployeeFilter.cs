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
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRAcaUpdateEmployeeFilter)]
	public class PRAcaUpdateEmployeeFilter : IBqlTable
	{
		#region UpdateAcaFTStatus
		public abstract class updateAcaFTStatus : PX.Data.BQL.BqlBool.Field<updateAcaFTStatus> { }
		[PXBool]
		[PXUIField(DisplayName = "ACA FT Status")]
		public bool? UpdateAcaFTStatus { get; set; }
		#endregion
		#region AcaFTStatus
		public abstract class acaFTStatus : PX.Data.BQL.BqlString.Field<acaFTStatus> { }
		[PXString(3)]
		[AcaFTStatus.List]
		[PXUIEnabled(typeof(Where<PRAcaUpdateEmployeeFilter.updateAcaFTStatus.IsEqual<True>>))]
		[PXUIField(DisplayName = "")]
		public string AcaFTStatus { get; set; }
		#endregion

		#region UpdateOfferOfCoverage
		public abstract class updateOfferOfCoverage : PX.Data.BQL.BqlBool.Field<updateOfferOfCoverage> { }
		[PXBool]
		[PXUIField(DisplayName = "Offer of Coverage")]
		public bool? UpdateOfferOfCoverage { get; set; }
		#endregion
		#region OfferOfCoverage
		public abstract class offerOfCoverage : PX.Data.BQL.BqlString.Field<offerOfCoverage> { }
		[PXString(2)]
		[AcaOfferOfCoverage.List]
		[PXUIEnabled(typeof(Where<PRAcaUpdateEmployeeFilter.updateOfferOfCoverage.IsEqual<True>>))]
		[PXUIField(DisplayName = "")]
		public string OfferOfCoverage { get; set; }
		#endregion

		#region UpdateSection4980H
		public abstract class updateSection4980H : PX.Data.BQL.BqlBool.Field<updateSection4980H> { }
		[PXBool]
		[PXUIField(DisplayName = "Section 4980H")]
		public bool? UpdateSection4980H { get; set; }
		#endregion
		#region Section4980H
		public abstract class section4980H : PX.Data.BQL.BqlString.Field<section4980H> { }
		[PXString(2)]
		[AcaSection4980H.List]
		[PXUIEnabled(typeof(Where<PRAcaUpdateEmployeeFilter.updateSection4980H.IsEqual<True>>))]
		[PXUIField(DisplayName = "")]
		public string Section4980H { get; set; }
		#endregion

		#region UpdateMinimumIndividualContribution
		public abstract class updateMinimumIndividualContribution : PX.Data.BQL.BqlBool.Field<updateMinimumIndividualContribution> { }
		[PXBool]
		[PXUIField(DisplayName = "Minimum Individual Contribution")]
		public bool? UpdateMinimumIndividualContribution { get; set; }
		#endregion
		#region MinimumIndividualContribution
		public abstract class minimumIndividualContribution : PX.Data.BQL.BqlDecimal.Field<minimumIndividualContribution> { }
		[PXDecimal(MinValue = 0)]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIEnabled(typeof(Where<PRAcaUpdateEmployeeFilter.updateMinimumIndividualContribution.IsEqual<True>>))]
		[PXUIField(DisplayName = "")]
		public decimal? MinimumIndividualContribution { get; set; }
		#endregion
	}
}
