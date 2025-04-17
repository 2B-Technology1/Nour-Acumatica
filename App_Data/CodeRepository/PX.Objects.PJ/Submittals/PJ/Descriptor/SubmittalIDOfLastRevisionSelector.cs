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
using PX.Data.BQL.Fluent;
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.Objects.PM;

namespace PX.Objects.PJ.Submittals.PJ.Descriptor
{
	public class SubmittalIDOfLastRevisionSelector : PXSelectorAttribute
	{
		public SubmittalIDOfLastRevisionSelector(Type searchType = null)
			: this(
				searchType
				?? typeof(SelectFrom<PJSubmittal>
					.InnerJoin<PMProject>.On<PMProject.contractID.IsEqual<PJSubmittal.projectId>>
					.Where<PJSubmittal.isLastRevision.IsEqual<True>.And<MatchUserFor<PMProject>>>
					.SearchFor<PJSubmittal.submittalID>),
				typeof(PJSubmittal.submittalID),
				typeof(PJSubmittal.revisionID),
				typeof(PJSubmittal.summary))
		{
		}

		public SubmittalIDOfLastRevisionSelector(Type type, params Type[] fieldList)
			: base(type, fieldList)
		{
		}

		public SubmittalIDOfLastRevisionSelector(Type type, Type lookupJoin, bool cacheGlobal, Type[] fieldList)
			: base(type, lookupJoin, cacheGlobal, fieldList)
		{
		}
	}
}
