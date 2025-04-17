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
using PX.Objects.CR;
using PX.Objects.CS;
using System;

namespace PX.Objects.AR
{
	public class ARPriceWorksheetMaintVisibilityRestriction : PXGraphExtension<ARPriceWorksheetMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2024R1)]
		public PXSelect<BAccount,
			Where<BAccount.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
				And<Where<BAccount.type, Equal<BAccountType.customerType>,
					Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>
			CustomerCode;

		public override void Initialize()
		{
			base.Initialize();

			Base.Details.WhereAnd<Where<BAccount.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>,
									Or<ARPriceWorksheetDetail.customerID, IsNull>>>();
		}
	}
}
