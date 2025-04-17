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
using PX.Data.BQL;
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.Objects.PM;

namespace PX.Objects.PJ.Submittals.PJ.Descriptor
{
	public class SubmittalRevisionIDSelector: PXSelectorAttribute
	{
		public SubmittalRevisionIDSelector(Type submittalIDFieldType) : this(
			BqlTemplate
				.OfCommand<
					Search2<PJSubmittal.revisionID,
					InnerJoin<PMProject, On<PMProject.contractID, Equal<PJSubmittal.projectId>>>,
					Where<PJSubmittal.submittalID, Equal<Optional2<BqlPlaceholder.A>>,
						And<MatchUserFor<PMProject>>>,
					OrderBy<Asc<PJSubmittal.submittalID, Desc<PJSubmittal.revisionID>>>>>
				.Replace<BqlPlaceholder.A>(submittalIDFieldType)
				.ToType(),
			typeof(PJSubmittal.revisionID),
			typeof(PJSubmittal.summary))
		{
		}

		public SubmittalRevisionIDSelector(Type type, params Type[] fieldList)
			: base(type, fieldList)
		{
		}

		public SubmittalRevisionIDSelector(Type type, Type lookupJoin, bool cacheGlobal, Type[] fieldList)
			: base(type, lookupJoin, cacheGlobal, fieldList)
		{
		}
	}
}
