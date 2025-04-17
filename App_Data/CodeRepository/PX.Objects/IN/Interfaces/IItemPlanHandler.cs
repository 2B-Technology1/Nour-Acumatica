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
using PX.Common;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.Abstraction;

namespace PX.Objects.IN
{
	public interface IItemPlanHandler<TItemType>
		where TItemType : class, IItemPlanMaster, IBqlTable, new()
	{
		AvailabilitySigns GetAvailabilitySigns<TNode>(TItemType data)
			where TNode : class, IQtyAllocatedBase, IBqlTable, new();

		IDisposable ReleaseModeScope();
	}

	public class AvailabilitySigns
	{
		public Sign SignQtyAvail { get; private set; }
		public Sign SignQtyHardAvail { get; private set; }
		public Sign SignQtyActual { get; private set; }

		public AvailabilitySigns()
		{
		}

		public AvailabilitySigns(decimal signQtyAvail, decimal signQtyHardAvail, decimal signQtyActual)
		{
			SignQtyAvail = Sign.Of(signQtyAvail);
			SignQtyHardAvail = Sign.Of(signQtyHardAvail);
			SignQtyActual = Sign.Of(signQtyActual);
		}
	}
}
