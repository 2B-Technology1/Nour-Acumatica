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
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CA;
using System.Collections;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL.DAC;

namespace PX.Objects.GL.Consolidation
{
	public class ConsolBranchMaint : PXGraph<ConsolBranchMaint>
	{
		public PXSelectJoin<Branch,
						InnerJoin<Organization,
							On<Organization.organizationID, Equal<Branch.organizationID>,
								And<Organization.organizationType, Equal<OrganizationTypes.withBranchesBalancing>>>,
						InnerJoin<Ledger,
							On<Ledger.ledgerID, Equal<Branch.ledgerID>>>>,
						Where<Ledger.consolAllowed, Equal<True>,
							Or<Ledger.balanceType, Equal<LedgerBalanceType.actual>>>> BranchRecords;

		public ConsolBranchMaint()
		{
		}
	}
}
