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
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.PO.GraphExtensions.APReleaseProcessExt;
using System;

namespace PX.Objects.PM
{
	public class UpdatePOOnReleaseExt : PXGraphExtension<UpdatePOOnRelease, APReleaseProcess.MultiCurrency, APReleaseProcess>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectModule>();
		}

		[PXOverride]
		public virtual INRegister CreatePPVAdjustment(APInvoice apdoc, Func<APInvoice, INRegister> baseCreatePPVAdjustment)
		{
			using (new SkipDefaultingFromLocationScope())
			{
				return baseCreatePPVAdjustment(apdoc);
			}
		}

		[PXOverride]
		public virtual void SetProjectForPPVTransaction(GLTran ppvTran, APTran tran, POReceiptLineR1 rctLine,
				Action<GLTran, APTran, POReceiptLineR1> baseMethod)
		{
			if (IsProjectAccount(ppvTran.AccountID))
			{
				ppvTran.ProjectID = tran.ProjectID;
				ppvTran.TaskID = tran.TaskID;
			}
		}

		public bool IsProjectAccount(int? accountID)
		{
			Account account = Account.PK.Find(Base, accountID);
			return account?.AccountGroupID != null;
		}
	}
}
