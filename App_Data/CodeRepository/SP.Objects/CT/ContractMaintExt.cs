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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using SP.Objects.CR;

namespace SP.Objects.CT
{
	public class ContractMaintExt : PXGraphExtension<ContractMaint>
	{
		#region ctor
		public override void Initialize()
		{
			Base.action.SetVisible(false);
			Base.inquiry.SetVisible(false);

			if (Base.Contracts.Cache.Current != null)
			{
				Contract row = Base.Contracts.Cache.Current as Contract;
				if (row != null)
				{
					foreach (
					var res in
						PXSelect<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Required<Contract.contractID>>>>
							.Select(Base, row.ContractID))
					{
						ContractBillingSchedule curContractBillingSchedule =
							res[typeof(ContractBillingSchedule)] as ContractBillingSchedule;

						if (curContractBillingSchedule != null)
						{
							bool visibility = true;
							ContractExt row1;
							row1 = PXCache<Contract>.GetExtension<ContractExt>(row);
							row1.FinanceVisible = 1;

                            BAccount currentBA = ReadBAccount.ReadCurrentAccount();
							if (curContractBillingSchedule.AccountID != currentBA.BAccountID)
							{
								visibility = false;
								row1.FinanceVisible = 2;
							}
							PXUIFieldAttribute.SetVisible<Contract.balance>(Base.Contracts.Cache, null, visibility);
							PXUIFieldAttribute.SetVisible<ContractBillingSchedule.locationID>(Base.Caches[typeof(ContractBillingSchedule)], null, visibility);
							PXUIFieldAttribute.SetVisible<ContractBillingSchedule.startBilling>(Base.Caches[typeof(ContractBillingSchedule)], null, visibility);
							PXUIFieldAttribute.SetVisible<ContractBillingSchedule.type>(Base.Caches[typeof(ContractBillingSchedule)], null, visibility);
							PXUIFieldAttribute.SetVisible<ContractBillingSchedule.lastDate>(Base.Caches[typeof(ContractBillingSchedule)], null, visibility);
							PXUIFieldAttribute.SetVisible<ContractBillingSchedule.nextDate>(Base.Caches[typeof(ContractBillingSchedule)], null, visibility);
						}
					}
				}
			}
		}
		#endregion

		#region Select
		[PXHidden] 
		public PXSelect<ContractItem> ContractItems;
		
		/*public PXSelectJoin<Contract,
				InnerJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>,
				LeftJoin<Customer, On<Customer.bAccountID, Equal<Contract.customerID>>>>,
				Where<Contract.isTemplate, Equal<boolFalse>,
				And<Contract.baseType, Equal<Contract.ContractBaseType>,
				And<Contract.status, NotEqual<ContractStatus.ContractStatusCanceled>,
						And<
						Where2<
						Where2<MatchWithBAccount<Contract.customerID, Current<AccessInfo.userID>>, 
								And<Contract.customerID, IsNotNull>>, 
						Or<
						Where2<MatchWithBAccount<ContractBillingSchedule.accountID, Current<AccessInfo.userID>>, 
							And<ContractBillingSchedule.accountID, IsNotNull>>>>>>>>> Contracts;*/


		public PXSelectJoinGroupBy<Contract,
            LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contract.customerID>>,
                LeftJoin<Contact, On<Contact.bAccountID, Equal<BAccount.bAccountID>>,
					LeftJoin<ContractBillingSchedule, On<ContractBillingSchedule.contractID, Equal<Contract.contractID>>,
						LeftJoin<Customer, On<Customer.bAccountID, Equal<Contract.customerID>>>>>>,
			Where<Contract.customerID, IsNotNull,
				And<Contract.isTemplate, Equal<boolFalse>,
					And<Contract.baseType, Equal<Contract.ContractBaseType>,
						And<Contract.status, NotEqual<Contract.status.canceled>,
							And<
								Where2<
									Where2<MatchWithBAccount<Contract.customerID, Current<AccessInfo.userID>>,
										And<Contract.customerID, IsNotNull>>,
									Or<
										Where2<MatchWithBAccount<ContractBillingSchedule.accountID, Current<AccessInfo.userID>>,
											And<ContractBillingSchedule.accountID, IsNotNull>>>>>>>>>,
			Aggregate<GroupBy<Contract.contractID>>,
			OrderBy<Asc<Contract.contractCD>>> Contracts;

		#endregion

		#region Action
		public PXAction<Contract> PrintSelectedDocument;
		[PXUIField(DisplayName = "Print Selected Document", Enabled = true, Visible = false)]
		[PXButton]
		public virtual void printSelectedDocument()
		{
			if (Base.Invoices.Current != null)
			{
				if ((Base.Invoices.Current.DocType == ARDocType.Invoice) ||
					(Base.Invoices.Current.DocType == ARDocType.DebitMemo) ||
					(Base.Invoices.Current.DocType == ARDocType.CreditMemo))
				{
					Dictionary<string, string> parameters = new Dictionary<string, string>();
					Export(parameters, Base.Invoices.Current);
					throw new PXReportRequiredException(parameters, "AR641000", "Invoice/Memo");
				}
			}
		}

		protected static void Export(Dictionary<string, string> aRes, ARInvoice aDetail)
		{
			aRes["DocType"] = aDetail.DocType;
			aRes["RefNbr"] = aDetail.RefNbr;
		}
		#endregion

		#region Event Handlers
		protected virtual void Contract_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected sel)
		{
			SetUpActionsVisibility(false);

			if (sel != null)
				sel(sender, e);

			Contract row = e.Row as Contract;
			if (row == null) return;

			foreach (
					var res in
						PXSelect<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Required<Contract.contractID>>>>
							.Select(Base, row.ContractID))
			{
				ContractBillingSchedule curContractBillingSchedule =
					res[typeof(ContractBillingSchedule)] as ContractBillingSchedule;

				if (curContractBillingSchedule != null)
				{
					bool visibility = true;
					ContractExt row1;
					row1 = PXCache<Contract>.GetExtension<ContractExt>(row);
					row1.FinanceVisible = 1;

                    BAccount currentBA = ReadBAccount.ReadCurrentAccount();
					if (curContractBillingSchedule.AccountID != currentBA.BAccountID)
					{
						visibility = false;
						row1.FinanceVisible = 2;
					}

					PXUIFieldAttribute.SetVisible<Contract.balance>(Base.Contracts.Cache, null, visibility);
					PXUIFieldAttribute.SetVisible<Contract.templateID>(Base.Contracts.Cache, null, visibility);
					PXUIFieldAttribute.SetVisible<ContractBillingSchedule.locationID>(Base.Caches[typeof(ContractBillingSchedule)], null, visibility);
					PXUIFieldAttribute.SetVisible<ContractBillingSchedule.startBilling>(Base.Caches[typeof(ContractBillingSchedule)], null, visibility);
					PXUIFieldAttribute.SetVisible<ContractBillingSchedule.type>(Base.Caches[typeof(ContractBillingSchedule)], null, visibility);
					PXUIFieldAttribute.SetVisible<ContractBillingSchedule.lastDate>(Base.Caches[typeof(ContractBillingSchedule)], null, visibility);
					PXUIFieldAttribute.SetVisible<ContractBillingSchedule.nextDate>(Base.Caches[typeof(ContractBillingSchedule)], null, visibility);
				}
			}
		}
		#endregion

		private void SetUpActionsVisibility(bool isVisible)
		{
			Base.action.SetVisible(isVisible);
			Base.bill.SetVisible(isVisible);
			Base.inquiry.SetVisible(isVisible);
			Base.setup.SetVisible(isVisible);
			Base.activate.SetVisible(isVisible);
			Base.setupAndActivate.SetVisible(isVisible);
			Base.renew.SetVisible(isVisible);
			Base.terminate.SetVisible(isVisible);
			Base.upgrade.SetVisible(isVisible);
			Base.activateUpgrade.SetVisible(isVisible);
			Base.undoBilling.SetVisible(isVisible);
			Base.viewUsage.SetVisible(isVisible);
			Base.ChangeID.SetVisible(isVisible);
		}
	}
}
