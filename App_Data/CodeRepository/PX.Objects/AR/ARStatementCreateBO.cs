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
using System;
using System.Linq;

namespace PX.Objects.AR
{

	[PXHidden]
	public class StatementCreateBO : PXGraph<StatementCreateBO>
	{
		public PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>> Customer;

		public PXSelect<
			ARStatement,
			Where<
				ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
				And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>>>>
			Statement;

		public PXSelect<
			ARStatement,
			Where<
				ARStatement.customerID, Equal<Required<Customer.bAccountID>>,
				And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>>>>
			CustomerStatement;

		public PXSelect<
			ARStatement,
			Where<
				ARStatement.customerID, Equal<Required<Customer.bAccountID>>,
				And<ARStatement.onDemand, Equal<False>,
				And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>>>>>
			CustomerStatementForDelete;

		public PXSelect<
			ARStatementDetail,
			Where<
				ARStatementDetail.customerID, Equal<Current<ARStatement.customerID>>,
				And<ARStatementDetail.statementDate, Equal<Current<ARStatement.statementDate>>,
				And<ARStatementDetail.curyID, Equal<Current<ARStatement.curyID>>>>>>
			StatementDetail;

		public PXSelect<
			ARRegister,
			Where<
				ARRegister.docType, Equal<Optional<ARStatementDetail.docType>>,
				And<ARRegister.refNbr, Equal<Optional<ARStatementDetail.refNbr>>>>>
			Docs;

		public virtual void ARStatement_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			ARStatement statement = e.Row as ARStatement;

			if (statement == null) return;

			if (statement.OnDemand != true)
			{
				DateTime? newStatementDate = FindLastCstmStatementDate(statement.CustomerID, statement.StatementDate);
				PXUpdate<
					Set<Override.Customer.statementLastDate, Required<Override.Customer.statementLastDate>>,
					Override.Customer,
					Where<
						Override.Customer.bAccountID, Equal<Required<Override.Customer.bAccountID>>,
						And<Override.Customer.statementLastDate, Equal<Required<Override.Customer.statementLastDate>>>>>
				.Update(this, newStatementDate, statement.CustomerID, statement.StatementDate);
			}
		}

		public DateTime? FindLastCstmStatementDate(int? aCustomer, DateTime? aBeforeDate) => PXSelect<
			ARStatement,
			Where<
				ARStatement.customerID, Equal<Required<ARStatement.customerID>>,
				And<ARStatement.statementDate, Less<Required<ARStatement.statementDate>>,
				And<ARStatement.onDemand, Equal<False>>>>,
			OrderBy<
				Desc<ARStatement.statementDate>>>
			.SelectWindowed(this, 0, 1, aCustomer, aBeforeDate)
			.RowCast<ARStatement>()
			.FirstOrDefault()
			?.StatementDate;

		public virtual void ARStatementDetail_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			ARStatementDetail statementDetail = e.Row as ARStatementDetail;

			if (statementDetail == null) return;

			ARStatement parentStatement = FindParentStatement(
				statementDetail.BranchID,
				statementDetail.CuryID,
				statementDetail.CustomerID,
				statementDetail.StatementDate);
			if (parentStatement.OnDemand == true) return;

			PXUpdate<Set<ARAdjust.statementDate, Null>, ARAdjust, Where<ARAdjust.noteID, Equal<Required<ARStatementDetail.refNoteID>>>>
				.Update(this, statementDetail.RefNoteID);
		}

		private ARStatement FindParentStatement(int? branchID, string currencyID, int? customerID, DateTime? statementDate)
		{
			if (Statement.Current?.BranchID == branchID
				&& Statement.Current?.CuryID == currencyID
				&& Statement.Current?.CustomerID == customerID
				&& Statement.Current?.StatementDate == statementDate)
			{
				return Statement.Current;
			}

			ARStatement statementKey = new ARStatement
			{
				BranchID = branchID,
				CuryID = currencyID,
				CustomerID = customerID,
				StatementDate = statementDate,
			};

			ARStatement statementFromCache = Statement.Locate(statementKey);

			if (statementFromCache != null)
			{
				return statementFromCache;
			}

			return PXSelect<
				ARStatement,
				Where<
					ARStatement.branchID, Equal<Required<ARStatement.branchID>>,
					And<ARStatement.curyID, Equal<Required<ARStatement.curyID>>,
					And<ARStatement.customerID, Equal<Required<ARStatement.customerID>>,
					And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>>>>>>
				.SelectWindowed(this, 0, 1, branchID, currencyID, customerID, statementDate);
		}
	}
}
