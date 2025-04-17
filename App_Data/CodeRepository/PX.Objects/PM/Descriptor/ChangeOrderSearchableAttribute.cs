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
	public class ChangeOrderSearchableAttribute : PXSearchableAttribute
	{
		public ChangeOrderSearchableAttribute()
			: base(SM.SearchCategory.PM,
				Messages.ChangeOrderSearchTitle,
				new[] { typeof(PMChangeOrder.refNbr) },
				new[]
				{
					typeof(PMChangeOrder.description),
					typeof(PMChangeOrder.extRefNbr),
					typeof(PMChangeOrder.projectID),
					typeof(PMProject.contractCD),
					typeof(PMProject.description)
				})
		{
			NumberFields = new[] { typeof(PMChangeOrder.refNbr) };
			Line1Format = "{0:d}{1}{2}";
			Line1Fields = new[]
			{
				typeof(PMChangeOrder.date),
				typeof(PMChangeOrder.status),
				typeof(PMChangeOrder.projectID)
			};
			Line2Format = "{0}";
			Line2Fields = new[]
			{
				typeof(PMChangeOrder.description)
			};
			SelectForFastIndexing = typeof(Select2<PMChangeOrder,
				InnerJoin<PMProject, On<PMChangeOrder.projectID, Equal<PMProject.contractID>>>>);
			MatchWithJoin = typeof(InnerJoin<PMProject,
				On<PMProject.contractID, Equal<PMChangeOrder.projectID>>>);
		}
	}
}
