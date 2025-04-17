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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.EP;
using System;

namespace PX.Objects.Extension
{
	/// <exclude/>
	public abstract class EPEmployeeDelegateExtension<TGraph> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
	{
		#region State

		protected static bool IsExtensionActive() =>
			PXAccess.FeatureInstalled<FeaturesSet.expenseManagement>()
			|| PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>();

		#endregion

		#region Selects

		public SelectFrom<
					EPWingman>
				.Where<
					EPWingman.employeeID.IsEqual<EPEmployee.bAccountID.FromCurrent>>
				.OrderBy<
					EPWingman.startsOn.Desc>
				.View Delegates;

		[PXHidden]
		public SelectFrom<
					EPWingman>
				.Where<
					EPWingman.delegationOf.IsEqual<EPDelegationOf.approvals>
					.And<EPWingman.employeeID.IsEqual<@P.AsInt>>>
				.View DelegatesForApprovals;

		#endregion

		#region Events

		protected virtual void _(Events.FieldUpdated<EPWingman, EPWingman.delegationOf> e)
		{
			if (EPDelegationOf.Expenses.Equals(e.NewValue))
			{
				e.Row.StartsOn = null;
				e.Row.ExpiresOn = null;
			}
		}

		protected virtual void _(Events.RowPersisting<EPWingman> e)
		{
			if (e.Row == null) return;

			VerifyApprovalDelegateRow(e.Cache, e.Row);
		}

		protected virtual void _(Events.FieldVerifying<EPWingman, EPWingman.startsOn> e)
		{
			if (e.Row == null) return;

			if (e.NewValue == null)
			{
				throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty);
			}

			// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Justification]
			e.Row.StartsOn = (DateTime)e.NewValue;

			VerifyStartsOnDateInThePast(e.Cache, e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<EPWingman, EPWingman.startsOn> e)
		{
			if (e.Row == null) return;

			if (EPDelegationOf.Approvals.Equals(e.Row.DelegationOf) && e.NewValue == null)
			{
				e.NewValue = GetLastAllowedStartOnDate(e.Row);
				e.Cancel = true;
			}
		}

		#endregion

		#region Methods

		protected virtual void VerifyApprovalDelegateRow(PXCache cache, EPWingman row)
		{
			if (row.DelegationOf != EPDelegationOf.Approvals)
			{
				return;
			}

			VerifyStartsOnDateInThePast(cache, row);

			if (row.StartsOn != null && row.ExpiresOn != null && row.StartsOn > row.ExpiresOn)
			{
				cache.RaiseExceptionHandling<EPWingman.expiresOn>
					(row, row.ExpiresOn,
						new PXSetPropertyException(PX.Objects.EP.MessagesNoPrefix.DelegateExpiresOnDateBeforeStartsOn));
			}

			DateTime rowStartsOn = row.StartsOn.Value;
			DateTime rowExpiresOn = row.ExpiresOn.HasValue ? row.ExpiresOn.Value : DateTime.MaxValue;

			foreach (EPWingman item in DelegatesForApprovals.Select(row.EmployeeID))
			{
				if (item.RecordID == row.RecordID)
					continue;

				DateTime tStartA = item.StartsOn.Value;
				DateTime tEndA = item.ExpiresOn.HasValue ? item.ExpiresOn.Value : DateTime.MaxValue;

				if (
					tStartA <= rowExpiresOn && rowExpiresOn <= tEndA
					|| rowStartsOn <= tStartA && tEndA <= rowExpiresOn
					|| tStartA <= rowStartsOn && rowStartsOn <= tEndA
				)
				{
					if (
						row.ExpiresOn != null
						&& (
							tStartA <= rowExpiresOn && rowExpiresOn <= tEndA
							|| rowStartsOn <= tStartA && tEndA <= rowExpiresOn
						)
					)
					{
						cache.RaiseExceptionHandling<EPWingman.expiresOn>
							(row, row.ExpiresOn,
								new PXSetPropertyException(PX.Objects.EP.MessagesNoPrefix.DelegateStartsExpiresOnDatesInsideExistingPeriod));
					}

					if (
						tStartA <= rowStartsOn && rowStartsOn <= tEndA
						|| rowStartsOn <= tStartA && tEndA <= rowExpiresOn
					)
					{
						cache.RaiseExceptionHandling<EPWingman.startsOn>
							(row, row.StartsOn,
								new PXSetPropertyException(PX.Objects.EP.MessagesNoPrefix.DelegateStartsExpiresOnDatesInsideExistingPeriod));
					}
				}
			}
		}

		protected virtual void VerifyStartsOnDateInThePast(PXCache cache, EPWingman row)
		{
			if (row.StartsOn < PXTimeZoneInfo.Now.Date.AddMilliseconds(-1))
			{
				cache.RaiseExceptionHandling<EPWingman.startsOn>
					(row, row.StartsOn,
						new PXSetPropertyException(PX.Objects.EP.MessagesNoPrefix.DelegateStartsOnDateInThePast));
			}
		}

		protected virtual DateTime? GetLastAllowedStartOnDate(EPWingman row, DateTime? startsDate = null)
		{
			DateTime maxExpireOn = PXTimeZoneInfo.Now;
			foreach (EPWingman item in DelegatesForApprovals.Select(row.EmployeeID))
			{
				var dt = item.ExpiresOn ?? item.StartsOn;
				if (maxExpireOn < dt.Value)
				{
					maxExpireOn = dt.Value;
				}
			}
			return maxExpireOn.AddDays(1);
		}

		#endregion
	}
}
