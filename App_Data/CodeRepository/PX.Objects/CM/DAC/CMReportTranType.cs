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
using PX.Data;
using PX.Objects.Common;

namespace PX.Objects.CM
{
	[Serializable]
	public class CMReportTranType : PX.Data.IBqlTable
	{
		#region TranType

		public class tranType : PX.Data.BQL.BqlString.Field<tranType>, ILabelProvider
		{
			public const string Revalue = "REV";

			public IEnumerable<ValueLabelPair> ValueLabelPairs => new ValueLabelList
			{
				{Revalue, Messages.Revalue},
			};

			public class revalue : PX.Data.BQL.BqlString.Constant<revalue>
			{
				public revalue() : base(Revalue) { }
			}
		}

		[PXString(3, IsFixed = true)]
		[LabelList(typeof(tranType))]
		public virtual string TranType { get; set; }
		#endregion
	}
}
