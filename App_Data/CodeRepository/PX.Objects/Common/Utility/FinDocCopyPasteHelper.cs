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
using System;
using System.Collections.Generic;
using System.Linq;
using PX.Api.Models;

namespace PX.Objects.Common
{
	public class FinDocCopyPasteHelper
	{
		private const string BranchIDFieldName = "BranchID";
		private const string OriginalObjectName = "CurrentDocument";
		private const string DesiredObjectName = "Document";

		public FinDocCopyPasteHelper(PXGraph graph)
		{
			if (graph.PrimaryItemType == null) return;

			if (graph.PrimaryItemType.GetProperty(BranchIDFieldName) == null)
				throw new InvalidOperationException("The graph is not suitable for this helper because its primary do not have field " + BranchIDFieldName);

			const string GraphDoesNotHaveView = "The graph is not suitable for this helper because it does not have view ";

			if (graph.GetType().GetField(OriginalObjectName) == null)
				throw new InvalidOperationException(GraphDoesNotHaveView + OriginalObjectName);

			if (graph.GetType().GetField(DesiredObjectName) == null)
				throw new InvalidOperationException(GraphDoesNotHaveView + DesiredObjectName);
		}

		public void SetBranchFieldCommandToTheTop(List<Command> script)
		{
			Command cmdBranch = script.Single(cmd => cmd.FieldName == BranchIDFieldName && cmd.ObjectName == OriginalObjectName);
			cmdBranch.ObjectName = DesiredObjectName;
			script.Remove(cmdBranch);
			script.Insert(0, cmdBranch);
		}
	}
}
