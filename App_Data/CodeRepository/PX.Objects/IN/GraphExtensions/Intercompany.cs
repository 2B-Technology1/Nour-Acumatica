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
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN.InventoryRelease;
using PX.Objects.SO;

namespace PX.Objects.IN.GraphExtensions
{
	public class Intercompany : PXGraphExtension<INReleaseProcess>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<FeaturesSet.interBranch>()
			&& PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();

		[PXOverride]
		public virtual int? GetCogsAcctID(InventoryItem item, INSite site, INPostClass postclass, INTran tran, bool useTran,
			Func<InventoryItem, INSite, INPostClass, INTran, bool, int?> baseFunc)
		{
			if (tran.BAccountID != null && tran.SOOrderType != null)
			{
				Customer customer = Customer.PK.Find(Base, tran.BAccountID);
				if (customer != null && customer.IsBranch == true)
				{			
					SOOrderType ordertype = SOOrderType.PK.Find(Base, tran.SOOrderType);
					if (ordertype != null)
					{
						switch (ordertype.IntercompanyCOGSAcctDefault)
						{
							case SOIntercompanyAcctDefault.MaskItem:
								return item?.COGSAcctID;
							case SOIntercompanyAcctDefault.MaskLocation:
								if (customer.COGSAcctID == null)
								{
									throw new PXException(Messages.CustomerGOGSAccountIsEmpty);
								}
								return customer.COGSAcctID;
						}
					}
					return null;
				}
			}

			return baseFunc(item, site, postclass, tran, useTran);
		}
	}
}
