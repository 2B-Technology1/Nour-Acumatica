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

namespace PX.Objects.Common.DAC
{
	[Serializable]
	[PXCacheName("AP AR Overdue Bucket")]
	public class APAROverdueBucket : IBqlTable
	{
		#region OverdueBucket
		public new abstract class overdueBucket : PX.Data.BQL.BqlString.Field<overdueBucket>
		{
			public const string Current = "B1-Current";
			public const string Overdue1to30 = "B2-1-30";
			public const string Overdue31to60 = "B3-31-60";
			public const string Overdue61to90 = "B4-61-90";
			public const string OverdueOver90 = "B5->90";

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
					(Current, Messages.CurrentBucket),
					(Overdue1to30, Messages.Overdue1to30Bucket),
					(Overdue31to60, Messages.Overdue31to60Bucket),
					(Overdue61to90, Messages.Overdue61to90Bucket),
					(OverdueOver90, Messages.OverdueOver90Bucket)
				)
				{ }
			}
		}

		[PXString]
		[overdueBucket.List]
		[PXUIField(DisplayName = "Overdue Bucket")]
		public virtual string OverdueBucket { get; set; }
		#endregion
	}
}
