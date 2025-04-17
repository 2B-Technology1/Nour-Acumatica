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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public abstract class RegularAmountAttribute : PXEventSubscriberAttribute, IPXRowSelectedSubscriber, 
		IPXFieldDefaultingSubscriber, IPXFieldUpdatedSubscriber
	{
		protected bool _UpdateEarningDetailsRate;
		protected readonly string _EarningDetailsViewName;
		protected readonly Type _EmpTypeField;
		protected readonly Type _ManualRegularAmountField;
		protected PXErrorLevel _ErrorLevel;
		
		protected RegularAmountAttribute(Type empTypeField, Type manualRegularAmountField, string earningDetailsViewName)
		{
			_UpdateEarningDetailsRate = true;
			_EmpTypeField = empTypeField;
			_ManualRegularAmountField = manualRegularAmountField;
			_EarningDetailsViewName = earningDetailsViewName;
			_ErrorLevel = PXErrorLevel.Undefined;
		}

		public static void EnforceEarningDetailUpdate<Field>(PXCache cache, object record, bool enforceUpdate) where Field : IBqlField
		{
			string fieldName = typeof(Field).Name;
			foreach (RegularAmountAttribute attribute in cache.GetAttributesReadonly(fieldName).OfType<RegularAmountAttribute>())
			{
				attribute._UpdateEarningDetailsRate = enforceUpdate;
				if (enforceUpdate)
					attribute.OnRegularAmountUpdated(cache, record);
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdated.AddHandler(_EmpTypeField.DeclaringType, _EmpTypeField.Name, ApplyRegularAmount);
			sender.Graph.FieldUpdated.AddHandler(_ManualRegularAmountField.DeclaringType, _ManualRegularAmountField.Name, ManualUpdateRegularAmount);
			
			sender.Graph.FieldUpdated.AddHandler<PREarningDetail.hours>(EarningDetailHoursUpdated);
			sender.Graph.FieldUpdated.AddHandler<PREarningDetail.typeCD>(EarningDetailTypeUpdated);
			sender.Graph.RowInserted.AddHandler<PREarningDetail>(EarningDetailInserted);
			sender.Graph.RowDeleted.AddHandler<PREarningDetail>(EarningDetailDeleted);
		}
		
		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
				return;
			
			bool isVisible = IsRegularRateRequired(sender, e.Row);
			PXUIFieldAttribute.SetVisible(sender, e.Row, _FieldName, isVisible);
			PXUIFieldAttribute.SetVisible(sender, e.Row, _ManualRegularAmountField.Name, isVisible);

			string errorMessage = PXUIFieldAttribute.GetError(sender, e.Row, FieldName);
			if (!string.IsNullOrWhiteSpace(errorMessage))
				sender.RaiseExceptionHandling(FieldName, e.Row, sender.GetValue(e.Row, FieldName), new PXSetPropertyException(errorMessage, this.GetErrorLevel()));
		}

		public virtual void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			OnRegularAmountUpdated(sender, e.Row);
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && IsRegularRateRequired(sender, e.Row))
			{
				decimal? regularAmount = GetRegularAmount(sender, e.Row, out string errorMessage, out PXErrorLevel errorLevel);
				e.NewValue = regularAmount;

				switch (errorLevel)
				{
					case PXErrorLevel.Error:
				PXUIFieldAttribute.SetError(sender, e.Row, FieldName, errorMessage);
						this.SetErrorLevel(PXErrorLevel.Error);
						break;
					case PXErrorLevel.Warning:
						PXUIFieldAttribute.SetWarning(sender, e.Row, FieldName, errorMessage);
						this.SetErrorLevel(PXErrorLevel.Warning);
						break;
					default:
						break;
				}
			}
		}

		protected virtual void OnRegularAmountUpdated(PXCache sender, object currentRecord)
		{
			if (currentRecord == null)
				return;

			bool isRegularRateRequired = IsRegularRateRequired(sender, currentRecord);
			if (!isRegularRateRequired)
			{
				ApplyStandardPayRate(sender);
				return;
			}

			decimal? newRegularAmount = sender.GetValue(currentRecord, FieldName) as decimal?;
			decimal? standardRegularAmount = GetRegularAmount(sender, currentRecord, out string errorMessage, out PXErrorLevel errorLevel);

			if (standardRegularAmount != null)
			{
				int precision = PRCurrencyAttribute.GetPrecision(sender, currentRecord, FieldName) ?? 2;
				standardRegularAmount = Math.Round(standardRegularAmount.Value, precision, MidpointRounding.AwayFromZero);
			}

			if (standardRegularAmount != newRegularAmount)
				sender.SetValue(currentRecord, _ManualRegularAmountField.Name, true);

			ApplyRegularNonExemptRate(sender, newRegularAmount);
		}

		protected abstract bool IsRegularRateRequired(PXCache sender, object currentRecord = null);

		protected abstract decimal? GetRegularAmount(PXCache cache, object currentRecord, out string errorMessage);

		protected abstract decimal? GetStoredRegularAmount(PXCache cache);

		protected virtual void UpdateRegularAmount(PXCache sender, PXFieldUpdatedEventArgs args)
		{
			if (args.Row != null)
				sender.SetDefaultExt(args.Row, FieldName);
		}

		protected virtual decimal? GetRegularAmount(PXCache cache, int? employeeID, string payGroupID, string payPeriodID, DateTime? startDate, DateTime? endDate, out string errorMessage, out PXErrorLevel errorLevel)
		{
			errorMessage = null;
			errorLevel = PXErrorLevel.Undefined;

			if (employeeID == null || payGroupID == null || payPeriodID == null ||  startDate == null || endDate == null)
				return null;

			PRPayGroupYear payGroupYear =
				SelectFrom<PRPayGroupYear>
					.InnerJoin<PRPayGroupPeriod>.On<PRPayGroupPeriod.FK.PayGroupYear>
					.Where<PRPayGroupYear.payGroupID.IsEqual<P.AsString>
						.And<PRPayGroupPeriod.finPeriodID.IsEqual<P.AsString>>>.View
					.SelectSingleBound(cache.Graph, null, payGroupID, payPeriodID);
			short numberOfPayPeriods = payGroupYear?.FinPeriods ?? 0;

			if (numberOfPayPeriods < 1)
			{
				errorMessage = PXMessages.LocalizeFormat(Messages.IncorrectNumberOfPayPeriodsInPayGroup, payGroupYear?.PayGroupID);
				errorLevel = PXErrorLevel.Error;
				return null;
			}

			string regularHoursType = PRSetupMaint.GetEarningTypeFromSetup<PRSetup.regularHoursType>(cache.Graph);

			if (string.IsNullOrWhiteSpace(regularHoursType))
			{
				errorMessage = Messages.RegularHoursTypeIsNotSetUpInPayrollPreferences;
				errorLevel = PXErrorLevel.Error;
				return null;
			}

			List<PXResult<PREmployeeEarning, PREmployee>> employeeEarningSelectResultList =
				SelectFrom<PREmployeeEarning>
					.InnerJoin<PREmployee>.On<PREmployeeEarning.bAccountID.IsEqual<PREmployee.bAccountID>>
					.InnerJoin<EPEarningType>.On<PREmployeeEarning.typeCD.IsEqual<EPEarningType.typeCD>>
					.Where<PREmployeeEarning.isActive.IsEqual<True>
						.And<PREmployeeEarning.bAccountID.IsEqual<P.AsInt>>
						.And<EPEarningType.isOvertime.IsNotEqual<True>>
						.And<PREarningType.isPiecework.IsNotEqual<True>>
						.And<PREarningType.isAmountBased.IsNotEqual<True>>
						.And<PREmployeeEarning.payRate.IsNotNull>
						.And<PREmployeeEarning.startDate.IsLessEqual<P.AsDateTime.UTC>>
						.And<PREmployeeEarning.endDate.IsNull
							.Or<PREmployeeEarning.endDate.IsGreaterEqual<P.AsDateTime.UTC>>>>
					.OrderBy<Asc<PREmployeeEarning.startDate>>.View
					.Select(cache.Graph, employeeID, endDate, startDate).Select(x => (PXResult<PREmployeeEarning, PREmployee>)x).ToList();

			if (employeeEarningSelectResultList.Count == 0)
			{
				errorMessage = Messages.SuitableEmployeeEarningNotFound;
				errorLevel = PXErrorLevel.Error;
				return null;
			}

			if (employeeEarningSelectResultList.Count > 1)
			{
				errorMessage = Messages.OverlappingEarnings;
				errorLevel = PXErrorLevel.Warning;
			}

			PXResult<PREmployeeEarning, PREmployee> employeeEarningSelectResult =
				employeeEarningSelectResultList.FirstOrDefault(item => ((PREmployeeEarning)item).TypeCD == regularHoursType) ??
				employeeEarningSelectResultList.First();

			PREmployeeEarning employeeEarning = employeeEarningSelectResult;
			PREmployee currentEmployee = employeeEarningSelectResult;

			decimal prorateCoefficient = 1m;
			if (currentEmployee.StdWeeksPerYear != DateConstants.WeeksPerYear && currentEmployee.StdWeeksPerYear > 0)
				prorateCoefficient = (decimal) DateConstants.WeeksPerYear / currentEmployee.StdWeeksPerYear.Value;

			decimal employeePayRate = employeeEarning.PayRate.GetValueOrDefault();
			switch (employeeEarning.UnitType)
			{
				case UnitType.Year:
					return employeePayRate * prorateCoefficient / numberOfPayPeriods;
				case UnitType.Hour:
					decimal hoursPerYear = GetEmployeeHoursPerYear(cache, currentEmployee);
					return employeePayRate * hoursPerYear * prorateCoefficient / numberOfPayPeriods;
				default:
					errorMessage = Messages.SuitableEmployeeEarningNotFound;
					errorLevel = PXErrorLevel.Error;
					return null;
			}
		}

		protected virtual decimal? GetRegularAmount(PXCache cache, object currentRecord, out string errorMessage, out PXErrorLevel errorLevel)
		{
			throw new NotImplementedException();
		}

		protected virtual void ApplyRegularAmount(PXCache sender, PXFieldUpdatedEventArgs args)
		{
			if (args.Row == null)
				return;

			var value = sender.GetValue(args.Row, _EmpTypeField.Name) as string;
			bool isSalaried = EmployeeType.IsSalaried(value);
			var oldValue = (string)args.OldValue;
			bool wasSalaried = EmployeeType.IsSalaried(oldValue);
			if (!wasSalaried && isSalaried)
			{
				sender.SetDefaultExt(args.Row, FieldName);
			}
			else if (!isSalaried)
			{
				sender.SetValueExt(args.Row, _ManualRegularAmountField.Name, false);
				sender.SetValueExt(args.Row, FieldName, null);
			}
		}

		protected virtual void ManualUpdateRegularAmount(PXCache sender, PXFieldUpdatedEventArgs args)
		{
			if (args.Row == null || !IsRegularRateRequired(sender, args.Row) || args.OldValue as bool? != true)
				return;

			sender.SetDefaultExt(args.Row, FieldName);
		}

		protected virtual void EarningDetailHoursUpdated(PXCache sender, PXFieldUpdatedEventArgs args)
		{
			PREarningDetail row = args.Row as PREarningDetail;
			var graph = sender.Graph as PRPayChecksAndAdjustments;
			if (row == null || row.IsRegularRate != true || !IsRegularRateRequired(sender) || graph?.IsVoidCheckInProgress == true)
				return;

			decimal? oldRegularHoursQty = args.OldValue as decimal?;
			decimal? newRegularHoursQty = row.Hours;

			if (newRegularHoursQty == oldRegularHoursQty)
				return;

			OnEarningDetailsModification(sender, row.RecordID);
		}

		protected virtual void EarningDetailTypeUpdated(PXCache sender, PXFieldUpdatedEventArgs args)
		{
			PREarningDetail row = args.Row as PREarningDetail;
			if (row == null || !IsRegularRateRequired(sender))
				return;

			bool? oldIsRegularRate = row.IsRegularRate;
			row.IsRegularRate = IsRegularRateEarningDetail(sender, row);

			if (oldIsRegularRate == row.IsRegularRate)
				return;

			OnEarningDetailsModification(sender, row.RecordID);
		}

		protected virtual void EarningDetailInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PREarningDetail row = e.Row as PREarningDetail;
			var graph = sender.Graph as PRPayChecksAndAdjustments;
			if (row == null || !IsRegularRateRequired(sender) || graph?.IsVoidCheckInProgress == true)
			{
				return;
			}

			row.IsRegularRate = IsRegularRateEarningDetail(sender, row);

			if (row.IsRegularRate == true)
				OnEarningDetailsModification(sender);
		}

		protected virtual void EarningDetailDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			PREarningDetail row = e.Row as PREarningDetail;
			if (row == null || !IsRegularRateRequired(sender))
				return;

			if (row.IsRegularRate == true)
				OnEarningDetailsModification(sender);
		}

		protected virtual void ApplyStandardPayRate(PXCache sender)
		{
			PXView earningDetailsView = GetEarningDetailsView(sender);
			if (earningDetailsView == null)
				return;

			PXCache earningDetailsCache = earningDetailsView.Cache;
			foreach (object record in earningDetailsView.SelectMulti())
			{
				PREarningDetail earningDetail = GetEarningDetailFromViewRecord(record);

				if (earningDetail == null || earningDetail.IsRegularRate == false)
					continue;

				earningDetail.IsRegularRate = false;
				earningDetailsCache.Update(earningDetail);
			}
		}

		protected virtual void ApplyRegularNonExemptRate(PXCache sender, decimal? newRegularAmount)
		{
			UpdateRegularEarningDetails(sender, true, newRegularAmount);
		}

		protected virtual void OnEarningDetailsModification(PXCache sender, int? recordIDToSkip = null)
		{
			UpdateRegularEarningDetails(sender, false, null, recordIDToSkip);
		}

		protected virtual void UpdateRegularEarningDetails(PXCache sender, bool regularRateUpdated, decimal? newRegularAmount, int? recordIDToSkip = null)
		{
			if (!_UpdateEarningDetailsRate)
				return;
			
			PXView earningDetailsView = GetEarningDetailsView(sender);
			if (earningDetailsView == null)
				return;

			List<PREarningDetail> earningDetailsToUpdate = new List<PREarningDetail>();
			decimal regularAmount = (regularRateUpdated ? newRegularAmount : GetStoredRegularAmount(sender)).GetValueOrDefault();
			decimal regularHoursQty = 0;
			int lastRecordWithNonZeroHoursIndex = -1;

			foreach (var record in earningDetailsView.SelectMulti())
			{
				PREarningDetail earningDetail = GetEarningDetailFromViewRecord(record);

				if (earningDetail != null &&
					(regularRateUpdated && IsRegularRateEarningDetail(sender, earningDetail) ||
					!regularRateUpdated && earningDetail.IsRegularRate == true))
				{
					decimal earningDetailHours = earningDetail.Hours.GetValueOrDefault();
					
					regularHoursQty += earningDetailHours;
					earningDetailsToUpdate.Add(earningDetail);

					if (earningDetailHours > 0)
						lastRecordWithNonZeroHoursIndex = earningDetailsToUpdate.Count - 1;
				}
			}

			decimal hourlyRate = (regularHoursQty != 0) ? regularAmount / regularHoursQty : 0m;

			for (var index = 0; index < earningDetailsToUpdate.Count; index++)
			{
				PREarningDetail earningDetail = earningDetailsToUpdate[index];

				if (regularRateUpdated)
					earningDetail.IsRegularRate = true;

				int amountPrecision = PRCurrencyAttribute.GetPrecision(earningDetailsView.Cache, earningDetail, nameof(PREarningDetail.amount)) ?? 2;
				decimal currentAmount = Math.Round(hourlyRate * earningDetail.Hours.GetValueOrDefault(), amountPrecision, MidpointRounding.AwayFromZero);

				earningDetail.Rate = hourlyRate;
				// Last record with non-zero hours gets the reminder to ensure that TotalAmount is equal to the sum of all regular EarningDetail.Amounts
				// "currentAmount > regularAmount" condition is needed to ensure that the sum of "earningDetail.Amount" in processed records does not exceed 
				// the Regular Amount because of "MidpointRounding.AwayFromZero" rounding. 
				// For example, Regular Amount is 0.1$ and there are 15 Earning Detail records. Thus, 10 records will have 0.01$ Amount and 5 records will have 0$ Amount
				if (index == lastRecordWithNonZeroHoursIndex || currentAmount > regularAmount)
					earningDetail.Amount = regularAmount; 
				else
					earningDetail.Amount = currentAmount;

				if (earningDetail.RecordID != recordIDToSkip)
					earningDetailsView.Cache.Update(earningDetail);

				regularAmount -= earningDetail.Amount.GetValueOrDefault();
			}

			if (!regularRateUpdated)
				earningDetailsView.RequestRefresh();
		}

		protected virtual PXView GetEarningDetailsView(PXCache sender, string earningDetailsViewName)
		{
			sender.Graph.Views.TryGetValue(earningDetailsViewName, out PXView earningDetailsView);
			return earningDetailsView;
		}

		protected virtual PXView GetEarningDetailsView(PXCache sender)
		{
			return GetEarningDetailsView(sender, _EarningDetailsViewName);
		}

		protected virtual bool IsOvertimeEarningDetail(PXCache sender, PREarningDetail row)
		{
			object earningTypeRecord = PXSelectorAttribute.Select<PREarningDetail.typeCD>(sender.Graph.Caches[typeof(PREarningDetail)], row);
			return sender.Graph.Caches[typeof(EPEarningType)].GetValueExt<EPEarningType.isOvertime>(earningTypeRecord) as bool? ?? false;
		}

		protected virtual bool IsRegularRateEarningDetail(PXCache sender, PREarningDetail row)
		{
			return row.TypeCD != null
				&& (row.IsOvertime == false || row.IsOvertime == null && !IsOvertimeEarningDetail(sender, row))
				&& row.IsPiecework != true
				&& row.IsAmountBased != true
				&& row.IsFringeRateEarning != true
				&& row.IsPayingCarryover != true
				&& row.IsPayingSettlement != true;
		}

		protected virtual bool IsRegularRateEarningDetail(PREarningDetail row)
		{
			return row.TypeCD != null && row.IsOvertime != true && row.IsPiecework != true
				&& row.IsAmountBased != true && row.IsFringeRateEarning != true && row.IsPayingCarryover != true;
		}

		protected virtual PREarningDetail GetEarningDetailFromViewRecord(object viewRecord)
		{
			switch (viewRecord)
			{
				case PREarningDetail earningDetailRecord:
					return earningDetailRecord;
				case PXResult<PREarningDetail> pxResultRecord:
					return pxResultRecord[0] as PREarningDetail;
			}

			return null;
		}

		protected virtual decimal GetEmployeeHoursPerYear(PXCache cache, PREmployee currentEmployee)
		{
			cache.Graph.Caches[typeof(PREmployee)].Locate(currentEmployee);
			object hoursPerYearField = cache.Graph.Caches[typeof(PREmployee)].GetValueExt<PREmployee.hoursPerYear>(currentEmployee);
			decimal hoursPerYear = hoursPerYearField is PXFieldState hoursPerYearFieldState
				? hoursPerYearFieldState.Value as decimal? ?? 0m
				: hoursPerYearField as decimal? ?? 0m;

			return hoursPerYear;
		}

		protected PXErrorLevel GetErrorLevel()
		{
			return _ErrorLevel;
		}

		protected void SetErrorLevel(PXErrorLevel errorLevel)
		{
			this._ErrorLevel = errorLevel;
		}
	}

	public class BatchEmployeeRegularAmountAttribute : RegularAmountAttribute
	{
		protected readonly string _AllEarningDetailsViewName;

		public BatchEmployeeRegularAmountAttribute() : 
			base(typeof(PRBatchEmployee.empType), typeof(PRBatchEmployee.manualRegularAmount), nameof(PRPayBatchEntry.EmployeeEarningDetails))
		{
			_AllEarningDetailsViewName = nameof(PRPayBatchEntry.EarningDetails);
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdated.AddHandler<PRBatchEmployee.employeeID>(UpdateRegularAmount);
		}

		protected override bool IsRegularRateRequired(PXCache sender, object currentRecord = null)
		{
			PRBatchEmployee batchEmployee = currentRecord != null
				? currentRecord as PRBatchEmployee
				: sender.Graph.Caches[typeof(PRBatchEmployee)].Current as PRBatchEmployee;

			return EmployeeType.IsSalaried(batchEmployee?.EmpType);
		}

		protected override decimal? GetRegularAmount(PXCache cache, object currentRecord, out string errorMessage, out PXErrorLevel errorLevel)
		{
			int? employeeID = cache.GetValue<PRBatchEmployee.employeeID>(currentRecord) as int?;
			PRBatch payrollBatch = cache.Graph.Caches[typeof(PRBatch)].Current as PRBatch ?? PXParentAttribute.SelectParent<PRBatch>(cache, currentRecord);
			string payGroupID = payrollBatch?.PayGroupID;
			string payPeriodID = payrollBatch?.PayPeriodID;
			DateTime? startDate = payrollBatch?.StartDate;
			DateTime? endDate = payrollBatch?.EndDate;

			return GetRegularAmount(cache, employeeID, payGroupID, payPeriodID, startDate, endDate, out errorMessage, out errorLevel);
		}

		protected override decimal? GetStoredRegularAmount(PXCache cache)
		{
			return (cache.Graph.Caches[typeof(PRBatchEmployee)].Current as PRBatchEmployee)?.RegularAmount;
		}

		protected override void UpdateRegularEarningDetails(PXCache sender, bool regularRateUpdated, decimal? newRegularAmount, int? recordIDToSkip = null)
		{
			base.UpdateRegularEarningDetails(sender, regularRateUpdated, newRegularAmount, recordIDToSkip);

			if (!regularRateUpdated)
			{
				PXView earningDetailsView = GetEarningDetailsView(sender, _AllEarningDetailsViewName);
				earningDetailsView?.RequestRefresh();
			}
		}

		#region Avoid breaking changes in 2022R2
		protected override decimal? GetRegularAmount(PXCache cache, object currentRecord, out string errorMessage)
		{
			return GetRegularAmount(cache, currentRecord, out errorMessage, out PXErrorLevel errorLevel);
		}
		#endregion
	}

	public class PaymentRegularAmountAttribute : RegularAmountAttribute
	{
		public PaymentRegularAmountAttribute(string earningDetailsViewName) : 
			base(typeof(PRPayment.empType), typeof(PRPayment.manualRegularAmount), earningDetailsViewName)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdated.AddHandler<PRPayment.employeeID>(UpdateRegularAmount);
			sender.Graph.FieldUpdated.AddHandler<PRPayment.payGroupID>(UpdateRegularAmount);
			sender.Graph.FieldUpdated.AddHandler<PRPayment.payPeriodID>(UpdateRegularAmount);
		}

		protected override bool IsRegularRateRequired(PXCache sender, object currentRecord = null)
		{
			PRPayment payment = GetPayment(sender, currentRecord);

			return EmployeeType.IsSalaried(payment?.EmpType) && payment.DocType != PayrollType.VoidCheck;
		}

		protected override decimal? GetRegularAmount(PXCache cache, object currentRecord, out string errorMessage, out PXErrorLevel errorLevel)
		{
			PRPayment payment = GetPayment(cache, currentRecord);
			int? employeeID = payment?.EmployeeID;
			string payGroupID = payment?.PayGroupID;
			string payPeriodID = payment?.PayPeriodID;
			DateTime? startDate = payment?.StartDate;
			DateTime? endDate = payment?.EndDate;

			return GetRegularAmount(cache, employeeID, payGroupID, payPeriodID, startDate, endDate, out errorMessage, out errorLevel);
		}

		protected override decimal? GetStoredRegularAmount(PXCache cache)
		{
			return GetPayment(cache)?.RegularAmount;
		}

		protected virtual PRPayment GetPayment(PXCache cache, object currentRecord = null)
		{
			return currentRecord as PRPayment ?? cache.Graph.Caches[typeof(PRPayment)].Current as PRPayment;
		}

		#region Avoid breaking changes in 2022R2
		protected override decimal? GetRegularAmount(PXCache cache, object currentRecord, out string errorMessage)
		{
			return GetRegularAmount(cache, currentRecord, out errorMessage, out PXErrorLevel errorLevel);
		}
		#endregion
	}
}
