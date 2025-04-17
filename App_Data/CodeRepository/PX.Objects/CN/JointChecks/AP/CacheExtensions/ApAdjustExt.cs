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
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CS;
using System;

namespace PX.Objects.CN.JointChecks
{
    public sealed class ApAdjustExt : PXCacheExtension<APAdjust>
    {
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.construction>();
		}

		#region JointPayeeExternalName
		[PXString(30)]
		[PXUIField(DisplayName = "Joint Payee Name")]
		public string JointPayeeExternalName
		{
			get;
			set;
		}

		public abstract class jointPayeeExternalName : BqlString.Field<jointPayeeExternalName>
		{
		}
		#endregion

		#region CuryJointAmountOwed
		[PXCurrency(typeof(APRegister.curyInfoID), typeof(ApAdjustExt.curyJointAmountOwed))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Joint Amount Owed", Enabled = false)]
		public decimal? CuryJointAmountOwed
		{
			get;
			set;
		}
		public abstract class curyJointAmountOwed : BqlDecimal.Field<curyJointAmountOwed> { }

		#endregion

		#region JointAmountOwed
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public decimal? JointAmountOwed
		{
			get;
			set;
		}
		public abstract class jointAmountOwed : BqlDecimal.Field<jointAmountOwed> { }

		#endregion

		#region CuryJointBalance
		[PXCurrency(typeof(APRegister.curyInfoID), typeof(ApAdjustExt.curyJointBalance))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Joint Balance", Enabled = true)]
		public decimal? CuryJointBalance
		{
			get;
			set;
		}
		public abstract class curyJointBalance : BqlDecimal.Field<curyJointBalance>
		{
		}
		#endregion

		#region JointBalance
		[PXBaseCury]
		[PXUIField(DisplayName = "Joint Balance", Enabled = true)]
		public decimal? JointBalance
		{
			get;
			set;
		}
		public abstract class jointBalance : BqlDecimal.Field<jointBalance>
		{
		}
		#endregion

		
	}
}
