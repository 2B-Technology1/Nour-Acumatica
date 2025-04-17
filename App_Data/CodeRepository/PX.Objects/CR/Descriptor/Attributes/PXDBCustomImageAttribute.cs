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
using PX.Data.SQLTree;

namespace PX.Objects.CR
{
	public class PXDBCustomImageAttribute : PXDBImageAttribute
	{
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			base.CommandPreparing(sender, e);

			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert) ||
				((e.Operation & PXDBOperation.Command) == PXDBOperation.Update) ||
				((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete))
			{
				e.ExcludeFromInsertUpdate();
				e.Expr = null;
			}
			else
			{
				e.Expr = SQLExpression.Null();
			}
		}
	}
}
