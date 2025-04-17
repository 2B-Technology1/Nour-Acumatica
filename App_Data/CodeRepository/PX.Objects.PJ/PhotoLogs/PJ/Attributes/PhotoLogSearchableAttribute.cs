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
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Objects.PM;

namespace PX.Objects.PJ.PhotoLogs.PJ.Attributes
{
	public class PhotoLogSearchableAttribute : PXSearchableAttribute
	{
		private const string TitlePrefix = "Photo Log: {0}";
		private const string FirstLineFormat = "{0}{1:d}{2}{3}";
		private const string SecondLineFormat = "{0}{1}";

		private static readonly Type[] TitleFields =
		{
			typeof(PhotoLog.photoLogCd)
		};

		private static readonly Type[] FieldsForTheFirstLine =
		{
			typeof(PhotoLog.statusId),
			typeof(PhotoLog.date),
			typeof(PhotoLog.description),
			typeof(PhotoLog.createdById)
		};

		private static readonly Type[] FieldsForTheSecondLine =
		{
			typeof(PhotoLog.projectId),
			typeof(PhotoLog.projectTaskId)
		};

		private static readonly Type[] IndexedFields =
		{
			typeof(PhotoLog.description),
			typeof(PhotoLog.projectId),
			typeof(PMProject.contractCD),
			typeof(PMProject.description)
		};

		public PhotoLogSearchableAttribute()
			: base(SM.SearchCategory.PM, TitlePrefix, TitleFields, IndexedFields)
		{
			NumberFields = TitleFields;
			Line1Format = FirstLineFormat;
			Line1Fields = FieldsForTheFirstLine;
			Line2Format = SecondLineFormat;
			Line2Fields = FieldsForTheSecondLine;
			SelectForFastIndexing = typeof(Select2<PhotoLog,
				InnerJoin<PMProject, On<PhotoLog.projectId, Equal<PMProject.contractID>>>>);
			MatchWithJoin = typeof(InnerJoin<PMProject,
				On<PMProject.contractID, Equal<PhotoLog.projectId>>>);
		}
	}
}
