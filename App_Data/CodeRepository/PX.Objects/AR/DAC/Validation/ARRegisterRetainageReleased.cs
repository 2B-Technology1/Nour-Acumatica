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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using System;

namespace PX.Objects.AR
{
	[PXProjection(typeof(Select<
		ARRegister,
		Where<ARRegister.isRetainageDocument, Equal<True>,
			And<ARRegister.released, Equal<True>
			>>>), Persistent = false)]
	[PXHidden]
	public class ARRegisterRetainageReleased : IBqlTable
	{
		#region Keys
		public new class PK : PrimaryKeyOf<ARRegisterRetainageReleased>.By<origDocType, origRefNbr>
		{
			public static ARRegisterRetainageReleased Find(PXGraph graph, string origDocType, string origRefNbr, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, origDocType, origRefNbr, options);
		}
		#endregion

		#region OrigDocType
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }

		[PXDBString(IsKey = true, BqlField = typeof(ARRegister.origDocType))]
		public virtual string OrigDocType { get; set; }
		#endregion

		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }

		[PXDBString(IsKey = true, BqlField = typeof(ARRegister.origRefNbr))]
		public virtual string OrigRefNbr { get; set; }
		#endregion

		#region BalanceSign
		public abstract class balanceSign : PX.Data.BQL.BqlDecimal.Field<balanceSign> { }

		[PXDecimal]
		[PXDBCalced(typeof(
				Switch<Case<Where<ARRegister.docType.IsIn<ARDocType.refund, ARDocType.voidRefund, ARDocType.invoice, ARDocType.debitMemo, ARDocType.finCharge, ARDocType.smallCreditWO, ARDocType.cashSale>>,
					decimal1>,
					decimal_1>), typeof(decimal))]
		public virtual decimal? BalanceSign { get; set; }
		#endregion

		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }

		[PXDecimal]
		[PXDBCalced(typeof(ARRegister.curyOrigDocAmt.Multiply<balanceSign>), typeof(decimal))]
		public virtual Decimal? CuryOrigDocAmt { get; set; }
		#endregion

		#region OrigDocAmt
		public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }

		[PXDecimal]
		[PXDBCalced(typeof(ARRegister.origDocAmt.Multiply<balanceSign>), typeof(decimal))]
		public virtual Decimal? OrigDocAmt { get; set; }
		#endregion
	}

	[PXProjection(typeof(Select4<
		ARRegisterRetainageReleased,
		Aggregate<
			GroupBy<ARRegisterRetainageReleased.origDocType,
			GroupBy<ARRegisterRetainageReleased.origRefNbr,

			Sum<ARRegisterRetainageReleased.curyOrigDocAmt,
			Sum<ARRegisterRetainageReleased.origDocAmt
			>>>>>>), Persistent = false)]
	[PXHidden]
	public class ARRegisterRetainageReleasedTotal : IBqlTable
	{
		#region Keys
		public new class PK : PrimaryKeyOf<ARRegisterRetainageReleasedTotal>.By<origDocType, origRefNbr>
		{
			public static ARRegisterRetainageReleasedTotal Find(PXGraph graph, string origDocType, string origRefNbr, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, origDocType, origRefNbr, options);
		}
		#endregion

		#region OrigDocType
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }

		[PXDBString(IsKey = true, BqlTable = typeof(ARRegisterRetainageReleased))]
		public virtual string OrigDocType { get; set; }
		#endregion

		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }

		[PXDBString(IsKey = true, BqlTable = typeof(ARRegisterRetainageReleased))]
		public virtual string OrigRefNbr { get; set; }
		#endregion


		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }

		[PXDBDecimal(BqlField = typeof(ARRegisterRetainageReleased.curyOrigDocAmt))]
		public virtual Decimal? CuryOrigDocAmt { get; set; }
		#endregion

		#region OrigDocAmt
		public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }

		[PXDBDecimal(BqlField = typeof(ARRegisterRetainageReleased.origDocAmt))]
		public virtual Decimal? OrigDocAmt { get; set; }
		#endregion
	}
}
