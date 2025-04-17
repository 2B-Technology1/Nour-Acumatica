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

using PX.Common;
using PX.Data;

namespace PX.Objects.Common
{
	[PXInternalUseOnly]
	public class RecordExistsSlot<Table, KeyField, Where> : IPrefetchable
		where Table : class, IBqlTable, new()
		where KeyField : IBqlField
		where Where : IBqlWhere, new()
	{
		public bool RecordExists { get; private set; }

		public void Prefetch()
		{
			var tempGraph = PXGraph.CreateInstance<PXGraph>();

			PXSelectBase<Table> select = CreateSelect(tempGraph);

			using (new PXFieldScope(select.View, typeof(KeyField)))
				RecordExists = ((Table)select.SelectSingle()) != null;
		}

		protected virtual PXSelectBase<Table> CreateSelect(PXGraph tempGraph)
			=> new PXSelectReadonly<Table, Where>(tempGraph);

		public static bool IsRowsExists()
		{
			string slotName = typeof(RecordExistsSlot<Table, KeyField, Where>).ToString();
			var slot = PXDatabase.GetSlot<RecordExistsSlot<Table, KeyField, Where>>(slotName, typeof(Table));

			return slot.RecordExists;
		}
	}

	public class RecordExistsSlot<Table, KeyField> : RecordExistsSlot<Table, KeyField, Where<True, Equal<True>>>
		where Table : class, IBqlTable, new()
		where KeyField : IBqlField
	{
		protected override PXSelectBase<Table> CreateSelect(PXGraph tempGraph)
			=> new PXSelectReadonly<Table>(tempGraph);
	}
}
