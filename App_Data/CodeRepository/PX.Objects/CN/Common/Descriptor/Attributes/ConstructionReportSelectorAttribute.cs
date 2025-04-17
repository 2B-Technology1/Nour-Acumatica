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
using PX.Objects.CA;
using PX.SM;
using urlReports = PX.Objects.Common.urlReports;

namespace PX.Objects.CN.Common.Descriptor.Attributes
{
	public sealed class ConstructionReportSelectorAttribute : PXSelectorAttribute
	{
		public ConstructionReportSelectorAttribute()
			: base(typeof(SelectFrom<SiteMap>
					.Where<SiteMap.url.IsLike<urlReports>
						.And<SiteMap.screenID.IsLike<PXModule.ap_>
							.Or<SiteMap.screenID.IsLike<PXModule.po_>>
							.Or<SiteMap.screenID.IsLike<PXModule.sc_>>
							.Or<SiteMap.screenID.IsLike<PXModule.rq_>>
							.Or<SiteMap.screenID.IsLike<PXModule.cl_>>>>
					.AggregateTo<GroupBy<SiteMap.screenID>>
					.SearchFor<SiteMap.screenID>),
				typeof(SiteMap.screenID), typeof(SiteMap.title))
		{
			Headers = new[]
			{
				CA.Messages.ReportID,
				CA.Messages.ReportName
			};
			DescriptionField = typeof(SiteMap.title);
		}
	}
}