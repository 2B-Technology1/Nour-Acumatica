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
using PX.Data.BQL.Fluent;
using PX.Data.SQLTree;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Payroll.Data;
using System;

namespace PX.Objects.PR
{
	public class PRCountryAttribute : CountryAttribute, IPXFieldDefaultingSubscriber, IPXCommandPreparingSubscriber
	{
		public bool UseDefault { get; set; } = true;

		public PRCountryAttribute() : base(typeof(SearchFor<Country.countryID>
			.Where<Country.countryID.IsIn<BQLLocationConstants.CountryCAN, BQLLocationConstants.CountryUS>>))
		{ }

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (UseDefault)
			{
				e.NewValue = GetPayrollCountry();
			}
		}

		public void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			// Scalars are not supported inside of aggregates, so this logic will only be executed for normal, selects (not inside GROUP BY)
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && (e.Operation & PXDBOperation.GroupBy) == 0)
			{
				Type table = e.Table ?? e.BqlTable;
				SQLExpression whenExpr = new Column(FieldName, table).IsNull();
				SQLExpression thenExpr = new SQLConst(GetPayrollCountry());
				SQLExpression elseExpr = new Column(FieldName, table);
				SQLExpression switchExpr = new SQLSwitch().Case(whenExpr, thenExpr).Default(elseExpr);
				e.Expr = switchExpr;
				e.Cancel = true;
			}
		}

		public static string GetPayrollCountry()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollCAN>() ? LocationConstants.CanadaCountryCode : LocationConstants.USCountryCode;
		}
	}
}
