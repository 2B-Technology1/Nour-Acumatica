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

using PX.Objects.Common;
using PX.Objects.GL;
using System;
using System.Collections.Generic;

namespace PX.Objects.FA
{
	public interface IFABookPeriodRepository
	{
		int GetFABookPeriodOrganizationID(int? bookID, int? assetID, bool check = true);
		int GetFABookPeriodOrganizationID(FABookBalance balance, bool check = true);
		IYearSetup FindFABookYearSetup(FABook book, bool clearQueryCache = false);
		IYearSetup FindFABookYearSetup(int? bookID, bool clearQueryCache = false);
		IEnumerable<IPeriodSetup> FindFABookPeriodSetup(FABook book, bool clearQueryCache = false);
		IEnumerable<IPeriodSetup> FindFABookPeriodSetup(int? bookID, bool clearQueryCache = false);
		FABookYear FindFirstFABookYear(int? bookID, int? organizationID, bool clearQueryCache = false, bool mergeCache = false);
		FABookYear FindLastFABookYear(int? bookID, int? organizationID, bool clearQueryCache = false, bool mergeCache = false);
		FABookYear FindMasterFABookYearByID(FABook book, string yearNumber, bool clearQueryCache = false, bool mergeCache = false);
		FABookPeriod FindMasterFABookPeriodByID(FABook book, string periodID, bool clearQueryCache = false, bool mergeCache = false);
		FABookPeriod FindOrganizationFABookPeriodByID(string periodID, int? bookID, int? assetID, bool clearQueryCache = false, bool mergeCache = false);
		FABookPeriod FindNextNonAdjustmentMasterFABookPeriod(FABook book, string prevFABookPeriodID, bool clearQueryCache = false, bool mergeCache = false);
		FABookPeriod FindLastNonAdjustmentOrganizationFABookPeriodOfYear(FAOrganizationBook book, string finYear, bool clearQueryCache = false, bool mergeCache = false);
		FABookPeriod FindFABookPeriodOfDate(DateTime? date, int? bookID, int? assetID, bool check = true);
		FABookPeriod FindFABookPeriodOfDateByBranchID(DateTime? date, int? bookID, int? branchID, bool check = true);
		string GetFABookPeriodIDOfDate(DateTime? date, int? bookID, int? assetID, bool check = true);
		short GetQuarterNumberOfDate(DateTime? date, int? bookID, int? assetID);
		short GetPeriodNumberOfDate(DateTime? date, int? bookID, int? assetID);
		int GetYearNumberOfDate(DateTime? date, int? bookID, int? assetID);
		FABookPeriod FindMappedPeriod(FABookPeriod.Key fromPeriodKey, FABookPeriod.Key toPeriodKey);
	    FABookPeriod FindByKey(int? bookID, int? organizaionID, string periodID);
	    FABookPeriod FindPeriodByMasterPeriodID(int? bookID, int? organizaionID, string masterPeriodID);
		FABookPeriod GetMappedFABookPeriod(int? bookID, int? sourceOrganizationID, string finPeriodID, int? targetOrganizationID);
		ProcessingResult<FABookPeriod> FindMappedFABookPeriod(int? bookID, int? sourceOrganizationID, string sourcefinPeriodID, int? targetOrganizationID);
		ProcessingResult<FABookPeriod> FindMappedFABookPeriodUsingFinPeriod(int? bookID, int? sourceOrganizationID, string sourcefinPeriodID, int? targetOrganizationID);
		FABookPeriod GetMappedFABookPeriodByBranches(int? bookID, int? sourceBranchID, string finPeriodID, int? targetBranchID);
		ProcessingResult<FABookPeriod> GetFABookPeriodByMasterPeriodID(int? bookID, int? organizationID, string masterFinPeriodID);
		bool IsPostingFABook(int? bookID);
	}
}
