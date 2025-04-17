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
using PX.Objects.GL.FinPeriods;
using System;

namespace PX.Objects.FA
{
	public interface IFABookPeriodUtils
	{
		int? PeriodMinusPeriod(string finPeriodID1, string finPeriodID2, int? bookID, int? assetID);
		string PeriodPlusPeriodsCount(string finPeriodID, int counter, int? bookID, int organizationID);
		string PeriodPlusPeriodsCount(string finPeriodID, int counter, int? bookID, int? assetID);
		string GetNextFABookPeriodID(string finPeriodID, int? bookID, int organizationID);
		string GetNextFABookPeriodID(string finPeriodID, int? bookID, int? assetID);
		DateTime GetFABookPeriodStartDate(string finPeriodID, int? bookID, int? assetID);
		DateTime GetFABookPeriodEndDate(string finPeriodID, int? bookID, int? assetID);
		OrganizationFinPeriod GetNearestOpenOrganizationMappedFABookPeriodInSubledger<TClosedInSubledgerField>(int? bookID, int? sourceBranchID, string sourcefinPeriodID, int? targetBranchID)
			where TClosedInSubledgerField : IBqlField;
	}
}
