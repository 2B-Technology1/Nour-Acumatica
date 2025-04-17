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
using PX.Data.BQL;

namespace PX.Objects.IN
{
	public static class InventoryMultiplicator
	{
		public const short NoUpdate =  0;
		public class noUpdate : BqlShort.Constant<noUpdate> { public noUpdate() : base(NoUpdate) { } }

		public const short Decrease = -1;
		public class decrease : BqlShort.Constant<decrease> { public decrease() : base(Decrease) { } }

		public const short Increase = +1;
		public class increase : BqlShort.Constant<increase> { public increase() : base(Increase) { } }
	}
}