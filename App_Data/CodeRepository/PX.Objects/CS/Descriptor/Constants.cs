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
using System.Text;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.SQLTree;

namespace PX.Objects.CS
{
    public static class RoundingType
    {
        public const string Currency = "N";
        public const string Mathematical = "R";
        public const string Ceil = "C";
        public const string Floor = "F";
    }

    public static class InvoiceRounding
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(new string[] { RoundingType.Currency, RoundingType.Mathematical, RoundingType.Ceil, RoundingType.Floor }, new string[] { UseCurrencyPrecision, Nearest, Up, Down }) { }
        }

        public const string UseCurrencyPrecision = "Use Currency Precision";
        public const string Nearest = "Nearest";
        public const string Up = "Up";
        public const string Down = "Down";
    }

    public static class InvoicePrecision
    {
        public class ListAttribute : PXDecimalListAttribute
        {
            public ListAttribute()
                : base(new string[] { m005, m01, m05, m1, m10, m100 }, new string[] { m005, m01, m05, m1, m10, m100 }) { }
        }

        public const string m005 = "0.05";
        public const string m01 = "0.1";
        public const string m05 = "0.5";
        public const string m1 = "1.0";
        public const string m10 = "10";
        public const string m100 = "100";
    }

	public static class CarrierBillingTypes
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { Receiver, ThirdParty },
				new string[] { Messages.Receiver, Messages.ThirdParty })
			{ }
		}

		public const string Receiver = "R";
		public const string ThirdParty = "T";

		public class ReceiverType : BqlString.Constant<ReceiverType> { public ReceiverType() : base(Receiver) { } }
		public class ThirdPartyType : BqlString.Constant<ThirdPartyType> { public ThirdPartyType() : base(ThirdParty) { } }
	}

    public class int0 : PX.Data.BQL.BqlInt.Constant<int0>
	{
		public int0()
			: base((int)0)
		{
		}
	}

	public class int1 : PX.Data.BQL.BqlInt.Constant<int1>
	{
		public int1()
			: base((int)1)
		{
		}
	}
	public class int_1 : PX.Data.BQL.BqlInt.Constant<int_1>
	{
		public int_1()
			: base((int)-1)
		{
		}
	}

	public class int2 : PX.Data.BQL.BqlInt.Constant<int2>
	{
		public int2() : base(2) {}
	}

	public class int3 : PX.Data.BQL.BqlInt.Constant<int3>
	{
		public int3() : base(3) { }
	}

	public class int4 : PX.Data.BQL.BqlInt.Constant<int4>
	{
        public int4()
            : base((int)4)
        {
        }
    }
	public class int5 : PX.Data.BQL.BqlInt.Constant<int2>
	{
		public int5() : base(5) { }
	}

	public class int6 : PX.Data.BQL.BqlInt.Constant<int6>
	{
		public int6() : base(6) { }
	}
	public class int10 : PX.Data.BQL.BqlInt.Constant<int10>
	{
		public int10() : base(10) { }
	}

	public class int15 : PX.Data.BQL.BqlInt.Constant<int15>
	{
			public int15()
				: base(15)
			{
			}
		}

	public class int24 : PX.Data.BQL.BqlInt.Constant<int24>
	{
		public int24() : base(24) { }
	}

	public class int30 : Data.BQL.BqlInt.Constant<int30>
	{
		public int30() : base(30) { }
	}

	public class int60 : Data.BQL.BqlInt.Constant<int30>
	{
		public int60() : base(60) { }
	}

	public class int90 : Data.BQL.BqlInt.Constant<int30>
	{
		public int90() : base(90) { }
	}

	public class short0 : PX.Data.BQL.BqlShort.Constant<short0>
	{
		public short0()
			: base((short)0)
		{
		}
	}

	public class shortMinus1 : PX.Data.BQL.BqlShort.Constant<shortMinus1>
	{
		public shortMinus1()
			: base((short)-1)
		{
		}
	}

	public class short1 : PX.Data.BQL.BqlShort.Constant<short1>
	{
		public short1()
			: base((short)1)
		{
		}
	}

	public class short2 : PX.Data.BQL.BqlShort.Constant<short2>
	{
		public short2()
			: base((short)2)
		{
		}
	}

	public class decimal0 : PX.Data.BQL.BqlDecimal.Constant<decimal0>
	{
		public decimal0()
			: base(0m)
		{
		}
	}

	public class decimal1 : PX.Data.BQL.BqlDecimal.Constant<decimal1>
	{
		public decimal1()
			: base(1m)
		{
		}
	}
	public class decimal_1 : PX.Data.BQL.BqlDecimal.Constant<decimal_1>
	{
		public decimal_1()
			: base(-1m)
		{
		}
	}

	public class decimal100 : PX.Data.BQL.BqlDecimal.Constant<decimal100>
	{
		public decimal100() : base(100m) { ;}
	}

	public class decimalMinus100 : PX.Data.BQL.BqlDecimal.Constant<decimalMinus100>
	{
		public decimalMinus100() : base(-100m) {; }
	}

	public class decimal365 : PX.Data.BQL.BqlDecimal.Constant<decimal365>
	{
		public decimal365() : base(365m) { ;}
	}

	public class decimalMax : PX.Data.BQL.BqlDecimal.Constant<decimalMax>
	{
		public decimalMax() : base((decimal)int.MaxValue) { ;}
	}

	public class guidEmpty : PX.Data.BQL.BqlGuid.Constant<guidEmpty>
	{
		public guidEmpty()
			: base(Guid.Empty)
		{
		}
	}

	public class string0 : PX.Data.BQL.BqlString.Constant<string0>
	{
		public string0()
			: base("0")
		{
		}
	}

	public class string1 : PX.Data.BQL.BqlString.Constant<string1>
	{
		public string1()
			: base("1")
		{
		}
	}

	public class stringA : PX.Data.BQL.BqlString.Constant<stringA>
	{
		public stringA()
			: base("A")
		{
		}
	}

	public class stringO : PX.Data.BQL.BqlString.Constant<stringO>
	{
		public stringO()
			: base("O")
		{
		}
	}
	public class string01 : PX.Data.BQL.BqlString.Constant<string0>
	{
		public string01()
			: base("01")
		{
		}
	}
	/// <summary>
	/// This constant type is deprecated and is only preserved for 
	/// compatibility purposes. Please use <see cref="False"/> instead.
	/// </summary>
	public class boolFalse : PX.Data.BQL.BqlBool.Constant<boolFalse>
	{
		public boolFalse() : base(false) { }
	}

	/// <summary>
	/// This constant type is deprecated and is only preserved for 
	/// compatibility purposes. Please use <see cref="True"/> instead.
	/// </summary>
	public class boolTrue : PX.Data.BQL.BqlBool.Constant<boolTrue>
	{
		public boolTrue() : base(true) { }
	}

	public class intMax : PX.Data.BQL.BqlInt.Constant<intMax>
	{
		public intMax()
			: base(int.MaxValue)
		{
		}
	}



	public class int32000 : PX.Data.BQL.BqlInt.Constant<int32000>
	{
		public int32000()
			: base(32000)
		{
		}
	}

	public class shortMax : PX.Data.BQL.BqlShort.Constant<shortMax>
	{
		public shortMax()
			: base(short.MaxValue)
		{
		}
	}

	public sealed class segmentValueType : PX.Data.BQL.BqlString.Constant<segmentValueType>
	{
		public segmentValueType()
			: base(typeof(SegmentValue).FullName)
		{
		}
	}

	public sealed class TimeZoneNow : IBqlCreator, IBqlOperand
	{
		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			return true;
		}

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			value = PXTimeZoneInfo.Now;
		}
	}

	public sealed class Quotes : PX.Data.BQL.BqlString.Constant<Quotes>
	{
		public Quotes() : base( "\"")
		{
		}
	}
	public sealed class OpenBracket : PX.Data.BQL.BqlString.Constant<OpenBracket>
	{
		public OpenBracket()
			: base("(")
		{
		}
	}
	public sealed class CloseBracket : PX.Data.BQL.BqlString.Constant<CloseBracket>
	{
		public CloseBracket()
			: base(")")
		{
		}
	}

	public static class CountryCodes
	{
		public class us : PX.Data.BQL.BqlString.Constant<us>
		{
			public us() : base(US) { }
		}

		public class canada : PX.Data.BQL.BqlString.Constant<canada>
		{
			public canada() : base(Canada) { }
		}

		public const string US = "US";
		public const string Canada = "CA";
	}
}
