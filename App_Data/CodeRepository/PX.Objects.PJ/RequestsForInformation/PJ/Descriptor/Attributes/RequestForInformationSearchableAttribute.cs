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
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PM;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes
{
	public class RequestForInformationSearchableAttribute : PXSearchableAttribute
	{
		private const string TitlePrefix = "Request for Information: {0}";
		private const string FirstLineFormat = "{0}{1:d}{2}";
		private const string SecondLineFormat = "{0:d}{1}{2}";

		private static readonly Type[] FieldsForTheFirstLine =
		{
			typeof(RequestForInformation.status),
			typeof(RequestForInformation.creationDate),
			typeof(RequestForInformation.projectId)
		};

		private static readonly Type[] FieldsForTheSecondLine =
		{
			typeof(RequestForInformation.dueResponseDate),
			typeof(RequestForInformation.incoming),
			typeof(RequestForInformation.summary)
		};

		private static readonly Type[] TitleFields =
		{
			typeof(RequestForInformation.requestForInformationCd)
		};

		private static readonly Type[] IndexedFields =
		{
			typeof(RequestForInformation.summary),
			typeof(RequestForInformation.projectId),
			typeof(PMProject.contractCD),
			typeof(PMProject.description)
		};

		public RequestForInformationSearchableAttribute()
			: base(SM.SearchCategory.PM, TitlePrefix, TitleFields, IndexedFields)
		{
			NumberFields = TitleFields;
			Line1Format = FirstLineFormat;
			Line1Fields = FieldsForTheFirstLine;
			Line2Format = SecondLineFormat;
			Line2Fields = FieldsForTheSecondLine;
			SelectForFastIndexing = typeof(Select2<RequestForInformation,
				InnerJoin<PMProject, On<RequestForInformation.projectId, Equal<PMProject.contractID>>>>);
			MatchWithJoin = typeof(InnerJoin<PMProject,
							On<PMProject.contractID, Equal<RequestForInformation.projectId>>>);
		}
	}
}
