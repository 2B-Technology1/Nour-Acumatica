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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.PJ.Common.Actions
{
	public class PXInsertNoKeysFromUI<TDAC>: PXInsert<TDAC>
		where TDAC : class, IBqlTable, new()
	{
		public PXInsertNoKeysFromUI(PXGraph graph, string name) : base(graph, name)
		{
		}

		public PXInsertNoKeysFromUI(PXGraph graph, Delegate handler) : base(graph, handler)
		{
		}

		protected override void Insert(PXAdapter adapter)
		{
			PXActionHelper.InsertNoKeysFromUI(adapter);
		}
	}

	public class PXDeleteNoKeysFromUI<TDAC> : PXDelete<TDAC>
		where TDAC : class, IBqlTable, new()
	{
		public PXDeleteNoKeysFromUI(PXGraph graph, string name) : base(graph, name)
		{
		}

		public PXDeleteNoKeysFromUI(PXGraph graph, Delegate handler) : base(graph, handler)
		{
		}

		protected override void Insert(PXAdapter adapter)
		{
			PXActionHelper.InsertNoKeysFromUI(adapter);
		}
	}

	public class PXFirstNoKeysFromUI<TDAC> : PXFirst<TDAC>
		where TDAC : class, IBqlTable, new()
	{
		public PXFirstNoKeysFromUI(PXGraph graph, string name) : base(graph, name)
		{
		}

		public PXFirstNoKeysFromUI(PXGraph graph, Delegate handler) : base(graph, handler)
		{
		}

		protected override void Insert(PXAdapter adapter)
		{
			PXActionHelper.InsertNoKeysFromUI(adapter);
		}
	}

	public class PXLastNoKeysFromUI<TDAC> : PXLast<TDAC>
		where TDAC : class, IBqlTable, new()
	{
		public PXLastNoKeysFromUI(PXGraph graph, string name) : base(graph, name)
		{
		}

		public PXLastNoKeysFromUI(PXGraph graph, Delegate handler) : base(graph, handler)
		{
		}

		protected override void Insert(PXAdapter adapter)
		{
			PXActionHelper.InsertNoKeysFromUI(adapter);
		}
	}

	public class PXNextNoKeysFromUI<TDAC> : PXNext<TDAC>
		where TDAC : class, IBqlTable, new()
	{
		public PXNextNoKeysFromUI(PXGraph graph, string name) : base(graph, name)
		{
		}

		public PXNextNoKeysFromUI(PXGraph graph, Delegate handler) : base(graph, handler)
		{
		}

		protected override void Insert(PXAdapter adapter)
		{
			PXActionHelper.InsertNoKeysFromUI(adapter);
		}
	}

	public class PXPreviousNoKeysFromUI<TDAC> : PXPrevious<TDAC>
		where TDAC : class, IBqlTable, new()
	{
		public PXPreviousNoKeysFromUI(PXGraph graph, string name) : base(graph, name)
		{
		}

		public PXPreviousNoKeysFromUI(PXGraph graph, Delegate handler) : base(graph, handler)
		{
		}

		protected override void Insert(PXAdapter adapter)
		{
			PXActionHelper.InsertNoKeysFromUI(adapter);
		}
	}
}
