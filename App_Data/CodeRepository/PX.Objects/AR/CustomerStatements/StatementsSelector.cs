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
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AR
{
	public partial class ARStatementPrint : PXGraph<ARStatementDetails>
	{
		private static IStatementsSelector GetStatementSelector(PXGraph graph, PrintParameters filter)
		{
			ARSetup arSetup = PXSetup<ARSetup>.Select(graph);
			switch (arSetup?.PrepareStatements)
			{
				case null: return new EmptyStatementsSelector();
				case AR.ARSetup.prepareStatements.ForEachBranch: return new StatementsSelectorEachBranch(graph, filter);
				case AR.ARSetup.prepareStatements.ConsolidatedForCompany: return new StatementsSelectorConsolidatedForCompany(graph, filter);
				case AR.ARSetup.prepareStatements.ConsolidatedForAllCompanies: 
				default: return new StatementsSelector(graph, filter);
			}
		}

		private interface IStatementsSelector
		{
			IEnumerable<ARStatement> Select(DetailsResult detailsResult);
			void Update(ARStatement statement);
		}

		private class EmptyStatementsSelector : IStatementsSelector
		{
			public IEnumerable<ARStatement> Select(DetailsResult detailsResult) => Enumerable.Empty<ARStatement>();
			public void Update(ARStatement statement) { }
		}

		private class StatementsSelector : IStatementsSelector
		{
			private readonly PXView selectStatementsView;
			protected readonly PrintParameters filter;

			public StatementsSelector(PXGraph viewGraph, PrintParameters filter)
			{
				this.filter = filter;

				selectStatementsView = new PXView(viewGraph, false, AddAdditionalConditions(new Select<
				   ARStatement,
				   Where<
					   ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>,
					   And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>,
					   And<ARStatement.statementCustomerID, Equal<Required<ARStatement.customerID>>>>>>()));

				viewGraph.Views["_ARStatement_"] = selectStatementsView;
				viewGraph.Views.Caches.Add(typeof(ARStatement));
			}


			public IEnumerable<ARStatement> Select(DetailsResult detailsResult)
			{
				object[] parameters = new object[] { filter.StatementCycleId, filter.StatementDate, detailsResult.CustomerID }
				.Concat(GetParametersForAdditionalConditions(detailsResult))
				.ToArray();

				return selectStatementsView.SelectMulti(parameters).RowCast<ARStatement>();
			}

			protected virtual BqlCommand AddAdditionalConditions(BqlCommand bqlCommand)
			{
				if (filter.CuryStatements == true) return bqlCommand.WhereAnd<Where<ARStatement.curyID, Equal<Required<ARStatement.curyID>>>>();
				else return bqlCommand; 
			}

			protected virtual IEnumerable<object> GetParametersForAdditionalConditions(DetailsResult detailsResult)
			{
				if (filter.CuryStatements == true) yield return detailsResult.CuryID;
			}

			public void Update(ARStatement statement) => selectStatementsView.Cache.Update(statement);

		}

		private class StatementsSelectorEachBranch : StatementsSelector
		{
			public StatementsSelectorEachBranch(PXGraph viewGraph, PrintParameters filter)
				: base(viewGraph, filter) { }

			protected override BqlCommand AddAdditionalConditions(BqlCommand bqlCommand)
			{
				return base.AddAdditionalConditions(bqlCommand).WhereAnd<Where<ARStatement.branchID, Equal<Required<ARStatement.branchID>>>>();
			}

			protected override IEnumerable<object> GetParametersForAdditionalConditions(DetailsResult detailsResult)
			{
				return base.GetParametersForAdditionalConditions(detailsResult).Append(filter.BranchID);
			}
		}

		private class StatementsSelectorConsolidatedForCompany : StatementsSelector
		{
			private readonly int[] branches;

			public StatementsSelectorConsolidatedForCompany(PXGraph viewGraph, PrintParameters filter)
				: base(viewGraph, filter)
			{
				branches = PXAccess.GetChildBranchIDs(filter.OrganizationID);
			}

			protected override BqlCommand AddAdditionalConditions(BqlCommand bqlCommand)
			{
				return base.AddAdditionalConditions(bqlCommand).WhereAnd<Where<ARStatement.branchID, In<Required<ARStatement.branchID>>>>();
			}

			protected override IEnumerable<object> GetParametersForAdditionalConditions(DetailsResult detailsResult)
			{
				return base.GetParametersForAdditionalConditions(detailsResult).Append(branches);
			}
		}
	}
}
