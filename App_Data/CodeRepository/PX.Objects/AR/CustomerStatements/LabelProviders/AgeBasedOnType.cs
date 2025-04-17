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

using PX.Objects.Common;

using PX.Data;

namespace PX.Objects.AR
{
	public class AgeBasedOnType : ILabelProvider
	{
		public const string DueDate = "U";
		public const string DocDate = "O";

		public IEnumerable<ValueLabelPair> ValueLabelPairs => new ValueLabelList
		{
			{ DueDate, Messages.DueDate },
			{ DocDate, Messages.DocDate }
		};

		public class dueDate : PX.Data.BQL.BqlString.Constant<dueDate>
		{
			public dueDate() : base(DueDate) { }
		}

		public class docDate : PX.Data.BQL.BqlString.Constant<docDate>
		{
			public docDate() : base(DocDate) { }
		}
	}
}