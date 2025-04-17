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
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.GL.DAC.Abstract;

namespace PX.Objects.GL.FinPeriods
{
	[PXInternalUseOnly]
	public interface IFinPeriodUtils
	{
		bool AllowPostToUnlockedPeriodAnyway { get; set; }

		void VerifyAndSetFirstOpenedFinPeriod<TFinPeriodField, TBranchField>(PXCache rowCache, object row, PXSelectBase<OrganizationFinPeriod> finPeriodView, Type fieldModuleClosed = null)
			where TFinPeriodField : class, IBqlField
			where TBranchField : class, IBqlField;


		void ValidateFinPeriod(IEnumerable<IAccountable> records, Type fieldModuleClosed = null);

		void ValidateFinPeriod<T>(IEnumerable<T> records, Func<T, string> getFinPeriodID, Func<T, int?[]> getBranchID, Type fieldModuleClosed = null);

		void ValidateMasterFinPeriod<T>(IEnumerable<T> records, Func<T, string> getMasterFinPeriodID, Func<T, int?[]> getBranchIDs, Type fieldModuleClosed = null);

		ProcessingResult CanPostToPeriod(IFinPeriod finPeriod, Type fieldModuleClosed = null);

		bool CanPostToClosedPeriod();

		OrganizationFinPeriod GetOpenOrganizationFinPeriodInSubledger<TClosedInSubledgerField>(string orgFinPeriodID, int? branchID)
			where TClosedInSubledgerField: IBqlField;

		OrganizationFinPeriod GetNearestOpenOrganizationFinPeriodInSubledger<TClosedInSubledgerField>(string orgFinPeriodID, int? branchID, Func<bool> additionalCondition = null)
			where TClosedInSubledgerField : IBqlField;

		OrganizationFinPeriod GetNearestOpenOrganizationFinPeriodInSubledger<TClosedInSubledgerField>(IPeriod orgFinPeriod)
			where TClosedInSubledgerField : IBqlField;

		OrganizationFinPeriod GetOpenOrganizationFinPeriodInFA(string orgFinPeriodID, int? assetID);

		string ComposeFinPeriodID(string yearNumber, string periodNumber);

		(string yearNumber, string periodNumber) ParseFinPeriodID(string finPeriodID);

		void CheckParametersOfCalendarGeneration(int? organizationID, int fromYear, int toYear);

		(int firstYear, int lastYear) GetFirstLastYearForGeneration(int? organizationID, int fromYear, int toYear, bool clearQueryCache = false);

		void CopyPeriods<TDAC, TFinPeriodID, TMasterFinPeriodID>(PXCache cache, TDAC src, TDAC dest)
			where TDAC : class, IBqlTable, new()
			where TFinPeriodID : class, IBqlField
			where TMasterFinPeriodID : class, IBqlField;

		void CopyPeriods<TDAC, TSourceFinPeriodID, TSourceMasterFinPeriodID, TDestFinPeriodID, TDestMasterFinPeriodID>(PXCache cache, TDAC src, TDAC dest)
			where TDAC : class, IBqlTable, new()
			where TSourceFinPeriodID : class, IBqlField
			where TSourceMasterFinPeriodID : class, IBqlField
			where TDestFinPeriodID : class, IBqlField
			where TDestMasterFinPeriodID : class, IBqlField;
	}
}
