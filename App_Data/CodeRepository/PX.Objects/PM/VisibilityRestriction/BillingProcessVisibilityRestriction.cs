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
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.PM.Billing;
using static PX.Objects.PM.BillingProcess;

namespace PX.Objects.PM
{
	public class BillingProcessVisibilityRestriction : PXGraphExtension<BillingProcess>
	{
		// ReSharper disable InconsistentNaming
		[InjectDependency]
		private ICurrentUserInformationProvider _currentUserInformationProvider { get; set; }
		// ReSharper restore InconsistentNaming

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.ProjectsUnbilled.WhereAnd<
				Where<Contract.defaultBranchID, IsNotNull,
					Or<Where<Contract.defaultBranchID, IsNull, And<Customer.cOrgBAccountID, RestrictByBranch<Current<AccessInfo.branchID>>>>>>>();
			Base.ProjectsUbilledCutOffDateExcluded.WhereAnd<
				Where<Contract.defaultBranchID, IsNotNull,
					Or<Where<Contract.defaultBranchID, IsNull, And<Customer.cOrgBAccountID, RestrictByBranch<Current<AccessInfo.branchID>>>>>>>();
			Base.ProjectsProgressive.WhereAnd<
				Where<Contract.defaultBranchID, IsNotNull,
					Or<Where<Contract.defaultBranchID, IsNull, And<Customer.cOrgBAccountID, RestrictByBranch<Current<AccessInfo.branchID>>>>>>>();
			Base.ProjectsRecurring.WhereAnd<
				Where<Contract.defaultBranchID, IsNotNull,
					Or<Where<Contract.defaultBranchID, IsNull, And<Customer.cOrgBAccountID, RestrictByBranch<Current<AccessInfo.branchID>>>>>>>();
		}

		public void _(Events.RowSelected<BillingFilter> e)
		{
			BillingFilter filter = Base.Filter.Current;

			var branch = _currentUserInformationProvider.GetBranchCD();

			Base.Items.SetProcessDelegate<PMBillEngine>(
					delegate (PMBillEngine engine, Contract item)
					{
						PMProject project = PMProject.PK.Find(engine, item.ContractID);
						if (project.DefaultBranchID != null)
						{
							Customer customer = null;
							if (project.CustomerID != null)
								customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>,
									And<Customer.cOrgBAccountID, RestrictByBranch<Current<AccessInfo.branchID>>>>>.Select(engine, project.CustomerID);

							if (customer == null)
							{
								string customerName = null;
								if (project.CustomerID.HasValue)
								{
									Customer customerData = PXSelect<Customer,
										Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(engine, project.CustomerID);
									customerName = customerData?.AcctName;
								}

								throw new PXException(AR.Messages.BranchRestrictedByCustomer, customerName, branch);
							}
						}

						engine.Clear();
						if (engine.Bill(item.ContractID, filter.InvoiceDate, filter.InvFinPeriodID).IsEmpty)
						{
							throw new PXSetPropertyException(Warnings.NothingToBill, PXErrorLevel.RowWarning);
						}
					});
		}

		private string GetCustomerName(int? customerId)
		{
			if (!customerId.HasValue)
			{
				return null;
			}
			Customer customer = PXSelect<Customer,
				Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(Base, customerId);
			return customer?.AcctName;
		}
	}
}
