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
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;

namespace PX.Objects.PR
{
	/// <summary>
	/// Payroll Module's extension of the GLTran DAC.
	/// </summary>
	public sealed class PRxGLTran : PXCacheExtension<GLTran>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region Keys
		public static class FK
		{
			public class EarningType : EPEarningType.PK.ForeignKeyOf<GLTran>.By<earningTypeCD> { }
			public class PayrollWorkLocation : PRLocation.PK.ForeignKeyOf<GLTran>.By<payrollWorkLocationID> { }
		}
		#endregion

		#region EarningTypeCD
		public abstract class earningTypeCD : PX.Data.BQL.BqlString.Field<earningTypeCD> { }
		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true)]
		public string EarningTypeCD { get; set; }
		#endregion
		#region PayrollWorkLocationID
		public abstract class payrollWorkLocationID : PX.Data.BQL.BqlInt.Field<payrollWorkLocationID> { }
		[PXDBInt]
		public int? PayrollWorkLocationID { get; set; }
		#endregion
	}
}
