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
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.CR;

namespace SP.Objects.CR
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[CRPrimaryGraphRestricted(
		new[]{
			typeof(ContractMaint)
		},
		new[]{
			typeof(Select<
				Contract,
				Where<
					Contract.baseType, Equal<Current<Contract.baseType>>,
					And<Contract.contractCD, Equal<Current<Contract.contractCD>>,
					And<MatchWithBAccountNotNull<Contract.customerID>>>>>)
		})]
	public class ContractExt : PXCacheExtension<Contract>
	{
		#region FinanceVisible
		public abstract class financeVisible : PX.Data.IBqlField { }

		[PXInt()]
		[PXUIField(DisplayName = "Finance Visible", Visible = false)]
		[PXDefault(0)]
		public virtual Int32? FinanceVisible { get; set; }
		#endregion

		#region Contract Cache Attached
		public abstract class contractCD : PX.Data.IBqlField
		{
		}
		protected String _ContractCD;
		[PXDimensionSelector(ContractAttribute.DimensionName,
			typeof(Search5<Contract.contractCD,
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
			OrderBy<Asc<Contract.contractCD>>>),
			typeof(Contract.contractCD),
			typeof(Contract.contractCD), typeof(Contract.customerID), typeof(Customer.acctName), typeof(Contract.locationID),
			typeof(Contract.description), typeof(Contract.status), typeof(Contract.expireDate),
			typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate),
			DescriptionField = typeof(Contract.description), Filterable = true)]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Contract ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String ContractCD
		{
			get
			{
				return this._ContractCD;
			}
			set
			{
				if (this._ContractCD != value)
					Base._contractInfo = null;
				this._ContractCD = value;
			}
		}
		#endregion
	}
}
