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


namespace PX.Objects.BQLConstants
{
	#region Misc
	public class BitOn : PX.Data.BQL.BqlBool.Constant<BitOn>
	{
		public BitOn() : base(true) { }
	}

	public class BitOff : PX.Data.BQL.BqlBool.Constant<BitOff>
	{
		public BitOff() : base(false) {}
	}
	public class EmptyString : PX.Data.BQL.BqlString.Constant<EmptyString>
	{
		public EmptyString() : base(string.Empty) { }
	}
	#endregion
	#region Batch Module
	public class moduleGL : PX.Data.BQL.BqlString.Constant<moduleGL>
	{
		public moduleGL() : base("GL") { }
	}

	public class moduleAP : PX.Data.BQL.BqlString.Constant<moduleAP>
	{
		public moduleAP()
			: base("AP")
		{
		}
	}
	public class moduleAR : PX.Data.BQL.BqlString.Constant<moduleAR>
	{
		public moduleAR()
			: base("AR")
		{
		}
	}
	#endregion
	#region Batch Status
	public class statusU : PX.Data.BQL.BqlString.Constant<statusU>
	{
		public statusU()
			: base("U")
		{
		}
	}

	public class statusB : PX.Data.BQL.BqlString.Constant<statusB>
	{
		public statusB()
			: base("B")
		{
		}
	}

	public class statusP : PX.Data.BQL.BqlString.Constant<statusP>
	{
		public statusP()
			: base("P")
		{
		}
	}

	public class statusC : PX.Data.BQL.BqlString.Constant<statusC>
	{
		public statusC()
			: base("C")
		{
		}
	}

	public class statusH : PX.Data.BQL.BqlString.Constant<statusH>
	{
		public statusH()
			: base("H")
		{
		}
	}
	public class statusS : PX.Data.BQL.BqlString.Constant<statusS>
	{
		public statusS()
			: base("S")
		{
		}
	}
	#endregion
	#region Business Account Types
	public class accountVendor : PX.Data.BQL.BqlString.Constant<accountVendor>
	{
		public accountVendor()
			: base("VE")
		{
		}
	}
	public class accountCustomer : PX.Data.BQL.BqlString.Constant<accountCustomer>
	{
		public accountCustomer()
			: base("CU")
		{
		}
	}
	public class accountVendorCustomer : PX.Data.BQL.BqlString.Constant<accountVendorCustomer>
	{
		public accountVendorCustomer()
			: base("VC")
		{
		}
	}
	public class accountUnknown : PX.Data.BQL.BqlString.Constant<accountUnknown>
	{
		public accountUnknown()
			: base("UN")
		{
		}
	}
	#endregion
}
