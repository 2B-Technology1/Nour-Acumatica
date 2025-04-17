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

namespace PX.Objects.CR
{
	#region ShowAsListAttribute

	public class ShowAsListAttribute : PXIntListAttribute
	{
		public const int Free = 1;
		public const int OutOfOffice = 3;
		public const int Busy = 2;
		public const int Tentative = 4;

		public ShowAsListAttribute()
			: base(
				new[]
				{
					Free,
					OutOfOffice,
					Busy,
					Tentative
				},
				new[]
				{
					Messages.Free,
					Messages.OutOfOffice,
					Messages.Busy,
					Messages.Tentative
				})
		{
		}

		public class free : PX.Data.BQL.BqlInt.Constant<free>
		{
			public free() : base(Free) { }
		}
		public class outOfOffice : PX.Data.BQL.BqlInt.Constant<outOfOffice>
		{
			public outOfOffice() : base(OutOfOffice) { }
		}
		public class busy : PX.Data.BQL.BqlInt.Constant<busy>
		{
			public busy() : base(Busy) { }
		}
		public class tentative : PX.Data.BQL.BqlInt.Constant<tentative>
		{
			public tentative() : base(Tentative) { }
		}
	}

	#endregion
}