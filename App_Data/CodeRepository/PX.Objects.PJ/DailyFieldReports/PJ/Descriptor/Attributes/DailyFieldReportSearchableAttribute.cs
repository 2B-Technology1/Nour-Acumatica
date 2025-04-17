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

using PX.Data;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
	public class DailyFieldReportSearchableAttribute : PXSearchableAttribute
	{
		private const string TitlePrefix = "Daily Field Report: {0}";
		private const string FirstLineFormat = "{0:d}{1}{2}";
		private const string SecondLineFormat = "{0}{2}";

		private static readonly Type[] FieldsForTheFirstLine =
		{
			typeof(DailyFieldReport.date),
			typeof(DailyFieldReport.status),
			typeof(DailyFieldReport.projectId)
		};

		private static readonly Type[] FieldsForTheSecondLine =
		{
			typeof(DailyFieldReport.projectId),
			typeof(DailyFieldReport.projectManagerId),
			typeof(TM.OwnerAttribute.Owner.acctCD)
		};

		private static readonly Type[] TitleFields =
		{
			typeof(DailyFieldReport.dailyFieldReportCd)
		};

		private static readonly Type[] IndexedFields =
		{
			typeof(DailyFieldReport.projectId),
			typeof(PMProject.contractCD),
			typeof(PMProject.description)
		};

		public DailyFieldReportSearchableAttribute()
			: base(Objects.SM.SearchCategory.PM, TitlePrefix, TitleFields, IndexedFields)
		{
			NumberFields = TitleFields;
			Line1Format = FirstLineFormat;
			Line1Fields = FieldsForTheFirstLine;
			Line2Format = SecondLineFormat;
			Line2Fields = FieldsForTheSecondLine;
			SelectForFastIndexing = typeof(Select2<DailyFieldReport,
				InnerJoin<PMProject, On<DailyFieldReport.projectId, Equal<PMProject.contractID>>>>);
			MatchWithJoin = typeof(InnerJoin<PMProject,
				On<PMProject.contractID, Equal<DailyFieldReport.projectId>>>);
		}

		protected override string OverrideDisplayName(Type field, string displayName)
		{
			if (field == typeof(TM.OwnerAttribute.Owner.acctCD))
			{
				return PX.Objects.PJ.Common.Descriptor.CacheNames.ProjectManager;
			}

			return displayName;
		}
	}
}
