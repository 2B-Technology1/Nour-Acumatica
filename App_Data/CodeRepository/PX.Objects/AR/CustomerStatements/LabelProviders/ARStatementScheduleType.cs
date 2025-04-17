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

namespace PX.Objects.AR
{
	public class ARStatementScheduleType : ILabelProvider
	{
		public const string Weekly = "W";
		public const string TwiceAMonth = "C";
		public const string FixedDayOfMonth = "F";
		public const string EndOfMonth = "E";
		public const string EndOfPeriod = "P";
		[Obsolete("This constant is not used anymore and will be removed in Acumatica ERP 8.0. Please use " + nameof(TwiceAMonth) + "instead.")]
		public const string Custom = TwiceAMonth;

		public IEnumerable<ValueLabelPair> ValueLabelPairs => new ValueLabelList
		{
			{ Weekly, Messages.Weekly },
			{ TwiceAMonth, Messages.TwiceAMonth },
			{ FixedDayOfMonth, Messages.FixedDayOfMonth },
			{ EndOfMonth, Messages.EndOfMonth },
			{ EndOfPeriod, Messages.EndOfPeriod },
		};

		public class weekly : PX.Data.BQL.BqlString.Constant<weekly>
		{
			public weekly() : base(Weekly) { }
		}

		public class twiceAMonth : PX.Data.BQL.BqlString.Constant<twiceAMonth>
		{
			public twiceAMonth() : base(TwiceAMonth) { }
		}

		public class fixedDayOfMonth : PX.Data.BQL.BqlString.Constant<fixedDayOfMonth>
		{
			public fixedDayOfMonth() : base(FixedDayOfMonth) { }
		}

		public class endOfMonth : PX.Data.BQL.BqlString.Constant<endOfMonth>
		{
			public endOfMonth() : base(EndOfMonth) { }
		}

		public class endOfPeriod : PX.Data.BQL.BqlString.Constant<endOfPeriod>
		{
			public endOfPeriod() : base(EndOfPeriod) { }
		}

		[Obsolete("This attribute is not used anymore and will be removed in Acumatica ERP 8.0. Please use " + nameof(LabelListAttribute) + " instead.")]
		public class ListAttribute : PXStringListAttribute { }
	}

	[Obsolete("This type is not used anymore and will be removed in Acumatica ERP 8.0. Please use " + nameof(ARStatementScheduleType) + " instead.")]
	public class PrepareOnType : ARStatementScheduleType { }
}
