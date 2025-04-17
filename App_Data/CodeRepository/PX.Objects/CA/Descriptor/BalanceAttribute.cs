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
using System.Reflection;
using PX.Data;
using PX.CCProcessingBase.Attributes;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CA
{
	#region CashBalanceAttribute
	/// <summary>
	/// This attribute allows to display current CashAccount balance from CADailySummary<br/>
	/// Read-only. Should be placed on Decimal? field<br/>
	/// <example>
	/// [CashBalance(typeof(PayBillsFilter.payAccountID))]
	/// </example>
	/// </summary>
	public class CashBalanceAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		protected string _CashAccount = null;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="cashAccountType">Must be IBqlField. Refers to the cashAccountID field in the row</param>
		public CashBalanceAttribute(Type cashAccountType)
		{
			_CashAccount = cashAccountType.Name;
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			CASetup caSetup = PXSelect<CASetup>.Select(sender.Graph);
			decimal? result = 0m;
			object cashAccountID = sender.GetValue(e.Row, _CashAccount);

			CADailySummary caBalance = PXSelectGroupBy<CADailySummary,
														 Where<CADailySummary.cashAccountID, Equal<Required<CADailySummary.cashAccountID>>>,
																	Aggregate<Sum<CADailySummary.amtReleasedClearedCr,
																	 Sum<CADailySummary.amtReleasedClearedDr,
																	 Sum<CADailySummary.amtReleasedUnclearedCr,
																	 Sum<CADailySummary.amtReleasedUnclearedDr,
																	 Sum<CADailySummary.amtUnreleasedClearedCr,
																	 Sum<CADailySummary.amtUnreleasedClearedDr,
																	 Sum<CADailySummary.amtUnreleasedUnclearedCr,
																	 Sum<CADailySummary.amtUnreleasedUnclearedDr>>>>>>>>>>.
																	 Select(sender.Graph, cashAccountID);
			if ((caBalance != null) && (caBalance.CashAccountID != null))
			{
				result = caBalance.AmtReleasedClearedDr - caBalance.AmtReleasedClearedCr;

				if ((bool)caSetup.CalcBalDebitClearedUnreleased)
				{
					result += caBalance.AmtUnreleasedClearedDr;
				}
				if ((bool)caSetup.CalcBalCreditClearedUnreleased)
				{
					result -= caBalance.AmtUnreleasedClearedCr;
				}
				if ((bool)caSetup.CalcBalDebitUnclearedReleased)
				{
					result += caBalance.AmtReleasedUnclearedDr;
				}
				if ((bool)caSetup.CalcBalCreditUnclearedReleased)

				{
					result -= caBalance.AmtReleasedUnclearedCr;
				}
				if ((bool)caSetup.CalcBalDebitUnclearedUnreleased)
				{
					result += caBalance.AmtUnreleasedUnclearedDr;
				}
				if ((bool)caSetup.CalcBalCreditUnclearedUnreleased)
				{
					result -= caBalance.AmtUnreleasedUnclearedCr;
				}
			}
			e.ReturnValue = result;
			e.Cancel = true;
		}
	}
	#endregion

	#region GLBalanceAttribute
	/// <summary>
	/// This attribute allows to display a  CashAccount balance from GLHistory for <br/>
	/// the defined Fin. Period. If the fin date is provided, the period, containing <br/>
	/// the date will be selected (Fin. Period parameter will be ignored in this case)<br/>
	/// Balance corresponds to the CuryFinYtdBalance for the period <br/>
	/// Read-only. Should be placed on the field having type Decimal?<br/>
	/// <example>
	/// [GLBalance(typeof(CATransfer.outAccountID), null, typeof(CATransfer.outDate))]
	/// or
	/// [GLBalance(typeof(PrintChecksFilter.payAccountID), typeof(PrintChecksFilter.payFinPeriodID))]
	/// </example>
	/// </summary>
	public class GLBalanceAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		protected string _CashAccount = null;
		protected string _FinDate = null;
		protected string _FinPeriodID = null;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="cashAccountType">Must be IBqlField type. Refers CashAccountID field of the row.</param>
		/// <param name="finPeriodID">Must be IBqlField type. Refers FinPeriodID field of the row.</param>
		public GLBalanceAttribute(Type cashAccountType, Type finPeriodID)
		{
			_CashAccount = cashAccountType.Name;
			_FinPeriodID = finPeriodID.Name;

		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="cashAccountType">Must be IBqlField type. Refers CashAccountID field of the row.</param>
		/// <param name="finPeriodID">Not used.Value is ignored</param>
		/// <param name="finDateType">Must be IBqlField type. Refers FinDate field of the row.</param>
		public GLBalanceAttribute(Type cashAccountType, Type finPeriodID, Type finDateType)
		{
			_CashAccount = cashAccountType.Name;
			_FinDate = finDateType.Name;
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			object cashAccountID = sender.GetValue(e.Row, _CashAccount);

			object finPeriodID = null;

			if (string.IsNullOrEmpty(_FinPeriodID))
			{
				CashAccount cashaccount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(sender.Graph, cashAccountID);

				if (cashaccount != null)
				{
					finPeriodID = sender.Graph.GetService<IFinPeriodRepository>().FindFinPeriodByDate((DateTime?)sender.GetValue(e.Row, _FinDate), PXAccess.GetParentOrganizationID(cashaccount.BranchID))?.FinPeriodID;
				}
			}
			else
			{
				finPeriodID = sender.GetValue(e.Row, _FinPeriodID);
			}

			if (cashAccountID != null && finPeriodID != null)
			{
				e.ReturnValue = PXSelectJoin<GLHistory,
													LeftJoin<Branch,
															On<Branch.branchID, Equal<GLHistory.branchID>,
															And<Branch.ledgerID, Equal<GLHistory.ledgerID>>>,
													LeftJoin<CashAccount,
															On<GLHistory.branchID, Equal<CashAccount.branchID>,
															And<GLHistory.accountID, Equal<CashAccount.accountID>,
															And<GLHistory.subID, Equal<CashAccount.subID>>>>,
													LeftJoin<Account,
															On<GLHistory.accountID, Equal<Account.accountID>,
															And<Match<Account, Current<AccessInfo.userName>>>>,
													LeftJoin<Sub,
															On<GLHistory.subID, Equal<Sub.subID>, And<Match<Sub, Current<AccessInfo.userName>>>>>>>>,
													Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>,
													   And<GLHistory.finPeriodID, LessEqual<Required<GLHistory.finPeriodID>>>>>
													   .Select(sender.Graph, cashAccountID, finPeriodID)
													   .Select(_ => _.GetItem<GLHistory>())
													   .OrderByDescending(_ => _.FinPeriodID)
													   .Select(_ => _.CuryTranYtdBalance)
													   .FirstOrDefault() ?? 0m;
			}
			else e.ReturnValue = 0m;
			e.Cancel = true;
		}
	}
	#endregion
}
