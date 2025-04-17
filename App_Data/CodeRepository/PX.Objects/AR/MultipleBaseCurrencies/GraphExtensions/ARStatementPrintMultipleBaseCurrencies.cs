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
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using static PX.Objects.AR.ARStatementPrint;

namespace PX.Objects.AR
{
	public sealed class ARStatementPrintMultipleBaseCurrencies : PXGraphExtension<ARStatementPrint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		protected void _(Events.FieldVerifying<PrintParameters.statementCycleId> e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>()
					&& Base.ARSetup.Current.PrepareStatements.Equals(AR.ARSetup.prepareStatements.ConsolidatedForAllCompanies))
			{
				e.Cache.RaiseExceptionHandling<PrintParameters.statementCycleId>(e.Row, e.NewValue, new PXSetPropertyException(Messages.StatementsCannotBePrinted));
			}
		}
	}
}
