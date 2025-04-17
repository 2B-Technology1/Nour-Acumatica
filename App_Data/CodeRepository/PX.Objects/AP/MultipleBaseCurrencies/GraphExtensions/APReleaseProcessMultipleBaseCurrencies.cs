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

namespace PX.Objects.AP
{
	public class APReleaseProcessMultipleBaseCurrencies : PXGraphExtension<APReleaseProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		public delegate void PerformBasicReleaseChecksDelegate(APRegister document);
		[PXOverride]
		public void PerformBasicReleaseChecks(APRegister document, PerformBasicReleaseChecksDelegate baseMethod)
		{
			baseMethod(document);

			var SelectVendor = SelectFrom<APRegister>
					.InnerJoin<Branch>.On<APRegister.branchID.IsEqual<Branch.branchID>>
					.InnerJoin<Vendor>.On<APRegister.vendorID.IsEqual<Vendor.bAccountID>>
				.Where<APRegister.docType.IsEqual<@P.AsString>
					.And<APRegister.refNbr.IsEqual<@P.AsString>>
					.And<Branch.baseCuryID.IsNotEqual<Vendor.baseCuryID>>
					.And<Vendor.baseCuryID.IsNotNull>>
				.View.SelectSingleBound(Base, null, new object[] { document.DocType, document.RefNbr }).RowCast<Vendor>();

			if (SelectVendor.Any())
			{
				Vendor vendor = SelectVendor.First();
				throw new PX.Objects.Common.Exceptions.ReleaseException(Messages.BranchVendorDifferentBaseCuryReleased, PXOrgAccess.GetCD(vendor?.VOrgBAccountID), vendor?.AcctCD);
			}
		}
	}
}
