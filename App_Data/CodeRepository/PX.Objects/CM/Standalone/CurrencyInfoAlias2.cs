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

namespace PX.Objects.CM.Standalone
{
	/// <summary>
	/// An alias DAC for <see cref="CurrencyInfo"/> that can e.g. be used
	/// to join <see cref="CurrencyInfo"/> twice in BQL queries.
	/// </summary>
	[Serializable]
	[PXHidden]
	public class CurrencyInfoAlias2 : CurrencyInfo
	{
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		public new abstract class baseCalc : PX.Data.BQL.BqlBool.Field<baseCalc> { }
		public new abstract class baseCuryID : PX.Data.BQL.BqlString.Field<baseCuryID> { }
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		public new abstract class displayCuryID : PX.Data.BQL.BqlString.Field<displayCuryID> { }
		public new abstract class curyRateTypeID : PX.Data.BQL.BqlString.Field<curyRateTypeID> { }
		public new abstract class curyEffDate : PX.Data.BQL.BqlDateTime.Field<curyEffDate> { }
		public new abstract class curyMultDiv : PX.Data.BQL.BqlString.Field<curyMultDiv> { }
		public new abstract class curyRate : PX.Data.BQL.BqlDecimal.Field<curyRate> { }
		public new abstract class recipRate : PX.Data.BQL.BqlDecimal.Field<recipRate> { }
		public new abstract class sampleCuryRate : PX.Data.BQL.BqlDecimal.Field<sampleCuryRate> { }
		public new abstract class sampleRecipRate : PX.Data.BQL.BqlDecimal.Field<sampleRecipRate> { }
		public new abstract class curyPrecision : PX.Data.BQL.BqlShort.Field<curyPrecision> { }
		public new abstract class basePrecision : PX.Data.BQL.BqlShort.Field<basePrecision> { }
		public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
	}
}
