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

namespace PX.Objects.PM
{
	public class ChangeRequestSearchableAttribute : PXSearchableAttribute
	{
		public ChangeRequestSearchableAttribute() : base(
			SM.SearchCategory.PM,
			Messages.ChangeRequestSearchTitle,
			new[] { typeof(PMChangeRequest.refNbr) },
			new[]
			{
				typeof(PMChangeRequest.description),
				typeof(PMChangeRequest.extRefNbr),
				typeof(PMChangeRequest.extRefNbr),
				typeof(PMChangeRequest.projectID),
				typeof(PMProject.contractCD),
				typeof(PMProject.description)
			})
		{
			NumberFields = new[] { typeof(PMChangeRequest.refNbr) };
			Line1Format = "{0:d}{1}{2}";
			Line1Fields = new[]
			{
				typeof(PMChangeRequest.date),
				typeof(PMChangeRequest.status),
				typeof(PMChangeRequest.projectID)
			};
			Line2Format = "{0}";
			Line2Fields = new[]
			{
				typeof(PMChangeRequest.description)
			};
			SelectForFastIndexing = typeof(Select2<PMChangeRequest,
				InnerJoin<PMProject, On<PMChangeRequest.projectID, Equal<PMProject.contractID>>>>);
			MatchWithJoin = typeof(InnerJoin<PMProject,
				On<PMProject.contractID, Equal<PMChangeRequest.projectID>>>);
		}
	}
}
