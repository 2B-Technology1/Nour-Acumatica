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

namespace PX.Objects.AM.Attributes
{
    public class ClockTranStatus 
    {
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { NewStatus, ClockedIn, ClockedOut },
				new string[]
				{
					Messages.NewStatus,
					Messages.ClockedIn,
					Messages.ClockedOut,
				})
			{
			}
		}

		public class newStatus : PX.Data.BQL.BqlString.Constant<newStatus>
		{
			public newStatus() : base(NewStatus) { }
		}

		public class clockedIn : PX.Data.BQL.BqlString.Constant<clockedIn>
		{
			public clockedIn() : base(ClockedIn) { }
		}

		public class clockedOut : PX.Data.BQL.BqlString.Constant<clockedOut>
		{
			public clockedOut() : base(ClockedOut) { }
		}

		public const string NewStatus = "N";
		public const string ClockedIn = "I";
		public const string ClockedOut = "O";
	}
}
