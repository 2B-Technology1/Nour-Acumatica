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

namespace PX.Objects.AP
{
	public class AdjustmentGroupKey
	{
		public class AdjustmentType
		{
			public const string APAdjustment = "A";
			public const string POAdjustment = "P";
			public const string OutstandingBalance = "T";

			public class aPAdjustment : PX.Data.BQL.BqlString.Constant<aPAdjustment>
			{
				public aPAdjustment() : base(APAdjustment) {}
			}
			public class pOAdjustment : PX.Data.BQL.BqlString.Constant<pOAdjustment>
			{
				public pOAdjustment() : base(POAdjustment) { }
			}

			public class outstandingBalance : PX.Data.BQL.BqlString.Constant<outstandingBalance>
			{
				public outstandingBalance() : base(OutstandingBalance) { }
			}

		}

		public string Source { get; set; }
		public string AdjdDocType { get; set; }
		public string AdjdRefNbr { get; set; }
		public long? AdjdCuryInfoID { get; set; }

		public override int GetHashCode() => (Source, AdjdDocType, AdjdRefNbr).GetHashCode();

		public override bool Equals(object obj)
		{
			if (obj is AdjustmentGroupKey key)
				return Source == key.Source && AdjdDocType == key.AdjdDocType && AdjdRefNbr == key.AdjdRefNbr;

			return base.Equals(obj);
		}
	}
}
