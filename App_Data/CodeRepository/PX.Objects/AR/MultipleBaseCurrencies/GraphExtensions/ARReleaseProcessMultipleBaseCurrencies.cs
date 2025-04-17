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
using System.Linq;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.AR
{
	public class ARReleaseProcessMultipleBaseCurrencies : PXGraphExtension<ARReleaseProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		public delegate void PerformBasicReleaseChecksDelegate(PXGraph selectGraph, ARRegister document);
		[PXOverride]
		public void PerformBasicReleaseChecks(PXGraph selectGraph, ARRegister document, PerformBasicReleaseChecksDelegate baseMethod)
		{
			baseMethod(selectGraph, document);

			var SelectCustomer = SelectFrom<ARRegister>
					.InnerJoin<Branch>.On<ARRegister.branchID.IsEqual<Branch.branchID>>
					.InnerJoin<Customer>.On<ARRegister.customerID.IsEqual<Customer.bAccountID>>
				.Where<ARRegister.docType.IsEqual<@P.AsString>
					.And<ARRegister.refNbr.IsEqual<@P.AsString>>
					.And<Branch.baseCuryID.IsNotEqual<Customer.baseCuryID>>
					.And<Customer.baseCuryID.IsNotNull>>
				.View.SelectSingleBound(Base, null, new object[] { document.DocType, document.RefNbr }).RowCast<Customer>();

			if (SelectCustomer.Any())
			{
				Customer customer = SelectCustomer.First();
				throw new PX.Objects.Common.Exceptions.ReleaseException(Messages.BranchCustomerDifferentBaseCuryReleased, PXOrgAccess.GetCD(customer?.COrgBAccountID), customer?.AcctCD);
			}
		}
	}
}
