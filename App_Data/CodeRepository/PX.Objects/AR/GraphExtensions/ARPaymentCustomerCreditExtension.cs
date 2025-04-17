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
using PX.Objects.Extensions.CustomerCreditHold;
using EnforceType = PX.Objects.Extensions.CustomerCreditHold.CreditVerificationResult.EnforceType;

namespace PX.Objects.AR.GraphExtensions
{
	/// <summary>A mapped generic graph extension that defines the AR payment credit helper functionality.</summary>	
	public class ARPaymentCustomerCreditExtension : CustomerCreditExtension<
			ARPaymentEntry,
			ARPayment,
			ARPayment.customerID,
			ARPayment.hold,
			ARPayment.released,
			ARPayment.status>
	{
		protected virtual void _(Events.RowInserted<ARPayment> e)
		{
			if (e.Row == null) return;

			UpdateARBalances(e.Cache, e.Row, null);
		}

		protected override void _(Events.RowUpdated<ARPayment> e)
		{
			if (e.Row != null && e.OldRow != null)
				using (new ReadOnlyScope(e.Cache, Base.Caches[typeof(ARBalances)]))
					UpdateARBalances(e.Cache, e.Row, e.OldRow);

			base._(e);
		}

		protected virtual void _(Events.RowDeleted<ARPayment> e)
		{
			if (e.Row == null) return;

			UpdateARBalances(e.Cache, null, e.Row);
		}

		public override void UpdateARBalances(PXCache cache, ARPayment newRow, ARPayment oldRow)
		{
			if (oldRow != null)
			{
				ARReleaseProcess.UpdateARBalances(cache.Graph, oldRow, -oldRow.OrigDocAmt);
			}

			if (newRow != null)
			{
				ARReleaseProcess.UpdateARBalances(cache.Graph, newRow, newRow.OrigDocAmt);
			}
		}

		protected override ARSetup GetARSetup()
			=> Base.arsetup.Current;

		protected override CreditVerificationResult VerifyByCreditRules(
			PXCache sender, ARPayment Row,
			Customer customer, CustomerClass customerclass)
		{
			CreditVerificationResult ret = base.VerifyByCreditRules(sender, Row, customer, customerclass);

			if (ret.Failed == true && customer.Status == CustomerStatus.Hold)
			{
				ret.Failed = false;
				ret.Enforce = EnforceType.None;
			}

			return ret;
		}
	}
}
