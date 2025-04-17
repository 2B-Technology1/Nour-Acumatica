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


namespace PX.Objects.TX
{
	public class MappingTypesAttribute : PXStringListAttribute
	{
		public const string OneOrMoreCountires = "C";
		public const string OneOrMoreStates = "S";
		public const string OneOrMorePostalCodes = "P";

		public MappingTypesAttribute()
			: base(
			new string[] { OneOrMoreCountires, OneOrMoreStates, OneOrMorePostalCodes },
			new string[] { Messages.OneOrMoreCountires, Messages.OneOrMoreStates, Messages.OneOrMorePostalCodes })
		{ }

		public class oneOrMoreCountires : PX.Data.BQL.BqlString.Constant<oneOrMoreCountires>
		{
			public oneOrMoreCountires() : base(OneOrMoreCountires) { }
		}

		public class oneOrMoreStates : PX.Data.BQL.BqlString.Constant<oneOrMoreStates>
		{
			public oneOrMoreStates() : base(OneOrMoreStates) { }
		}

		public class oneOrMorePostalCodes : PX.Data.BQL.BqlString.Constant<oneOrMorePostalCodes>
		{
			public oneOrMorePostalCodes() : base(OneOrMorePostalCodes) { }
		}
	}
}
