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
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.Objects.PJ.Submittals.PJ.Descriptor;
using PX.Objects.PM;

namespace PX.Objects.PJ.Submittals
{
	public class SubmittalSearchableAttribute : PXSearchableAttribute
	{
		 private const string FirstLineFormat = "{0}{1}";
		private const string SecondLineFormat = "{1}{2}";

		private static readonly Type[] FieldsForTheFirstLine =
		{
			typeof(PJSubmittal.status),
			typeof(PJSubmittal.projectId)
		};

		private static readonly Type[] FieldsForTheSecondLine =
		{
			typeof(PJSubmittal.currentWorkflowItemContactID), 
			typeof(Objects.CR.Contact.displayName), 
			typeof(PJSubmittal.summary)
		};

		private static readonly Type[] TitleFields =
		{
			typeof(PJSubmittal.submittalID), 
			typeof(PJSubmittal.revisionID)
		};

		private static readonly Type[] IndexedFields =
		{
			typeof(PJSubmittal.summary),
			typeof(PJSubmittal.projectId),
			typeof(PMProject.contractCD),
			typeof(PMProject.description)
		};

		public SubmittalSearchableAttribute()
			: base(SM.SearchCategory.PM, SubmittalMessage.SubmittleSearchTitle, TitleFields, IndexedFields)
		{
			NumberFields = TitleFields;
			Line1Format = FirstLineFormat;
			Line1Fields = FieldsForTheFirstLine;
			Line2Format = SecondLineFormat;
			Line2Fields = FieldsForTheSecondLine;
			SelectForFastIndexing = typeof(Select2<PJSubmittal,
				InnerJoin<PMProject, On<PJSubmittal.projectId, Equal<PMProject.contractID>>>>);
			MatchWithJoin = typeof(InnerJoin<PMProject,
				On<PMProject.contractID, Equal<PJSubmittal.projectId>>>);
		}

		protected override string OverrideDisplayName(Type field, string displayName)
		{
			if (field == typeof(Objects.CR.Contact.displayName))
			{
				return SubmittalMessage.BallInCourt;
			}

			return displayName;
		}
	}
}
