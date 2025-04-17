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

namespace PX.Objects.GL
{
	public class FormatDirection
	{
		public const string Display = "D";
		public const string Store = "S";
		public const string Error = "E";

		public sealed class display : PX.Data.BQL.BqlString.Constant<display>
		{
			public display() : base(Display) { }
		}

		public sealed class store : PX.Data.BQL.BqlString.Constant<store>
		{
			public store() : base(Store) { }
		}

		public sealed class error : PX.Data.BQL.BqlString.Constant<error>
		{
			public error() : base(Error) { }
		}
	}

	public class FormatPeriodID<Direction, PeriodID> : BqlFormulaEvaluator<Direction, PeriodID>, IBqlOperand
		where Direction : IBqlOperand
		where PeriodID : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
		{
			string direction = (string)parameters[typeof(Direction)];
			string periodID = (string)parameters[typeof(PeriodID)];

			switch (direction)
			{
				case FormatDirection.Display:
					return FinPeriodIDFormattingAttribute.FormatForDisplay(periodID);
				case FormatDirection.Store:
					return FinPeriodIDFormattingAttribute.FormatForStoring(periodID);
				case FormatDirection.Error:
					return FinPeriodIDFormattingAttribute.FormatForError(periodID);
				default:
					return null;
			}
		}
	}
}
