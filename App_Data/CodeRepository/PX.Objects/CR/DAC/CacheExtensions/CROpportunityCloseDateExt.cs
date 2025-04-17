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
using PX.Objects.PR;

namespace PX.Objects.CR.DAC.CacheExtensions
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public sealed class CROpportunityCloseDateExt : PXCacheExtension<CROpportunity>
	{
		#region CloseDateYear
		public abstract class closeDateYear : PX.Data.BQL.BqlDateTime.Field<closeDateYear> { }

		/// <summary>
		/// The estimated date of closing the deal (year part)
		/// </summary>
		/// <value>
		/// Year
		/// </value>
		[PXInt]
		[PXUIField(DisplayName = "Year of Estimated Close Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBCalced(typeof(DatePart<DatePart.year, CROpportunity.closeDate>), typeof(int))]
		public int? CloseDateYear { get; set; }
		#endregion

		#region CloseDateQuarter
		public abstract class closeDateQuarter : PX.Data.BQL.BqlDateTime.Field<closeDateQuarter> { }

		/// <summary>
		/// The estimated date of closing the deal (quarter part)
		/// </summary>
		/// <value>
		/// Quarter
		/// </value>
		[PXInt]
		[PXUIField(DisplayName = "Quarter of Estimated Close Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBCalced(typeof(DatePart<DatePart.quarter, CROpportunity.closeDate>), typeof(int))]
		[Quarter.List]
		public int? CloseDateQuarter { get; set; }
		#endregion

		#region CloseDateMonth
		public abstract class closeDateMonth : PX.Data.BQL.BqlDateTime.Field<closeDateMonth> { }

		/// <summary>
		/// The estimated date of closing the deal (month part)
		/// </summary>
		/// <value>
		/// Month
		/// </value>
		[PXInt]
		[PXUIField(DisplayName = "Month of Estimated Close Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBCalced(typeof(DatePart<DatePart.month, CROpportunity.closeDate>), typeof(int))]
		[Month.List]
		public int? CloseDateMonth { get; set; }
		#endregion
	}
}
