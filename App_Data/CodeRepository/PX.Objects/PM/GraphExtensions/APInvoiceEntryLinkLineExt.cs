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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.PO;
using PX.Objects.PO.GraphExtensions.APInvoiceSmartPanel;
using PX.Objects.GL;

namespace PX.Objects.PM.GraphExtensions
{
	/// <summary>
	/// Extends AP Invoice Entry with Project related functionality. Requires Project Accounting feature.
	/// </summary>
	public class APInvoiceEntryLinkLineExt : PXGraphExtension<LinkLineExtension, APInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.projectAccounting>();
		}

		public PXAction<APInvoice> linkLine;

		[PXUIField(DisplayName = AP.Messages.LinkLine, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, FieldClass = "DISTR", Visible = false)]
		[PXLookupButton]
		[APMigrationModeDependentActionRestriction(
					restrictInMigrationMode: true,
					restrictForRegularDocumentInMigrationMode: true,
					restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable LinkLine(PXAdapter adapter)
		{
			int? oldAccountID = null;
			int? oldProjectID = null;
			if (Base.Transactions.Current != null)
			{
				oldAccountID = Base.Transactions.Current.AccountID;
				oldProjectID = Base.Transactions.Current.ProjectID;
			}

			var result = Base1.linkLine.Press(adapter).ToArray<object>();

			APTran tran = Base.Transactions.Current;
			if (tran != null &&
				oldAccountID != tran.AccountID &&
				(oldProjectID == null || ProjectDefaultAttribute.IsNonProject(oldProjectID)) &&
				(tran.ProjectID == null || ProjectDefaultAttribute.IsNonProject(tran.ProjectID)) && tran.PONbr != null)
			{
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this.Base, tran.AccountID);
				if (account != null && account.AccountGroupID != null)
				{
					POLine poLine = PXSelect<POLine,
						Where<POLine.orderNbr, Equal<Required<POLine.orderNbr>>, And<POLine.orderType, Equal<Required<POLine.orderType>>, And<POLine.lineNbr, Equal<Required<POLine.lineNbr>>>>>>
						.Select(this.Base, tran.PONbr, tran.POOrderType, tran.POLineNbr);
					if (poLine != null)
					{
						Base.Transactions.SetValueExt<APTran.projectID>(tran, poLine.ProjectID);
					}
				}
			}

			return result;
		}
	}
}
