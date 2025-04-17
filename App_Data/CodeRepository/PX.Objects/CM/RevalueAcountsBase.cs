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
using PX.Objects.Common;
using PX.Objects.Common.Tools;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;
using System;

namespace PX.Objects.CM
{
	public class RevalueAcountsBase<THistory> : PXGraph<RevalueAcountsBase<THistory>> 
		where THistory : class, IBqlTable, new()
	{
		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		public virtual ProcessingResult CheckFinPeriod(string finPeriodID, int? branchID)
		{
			ProcessingResult result = new ProcessingResult();
			int? organizationID = PXAccess.GetParentOrganizationID(branchID);
			FinPeriod period = FinPeriodRepository.FindByID(organizationID, finPeriodID);

			if (period == null)
			{
				result.AddErrorMessage(GL.Messages.FinPeriodDoesNotExistForCompany,
						FinPeriodIDFormattingAttribute.FormatForError(finPeriodID),
						PXAccess.GetOrganizationCD(PXAccess.GetParentOrganizationID(branchID)));
			}
			else
			{
				result = FinPeriodUtils.CanPostToPeriod(period);
			}

			if (!result.IsSuccess)
			{
				PXProcessing<THistory>.SetError(new PXException(result.GetGeneralMessage()));
			}

			return result;
		}

		public virtual void VerifyCurrencyEffectiveDate(PXCache cache, RevalueFilter filter)
		{
			cache.RaiseExceptionHandling<RevalueFilter.curyEffDate>(filter, filter.CuryEffDate, null);

			string finPeriodID = filter?.FinPeriodID;
			DateTime? curyEffDate = filter?.CuryEffDate;

			if (curyEffDate == null || String.IsNullOrEmpty(finPeriodID)) return;

			FinPeriod currentPeriod = FinPeriodRepository.FindByID(filter.OrganizationID ?? FinPeriod.organizationID.MasterValue, finPeriodID);

			if (currentPeriod == null) return;

			bool dateBelongsToFinancialPeriod = currentPeriod.IsAdjustment == true && curyEffDate == currentPeriod.EndDate.Value.AddDays(-1)
												|| currentPeriod.IsAdjustment != true && curyEffDate >= currentPeriod.StartDate && curyEffDate < currentPeriod.EndDate;

			if (!dateBelongsToFinancialPeriod)
			{
				cache.RaiseExceptionHandling<RevalueFilter.curyEffDate>(filter, filter.CuryEffDate, new PXSetPropertyException(Messages.DateNotBelongFinancialPeriod, PXErrorLevel.Warning));
			}
		}

		protected virtual void _(Events.FieldDefaulting<RevalueFilter, RevalueFilter.curyEffDate> e)
		{
			if (e.Row == null) return;

			FinPeriod currentPeriod = FinPeriodRepository.FindByID(
				e.Row.OrganizationID ?? FinPeriod.organizationID.MasterValue,
				e.Row.FinPeriodID);

			if (currentPeriod?.EndDate != null)
			{
				e.NewValue = currentPeriod.EndDate;
				e.Cancel = true;
			}
		}

	}
}
