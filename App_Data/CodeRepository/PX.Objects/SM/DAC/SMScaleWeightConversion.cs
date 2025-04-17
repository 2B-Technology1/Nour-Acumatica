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
using PX.Objects.CS;
using PX.Objects.IN;
using PX.SM;

namespace PX.Objects.SM
{
	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public sealed class SMScaleWeightConversion: PXCacheExtension<SMScale>
	{
		#region CompanyUOM
		[INUnboundUnit(DisplayName = "UOM", Enabled = false)]
		[PXFormula(typeof(CommonSetup.weightUOM.FromSetup))]
		public string CompanyUOM { get; set; }
		public abstract class companyUOM : BqlString.Field<companyUOM> { }
		#endregion

		#region CompanyLastWeight
		[INUnitConvert(
			typeof(SMScale.lastWeight),
			typeof(SMScale.uOM),
			typeof(companyUOM),
			DisplayName = "Last Weight",
			Enabled = false)]
		public decimal? CompanyLastWeight { get; set; }
		public abstract class companyLastWeight : BqlDecimal.Field<companyLastWeight> { }
		#endregion
	}
}
