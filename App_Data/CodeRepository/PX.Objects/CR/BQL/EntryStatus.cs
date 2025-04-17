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

using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.SQLTree;

namespace PX.Objects.CR
{
	public interface IBqlEntryStatus :
		IBqlDataType,
		IBqlEquitable,
		IBqlCastableTo<IBqlEntryStatus>
	{ }
	public abstract class BqlEntryStatus : BqlType<IBqlEntryStatus, PXEntryStatus> { private BqlEntryStatus() { } }

	public class EntryStatus: BqlEntryStatus.Operand<EntryStatus>, IBqlCreator
	{
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			value = cache.InternalCurrent != null ? cache.GetStatus(cache.InternalCurrent) : PXEntryStatus.Notchanged;
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			return true;
		}

		public sealed class inserted : BqlEntryStatus.Constant<inserted>
		{
			public inserted() : base(PXEntryStatus.Inserted) { }
		}
		public sealed class updated : BqlEntryStatus.Constant<updated>
		{
			public updated() : base(PXEntryStatus.Updated) { }
		}
		public sealed class deleted : BqlEntryStatus.Constant<deleted>
		{
			public deleted() : base(PXEntryStatus.Deleted) { }
		}
		public sealed class notchanged : BqlEntryStatus.Constant<notchanged>
		{
			public notchanged() : base(PXEntryStatus.Notchanged) { }
		}
	}
}
