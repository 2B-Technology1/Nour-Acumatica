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
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PM;

namespace PX.Objects.PJ.DrawingLogs.PJ.Descriptor.Attributes
{
	public class DrawingLogSearchableAttribute : PXSearchableAttribute
	{
		private const string TitlePrefix = "Drawing Log: {0}";
		private const string FirstLineFormat = "{0}{1:d}{2}{3}";
		private const string SecondLineFormat = "{0}{1}{2}{3}";

		private static readonly Type[] FieldsForTheFirstLine =
		{
			typeof(DrawingLog.statusId),
			typeof(DrawingLog.drawingDate),
			typeof(DrawingLog.projectId),
			typeof(DrawingLog.disciplineId)
		};

		private static readonly Type[] FieldsForTheSecondLine =
		{
			typeof(DrawingLog.number),
			typeof(DrawingLog.title),
			typeof(DrawingLog.description),
			typeof(DrawingLog.isCurrent)
		};

		private static readonly Type[] TitleFields =
		{
			typeof(DrawingLog.drawingLogCd)
		};

		private static readonly Type[] IndexedFields =
		{
			typeof(DrawingLog.description),
			typeof(DrawingLog.projectId),
			typeof(PMProject.contractCD),
			typeof(PMProject.description)
		};

		public DrawingLogSearchableAttribute()
			: base(SM.SearchCategory.PM, TitlePrefix, TitleFields, IndexedFields)
		{
			NumberFields = TitleFields;
			Line1Format = FirstLineFormat;
			Line1Fields = FieldsForTheFirstLine;
			Line2Format = SecondLineFormat;
			Line2Fields = FieldsForTheSecondLine;
			SelectForFastIndexing = typeof(Select2<DrawingLog,
				InnerJoin<PMProject, On<DrawingLog.projectId, Equal<PMProject.contractID>>>>);
			MatchWithJoin = typeof(InnerJoin<PMProject,
				On<PMProject.contractID, Equal<DrawingLog.projectId>>>);
		}
	}
}
