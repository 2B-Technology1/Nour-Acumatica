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
using PX.Objects.AP;
using PX.Objects.CN.ProjectAccounting.AP.GraphExtensions;
using PX.Objects.CS;
using System;

namespace PX.Objects.FS
{
	public class APInvoiceEntryExt : PXGraphExtension<APInvoiceEntryReclassifyingExt, APInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
		}

		[PXOverride]
		public virtual void Reclassify(Action baseHandler)
		{
			foreach (APTran tran in Base.Transactions.Select())
			{
				FSxAPTran extTran = Base.Transactions.Cache.GetExtension<FSxAPTran>(tran);
				if (extTran.RelatedDocNoteID != null)
				{
					throw new PXException(CN.ProjectAccounting.Descriptor.ProjectAccountingMessages.CannotReclassifiedWithServiceDoc);
				}
			}
			baseHandler();
		}
	}
}