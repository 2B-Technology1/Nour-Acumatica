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
using System.Collections.Generic;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.FA
{
	public class DeleteDocsProcess : PXGraph<DeleteDocsProcess>
	{
		public PXCancel<FARegister> Cancel;

		[PXFilterable]
		[PX.SM.PXViewDetailsButton(typeof(FARegister.refNbr), WindowMode = PXRedirectHelper.WindowMode.NewWindow)]
		public PXProcessing<FARegister, Where<FARegister.released, NotEqual<True>>> Docs;

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Branch", Visible = false)]
		[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.branch>.Or<FeatureInstalled<FeaturesSet.multiCompany>>))]
		protected virtual void _(Events.CacheAttached<FARegister.branchID> e) { }

		public DeleteDocsProcess()
		{
			Docs.SetProcessCaption(Messages.DeleteProc);
			Docs.SetProcessAllCaption(Messages.DeleteAllProc);
			Docs.SetProcessDelegate(delegate(List<FARegister> list)
			{
				bool failed = false;
				TransactionEntry entryGraph = CreateInstance<TransactionEntry>();
				foreach (FARegister register in list)
				{
					PXProcessing.SetCurrentItem(register);
					try
					{
						entryGraph.Clear();
						entryGraph.Document.Current = entryGraph.Document.Search<FARegister.refNbr>(register.RefNbr);
						entryGraph.Delete.Press();
						PXProcessing.SetProcessed();
					}
					catch (Exception e)
					{
						failed = true;
						PXProcessing.SetError(e);
					}
				}
				if (failed)
				{
					throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
				}
			});
		}
	}
}