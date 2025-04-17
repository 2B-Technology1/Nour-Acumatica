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
using System.Collections;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using static PX.Objects.AR.ARStatementDetails;

namespace PX.Objects.AR
{
	public sealed class ARStatementDetailsMultipleBaseCurrencies : PXGraphExtension<ARStatementDetails>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		public delegate IEnumerable PrintReportDelegate(PXAdapter adapter);
		[PXOverride]
		public IEnumerable PrintReport(PXAdapter adapter, PrintReportDelegate baseMethod)
		{
			if (Base.Details.Current != null)
			{
				DetailsResult res = Base.Details.Current;

				Customer customer = PXSelect<Customer,
						Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
						.Select(Base, res.CustomerId);

				if (PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>()
					&& Base.ARSetup.Current.PrepareStatements.Equals(AR.ARSetup.prepareStatements.ConsolidatedForAllCompanies)
					&& customer.BaseCuryID == null)
				{
					throw new PXException(Messages.StatementsCannotBePrinted);
				}
			}

			return baseMethod(adapter);
		}
	}
}
