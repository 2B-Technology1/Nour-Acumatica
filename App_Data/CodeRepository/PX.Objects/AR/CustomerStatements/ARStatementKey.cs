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

namespace PX.Objects.AR.CustomerStatements
{
	public class ARStatementKey : Tuple<int, string, int, DateTime>
	{
		public int BranchID => Item1;
		public string CurrencyID => Item2;
		public int CustomerID => Item3;
		public DateTime StatementDate => Item4;

		public ARStatementKey(int branchID, string currencyID, int customerID, DateTime statementDate)
			: base(branchID, currencyID, customerID, statementDate)
		{ }

		public ARStatementKey(ARStatement statement)
			: this(
				  statement.BranchID.Value, 
				  statement.CuryID, 
				  statement.CustomerID.Value, 
				  statement.StatementDate.Value)
		{ }

		public ARStatementKey CopyForAnotherCustomer(int customerID) => new ARStatementKey(BranchID, CurrencyID, customerID, StatementDate);
	}
}
