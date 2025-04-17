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
using PX.Data.BQL;
using PX.Objects.AM.Attributes;
using PX.Objects.EP;
using PX.Objects.IN;
using System;

namespace PX.Objects.AM.CacheExtensions
{
	public sealed class EPShiftCodeExt : PXCacheExtension<EPShiftCode>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
		}

		#region DiffType
		[PXString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Diff Type", Required = true)]
		[ShiftDiffType.List]
		public string DiffType { get; set; }
		public abstract class diffType : BqlString.Field<diffType> { }
		#endregion
		#region ShftDiff
		[PXPriceCost]
		[PXUIField(DisplayName = "Shift Diff", Required = true)]
		public decimal? ShftDiff { get; set; }
		public abstract class shftDiff : BqlDecimal.Field<shftDiff> { }
		#endregion
		#region AMCrewSize
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Crew Size", Required = true, FieldClass = "MFGADVANCEDPLANNING")]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public decimal? AMCrewSize { get; set; }
		public abstract class amCrewSize : BqlDecimal.Field<amCrewSize> { }
		#endregion
	}
}
