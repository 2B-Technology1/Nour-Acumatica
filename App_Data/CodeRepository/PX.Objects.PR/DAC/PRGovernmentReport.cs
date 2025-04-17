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
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRGovernmentReport)]
	[Serializable]
	public class PRGovernmentReport : IBqlTable
	{
		public const string CertifiedReportType = "CertifiedPayroll";
		public const string AcaReportType = "Aca";
		public const string W2ReportType = "W2";
		public const string Federal941ReportType = "Federal";
		
		#region Keys
		public class PK : PrimaryKeyOf<PRGovernmentReport>.By<formName, state>
		{
			public static PRGovernmentReport Find(PXGraph graph, string formName, string state, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, formName, state, options);
		}
		#endregion

		#region FormName
		public abstract class formName : PX.Data.BQL.BqlString.Field<formName> { }
		[PXString(40, IsKey = true)]
		[PXUIField(Visible = false)]
		public string FormName { get; set; }
		#endregion
		#region FormDisplayName
		public abstract class formDisplayName : PX.Data.BQL.BqlString.Field<formDisplayName> { }
		[PXString(40)]
		[PXUIField(DisplayName = "Form")]
		public string FormDisplayName { get; set; }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXString(255)]
		[PXUIField(DisplayName = "Description")]
		public string Description { get; set; }
		#endregion
		#region State
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		[PXString(3, IsKey = true)]
		[PXUIField(DisplayName = "State")]
		public string State { get; set; }
		#endregion
		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		[PXString(2, IsKey = true)]
		public string CountryID { get; set; }
		#endregion
		#region ReportingPeriod
		public abstract class reportingPeriod : PX.Data.BQL.BqlString.Field<reportingPeriod> { }
		[PXString(3)]
		[PXUIField(DisplayName = "Reporting Period")]
		[GovernmentReportingPeriod.List]
		public string ReportingPeriod { get; set; }
		#endregion
		#region ReportType
		public abstract class reportType : PX.Data.BQL.BqlString.Field<reportType> { }
		[PXString(IsUnicode = true)]
		[PXUIField(DisplayName = "Report Type")]
		public string ReportType { get; set; }
		#endregion

		#region Year
		public abstract class year : PX.Data.BQL.BqlString.Field<year> { }
		[PXString(4)]
		[PXUIField(DisplayName = Messages.Year)]
		[PXUIVisible(typeof(Where<PRGovernmentReport.reportingPeriod.IsNotEqual<GovernmentReportingPeriod.dateRange>>))]
		[PXSelector(typeof(SelectFrom<PRPayGroupYear>
			.AggregateTo<GroupBy<PRPayGroupYear.year>>
			.OrderBy<PRPayGroupYear.year.Desc>
			.SearchFor<PRPayGroupYear.year>))]
		public string Year { get; set; }
		#endregion
		#region Quarter
		public abstract class quarter : PX.Data.BQL.BqlInt.Field<quarter> { }
		[PXInt]
		[PXUIField(DisplayName = Messages.Quarter)]
		[PXUIVisible(typeof(Where<PRGovernmentReport.reportingPeriod.IsEqual<GovernmentReportingPeriod.quarterly>>))]
		[Quarter.List]
		public int? Quarter { get; set; }
		#endregion
		#region Month
		public abstract class month : PX.Data.BQL.BqlInt.Field<month> { }
		[PXInt]
		[PXUIField(DisplayName = Messages.Month)]
		[PXUIVisible(typeof(Where<PRGovernmentReport.reportingPeriod.IsEqual<GovernmentReportingPeriod.monthly>>))]
		[Month.List]
		public int? Month { get; set; }
		#endregion
		#region DateFrom
		public abstract class dateFrom : PX.Data.BQL.BqlDateTime.Field<dateFrom> { }
		[PXDate]
		[PXUIField(DisplayName = Messages.DateFrom)]
		[PXUIVisible(typeof(Where<PRGovernmentReport.reportingPeriod.IsEqual<GovernmentReportingPeriod.dateRange>>))]
		[PXUnboundDefault(typeof(AccessInfo.businessDate))]
		public DateTime? DateFrom { get; set; }
		#endregion
		#region DateTo
		public abstract class dateTo : PX.Data.BQL.BqlDateTime.Field<dateTo> { }
		[PXDate]
		[PXUIField(DisplayName = Messages.DateTo)]
		[PXUIVisible(typeof(Where<PRGovernmentReport.reportingPeriod.IsEqual<GovernmentReportingPeriod.dateRange>>))]
		[PXUnboundDefault(typeof(AccessInfo.businessDate))]
		public DateTime? DateTo { get; set; }
		#endregion
	}

	public class certifiedReportType : PX.Data.BQL.BqlString.Constant<certifiedReportType>
	{
		public certifiedReportType() : base(PRGovernmentReport.CertifiedReportType) { }
	}

	public class acaReportType : PX.Data.BQL.BqlString.Constant<acaReportType>
	{
		public acaReportType() : base(PRGovernmentReport.AcaReportType) { }
	}
}
