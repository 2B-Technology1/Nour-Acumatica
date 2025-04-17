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

namespace PX.Objects.PR
{
	class PRPayGroupPeriodEndDateUIAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber, IPXFieldUpdatingSubscriber
	{
		private Type _StartDateField;
		private Type _EndDateField;

		public PRPayGroupPeriodEndDateUIAttribute(Type startDateField, Type endDateField)
		{
			_StartDateField = startDateField;
			_EndDateField = endDateField;
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			DateTime? startDate = sender.GetValue(e.Row, _StartDateField.Name) as DateTime?;
			DateTime? endDate = sender.GetValue(e.Row, _EndDateField.Name) as DateTime?;
			e.ReturnValue = ResolveEndDateValue(startDate, endDate);
		}

		public void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			DateTime? startDate = sender.GetValue(e.Row, _StartDateField.Name) as DateTime?;
			DateTime? newValue = e.NewValue as DateTime?;
			object oldValue = sender.GetValue(e.Row, _FieldName);

			if (newValue != null || oldValue != null)
			{
				bool isEmpty = newValue.HasValue && startDate.HasValue && startDate == newValue;
				DateTime? endDate = newValue.HasValue && !isEmpty ?
					newValue.Value.AddDays(1) : newValue;

				sender.SetValue(e.Row, _EndDateField.Name, endDate);
			}
		}
		public static DateTime? ResolveEndDateValue(DateTime? startDate, DateTime? endDate)
		{
			bool isEmpty = endDate.HasValue && startDate.HasValue && startDate == endDate;
			return endDate.HasValue && !isEmpty ? endDate.Value.AddDays(-1) : endDate;
		}
	}
}
