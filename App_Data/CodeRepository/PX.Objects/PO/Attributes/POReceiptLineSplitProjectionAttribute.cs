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
using PX.Common;
using PX.Data;

namespace PX.Objects.PO
{
	/// <summary>
	/// Special projection for POReceiptLineSplit records.
	/// It returns both assigned and unassigned records in the scope of reports or generic inquiries,
	/// but only one type of records depending on the passed parameter in the scope of other graphs.
	/// </summary>
	public class POReceiptLineSplitProjectionAttribute : PXProjectionAttribute
	{
		protected readonly Type CustomSelect;

		public POReceiptLineSplitProjectionAttribute(Type select, Type unassignedType, bool isUnassignedValue)
			: base(select)
		{
			var cmd = BqlCommand.CreateInstance(select);
			cmd = cmd.WhereAnd(
				BqlCommand.Compose(typeof(Where<,>),
				unassignedType,
				typeof(Equal<>),
				isUnassignedValue ? typeof(True) : typeof(False)));

			CustomSelect = cmd.GetSelectType();
			Persistent = true;
		}

		protected override Type GetSelect(PXCache sender)
		{
			if (sender.Graph.GetType().IsIn(typeof(PXGraph), typeof(PXGenericInqGrph)))//report or GI modes
				return base.GetSelect(sender);

			return CustomSelect;
		}
	}
}
